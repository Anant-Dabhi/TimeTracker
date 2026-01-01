using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TimeTicker.ViewModel;

namespace TimeTicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class TimeTickerView 
    {
        
        public TimeTickerView()
        {
            InitializeComponent();
            StateChanged += TimeTickerView_StateChanged;
            //notifyIcon.TrayToolTipOpen += notifyIcon_TrayToolTipOpen;
            //notifyIcon.TrayToolTipClose+= notifyIcon_TrayToolTipClose;
            
            
        }

        public void CalculateTime()
        {
            (DataContext as TimeTickerViewModel).TimeEntryList = new System.Collections.ObjectModel.ObservableCollection<Model.TimeEntryModel>();
            (DataContext as TimeTickerViewModel).LoadTimeData();
        }

        private void notifyIcon_TrayToolTipClose(object sender, RoutedEventArgs e)
        {
            //notifyIcon.CloseBalloon();
        }

        void notifyIcon_TrayToolTipOpen(object sender, RoutedEventArgs e)
        {
            //notifyIcon.ShowCustomBalloon(new UserControl.PopUp(), System.Windows.Controls.Primitives.PopupAnimation.Fade, null);
        }

        public TimeTickerViewModel Type
        {
            get { return DataContext as TimeTickerViewModel; }

        }


        void TimeTickerView_StateChanged(object sender, EventArgs e)
        {
            if (WindowState== System.Windows.WindowState.Minimized)
            {
                Hide();
            }
        }
        private bool m_isExplicitClose = false;// Indicate if it is an explicit form close request from the user.
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {

            base.OnClosing(e);

            if (m_isExplicitClose == false)//NOT a user close request? ... then hide
            {
                e.Cancel = true;
                this.Hide();
            }

        }


        

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var popup = new DateTimePickerWindow
            {
                Owner = this
            };

            if (popup.ShowDialog() == true)
            {
                DateTime selected = popup.SelectedDateTime.Value;
                //TimeTickerUtil.TimeTickerHelper.InsertSingleTimeEntry(selected);
                //(DataContext as TimeTickerViewModel).TimeEntryList = new System.Collections.ObjectModel.ObservableCollection<Model.TimeEntryModel>();

                //(DataContext as TimeTickerViewModel).GetTimeData();
            }
        }

        private void fileopen_click(object sender, RoutedEventArgs e)
        {
            
                Process.Start(new ProcessStartInfo
                {
                    FileName = TimeTickerUtil.TimeTickerHelper.TodayFile,
                    UseShellExecute = true
                });

        }
    }
}
