using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using DrawADiagram.Annotations;

namespace DrawADiagram.Utils
{
    public class PropertyObservingCollection<T> : ICollection<T>,INotifyPropertyChanged where T : INotifyPropertyChanged
    {
        private const string PropertyName = "Collection";
        private List<T> collection;

        public event PropertyChangedEventHandler PropertyChanged;

        public PropertyObservingCollection()
        {
            Clear();
        }

        public List<T> Collection
        {
            get { return collection; }
            set
            {
                if (collection != value)
                {
                    collection = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
                }
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Add(T item)
        {
            collection.Add(item);
            PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }

        public void Clear()
        {
            collection = new List<T>();
            PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }

        public bool Contains(T item)
        {
            return collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            bool canRemove = collection.Remove(item);
            if (canRemove)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
            return canRemove;
        }

        public int Count
        {
            get
            {
                return collection.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return collection.GetEnumerator();
        }
    }
}