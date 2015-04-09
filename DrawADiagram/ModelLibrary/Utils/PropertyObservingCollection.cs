using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ModelLibrary.Utils
{
    public class PropertyObservingCollection<T> : ICollection<T>, INotifyCollectionChanged where T : INotifyPropertyChanged
    {
        private List<T> collection;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

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
                    OnCollectionChanged(collection, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
        }

        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null) handler(sender, e);
        }

        public void Add(T item)
        {
            collection.Add(item);
            OnCollectionChanged(item, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void Clear()
        {
            collection = new List<T>();
            OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(T item)
        {
            return collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            collection.CopyTo(array,arrayIndex);
        }

        public bool Remove(T item)
        {
            bool canRemove = collection.Remove(item);
            if (canRemove)
            {
                OnCollectionChanged(item, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,item));
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