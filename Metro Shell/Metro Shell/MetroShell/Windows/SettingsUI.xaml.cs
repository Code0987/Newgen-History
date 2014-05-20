using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ftware.Apps.MetroShell.Base;
using Ftware.Apps.MetroShell.Base.Messaging;
using Ftware.Apps.MetroShell.Controls;
using Ftware.Apps.MetroShell.Core;
using Ftware.Apps.MetroShell.Native;
using Microsoft.Win32;

namespace Ftware.Apps.MetroShell.Windows
{
    /// <summary>
    /// Interaction logic for SettingsUI.xaml
    /// </summary>
    public partial class SettingsUI : Window
    {
        private bool restart = false;
        private bool isnolicense = false;

        public SettingsUI(bool nlic = false)
        {
            InitializeComponent();
            this.isnolicense = nlic;
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
            if (this.isnolicense)
            {
                this.Tab_UI.Visibility = Visibility.Collapsed;
                this.Tab_Tiles.Visibility = Visibility.Collapsed;
                this.Tab_Taskbar.Visibility = Visibility.Collapsed;
                this.Tab_General.Visibility = Visibility.Collapsed;

                this.Tabs.SelectedItem = this.Tab_About;
            }

            Helper.Animate(this, OpacityProperty, 250, 0, 1, 0.7, 0.3);

            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            TextBlock_About.Inlines.Add(new Run("Copyright © 2012 Metro Shell, ftware.\n" +
                                                "All rights reserved.\n"));

            // Load Settings
            try
            {
                Rect_ThemeColor1.Fill = new SolidColorBrush(Settings.Current.ThemeColor1);
                Rect_ThemeColor2.Fill = new SolidColorBrush(Settings.Current.ThemeColor2);
                CheckBox_BgImage.IsChecked = Settings.Current.UseBgImage;

                double tilesheight = StartSystem.StartScreen.TilesContainer.ActualHeight - (20);
                double rh = ((tilesheight - E.TileSpacing * 2) / 3);
                Slider_TilesSize.Maximum = (double)rh;
                Slider_TilesSize.Minimum = (double)E.MinTilesSize;
                Slider_TilesSize.Value = (double)E.MinTileHeight;
                Slider_TilesSize.ValueChanged += new RoutedPropertyChangedEventHandler<double>(Slider_TilesSize_ValueChanged);
                TextBox_TilesSpacing.Text = Settings.Current.TileSpacing.ToString();
                TextBox_TilesSpacing.TextChanged += new System.Windows.Controls.TextChangedEventHandler(TextBox_TilesSpacing_TextChanged);
                Button_TilesLock.Content = Settings.Current.TilesLock ? "UnLock" : "Lock";

                CheckBox_AutoStart.IsChecked = Settings.Current.Autostart;
                if (Settings.Current.TimeMode == 1) RadioButton_TimeFormat_12h.IsChecked = true;
                else RadioButton_TimeFormat_24h.IsChecked = true;
                Rect_UserTile_Image.Fill = new ImageBrush(E.GetBitmap(E.UserImage));
                TextBox_UserTile_Text.Text = Settings.Current.UserTileText;
                TextBox_UserTile_Text.TextChanged += new System.Windows.Controls.TextChangedEventHandler(TextBox_UserTile_Text_TextChanged);

                UpdateTaskBarPEXL();
            }
            catch { }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StartSystem.ShowHideAnimationWindow(false, "...", false);

            Settings.Save();

            if (restart)
            {
                StartSystem.ForEachWindow((wnd) =>
                {
                    wnd.Close();
                    return true;
                }, new System.Collections.Generic.List<Window>() { this });

                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

        private void TouchSupportPin_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                try
                {
                    SavePEXL();
                    StartSystem.tbtimer.Start();
                }
                catch { }

                Helper.Animate(this, OpacityProperty, 250, 1, 0, 0.3, 0.7);

                Helper.Delay(() =>
                {
                    this.Close();
                }, 260);
            }
            catch { }
        }

        private void SiteLink1_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MessagingHelper.SendMessageToMetroShell("URL", SiteLink1.Text);
        }

