using System;
using System.Windows;
using Ftware.Apps.MetroShell.Base;

namespace Computer
{
    public class Widget : MetroShellWidget
    {
        private ComputerWidget widgetControl;
        //public static Settings Settings;

        public override string Name
        {
            get { return "Computer"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/Computer;component/Resources/icon.png", UriKind.Relative); }
        }

        public override int ColumnSpan
        {
            get { return 1; }
        }

        public override void Load()
        {
            widgetControl = new ComputerWidget();
            widgetControl.Load();
        }

        public override void Unload()
        {
        }
    }
}