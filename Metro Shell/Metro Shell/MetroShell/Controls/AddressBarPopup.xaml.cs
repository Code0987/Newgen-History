using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Ftware.Apps.MetroShell.Base;
using Ftware.Apps.MetroShell.Native;

namespace Ftware.Apps.MetroShell.Windows
{
    public partial class AddressBarPopup : Window
    {
        private Point pcenter = new Point();

        private UIElement towner;

        private bool isclosepending = true;

        public AddressBarPopup(UIElement owner)
        {
            InitializeComponent();

            towner = owner;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            Dwm.RemoveFromAeroPeek(handle);
            Dwm.RemoveFromAltTab(handle);
            Dwm.RemoveFromFlip3D(handle);

            try
            {
                Point locationFromScreen = towner.PointToScreen(new Point(0, 0));
                pcenter = new Point(
                    locationFromScreen.X + towner.RenderSize.Width + 10,
                    locationFromScreen.Y + towner.RenderSize.Height / 2 - (Height / 2));
            }
            catch { }

            this.Top = pcenter.Y;
            this.Left = pcenter.X;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Helper.Animate(this, OpacityProperty, 150, 1, 0.7, 0.3);
            Helper.Animate(this, LeftProperty, 180, pcenter.X - ActualWidth, pcenter.X, 0.7, 0.3);

            try
            {
                AddressBox.Focus();
                AddressBox.CaretIndex = AddressBox.Text.Length;
            }
            catch { }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isclosepending)
            {
                Helper.Delay(() =>
                {
                    isclosepending = false;
                    this.Close();
                }, 200);

                Helper.Animate(this, OpacityProperty, 150, 0, 0.7, 0.3);
                Helper.Animate(this, LeftProperty, 180, pcenter.X - ActualWidth, 0.7, 0.3);

                e.Cancel = true;
            }
        }

        private void AddressBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) this.Close();
            if (e.Key == Key.Escape)
            {
                AddressBox.Text = string.Empty;
                this.Close();
            }
        }
    }
}