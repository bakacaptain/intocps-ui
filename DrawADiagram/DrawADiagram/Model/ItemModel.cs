using System.ComponentModel;
using System.Windows;
using DrawADiagram.Annotations;
using DrawADiagram.Utils;

namespace DrawADiagram.Model
{
    public class ItemModel : INotifyPropertyChanged
    {
        private string name;
        private Point position;
        private string metaData;
        private PropertyObservingCollection<ConnectorModel> connectors;

        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Name"));
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
                    PropertyChanged(this, new PropertyChangedEventArgs("Position"));
                }
            }
        }

        public string MetaData
        {
            get { return metaData; }
            set
            {
                if (metaData != value)
                {
                    metaData = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("MetaData"));
                }
            }
        }

        public PropertyObservingCollection<ConnectorModel> Connectors
        {
            get { return connectors; }
            set
            {
                if (connectors != value)
                {
                    connectors.PropertyChanged -= PropertyChanged;
                    connectors = value;
                    connectors.PropertyChanged += PropertyChanged;
                    PropertyChanged(this, new PropertyChangedEventArgs("Connectors"));
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}