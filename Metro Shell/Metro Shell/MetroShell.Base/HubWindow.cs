using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Ftware.Apps.MetroShell.Base
{
    public enum AnimationType { Internal = 0, Custom = 1 }

    /// <summary>
    /// Interaction logic for HubWindow.xaml
    /// </summary>
    public partial class HubWindow : Window
    {
        [DefaultValue(AnimationType.Internal)]
        public AnimationType Animation { get; set; }

        [DefaultValue(true)]
        private bool IsHubActive { get; set; }

        public HubWindow()
        {
            Animation = AnimationType.Internal;

            InitializeComponent();

            SourceInitialized += new EventHandler(HubWindow_SourceInitialized);
            Closing += new System.ComponentModel.CancelEventHandler(HubWindow_Closing);
            KeyUp += new KeyEventHandler(HubWindow_KeyUp);
        }

        public void InitializeComponent()
        {
            Left = 0;
            Top = 0;
            ShowInTaskbar = false;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
        }

        private void HubWindow_SourceInitialized(object sender, EventArgs e)
        {
            AnimateStart();
        }

        private void HubWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) base.Close();
        }

        private void HubWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsHubActive)
            {
                e.Cancel = true;
                AnimateClose();
            }
        }

        private void AnimateStart()
        {
            IsHubActive = true;
            switch (Animation)
            {
                case AnimationType.Custom:
                    {
                        IsHubActive = false;
                        this.Left = 0;
                        this.Top = 0;
                        this.Width = SystemParameters.PrimaryScreenWidth;
                        this.Height = SystemParameters.PrimaryScreenHeight;
                    }
                    break;
                case AnimationType.Internal:
                default:
                    {
                        IsHubActive = true;
                        this.Left = -SystemParameters.PrimaryScreenWidth;
                        this.Top = 0;
                        this.Width = SystemParameters.PrimaryScreenWidth;
                        this.Height = SystemParameters.PrimaryScreenHeight;

                        DoubleAnimation leftanimation = new DoubleAnimation()
                        {
                            To = 0,
                            Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                            BeginTime = TimeSpan.FromMilliseconds(50),
                            AccelerationRatio = 0.3,
                            DecelerationRatio = 0.7,
                        };
                        this.BeginAnimation(LeftProperty, leftanimation);
                        Helper.Animate(this, OpacityProperty, 3, 0, 1, 0.3, 0.7);
                    }
                    break;
            }
        }

        private void AnimateClose()
        {
            switch (Animation)
            {
                case AnimationType.Custom:
                    {
                        Topmost = false;
                        IsHubActive = false;
                        Close();
                    }
                    break;
                case AnimationType.Internal:
                default:
                    {
                        DoubleAnimation leftanimation = new DoubleAnimation()
                        {
                            To = -this.ActualWidth,
                            Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                            BeginTime = TimeSpan.FromMilliseconds(1),
                            AccelerationRatio = 0.7,
                            DecelerationRatio = 0,
                        };
                        leftanimation.Completed += (a, b) =>
                        {
                            Left = -this.ActualWidth;

                            leftanimation = null;
                            Helper.Delay(new Action(() =>
                            {
                                IsHubActive = false; Topmost = false; Hide(); Close();
                            }), 1);
                        };
                        this.BeginAnimation(LeftProperty, leftanimation);
                    }
                    break;
            }
        }
    }
}