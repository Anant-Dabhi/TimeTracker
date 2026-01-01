using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTicker.Model
{

    public class TimeEntryModel
    {
        public DateTime TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }

        public TimeSpan WorkDuration =>
            TimeOut.HasValue ? TimeOut.Value - TimeIn : DateTime.Now - TimeIn;

        

        public string TimeInText => TimeIn.ToString("h:mm tt");
        public string TimeOutText => TimeOut?.ToString("h:mm tt") ?? "--";
        public string WorkHoursText => WorkDuration.ToString(@"h\:mm\:ss");
    }


    public class ActiveUserInfo
    {
        public string Ip { get; set; }
        public string MachineName { get; set; }
        public string UserName { get; set; }
    }


}


