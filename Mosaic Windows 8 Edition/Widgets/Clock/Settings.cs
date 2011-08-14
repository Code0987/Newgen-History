using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mosaic.Base;

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
