using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TimeTicker.Model;
using TimeTickerUtil;

namespace TimeTicker.ViewModel
{
    public class TimeTickerViewModel : MainViewModel
    {

        private ObservableCollection<TimeEntryModel> _timeEntryList;
        private readonly TimeSpan _targetWork = TimeSpan.FromHours(8.5);
        private DispatcherTimer _timer;
        private bool _celebrationShown;
        DateTime _timeToGoHome;
        TimeSpan _totalWorkHours;
        private TimeEntryModel _lastTimeEntry;

        string[] completionMessages =
{
    "🎉 8.5 hours done! You survived the workday! 🏆",
    "🚀 Mission complete! 8.5 hours successfully logged!",
    "👏 Congrats! You've officially earned your chair today!",
    "🕺 8.5 hours done! Time to pretend you're off work!",
    "☕ Achievement unlocked: Professional Time Tracker!",
    "🔥 Boom! 8.5 hours crushed like a boss!",
    "🎯 Target achieved! Now go reward yourself!",
    "😎 8.5 hours complete. Productivity level: LEGENDARY!",
    "🏁 Finish line crossed! Time to shut down that laptop!",
    "🎊 Congrats! Your keyboard deserves a break too!",

            "😴 8.5 hours done! Now act busy for 5 more minutes!",
"🧠 Brain officially overworked. Please reboot tomorrow.",
"📊 8.5 hours tracked! HR would be proud!",
"🐢 You made it! Slowly… but you made it!"

};
        private TimeSpan _totalOutHours;

        public TimeTickerViewModel()
        {

        }

        public ObservableCollection<TimeEntryModel> TimeEntryList
        {
            get { return _timeEntryList ?? (_timeEntryList = new ObservableCollection<TimeEntryModel>()); }
            set
            {
                _timeEntryList = value;
                RaisePropertyChanged(() => TimeEntryList);
            }
        }

        private const double TargetHours = 8.5;

        public double WorkProgressPercent
        {
            get
            {
                var progress = TotalWorkHours.TotalHours / TargetHours * 100;
                return Math.Min(progress, 100);
            }
        }

        public string WorkProgressText =>
            $"{WorkProgressPercent:0}%";

        public TimeSpan TotalWorkHours
        {
            get { return _totalWorkHours; }
            set
            {
                _totalWorkHours = value;
                TimeToGoHome = DateTime.Now.Add(_targetWork.Subtract(TotalWorkHours));
                RaisePropertyChanged(() => TotalWorkHours);
                RaisePropertyChanged(() => WorkProgressPercent);
                RaisePropertyChanged(() => WorkProgressText);
                RaisePropertyChanged(() => TotalOutHoursString);

            }
        }
        public DateTime TimeToGoHome
        {
            get { return _timeToGoHome; }
            set
            {
                _timeToGoHome = value;
                RaisePropertyChanged(() => TimeToGoHome);
            }
        }
        public TimeSpan TotalOutHours
        {
            get { return _totalOutHours; }
            set
            {
                _totalOutHours = value;
                RaisePropertyChanged(() => TotalOutHours);
                RaisePropertyChanged(() => TotalOutHoursString);

            }
        }

        public string TotalOutHoursString
        {
            get { return TimeSpan.FromMinutes(TotalOutHours.TotalMinutes).ToString(@"h\:mm"); }

        }



        public TimeEntryModel LastTimeEntry
        {
            get
            {
                return _lastTimeEntry;
            }
            set
            {
                _lastTimeEntry = value;
                RaisePropertyChanged(() => LastTimeEntry);

            }
        }
        private DateTime? GetFirstTimeIn()
        {
            if (TimeEntryList == null || TimeEntryList.Count == 0)
                return null;

            return TimeEntryList
                .Select(e => e.TimeIn)
                .OrderBy(t => t)
                .FirstOrDefault();
        }



        private void RecalculateTotals()
        {
            TotalWorkHours = TimeEntryList.Aggregate(
                TimeSpan.Zero, (sum, e) => sum + e.WorkDuration);

            // 2️⃣ Total elapsed time since first login
            var firstIn = GetFirstTimeIn();
            if (firstIn.HasValue)
            {
                var totalElapsed = DateTime.Now - firstIn.Value;

                // 3️⃣ Out Hours = elapsed - work
                TotalOutHours = totalElapsed - TotalWorkHours;

                if (TotalOutHours < TimeSpan.Zero)
                    TotalOutHours = TimeSpan.Zero;
            }



            TimeToGoHome = DateTime.Now + (_targetWork - TotalWorkHours);
        }

