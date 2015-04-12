using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ModelLibrary.Models;
using SimpleDiagram.Models;

namespace SimpleDiagram.Controls.Adorners
{
    public class ConnectionAdorner : Adorner
    {
        private DiagramCanvas diagram;
        private Canvas canvas;
        private ConnectionViewModel connection;
        private PathGeometry pathGeometry;
        private ConnectorViewModel fixConnector, dragConnector;
        private Thumb sourceDragThumb, sinkDragThumb;
        private Pen drawingPen;

        private BlockViewModel hitBlock;
        private BlockViewModel HitBlock
        {
            get { return hitBlock; }
            set
            {
                if (hitBlock != value)
                {
                    if (hitBlock != null)
                        hitBlock.IsDragConnectionOver = false;

                    hitBlock = value;

                    if (hitBlock != null)
                        hitBlock.IsDragConnectionOver = true;
                }
            }
        }

        private ConnectorViewModel hitConnector;
        private ConnectorViewModel HitConnector
        {
            get { return hitConnector; }
            set
            {
                if (hitConnector != value)
                {
                    hitConnector = value;
                }
            }
        }

        private VisualCollection visualChildren;
        protected override int VisualChildrenCount
        {
            get
            {
                return this.visualChildren.Count;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.visualChildren[index];
        }

        public ConnectionAdorner(DiagramCanvas diagram, ConnectionViewModel connection)
            : base(diagram)
        {
            this.diagram = diagram;
            canvas = new Canvas();
            this.visualChildren = new VisualCollection(this) {canvas};

            this.connection = connection;
            this.connection.PropertyChanged += new PropertyChangedEventHandler(AnchorPositionChanged);

            InitializeDragThumbs();

            drawingPen = new Pen(Brushes.LightSlateGray, 1) {LineJoin = PenLineJoin.Round};
        }

        private void InitializeDragThumbs()
        {
            Style dragThumbStyle = connection.FindResource("ConnectionAdornerThumbStyle") as Style;

            //source drag thumb
            sourceDragThumb = new Thumb();
            Canvas.SetLeft(sourceDragThumb, connection.AnchorPositionSource.X);
            Canvas.SetTop(sourceDragThumb, connection.AnchorPositionSource.Y);
            this.canvas.Children.Add(sourceDragThumb);
            if (dragThumbStyle != null)
                sourceDragThumb.Style = dragThumbStyle;

            sourceDragThumb.DragDelta += new DragDeltaEventHandler(thumbDragThumb_DragDelta);
            sourceDragThumb.DragStarted += new DragStartedEventHandler(thumbDragThumb_DragStarted);
            sourceDragThumb.DragCompleted += new DragCompletedEventHandler(thumbDragThumb_DragCompleted);

            // sink drag thumb
            sinkDragThumb = new Thumb();
            Canvas.SetLeft(sinkDragThumb, connection.AnchorPositionSink.X);
            Canvas.SetTop(sinkDragThumb, connection.AnchorPositionSink.Y);
            this.canvas.Children.Add(sinkDragThumb);
            if (dragThumbStyle != null)
                sinkDragThumb.Style = dragThumbStyle;

            sinkDragThumb.DragDelta += new DragDeltaEventHandler(thumbDragThumb_DragDelta);
            sinkDragThumb.DragStarted += new DragStartedEventHandler(thumbDragThumb_DragStarted);
            sinkDragThumb.DragCompleted += new DragCompletedEventHandler(thumbDragThumb_DragCompleted);
        }
        void AnchorPositionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("AnchorPositionSource"))
            {
                Canvas.SetLeft(sourceDragThumb, connection.AnchorPositionSource.X);
                Canvas.SetTop(sourceDragThumb, connection.AnchorPositionSource.Y);
            }

            if (e.PropertyName.Equals("AnchorPositionSink"))
            {
                Canvas.SetLeft(sinkDragThumb, connection.AnchorPositionSink.X);
                Canvas.SetTop(sinkDragThumb, connection.AnchorPositionSink.Y);
            }
        }

        void thumbDragThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (HitConnector != null)
            {
                if (connection != null)
                {
                    if (connection.Source == fixConnector)
                        connection.Sink = this.HitConnector;
                    else
                        connection.Source = this.HitConnector;
                }
            }

            this.HitBlock = null;
            this.HitConnector = null;
            this.pathGeometry = null;
            this.connection.StrokeDashArray = null;
            this.InvalidateVisual();
        }

        void thumbDragThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            this.HitBlock = null;
            this.HitConnector = null;
            this.pathGeometry = null;
            this.Cursor = Cursors.Cross;
            this.connection.StrokeDashArray = new DoubleCollection(new double[] { 1, 2 });

            if (sender == sourceDragThumb)
            {
                fixConnector = connection.Sink;
                dragConnector = connection.Source;
            }
            else if (sender == sinkDragThumb)
            {
                dragConnector = connection.Sink;
                fixConnector = connection.Source;
            }
        }

        void thumbDragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Point currentPosition = Mouse.GetPosition(this);
            this.HitTesting(currentPosition);
            this.pathGeometry = UpdatePathGeometry(currentPosition);
            this.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawGeometry(null, drawingPen, this.pathGeometry);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            canvas.Arrange(new Rect(0, 0, this.diagram.ActualWidth, this.diagram.ActualHeight));
            return finalSize;
        }

        private PathGeometry UpdatePathGeometry(Point position)
        {
            PathGeometry geometry = new PathGeometry();

            Direction targetDirection;
            if (HitConnector != null)
                targetDirection = HitConnector.Connector.Type;
            else
                targetDirection = dragConnector.Connector.Type;

            List<Point> linePoints = PathFinder.GetConnectionLine(fixConnector.GetInfo(), position, targetDirection);

            if (linePoints.Count > 0)
            {
                PathFigure figure = new PathFigure();
                figure.StartPoint = linePoints[0];
                linePoints.Remove(linePoints[0]);
                figure.Segments.Add(new PolyLineSegment(linePoints, true));
                geometry.Figures.Add(figure);
            }

            return geometry;
        }

        private void HitTesting(Point hitPoint)
        {
            bool hitConnectorFlag = false;

            DependencyObject hitObject = diagram.InputHitTest(hitPoint) as DependencyObject;
            while (hitObject != null &&
                   hitObject != fixConnector.ParentContainer &&
                   hitObject.GetType() != typeof(DiagramCanvas))
            {
                if (hitObject is ConnectorViewModel)
                {
                    HitConnector = hitObject as ConnectorViewModel;
                    hitConnectorFlag = true;
                }

                if (hitObject is BlockViewModel)
                {
                    HitBlock = hitObject as BlockViewModel;
                    if (!hitConnectorFlag)
                        HitConnector = null;
                    return;
                }
                hitObject = VisualTreeHelper.GetParent(hitObject);
            }

            HitConnector = null;
            HitBlock = null;
        }
    }
}
