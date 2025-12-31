using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackerService
{
    public partial class TimeTrackerService : ServiceBase
    {
        public TimeTrackerService()
        {
            InitializeComponent();
            CanHandleSessionChangeEvent = true; // 🔴 REQUIRED

        }

        protected override void OnStart(string[] args)
        {
            
            try
            {
                TimeTickerUtil.TimeTickerHelper.WriteToFile(true);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(
                    "Startup failed: " + ex.ToString(),
                    EventLogEntryType.Error
                );
                throw;
            }
        }

        protected override void OnStop()
        {
        }

        protected override void OnSessionChange(SessionChangeDescription change)
        {
            DateTime now = DateTime.Now;

            switch (change.Reason)
            {
                case SessionChangeReason.SessionLock:
                    TimeTickerUtil.TimeTickerHelper.InsertSingleTimeEntry(now);
                    break;

                case SessionChangeReason.SessionUnlock:
                case SessionChangeReason.SessionLogon:
                    TimeTickerUtil.TimeTickerHelper.InsertSingleTimeEntry(now);
                    break;
            }
        }

    }
}
