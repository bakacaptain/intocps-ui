using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ModelLibrary.Models;
using SimpleDiagram.Models;

namespace SimpleDiagram.Services
{
    public class SimulationManager : ISimulationManager
    {
        private readonly IModelManager modelManager;
        public event EventHandler OnConfigureRequest;

        public SimulationManager(IModelManager modelManager)
        {
            this.modelManager = modelManager;
            // Initializing default eventhandler
            OnConfigureRequest += (sender, args) => { };
        }

        public void Configure()
        {
            OnConfigureRequest(this, null);
        }

        public void ConfigureConnections()
        {
            Console.WriteLine(@"Configuring connections");
            var models = modelManager.GetBlocks();
            var connections = modelManager.GetConnections();

            var map = new Dictionary<Connector, Block>();

            foreach (var model in models)
            {
                foreach (var connector in model.BlockModel.Connectors)
                {
                    map.Add(connector, model.BlockModel);
                }
            }

            foreach (var connection in connections)
            {
                var sink = connection.Sink.Connector;
                var source = connection.Source.Connector;
                if (sink != null && source != null)
                {
                    Console.WriteLine("Connecting {0}.{1} with {2}.{3}\nCOE: {4} --> {5}\n#:{6}",
                        map[source].Name, source.Name, map[sink].Name, sink.Name, source.Hook, sink.Hook,connection.GetHashCode());
                }
                else
                {
                    Console.WriteLine("Sink: {0} Sourc: {1}\n#:{2}",sink,source,connection.GetHashCode());
                }
                
            }
        }


        public bool CanRun()
        {
            throw new System.NotImplementedException();
        }

        public void Cosimulate()
        {
            throw new NotImplementedException();
        }

        public void DesignSpaceExplore()
        {
            throw new NotImplementedException();
        }
    }
}