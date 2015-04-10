using System;
using System.Collections;

namespace SimpleDiagram.Services
{
    public interface ISimulationManager
    {
        event EventHandler OnConfigureRequest;

        /// <summary>
        /// Maps the connections of the structural model with the interfaces of the simulation model
        /// </summary>
        void Configure();

        /// <summary>
        /// 
        /// </summary>
        void ConfigureConnections();

        /// <summary>
        /// Model consistency check
        /// </summary>
        /// <returns></returns>
        bool CanRun();

        void Cosimulate();

        void DesignSpaceExplore();
    }
}