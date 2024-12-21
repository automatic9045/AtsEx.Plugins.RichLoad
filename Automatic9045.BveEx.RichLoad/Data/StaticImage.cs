using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Automatic9045.BveEx.RichLoad.Data
{
    [XmlRoot]
    public class StaticImage
    {
        [XmlAttribute]
        public string Path = null;

        [XmlAttribute]
        public int Width = 2048;

        [XmlAttribute]
        public int Height = 2048;
    }
}
