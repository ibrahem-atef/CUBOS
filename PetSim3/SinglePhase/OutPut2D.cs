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
        string marks2 = "````````````````````````````````````````````````````````````````````````````````";

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

        //a variable to keep track of the current simulation total time
        private double time;

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
                if (time != -1)
                {
                    Console.WriteLine(marks2);
                    Console.WriteLine(marks2);

                    Console.WriteLine("Current Simulation Time : {0}", time);
                    Console.WriteLine();
                    Console.WriteLine();
                }
                
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
                                    Console.Write(marks + "    ");
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
                                    Console.Write(marks + "    ");
                                    continue;
                                }
                                //property for output
                                Console.Write(block.pressure.ToString().PadRight(16) + "    ");

                                counter += 1;
                            }
                            Console.Write("\r\n");
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
                                    Console.Write(marks + "    ");
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
                                    Console.Write(marks + "    ");
                                    continue;
                                }
                                //property for output
                                if (block.type == GridBlock.Type.Well)
                                {
                                    Console.Write(block.well_flow_rate.ToString().PadRight(16) + "    ");
                                }
                                else
                                {
                                    Console.Write(marks + "    ");
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
                                    Console.Write(marks + "    ");
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
                                    Console.Write(marks + "    ");
                                    continue;
                                }
                                //property for output
                                if (block.type == GridBlock.Type.Well)
                                {
                                    Console.Write(block.pressure.ToString().PadRight(16) + "    ");
                                }
                                else
                                {
                                    Console.Write(marks + "    ");
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
                                    Console.Write(marks + "    ");
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
                                    Console.Write(marks + "    ");
                                    continue;
                                }
                                //property for output
                                if (block.type == GridBlock.Type.Well)
                                {
                                    Console.Write(block.BHP.ToString().PadRight(16) + "    ");
                                }
                                else
                                {
                                    Console.Write(marks + "    ");
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

                    if (true)
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
                            if (a == 0 && time != -1)
                            {
                                file.WriteLine(marks2);
                                file.WriteLine(marks2);

                                file.WriteLine("Current Simulation Time : {0}", time);
                                file.WriteLine();
                            }

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
                                                file.Write(marks + "    ");
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
                                                file.Write(marks + "    ");
                                                continue;
                                            }
                                            //property for output
                                            file.Write(block.pressure.ToString().PadRight(16) + "    ");

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
                                                file.Write(marks + "    ");
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
                                                file.Write(marks + "    ");
                                                continue;
                                            }
                                            //property for output
                                            if (block.type == GridBlock.Type.Well)
                                            {
                                                file.Write(block.well_flow_rate.ToString().PadRight(16) + "    ");
                                            }
                                            else
                                            {
                                                file.Write(marks + "    ");
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
                                                file.Write(marks + "    ");
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
                                                file.Write(marks + "    ");
                                                continue;
                                            }
                                            //property for output
                                            if (block.type == GridBlock.Type.Well)
                                            {
                                                file.Write(block.pressure.ToString().PadRight(16) + "    ");
                                            }
                                            else
                                            {
                                                file.Write(marks + "    ");
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
                                                file.Write(marks + "    ");
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
                                                file.Write(marks + "    ");
                                                continue;
                                            }
                                            //property for output
                                            if (block.type == GridBlock.Type.Well)
                                            {
                                                file.Write(block.BHP.ToString().PadRight(16) + "    ");
                                            }
                                            else
                                            {
                                                file.Write(marks + "    ");
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

                                if (true)
                                {
                                    double error = IMB;
                                    file.WriteLine(error);
                                }
                            }

                            file.WriteLine();
                        }

                    Console.WriteLine("Data was output to the {0}_ouput.txt file for simulation time = {1}", file_name, time);
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
                                        if (counter == (grid_size - x) && time != -1)
                                        {
                                            file.WriteLine(marks2);
                                            file.WriteLine(marks2);

                                            file.Write("Current Simulation Time : {0}", time);
                                            file.WriteLine();
                                            file.WriteLine();

                                        }

                                        //initialize
                                        skip = 0;
                                        if (inactive_blocks.Contains(counter))
                                        {
                                            file.Write(marks + "    ");
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
                                            file.Write(marks + "    ");
                                            continue;
                                        }
                                        //property for output
                                        file.Write(block.pressure.ToString().PadRight(16) + "    ");

                                        counter += 1;
                                    }
                                    file.Write("\r\n");
                                }
                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file for simulation time = {2}", property, file_name, time);
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
                                    if (counter == (grid_size - x) && time != -1)
                                    {
                                        file.WriteLine(marks2);
                                        file.WriteLine(marks2);

                                        file.Write("Current Simulation Time : {0}", time);
                                        file.WriteLine();
                                        file.WriteLine();
                                    }

                                    //this line is used to print blocks in a reversed order so they appear exactly like the naturally ordered blocks
                                    counter = grid_size - k * j * x;
                                    for (int i = 1; i <= x; i++)
                                    {
                                        //initialize
                                        skip = 0;
                                        if (inactive_blocks.Contains(counter))
                                        {
                                            file.Write(marks + "    ");
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
                                            file.Write(marks + "    ");
                                            continue;
                                        }
                                        //property for output
                                        if (block.type == GridBlock.Type.Well)
                                        {
                                            file.Write(block.well_flow_rate.ToString().PadRight(16) + "    ");
                                        }
                                        else
                                        {
                                            file.Write(marks + "    ");
                                        }

                                        counter += 1;
                                    }
                                    file.Write("\r\n");
                                }
                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file for simulation time = {2}", property, file_name, time);
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
                                    if (counter == (grid_size - x) && time != -1)
                                    {
                                        file.WriteLine(marks2);
                                        file.WriteLine(marks2);

                                        file.Write("Current Simulation Time : {0}", time);
                                        file.WriteLine();
                                        file.WriteLine();
                                    }

                                    //this line is used to print blocks in a reversed order so they appear exactly like the naturally ordered blocks
                                    counter = grid_size - k * j * x;
                                    for (int i = 1; i <= x; i++)
                                    {
                                        //initialize
                                        skip = 0;
                                        if (inactive_blocks.Contains(counter))
                                        {
                                            file.Write(marks + "    ");
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
                                            file.Write(marks + "    ");
                                            continue;
                                        }
                                        //property for output
                                        if (block.type == GridBlock.Type.Well)
                                        {
                                            file.Write(block.pressure.ToString().PadRight(16) + "    ");
                                        }
                                        else
                                        {
                                            file.Write(marks + "    ");
                                        }

                                        counter += 1;
                                    }
                                    file.Write("\r\n");
                                }
                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file for simulation time = {2}", property, file_name, time);
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
                                    if (counter == (grid_size - x) && time != -1)
                                    {
                                        file.WriteLine(marks2);
                                        file.WriteLine(marks2);

                                        file.Write("Current Simulation Time : {0}", time);
                                        file.WriteLine();
                                        file.WriteLine();
                                    }

                                    //this line is used to print blocks in a reversed order so they appear exactly like the naturally ordered blocks
                                    counter = grid_size - k * j * x;
                                    for (int i = 1; i <= x; i++)
                                    {
                                        //initialize
                                        skip = 0;
                                        if (inactive_blocks.Contains(counter))
                                        {
                                            file.Write(marks + "    ");
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
                                            file.Write(marks + "    ");
                                            continue;
                                        }
                                        //property for output
                                        if (block.type == GridBlock.Type.Well)
                                        {
                                            file.Write(block.BHP.ToString().PadRight(16) + "    ");
                                        }
                                        else
                                        {
                                            file.Write(marks + "    ");
                                        }

                                        counter += 1;
                                    }
                                    file.Write("\r\n");
                                }
                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file for simulation time = {2}", property, file_name, time);
                    }

                    /////////////////////////////////////////////////////////////////////////
                    property = "MBE";
                    if (what.Contains(property))
                    {
                        using (StreamWriter file = new StreamWriter(file_name + "_" + property + "_output.txt", true))
                            if (true)
                            {
                                if (time != -1)
                                {
                                    file.Write(time.ToString().PadRight(8) + "    ");
                                }

                                double error = IMB;
                                file.Write(error);

                                file.Write("\r\n");
                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file for simulation time = {2}", property, file_name, time);
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
                if (time != -1)
                {
                    Console.WriteLine(marks2);
                    Console.WriteLine(marks2);

                    Console.WriteLine("Current Simulation Time : {0}", time);
                    Console.WriteLine();
                    Console.WriteLine();
                }

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
                            Console.Write(marks + "    ");
                            continue;
                        }
                        //property for output
                        Console.Write(block.pressure.ToString().PadRight(16) + "    ");
                    }

                    Console.Write("\n\n");
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
                            Console.Write(marks + "    ");
                            continue;
                        }
                        else if (block.type != GridBlock.Type.Well)
                        {
                            Console.Write(marks + "    ");
                            continue;
                        }
                        //property for output
                        Console.Write(block.well_flow_rate.ToString().PadRight(16) + "    ");
                    }

                    Console.Write("\n\n");
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
                            Console.Write(marks + "    ");
                            continue;
                        }
                        else if (block.type != GridBlock.Type.Well)
                        {
                            Console.Write(marks + "    ");
                            continue;
                        }
                        //property for output
                        Console.Write(block.pressure.ToString().PadRight(16) + "    ");
                    }

                    Console.Write("\n\n");
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
                            Console.Write(marks + "    ");
                            continue;
                        }
                        else if (block.type != GridBlock.Type.Well)
                        {
                            Console.Write(marks + "    ");
                            continue;
                        }
                        //property for output
                        Console.Write(block.BHP.ToString().PadRight(16) + "    ");
                    }

                    Console.Write("\n\n");
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
                            if (a == 0 && time != -1)
                            {
                                file.WriteLine(marks2);
                                file.WriteLine(marks2);

                                file.WriteLine("Current Simulation Time : {0}", time);
                                file.WriteLine();
                            }

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
                                        file.Write(marks + "    ");
                                        continue;
                                    }
                                    //property for output
                                    file.Write(block.pressure.ToString().PadRight(16) + "    ");
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
                                        file.Write(marks + "    ");
                                        continue;
                                    }
                                    //property for output
                                    if (block.type == GridBlock.Type.Well)
                                    {
                                        file.Write(block.well_flow_rate.ToString().PadRight(16) + "    ");
                                    }
                                    else
                                    {
                                        file.Write(marks + "    ");
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
                                        file.Write(marks + "    ");
                                        continue;
                                    }
                                    //property for output
                                    if (block.type == GridBlock.Type.Well)
                                    {
                                        file.Write(block.pressure.ToString().PadRight(16) + "    ");
                                    }
                                    else
                                    {
                                        file.Write(marks + "    ");
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
                                        file.Write(marks + "    ");
                                        continue;
                                    }
                                    //property for output
                                    if (block.type == GridBlock.Type.Well)
                                    {
                                        file.Write(block.BHP.ToString().PadRight(16) + "    ");
                                    }
                                    else
                                    {
                                        file.Write(marks + "    ");
                                    }
                                }

                                file.WriteLine();
                            }
                            /////////////////////////////////////////////////////////////////////////
                            else if (property.Contains("MBE"))
                            {
                                file.WriteLine("Material Balance Error :");
                                file.WriteLine();

                                if (true)
                                {
                                    double error = IMB;
                                    file.WriteLine(error);
                                }
                            }

                            file.WriteLine();
                        }

                    Console.WriteLine("Data was output to the {0}_ouput.txt file for simulation time = {1}", file_name, time);
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
                                if (time == 0 && i == 0 && i == 0)
                                {
                                    file.Write("Time".PadRight(8) );
                                    for (int c = 0; c < grid.Length; c++)
                                    {
                                        string temp = "GridBlock " + c;
                                        file.Write(temp.PadRight(16) + "    ");
                                    }
                                    file.Write("\r\n");
                                    file.Write("\r\n");

                                }

                                if (i == 0 && time != -1)
                                {
                                    //file.WriteLine();
                                    file.Write(time.ToString().PadRight(8));
                                }

                                block = grid[i];
                                //omit inactive blocks
                                if (block.type == GridBlock.Type.Inactive)
                                {
                                    file.Write(marks + "    ");
                                    continue;
                                }
                                //property for output
                                file.Write(block.pressure.ToString().PadRight(16) + "    ");

                                if (i == grid.Length - 1) file.Write("\r\n");
                                
                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file for simulation time = {2}", property, file_name, time);
                    }

                    /////////////////////////////////////////////////////////////////////////
                    property = "well_rate";
                    if (what.Contains(property))
                    {
                        using (StreamWriter file = new StreamWriter(file_name + "_" + property + "_output.txt", true))
                            for (int i = 0; i < grid.Length; i++)
                            {
                                if (time == 0 && i == 0)
                                {
                                    file.Write("Time".PadRight(8) );
                                    for (int c = 0; c < grid.Length; c++)
                                    {
                                        if (grid[c].type == GridBlock.Type.Well)
                                        {
                                            string temp = "GridBlock " + c;
                                            file.Write(temp.PadRight(16) + "    ");
                                        }
                                    }
                                    file.Write("\r\n");
                                    file.Write("\r\n");

                                }

                                if (i == 0 && time != -1)
                                {
                                    file.Write(time.ToString().PadRight(8));
                                }

                                block = grid[i];
                                //omit inactive blocks
                                if (block.type == GridBlock.Type.Inactive)
                                {
                                    //file.Write(marks + "    ");
                                    continue;
                                }
                                //property for output
                                if (block.type == GridBlock.Type.Well)
                                {
                                    file.Write(block.well_flow_rate.ToString().PadRight(16) + "    ");
                                }
                                else
                                {
                                    //file.Write(marks + "    ");
                                }

                                if (i == grid.Length - 1) file.Write("\r\n");

                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file for simulation time = {2}", property, file_name, time);
                    }

                    /////////////////////////////////////////////////////////////////////////
                    property = "well_block_pressure";
                    if (what.Contains(property))
                    {
                        using (StreamWriter file = new StreamWriter(file_name + "_" + property + "_output.txt", true))
                            for (int i = 0; i < grid.Length; i++)
                            {
                                if (time == 0 && i == 0)
                                {
                                    file.Write("Time".PadRight(8) );
                                    for (int c = 0; c < grid.Length; c++)
                                    {
                                        if (grid[c].type == GridBlock.Type.Well)
                                        {
                                            string temp = "GridBlock " + c;
                                            file.Write(temp.PadRight(16) + "    ");
                                        }
                                    }
                                    file.Write("\r\n");
                                    file.Write("\r\n");

                                }

                                if (i == 0 && time != -1)
                                {
                                    file.Write(time.ToString().PadRight(8));
                                }

                                block = grid[i];
                                //omit inactive blocks
                                if (block.type == GridBlock.Type.Inactive)
                                {
                                    //file.Write(marks + "    ");
                                    continue;
                                }
                                //property for output
                                if (block.type == GridBlock.Type.Well)
                                {
                                    file.Write(block.pressure.ToString().PadRight(16) + "    ");
                                }
                                else
                                {
                                    //file.Write(marks + "    ");
                                }

                                if (i == grid.Length - 1) file.Write("\r\n");

                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file for simulation time = {2}", property, file_name, time);
                    }

                    /////////////////////////////////////////////////////////////////////////
                    property = "BHP";
                    if (what.Contains(property))
                    {
                        using (StreamWriter file = new StreamWriter(file_name + "_" + property + "_output.txt", true))
                            for (int i = 0; i < grid.Length; i++)
                            {
                                if (time == 0 && i == 0)
                                {
                                    file.Write("Time".PadRight(8) );
                                    for (int c = 0; c < grid.Length; c++)
                                    {
                                        string temp = "GridBlock " + c;
                                        file.Write(temp.PadRight(16) + "    ");
                                    }
                                    file.Write("\r\n");
                                    file.Write("\r\n");

                                }

                                if (i == 0 && time != -1)
                                {
                                    file.Write(time.ToString().PadRight(8));
                                }

                                block = grid[i];
                                //omit inactive blocks
                                if (block.type == GridBlock.Type.Inactive)
                                {
                                    file.Write(marks + "    ");
                                    continue;
                                }
                                //property for output
                                if (block.type == GridBlock.Type.Well)
                                {
                                    file.Write(block.BHP.ToString().PadRight(16) + "    ");
                                }
                                else
                                {
                                    file.Write(marks + "    ");
                                }

                                if (i == grid.Length - 1) file.Write("\r\n");

                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file for simulation time = {2}", property, file_name, time);
                    }

                    /////////////////////////////////////////////////////////////////////////
                    property = "MBE";
                    if (what.Contains(property))
                    {
                        using (StreamWriter file = new StreamWriter(file_name + "_" + property + "_output.txt", true))
                            if (true)
                            {
                                if (time != -1)
                                {
                                    file.Write(time.ToString().PadRight(8));
                                }

                                double error = IMB;
                                file.Write(error);

                                file.WriteLine();
                            }

                        Console.WriteLine("{0} was output to the {1}_{0}_ouput.txt file for simulation time = {2}", property, file_name, time);
                    }
                }
            }
            #endregion
        }

        private void writeNoFormat()
        {
            writeNoFormat(this.where);
        }

        public void write(double IMB, double time = -1)
        {
            this.time = time;

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
