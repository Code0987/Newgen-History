using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using Ftware.Apps.MetroShell.Base;
using Ftware.Apps.MetroShell.Core;
using Ftware.Apps.MetroShell.Native;
using Microsoft.Shell;

namespace Ftware.Apps.MetroShell
{
    public partial class App : Application, ISingleInstanceApp
    {
        #region Internals

        public static WindowManager WindowManager;
        public static WidgetManager WidgetManager;

        private IntPtr ptrHook;
        private WinAPI.LowLevelKeyboardProc objKeyboardProcess;

        internal static string LicenseFile = E.Root + "\\iLicense.Client.License.iFr-License";

        #endregion Internals

        #region App

        #region ISingleInstanceApp Members

        private const string Unique = "Ftware.Apps.MetroShell";

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                App app = new App();
                app.InitializeComponent();
                app.Run();

                SingleInstance<App>.Cleanup();
            }
            else
            {
                Helper.ShowInfoMessage("Another instance of Metro Dock is already running. If you are having problems with it, please close it using Task Manager and then run Metro Dock again.");
            }
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            if (!Helper.CheckIESettingsEnabled())
            {
                try
                {
                    Helper.UpdateIESettings();
                }
                catch { }
            }

            CommandLineArgumentsParser command = new CommandLineArgumentsParser(args.ToArray<string>());

            if (command["nwp"] != null)
            {
                MessageBoxResult result = MessageBox.Show("Click Yes to install widget, No to ignore installation.", "// MetroShell / : Information", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.OK || result == MessageBoxResult.Yes)
                {
                    try
                    {
                        System.IO.FileInfo fi = null;
                        try { fi = new System.IO.FileInfo(command["nwp"]); }
                        catch { }
                        WidgetManager.InstallWidgetFromPackage(fi.FullName ?? command["nwp"], fi.Name ?? "Widget-" + DateTime.Now.Ticks.ToString());
                    }
                    catch { }
                }
            }

