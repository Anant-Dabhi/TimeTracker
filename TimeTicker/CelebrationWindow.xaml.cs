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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TimeTicker
{
    /// <summary>
    /// Interaction logic for CelebrationWindow.xaml
    /// </summary>
    public partial class CelebrationWindow : Window
    {
        public CelebrationWindow()
        {
            InitializeComponent();
            ShowConfetti();

        }

        private void ShowConfetti()
        {
            var rand = new Random();
            for (int i = 0; i < 100; i++)
            {
                var ellipse = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = new SolidColorBrush(Color.FromRgb(
                        (byte)rand.Next(256),
                        (byte)rand.Next(256),
                        (byte)rand.Next(256)))
                };

                Canvas.SetLeft(ellipse, rand.Next((int)ConfettiCanvas.ActualWidth));
                Canvas.SetTop(ellipse, -10);

                ConfettiCanvas.Children.Add(ellipse);

                var animation = new DoubleAnimation
                {
                    From = -10,
                    To = ConfettiCanvas.ActualHeight + 10,
                    Duration = TimeSpan.FromSeconds(2 + rand.NextDouble())
                };

                ellipse.BeginAnimation(Canvas.TopProperty, animation);
            }
        }
    }
}
