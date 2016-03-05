using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUBOS
{
    class RectangularBlockNumbering
    {
        //Defines an enumeration of the directions
        public enum Direction { East, West, North, South, Top, Bottom }
        //Defines an enumeration of different numbering schemes
        public enum NumberingScheme { Active_Only, All}

        //Method Name: assignGridOrdering
        //Objectives: gives each block in the grid a number according to the numbering scheme used
        //Inputs: an array of the (x, y, z) grid dimensions and an array of the Inactive blocks numberings
        //Ouputs: an array of GridBlocks that are given numbers according to the numbering scheme used
        public static GridBlock[] assignGridOrdering(int[] grid_dimensions, int[] inactive_blocks, NumberingScheme scheme)
        {
            //variables to store the grid dimensions
            int x, y, z;
            //store the grid dimensions
            x = grid_dimensions[0]; y = grid_dimensions[1]; z = grid_dimensions[2];
            //an array to store a block's i, j and k coordinates within the grid according to the engineering notation
            int[] block_coordinates = new int[3];
            //A variable to store block's data
            GridBlock block;
            //A variable representing the grid array of blocks
            GridBlock[] grid = new GridBlock[0];
            if (scheme == NumberingScheme.All)
            {
                //Grid array size is equal to all the block "including in-active blocks"
                grid = new GridBlock[x * y * z];
            }
            else if (scheme == NumberingScheme.Active_Only)
            {
                //Grid array size is equal to the number of active-only blocks
                grid = new GridBlock[x * y * z - inactive_blocks.Length];
            }
            

            //A general loop counter
            int counter = 0;
            //A block counter
            //This variable is used only with the "assignBlockOrdering_ActiveOnly" method to keep track of the number of active blocks added to the grid
            int block_counter = 0;

            //A loop to iterate over all the blocks in the grid according to the engineering notation
            for (int k = 0; k < z; k++)
            {
                for (int j = 0; j < y; j++)
                {
                    for (int i = 0; i < x; i++)
                    {
                        block = new GridBlock();
                        //assign the values of the block coordinates
                        block.x = i; block.y = j; block.z = k;

                        //set block type "either Inactive or normal"
                        if (inactive_blocks.Contains(counter))
                        {
                            block.type = GridBlock.Type.Inactive;
                        }

                        //set the block coordinates array that will be passed to other methods
                        block_coordinates[0] = i; block_coordinates[1] = j; block_coordinates[2] = k;

                        //###########################################################################################

                        if (scheme == NumberingScheme.All)
                        {
                            //Natural ordering for the entire grid "both active and in-active blocks"
                            RectangularBlockNumbering.assignBlockOrdering_Natural(block, counter, block_coordinates, grid_dimensions, inactive_blocks);
                        }
                        else if (scheme == NumberingScheme.Active_Only)
                        {
                            //Natural ordering for the grid "active blocks only"
                            if (block.type == GridBlock.Type.Inactive)
                            {
                                //increment the general loop counter and skip this block
                                counter += 1;
                                continue;
                            }

                            RectangularBlockNumbering.assignBlockOrdering_ActiveOnly(block, counter, block_counter, block_coordinates, grid_dimensions, inactive_blocks);
                        }

                        //###########################################################################################

                        //increment counter at the end of the loop
                        counter += 1;
                        //this statement adds the block to the grid array
                        grid[block_counter] = block;
                        //only increment block_counter when a block is added to the grid to keep track of how many blocks have been added
                        block_counter += 1;
                    }
                }
            }

            return grid;
        }

        //Method Name: assignBlockOrdering_Natural
        //Objectives: this method is used for each block "both active and in-active" in the grid to assign its numbering and the numbering of neighbouring blocks
        //Inputs: a variable of type "GridBlock" that contains all the data of the block, a general counter of the loop, array of block coordinates, array of grid dimensions and an array of the natural ordering of the in-active blocks
        //Ouputs: N/A. This is an internal method that calculates the numbering and assign it to the variable "block" that is passed to this method
        private static void assignBlockOrdering_Natural(GridBlock block, int counter, int[] block_dimensions, int[] grid_diemnsions, int[] inactive_blocks)
        {
            int i, j, k;
            i = block_dimensions[0]; j = block_dimensions[1]; k = block_dimensions[2];

            int x, y, z;
            x = grid_diemnsions[0]; y = grid_diemnsions[1]; z = grid_diemnsions[2];

            block.counter = counter;
            block.east_counter = i < (x - 1) ? counter + 1 : -1;
            block.west_counter = i > 0 ? counter - 1 : -1;
            block.north_counter = j < (y - 1) ? counter + x : -1;
            block.south_counter = j > 0 ? counter - x : -1;
            block.top_counter = k > 0 ? (k - 1) : -1;
            block.bottom_counter = k < (z - 1) ? (k + 1) : -1;
        }

        //Method Name: assignBlockOrdering_ActiveOnly
        //Objectives: this method is used for each "active block only" in the grid to assign its numbering and the numbering of neighbouring blocks
        //Inputs: a variable of type "GridBlock" that contains all the data of the block, a general counter of the loop, block counter, array of block coordinates, array of grid dimensions and an array of the natural ordering of the in-active blocks
        //Ouputs: N/A. This is an internal method that calculates the numbering and assign it to the variable "block" that is passed to this method
        private static void assignBlockOrdering_ActiveOnly(GridBlock block, int counter, int block_counter, int[] block_coordinates, int[] grid_diemnsions, int[] inactive_blocks)
        {
            int i, j, k;
            i = block_coordinates[0]; j = block_coordinates[1]; k = block_coordinates[2];

            int x, y, z;
            x = grid_diemnsions[0]; y = grid_diemnsions[1]; z = grid_diemnsions[2];

            block.counter = block_counter;
            block.east_counter = getNextBlockCounter(Direction.East, block_counter, counter, inactive_blocks, grid_diemnsions, block_coordinates);
            block.west_counter = getNextBlockCounter(Direction.West, block_counter, counter, inactive_blocks, grid_diemnsions, block_coordinates);
            block.north_counter = getNextBlockCounter(Direction.North, block_counter, counter, inactive_blocks, grid_diemnsions, block_coordinates);
            block.south_counter = getNextBlockCounter(Direction.South, block_counter, counter, inactive_blocks, grid_diemnsions, block_coordinates);
            block.top_counter = getNextBlockCounter(Direction.Top, block_counter, counter, inactive_blocks, grid_diemnsions, block_coordinates);
            block.bottom_counter = getNextBlockCounter(Direction.Bottom, block_counter, counter, inactive_blocks, grid_diemnsions, block_coordinates);
        }

        //Method Name: getNextBlockCounter
        //Objectives: gets the block counter "a number that indicates position in an array" after converting a grid of "active and non-active" blocks to a grid of only active blocks
        //Inputs: a variable of type "Direction", the counter of the current block in the modified grid, the counter of the block in the original "active non-active" grid
        //Inputs: the values of the grid dimensions "x, y, z" and the values of the current block position "k, j, k" in the grid initialization for loop
        //Outputs: the counter of the block in the specified direction in the new modified "active only" array of blocks
        private static int getNextBlockCounter(Direction direction, int block_counter, int counter, int[] inactive_blocks, int[] grid_dimensions, int[] block_coordinates)
        {
            int i, j, k;
            i = block_coordinates[0]; j = block_coordinates[1]; k = block_coordinates[2];

            int x, y, z;
            x = grid_dimensions[0]; y = grid_dimensions[1]; z = grid_dimensions[2];

            if (direction == Direction.East)
            {
                if (i == (x - 1)) return -1;

                int temp = counter + 1;
                temp = inactive_blocks.Contains(temp) ? -1 : temp;
                if (temp != -1)
                {
                    return block_counter + 1;
                }
                return temp;
            }
            else if (direction == Direction.West)
            {
                if (i == 0) return -1;

                int temp = counter - 1;
                temp = inactive_blocks.Contains(temp) ? -1 : temp;
                if (temp != -1)
                {
                    return block_counter - 1;
                }
                return temp;
            }
            else if (direction == Direction.North)
            {
                if (j == (y - 1)) return -1;

                int temp = counter + x;
                temp = inactive_blocks.Contains(temp) ? -1 : temp;

                int skip = 0;
                for (int a = 0; a < inactive_blocks.Length; a++)
                {
                    if (temp > inactive_blocks[a])
                    {
                        skip += 1;
                    }
                    else
                    {
                        break;
                    }
                }

                return temp - skip;
            }
            else if (direction == Direction.South)
            {
                if (j == 0) return -1;

                int temp = counter - x;
                temp = inactive_blocks.Contains(temp) ? -1 : temp;

                int skip = 0;
                for (int a = 0; a < inactive_blocks.Length; a++)
                {
                    if (temp > inactive_blocks[a])
                    {
                        skip += 1;
                    }
                    else
                    {
                        break;
                    }
                }

                return temp - skip;
            }
            else if (direction == Direction.Top)
            {
                if (k == 0) return -1;

                int layer_size = x * y;

                int temp = counter - layer_size;
                temp = inactive_blocks.Contains(temp) ? -1 : temp;

                int skip = 0;
                for (int a = 0; a < inactive_blocks.Length; a++)
                {
                    if (temp > inactive_blocks[a])
                    {
                        skip += 1;
                    }
                    else
                    {
                        break;
                    }
                }

                return temp - skip;
            }
            else if (direction == Direction.Bottom)
            {
                if (k == (z - 1)) return -1;

                int layer_size = x * y;

                int temp = counter + layer_size;
                temp = inactive_blocks.Contains(temp) ? -1 : temp;

                int skip = 0;
                for (int a = 0; a < inactive_blocks.Length; a++)
                {
                    if (temp > inactive_blocks[a])
                    {
                        skip += 1;
                    }
                    else
                    {
                        break;
                    }
                }

                return temp - skip;
            }

            return -1;
        }
    }
}
