using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUBOS;
using System.Diagnostics;

namespace PetSim3
{
    class Program
    {
        static void Main(string[] args)
        {
            //initialize the simulator
            SimulatorData data;
            data = DataEntry.ReadData("SinglePhase_Incompressible_1.txt");
            Initialize.initializeSimulationSinglePhase(data);
            //Initialize.initializeSimulationSinglePhase();

            Console.ReadKey();
        }   
    }
}
