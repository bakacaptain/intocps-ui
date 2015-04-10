using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ModelLibrary.Models;
using SimpleDiagram.Models;

namespace SimpleDiagram.Controls
{
    // Note: I couldn't find a useful open source library that does
    // orthogonal routing so started to write something on my own.
    // Categorize this as a quick and dirty short term solution.
    // I will keep on searching.

    // Helper class to provide an orthogonal connection path
    internal class PathFinder
    {
        private const int margin = 20;

        internal static List<Point> GetConnectionLine(ConnectorInfo source, ConnectorInfo sink, bool showLastLine)
        {
            var linePoints = new List<Point>();

            Rect rectSource = GetRectWithMargin(source, margin);
            Rect rectSink = GetRectWithMargin(sink, margin);

            Point startPoint = GetOffsetPoint(source, rectSource);
            Point endPoint = GetOffsetPoint(sink, rectSink);

            linePoints.Add(startPoint);
            Point currentPoint = startPoint;

            if (!rectSink.Contains(currentPoint) && !rectSource.Contains(endPoint))
            {
                while (true)
                {
                    #region source node

                    if (IsPointVisible(currentPoint, endPoint, new Rect[] { rectSource, rectSink }))
                    {
                        linePoints.Add(endPoint);
                        currentPoint = endPoint;
                        break;
                    }

                    Point neighbour = GetNearestVisibleNeighborSink(currentPoint, endPoint, sink, rectSource, rectSink);
                    if (!double.IsNaN(neighbour.X))
                    {
                        linePoints.Add(neighbour);
                        linePoints.Add(endPoint);
                        currentPoint = endPoint;
                        break;
                    }

                    if (currentPoint == startPoint)
                    {
                        bool flag;
                        Point n = GetNearestNeighborSource(source, endPoint, rectSource, rectSink, out flag);
                        linePoints.Add(n);
                        currentPoint = n;

                        if (!IsRectVisible(currentPoint, rectSink, new Rect[] { rectSource }))
                        {
                            Point n1, n2;
                            GetOppositeCorners(source.Type, rectSource, out n1, out n2);
                            if (flag)
                            {
                                linePoints.Add(n1);
                                currentPoint = n1;
                            }
                            else
                            {
                                linePoints.Add(n2);
                                currentPoint = n2;
                            }
                            if (!IsRectVisible(currentPoint, rectSink, new Rect[] { rectSource }))
                            {
                                if (flag)
                                {
                                    linePoints.Add(n2);
                                    currentPoint = n2;
                                }
                                else
                                {
                                    linePoints.Add(n1);
                                    currentPoint = n1;
                                }
                            }
                        }
                    }
                    #endregion

                    #region sink node

                    else // from here on we jump to the sink node
                    {
                        Point n1, n2; // neighbour corner
                        Point s1, s2; // opposite corner
                        GetNeighborCorners(sink.Type, rectSink, out s1, out s2);
                        GetOppositeCorners(sink.Type, rectSink, out n1, out n2);

                        bool n1Visible = IsPointVisible(currentPoint, n1, new Rect[] { rectSource, rectSink });
                        bool n2Visible = IsPointVisible(currentPoint, n2, new Rect[] { rectSource, rectSink });

                        if (n1Visible && n2Visible)
                        {
                            if (rectSource.Contains(n1))
                            {
                                linePoints.Add(n2);
                                if (rectSource.Contains(s2))
                                {
                                    linePoints.Add(n1);
                                    linePoints.Add(s1);
                                }
                                else
                                    linePoints.Add(s2);

                                linePoints.Add(endPoint);
                                currentPoint = endPoint;
                                break;
                            }

                            if (rectSource.Contains(n2))
                            {
                                linePoints.Add(n1);
                                if (rectSource.Contains(s1))
                                {
                                    linePoints.Add(n2);
                                    linePoints.Add(s2);
                                }
                                else
                                    linePoints.Add(s1);

                                linePoints.Add(endPoint);
                                currentPoint = endPoint;
                                break;
                            }

                            if ((Distance(n1, endPoint) <= Distance(n2, endPoint)))
                            {
                                linePoints.Add(n1);
                                if (rectSource.Contains(s1))
                                {
                                    linePoints.Add(n2);
                                    linePoints.Add(s2);
                                }
                                else
                                    linePoints.Add(s1);
                                linePoints.Add(endPoint);
                                currentPoint = endPoint;
                                break;
                            }
                            else
                            {
                                linePoints.Add(n2);
                                if (rectSource.Contains(s2))
                                {
                                    linePoints.Add(n1);
                                    linePoints.Add(s1);
                                }
                                else
                                    linePoints.Add(s2);
                                linePoints.Add(endPoint);
                                currentPoint = endPoint;
                                break;
                            }
                        }
                        else if (n1Visible)
                        {
                            linePoints.Add(n1);
                            if (rectSource.Contains(s1))
                            {
                                linePoints.Add(n2);
                                linePoints.Add(s2);
                            }
                            else
                                linePoints.Add(s1);
                            linePoints.Add(endPoint);
                            currentPoint = endPoint;
                            break;
                        }
                        else
                        {
                            linePoints.Add(n2);
                            if (rectSource.Contains(s2))
                            {
                                linePoints.Add(n1);
                                linePoints.Add(s1);
                            }
                            else
                                linePoints.Add(s2);
                            linePoints.Add(endPoint);
                            currentPoint = endPoint;
                            break;
                        }
                    }
                    #endregion
                }
            }
            else
            {
                linePoints.Add(endPoint);
            }

            linePoints = OptimizeLinePoints(linePoints, new Rect[] { rectSource, rectSink }, source.Type, sink.Type);

            CheckPathEnd(source, sink, showLastLine, linePoints);
            return linePoints;
        }

