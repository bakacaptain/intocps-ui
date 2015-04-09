using System.Windows;
using System.Windows.Controls;
using DrawADiagram.Controls;

namespace DrawADiagram.Utils
{
    public class DesignerItemFactory
    {
        private readonly DesignerCanvas designer;

        public DesignerItemFactory(DesignerCanvas designer)
        {
            this.designer = designer;
        }

        public void Create(UIElement element, double x, double y)
        {
            DesignerCanvas.SetLeft(element, x);
            DesignerCanvas.SetTop(element, y);
            Canvas.SetZIndex(element, designer.Children.Count);
            designer.Children.Add(element);
        }
    }
}