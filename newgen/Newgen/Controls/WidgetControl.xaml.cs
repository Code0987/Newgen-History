using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Newgen.Base;
using Newgen.Core;

namespace Newgen.Controls
{
    /// <summary>
    /// Interaction logic for NewgenTile.xaml
    /// </summary>
    public partial class WidgetControl : UserControl
    {
        public readonly WidgetProxy WidgetProxy; private int order = 0;
        public bool MousePressed;
        private ContextMenu contextMenu;
        private MenuItem removeItem;
        private MenuItem refreshItem;

        public int Order
        {
            get { return order; }
            set
            {
                order = value;

                var s = Resources["LoadAnim"] as Storyboard;
                s.BeginTime = TimeSpan.FromMilliseconds(10 + 70 * value);
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
            WidgetProxy.Load();
            Root.Children.Clear();
            Root.Children.Add(WidgetProxy.WidgetComponent.WidgetControl);
            this.Width = E.MinTileWidth * WidgetProxy.WidgetComponent.ColumnSpan - E.TileSpacing * 2 * (WidgetProxy.WidgetComponent.ColumnSpan - 1);
            this.Height = E.MinTileHeight - E.TileSpacing * 2;
            this.Margin = new Thickness(E.TileSpacing);
            Grid.SetColumnSpan(this, WidgetProxy.WidgetComponent.ColumnSpan);

            /*if (WidgetProxy.WidgetType == WidgetType.Html)
            {
                WidgetProxy.WidgetComponent.WidgetControl.MouseLeftButtonDown += WidgetControlMouseLeftButtonDown;
                WidgetProxy.WidgetComponent.WidgetControl.MouseLeftButtonUp += WidgetControlMouseLeftButtonUp;
                WidgetProxy.WidgetComponent.WidgetControl.MouseMove += WidgetControlMouseMove;
                if (!App.Settings.IsExclusiveMode)
                {
                    this.Opacity = 1;
                    var tb = new TextBlock();
                    tb.FontSize = 18;
                    tb.Foreground = Brushes.White;
                    tb.TextWrapping = TextWrapping.Wrap;
                    tb.Margin = new Thickness(10);
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.Text = Properties.Resources.WidgetInvisible;
                    Root.Children.Add(tb);
                }
            }
            else
            {*/
            var s = Resources["LoadAnim"] as Storyboard;
            s.Begin();
            iFr.Helper.Delay(new Action(() => { iFr.Helper.Animate(this, OpacityProperty, 1000, 1); }), s.BeginTime.HasValue ? s.BeginTime.Value.TotalMilliseconds : 0);

            if (WidgetProxy.WidgetType == WidgetType.Generated)
            {
                contextMenu = new ContextMenu();

                if (!string.IsNullOrEmpty(WidgetProxy.Path) && WidgetProxy.Path.StartsWith("http://"))
                {
                    refreshItem = new MenuItem();
                    refreshItem.Header = Properties.Resources.RefreshItem;
                    refreshItem.Click += RefreshItemClick;
                    contextMenu.Items.Add(refreshItem);
                }

                removeItem = new MenuItem();
                removeItem.Header = Properties.Resources.RemoveItem;
                removeItem.Click += RemoveItemClick;

                contextMenu.Items.Add(removeItem);
                this.ContextMenu = contextMenu;
            }
            //}
        }

        private void RefreshItemClick(object sender, RoutedEventArgs e)
        {
            WidgetProxy.WidgetComponent.Refresh();
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
            this.RaiseEvent(e);
        }

        private void WidgetControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MousePressed = true;
            this.RaiseEvent(e);
        }

        public void Unload()
        {
            if (refreshItem != null)
                refreshItem.Click -= RefreshItemClick;
            WidgetProxy.Unload();
            Root.Children.Clear();
        }

        private void StoryboardCompleted(object sender, EventArgs e)
        {
            //Opacity = 1;
        }

        private void UserControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var s = Resources["MouseDownAnim"] as Storyboard;
            s.Begin();
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                //Ctrl key is down

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
            var s = Resources["MouseUpAnim"] as Storyboard;
            s.Begin();
            if (!MousePressed)
                return;
            MousePressed = false;
        }
    }
}