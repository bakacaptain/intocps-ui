using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ModelLibrary.Annotations;

namespace ModelLibrary.Models
{
    [Serializable]
    public class Connection : INotifyPropertyChanged
    {
        private string name;
        private Connector sink;
        private Connector source;

        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public Connector Sink
        {
            get { return sink; }
            set
            {
                if (sink != value)
                {
                    sink = value;
                    OnPropertyChanged("Sink");
                }
            }
        }

        public Connector Source
        {
            get { return source; }
            set
            {
                if (source != value)
                {
                    source = value;
                    OnPropertyChanged("Source");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}