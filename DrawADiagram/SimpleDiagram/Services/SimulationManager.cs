using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Castle.Core.Internal;
using ModelLibrary.Annotations;
using OxyPlot;
using OxyPlot.Series;
using SimpleDiagram.Models;
using SimpleDiagram.Utils;

namespace SimpleDiagram.Services
{
    public class SimulationManager : ISimulationManager, INotifyPropertyChanged
    {
        private readonly IModelManager modelManager;
        private readonly IResultUpdater resultUpdater;
        public event EventHandler OnConfigureRequest;
        public event EventHandler OnPlotUpdated;

        private bool isRunning;

        public SimulationManager(IModelManager modelManager, IResultUpdater resultUpdater)
        {
            isRunning = false;
            Results = new PlotModel();

            Watched = new ObservableCollection<ConfigurationItemModel>();
            Watchable = new List<ConfigurationItemModel>();

            Watched.CollectionChanged += (sender, e) => { UpdateConfiguration(); };

            this.modelManager = modelManager;
            this.resultUpdater = resultUpdater;

            // Initializing default eventhandler
            OnConfigureRequest += (sender, args) => { };
            OnPlotUpdated += (sender, series) => { };

            resultUpdater.OnResultsChanged += (sender, series) =>
            {
                var line = new LineSeries{Tag = series.Tag};
                line.Points.AddRange(series.New);

                var old = Results.Series.Where(s => (string) s.Tag == series.Tag);

                foreach (var oldLine in old)
                {
                    Results.Series.Remove(oldLine);
                }
                
                Results.Series.Add(line);
                Results.InvalidatePlot(true);
            };
        }

        public bool CanRun
        {
            get
            {
                //TODO: insert actual logic
                return !Watched.IsNullOrEmpty();
            }
        }

        public bool IsRunning
        {
            get
            {
                return isRunning;
            }
            private set
            {
                isRunning = value;
                OnPropertyChanged("IsRunning");
            }
        }

        public void Configure()
        {
            OnConfigureRequest(this, null);
        }

        public void ConfigureConnections()
        {
            Console.WriteLine(@"Configuring connections");
            var blocks = modelManager.GetBlocks();
            var connections = modelManager.GetConnections();

            var watchables = from block in blocks
                from connector in block.BlockModel.Connectors
                select new ConfigurationItemModel
                {
                    Connector = connector,
                    Parent= block.BlockModel,
                    //TODO: include first axis label and second axis
                };

            var map = watchables.ToDictionary(item => item.Connector, item => item);

            foreach (var connection in connections)
            {
                var sink = connection.Sink.Connector;
                var source = connection.Source.Connector;
                if (sink != null && source != null)
                {
                    Console.WriteLine("Connecting {0}.{1} with {2}.{3}\nCOE: {4} --> {5}\n#:{6}",
                        map[source].Parent.Name, source.Name, map[sink].Parent.Name, sink.Name, source.Hook, sink.Hook, connection.GetHashCode());
                }
                else
                {
                    Console.WriteLine("Sink: {0} Sourc: {1}\n#:{2}", sink, source, connection.GetHashCode());
                }
            }

            Watchable = map.Values;
        }

        public void WatchVariable(ConfigurationItemModel item)
        {
            if (!Watched.Any(x => x.Parent == item.Parent && x.Connector == item.Connector))
            {
                Watched.Add(item);
            }
            OnPropertyChanged("Watched");
        }

        public void UnWatchVariable(ConfigurationItemModel item)
        {
            if (Watched.Any(x => x.Parent == item.Parent && x.Connector == item.Connector))
            {
                Watched.Remove(item);
            }
            OnPropertyChanged("Watched");
        }

        public PlotModel Results { get; private set; }

        public IEnumerable<ConfigurationItemModel> Watchable { get; private set; }

        public List<ConfigurationItemModel> NotWatched
        {
            get { return Watchable.Where(item => !Watched.Any(x => x.Parent == item.Parent && x.Connector == item.Connector)).ToList(); }
        }

        public ObservableCollection<ConfigurationItemModel> Watched { get; private set; } 

        

        public void StopSimulation()
        {
            if (IsRunning)
            {
                resultUpdater.StopWatching();
                IsRunning = false;
            }
        }

        public void Cosimulate()
        {
            if (!CanRun || IsRunning) return;

            IsRunning = true;

            //TODO: open simulation
        }

        private void UpdateConfiguration()
        {
            //TODO: simulate each sub-model
            //resultUpdater.WatchFile(filename);

            var configs = from item in Watched
                          group item by item.Parent into p
                          select new { PARENT = p.Key, items = p.ToList() };

            foreach (var config in configs.ToList())
            {
                var selected = PropertyAdvisor.GetSelectedExternalToolParameter(config.PARENT.Parameters);
                var path = PropertyAdvisor.GetExternalResultLocationParameter(config.PARENT.Parameters, selected);

                //TODO: add each config/connector from result file to graph
                //foreach (var item in config.items)
                //{
                //    resultUpdater.ForceUpdate(path);
                //}

                resultUpdater.ForceUpdate(path);
            }
        }

        public void DesignSpaceExplore()
        {
            throw new NotImplementedException();
        }

        #region Property changed
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}