using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ModelLibrary.Models;
using Ninject;
using SimpleDiagram.Controls;
using SimpleDiagram.Controls.Actions;
using SimpleDiagram.Factories;
using SimpleDiagram.Services;

namespace SimpleDiagram.Models
{
    //These attributes identify the types of the named parts that are used for templating
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_ConnectorDecorator", Type = typeof(Control))]
    [TemplatePart(Name = "PART_DragThumb", Type = typeof(DragThumb))]

    [Serializable]
    public class BlockViewModel : ContentControl, ISelectable
    {
        // Property injection using Ninject
        [Inject]
        public IModelManager ModelManager { private get; set; }

        #region Model Binding
        private Block blockModel;

        public Block BlockModel
        {
            get { return blockModel; }
            set
            {
                if (blockModel != null)
                {
                    blockModel.PropertyChanged -= OnModelChanged;
                    blockModel.Connectors.CollectionChanged -= OnConnectorsChanged;
                }
                blockModel = value;
                blockModel.PropertyChanged += OnModelChanged;
                blockModel.Connectors.CollectionChanged += OnConnectorsChanged;
            }
        }

        private void OnModelChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Height")
            {
                Height = blockModel.Height;
            }
            if (e.PropertyName == "Width")
            {
                Width = blockModel.Width;
            }

