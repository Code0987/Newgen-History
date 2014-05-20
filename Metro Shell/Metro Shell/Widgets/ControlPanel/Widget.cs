using System;
using System.Windows;
using Ftware.Apps.MetroShell.Base;

namespace ControlPanel
{
    public class Widget : MetroShellWidget
    {
        private ControlPanelWidget widgetControl;

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
            get { return new Uri("/ControlPanel;component/Resources/icon.png", UriKind.Relative); }
        }

        public override int ColumnSpan
        {
            get { return 1; }
        }

        public override void Load()
        {
            widgetControl = new ControlPanelWidget();
            //widgetControl.Load();
        }

        public override void Unload()
        {
            //widgetControl.Unload();
        }
    }
}