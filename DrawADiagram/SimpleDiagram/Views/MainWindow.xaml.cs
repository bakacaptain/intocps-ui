using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ModelLibrary.Models;
using ModelLibrary.Utils;
using Ninject;
using SimpleDiagram.Bootstrap;
using SimpleDiagram.Models;
using SimpleDiagram.Services;
using SimpleDiagram.Utils;

namespace SimpleDiagram.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // Dependency injection framework
            var kernel = new StandardKernel(new InjectModule());
            var modelManager = kernel.Get<IModelManager>();
            var simManager = kernel.Get<ISimulationManager>();

            InitializeComponent();
            ParamList.ItemsSource = new Dictionary<string, string>();
            PlotView.Model = simManager.Results;

            var watchedBind = new Binding {Source = simManager,Path = new PropertyPath("Watched"),Mode=BindingMode.OneWay};
            WatchedResultVar.SetBinding(ListBox.ItemsSourceProperty, watchedBind);

            modelManager.OnModelAdded += AddModel;
            modelManager.OnModelRemoved += RemoveModel;
            modelManager.OnViewModelParameters += UpdateParams;
            DiagramCanvas.OnModelCreate += p => modelManager.CreateModel(p.X,p.Y);
            DiagramCanvas.OnConnectionAdded += c => modelManager.AddConnection(c);
            simManager.OnConfigureRequest += (sender, args) =>
            {
                simManager.ConfigureConnections();
            };

            ConfigureCoeButton.Click += (sender, e) =>
            {
                simManager.ConfigureConnections();
                NotWatchedResultVar.ItemsSource = simManager.NotWatched;
            };
            AddWatchableResultVarButton.Click += (sender, e) =>
            {
                var item = NotWatchedResultVar.SelectedValue as ConfigurationItemModel;
                if (item != null)
                {
                    simManager.WatchVariable(item);
                    NotWatchedResultVar.ItemsSource = simManager.NotWatched;           
                }
            };
            WatchedResultVar.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Delete)
                {
                    var item = WatchedResultVar.SelectedValue as ConfigurationItemModel;
                    simManager.UnWatchVariable(item);
                    NotWatchedResultVar.ItemsSource = simManager.NotWatched;  
                }
            };
            CosimulationButton.Click += (sender, e) => { simManager.Cosimulate(); };
            StopSimulationButton.Click += (sender, e) => { simManager.StopSimulation(); };

            #region model

            var connector2 = new Connector(){Type = Direction.OUT, Name = "C2"};
            var connectors = new PropertyObservingCollection<Connector>()
            {
                new Connector(){Type = Direction.IN, Name = "C1"},
                connector2,
                new Connector(){Type = Direction.OUT, Name = "C3"},
            };

            var block1 = new Block()
            {
                Name = "Block1",
                Position = new Point(60,100),
                Connectors = connectors
            };

            var connector4 = new Connector() {Type = Direction.IN, Name="C4"};
            var connectors2 = new PropertyObservingCollection<Connector>()
            {
                connector4
            };

            var block2 = new Block()
            {
                Name = "Block2",
                Position = new Point(300, 100),
                Connectors = connectors2
            };

            var selected = "Overture";
            PropertyAdvisor.SetSelectedExternalToolParameter(block2.Parameters, selected);
            PropertyAdvisor.SetExternalToolParameter(block2.Parameters, "Overture", @"C:\Study\Overture\Overture.exe", @"C:\Users\Sam\Documents\Overture\workspace\Nested", "-import");
            PropertyAdvisor.SetExternalResultLocationParameter(block2.Parameters, selected, @"C:\Users\Sam\Desktop\data2.csv");

            PropertyAdvisor.SetSelectedExternalToolParameter(block1.Parameters, selected);
            PropertyAdvisor.SetExternalToolParameter(block1.Parameters, "Overture", @"C:\Study\Overture\Overture.exe", @"C:\Users\Sam\Documents\Overture\workspace\TestProject", "-import");
            PropertyAdvisor.SetExternalResultLocationParameter(block1.Parameters, selected, @"C:\Users\Sam\Desktop\data.csv");

            #endregion model

            // Creating a new Block using the manager
            modelManager.CreateModel(block1);
            modelManager.CreateModel(block2);

            //TODO: bind to the model instead of the viewmodel of the connector
            //var connection = new ConnectionViewModel(connectorIn, connectorOut);
        }

        private void AddModel(object sender, BlockViewModel model)
        {
            DiagramCanvas.Add(model);
        }

        private void UpdateParams(object sender, BlockViewModel model)
        {
            TabControl.SelectedIndex = 1;
            var displayNameBind = new Binding
            {
                Source = model,
                Path = new PropertyPath("BlockModel.Name"),
                Mode = BindingMode.TwoWay
            };

            var versionBind = new Binding
            {
                Source = model,
                Path = new PropertyPath("BlockModel.Version"),
                Mode = BindingMode.OneWay
            };

            ParamDisplayName.SetBinding(TextBox.TextProperty, displayNameBind);
            ParamModelVersion.SetBinding(ContentProperty, versionBind);

            ParamList.ItemsSource = model.BlockModel.Parameters;
            IOList.ItemsSource = model.BlockModel.Connectors;
        }

        private void RemoveModel(object sender, BlockViewModel model)
        {
            DiagramCanvas.Remove(model);
        }
    }
}
