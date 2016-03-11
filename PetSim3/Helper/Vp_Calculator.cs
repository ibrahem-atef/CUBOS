using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUBOS
{
    class Vp_Calculator
    {
        //Rock compressibility factor
        double Cf;

        double new_Vp;

        //Constructor that takes a Cf value as an argument
        public Vp_Calculator(double Cf)
        {
            this.Cf = Cf;
        }

        //The method used to generate the new pore volume
        public double getVp(double old_Vp, double old_pressure, double new_pressure)
        {
            new_Vp = old_Vp * (1 + Cf * (new_pressure - old_pressure));
            return new_Vp;
        }

        //Method name: chord_slope_Vp
        //Objectives: calculates the Vp property at a future point pressure "P at time n+1" using the value of rock compressibility
        //Inputs: the value of the compressibility, the old value of the Vp property "Vp at time n" and the old and new pressures to calculate the pressure difference
        //Outputs: the new value of the Vp propery "Vp at time n+1"
        public static double chord_slope_Vp(double C, double old_Vp, double new_pressure, double old_pressure)
        {
            return old_Vp * (1 + C * (new_pressure - old_pressure));
        }
    }
}
