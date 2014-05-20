using System;
using System.Windows;
using Ftware.Apps.MetroShell.Base;

namespace Calendar
{
    public class Widget : MetroShellWidget
    {
        private CalendarWidget widgetControl;
        public static Settings Settings;

        public override string Name
        {
            get { return "Calendar"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/Calendar;component/Resources/icon.png", UriKind.Relative); }
        }

        public override int ColumnSpan
        {
            get { return 2; }
        }

        public override void Load()
        {
            Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.WidgetsRoot + "\\Calendar\\Calendar.config") ?? new Settings();
            widgetControl = new CalendarWidget();
            widgetControl.Load();
        }

        public override void Unload()
        {
            Settings.Save(E.WidgetsRoot + "\\Calendar\\Calendar.config");
            widgetControl.Unload();
        }
    }
}