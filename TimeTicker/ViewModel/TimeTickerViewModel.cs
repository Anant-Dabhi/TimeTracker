using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using TimeTicker.Model;
using TimeTickerUtil;

namespace TimeTicker.ViewModel
{
    public class TimeTickerViewModel : MainViewModel
    {
        TimeSpan _totalWorkTimeOfDay = new TimeSpan(8, 30, 00);
        private int count = 0;
        private const string TimeFormat = "h:mm tt";
        private ObservableCollection<TimeEntryModel> _timeEntryList;
        private TimeSpan _totalWorkHours, _totalOutHours;
        private DateTime _timeToGoHome;
        private TimeEntryModel _lastTimeEntry;
        private DispatcherTimer _dispatcherTimer;
        private TaskbarIcon notifyIcon;


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


        public TimeTickerViewModel()
        {
            
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
        public ObservableCollection<TimeEntryModel> TimeEntryList
        {
            get { return _timeEntryList ?? (_timeEntryList = new ObservableCollection<TimeEntryModel>()); }
            set
            {
                _timeEntryList = value;
                RaisePropertyChanged(() => TimeEntryList);
            }
        }
        public TimeSpan TotalWorkHours
        {
            get { return _totalWorkHours; }
            set
            {
                _totalWorkHours = value;
                TimeToGoHome = DateTime.Now.Add(_totalWorkTimeOfDay.Subtract(TotalWorkHours));
                RaisePropertyChanged(() => TotalWorkHours);

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

      

        private void StartTimerCount()
        {

            if (LastTimeEntry == null)
                return;
            if (_dispatcherTimer == null)
            {
                _dispatcherTimer = new DispatcherTimer
                {
                    Interval = new TimeSpan(0, 0, 5)
                };
                _dispatcherTimer.Tick += (s, e) =>
                {
                    LastTimeEntry.TimeOut = DateTime.Now.ToString(TimeFormat);
                    var outHours = DateTime.ParseExact(LastTimeEntry.TimeOut, TimeFormat, System.Globalization.CultureInfo.InvariantCulture) -
                                               DateTime.ParseExact(LastTimeEntry.TimeIn, TimeFormat, System.Globalization.CultureInfo.InvariantCulture);

                    LastTimeEntry.WorkHours = TimeSpan.FromMinutes(outHours.TotalMinutes).ToString(@"h\:mm\:ss");
                    TimeEntryList = new ObservableCollection<TimeEntryModel>(TimeEntryList);

                    var totalWorkHours = new TimeSpan();
                    foreach (var timeEntryModel in TimeEntryList)
                    {
                        var ss = DateTime.ParseExact(timeEntryModel.WorkHours, @"h\:mm\:ss", System.Globalization.CultureInfo.InvariantCulture);
                        var timespan = new TimeSpan(ss.Hour, ss.Minute, ss.Second);
                        totalWorkHours = totalWorkHours.Add(timespan);
                    }
                    TotalWorkHours = totalWorkHours;
                    if (TotalWorkHours> TimeSpan.FromHours(8.5))
                    {
                        var app = App.Current as App;
                        if (app != null && app._notifyIcon.Tag==null)
                        {

                            var rnd = new Random();
                            app._notifyIcon.ShowBalloonTip(
                                "Time Tracker",
                                completionMessages[rnd.Next(completionMessages.Length)],
                                BalloonIcon.None
                            );

                            app._notifyIcon.Tag= true;
                        }
                    }

                    if (count>10)
                    {
                        GetTimeData();
                        count = 0;

                        if (notifyIcon==null)
                        {
                            notifyIcon = (TaskbarIcon)Application.Current.FindResource("NotifyIcon");
                            notifyIcon.ToolTipText = TimeToGoHome.ToString(TimeFormat);
                        }
                    }

                    if (IsInDesignMode)
                    {
                        _dispatcherTimer.Stop();
                    }

                };
            }

            _dispatcherTimer.Start();
        }

        public void StopTimerCount()
        {
            _dispatcherTimer.Stop();
        }


        public void GetTimeData()
        {
            var filepath = TimeTickerUtil.TimeTickerHelper.FilePath;
            if (File.Exists(filepath))
            {
                DateTime? firstInTime=null;
                
                    var fileLines = File.ReadAllText(filepath).Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    TotalWorkHours = new TimeSpan();
                    foreach (var line in fileLines)
                    {
                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        var timelog = line.Split(',');
                        var timein = timelog[0];
                        if (firstInTime==null)
                        {
                            firstInTime = DateTime.ParseExact(timein, TimeFormat, System.Globalization.CultureInfo.InvariantCulture);
                        }

                        var timeout = timelog.Length > 1 ? timelog[1] : DateTime.Now.ToString(TimeFormat);

                        var workingHours = DateTime.ParseExact(timeout, TimeFormat, System.Globalization.CultureInfo.InvariantCulture) -
                                        DateTime.ParseExact(timein, TimeFormat, System.Globalization.CultureInfo.InvariantCulture);

                        TotalWorkHours = TotalWorkHours.Add(workingHours);

                        var timeDetails = new TimeEntryModel
                        {
                            TimeIn = timein,
                            TimeOut = timeout,
                            WorkHours = TimeSpan.FromMinutes(workingHours.TotalMinutes).ToString(@"h\:mm\:ss")
                        };

                        if (timelog.Length == 1)
                        {
                            LastTimeEntry = timeDetails;
                        }
                        TimeEntryList.Add(timeDetails);
                    }
                    if (firstInTime.HasValue)
                    {
                        var totalhoursInOffice= DateTime.Now - firstInTime.GetValueOrDefault();
                        TotalOutHours = totalhoursInOffice - _totalWorkHours;
                    }

                    RaisePropertyChanged(() => TotalWorkHours);
            }
            else
            {
                TimeTickerHelper.WriteToFile(true);

                GetTimeData();
            }

            StartTimerCount();
        }
    }



}
