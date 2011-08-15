using System;
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
    public partial class ToolbarWindow : Window
    {
        private bool isOpened;

        public ToolbarWindow()
        {
            if (!App.Settings.IsExclusiveMode)
            {
                this.Height = SystemParameters.WorkArea.Height;
                this.Top = SystemParameters.WorkArea.Top;
                this.Opacity = 1;
            }
            else
            {
                this.Height = SystemParameters.PrimaryScreenHeight;
                this.Top = 0;
                this.Opacity = 1;
            }

            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.Left = SystemParameters.PrimaryScreenWidth - this.Width;

            try
            {
                var s = Resources["ToolbarCloseAnim"] as Storyboard;
                ((DoubleAnimation)s.Children[0]).To = SystemParameters.PrimaryScreenWidth - (TouchSupportPin.ActualWidth + 1);
                s.BeginTime = TimeSpan.FromMilliseconds(800);
                s.Begin();
                s.BeginTime = TimeSpan.FromMilliseconds(300);
            }
            catch (Exception)
            {
                throw;
            }

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
            ((DoubleAnimation)s.Children[0]).To = SystemParameters.PrimaryScreenWidth - this.Width;

            s.Begin();
            iFr.Helper.Animate(TouchSupportPin, OpacityProperty, 180, 0);
            isOpened = true;
            lc = new LeftClock();
            lc.Show();

            try
            {
                var Stb_UT = ((MainWindow)App.Current.MainWindow).UserTile.Resources["LeftAnim"] as Storyboard;
                ((DoubleAnimation)Stb_UT.Children[0]).To = -this.Width;
                ((DoubleAnimation)Stb_UT.Children[0]).AccelerationRatio = 0.7;
                ((DoubleAnimation)Stb_UT.Children[0]).DecelerationRatio = 0.3;
                Stb_UT.Begin();
            }
            catch { }

            iFr.Helper.Animate(this.StartButton, OpacityProperty, 300, 0, 1);
            iFr.Helper.Animate(this.TilesButton, OpacityProperty, 400, 0, 1);
            iFr.Helper.Animate(this.PeopleButton, OpacityProperty, 500, 0, 1);
            iFr.Helper.Animate(this.SettingsButton, OpacityProperty, 600, 0, 1);
            //iFr.Helper.Animate(this.ExitButton, OpacityProperty, 700, 0, 1);
        }

        public void CloseToolbar()
        {
            var s = Resources["ToolbarCloseAnim"] as Storyboard;
            ((DoubleAnimation)s.Children[0]).To = SystemParameters.PrimaryScreenWidth - (TouchSupportPin.ActualWidth + 1);

            s.Begin();
            iFr.Helper.Animate(TouchSupportPin, OpacityProperty, 180, 1);
            isOpened = false;
            if (lc != null)
            {
                iFr.Helper.Animate(lc, OpacityProperty, 200, 0);
                iFr.Helper.Delay(new Action(() =>
                {
                    lc.Close();
                    lc = null;
                }), 200);
            }

            try
            {
                var Stb_UT = ((MainWindow)App.Current.MainWindow).UserTile.Resources["LeftAnim"] as Storyboard;
                ((DoubleAnimation)Stb_UT.Children[0]).To = 0;
                ((DoubleAnimation)Stb_UT.Children[0]).AccelerationRatio = 0.3;
                ((DoubleAnimation)Stb_UT.Children[0]).DecelerationRatio = 0.7;
                Stb_UT.Begin();
            }
            catch { }
        }

        private void MainContentMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void SettingsButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            App.ShowOptions();
        }

        private void StartButtonMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                App.Current.MainWindow.Hide();
            }
            else
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
        }

        private void SearchButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var sh = new SearchHub();
            sh.Show();
        }

        private void TilesButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TilesListGrid.Visibility = System.Windows.Visibility.Visible;
            TilesList.Visibility = System.Windows.Visibility.Visible;
        }

        private void WidgetsButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TilesList.Visibility = System.Windows.Visibility.Collapsed;
            WidgetsList.Visibility = System.Windows.Visibility.Visible;
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
            TilesListGrid.Visibility = System.Windows.Visibility.Collapsed;
            foreach (ToolbarItem item in WidgetsList.Children)
            {
                item.MouseLeftButtonDown -= ItemMouseLeftButtonDown;
                item.MouseLeftButtonUp -= ItemMouseLeftButtonUp;
            }

            WidgetsList.Children.Clear();
        }

        private void ToolbarCloseAnimCompleted(object sender, EventArgs e)
        {
            TilesList.Visibility = System.Windows.Visibility.Collapsed;
            WidgetsList.Visibility = System.Windows.Visibility.Collapsed;
            TilesListGrid.Visibility = System.Windows.Visibility.Collapsed;
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