using System;
using System.Windows;
using Newgen.Base;

namespace Clock
{
    public class Widget : NewgenWidget
    {
        private ClockWidget widgetControl;
        public static Settings Settings;

        public override string Name
        {
            get { return "Clock"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/Clock;component/Resources/icon.png", UriKind.Relative); }
        }

        public override int ColumnSpan
        {
            get { return 2; }
        }

        public override void Load()
        {
            Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.WidgetsRoot + "\\Clock\\Clock.config") ?? new Settings();
            widgetControl = new ClockWidget();
            widgetControl.Load();
        }

        public override void Unload()
        {
            Settings.Save(E.WidgetsRoot + "\\Clock\\Clock.config");
            widgetControl.Unload();
        }
    }
}