        private void Button_ThemeColor1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    var c = new System.Windows.Forms.ColorDialog();
                    if (c.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        var color = Color.FromRgb(c.Color.R, c.Color.G, c.Color.B);
                        Settings.Current.ThemeColor1 = color;
                        Rect_ThemeColor1.Fill = new SolidColorBrush(color);

                        Helper.RunMethodAsyncThreadSafe(() =>
                        {
                            StartSystem.StartScreen.Background = new SolidColorBrush(Settings.Current.ThemeColor1);

                            if (Settings.Current.UseBgImage)
                            {
                                try
                                {
                                    StartSystem.StartScreen.Background = new ImageBrush(E.GetBitmap(E.BgImage));
                                }
                                catch (Exception)
                                {
                                    Helper.ShowErrorMessage("Cannot use background image feature now. Problem with image.");
                                }
                            }
                        });
                    }
                }
                catch (Exception)
                {
                    Helper.ShowErrorMessage("Cannot update Theme Color.");
                }
            }
            catch
            {
                Helper.ShowErrorMessage("Cannot process your request.");
            }
        }

        private void Button_ThemeColor2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    var c = new System.Windows.Forms.ColorDialog();
                    if (c.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        var color = Color.FromRgb(c.Color.R, c.Color.G, c.Color.B);
                        Settings.Current.ThemeColor2 = color;
                        Rect_ThemeColor2.Fill = new SolidColorBrush(color);

                        Helper.RunMethodAsyncThreadSafe(() =>
                        {
                            try
                            {
                                StartSystem.StartScreen.Effects.Background = new SolidColorBrush(Settings.Current.ThemeColor2);
                                StartSystem.ContentRef.Background = new SolidColorBrush(Settings.Current.ThemeColor2);
                            }
                            catch { }
                        });
                    }
                }
                catch (Exception)
                {
                    Helper.ShowErrorMessage("Cannot update Theme Color.");
                }
            }
            catch
            {
                Helper.ShowErrorMessage("Cannot process your request.");
            }
        }

        private void CheckBox_BgImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Settings.Current.UseBgImage = (bool)CheckBox_BgImage.IsChecked;

                if (Settings.Current.UseBgImage)
                {
                    try
                    {
                        StartSystem.StartScreen.Background = new ImageBrush(E.GetBitmap(E.BgImage));
                    }
                    catch (Exception)
                    {
                        Helper.ShowErrorMessage("Cannot use background image feature now. Problem with image.");
                    }
                }
                else
                {
                    StartSystem.StartScreen.Background = new SolidColorBrush(Settings.Current.ThemeColor1);
                }
            }
            catch (Exception)
            {
                Helper.ShowErrorMessage("Cannot process your request.");
            }
        }

        private void Button_TSBG_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = E.ImageFilter;
                if (!(bool)dialog.ShowDialog()) return;

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

                    StartSystem.StartScreen.Background = new ImageBrush(bi);
                    Settings.Current.UseBgImage = true;
                }
                catch (Exception)
                {
                    Helper.ShowErrorMessage("Cannot use this feature now. Problem with image.");
                    Settings.Current.UseBgImage = false;
                }
            }
            catch
            {
                Helper.ShowErrorMessage("Cannot process your request.");
            }
        }

        private void Slider_TilesSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                Settings.Current.MinTileHeight = Slider_TilesSize.Value;
                Settings.Current.MinTileWidth = Settings.Current.MinTileHeight * 1;
                E.MinTileWidth = Settings.Current.MinTileWidth;
                E.MinTileHeight = Settings.Current.MinTileHeight;

                restart = true;
            }
            catch
            {
                Helper.ShowErrorMessage("Cannot process your request.");
            }
        }

        private void TextBox_TilesSpacing_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                try
                {
                    if (string.IsNullOrEmpty(TextBox_TilesSpacing.Text) || string.IsNullOrWhiteSpace(TextBox_TilesSpacing.Text))
                    {
                        return;
                    }

                    int anInteger;
                    anInteger = Convert.ToInt32(TextBox_TilesSpacing.Text);
                    anInteger = int.Parse(TextBox_TilesSpacing.Text);
                    bool valid = anInteger > 0;
                    if (valid)
                    {
                        Settings.Current.TileSpacing = anInteger;
                        E.TileSpacing = Settings.Current.TileSpacing;
                        restart = true;
                    }
                }
                catch
                {
                    TextBox_TilesSpacing.Text = Settings.Current.TileSpacing.ToString();
                }
            }
            catch
            {
                Helper.ShowErrorMessage("Cannot process your request.");
            }
        }

        private void Button_TilesLock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Button_TilesLock.Content.ToString().ToLower().Equals("lock"))
                {
                    Settings.Current.TilesLock = true;
                    Button_TilesLock.Content = "UnLock";
                }
                else
                {
                    Settings.Current.TilesLock = false;
                    Button_TilesLock.Content = "Lock";
                }
            }
            catch (Exception)
            {
                Helper.ShowErrorMessage("Cannot process your request.");
            }
        }

        private void CheckBox_AutoStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Settings.Current.Autostart = (bool)CheckBox_AutoStart.IsChecked;

                if (Settings.Current.Autostart)
                {
                    E.SetAutoStart(true);
                }
                else
                {
                    E.SetAutoStart(false);
                }
            }
            catch (Exception)
            {
                Helper.ShowErrorMessage("Cannot process your request.");
            }
        }

        private void RadioButton_TimeFormat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Settings.Current.TimeMode = (sender == RadioButton_TimeFormat_12h) ? 0 : 1;
            }
            catch (Exception)
            {
                Helper.ShowErrorMessage("Cannot process your request.");
            }
        }

        private void Button_UserTile_Image_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = E.ImageFilter;
                if (!(bool)dialog.ShowDialog()) return;
                try
                {
                    File.Copy(dialog.FileName, E.UserImage, true);
                    try
                    {
                        Rect_UserTile_Image.Fill = new ImageBrush(E.GetBitmap(E.UserImage));
                    }
                    catch { }

                    StartSystem.StartScreen.LoadUserTile();
                }
                catch (Exception)
                {
                    MessageBox.Show("Problem with user account image.", "Error");
                }
            }
            catch (Exception)
            {
                Helper.ShowErrorMessage("Cannot process your request.");
            }
        }

        private void TextBox_UserTile_Text_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                Settings.Current.UserTileText = TextBox_UserTile_Text.Text;
                StartSystem.StartScreen.LoadUserTile();
            }
            catch (Exception)
            {
                Helper.ShowErrorMessage("Cannot process your request.");
            }
        }

        private void TabItem_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //var lic = iFramework.Security.Licensing.LicenseManager.Current.LoadLicense(App.LicenseFile);
                //if (lic != null)
                //{
                //    if (lic.Status == iFramework.Security.Licensing.LicenseStatus.Invalid || lic.Status == iFramework.Security.Licensing.LicenseStatus.InternalError || lic.Status == iFramework.Security.Licensing.LicenseStatus.MachineHashMismatch || lic.Status == iFramework.Security.Licensing.LicenseStatus.NotFound || lic.Status == iFramework.Security.Licensing.LicenseStatus.Expired)
                //        Helper.ShowErrorMessage("License is not present or invalid or expired. So, you won't be able to access certain features of this application.");
                //    if (lic.Status == iFramework.Security.Licensing.LicenseStatus.TrialVersion)
                //        this.TextBlock_About_ActivationMessage.Text = ("In Trial Period, " + lic.TrialPeriodLeft.Days + "days left. You still can't use pro features of this application.");
                //    if (lic.Status == iFramework.Security.Licensing.LicenseStatus.Licensed)
                //        this.TextBlock_About_ActivationMessage.Text = ("License is present, was activated on " + lic.ActivationDate.ToShortDateString() + ".");
                //}
            }
            catch { }
        }

        private void Button_About_ActivationCode_Click(object sender, RoutedEventArgs e)
        {
            //var lic = iFramework.Security.Licensing.LicenseManager.Current.GetLicense(this.TextBox_About_ActivationCode.Text);
            //if (lic == null ||
            //    lic.Status == iFramework.Security.Licensing.LicenseStatus.Invalid ||
            //    lic.Status == iFramework.Security.Licensing.LicenseStatus.InternalError ||
            //    lic.Status == iFramework.Security.Licensing.LicenseStatus.MachineHashMismatch ||
            //    lic.Status == iFramework.Security.Licensing.LicenseStatus.Expired ||
            //    lic.Status == iFramework.Security.Licensing.LicenseStatus.NotFound)
            //{
            //    Helper.ShowErrorMessage("Failed to validate license code or the code is invalid.");
            //    return;
            //}
            //iFramework.Security.Licensing.LicenseManager.Current.SaveLicense(App.LicenseFile, lic);
            try
            {
                try
                {
                    SavePEXL();
                    StartSystem.tbtimer.Start();
                }
                catch { }

                Helper.Animate(this, OpacityProperty, 250, 1, 0, 0.3, 0.7);

                Helper.Delay(() =>
                {
                    Helper.ShowInfoMessage("Please manually re-start the application in order to apply new license. Now this application will exit.");
                    this.Close();
                    App.Current.Shutdown(1105);
                }, 260);
            }
            catch { }
        }

        private bool isLoadingTaskBarDatadone = false;

        private void UpdateTaskBarPEXL()
        {
            try
            {
                StartSystem.tbtimer.Stop();

                ListBox_ItemsToExclude.Items.Clear();

                IntPtr handle = ((System.Windows.Interop.HwndSource)System.Windows.Interop.HwndSource.FromVisual(this)).Handle;
                IntPtr current = WinAPI.GetWindow(handle, WinAPI.GetWindowCmd.First);

                do
                {
                    int GWL_STYLE = -16;
                    uint normalWnd = 0x10000000 | 0x00800000 | 0x00080000;
                    uint popupWnd = 0x10000000 | 0x80000000 | 0x00080000;
                    var windowLong = WinAPI.GetWindowLong(current, GWL_STYLE);
                    var text = WinAPI.GetText(current);
                    if (((normalWnd & windowLong) == normalWnd || (popupWnd & windowLong) == popupWnd) && !string.IsNullOrEmpty(text))
                    {
                        try
                        {
                            FileInfo fip = new FileInfo(WinAPI.GetProcessPath(current));
                            ListBox_ItemsToExclude.Items.Add(new TaskBarProcessExclusionData()
                            {
                                ProcessName = fip.Name,
                                Icon = IconExtractor.GetIcon(WinAPI.GetProcessPath(current))
                            });
                        }
                        catch { }
                    }

                    current = WinAPI.GetWindow(current, WinAPI.GetWindowCmd.Next);

                    if (current == handle) current = WinAPI.GetWindow(current, WinAPI.GetWindowCmd.Next);
                }
                while (current != IntPtr.Zero);

                List<TaskBarProcessExclusionData> addeditems = ListBox_ItemsToExclude.Items.OfType<TaskBarProcessExclusionData>().ToList();

                foreach (string item in Settings.Current.TaskBarProcessExclusionList)
                {
                    try
                    {
                        int existcount = 0;
                        foreach (TaskBarProcessExclusionData item2 in addeditems)
                        {
                            if (item2.ProcessName == item) { existcount++; ListBox_ItemsToExclude.SelectedItems.Add(item2); }
                        }
                        if (existcount <= 0)
                        {
                            TaskBarProcessExclusionData data = new TaskBarProcessExclusionData()
                                 {
                                     ProcessName = item
                                 };
                            ListBox_ItemsToExclude.Items.Add(data);
                            ListBox_ItemsToExclude.SelectedItems.Add(data);
                        }
                    }
                    catch { }
                }

                isLoadingTaskBarDatadone = true;
            }
            catch { }
        }

        private void ListBox_ItemsToExclude_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (isLoadingTaskBarDatadone)
            {
                //SavePEXL();
            }
        }

        private void SavePEXL()
        {
            try
            {
                Settings.Current.TaskBarProcessExclusionList.Clear();

                foreach (var item in ListBox_ItemsToExclude.SelectedItems.OfType<TaskBarProcessExclusionData>())
                {
                    Settings.Current.TaskBarProcessExclusionList.Add(item.ProcessName);
                }
            }
            catch { }

            try
            {
                foreach (string item in Settings.Current.TaskBarProcessExclusionList)
                {
                    foreach (Window wnd in App.Current.Windows)
                    {
                        if (wnd is StartSystem)
                        {
                            List<StartBarItem> items = ((StartSystem)wnd).Icons.Children.OfType<StartBarItem>().ToList();

                            foreach (StartBarItem item2 in items)
                            {
                                if (WinAPI.GetProcessPath(item2.Handles[0]).Contains(item)) ((StartSystem)wnd).RemoveIcon(item2);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void Button_AddPEXL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = "Executable Files|*.exe";
                if (!(bool)dialog.ShowDialog()) return;

                FileInfo fip = new FileInfo(dialog.FileName);

                TaskBarProcessExclusionData data = new TaskBarProcessExclusionData()
                {
                    ProcessName = fip.Name,
                    Icon = IconExtractor.GetIcon(dialog.FileName)
                };

                ListBox_ItemsToExclude.Items.Add(data);

                ListBox_ItemsToExclude.SelectedItems.Add(data);
            }
            catch (Exception)
            {
                Helper.ShowErrorMessage("Cannot process your request.");
            }
        }
    }
}