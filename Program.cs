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


            Initialize.initializeSimulationSinglePhase();

            Console.ReadKey();
        }   
    }
}
