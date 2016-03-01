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

                    int end_of_key = line.IndexOf("=");
                    if (end_of_key >= 0)
                    {
                        string key = line.Substring(0, end_of_key);
                        string value = line.Substring(end_of_key + 1);
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
            Dictionary<string, string> dictionary = ReadFile(file_name);
            SimulatorData simulator_data = new SimulatorData();
            string key;

            if (dictionary != null)
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

                key = "grid_dimensions";
                if (dictionary.ContainsKey(key))
                {
                    string[] grid_dimensions = dictionary[key].Split(',');
                    simulator_data.x = int.Parse(grid_dimensions[0]); simulator_data.y = int.Parse(grid_dimensions[1]); simulator_data.z = int.Parse(grid_dimensions[2]);
                }

                key = "inactive_blocks";
                if (dictionary.ContainsKey(key))
                {
                    simulator_data.inactive_blocks = getArrayFromDictionary_int(dictionary, key);
                }

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

                if (simulator_data.homogeneous)
                {
                    simulator_data.Kx_data = int.Parse(dictionary["Kx"]); simulator_data.Ky_data = int.Parse(dictionary["Ky"]); simulator_data.Kz_data = int.Parse(dictionary["Kz"]);
                    simulator_data.porosity = int.Parse(dictionary["porosity"]);
                    simulator_data.compressibility_rock = int.Parse(dictionary["compressibility_rock"]);
                }
                else
                {
                    List<int> temp_list = new List<int>();
                    List<double> temp_list_double = new List<double>();

                    //Kx values
                    string[] Kx_array = dictionary["Kx"].Split(',');
                    for (int i = 0; i < Kx_array.Length; i++)
                    {
                        temp_list.Add(int.Parse(Kx_array[i]));
                    }
                    simulator_data.Kx_data_array = temp_list.ToArray();
                    //Empty the list before reuse
                    temp_list.Clear();

                    //Ky values
                    string[] Ky_array = dictionary["Ky"].Split(',');
                    for (int i = 0; i < Ky_array.Length; i++)
                    {
                        temp_list.Add(int.Parse(Ky_array[i]));
                    }
                    simulator_data.Ky_data_array = temp_list.ToArray();
                    //Empty the list before reuse
                    temp_list.Clear();

                    //Kz values
                    string[] Kz_array = dictionary["Kz"].Split(',');
                    for (int i = 0; i < Kz_array.Length; i++)
                    {
                        temp_list.Add(int.Parse(Kz_array[i]));
                    }
                    simulator_data.Kz_data_array = temp_list.ToArray();
                    //Empty the list before reuse
                    temp_list.Clear();

                    //Porosity values
                    string[] porosity_array = dictionary["porosity"].Split(',');
                    for (int i = 0; i < porosity_array.Length; i++)
                    {
                        temp_list.Add(int.Parse(porosity_array[i]));
                    }
                    simulator_data.porosity_array = temp_list_double.ToArray();
                    //Empty the list before reuse
                    temp_list_double.Clear();
                }
            }
        }

        public static double[] getArrayFromDictionary_double(Dictionary<string, string> dictionary, string key)
        {
            double[] temp_array;
            List<double> temp_list = new List<double>();

            string[] string_array = dictionary[key].Split(',');
            
            for (int i = 0; i < string_array.Length; i++)
            {
                temp_list.Add(double.Parse(string_array[i]));
            }

            temp_array = temp_list.ToArray();

            return temp_array;
        }

        public static int[] getArrayFromDictionary_int(Dictionary<string, string> dictionary, string key)
        {
            int[] temp_array;
            List<int> temp_list = new List<int>();

            string[] string_array = dictionary[key].Split(',');

            for (int i = 0; i < string_array.Length; i++)
            {
                temp_list.Add(int.Parse(string_array[i]));
            }

            temp_array = temp_list.ToArray();

            return temp_array;
        }
    }
}
