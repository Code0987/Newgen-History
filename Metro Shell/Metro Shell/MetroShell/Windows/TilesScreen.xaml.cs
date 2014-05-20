using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ftware.Apps.MetroShell.Base;
using Ftware.Apps.MetroShell.Controls;
using Ftware.Apps.MetroShell.Core;
using Ftware.Apps.MetroShell.Native;
using Microsoft.Win32;

namespace Ftware.Apps.MetroShell.Windows
{
    public partial class TilesScreen : Window
    {
        internal List<WidgetControl> runningWidgets = new List<WidgetControl>();

        private bool isOpened;

        private double mouseX, mouseY;

        public TilesScreen()
        {
            InitializeComponent();

            Helper.RunMethodAsyncThreadSafe(() =>
            {
                this.Background = new SolidColorBrush(Settings.Current.ThemeColor1);
                if (Settings.Current.UseBgImage)
                {
                    try { this.Background = new ImageBrush(E.GetBitmap(E.BgImage)); }
                    catch (Exception)
                    {
                        Helper.ShowErrorMessage("Cannot use background image feature now. Problem with image.");
                    }
                }
                this.Effects.Background = new SolidColorBrush(Settings.Current.ThemeColor2);
            });
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            Dwm.RemoveFromAeroPeek(handle);
            Dwm.RemoveFromAltTab(handle);
            Dwm.RemoveFromFlip3D(handle);

            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            this.Top = 0;
            this.Left = 0;

            this.Toolbar_TT.X = -(this.Toolbar.Width + 10);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Helper.Delay(() =>
            {
                Effects.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 8.0,
                    ShadowDepth = 3.0,
                    Opacity = 1
                };

                this.Toolbar.Margin = new Thickness(0, 0, 10, 0);

                this.LoadUserTile();

                this.OpenToolbar();
            }, 200);

            Helper.Delay(() =>
            {
                Helper.RunMethodAsync(() =>
                    {
                        double tileswidth = this.Tiles_DragScroll.ActualWidth - (20);
                        double c = Math.Round(tileswidth / (E.MinTileWidth / 2));
                        Helper.RunMethodAsyncThreadSafe(() => TilesContainer.Height = this.TilesContainer.ActualHeight * 25);
                        double tilesheight = this.Tiles_DragScroll.ActualHeight * 25 - (20);
                        double r = (tilesheight / (E.MinTileHeight - E.TileSpacing * 2));

                        App.WindowManager.Initialize((int)c, (int)r);

                        Helper.RunMethodAsyncThreadSafe(() => ComposeMarkupGrid());

                        App.WidgetManager.WidgetLoaded += WidgetManagerWidgetLoaded;
                        App.WidgetManager.WidgetUnloaded += WidgetManagerWidgetUnloaded;

                        App.WidgetManager.FindWidgets();

                        if (Settings.Current.LoadedWidgets.Count == 0)
                        {
                            try
                            {
                                Settings.Current.LoadedWidgets.Add(new LoadedWidget() { Name = "Clock", Column = 0, Row = 0 });
                            }
                            catch { }
                        }

                        foreach (var lw in Settings.Current.LoadedWidgets)
                        {
                            LoadedWidget loadedWidget = lw;
                            Helper.RunMethodAsyncThreadSafe(() =>
                            {
                                WidgetProxy widget;
                                if (!string.IsNullOrEmpty(loadedWidget.Name) && string.IsNullOrEmpty(loadedWidget.Id))
                                {
                                    widget = App.WidgetManager.Widgets.Find(x => x.Name == loadedWidget.Name);
                                    if (widget == null) return;

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
                            });

                            System.Threading.Thread.Sleep(200);
                        }
                    });
            }, 500);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Current.LoadedWidgets.Clear();
            var handle = new WindowInteropHelper(this).Handle;
            try { MetroShell.Base.Messaging.MessagingHelper.RemoveListner(handle); }
            catch { }
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
                Settings.Current.LoadedWidgets.Add(loadedWidget);
            }
        }

