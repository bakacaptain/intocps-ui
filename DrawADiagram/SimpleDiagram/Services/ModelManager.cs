using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Castle.Core.Internal;
using ModelLibrary.Models;
using SimpleDiagram.Factories;
using SimpleDiagram.Models;
using SimpleDiagram.Utils;

namespace SimpleDiagram.Services
{
    public class ModelManager : IModelManager
    {
        private readonly IModelFactory modelFactory;
        private readonly IConnectorFactory connectorFactory;
        public event EventHandler<Block> OnModelRedraw;
        public event EventHandler<BlockViewModel> OnModelAdded;
        public event EventHandler<BlockViewModel> OnModelRemoved;
        public event EventHandler<BlockViewModel> OnViewModelParameters;

        private List<BlockViewModel> blockViewModels;
        private List<ConnectionViewModel> connectionViewModels;

        public ModelManager(IModelFactory modelFactory, IConnectorFactory connectorFactory)
        {
            this.modelFactory = modelFactory;
            this.connectorFactory = connectorFactory;

            blockViewModels = new List<BlockViewModel>();
            connectionViewModels = new List<ConnectionViewModel>();

            // Initializing default eventhandlers
            OnModelRedraw += (sender, block) => { };
            OnModelAdded += (sender, model) => { };
            OnModelRemoved += (sender, model) => { };
            OnViewModelParameters += (sender, parameters) => { };
        }


        public void CreateModel(double x, double y)
        {
            var block = new Block()
            {
                Name = "New Block",
                Version = "N/A",
                Position = new Point(x,y),
            };

            CreateModel(block);
        }

        public void CreateModel(Block block)
        {
            #region internal elements
            var rect = new Rectangle
            {
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                IsHitTestVisible = false
            };

            var blockTitle = new TextBlock()
            {
                Text = "<<Block>>",
                IsHitTestVisible = false,
                FontSize = 10,
                FontStyle = FontStyles.Oblique,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var blockName = new TextBlock
            {
                IsHitTestVisible = false,
                FontSize = 11,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            var nameBind = new Binding
            {
                Source = block,
                Path = new PropertyPath("Name"),
                Mode = BindingMode.OneWay
            };
            blockName.SetBinding(TextBlock.TextProperty, nameBind);
            #endregion

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            stack.Children.Add(blockTitle);
            stack.Children.Add(blockName);

            var grid = new Grid();
            grid.Children.Add(rect);
            grid.Children.Add(stack);

            CreateModel(block, grid);
        }

        public void CreateModel(Block block, object content)
        {
            var model = modelFactory.CreateModel();
            model.Content = content;
            model.BlockModel = block;
            model.Height = 100;
            model.Width = 80;

            AddModel(model);
        }

        public void AddModel(BlockViewModel model)
        {
            OnModelAdded(this, model);
            blockViewModels.Add(model);
        }

        public void RemoveModel(BlockViewModel model)
        {
            //TODO: dispose connection objects
            //TODO: dispose this object
            OnModelRemoved(this, model);
            blockViewModels.Remove(model);
        }

        public void CopyModel(BlockViewModel model)
        {
            var copy = model.CloneJson();
            var position = model.BlockModel.Position;
            copy.BlockModel.Position = new Point(position.X+20,position.Y+20);
            OnModelAdded(this, copy);
            blockViewModels.Add(copy);
        }

        public void DisplayParameters(BlockViewModel model)
        {
            OnViewModelParameters(this, model);
        }

        public void RedrawModel(BlockViewModel model)
        {
            var connector = model.Template.FindName("PART_ConnectorDecorator", model) as Control;
            if (connector != null)
            {
                connector.Template = ConnectorXamlFactory.CreateTemplate("ConnectorDecoratorTemplate", model.BlockModel.Connectors);
            }
            OnModelRedraw(this, model.BlockModel);
        }

        public void OpenModelInExternalTool(BlockViewModel model)
        {
            var parameters = PropertyAdvisor.GetExternalToolParameter(model.BlockModel.Parameters, "Overture");

            var arg = parameters.First(parm => parm.Key == PropertyAdvisor.ARGUMENT).Value;
            var proj = parameters.First(parm => parm.Key == PropertyAdvisor.PROJECT).Value;
            var processname = parameters.First(parm => parm.Key == PropertyAdvisor.TOOL).Value;

            if (!(processname.IsNullOrEmpty() || proj.IsNullOrEmpty()))
            {
                var commands = string.Format("{0} {1}", arg, proj);
                Task.Factory.StartNew(() =>
                {
                    SimpleProcessSpawner.Start(processname, commands);
                });
            }
        }

        public void AddInputConnector(BlockViewModel model)
        {
            var newConnector = connectorFactory.Create();
            var name = string.Format("input_{0}", model.BlockModel.Connectors.Count);
            newConnector.Name = name;
            newConnector.Type = Direction.IN;
            newConnector.UnitType = "N/A";
            newConnector.Hook = "N/A";
            model.BlockModel.Connectors.Add(newConnector);
        }

        public void AddOutputConnector(BlockViewModel model)
        {
            var newConnector = connectorFactory.Create();
            var name = string.Format("output_{0}", model.BlockModel.Connectors.Count);
            newConnector.Name = name;
            newConnector.Type = Direction.OUT;
            newConnector.UnitType = "N/A";
            newConnector.Hook = "N/A";
            model.BlockModel.Connectors.Add(newConnector);
        }

        public void AddConnection(ConnectionViewModel connection)
        {
            connectionViewModels.Add(connection);
            connection.Sink.PropertyChanged += (sender, args) =>
            {
                Console.WriteLine("Property changed");
            };
        }

        public void RemoveConnection(ConnectionViewModel connection)
        {
            connectionViewModels.Remove(connection);
        }

        public IEnumerable<BlockViewModel> GetBlocks()
        {
            return blockViewModels;
        }

        public IEnumerable<ConnectionViewModel> GetConnections()
        {
            return connectionViewModels;
        }
    }
}