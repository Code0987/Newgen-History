using System;
using System.Windows;
using Newgen.Base;

namespace Pictures
{
    public class Widget : NewgenWidget
    {
        private PicturesWidget widgetControl;
        //public static Settings Settings;

        public override string Name
        {
            get { return "Pictures"; }
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
            get { return 2; }
        }

        public override void Load()
        {
            //Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.WidgetsRoot + "\\Me\\Me.config") ?? new Settings();
            widgetControl = new PicturesWidget();
            widgetControl.Load();
        }

        public override void Unload()
        {
            //Settings.Save(E.WidgetsRoot + "\\Me\\Me.config");
            //widgetControl.Unload();
        }
    }
}