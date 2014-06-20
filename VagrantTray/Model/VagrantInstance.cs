using System;
using System.Xml.Serialization;

namespace MikeWaltonWeb.VagrantTray.Model
{
    [Serializable]
    [XmlRoot(IsNullable = false)]
    public class VagrantInstance
    {
        private State _currentState;
        public event EventHandler StateChanged;

        [XmlElement]
        public string Id { get; set; }

        [XmlElement]
        public string Name { get; set; }

        [XmlElement]
        public string Provider { get; set; }

        [XmlElement]
        public State CurrentState
        {
            get { return _currentState; }
            set
            {
                _currentState = value;
                if (StateChanged != null)
                {
                    StateChanged(this, EventArgs.Empty);
                }
            }
        }

        [XmlElement]
        public string Directory { get; set; }

        public enum State
        {
            Running,
            Saved,
            Poweroff,
            Loading
        }
    }
}
