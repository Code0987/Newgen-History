using System;
using System.Windows;
using Mosaic.Base;

namespace ControlPanel
{
    public class Widget : MosaicWidget
    {
        private StoreWidget widgetControl;

        public override string Name
        {
            get { return "Control Panel"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/ControlPanel;component/Resources/controlpanelwidget_icon.png", UriKind.Relative); }
        }

        public override int ColumnSpan
        {
            get { return 1; }
        }

        public override void Load()
        {
            widgetControl = new StoreWidget();
            //widgetControl.Load();
        }

        public override void Unload()
        {
            //widgetControl.Unload();
        }
    }
}
