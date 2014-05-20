using System;
using System.Xml.Serialization;

namespace Ftware.Apps.MetroShell
{
    [Serializable]
    public class LoadedWidget
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name;
        [XmlAttribute(AttributeName = "path")]
        public string Path;
        [XmlAttribute(AttributeName = "id")]
        public string Id;
        [XmlAttribute(AttributeName = "column", DataType = "int")]
        public int Column;
        [XmlAttribute(AttributeName = "row", DataType = "int")]
        public int Row;
    }
}