using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUBOS
{
    //Method Name: lookUp
    //Objectives: this is a private (internal method for the internal use of this class only) method that makes table lookup interpolation
    //Inputs: two variables representing the X and Y arrays "The two columns of the table" and the Y value
    //Outputs: a variable that represents the X value corresponding to the particular Y value

    //Method Name: extrapolate
    //Objectives: this is a private (internal method for the internal use of this class only) method that makes table lookup and extrapolation
    //Inputs: two variables representing the X and Y arrays "The two columns of the table" and the Y value
    //Outputs: a variable that represents the X value corresponding to the particular Y value

    //Method Name: getPVT "where PVT may be OilFVF, GasViscosity, WaterDensity, ..."
    //Objectives: calculate the corresponding PVT value at a certain pressure
    //Inputs: a variable representing the value of the pressure
    //Outputs: the corresponding PVT value
    class PVT
    {
        private double bubble_point_pressure;
        private double[][] s_o_data, us_o_d, s_w_data, us_w_d, g_data;

        //Constructor Name: PVT
        //Objectives: Public constructor of the class. This is used only once to get the PVT data stored within the PVT class instance
        //Inputs: arrays representing the PVT values
        public PVT(double[][] s_o_data = null, double[][] us_o_d = null, double[][] s_w_data = null, double[][] us_w_d = null, double[][] g_data = null, double bubble_point = 14.7)
        {
            this.s_o_data = s_o_data;
            this.us_o_d = us_o_d;

            this.s_w_data = s_w_data;
            this.us_w_d = us_w_d;

            this.g_data = g_data;

            this.bubble_point_pressure = bubble_point;
        }

        //Internal helper method to do the table lookups and interpolations
        private double lookUp(double[] data_y, double[] data_x, double y)
        {
            double y1, y2, x1, x2, x = -1;
            for (int i = 0; i < data_y.Length; i++)
            {
                //if an exact match exists, return the table value. No interpolation is required
                if (data_y[i] == y)
                {
                    return data_x[i];
                }

                //Look for the first element in the data_y array that is smaller than y
                if (data_y[i] > y)
                {
                    y1 = data_y[i - 1]; x1 = data_x[i - 1];
                    y2 = data_y[i]; x2 = data_x[i];
                    x = x1 + (y - y1) / ((y2 - y1) / (x2 - x1));
                    break;
                }
            }

            return x;
        }

        //Internal helper method for extrapolation
        private double extrapolate(double[] data_y, double[] data_x, double y)
        {
            double y1, y2, x1, x2, x = -1;

            y1 = data_y[0]; x1 = data_x[0];
            y2 = data_y[1]; x2 = data_x[1];
            x = x1 + (y - y1) / ((y2 - y1) / (x2 - x1));

            return x >= 0 ? x : 0;
        }

        public double getOilFVF(double pressure)
        {
            double[] Y, X;

            if (pressure >= bubble_point_pressure)
            {
                Y = us_o_d[0]; X = us_o_d[1];
                return extrapolate(Y, X, pressure);
            }
            else
            {
                Y = s_o_data[0]; X = s_o_data[1];
                return lookUp(Y, X, pressure);
            }

        }
        public double getWaterFVF(double pressure)
        {
            double[] Y, X;

            if (pressure >= bubble_point_pressure)
            {
                Y = us_w_d[0]; X = us_w_d[1];
                return extrapolate(Y, X, pressure);
            }
            else
            {
                Y = s_w_data[0]; X = s_w_data[1];
                return lookUp(Y, X, pressure);
            }
        }
        public double getGasFVF(double pressure)
        {
            double[] Y, X;
            Y = g_data[0]; X = g_data[1];

            return lookUp(Y, X, pressure);
        }

        public double getOilViscosity(double pressure)
        {
            double[] Y, X;

            if (pressure >= bubble_point_pressure)
            {
                Y = us_o_d[0]; X = us_o_d[2];
                return extrapolate(Y, X, pressure);
            }
            else
            {
                Y = s_o_data[0]; X = s_o_data[2];
                return lookUp(Y, X, pressure);
            }
        }
        public double getWaterViscosity(double pressure)
        {
            double[] Y, X;

            if (pressure >= bubble_point_pressure)
            {
                Y = us_w_d[0]; X = us_w_d[2];
                return extrapolate(Y, X, pressure);
            }
            else
            {
                Y = s_w_data[0]; X = s_w_data[2];
                return lookUp(Y, X, pressure);
            }
        }
        public double getGasViscosity(double pressure)
        {
            double[] Y, X;
            Y = g_data[0]; X = g_data[2];

            return lookUp(Y, X, pressure);
        }

        public double getRso(double pressure)
        {
            double[] Y, X;

            if (pressure >= bubble_point_pressure)
            {
                return us_o_d[4][0];
            }
            else
            {
                Y = s_o_data[0]; X = s_o_data[4];
                return lookUp(Y, X, pressure);
            }
        }
        public double getRsw(double pressure)
        {
            double[] Y, X;

            if (pressure >= bubble_point_pressure)
            {
                return us_w_d[4][0];
            }
            else
            {
                Y = s_o_data[0]; X = s_o_data[1];
                return lookUp(Y, X, pressure);
            }
        }

        //Method name: chord_slope_FVF
        //Objectives: calculates the FVF property at a future point pressure "P at time n+1" using the value of compressibility
        //Inputs: the value of the compressibility, the old value of the FVF property "FVF at time n" and the old and new pressures to calculate the pressure difference
        //Outputs: the new value of the FVF propery "FVF at time n+1"
        public static double chord_slope_FVF(double C, double old_FVF, double new_pressure, double old_pressure)
        {
            return old_FVF * (1 - C * (new_pressure - old_pressure));
        }

        //Method Name: initializePressure
        //Objectives: initializing the pressure for problems considering the gravity effect
        //Inputs: there are two overloads "versions" of this method. one that takes an input for fluid compressibility "C" used for incompressible and slightly-compressible fluids
        //Inputs: the other form takes no input for compressibility, but will use the PVT values provided for density calculations
        //Outputs: an array of the values of the pressure for each depth
        public static double[] initializePressure(double[] depth_array, double pressure, double pressure_measurement_depth, double density, double C, double FVF)
        {
            double convergence_criterion = 0.001;
            int length = depth_array.Length;
            double[] pressure_array = new double[length];
            double[] old_pressure = new double[length];
            double[] density_array = new double[length];
            //
            //Initialize density-array
            for (int i = 0; i < length; i++)
            {
                depth_array[i] = density;
            }

            double density_standard = density * FVF;
            double new_FVF;

            //if the fluid is incompressible, then no-iterations are needed
            if (C == 0)
            {
                for (int i = 0; i < length; i++)
                {
                    pressure_array[i] = pressure + density * (depth_array[i] - pressure_measurement_depth);
                }
            }
            //iterate for the value of density for slightly-compressible fluids
            else
            {
                //First iteration
                for (int i = 0; i < length; i++)
                {
                    pressure_array[i] = pressure + density_array[i] * (depth_array[i] - pressure_measurement_depth);
                }
                //update density-array
                for (int i = 0; i < length; i++)
                {
                    new_FVF = chord_slope_FVF(C, FVF, pressure_array[i], pressure);
                    density_array[i] = density_standard / new_FVF;
                }

                //From the second iteration on, check for convergence
                while (checkPressureConvergence(pressure_array, old_pressure) >= convergence_criterion)
                {
                    old_pressure = pressure_array.Clone() as double[];

                    for (int i = 0; i < length; i++)
                    {
                        pressure_array[i] = pressure + density_array[i] * (depth_array[i] - pressure_measurement_depth);
                    }
                    //update density-array
                    for (int i = 0; i < length; i++)
                    {
                        new_FVF = chord_slope_FVF(C, FVF, pressure_array[i], pressure);
                        density_array[i] = density_standard / new_FVF;
                    }
                }
            }

            return pressure_array;
        }

        //Method Name: checkPressureConvergence
        //Objectives: used to check if the pressure solution converges through the non-linear iterations
        //Inputs: two arrays with the values of the old and new grid blocks pressures
        //Outputs: the maximum change in pressure that occurs in any of the grid blocks between two successive non-linear iterations
        //The output is compared against a specified criterion of convergence
        public static double checkPressureConvergence(double[] new_P, double[] old_P)
        {
            int length = new_P.Length;
            double[] temp = new double[length];
            for (int i = 0; i < length; i++)
            {
                temp[i] = Math.Abs(new_P[i] - old_P[i]) / old_P[i];
            }

            return temp.Max();
        }
    }
}
