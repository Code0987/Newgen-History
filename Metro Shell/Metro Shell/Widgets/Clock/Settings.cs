using Ftware.Apps.MetroShell.Base;

namespace Clock
{
    public class Settings : XmlSerializable
    {
        public Settings()
        {
            TimeMode = 1;
        }

        public int TimeMode { get; set; }
    }
}