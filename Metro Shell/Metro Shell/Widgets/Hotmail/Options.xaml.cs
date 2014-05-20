using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ftware.Apps.MetroShell.Base;
using Ftware.Apps.MetroShell.Base.Messaging;

namespace Hotmail
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class Options : Window
    {
        public event EventHandler UpdateSettings;

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

            UsernameBox.Text = Widget.Settings.Username;
            PassBox.Password = Widget.Settings.Password;
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
            if ((UsernameBox.Text != Widget.Settings.Username) && (Widget.Settings.Username != null))
                resetMsgCount();

            Widget.Settings.Username = UsernameBox.Text;
            Widget.Settings.Password = PassBox.Password;

            Widget.Settings.Save(E.WidgetsRoot + "\\Hotmail\\Hotmail.config");
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

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void resetMsgCount()
        {
            Widget.Settings.LastMsgCount = -1;
        }
    }
}