using System.ComponentModel;
using DrawADiagram.Annotations;

namespace DrawADiagram.Model
{
    public class ConnectionModel : INotifyPropertyChanged
    {
        //TODO: to implement notification
        public string Name { get; set; }
        public ConnectorModel Source { get; set; }
        public ConnectorModel Sink { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}