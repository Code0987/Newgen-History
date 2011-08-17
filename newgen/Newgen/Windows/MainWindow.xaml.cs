using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newgen.Base;
using Newgen.Controls;
using Newgen.Core;

namespace Newgen.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal List<WidgetControl> runningWidgets;
        private ToolbarWindow toolbar;
        private ThumbnailsBar thumbBar;
        private BottomToolbar bottomToolbar;
        private LockScreen lockscreen;

        public MainWindow()
        {
            InitializeComponent();

            E.Play(new Uri("Cache\\Sounds\\Windows Logon Sound.wav", UriKind.RelativeOrAbsolute));

            if (App.Settings.IsExclusiveMode)
            {
                this.Background = new SolidColorBrush(E.BackgroundColor);
                DragScroll.DragEverywhere = App.Settings.DragEverywhere;

                if (App.Settings.UseBgImage)
                {
                    try { this.Background = new ImageBrush(E.GetBitmap(E.BgImage)); }
                    catch (Exception)
                    {
                        MessageBox.Show("Cannot use background image feature now. Problem with image.", "Error");
                    }
                }
            }
        }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;

            Dwm.RemoveFromAeroPeek(handle);
            Dwm.RemoveFromAltTab(handle);
            Dwm.RemoveFromFlip3D(handle);

            toolbar = new ToolbarWindow();
            toolbar.Opacity = 0;
            toolbar.Show();
            if (App.Settings.EnableThumbnailsBar)
            {
                thumbBar = new ThumbnailsBar();
                thumbBar.Opacity = 0;
                thumbBar.Show();
            }

            iFr.Helper.Animate(this, OpacityProperty, 1000, 0, 1);
            iFr.Helper.Delay(new Action(() =>
            {
                if (this.thumbBar != null) { this.thumbBar.Opacity = 1; }
                this.toolbar.Opacity = 1;
            }), 1500);

            lockscreen = new LockScreen();
            this.Root.Children.Add(lockscreen);
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.Width = SystemParameters.PrimaryScreenWidth;
            if (!App.Settings.IsExclusiveMode)
            {
                this.Height = SystemParameters.PrimaryScreenHeight - 1;
                this.Top = 1;
            }
            else
            {
                this.Height = SystemParameters.PrimaryScreenHeight;
                this.Top = 0;
            }
            this.Left = 0;

            // DRH
            double tileswidth = SystemParameters.PrimaryScreenWidth * 2;
            int c = (int)Math.Round(tileswidth / E.MinTileWidth);
            double tilesheight = this.WidgetsContainer.ActualHeight - (20); // -20 for the scrollbar
            int r = (int)(tilesheight / (E.MinTileHeight - E.TileSpacing * 2));

            App.WindowManager.Initialize(c, r);
            MarkupGrid();

            runningWidgets = new List<WidgetControl>();
            App.WidgetManager.WidgetLoaded += WidgetManagerWidgetLoaded;
            App.WidgetManager.WidgetUnloaded += WidgetManagerWidgetUnloaded;
            foreach (var loadedWidget in App.Settings.LoadedWidgets)
            {
                WidgetProxy widget;
                if (!string.IsNullOrEmpty(loadedWidget.Name) && string.IsNullOrEmpty(loadedWidget.Id))
                {
                    widget = App.WidgetManager.Widgets.Find(x => x.Name == loadedWidget.Name);
                    if (widget == null)
                        continue;

                    widget.Row = loadedWidget.Row;
                    widget.Column = loadedWidget.Column;
                    App.WidgetManager.LoadWidget(widget);
                }
                else if (!string.IsNullOrEmpty(loadedWidget.Path))
                {
                    widget = App.WidgetManager.CreateWidget(loadedWidget.Path);
                    widget.Row = loadedWidget.Row;
                    widget.Column = loadedWidget.Column;
                    App.WidgetManager.LoadWidget(widget);
                }
                else
                {
                    widget = App.WidgetManager.CreateFriendWidget(loadedWidget.Id, loadedWidget.Name);
                    widget.Row = loadedWidget.Row;
                    widget.Column = loadedWidget.Column;
                    App.WidgetManager.LoadWidget(widget);
                }
            }

            // UserTile
            this.LoadUserTileInfo();
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            E.Play(new Uri("Cache\\Sounds\\Windows Shutdown.wav", UriKind.RelativeOrAbsolute));
            App.Settings.LoadedWidgets.Clear();
            foreach (var runningWidget in runningWidgets)
            {
                var loadedWidget = new LoadedWidget();
                loadedWidget.Column = Grid.GetColumn(runningWidget);
                loadedWidget.Row = Grid.GetRow(runningWidget);
                loadedWidget.Name = runningWidget.WidgetProxy.Name;
                if (runningWidget.WidgetProxy.Path != null && (runningWidget.WidgetProxy.Path.Contains(@"\") || runningWidget.WidgetProxy.Path.Contains(@"/")))
                    loadedWidget.Path = runningWidget.WidgetProxy.Path;
                else
                    loadedWidget.Id = runningWidget.WidgetProxy.Path;
                App.Settings.LoadedWidgets.Add(loadedWidget);
            }
        }

        private void WindowMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (App.Settings.EnableThumbnailsBar && thumbBar == null)
            {
                thumbBar = new ThumbnailsBar();
                thumbBar.Show();
            }
            if (thumbBar != null) thumbBar.Activate();
            toolbar.Activate();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void ControlMouseMove(object sender, MouseEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (App.Settings.DragEverywhere == true)
                {
                    DragScroll.DragEverywhere = false;
                }
            }
            else
            {
                if (App.Settings.DragEverywhere == true)
                {
                    DragScroll.DragEverywhere = true;
                }
            }
            if (DragScroll.IsDragging)
                return;

            var widget = (WidgetControl)sender;
            if (e.LeftButton == MouseButtonState.Released && !widget.MousePressed)
                return;
            if (Mouse.Captured == sender)
            {
                Canvas.SetLeft((UIElement)sender, e.GetPosition(DragCanvas).X - mouseX);
                Canvas.SetTop((UIElement)sender, e.GetPosition(DragCanvas).Y - mouseY);
            }

            var drag = Math.Abs(e.GetPosition(widget).X - mouseX) >= 15 || Math.Abs(e.GetPosition(widget).Y - mouseY) >= 15;

            if (Mouse.Captured == null && drag)
            {
                this.Topmost = true;
                Mouse.Capture(widget);
                if (DragCanvas.Children.Contains(widget))
                    return;
                WidgetHost.Children.Remove(widget);
                DragCanvas.Children.Add(widget);
                bottomToolbar = new BottomToolbar();
                if (App.Settings.IsExclusiveMode)
                    bottomToolbar.Width = App.WindowManager.Region.Width * 2;
                else
                    bottomToolbar.Width = App.WindowManager.Region.Width;

                Canvas.SetTop(bottomToolbar, this.Height - bottomToolbar.Height);
                Canvas.SetZIndex(bottomToolbar, -1);
                bottomToolbar.OpenToolbar();
                DragCanvas.Children.Add(bottomToolbar);
                Canvas.SetLeft(widget, e.GetPosition(DragCanvas).X - mouseX);
                Canvas.SetTop(widget, e.GetPosition(DragCanvas).Y - mouseY);
                var col = Grid.GetColumn(widget);
                var row = Grid.GetRow(widget);
                var colspan = Grid.GetColumnSpan(widget);

                App.WindowManager.Matrix.FreeSpace(col, row, colspan);
                widget.Width = E.MinTileHeight * colspan;
                widget.Height = E.MinTileWidth;
                if (App.Settings.ShowGrid)
                    WidgetHost.ShowGridLines = true;
                widget.WidgetProxy.Column = -1;
                widget.WidgetProxy.Row = -1;
            }
        }

        private void ControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Captured != sender)
                return;
            this.Topmost = false;
            Mouse.Capture(null);
            bottomToolbar.CloseToolbar();
            var widget = (WidgetControl)sender;
            var coords = widget.TransformToAncestor(DragCanvas).Transform(new Point(0, 0));
            DragCanvas.Children.Remove(widget);
            WidgetHost.Children.Add(widget);

            var colSpan = Grid.GetColumnSpan(widget);

            //var column = (int)Math.Truncate(e.GetPosition(DragCanvas).X / E.MinTileWidth);
            var column = (int)Math.Truncate((coords.X + E.MinTileWidth / 2) / E.MinTileWidth);
            //var row = (int)Math.Truncate(e.GetPosition(DragCanvas).Y / E.MinTileHeight);
            var row = (int)Math.Truncate((coords.Y + E.MinTileHeight / 2) / E.MinTileHeight);
            var isFree = App.WindowManager.Matrix.IsCellFree(column, row, colSpan);
            if (!isFree ||
                (column >= App.WindowManager.Matrix.ColumnsCount || row >= App.WindowManager.Matrix.RowsCount))
                PlaceWidget(widget);
            else
            {
                Grid.SetColumn(widget, column);
                Grid.SetRow(widget, row);
                widget.Width = colSpan * E.MinTileWidth - E.TileSpacing * 2;
                widget.Height = E.MinTileHeight - E.TileSpacing * 2;
                App.WindowManager.Matrix.ReserveSpace(column, row, colSpan);
            }
            if (App.Settings.ShowGrid)
                WidgetHost.ShowGridLines = false;
            widget.MousePressed = false;
            if (e.GetPosition(DragCanvas).Y >= this.Height - bottomToolbar.Height)
                App.WidgetManager.UnloadWidget(widget.WidgetProxy);
        }

        private double mouseX, mouseY;

        private void ControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Captured == sender)
            {
                Canvas.SetLeft((UIElement)sender, e.GetPosition(DragCanvas).X - mouseX);
                Canvas.SetTop((UIElement)sender, e.GetPosition(DragCanvas).Y - mouseY);
                return;
            }

            var widget = (WidgetControl)sender;
            mouseX = e.GetPosition((IInputElement)sender).X;
            mouseY = e.GetPosition((IInputElement)sender).Y;
            /*var widget = (WidgetControl)sender;
            Mouse.Capture(widget);
            WidgetHost.Children.Remove(widget);
            DragCanvas.Children.Add(widget);
            Canvas.SetLeft(widget, e.GetPosition(DragCanvas).X - mouseX);
            Canvas.SetTop(widget, e.GetPosition(DragCanvas).Y - mouseY);
            var col = Grid.GetColumn(widget);
            var row = Grid.GetRow(widget);
            var colspan = Grid.GetColumnSpan(widget);

            App.WindowManager.Matrix.FreeSpace(col, row, colspan);
            widget.Width = E.MinTileHeight * colspan;
            widget.Height = E.MinTileWidth;
            WidgetHost.ShowGridLines = true;
            widget.WidgetProxy.Column = -1;
            widget.WidgetProxy.Row = -1;*/
        }

        private void CloseItemClick(object sender, RoutedEventArgs e)
        {
            this.Close();
            Application.Current.Shutdown();
        }

        private void Header_TitleText_MouseUp(object sender, MouseButtonEventArgs e)
        {
            E.Play(new Uri("Cache\\Sounds\\Windows Menu Command.wav", UriKind.RelativeOrAbsolute));
            DragScroll.ScrollToLeftEnd();
        }

        private void Button_Lock_Click(object sender, RoutedEventArgs e)
        {
            E.Play(new Uri("Cache\\Sounds\\Windows Menu Command.wav", UriKind.RelativeOrAbsolute));

            try { WinAPI.LockWorkStation(); }
            catch { }
        }

        private void WidgetManagerWidgetLoaded(WidgetProxy widget)
        {
            var control = new WidgetControl(widget);
            control.Order = WidgetHost.Children.Count;
            control.MouseLeftButtonDown += ControlMouseLeftButtonDown;
            control.MouseLeftButtonUp += ControlMouseLeftButtonUp;
            control.MouseMove += ControlMouseMove;
            runningWidgets.Add(control);
            control.Load();
            PlaceWidget(control);
            WidgetHost.Children.Add(control);
            if (widget.WidgetComponent != null)
            {
                if (widget.WidgetComponent.GetType() == typeof(Newgen.Core.NewgenAppWidget))
                {
                    if (App.Settings.IsAppWidgetBgStatic)
                    {
                        try
                        {
                            control.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Settings.AppWidgetBackgroundColor));
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        private void WidgetManagerWidgetUnloaded(WidgetProxy widget)
        {
            var control = runningWidgets.Find(x => x.WidgetProxy == widget);
            if (control == null)
                return;
            iFr.Helper.Animate(control, OpacityProperty, 250, 0, 0.7, 0.3);
            iFr.Helper.Delay(new Action(() =>
            {
                control.MouseLeftButtonDown -= ControlMouseLeftButtonDown;
                control.MouseLeftButtonUp -= ControlMouseLeftButtonUp;
                control.MouseMove -= ControlMouseMove;
                var col = Grid.GetColumn(control);
                var row = Grid.GetRow(control);
                var colspan = Grid.GetColumnSpan(control);
                WidgetHost.Children.Remove(control);
                runningWidgets.Remove(control);
                control.Unload();
                App.WindowManager.Matrix.FreeSpace(col, row, colspan);
            }), 250);
        }

        public void PlaceWidget(WidgetControl widget)
        {
            var colSpan = Grid.GetColumnSpan(widget);
            NewgenCell cell;
            if (widget.WidgetProxy.Column == -1 || widget.WidgetProxy.Row == -1)
                cell = App.WindowManager.Matrix.GetFreeCell(colSpan);
            else
                cell = new NewgenCell(widget.WidgetProxy.Column, widget.WidgetProxy.Row);
            Grid.SetColumn(widget, (int)cell.Column);
            Grid.SetRow(widget, (int)cell.Row);
            App.WindowManager.Matrix.ReserveSpace(cell.Column, cell.Row, colSpan);
            widget.Width = colSpan * E.MinTileWidth - E.TileSpacing * 2;
            widget.Height = E.MinTileHeight - E.TileSpacing * 2;
        }

        public void MarkupGrid()
        {
            WidgetHost.RowDefinitions.Clear();
            WidgetHost.ColumnDefinitions.Clear();

            for (var i = 0; i < App.WindowManager.Matrix.RowsCount; i++)
            {
                var row = new RowDefinition();
                row.Height = new GridLength(E.MinTileHeight);
                WidgetHost.RowDefinitions.Add(row);
            }

            for (var i = 0; i < App.WindowManager.Matrix.ColumnsCount; i++)
            {
                var column = new ColumnDefinition();
                column.Width = new GridLength(E.MinTileWidth);
                WidgetHost.ColumnDefinitions.Add(column);
            }
        }

        internal void LoadUserTileInfo()
        {
            if (!App.Settings.IsUserTileEnabled) { this.UserTile.Visibility = Visibility.Collapsed; }
            else
            {
                this.UserTile.Visibility = Visibility.Visible;
                this.UserTile.Opacity = 0;
                iFr.Helper.Animate(this.UserTile, OpacityProperty, 1000, 1);

                try
                {
                    if (File.Exists(System.IO.Path.GetTempPath() + "\\" + Environment.UserName + ".bmp"))
                    { File.Copy(System.IO.Path.GetTempPath() + "\\" + Environment.UserName + ".bmp", E.UserImage, true); }
                    Header_UserName.Text = Environment.UserName;
                    BitmapSource bs = E.GetBitmap(E.UserImage);
                    Header_UserPic.Source = bs == null ? Header_UserPic.Source : bs;
                }
                catch { throw; }
            }
        }
    }
}