            DiagramCanvas.UpdatePosition(this);
        }
        private void OnConnectorsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ModelManager.RedrawModel(this);
        }

        #endregion Model Binding

        static BlockViewModel()
        {
            // set the key to reference the style for this control
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(BlockViewModel), 
                new FrameworkPropertyMetadata(typeof(BlockViewModel))
            );
        }

        public BlockViewModel()
        {
            Loaded += new RoutedEventHandler(OnLoaded);
            InitializeComponents();
        }

        #region View events

        private void InitializeComponents()
        {
            var openMenuItem = new MenuItem { Header = "Open in Tool" };
            openMenuItem.Click += OpenModelInExternalTool;

            var removeMenuItem = new MenuItem { Header = "Remove" };
            removeMenuItem.Click += RemoveModelFromDiagram;

            var paramMenuItem = new MenuItem { Header = "Parameters" };
            paramMenuItem.Click += OpenParameterMenu;

            var addInputMenuItem = new MenuItem {Header = "Add Input Port"};
            addInputMenuItem.Click += AddInputMenu;

            var addOutputMenuItem = new MenuItem {Header = "Add Output Port"};
            addOutputMenuItem.Click += AddOutputMenu;

            ContextMenu = new ContextMenu();
            ContextMenu.Items.Add(openMenuItem);
            ContextMenu.Items.Add(removeMenuItem);
            ContextMenu.Items.Add(paramMenuItem);
            ContextMenu.Items.Add(addInputMenuItem);
            ContextMenu.Items.Add(addOutputMenuItem);
        }

        private void AddOutputMenu(object sender, RoutedEventArgs e)
        {
            ModelManager.AddOutputConnector(this);
        }

        private void AddInputMenu(object sender, RoutedEventArgs e)
        {
            ModelManager.AddInputConnector(this);
        }

        private void OpenModelInExternalTool(object sender, RoutedEventArgs e)
        {
            ModelManager.OpenModelInExternalTool(this);
        }

        private void RemoveModelFromDiagram(object sender, RoutedEventArgs e)
        {
            ModelManager.RemoveModel(this);
        }

        private void OpenParameterMenu(object sender, RoutedEventArgs e)
        {
            ModelManager.DisplayParameters(this);
        }

        #endregion View events

        #region Events
        /// <summary> 
        /// Note: this method is only executed when the Loaded event is fired, 
        /// so setting DragThumbTemplate or ConnectorDecoratorTemplate 
        /// properties after will have no effect.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Template != null)
            {
                var contentPresenter = Template.FindName("PART_ContentPresenter", this) as ContentPresenter;
                if (contentPresenter != null)
                {
                    var contentVisual = VisualTreeHelper.GetChild(contentPresenter, 0) as UIElement;
                    if (contentVisual != null)
                    {
                        // Find your templates in here

                        var drag = Template.FindName("PART_DragThumb", this) as DragThumb;
                        if (drag != null)
                        {
                            var template = GetDragThumbTemplate(contentVisual);
                            if (template != null)
                            {
                                drag.Template = template;
                            }
                        }

                        var connector = Template.FindName("PART_ConnectorDecorator", this) as Control;
                        if (connector != null)
                        {
                            connector.Template = ConnectorXamlFactory.CreateTemplate("ConnectorDecoratorTemplate", BlockModel.Connectors);
                        }
                    }
                }
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            var diagram = VisualTreeHelper.GetParent(this) as DiagramCanvas;

            // update selection
            if (diagram != null)
                if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                    if (this.IsSelected)
                    {
                        this.IsSelected = false;
                        diagram.SelectedElements.Remove(this);
                    }
                    else
                    {
                        this.IsSelected = true;
                        diagram.SelectedElements.Add(this);
                    }
                else if (!this.IsSelected)
                {
                    foreach (ISelectable item in diagram.SelectedElements)
                        item.IsSelected = false;

                    diagram.SelectedElements.Clear();
                    this.IsSelected = true;
                    diagram.SelectedElements.Add(this);
                }
            e.Handled = false;
        }

        
        #endregion Events

        #region IsSelected

        public bool IsSelected
        {
            get { return (bool) GetValue(IsSelectedProperty); } 
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty = 
            DependencyProperty.Register("IsSelected",typeof(bool),typeof(BlockViewModel),new FrameworkPropertyMetadata(false)); 

        #endregion IsSelected

        #region IsDragConnectionOver

        // while drag connection procedure is ongoing and the mouse moves over 
        // this item this value is true; if true the ConnectorDecorator is triggered
        // to be visible, see template
        public bool IsDragConnectionOver
        {
            get { return (bool)GetValue(IsDragConnectionOverProperty); }
            set { SetValue(IsDragConnectionOverProperty, value); }
        }

        public static readonly DependencyProperty IsDragConnectionOverProperty =
            DependencyProperty.Register("IsDragConnectionOver",
                                         typeof(bool),
                                         typeof(BlockViewModel),
                                         new FrameworkPropertyMetadata(false));

        #endregion IsDragConnectionOver

        #region DragTemplate

        public static readonly DependencyProperty DragThumbTemplateProperty = DependencyProperty.RegisterAttached("DragThumbTemplate", typeof(ControlTemplate), typeof(BlockViewModel));

        public static ControlTemplate GetDragThumbTemplate(UIElement element)
        {
            return (ControlTemplate) element.GetValue(DragThumbTemplateProperty);
        }

        public static void SetDragThumbTemplate(UIElement element, ControlTemplate template)
        {
            element.SetValue(DragThumbTemplateProperty, template);
        }

        #endregion DragTemlpate

        #region ConnectorTemplate

        // can be used to replace the default template for the ConnectorDecorator
        public static readonly DependencyProperty ConnectorDecoratorTemplateProperty = DependencyProperty.RegisterAttached("ConnectorDecoratorTemplate", typeof(ControlTemplate), typeof(BlockViewModel));

        public static ControlTemplate GetConnectorDecoratorTemplate(UIElement element)
        {
            return (ControlTemplate) element.GetValue(ConnectorDecoratorTemplateProperty);
        }

        public static void SetConnectorDecoratorTemplate(UIElement element, ControlTemplate value)
        {
            element.SetValue(ConnectorDecoratorTemplateProperty, value);
        }

        #endregion ConnectorTemplate

        #region Parameters tab

        public string DisplayName
        {
            get { return BlockModel.Name; }
            set { BlockModel.Name = value; }
        }

        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register(
                "DisplayName",
                typeof (string),
                typeof(BlockViewModel));

        #endregion
    }
}