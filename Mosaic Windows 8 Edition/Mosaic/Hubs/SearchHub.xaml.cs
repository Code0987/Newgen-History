using System;
using System.Windows;
using System.Windows.Input;
using Social.Base;

namespace Mosaic.Windows
{
    /// <summary>
    /// Interaction logic for PeopleHub.xaml
    /// </summary>
    public partial class SearchHub : Window
    {
        private SocialProvider socialProvider;

        public SearchHub()
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

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            iFr.Helper.Animate(this, OpacityProperty, 250, 0);
        }
    }
}