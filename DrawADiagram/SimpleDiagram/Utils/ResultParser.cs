﻿using System;
using System.Collections.Generic;
using System.Linq;
using OxyPlot;

namespace SimpleDiagram.Utils
{
    public interface IResultParser
    {
        /// <summary>
        /// Parses a text file to a collection of DataPoints.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="rowSplitter">A method to split the file into point</param>
        /// <param name="colSplitter">A method to split the points into datapoints</param>
        /// <returns></returns>
        IEnumerable<DataPoint> Parse(string filepath, Func<string,IEnumerable<string>> rowSplitter, Func<string, DataPoint> colSplitter);
    }

    /// <summary>
    /// This is a very simplified version that only reads the entire file to the end and parses it
    /// </summary>
    public class ResultParser : IResultParser
    {
        private readonly IReader reader;

        public ResultParser(IReader reader)
        {
            this.reader = reader;
        }

        public IEnumerable<DataPoint> Parse(string filepath, Func<string, IEnumerable<string>> rowSplitter, Func<string, DataPoint> colSplitter)
        {
            var file = reader.Read(filepath);

            var rows = rowSplitter(file);
            return rows.Select(colSplitter).ToList();
        }
    }
}