using ModelLibrary.Models;
using Ninject.Extensions.Factory;
using Ninject.Modules;
using SimpleDiagram.Controls;
using SimpleDiagram.Factories;
using SimpleDiagram.Models;
using SimpleDiagram.Services;

namespace SimpleDiagram.Bootstrap
{
    public class InjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IModelManager>().To<ModelManager>().InSingletonScope();
            Bind<ISimulationManager>().To<SimulationManager>().InSingletonScope();
            Bind<BlockViewModel>().ToSelf();
            Bind<DiagramCanvas>().ToSelf();
            Bind<Connector>().ToSelf();

            // Read the Ninject Factory Extension for understanding the magic
            Bind<IModelFactory>().ToFactory();
            Bind<IConnectorFactory>().ToFactory();
        }
    }
}