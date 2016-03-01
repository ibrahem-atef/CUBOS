using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUBOS
{
    class SimulatorData
    {
        #region Initialize Rectangular grid dimensions
        public bool homogeneous = false;
        public int x, y, z;

        public int[] inactive_blocks;

        #endregion

        #region Initialize grid
        //The gridding technique used here is for only a single row or a single column
        public int delta_X, delta_Y, delta_Z;
        public int[] delta_X_array, delta_Y_array, delta_Z_array;

        #endregion

        #region Initialize Rock properties
        //The gridding technique used here is for the whole grid
        public int Kx_data, Ky_data, Kz_data;
        public double porosity, compressibility_rock;

        public int[] Kx_data_array, Ky_data_array, Kz_data_array;
        public double[] porosity_array, compressibility_rock_array;

        #endregion

        #region Initialize PVT

        public double FVF, viscosity;

        //For slightly-compressibly fluid
        public double compressibility_fluid;

        //For compressible fluid, this data is used for constructing the PVT table
        public double[][] g_data = new double[4][];

        #endregion

        #region  Initialize boundary conditions

        public int initial_pressure;

        public double[] boundary_flow_rate;
        public double[] boundary_pressure_x;
        public double[] boundary_pressure_y;
        public double[] boundary_pressure_gradient_x;
        public double[] boundary_pressure_gradient_y;

        #endregion

        #region  Initialize wells data
        public int[] well_locations;

        #endregion

        #region run specifications
        TypeDefinitions.Compressibility compressibility;
        Transmissibility.Phase phase;
        Transmissibility.GridType grid_type;

        //For compressible and slightly-compressible fluids, define the values for time steps and total simulation time
        public double delta_t = 5;
        public double time_max = 180;

        //For compressible fluid problems
        public double convergence_pressure = 0.001;
        #endregion
    }
}
