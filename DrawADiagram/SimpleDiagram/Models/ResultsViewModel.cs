using System;
using ModelLibrary.Models;
using OxyPlot;
using OxyPlot.Series;

namespace SimpleDiagram.Models
{
    public class ResultsViewModel
    {
        public ResultsViewModel()
        {
            Results = new PlotModel{ Title = "Test Graph" };
            Results.Series.Add(new FunctionSeries(Math.Cos,0,10,0.1,"cos(x)"));
        }

        public PlotModel Results { get; set; }
    }

    public class ResultItemModel
    {
        public string ParentModelName { get; set; }
        public Connector Connector { get; set; }
        public string FirstAxisLabel { get; set; }
        public string SecondAxisLabel { get; set; }

        public string DisplayName
        {
            get { return string.Format("{0} - {1} - {2}",ParentModelName,Connector.Name,Connector.Type); }
        }
    }

    
}