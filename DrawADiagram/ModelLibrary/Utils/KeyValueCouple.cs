namespace ModelLibrary.Utils
{
    /// <summary>
    /// Because the KeyValuePair doesn not have a setter for Value
    /// </summary>
    /// <typeparam name="T">Type of the Key</typeparam>
    /// <typeparam name="TK">Type of the Value</typeparam>
    public class KeyValueCouple<T,TK>
    {
        public KeyValueCouple(T key, TK value)
        {
            Key = key;
            Value = value;
        }

        public T Key { get; set; }
        public TK Value { get; set; }
        public override string ToString()
        {
            return string.Format("{{{0}.{1}}}",Key,Value);
        }
    }
}