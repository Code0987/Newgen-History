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
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Newgen.Base;
using Newgen.Controls;

namespace Newgen.Windows
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private readonly List<string> langCodes = new List<string>();
        private bool restartRequired;

        public Settings()
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
            NewgenBgColor.Fill = new SolidColorBrush(E.BackgroundColor);
            BgColorAlpha.ValueChanged += new RoutedPropertyChangedEventHandler<double>(BgColorAlpha_ValueChanged);
            BgColorAlpha.Value = (double)(int)E.BackgroundColor.A;
            EnableStaticAppWidgetBg.IsChecked = App.Settings.IsAppWidgetBgStatic;
            EnableBgImage.IsChecked = App.Settings.UseBgImage;
            EnableAutoStartCheckBox.IsChecked = App.Settings.Autostart = E.GetAutoStart();
            if (App.Settings.TimeMode == 1) Time24HRadioButton.IsChecked = true;
            try
            {
                AppWidgetBgColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Settings.AppWidgetBackgroundColor));
            }
            catch
            {
            }

            double tilesheight = ((MainWindow)App.Current.MainWindow).WidgetsContainer.ActualHeight - (20);
            double rh = ((tilesheight - E.TileSpacing * 2) / 3);

            TilesSizeScale.Maximum = (double)rh;
            TilesSizeScale.Minimum = (double)E.MinTilesSize;
            TilesSizeScale.Value = (double)E.MinTileHeight;
            TilesSizeScale.ValueChanged += new RoutedPropertyChangedEventHandler<double>(TilesSizeScale_ValueChanged);
            ValTilesSpacing.Text = App.Settings.TileSpacing.ToString();
            ValLockTime.Text = App.Settings.LockScreenTime.ToString();

            this.CheckBoxClick(this.EnableStaticAppWidgetBg, new RoutedEventArgs());
            this.CheckBoxClick(this.EnableBgImage, new RoutedEventArgs());

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
            if (App.Settings.IsExclusiveMode != (bool)EnableExclusiveCheckBox.IsChecked) { restartRequired = true; }
            if (App.Settings.DragEverywhere != (bool)CheckBox_DragEW.IsChecked) { restartRequired = true; }
            App.Settings.IsExclusiveMode = (bool)EnableExclusiveCheckBox.IsChecked;
            App.Settings.AnimationEnabled = (bool)EnableAnimationCheckBox.IsChecked;
            App.Settings.EnableThumbnailsBar = (bool)EnableThumbBarCheckBox.IsChecked;
            App.Settings.DragEverywhere = (bool)CheckBox_DragEW.IsChecked;
            App.Settings.IsUserTileEnabled = (bool)EnableUserTile.IsChecked;
            App.Settings.UseBgImage = (bool)EnableBgImage.IsChecked;
            App.Settings.Autostart = (bool)EnableAutoStartCheckBox.IsChecked;
            if (Time24HRadioButton.IsChecked == true) App.Settings.TimeMode = 1;
            else App.Settings.TimeMode = 0;

            try
            {
                E.SetAutoStart(App.Settings.Autostart);
            }
            catch { }
            var color = ((SolidColorBrush)this.AppWidgetBgColor.Fill).Color;
            App.Settings.AppWidgetBackgroundColor = color.ToString();
            if (App.Settings.IsAppWidgetBgStatic == true && !(bool)EnableStaticAppWidgetBg.IsChecked) { restartRequired = true; }
            else
            {
                foreach (WidgetControl c in ((MainWindow)App.Current.MainWindow).runningWidgets)
                {
                    if (c.WidgetProxy.WidgetComponent != null)
                    {
                        if (c.WidgetProxy.WidgetComponent.GetType() == typeof(Newgen.Core.NewgenAppWidget))
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

            App.Settings.Save(E.Root + "\\Newgen.config");

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
            WinAPI.ShellExecute(IntPtr.Zero, "open", "http://Newgenwin8.codeplex.com", string.Empty, string.Empty, 0);
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            WinAPI.ShellExecute(IntPtr.Zero, "open", "https://mail.google.com/mail/?shva=1#compose", string.Empty, string.Empty, 0);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to reset Newgen settings(This will not remove your widgets) ?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
                App.Settings.UseBgImage = false;
                App.Settings.Save(E.Root + "\\Newgen.config");
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
            if (sender == this.EnableBgImage)
            {
                if ((bool)this.EnableBgImage.IsChecked)
                {
                    this.TextBgImg.Visibility = Visibility.Visible;
                    this.ChangeBgImg.Visibility = Visibility.Visible;
                    this.NewgenBgColor.Visibility = Visibility.Collapsed;
                    this.TextBgColor.Visibility = Visibility.Collapsed;
                    this.ChangeBgColorButton.Visibility = Visibility.Collapsed;
                    this.TextBgTrans.Visibility = Visibility.Collapsed;
                    this.BgColorAlpha.Visibility = Visibility.Collapsed;
                    iFr.Helper.Animate(this.TextBgImg, OpacityProperty, 250, 0, 1);
                    iFr.Helper.Animate(this.ChangeBgImg, OpacityProperty, 250, 0, 1);
                    iFr.Helper.Animate(this.NewgenBgColor, OpacityProperty, 250, 0);
                    iFr.Helper.Animate(this.TextBgColor, OpacityProperty, 250, 0);
                    iFr.Helper.Animate(this.ChangeBgColorButton, OpacityProperty, 250, 0);
                    iFr.Helper.Animate(this.TextBgTrans, OpacityProperty, 250, 0);
                    iFr.Helper.Animate(this.BgColorAlpha, OpacityProperty, 250, 0);
                }
                else
                {
                    iFr.Helper.Animate(this.TextBgImg, OpacityProperty, 250, 0);
                    iFr.Helper.Animate(this.ChangeBgImg, OpacityProperty, 250, 0);
                    iFr.Helper.Animate(this.NewgenBgColor, OpacityProperty, 250, 0, 1);
                    iFr.Helper.Animate(this.TextBgColor, OpacityProperty, 250, 0, 1);
                    iFr.Helper.Animate(this.ChangeBgColorButton, OpacityProperty, 250, 0, 1);
                    iFr.Helper.Animate(this.TextBgTrans, OpacityProperty, 250, 0, 1);
                    iFr.Helper.Animate(this.BgColorAlpha, OpacityProperty, 250, 0, 1);
                    iFr.Helper.Delay(new Action(() =>
                    {
                        this.TextBgImg.Visibility = Visibility.Collapsed;
                        this.ChangeBgImg.Visibility = Visibility.Collapsed;
                        this.NewgenBgColor.Visibility = Visibility.Visible;
                        this.TextBgColor.Visibility = Visibility.Visible;
                        this.ChangeBgColorButton.Visibility = Visibility.Visible;
                        this.TextBgTrans.Visibility = Visibility.Visible;
                        this.BgColorAlpha.Visibility = Visibility.Visible;
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
                NewgenBgColor.Fill = new SolidColorBrush(E.BackgroundColor);
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
                if (string.IsNullOrEmpty(ValLockTime.Text) || string.IsNullOrWhiteSpace(ValLockTime.Text)) { App.Settings.LockScreenTime = -1; }
                else { App.Settings.LockScreenTime = Convert.ToInt32(ValLockTime.Text); }
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
            NewgenBgColor.Fill = new SolidColorBrush(E.BackgroundColor);
            if (App.Settings.IsExclusiveMode)
            {
                var window = (MainWindow)App.Current.MainWindow;
                window.Background = new SolidColorBrush(E.BackgroundColor);
            }
        }

        private void TilesSizeScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            App.Settings.MinTileHeight = TilesSizeScale.Value;

            App.Settings.MinTileWidth = App.Settings.MinTileHeight * E.TilesSizeFactor;
            E.MinTileWidth = App.Settings.MinTileWidth;
            E.MinTileHeight = App.Settings.MinTileHeight;

            try
            {
                ((MainWindow)App.Current.MainWindow).MarkupGrid();
                foreach (WidgetControl control in ((MainWindow)App.Current.MainWindow).runningWidgets)
                {
                    ((MainWindow)App.Current.MainWindow).PlaceWidget(control);
                }
            }
            catch { }
        }

        private void ChangeBgImgClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = E.ImageFilter;
            if (!(bool)dialog.ShowDialog())
                return;

            if (App.Settings.IsExclusiveMode)
            {
                var window = (MainWindow)App.Current.MainWindow;
                try
                {
                    if (!File.Exists(E.BgImage)) File.Create(E.BgImage);

                    byte[] bytArray = File.ReadAllBytes(dialog.FileName);
                    File.WriteAllBytes(E.BgImage, bytArray);

                    MemoryStream ms = new MemoryStream();
                    BitmapImage bi = new BitmapImage();

                    ms.Write(bytArray, 0, bytArray.Length); ms.Position = 0;
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.EndInit();

                    window.Background = new ImageBrush(bi);
                    App.Settings.UseBgImage = true;
                }
                catch (Exception)
                {
                    MessageBox.Show("Cannot use this feature now. Problem with image.", "Error");
                    App.Settings.UseBgImage = false;
                }
            }
        }

        private void ValTilesSpacing_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(ValTilesSpacing.Text) || string.IsNullOrWhiteSpace(ValTilesSpacing.Text))
                {
                    return;
                }

                int anInteger;
                anInteger = Convert.ToInt32(ValTilesSpacing.Text);
                anInteger = int.Parse(ValTilesSpacing.Text);
                bool valid = anInteger > 0;
                if (valid)
                {
                    App.Settings.TileSpacing = anInteger;
                    E.TileSpacing = App.Settings.TileSpacing;
                    try
                    {
                        ((MainWindow)App.Current.MainWindow).MarkupGrid();

                        foreach (WidgetControl control in ((MainWindow)App.Current.MainWindow).runningWidgets)
                        {
                            ((MainWindow)App.Current.MainWindow).PlaceWidget(control);
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                ValTilesSpacing.Text = App.Settings.LockScreenTime.ToString();
            }
        }
    }
}