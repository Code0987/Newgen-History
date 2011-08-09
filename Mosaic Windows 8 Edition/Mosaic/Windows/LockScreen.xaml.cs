using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Mosaic.Windows
{
    /// <summary>
    /// Interaction logic for Hub.xaml
    /// </summary>
    public partial class LockScreen : UserControl
    {
        public event EventHandler Unlocked;
        private DispatcherTimer timer;
        private DispatcherTimer udt;
        private DispatcherTimer dt;
        private bool ison = false;

        public LockScreen()
        {
            InitializeComponent();
        }

        private string wallpaperPath;

        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            //System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language);
            //System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language);

            //var wpReg = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
            //var wallpaperPath = wpReg.GetValue("WallPaper").ToString();
            //wpReg.Close();

            //var bi = new BitmapImage();
            //bi.BeginInit();
            //bi.CacheOption = BitmapCacheOption.OnLoad;
            //bi.UriSource = new Uri(wallpaperPath);
            //bi.EndInit();
            ////Windows 7
            //if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
            //{
            //    var wpReg = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Internet Explorer\\Desktop\\General\\", false);
            //    wallpaperPath = wpReg.GetValue("WallpaperSource").ToString();
            //    wpReg.Close();
            //}
            //else
            //{
            //    //Windows XP
            //    var wpReg = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
            //    wallpaperPath = wpReg.GetValue("Wallpaper").ToString();
            //    wpReg.Close();
            //}
            var wpReg = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
            wallpaperPath = wpReg.GetValue("WallPaper").ToString();
            wpReg.Close();
            UpdateWallpaper();
            udt = iFr.Helper.RunFor(new Action(UpdateWallpaper), -1, 2000);
            udt.Stop();

            //LockScreenBg.Source = bi;// new BitmapImage(new Uri(wallpaperPath));
            Day.Text = DateTime.Now.ToString("dddd");
            Day.Text = char.ToUpper(Day.Text[0]) + Day.Text.Substring(1);
            Month.Text = DateTime.Now.ToString("MMMM") + " " + DateTime.Now.Day;
            Time.Text = DateTime.Now.ToShortTimeString();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(TimerTick);

            this.Visibility = Visibility.Collapsed;

            iFr.Helper.RunFor(new Action(() =>
 {
     if ((int)Win32.GetIdleTime() >= (App.Settings.LockScreenTime * 60 * 1000))
     {
         Lock();
     }
 }), -1, 2000);
        }

        private void UpdateWallpaper()
        {
            LockScreenBg.Source = null;

            if (!File.Exists(wallpaperPath))
            { return; }

            //var bi = new BitmapImage();
            //bi.BeginInit();
            //bi.CacheOption = BitmapCacheOption.OnLoad;
            //bi.UriSource = new Uri(wallpaperPath, UriKind.Absolute);
            //bi.EndInit();

            MemoryStream ms = new MemoryStream();
            BitmapImage bi = new BitmapImage();

            byte[] bytArray = File.ReadAllBytes(wallpaperPath);

            ms.Write(bytArray, 0, bytArray.Length); ms.Position = 0;
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();

            LockScreenBg.Source = bi;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            Time.Text = DateTime.Now.ToShortTimeString();
        }

        private double mouseY;

        private void UserControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(this);
            mouseY = e.GetPosition((IInputElement)this.Parent).Y;
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            mouseY = 0;
            if (Translate.Y == 0)
            {
                var s = (Storyboard)Resources["JumpAnim"];

                s.Begin();
                return;
            }

            if (Translate.Y > -120)
            {
                var s = (Storyboard)Resources["MoveBackAnim"];

                s.Begin();
                return;
            }
            Unlock();
        }

        private void UserControlMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.Captured != this || e.LeftButton == MouseButtonState.Released)
                return;
            var y = e.GetPosition((IInputElement)this.Parent).Y;
            if (y >= mouseY)
                return;
            Translate.Y = y - mouseY;
        }

        private void UnlockAnimCompleted(object sender, EventArgs e)
        {
            if (Unlocked != null)
                Unlocked(this, EventArgs.Empty);
        }

        private void UserControlUnloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void MoveBackAnimCompleted(object sender, EventArgs e)
        {
            Translate.Y = 0;
        }

        public void Lock()
        {
            if (!ison)
            {
                ison = true;
                this.Visibility = Visibility.Visible;
                udt.Start();
                timer.Start();
                Translate.Y = -SystemParameters.PrimaryScreenHeight;
                var s = (Storyboard)Resources["MoveBackAnim"];
                s.Begin();
            }
        }

        public void Unlock()
        {
            ison = false;
            var s = (Storyboard)Resources["UnlockAnim"];
            ((DoubleAnimation)s.Children[0]).To = -SystemParameters.PrimaryScreenHeight;
            s.Begin();
            timer.Stop();
            udt.Stop();
        }

        private void JumpAnimCompleted(object sender, EventArgs e)
        {
            Translate.Y = 0;
        }

        internal struct LASTINPUTINFO
        {
            public uint cbSize;

            public uint dwTime;
        }

        /// <summary>
        /// Summary description for Win32.
        /// </summary>
        public class Win32
        {
            [DllImport("User32.dll")]
            public static extern bool LockWorkStation();

            [DllImport("User32.dll")]
            private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

            [DllImport("Kernel32.dll")]
            private static extern uint GetLastError();

            public static uint GetIdleTime()
            {
                LASTINPUTINFO lastInPut = new LASTINPUTINFO();
                lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
                GetLastInputInfo(ref lastInPut);

                return ((uint)Environment.TickCount - lastInPut.dwTime);
            }

            public static long GetTickCount()
            {
                return Environment.TickCount;
            }

            public static long GetLastInputTime()
            {
                LASTINPUTINFO lastInPut = new LASTINPUTINFO();
                lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
                if (!GetLastInputInfo(ref lastInPut))
                {
                    throw new Exception(GetLastError().ToString());
                }

                return lastInPut.dwTime;
            }
        }
    }
}