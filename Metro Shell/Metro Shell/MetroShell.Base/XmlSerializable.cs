using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Ftware.Apps.MetroShell.Base
{
    public abstract class XmlSerializable
    {
        //private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public virtual void Save(string path)
        {
            try
            {
                //var w = new StreamWriter(path);
                var w = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
                var s = new XmlSerializer(this.GetType());
                s.Serialize(w, this);
                w.Close();
            }
            catch { }
        }

        public static object Load(Type t, string path)
        {
            if (File.Exists(path))
            {
                var sr = new StreamReader(path);
                var xr = new XmlTextReader(sr);
                var xs = new XmlSerializer(t);
                object result = null;
                //if (xs.CanDeserialize(xr))
                //{
                try
                {
                    result = xs.Deserialize(xr);
                }
                catch
                {
                    //logger.Error("Can't read xml file: " + path + ". Deserialization failed.\n" + ex.ToString());
                    //MessageBox.Show("Can't read xml file: " + path + ". See details in log.");
                }
                //var t = this.GetType();
                //var properties = t.GetProperties();
                //foreach (var p in properties)
                //{
                //    p.SetValue(this, p.GetValue(c, null), null);
                //}
                /*}
                else
                {
                    logger.Error("Can't read xml file: " + path + ". Deserialization failed.");
                }*/
                xr.Close();
                sr.Close();
                return result;
            }
            return null;
        }
    }
}