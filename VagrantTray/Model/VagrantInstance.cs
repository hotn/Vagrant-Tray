using System;
using System.Xml.Serialization;

namespace MikeWaltonWeb.VagrantTray.Model
{
    [Serializable]
    [XmlRoot(IsNullable = false)]
    public class VagrantInstance
    {
        private State _currentState;

        [field:NonSerialized]
        public event EventHandler StateChanged;

        [XmlElement]
        public string Id { get; set; }

        [XmlElement]
        public string Name { get; set; }

        [XmlElement]
        public string Provider { get; set; }

        [XmlIgnore]
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

        [XmlIgnore]
        public string CurrentStateString
        {
            get
            {
                return Regex.Replace(CurrentState.ToString(), "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
            }
        }

        [XmlElement]
        public string Directory { get; set; }

        public enum State
        {
            Running,
            Saved,
            Poweroff,
            Loading,
            NotCreated
        }
    }
}
