using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Screen = System.Windows.Forms.Screen;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
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

            var s = Resources["ToolbarCloseAnim"] as Storyboard;
            ((DoubleAnimation)s.Children[0]).To = SystemParameters.PrimaryScreenWidth - 1;
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

            s.BeginTime = TimeSpan.FromMilliseconds(800);
            s.Begin();
            s.BeginTime = TimeSpan.FromMilliseconds(300);

            isOpened = false;
        }

        void ItemMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
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

            foreach (var window in App.Current.Windows)
            {
                if (window.GetType() == typeof(HubWindow))
                    continue;
                ((Window)window).Activate();
                ((Window)window).Show();
            }
        }

        public void Open()
        {
            var s = Resources["ToolbarOpenAnim"] as Storyboard;
            ((DoubleAnimation)s.Children[0]).To = SystemParameters.PrimaryScreenWidth - 120;
            //((DoubleAnimation)s.Children[0]).To = totalWorkingAreaWidth - 120;

            s.Begin();
            isOpened = true;
        }

        public void CloseToolbar()
        {
            var s = Resources["ToolbarCloseAnim"] as Storyboard;
            ((DoubleAnimation)s.Children[0]).To = SystemParameters.PrimaryScreenWidth - 1; 
            //((DoubleAnimation)s.Children[0]).To = totalWorkingAreaWidth - 1;

            s.Begin();
            isOpened = false;

        }

        private void ExitButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void SettingsButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            App.ShowOptions();
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
        void ItemMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
