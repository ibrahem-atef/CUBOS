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
                    string[] inactive_blocks = dictionary[key].Split(',');
                    List<int> temp_list = new List<int>();
                    for (int i = 0; i < inactive_blocks.Length; i++)
                    {
                        temp_list.Add(int.Parse(inactive_blocks[i]));
                    }
                    simulator_data.inactive_blocks = temp_list.ToArray();
                }

                if (simulator_data.homogeneous == true)
                {
                    simulator_data.delta_X = int.Parse(dictionary["delta_x"]);
                    simulator_data.delta_Y = int.Parse(dictionary["delta_y"]);
                    simulator_data.delta_Z = int.Parse(dictionary["delta_z"]);
                }
                else
                {

                }
            }
        }
    }
}
