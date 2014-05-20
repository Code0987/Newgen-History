using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ftware.Apps.MetroShell.Base;
using Ftware.Apps.MetroShell.Base.Messaging;
using TweetSharp;

namespace Twitter
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class Options : Window
    {
        public event EventHandler UpdateSettings;
        private OAuthRequestToken requestToken;

        public Options()
        {
            InitializeComponent();
        }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            this.Top = 0;
            this.Left = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            BuildTag.Text = version + "." + fileInfo.LastWriteTimeUtc.ToString("yyMMdd-HHmm");
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            ApplySettings();
            this.Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ApplyButtonClick(object sender, RoutedEventArgs e)
        {
            ApplySettings();
        }

        private void SiteLinkMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessagingHelper.SendMessageToMetroShell("URL", "http://ftware.blogspot.com");
        }

        private void ApplySettings()
        {
            Widget.Settings.Save(E.WidgetsRoot + "\\Twitter\\Twitter.config");
            if (UpdateSettings != null)
            {
                UpdateSettings(null, EventArgs.Empty);
            }
        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void PasswordBoxTextChanged(object sender, RoutedEventArgs e)
        {
        }

        private void SignInBrowserButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                requestToken = TwitterWidget.Service.GetRequestToken();
                var uri = TwitterWidget.Service.GetAuthorizationUri(requestToken);
                Process.Start(uri.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SignInButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PinBox.Text) || requestToken == null)
                return;

            var access = TwitterWidget.Service.GetAccessToken(requestToken, PinBox.Text);
            Widget.Settings.AccessToken = access.Token;
            Widget.Settings.AccessTokenSecret = access.TokenSecret;
            Widget.Settings.Save(E.WidgetsRoot + "\\Twitter\\Twitter.config");
            this.Close();
            UpdateSettings(null, EventArgs.Empty);
        }

        private void PinBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (PinBox.Text.Length < 5)
                SignInButton.IsEnabled = false;
            else
            {
                SignInButton.IsEnabled = true;
            }
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}