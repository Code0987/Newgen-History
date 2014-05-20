using System;
using System.Collections.Generic;
using System.Windows.Media;
using Ftware.Apps.MetroShell.Base;

namespace Ftware.Apps.MetroShell
{
    public class Settings : XmlSerializable
    {
        #region Properties

        public List<LoadedWidget> LoadedWidgets { get; set; }

        public Color ThemeColor1 { get; set; }

        public Color ThemeColor2 { get; set; }

        public bool UseBgImage { get; set; }

        public double MinTileHeight { get; set; }

        public double MinTileWidth { get; set; }

        public int TileSpacing { get; set; }

        public bool TilesLock { get; set; }

        public bool Autostart { get; set; }

        public bool EnableHotkeys { get; set; }

        public int TimeMode { get; set; }

        public string UserTileText { get; set; }

        public List<string> TaskBarProcessExclusionList { get; set; }

        #endregion Properties

        #region Current Settings

        [NonSerialized()]
        private static Settings settings;

        public static Settings Current { get { return settings; } set { settings = value; } }

        #endregion Current Settings

        #region Methods

        static Settings()
        {
        }

        public Settings()
        {
            LoadedWidgets = new List<LoadedWidget>();
            ThemeColor1 = Color.FromArgb(255, 40, 40, 40);
            ThemeColor2 = Color.FromArgb(255, 50, 50, 50);
            UseBgImage = false;
            MinTileWidth = 180;
            MinTileHeight = 180;
            TileSpacing = 10;
            TilesLock = false;
            Autostart = false;
            EnableHotkeys = true;
            TimeMode = 1;
            UserTileText = "{UserName}";
            TaskBarProcessExclusionList = new List<string>();
        }

        public static void Init()
        {
            settings = (Settings)XmlSerializable.Load(typeof(Settings), E.Config) ?? new Settings();
        }

        public static void Save()
        {
            try
            {
                Current.Save(E.Config);
            }
            catch { }
        }

        #endregion Methods
    }
}