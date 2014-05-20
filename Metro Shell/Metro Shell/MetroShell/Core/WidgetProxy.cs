using System;
using System.Linq;
using System.Reflection;
using Ftware.Apps.MetroShell.Base;

namespace Ftware.Apps.MetroShell.Core
{
    public class WidgetProxy
    {
        public readonly string Path;
        private Assembly assembly;

        public MetroShellWidget WidgetComponent { get; private set; }

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
                widgetType = assembly.GetTypes().FirstOrDefault(type => typeof(MetroShellWidget).IsAssignableFrom(type));
            }
            catch (ReflectionTypeLoadException ex)
            {
                //throw new Exception("Failed to load provider from " + Path + ".\n" + ex);
                HasErrors = true;
                return;
            }

            if (widgetType == null)
            {
                //throw new Exception("Failed to find IWeatherProvider in " + Path);
                HasErrors = true;
                return;
            }

            WidgetComponent = Activator.CreateInstance(widgetType) as MetroShellWidget;
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
            WidgetComponent = new MetroShellHtmlWidget(Path);
            Name = WidgetComponent.Name;
            WidgetType = WidgetType.Html;
        }

        private void InitializeGenerated()
        {
            if (Path.StartsWith("http://"))
                WidgetComponent = new MetroShellWebPreviewWidget();
            else
                WidgetComponent = new MetroShellAppWidget();
            WidgetType = WidgetType.Generated;
            Name = WidgetComponent.Name;
        }

        private void InitializeSocial()
        {
            // WidgetComponent = new MetroShellFriendWidget();
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