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
        private static readonly object _lock = new object();

        private static string RootPath => @"C:\TimeEntry";

        public static string TodayFile =>
            Path.Combine(RootPath, $"{DateTime.Today:yyyy-MM-dd}.log");


        // =========================
        // WRITE (Service calls this)
        // =========================
        public static void LogState(bool isActive)
        {
            lock (_lock)
            {
                EnsureFolder();

                // prevent duplicate consecutive entries
                var last = GetLastState();
                if (last == isActive) return;

                File.AppendAllText(
                    TodayFile,
                    $"{DateTime.Now:HH:mm:ss} | {(isActive ? "ACTIVE" : "INACTIVE")}{Environment.NewLine}"
                );
            }
        }


        public static List<(DateTime time, bool isIn)> ReadStates()
        {
            if (!File.Exists(TodayFile))
                return new List<(DateTime time, bool isIn)>();

            return File.ReadAllLines(TodayFile)
                .Select(l =>
                {
                    var p = l.Split('|');
                    return (DateTime.Parse(p[0]), p[1].Trim() == "ACTIVE");
                })
                .ToList();
        }

        // =========================
        // READ (UI calls this)
        // =========================
        public static List<(DateTime time, bool active)> ReadToday()
        {
            var result = new List<(DateTime, bool)>();

            if (!File.Exists(TodayFile))
                return result;

            foreach (var line in File.ReadAllLines(TodayFile))
            {
                var parts = line.Split('|');
                if (parts.Length != 2) continue;

                if (!DateTime.TryParse(parts[0].Trim(), out var time))
                    continue;

                bool active = parts[1].Trim() == "ACTIVE";
                result.Add((DateTime.Today.Add(time.TimeOfDay), active));
            }

            return result;
        }

        // =========================
        // CALCULATION (UI helper)
        // =========================
        public static TimeSpan CalculateWorkedTime()
        {
            var entries = ReadToday();
            TimeSpan total = TimeSpan.Zero;

            DateTime? start = null;

            foreach (var e in entries)
            {
                if (e.active)
                    start = e.time;
                else if (start.HasValue)
                {
                    total += e.time - start.Value;
                    start = null;
                }
            }

            // still working
            if (start.HasValue)
                total += DateTime.Now - start.Value;

            return total;
        }

        // =========================
        // INTERNAL HELPERS
        // =========================
        private static bool? GetLastState()
        {
            if (!File.Exists(TodayFile))
                return null;

            var lastLine = File.ReadLines(TodayFile).LastOrDefault();
            if (lastLine == null) return null;

            return !lastLine.Contains("INACTIVE");
        }

        private static void EnsureFolder()
        {
            if (!Directory.Exists(RootPath))
                Directory.CreateDirectory(RootPath);
        }
    }

}
