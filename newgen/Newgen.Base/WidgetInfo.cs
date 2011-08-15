using Newgen.Base;

namespace Newgen
{
    public class WidgetInfo : XmlSerializable
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string Description { get; set; }

        public string Author { get; set; }

        public string AuthorWeb { get; set; }
    }
}