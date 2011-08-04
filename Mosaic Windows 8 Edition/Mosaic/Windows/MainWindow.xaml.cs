using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Mosaic.Base;
using Mosaic.Controls;
using Mosaic.Core;

namespace Mosaic.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<WidgetControl> runningWidgets;
        private ToolbarWindow toolbar;
        private ThumbnailsBar thumbBar;
        private BottomToolbar bottomToolbar;

        public MainWindow()
        {
            InitializeComponent();

            E.BackgroundColor = (Color)ColorConverter.ConvertFromString(App.Settings.BackgroundColor);

            if (App.Settings.IsExclusiveMode)
            {
                this.Background = new SolidColorBrush(E.BackgroundColor);
                DragScroll.DragEverywhere = App.Settings.DragEverywhere;
                //this.AllowsTransparency = false;
            }
        }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;

            Dwm.RemoveFromAeroPeek(handle);
            Dwm.RemoveFromAltTab(handle);
            Dwm.RemoveFromFlip3D(handle);

            if (App.Settings.EnableThumbnailsBar)
            {
                thumbBar = new ThumbnailsBar();
                thumbBar.Show();
            }

            toolbar = new ToolbarWindow();
            toolbar.Show();
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

            App.WindowManager.Initialize();
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

        }

        void WidgetManagerWidgetLoaded(WidgetProxy widget)
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
        }

        void WidgetManagerWidgetUnloaded(WidgetProxy widget)
        {
            var control = runningWidgets.Find(x => x.WidgetProxy == widget);
            if (control == null)
                return;
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
        }
        
        void ControlMouseMove(object sender, MouseEventArgs e)
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

        void ControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

        void ControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

        private void MarkupGrid()
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

        public void PlaceWidget(WidgetControl widget)
        {
            var colSpan = Grid.GetColumnSpan(widget);
            MosaicCell cell;
            if (widget.WidgetProxy.Column == -1 || widget.WidgetProxy.Row == -1)
                cell = App.WindowManager.Matrix.GetFreeCell(colSpan);
            else
                cell = new MosaicCell(widget.WidgetProxy.Column, widget.WidgetProxy.Row);
            Grid.SetColumn(widget, (int)cell.Column);
            Grid.SetRow(widget, (int)cell.Row);
            App.WindowManager.Matrix.ReserveSpace(cell.Column, cell.Row, colSpan);
            widget.Width = colSpan * E.MinTileWidth - E.TileSpacing * 2;
            widget.Height = E.MinTileHeight - E.TileSpacing * 2;
            //((Window)widget).Left = cell.Column * (E.MinTileWidth * colSpan) + E.TileSpacing;
            //((Window)widget).Top = cell.Row * (E.MinTileHeight) + E.TileSpacing;
        }


        private void CloseItemClick(object sender, RoutedEventArgs e)
        {
            this.Close();
            Application.Current.Shutdown();
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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

            if (thumbBar != null)
                thumbBar.Activate();

            toolbar.Activate();
        }
    }
}
