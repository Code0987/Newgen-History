using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Mosaic.Base;
using Mosaic.Controls;

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
            BuildTag.Text = version + ".beta." + fileInfo.LastWriteTimeUtc.ToString("yyMMdd-HHmm");

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

            EnableExclusiveCheckBox.IsChecked = App.Settings.IsExclusiveMode;
            EnableAnimationCheckBox.IsChecked = App.Settings.AnimationEnabled;
            EnableThumbBarCheckBox.IsChecked = App.Settings.EnableThumbnailsBar;
            EnableUserTile.IsChecked = App.Settings.IsUserTileEnabled;
            CheckBox_DragEW.IsChecked = App.Settings.DragEverywhere;
            MosaicBgColor.Fill = new SolidColorBrush(E.BackgroundColor);
            BgColorAlpha.ValueChanged += new RoutedPropertyChangedEventHandler<double>(BgColorAlpha_ValueChanged);
            BgColorAlpha.Value = (double)(int)E.BackgroundColor.A;
            EnableStaticAppWidgetBg.IsChecked = App.Settings.IsAppWidgetBgStatic;
            try
            {
                AppWidgetBgColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Settings.AppWidgetBackgroundColor));
            }
            catch
            {
            }
            ValLockTime.Text = App.Settings.LockScreenTime.ToString();

            this.CheckBoxClick(this.EnableStaticAppWidgetBg, new RoutedEventArgs());

            iFr.Helper.Animate(this, OpacityProperty, 500, 0, 1);
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
            if (App.Settings.IsExclusiveMode != (bool)EnableExclusiveCheckBox.IsChecked)
            {
                restartRequired = true;
            }
            if (App.Settings.DragEverywhere != (bool)CheckBox_DragEW.IsChecked)
            {
                restartRequired = true;
            }
            App.Settings.IsExclusiveMode = (bool)EnableExclusiveCheckBox.IsChecked;
            App.Settings.AnimationEnabled = (bool)EnableAnimationCheckBox.IsChecked;
            App.Settings.EnableThumbnailsBar = (bool)EnableThumbBarCheckBox.IsChecked;
            App.Settings.DragEverywhere = (bool)CheckBox_DragEW.IsChecked;
            App.Settings.IsUserTileEnabled = (bool)EnableUserTile.IsChecked;

            if (string.IsNullOrEmpty(ValLockTime.Text) || string.IsNullOrWhiteSpace(ValLockTime.Text))
            { App.Settings.LockScreenTime = -1; }
            else { Convert.ToInt32(ValLockTime.Text); }

            var color = ((SolidColorBrush)this.AppWidgetBgColor.Fill).Color;
            App.Settings.AppWidgetBackgroundColor = color.ToString();

            if (App.Settings.IsAppWidgetBgStatic == true && !(bool)EnableStaticAppWidgetBg.IsChecked)
            {
                restartRequired = true;
            }
            else
            {
                foreach (WidgetControl c in ((MainWindow)App.Current.MainWindow).runningWidgets)
                {
                    if (c.WidgetProxy.WidgetComponent != null)
                    {
                        if (c.WidgetProxy.WidgetComponent.GetType() == typeof(Mosaic.Core.MosaicAppWidget))
                        {
                            c.Background = new SolidColorBrush(color);
                            c.InvalidateVisual();
                        }
                    }
                }
            }
            App.Settings.IsAppWidgetBgStatic = (bool)EnableStaticAppWidgetBg.IsChecked;

            var lastLang = App.Settings.Language;
            if (LanguageComboBox.SelectedIndex >= 0)
                App.Settings.Language = langCodes[LanguageComboBox.SelectedIndex];
            if (!restartRequired)
                restartRequired = lastLang != App.Settings.Language;

            App.Settings.Save(E.Root + "\\Mosaic.config");

            // Update UserTile
            if (App.Settings.IsExclusiveMode)
            {
                var window = (MainWindow)App.Current.MainWindow;
                window.LoadUserTileInfo();
            }

            iFr.Helper.Animate(this, OpacityProperty, 250, 0);
        }

        private void SiteLinkMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WinAPI.ShellExecute(IntPtr.Zero, "open", "http://mosaicwin8.codeplex.com", string.Empty, string.Empty, 0);
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            WinAPI.ShellExecute(IntPtr.Zero, "open", "https://mail.google.com/mail/?shva=1#compose", string.Empty, string.Empty, 0);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to reset Mosaic settings(This will not remove your widgets) ?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                App.Settings.BackgroundColor = "#FF250931";
                App.Settings.Autostart = false;
                App.Settings.Language = CultureInfo.CurrentUICulture.Name;
                App.Settings.IsExclusiveMode = true;
                App.Settings.AnimationEnabled = true;
                App.Settings.ShowGrid = false;
                App.Settings.UseSoftwareRendering = false;
                App.Settings.BackgroundColor = "#FF250931";
                App.Settings.DragEverywhere = true;
                App.Settings.EnableThumbnailsBar = Dwm.IsGlassAvailable() && Dwm.IsGlassEnabled();
                App.Settings.IsUserTileEnabled = true;
                App.Settings.IsAppWidgetBgStatic = false;
                App.Settings.LockScreenTime = -1;
                App.Settings.AppWidgetBackgroundColor = "#FF000000";
                App.Settings.Save(E.Root + "\\Mosaic.config");
            }
        }

        private void ComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void CheckBoxClick(object sender, RoutedEventArgs e)
        {
            if (sender == this.EnableStaticAppWidgetBg)
            {
                if ((bool)this.EnableStaticAppWidgetBg.IsChecked)
                {
                    this.TextAppWidgetBgColor.Visibility = Visibility.Visible;
                    this.AppWidgetBgColor.Visibility = Visibility.Visible;
                    this.ChangeAppWidgetBgColorButton.Visibility = Visibility.Visible;
                    iFr.Helper.Animate(this.TextAppWidgetBgColor, OpacityProperty, 250, 0, 1);
                    iFr.Helper.Animate(this.AppWidgetBgColor, OpacityProperty, 250, 0, 1);
                    iFr.Helper.Animate(this.ChangeAppWidgetBgColorButton, OpacityProperty, 250, 0, 1);
                }
                else
                {
                    iFr.Helper.Animate(this.TextAppWidgetBgColor, OpacityProperty, 250, 0);
                    iFr.Helper.Animate(this.AppWidgetBgColor, OpacityProperty, 250, 0);
                    iFr.Helper.Animate(this.ChangeAppWidgetBgColorButton, OpacityProperty, 250, 0);
                    iFr.Helper.Delay(new Action(() =>
                    {
                        this.TextAppWidgetBgColor.Visibility = Visibility.Collapsed;
                        this.AppWidgetBgColor.Visibility = Visibility.Collapsed;
                        this.ChangeAppWidgetBgColorButton.Visibility = Visibility.Collapsed;
                    }), 200);
                }
            }
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void ChangeBgColorButtonClick(object sender, RoutedEventArgs e)
        {
            var c = new System.Windows.Forms.ColorDialog();
            if (c.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var color = Color.FromArgb(E.BackgroundColor.A, c.Color.R, c.Color.G, c.Color.B);
                App.Settings.BackgroundColor = color.ToString();
                E.BackgroundColor = color;
                MosaicBgColor.Fill = new SolidColorBrush(E.BackgroundColor);
                BgColorAlpha.Value = (double)(int)color.A;
                if (App.Settings.IsExclusiveMode)
                {
                    var window = (MainWindow)App.Current.MainWindow;
                    window.Background = new SolidColorBrush(E.BackgroundColor);
                }
            }
        }

        private void ChangeAppWidgetBgColorButtonClick(object sender, RoutedEventArgs e)
        {
            var c = new System.Windows.Forms.ColorDialog();
            if (c.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.AppWidgetBgColor.Fill = new SolidColorBrush(Color.FromArgb(c.Color.A, c.Color.R, c.Color.G, c.Color.B));
            }
        }

        private void ValLockTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(ValLockTime.Text) || string.IsNullOrWhiteSpace(ValLockTime.Text))
                {
                    return;
                }

                int anInteger;
                anInteger = Convert.ToInt32(ValLockTime.Text);
                anInteger = int.Parse(ValLockTime.Text);
            }
            catch (Exception ex)
            {
                ValLockTime.Text = App.Settings.LockScreenTime.ToString();
            }
        }

        private void BgColorAlpha_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Color c = (Color)ColorConverter.ConvertFromString(App.Settings.BackgroundColor);
            var color = Color.FromArgb((byte)e.NewValue, c.R, c.G, c.B);
            App.Settings.BackgroundColor = color.ToString();
            E.BackgroundColor = color;
            MosaicBgColor.Fill = new SolidColorBrush(E.BackgroundColor);
            if (App.Settings.IsExclusiveMode)
            {
                var window = (MainWindow)App.Current.MainWindow;
                window.Background = new SolidColorBrush(E.BackgroundColor);
            }
        }
    }
}