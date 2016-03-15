using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUBOS
{
    //Class Name: DataEntry
    //Objectives: contains the methods required for reading input data files and assigining the values to the appropriate methods for further work
    //How-to-use?: You don't create objects of the type of this class "you don't create instances with -new DataEntry()-", you only use the methods within this class directly
    abstract class DataEntry
    {
        //Method Name: ReadFile
        //Objectives: This method reads the input data file and extracts the keywords along with their corresonding values
        //Inputs: file name
        //Outputs: Dictionary of [key, value] pairs of type strins
        private static Dictionary<string, string> ReadFile(string file_name)
        {
            //a variable to store key-value pairs
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            //Check if the specified file exists
            if (File.Exists(file_name))
            {
                //an array of all the lines
                string[] data_array = File.ReadAllLines(file_name);
                //loop over the lines
                for (var i = 0; i < data_array.Length; i++)
                {
                    //a single line line
                    string line = data_array[i];
                    //Check if the line begins with a "#", then it's a comment and the line should be skipped
                    if (line.Length > 0)
                    {
                        if (line.First() == '#')
                        {
                            continue;
                        }
                    }
                    

                    int end_of_key = line.IndexOf("=");
                    if (end_of_key >= 0)
                    {
                        string key = line.Substring(0, end_of_key);
                        //Remove white space before the "=" sign
                        while (key.Contains(" "))
                        {
                            key = key.Remove(key.IndexOf(' '), 1);
                        }
                        string value = line.Substring(end_of_key + 1);
                        //Remove white space before the "=" sign
                        while (value.Contains(" "))
                        {
                            value = value.Remove(value.IndexOf(' '), 1);
                        }
                        if (!dictionary.ContainsKey(key))
                        {
                            dictionary.Add(key, value);
                        }
                    }
                }

                return dictionary;
            }
            //If the specified data file is not found
            else
            {
                Console.WriteLine("The specified file does not exist. Make sure you entered the path correctly!");
                return null;
            }
        }

        //Method Name: AssignValues
        //Objectives: It gets the data from the ReadFile method and decides on which variables get values and which solvers are used or methods are called depending on these values
        //Inputs: a string to indicate file_name
        //Outputs: a variabe of type "SimulatorData" that contains the data read from the file in a form suitable for the use of the simulator
        public static SimulatorData ReadData(string file_name)
        {
            //a dictionary to store the data read from the file in a key-value pairs format
            Dictionary<string, string> dictionary = ReadFile(file_name + ".txt");
            SimulatorData simulator_data = new SimulatorData();

            if (dictionary.Count > 0)
            {
                // Chek if the input data file exists and it succefully was loaded.
                simulator_data.successfully_loaded_data = true;
                // Sets the file name.
                simulator_data.file_name = file_name;
                // This variable stores the size of the grid according to the numbering system used "active-blocks only, or all blocks".
                int size = 0;
                String key;

                #region Run Specs
                key = "natural_ordering";
                if (dictionary.ContainsKey(key))
                {
                    if (dictionary[key] == "all_blocks")
                    {
                        simulator_data.natural_ordering = SimulatorData.NaturalOrdering.All_Blocks;
                    }
                    else if (dictionary[key] == "active_only")
                    {
                        simulator_data.natural_ordering = SimulatorData.NaturalOrdering.Active_Only;
                    }
                }

                key = "single_phase_compressibility";
                if (dictionary.ContainsKey(key))
                {
                    if (dictionary[key] == "incompressible")
                    {
                        simulator_data.compressibility = TypeDefinitions.Compressibility.Incompressible;
                    }
                    else if (dictionary[key] == "slightly_compressible")
                    {
                        simulator_data.compressibility = TypeDefinitions.Compressibility.Slightly_Compressible;
                    }
                    else if (dictionary[key] == "compressible")
                    {
                        simulator_data.compressibility = TypeDefinitions.Compressibility.Compressible;
                    }
                }

                key = "grid_type";
                if (dictionary.ContainsKey(key))
                {
                    if (dictionary[key] == "rectangular")
                    {
                        simulator_data.grid_type = Transmissibility.GridType.Rectangular;
                    }
                    else if (dictionary[key] == "cylindrical")
                    {
                        simulator_data.grid_type = Transmissibility.GridType.Cylindrical;
                    }

                }

                key = "delta_t";
                if (dictionary.ContainsKey(key))
                {
                    simulator_data.delta_t = double.Parse(dictionary[key]);
                }

                key = "time_max";
                if (dictionary.ContainsKey(key))
                {
                    simulator_data.time_max = double.Parse(dictionary[key]);
                }

                key = "convergence_pressure";
                if (dictionary.ContainsKey(key))
                {
                    simulator_data.convergence_pressure = double.Parse(dictionary[key]);
                }
                #endregion

                #region Output
                key = "what";
                if (dictionary.ContainsKey(key))
                {
                    simulator_data.what = dictionary[key].Split(',');
                }

                key = "where";
                if (dictionary.ContainsKey(key))
                {
                    string temp = dictionary[key];
                    simulator_data.where = temp == "console" ? SinglePhase.OutPut2D.Where.Console : SinglePhase.OutPut2D.Where.File;
                }

                key = "formatted";
                if (dictionary.ContainsKey(key))
                {
                    string temp = dictionary[key];
                    simulator_data.formatted = temp == "yes" ? true : false;
                }

                key = "single_file";
                if (dictionary.ContainsKey(key))
                {
                    string temp = dictionary[key];
                    simulator_data.single_file = temp == "yes" ? true : false;
                }
                #endregion

                #region Grid

                // For a rectangular grid
                if (simulator_data.grid_type == Transmissibility.GridType.Rectangular)
                {
                    key = "homogeneous";
                    if (dictionary.ContainsKey(key))
                    {
                        if (dictionary[key] == "yes")
                        {
                            simulator_data.homogeneous = true;
                        }
                        else
                        {
                            simulator_data.homogeneous = false;
                        }
                    }

                    key = "inactive_blocks";
                    if (dictionary.ContainsKey(key))
                    {
                        simulator_data.inactive_blocks = getArrayFromDictionary_int(dictionary, key);
                    }
                    else
                    {
                        simulator_data.inactive_blocks = new int[0];
                    }

                    key = "grid_dimensions";
                    if (dictionary.ContainsKey(key))
                    {
                        string[] grid_dimensions = dictionary[key].Split(',');
                        simulator_data.x = int.Parse(grid_dimensions[0]); simulator_data.y = int.Parse(grid_dimensions[1]); simulator_data.z = int.Parse(grid_dimensions[2]);
                    }

                    // Set the size of the grid according to the numbering system used "active-blocks only, or all blocks"
                    if (simulator_data.natural_ordering == SimulatorData.NaturalOrdering.All_Blocks)
                    {
                        size = simulator_data.x * simulator_data.y * simulator_data.z;
                    }
                    else
                    {
                        size = simulator_data.x * simulator_data.y * simulator_data.z - simulator_data.inactive_blocks.Length;
                    }
                    //////////////////////////////////////////////////////////////////

                    //Block dimensions
                    if (simulator_data.homogeneous == true)
                    {
                        simulator_data.delta_X = int.Parse(dictionary["delta_x"]);
                        simulator_data.delta_Y = int.Parse(dictionary["delta_y"]);
                        simulator_data.delta_Z = int.Parse(dictionary["delta_z"]);
                    }
                    else
                    {
                        //Delta x values
                        simulator_data.delta_X_array = getArrayFromDictionary_int(dictionary, "delta_x");

                        //Delta y values
                        simulator_data.delta_Y_array = getArrayFromDictionary_int(dictionary, "delta_y");

                        //Heights values
                        simulator_data.delta_Z_array = getArrayFromDictionary_int(dictionary, "delta_z");

                    }
                }
                // For a cylindrical grid
                // Single-well simulations. Always homogeneous rock properties. No in-active blocks
                else
                {

                }
                

                #endregion

                #region Rock Properties

                if (simulator_data.homogeneous)
                {
                    if (dictionary.ContainsKey("Kz"))
                    {
                        //
                        simulator_data.Kz_data = double.Parse(dictionary["Kz"]);
                    }
                    simulator_data.Kx_data = double.Parse(dictionary["Kx"]);
                    simulator_data.Ky_data = double.Parse(dictionary["Ky"]);
                    
                    simulator_data.porosity = double.Parse(dictionary["porosity"]);
                    //Rock Compressiblity
                    key = "compressibility_rock";
                    if (dictionary.ContainsKey(key))
                    {
                        simulator_data.compressibility_rock = double.Parse(dictionary["compressibility_rock"]);
                    }
                }
                else
                {
                    //Kx values
                    simulator_data.Kx_data_array = getArrayFromDictionary_double(dictionary, "Kx");

                    //Ky values
                    simulator_data.Ky_data_array = getArrayFromDictionary_double(dictionary, "Ky");

                    //Kz values
                    if (dictionary.ContainsKey("Kz"))
                    {
                        simulator_data.Kz_data_array = getArrayFromDictionary_double(dictionary, "Kz");
                    }
                    else
                    {
                        simulator_data.Kz_data_array = new double[size];
                    }

                    //Porosity values
                    simulator_data.porosity_array = getArrayFromDictionary_double(dictionary, "porosity");

                    //Rock Compressiblity
                    key = "compressibility_rock";
                    if (dictionary.ContainsKey(key))
                    {
                        simulator_data.compressibility_rock = double.Parse(dictionary["compressibility_rock"]);
                    }
                    
                }

                #endregion

                #region PVT
                key = "FVF";
                if (dictionary.ContainsKey(key))
                {
                    simulator_data.FVF = double.Parse(dictionary[key]);
                }

                key = "viscosity";
                if (dictionary.ContainsKey(key))
                {
                    simulator_data.viscosity = double.Parse(dictionary[key]);
                }

                key = "compressibility_fluid";
                if (dictionary.ContainsKey(key))
                {
                    simulator_data.compressibility_fluid = double.Parse(dictionary[key]);
                }

                key = "g_data_pressure";
                if (dictionary.ContainsKey(key))
                {
                    //pressure
                    simulator_data.g_data[0] = getArrayFromDictionary_double(dictionary, "g_data_pressure");
                    //FVF
                    simulator_data.g_data[1] = getArrayFromDictionary_double(dictionary, "g_data_FVF");
                    //Viscosity
                    simulator_data.g_data[2] = getArrayFromDictionary_double(dictionary, "g_data_viscosity");
                    //Density
                    simulator_data.g_data[3] = getArrayFromDictionary_double(dictionary, "g_data_density");
                }
                #endregion

                #region Boundary Conditions
                key = "initial_pressure";
                if (dictionary.ContainsKey(key))
                {
                    simulator_data.initial_pressure = double.Parse(dictionary[key]);
                }

                key = "boundary_flow_rate";
                if (dictionary.ContainsKey(key))
                {
                    simulator_data.boundary_flow_rate = getArrayFromDictionary_double(dictionary, key);
                }
                else
                {
                    simulator_data.boundary_flow_rate = new double[size];
                }

                key = "boundary_pressure_x";
                if (dictionary.ContainsKey(key))
                {
                    simulator_data.boundary_pressure_x = getArrayFromDictionary_double(dictionary, key);
                }
                else
                {
                    simulator_data.boundary_pressure_x = new double[size];
                }

                key = "boundary_pressure_y";
                if (dictionary.ContainsKey(key))
                {
                    simulator_data.boundary_pressure_y = getArrayFromDictionary_double(dictionary, key);
                }
                else
                {
                    simulator_data.boundary_pressure_y = new double[size];
                }

                key = "boundary_pressure_gradient_x";
                if (dictionary.ContainsKey(key))
                {
                    simulator_data.boundary_pressure_gradient_x = getArrayFromDictionary_double(dictionary, key);
                }
                else
                {
                    simulator_data.boundary_pressure_gradient_x = new double[size];
                }

                key = "boundary_pressure_gradient_y";
                if (dictionary.ContainsKey(key))
                {
                    simulator_data.boundary_pressure_gradient_y = getArrayFromDictionary_double(dictionary, key);
                }
                else
                {
                    simulator_data.boundary_pressure_gradient_y = new double[size];
                }
                #endregion

                #region Well Data
                key = "well_locations";
                if (dictionary.ContainsKey(key))
                {
                    simulator_data.well_locations = getArrayFromDictionary_int(dictionary, key);
                }


                int number_of_wells = simulator_data.well_locations.Length;
                
                simulator_data.well_array = new Well[size];

                for (int i = 1; i <= number_of_wells; i++)
                {
                    Well well = new Well();

                    double[] well_data = getArrayFromDictionary_double(dictionary, "well_" + i);
                    well.rw = well_data[0]; well.skin = well_data[1]; well.specified_BHP = well_data[2]; well.specified_flow_rate = well_data[3];
                    well.type_calculation = well_data[4] == 0 ? Well.TypeCalculation.Specified_BHP : Well.TypeCalculation.Specified_Flow_Rate;

                    int location = simulator_data.well_locations[i - 1];
                    simulator_data.well_array[location] = well;
                }
                #endregion
            }

            return simulator_data;
        }

        //Method Name: getArrayFromDictionary_double
        //Objectives: gets an array of doubles out of a dictionary of key-value pairs of strings in which the key is a single string "Key", and the value is a comma separated strings [value = "double1", "double2", ..]
        //Inputs: the dictionary and the key string
        //Outputs: an array of doubles
        public static double[] getArrayFromDictionary_double(Dictionary<string, string> dictionary, string key)
        {
            double[] temp_array;
            List<double> temp_list = new List<double>();
            string value = dictionary[key];

            if (value.Contains(","))
            {
                string[] string_array = value.Split(',');

                for (int i = 0; i < string_array.Length; i++)
                {
                    temp_list.Add(double.Parse(string_array[i]));
                }
            }
            else
            {
                temp_list.Add(double.Parse(value));
            }

            temp_array = temp_list.ToArray();

            return temp_array;
        }

        //Method Name: getArrayFromDictionary_int
        //Objectives: gets an array of integers out of a dictionary of key-value pairs of strings in which the key is a single string "Key", and the value is a comma separated strings [value = "double1", "double2", ..]
        //Inputs: the dictionary and the key string
        //Outputs: an array of integers
        public static int[] getArrayFromDictionary_int(Dictionary<string, string> dictionary, string key)
        {
            int[] temp_array;
            List<int> temp_list = new List<int>();
            string value = dictionary[key];

            if (value.Contains(","))
            {
                string[] string_array = value.Split(',');

                for (int i = 0; i < string_array.Length; i++)
                {
                    temp_list.Add(int.Parse(string_array[i]));
                }
            }
            else
            {
                temp_list.Add(int.Parse(value));
            }

            temp_array = temp_list.ToArray();

            return temp_array;
        }
    }
}
