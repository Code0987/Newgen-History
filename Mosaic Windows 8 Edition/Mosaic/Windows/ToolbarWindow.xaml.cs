﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Mosaic.Base;
using Mosaic.Controls;
using Mosaic.Core;

namespace Mosaic.Windows
{
    /// <summary>
    /// Interaction logic for ToolbarWindow.xaml
    /// </summary>
    public partial class ToolbarWindow : Window
    {
        //private readonly int totalWorkingAreaWidth = 0;
        private bool isOpened;

        public ToolbarWindow()
        {
            if (!App.Settings.IsExclusiveMode)
            {
                this.Height = SystemParameters.WorkArea.Height;
                this.Top = SystemParameters.WorkArea.Top;
                //this.Left = SystemParameters.WorkArea.Width - 2;
                this.Opacity = 1;
            }
            else
            {
                this.Height = SystemParameters.PrimaryScreenHeight;
                this.Top = 0;
                //this.Left = SystemParameters.PrimaryScreenWidth - this.Width;
                this.Opacity = 1;
            }

            //List<Screen> screens = Screen.AllScreens.ToList();
            //get primary screen
            //Screen primaryScreen = screens.Where(x => x.Primary).SingleOrDefault();
            //get secondary screen
            //Screen secondaryScreen = screens.Where(x => !x.Primary).SingleOrDefault();

            //if (screens.Count > 1 && !object.Equals(primaryScreen, null) && !object.Equals(secondaryScreen, null))
            //{
            //    //secondary screen is to the left of the primary screen because it has a negative left position
            //    if (secondaryScreen.Bounds.Left < 0)
            //    {
            //        //calculate the primary screen width + secondary screen width + the position of the secondary screen
            //        totalWorkingAreaWidth = primaryScreen.Bounds.Width + secondaryScreen.Bounds.Width + secondaryScreen.Bounds.Left;
            //    }
            //    //secondary screen is to the right of the primary screen so only calculate total bounds width
            //    else
            //    {
            //        totalWorkingAreaWidth = primaryScreen.Bounds.Width + secondaryScreen.Bounds.Width;
            //    }
            //}
            //else
            //{
            //    //there's only one screen
            //    totalWorkingAreaWidth = !object.Equals(primaryScreen, null) ? primaryScreen.Bounds.Width : (int)SystemParameters.PrimaryScreenWidth;
            //}

            //Set the correct left position to get the correct handle(current screen toolbox is on) and can calculate the appropiate height
            //this.Left = totalWorkingAreaWidth - this.Width;

            InitializeComponent();
        }

        private void LoadUserPicFromCache()
        {
            var ms = new MemoryStream();
            var stream = new FileStream(E.Root + "\\Cache\\user.png", FileMode.Open, FileAccess.Read);
            ms.SetLength(stream.Length);
            stream.Read(ms.GetBuffer(), 0, (int)stream.Length);

            ms.Flush();
            stream.Close();

            var src = new BitmapImage();
            src.BeginInit();
            src.StreamSource = ms;
            src.EndInit();
            //UserPic.Icon = src;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(E.Root + "\\Cache\\user.png"))
            {
                LoadUserPicFromCache();
            }
            else if (File.Exists(System.IO.Path.GetTempPath() + "\\" + Environment.UserName + ".bmp"))
            {
                File.Copy(System.IO.Path.GetTempPath() + "\\" + Environment.UserName + ".bmp", E.Root + "\\Cache\\user.png", true);
                LoadUserPicFromCache();
            }

            //if (!App.Settings.IsExclusiveMode)
            //    this.Left = SystemParameters.WorkArea.Width - this.Width;
            //else
            this.Left = SystemParameters.PrimaryScreenWidth - this.Width;
            //this.Left = totalWorkingAreaWidth - this.Width;
            //foreach (var w in App.WidgetManager.Widgets)
            //{
            //    var item = new ToolbarItem();
            //    item.Title = w.Name;
            //    if (string.IsNullOrEmpty(w.WidgetComponent.IconPath))
            //        item.Icon = new BitmapImage(new Uri("/Resources/default_icon.png", UriKind.Relative));
            //    else
            //        item.Icon = new BitmapImage(new Uri(w.WidgetComponent.IconPath));
            //    //item.Order = WidgetsLibrary.Children.Count;
            //    item.MouseLeftButtonUp += ItemMouseLeftButtonUp;
            //    ItemsHost.Children.Add(item);
            //}

            try
            {
                var s = Resources["ToolbarCloseAnim"] as Storyboard;
                ((DoubleAnimation)s.Children[0]).To = SystemParameters.PrimaryScreenWidth - 1;
                s.BeginTime = TimeSpan.FromMilliseconds(800);
                s.Begin();
                s.BeginTime = TimeSpan.FromMilliseconds(300);
            }
            catch (Exception)
            {
                throw;
            }
            //((DoubleAnimation)s.Children[0]).To = totalWorkingAreaWidth - 1;

            //var handle = new WindowInteropHelper(this).Handle;
            //Screen screen = Screen.FromHandle(handle);

            //if (App.Settings.IsExclusiveMode)
            //{
            //    //fullscreen
            //    this.Top = screen.Bounds.Top; //recalculate the top. It's possible that the screen resolution of the current screen is different. It's also possible that this screen doesn't have a taskbar
            //    this.Height = screen.Bounds.Height;
            //}
            //else
            //{
            //    //we need to see the taskbar etc
            //    this.Top = screen.Bounds.Top;
            //    this.Height = screen.WorkingArea.Height;
            //}

            isOpened = false;
        }

