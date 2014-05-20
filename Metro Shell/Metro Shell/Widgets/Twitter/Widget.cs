using System;
using System.Windows;
using Ftware.Apps.MetroShell.Base;

namespace Twitter
{
    public class Widget : MetroShellWidget
    {
        private TwitterWidget widgetControl;
        public static Settings Settings;

        public override string Name
        {
            get { return "Twitter"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/Twitter;component/Resources/icon.png", UriKind.Relative); }
        }

        public override int ColumnSpan
        {
            get { return 2; }
        }

        public override void Load()
        {
            Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.WidgetsRoot + "\\Twitter\\Twitter.config") ?? new Settings();
            widgetControl = new TwitterWidget();
            widgetControl.Load();
        }

        public override void Unload()
        {
            Settings.Save(E.WidgetsRoot + "\\Twitter\\Twitter.config");
            widgetControl.Unload();
        }
    }
}