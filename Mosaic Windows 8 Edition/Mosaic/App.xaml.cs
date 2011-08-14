using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Mosaic.Base;
using Mosaic.Core;

namespace Mosaic
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static WindowManager WindowManager;
        public static WidgetManager WidgetManager;
        public static Windows.Settings SettingsWindow;
        public static Settings Settings;

        public App()
        {
            App.Current.DispatcherUnhandledException +=
                new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(ApplicationDispatcherUnhandledException);

            AppDomain.CurrentDomain.UnhandledException +=
                      new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            E.Init();
        }

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            iFr.CommandLineArgumentsParser comm = new iFr.CommandLineArgumentsParser(e.Args);

            if (e.Args.Contains("-enableie9mode"))
            {
                E.EnableIe9Mode();
                Shutdown();
                return;
            }
            if (!E.CheckIe9ModeEnabled())
            {
                var p = new ProcessStartInfo { Verb = "runas", FileName = Assembly.GetExecutingAssembly().Location, Arguments = "-enableie9mode" };
                var proc = Process.Start(p);
                proc.WaitForExit();
            }

            if (comm["mwi"] != null) WidgetPackage.UnpackWidget(comm["mwi"]);

            StartupUri = new Uri("Windows/MainWindow.xaml", UriKind.Relative);

            Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.Config) ?? new Settings();

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(Settings.Language);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(Settings.Language);

            E.Language = Settings.Language;
            E.AnimationEnabled = Settings.AnimationEnabled;
            E.TileSpacing = Settings.TileSpacing;
            E.MinTileWidth = Settings.MinTileWidth;
            E.MinTileHeight = Settings.MinTileHeight;
            E.BackgroundColor = (Color)ColorConverter.ConvertFromString(App.Settings.BackgroundColor);

            if (Settings.UseSoftwareRendering) RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            WindowManager = new WindowManager();
            WidgetManager = new WidgetManager();
            WidgetManager.FindWidgets();

            if (Settings.LoadedWidgets.Count == 0) Settings.LoadedWidgets.Add(new LoadedWidget() { Name = "Clock" });
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            if (WindowManager == null) return;

            foreach (var widget in WidgetManager.Widgets)
            { if (widget.IsLoaded) widget.Unload(); }

            Settings.Save(E.Config);
        }

        public static void ShowOptions()
        {
            if (SettingsWindow != null && SettingsWindow.IsVisible)
            {
                SettingsWindow.Activate();
                return;
            }

            SettingsWindow = new Windows.Settings();

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

                File.AppendAllText(E.ErrorLog, sb.ToString());
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

                File.AppendAllText(E.ErrorLog, sb.ToString());
            }
            catch { }
        }
    }
}