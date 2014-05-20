using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Ftware.Apps.MetroShell.Base;
using Ftware.Apps.MetroShell.Controls;
using Ftware.Apps.MetroShell.Native;

namespace Ftware.Apps.MetroShell.Windows
{
    public partial class StartSystem : Window
    {
        internal static TilesScreen StartScreen;
        internal static WaitWindow ww;
        internal static Panel ContentRef;

        internal static DispatcherTimer tbtimer;

        internal static string asn = Assembly.GetExecutingAssembly().GetName().Name + ".exe";

        private WinAPI.RECT lasttaskbararea;

        public StartSystem()
        {
            InitializeComponent();

            StartScreen = new TilesScreen();

            Content.Background = new SolidColorBrush(Settings.Current.ThemeColor2);
            ContentRef = Content;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;

            Dwm.RemoveFromAeroPeek(handle);
            Dwm.RemoveFromAltTab(handle);
            Dwm.RemoveFromFlip3D(handle);

            try { MetroShell.Base.Messaging.MessagingHelper.AddListner(handle); }
            catch { }

            try
            {
                Width = SystemParameters.PrimaryScreenWidth;

                IntPtr taskbar = WinAPI.FindWindow("Shell_TrayWnd", "");
                IntPtr hwndOrb = WinAPI.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)0xC017, null);

                WinAPI.ShowWindow(taskbar, WinAPI.WindowShowStyle.Hide);
                WinAPI.ShowWindow(hwndOrb, WinAPI.WindowShowStyle.Hide);
            }
            catch { }

            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = 55;
            this.Top = SystemParameters.PrimaryScreenHeight - this.Height + 5;
            this.Left = 0;

            ZOrderHelper();

            try
            {
                WinAPI.RECT area = new WinAPI.RECT();
                WinAPI.GetWorkingArea(ref area);
                this.lasttaskbararea = area;

                area.left = (int)(StartScreen.Toolbar.Width);
                area.top = (int)(0);
                area.right = (int)(SystemParameters.PrimaryScreenWidth);
                area.bottom = (int)(SystemParameters.PrimaryScreenHeight - this.Height + 10);

                WinAPI.SetWorkingArea(area);
            }
            catch { }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try { WinAPI.SetWorkingArea(lasttaskbararea); }
            catch { }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartScreen.Show();

            tbtimer = new DispatcherTimer();
            tbtimer.Interval = TimeSpan.FromSeconds(5);
            tbtimer.Tick += new EventHandler(tbtimer_Tick);

