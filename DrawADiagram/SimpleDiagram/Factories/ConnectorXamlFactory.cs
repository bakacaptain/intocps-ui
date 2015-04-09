using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ModelLibrary.Models;
using SimpleDiagram.Controls.Actions;
using SimpleDiagram.Models;

namespace SimpleDiagram.Factories
{
    public class ConnectorXamlFactory
    {
        public static ControlTemplate CreateTemplate(string key, IEnumerable<Connector> connectors)
        {
            var enumerable = connectors as IList<Connector> ?? connectors.ToList();
            
            var outputs = (from con in enumerable
                where (con.Type == Direction.OUT || con.Type == Direction.INOUT) 
                select con).ToList();

            var inputs = (from con in enumerable
                where !outputs.Contains(con)
                select con).ToList();

            //Create the control template
            var controlTemplate = new ControlTemplate();
            controlTemplate.TargetType = typeof (Control);

            // set up wrapping stackpanel
            var layout = new FrameworkElementFactory(typeof (RelativePositionPanel));

            for (int i = 0; i < inputs.Count; i++)
            {
                double height = (1.0 / (inputs.Count+1)) * (i + 1);
                var inputConnection = new FrameworkElementFactory(typeof(ConnectorViewModel));
                inputConnection.SetValue(ConnectorViewModel.OrientationProperty, ConnectorOrientation.Left);
                inputConnection.SetValue(RelativePositionPanel.RelativePositionProperty, new Point(0,height));
                inputConnection.SetValue(ConnectorViewModel.ConnectorProperty, inputs[i]);
                layout.AppendChild(inputConnection);
            }
            for (var i = 0; i<outputs.Count; i++)
            {
                double height = (1.0/ (outputs.Count+1))*(i+1);
                var outputConnection = new FrameworkElementFactory(typeof(ConnectorViewModel));
                outputConnection.SetValue(ConnectorViewModel.OrientationProperty, ConnectorOrientation.Right);
                outputConnection.SetValue(RelativePositionPanel.RelativePositionProperty, new Point(1, height));
                outputConnection.SetValue(ConnectorViewModel.ConnectorProperty, outputs[i]);
                layout.AppendChild(outputConnection);
            }
            controlTemplate.VisualTree = layout;
            return controlTemplate;
        }
    }
}