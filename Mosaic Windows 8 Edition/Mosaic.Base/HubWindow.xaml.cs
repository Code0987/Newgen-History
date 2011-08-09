using System;
using System.Windows;
using System.Windows.Input;

namespace Mosaic.Base
{
    /// <summary>
    /// Interaction logic for HubWindow.xaml
    /// </summary>
    public partial class HubWindow : Window
    {
        public HubWindow()
        {
            InitializeComponent();
        }

        private void WindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            iFr.Helper.Animate(this, OpacityProperty, 500, 0, 1);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            iFr.Helper.Animate(this, OpacityProperty, 250, 0);
        }
    }
}