        internal static List<Point> GetConnectionLine(ConnectorInfo source, Point sinkPoint, Direction preferedType)
        {
            List<Point> linePoints = new List<Point>();
            Rect rectSource = GetRectWithMargin(source, 10);
            Point startPoint = GetOffsetPoint(source, rectSource);
            Point endPoint = sinkPoint;

            linePoints.Add(startPoint);
            Point currentPoint = startPoint;

            if (!rectSource.Contains(endPoint))
            {
                while (true)
                {
                    if (IsPointVisible(currentPoint, endPoint, new Rect[] { rectSource }))
                    {
                        linePoints.Add(endPoint);
                        break;
                    }

                    bool sideFlag;
                    Point n = GetNearestNeighborSource(source, endPoint, rectSource, out sideFlag);
                    linePoints.Add(n);
                    currentPoint = n;

                    if (IsPointVisible(currentPoint, endPoint, new Rect[] { rectSource }))
                    {
                        linePoints.Add(endPoint);
                        break;
                    }
                    else
                    {
                        Point n1, n2;
                        GetOppositeCorners(source.Type, rectSource, out n1, out n2);
                        if (sideFlag)
                            linePoints.Add(n1);
                        else
                            linePoints.Add(n2);

                        linePoints.Add(endPoint);
                        break;
                    }
                }
            }
            else
            {
                linePoints.Add(endPoint);
            }

            if (preferedType != Direction.UNKNOWN)
                linePoints = OptimizeLinePoints(linePoints, new Rect[] { rectSource }, source.Type, preferedType);
            else
                linePoints = OptimizeLinePoints(linePoints, new Rect[] { rectSource }, source.Type, GetOpositeDirection(source.Type));

            return linePoints;
        }

        private static List<Point> OptimizeLinePoints(List<Point> linePoints, Rect[] rectangles, Direction sourceOrientation, Direction sinkOrientation)
        {
            List<Point> points = new List<Point>();
            int cut = 0;

            for (int i = 0; i < linePoints.Count; i++)
            {
                if (i >= cut)
                {
                    for (int k = linePoints.Count - 1; k > i; k--)
                    {
                        if (IsPointVisible(linePoints[i], linePoints[k], rectangles))
                        {
                            cut = k;
                            break;
                        }
                    }
                    points.Add(linePoints[i]);
                }
            }

            #region Line
            for (int j = 0; j < points.Count - 1; j++)
            {
                if (points[j].X != points[j + 1].X && points[j].Y != points[j + 1].Y)
                {
                    Direction directionFrom;
                    Direction directionTo;

                    // type from point
                    if (j == 0)
                        directionFrom = sourceOrientation;
                    else
                        directionFrom = GetOrientation(points[j], points[j - 1]);

                    // type to pint 
                    if (j == points.Count - 2)
                        directionTo = sinkOrientation;
                    else
                        directionTo = GetOrientation(points[j + 1], points[j + 2]);


                    if ((directionFrom == Direction.IN || directionFrom == Direction.OUT) &&
                        (directionTo == Direction.IN || directionTo == Direction.OUT))
                    {
                        double centerX = Math.Min(points[j].X, points[j + 1].X) + Math.Abs(points[j].X - points[j + 1].X) / 2;
                        points.Insert(j + 1, new Point(centerX, points[j].Y));
                        points.Insert(j + 2, new Point(centerX, points[j + 2].Y));
                        if (points.Count - 1 > j + 3)
                            points.RemoveAt(j + 3);
                        return points;
                    }

                    //// Should not be needed as there are two directions
                    //if ((directionFrom == ConnectorOrientation.Top || directionFrom == ConnectorOrientation.Bottom) &&
                    //    (directionTo == ConnectorOrientation.Top || directionTo == ConnectorOrientation.Bottom))
                    //{
                    //    double centerY = Math.Min(points[j].Y, points[j + 1].Y) + Math.Abs(points[j].Y - points[j + 1].Y) / 2;
                    //    points.Insert(j + 1, new Point(points[j].X, centerY));
                    //    points.Insert(j + 2, new Point(points[j + 2].X, centerY));
                    //    if (points.Count - 1 > j + 3)
                    //        points.RemoveAt(j + 3);
                    //    return points;
                    //}

                    //if ((directionFrom == ConnectorOrientation.Left || directionFrom == ConnectorOrientation.Right) &&
                    //    (directionTo == ConnectorOrientation.Top || directionTo == ConnectorOrientation.Bottom))
                    //{
                    //    points.Insert(j + 1, new Point(points[j + 1].X, points[j].Y));
                    //    return points;
                    //}

                    //if ((directionFrom == ConnectorOrientation.Top || directionFrom == ConnectorOrientation.Bottom) &&
                    //    (directionTo == ConnectorOrientation.Left || directionTo == ConnectorOrientation.Right))
                    //{
                    //    points.Insert(j + 1, new Point(points[j].X, points[j + 1].Y));
                    //    return points;
                    //}
                }
            }
            #endregion

            return points;
        }

