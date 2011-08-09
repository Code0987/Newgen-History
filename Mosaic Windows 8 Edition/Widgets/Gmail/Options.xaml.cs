using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Mosaic.Base;

namespace Gmail
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
            BuildTag.Text = version + ".alpha." + fileInfo.LastWriteTimeUtc.ToString("yyMMdd-HHmm");

            UsernameBox.Text = iFr.Helper.Decrypt(Widget.Settings.Username);
            PassBox.Password = iFr.Helper.Decrypt(Widget.Settings.Password);
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
            WinAPI.ShellExecute(IntPtr.Zero, "open", "http://mosaicproject.codeplex.com", string.Empty, string.Empty, 0);
        }

        private void ApplySettings()
        {
            Widget.Settings.Username = iFr.Helper.Encrypt(UsernameBox.Text);
            Widget.Settings.Password = iFr.Helper.Encrypt(PassBox.Password);

            Widget.Settings.Save(E.WidgetsRoot + "\\Gmail\\Gmail.config");
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
    }
}