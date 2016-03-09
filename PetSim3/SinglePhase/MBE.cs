using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUBOS;

namespace SinglePhase
{
    class MBE
    {
        //Notes:
        //IMB "Incremental Material Balance": a check performed every timestep
        //CMB "Cumulative Material Balance": a check performed at the end of the simulation

        //a constant
        const double a = 5.614583;

        //Method Name: production
        //Objectives: calculates the summation of the production terms of all the grid blocks
        //Objectives: this is equal to the MBE for incompressible fluid flow, or the production term only of the IMB and CMB
        //Inputs: an array of elements of type "GridBlock"
        //Outputs: a value of type "double"
        public static double production(GridBlock[] grid)
        {
            //set grid size
            int length = grid.Length;

            //variables to store material balance error
            double temp = 0;
            //double CMB = 0;

            GridBlock block;

            for (int i = 0; i < length; i++)
            {
                block = grid[i];

                //boundary conditions
                temp += block.boundary_flow_rate;
                temp += block.boundary_pressure_gradient;
                temp += (block.pressure - block.boundary_pressure) * block.boundary_transmissibility;
                //well conditions
                temp += block.well_flow_rate;
            }

            return temp;
        }

        //Method Name: accumulation
        //Objectives: calculates the summation of the accumulation terms of all the grid blocks at a definite time level "n or n+1" "not the difference"
        //Objectives: this is equal to the accumulation term only of the IMB and CMB
        //Inputs: an array of elements of type "GridBlock"
        //Outputs: a value of type "double"
        public static double accumulation(GridBlock[] grid)
        {
            //set grid size
            int length = grid.Length;

            //variables to store material balance error
            double temp = 0;
            //double CMB = 0;

            GridBlock block;

            for (int i = 0; i < length; i++)
            {
                block = grid[i];

                //boundary conditions
                temp += block.bulk_volume / a * block.porosity / block.Bw;
            }

            return temp;
        }

        //a method for storing and retrieving the value for IMB
        public static double IMB { get; set; }
    }
}
