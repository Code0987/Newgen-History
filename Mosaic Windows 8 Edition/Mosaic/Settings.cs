using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
            IsExclusiveMode = false;
            AnimationEnabled = true;
            ShowGrid = false;
            UseSoftwareRendering = false;
            BackgroundColor = "#FF250931";
            EnableThumbnailsBar = Dwm.IsGlassAvailable() && Dwm.IsGlassEnabled();
        }

        public List<LoadedWidget> LoadedWidgets { get; set; }
        public bool Autostart { get; set; }
        public string Language { get; set; }
        public bool IsExclusiveMode { get; set; }
        public bool AnimationEnabled { get; set; }
        public bool ShowGrid { get; set; }
        public bool UseSoftwareRendering { get; set; }
        public string BackgroundColor { get; set; }
        public bool EnableThumbnailsBar { get; set; }
    }
}
