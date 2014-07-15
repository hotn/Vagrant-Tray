using System.Collections.Generic;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Business.Utility.Comparers
{
    public class BookmarkEqualityComparer : IEqualityComparer<Bookmark>
    {
        public bool Equals(Bookmark x, Bookmark y)
        {
            return x.Name == y.Name &&
                   new VagrantInstanceEqualityComparer().Equals(x.VagrantInstance, y.VagrantInstance);
        }

        public int GetHashCode(Bookmark obj)
        {
            return obj.GetHashCode();
        }
    }
}
