using System.IO;

namespace SimpleDiagram.Utils
{
    public interface IReader
    {
        string Read(string filepath);
    }

    public class Reader : IReader
    {
        /// <summary>
        /// Simply reads to the end of the file.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public string Read(string filepath)
        {
            using (var file = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(file))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}