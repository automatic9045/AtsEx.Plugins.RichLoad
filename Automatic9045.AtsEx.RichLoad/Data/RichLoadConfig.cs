using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Automatic9045.AtsEx.RichLoad.Data
{
    public class RichLoadConfig
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(RichLoadConfig));

        [XmlAttribute(nameof(BackgroundColor))]
        public string BackgroundColorString = "Black";

        [XmlIgnore]
        public Color BackgroundColor => ColorTranslator.FromHtml(BackgroundColorString);

        [XmlAttribute(nameof(ProgressBarColor))]
        public string ProgressBarColorString = "White";

        [XmlIgnore]
        public Color ProgressBarColor => ColorTranslator.FromHtml(ProgressBarColorString);

        public StaticImage StaticImage = new StaticImage();

        public static RichLoadConfig Deserialize(string filePath, bool throwExceptionIfNotExists)
        {
            if (!File.Exists(filePath) && !throwExceptionIfNotExists) return new RichLoadConfig();

            using (StreamReader sr = new StreamReader(filePath, Encoding.UTF8))
            {
                return (RichLoadConfig)Serializer.Deserialize(sr);
            }
        }
    }
}
