using System;
using System.IO;
using System.Windows;
using Ftware.Apps.MetroShell;
using Ftware.Apps.MetroShell.Base;

namespace Store
{
    public class Widget : MetroShellWidget
    {
        private WidgetControl widgetControl;

        public const string WidgetsBase = "http://dl.dropbox.com/u/38368203/Apps/MetroShell/Widgets/bin/";
        public const string WidgetsListBase = "http://dl.dropbox.com/u/38368203/Apps/MetroShell/Widgets/widgets.xml";
        public const string WidgetsLogosBase = "http://dl.dropbox.com/u/38368203/Apps/MetroShell/Widgets/logos/";
        public const string WidgetsThumbsBase = "http://dl.dropbox.com/u/38368203/Apps/MetroShell/Widgets/thumbs/";

        public override string Name
        {
            get { return "Store"; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/Store;component/Resources/icon.png", UriKind.Relative); }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override int ColumnSpan
        {
            get { return 1; }
        }

        public override void Load()
        {
            widgetfolder = E.WidgetsRoot + Name + "\\";
            widgetControl = new WidgetControl();
            widgetControl.Load();
        }

        public override void Unload()
        {
            widgetControl.Unload();
        }

        internal static string widgetfolder = null;

        internal static bool IsWidgetInstalled(string name)
        {
            try
            {
                if (Directory.Exists(widgetfolder) && File.Exists(E.WidgetsRoot + name + "\\Widget.xml")) { return true; }
                else { return false; }
            }
            catch { return false; }
        }

        internal static bool IsWidgetUpdateAvailable(string name, string version)
        {
            try
            {
                string widgetfolder = E.WidgetsRoot + name;
                if (Directory.Exists(widgetfolder) && File.Exists(E.WidgetsRoot + name + "\\Widget.xml"))
                {
                    WidgetInfo info = null;
                    bool available = false;

                    try { info = (WidgetInfo)XmlSerializable.Load(typeof(WidgetInfo), E.WidgetsRoot + name + "\\Widget.xml"); }
                    catch { return false; }

                    if (double.Parse(info.Version) < double.Parse(version))
                    {
                        available = true;
                    }

                    return available;
                }
                else { return false; }
            }
            catch { return false; }
        }

        public override void HandleMessage(string message)
        {
            switch (message)
            {
                case "WidgetInstalled":
                    MessageBox.Show("Widget Installed / Updated. Please restart MetroShell to apply new updates.", "// MetroShell / : Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case "WidgetRemoved":
                    MessageBox.Show("Widget Removed. Please restart MetroShell to apply changes.", "// MetroShell / : Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                default:
                    break;
            }
        }
    }
}