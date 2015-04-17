using System.ComponentModel;
using System.Runtime.CompilerServices;
using ModelLibrary.Annotations;
using ModelLibrary.Models;

namespace SimpleDiagram.Models
{
    public class DSEConfigurationItemModel : INotifyPropertyChanged
    {
        private Connector input;
        private double from, to, increment;
        private string filepath;
        private bool useFile;

        public DSEConfigurationItemModel()
        {
            input = new Connector{Name = string.Empty};
            useFile = false;
            filepath = string.Empty;
        }

        public Connector Input
        {
            get { return input; }
            set 
            {
                if (input != value)
                {
                    input = value;
                    OnPropertyChanged("Input");
                } 
            }
        }

        public double From
        {
            get { return from; }
            set
            {
                if (from != value)
                {
                    from = value;
                    OnPropertyChanged("From");
                }
            }
        }

        public double To
        {
            get { return to; }
            set
            {
                if (to != value)
                {
                    to = value;
                    OnPropertyChanged("To");
                }
            }
        }

        public double Increment
        {
            get { return increment; }
            set
            {
                if (increment != value)
                {
                    increment = value;
                    OnPropertyChanged("Increment");
                }
            }
        }

        public string Filepath
        {
            get { return filepath; }
            set
            {
                if (filepath != value)
                {
                    filepath = value;
                    OnPropertyChanged("Filepath");
                }
            }
        }

        public bool UseFile
        {
            get { return useFile; }
            set
            {
                if (useFile != value)
                {
                    useFile = value;
                    OnPropertyChanged("UseFile");
                }
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        #region property changed
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}