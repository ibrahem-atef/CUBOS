using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUBOS
{
    class Well
    {
        //Notes: a variable of type "GridBlock" represents an object that contains all the data of a certain block in the grid
        //Notes: a variable of type "Phase" to represent the phase (oil, gas or water) to compute the transmisibility for

        //Declarations of constants and variables to be used within the static methods of this class
        public enum Phase { Oil, Gas, Water }
        private const double Bc = 0.001127;

        //Variables of a variable of type "Well" instance
        public enum TypeCalculation { Specified_Flow_Rate, Specified_BHP, Inactive}
        public TypeCalculation type_calculation;
        public enum Type { Production, Injection}
        public Type type;
        public double rw;
        public double skin;
        public double specified_flow_rate;
        public double specified_BHP;

        //constructor with default values set
        public Well(TypeCalculation well_type_calculation = TypeCalculation.Specified_Flow_Rate, double rw = 3.0, double skin = 0, double specified_flow_rate = 1000, double specified_BHP = 3000, Type type = Type.Production)
        {
            this.type = type;
            this.type_calculation = well_type_calculation;
            this.rw = rw;
            this.skin = skin;
            this.specified_flow_rate = specified_flow_rate;
            this.specified_BHP = specified_BHP;
        }

        //Method Name: getGeometricFactor
        //Objectives: calculate the geometric factor of the well if it is perfectly centred within a grid block
        //Inputs: a variable of type "GridBlock"
        //Outputs: the value of the geometric factor of the well
        public static double getGeometricFactor(GridBlock block)
        {
            double r_equivalent;
            double Kh;

            if (block.delta_x == block.delta_y && block.Kx == block .Ky)
            {
                r_equivalent = 0.198 * block.delta_x;
            }
            else if (block.delta_x == block.delta_y)
            {
                r_equivalent = 0.14 * Math.Sqrt(Math.Pow(block.delta_x, 2) + Math.Pow(block.delta_y, 2));
            }
            else
            {
                r_equivalent = 0.28 * Math.Sqrt(Math.Sqrt(block.Ky / block.Kx) * Math.Pow(block.delta_x, 2) + Math.Sqrt(block.Kx / block.Ky) * Math.Pow(block.delta_y, 2)) / (Math.Pow((block.Ky / block.Kx), 0.25) + Math.Pow((block.Kx / block.Ky), 0.25));
            }

            Kh = Math.Sqrt(block.Kx * block.Ky);

            double G = 2 * Math.PI * Bc * Kh * block.h / (Math.Log(r_equivalent / (block.rw / 12)) + block.skin);
            return G;
            
        }

        //Method Name:getTransmissibility
        //Objectives: calculate the transmissibility of the fluid in the grid block to the well
        //Inputs: a variable of type "GridBlock", the value of the geometric factor of the well and a variable of type "Phase"
        //Outputs: the value of the transmissibility of the well
        public static double getTransmissibility(GridBlock block, double Geometric_factor, Phase phase)
        {
            double viscosity, FVF, Kr;

            //Assign the values of viscosity, FVF and Kr according to the appropriate phase
            if (phase == Phase.Oil)
            {
                viscosity = block.oil_viscosity;
                FVF = block.Bo;
                Kr = block.Kro;
            }
            else if (phase == Phase.Gas)
            {
                viscosity = block.gas_viscosity;
                FVF = block.Bg;
                Kr = block.Krg;
            }
            else
            {
                viscosity = block.water_viscosity;
                FVF = block.Bw;
                Kr = block.Krw;
            }

            double T = Geometric_factor * Kr / (viscosity * FVF);

            return T;
        }

        public static double calculate_BHP(GridBlock block)
        {
            return block.pressure - (block.well_flow_rate / block.well_transmissibility);
        }
    }
}
