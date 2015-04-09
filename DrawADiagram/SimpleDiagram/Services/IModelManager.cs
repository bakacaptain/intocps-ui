﻿using System;
using System.Collections;
using ModelLibrary.Models;
using SimpleDiagram.Models;

namespace SimpleDiagram.Services
{
    /// <summary>
    /// Responsible for the lifecycle of the view+viewmodels+model
    /// </summary>
    public interface IModelManager
    {
        /// <summary>
        /// Raised when a model need to be redrawn
        /// </summary>
        event EventHandler<Block> OnModelRedraw;

        /// <summary>
        /// Raised when a model needs to be added to the view
        /// </summary>
        event EventHandler<BlockViewModel> OnModelAdded;
        
        /// <summary>
        /// Raised when model should be removed from the view
        /// </summary>
        event EventHandler<BlockViewModel> OnModelRemoved;

        /// <summary>
        /// Raised when parameters for the need to be displayed
        /// </summary>
        event EventHandler<IEnumerable> OnViewModelParameters;

        void CreateModel(double x, double y);
        void CreateModel(Block block);
        void CreateModel(Block block, object content);
        void AddModel(BlockViewModel model);
        void RemoveModel(BlockViewModel model);
        void CopyModel(BlockViewModel model);
        void DisplayParameters(IEnumerable parameters);
        void RedrawModel(BlockViewModel model);
        void OpenModelInExternalTool(BlockViewModel model);
        void AddInputConnector(BlockViewModel model);
        void AddOutputConnector(BlockViewModel model);
    }
}