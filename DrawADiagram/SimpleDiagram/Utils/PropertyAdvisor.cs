using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Castle.Core.Internal;
using ModelLibrary.Utils;

namespace SimpleDiagram.Utils
{
    /// <summary>
    /// Used for getting and setting properties on the underlying model
    /// </summary>
    public class PropertyAdvisor
    {
        public const string TOOL = "ToolLocation";
        public const string PROJECT = "ProjectLocation";
        public const string ARGUMENT = "ExecutionArgument";
        public const string RESULT = "ResultLocation";
        public const string SELECTED = "SelectedTool";

        #region Keys
        private static string KeyDecorator(string keyStem, string prefix)
        {
            return string.Format("{0}{1}", prefix, keyStem);
        }

        #endregion Keys

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="keyStem">Prefixes will be applied to the keyStem</param>
        /// <returns></returns>
        public static ObservableCollection <KeyValueCouple<string, string>> GetExternalToolParameter(ICollection<KeyValueCouple<string,string>> collection, string keyStem)
        {
            var Get = new Func<string,string>((key) =>
            {
                return collection.First(pair => pair.Key == key).Value;
            });

            var toolPath = Get(KeyDecorator(keyStem,TOOL));
            var projectPath = Get(KeyDecorator(keyStem,PROJECT));
            var argument = Get(KeyDecorator(keyStem,ARGUMENT));

            return new ObservableCollection<KeyValueCouple<string, string>>
            {
                new KeyValueCouple<string, string>(TOOL, toolPath),
                new KeyValueCouple<string, string>(PROJECT, projectPath),
                new KeyValueCouple<string, string>(ARGUMENT, argument)
            };
        }

        public static void SetExternalToolParameter(ICollection<KeyValueCouple<string, string>> collection, string keyStem,
            string toolPath, string projectPath, string argument)
        {
            var toolKey = KeyDecorator(keyStem,TOOL);
            var projectKey = KeyDecorator(keyStem,PROJECT);
            var argumentKey = KeyDecorator(keyStem,ARGUMENT);

            var Add = new Action<string, string>((key, value) =>
            {
                collection.Where(pair => pair.Key == key).ForEach(pair => collection.Remove(pair));
                collection.Add(new KeyValueCouple<string, string>(key, value));
            });

            Add(toolKey, toolPath);
            Add(projectKey, projectPath);
            Add(argumentKey, argument);
        }

        public static string GetExternalResultLocationParameter(ICollection<KeyValueCouple<string, string>> collection,
            string keyStem)
        {
            var resultKey = KeyDecorator(keyStem, RESULT);
            var resultLocation = collection.First(pair => pair.Key == resultKey);
            return (resultLocation != null) ? resultLocation.Value : null;
        }

        public static void SetExternalResultLocationParameter(ICollection<KeyValueCouple<string, string>> collection,
            string keyStem, string location)
        {
            var resultKey = KeyDecorator(keyStem, RESULT);
         
            collection.Where(pair => pair.Key == resultKey).ForEach(pair => collection.Remove(pair));
            collection.Add(new KeyValueCouple<string, string>(resultKey, location));
        }

        public static string GetSelectedExternalToolParameter(ICollection<KeyValueCouple<string, string>> collection)
        {
            var selectedTool = collection.First(pair => pair.Key == SELECTED);
            return (selectedTool != null) ? selectedTool.Value : null;
        }

        public static void SetSelectedExternalToolParameter(ICollection<KeyValueCouple<string, string>> collection,
            string keyStem)
        {
            collection.Where(pair => pair.Key == SELECTED).ForEach(pair => collection.Remove(pair));
            collection.Add(new KeyValueCouple<string, string>(SELECTED, keyStem));
        }
    } 
}