        private static Direction GetOrientation(Point p1, Point p2)
        {
            //// Original ConnectorOrientation
            //if (p1.X == p2.X)
            //{
            //    if (p1.Y >= p2.Y)
            //        return ConnectorOrientation.Bottom;
            //    else
            //        return ConnectorOrientation.Top;
            //}
            //else if (p1.Y == p2.Y)
            //{
            //    if (p1.X >= p2.X)
            //        return ConnectorOrientation.Right;
            //    else
            //        return ConnectorOrientation.Left;
            //}

            return p1.X <= p2.X ? Direction.OUT : Direction.IN;
            throw new Exception("Failed to retrieve type");
        }

        private static Orientation GetOrientation(Direction type)
        {
            switch (type)
            {
                case Direction.IN:
                    return Orientation.Horizontal;
                case Direction.OUT:
                    return Orientation.Horizontal;
                case Direction.INOUT:
                    //TODO: decide if it is needed
                default:
                    throw new Exception(string.Format("Unknown Direction: {0}",type));
            }
        }

        private static Point GetNearestNeighborSource(ConnectorInfo source, Point endPoint, Rect rectSource, Rect rectSink, out bool flag)
        {
            Point n1, n2; // neighbors
            GetNeighborCorners(source.Type, rectSource, out n1, out n2);

            if (rectSink.Contains(n1))
            {
                flag = false;
                return n2;
            }

            if (rectSink.Contains(n2))
            {
                flag = true;
                return n1;
            }

            if ((Distance(n1, endPoint) <= Distance(n2, endPoint)))
            {
                flag = true;
                return n1;
            }
            else
            {
                flag = false;
                return n2;
            }
        }

        private static Point GetNearestNeighborSource(ConnectorInfo source, Point endPoint, Rect rectSource, out bool flag)
        {
            Point n1, n2; // neighbors
            GetNeighborCorners(source.Type, rectSource, out n1, out n2);

            if ((Distance(n1, endPoint) <= Distance(n2, endPoint)))
            {
                flag = true;
                return n1;
            }
            else
            {
                flag = false;
                return n2;
            }
        }

        private static Point GetNearestVisibleNeighborSink(Point currentPoint, Point endPoint, ConnectorInfo sink, Rect rectSource, Rect rectSink)
        {
            Point s1, s2; // neighbors on sink side
            GetNeighborCorners(sink.Type, rectSink, out s1, out s2);

            bool flag1 = IsPointVisible(currentPoint, s1, new Rect[] { rectSource, rectSink });
            bool flag2 = IsPointVisible(currentPoint, s2, new Rect[] { rectSource, rectSink });

            if (flag1) // s1 visible
            {
                if (flag2) // s1 and s2 visible
                {
                    if (rectSink.Contains(s1))
                        return s2;

                    if (rectSink.Contains(s2))
                        return s1;

                    if ((Distance(s1, endPoint) <= Distance(s2, endPoint)))
                        return s1;
                    else
                        return s2;

                }
                else
                {
                    return s1;
                }
            }
            else // s1 not visible
            {
                if (flag2) // only s2 visible
                {
                    return s2;
                }
                else // s1 and s2 not visible
                {
                    return new Point(double.NaN, double.NaN);
                }
            }
        }

