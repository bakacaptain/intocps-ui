using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ModelLibrary.Models;
using SimpleDiagram.Models;

namespace SimpleDiagram.Controls.Adorners
{
    public class ConnectorAdorner : Adorner
    {
        private PathGeometry pathGeometry;
        private DiagramCanvas diagram;
        private ConnectorViewModel sourceConnector;
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

        public ConnectorAdorner(DiagramCanvas diagram, ConnectorViewModel sourceConnector)
            : base(diagram)
        {
            this.diagram = diagram;
            this.sourceConnector = sourceConnector;
            drawingPen = new Pen(Brushes.LightSlateGray, 1) {LineJoin = PenLineJoin.Round};
            Cursor = Cursors.Cross;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (HitConnector != null)
            {
                var sourceConnector = this.sourceConnector;
                var sinkConnector = HitConnector;
                diagram.Add(sourceConnector, sinkConnector);
            }
            if (HitBlock != null)
            {
                HitBlock.IsDragConnectionOver = false;
            }

            if (IsMouseCaptured) ReleaseMouseCapture();

            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(diagram);
            if (adornerLayer != null)
            {
                adornerLayer.Remove(this);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured) this.CaptureMouse();
                HitTesting(e.GetPosition(this));
                this.pathGeometry = GetPathGeometry(e.GetPosition(this));
                this.InvalidateVisual();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawGeometry(null, drawingPen, this.pathGeometry);

            // without a background the OnMouseMove event would not be fired
            // Alternative: implement a Canvas as a child of this adorner, like
            // the ConnectionAdorner does.
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        }

        private PathGeometry GetPathGeometry(Point position)
        {
            PathGeometry geometry = new PathGeometry();

            Direction targetDirection;
            if (HitConnector != null)
                targetDirection = HitConnector.Connector.Type;
            else
                targetDirection = Direction.UNKNOWN;

            List<Point> pathPoints = PathFinder.GetConnectionLine(sourceConnector.GetInfo(), position, targetDirection);

            if (pathPoints.Count > 0)
            {
                PathFigure figure = new PathFigure();
                figure.StartPoint = pathPoints[0];
                pathPoints.Remove(pathPoints[0]);
                figure.Segments.Add(new PolyLineSegment(pathPoints, true));
                geometry.Figures.Add(figure);
            }

            return geometry;
        }

        private void HitTesting(Point hitPoint)
        {
            bool hitConnectorFlag = false;

            DependencyObject hitObject = diagram.InputHitTest(hitPoint) as DependencyObject;
            while (hitObject != null &&
                   hitObject != sourceConnector.ParentContainer &&
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
