using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Ftware.Apps.MetroShell.Base
{
    //Environment
    public static class E
    {
        public const double MinTilesSize = 150d;

        public const string ImageFilter = "Image files|*.png;*.jpg;*.jpeg";

        public static string Root { get { return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); } }

        public static string DataRoot { get { return Root + "\\App.Data"; } }

        public static string LogsRoot { get { return DataRoot + "\\Logs\\"; } }

        public static string CacheRoot { get { return DataRoot + "\\Cache\\"; } }

        public static string WidgetsRoot { get { return DataRoot + "\\Plugins\\"; } }

        public static string ErrorLog { get { return DataRoot + "\\Logs\\ErrorLog.txt"; } }

        public static string Config { get { return DataRoot + "\\App.config"; } }

        public static string BgImage { get { return CacheRoot + "\\BgImage.data"; } }

        public static string UserImage { get { return CacheRoot + "\\UserThumb.data"; } }

        public static int RowsCount { get; set; }

        public static int ColumnsCount { get; set; }

        public static Thickness Margin { get; set; }

        public static int AnimationTimePrecision { get; set; }

        public static int TileSpacing { get; set; }

        public static double MinTileHeight { get; set; }

        public static double MinTileWidth { get; set; }

        public static Dictionary<string, object> Objects { get; set; }

        static E()
        {
            Margin = new Thickness(0, 0, 0, 0);

            AnimationTimePrecision = 1200;

            Objects = new Dictionary<string, object>();

            Init();
        }

        public static void Init()
        {
            if (!Directory.Exists(E.DataRoot)) Directory.CreateDirectory(E.DataRoot);
            if (!Directory.Exists(E.LogsRoot)) Directory.CreateDirectory(E.LogsRoot);
            if (!Directory.Exists(E.WidgetsRoot)) Directory.CreateDirectory(E.WidgetsRoot);
            if (!Directory.Exists(E.CacheRoot)) Directory.CreateDirectory(E.CacheRoot);
            if (!File.Exists(E.ErrorLog)) File.Create(E.ErrorLog);
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
            try
            {
                RegistryKey regSUK = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (autostart)
                {
                    regSUK.SetValue("Ftware.Apps.MetroShell", Process.GetCurrentProcess().MainModule.FileName + " -winstart");
                }
                else
                {
                    regSUK.DeleteValue("Ftware.Apps.MetroShell", false);
                }
            }
            catch { }
        }

        public static bool GetAutoStart()
        {
            try
            {
                RegistryKey regSUK = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                bool result = false;
                if (regSUK.GetValue("Ftware.Apps.MetroShell", null) != null) result = true;
                return result;
            }
            catch { }
            return false;
        }

        private static List<MediaPlayer> mediaplayers = new List<MediaPlayer>();

        public static void Play(Uri source)
        {
            try
            {
                MediaPlayer mp = new MediaPlayer()
                {
                    Volume = 1.0
                };
                mediaplayers.Add(mp);
                mp.MediaEnded += (s, e) => { try { mediaplayers.Remove(mp); } catch { } };
                mp.MediaFailed += (s, e) => { try { mediaplayers.Remove(mp); } catch { } };
                mp.MediaOpened += (s, e) => { mp.Play(); };
                mp.Open(source);
            }
            catch { }
        }

        public static void AddorUpdateData(string key, object value)
        {
            if (Objects.ContainsKey(key))
            {
                Objects[key] = value;
            }
            else { Objects.Add(key, value); }
        }

        public static void RemoveData(string key)
        {
            if (Objects.ContainsKey(key))
            {
                Objects.Remove(key);
            }
        }
    }
}