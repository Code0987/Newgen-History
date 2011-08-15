using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Newgen.Windows
{
    public partial class LeftClock : Window
    {
        private DispatcherTimer timer;

        public LeftClock()
        {
            InitializeComponent();
            this.Top = SystemParameters.PrimaryScreenHeight - (150 + 100);
            this.Left = 100;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick;
            timer.Start();
            TimerTick(null, EventArgs.Empty);

            iFr.Helper.Animate(this, OpacityProperty, 200, 0, 1);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            try
            {
                if (App.Settings.TimeMode == 1)
                {
                    Hours.Text = DateTime.Now.ToString("HH");
                    Minutes.Text = DateTime.Now.ToString("mm");
                    AmPm.Text = "";
                }
                else
                {
                    Hours.Text = DateTime.Now.ToString(" h");
                    Minutes.Text = DateTime.Now.ToString("mm");
                    AmPm.Text = DateTime.Now.ToString("tt");
                }

                var power = SystemInformation.PowerStatus;
                if (power.BatteryChargeStatus == BatteryChargeStatus.NoSystemBattery)
                    BatteryIcon.Visibility = System.Windows.Visibility.Collapsed;
                else
                {
                    BatteryIcon.Visibility = System.Windows.Visibility.Visible;
                    var iconNumber = (int)(power.BatteryLifePercent * 10) + 1;
                    if (iconNumber >= 10)
                        BatteryIcon.Source = new BitmapImage(new Uri("/Resources/Icons/batt10.png", UriKind.Relative));
                    else
                        BatteryIcon.Source = new BitmapImage(new Uri(string.Format("/Resources/batt{0}.png", iconNumber), UriKind.Relative));
                }

                Day.Text = DateTime.Now.ToString("dddd");
                Day.Text = char.ToUpper(Day.Text[0]) + Day.Text.Substring(1);
                Date.Text = DateTime.Now.ToString("MMMM") + " " + DateTime.Now.Day;
            }
            catch { throw; }
        }

        private void this_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            iFr.Helper.Animate(this, OpacityProperty, 200, 0);
        }
    }
}