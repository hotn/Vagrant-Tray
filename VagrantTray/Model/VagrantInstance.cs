using System;
using System.Xml.Serialization;

namespace MikeWaltonWeb.VagrantTray.Model
{
    [Serializable]
    [XmlRoot(IsNullable = false)]
    public class VagrantInstance
    {
        [XmlElement]
        public string Id { get; set; }

        [XmlElement]
        public string Name { get; set; }

        [XmlElement]
        public string Provider { get; set; }

        [XmlElement]
        public string State { get; set; }

        [XmlElement]
        public string Directory { get; set; }
    }
}
