using System.Collections.Generic;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Business.Utility.Comparers
{
    public class VagrantInstanceEqualityComparer : IEqualityComparer<VagrantInstance>
    {
        public bool Equals(VagrantInstance x, VagrantInstance y)
        {
            return x.Directory == y.Directory;
        }

        public int GetHashCode(VagrantInstance obj)
        {
            return obj.GetHashCode();
        }
    }
}
