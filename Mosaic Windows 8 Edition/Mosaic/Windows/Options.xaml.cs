using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Mosaic.Base;

namespace Mosaic.Windows
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        private readonly List<string> langCodes = new List<string>();
        private bool restartRequired;

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

            LanguageComboBox.Items.Add(new ComboBoxItem() { Content = CultureInfo.GetCultureInfo("en-US").NativeName });
            langCodes.Add("en-US");
            var langs = from x in Directory.GetDirectories(E.Root) where x.Contains("-") select System.IO.Path.GetFileNameWithoutExtension(x);
            foreach (var l in langs)
            {
                try
                {
                    var c = CultureInfo.GetCultureInfo(l);
                    langCodes.Add(c.Name);
                    LanguageComboBox.Items.Add(new ComboBoxItem() { Content = c.NativeName });
                }
                catch { }
            }

            LanguageComboBox.Text = CultureInfo.GetCultureInfo(App.Settings.Language).NativeName;

          //  EnableExclusiveCheckBox.IsChecked = App.Settings.IsExclusiveMode;
            EnableAnimationCheckBox.IsChecked = App.Settings.AnimationEnabled;
            EnableThumbBarCheckBox.IsChecked = App.Settings.EnableThumbnailsBar;

           // MosaicBgColor.Fill = new SolidColorBrush(E.BackgroundColor);
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            if (restartRequired)
            {
                foreach (Window window in App.Current.Windows)
                {
                    window.Close();
                }

                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
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

        private void ApplySettings()
        {
            /* if (App.Settings.IsExclusiveMode != (bool)EnableExclusiveCheckBox.IsChecked)
                restartRequired = true; */

           // App.Settings.IsExclusiveMode = (bool)EnableExclusiveCheckBox.IsChecked;
            App.Settings.AnimationEnabled = (bool)EnableAnimationCheckBox.IsChecked;
            App.Settings.EnableThumbnailsBar = (bool)EnableThumbBarCheckBox.IsChecked;

            var lastLang = App.Settings.Language;
            if (LanguageComboBox.SelectedIndex >= 0)
                App.Settings.Language = langCodes[LanguageComboBox.SelectedIndex];
            if (!restartRequired)
                restartRequired = lastLang != App.Settings.Language;

            App.Settings.Save(E.Root + "\\Mosaic.config");
        }


        private void SiteLinkMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WinAPI.ShellExecute(IntPtr.Zero, "open", "http://mosaicwin8.codeplex.com", string.Empty, string.Empty, 0);
        }

        private void ComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CheckBoxClick(object sender, RoutedEventArgs e)
        {

        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            WinAPI.ShellExecute(IntPtr.Zero, "open", "https://mail.google.com/mail/?shva=1#compose", string.Empty, string.Empty, 0);
        }

     /* private void ChangeBgColorButtonClick(object sender, RoutedEventArgs e)
        {
            var c = new System.Windows.Forms.ColorDialog();
            if (c.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var color = Color.FromArgb(E.BackgroundColor.A, c.Color.R, c.Color.G, c.Color.B);
                App.Settings.BackgroundColor = color.ToString();
                E.BackgroundColor = color;
                MosaicBgColor.Fill = new SolidColorBrush(E.BackgroundColor);
                if (App.Settings.IsExclusiveMode)
                {
                    var window = (MainWindow)App.Current.MainWindow;
                    window.Background = new SolidColorBrush(E.BackgroundColor);
                }
            }
        } */
    }
}
