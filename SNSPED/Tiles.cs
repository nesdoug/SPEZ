namespace SNSPED
{
    public static class Tiles
    {
        public static int[] Tile_Arrays = new int[2 * 256 * 8 * 8]; // 32768 
                                                                    // 2 sets, 256 tiles, 8 high, 8 wide
                                                                    // 4 sets 4bpp allow values 0-15
        public static int[] Undo_Tile_Array = new int[2 * 256 * 8 * 8];
        public static int[] Tile_Copier = new int[8 * 8]; // one tile
        public static bool Has_Copied = false;


        public static int[] trans_array = new int[16384]; // linear
        public static int[] copy_array = new int[16384];
        public static int trans_x, trans_y; // in tiles
        public static int trans_w = 1; // in tiles
        public static int trans_h = 1;
        public static int copy_x, copy_y; // in tiles
        public static int copy_w = 1; // in tiles
        public static int copy_h = 1;


        // you know, I regret the way I set up the tile array
        // and I think a linear format woud be easier to
        // work with for large transformations
        // this converts from tile array to a linear format
        public static void Tiles_2_Linear()
        {
            //in - globals trans_x, trans_y, trans_w, trans_h

            int start_x = trans_x * 8;
            int final_x = (trans_w * 8) + start_x;
            int start_y = trans_y * 8;
            int final_y = (trans_h * 8) + start_y;
            // should be values 0-127

            int offset = Form1.tile_set * 16384;

            for(int y1 = start_y; y1 < final_y; y1++)
            {
                for (int x1 = start_x; x1 < final_x; x1++)
                {
                    int tile_xH = x1 >> 3;
                    int tile_xL = x1 & 7;
                    int tile_yH = y1 >> 3;
                    int tile_yL = y1 & 7;
                    int index1 = (tile_yH * 1024) + (tile_xH * 64) + (tile_yL * 8) + tile_xL;
                    index1 += offset;
                    int index2 = (y1 * 128) + x1;
                    trans_array[index2] = Tile_Arrays[index1];
                }
            }

        }


        // this converts the linear / transformation array
        // back to the old tile format
        public static void Linear_2_Tiles()
        {
            //in - globals trans_x, trans_y, trans_w, trans_h

            int start_x = trans_x * 8;
            int final_x = (trans_w * 8) + start_x;
            int start_y = trans_y * 8;
            int final_y = (trans_h * 8) + start_y;
            // should be values 0-127

            int offset = Form1.tile_set * 16384;

            for (int y1 = start_y; y1 < final_y; y1++)
            {
                for (int x1 = start_x; x1 < final_x; x1++)
                {
                    int tile_xH = x1 >> 3;
                    int tile_xL = x1 & 7;
                    int tile_yH = y1 >> 3;
                    int tile_yL = y1 & 7;
                    int index1 = (tile_yH * 1024) + (tile_xH * 64) + (tile_yL * 8) + tile_xL;
                    index1 += offset;
                    int index2 = (y1 * 128) + x1;
                    Tile_Arrays[index1] = trans_array[index2];
                }
            }
        }


        public static void shift_left()
        {
            if(Form1.BIG_EDIT_MODE == false)
            {
                int z = (Form1.tile_set * 256 * 8 * 8) + (Form1.tile_num * 8 * 8); // base index
                for (int y = 0; y < 8; y++)
                {
                    int temp = Tile_Arrays[z + (y * 8)]; // save the left most
                    for (int x = 0; x < 7; x++)
                    {
                        Tile_Arrays[z + (y * 8) + x] = Tile_Arrays[z + (y * 8) + x + 1];
                    }
                    Tile_Arrays[z + (y * 8) + 7] = temp; // put it on the right
                }
            }
            else // BIG_EDIT_MODE
            {
                Tiles_2_Linear();

                int start_x = trans_x * 8;
                int final_x = (trans_w * 8) + start_x;
                int start_y = trans_y * 8;
                int final_y = (trans_h * 8) + start_y;

                for (int y1 = start_y; y1 < final_y; y1++)
                {
                    int index = (y1 * 128) + start_x;
                    int temp = trans_array[index];
                    for (int x1 = start_x; x1 < final_x - 1; x1++)
                    {
                        index = (y1 * 128) + x1;
                        trans_array[index] = trans_array[index + 1];
                    }
                    index = (y1 * 128) + final_x - 1;
                    trans_array[index] = temp;
                }

                Linear_2_Tiles();
            }
            
        }

        public static void shift_right()
        {
            if (Form1.BIG_EDIT_MODE == false)
            {
                int z = (Form1.tile_set * 256 * 8 * 8) + (Form1.tile_num * 8 * 8); // base index
                for (int y = 0; y < 8; y++)
                {
                    int temp = Tile_Arrays[z + (y * 8) + 7]; // save the right most
                    for (int x = 6; x >= 0; x--)
                    {
                        Tile_Arrays[z + (y * 8) + x + 1] = Tile_Arrays[z + (y * 8) + x];
                    }
                    Tile_Arrays[z + (y * 8)] = temp; // put it on the left
                }
            }
            else
            {
                Tiles_2_Linear();

                int start_x = trans_x * 8;
                int final_x = (trans_w * 8) + start_x;
                int start_y = trans_y * 8;
                int final_y = (trans_h * 8) + start_y;

                for (int y1 = start_y; y1 < final_y; y1++)
                {
                    int index = (y1 * 128) + final_x - 1;
                    int temp = trans_array[index];
                    for (int x1 = final_x - 2; x1 >= start_x; x1--)
                    {
                        index = (y1 * 128) + x1;
                        trans_array[index + 1] = trans_array[index];
                    }
                    index = (y1 * 128) + start_x;
                    trans_array[index] = temp;
                }

                Linear_2_Tiles();
            }
            
        }

        public static void shift_up()
        {
            if (Form1.BIG_EDIT_MODE == false)
            {
                int z = (Form1.tile_set * 256 * 8 * 8) + (Form1.tile_num * 8 * 8); // base index
                for (int x = 0; x < 8; x++)
                {
                    int temp = Tile_Arrays[z + x]; // save the top most
                    for (int y = 0; y < 7; y++)
                    {
                        Tile_Arrays[z + (y * 8) + x] = Tile_Arrays[z + ((y + 1) * 8) + x];
                    }
                    Tile_Arrays[z + 56 + x] = temp; // put it on the bottom
                }
            }
            else
            {
                Tiles_2_Linear(); 
                
                int start_x = trans_x * 8;
                int final_x = (trans_w * 8) + start_x;
                int start_y = trans_y * 8;
                int final_y = (trans_h * 8) + start_y;

                for(int x1 = start_x; x1 < final_x; x1++)
                {
                    int index = (start_y * 128) + x1;
                    int temp = trans_array[index]; 
                    for (int y1 = start_y; y1 < final_y - 1; y1++)
                    {
                        index = (y1 * 128) + x1;
                        trans_array[index] = trans_array[index + 128];
                    }
                    index = ((final_y - 1) * 128) + x1;
                    trans_array[index] = temp;
                }

                Linear_2_Tiles();
            }
            
        }

        public static void shift_down()
        {
            if (Form1.BIG_EDIT_MODE == false)
            {
                int z = (Form1.tile_set * 256 * 8 * 8) + (Form1.tile_num * 8 * 8); // base index
                for (int x = 0; x < 8; x++)
                {
                    int temp = Tile_Arrays[z + 56 + x]; // save the bottom most
                    for (int y = 6; y >= 0; y--)
                    {
                        Tile_Arrays[z + ((y + 1) * 8) + x] = Tile_Arrays[z + (y * 8) + x];
                    }
                    Tile_Arrays[z + x] = temp; // put it on the top
                }
            }
            else
            {
                Tiles_2_Linear();

                int start_x = trans_x * 8;
                int final_x = (trans_w * 8) + start_x;
                int start_y = trans_y * 8;
                int final_y = (trans_h * 8) + start_y;

                for (int x1 = start_x; x1 < final_x; x1++)
                {
                    int index = ((final_y - 1) * 128) + x1;
                    int temp = trans_array[index];
                    for (int y1 = final_y - 2; y1 >= start_y; y1--)
                    {
                        index = (y1 * 128) + x1;
                        trans_array[index + 128] = trans_array[index];
                    }
                    index = (start_y * 128) + x1;
                    trans_array[index] = temp;
                }

                Linear_2_Tiles();
            }
            
        }

        public static void tile_copy()
        {
            if (Form1.BIG_EDIT_MODE == false)
            {
                int z = (Form1.tile_set * 256 * 8 * 8) + (Form1.tile_num * 8 * 8); // base index
                for (int x = 0; x < 64; x++)
                {
                    Tile_Copier[x] = Tile_Arrays[z + x];
                }
                Has_Copied = true;
            }
            else
            {
                // just copy the entire set
                int offset = Form1.tile_set * 16384;
                for(int i = 0; i < 16384; i++)
                {
                    copy_array[i] = Tile_Arrays[i + offset];
                }
                // remember the bounds
                copy_x = trans_x;
                copy_y = trans_y;
                copy_w = trans_w;
                copy_h = trans_h;

                Has_Copied = true;
            }
            
        }

        public static void tile_paste()
        {
            if (Has_Copied == true)
            {
                if (Form1.BIG_EDIT_MODE == false)
                {
                    int z = (Form1.tile_set * 256 * 8 * 8) + (Form1.tile_num * 8 * 8); // base index
                    for (int x = 0; x < 64; x++)
                    {
                        Tile_Arrays[z + x] = Tile_Copier[x];
                    }
                }
                else
                {
                    // copy_x, copy_y, copy_w, copy_h = bounds
                    // copy_array[] = where, usual tile format
                    int offset = Form1.tile_set * 16384;

                    // 0, 64, 128, etc.
                    // 1024

                    for(int y1 = 0; y1 < copy_h; y1++) // in tiles
                    {
                        for (int x1 = 0; x1 < copy_w; x1++)
                        {
                            if ((Form1.tile_x + x1) >= 16) continue; // no wrapping
                            if ((Form1.tile_y + y1) >= 16) continue; // no overflow
                            int src_index = ((copy_y + y1) * 1024) + ((copy_x + x1) * 64);
                            int dest_index = ((Form1.tile_y + y1) * 1024) + ((Form1.tile_x + x1) * 64);
                            if (dest_index >= 16384) continue; // no overflow
                            dest_index += offset;

                            //copy tile by tile, which is 64 values
                            for (int i = 0; i < 64; i++)
                            {
                                Tile_Arrays[dest_index + i] = copy_array[src_index + i];
                            }
                        }
                    }
                }

            }
        }

        public static void tile_delete()
        {
            if (Form1.BIG_EDIT_MODE == false)
            {
                int z = (Form1.tile_set * 256 * 8 * 8) + (Form1.tile_num * 8 * 8); // base index
                for (int x = 0; x < 64; x++)
                {
                    Tile_Arrays[z + x] = 0;
                }
            }
            else
            {
                Tiles_2_Linear();

                int start_x = trans_x * 8;
                int final_x = (trans_w * 8) + start_x;
                int start_y = trans_y * 8;
                int final_y = (trans_h * 8) + start_y;

                for (int y1 = start_y; y1 < final_y; y1++)
                {
                    for (int x1 = start_x; x1 < final_x; x1++)
                    {
                        int index = (y1 * 128) + x1;
                        trans_array[index] = 0;
                    }
                }

                Linear_2_Tiles();
            }
            
        }

        public static void tile_h_flip()
        {
            if (Form1.BIG_EDIT_MODE == false)
            {
                int z = (Form1.tile_set * 256 * 8 * 8) + (Form1.tile_num * 8 * 8); // base index
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        int temp = Tile_Arrays[z + (y * 8) + x];
                        Tile_Arrays[z + (y * 8) + x] = Tile_Arrays[z + (y * 8) + (7 - x)];
                        Tile_Arrays[z + (y * 8) + (7 - x)] = temp;
                    }
                }
            }
            else
            {
                Tiles_2_Linear();

                int start_x = trans_x * 8;
                int final_x = (trans_w * 8) + start_x;
                int start_y = trans_y * 8;
                int final_y = (trans_h * 8) + start_y;
                int mid_x = trans_w * 4;

                for (int y1 = start_y; y1 < final_y; y1++)
                {
                    for(int x1 = 0; x1 < mid_x; x1++)
                    {
                        int left_x = start_x + x1;
                        int right_x = (final_x - x1) - 1;
                        int index1 = (y1 * 128) + left_x;
                        int index2 = (y1 * 128) + right_x;
                        int temp = trans_array[index1];
                        trans_array[index1] = trans_array[index2];
                        trans_array[index2] = temp;
                    }
                }

                Linear_2_Tiles();
            }
            
        }

        public static void tile_v_flip()
        {
            if (Form1.BIG_EDIT_MODE == false)
            {
                int z = (Form1.tile_set * 256 * 8 * 8) + (Form1.tile_num * 8 * 8); // base index
                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        int temp = Tile_Arrays[z + (y * 8) + x];
                        Tile_Arrays[z + (y * 8) + x] = Tile_Arrays[z + ((7 - y) * 8) + x];
                        Tile_Arrays[z + ((7 - y) * 8) + x] = temp;
                    }
                }
            }
            else
            {
                Tiles_2_Linear();

                int start_x = trans_x * 8;
                int final_x = (trans_w * 8) + start_x;
                int start_y = trans_y * 8;
                int final_y = (trans_h * 8) + start_y;
                int mid_y = trans_h * 4;


                for (int x1 = start_x; x1 < final_x; x1++)
                {
                    for(int y1 = 0; y1 < mid_y; y1++)
                    {
                        int top_y = start_y + y1;
                        int low_y = (final_y - y1) - 1;
                        int index1 = (top_y * 128) + x1;
                        int index2 = (low_y * 128) + x1;
                        int temp = trans_array[index1];
                        trans_array[index1] = trans_array[index2];
                        trans_array[index2] = temp;
                    }
                }

                Linear_2_Tiles();
            }
            
        }

        public static void tile_rot_cw() // R, rotate clockwise
        {
            if (Form1.BIG_EDIT_MODE == false)
            {
                int z = (Form1.tile_set * 256 * 8 * 8) + (Form1.tile_num * 8 * 8); // base index
                int[] temp_arr = new int[64];
                int count = 0;
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 7; y >= 0; y--)
                    {
                        temp_arr[count++] = Tile_Arrays[z + (y * 8) + x];
                    }
                }
                for (int i = 0; i < 64; i++)
                {
                    Tile_Arrays[z + i] = temp_arr[i];
                }
            }
            else
            {
                Tiles_2_Linear();

                // make it a square, or else this function will mangle it
                if (trans_w > trans_h) 
                {
                    trans_w = trans_h;
                    Form1.BE_x2 = Form1.BE_x1 + trans_w;
                }
                if(trans_h > trans_w)
                {
                    trans_h = trans_w;
                    Form1.BE_y2 = Form1.BE_y1 + trans_h;
                }

                int start_x = trans_x * 8;
                int final_x = (trans_w * 8) + start_x;
                int start_y = trans_y * 8;
                int final_y = (trans_h * 8) + start_y;
                int mid_x = trans_w * 4;
                int mid_y = trans_h * 4;

                for (int y1 = 0; y1 < mid_y; y1++)
                {
                    for(int x1 = 0; x1 < mid_x; x1++)
                    {
                        int index1, index2, index3, index4;
                        int y2 = start_y + y1;
                        int x2 = start_x + x1;
                        index1 = (y2 * 128) + x2;
                        int temp = trans_array[index1];

                        int y3 = final_y - x1 - 1;
                        int x3 = start_x + y1;
                        index3 = (y3 * 128) + x3;
                        trans_array[index1] = trans_array[index3];

                        y2 = final_y - y1 - 1;
                        x2 = final_x - x1 - 1;
                        index4 = (y2 * 128) + x2;
                        trans_array[index3] = trans_array[index4];

                        y3 = start_y + x1;
                        x3 = final_x - y1 - 1;
                        index2 = (y3 * 128) + x3;
                        trans_array[index4] = trans_array[index2];

                        trans_array[index2] = temp;

                    }
                }

                Linear_2_Tiles();
            }
            
        }

        public static void tile_rot_ccw() // L, rotate counter clockwise
        {
            if (Form1.BIG_EDIT_MODE == false)
            {
                int z = (Form1.tile_set * 256 * 8 * 8) + (Form1.tile_num * 8 * 8); // base index
                int[] temp_arr = new int[64];
                int count = 0;
                for (int x = 7; x >= 0; x--)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        temp_arr[count++] = Tile_Arrays[z + (y * 8) + x];
                    }
                }
                for (int i = 0; i < 64; i++)
                {
                    Tile_Arrays[z + i] = temp_arr[i];
                }
            }
            else
            {
                Tiles_2_Linear();

                // make it a square, or else this function will mangle it
                if (trans_w > trans_h)
                {
                    trans_w = trans_h;
                    Form1.BE_x2 = Form1.BE_x1 + trans_w;
                }
                if (trans_h > trans_w)
                {
                    trans_h = trans_w;
                    Form1.BE_y2 = Form1.BE_y1 + trans_h;
                }

                int start_x = trans_x * 8;
                int final_x = (trans_w * 8) + start_x;
                int start_y = trans_y * 8;
                int final_y = (trans_h * 8) + start_y;
                int mid_x = trans_w * 4;
                int mid_y = trans_h * 4;

                for (int y1 = 0; y1 < mid_y; y1++)
                {
                    for (int x1 = 0; x1 < mid_x; x1++)
                    {
                        int index1, index2, index3, index4;
                        int y2 = start_y + y1;
                        int x2 = start_x + x1;
                        index1 = (y2 * 128) + x2;
                        int temp = trans_array[index1];

                        int y3 = start_y + x1;
                        int x3 = final_x - y1 - 1;
                        index2 = (y3 * 128) + x3;
                        trans_array[index1] = trans_array[index2];

                        y2 = final_y - y1 - 1;
                        x2 = final_x - x1 - 1;
                        index4 = (y2 * 128) + x2;
                        trans_array[index2] = trans_array[index4];

                        y3 = final_y - x1 - 1;
                        x3 = start_x + y1;
                        index3 = (y3 * 128) + x3;
                        trans_array[index4] = trans_array[index3];

                        trans_array[index3] = temp;
                    }
                }

                Linear_2_Tiles();
            }
            
        }

        public static void tile_fill()
        { // fill with currently selected color.
            if (Form1.BIG_EDIT_MODE == false)
            {
                int z = (Form1.tile_set * 256 * 8 * 8) + (Form1.tile_num * 8 * 8); // base index
                int color = Form1.pal_x;
                
                for (int x = 0; x < 64; x++)
                {
                    Tile_Arrays[z + x] = color;
                }
            }
            else
            {
                Tiles_2_Linear();

                int start_x = trans_x * 8;
                int final_x = (trans_w * 8) + start_x;
                int start_y = trans_y * 8;
                int final_y = (trans_h * 8) + start_y;

                int color = Form1.pal_x;

                for (int y1 = start_y; y1 < final_y; y1++)
                {
                    for (int x1 = start_x; x1 < final_x; x1++)
                    {
                        int index = (y1 * 128) + x1;
                        trans_array[index] = color;
                    }
                }

                Linear_2_Tiles();
            }
                
        }


        public static void Select_All()
        {
            if (Form1.BIG_EDIT_MODE == false)
            {
                return;
            }
            trans_x = 0;
            trans_y = 0;
            trans_w = 16;
            trans_h = 16;
            Form1.BE_x1 = 0;
            Form1.BE_y1 = 0;
            Form1.BE_x2 = 16;
            Form1.BE_y2 = 16;
            // seems redundant to have 2 sets of vars basically the same
            // decided to leave duplicate vars anyway
            Form1.tile_x = 0;
            Form1.tile_y = 0;
            Form1.tile_num = 0;
        }


    }
}
