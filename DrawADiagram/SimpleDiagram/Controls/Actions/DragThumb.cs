using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using SimpleDiagram.Models;

namespace SimpleDiagram.Controls.Actions
{
    public class DragThumb : Thumb
    {
        public DragThumb()
        {
            DragDelta += OnDragDelta;
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var block = DataContext as BlockViewModel;
            var diagram = VisualTreeHelper.GetParent(block) as DiagramCanvas;

            if (block != null && diagram != null && block.IsSelected)
            {
                double minLeft = double.MaxValue;
                double minTop = double.MaxValue;

                // only move blocks
                var blocks = from element in diagram.SelectedElements
                            where element is BlockViewModel
                            select element;

                // Sets left and top position
                foreach (BlockViewModel element in blocks)
                {
                    var left = Canvas.GetLeft(element);
                    var top = Canvas.GetTop(element);

                    minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                    minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);
                }

                // Calculate delta movement
                var deltaHorizontal = Math.Max(-minLeft, e.HorizontalChange);
                var deltaVertical = Math.Max(-minTop, e.VerticalChange);

                // Set moved postion
                foreach (BlockViewModel element in blocks)
                {
                    var left = Canvas.GetLeft(element);
                    var top = Canvas.GetTop(element);

                    if (double.IsNaN(left)) left = 0;
                    if (double.IsNaN(top)) top = 0;

                    element.BlockModel.Position = new Point(left + deltaHorizontal, top + deltaVertical);
                    DiagramCanvas.UpdatePosition(element);
                }

                diagram.InvalidateMeasure();
                e.Handled = true;
            }
        }
    }
}