using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DrawADiagram.Controls;
using DrawADiagram.Utils;

namespace DrawADiagram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DesignerItemFactory factory = new DesignerItemFactory(designer);

            DesignerItem newItem = new DesignerItem();

            Rectangle rect = new Rectangle
            {
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                IsHitTestVisible = false
            };

            Grid grid = new Grid();
            grid.Children.Add(rect);
            grid.Children.Add(new TextBlock()
            {
                Text = "<<Block>>",
                IsHitTestVisible = false
            });

            newItem.Content = grid;
            newItem.Width = 80;
            newItem.Height = 160;

            factory.Create(newItem, 20, 60);

            //var newnewItem = new DesignerItem();

            //newnewItem.Content = grid;
            //newnewItem.Width = 100;
            //newnewItem.Height = 160;

            //factory.Create(newnewItem, 200,200);

        }
    }
}
