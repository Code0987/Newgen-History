using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Ftware.Apps.MetroShell.Base;

namespace Ftware.Apps.MetroShell.Controls
{
    /// <summary>
    /// Interaction logic for DateTimeView.xaml
    /// </summary>
    public partial class DateTimeView : UserControl
    {
        private DispatcherTimer timer;

        public DateTimeView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += TimerTick;
                timer.Start();
                TimerTick(null, EventArgs.Empty);
            }
            catch { }

            Helper.Animate(this, OpacityProperty, 150, 0.5);
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Helper.Animate(this, OpacityProperty, 150, 0.5, 1);
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Helper.Animate(this, OpacityProperty, 150, 0.5);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            try
            {
                TextBlock_Time.Text = DateTime.Now.ToString("tt hh:mm");
                TextBlock_Date.Text = DateTime.Now.ToString("dd-MM-yy");
                if (Settings.Current.TimeMode == 1)
                {
                    TextBlock_Time.Text = DateTime.Now.ToString("tt HH:mm");
                }
            }
            catch { }
        }
    }
}