using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Mosaic.Base;

namespace Music
{
    public class Widget : MosaicWidget
    {
        private MusicWidget widgetControl;
        //public static Settings Settings;

        public override string Name
        {
            get { return "Music"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return  new Uri("/Music;component/Resources/icon.png", UriKind.Relative);; }
        }

        public override int ColumnSpan
        {
            get { return 2; }
        }

        public override void Load()
        {
            //Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.WidgetsRoot + "\\Music\\Music.config") ?? new Settings();
            widgetControl = new MusicWidget();
            widgetControl.Load();
        }

        public override void Unload()
        {
            //Settings.Save(E.WidgetsRoot + "\\Music\\Music.config");
            widgetControl.Unload();
        }
    }
}
