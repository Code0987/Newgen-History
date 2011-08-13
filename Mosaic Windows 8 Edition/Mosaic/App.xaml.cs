using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Win32;
using Mosaic.Base;
using Mosaic.Core;

namespace Mosaic
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static WindowManager WindowManager;
        public static WidgetManager WidgetManager;
        public static Settings Settings;
        public static Windows.Settings SettingsWindow;

        public static string Root = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public App()
        {
            App.Current.DispatcherUnhandledException +=
                new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(ApplicationDispatcherUnhandledException);

            AppDomain.CurrentDomain.UnhandledException +=
                      new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            if (!Directory.Exists(Root + "\\Logs"))
                Directory.CreateDirectory(Root + "\\Logs");
            if (!File.Exists(Root + "\\Logs\\ErrorLog.txt"))
                File.Create(Root + "\\Logs\\ErrorLog.txt");
        }

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            if (e.Args.Contains("-enableie9mode"))
            {
                EnableIe9Mode();
                Shutdown();
                return;
            }

            if (!CheckIe9ModeEnabled())
            {
                var p = new ProcessStartInfo { Verb = "runas", FileName = Assembly.GetExecutingAssembly().Location, Arguments = "-enableie9mode" };
                var proc = Process.Start(p);
                proc.WaitForExit();
            }

            if (!Directory.Exists(E.Root + "\\Thumbnails"))
                Directory.CreateDirectory(E.Root + "\\Thumbnails");
            if (!Directory.Exists(E.Root + "\\Cache"))
                Directory.CreateDirectory(E.Root + "\\Cache");

            StartupUri = new Uri("Windows/MainWindow.xaml", UriKind.Relative);

            Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.Root + "\\Mosaic.config") ?? new Settings();

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(Settings.Language);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(Settings.Language);
            E.Language = Settings.Language;
            E.AnimationEnabled = Settings.AnimationEnabled;

            if (Settings.UseSoftwareRendering)
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            WindowManager = new WindowManager();
            WidgetManager = new WidgetManager();
            WidgetManager.FindWidgets();

            if (Settings.LoadedWidgets.Count == 0)
                Settings.LoadedWidgets.Add(new LoadedWidget() { Name = "Clock" });
        }

        private bool CheckIe9ModeEnabled()
        {
            using (var key = Registry.LocalMachine.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadSubTree).OpenSubKey("Microsoft").OpenSubKey("Internet Explorer").OpenSubKey("MAIN").OpenSubKey("FeatureControl").OpenSubKey("FEATURE_BROWSER_EMULATION"))
            {
                var v = key.GetValue("Mosaic.exe");
                key.Close();
                if (v == null)
                    return false;
                return true;
            }
        }

        private void EnableIe9Mode()
        {
            var key = Registry.LocalMachine.OpenSubKey("Software").OpenSubKey("Microsoft").OpenSubKey("Internet Explorer").OpenSubKey("MAIN").OpenSubKey("FeatureControl").OpenSubKey("FEATURE_BROWSER_EMULATION", true);
            key.SetValue("Mosaic.exe", 9000, RegistryValueKind.DWord);
            key.Close();
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            if (WindowManager == null)
                return;
            foreach (var widget in WidgetManager.Widgets)
            {
                if (widget.IsLoaded)
                    widget.Unload();
            }

            Settings.Save(E.Root + "\\Mosaic.config");
        }

        public static void ShowOptions()
        {
            if (SettingsWindow != null && SettingsWindow.IsVisible)
            {
                SettingsWindow.Activate();
                return;
            }

            SettingsWindow = new Windows.Settings();
            //optionsWindow.UpdateSettings += OptionsWindowUpdateSettings;

            if (App.Settings.Language == "he-IL" || App.Settings.Language == "ar-SA")
            {
                SettingsWindow.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }
            else
            {
                SettingsWindow.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            }

            SettingsWindow.ShowDialog();
        }

        private void ApplicationDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                logger.Error("An error occurred.\n" + e.Exception);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("----------------------------------------------------------------");
                sb.AppendLine(DateTime.Now.ToString());
                sb.AppendLine("----------------------------------------------------------------");
                sb.AppendLine(e.Exception.Message);
                if (e.Exception.StackTrace != null)
                {
                    sb.AppendLine();
                    sb.AppendLine(e.Exception.StackTrace);
                    sb.AppendLine();
                }
                if (e.Exception.InnerException != null && e.Exception.InnerException.Message != null)
                {
                    sb.AppendLine();
                    sb.AppendLine(e.Exception.InnerException.Message);
                    sb.AppendLine();
                }
                sb.AppendLine("----------------------------------------------------------------");

                File.AppendAllText(Root + "\\Logs\\ErrorLog.txt", sb.ToString());
            }
            catch { }

            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = e.ExceptionObject as Exception; StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("----------------------------------------------------------------");
                sb.AppendLine(DateTime.Now.ToString());
                sb.AppendLine("----------------------------------------------------------------");
                sb.AppendLine(ex.Message);
                if (ex.StackTrace != null)
                {
                    sb.AppendLine();
                    sb.AppendLine(ex.StackTrace);
                    sb.AppendLine();
                }
                if (ex.InnerException != null && ex.InnerException.Message != null)
                {
                    sb.AppendLine();
                    sb.AppendLine(ex.InnerException.Message);
                    sb.AppendLine();
                }
                sb.AppendLine("----------------------------------------------------------------");

                File.AppendAllText(Root + "\\Logs\\ErrorLog.txt", sb.ToString());
            }
            catch { }
        }
    }
}