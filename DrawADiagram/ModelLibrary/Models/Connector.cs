using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ModelLibrary.Annotations;

namespace ModelLibrary.Models
{
    [Serializable]
    public class Connector : INotifyPropertyChanged
    {
        private string name;
        private string unittype;
        private string datatype;
        private Direction type;
        private Point position;
        /// <summary>
        /// The hook to the actual input called by the COE
        /// </summary>
        private string hook;
        private ObservableCollection<Connection> connections; 

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

        public string DataType
        {
            get { return datatype; }
            set
            {
                if (datatype != value)
                {
                    datatype = value;
                    OnPropertyChanged("DataType");
                }
            }
        }

        public string UnitType
        {
            get { return unittype; }
            set
            {
                if (unittype != value)
                {
                    unittype = value;
                    OnPropertyChanged("UnitType");
                }
            }
        }

        public Direction Type
        {
            get { return type; }
            set
            {
                if (type != value)
                {
                    type = value;
                    OnPropertyChanged("Type");
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

        public string Hook
        {
            get { return hook; }
            set
            {
                if (hook != value)
                {
                    hook = value;
                    OnPropertyChanged("Hook");
                }
            }
        }

        public ObservableCollection<Connection> Connections
        {
            get { return connections; }
            set
            {
                if (connections != value)
                {
                    if (connections == null)
                    {
                        connections = new ObservableCollection<Connection>();
                    }
                    else
                    {
                        connections.CollectionChanged -= ConnectionsChanged;
                    }
                    connections = value;
                    connections.CollectionChanged += ConnectionsChanged;
                    OnPropertyChanged("Connections");
                }
            }
        }

        private void ConnectionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Connections");
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