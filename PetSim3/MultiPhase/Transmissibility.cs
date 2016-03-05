using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUBOS
{
    //Notes: a variable of type "GridType" to represent if the grid is rectangular or cylindrical
    //Notes: a variable of type "Direction" to represent the direction of flow (x, y or z) for rectangular grids and (r, theta, z) for cylindrical grids
    //Notes: a variable of type "GridBlock" represents an object that contains all the data of a certain block in the grid
    //Notes: a variable of type "Phase" to represent the phase (oil, gas or water) to compute the transmisibility for

    class Transmissibility
    {
        //Constant
        private const double Bc = 0.001127;

        //Declarations of the types of grid and direction
        public enum GridType { Rectangular, Cylindrical}
        public enum Direction { x, y, z, r, theta}
        public enum Phase { Oil, Gas, Water}

        //Method Name: getGeometricFactor
        //Objectives: calculating the geometric factor of the transmissibilities " T = G * 1 / (Viscosity FVF) "
        //Inputs: There are two versions of the method, one that takes a single parameter for homogeneous grids and the other takes two blocks for heterogeneous grids
        //Inputs: ,a variable of type "GridType" and a variable of type "Direction" 
        //Outputs: the methods return a geometric factor

        //Heterogeneous grid
        public static double getGeometricFactor(GridBlock block_1, GridBlock block_2, GridType type, Direction direction)
        {
            double area_1, length_1, permeability_1, area_2, length_2, permeability_2, G;
            //Rectangular grid
            if (type == GridType.Rectangular)
            {
                if (direction == Direction.x)
                {
                    area_1 = block_1.delta_y * block_1.h;
                    length_1 = block_1.delta_x;
                    permeability_1 = block_1.Kx;

                    area_2 = block_2.delta_y * block_2.h;
                    length_2 = block_2.delta_x;
                    permeability_2 = block_2.Kx;
                }
                else if (direction == Direction.y)
                {
                    area_1 = block_1.delta_x * block_1.h;
                    length_1 = block_1.delta_y;
                    permeability_1 = block_1.Ky;

                    area_2 = block_2.delta_x * block_2.h;
                    length_2 = block_2.delta_y;
                    permeability_2 = block_2.Ky;
                }
                else
                {
                    area_1 = block_1.delta_x * block_1.delta_y;
                    length_1 = block_1.h;
                    permeability_1 = block_1.Kz;

                    area_2 = block_2.delta_x * block_2.delta_y;
                    length_2 = block_2.h;
                    permeability_2 = block_2.Kz;
                }

                G = 2 * Bc / (length_1 / (area_1 * permeability_1) + length_2 / (area_2 * permeability_2));
                return G;
            }
            //Cylindrical grid
            else
            {
                //To-Do: implement this method
                return 0;
            }
        }

        //Homogeneous grid
        public static double getGeometricFactor(GridBlock block, GridType type, Direction direction)
        {
            double area, length, permeability, G;
            //Rectangular grid
            if (type == GridType.Rectangular)
            {
                if (direction == Direction.x)
                {
                    area = block.delta_y * block.h;
                    length = block.delta_x;
                    permeability = block.Kx;
                }
                else if (direction == Direction.y)
                {
                    area = block.delta_x * block.h;
                    length = block.delta_y;
                    permeability = block.Ky;
                }
                else
                {
                    area = block.delta_x * block.delta_y;
                    length = block.h;
                    permeability = block.Kz;
                }

                G = Bc * permeability * area / length;
                return G;
            }
            //Cylindrical grid
            else
            {
                //To-Do: implement this method
                return 0;
            }
        }

        //Method Name: getTransmissibility
        //Objectives: calculating the transmissibility between two blocks
        //Inputs: two variables of type "GridBlock", the value of the geometric factor and a variable of type "Phase"
        public static double getTransmissibility(GridBlock block_1, GridBlock block_2, double geometric_factor, Phase phase)
        {
            GridBlock upstream_block;
            double viscosity, FVF, Kr;
            
            //Check for Inactive blocks
            if (block_1.type == GridBlock.Type.Inactive || block_2.type == GridBlock.Type.Inactive)
            {
                return 0;
            }

            //Determine the upstream block
            if (block_1.pressure >= block_2.pressure)
            {
                upstream_block = block_1;
            }
            else
            {
                upstream_block = block_2;
            }

            //Assign the values of viscosity, FVF and Kr according to the appropriate phase
            if (phase == Phase.Oil)
            {
                viscosity = upstream_block.oil_viscosity;
                FVF = upstream_block.Bo;
                Kr = upstream_block.Kro;
            }
            else if (phase == Phase.Gas)
            {
                viscosity = upstream_block.gas_viscosity;
                FVF = upstream_block.Bg;
                Kr = upstream_block.Krg;
            }
            else
            {
                viscosity = upstream_block.water_viscosity;
                FVF = upstream_block.Bw;
                Kr = upstream_block.Krw;
            }

            double T = geometric_factor * Kr / (viscosity * FVF);
            return T;
        }

        //Method_2 Name: getBoundaryTransmissibility
        //Objectives: calculating the transmissibility of a boundary block
        //Inputs: a variable of type "GridBlock", a variable of type "Direction", a variable of type "GridType" and a variiable of type "Phase"
        public static double getBoundaryTransmissibility(GridBlock block, GridType type, Direction direction, Phase phase)
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

            double G = getGeometricFactor(block, type, direction) * 2;
            double T = G * Kr / (viscosity * FVF);

            return T;
        }

    }
}
