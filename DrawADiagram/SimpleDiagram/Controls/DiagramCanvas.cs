using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using SimpleDiagram.Controls.Adorners;
using SimpleDiagram.Models;

namespace SimpleDiagram.Controls
{
    public class DiagramCanvas : Canvas
    {
        public DiagramCanvas()
        {
            AllowDrop = true;
            OnModelCreate += _ => { };
            OnConnectionAdded += _ => { };
        }

        /// <summary>
        /// Start point for rubberband drag operation
        /// </summary>
        private Point? rubberbandSelectionStartPoint = null;

        private List<ISelectable> selectedElements;

        public List<ISelectable> SelectedElements
        {
            get { return selectedElements ?? (selectedElements = new List<ISelectable>()); }
            set { selectedElements = value; }
        }

        public Action<Point> OnModelCreate;
        public Action<ConnectionViewModel> OnConnectionAdded;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Source == this)
            {
                // in case that this click is the start for a 
                // drag operation we cache the start point
                rubberbandSelectionStartPoint = e.GetPosition(this);

                // if you click directly on the canvas all 
                // selected items are 'de-selected'
                foreach (ISelectable element in SelectedElements)
                {
                    element.IsSelected = false;
                }
                selectedElements.Clear();

                e.Handled = false;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // if mouse button is not pressed we have no drag operation, ...
            if (e.LeftButton != MouseButtonState.Pressed)
                this.rubberbandSelectionStartPoint = null;

            // ... but if mouse button is pressed and start
            // point value is set we do have one
            if (this.rubberbandSelectionStartPoint.HasValue)
            {
                // create rubberband adorner
                var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    var adorner = new RubberbandAdorner(this, rubberbandSelectionStartPoint);
                    if (adorner != null)
                    {
                        adornerLayer.Add(adorner);
                    }
                }
            }
            e.Handled = true;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            DragObject dragObject = e.Data.GetData(typeof(DragObject)) as DragObject;
            if (dragObject != null && !String.IsNullOrEmpty(dragObject.Xaml))
            {
                OnModelCreate(e.GetPosition(this));
                e.Handled = true;
            }
        }

        /// <summary>
        /// Dropping an item outside the canvas should make it larger as this does.
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Size size = new Size();
            foreach (UIElement element in base.Children)
            {
                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);
                left = double.IsNaN(left) ? 0 : left;
                top = double.IsNaN(top) ? 0 : top;

                //measure desired size for each child
                element.Measure(constraint);

                Size desiredSize = element.DesiredSize;
                if (!double.IsNaN(desiredSize.Width) && !double.IsNaN(desiredSize.Height))
                {
                    size.Width = Math.Max(size.Width, left + desiredSize.Width);
                    size.Height = Math.Max(size.Height, top + desiredSize.Height);
                }
            }
            // add margin 
            size.Width += 10;
            size.Height += 10;
            return size;
        }

        /// <summary>
        /// Adds an element to the Diagram
        /// </summary>
        /// <param name="element"></param>
        /// <param name="x">From left</param>
        /// <param name="y">From top</param>
        /// <param name="z"></param>
        public void Add(BlockViewModel element, int z)
        {
            UpdatePosition(element);
            SetZIndex(element, z);
            Children.Add(element);

            //update selection
            foreach (ISelectable item in this.SelectedElements)
                item.IsSelected = false;
            SelectedElements.Clear();
            element.IsSelected = true;
            this.SelectedElements.Add(element);

        }

        /// <summary>
        /// Adds the element on top of existing elements.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Add(BlockViewModel element)
        {
            Add(element, Children.Count);
        }

        public void Add(ConnectorViewModel source, ConnectorViewModel sink)
        {
            Add(new ConnectionViewModel(source.Connector, sink.Connector), 0);
        }

        public void Add(ConnectionViewModel element, int index)
        {
            Children.Insert(index, element);
            OnConnectionAdded(element);
        }

        public void Add(ConnectionViewModel element)
        {
            Add(element,0);            
        }

        public void Remove(BlockViewModel element)
        {
            Children.Remove(element);
        }

        public void Remove(ConnectionViewModel element)
        {
            Children.Remove(element);
        }

        /// <summary>
        /// Changes the position of the view
        /// </summary>
        /// <param name="element"></param>
        public static void UpdatePosition(BlockViewModel element)
        {
            SetLeft(element,element.BlockModel.Position.X);
            SetTop(element,element.BlockModel.Position.Y);
        }
    }
}