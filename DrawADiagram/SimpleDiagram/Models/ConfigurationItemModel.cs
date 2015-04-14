using ModelLibrary.Models;

namespace SimpleDiagram.Models
{
    public class ConfigurationItemModel
    {
        public Connector Connector { get; set; }
        public Block Parent { get; set; }
        /// <summary>
        /// Label used for identifying the variable in the results.
        /// </summary>
        public string FirstAxisLabel { get; set; }
        
        /// <summary>
        /// Label used for identifying the variable in the results.
        /// </summary>
        public string SecondAxisLabel { get; set; }

        public string DisplayName
        {
            get { return string.Format("{0}.{1} - {2}", Parent.Name, Connector.Name, Connector.Type); }
        }
    }
}