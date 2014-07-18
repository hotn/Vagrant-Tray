using System;
using System.Drawing;
using System.Windows.Forms;

namespace MikeWaltonWeb.VagrantTray.UI.Tray
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class VagrantToolStripMenuItem : ToolStripMenuItem
    {
        public VagrantToolStripMenuItem(string text)
            : base(text)
        {

        }

        public VagrantToolStripMenuItem(string text, Image image, EventHandler onClick)
            : base(text, image, onClick)
        {

        }

        private bool _isActionItem;

        /// <summary>
        /// Gets or sets whether the menu item is associated to an action and can be clicked. Allows conditional overriding of Enabled property.
        /// </summary>
        public bool IsActionItem
        {
            get { return _isActionItem; }
            set
            {
                if (!value)
                {
                    base.Enabled = false;
                }

                _isActionItem = value;
            }
        }

        public override bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                if (IsActionItem)
                {
                    base.Enabled = value;
                }
            }
        }
    }
}
