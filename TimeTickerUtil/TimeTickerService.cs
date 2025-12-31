using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace TimeTickerUtil
{
    public static class TimeTickerHelper
    {
        private static readonly string ApplicationFolderPath = Path.Combine(GetTimeEntryRootPath(), "Time Entries");

        private static readonly object lockObj = new object();
        public static string FilePath = Path.Combine(ApplicationFolderPath, DateTime.Now.Date.ToString("dddd, MMMM dd, yyyy") + ".txt");

        public static void WriteToFile(bool isFirstTime = false)
        {
            try
            {
                FilePath = Path.Combine(ApplicationFolderPath, DateTime.Now.Date.ToString("dddd, MMMM dd, yyyy") + ".txt");
                lock (lockObj)
                {
                    if (!Directory.Exists(ApplicationFolderPath))
                    {
                        Directory.CreateDirectory(ApplicationFolderPath);
                    }

                    // ✅ FIRST LAUNCH TODAY → FULL RECONSTRUCTION (NOT JUST LOGIN)
                    if (isFirstTime && !File.Exists(FilePath))
                    {
                        BuildTodayFileFromWindowsEvents();
                        return;
                    }

                    // ✅ App restarted later → do nothing
                    if (isFirstTime && File.Exists(FilePath))
                    {
                        return;
                    }

                    // ✅ Normal runtime tracking (manual trigger / unlock)
                    string text = DateTime.Now.ToString("h:mm tt");

                    if (File.Exists(FilePath))
                    {
                        string lastLine = File.ReadAllText(FilePath)
                            .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                            .Last();

                        if (lastLine.Contains(","))
                        {
                            text = Environment.NewLine + text;
                        }
                        else
                        {
                            text = "," + text;
                        }
                    }

                    File.AppendAllText(FilePath, text);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("TimeTickerHelper Error", ex.ToString() + ex.StackTrace, EventLogEntryType.Error);
            }
        }



        public static void BuildTodayFileFromWindowsEvents()
        {
            DateTime loginTime = GetFirstLoginTimeToday();
            var events = GetTodayLockUnlockTimeline();

            var lines = new List<string>();
            DateTime currentIn = loginTime;

            foreach (var evt in events)
            {
                // LOCK → close session
                lines.Add($"{currentIn:h:mm tt},{evt:h:mm tt}");

                // Next UNLOCK becomes new session start
                currentIn = evt;
            }

            // If user is currently working → keep last entry OPEN
            lines.Add($"{currentIn:h:mm tt}");

            File.WriteAllLines(FilePath, lines);
        }

        public static List<DateTime> GetTodayLockUnlockTimeline()
        {
            DateTime today = DateTime.Today;
            var result = new List<DateTime>();

            try
            {
                string query = @"
        <QueryList>
          <Query Id='0' Path='Security'>
            <Select Path='Security'>
              *[System[(EventID=4800 or EventID=4801)
              and TimeCreated[@SystemTime>='" + today.ToUniversalTime().ToString("o") + @"']]]
            </Select>
          </Query>
        </QueryList>";

                var eventLogQuery = new EventLogQuery("Security", PathType.LogName, query);
                using (var reader = new EventLogReader(eventLogQuery))
                {

                    for (EventRecord record = reader.ReadEvent();
                         record != null;
                         record = reader.ReadEvent())
                    {
                        using (record)
                        {
                            if (record.TimeCreated.HasValue)
                                result.Add(record.TimeCreated.Value);
                        }
                    }
                }
            }
            catch
            {
                // swallow – fallback will still work safely
            }

            return result.OrderBy(x => x).ToList();
        }

        public static void InsertSingleTimeEntry(DateTime newTime)
        {
            if (!File.Exists(FilePath))
                return;

            var lines = File.ReadAllLines(FilePath).ToList();
            bool entryInserted = false;

            for (int i = 0; i < lines.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                var parts = lines[i].Split(',');

                DateTime start = DateTime.Parse(parts[0]);

                DateTime? end = null;
                if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                {
                    end = DateTime.Parse(parts[1]);
                }

                // ✅ 1. Close open interval
                if (!end.HasValue && newTime > start)
                {
                    lines[i] = string.Format(
                        "{0:h:mm tt},{1:h:mm tt}", start, newTime);
                    entryInserted = true;
                    break;
                }

                // ✅ 2. Trim a closed interval (NO split)
                if (end.HasValue && newTime > start && newTime < end.Value)
                {
                    lines[i] = string.Format(
                        "{0:h:mm tt},{1:h:mm tt}", start, newTime);
                    entryInserted = true;
                    break;
                }
            }

            // ✅ 3. New time AFTER all existing intervals → add new open entry
            if (!entryInserted)
            {
                lines.Add(newTime.ToString("h:mm tt"));
            }

            File.WriteAllLines(FilePath, lines);
        }






        public static DateTime GetFirstLoginTimeToday()
    {
        DateTime today = DateTime.Today;
        string currentUser = WindowsIdentity.GetCurrent().Name; // DOMAIN\Username

        try
        {
            string query = @"
        <QueryList>
          <Query Id='0' Path='Security'>
            <Select Path='Security'>
              *[System[(EventID=4624) and TimeCreated[@SystemTime>='" + today.ToUniversalTime().ToString("o") + @"']]]
            </Select>
          </Query>
        </QueryList>";

            var eventLogQuery = new EventLogQuery("Security", PathType.LogName, query);
            var reader = new EventLogReader(eventLogQuery);

            DateTime? firstLoginTime = null;

            for (EventRecord record = reader.ReadEvent();
                 record != null;
                 record = reader.ReadEvent())
            {
                using (record)
                {
                    string message = record.FormatDescription();

                    // ✅ Must be Interactive or Unlock login
                    bool isValidLogonType =
                        message.Contains("Logon Type:\t\t2") ||   // Interactive
                        message.Contains("Logon Type:\t\t7");     // Unlock

                    // ✅ Must match current user
                    bool isCurrentUser = message.Contains(currentUser.Split('\\').Last());

                    if (isValidLogonType && isCurrentUser)
                    {
                        if (firstLoginTime == null || record.TimeCreated < firstLoginTime)
                        {
                            firstLoginTime = record.TimeCreated;
                        }
                    }
                }
            }

            return firstLoginTime ?? DateTime.Now;
        }
        catch
        {
            return DateTime.Now;
        }
    }
        public static string GetTimeEntryRootPath()
        {
            string path = @"C:\TimeEntry";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }






    }
}
