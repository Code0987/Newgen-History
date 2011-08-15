using System;
using System.Windows;
using Newgen.Base;

namespace Store
{
    /// <summary>
    /// Provides information about widget for Newgen and allows Newgen determine that this .dll file is a widget
    /// </summary>
    public class Widget : Newgen.Base.NewgenWidget
    {
        private WidgetControl widgetControl;
        public static Settings Settings; //settings variable, remove it if you don't need settings

        /// <summary>
        /// Returns a name of this widget. This name displays in Newgen menu
        /// </summary>
        public override string Name
        {
            get { return "Store"; }
        }

        /// <summary>
        /// Returns path to widget icon. Return null if there is no icon (Newgen will use default)
        /// </summary>
        public override Uri IconPath
        {
            get { return new Uri("/Store;component/Resources/default_icon.png", UriKind.Relative); }
        }

        /// <summary>
        /// Returns widget control
        /// </summary>
        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        /// <summary>
        /// Returns number of horizontal cells occupied by widget. Should be 1 or 2 (more is possible but not recommended)
        /// </summary>
        public override int ColumnSpan
        {
            get { return 2; }
        }

        /// <summary>
        /// Widget initialization (e.g.variables initialization, reading settings, loading resources) must be here.
        /// This method calls when user clicks on widget icon in Newgen menu or at Newgen launch if widget was added earlier
        /// </summary>
        public override void Load()
        {
            //read settings, remove if you don't need settings
            Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.WidgetsRoot + "\\Store\\Store.config") ?? new Settings();

            widgetControl = new WidgetControl();
            widgetControl.Load();
        }

        /// <summary>
        /// Releasing resources and settings saving must be here.
        /// This method calls when user removes widget from Newgen grid or when user closes Newgen if widget was loaded earlier
        /// </summary>
        public override void Unload()
        {
            //write settings, remove if you don't need settings
            Settings.Save(E.WidgetsRoot + "\\Store\\Store.config");

            widgetControl.Unload();
        }
    }
}