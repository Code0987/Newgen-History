namespace Internet
{
    using Ftware.Apps.MetroShell.Base;

    public class Settings : XmlSerializable
    {
        public Settings()
        {
            LastSearchURL = "";
        }

        public string LastSearchURL { get; set; }
    }
}