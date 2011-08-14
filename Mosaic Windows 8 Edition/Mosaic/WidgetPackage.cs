using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Ionic.Zip;

namespace Mosaic.Base
{
    public static class WidgetPackage
    {
        public static string WidgetCache = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Widgets\\";

        public static void UnpackWidget(string packagefile)
        {
            Task t = new Task(new Action(() =>
            {
                FileInfo pf = new FileInfo(packagefile);
                if (!pf.Exists) { return; }

                try
                {
                    using (ZipFile zip = ZipFile.Read(packagefile))
                    {
                        foreach (ZipEntry e in zip)
                        {
                            e.Extract(WidgetCache, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                }
                catch { }
            }));
            t.Start();
        }
    }
}