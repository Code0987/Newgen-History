using System;
using System.Windows;
using System.Windows.Interop;
using Ftware.Apps.MetroShell.Base;
using Ftware.Apps.MetroShell.Native;

namespace Ftware.Apps.MetroShell.Windows
{
    /// <summary>
    /// Interaction logic for ExitOptionUI.xaml
    /// </summary>
    public partial class ExitOptionUI : Window
    {
        public ExitOptionUI()
        {
            InitializeComponent();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;

            Dwm.RemoveFromAeroPeek(handle);
            Dwm.RemoveFromAltTab(handle);
            Dwm.RemoveFromFlip3D(handle);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Helper.Animate(this, OpacityProperty, 250, 0, 1, 0.7, 0.3);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StartSystem.ShowHideAnimationWindow(false, "...", false);
        }

        private void TouchSupportPin_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                Helper.Animate(this, OpacityProperty, 250, 1, 0, 0.3, 0.7);

                Helper.Delay(() =>
                {
                    this.Close();
                }, 260);
            }
            catch { }
        }

        private void Button_Click_Logoff(object sender, RoutedEventArgs e)
        {
            try { WinAPI.ExitWindowsEx(0, 0); }
            catch { }
        }

        private void Button_Click_Restart(object sender, RoutedEventArgs e)
        {
            try { System.Diagnostics.Process.Start("shutdown.exe", "-r -f"); }
            catch { }
        }

        private void Button_Click_Shutdown(object sender, RoutedEventArgs e)
        {
            try { System.Diagnostics.Process.Start("shutdown.exe", "-s -f"); }
            catch { }
        }

        private void Button_Click_Exit(object sender, RoutedEventArgs e)
        {
            try
            {
                Helper.Animate(this, OpacityProperty, 250, 1, 0, 0.3, 0.7);

                Helper.Delay(() =>
                {
                    this.Close();

                    App.Current.Shutdown(0);
                }, 260);
            }
            catch { }
        }
    }
}