using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
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

        public ModelManager(IModelFactory modelFactory, IConnectorFactory connectorFactory)
        {
            this.modelFactory = modelFactory;
            this.connectorFactory = connectorFactory;

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
                Position = new Point(x,y),
            };

            CreateModel(block);
        }

        public void CreateModel(Block block)
        {
            var rect = new Rectangle
            {
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                IsHitTestVisible = false
            };

            var grid = new Grid();
            grid.Children.Add(rect);
            grid.Children.Add(new TextBlock()
            {
                Text = "<<Block>>",
                IsHitTestVisible = false
            });

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
        }

        public void RemoveModel(BlockViewModel model)
        {
            //TODO: dispose connection objects
            //TODO: dispose this object
            OnModelRemoved(this, model);
        }

        public void CopyModel(BlockViewModel model)
        {
            var copy = model.CloneJson();
            var position = model.BlockModel.Position;
            copy.BlockModel.Position = new Point(position.X+20,position.Y+20);
            OnModelAdded(this, copy);
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

            var args = string.Format("{0} {1}", 
                parameters.First(parm => parm.Key == PropertyAdvisor.ARGUMENT).Value,
                parameters.First(parm => parm.Key == PropertyAdvisor.PROJECT).Value);
            Task.Factory.StartNew(() => SimpleProcessSpawner.Start(parameters.First(parm => parm.Key == PropertyAdvisor.TOOL).Value, args));
        }

        public void AddInputConnector(BlockViewModel model)
        {
            var newConnector = connectorFactory.Create();
            var name = string.Format("input_{0}", model.BlockModel.Connectors.Count);
            newConnector.Name = name;
            newConnector.Type = Direction.IN;
            model.BlockModel.Connectors.Add(newConnector);
        }

        public void AddOutputConnector(BlockViewModel model)
        {
            var newConnector = connectorFactory.Create();
            var name = string.Format("output_{0}", model.BlockModel.Connectors.Count);
            newConnector.Name = name;
            newConnector.Type = Direction.OUT;
            model.BlockModel.Connectors.Add(newConnector);
        }
    }
}