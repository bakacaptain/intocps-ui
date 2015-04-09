using System;
using System.Collections.Generic;
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
        private Point position;
        private double height;
        private double width;
        private PropertyObservingCollection<Connector> connectors;
        private ObservableCollection<KeyValuePair<string,string>> parameters;

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
                }
            }
        }

        public ObservableCollection<KeyValuePair<string, string>> Parameters
        {
            get { return parameters ?? (parameters = new ObservableCollection<KeyValuePair<string, string>>()); }
            set
            {
                if (parameters != value)
                {
                    parameters = value;
                    OnPropertyChanged("Parameters");
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