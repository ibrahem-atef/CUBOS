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

        public static double incompressible(GridBlock[] grid)
        {
            //set grid size
            int length = grid.Length;

            //variables to store material balance error
            double IMB = 0;
            //double CMB = 0;

            GridBlock block;

            for (int i = 0; i < length; i++)
            {
                block = grid[i];

                //boundary conditions
                IMB += block.boundary_flow_rate;
                IMB += block.boundary_pressure_gradient;
                IMB += (block.pressure - block.boundary_pressure) * block.boundary_transmissibility;
                //well conditions
                IMB += block.well_flow_rate;
            }

            return IMB;
        }
    }
}
