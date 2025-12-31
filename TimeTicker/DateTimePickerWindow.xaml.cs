using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace TimeTicker
{
    /// <summary>
    /// Interaction logic for DateTimePickerWindow.xaml
    /// </summary>
    public partial class DateTimePickerWindow : Window
    {
        public DateTime? SelectedDateTime { get; private set; }

        public DateTimePickerWindow()
        {
            InitializeComponent();


            // Hours 1–12
            for (int i = 1; i <= 12; i++)
                hourBox.Items.Add(i.ToString("00"));

            // Minutes 00–59
            for (int i = 0; i < 60; i++)
                minuteBox.Items.Add(i.ToString("00"));

            ampmBox.Items.Add("AM");
            ampmBox.Items.Add("PM");

            hourBox.SelectedIndex = 0;
            minuteBox.SelectedIndex = 0;
            ampmBox.SelectedIndex = 0;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            

            int hour = int.Parse(hourBox.SelectedItem.ToString());
            int minute = int.Parse(minuteBox.SelectedItem.ToString());
            string ampm = ampmBox.SelectedItem.ToString();

            if (ampm == "PM" && hour != 12) hour += 12;
            if (ampm == "AM" && hour == 12) hour = 0;

            SelectedDateTime = DateTime.Now.Date
                .AddHours(hour)
                .AddMinutes(minute);

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
