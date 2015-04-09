using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ModelLibrary.Utils
{
    /// <summary>
    /// A dictionary that fires an event when elements has been changed
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class PropertyObservingDictionary<TKey, TValue> : IDictionary<TKey,TValue>, INotifyCollectionChanged
    {
        private Dictionary<TKey, TValue> dictionary;
        public ICollection<TKey> Keys { get { return dictionary.Keys; } }
        public ICollection<TValue> Values { get { return dictionary.Values; } }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null) handler(sender, e);
        }

        public PropertyObservingDictionary()
        {
            Clear();
        }


        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key,item.Value);
        }

        public void Clear()
        {
            dictionary = new Dictionary<TKey, TValue>();
            OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public int Count { get { return dictionary.Count; } }
        public bool IsReadOnly { get; private set; }
        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            dictionary.Add(key,value);
            OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, key));
        }

        public bool Remove(TKey key)
        {
            var wasRemoved = dictionary.Remove(key);
            if (wasRemoved)
            {
                OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,key));
            }
            return wasRemoved;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get { return dictionary[key]; }
            set
            {
                if (!dictionary.ContainsKey(key) || dictionary[key].Equals(value))
                {
                    var old = dictionary[key];
                    dictionary[key] = value;
                    OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, old));
                }
            }
        }
    }
}