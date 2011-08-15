using System;
using System.Windows;
using Newgen.Base;

namespace People
{
    public class Widget : NewgenWidget
    {
        private PeopleWidget widgetControl;
        //public static Settings Settings;

        public override string Name
        {
            get { return "People"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return null; }
        }

        public override int ColumnSpan
        {
            get { return 1; }
        }

        public override void Load()
        {
            //Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.WidgetsRoot + "\\Me\\Me.config") ?? new Settings();
            widgetControl = new PeopleWidget();
            widgetControl.Load();
        }

        public override void Unload()
        {
            //Settings.Save(E.WidgetsRoot + "\\Me\\Me.config");
            widgetControl.Unload();
        }
    }
}