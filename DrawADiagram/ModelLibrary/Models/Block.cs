using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ModelLibrary.Annotations;
using ModelLibrary.Utils;

namespace ModelLibrary.Models
{
    [Serializable]
    public class Block : INotifyPropertyChanged
    {
        private string name;
        private string version;
        private Point position;
        private double height;
        private double width;
        private PropertyObservingCollection<Connector> connectors;
        private ObservableCollection<KeyValueCouple<string,string>> parameters;

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

        public string Version
        {
            get { return version; }
            set
            {
                if (version != value)
                {
                    version = value;
                    OnPropertyChanged("Version");
                }
            }
        }

        public Point Position
        {
            get { return position; }
            set
            {
                if (position != value)
                {
                    position = value;
                    OnPropertyChanged("Position");
                }
            }
        }

        public double Height
        {
            get { return height; }
            set
            {
                if (height != value)
                {
                    height = value;
                    OnPropertyChanged("Height");
                }
            }
        }

        public double Width
        {
            get { return width; }
            set
            {
                if (width != value)
                {
                    width = value;
                    OnPropertyChanged("Width");
                }
            }
        }

        public PropertyObservingCollection<Connector> Connectors
        {
            get { return connectors ?? (connectors = new PropertyObservingCollection<Connector>()); }
            set
            {
                if (connectors != value)
                {
                    connectors = value;
                    OnPropertyChanged("Connectors");
                    if (connectors != null)
                    {
                        //TODO: cahnge to non-anonymous method for deletion
                        connectors.CollectionChanged += (sender, e) => { OnPropertyChanged("Connectors"); };
                    }
                }
            }
        }

        public ObservableCollection<KeyValueCouple<string, string>> Parameters
        {
            get { return parameters ?? (parameters = new ObservableCollection<KeyValueCouple<string, string>>()); }
            set
            {
                if (parameters != value)
                {
                    parameters = value;
                    OnPropertyChanged("Parameters");
                    if (parameters != null)
                    {
                        // TODO: change to non-anonymous method for deletion
                        parameters.CollectionChanged += (sender, e) => { OnPropertyChanged("Parameters");  };
                    }
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