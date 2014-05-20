using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Ftware.Apps.MetroShell.Base;
using Ftware.Apps.MetroShell.Core;

namespace Ftware.Apps.MetroShell.Controls
{
    public partial class WidgetControl : UserControl
    {
        public readonly WidgetProxy WidgetProxy;

        public bool MousePressed;

        private ContextMenu contextMenu;

        private MenuItem removeItem;
        private MenuItem refreshItem;

        private int order = 0;

        public int Order
        {
            get { return order; }
            set
            {
                order = value;

                var s = Resources["LoadAnim"] as Storyboard;
                s.BeginTime = TimeSpan.FromMilliseconds(500 + 25 * value);
            }
        }

        public WidgetControl()
        {
            InitializeComponent();
        }

        public WidgetControl(WidgetProxy widgetProxy)
        {
            InitializeComponent();

            this.WidgetProxy = widgetProxy;
        }

        public void Load()
        {
            FocusManager.SetIsFocusScope(this, true);

            contextMenu = new ContextMenu();
            removeItem = new MenuItem();
            removeItem.Header = "Remove";
            removeItem.Click += RemoveItemClick;
            contextMenu.Items.Add(removeItem);
            this.ContextMenu = contextMenu;

            WidgetProxy.Load();

            Root.Children.Clear();
            if (WidgetProxy.WidgetComponent.WidgetControl != null) Root.Children.Add(WidgetProxy.WidgetComponent.WidgetControl);

            this.Width = E.MinTileWidth * WidgetProxy.WidgetComponent.ColumnSpan - E.TileSpacing * 2 * (WidgetProxy.WidgetComponent.ColumnSpan - 1);
            this.Height = E.MinTileHeight - E.TileSpacing * 2;
            this.Margin = new Thickness(E.TileSpacing);

            Grid.SetColumnSpan(this, WidgetProxy.WidgetComponent.ColumnSpan);

            if (WidgetProxy.WidgetType == WidgetType.Html)
            {
                WidgetProxy.WidgetComponent.WidgetControl.MouseLeftButtonDown += WidgetControlMouseLeftButtonDown;
                WidgetProxy.WidgetComponent.WidgetControl.MouseLeftButtonUp += WidgetControlMouseLeftButtonUp;
                WidgetProxy.WidgetComponent.WidgetControl.MouseMove += WidgetControlMouseMove;
            }
            if (WidgetProxy.WidgetType == WidgetType.Generated)
            {
                if (!string.IsNullOrEmpty(WidgetProxy.Path) && WidgetProxy.Path.StartsWith("http://"))
                {
                    refreshItem = new MenuItem();
                    refreshItem.Header = "Refresh";
                    refreshItem.Click += RefreshItemClick;
                    contextMenu.Items.Add(refreshItem);
                }
            }

            try
            {
                RootOuter.Background = ((UserControl)(WidgetProxy.WidgetComponent.WidgetControl)).Background;
                ((UserControl)(WidgetProxy.WidgetComponent.WidgetControl)).Background = null;

                Root.Clip = new RectangleGeometry(new Rect(0, 0, Width, Height), RootOuter.CornerRadius.TopLeft, RootOuter.CornerRadius.BottomRight);

                ColorAnimation ca = (this.Resources["MouseDownAnim"] as Storyboard).Children[0] as ColorAnimation;
                ca.To = Settings.Current.ThemeColor2;
            }
            catch { }

            try
            {
                DoubleAnimation doubleAnimation1 = new DoubleAnimation()
                {
                    From = 0.4,
                    To = 1,
                    Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                    AccelerationRatio = 0.3,
                    DecelerationRatio = 0.7
                };
                Scale.BeginAnimation(System.Windows.Media.ScaleTransform.CenterXProperty, doubleAnimation1);
                DoubleAnimation doubleAnimation2 = new DoubleAnimation()
                {
                    From = 100,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                    AccelerationRatio = 0.3,
                    DecelerationRatio = 0.7
                };
                Translate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, doubleAnimation2);
            }
            catch { }
        }

        public void Unload()
        {
            try
            {
                DoubleAnimation doubleAnimation1 = new DoubleAnimation()
               {
                   From = 1,
                   To = 0.4,
                   Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                   AccelerationRatio = 0.3,
                   DecelerationRatio = 0.7
               };
                Scale.BeginAnimation(System.Windows.Media.ScaleTransform.CenterXProperty, doubleAnimation1);
                DoubleAnimation doubleAnimation2 = new DoubleAnimation()
                {
                    From = 0,
                    To = -100,
                    Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                    AccelerationRatio = 0.3,
                    DecelerationRatio = 0.7
                };
                Translate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, doubleAnimation2);
                Helper.Animate(this, OpacityProperty, 250, 0, 0.3, 0.7);
            }
            catch { }

            if (refreshItem != null) refreshItem.Click -= RefreshItemClick;
            WidgetProxy.Unload();
            Root.Children.Clear();
        }

        private void RefreshItemClick(object sender, RoutedEventArgs e)
        {
            WidgetProxy.WidgetComponent.Refresh();

            InvalidateVisual();
        }

        private void RemoveItemClick(object sender, RoutedEventArgs e)
        {
            removeItem.Click -= RemoveItemClick;
            App.WidgetManager.UnloadWidget(WidgetProxy);
        }

        private void WidgetControlMouseMove(object sender, MouseEventArgs e)
        {
            this.RaiseEvent(e);
        }

        private void WidgetControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MousePressed = false;
            if (!Settings.Current.TilesLock) this.RaiseEvent(e);
        }

        private void WidgetControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MousePressed = true;
            this.RaiseEvent(e);
        }

        private void UserControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ColorAnimation ca = (this.Resources["MouseDownAnim"] as Storyboard).Children[0] as ColorAnimation;
                ca.To = Settings.Current.ThemeColor2;
            }
            catch { }
            var s = Resources["MouseDownAnim"] as Storyboard;
            s.Begin();
            if (!Settings.Current.TilesLock)
            {
                MousePressed = true;
                Keyboard.Focus(this);
                FocusManager.SetFocusedElement(this, this);
                var a = Keyboard.FocusedElement;
            }
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var s = Resources["MouseUpAnim"] as Storyboard;
            s.Begin();
            MousePressed = false;
        }

        private void UserControlMouseLeave(object sender, MouseEventArgs e)
        {
            if (!MousePressed) return;
            MousePressed = false;
        }
    }
}