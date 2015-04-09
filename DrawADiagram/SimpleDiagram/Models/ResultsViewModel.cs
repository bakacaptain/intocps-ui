using System;
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

        public PlotModel Results { get; private set; }
    }

    
}