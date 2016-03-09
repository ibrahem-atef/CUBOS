using CUBOS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinglePhase
{
    class OutPut2D
    {
        //this string indicates an inactive block or a block that does not contain the property to be output
        string marks = "****************";

        //used to keep track of inactive blocks
        int skip;

        //an array of the numbers of in-active grid blocks "use to output an active-only grid in a format that shows inactive blocks as well"
        int[] inactive_blocks;

        //an array to store the properties that need to be output
        string[] what;
        //Define an enumeration of the output devices
        public enum Where { Console, File}
        //a variable of type "Where" to store the output device
        Where where;

        //variables to store grid dimensions "to output data in a format that resembles the reservoir shape"
        int x, y, z;

        //an array of elements of type "GridBlock" to store the grid data
        GridBlock[] grid;
        int grid_size;
        //
        GridBlock block;

        //a variable to store the type of fluid compressibility for use with MBE output
        TypeDefinitions.Compressibility compressibility;

        //a variable to store the file_name to store the output in
        string file_name;

        //output format
        bool formatted = false;
        bool single_file = true;
        bool over_write = true;

        //a variable for storing and retrieving the value for IMB
        private double IMB;

        //A public constructor of the class to initialize the variables
        public OutPut2D(GridBlock[] grid, int[] grid_dimensions, string[] what, Where where, TypeDefinitions.Compressibility compressibility, string file_name, bool formatted, bool single_file, int[] inactive_blocks)
        {
            this.grid = grid;
            this.what = what;
            this.where = where;
            this.x = grid_dimensions[0]; this.y = grid_dimensions[1]; this.z = grid_dimensions[2];
            this.compressibility = compressibility;
            this.file_name = file_name;
            this.formatted = formatted;
            this.single_file = single_file;
            this.inactive_blocks = inactive_blocks;
            grid_size = x * y * z;
            //over_writes the output file at the beginning of the simulation only once
            //it doesn't over-write the file ine subsequen time-steps during the same simulation run
            if (where == Where.File)
            {
                overWriteFiles(single_file, ref over_write);
            }
        }

        private void overWriteFiles(bool one_file, ref bool over_write)
        {
            if (over_write)
            {
                if (one_file)
                {
                    string path = file_name + "_output.txt";
                    using (StreamWriter file = new StreamWriter(path, false)) ;
                }
                else
                {
                    string path;

                    for (int i = 0; i < what.Length; i++)
                    {
                        path = file_name + "_" + what[i] + "_output.txt";
                        using (StreamWriter file = new StreamWriter(path, false)) ;
                    }
                }
                over_write = false;
            }
        }

        private void writeFormatted(Where where)
        {
            #region Console
            //output to the console
            if (where == Where.Console)
            {
                /////////////////////////////////////////////////////////////////////////
                if (what.Contains("pressure"))
                {
                    Console.WriteLine("Pressure :");
                    Console.WriteLine();

                    int counter = 0;
                    for (int k = 1; k <= z; k++)
                    {
                        for (int j = 1; j <= y; j++)
                        {
                            //this line is used to print blocks in a reversed order so they appear exactly like the naturally ordered blocks
                            counter = grid_size - k * j * x;
                            for (int i = 1; i <= x; i++)
                            {
                                //initialize
                                skip = 0;
                                if (inactive_blocks.Contains(counter))
                                {
                                    Console.Write(marks + "\t");
                                    counter += 1;
                                    continue;
                                }
                                for (int b = 0; b < inactive_blocks.Length; b++)
                                {
                                    if (inactive_blocks[b] < counter)
                                    {
                                        skip += 1;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                block = grid[counter - skip];
                                //omit inactive blocks
                                if (block.type == GridBlock.Type.Inactive)
                                {
                                    Console.Write(marks + "\t");
                                    continue;
                                }
                                //property for output
                                Console.Write(block.pressure + "\t");

                                counter += 1;
                            }
                            Console.Write("\n");
                        }
                    }

                    Console.WriteLine();
                }

                /////////////////////////////////////////////////////////////////////////
                if (what.Contains("well_rate"))
                {
                    Console.WriteLine("Well Rates :");
                    Console.WriteLine();

                    int counter = 0;
                    for (int k = 1; k <= z; k++)
                    {
                        for (int j = 1; j <= y; j++)
                        {
                            //this line is used to print blocks in a reversed order so they appear exactly like the naturally ordered blocks
                            counter = grid_size - k * j * x;
                            for (int i = 1; i <= x; i++)
                            {
                                //initialize
                                skip = 0;
                                if (inactive_blocks.Contains(counter))
                                {
                                    Console.Write(marks + "\t");
                                    counter += 1;
                                    continue;
                                }
                                for (int b = 0; b < inactive_blocks.Length; b++)
                                {
                                    if (inactive_blocks[b] < counter)
                                    {
                                        skip += 1;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                block = grid[counter - skip];
                                //omit inactive blocks
                                if (block.type == GridBlock.Type.Inactive)
                                {
                                    Console.Write(marks + "\t");
                                    continue;
                                }
                                //property for output
                                if (block.type == GridBlock.Type.Well)
                                {
                                    Console.Write(block.well_flow_rate.ToString().PadRight(16) + "\t");
                                }
                                else
                                {
                                    Console.Write(marks + "\t");
                                }

                                counter += 1;
                            }
                            Console.Write("\n");
                        }
                    }

                    Console.WriteLine();
                }

                /////////////////////////////////////////////////////////////////////////
                if (what.Contains("well_block_pressure"))
                {
                    Console.WriteLine("well_block_pressure :");
                    Console.WriteLine();

                    int counter = 0;
                    for (int k = 1; k <= z; k++)
                    {
                        for (int j = 1; j <= y; j++)
                        {
                            //this line is used to print blocks in a reversed order so they appear exactly like the naturally ordered blocks
                            counter = grid_size - k * j * x;
                            for (int i = 1; i <= x; i++)
                            {
                                //initialize
                                skip = 0;
                                if (inactive_blocks.Contains(counter))
                                {
                                    Console.Write(marks + "\t");
                                    counter += 1;
                                    continue;
                                }
                                for (int b = 0; b < inactive_blocks.Length; b++)
                                {
                                    if (inactive_blocks[b] < counter)
                                    {
                                        skip += 1;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                block = grid[counter - skip];
                                //omit inactive blocks
                                if (block.type == GridBlock.Type.Inactive)
                                {
                                    Console.Write(marks + "\t");
                                    continue;
                                }
                                //property for output
                                if (block.type == GridBlock.Type.Well)
                                {
                                    Console.Write(block.pressure + "\t");
                                }
                                else
                                {
                                    Console.Write(marks + "\t");
                                }

                                counter += 1;
                            }
                            Console.Write("\n");
                        }
                    }

                    Console.WriteLine();
                }
                /////////////////////////////////////////////////////////////////////////
                if (what.Contains("BHP"))
                {
                    Console.WriteLine("Bottom Hole Flowing Pressure :");
                    Console.WriteLine();

                    int counter = 0;
                    for (int k = 1; k <= z; k++)
                    {
                        for (int j = 1; j <= y; j++)
                        {
                            //this line is used to print blocks in a reversed order so they appear exactly like the naturally ordered blocks
                            counter = grid_size - k * j * x;
                            for (int i = 1; i <= x; i++)
                            {
                                //initialize
                                skip = 0;
                                if (inactive_blocks.Contains(counter))
                                {
                                    Console.Write(marks + "\t");
                                    counter += 1;
                                    continue;
                                }
                                for (int b = 0; b < inactive_blocks.Length; b++)
                                {
                                    if (inactive_blocks[b] < counter)
                                    {
                                        skip += 1;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                block = grid[counter - skip];
                                //omit inactive blocks
                                if (block.type == GridBlock.Type.Inactive)
                                {
                                    Console.Write(marks + "\t");
                                    continue;
                                }
                                //property for output
                                if (block.type == GridBlock.Type.Well)
                                {
                                    Console.Write(block.BHP.ToString().PadRight(16) + "\t");
                                }
                                else
                                {
                                    Console.Write(marks + "\t");
                                }

                                counter += 1;
                            }
                            Console.Write("\n");
                        }
                    }

                    Console.WriteLine();
                }

                /////////////////////////////////////////////////////////////////////////
                if (what.Contains("MBE"))
                {
                    Console.WriteLine("Material Balance Error :");
                    Console.WriteLine();

                    if (compressibility == TypeDefinitions.Compressibility.Incompressible)
                    {
                        double error = IMB;
                        Console.WriteLine(error);
                    }
                }

                Console.WriteLine();
            }
            #endregion

            #region File
            //output to a file
            else
            {
                if (single_file)
                {
                    using (StreamWriter file = new StreamWriter(file_name + "_output.txt", true))
                        for (int a = 0; a < what.Length; a++)
                        {
                            string property = what[a];
                            /////////////////////////////////////////////////////////////////////////
                            if (property == "pressure")
                            {
                                file.WriteLine("Pressure :");
                                file.WriteLine();

                                int counter = 0;
                                for (int k = 1; k <= z; k++)
                                {
                                    for (int j = 1; j <= y; j++)
                                    {
                                        //this line is used to print blocks in a reversed order so they appear exactly like the naturally ordered blocks
                                        counter = grid_size - k * j * x;
                                        for (int i = 1; i <= x; i++)
                                        {
                                            //initialize
                                            skip = 0;
                                            if (inactive_blocks.Contains(counter))
                                            {
                                                file.Write(marks + "\t");
                                                counter += 1;
                                                continue;
                                            }
                                            for (int b = 0; b < inactive_blocks.Length; b++)
                                            {
                                                if (inactive_blocks[b] < counter)
                                                {
                                                    skip += 1;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }

                                            block = grid[counter - skip];
                                            //omit inactive blocks
                                            if (block.type == GridBlock.Type.Inactive)
                                            {
                                                file.Write(marks + "\t");
                                                continue;
                                            }
                                            //property for output
                                            file.Write(block.pressure + "\t");

                                            counter += 1;
                                        }
                                        file.Write("\r\n");
                                    }
                                }

                                file.WriteLine();
                                file.WriteLine();
                            }
                            /////////////////////////////////////////////////////////////////////////
                            else if (property.Contains("well_rate"))
                            {
                                file.WriteLine("Well Rates :");
                                file.WriteLine();

                                int counter = 0;
                                for (int k = 1; k <= z; k++)
                                {
                                    for (int j = 1; j <= y; j++)
                                    {
                                        //this line is used to print blocks in a reversed order so they appear exactly like the naturally ordered blocks
                                        counter = grid_size - k * j * x;
                                        for (int i = 1; i <= x; i++)
                                        {
                                            //initialize
                                            skip = 0;
                                            if (inactive_blocks.Contains(counter))
                                            {
                                                file.Write(marks + "\t");
                                                counter += 1;
                                                continue;
                                            }
                                            for (int b = 0; b < inactive_blocks.Length; b++)
                                            {
                                                if (inactive_blocks[b] < counter)
                                                {
                                                    skip += 1;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }

                                            block = grid[counter - skip];
                                            //omit inactive blocks
                                            if (block.type == GridBlock.Type.Inactive)
                                            {
                                                file.Write(marks + "\t");
                                                continue;
                                            }
                                            //property for output
                                            if (block.type == GridBlock.Type.Well)
                                            {
                                                file.Write(block.well_flow_rate.ToString().PadRight(16) + "\t");
                                            }
                                            else
                                            {
                                                file.Write(marks + "\t");
                                            }

                                            counter += 1;
                                        }
                                        file.Write("\r\n");
                                    }
                                }

                                file.WriteLine();
                                file.WriteLine();
                            }
                            /////////////////////////////////////////////////////////////////////////
                            else if (property.Contains("well_block_pressure"))
                            {
                                file.WriteLine("well_block_pressure :");
                                file.WriteLine();

                                int counter = 0;
                                for (int k = 1; k <= z; k++)
                                {
                                    for (int j = 1; j <= y; j++)
                                    {
                                        //this line is used to print blocks in a reversed order so they appear exactly like the naturally ordered blocks
                                        counter = grid_size - k * j * x;
                                        for (int i = 1; i <= x; i++)
                                        {
                                            //initialize
                                            skip = 0;
                                            if (inactive_blocks.Contains(counter))
                                            {
                                                file.Write(marks + "\t");
                                                counter += 1;
                                                continue;
                                            }
                                            for (int b = 0; b < inactive_blocks.Length; b++)
                                            {
                                                if (inactive_blocks[b] < counter)
                                                {
                                                    skip += 1;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }

                                            block = grid[counter - skip];
                                            //omit inactive blocks
                                            if (block.type == GridBlock.Type.Inactive)
                                            {
                                                file.Write(marks + "\t");
                                                continue;
                                            }
                                            //property for output
                                            if (block.type == GridBlock.Type.Well)
                                            {
                                                file.Write(block.pressure + "\t");
                                            }
                                            else
                                            {
                                                file.Write(marks + "\t");
                                            }

                                            counter += 1;
                                        }
                                        file.Write("\r\n");
                                    }
                                }

                                file.WriteLine();
                                file.WriteLine();
                            }
                            /////////////////////////////////////////////////////////////////////////
                            else if (property.Contains("BHP"))
                            {
                                file.WriteLine("Bottom Hole Flowing Pressure :");
                                file.WriteLine();

                                int counter = 0;
                                for (int k = 1; k <= z; k++)
                                {
                                    for (int j = 1; j <= y; j++)
                                    {
                                        //this line is used to print blocks in a reversed order so they appear exactly like the naturally ordered blocks
                                        counter = grid_size - k * j * x;
                                        for (int i = 1; i <= x; i++)
                                        {
                                            //initialize
                                            skip = 0;
                                            if (inactive_blocks.Contains(counter))
                                            {
                                                file.Write(marks + "\t");
                                                counter += 1;
                                                continue;
                                            }
                                            for (int b = 0; b < inactive_blocks.Length; b++)
                                            {
                                                if (inactive_blocks[b] < counter)
                                                {
                                                    skip += 1;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }

                                            block = grid[counter - skip];
                                            //omit inactive blocks
                                            if (block.type == GridBlock.Type.Inactive)
                                            {
                                                file.Write(marks + "\t");
                                                continue;
                                            }
                                            //property for output
                                            if (block.type == GridBlock.Type.Well)
                                            {
                                                file.Write(block.BHP.ToString().PadRight(16) + "\t");
                                            }
                                            else
                                            {
                                                file.Write(marks + "\t");
                                            }

                                            counter += 1;
                                        }
                                        file.Write("\r\n");
                                    }
                                }

                                file.WriteLine();
                                file.WriteLine();
                            }
                            /////////////////////////////////////////////////////////////////////////
                            else if (property.Contains("MBE"))
                            {
                                file.WriteLine("Material Balance Error :");
                                file.WriteLine();

                                if (compressibility == TypeDefinitions.Compressibility.Incompressible)
                                {
                                    double error = IMB;
                                    file.WriteLine(error);
                                }
                            }
                        }

                    Console.WriteLine("Data was output to the {0}_ouput.txt file", file_name);
                }
                else
                {
                    /////////////////////////////////////////////////////////////////////////
                    string property = "pressure";
                    if (what.Contains(property))
                    {
                        int counter = 0;

                        using (StreamWriter file = new StreamWriter(file_name + "_" + property + "_output.txt", true))
                            for (int k = 1; k <= z; k++)
                            {
                                for (int j = 1; j <= y; j++)
                                {
                                    //this line is used to print blocks in a reversed order so they appear exactly like the naturally ordered blocks
                                    counter = grid_size - k * j * x;
                                    for (int i = 1; i <= x; i++)
                                    {
                                        //initialize
                                        skip = 0;
                                        if (inactive_blocks.Contains(counter))
                                        {
                                            file.Write(marks + "\t");
                                            counter += 1;
                                            continue;
                                        }
                                        for (int b = 0; b < inactive_blocks.Length; b++)
                                        {
                                            if (inactive_blocks[b] < counter)
                                            {
                                                skip += 1;
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }

                                        block = grid[counter - skip];
                                        //omit inactive blocks
                                        if (block.type == GridBlock.Type.Inactive)
                                        {
                                            file.Write(marks + "\t");
                                            continue;
                                        }
                                        //property for output
                                        file.Write(block.pressure + "\t");

                                        counter += 1;
                                    }
                                    file.Write("\r\n");
                                }
                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file", property, file_name);
                    }

                    /////////////////////////////////////////////////////////////////////////
                    property = "well_rate";
                    if (what.Contains(property))
                    {
                        int counter = 0;

                        using (StreamWriter file = new StreamWriter(file_name + "_" + property + "_output.txt", true))
                            for (int k = 1; k <= z; k++)
                            {
                                for (int j = 1; j <= y; j++)
                                {
                                    //this line is used to print blocks in a reversed order so they appear exactly like the naturally ordered blocks
                                    counter = grid_size - k * j * x;
                                    for (int i = 1; i <= x; i++)
                                    {
                                        //initialize
                                        skip = 0;
                                        if (inactive_blocks.Contains(counter))
                                        {
                                            file.Write(marks + "\t");
                                            counter += 1;
                                            continue;
                                        }
                                        for (int b = 0; b < inactive_blocks.Length; b++)
                                        {
                                            if (inactive_blocks[b] < counter)
                                            {
                                                skip += 1;
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }

                                        block = grid[counter - skip];
                                        //omit inactive blocks
                                        if (block.type == GridBlock.Type.Inactive)
                                        {
                                            file.Write(marks + "\t");
                                            continue;
                                        }
                                        //property for output
                                        if (block.type == GridBlock.Type.Well)
                                        {
                                            file.Write(block.well_flow_rate.ToString().PadRight(16) + "\t");
                                        }
                                        else
                                        {
                                            file.Write(marks + "\t");
                                        }

                                        counter += 1;
                                    }
                                    file.Write("\r\n");
                                }
                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file", property, file_name);
                    }

                    /////////////////////////////////////////////////////////////////////////
                    property = "well_block_pressure";
                    if (what.Contains(property))
                    {
                        int counter = 0;

                        using (StreamWriter file = new StreamWriter(file_name + "_" + property + "_output.txt", true))
                            for (int k = 1; k <= z; k++)
                            {
                                for (int j = 1; j <= y; j++)
                                {
                                    //this line is used to print blocks in a reversed order so they appear exactly like the naturally ordered blocks
                                    counter = grid_size - k * j * x;
                                    for (int i = 1; i <= x; i++)
                                    {
                                        //initialize
                                        skip = 0;
                                        if (inactive_blocks.Contains(counter))
                                        {
                                            file.Write(marks + "\t");
                                            counter += 1;
                                            continue;
                                        }
                                        for (int b = 0; b < inactive_blocks.Length; b++)
                                        {
                                            if (inactive_blocks[b] < counter)
                                            {
                                                skip += 1;
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }

                                        block = grid[counter - skip];
                                        //omit inactive blocks
                                        if (block.type == GridBlock.Type.Inactive)
                                        {
                                            file.Write(marks + "\t");
                                            continue;
                                        }
                                        //property for output
                                        if (block.type == GridBlock.Type.Well)
                                        {
                                            file.Write(block.pressure + "\t");
                                        }
                                        else
                                        {
                                            file.Write(marks + "\t");
                                        }

                                        counter += 1;
                                    }
                                    file.Write("\r\n");
                                }
                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file", property, file_name);
                    }

                    /////////////////////////////////////////////////////////////////////////
                    property = "BHP";
                    if (what.Contains(property))
                    {
                        int counter = 0;

                        using (StreamWriter file = new StreamWriter(file_name + "_" + property + "_output.txt", true))
                            for (int k = 1; k <= z; k++)
                            {
                                for (int j = 1; j <= y; j++)
                                {
                                    //this line is used to print blocks in a reversed order so they appear exactly like the naturally ordered blocks
                                    counter = grid_size - k * j * x;
                                    for (int i = 1; i <= x; i++)
                                    {
                                        //initialize
                                        skip = 0;
                                        if (inactive_blocks.Contains(counter))
                                        {
                                            file.Write(marks + "\t");
                                            counter += 1;
                                            continue;
                                        }
                                        for (int b = 0; b < inactive_blocks.Length; b++)
                                        {
                                            if (inactive_blocks[b] < counter)
                                            {
                                                skip += 1;
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }

                                        block = grid[counter - skip];
                                        //omit inactive blocks
                                        if (block.type == GridBlock.Type.Inactive)
                                        {
                                            file.Write(marks + "\t");
                                            continue;
                                        }
                                        //property for output
                                        if (block.type == GridBlock.Type.Well)
                                        {
                                            file.Write(block.BHP.ToString().PadRight(16) + "\t");
                                        }
                                        else
                                        {
                                            file.Write(marks + "\t");
                                        }

                                        counter += 1;
                                    }
                                    file.Write("\r\n");
                                }
                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file", property, file_name);
                    }

                    /////////////////////////////////////////////////////////////////////////
                    property = "MBE";
                    if (what.Contains(property))
                    {
                        using (StreamWriter file = new StreamWriter(file_name + "_" + property + "_output.txt", true))
                            if (compressibility == TypeDefinitions.Compressibility.Incompressible)
                            {
                                double error = IMB;
                                file.WriteLine(error);
                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file", property, file_name);
                    }
                }
            }
            #endregion
        }

        private void writeFormatted()
        {
            writeFormatted(this.where);
        }

        private void writeNoFormat(Where where)
        {
            #region Console
            //output to the console
            if (where == Where.Console)
            {
                /////////////////////////////////////////////////////////////////////////
                if (what.Contains("pressure"))
                {
                    Console.WriteLine("Pressure :");
                    Console.WriteLine();

                    for (int i = 0; i < grid.Length; i++)
                    {
                        block = grid[i];
                        //omit inactive blocks
                        if (block.type == GridBlock.Type.Inactive)
                        {
                            Console.Write(marks + "\t");
                            continue;
                        }
                        //property for output
                        Console.Write(block.pressure + "\t");
                    }

                    Console.Write("\n");
                }

                /////////////////////////////////////////////////////////////////////////
                if (what.Contains("well_rate"))
                {
                    Console.WriteLine("Well Rates :");
                    Console.WriteLine();

                    for (int i = 0; i < grid.Length; i++)
                    {
                        block = grid[i];
                        //omit inactive blocks
                        if (block.type == GridBlock.Type.Inactive)
                        {
                            Console.Write(marks + "\t");
                            continue;
                        }
                        //property for output
                        Console.Write(block.well_flow_rate.ToString().PadRight(16) + "\t");
                    }

                    Console.Write("\n");
                }

                /////////////////////////////////////////////////////////////////////////
                if (what.Contains("well_block_pressure"))
                {
                    Console.WriteLine("Well Block Pressure :");
                    Console.WriteLine();

                    for (int i = 0; i < grid.Length; i++)
                    {
                        block = grid[i];
                        //omit inactive blocks
                        if (block.type == GridBlock.Type.Inactive)
                        {
                            Console.Write(marks + "\t");
                            continue;
                        }
                        //property for output
                        Console.Write(block.pressure + "\t");
                    }

                    Console.Write("\n");
                }

                /////////////////////////////////////////////////////////////////////////
                if (what.Contains("BHP"))
                {
                    Console.WriteLine("Bottom Hole Flowing Pressure :");
                    Console.WriteLine();

                    for (int i = 0; i < grid.Length; i++)
                    {
                        block = grid[i];
                        //omit inactive blocks
                        if (block.type == GridBlock.Type.Inactive)
                        {
                            Console.Write(marks + "\t");
                            continue;
                        }
                        //property for output
                        Console.Write(block.BHP.ToString().PadRight(16) + "\t");
                    }

                    Console.Write("\n");
                }

                /////////////////////////////////////////////////////////////////////////
                if (what.Contains("MBE"))
                {
                    Console.WriteLine("Material Balance Error :");
                    Console.WriteLine();

                    double error = IMB;
                    Console.WriteLine(error);
                }

                Console.WriteLine();
            }
            #endregion

            #region File
            //output to a file
            else
            {
                if (single_file)
                {
                    using (StreamWriter file = new StreamWriter(file_name + "_output.txt", true))
                        for (int a = 0; a < what.Length; a++)
                        {
                            string property = what[a];
                            /////////////////////////////////////////////////////////////////////////
                            if (property == "pressure")
                            {
                                file.WriteLine("Pressure :");
                                file.WriteLine();

                                for (int i = 0; i < grid.Length; i++)
                                {
                                    block = grid[i];
                                    //omit inactive blocks
                                    if (block.type == GridBlock.Type.Inactive)
                                    {
                                        file.Write(marks + "\t");
                                        continue;
                                    }
                                    //property for output
                                    file.Write(block.pressure + "\t");
                                }

                                file.WriteLine();
                            }
                            /////////////////////////////////////////////////////////////////////////
                            else if (property.Contains("well_rate"))
                            {
                                file.WriteLine("Well Rates :");
                                file.WriteLine();

                                for (int i = 0; i < grid.Length; i++)
                                {
                                    block = grid[i];
                                    //omit inactive blocks
                                    if (block.type == GridBlock.Type.Inactive)
                                    {
                                        file.Write(marks + "\t");
                                        continue;
                                    }
                                    //property for output
                                    if (block.type == GridBlock.Type.Well)
                                    {
                                        file.Write(block.well_flow_rate.ToString().PadRight(16) + "\t");
                                    }
                                    else
                                    {
                                        file.Write(marks + "\t");
                                    }
                                }

                                file.WriteLine();
                            }
                            /////////////////////////////////////////////////////////////////////////
                            else if (property.Contains("well_block_pressure"))
                            {
                                file.WriteLine("Well Block Pressure :");
                                file.WriteLine();

                                for (int i = 0; i < grid.Length; i++)
                                {
                                    block = grid[i];
                                    //omit inactive blocks
                                    if (block.type == GridBlock.Type.Inactive)
                                    {
                                        file.Write(marks + "\t");
                                        continue;
                                    }
                                    //property for output
                                    if (block.type == GridBlock.Type.Well)
                                    {
                                        file.Write(block.pressure + "\t");
                                    }
                                    else
                                    {
                                        file.Write(marks + "\t");
                                    }
                                }

                                file.WriteLine();
                            }
                            /////////////////////////////////////////////////////////////////////////
                            else if (property.Contains("BHP"))
                            {
                                file.WriteLine("Bottom Hole Flowing Pressure :");
                                file.WriteLine();

                                for (int i = 0; i < grid.Length; i++)
                                {
                                    block = grid[i];
                                    //omit inactive blocks
                                    if (block.type == GridBlock.Type.Inactive)
                                    {
                                        file.Write(marks + "\t");
                                        continue;
                                    }
                                    //property for output
                                    if (block.type == GridBlock.Type.Well)
                                    {
                                        file.Write(block.BHP.ToString().PadRight(16) + "\t");
                                    }
                                    else
                                    {
                                        file.Write(marks + "\t");
                                    }
                                }

                                file.WriteLine();
                            }
                            /////////////////////////////////////////////////////////////////////////
                            else if (property.Contains("MBE"))
                            {
                                file.WriteLine("Material Balance Error :");
                                file.WriteLine();

                                if (compressibility == TypeDefinitions.Compressibility.Incompressible)
                                {
                                    double error = IMB;
                                    file.WriteLine(error);
                                }
                            }

                            file.WriteLine();
                        }

                    Console.WriteLine("Data was output to the {0}_ouput.txt file", file_name);
                }
                else
                {
                    /////////////////////////////////////////////////////////////////////////
                    string property = "pressure";
                    if (what.Contains(property))
                    {
                        using (StreamWriter file = new StreamWriter(file_name + "_" + property + "_output.txt", true))
                            for (int i = 0; i < grid.Length; i++)
                            {
                                block = grid[i];
                                //omit inactive blocks
                                if (block.type == GridBlock.Type.Inactive)
                                {
                                    file.Write(marks + "\t");
                                    continue;
                                }
                                //property for output
                                file.Write(block.pressure + "\t");
                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file", property, file_name);
                    }

                    /////////////////////////////////////////////////////////////////////////
                    property = "well_rate";
                    if (what.Contains(property))
                    {
                        using (StreamWriter file = new StreamWriter(file_name + "_" + property + "_output.txt", true))
                            for (int i = 0; i < grid.Length; i++)
                            {
                                block = grid[i];
                                //omit inactive blocks
                                if (block.type == GridBlock.Type.Inactive)
                                {
                                    file.Write(marks + "\t");
                                    continue;
                                }
                                //property for output
                                if (block.type == GridBlock.Type.Well)
                                {
                                    file.Write(block.well_flow_rate.ToString().PadRight(16) + "\t");
                                }
                                else
                                {
                                    file.Write(marks + "\t");
                                }
                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file", property, file_name);
                    }

                    /////////////////////////////////////////////////////////////////////////
                    property = "well_block_pressure";
                    if (what.Contains(property))
                    {
                        using (StreamWriter file = new StreamWriter(file_name + "_" + property + "_output.txt", true))
                            for (int i = 0; i < grid.Length; i++)
                            {
                                block = grid[i];
                                //omit inactive blocks
                                if (block.type == GridBlock.Type.Inactive)
                                {
                                    file.Write(marks + "\t");
                                    continue;
                                }
                                //property for output
                                if (block.type == GridBlock.Type.Well)
                                {
                                    file.Write(block.pressure + "\t");
                                }
                                else
                                {
                                    file.Write(marks + "\t");
                                }
                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file", property, file_name);
                    }

                    /////////////////////////////////////////////////////////////////////////
                    property = "BHP";
                    if (what.Contains(property))
                    {
                        using (StreamWriter file = new StreamWriter(file_name + "_" + property + "_output.txt", true))
                            for (int i = 0; i < grid.Length; i++)
                            {
                                block = grid[i];
                                //omit inactive blocks
                                if (block.type == GridBlock.Type.Inactive)
                                {
                                    file.Write(marks + "\t");
                                    continue;
                                }
                                //property for output
                                if (block.type == GridBlock.Type.Well)
                                {
                                    file.Write(block.BHP.ToString().PadRight(16) + "\t");
                                }
                                else
                                {
                                    file.Write(marks + "\t");
                                }
                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file", property, file_name);
                    }

                    /////////////////////////////////////////////////////////////////////////
                    property = "MBE";
                    if (what.Contains(property))
                    {
                        using (StreamWriter file = new StreamWriter(file_name + "_" + property + "_output.txt", true))
                            if (compressibility == TypeDefinitions.Compressibility.Incompressible)
                            {
                                double error = IMB;
                                file.WriteLine(error);
                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file", property, file_name);
                    }
                }
            }
            #endregion
        }

        private void writeNoFormat()
        {
            writeNoFormat(this.where);
        }

        public void write(double IMB)
        {
            this.IMB = IMB;

            if (formatted)
            {
                writeFormatted();
            }
            else
            {
                writeNoFormat();
            }
        }
    }
}