        private void ControlMouseMove(object sender, MouseEventArgs e)
        {
            if (Settings.Current.TilesLock)
            {
                this.Tiles_DragScroll.DragEverywhere = true;
            }
            else
            {
                this.Tiles_DragScroll.DragEverywhere = false;
            }

            if (this.Tiles_DragScroll.IsDragging) return;

            var widget = (WidgetControl)sender;

            if (widget.MousePressed && !Settings.Current.TilesLock)
            {
                if (Mouse.Captured == sender)
                {
                    Canvas.SetLeft((UIElement)sender, e.GetPosition(DragCanvas).X - mouseX);
                    Canvas.SetTop((UIElement)sender, e.GetPosition(DragCanvas).Y - mouseY);
                }

                var drag = Math.Abs(e.GetPosition(widget).X - mouseX) >= 15 || Math.Abs(e.GetPosition(widget).Y - mouseY) >= 15;

                if (Mouse.Captured == null && drag)
                {
                    Mouse.Capture(widget);
                    if (DragCanvas.Children.Contains(widget)) return;
                    TilesHost.Children.Remove(widget);
                    DragCanvas.Children.Add(widget);

                    Canvas.SetLeft(widget, e.GetPosition(DragCanvas).X - mouseX);
                    Canvas.SetTop(widget, e.GetPosition(DragCanvas).Y - mouseY);
                    var col = Grid.GetColumn(widget);
                    var row = Grid.GetRow(widget);
                    var colspan = Grid.GetColumnSpan(widget);

                    App.WindowManager.Matrix.FreeSpace(col, row, colspan);
                    widget.Width = E.MinTileWidth * (colspan / 2);
                    widget.Height = E.MinTileHeight;

                    TilesHost.ShowGridLines = true;

                    widget.WidgetProxy.Column = -1;
                    widget.WidgetProxy.Row = -1;

                    e.Handled = true;
                }
            }
        }

        private void ControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (Mouse.Captured != sender) return;
                Mouse.Capture(null);
                var widget = (WidgetControl)sender;
                var coords = widget.TransformToAncestor(DragCanvas).Transform(new Point(0, 0));
                DragCanvas.Children.Remove(widget);
                TilesHost.Children.Add(widget);

                var colSpan = Grid.GetColumnSpan(widget);

                var column = (int)Math.Truncate((coords.X) / E.MinTileWidth * 2);
                var row = (int)Math.Truncate((coords.Y + E.MinTileHeight / 2) / E.MinTileHeight);
                var isFree = App.WindowManager.Matrix.IsCellFree(column, row, colSpan);
                if (!isFree ||
                    (column >= App.WindowManager.Matrix.ColumnsCount || row >= App.WindowManager.Matrix.RowsCount))
                    PlaceWidget(widget);
                else
                {
                    Grid.SetColumn(widget, column);
                    Grid.SetRow(widget, row);
                    widget.Width = (colSpan / 2) * E.MinTileWidth - E.TileSpacing * 2;
                    widget.Height = E.MinTileHeight - E.TileSpacing * 2;
                    App.WindowManager.Matrix.ReserveSpace(column, row, colSpan);
                }

                TilesHost.ShowGridLines = false;

