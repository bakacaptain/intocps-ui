using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using ModelLibrary.Models;
using ModelLibrary.Utils;
using Ninject;
using OxyPlot;
using OxyPlot.Series;
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
            

            modelManager.OnModelAdded += AddModel;
            modelManager.OnModelRemoved += RemoveModel;
            modelManager.OnViewModelParameters += UpdateParams;
            DiagramCanvas.OnModelCreate += p => modelManager.CreateModel(p.X,p.Y);
            DiagramCanvas.OnConnectionAdded += c => modelManager.AddConnection(c);
            simManager.OnConfigureRequest += (sender, args) =>
            {
                simManager.ConfigureConnections();
            };
            ConfigureCoeButton.Click += (sender, e) => { simManager.ConfigureConnections(); };
            LoadResultItemsButton.Click += (sender, e) =>
            {
                var blocks = modelManager.GetBlocks();
                var models = new ObservableCollection<ResultItemModel>();
                foreach (var block in blocks)
                {
                    foreach (var connector in block.BlockModel.Connectors)
                    {
                        models.Add(new ResultItemModel
                        {
                            Connector = connector,
                            ParentModelName = block.BlockModel.Name,
                            //TODO: include first axis label and second axis
                        });
                    }
                }
                WatchableResultVar.ItemsSource = models;

            
                ResultsViewModel.Results = new PlotModel { Title = "Test Graph" };
                ResultsViewModel.Results.Series.Add(new FunctionSeries(Math.Sin, 0, 10, 0.1, "sin(x)"));
            };

            #region fillings
            Rectangle rect = new Rectangle
            {
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                IsHitTestVisible = false
            };

            Grid grid = new Grid();
            grid.Children.Add(rect);
            grid.Children.Add(new TextBlock()
            {
                Text = "<<Block>>",
                IsHitTestVisible = false
            });


            var rect2 = new Rectangle
            {
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                IsHitTestVisible = false
            };

            var grid2 = new Grid();
            grid2.Children.Add(rect2);
            grid2.Children.Add(new TextBlock()
            {
                Text = "<<Block>>",
                IsHitTestVisible = false
            });
            #endregion fillings

            #region model

            var connectorOut = new Connector(){Type = Direction.OUT};
            var connectors = new PropertyObservingCollection<Connector>()
            {
                new Connector(){Type = Direction.IN},
                connectorOut,
                new Connector(){Type = Direction.OUT},
            };

            var block1 = new Block()
            {
                Name = "Block1",
                Position = new Point(60,100),
                Connectors = connectors
            };

            var connectorIn = new Connector() {Type = Direction.IN};
            var c2 = new PropertyObservingCollection<Connector>()
            {
                connectorIn
            };

            var block2 = new Block()
            {
                Name = "Block2",
                Position = new Point(300, 100),
                Connectors = c2
            };

            PropertyAdvisor.SetExternalToolParameter(block2.Parameters, "Overture", @"C:\Study\Overture\Overture.exe", @"C:\Users\Sam\Documents\Overture\workspace\Nested", "-import");

            PropertyAdvisor.SetExternalToolParameter(block1.Parameters, "Overture", @"C:\Study\Overture\Overture.exe", @"C:\Users\Sam\Documents\Overture\workspace\TestProject", "-import");

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
