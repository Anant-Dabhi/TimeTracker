using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTicker.Model
{
  public  class TimeEntryModel
    {
        public string TimeIn { get; set; }
        public string TimeOut { get; set; }
        public string OutHours { get; set; }
        public string WorkHours { get; set; }
    }
}
