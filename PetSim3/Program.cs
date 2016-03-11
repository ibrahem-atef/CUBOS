using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUBOS;
using System.Diagnostics;

namespace PetSim
{
    class Program
    {
        static void Main(string[] args)
        {
            //initialize the simulator
            SimulatorData data;

            //Print Screen Logo
            consoleScreenWelcomeImage();

            Console.Write("Please, enter the data file code : ");

            int input = int.Parse(Console.ReadLine());
            data = DataEntry.ReadData("data/" + input);
            Console.WriteLine();

            if (data.successfully_loaded_data)
            {
                Initialize.initializeSimulationSinglePhase(data);
            }

            //this line prevents the console from closing automatically after finshing execution
            Console.ReadKey();
        }

        private static void consoleScreenWelcomeImage()
        {
            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine("++__________________________________________________________________________++");
            Console.WriteLine("++______##########___##       #___#########____##########___##########______++");
            Console.WriteLine("++______##########___##       #___##       #___##       #___##        ______++");
            Console.WriteLine("++______##        ___##       #___##       #___##       #___##        ______++");
            Console.WriteLine("++______##        ___##       #___##########___##       #___#########_______++");
            Console.WriteLine("++______##        ___##       #___##        #__##       #___        ##______++");
            Console.WriteLine("++______##        ___##       #___##        #__##       #___        ##______++");
            Console.WriteLine("++______##########___##########___###########__##       #___        ##______++");
            Console.WriteLine("++______##########___##########___###########__##########___##########______++");
            Console.WriteLine("++______Cairo________University___Black________Oil__________Simulator_______++");
            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");

            Console.WriteLine(); Console.WriteLine(); Console.WriteLine();
        }
    }
}