        public void LoadTimeData()
        {
            TimeEntryList.Clear();
            var states = TimeTickerHelper.ReadStates();

            DateTime? openIn = null;

            foreach (var (time, isIn) in states)
            {
                if (isIn)
                {
                    openIn = time;
                }
                else if (openIn.HasValue)
                {
                    TimeEntryList.Add(new TimeEntryModel
                    {
                        TimeIn = openIn.Value,
                        TimeOut = time
                    });
                    openIn = null;
                }
            }

            if (openIn.HasValue)
            {
                LastTimeEntry = new TimeEntryModel { TimeIn = openIn.Value };
                TimeEntryList.Add(LastTimeEntry);
            }

            RecalculateTotals();
            StartTimer();
        }


        private bool _isUserActive = true;

        private void StartTimer()
        {
            if (_timer == null)
            {
                _timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
            }

            // Create mover with 150px radius
            var mover = new MouseCircleMover();
            if (!_timer.IsEnabled)
            {

                _timer.Tick += (_, __) =>
                {
                    RaisePropertyChanged(() => TimeEntryList);
                    RecalculateTotals();

                    var idle = IdleTimeDetector.GetIdleTime();
                    bool isActiveNow = idle < TimeSpan.FromSeconds(300);
                    System.Diagnostics.Debug.WriteLine("Idle time : " + idle.TotalSeconds);
                    if (isActiveNow != _isUserActive)
                    {
                        _isUserActive = isActiveNow;
                        Task.Run(() =>
                    {
                        mover.Run();
                    });
                    }


                    if (!_celebrationShown && TotalWorkHours >= _targetWork)
                    {
                        ShowCelebration();
                        _celebrationShown = true;
                    }

                };
                _timer.Start();
            }

            
        }
        private void ShowCelebration()
        {
            var rnd = new Random();
            var app = (App)Application.Current;

            app._notifyIcon.ShowBalloonTip(
                "TimeTicker 🎉",
                completionMessages[rnd.Next(completionMessages.Length)],
                BalloonIcon.None
            );
        }

    //    private const int Port = 50505;
    //    private readonly ConcurrentDictionary<string, DateTime> _peers = new ConcurrentDictionary<string, DateTime>();
    //    public ObservableCollection<ActiveUserInfo> ActiveUsers { get; }
    //= new ObservableCollection<ActiveUserInfo>();


    //    private int _activeUserCount;
    //    public int ActiveUserCount
    //    {
    //        get => _activeUserCount;
    //        private set
    //        {
    //            if (_activeUserCount != value)
    //            {
    //                _activeUserCount = value;
    //                RaisePropertyChanged(() => ActiveUserCount);
    //            }
    //        }
    //    }


        //private void StartPresence()
        //{
        //    _ = Task.Run(BroadcastLoop);
        //    _ = Task.Run(ListenLoop);
        //    _ = Task.Run(CleanupLoop);
        //}



        //// 🔹 Broadcast "I'm alive"
        //private async Task BroadcastLoop()
        //{
        //    using (var udp = new UdpClient())
        //    {
        //        udp.EnableBroadcast = true;

        //        var endpoint = new IPEndPoint(IPAddress.Broadcast, Port);

        //        while (true)
        //        {
        //            string msg = $"{Environment.MachineName}|{Environment.UserName}|{DateTime.UtcNow:o}";
        //            byte[] data = Encoding.UTF8.GetBytes(msg);

        //            await udp.SendAsync(data, data.Length, endpoint);
        //            await Task.Delay(3000);
        //        }
        //    }
        //}

        //// 🔹 Listen for others
        //private async Task ListenLoop()
        //{
        //    using (var udp = new UdpClient(Port))

        //        while (true)
        //        {
        //            var result = await udp.ReceiveAsync();
        //            var parts = Encoding.UTF8.GetString(result.Buffer).Split('|');

        //            if (parts.Length < 3)
        //                continue;

        //            var ip = result.RemoteEndPoint.Address.ToString();

        //            App.Current.Dispatcher.Invoke(() =>
        //            {
        //                var existing = ActiveUsers.FirstOrDefault(x => x.Ip == ip);
        //                if (existing == null)
        //                {
        //                    ActiveUsers.Add(new ActiveUserInfo
        //                    {
        //                        Ip = ip,
        //                        MachineName = parts[0],
        //                        UserName = parts[1]
        //                    });
        //                }
        //            });
        //        }
        //}


        //// 🔹 Remove inactive peers & update count
        //private async Task CleanupLoop()
        //{
        //    while (true)
        //    {
        //        App.Current.Dispatcher.Invoke(() =>
        //        {
        //            ActiveUserCount = ActiveUsers.Count;
        //        });

        //        await Task.Delay(2000);
        //    }
        //}






    }
}
