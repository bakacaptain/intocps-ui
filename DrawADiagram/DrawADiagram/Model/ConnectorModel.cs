using System.ComponentModel;
using System.Windows;
using DrawADiagram.Annotations;

namespace DrawADiagram.Model
{
    public class ConnectorModel : INotifyPropertyChanged
    {
        private string name;
        private string unittype;
        private Point position;
        private Direction direction;

        public string Name {
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

        public string UnitType
        {
            get { return unittype; }
            set
            {
                if (unittype != value)
                {
                    unittype = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("UnitType"));
                }
            }
        }

        public Direction Direction
        {
            get { return direction; }
            set
            {
                if (direction != value)
                {
                    direction = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Direction"));
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

    public enum Direction
    {
        Input,
        Output,
        InOut
    }
}