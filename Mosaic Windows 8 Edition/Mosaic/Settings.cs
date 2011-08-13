using System.Collections.Generic;
using System.Globalization;
using Mosaic.Base;

namespace Mosaic
{
    public class Settings : XmlSerializable
    {
        public Settings()
        {
            LoadedWidgets = new List<LoadedWidget>();
            Autostart = false;
            Language = CultureInfo.CurrentUICulture.Name;
            IsExclusiveMode = true;
            AnimationEnabled = true;
            ShowGrid = false;
            UseSoftwareRendering = false;
            BackgroundColor = "#FF250931";
            DragEverywhere = true;
            EnableThumbnailsBar = Dwm.IsGlassAvailable() && Dwm.IsGlassEnabled();
            IsUserTileEnabled = true;
            IsAppWidgetBgStatic = false;
            AppWidgetBackgroundColor = "#FF000000";
            LockScreenTime = -1;
            UseBgImage = false;
        }

        public List<LoadedWidget> LoadedWidgets { get; set; }

        public bool DragEverywhere { get; set; }

        public bool Autostart { get; set; }

        public string Language { get; set; }

        public bool IsExclusiveMode { get; set; }

        public bool AnimationEnabled { get; set; }

        public bool ShowGrid { get; set; }

        public bool UseSoftwareRendering { get; set; }

        public string BackgroundColor { get; set; }

        public bool EnableThumbnailsBar { get; set; }

        public bool IsUserTileEnabled { get; set; }

        public bool IsAppWidgetBgStatic { get; set; }

        public string AppWidgetBackgroundColor { get; set; }

        public int LockScreenTime { get; set; }

        public bool UseBgImage { get; set; }
    }
}