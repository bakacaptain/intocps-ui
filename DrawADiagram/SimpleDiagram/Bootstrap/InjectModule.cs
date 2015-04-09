using ModelLibrary.Models;
using Ninject.Extensions.Factory;
using Ninject.Modules;
using SimpleDiagram.Controls;
using SimpleDiagram.Factories;
using SimpleDiagram.Models;
using SimpleDiagram.Services;
using SimpleDiagram.Utils;

namespace SimpleDiagram.Bootstrap
{
    public class InjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IModelManager>().To<ModelManager>().InSingletonScope();
            Bind<ISimulationManager>().To<SimulationManager>().InSingletonScope();
            Bind<IReader>().To<Reader>();
            Bind<IResultParser>().To<ResultParser>();
            Bind<IResultUpdater>().To<ResultUpdater>();
            Bind<BlockViewModel>().ToSelf();
            Bind<DiagramCanvas>().ToSelf();
            Bind<Connector>().ToSelf();

            // Read the Ninject Factory Extension for understanding the magic
            Bind<IModelFactory>().ToFactory();
            Bind<IConnectorFactory>().ToFactory();
        }
    }
}