using System;
using System.Xml.Serialization;

namespace MikeWaltonWeb.VagrantTray.Model
{
    [Serializable]
    [XmlRoot(IsNullable = false)]
    public class Bookmark : ICloneable<Bookmark>
    {
        [XmlElement]
        public string Name { get; set; }

        [XmlElement]
        public VagrantInstance VagrantInstance { get; set; }

        public Bookmark Clone()
        {
            return new Bookmark {Name = Name, VagrantInstance = VagrantInstance.Clone()};
        }

        public override bool Equals(object obj)
        {
            var newBookmark = obj as Bookmark;

            if (newBookmark == null)
            {
                return false;
            }

            return newBookmark.Name == Name && newBookmark.VagrantInstance.Equals(VagrantInstance);
        }
    }
}
