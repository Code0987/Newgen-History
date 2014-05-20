using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using Ftware.Apps.MetroShell.Base;
using Ftware.Apps.MetroShell.Core;
using Ftware.Apps.MetroShell.Native;
using Ftware.Apps.MetroShell.Windows;

namespace Ftware.Apps.MetroShell.Controls
{
    public partial class StartBarItem : UserControl
    {
        public List<IntPtr> Handles { get; set; }

        private DWMPreviewPopup Popup;

        private StartSystem towner;

        public StartBarItem(StartSystem owner, List<IntPtr> handle)
        {
            InitializeComponent();

            Handles = handle;
            towner = owner;

            this.IconImage.Source = IconExtractor.GetIcon(WinAPI.GetProcessPath(handle[0]));

            if (this.IconImage.Source == null) { towner.RemoveIcon(this); return; }
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                foreach (IntPtr hWnd in Handles)
                {
                    Process p = WinAPI.GetProcess(hWnd);
                    p.EnableRaisingEvents = true;
                    p.Exited += new EventHandler((o, a) =>
                    {
                        try { Helper.RunMethodAsyncThreadSafe(() => { towner.RemoveIcon(this); }); }
                        catch { }
                    });
                }
            }
            catch { }
        }

        private void UserControlUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        private void UserControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 8,
                ShadowDepth = 0,
                Opacity = 0.9
            };
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Effect = null;
            try
            {
                foreach (IntPtr hWnd in Handles)
                {
                    if (!WinAPI.IsIconic(hWnd)) WinAPI.ShowWindow(hWnd, WinAPI.WindowShowStyle.Minimize);
                    else WinAPI.SwitchToThisWindow(hWnd, true);
                }
            }
            catch { }
        }

        private void UserControlMouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                Popup.Close();
            }
            catch { }

            Popup = new DWMPreviewPopup(this, Handles);

            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 15,
                ShadowDepth = 0,
                Opacity = 0.9
            };

            Popup.Show();
        }

        private void UserControlMouseLeave(object sender, MouseEventArgs e)
        {
            Effect = null;
        }

        public void AddhWnd(IntPtr hWnd)
        {
            Handles.Add(hWnd);
        }
    }
}