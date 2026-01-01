using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using TimeTickerUtil;

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
            //System.Diagnostics.Debugger.Launch();
            // Service started while user might already be logged in
            if (IsUserSessionActive())
            {
                TimeTickerHelper.LogState(true);
            }
        }



        protected override void OnStop()
        {
            // Optional: close open session
            TimeTickerHelper.LogState(false);
        }

        protected override void OnSessionChange(SessionChangeDescription change)
        {
            switch (change.Reason)
            {
                case SessionChangeReason.SessionLogon:
                case SessionChangeReason.SessionUnlock:
                    TimeTickerHelper.LogState(true);
                    System.Diagnostics.Debug.WriteLine("unlock");
                    break;

                case SessionChangeReason.SessionLock:
                    TimeTickerHelper.LogState(false);
                    System.Diagnostics.Debug.WriteLine("lock");
                    break;
            }
        }

        private bool IsUserSessionActive()
        {
            // If at least one interactive session exists → user active
            return System.Diagnostics.Process
                .GetProcessesByName("explorer")
                .Any();
        }





    }
}
