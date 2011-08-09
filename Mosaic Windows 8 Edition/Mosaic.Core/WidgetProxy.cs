﻿using System;
using System.Linq;
using System.Reflection;
using Mosaic.Base;

namespace Mosaic.Core
{
    public class WidgetProxy
    {
        public readonly string Path;
        private Assembly assembly;

        public MosaicWidget WidgetComponent { get; private set; }

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public bool IsLoaded { get; private set; }

        public string Name { get; private set; }

        public bool HasErrors { get; private set; }

        public int Column { get; set; }

        public int Row { get; set; }

        public WidgetType WidgetType { get; set; }

        public WidgetProxy(string path, string name = null, bool isHtml = false, bool isGenerated = false, bool isSocial = false)
        {
            Path = path;
            Column = -1;
            Row = -1;

            if (isHtml)
            {
                InitializeHtml();
                return;
            }

            if (isGenerated)
            {
                InitializeGenerated();
                return;
            }

            if (isSocial)
            {
                Name = name;
                InitializeSocial();
                return;
            }

            Initialize();
        }

        private void Initialize()
        {
            assembly = Assembly.LoadFrom(Path);
            Type widgetType = null;
            try
            {
                widgetType = assembly.GetTypes().FirstOrDefault(type => typeof(MosaicWidget).IsAssignableFrom(type));
            }
            catch (ReflectionTypeLoadException ex)
            {
                logger.Error("Failed to load provider from " + Path + ".\n" + ex);
                HasErrors = true;
                return;
            }

            if (widgetType == null)
            {
                logger.Error("Failed to find IWeatherProvider in " + Path);
                HasErrors = true;
                return;
            }

            WidgetComponent = Activator.CreateInstance(widgetType) as MosaicWidget;
            if (WidgetComponent == null)
            {
                HasErrors = true;
                return;
            }

            Name = WidgetComponent.Name;
            WidgetType = WidgetType.Native;
        }

        private void InitializeHtml()
        {
            WidgetComponent = new MosaicHtmlWidget2(Path);
            Name = WidgetComponent.Name;
            WidgetType = WidgetType.Html;
        }

        private void InitializeGenerated()
        {
            if (Path.StartsWith("http://"))
                WidgetComponent = new MosaicWebPreviewWidget();
            else
                WidgetComponent = new MosaicAppWidget();
            WidgetType = WidgetType.Generated;
            Name = WidgetComponent.Name;
        }

        private void InitializeSocial()
        {
            WidgetComponent = new MosaicFriendWidget();
            WidgetType = WidgetType.Generated;
        }

        public void Load()
        {
            if (WidgetType == WidgetType.Generated)
                if (string.IsNullOrEmpty(Name))
                    WidgetComponent.Load(Path);
                else
                    WidgetComponent.Load(Path, Name, Environment.TickCount * Row * Column);
            else
                WidgetComponent.Load();
            IsLoaded = true;
        }

        public void Unload()
        {
            try
            {
                WidgetComponent.Unload();
            }
            catch { }
            IsLoaded = false;
        }
    }
}