using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace TimeTrackerService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            Installers.Add(new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            });

            Installers.Add(new ServiceInstaller
            {
                ServiceName = "TimeTrackerService",
                DisplayName = "Time Tracker Service",
                StartType = ServiceStartMode.Automatic
            });

        }
    }
}
