using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OxyPlot;
using SimpleDiagram.Models;

namespace SimpleDiagram.Services
{
    public interface ISimulationManager
    {
        event EventHandler OnConfigureRequest;
        event EventHandler OnPlotUpdated;
        event EventHandler OnSimulationEnd;

        double DseProgress { get; }
        double CosimProgress { get; }
        PlotModel Results { get; }
        List<ConfigurationItemModel> NotWatched { get; }
        ObservableCollection<ConfigurationItemModel> Watched { get; }
        ObservableCollection<ConfigurationItemModel> Watchable { get; } 
        ObservableCollection<DSEConfigurationItemModel> DSEParameters { get; } 

        /// <summary>
        /// Maps the connections of the structural model with the interfaces of the simulation model
        /// </summary>
        void Configure();

        /// <summary>
        /// 
        /// </summary>
        void ConfigureConnections();

        void WatchVariable(ConfigurationItemModel item);
        void UnWatchVariable(ConfigurationItemModel item);

        void AddDesignSpaceParameter(DSEConfigurationItemModel item);
        void RemoveDesignSpaceParameter(DSEConfigurationItemModel item);

        bool CanRun { get; }
        bool IsRunning { get; }

        void StopSimulation();

        void Cosimulate();

        void DesignSpaceExplore();
    }
}