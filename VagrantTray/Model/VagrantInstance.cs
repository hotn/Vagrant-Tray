using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using MikeWaltonWeb.VagrantTray.Business.VagrantExe.Processes;

namespace MikeWaltonWeb.VagrantTray.Model
{
    [Serializable]
    [XmlRoot(IsNullable = false)]
    public class VagrantInstance : ICloneable<VagrantInstance>
    {
        private State _currentState;

        [NonSerialized]
        private VagrantProcess _currentProcess;

        [field:NonSerialized]
        public event EventHandler StateChanged;

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

        public VagrantProcess CurrentProcess
        {
            get { return _currentProcess; }
            set { _currentProcess = value; }
        }

        public enum State
        {
            Running,
            Saved,
            Poweroff,
            Loading,
            NotCreated
        }

        public VagrantInstance Clone()
        {
            return new VagrantInstance {CurrentState = CurrentState, Directory = Directory, CurrentProcess = CurrentProcess};
        }

        public override bool Equals(object obj)
        {
            if (!(obj.GetType() == GetType()))
            {
                return false;
            }

            return Directory == ((VagrantInstance) obj).Directory;
        }

        public override int GetHashCode()
        {
            return Directory.GetHashCode();
        }
    }
}
