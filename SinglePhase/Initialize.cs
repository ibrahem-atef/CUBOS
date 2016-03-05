using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUBOS
{
    class Initialize
    {
        //Method Name: initializeSimulationSinglePhase
        //Objectives: reads the data from the data object passed to it and assign the values to the blocks after constructing the grid and starting the solver
        //Inputs: a variable of type "SimulatorData" that contains all the simulation data read from the data input file
        //Outpits: N/A
        public static void initializeSimulationSinglePhase(SimulatorData data)
        {
            #region Initialize Rectangular grid dimensions
            bool homogeneous = data.homogeneous;
            int x = data.x, y = data.y, z = data.z;

            int[] inactive_blocks = data.inactive_blocks;

            //creates an array for the grid x, y and z dimensions
            int[] grid_dimensions = new int[] { x, y, z };

            //calculates the size of the entire grid "including both active and non-active blocks"
            int size = x * y * z;
            //calculates the size of the active-blocks only
            int size_active = x * y * z - inactive_blocks.Length;

            //use this if the natural ordering excludes inactive blocks
            if (data.natural_ordering == SimulatorData.NaturalOrdering.Active_Only)
            {
                size = size_active;
            }

            GridBlock[] grid;
            #endregion

            #region Initialize grid data
            //The gridding technique used here is for only a single row or a single column
            int[] delta_X = new int[x];
            int[] delta_Y = new int[y];
            int[] delta_Z = new int[z];

            //Homogeneous grid sizing
            if (homogeneous)
            {
                for (int i = 0; i < x; i++) { delta_X[i] = data.delta_X; }
                for (int i = 0; i < y; i++) { delta_Y[i] = data.delta_Y; }
                for (int i = 0; i < z; i++) { delta_Z[i] = data.delta_Z; }
            }
            //Heterogeneous grid sizing
            else
            {
                delta_X = data.delta_X_array;
                delta_Y = data.delta_Y_array;
                delta_Z = data.delta_Z_array;
            }
            #endregion

            #region Initialize Rock properties
            //The gridding technique used here is for the whole grid

            double[] Kx_data = new double[size];
            double[] Ky_data = new double[size];
            double[] Kz_data = new double[size];
            double[] porosity = new double[size];
            double compressibility_rock = 0;

            //Homogeneous grid
            if (homogeneous)
            {
                for (int i = 0; i < size; i++)
                {
                    Kx_data[i] = data.Kx_data;
                    Ky_data[i] = data.Ky_data;
                    Kz_data[i] = data.Kz_data;
                    porosity[i] = data.porosity;
                    compressibility_rock = data.compressibility_rock;
                }
            }
            //Heterogeneous grid
            else
            {
                Kx_data = data.Kx_data_array;
                Ky_data = data.Ky_data_array;
                Kz_data = data.Kz_data_array;
                porosity = data.porosity_array;
                compressibility_rock = data.compressibility_rock;
            }
            #endregion

            #region Initialize PVT

            double FVF = data.FVF;
            double viscosity = data.viscosity;

            //For slightly-compressibly fluid
            double compressibility_fluid = data.compressibility_fluid;

            //For compressible fluid, this data is used for constructing the PVT table
            double[][] g_data = data.g_data;

            PVT pvt = new PVT(g_data: g_data);
            #endregion

            #region  Initialize boundary conditions

            double initial_pressure = data.initial_pressure;

            double[] boundary_flow_rate = data.boundary_flow_rate;
            double[] boundary_pressure_x = data.boundary_pressure_x;
            double[] boundary_pressure_y = data.boundary_pressure_y;
            double[] boundary_pressure_gradient_x = data.boundary_pressure_gradient_x;
            double[] boundary_pressure_gradient_y = data.boundary_pressure_gradient_y;

            #endregion

            #region  Initialize wells data
            Well[] wells = data.well_array;
            Well well;

            #endregion

            #region run specifications
            TypeDefinitions.Compressibility compressibility = data.compressibility;
            var phase = Transmissibility.Phase.Water;
            var grid_type = data.grid_type;

            //For compressible and slightly-compressible fluids, define the values for time steps and total simulation time
            double delta_t = data.delta_t;
            double time_max = data.time_max;

            //For compressible fluid problems
            double convergence_pressure = data.convergence_pressure;
            #endregion

            #region Initialize grid
            //#########################################################################################
            //Assign neighbouring blocks "according to the natural ordering procedure"
            //set numbering scheme
            var numbering_scheme = RectangularBlockNumbering.NumberingScheme.Active_Only;
            //contruct the grid
            grid = RectangularBlockNumbering.assignGridOrdering(grid_dimensions, inactive_blocks, numbering_scheme);
            
            GridBlock block;
            //assign properties to each block in the grid
            for (int counter = 0; counter < grid.Length; counter++)
            {
                block = grid[counter];

                //#########################################################################################
                //Assign PVT and rock properties

                block.delta_x = delta_X[block.x]; block.delta_y = delta_Y[block.y]; block.h = delta_Z[block.z];
                block.bulk_volume = block.delta_x * block.delta_y * block.h;

                block.Kx = Kx_data[counter]; block.Ky = Ky_data[counter]; block.Kz = Kz_data[counter];

                //To-Do: initialize pressure through hydraulic equilibrium
                block.pressure = initial_pressure;

                if (compressibility == TypeDefinitions.Compressibility.Compressible)
                {
                    block.water_viscosity = pvt.getWaterViscosity(initial_pressure);
                    block.So = 0; block.Sg = 0; block.Sw = 1;
                    block.Kro = 0; block.Krg = 0; block.Krw = 1;
                    block.Cf = compressibility_rock; block.C = compressibility_fluid;

                    block.Bw = pvt.getWaterFVF(initial_pressure);
                    block.porosity = Vp_Calculator.chord_slope_Vp(compressibility_rock, porosity[counter], block.pressure, initial_pressure);
                }
                else if (compressibility == TypeDefinitions.Compressibility.Slightly_Compressible)
                {
                    //For slightly compressibly fluids, viscosity is independent of pressure
                    block.water_viscosity = viscosity;
                    //For a single phase fluid, saturation and relative permeabilities of the fluid is equal to unity
                    block.So = 0; block.Sg = 0; block.Sw = 1;
                    block.Kro = 0; block.Krg = 0; block.Krw = 1;
                    //For slightly compressible fluids, the values of rock compressibility "Cf" and fluid compressibility C_fluid are used to estimate the new values
                    block.Cf = compressibility_rock; block.C = compressibility_fluid;

                    block.Bw = PVT.chord_slope_FVF(compressibility_fluid, FVF, block.pressure, initial_pressure);
                    block.porosity = Vp_Calculator.chord_slope_Vp(compressibility_rock, porosity[counter], block.pressure, initial_pressure);

                }
                else
                {
                    block.water_viscosity = viscosity; block.Bw = FVF;
                    block.porosity = porosity[counter];
                    block.So = 0; block.Sg = 0; block.Sw = 1;
                    block.Kro = 0; block.Krg = 0; block.Krw = 1;
                }

                //#########################################################################################
                //Calculate geometric factors "the part of the transmissibility that is always constant" only once in the initialization process
                var direction_x = Transmissibility.Direction.x;
                var direction_y = Transmissibility.Direction.y;

                if (homogeneous)
                {
                    block.GF_x = Transmissibility.getGeometricFactor(block, grid_type, direction_x);
                    block.GF_y = Transmissibility.getGeometricFactor(block, grid_type, direction_y);
                }

                //#########################################################################################
                //Set boundary conditions

                double boundary_transmissibility = 0;

                block.boundary_flow_rate = boundary_flow_rate[counter];
                if (boundary_pressure_x[counter] != 0)
                {
                    boundary_transmissibility = Transmissibility.getBoundaryTransmissibility(block, grid_type, direction_x, phase);
                    block.boundary_pressure = boundary_pressure_x[counter];
                    block.boundary_pressure_times_transmissibility = boundary_transmissibility * boundary_pressure_x[counter];
                    block.boundary_transmissibility = boundary_transmissibility;
                }
                else if (boundary_pressure_y[counter] != 0)
                {
                    boundary_transmissibility = Transmissibility.getBoundaryTransmissibility(block, grid_type, direction_y, phase);
                    block.boundary_pressure = boundary_pressure_y[counter];
                    block.boundary_pressure_times_transmissibility = boundary_transmissibility * boundary_pressure_y[counter];
                    block.boundary_transmissibility = boundary_transmissibility;
                }

                //boundary pressure gradient is positive for depletion and negative for encroachment
                boundary_transmissibility = Transmissibility.getBoundaryTransmissibility(block, grid_type, direction_x, phase);
                block.boundary_pressure_gradient += boundary_pressure_gradient_x[counter] * block.delta_x / 2 * boundary_transmissibility;

                boundary_transmissibility = Transmissibility.getBoundaryTransmissibility(block, grid_type, direction_y, phase);
                block.boundary_pressure_gradient += boundary_pressure_gradient_y[counter] * block.delta_y / 2 * boundary_transmissibility;


                //#########################################################################################
                //Set well rates
                if (wells[counter] != null)
                {
                    well = wells[counter];
                    block.type = GridBlock.Type.Well;

                    //specified flow rate wells
                    if (well.type_calculation == Well.TypeCalculation.Specified_Flow_Rate)
                    {
                        block.well_type = GridBlock.WellType.Specified_Flow_Rate;
                        block.well_flow_rate = well.specified_flow_rate;
                        block.specified_BHP = well.specified_BHP;

                        block.rw = well.rw;
                        double well_geometric_factor = Well.getGeometricFactor(block);
                        block.well_geometric_factor = well_geometric_factor;
                        block.well_transmissibility = Well.getTransmissibility(block, well_geometric_factor, Well.Phase.Water);
                    }
                    //specified BHP wells
                    else
                    {
                        block.well_type = GridBlock.WellType.Specified_BHP;
                        block.BHP = well.specified_BHP;
                        block.rw = well.rw;
                        block.skin = well.skin;
                        double well_geometric_factor = Well.getGeometricFactor(block);
                        block.well_geometric_factor = well_geometric_factor;
                        block.well_transmissibility = Well.getTransmissibility(block, well_geometric_factor, Well.Phase.Water);
                    }
                }
            }

            #region calculate the geometric factors of a heterogeneous grid
            if (!homogeneous)
            {
                var direction_x = Transmissibility.Direction.x;
                var direction_y = Transmissibility.Direction.y;
                int x_counter = 0;
                int y_counter = 0;
                //int z_counter = 0;

                for (int i = 0; i < grid.Length; i++)
                {
                    block = grid[i];

                    if (x > 1)
                    {
                        x_counter = block.east_counter != -1 ? block.east_counter : block.west_counter;

                        block.GF_x = Transmissibility.getGeometricFactor(block, grid[x_counter], grid_type, direction_x);
                    }
                    if (y > 1)
                    {
                        y_counter = block.north_counter != -1 ? block.north_counter : block.south_counter;

                        block.GF_y = Transmissibility.getGeometricFactor(block, grid[y_counter], grid_type, direction_x);
                    }
                }
            }
            #endregion

            #endregion

            #region Output
            SinglePhase.OutPut2D output = new SinglePhase.OutPut2D(grid, grid_dimensions, data.what, data.where, compressibility, data.file_name, data.formatted, data.single_file);
            #endregion

            #region Choose Solver
            if (compressibility == TypeDefinitions.Compressibility.Incompressible)
            {
                SolverSinglePhase.incompressible(grid, output);
            }
            else if (compressibility == TypeDefinitions.Compressibility.Slightly_Compressible)
            {
                SolverSinglePhase.slightly_compressible(grid, delta_t, time_max);
            }
            else
            {
                SolverSinglePhase.compressible(grid, delta_t, time_max, convergence_pressure, pvt);
            }
            #endregion
        }

        //Method Name: initializeSimulationSinglePhase
        //Objectives: The same as the previous method, but it does not take data as input. It uses internal hard coded sata.
        //Inputs: N/A
        //Outputs: N/A
        public static void initializeSimulationSinglePhase()
        {
            #region Initialize Rectangular grid dimensions
            bool homogeneous = false;
            int x = 5, y = 1, z = 1;

            int[] inactive_blocks = new int[] { };

            //creates an arrayfor the grid x, y and z dimensions
            int[] grid_dimensions = new int[] { x, y, z };

            //calculates the size of the entire grid "including both active and non-active blocks"
            int size = x * y * z;
            //calculates the size of the active-blocks only
            int size_active = x * y * z - inactive_blocks.Length;

            //use this if the natural ordering excludes inactive blocks
            if (inactive_blocks.Length > 0)
            {
                size = size_active;
            }

            GridBlock[] grid;
            #endregion

            #region Initialize grid
            //The gridding technique used here is for only a single row or a single column
            int[] delta_X = new int[x];
            int[] delta_Y = new int[y];
            int[] delta_Z = new int[z];

            //Homogeneous grid sizing
            if (homogeneous)
            {
                for (int i = 0; i < x; i++) { delta_X[i] = 300; }
                for (int i = 0; i < y; i++) { delta_Y[i] = 350; }
                for (int i = 0; i < z; i++) { delta_Z[i] = 40; }
            }
            //Heterogeneous grid sizing
            else
            {
                //General grid values
                for (int i = 0; i < x; i++) { delta_X[i] = 300; }
                for (int i = 0; i < y; i++) { delta_Y[i] = 500; }
                for (int i = 0; i < z; i++) { delta_Z[i] = 50; }
                //Specific heterogenities
                delta_X = new int[] { 400, 300, 150, 200, 250 };
            }
            #endregion

            #region Initialize Rock properties
            //The gridding technique used here is for the whole grid


            double[] Kx_data = new double[size];
            double[] Ky_data = new double[size];
            double[] Kz_data = new double[size];
            double[] porosity = new double[size];
            double compressibility_rock = 0;

            //Homogeneous grid
            if (homogeneous)
            {
                for (int i = 0; i < size; i++)
                {
                    Kx_data[i] = 270;
                    Ky_data[i] = 270;
                    porosity[i] = 0.27;
                    compressibility_rock = 0.000001;
                }
            }
            //Heterogeneous grid
            else
            {
                //General 
                for (int i = 0; i < size; i++)
                {
                    Kx_data[i] = 270;
                    Ky_data[i] = 270;
                    porosity[i] = 0.27;
                    //compressibility_rock = 0.000001;
                }
                //Specific heterogenities
                Kx_data = new double[] { 273, 248, 127, 333, 198 };
                Ky_data = Kx_data;
                porosity = new double[] { 0.21, 0.17, 0.1, 0.25, 0.13 };
                compressibility_rock = 0;
            }
            #endregion

            #region Initialize PVT

            double FVF = 1;
            double viscosity = 1.5;

            //For slightly-compressibly fluid
            double compressibility_fluid = 2.5 * Math.Pow(10, -5);

            //For compressible fluid, this data is used for constructing the PVT table
            double[][] g_data = new double[4][];
            g_data[0] = new double[] { 14.7, 264.7, 514.7, 1014.7, 2014.7, 2514.7, 3014.7, 4014.7, 5014.7, 9014.7 };
            g_data[1] = new double[] { 0.166666, 0.012093, 0.006274, 0.003197, 0.001614, 0.001294, 0.00108, 0.000811, 0.0006490, 0.000386 };
            g_data[2] = new double[] { 0.008, 0.0096, 0.0112, 0.014, 0.0189, 0.0208, 0.0228, 0.0268, 0.0309, 0.047 };
            g_data[3] = new double[] { 0.0647, 0.8916, 1.7185, 3.3727, 6.8806, 8.3326, 9.9837, 13.2952, 16.6139, 27.948 };

            PVT pvt = new PVT(g_data: g_data);
            #endregion

            #region  Initialize boundary conditions

            int initial_pressure = 3000;

            double[] boundary_flow_rate = new double[size];
            double[] boundary_pressure_x = new double[size];
            double[] boundary_pressure_y = new double[size];
            double[] boundary_pressure_gradient_x = new double[size];
            double[] boundary_pressure_gradient_y = new double[size];


            //boundary_pressure_x[0] = 4000;
            //boundary_flow_rate[0] = -500;
            //boundary_flow_rate[3] = 200;
            //boundary_pressure_gradient_x[1] = 0.3; boundary_pressure_gradient_x[3] = 0.3;
            //boundary_pressure_y[0] = 4000; boundary_pressure_y[1] = 4000;
            //boundary_pressure_gradient_x[3] = 0.2;
            #endregion

            #region  Initialize wells data
            Well[] wells = new Well[size];
            Well well;
            for (int i = 0; i < wells.Length; i++)
            {

                if (i == 3)
                {
                    well = new Well();

                    well.type_calculation = Well.TypeCalculation.Specified_Flow_Rate;
                    well.specified_flow_rate = 400;
                    well.specified_BHP = 1500;
                    well.skin = 0;
                    well.rw = 3;
                    well.type = Well.Type.Production;
                    //Add the well to the array
                    wells[i] = well;
                }

                //if (i == 3)
                //{
                //    well = new Well();

                //    well.type_calculation = Well.TypeCalculation.Specified_BHP;
                //    well.specified_BHP = 3500;
                //    well.rw = 3;
                //    well.skin = 0;
                //    well.type = Well.Type.Injection;
                //    //Add the well to the array
                //    wells[i] = well;
                //}
                //if (i == 4)
                //{
                //    well = new Well();

                //    well.type_calculation = Well.TypeCalculation.Specified_Flow_Rate;
                //    well.well_flow_rate = 1000;
                //    well.type = Well.Type.Production;

                //    //Add the well to the array
                //    wells[i] = well;
                //}
            }
            #endregion

            #region run specifications
            TypeDefinitions.Compressibility compressibility = TypeDefinitions.Compressibility.Slightly_Compressible;
            var phase = Transmissibility.Phase.Water;
            var grid_type = Transmissibility.GridType.Rectangular;

            //For compressible and slightly-compressible fluids, define the values for time steps and total simulation time
            double delta_t = 5;
            double time_max = 180;

            //For compressible fluid problems
            double convergence_pressure = 0.001;
            #endregion

            #region Initialize
            //#########################################################################################
            //Assign neighbouring blocks "according to the natural ordering procedure"
            grid = RectangularBlockNumbering.assignGridOrdering(grid_dimensions, inactive_blocks, RectangularBlockNumbering.NumberingScheme.Active_Only);

            GridBlock block;

            //Parallel.For(0, grid.Length, (counter) =>
            //{

            //});
            for (int counter = 0; counter < grid.Length; counter++)
            {
                block = grid[counter];

                //#########################################################################################
                //Assign PVT and rock properties

                block.delta_x = delta_X[block.x]; block.delta_y = delta_Y[block.y]; block.h = delta_Z[block.z];
                block.bulk_volume = block.delta_x * block.delta_y * block.h;

                block.Kx = Kx_data[counter]; block.Ky = Ky_data[counter]; block.Kz = Kz_data[counter];

                //To-Do: initialize pressure through hydraulic equilibrium
                block.pressure = initial_pressure;

                if (compressibility == TypeDefinitions.Compressibility.Compressible)
                {
                    block.water_viscosity = pvt.getWaterViscosity(initial_pressure);
                    block.So = 0; block.Sg = 0; block.Sw = 1;
                    block.Kro = 0; block.Krg = 0; block.Krw = 1;
                    block.Cf = compressibility_rock; block.C = compressibility_fluid;

                    block.Bw = pvt.getWaterFVF(initial_pressure);
                    block.porosity = Vp_Calculator.chord_slope_Vp(compressibility_rock, porosity[counter], block.pressure, initial_pressure);
                }
                else if (compressibility == TypeDefinitions.Compressibility.Slightly_Compressible)
                {
                    //For slightly compressibly fluids, viscosity is independent of pressure
                    block.water_viscosity = viscosity;
                    //For a single phase fluid, saturation and relative permeabilities of the fluid is equal to unity
                    block.So = 0; block.Sg = 0; block.Sw = 1;
                    block.Kro = 0; block.Krg = 0; block.Krw = 1;
                    //For slightly compressible fluids, the values of rock compressibility "Cf" and fluid compressibility C_fluid are used to estimate the new values
                    block.Cf = compressibility_rock; block.C = compressibility_fluid;

                    block.Bw = PVT.chord_slope_FVF(compressibility_fluid, FVF, block.pressure, initial_pressure);
                    block.porosity = Vp_Calculator.chord_slope_Vp(compressibility_rock, porosity[counter], block.pressure, initial_pressure);

                }
                else
                {
                    block.water_viscosity = viscosity; block.Bw = FVF;
                    block.porosity = porosity[counter];
                    block.So = 0; block.Sg = 0; block.Sw = 1;
                    block.Kro = 0; block.Krg = 0; block.Krw = 1;
                }

                //#########################################################################################
                //Calculate geometric factors "the part of the transmissibility that is always constant" only once in the initialization process
                var direction_x = Transmissibility.Direction.x;
                var direction_y = Transmissibility.Direction.y;

                if (homogeneous)
                {
                    block.GF_x = Transmissibility.getGeometricFactor(block, grid_type, direction_x);
                    block.GF_y = Transmissibility.getGeometricFactor(block, grid_type, direction_y);
                }

                //#########################################################################################
                //Set boundary conditions

                double boundary_transmissibility = 0;

                block.boundary_flow_rate = boundary_flow_rate[counter];
                if (boundary_pressure_x[counter] != 0)
                {
                    boundary_transmissibility = Transmissibility.getBoundaryTransmissibility(block, grid_type, direction_x, phase);
                    block.boundary_pressure_times_transmissibility = boundary_transmissibility * boundary_pressure_x[counter];
                    block.boundary_transmissibility = boundary_transmissibility;
                }
                else if (boundary_pressure_y[counter] != 0)
                {
                    boundary_transmissibility = Transmissibility.getBoundaryTransmissibility(block, grid_type, direction_y, phase);
                    block.boundary_pressure_times_transmissibility = boundary_transmissibility * boundary_pressure_y[counter];
                    block.boundary_transmissibility = boundary_transmissibility;
                }

                //boundary pressure gradient is positive for depletion and negative for encroachment
                boundary_transmissibility = Transmissibility.getBoundaryTransmissibility(block, grid_type, direction_x, phase);
                block.boundary_pressure_gradient += boundary_pressure_gradient_x[counter] * block.delta_x / 2 * boundary_transmissibility;

                boundary_transmissibility = Transmissibility.getBoundaryTransmissibility(block, grid_type, direction_y, phase);
                block.boundary_pressure_gradient += boundary_pressure_gradient_y[counter] * block.delta_y / 2 * boundary_transmissibility;


                //#########################################################################################
                //Set well rates
                if (wells[counter] != null)
                {
                    well = wells[counter];
                    block.type = GridBlock.Type.Well;

                    if (well.type_calculation == Well.TypeCalculation.Specified_Flow_Rate)
                    {
                        block.well_type = GridBlock.WellType.Specified_Flow_Rate;
                        block.well_flow_rate = well.specified_flow_rate;
                        block.specified_BHP = well.specified_BHP;

                        block.rw = well.rw;
                        double well_geometric_factor = Well.getGeometricFactor(block);
                        block.well_geometric_factor = well_geometric_factor;
                        block.well_transmissibility = Well.getTransmissibility(block, well_geometric_factor, Well.Phase.Water);
                    }
                    else
                    {
                        block.well_type = GridBlock.WellType.Specified_BHP;
                        block.BHP = well.specified_BHP;
                        block.rw = well.rw;
                        block.skin = well.skin;
                        double well_geometric_factor = Well.getGeometricFactor(block);
                        block.well_geometric_factor = well_geometric_factor;
                        block.well_transmissibility = Well.getTransmissibility(block, well_geometric_factor, Well.Phase.Water);
                    }
                }
            }

            #region calculate the geometric factors of a heterogeneous grid
            if (!homogeneous)
            {
                var direction_x = Transmissibility.Direction.x;
                var direction_y = Transmissibility.Direction.y;
                int x_counter = 0;
                int y_counter = 0;
                //int z_counter = 0;

                for (int i = 0; i < grid.Length; i++)
                {
                    block = grid[i];

                    if (x > 1)
                    {
                        x_counter = block.east_counter != -1 ? block.east_counter : block.west_counter;

                        block.GF_x = Transmissibility.getGeometricFactor(block, grid[x_counter], grid_type, direction_x);
                    }
                    if (y > 1)
                    {
                        y_counter = block.north_counter != -1 ? block.north_counter : block.south_counter;

                        block.GF_y = Transmissibility.getGeometricFactor(block, grid[y_counter], grid_type, direction_x);
                    }
                }
            }
            #endregion

            #endregion

            #region Choose Solver
            if (compressibility == TypeDefinitions.Compressibility.Incompressible)
            {
                //SolverSinglePhase.incompressible(grid);
            }
            else if (compressibility == TypeDefinitions.Compressibility.Slightly_Compressible)
            {
                SolverSinglePhase.slightly_compressible(grid, delta_t, time_max);
            }
            else
            {
                SolverSinglePhase.compressible(grid, delta_t, time_max, convergence_pressure, pvt);
            }
            #endregion
        }
    }
}