            return true;
        }

        #endregion ISingleInstanceApp Members

        public App()
        {
            this.Startup += new StartupEventHandler(App_Startup);
            this.Exit += new ExitEventHandler(App_Exit);
            this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);

            E.Init();
            Settings.Init();

            try
            {
                //iFramework.Security.Licensing.LicenseManager.Initialize();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            SignalExternalCommandLineArgs(e.Args.ToList<string>());
            E.TileSpacing = Settings.Current.TileSpacing;
            E.MinTileWidth = Settings.Current.MinTileWidth;
            E.MinTileHeight = Settings.Current.MinTileHeight;

            WindowManager = new WindowManager();
            WidgetManager = new WidgetManager();

            try
            {
                //var lic = iFramework.Security.Licensing.LicenseManager.Current.LoadLicense(App.LicenseFile);
                //if (lic != null)
                //{
                //    if (!(lic.Status == iFramework.Security.Licensing.LicenseStatus.TrialVersion |
                //          lic.Status == iFramework.Security.Licensing.LicenseStatus.Licensed))
                //    {
                //        (new Ftware.Apps.MetroShell.Windows.SettingsUI(true)).ShowDialog();
                //    }
                //}
            }
            catch { }

            try
            {
                //ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
                //objKeyboardProcess = new WinAPI.LowLevelKeyboardProc(CaptureKey);
                //ptrHook = WinAPI.SetWindowsHookEx(13, objKeyboardProcess, WinAPI.GetModuleHandle(objCurrentModule.ModuleName), 0);
            }
            catch { }

            StartupUri = new Uri("Windows/StartSystem.xaml", UriKind.Relative);

            MetroShell.Base.Messaging.MessagingHelper.MessageReceived += new Base.Messaging.MessagingHelper.MessageHandler(MessagingHelper_MessageReceived);
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                if (WindowManager == null) return;
                try
                {
                    MetroShell.Base.Messaging.MessagingHelper.MessageReceived -= new Base.Messaging.MessagingHelper.MessageHandler(MessagingHelper_MessageReceived);
                }
                catch { }
                try
                {
                    IntPtr taskbar = WinAPI.FindWindow("Shell_TrayWnd", "");
                    IntPtr hwndOrb = WinAPI.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)0xC017, null);
                    WinAPI.ShowWindow(taskbar, WinAPI.WindowShowStyle.Show);
                    WinAPI.ShowWindow(hwndOrb, WinAPI.WindowShowStyle.Show);
                }
                catch { }

                foreach (var widget in WidgetManager.Widgets)
                {
                    if (widget.IsLoaded) widget.Unload();
                }

                Settings.Current.Save(E.Config);
            }
            catch { }
        }

        private void MessagingHelper_MessageReceived(Base.Messaging.MessageEventArgs e)
        {
            string command = e.Data.Message;
            List<string> parameters = null;

            if (e.Data.Message.Contains(":"))
            {
                string[] commandandparam = e.Data.Message.Split(new char[] { ':' }, 2);
                command = commandandparam[0];
                parameters = commandandparam[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            if (e.Data.MessageKey == MetroShell.Base.Messaging.MessagingHelper.MetroShellWidgetKey)
            {
                var widget = WidgetManager.GetWidgetByName(command);
                if (widget != null && parameters != null && parameters.Count > 0)
                {
                    widget.WidgetComponent.HandleMessage(parameters[0]);
                }
            }
            else if (e.Data.MessageKey == MetroShell.Base.Messaging.MessagingHelper.MetroShellKey)
            {
                switch (command)
                {
                    case "URL":
                        if (parameters == null) { return; }
                        else
                        {
                            if (parameters[0] != null)
                            {
                                if (App.WidgetManager.IsWidgetLoaded("Internet Explorer")) { MetroShell.Base.Messaging.MessagingHelper.SendMessageToWidget("Internet Explorer", (parameters[0])); }
                                else { MetroShell.Native.WinAPI.ShellExecute(IntPtr.Zero, "open", (parameters[0]), string.Empty, string.Empty, 0); }
                            }
                        }
                        break;
                    case "InstallWidget":
                        if (parameters == null || parameters.Count < 2)
                        { return; }
                        WidgetManager.InstallWidgetFromPackage(parameters[0], parameters[1]);
                        break;
                    case "RemoveWidget":
                        if (parameters == null || parameters.Count < 1)
                        { return; }
                        WidgetManager.RemoveWidget(parameters[0]);
                        break;
                }
            }
        }

        #endregion App

        #region Win32 Hooks

        private int MouseState = 1; //1=Down, 2=Up

        private IntPtr CaptureKey(int nCode, IntPtr wp, IntPtr lp)
        {
            if (MouseState == 2) { MouseState = 0; }
            if (MouseState == 1) { MouseState = 2; }
            if (MouseState == 0) { MouseState = 1; }

            if (nCode >= 0 && Settings.Current.EnableHotkeys)
            {
                WinAPI.KBDLLHOOKSTRUCT objKeyInfo = (WinAPI.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(WinAPI.KBDLLHOOKSTRUCT));

                if (MouseState == 2 && (objKeyInfo.vkCode == KeyInterop.VirtualKeyFromKey(Key.RWin) || objKeyInfo.vkCode == KeyInterop.VirtualKeyFromKey(Key.LWin)))
                {
                    try
                    {
                        foreach (Window window in Application.Current.Windows)
                        {
                            if (window.GetType() == typeof(Windows.TilesScreen))
                            {
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    if (window.IsVisible)
                                    {
                                        window.Hide();
                                    }
                                    else
                                    {
                                        window.Show();
                                        window.Activate();
                                    }
                                }));
                            }
                        }
                    }
                    catch { }
                    return (IntPtr)1;
                }
            }
            return WinAPI.CallNextHookEx(ptrHook, nCode, wp, lp);
        }

        #endregion Win32 Hooks

        #region Error Handling

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                Helper.ProcessUnhandledException(e.Exception);
            }
            catch { }

            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Helper.ProcessUnhandledException(e.ExceptionObject as Exception);
            }
            catch { }
        }

        #endregion Error Handling
    }
}