            Helper.Delay(() =>
            {
                Content.Margin = new Thickness(0, 0, 0, 0);

                Icons.Margin = new Thickness(10, 5, 5, 5);

                StartScreen.PreviewMouseDown += new System.Windows.Input.MouseButtonEventHandler((o, a) => { ZOrderHelper(); });

                tbtimer.Start();
            }, 3000);
        }

        private void tbtimer_Tick(object sender, EventArgs e)
        {
            try
            {
                List<StartBarItem> items = Icons.Children.OfType<StartBarItem>().ToList();

                IntPtr handle = ((System.Windows.Interop.HwndSource)System.Windows.Interop.HwndSource.FromVisual(this)).Handle;
                IntPtr current = WinAPI.GetWindow(handle, WinAPI.GetWindowCmd.First);

                do
                {
                    int GWL_STYLE = -16;
                    uint normalWnd = 0x10000000 | 0x00800000 | 0x00080000;
                    uint popupWnd = 0x10000000 | 0x80000000 | 0x00080000;
                    var windowLong = WinAPI.GetWindowLong(current, GWL_STYLE);
                    var text = WinAPI.GetText(current);
                    if (((normalWnd & windowLong) == normalWnd || (popupWnd & windowLong) == popupWnd) && !string.IsNullOrEmpty(text))
                    {
                        try
                        {
                            if (items.Count == 0)
                            {
                                FileInfo fip = new FileInfo(WinAPI.GetProcessPath(current));
                                if (!Settings.Current.TaskBarProcessExclusionList.Contains(fip.Name) && !asn.Contains(fip.Name))
                                {
                                    AddIcon(current);
                                }
                            }
                            else
                            {
                                int existcount = 0;
                                foreach (var item in items)
                                {
                                    if (item.Handles.Contains(current)) existcount++;
                                }
                                if (existcount <= 0)
                                {
                                    FileInfo fip = new FileInfo(WinAPI.GetProcessPath(current));
                                    if (!Settings.Current.TaskBarProcessExclusionList.Contains(fip.Name) && !asn.Contains(fip.Name))
                                    {
                                        AddIcon(current);
                                    }
                                }
                            }
                        }
                        catch { }
                    }

                    current = WinAPI.GetWindow(current, WinAPI.GetWindowCmd.Next);

                    if (current == handle) current = WinAPI.GetWindow(current, WinAPI.GetWindowCmd.Next);
                }
                while (current != IntPtr.Zero);
            }
            catch { }
        }

        private void Window_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            StartSystem.ShowHide(true);
        }

        internal void AddIcon(IntPtr hWnd)
        {
            try
            {
                string hWndpath = new FileInfo(WinAPI.GetProcessPath(hWnd)).Name;
                List<StartBarItem> items = Icons.Children.OfType<StartBarItem>().ToList();
                bool isadded = false;

                foreach (StartBarItem sbi in items)
                {
                    if (WinAPI.GetProcessPath(sbi.Handles[0]).Contains(hWndpath))
                    {
                        sbi.AddhWnd(hWnd);
                        isadded = true;

                        Helper.Animate(sbi, OpacityProperty, 200, 0, 1, 0.3, 0.7);
                    }
                }
                if (!isadded)
                {
                    StartBarItem icon = new StartBarItem(this, new List<IntPtr> { hWnd });
                    Icons.Children.Add(icon);
                    Helper.Animate(icon, OpacityProperty, 200, 0, 1, 0.3, 0.7);
                }
            }
            catch { }
        }

        internal void RemoveIcon(StartBarItem icon)
        {
            try
            {
                if (icon != null)
                {
                    Helper.Animate(icon, OpacityProperty, 200, 0, 0.3, 0.7);
                    Helper.Delay(() => { Icons.Children.Remove(icon); }, 205);
                }
            }
            catch { }
        }

        internal void ZOrderHelper()
        {
            StartScreen.Topmost = true;
            StartScreen.Topmost = false;
            this.Topmost = false;
            this.Topmost = true;
        }

        internal static void ShowHideAnimationWindow(bool b, string text, bool circles)
        {
            try
            {
                if (b)
                {
                    ww = new WaitWindow(); ww.Show(); ww.Text.Text = text; ww.Start(circles);
                }
                else
                {
                    ww.Stop(); Helper.Delay(() => { ww.Hide(); ww = null; }, 600);
                    WinAPI.FlushMemory();
                }
            }
            catch { }
        }

        internal static void ShowHide(bool b)
        {
            if (b) { foreach (Window wnd in App.Current.Windows) if (!(wnd is StartSystem)) wnd.Show(); }
            else
            {
                foreach (Window wnd in App.Current.Windows) if (!(wnd is StartSystem)) wnd.Hide();
                WinAPI.FlushMemory();
            }
        }

        internal static void ForEachWindow(Func<Window, bool> func, List<Window> toexcude)
        {
            foreach (Window wnd in App.Current.Windows) if (!toexcude.Contains(wnd)) func.Invoke(wnd);
        }

        private void StartButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            StartButton.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 8,
                ShadowDepth = 0,
                Opacity = 0.9
            };

            try
            {
                StartScreen.Topmost = true;
                StartScreen.Topmost = false;
                this.Topmost = false;
                this.Topmost = true;
            }
            catch { }
        }

        private void StartButton_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            StartButton.Effect = null;
        }
    }
}