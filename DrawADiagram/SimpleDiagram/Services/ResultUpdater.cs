using System;
using System.Collections.Generic;
using System.IO;
using OxyPlot;
using SimpleDiagram.Utils;

namespace SimpleDiagram.Services
{
    public interface IResultUpdater
    {
        event EventHandler<GraphUpdateEvent> OnResultsChanged;
        event EventHandler<string> OnErrorOccured;

        /// <summary>
        /// Starts watching a filepath based on a uri to the filepath. An identifier is supplied to stop watching.
        /// </summary>
        /// <param name="file">Filename</param>
        /// <param name="uri">Full path to filepath</param>
        bool WatchFile(string file, string uri);
        
        /// <summary>
        /// Will stop watching a filepath based on the identifier
        /// </summary>
        /// <param name="file"></param>
        bool StopWatching(string file);

        /// <summary>
        /// Will stop watching all files.
        /// </summary>
        void StopWatching();

        /// <summary>
        /// Forces a filepath to update and raise the OnResultsChanged event
        /// </summary>
        /// <param name="filepath"></param>
        void ForceUpdate(string filepath);

        void ForceUpdate(string filepath, params string[] arguments);
    }

    public class ResultUpdater : IResultUpdater
    {
        private readonly IResultParser parser;

        private readonly Dictionary<string,FileSystemWatcher> watchers; 

        public ResultUpdater(IResultParser parser)
        {
            watchers = new Dictionary<string, FileSystemWatcher>();
            this.parser = parser;

            OnResultsChanged += (s, e) => { };
            OnErrorOccured += (s, e) => { };
        }

        public event EventHandler<GraphUpdateEvent> OnResultsChanged;
        public event EventHandler<string> OnErrorOccured;

        public bool WatchFile(string name, string uri)
        {
            var exists = watchers.ContainsKey(name);
            if (exists) return false;
            
            var watcher = new FileSystemWatcher(uri);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += OnFileChanged;
            watcher.Deleted += OnFileDeleted;
            watchers.Add(name, watcher);
            return true;
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Read the filepath
            if (watchers.ContainsKey(e.Name))
            {
                var filepath = string.Format("{0}\\{1}", watchers[e.Name].Path, e.Name);
                ForceUpdate(filepath);
            }
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            // Stop watching that filepath
            StopWatching(e.Name);
        }
        
        public bool StopWatching(string name)
        {
            var exists = watchers.ContainsKey(name);
            if (exists)
            {
                var watcher = watchers[name];
                // prevents new events from being raised
                watcher.EnableRaisingEvents = false;
                watcher.Changed -= OnFileChanged;
                watcher.Deleted -= OnFileDeleted;
            }
            return watchers.Remove(name);
        }

        public void StopWatching()
        {
            foreach (var watcher in watchers.Keys)
            {
                StopWatching(watcher);
            }
        }

        public void ForceUpdate(string filepath)
        {
            try
            {
                var points = parser.Parse(
                    filepath,
                    input => input.Split(new[] {Environment.NewLine}, StringSplitOptions.None),
                    row =>
                    {
                        var cols = row.Split(new[] {';', ','}, StringSplitOptions.None);
                        var x = double.Parse(cols[0]);
                        var y = double.Parse(cols[1]);
                        return new DataPoint(x, y);
                    });

                OnResultsChanged(this, new GraphUpdateEvent {ResultAction = ResultAction.Update, New = points, Tag = filepath});
            }
            catch (Exception e)
            {
                OnErrorOccured(this, e.Message);
            }
        }

        public void ForceUpdate(string filepath, params string[] arguments)
        {
            try
            {
                var points = parser.Parse(
                    filepath,
                    input => input.Split(new[] { Environment.NewLine }, StringSplitOptions.None),
                    row =>
                    {
                        var cols = row.Split(new[] { ';', ',' }, StringSplitOptions.None);
                        var x = double.Parse(cols[0]);
                        var y = double.Parse(cols[1]);
                        return new DataPoint(x, y);
                    });

                OnResultsChanged(this, new GraphUpdateEvent { ResultAction = ResultAction.Update, New = points, Tag = filepath });
            }
            catch (Exception e)
            {
                OnErrorOccured(this, e.Message);
            }
        }

    }

    public class GraphUpdateEvent
    {
        public string Tag { get; set; }
        public ResultAction ResultAction { get; set; }
        public IEnumerable<DataPoint> New { get; set; }
        public IEnumerable<DataPoint> Old { get; set; } 
    }

    public enum ResultAction
    {
        Create,
        Remove,
        Update,
        Delete,
    }
}