        DateTime mouseclicktimestamp = DateTime.Now;

        private void ItemMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int timediff = (int)DateTime.Now.Subtract(mouseclicktimestamp).TotalMilliseconds;
            mouseclicktimestamp = DateTime.Now;
            if (timediff < E.AnimationTimePrecision)
            { return; }

            if (mouseX != e.GetPosition(this).X || mouseY != e.GetPosition(this).Y)
                return;

            var name = ((ToolbarItem)sender).Title;
            if (App.WidgetManager.IsWidgetLoaded(name))
                App.WidgetManager.UnloadWidget(name);
            else
                App.WidgetManager.LoadWidget(name);
        }

        private void ToolbarMouseLeave(object sender, MouseEventArgs e)
        {
            CloseToolbar();
        }

        private void WindowMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!isOpened)
            {
                Open();
            }
        }

        private LeftClock lc = null;

        public void Open()
        {
            var s = Resources["ToolbarOpenAnim"] as Storyboard;
            ((DoubleAnimation)s.Children[0]).To = SystemParameters.PrimaryScreenWidth - 120;
            //((DoubleAnimation)s.Children[0]).To = totalWorkingAreaWidth - 120;

            s.Begin();

            isOpened = true;
            lc = new LeftClock();
            lc.Show();
        }

        public void CloseToolbar()
        {
            var s = Resources["ToolbarCloseAnim"] as Storyboard;
            ((DoubleAnimation)s.Children[0]).To = SystemParameters.PrimaryScreenWidth - 1;
            //((DoubleAnimation)s.Children[0]).To = totalWorkingAreaWidth - 1;

            s.Begin();

            isOpened = false;
            iFr.Helper.Animate(lc, OpacityProperty, 200, 0);
            iFr.Helper.Delay(new Action(() =>
            {
                lc.Close();
                lc = null;
            }), 200);
        }

        private void ExitButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void SettingsButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            App.ShowOptions();
        }

        private void StartButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            foreach (var window in App.Current.Windows)
            {
                if (window.GetType() == typeof(HubWindow))
                    continue;
                ((Window)window).Activate();
                ((Window)window).Show();
            }
            try
            {
                foreach (WidgetControl c in ((MainWindow)App.Current.MainWindow).runningWidgets)
                {
                    iFr.Helper.Animate(c, OpacityProperty, 250, 0, 1);
                }
            }
            catch (Exception)
            {
            }
        }

        private void WidgetsButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WidgetsListGrid.Visibility = System.Windows.Visibility.Visible;
            foreach (var w in App.WidgetManager.Widgets)
            {
                if (w.WidgetType == WidgetType.Generated)
                    continue;
                var item = new ToolbarItem();
                item.Title = w.Name;
                if (w.WidgetComponent.IconPath == null)
                    item.Icon = new BitmapImage(new Uri("/Resources/default_icon.png", UriKind.Relative));
                else
                    item.Icon = new BitmapImage((w.WidgetComponent.IconPath));
                //item.Order = WidgetsLibrary.Children.Count;
                item.MouseLeftButtonDown += ItemMouseLeftButtonDown;
                item.MouseLeftButtonUp += ItemMouseLeftButtonUp;
                WidgetsList.Children.Add(item);
            }
        }

        private double mouseX, mouseY;

        private void ItemMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseX = e.GetPosition(this).X;
            mouseY = e.GetPosition(this).Y;
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WidgetsListGrid.Visibility = System.Windows.Visibility.Collapsed;
            foreach (ToolbarItem item in WidgetsList.Children)
            {
                item.MouseLeftButtonDown -= ItemMouseLeftButtonDown;
                item.MouseLeftButtonUp -= ItemMouseLeftButtonUp;
            }

            WidgetsList.Children.Clear();
        }

        private void ToolbarCloseAnimCompleted(object sender, EventArgs e)
        {
            WidgetsListGrid.Visibility = System.Windows.Visibility.Collapsed;
            foreach (ToolbarItem item in WidgetsList.Children)
            {
                item.MouseLeftButtonDown -= ItemMouseLeftButtonDown;
                item.MouseLeftButtonUp -= ItemMouseLeftButtonUp;
            }

            WidgetsList.Children.Clear();
        }

        private void PinWebButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var addressWindow = new AddressBarWindow();
            addressWindow.ShowDialog();
            if (string.IsNullOrEmpty(addressWindow.AddressBox.Text))
                return;
            var widget = App.WidgetManager.CreateWidget(addressWindow.AddressBox.Text);
            App.WidgetManager.LoadWidget(widget);
        }

        private void PinAppButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Executable files|*.exe";
            if (!(bool)dialog.ShowDialog())
                return;
            var widget = App.WidgetManager.CreateWidget(dialog.FileName);
            App.WidgetManager.LoadWidget(widget);
        }

        private void PeopleButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var peopleHub = new PeopleHub();
            peopleHub.Show();
        }

        private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (!isOpened)
                    Open();
                else
                    CloseToolbar();
            }
        }
    }
}