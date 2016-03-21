using MathNet.Numerics.LinearAlgebra;
using System;
using SinglePhase;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUBOS
{
    class SolverSinglePhase
    {
        //this variable is used when determining if a well is capable of producing fluid under the specified flow rate
        private static bool skip;

        //#############################################################################################
        //Method Name: incompressible
        //Objectives: Solver for the single-phase incompressible fluid
        //The solution is time-independent
        //Inputs: an array of variables of type "GridBlock"
        //Inputs: a variable of type OutPut2D used to display the results
        //Outputs: N/A
        public static void incompressible(GridBlock[] grid, SinglePhase.OutPut2D output)
        {
            int grid_length = grid.Length;
            double[][] matrix_P = new double[grid_length][];
            double[] matrix_C = new double[grid_length];

            GridBlock block;
            GridBlock next_block;
            var phase = Transmissibility.Phase.Water;

            double temp = 0;
            double constants = 0;
            double T = 0;
            double[] new_P;

            double MBE;

            for (int i = 0; i < grid_length; i++)
            {
                matrix_P[i] = new double[grid_length];

                block = grid[i];
                temp = 0;
                constants = 0;
                T = 0;

                //#########################################################################################
                //Calculate transmissibilities
                if (block.east_counter != -1)
                {
                    next_block = grid[block.east_counter];
                    T = Transmissibility.getTransmissibility(block, next_block, block.GF_x, phase);
                    matrix_P[i][next_block.counter] = T;
                    temp -= T;

                    //gravity terms
                    constants += T * (block.water_density + next_block.water_density) * 0.5 * (next_block.depth - block.depth);
                }

                if (block.west_counter != -1)
                {
                    next_block = grid[block.west_counter];
                    T = Transmissibility.getTransmissibility(block, next_block, next_block.GF_x, phase);
                    matrix_P[i][next_block.counter] = T;
                    temp -= T;

                    //gravity terms
                    constants += T * (block.water_density + next_block.water_density) * 0.5 * (next_block.depth - block.depth);
                }

                if (block.north_counter != -1)
                {
                    next_block = grid[block.north_counter];
                    T = Transmissibility.getTransmissibility(block, next_block, block.GF_y, phase);
                    matrix_P[i][next_block.counter] = T;
                    temp -= T;

                    //gravity terms
                    constants += T * (block.water_density + next_block.water_density) * 0.5 * (next_block.depth - block.depth);
                }

                if (block.south_counter != -1)
                {
                    next_block = grid[block.south_counter];
                    T = Transmissibility.getTransmissibility(block, next_block, next_block.GF_y, phase);
                    matrix_P[i][next_block.counter] = T;
                    temp -= T;

                    //gravity terms
                    constants += T * (block.water_density + next_block.water_density) * 0.5 * (next_block.depth - block.depth);
                }

                //#########################################################################################
                //check boundary conditions
                //Specified boundary pressure
                constants -= block.boundary_pressure_times_transmissibility;
                temp -= block.boundary_transmissibility;

                //Specified boundary rate
                constants += block.boundary_flow_rate;

                //Specified boundary pressure gradient
                constants += block.boundary_pressure_gradient;

                //#########################################################################################
                //check well rates
                if (block.type == GridBlock.Type.Well)
                {
                    //Check for specified BHP condition
                    if (block.well_type == GridBlock.WellType.Specified_BHP)
                    {
                        constants -= block.well_transmissibility * block.BHP;
                        temp -= block.well_transmissibility;
                    }
                    else
                    {
                        constants += block.well_flow_rate;
                    }
                }

                //#########################################################################################
                matrix_P[i][block.counter] = temp;
                matrix_C[i] = constants;
            }

            new_P = solveForP(matrix_P, matrix_C);

            updatePropertiesIncompressible(new_P, grid);

            MBE = SinglePhase.MBE.production(grid);

            output.write(MBE);
            //Console.WriteLine(new_P[0] + ", " + new_P.Last());
        }

        //Method Name: slightly_compressible
        //Objectives: Solver for the single-phase slightly compressible fluid
        //The solution is time-dependent
        //Inputs: an array of variables of type "GridBlock", the value of the time step between subsequent runs and the value of the total time duration
        //Inputs: a variable of type OutPut2D used to display the results
        //Outputs: N/A
        public static void slightly_compressible(GridBlock[] grid, double delta_t, double time_max, SinglePhase.OutPut2D output, PVT pvt)
        {
            int grid_length = grid.Length;
            double[][] matrix_P = new double[grid_length][];
            double[] matrix_C = new double[grid_length];

            const double a = 5.614583;
            double accumulation_term;

            double IMB;
            double IMB_production;
            double IMB_accumulation;

            GridBlock block;
            GridBlock next_block;
            var phase = Transmissibility.Phase.Water;

            double temp = 0;
            double constants = 0;
            double T = 0;
            double[] new_P;

            output.write(0, 0);

            for (double current_time = 0; current_time < time_max; current_time += delta_t)
            {
                skip = false;

                #region grid blocks loop
                for (int i = 0; i < grid_length; i++)
                {
                    matrix_P[i] = new double[grid_length];

                    block = grid[i];
                    temp = 0;
                    constants = 0;
                    T = 0;
                    accumulation_term = 0;

                    //#########################################################################################
                    //Calculate transmissibilities
                    if (block.east_counter != -1)
                    {
                        next_block = grid[block.east_counter];
                        T = Transmissibility.getTransmissibility(block, next_block, block.GF_x, phase);
                        matrix_P[i][next_block.counter] = T;
                        temp -= T;

                        //gravity terms
                        constants += T * (block.water_density + next_block.water_density) * 0.5 * (next_block.depth - block.depth);
                    }

                    if (block.west_counter != -1)
                    {
                        next_block = grid[block.west_counter];
                        T = Transmissibility.getTransmissibility(block, next_block, next_block.GF_x, phase);
                        matrix_P[i][next_block.counter] = T;
                        temp -= T;

                        //gravity terms
                        constants += T * (block.water_density + next_block.water_density) * 0.5 * (next_block.depth - block.depth);
                    }

                    if (block.north_counter != -1)
                    {
                        next_block = grid[block.north_counter];
                        T = Transmissibility.getTransmissibility(block, next_block, block.GF_y, phase);
                        matrix_P[i][next_block.counter] = T;
                        temp -= T;

                        //gravity terms
                        constants += T * (block.water_density + next_block.water_density) * 0.5 * (next_block.depth - block.depth);
                    }

                    if (block.south_counter != -1)
                    {
                        next_block = grid[block.south_counter];
                        T = Transmissibility.getTransmissibility(block, next_block, next_block.GF_y, phase);
                        matrix_P[i][next_block.counter] = T;
                        temp -= T;

                        //gravity terms
                        constants += T * (block.water_density + next_block.water_density) * 0.5 * (next_block.depth - block.depth);
                    }

                    //#########################################################################################
                    //check boundary conditions
                    //Specified boundary pressure
                    constants -= block.boundary_pressure_times_transmissibility;
                    temp -= block.boundary_transmissibility;

                    //Specified boundary rate
                    constants += block.boundary_flow_rate;

                    //Specified boundary pressure gradient
                    constants += block.boundary_pressure_gradient;

                    //#########################################################################################
                    //check well rates
                    if (block.type == GridBlock.Type.Well)
                    {
                        //Check for specified BHP condition
                        if (block.well_type == GridBlock.WellType.Specified_BHP)
                        {
                            constants -= block.well_transmissibility * block.BHP;
                            temp -= block.well_transmissibility;
                        }
                        else
                        {
                            constants += block.well_flow_rate;
                        }
                    }

                    //#########################################################################################
                    //Accumulation terms
                    //reference FVF = 1
                    accumulation_term = block.bulk_volume * block.porosity * (block.Cf + block.C) / (a * 1 * delta_t);
                    constants -= accumulation_term * block.pressure;
                    temp -= accumulation_term;

                    //#########################################################################################
                    matrix_P[i][block.counter] = temp;
                    matrix_C[i] = constants;
                }
                #endregion

                new_P = solveForP(matrix_P, matrix_C);

                #region This code handle the transformation of a specified flow_rate well into a specified BHP well
                for (int i = 0; i < grid.Length; i++)
                {
                    block = grid[i];
                    if (block.type == GridBlock.Type.Well)
                    {
                        if (block.well_type == GridBlock.WellType.Specified_Flow_Rate)
                        {
                            double BHP = new_P[i] - block.well_flow_rate / block.well_transmissibility;
                            if (BHP > block.specified_BHP)
                            {
                                block.BHP = BHP;
                            }
                            //If the well can no longer produce the fluid at the specified flow rate, convert the well to a specified BHP type
                            //
                            else
                            {
                                block.well_type = GridBlock.WellType.Specified_BHP;
                                block.BHP = block.specified_BHP;
                                //Re do this iteration as the BHP of the well went below the minimum
                                current_time -= delta_t;
                                skip = true;
                                break;
                            }
                        }
                        else
                        {
                            block.well_flow_rate = (new_P[i] - block.BHP) * block.well_transmissibility;
                            if (block.well_flow_rate < 50)
                            {
                                block.well_type = GridBlock.WellType.Specified_Flow_Rate;
                                block.well_flow_rate = 0;
                                //Re do this iteration as the BHP of the well went below the minimum
                                current_time -= delta_t;
                                skip = true;
                                break;
                            }
                        }
                    }
                }
                #endregion


                if (!skip)
                {
                    //calculate old time step part of the MBE
                    IMB_accumulation = MBE.accumulation(grid);

                    updatePropertiesSlightlyCompressible(new_P, grid, pvt);

                    //calculate new time step part of the MBE
                    IMB_accumulation = MBE.accumulation(grid) - IMB_accumulation;
                    IMB_production = MBE.production(grid);
                    IMB = IMB_accumulation / IMB_production / delta_t;
                    //store the result in the MBE class for easy handling between different methods
                    MBE.IMB = IMB;

                    output.write(IMB, current_time + delta_t);
                }
            }

        }

        //Method Name: compressible
        //Objectives: Solver for the single-phase slightly compressible fluid
        //The solution is time-dependent
        //Inputs: an array of variables of type "GridBlock", the value of the time step between subsequent runs and the value of the total time duration
        //Outputs: N/A
        public static void compressible(GridBlock[] grid, double delta_t, double time_max, double convergence_pressure, SinglePhase.OutPut2D output, PVT pvt)
        {
            int grid_length = grid.Length;
            double[][] matrix_P = new double[grid_length][];
            double[] matrix_C = new double[grid_length];

            const double a = 5.614583;
            double accumulation_term;

            double IMB;
            double IMB_production;
            double IMB_accumulation;

            GridBlock block;
            GridBlock next_block;
            var phase = Transmissibility.Phase.Water;

            double temp = 0;
            double constants = 0;
            double T = 0;
            double[] new_P = new double[grid_length];
            double[] old_P = new double[grid_length];
            double[] old_FVF = new double[grid_length];
            double[] initial_pressure = new double[grid_length];

            GridBlock[] old_grid = new GridBlock[grid_length];

            output.write(0, 0);

            for (double current_time = 0; current_time < time_max; current_time += delta_t)
            {
                skip = false;

                //Array.Copy(grid, old_grid, grid_length);
                for (int i = 0; i < grid_length; i++)
                {
                    old_FVF[i] = grid[i].Bw;
                    initial_pressure[i] = grid[i].pressure;
                    new_P[i] = 0;
                }

                int counter = 0;
                do
                {
                    counter += 1;
                    Array.Copy(new_P, old_P, grid_length);
                    matrix_C = new double[grid_length];
                    #region grid blocks loop
                    for (int i = 0; i < grid_length; i++)
                    {
                        matrix_P[i] = new double[grid_length];

                        block = grid[i];
                        temp = 0;
                        constants = 0;
                        T = 0;
                        accumulation_term = 0;

                        //#########################################################################################
                        //Calculate transmissibilities
                        if (block.east_counter != -1)
                        {
                            next_block = grid[block.east_counter];
                            T = Transmissibility.getTransmissibility(block, next_block, block.GF_x, phase);
                            matrix_P[i][next_block.counter] = T;
                            temp -= T;

                            //gravity terms
                            constants += T * (block.water_density + next_block.water_density) * 0.5 * (next_block.depth - block.depth);
                        }

                        if (block.west_counter != -1)
                        {
                            next_block = grid[block.west_counter];
                            T = Transmissibility.getTransmissibility(block, next_block, next_block.GF_x, phase);
                            matrix_P[i][next_block.counter] = T;
                            temp -= T;

                            //gravity terms
                            constants += T * (block.water_density + next_block.water_density) * 0.5 * (next_block.depth - block.depth);
                        }

                        if (block.north_counter != -1)
                        {
                            next_block = grid[block.north_counter];
                            T = Transmissibility.getTransmissibility(block, next_block, block.GF_y, phase);
                            matrix_P[i][next_block.counter] = T;
                            temp -= T;

                            //gravity terms
                            constants += T * (block.water_density + next_block.water_density) * 0.5 * (next_block.depth - block.depth);
                        }

                        if (block.south_counter != -1)
                        {
                            next_block = grid[block.south_counter];
                            T = Transmissibility.getTransmissibility(block, next_block, next_block.GF_y, phase);
                            matrix_P[i][next_block.counter] = T;
                            temp -= T;

                            //gravity terms
                            constants += T * (block.water_density + next_block.water_density) * 0.5 * (next_block.depth - block.depth);
                        }

                        //#########################################################################################
                        //check boundary conditions
                        //Specified boundary pressure
                        constants -= block.boundary_pressure_times_transmissibility;
                        temp -= block.boundary_transmissibility;

                        //Specified boundary rate
                        constants += block.boundary_flow_rate;

                        //Specified boundary pressure gradient
                        constants += block.boundary_pressure_gradient;

                        //#########################################################################################
                        //check well rates
                        if (block.type == GridBlock.Type.Well)
                        {
                            //Check for specified BHP condition
                            if (block.well_type == GridBlock.WellType.Specified_BHP)
                            {
                                constants -= block.well_transmissibility * block.BHP;
                                temp -= block.well_transmissibility;
                            }
                            else
                            {
                                constants += block.well_flow_rate;
                            }
                        }

                        //#########################################################################################
                        //Accumulation terms
                        //reference FVF = 1
                        double delta_p;
                        double new_FVF = 0;
                        if (counter == 1)
                        {
                            delta_p = -1;

                            double FVF_constants = 14.7 / (60 + 460) * (190 + 460);
                            double z_factor = PVT.calculateZ(738.44, 418.38, block.pressure + delta_p, 190, 1);
                            new_FVF = FVF_constants * z_factor / (block.pressure + delta_p) / 1;
                        }
                        else
                        {
                            delta_p = block.pressure - initial_pressure[i];
                            new_FVF = block.Bw;
                        }
                        accumulation_term = block.bulk_volume / (a * delta_t) * block.porosity * (1 / new_FVF - 1 / old_FVF[i]) / delta_p;
                        constants -= accumulation_term * initial_pressure[i];
                        temp -= accumulation_term;

                        //#########################################################################################
                        matrix_P[i][block.counter] = temp;
                        matrix_C[i] = constants;
                    }
                    #endregion

                    new_P = solveForP(matrix_P, matrix_C);

                    updatePropertiesCompressible(new_P, grid, pvt);

                } while (PVT.checkPressureConvergence(new_P, old_P) > convergence_pressure && counter < 20);
                

                #region This code handle the transformation of a specified flow_rate well into a specified BHP well
                for (int i = 0; i < grid.Length; i++)
                {
                    block = grid[i];
                    if (block.type == GridBlock.Type.Well)
                    {
                        if (block.well_type == GridBlock.WellType.Specified_Flow_Rate)
                        {
                            double BHP = new_P[i] - block.well_flow_rate / block.well_transmissibility;
                            if (BHP > block.specified_BHP)
                            {
                                block.BHP = BHP;
                            }
                            //If the well can no longer produce the fluid at the specified flow rate, convert the well to a specified BHP type
                            //
                            else
                            {
                                block.well_type = GridBlock.WellType.Specified_BHP;
                                block.BHP = block.specified_BHP;
                                //Re do this iteration as the BHP of the well went below the minimum
                                current_time -= delta_t;
                                skip = true;
                                break;
                            }
                        }
                        else
                        {
                            block.well_flow_rate = (new_P[i] - block.BHP) * block.well_transmissibility;
                            if (block.well_flow_rate < 50)
                            {
                                block.well_type = GridBlock.WellType.Specified_Flow_Rate;
                                block.well_flow_rate = 0;
                                //Re do this iteration as the BHP of the well went below the minimum
                                current_time -= delta_t;
                                skip = true;
                                break;
                            }
                        }
                    }
                }
                #endregion


                if (!skip)
                {
                    //calculate old time step part of the MBE
                    IMB_accumulation = MBE.accumulation(grid, old_FVF);

                    //updatePropertiesSlightlyCompressible(new_P, grid, pvt);

                    //calculate new time step part of the MBE
                    IMB_accumulation = MBE.accumulation(grid) - IMB_accumulation;
                    IMB_production = MBE.production(grid);
                    IMB = IMB_accumulation / IMB_production / delta_t;
                    //store the result in the MBE class for easy handling between different methods
                    MBE.IMB = IMB;

                    output.write(IMB, current_time + delta_t);
                }
                else
                {
                    updatePropertiesCompressible(initial_pressure, grid, pvt);
                }

                if (current_time == 5)
                {
                    delta_t = 5;
                }
            }
        }

        //#############################################################################################

        //Method Name: updatePropertiesIncompressible
        //Objectives: updates the well rates for the specified BHP wells in the incompressible fluid problems
        //Objectives: updates the BHP for the specified flow rate wells in the incompressible fluid problems
        //Inputs: values of the new pressures and the array representing the grid blocks
        //Outputs: this method internally updates the values of the fluid PVT and rock properties of each grid block
        private static void updatePropertiesIncompressible(double[] new_P, GridBlock[] grid)
        {
            GridBlock block;
            for (int i = 0; i < grid.Length; i++)
            {
                block = grid[i];
                block.pressure = new_P[i];
                if (block.type == GridBlock.Type.Well)
                {
                    if (block.well_type == GridBlock.WellType.Specified_BHP)
                    {
                        block.well_flow_rate = block.well_transmissibility * (block.pressure - block.BHP);
                    }
                    else
                    {
                        block.BHP = block.pressure - (block.well_flow_rate / block.well_transmissibility);
                    }
                }
            }
        }

        //Method Name: updatePropertiesSlightlyCompressible
        //Objectives: uses the fluid and rock compressibilities to calculate the new properties for the slightly compressible solver
        //Inputs: values of the new pressures and the array representing the grid blocks and object containing the data for pvt calculations
        //Outputs: this method internally updates the values of the fluid PVT and rock properties of each grid block
        private static void updatePropertiesSlightlyCompressible(double[] new_P, GridBlock[] grid, PVT pvt)
        {
            GridBlock block;
            for (int i = 0; i < grid.Length; i++)
            {
                block = grid[i];
                block.pressure = new_P[i];
                //block.Bw = PVT.chord_slope_FVF(block.C, block.pressure);
                //block.water_viscosity = pvt.getWaterViscosity(block.pressure);
                //block.porosity = Vp_Calculator.chord_slope_Vp(block.Cf, block.porosity, new_P[i], block.pressure);
                if (block.type == GridBlock.Type.Well)
                {
                    block.well_transmissibility = Well.getTransmissibility(block, block.well_geometric_factor, Well.Phase.Water);
                }
            }
        }

        //Method Name: updatePropertiesCompressible
        //Objectives: uses the PVT properties table and rock compressibilities to calculate the new properties for the compressible solver
        //Inputs: values of the new pressures, the array representing the grid blocks and a variable of type "PVT" that makes table look-up and interpolations for new PVT values
        //Outputs: this method internally updates the values of the fluid PVT and rock properties of each grid block
        private static void updatePropertiesCompressible(double[] new_P, GridBlock[] grid, PVT pvt)
        {
            GridBlock block;
            double new_pressure;
            const double a = 5.614583;
            double FVF_constants = 14.7 / (60 + 460) * (190 + 460);

            for (int i = 0; i < grid.Length; i++)
            {
                block = grid[i];
                new_pressure = new_P[i];

                double z_factor = PVT.calculateZ(738.44, 418.38, new_pressure, 190, 1);
                double old_z_factor = PVT.calculateZ(738.44, 418.38, block.pressure, 190, 1);

                block.Bw = FVF_constants * z_factor / new_pressure / 1;

                block.water_viscosity = pvt.getWaterViscosity(new_pressure);
                

                if (block.well_transmissibility != 0)
                {
                    block.well_transmissibility = Well.getTransmissibility(block, block.well_geometric_factor, Well.Phase.Water);
                }

                block.water_density = block.water_density * new_pressure * old_z_factor / (block.pressure * z_factor);

                block.pressure = new_pressure;
            }
        }


        //#############################################################################################
        //
        private static double[] solveForP(double[][] matrix_P, double[] matrix_C)
        {
            Matrix<double> A = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix.OfRowArrays(matrix_P);
            Vector<double> B = MathNet.Numerics.LinearAlgebra.Double.DenseVector.OfArray(matrix_C);

            Vector<double> temp = A.Solve(B);

            return temp.ToArray();
        }
    }
}
