using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUBOS
{
    //Method_1 Name: lookUp
    //Objectives: this is a private (internal method for the internal use of this class only) method that makes table lookup interpolation
    //Inputs: two variables representing the X and Y arrays "The two columns of the table" and the Y value
    //Outputs: a variable that represents the X value corresponding to the particular Y value

    //Method_2 Name: extrapolate
    //Objectives: this is a private (internal method for the internal use of this class only) method that makes table lookup and extrapolation
    //Inputs: two variables representing the X and Y arrays "The two columns of the table" and the Y value
    //Outputs: a variable that represents the X value corresponding to the particular Y value

    //Method_3 Name: getPVT "where PVT may be OilFVF, GasViscosity, WaterDensity, ..."
    //Objectives: calculate the corresponding PVT value at a certain pressure
    //Inputs: a variable representing the value of the pressure
    //Outputs: the corresponding PVT value
    class PVT
    {
        private double bubble_point_pressure;
        private double[][] s_o_data, us_o_d, s_w_data, us_w_d, g_data;

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
    }
}