                widget.MousePressed = false;
            }
            catch { }
        }

        private void ControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Captured == sender)
            {
                Canvas.SetLeft((UIElement)sender, e.GetPosition(DragCanvas).X - mouseX);
                Canvas.SetTop((UIElement)sender, e.GetPosition(DragCanvas).Y - mouseY);
                return;
            }

            var widget = (WidgetControl)sender;
            widget.MousePressed = true;
            mouseX = e.GetPosition((IInputElement)sender).X;
            mouseY = e.GetPosition((IInputElement)sender).Y;
        }

        private void WidgetManagerWidgetLoaded(WidgetProxy widget)
        {
            var control = new WidgetControl(widget);
            control.Order = TilesHost.Children.Count;
            control.MouseLeftButtonDown += ControlMouseLeftButtonDown;
            control.MouseLeftButtonUp += ControlMouseLeftButtonUp;
            control.MouseMove += ControlMouseMove;
            runningWidgets.Add(control);
            control.Load();
            Grid.SetColumnSpan(control, widget.WidgetComponent.ColumnSpan * 2);
            PlaceWidget(control);
            TilesHost.Children.Add(control);
            //if (widget.WidgetComponent != null)
            //{
            //    if (widget.WidgetComponent.GetType() == typeof(MetroShell.Core.MetroShellAppWidget))
            //    {
            //    }
            //}
        }

        private void WidgetManagerWidgetUnloaded(WidgetProxy widget)
        {
            var control = runningWidgets.Find(x => x.WidgetProxy == widget);
            if (control == null)
                return;
            Helper.Animate(control, OpacityProperty, 150, 0, 0.7, 0.3);
            Helper.Delay(new Action(() =>
            {
                try
                {
                    control.MouseLeftButtonDown -= ControlMouseLeftButtonDown;
                    control.MouseLeftButtonUp -= ControlMouseLeftButtonUp;
                    control.MouseMove -= ControlMouseMove;
                    var col = Grid.GetColumn(control);
                    var row = Grid.GetRow(control);
                    var colspan = Grid.GetColumnSpan(control);
                    TilesHost.Children.Remove(control);
                    runningWidgets.Remove(control);
                    control.Unload();
                    App.WindowManager.Matrix.FreeSpace(col, row, colspan);
                }
                catch { }
            }), 160);
        }

        public void PlaceWidget(WidgetControl widget)
        {
            var colSpan = Grid.GetColumnSpan(widget);
            MetroShellCell cell;
            if (widget.WidgetProxy.Column == -1 || widget.WidgetProxy.Row == -1)
                cell = App.WindowManager.Matrix.GetFreeCell(colSpan);
            else
                cell = new MetroShellCell(widget.WidgetProxy.Column, widget.WidgetProxy.Row);
            Grid.SetColumn(widget, (int)cell.Column);
            Grid.SetRow(widget, (int)cell.Row);
            Grid.SetColumnSpan(widget, colSpan);
            App.WindowManager.Matrix.ReserveSpace(cell.Column, cell.Row, colSpan);
            widget.Width = (colSpan / 2) * E.MinTileWidth - E.TileSpacing * 2;
            widget.Height = E.MinTileHeight - E.TileSpacing * 2;
        }

        public void ComposeMarkupGrid()
        {
            TilesHost.RowDefinitions.Clear();
            TilesHost.ColumnDefinitions.Clear();

            System.Threading.Tasks.Parallel.For(
                0, App.WindowManager.Matrix.ColumnsCount, new Action<int>((int i) =>
                {
                    Helper.RunMethodAsyncThreadSafe(() =>
                    {
                        var column = new ColumnDefinition();
                        column.Width = new GridLength(E.MinTileWidth / 2);
                        TilesHost.ColumnDefinitions.Add(column);
                    });
                }));

            System.Threading.Tasks.Parallel.For(
               0, App.WindowManager.Matrix.RowsCount, new Action<int>((int i) =>
               {
                   Helper.RunMethodAsyncThreadSafe(() =>
                   {
                       var row = new RowDefinition();
                       row.Height = new GridLength(E.MinTileHeight);
                       TilesHost.RowDefinitions.Add(row);
                   });
               }));
        }

        public void OpenToolbar()
        {
            try
            {
                this.Toolbar.Width = 200;
                this.Toolbar_TT.X = -(this.Toolbar.Width + 10);

                Helper.Animate(this.Toolbar_TT, TranslateTransform.XProperty, 200, 0, 0.7, 0.3, true);

                isOpened = true;

                List<UIElement> items = Items.Children.OfType<UIElement>().ToList();

                for (int i = 0; i < items.Count; i++)
                {
                    try
                    {
                        UIElement item = null;
                        int order = 1;
                        if (i < items.Count / 2)
                        {
                            item = ((UIElement)items[i]);
                            order = items.Count - i;
                        }
                        else
                        {
                            item = ((UIElement)items[i]);
                            order = 1;
                        }

                        Helper.Animate(item, OpacityProperty, 150 + (order * 100), 0, 1);
                    }
                    catch { }
                }
            }
            catch { }
        }

        public void CloseToolbar()
        {
            //try
            //{
            //    Helper.Animate(this, LeftProperty, 200, (3 + (Settings.Current.EnableOutOfBoxExperience ? 5 : 0)), 0.7, 0.3);

            //    isOpened = false;
            //}
            //catch { }
        }

        private void Image_UserPic_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            StartSystem.ShowHideAnimationWindow(true, "...", false);

            CloseToolbar();

            ExitOptionUI dlg = new ExitOptionUI();
            dlg.ShowDialog();
        }

        private void ToolbarItem_MouseDown_Settings(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            StartSystem.ShowHideAnimationWindow(true, "...", false);

            CloseToolbar();

            SettingsUI settings = new SettingsUI();
            settings.ShowDialog();
        }

        private void ToolbarItem_MouseDown_Computer(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("::{20d04fe0-3aea-1069-a2d8-08002b30309d}");
        }

        private void ToolbarItem_MouseDown_Desktop(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        }

        private void ToolbarItem_MouseDown_Calc(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("calc.exe");
        }

        private void ToolbarItem_MouseDown_TM(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("taskmgr.exe");
        }

        private void ToolbarItem_MouseDown_Refresh(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            StartSystem.ShowHideAnimationWindow(true, "Refreshing ...", true);
            Helper.Delay(() =>
            {
                StartSystem.ShowHideAnimationWindow(false, "...", true);
                Helper.Delay(() =>
                {
                    WinAPI.FlushMemory();
                }, 600);
            }, 2000);
        }

        private void ToolbarItem_MouseDown_Pin(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CloseToolbar();

            TilesToolBar tiles = new TilesToolBar();
            tiles.Show();
            tiles.OpenToolbar();
        }

        private void ToolbarItem_MouseDown_PinApp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                CloseToolbar();

                var dialog = new OpenFileDialog();
                dialog.Filter = "Executable files|*.exe";
                if (!(bool)dialog.ShowDialog()) return;
                var widget = App.WidgetManager.CreateWidget(dialog.FileName);
                App.WidgetManager.LoadWidget(widget);
            }
            catch (Exception)
            {
                Helper.ShowErrorMessage("Cannot process your request.");
            }
        }

        private void ToolbarItem_MouseDown_PinWeb(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                CloseToolbar();

                var addressWindow = new AddressBarPopup(ToolbarItem_PinWeb);
                addressWindow.ShowDialog();
                if (string.IsNullOrEmpty(addressWindow.AddressBox.Text)) return;
                var widget = App.WidgetManager.CreateWidget(addressWindow.AddressBox.Text);
                App.WidgetManager.LoadWidget(widget);
            }
            catch (Exception)
            {
                Helper.ShowErrorMessage("Cannot process your request.");
            }
        }

        internal void LoadUserTile()
        {
            Helper.RunMethodAsyncThreadSafe(() =>
            {
                this.UserTile.Visibility = Visibility.Visible;
                this.UserTile.Opacity = 0;
                Helper.Animate(this.UserTile, OpacityProperty, 300, 1);

                try
                {
                    if (File.Exists(System.IO.Path.GetTempPath() + "\\" + Environment.UserName + ".bmp"))
                    { File.Copy(System.IO.Path.GetTempPath() + "\\" + Environment.UserName + ".bmp", E.UserImage, true); }
                    TextBlock_UserName.Text = (Settings.Current.UserTileText.Equals("{UserName}")) ? Environment.UserName : Settings.Current.UserTileText;
                    BitmapSource bs = E.GetBitmap(E.UserImage);
                    Image_UserPic.Source = bs == null ? Image_UserPic.Source : bs;
                }
                catch { }

                Helper.Animate(this.UserTile, OpacityProperty, 250, 1);
            });
        }
    }
}