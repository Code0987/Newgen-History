using System;
using System.Windows;
using Ftware.Apps.MetroShell.Base;

namespace Socialite
{
    public class Widget : MetroShellWidget
    {
        private SocialiteWidget widgetControl;
        //public static Settings Settings;

        public override string Name
        {
            get { return "Socialite"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/Socialite;component/Resources/icon.png", UriKind.Relative); }
        }

        public override int ColumnSpan
        {
            get { return 1; }
        }

        public override void Load()
        {
            //Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.WidgetsRoot + "\\Me\\Me.config") ?? new Settings();
            widgetControl = new SocialiteWidget();
            widgetControl.Load();
        }

        public override void Unload()
        {
            //Settings.Save(E.WidgetsRoot + "\\Me\\Me.config");
            widgetControl.Unload();
        }
    }
}