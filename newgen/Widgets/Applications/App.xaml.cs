using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Win32;
using Mosaic.Base;
using Mosaic.Core;
using Mosaic.Windows;

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
        public static Options OptionsWindow;

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
            if (OptionsWindow != null && OptionsWindow.IsVisible)
            {
                OptionsWindow.Activate();
                return;
            }

            OptionsWindow = new Options();
            //optionsWindow.UpdateSettings += OptionsWindowUpdateSettings;

            if (App.Settings.Language == "he-IL" || App.Settings.Language == "ar-SA")
            {
                OptionsWindow.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }
            else
            {
                OptionsWindow.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            }

            OptionsWindow.ShowDialog();
        }

        private void ApplicationDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            logger.Error("An error occured.\n" + e.Exception);
        }
    }
}
