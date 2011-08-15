using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Newgen.Base
{
    //Environment
    public static class E
    {
        public const double TilesSizeFactor = 1;

        public const double MinTilesSize = 150d;

        public const string ImageFilter = "Image files|*.png;*.jpg;*.jpeg";

        public static string Root { get { return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); } }

        public static string WidgetsRoot { get { return Root + "\\Widgets"; } }

        public static string LogsRoot { get { return Root + "\\Logs"; } }

        public static string ErrorLog { get { return Root + "\\Logs\\ErrorLog.txt"; } }

        public static string Config { get { return Root + "\\Newgen.config"; } }

        public static string CacheRoot { get { return Root + "\\Cache"; } }

        public static string BgImage { get { return CacheRoot + "\\BgImage.data"; } }

        public static string UserImage { get { return CacheRoot + "\\UserThumb.data"; } }

        public static int RowsCount { get; set; }

        public static int ColumnsCount { get; set; }

        public static Thickness Margin { get; set; }

        public static string Language { get; set; }

        public static bool AnimationEnabled { get; set; }

        public static Color BackgroundColor { get; set; }

        public static int AnimationTimePrecision { get; set; }

        public static int TileSpacing { get; set; }

        public static double MinTileHeight { get; set; }

        public static double MinTileWidth { get; set; }

        static E()
        {
            Margin = new Thickness(0, 0, 0, 0);

            AnimationTimePrecision = 1500;
        }

        public static void Init()
        {
            if (!Directory.Exists(E.LogsRoot)) Directory.CreateDirectory(E.LogsRoot);
            if (!Directory.Exists(E.WidgetsRoot)) Directory.CreateDirectory(E.WidgetsRoot);
            if (!Directory.Exists(E.CacheRoot)) Directory.CreateDirectory(E.CacheRoot);
            if (!File.Exists(E.ErrorLog)) File.Create(E.ErrorLog);
        }

        public static void EnableIe9Mode()
        {
            var key = Registry.LocalMachine.OpenSubKey("Software").OpenSubKey("Microsoft").OpenSubKey("Internet Explorer").OpenSubKey("MAIN").OpenSubKey("FeatureControl").OpenSubKey("FEATURE_BROWSER_EMULATION", true);
            key.SetValue("Newgen.exe", 9000, RegistryValueKind.DWord);
            key.Close();
        }

        public static bool CheckIe9ModeEnabled()
        {
            using (var key = Registry.LocalMachine.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadSubTree).OpenSubKey("Microsoft").OpenSubKey("Internet Explorer").OpenSubKey("MAIN").OpenSubKey("FeatureControl").OpenSubKey("FEATURE_BROWSER_EMULATION"))
            {
                var v = key.GetValue("Newgen.exe");
                key.Close();
                if (v == null)
                    return false;
                return true;
            }
        }

        public static BitmapSource GetBitmap(string path)
        {
            if (!File.Exists(path)) return null;

            MemoryStream ms = new MemoryStream();
            BitmapImage bi = new BitmapImage();
            byte[] bytArray = File.ReadAllBytes(path);
            ms.Write(bytArray, 0, bytArray.Length); ms.Position = 0;
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }

        public static void SetAutoStart(bool autostart)
        {
            RegistryKey regSUK = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (autostart)
            {
                regSUK.SetValue("Newgen Windows 8 Edition", Process.GetCurrentProcess().MainModule.FileName + " -winstart");
            }
            else
            {
                regSUK.DeleteValue("Newgen Windows 8 Edition", false);
            }
        }

        public static bool GetAutoStart()
        {
            RegistryKey regSUK = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            bool result = false;
            if (regSUK.GetValue("Newgen Windows 8 Edition", null) != null) result = true;
            return result;
        }
    }
}