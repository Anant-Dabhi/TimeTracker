using Hardcodet.Wpf.TaskbarNotification;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Microsoft.Win32;
using TimeTicker.ViewModel;


namespace TimeTicker
{
    /// <inheritdoc />
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App 
    {
        public TaskbarIcon _notifyIcon;
        private readonly UserControl.PopUp _popup = new UserControl.PopUp();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

            if (_notifyIcon != null)
            {
                _notifyIcon.TrayToolTipOpen += notifyIcon_TrayToolTipOpen;
                _notifyIcon.TrayToolTipClose += notifyIcon_TrayToolTipClose;
                

            }
            DispatcherUnhandledException += App_DispatcherUnhandledException;



        }

      
        private void notifyIcon_TrayToolTipClose(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                _notifyIcon?.CloseBalloon();

            });
        }

        private void notifyIcon_TrayToolTipOpen(object sender, RoutedEventArgs e)
        {
            _notifyIcon.ShowCustomBalloon(_popup, System.Windows.Controls.Primitives.PopupAnimation.Fade, null);
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
#if DEBUG
            Debugger.Launch();
#endif
        }
        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }

       

    }
}
