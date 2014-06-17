using System;
using System.Xml.Serialization;

namespace MikeWaltonWeb.VagrantTray.Model
{
    [Serializable]
    [XmlRoot(IsNullable = false)]
    public class Bookmark
    {
        [XmlElement]
        public string Name { get; set; }

        [XmlElement]
        public VagrantInstance VagrantInstance { get; set; }
    }
}
