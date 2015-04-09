using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace SimpleDiagram.Utils
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Extension method for all objects to deep copy.
        /// REMEMBER to add the [Serializable] above the class delaration
        /// To skip serialization of members please use [field:NonSerialized] e.g. for events
        /// </summary>
        /// <typeparam name="T">Type copied</typeparam>
        /// <param name="source">Object being copied</param>
        /// <returns>A copy of "source"</returns>
        public static T DeepClone<T>(this T source)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, source);
                stream.Position = 0;
                return (T) formatter.Deserialize(stream);
            }
        }
        /// <summary>
        /// Extension method for all objects to deep copy.
        /// REMEMBER to add the [Serializable] above the class delaration
        /// To skip serialization of members please use [field:NonSerialized] e.g. for events
        /// Uses Newtonsoft's JSON serializer.
        /// </summary>
        /// <typeparam name="T">Type copied</typeparam>
        /// <param name="source">Object being copied</param>
        /// <returns>A copy of "source"</returns>
        public static T CloneJson<T>(this T source)
        {
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source));
        }
    }
}