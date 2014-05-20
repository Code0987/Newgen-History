using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Ftware.Apps.MetroShell.Base;
using Ftware.Apps.MetroShell.Controls;
using Ftware.Apps.MetroShell.Native;

namespace Ftware.Apps.MetroShell.Windows
{
    public partial class DWMPreviewPopup : Window
    {
        private List<IntPtr> hWnds { get; set; }

        private Point pcenter = new Point();

        private bool isclosepending = true;

        private Visual towner;

        public DWMPreviewPopup(Visual owner, List<IntPtr> handles)
        {
            InitializeComponent();

            hWnds = handles;

            towner = owner;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            Dwm.RemoveFromAeroPeek(handle);
            Dwm.RemoveFromAltTab(handle);
            Dwm.RemoveFromFlip3D(handle);

            try
            {
                Point locationFromScreen = towner.PointToScreen(new Point(0, 0));
                pcenter = new Point(locationFromScreen.X - (Width / 2 - 20), locationFromScreen.Y - (Height + 10));
            }
            catch { }

            this.Top = pcenter.Y;
            this.Left = pcenter.X;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Helper.Animate(this, OpacityProperty, 150, 1, 0.7, 0.3);
            Helper.Animate(this, TopProperty, 150, pcenter.Y - ActualHeight, pcenter.Y, 0.7, 0.3);

            try
            {
                int index = 1;
                foreach (IntPtr hWnd in hWnds)
                {
                    var thumb = new Thumbnail();

                    thumb.ToolTip = WinAPI.GetText(hWnd);

                    thumb.Width = Width - 50;
                    thumb.Height = Height - 50;
                    thumb.Source = hWnd;
                    PopupPreview.Children.Add(thumb);

                    if (hWnds.Count > 1 && index > 1)
                    {
                        Width = Width + thumb.Width;
                    }
                    else
                    {
                        Width++;
                        Width--;
                    }
                    index++;
                }

                Helper.RunFor(() =>
                {
                    this.Close();
                }, -1, 1000);
            }
            catch { }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isclosepending)
            {
                Helper.Delay(() =>
                {
                    isclosepending = false;
                    this.Close();
                }, 200);

                PopupPreview.Children.Clear();

                Helper.Animate(this, OpacityProperty, 150, 0, 0.7, 0.3);
                Helper.Animate(this, TopProperty, 150, pcenter.Y - ActualHeight, 0.7, 0.3);

                e.Cancel = true;
            }
            Point pos = PointFromScreen(Mouse.GetPosition(this));
            Point bounds = PointFromScreen(new Point());
            if (Mouse.DirectlyOver == PopupPreview)
            {
                e.Cancel = true;
            }
        }

        private void PopupPreview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var c = e.GetPosition(PopupPreview);
            foreach (Thumbnail thumb in PopupPreview.Children.OfType<Thumbnail>())
            {
                var transform = thumb.TransformToVisual(PopupPreview);
                Point p = transform.Transform(new Point(0, 0));
                if (c.Y > p.Y && c.Y < p.Y + thumb.Height && c.X > p.X && c.X < p.X + thumb.Width)
                {
                    if (!WinAPI.IsIconic(thumb.Source)) WinAPI.ShowWindow(thumb.Source, WinAPI.WindowShowStyle.Minimize);
                    else WinAPI.SwitchToThisWindow(thumb.Source, true);
                    break;
                }
            }
        }
    }
}