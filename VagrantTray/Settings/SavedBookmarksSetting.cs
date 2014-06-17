using System.Collections.Generic;
using System.Configuration;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Settings
{
    public class SavedBookmarksSetting : ApplicationSettingsBase
    {
        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        public List<Bookmark> SavedBookmarks
        {
            get { return (List<Bookmark>) this["SavedBookmarks"]; }
            set { this["SavedBookmarks"] = value; }
        }
    }
}
