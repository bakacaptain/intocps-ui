using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace SimpleDiagram.Factories
{
    public class XamlFactory
    {
        public static ControlTemplate ToControlTemplate(string xaml, IEnumerable<KeyValuePair<string, string>> xmlns)
        {
            return Convert<ControlTemplate>(xaml, xmlns);
        }

        public static DataTemplate ToDataTemplate(string xaml, IEnumerable<KeyValuePair<string, string>> xmlns)
        {
            return Convert<DataTemplate>(xaml, xmlns);
        }

        private static T Convert<T>(string xaml, IEnumerable<KeyValuePair<string, string>> xmlns)
        {
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(xaml));
            var pc = new ParserContext();
            foreach (var ns in xmlns)
            {
                pc.XmlnsDictionary.Add(ns.Key, ns.Value);
            }
            return (T) XamlReader.Load(stream, pc);
        }
    }
}