        private static bool IsPointVisible(Point fromPoint, Point targetPoint, Rect[] rectangles)
        {
            foreach (Rect rect in rectangles)
            {
                if (RectangleIntersectsLine(rect, fromPoint, targetPoint))
                    return false;
            }
            return true;
        }

        private static bool IsRectVisible(Point fromPoint, Rect targetRect, Rect[] rectangles)
        {
            if (IsPointVisible(fromPoint, targetRect.TopLeft, rectangles))
                return true;

            if (IsPointVisible(fromPoint, targetRect.TopRight, rectangles))
                return true;

            if (IsPointVisible(fromPoint, targetRect.BottomLeft, rectangles))
                return true;

            if (IsPointVisible(fromPoint, targetRect.BottomRight, rectangles))
                return true;

            return false;
        }

        private static bool RectangleIntersectsLine(Rect rect, Point startPoint, Point endPoint)
        {
            rect.Inflate(-1, -1);
            return rect.IntersectsWith(new Rect(startPoint, endPoint));
        }

        private static void GetOppositeCorners(Direction type, Rect rect, out Point n1, out Point n2)
        {
            switch (type)
            {
                case Direction.IN:
                    n1 = rect.TopRight; n2 = rect.BottomRight;
                    break;
                case Direction.OUT:
                    n1 = rect.TopLeft; n2 = rect.BottomLeft;
                    break;
                case Direction.INOUT:
                    //TODO: decide if it is needed
                default:
                    throw new Exception("No opposite corners found!");
            }
        }

        private static void GetNeighborCorners(Direction type, Rect rect, out Point n1, out Point n2)
        {
            switch (type)
            {
                case Direction.IN:
                    n1 = rect.TopLeft; n2 = rect.BottomLeft;
                    break;
                case Direction.OUT:
                    n1 = rect.TopRight; n2 = rect.BottomRight;
                    break;
                case Direction.INOUT:
                    //TODO: decide if needed later
                default:
                    throw new Exception("No neighour corners found!");
            }
        }

        private static double Distance(Point p1, Point p2)
        {
            return Point.Subtract(p1, p2).Length;
        }

        private static Rect GetRectWithMargin(ConnectorInfo connectorThumb, double margin)
        {
            Rect rect = new Rect(connectorThumb.DesignerItemLeft,
                                 connectorThumb.DesignerItemTop,
                                 connectorThumb.DesignerItemSize.Width,
                                 connectorThumb.DesignerItemSize.Height);

            rect.Inflate(margin, margin);

            return rect;
        }

        private static Point GetOffsetPoint(ConnectorInfo connector, Rect rect)
        {
            Point offsetPoint = new Point();

            switch (connector.Type)
            {
                case Direction.IN:
                    offsetPoint = new Point(rect.Left, connector.Position.Y);
                    break;
                case Direction.OUT:
                    offsetPoint = new Point(rect.Right, connector.Position.Y);
                    break;
                default:
                    break;
            }

            return offsetPoint;
        }

        private static void CheckPathEnd(ConnectorInfo source, ConnectorInfo sink, bool showLastLine, List<Point> linePoints)
        {
            if (showLastLine)
            {
                Point startPoint = new Point(0, 0);
                Point endPoint = new Point(0, 0);
                double marginPath = 15;
                switch (source.Type)
                {
                    case Direction.IN:
                        startPoint = new Point(source.Position.X - marginPath, source.Position.Y);
                        break;
                    case Direction.OUT:
                        startPoint = new Point(source.Position.X + marginPath, source.Position.Y);
                        break;
                    default:
                        break;
                }

                switch (sink.Type)
                {
                    case Direction.IN:
                        endPoint = new Point(sink.Position.X - marginPath, sink.Position.Y);
                        break;
                    case Direction.OUT:
                        endPoint = new Point(sink.Position.X + marginPath, sink.Position.Y);
                        break;
                    default:
                        break;
                }
                linePoints.Insert(0, startPoint);
                linePoints.Add(endPoint);
            }
            else
            {
                linePoints.Insert(0, source.Position);
                linePoints.Add(sink.Position);
            }
        }

        private static Direction GetOpositeDirection(Direction type)
        {
            switch (type)
            {
                case Direction.IN:
                    return Direction.OUT;
                case Direction.OUT:
                    return Direction.IN;
                default:
                    return Direction.UNKNOWN;
            }
        }
    }
}
