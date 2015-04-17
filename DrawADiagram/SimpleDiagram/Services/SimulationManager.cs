using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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
        public event EventHandler OnSimulationEnd;

        #region Properties
        private bool isRunning;

        public double DseProgress { get; private set; }

        public double CosimProgress { get; private set; }

        public PlotModel Results { get; private set; }

        public ObservableCollection<ConfigurationItemModel> Watchable { get; private set; }

        public List<ConfigurationItemModel> NotWatched
        {
            get { return Watchable.Where(item => !Watched.Any(x => x.Parent == item.Parent && x.Connector == item.Connector)).ToList(); }
        }

        public ObservableCollection<ConfigurationItemModel> Watched { get; private set; }

        public ObservableCollection<DSEConfigurationItemModel> DSEParameters { get; private set; }
        #endregion

        public SimulationManager(IModelManager modelManager, IResultUpdater resultUpdater)
        {
            isRunning = false;
            Results = new PlotModel();
            Results.Series.Add(new LineSeries());
            DseProgress = 0;
            CosimProgress = 0;

            Watched = new ObservableCollection<ConfigurationItemModel>();
            Watchable = new ObservableCollection<ConfigurationItemModel>();
            DSEParameters = new ObservableCollection<DSEConfigurationItemModel>();

            //Watched.CollectionChanged += (sender, e) => { UpdateConfiguration(); };

            this.modelManager = modelManager;
            this.resultUpdater = resultUpdater;

            // Initializing default eventhandler
            OnConfigureRequest += (sender, args) => { };
            OnPlotUpdated += (sender, series) => { };
            OnSimulationEnd += (sender, e) => { };

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

            Watchable = map.Values.ToObservableCollection();
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

        public void AddDesignSpaceParameter(DSEConfigurationItemModel item)
        {
            DSEParameters.Add(item);
            OnPropertyChanged("DSEParameters");
        }

        public void RemoveDesignSpaceParameter(DSEConfigurationItemModel item)
        {
            DSEParameters.Remove(item);
            OnPropertyChanged("DSEParameters");
        }

        public void StopSimulation()
        {
            if (IsRunning)
            {
                resultUpdater.StopWatching();
                IsRunning = false;
                OnSimulationEnd(this, null);
            }
        }

        public void Cosimulate()
        {
            if (!CanRun || IsRunning) return;

            IsRunning = true;

            //TODO: open simulation
            UpdateConfiguration();
            FakeCoSimProgress(10000);
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
            if (IsRunning) return;

            IsRunning = true;
            //TODO: start DSE
            
            FakeDesignSpaceExplorationProgress(10000);
        }

        private void FakeDesignSpaceExplorationProgress(int duration)
        {
            const double epsilon = 0.005;
            const int step = 50;


            var task = Task.Factory.StartNew(() =>
            {
                DseProgress = 0;
                OnPropertyChanged("DseProgress");
                double stop = duration;
                double counter = 0;
                while (stop - counter > epsilon)
                {
                    DseProgress = (counter/stop)*100;
                    OnPropertyChanged("DseProgress");
                    counter += step;
                    Thread.Sleep(step);
                }
                DseProgress = 100;
                OnPropertyChanged("DseProgress");

                StopSimulation();
            });
        }

        private void FakeCoSimProgress(int duration)
        {
            const double epsilon = 0.005;
            const int step = 50;

            var task = Task.Factory.StartNew(() =>
            {
                CosimProgress = 0;
                OnPropertyChanged("CosimProgress");
                double stop = duration;
                double counter = 0;
                while (stop - counter > epsilon)
                {
                    CosimProgress = (counter/stop)*100;
                    OnPropertyChanged("CosimProgress");
                    counter += step;
                    Thread.Sleep(step);
                }
                CosimProgress = 100;
                OnPropertyChanged("CosimProgress");

                StopSimulation();
            });
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