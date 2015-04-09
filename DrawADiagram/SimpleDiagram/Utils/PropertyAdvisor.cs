using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Castle.Core.Internal;

namespace SimpleDiagram.Utils
{
    /// <summary>
    /// Used for getting and setting properties on the underlying model
    /// </summary>
    public class PropertyAdvisor
    {
        public const string TOOL = "Tool";
        public const string PROJECT = "Project";
        public const string ARGUMENT = "Argument";

        #region Keys
        private static string ToolKey(string keyStem)
        {
            return string.Format("{0}{1}", TOOL, keyStem);
        }

        private static string ProjectKey(string keyStem)
        {
            return string.Format("{0}{1}", PROJECT, keyStem);
        }

        private static string ArgumentKey(string keyStem)
        {
            return string.Format("{0}{1}", ARGUMENT, keyStem);
        }

        #endregion Keys

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="keyStem">Prefixes will be applied to the keyStem</param>
        /// <returns></returns>
        public static ObservableCollection <KeyValuePair<string, string>> GetExternalToolParameter(ICollection<KeyValuePair<string,string>> collection, string keyStem)
        {
            var Get = new Func<string,string>((key) =>
            {
                return collection.First(pair => pair.Key == key).Value;
            });

            var toolPath = Get(ToolKey(keyStem));
            var projectPath = Get(ProjectKey(keyStem));
            var argument = Get(ArgumentKey(keyStem));

            return new ObservableCollection<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(TOOL, toolPath),
                new KeyValuePair<string, string>(PROJECT, projectPath),
                new KeyValuePair<string, string>(ARGUMENT, argument)
            };
        }

        public static void SetExternalToolParameter(ICollection<KeyValuePair<string, string>> collection, string keyStem,
            string toolPath, string projectPath, string argument)
        {
            var toolKey = ToolKey(keyStem);
            var projectKey = ProjectKey(keyStem);
            var argumentKey = ArgumentKey(keyStem);

            var Add = new Action<string, string>((key, value) =>
            {
                collection.Where(pair => pair.Key == key).ForEach(pair => collection.Remove(pair));
                collection.Add(new KeyValuePair<string, string>(key, value));
            });

            Add(toolKey, toolPath);
            Add(projectKey, projectPath);
            Add(argumentKey, argument);
        }
    } 
}