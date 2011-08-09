using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Mosaic.Windows
{
    /// <summary>
    /// Interaction logic for LeftClock.xaml
    /// </summary>
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
            Hours.Text = DateTime.Now.ToString("HH");
            Minutes.Text = DateTime.Now.ToString("mm");
            AmPm.Text = "";
            Day.Text = DateTime.Now.ToString("dddd");
            Day.Text = char.ToUpper(Day.Text[0]) + Day.Text.Substring(1);
            Date.Text = DateTime.Now.ToString("MMMM") + " " + DateTime.Now.Day;
        }

        private void this_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            iFr.Helper.Animate(this, OpacityProperty, 200, 0);
        }
    }
}