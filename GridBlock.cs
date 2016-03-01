using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUBOS
{
    class GridBlock
    {
        //Indexing data
        public int counter;
        public int x;
        public int y;
        public int z;

        //if counter is equal to (-1), then there is no block
        //Example: block.east_counter = -1; then block has no east counter
        public int east_counter, west_counter, north_counter, south_counter, top_counter, bottom_counter;

        //Well data
        public enum Type { Normal, Well_Injection, Well_Production, Inactive};
        public enum WellType { Specified_Flow_Rate, Specified_BHP}
        public Type type;
        public WellType well_type;
        public double q_injection;
        public double P_injection;
        public double specified_flow_rate;
        public double q_production;
        public double BHP_minimum;
        public double BHP;
        public double rw;
        public double well_transmissibility;
        public double well_geometric_factor;
        public double drawDown;
        public double qo;
        public double qg;
        public double qg_wo_Rso;
        public double skin;

        //boundary conditions
        public double boundary_q;
        public double boundary_p;
        public double boundary_T;
        public double boundary_gradient;

        //gridding parameters
        public int delta_x;
        public int delta_y;
        public int h;
        public int depth;
        public double r;
        public double theta;

        //Rock properties
        public double porosity;
        public double pore_volume;
        public double bulk_volume;
        public double radius;
        public double Kx;
        public double Ky;
        public double Kz;

        //Compressibility
        public double Cf;
        public double Co;
        public double Cg;
        public double Cw;
        public double Ct;

        //Fluid PVT properties
        public double bubble_point_pressure;
        public double So;
        public double Sg;
        public double Sw;
        public double oil_viscosity;
        public double gas_viscosity;
        public double water_viscosity;
        public double Bo;
        public double Bg;
        public double Bw;
        public double Rso;
        public double Rsw;
        public double oil_density;
        public double gas_density;
        public double water_density;

        //SCAL
        public double Kro;
        public double Krg;
        public double Krw;

        //Pressure
        public double pressure;
        public double potential;


        //Transimissibilities
        //Geometric factors for transmissibility calculations
        public double GF_x;
        public double GF_y;
        public double boundary_GF_x;
        public double boundary_GF_y;

        public double[] TE, TW, TN, TS, TT, TB;
        public double T_East, T_West, T_North, T_South, T_Top, T_Bottom, Transimissibility, T_Oil, T_Gas, T_Water, T_Rso;

        public GridBlock()
        {
            this.type = Type.Normal;
        }
    }
}
