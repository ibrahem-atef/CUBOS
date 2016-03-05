using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUBOS
{
    class SimulatorData
    {
        //This variable is used when checking if the input file was read successfully
        public bool successfully_loaded_data = false;

        #region run specifications
        public enum NaturalOrdering { All_Blocks, Active_Only }
        //default value is active_only blocks
        public NaturalOrdering natural_ordering = NaturalOrdering.Active_Only;

        //defalut value is incompressible fluid
        public TypeDefinitions.Compressibility compressibility = TypeDefinitions.Compressibility.Incompressible;

        Transmissibility.Phase phase;

        //default value is rectangular
        public Transmissibility.GridType grid_type = Transmissibility.GridType.Rectangular;

        //For compressible and slightly-compressible fluids, define the values for time steps and total simulation time
        public double delta_t = 5;
        public double time_max = 180;

        //For compressible fluid problems
        public double convergence_pressure = 0.001;
        #endregion


        #region Output
        public string[] what = new string[] { "pressure", "well_rate", "well_block_pressure", "BHP", "MBE"};
        public SinglePhase.OutPut2D.Where where = SinglePhase.OutPut2D.Where.Console;
        public bool formatted = true;
        public bool single_file = true;
        public string file_name;
        #endregion

        #region Initialize Rectangular grid dimensions
        public bool homogeneous = true;
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
        public double Kx_data, Ky_data, Kz_data;
        public double porosity;
        public double compressibility_rock = 0;

        public double[] Kx_data_array, Ky_data_array, Kz_data_array;
        public double[] porosity_array;

        #endregion

        #region Initialize PVT

        public double FVF, viscosity;

        //For slightly-compressibly fluid
        public double compressibility_fluid;

        //For compressible fluid, this data is used for constructing the PVT table
        public double[][] g_data = new double[4][];

        #endregion

        #region  Initialize boundary conditions

        public double initial_pressure;

        public double[] boundary_flow_rate;
        public double[] boundary_pressure_x;
        public double[] boundary_pressure_y;
        public double[] boundary_pressure_gradient_x;
        public double[] boundary_pressure_gradient_y;

        #endregion

        #region  Initialize wells data
        public int[] well_locations;
        public Well[] well_array;
        #endregion
    }
}
