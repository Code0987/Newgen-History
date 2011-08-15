using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newgen.Base;

namespace Newgen.Core
{
    public class WidgetManager
    {
        public static bool WebCoreInitialized;

        public delegate void WidgetLoadedEventHandler(WidgetProxy widget);
        public delegate void WidgetUnloadedEventHandler(WidgetProxy widget);
        public event WidgetLoadedEventHandler WidgetLoaded;
        public event WidgetUnloadedEventHandler WidgetUnloaded;

        public List<WidgetProxy> Widgets { get; private set; }

        public WidgetManager()
        {
            Widgets = new List<WidgetProxy>();
        }

        public void FindWidgets()
        {
            if (!Directory.Exists(E.WidgetsRoot))
                return;
            var files = from x in Directory.GetDirectories(E.WidgetsRoot)
                        where File.Exists(x + "\\" + Path.GetFileNameWithoutExtension(x) + ".dll")
                        select x + "\\" + Path.GetFileNameWithoutExtension(x) + ".dll";
            foreach (var f in files)
            {
                var w = new WidgetProxy(f);
                //w.Load();
                if (w.HasErrors)
                    continue;
                Widgets.Add(w);
            }

            var htmlfiles = from x in Directory.GetDirectories(E.WidgetsRoot)
                            where File.Exists(x + "\\Widget.Description.xml")
                            select x;
            foreach (var f in htmlfiles)
            {
                var w = new WidgetProxy(f, null, true);
                //w.Load();
                if (w.HasErrors)
                    continue;
                Widgets.Add(w);
            }

            Widgets = Widgets.OrderBy(x => x.Name).ToList();
        }

        public bool IsWidgetLoaded(string name)
        {
            return Widgets.Where(widget => widget.Name == name).Select(widget => widget.IsLoaded).FirstOrDefault();
        }

        public void LoadWidget(string name)
        {
            foreach (var widget in Widgets.Where(widget => widget.Name == name))
            {
                //widget.Load();
                if (WidgetLoaded != null)
                    WidgetLoaded(widget);
            }
        }

        public void LoadWidget(WidgetProxy widget)
        {
            if (WidgetLoaded != null)
                WidgetLoaded(widget);
        }

        public void UnloadWidget(string name)
        {
            foreach (var widget in Widgets.Where(widget => widget.Name == name))
            {
                widget.Unload();
                if (WidgetUnloaded != null)
                    WidgetUnloaded(widget);
                break;
            }
        }

        public void UnloadWidget(WidgetProxy widget)
        {
            if (widget.WidgetType == WidgetType.Generated)
                Widgets.Remove(widget);
            widget.Unload();
            if (WidgetUnloaded != null)
                WidgetUnloaded(widget);
        }

        public WidgetProxy CreateWidget(string url)
        {
            var widget = new WidgetProxy(url, null, false, true);
            Widgets.Add(widget);
            return widget;
        }

        public WidgetProxy CreateFriendWidget(string id, string name)
        {
            var widget = new WidgetProxy(id, name, false, false, true);
            Widgets.Add(widget);
            return widget;
        }
    }
}