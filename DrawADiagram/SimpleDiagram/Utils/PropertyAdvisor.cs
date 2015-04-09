using System;
using System.Collections.Generic;

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
        /// <param name="dictionary"></param>
        /// <param name="keyStem">Prefixes will be applied to the keyStem</param>
        /// <returns></returns>
        public static IDictionary<string, string> GetExternalToolParameter(IDictionary<string,string> dictionary, string keyStem)
        {
            string toolPath, projectPath, argument;

            dictionary.TryGetValue(ToolKey(keyStem), out toolPath);
            dictionary.TryGetValue(ProjectKey(keyStem), out projectPath);
            dictionary.TryGetValue(ArgumentKey(keyStem), out argument);

            return new Dictionary<string, string>()
            {
                {TOOL, toolPath},
                {PROJECT, projectPath},
                {ARGUMENT, argument}
            };
        }

        public static void SetExternalToolParameter(IDictionary<string, string> dictionary, string keyStem,
            string toolPath, string projectPath, string argument)
        {
            var toolKey = ToolKey(keyStem);
            var projectKey = ProjectKey(keyStem);
            var argumentKey = ArgumentKey(keyStem);

            var Add = new Action<string, string>((key, value) =>
            {
                if (dictionary.ContainsKey(key))
                {
                    dictionary[key] = value;
                }
                else
                {
                    dictionary.Add(key, value);
                }
            });

            Add(toolKey, toolPath);
            Add(projectKey, projectPath);
            Add(argumentKey, argument);
        }
    } 
}