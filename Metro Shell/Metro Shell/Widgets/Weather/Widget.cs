using System;
using System.Windows;
using Ftware.Apps.MetroShell.Base;

namespace Weather
{
    public class Widget : MetroShellWidget
    {
        private WeatherWidget widgetControl;
        public static Settings Settings;

        public override string Name
        {
            get { return "Weather"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/Weather;component/Resources/icon.png", UriKind.Relative); }
        }

        public override int ColumnSpan
        {
            get { return 2; }
        }

        public override void Load()
        {
            Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.WidgetsRoot + "\\Weather\\Weather.config") ?? new Settings();
            widgetControl = new WeatherWidget();
            widgetControl.Load();
        }

        public override void Unload()
        {
            Settings.Save(E.WidgetsRoot + "\\Weather\\Weather.config");
            widgetControl.Unload();
        }
    }
}