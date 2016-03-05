using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUBOS
{
    class TypeDefinitions
    {
        public enum WellType { Specified_FlowRate, Specified_BHP}
        public enum Phase { Oil, Gas, Water}
        public enum Direction { East, West, North, South, Top, Bottom }
        public enum GridType { Rectangular, cylindrical}
        public enum Compressibility { Compressible, Slightly_Compressible, Incompressible}
        public enum Boundary { Internal_Block, Boundary_Block}
        public enum BlockType { Well_Block, Normal_Block, Inactive_Block}

    }
}
