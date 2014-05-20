using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Ftware.Apps.MetroShell.Base;
using Ftware.Apps.MetroShell.Controls;
using Ftware.Apps.MetroShell.Core;
using Ftware.Apps.MetroShell.Native;

namespace Ftware.Apps.MetroShell.Windows
{
    public partial class TilesToolBar : Window
    {
        private bool isOpened;
        DateTime mouseclicktimestamp = DateTime.Now;
        private double mouseX, mouseY;

        public TilesToolBar()
        {
            InitializeComponent();

            Effects.Background = new SolidColorBrush(Settings.Current.ThemeColor2);
            TouchSupportPin.Background = new SolidColorBrush(Settings.Current.ThemeColor2);
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            Dwm.RemoveFromAeroPeek(handle);
            Dwm.RemoveFromAltTab(handle);
            Dwm.RemoveFromFlip3D(handle);

            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = 140;
            this.Top = -this.ActualHeight;
            this.Left = 0;

            this.Opacity = 1;

            isOpened = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Effects.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 8.0,
                ShadowDepth = 3.0,
                Opacity = 1
            };
            Root.Margin = new Thickness(0, 0, 0, 10);
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (!isOpened) OpenToolbar();
                else CloseToolbar();
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Topmost = true;
            if (!isOpened)
            {
                OpenToolbar();
            }
        }

        private void TouchSupportPin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CloseToolbar();
        }

        public void OpenToolbar()
        {
            try
            {
                this.Width = SystemParameters.PrimaryScreenWidth;
                this.Height = 140;
                this.Top = -this.ActualHeight;
                this.Left = 0;

                this.Opacity = 1;

                Helper.Animate(this, TopProperty, 200, -ActualHeight, -1, 0.7, 0.3);

                isOpened = true;

                try
                {
                    foreach (var w in App.WidgetManager.Widgets)
                    {
                        if (w.WidgetType == WidgetType.Generated)
                            continue;
                        var item = new TilesToolbarItem();
                        item.Title = w.Name;
                        if (w.WidgetComponent.IconPath == null)
                            item.Icon = new BitmapImage(new Uri("/Resources/pin tiles.png", UriKind.Relative));
                        else
                            item.Icon = new BitmapImage((w.WidgetComponent.IconPath));
                        item.MouseLeftButtonDown += ItemMouseLeftButtonDown;
                        item.MouseLeftButtonUp += ItemMouseLeftButtonUp;
                        WidgetsList.Children.Add(item);
                    }

                    try
                    {
                        for (int i = 0; i < this.WidgetsList.Children.Count; i++)
                        {
                            try
                            {
                                TilesToolbarItem item = null;
                                int order = 1;
                                if (i < this.WidgetsList.Children.Count / 2)
                                {
                                    item = ((TilesToolbarItem)this.WidgetsList.Children[i]);
                                    order = this.WidgetsList.Children.Count - i;
                                    item.Translate.Y = 400;
                                }
                                else
                                {
                                    item = ((TilesToolbarItem)this.WidgetsList.Children[i]);
                                    order = this.WidgetsList.Children.Count - (this.WidgetsList.Children.Count - i);
                                    item.Translate.Y = -400;
                                }

                                DoubleAnimation doubleAnimation1 = new DoubleAnimation()
                                {
                                    To = 0,
                                    Duration = new Duration(TimeSpan.FromMilliseconds(800)),
                                    BeginTime = new TimeSpan?(TimeSpan.FromMilliseconds((double)(order * 50 + 10))),
                                    AccelerationRatio = 0.1,
                                    DecelerationRatio = 0.5,
                                    FillBehavior = FillBehavior.Stop
                                };
                                doubleAnimation1.Completed += (a, b) => { item.Translate.Y = 0; };
                                item.Translate.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, doubleAnimation1);
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
                catch { }
            }
            catch { }
        }

        public void CloseToolbar()
        {
            try
            {
                isOpened = false;

                Helper.Animate(this, TopProperty, 200, -this.ActualHeight, 0.3, 0.7);

                Helper.Delay(() =>
                {
                    try { RemoveItems(); }
                    catch { }

                    this.Close();

                    WinAPI.FlushMemory();
                }, 200);
            }
            catch { }
        }

        private void ItemMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int timediff = (int)DateTime.Now.Subtract(mouseclicktimestamp).TotalMilliseconds;
            mouseclicktimestamp = DateTime.Now;
            if (timediff < E.AnimationTimePrecision)
            { return; }

            if (mouseX != e.GetPosition(this).X || mouseY != e.GetPosition(this).Y)
                return;

            var name = ((TilesToolbarItem)sender).Title;
            if (App.WidgetManager.IsWidgetLoaded(name))
                App.WidgetManager.UnloadWidget(name);
            else
                App.WidgetManager.LoadWidget(name);
        }

        private void ItemMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseX = e.GetPosition(this).X;
            mouseY = e.GetPosition(this).Y;
        }

        private void RemoveItems()
        {
            foreach (TilesToolbarItem item in WidgetsList.Children)
            {
                item.MouseLeftButtonDown -= ItemMouseLeftButtonDown;
                item.MouseLeftButtonUp -= ItemMouseLeftButtonUp;
            }

            WidgetsList.Children.Clear();
        }
    }
}