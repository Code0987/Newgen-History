using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ftware.Apps.MetroShell.Base;
using Ionic.Zip;

namespace Ftware.Apps.MetroShell.Core
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
            try
            {
                if (!Directory.Exists(E.WidgetsRoot)) return;

                string[] filePaths = Directory.GetFiles(E.WidgetsRoot, "*", SearchOption.AllDirectories);
                foreach (string filePath in filePaths)
                {
                    FileInfo fi = new FileInfo(filePath);
                    var name = fi.Name;
                    if (name == "Remove.process")
                    {
                        Directory.Delete(fi.DirectoryName, true);
                    }

                    if (name == "Update.process")
                    {
                        File.Delete(fi.FullName);
                        foreach (string f in Directory.GetFiles(E.WidgetsRoot + "\\$[Update]\\" + fi.Directory.Name))
                        {
                            File.Copy(f, fi.DirectoryName + "\\" + new FileInfo(f).Name, true);
                        }
                        Directory.Delete(E.WidgetsRoot + "\\$[Update]\\" + fi.Directory.Name, true);
                    }
                }
            }
            catch { }

            var files = Directory.GetFiles(E.WidgetsRoot, "*.dll", SearchOption.AllDirectories);
            foreach (var f in files)
            {
                var w = new WidgetProxy(f);
                //w.Load();
                if (w.HasErrors)
                    continue;

                Widgets.Add(w);

                System.Threading.Thread.Sleep(200);
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

        public bool ContainsWidget(string name)
        {
            return this.Widgets.Any((WidgetProxy widget) => widget.Name == name);
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
                if (WidgetLoaded != null) WidgetLoaded(widget);
            }
        }

        public void LoadWidget(WidgetProxy widget)
        {
            if (WidgetLoaded != null) WidgetLoaded(widget);
        }

        public void UnloadWidget(string name)
        {
            foreach (var widget in Widgets.Where(widget => widget.Name == name))
            {
                widget.Unload();
                if (WidgetUnloaded != null) WidgetUnloaded(widget);
                break;
            }
        }

        public void UnloadWidget(WidgetProxy widget)
        {
            if (widget.WidgetType == WidgetType.Generated) Widgets.Remove(widget);
            widget.Unload();
            if (WidgetUnloaded != null) WidgetUnloaded(widget);
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

        public void InstallWidget(string source, string name)
        {
            if (System.IO.Directory.Exists(source))
            {
                if (!System.IO.Directory.Exists(E.WidgetsRoot + name)) { System.IO.Directory.CreateDirectory(E.WidgetsRoot + name); }
                string[] files = System.IO.Directory.GetFiles(source);
                for (int i = 0; i < files.Length; i++)
                {
                    string text = files[i];
                    System.IO.File.Copy(text, E.WidgetsRoot + name + "\\");
                }
                if (!this.ContainsWidget(name))
                {
                    string path = E.WidgetsRoot + name + "\\" + name + ".dll";

                    if (System.IO.File.Exists(path))
                    {
                        WidgetProxy widgetProxy = new WidgetProxy(path, null, false, false);
                        if (!widgetProxy.HasErrors) { this.Widgets.Add(widgetProxy); }
                    }
                }
            }
        }

        public void InstallWidgetFromPackage(string file, string name)
        {
            Task t = new Task(new Action(() =>
            {
                try
                {
                    if (!System.IO.File.Exists(file)) { return; }
                    else
                    {
                        if (!System.IO.Directory.Exists(E.WidgetsRoot + name)) { System.IO.Directory.CreateDirectory(E.WidgetsRoot + name); }

                        FileInfo pf = new FileInfo(file);
                        if (!pf.Exists) { return; }

                        try
                        {
                            using (ZipFile zip = ZipFile.Read(file))
                            {
                                foreach (ZipEntry e in zip)
                                {
                                    try
                                    {
                                        e.Extract(E.WidgetsRoot, ExtractExistingFileAction.Throw);
                                    }
                                    catch
                                    {
                                        e.Extract(E.WidgetsRoot + "\\$[Update]\\", ExtractExistingFileAction.DoNotOverwrite);
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }

                        string path = E.WidgetsRoot + name + "\\" + name + ".dll";
                        if (System.IO.File.Exists(path))
                        {
                            WidgetProxy widgetProxy = new WidgetProxy(path, null, false, false);
                            if (!widgetProxy.HasErrors) { this.Widgets.Add(widgetProxy); }
                        }

                        File.Create(E.WidgetsRoot + name + "\\Update.process");
                        MetroShell.Base.Messaging.MessagingHelper.SendMessageToWidget("Store", "WidgetInstalled");
                    }
                }
                catch { File.Create(E.WidgetsRoot + name + "\\Update.process"); }
            }));

            t.Start();
        }

        public void RemoveWidget(string name)
        {
            try
            {
                if (ContainsWidget(name))
                {
                    try
                    {
                        if (IsWidgetLoaded(name)) { UnloadWidget(name); }
                        Directory.Delete(E.WidgetsRoot + name, true);
                    }
                    catch
                    {
                        File.Create(E.WidgetsRoot + name + "\\Remove.process");
                    }
                    MetroShell.Base.Messaging.MessagingHelper.SendMessageToWidget("Store", "WidgetRemoved");
                }
            }
            catch { }
        }

        public WidgetProxy GetWidgetByName(string name)
        { return this.Widgets.Find((WidgetProxy widget) => widget.Name == name); }
    }
}