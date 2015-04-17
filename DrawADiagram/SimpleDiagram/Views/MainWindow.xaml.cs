using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using ModelLibrary.Models;
using ModelLibrary.Utils;
using Ninject;
using SimpleDiagram.Bootstrap;
using SimpleDiagram.Models;
using SimpleDiagram.Services;
using SimpleDiagram.Utils;
using Binding = System.Windows.Data.Binding;
using DataGrid = System.Windows.Controls.DataGrid;
using ListBox = System.Windows.Controls.ListBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using ProgressBar = System.Windows.Controls.ProgressBar;
using TextBox = System.Windows.Controls.TextBox;

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
            AlgorithmSelector.ItemsSource = new List<string> {"Fixed Step-size", "Variable Step-size"};

            var scriptList = new List<ScriptConfigurationItemModel>
            {
                new ScriptConfigurationItemModel{Location = "SensorFail1", Use = false},
                new ScriptConfigurationItemModel{Location = "SensorFail2", Use = false},
                new ScriptConfigurationItemModel{Location = "MotorInput", Use = true},
            };
            scriptListCosim.ItemsSource = scriptList;
            scriptListDse.ItemsSource = scriptList;

            var watchedBind = new Binding {Source = simManager,Path = new PropertyPath("Watched"),Mode=BindingMode.OneWay};
            WatchedResultVar.SetBinding(ListBox.ItemsSourceProperty, watchedBind);
            var dseParamBind = new Binding {Source = simManager,Path = new PropertyPath("DSEParameters"),Mode = BindingMode.OneWay};
            DseParameterList.SetBinding(DataGrid.ItemsSourceProperty, dseParamBind);
            var dseProgressBind = new Binding {Source=simManager, Path = new PropertyPath("DseProgress"), Mode=BindingMode.OneWay};
            DseProgressBar.SetBinding(ProgressBar.ValueProperty, dseProgressBind);
            var cosimProgressBind = new Binding { Source = simManager, Path = new PropertyPath("CosimProgress"), Mode = BindingMode.OneWay };
            CosimProgressBar.SetBinding(ProgressBar.ValueProperty, cosimProgressBind);

            modelManager.OnModelAdded += AddModel;
            modelManager.OnModelRemoved += RemoveModel;
            modelManager.OnViewModelParameters += UpdateParams;
            DiagramCanvas.OnModelCreate += p => modelManager.CreateModel(p.X,p.Y);
            DiagramCanvas.OnConnectionAdded += c => modelManager.AddConnection(c);
            modelManager.OnEntitiesUpdated += (sender, args) =>
            {
                simManager.ConfigureConnections();
                NotWatchedResultVar.ItemsSource = simManager.NotWatched;
                DataGridInputOptions.ItemsSource = simManager.Watchable;
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
            DseButton.Click += (sender, e) => { simManager.DesignSpaceExplore(); };
            StopDseButton.Click += (sender, e) => { simManager.StopSimulation(); };

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


            // Creating a new Block using the manager
            //modelManager.CreateModel(block1);
            //modelManager.CreateModel(block2);

            #endregion model

            #region LineFollower Robot Model

            var controllerConnectors = new PropertyObservingCollection<Connector>
            {
                new Connector{ Name = "sensorSignalLeft", Type = Direction.IN, DataType = "Real"},
                new Connector{ Name = "sensorSignalRight", Type = Direction.IN, DataType = "Real"},
                new Connector{ Name = "motorSignalLeft", Type = Direction.OUT, DataType = "Real"},
                new Connector{ Name = "motorSignalRight", Type = Direction.OUT, DataType = "Real"},
                new Connector{ Name = "encoderSignalLeft", Type = Direction.IN, DataType = "Real"},
                new Connector{ Name = "encoderSignalRight", Type = Direction.IN, DataType = "Real"}
            };

            var bodyConnectors = new PropertyObservingCollection<Connector>
            {
                new Connector{ Name = "toBodyLeft", Type = Direction.IN, DataType = "TranslationalForce"},
                new Connector{ Name = "toBodyRight", Type = Direction.IN, DataType = "TranslationalForce"},
                new Connector{ Name = "robotPosition", Type = Direction.OUT, DataType = "Position"}
            };

            var lineConnectors = new PropertyObservingCollection<Connector>
            {
                new Connector{ Name = "robotPosition", Type = Direction.IN, DataType = "Position"},
                new Connector{ Name="opticalReflectionLeft", Type = Direction.OUT, DataType = "Real"},
                new Connector{ Name="opticalReflectionRight", Type = Direction.OUT, DataType = "Real"}
            };

            var motorLeftConnectors = new PropertyObservingCollection<Connector>
            {
                new Connector{ Name = "motorSignalLeft", Type = Direction.IN, DataType = "Real"},
                new Connector{ Name = "toEncoderLeft", Type = Direction.OUT, DataType = "RotationForce"},
                new Connector{ Name = "toWheelLeft", Type = Direction.OUT, DataType = "RotationForce"}
            };

            var encoderLeftConnectors = new PropertyObservingCollection<Connector>
            {
                new Connector{ Name = "toEncoderLeft", Type = Direction.IN, DataType = "RotationForce"},
                new Connector{ Name = "encoderSignalLeft", Type = Direction.OUT, DataType = "Real"},
            };

            var wheelLeftConnectors = new PropertyObservingCollection<Connector>
            {
                new Connector{ Name = "toWheelLeft", Type = Direction.IN, DataType = "RotationForce"},
                new Connector{ Name = "toBodyLeft", Type = Direction.OUT, DataType = "TranslationalForce"}
            };

            var sensorLeftConnectors = new PropertyObservingCollection<Connector>
            {
                new Connector{ Name = "sensorSignalLeft", Type = Direction.OUT, DataType = "Real"},
                new Connector{ Name="opticalReflectionLeft", Type = Direction.IN, DataType = "Real"}
            };

            var motorRightConnectors = new PropertyObservingCollection<Connector>
            {
                new Connector{ Name = "motorSignalRight", Type = Direction.IN, DataType = "Real"},
                new Connector{ Name = "toEncoderRight", Type = Direction.OUT, DataType = "RotationForce"},
                new Connector{ Name = "toWheelRight", Type = Direction.OUT, DataType = "RotationForce"}
            };

            var encoderRightConnectors = new PropertyObservingCollection<Connector>
            {
                new Connector{ Name = "toEncoderRight", Type = Direction.IN, DataType = "RotationForce"},
                new Connector{ Name = "encoderSignalRight", Type = Direction.OUT, DataType = "Real"},
            };

            var wheelRightConnectors = new PropertyObservingCollection<Connector>
            {
                new Connector{ Name = "toWheelRight", Type = Direction.IN, DataType = "RotationForce"},
                new Connector{ Name = "toBodyRight", Type = Direction.OUT, DataType = "TranslationalForce"}
            };

            var sensorRightConnectors = new PropertyObservingCollection<Connector>
            {
                new Connector{ Name = "sensorSignalRight", Type = Direction.OUT, DataType = "Real"},
                new Connector{ Name="opticalReflectionRight", Type = Direction.IN, DataType = "Real"}
            };

            // ----------------------------

            var line = new Block
            {
                Name = "line",
                Connectors = lineConnectors,
                Position = new Point(580,290),
            };

            var controller = new Block
            {
                Name = "controller",
                Connectors = controllerConnectors,
                Position = new Point(60,290),
            };

            var body = new Block
            {
                Name = "body",
                Connectors = bodyConnectors,
                Position = new Point(450,290),
            };

            var leftMotor = new Block
            {
                Name = "motorLeft",
                Connectors = motorLeftConnectors,
                Position = new Point(190,370),
            };

            var leftEncoder = new Block
            {
                Name = "encoderLeft",
                Connectors = encoderLeftConnectors,
                Position = new Point(320,530),
            };

            var leftWheel = new Block
            {
                Name = "wheelLeft",
                Connectors = wheelLeftConnectors,
                Position = new Point(320,370),
            };

            var leftSensor = new Block
            {
                Name = "sensorLeft",
                Connectors = sensorLeftConnectors,
                Position = new Point(710,370),
            };

            // ----------------------------

            var rightMotor = new Block
            {
                Name = "motorRight",
                Connectors = motorRightConnectors,
                Position = new Point(190,210),
            };

            var rightEncoder = new Block
            {
                Name = "encoderRight",
                Connectors = encoderRightConnectors,
                Position = new Point(320,50),
            };

            var rightWheel = new Block
            {
                Name = "wheelRight",
                Connectors = wheelRightConnectors,
                Position = new Point(320,210),
            };

            var rightSensor = new Block
            {
                Name = "sensorRight",
                Connectors = sensorRightConnectors,
                Position = new Point(710,210),
            };

            // -----------------------------

            PropertyAdvisor.SetSelectedExternalToolParameter(controller.Parameters, selected);
            PropertyAdvisor.SetExternalToolParameter(controller.Parameters, "Overture", @"C:\Study\Overture\Overture.exe", @"C:\Users\Sam\Documents\Overture\workspace\RobotRT", "-import");
            PropertyAdvisor.SetExternalResultLocationParameter(controller.Parameters, selected, @"C:\Users\Sam\Desktop\data.csv");

            PropertyAdvisor.SetSelectedExternalToolParameter(line.Parameters, selected);
            PropertyAdvisor.SetExternalToolParameter(line.Parameters, "Overture", @"C:\Study\Overture\Overture.exe", @"C:\Users\Sam\Documents\Overture\workspace\RobotRT", "-import");
            PropertyAdvisor.SetExternalResultLocationParameter(line.Parameters, selected, @"C:\Users\Sam\Desktop\data2.csv");

            // -----------------------------

            modelManager.CreateModel(line);
            modelManager.CreateModel(controller);
            modelManager.CreateModel(body);
            modelManager.CreateModel(leftMotor);
            modelManager.CreateModel(leftEncoder);
            modelManager.CreateModel(leftSensor);
            modelManager.CreateModel(leftWheel);
            modelManager.CreateModel(rightMotor);
            modelManager.CreateModel(rightEncoder);
            modelManager.CreateModel(rightSensor);
            modelManager.CreateModel(rightWheel);

            #endregion

            //TODO: bind to the model instead of the viewmodel of the connector
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

        private void OnDataGridFileBrowseClicked(object sender, RoutedEventArgs e)
        {
            var item = ((FrameworkElement)sender).DataContext as DSEConfigurationItemModel;
            if (item != null)
            {
                var dialog = new OpenFileDialog {DefaultExt = "*.*", Filter = "All documents (.*)|*.*"};
                bool? result = dialog.ShowDialog();
                // Get the selected file name and display in a TextBox
                if (result == true)
                {
                    // Open document
                    string filename = dialog.FileName;
                    item.Filepath = filename;
                }
            }
        }

        private void OnBrowseDseOutputLocation(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            
        }
    }
}
