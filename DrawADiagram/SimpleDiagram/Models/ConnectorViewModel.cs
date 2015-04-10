using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ModelLibrary.Annotations;
using ModelLibrary.Models;
using SimpleDiagram.Controls;
using SimpleDiagram.Controls.Adorners;

namespace SimpleDiagram.Models
{
    public class ConnectorViewModel : Control, INotifyPropertyChanged
    {
        #region Properties
        
        #endregion

        public ConnectorViewModel()
        {
            LayoutUpdated += Connector_LayoutUpdated;
        }

        #region Events

        private void Connector_LayoutUpdated(object sender, EventArgs e)
        {
            var diagram = GetDiagramCanvas(this);
            if (diagram != null)
            {
                //get centre position of this Connector relative to the DesignerCanvas
                Connector.Position = TransformToAncestor(diagram).Transform(new Point(Width/2, Height/2));
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // if mouse button is not pressed we have no drag operation, ...
            if (e.LeftButton != MouseButtonState.Pressed)
                this.dragStartPoint = null;

            // but if mouse button is pressed and start point value is set we do have one
            if (this.dragStartPoint.HasValue)
            {
                // create connection adorner 
                var canvas = GetDiagramCanvas(this);
                if (canvas != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                    if (adornerLayer != null)
                    {
                        var adorner = new ConnectorAdorner(canvas, this);
                        if (adorner != null)
                        {
                            adornerLayer.Add(adorner);
                            e.Handled = true;
                        }
                    }
                }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            var canvas = GetDiagramCanvas(this);
            if (canvas != null)
            {
                // position relative to DesignerCanvas
                this.dragStartPoint = new Point?(e.GetPosition(canvas));
                e.Handled = true;
            }
        }



        #endregion Events

        private DiagramCanvas GetDiagramCanvas(DependencyObject element)
        {
            while (element != null && !(element is DiagramCanvas))
                element = VisualTreeHelper.GetParent(element);
            return element as DiagramCanvas;
        }

        // drag start point, relative to the DiagramCanvas
        private Point? dragStartPoint = null;

        public ConnectorOrientation Orientation { get; set; }

        #region DependencyProperties

        public static readonly DependencyProperty OrientationProperty =
           DependencyProperty.RegisterAttached("Type", typeof(ConnectorOrientation), typeof(ConnectorViewModel),
           new FrameworkPropertyMetadata(ConnectorOrientation.None,
                                         new PropertyChangedCallback(ConnectorViewModel.OnOrientationChanged)));

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var reference = d as ConnectorViewModel;
            if (reference != null)
            {
                reference.Orientation = (ConnectorOrientation) e.NewValue;
            }
        }

        public static readonly DependencyProperty ConnectorProperty =
           DependencyProperty.RegisterAttached("Connector", typeof(Connector), typeof(ConnectorViewModel),
           new FrameworkPropertyMetadata(null, new PropertyChangedCallback(ConnectorViewModel.OnConnectorChanged)));

        private static void OnConnectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var reference = d as ConnectorViewModel;
            if (reference != null)
            {
                var connector = e.NewValue as Connector;
                reference.Connector = connector;
            }
        }

        #endregion

        private List<ConnectionViewModel> connections;

        public List<ConnectionViewModel> Connections
        {
            get { return connections ?? (connections = new List<ConnectionViewModel>()); }
        }

        public Connector Connector { get; set; }

        private BlockViewModel parentContianer;

        public BlockViewModel ParentContainer
        {
            get { return parentContianer ?? (parentContianer = DataContext as BlockViewModel); }
        }

        internal ConnectorInfo GetInfo()
        {
            var info = new ConnectorInfo
            {
                DesignerItemLeft = Canvas.GetLeft(ParentContainer),
                DesignerItemTop = Canvas.GetTop(ParentContainer),
                DesignerItemSize = new Size(ParentContainer.ActualWidth, ParentContainer.ActualHeight),
                Type = Connector.Type,
                Position = Connector.Position
            };
            return info;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // provides compact info about a connector; used for the 
    // routing algorithm, instead of hand over a full fledged Connector
    internal struct ConnectorInfo
    {
        public double DesignerItemLeft { get; set; }
        public double DesignerItemTop { get; set; }
        public Size DesignerItemSize { get; set; }
        public Point Position { get; set; }
        public Direction Type { get; set; }
    }

    public enum ConnectorOrientation
    {
        None,
        Left,
        Top,
        Right,
        Bottom
    }
}