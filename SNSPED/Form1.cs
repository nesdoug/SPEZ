using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SNSPED
{
    public partial class Form1 : Form
    {
        

        public Form1()
        {
            InitializeComponent();
        }
        static Form2 newChild = null;
        static Form3 newChild3 = null;
        static Form4 newChild4 = null;

        public static void close_it()
        {
            newChild = null;
        }

        public static void close_it3()
        {
            newChild3 = null;
        }

        public static void close_it4()
        {
            newChild4 = null;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < MAX_METASP; i++)
            {
                // by default all values should be zero.
                MetaspriteArray[i] = new Metasprites();
                MetaspriteArray[i].name = "Metasprite " + i.ToString();
            }
            
            listBox1.SelectedIndex = 0;
            cancel_index_change = false;
            spr_size_mode = SIZES_8_16;

            update_palette();
            update_tile_image();
            update_metatile_image();
            label5.Focus();
            this.ActiveControl = label5;
        }

        //globals
        public const int MAX_METASP = 100;
        public const int MAX_SPRITE = 100;
        Metasprites[] MetaspriteArray = new Metasprites[MAX_METASP];
        Metasprites UndoMetasprite = new Metasprites();

        public static Bitmap image_meta = new Bitmap(160, 160);
        public static Bitmap image_tiles = new Bitmap(128, 128);
        public static Bitmap image_pal = new Bitmap(256, 256);
        public static Bitmap temp_bmp = new Bitmap(256, 256); //double size
        public static Bitmap temp_bmp2 = new Bitmap(320, 320); //double size
        public static Bitmap temp_bmp3 = new Bitmap(256, 256); //double size
        public static Bitmap cool_bmp = new Bitmap(128, 128); //import
        public static int pal_x, pal_y, tile_x, tile_y, tile_num, tile_set;
        public static int pal_r_copy, pal_g_copy, pal_b_copy;
        public static int spr_size_mode;
        public static int selected_meta, selected_spr;
        public static bool cancel_index_change;
        public static int r_start_x, r_start_y;
        const int SIZES_8_16 = 0;
        const int SIZES_8_32 = 1;
        const int SIZES_8_64 = 2;
        const int SIZES_16_32 = 3;
        const int SIZES_16_64 = 4;
        const int SIZES_32_64 = 5;
        const int MIN_X = -64; // -128 is the end of set marker
        const int MAX_X = 64;
        const int MIN_Y = -64;
        const int MAX_Y = 64;
        public static byte[] rle_array = new byte[65536];
        public static int rle_index, rle_index2, rle_count;
        public static int[] sel_array = new int[100]; // remember which items selected
        public static int disable_map_click;

        public static int dither_factor;
        public static double dither_db = 0.0;
        public static int dither_adjust = 0;
        public static bool f3_cb1 = true;

        public static int[] R_Array = new int[65536];
        public static int[] G_Array = new int[65536];
        public static int[] B_Array = new int[65536];
        public static int[] Count_Array = new int[65536]; // count each color
        public static int[] SixteenColorIndexes = new int[16];
        public static int[] SixteenColorsAdded = new int[16];
        public static int color_count; // how many total different colors
        public static int r_val, g_val, b_val, diff_val;
        public static int c_offset, c_offset2;
        public static int image_width, image_height;
        public static int[] needy_chr_array = new int[65536];

        public static bool undo_ready = false;
        


        public readonly int[,] BAYER_MATRIX =
        {
            { 0,48,12,60,3,51,15,63 },
            { 32,16,44,28,35,19,47,31 },
            { 8,56,4,52,11,59,7,55 },
            { 40,24,36,20,43,27,39,23 },
            { 2,50,14,62,1,49,13,61 },
            { 34,18,46,30,33,17,45,29 },
            { 10,58,6,54,9,57,5,53 },
            { 42,26,38,22,41,25,37,21 }
        }; // 1/64 times this



        public void Checkpoint()
        {
            // backup for undo function
            undo_ready = true;

            // save current metasprite
            int count = MetaspriteArray[selected_meta].sprite_count;

            for (int i = 0; i < count; ++i)
            { // copy current meta 
                UndoMetasprite.tile[i] = MetaspriteArray[selected_meta].tile[i];
                UndoMetasprite.set[i] = MetaspriteArray[selected_meta].set[i];
                UndoMetasprite.rel_x[i] = MetaspriteArray[selected_meta].rel_x[i];
                UndoMetasprite.rel_y[i] = MetaspriteArray[selected_meta].rel_y[i];
                UndoMetasprite.palette[i] = MetaspriteArray[selected_meta].palette[i];
                UndoMetasprite.h_flip[i] = MetaspriteArray[selected_meta].h_flip[i];
                UndoMetasprite.v_flip[i] = MetaspriteArray[selected_meta].v_flip[i];
                UndoMetasprite.size[i] = MetaspriteArray[selected_meta].size[i];
            }
            UndoMetasprite.priority = MetaspriteArray[selected_meta].priority;
            UndoMetasprite.sprite_count = MetaspriteArray[selected_meta].sprite_count;
            UndoMetasprite.name = MetaspriteArray[selected_meta].name;

            for (int i = 0; i < 32768; ++i)
            { // copy tilesets
                Tiles.Undo_Tile_Array[i] = Tiles.Tile_Arrays[i];
            }
        }

        public void Do_Undo()
        {
            if (undo_ready == false) return;

            int count = UndoMetasprite.sprite_count;

            for (int i = 0; i < count; ++i)
            { // copy current meta 
                MetaspriteArray[selected_meta].tile[i] = UndoMetasprite.tile[i];
                MetaspriteArray[selected_meta].set[i] = UndoMetasprite.set[i];
                MetaspriteArray[selected_meta].rel_x[i] = UndoMetasprite.rel_x[i];
                MetaspriteArray[selected_meta].rel_y[i] = UndoMetasprite.rel_y[i];
                MetaspriteArray[selected_meta].palette[i] = UndoMetasprite.palette[i];
                MetaspriteArray[selected_meta].h_flip[i] = UndoMetasprite.h_flip[i];
                MetaspriteArray[selected_meta].v_flip[i] = UndoMetasprite.v_flip[i];
                MetaspriteArray[selected_meta].size[i] = UndoMetasprite.size[i];
                
            }
            MetaspriteArray[selected_meta].priority = UndoMetasprite.priority;
            MetaspriteArray[selected_meta].sprite_count = UndoMetasprite.sprite_count;
            MetaspriteArray[selected_meta].name = UndoMetasprite.name;
            textBox6.Text = UndoMetasprite.name;
            listBox1.Items[listBox1.SelectedIndex] = UndoMetasprite.name;
            textBox7.Text = UndoMetasprite.priority.ToString();
            // skip textBox5, should be done by rebuild.. below

            for (int i = 0; i < 32768; ++i)
            { // copy tilesets
                Tiles.Tile_Arrays[i] = Tiles.Undo_Tile_Array[i];
            }

            // checkboxes and listboxes
            rebuild_spr_list();
            string str = "";
            if (selected_spr < 10) str = "0";
            str = str + selected_spr.ToString(); //hex
            if (selected_spr < 0) str = "00";
            label19.Text = str;

            common_update2();
            // update the tile edit box and
            //update_tile_image();
            //update_metatile_image();

            undo_ready = false;
        }



        public void update_palette() // use this one
        {
            byte zero = Palettes.pal_r[0]; // copy the first element to all the firsts
            Palettes.pal_r[16] = zero;
            Palettes.pal_r[16 * 1] = zero;
            Palettes.pal_r[16 * 2] = zero;
            Palettes.pal_r[16 * 3] = zero;
            Palettes.pal_r[16 * 4] = zero;
            Palettes.pal_r[16 * 5] = zero;
            Palettes.pal_r[16 * 6] = zero;
            Palettes.pal_r[16 * 7] = zero;
            zero = Palettes.pal_g[0];
            Palettes.pal_g[16] = zero;
            Palettes.pal_g[16 * 1] = zero;
            Palettes.pal_g[16 * 2] = zero;
            Palettes.pal_g[16 * 3] = zero;
            Palettes.pal_g[16 * 4] = zero;
            Palettes.pal_g[16 * 5] = zero;
            Palettes.pal_g[16 * 6] = zero;
            Palettes.pal_g[16 * 7] = zero;
            zero = Palettes.pal_b[0];
            Palettes.pal_b[16] = zero;
            Palettes.pal_b[16 * 1] = zero;
            Palettes.pal_b[16 * 2] = zero;
            Palettes.pal_b[16 * 3] = zero;
            Palettes.pal_b[16 * 4] = zero;
            Palettes.pal_b[16 * 5] = zero;
            Palettes.pal_b[16 * 6] = zero;
            Palettes.pal_b[16 * 7] = zero;

            // which palette square
            int xx = pal_x * 16;
            int yy = pal_y * 32;

            draw_palettes();

            // draw a square on selected box
            for (int i = 0; i < 16; i++)
            {
                image_pal.SetPixel(xx + i, yy, Color.White); //top line
                image_pal.SetPixel(xx, yy + i, Color.White); //left line

                image_pal.SetPixel(xx + i, yy + 15, Color.White); //bottom line
                image_pal.SetPixel(xx + 15, yy + i, Color.White); //right line

                if (i == 15) continue;
                image_pal.SetPixel(xx + 14, yy + i, Color.Black); //black right line
                image_pal.SetPixel(xx + i, yy + 14, Color.Black); //black bottom line
            }

            pictureBox3.Image = image_pal;
            pictureBox3.Refresh();
        }

        public static void draw_palettes() // sub routine of update palette
        {
            int count = 0;
            SolidBrush temp_brush = new SolidBrush(Color.White);
            
            for (int i = 0; i < 256; i += 32) //each row
            {
                for (int j = 0; j < 256; j += 16) //each box in the row
                {
                    // draw a rectangle
                    using (Graphics g = Graphics.FromImage(temp_bmp3))
                    {
                        temp_brush.Color = Color.FromArgb(Palettes.pal_r[count], Palettes.pal_g[count], Palettes.pal_b[count]);
                        g.FillRectangle(temp_brush, j, i, 16, 16);
                    }
                    count++;
                }
            }

            image_pal = temp_bmp3;
            temp_brush.Dispose();
        } // END DRAW PALETTES




        private int check_num(string str) // make sure string is number
        {
            int value = 0;

            int.TryParse(str, out value);
            if (value > 255) value = 255; // max value
            if (value < 0) value = 0; // min value
            value = value & 0xf8;
            return (value);
        }

        private int get_selection()
        {
            int selection = pal_x + (pal_y * 16);
            
            if (pal_x == 0) selection = 0;
            return selection;
        }

        private void update_rgb() // when r g or b boxes change
        {
            string str = textBox1.Text;
            int value = check_num(str);
            textBox1.Text = value.ToString();
            trackBar1.Value = value / 8;

            int selection = get_selection();
            Palettes.pal_r[selection] = (byte)value;

            str = textBox2.Text;
            value = check_num(str);
            textBox2.Text = value.ToString();
            trackBar2.Value = value / 8;

            Palettes.pal_g[selection] = (byte)value;

            str = textBox3.Text;
            value = check_num(str);
            textBox3.Text = value.ToString();
            trackBar3.Value = value / 8;

            Palettes.pal_b[selection] = (byte)value;
        }




        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            disable_map_click = 0;
            var mouseEventArgs = e as MouseEventArgs;
            int pixel_x = -1;
            int pixel_y = -1;

            if (mouseEventArgs.Button == MouseButtons.Right)
            {
                
                pixel_x = mouseEventArgs.X;
                pixel_y = mouseEventArgs.Y;

                if (pixel_x < 0) pixel_x = -1; // invalid start
                if (pixel_x > 319) pixel_x = -1;
                if (pixel_y < 0) pixel_y = -1; // invalid start
                if (pixel_y > 319) pixel_y = -1;
            }
            r_start_x = pixel_x;
            r_start_y = pixel_y;

            Checkpoint();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            var mouseEventArgs = e as MouseEventArgs;
            int pixel_x = -1;
            int pixel_y = -1;

            if (mouseEventArgs.Button == MouseButtons.Right)
            {
                if (listBox2.SelectedItems.Count < 1) return;

                if ((r_start_x >= 0) && (r_start_y >= 0)) // valid starts
                {
                    pixel_x = mouseEventArgs.X;
                    pixel_y = mouseEventArgs.Y;

                    if (pixel_x < 0) pixel_x = -1; // invalid move
                    if (pixel_x > 319) pixel_x = -1;
                    if (pixel_y < 0) pixel_y = -1; // invalid move
                    if (pixel_y > 319) pixel_y = -1;

                    if ((pixel_x >= 0) && (pixel_y >= 0)) // valid coords
                    {
                        int delta_x = pixel_x - r_start_x;
                        if (delta_x < -128) delta_x = -128;
                        if (delta_x > 128) delta_x = 128;
                        int delta_y = pixel_y - r_start_y;
                        if (delta_y < -128) delta_y = 128;
                        if (delta_y > 128) delta_y = 128;
                        // change in position is now in range
                        // then add to 1 or more sprite

                        delta_x = delta_x / 2;
                        delta_y = delta_y / 2;

                        if((delta_x != 0) || (delta_y != 0)) // skip if no meaningful change
                        {
                            int meta_x, meta_y;
                            selected_spr = listBox2.SelectedIndex;

                            foreach (int index in listBox2.SelectedIndices)
                            {
                                meta_x = MetaspriteArray[selected_meta].rel_x[index];
                                meta_x = meta_x + delta_x;
                                if (meta_x < -64) meta_x = -64;
                                if (meta_x > 64) meta_x = 64;
                                MetaspriteArray[selected_meta].rel_x[index] = meta_x;

                                meta_y = MetaspriteArray[selected_meta].rel_y[index];
                                meta_y = meta_y + delta_y;
                                if (meta_y < -64) meta_y = -64;
                                if (meta_y > 64) meta_y = 64;
                                MetaspriteArray[selected_meta].rel_y[index] = meta_y;
                            }
                            
                            // update start position as we go
                            r_start_x = r_start_x + (delta_x * 2);
                            if (r_start_x < 0) r_start_x = 0;
                            if (r_start_x > 319) r_start_x = 319;

                            r_start_y = r_start_y + (delta_y * 2);
                            if (r_start_y < 0) r_start_y = 0;
                            if (r_start_y > 319) r_start_y = 319;

                            update_metatile_image();
                            rebuild_spr_list();
                        }
                    }
                }
            }
        }




        private void pictureBox1_Click(object sender, EventArgs e)
        { // main metatile editor, top left
            if(disable_map_click == 1)
            {
                disable_map_click = 0;
                return;
            }

            // left click = drop a tile, right = shift selected tile
            // click the tile in the tile editor to change which selected
            int meta_x, meta_y, meta_x2, meta_y2, count, offset;
            
            if (MetaspriteArray[selected_meta].sprite_count >= MAX_SPRITE) return;
            
            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs.Button == MouseButtons.Right) return;

            if (mouseEventArgs != null)
            {
                meta_x = mouseEventArgs.X;
                meta_y = mouseEventArgs.Y;
            }
            else return;

            count = MetaspriteArray[selected_meta].sprite_count =
                    MetaspriteArray[selected_meta].sprite_count + 1;
            offset = count - 1;

            if (meta_x < 0) meta_x = 0;
            if (meta_y < 0) meta_y = 0;
            if (meta_x > 256) meta_x = 256;
            if (meta_y > 256) meta_y = 256;
            meta_x2 = ((meta_x / 2) & 0xf8) - 64; // -64 to 64
            meta_y2 = ((meta_y / 2) & 0xf8) - 64; // -64 to 64

            // add to the data
            MetaspriteArray[selected_meta].rel_x[offset] = meta_x2;
            MetaspriteArray[selected_meta].rel_y[offset] = meta_y2;

            int tile_sel = (tile_y * 16) + tile_x;
            MetaspriteArray[selected_meta].tile[offset] = tile_sel;
            if (tile_set == 0)
            {
                MetaspriteArray[selected_meta].set[offset] = 0;
            }
            else
            {
                MetaspriteArray[selected_meta].set[offset] = 1;
            }
            if (checkBox1.Checked == false) // h flip
            {
                MetaspriteArray[selected_meta].h_flip[offset] = 0;
            }
            else
            {
                MetaspriteArray[selected_meta].h_flip[offset] = 1;
            }
            if (checkBox2.Checked == false) // v flip
            {
                MetaspriteArray[selected_meta].v_flip[offset] = 0;
            }
            else
            {
                MetaspriteArray[selected_meta].v_flip[offset] = 1;
            }
            if (checkBox4.Checked == false) // size
            {
                MetaspriteArray[selected_meta].size[offset] = 0;
            }
            else
            {
                MetaspriteArray[selected_meta].size[offset] = 1;
            }
            // palette (ignore the box on the left, use selected palette)
            MetaspriteArray[selected_meta].palette[offset] = pal_y;
            textBox5.Text = pal_y.ToString();

            // add to the list box
            string str = "tile ";
            if (tile_sel < 16) str = str + "0";
            str = str + tile_sel.ToString("X"); //hex
            str = str + "  set=" + tile_set.ToString();
            str = str + "   x=" + meta_x2.ToString() + "   y=" + meta_y2.ToString();
            str = str + "   pal=" + MetaspriteArray[selected_meta].palette[offset].ToString();
            str = str + "   H=" + MetaspriteArray[selected_meta].h_flip[offset].ToString();
            str = str + "   V=" + MetaspriteArray[selected_meta].v_flip[offset].ToString();
            str = str + "   Sz=" + MetaspriteArray[selected_meta].size[offset].ToString();

            listBox2.ClearSelected();
            listBox2.Items.Add(str);
            
            listBox2.SelectedIndex = count - 1;
            listBox2.Refresh();

            str = "";
            if (offset < 10) str = "0";
            str = str + offset.ToString();
            label19.Text = str;
            
            update_metatile_image();
        }





        private void pictureBox2_Click(object sender, EventArgs e)
        { // tiles
            //change the label to tile number, in hex
            tile_x = 0; tile_y = 0; tile_num = 0; //globals

            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs != null)
            {

                tile_x = mouseEventArgs.X >> 4;
                tile_y = mouseEventArgs.Y >> 4;
            }
            if (tile_x < 0) tile_x = 0;
            if (tile_y < 0) tile_y = 0;
            if (tile_x > 15) tile_x = 15;
            if (tile_y > 15) tile_y = 15;
            tile_num = (tile_y * 16) + tile_x;
            tile_show_num();

            //last
            if (newChild != null)
            {
                newChild.BringToFront();
                newChild.update_tile_box();
            }
            else
            {
                // remember to put the form location as "manual"
                newChild = new Form2();
                newChild.Owner = this;
                int xx = Screen.PrimaryScreen.Bounds.Width;
                if (this.Location.X + 970 < xx) // set new form location
                {
                    newChild.Location = new Point(this.Location.X + 800, this.Location.Y + 80);
                }
                else
                {
                    newChild.Location = new Point(xx - 170, this.Location.Y);
                }

                newChild.Show();
                //update
            }

            update_tile_image();
            label5.Focus();
        }

        public void tile_show_num() // top right, above tileset
        {
            string str = "";
            int dec_num = (tile_y * 16) + tile_x + ((tile_set & 3) * 256);
            str = hex_char(tile_y) + hex_char(tile_x) + "   " + dec_num.ToString();
            label8.Text = str;
        }

        private string hex_char(int value)
        {
            switch (value)
            {
                case 15:
                    return "F";
                case 14:
                    return "E";
                case 13:
                    return "D";
                case 12:
                    return "C";
                case 11:
                    return "B";
                case 10:
                    return "A";
                case 9:
                    return "9";
                case 8:
                    return "8";
                case 7:
                    return "7";
                case 6:
                    return "6";
                case 5:
                    return "5";
                case 4:
                    return "4";
                case 3:
                    return "3";
                case 2:
                    return "2";
                case 1:
                    return "1";
                case 0:
                default:
                    return "0";
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        { // palette
            int selection;
            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs != null)
            {
                if ((mouseEventArgs.Y & 0x10) == 0)
                {
                    pal_x = (mouseEventArgs.X & 0xf0) >> 4;
                    pal_y = (mouseEventArgs.Y & 0xe0) >> 5;
                }
                if (pal_x < 0) pal_x = 0;
                if (pal_y < 0) pal_y = 0;
                if (pal_x > 15) pal_x = 15;
                if (pal_y > 7) pal_y = 7;

                selection = pal_x + (pal_y * 16);
                update_palette();

                //update the boxes
                rebuild_pal_boxes();
            }

            common_update2();
            label5.Focus();
        }


        public void rebuild_pal_boxes()
        {
            int selection = pal_x + (pal_y * 16);

            int red = Palettes.pal_r[selection];
            textBox1.Text = red.ToString();
            trackBar1.Value = red / 8;

            int green = Palettes.pal_g[selection];
            textBox2.Text = green.ToString();
            trackBar2.Value = green / 8;

            int blue = Palettes.pal_b[selection];
            textBox3.Text = blue.ToString();
            trackBar3.Value = blue / 8;

            update_box4();
        }


        private void update_box4() // when boxes 1,2,or 3 changed
        { // text box 4 = hex
            int value_red, value_green, value_blue;
            int sum;
            int selection = get_selection();

            value_red = Palettes.pal_r[selection];
            value_green = Palettes.pal_g[selection];
            value_blue = Palettes.pal_b[selection];


            sum = ((value_red & 0xf8) >> 3) + ((value_green & 0xf8) << 2) + ((value_blue & 0xf8) << 7);
            string hexValue = sum.ToString("X");
            // may have to append zeros to beginning


            if (hexValue.Length == 3) hexValue = String.Concat("0", hexValue);
            else if (hexValue.Length == 2) hexValue = String.Concat("00", hexValue);
            else if (hexValue.Length == 1) hexValue = String.Concat("000", hexValue);
            else if (hexValue.Length == 0) hexValue = "0000";

            textBox4.Text = hexValue;
        }

        

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        { // show grid

            update_metatile_image();
            label5.Focus();
        }


        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        { // palette changer for a tile
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;
            if (listBox2.SelectedIndex < 0) return;

            if (e.KeyChar == (char)Keys.Return)
            {
                update_textbox5();
                e.Handled = true; // prevent ding on return press
            }
        }


        private void textBox5_Leave(object sender, EventArgs e)
        {
            update_textbox5();
        }


        public void update_textbox5()
        {
            Checkpoint();

            string str = textBox5.Text;
            //if (str.Length > 1) return; // should be 1 digit

            char testchr = str[0];
            if ((testchr < '0') || (testchr > '7'))
            {
                textBox5.Text = "0";
                str = "0";
            }
            //else
            {
                int temp_val = 0;
                int.TryParse(str, out temp_val);
                if (listBox2.SelectedItems.Count < 1) return;

                foreach (int index in listBox2.SelectedIndices)
                {
                    MetaspriteArray[selected_meta].palette[index] = temp_val;

                }
            }

            rebuild_spr_list();
            update_metatile_image();
        }


        private void button2_Click(object sender, EventArgs e)
        { // h flip
            //flip the selected tile, unless select all, then flip all.
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;
            if (listBox2.SelectedItems.Count < 1) return;

            Checkpoint();

            int x_least, x_most, temp1, temp3;
            // get smallest and largest
            x_least = 64;
            x_most = -64;
            foreach (int index in listBox2.SelectedIndices)
            {
                temp1 = MetaspriteArray[selected_meta].rel_x[index];
                if (temp1 < x_least) x_least = temp1;
                if (temp1 > x_most) x_most = temp1;
            }
            
            foreach (int index in listBox2.SelectedIndices)
            {
                    
                MetaspriteArray[selected_meta].h_flip[index] =
                MetaspriteArray[selected_meta].h_flip[index] ^ 1; //xor

                temp3 = (x_most - MetaspriteArray[selected_meta].rel_x[index]) + x_least;

                if (temp3 < -64) temp3 = -64; // minimum allowed
                if (temp3 > 64) temp3 = 64; // max allowed
                MetaspriteArray[selected_meta].rel_x[index] = temp3;
            }
            rebuild_spr_list();
            string str = "";
            if (selected_spr < 10) str = "0";
            str = str + selected_spr.ToString(); //hex
            label19.Text = str;
            
            update_metatile_image();

            label5.Focus();
        }

        private void button3_Click(object sender, EventArgs e)
        { // v flip
            //flip the selected tile, unless select all, then flip all.
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;
            if (listBox2.SelectedItems.Count < 1) return;

            Checkpoint();

            int y_least, y_most, temp1, temp3;
            // get smallest and largest
            y_least = 64;
            y_most = -64;
            foreach (int index in listBox2.SelectedIndices)
            {
                temp1 = MetaspriteArray[selected_meta].rel_y[index];
                if (temp1 < y_least) y_least = temp1;
                if (temp1 > y_most) y_most = temp1;
            }
            
            foreach (int index in listBox2.SelectedIndices)
            {
                MetaspriteArray[selected_meta].v_flip[index] =
                MetaspriteArray[selected_meta].v_flip[index] ^ 1; //xor

                temp3 = (y_most - MetaspriteArray[selected_meta].rel_y[index]) + y_least;

                if (temp3 < -64) temp3 = -64;
                if (temp3 > 64) temp3 = 64; // max allowed
                MetaspriteArray[selected_meta].rel_y[index] = temp3;
            }
            rebuild_spr_list();
            string str = "";
            if (selected_spr < 10) str = "0";
            str = str + selected_spr.ToString(); //hex
            label19.Text = str;
            
            rebuild_one_item();
            update_metatile_image();

            label5.Focus();
        }

        private void button12_Click(object sender, EventArgs e)
        { // resize
            //resize the selected tile, unless select all, then resize all.
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;
            if (listBox2.SelectedItems.Count < 1) return;

            Checkpoint();

            foreach (int index in listBox2.SelectedIndices)
            {
                MetaspriteArray[selected_meta].size[index] =
                MetaspriteArray[selected_meta].size[index] ^ 1; //xor
            }
            rebuild_spr_list();
            string str = "";
            if (selected_spr < 10) str = "0";
            str = str + selected_spr.ToString(); //hex
            label19.Text = str;
            
            rebuild_one_item();
            update_metatile_image();

            label5.Focus();
        }





        private void button4_Click(object sender, EventArgs e)
        { // nudge left
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;
            if (listBox2.SelectedItems.Count < 1) return;
            int temp_x;
            
            int num_sprites = listBox2.Items.Count;
            if (num_sprites < 1) return;
            int test = 0;

            Checkpoint();

            foreach (int index in listBox2.SelectedIndices)
            {
                temp_x = MetaspriteArray[selected_meta].rel_x[index];
                temp_x--;
                if (temp_x < MIN_X) test++;
                if (test > 0) break;
            }
            if(test == 0) // safe
            {
                foreach (int index in listBox2.SelectedIndices)
                {
                    temp_x = MetaspriteArray[selected_meta].rel_x[index];
                    temp_x--;
                    if (temp_x < MIN_X) temp_x = MIN_X;
                    MetaspriteArray[selected_meta].rel_x[index] = temp_x;
                }
            }
                
            rebuild_spr_list();
            string str = "";
            if (selected_spr < 10) str = "0";
            str = str + selected_spr.ToString(); //hex
            label19.Text = str;
            
            update_metatile_image();

            label5.Focus();
        }

        private void button5_Click(object sender, EventArgs e)
        { // nudge right
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;
            if (listBox2.SelectedItems.Count < 1) return;
            int temp_x;
            
            int num_sprites = listBox2.Items.Count;
            if (num_sprites < 1) return;
            int test = 0;

            Checkpoint();

            // test each, make sure in range
            foreach (int index in listBox2.SelectedIndices)
            {
                temp_x = MetaspriteArray[selected_meta].rel_x[index];
                temp_x++;
                if (temp_x > MAX_X) test++;
                if (test > 0) break;
            }
            if (test == 0) // safe
            {
                foreach (int index in listBox2.SelectedIndices)
                {
                    temp_x = MetaspriteArray[selected_meta].rel_x[index];
                    temp_x++;
                    if (temp_x > MAX_X) temp_x = MAX_X;
                    MetaspriteArray[selected_meta].rel_x[index] = temp_x;
                }
            }


            rebuild_spr_list();
            string str = "";
            if (selected_spr < 10) str = "0";
            str = str + selected_spr.ToString(); //hex
            label19.Text = str;
            
            update_metatile_image();

            label5.Focus();
        }

        private void button6_Click(object sender, EventArgs e)
        { // nudge up
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;
            if (listBox2.SelectedItems.Count < 1) return;
            int temp_y;
            
            int num_sprites = listBox2.Items.Count;
            if (num_sprites < 1) return;
            int test = 0;

            Checkpoint();

            // test each, make sure in range
            foreach (int index in listBox2.SelectedIndices)
            {
                temp_y = MetaspriteArray[selected_meta].rel_y[index];
                temp_y--;
                if (temp_y < MIN_Y) test++;
                if (test > 0) break;
            }
            if (test == 0) // safe
            {
                foreach (int index in listBox2.SelectedIndices)
                {
                    temp_y = MetaspriteArray[selected_meta].rel_y[index];
                    temp_y--;
                    if (temp_y < MIN_Y) temp_y = MIN_Y;
                    MetaspriteArray[selected_meta].rel_y[index] = temp_y;
                }
            }

            rebuild_spr_list();
            string str = "";
            if (selected_spr < 10) str = "0";
            str = str + selected_spr.ToString(); //hex
            label19.Text = str;
            
            update_metatile_image();

            label5.Focus();
        }

        private void button7_Click(object sender, EventArgs e)
        { // nudge down
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;
            if (listBox2.SelectedItems.Count < 1) return;
            int temp_y;
            
            int num_sprites = listBox2.Items.Count;
            if (num_sprites < 1) return;
            int test = 0;

            Checkpoint();

            // test each, make sure in range
            foreach (int index in listBox2.SelectedIndices)
            {
                temp_y = MetaspriteArray[selected_meta].rel_y[index];
                temp_y++;
                if (temp_y > MAX_Y) test++;
                if (test > 0) break;
            }
            if (test == 0) // safe
            {
                foreach (int index in listBox2.SelectedIndices)
                {
                    temp_y = MetaspriteArray[selected_meta].rel_y[index];
                    temp_y++;
                    if (temp_y > MAX_Y) temp_y = MAX_Y;
                    MetaspriteArray[selected_meta].rel_y[index] = temp_y;
                }
            }

            rebuild_spr_list();
            string str = "";
            if (selected_spr < 10) str = "0";
            str = str + selected_spr.ToString(); //hex
            label19.Text = str;
            
            update_metatile_image();

            label5.Focus();
        }




        private void listBox2_Click(object sender, EventArgs e)
        { // list of tiles, selected tile changed
            // default value of number of sprite is zero
            label19.Text = "00";
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;

            //change the selected tile info
            int selected = listBox2.SelectedIndex;
            if (selected < 0) return;
            selected_spr = selected;
            textBox5.Text = MetaspriteArray[selected_meta].palette[selected].ToString();
            
            string str = "";
            if (selected < 10) str = "0";
            str = str + selected.ToString(); //hex
            label19.Text = str;

            update_metatile_image();
        }

        

        private void button8_Click(object sender, EventArgs e)
        { // reorder up
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;
            if (listBox2.SelectedItems.Count < 1) return;
            if (listBox2.SelectedItems.Count > 1) // don't allow multi
            {
                MessageBox.Show("Select only one sprite.");
                return;
            }
            int temp1, temp2, temp3, temp4, temp5, temp6, temp7, temp8;
            int index2 = listBox2.SelectedIndex;
            if (index2 < 1) return;

            Checkpoint();

            int index1 = index2 - 1;

            //swap 2 sprites
            temp1 = MetaspriteArray[selected_meta].tile[index1];
            temp2 = MetaspriteArray[selected_meta].set[index1];
            temp3 = MetaspriteArray[selected_meta].rel_x[index1];
            temp4 = MetaspriteArray[selected_meta].rel_y[index1];
            temp5 = MetaspriteArray[selected_meta].palette[index1];
            temp6 = MetaspriteArray[selected_meta].h_flip[index1];
            temp7 = MetaspriteArray[selected_meta].v_flip[index1];
            temp8 = MetaspriteArray[selected_meta].size[index1];

            MetaspriteArray[selected_meta].tile[index1] = MetaspriteArray[selected_meta].tile[index2];
            MetaspriteArray[selected_meta].set[index1] = MetaspriteArray[selected_meta].set[index2];
            MetaspriteArray[selected_meta].rel_x[index1] = MetaspriteArray[selected_meta].rel_x[index2];
            MetaspriteArray[selected_meta].rel_y[index1] = MetaspriteArray[selected_meta].rel_y[index2];
            MetaspriteArray[selected_meta].palette[index1] = MetaspriteArray[selected_meta].palette[index2];
            MetaspriteArray[selected_meta].h_flip[index1] = MetaspriteArray[selected_meta].h_flip[index2];
            MetaspriteArray[selected_meta].v_flip[index1] = MetaspriteArray[selected_meta].v_flip[index2];
            MetaspriteArray[selected_meta].size[index1] = MetaspriteArray[selected_meta].size[index2];

            MetaspriteArray[selected_meta].tile[index2] = temp1;
            MetaspriteArray[selected_meta].set[index2] = temp2;
            MetaspriteArray[selected_meta].rel_x[index2] = temp3;
            MetaspriteArray[selected_meta].rel_y[index2] = temp4;
            MetaspriteArray[selected_meta].palette[index2] = temp5;
            MetaspriteArray[selected_meta].h_flip[index2] = temp6;
            MetaspriteArray[selected_meta].v_flip[index2] = temp7;
            MetaspriteArray[selected_meta].size[index2] = temp8;

            rebuild_one_item();
            listBox2.ClearSelected();
            listBox2.SelectedIndex = index1;
            rebuild_one_item();
            update_metatile_image();

            label5.Focus();
        }

        private void button9_Click(object sender, EventArgs e)
        { // reorder down
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;
            //if (listBox2.SelectedIndex < 0) return;
            if (listBox2.SelectedItems.Count < 1) return;
            if (listBox2.SelectedItems.Count > 1) // don't allow multi
            {
                MessageBox.Show("Select only one sprite.");
                return;
            }
            int temp1, temp2, temp3, temp4, temp5, temp6, temp7, temp8;
            int index1 = listBox2.SelectedIndex;
            int index2 = index1 + 1;
            if (index2 >= listBox2.Items.Count) return;

            Checkpoint();

            //swap 2 sprites
            temp1 = MetaspriteArray[selected_meta].tile[index1];
            temp2 = MetaspriteArray[selected_meta].set[index1];
            temp3 = MetaspriteArray[selected_meta].rel_x[index1];
            temp4 = MetaspriteArray[selected_meta].rel_y[index1];
            temp5 = MetaspriteArray[selected_meta].palette[index1];
            temp6 = MetaspriteArray[selected_meta].h_flip[index1];
            temp7 = MetaspriteArray[selected_meta].v_flip[index1];
            temp8 = MetaspriteArray[selected_meta].size[index1];

            MetaspriteArray[selected_meta].tile[index1] = MetaspriteArray[selected_meta].tile[index2];
            MetaspriteArray[selected_meta].set[index1] = MetaspriteArray[selected_meta].set[index2];
            MetaspriteArray[selected_meta].rel_x[index1] = MetaspriteArray[selected_meta].rel_x[index2];
            MetaspriteArray[selected_meta].rel_y[index1] = MetaspriteArray[selected_meta].rel_y[index2];
            MetaspriteArray[selected_meta].palette[index1] = MetaspriteArray[selected_meta].palette[index2];
            MetaspriteArray[selected_meta].h_flip[index1] = MetaspriteArray[selected_meta].h_flip[index2];
            MetaspriteArray[selected_meta].v_flip[index1] = MetaspriteArray[selected_meta].v_flip[index2];
            MetaspriteArray[selected_meta].size[index1] = MetaspriteArray[selected_meta].size[index2];

            MetaspriteArray[selected_meta].tile[index2] = temp1;
            MetaspriteArray[selected_meta].set[index2] = temp2;
            MetaspriteArray[selected_meta].rel_x[index2] = temp3;
            MetaspriteArray[selected_meta].rel_y[index2] = temp4;
            MetaspriteArray[selected_meta].palette[index2] = temp5;
            MetaspriteArray[selected_meta].h_flip[index2] = temp6;
            MetaspriteArray[selected_meta].v_flip[index2] = temp7;
            MetaspriteArray[selected_meta].size[index2] = temp8;

            rebuild_one_item();
            listBox2.ClearSelected();
            listBox2.SelectedIndex = index2;
            rebuild_one_item();
            update_metatile_image();

            label5.Focus();
        }

        

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        { // Red
            if (e.KeyChar == (char)Keys.Return)
            {
                update_rgb();
                update_box4();

                update_palette();

                common_update2();

                e.Handled = true; // prevent ding on return press
                label5.Focus();
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        { // Green
            if (e.KeyChar == (char)Keys.Return)
            {
                update_rgb();
                update_box4();

                update_palette();

                common_update2();

                e.Handled = true; // prevent ding on return press
                label5.Focus();
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        { // Blue
            if (e.KeyChar == (char)Keys.Return)
            {
                update_rgb();
                update_box4();

                update_palette();

                common_update2();

                e.Handled = true; // prevent ding on return press
                label5.Focus();
            }
        }

        

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        { // Hex
            if (e.KeyChar == (char)Keys.Return)
            {
                string str = textBox4.Text;
                str = str.Trim(); // remove spaces
                int[] value = new int[4];
                int temp;

                if (str.Length < 4)
                {
                    str = str.PadLeft(4, '0');
                }

                if (str.Length != 4) return;
                str = str.ToUpper();
                str = check_hex(str); //returns "Z" if fail
                if (str == "Z") return;

                textBox4.Text = str;

                value[0] = hex_val(str[0]); //get int value, 0-15
                value[1] = hex_val(str[1]);
                value[2] = hex_val(str[2]);
                value[3] = hex_val(str[3]);

                //pass values to the other boxes
                temp = ((value[3] & 0x0f) << 3) + ((value[2] & 0x01) << 7); // red, 5 bits
                textBox1.Text = temp.ToString();
                temp = ((value[2] & 0x0e) << 2) + ((value[1] & 0x03) << 6); // green, 5 bits
                textBox2.Text = temp.ToString();
                temp = ((value[1] & 0x0c) << 1) + ((value[0] & 0x07) << 5); // blue, 5 bits
                textBox3.Text = temp.ToString();

                update_rgb();
                update_palette();
                common_update2();

                e.Handled = true; // prevent ding on return press
                label5.Focus();
            }
        }

        private bool is_hex(char ch1)
        {
            if ((ch1 >= '0') && (ch1 <= '9')) return true;
            if ((ch1 >= 'A') && (ch1 <= 'F')) return true;
            //should be upper case letters
            return false;
        }



        private string check_hex(string str) //str.Length should be exacly 4
        {
            if ((!is_hex(str[0])) ||
                (!is_hex(str[1])) ||
                (!is_hex(str[2])) ||
                (!is_hex(str[3])))
            {
                //something isn't a hex string
                return "Z";
            }

            //make sure the high byte is 0-7
            if (str[0] > '7')
            {
                char[] letters = str.ToCharArray();
                char letter;
                switch (letters[0])
                {
                    case 'F':
                        letter = '7'; break;
                    case 'E':
                        letter = '6'; break;
                    case 'D':
                        letter = '5'; break;
                    case 'C':
                        letter = '4'; break;
                    case 'B':
                        letter = '3'; break;
                    case 'A':
                        letter = '2'; break;
                    case '9':
                        letter = '1'; break;
                    case '8':
                    default:
                        letter = '0'; break;
                }
                letters[0] = letter;
                return string.Join("", letters);
            }
            return str;
        }



        private int hex_val(char chr) // convert single hex digit to int value
        {
            switch (chr)
            {
                case 'F':
                    return 15;
                case 'E':
                    return 14;
                case 'D':
                    return 13;
                case 'C':
                    return 12;
                case 'B':
                    return 11;
                case 'A':
                    return 10;
                case '9':
                    return 9;
                case '8':
                    return 8;
                case '7':
                    return 7;
                case '6':
                    return 6;
                case '5':
                    return 5;
                case '4':
                    return 4;
                case '3':
                    return 3;
                case '2':
                    return 2;
                case '1':
                    return 1;
                case '0':
                default:
                    return 0;
            }
        }

        private void label5_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        { // key presses on main form go here
            int selection = pal_x + (pal_y * 16);

            if (e.KeyCode == Keys.Left)
            {
                Checkpoint();
                e.IsInputKey = true;
                Tiles.shift_left();
            }
            else if (e.KeyCode == Keys.Up)
            {
                Checkpoint();
                e.IsInputKey = true;
                Tiles.shift_up();
            }
            else if (e.KeyCode == Keys.Right)
            {
                Checkpoint();
                e.IsInputKey = true;
                Tiles.shift_right();
            }
            else if (e.KeyCode == Keys.Down)
            {
                Checkpoint();
                e.IsInputKey = true;
                Tiles.shift_down();
            }

            else if (e.KeyCode == Keys.NumPad2)
            {
                if (tile_y < 15) tile_y++;
                tile_num = (tile_y * 16) + tile_x;
            }
            else if (e.KeyCode == Keys.NumPad4)
            {
                if (tile_x > 0) tile_x--;
                tile_num = (tile_y * 16) + tile_x;
            }
            else if (e.KeyCode == Keys.NumPad6)
            {
                if (tile_x < 15) tile_x++;
                tile_num = (tile_y * 16) + tile_x;
            }
            else if (e.KeyCode == Keys.NumPad8)
            {
                if (tile_y > 0) tile_y--;
                tile_num = (tile_y * 16) + tile_x;
            }
            else if (e.KeyCode == Keys.H)
            {
                Checkpoint();
                Tiles.tile_h_flip();
            }
            else if (e.KeyCode == Keys.V)
            {
                Checkpoint();
                Tiles.tile_v_flip();
            }
            else if (e.KeyCode == Keys.R)
            {
                Checkpoint();
                Tiles.tile_rot_cw();
            }
            else if (e.KeyCode == Keys.L)
            {
                Checkpoint();
                Tiles.tile_rot_ccw();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                Checkpoint();
                Tiles.tile_delete();
            }
            else if (e.KeyCode == Keys.C)
            {
                Tiles.tile_copy();
            }
            else if (e.KeyCode == Keys.P)
            {
                Checkpoint();
                Tiles.tile_paste();
            }
            else if (e.KeyCode == Keys.F)
            {
                Checkpoint();
                Tiles.tile_fill();
            }
            else if (e.KeyCode == Keys.Q)
            { // copy selected color
                pal_r_copy = Palettes.pal_r[selection];
                pal_g_copy = Palettes.pal_g[selection];
                pal_b_copy = Palettes.pal_b[selection];
            }
            else if (e.KeyCode == Keys.W)
            { // paste selected to color
                Palettes.pal_r[selection] = (byte)pal_r_copy;
                Palettes.pal_g[selection] = (byte)pal_g_copy;
                Palettes.pal_b[selection] = (byte)pal_b_copy;
                update_palette();
                rebuild_pal_boxes();
            }
            else if (e.KeyCode == Keys.E)
            { // clear selected to color
                Palettes.pal_r[selection] = 0;
                Palettes.pal_g[selection] = 0;
                Palettes.pal_b[selection] = 0;
                update_palette();
                rebuild_pal_boxes();
            }

            else if (e.KeyCode == Keys.D1) // number key 1
            {
                set1_change(); // change the tileset
            }
            else if (e.KeyCode == Keys.D2)
            {
                set2_change();
            }
            else if (e.KeyCode == Keys.Z)
            {
                Do_Undo();
            }


            common_update2();
            // prevent change in focus
            label5.Focus();
        }

        


        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        { // rename metasprite
            if (listBox1.SelectedIndex < 0) return;
            selected_meta = listBox1.SelectedIndex;

            if (e.KeyChar == (char)Keys.Return)
            {
                update_textbox6();

                e.Handled = true; // prevent ding on return press
            }
        }


        private void textBox6_Leave(object sender, EventArgs e)
        {
            update_textbox6();
        }


        public void update_textbox6()
        { // rename metasprite
            Checkpoint();

            MetaspriteArray[selected_meta].name = textBox6.Text;

            cancel_index_change = true;
            // I think it was calling the index changed event
            listBox1.Items[selected_meta] = textBox6.Text;
            cancel_index_change = false;
        }


        private void textBox7_KeyPress(object sender, KeyPressEventArgs e)
        { // priority for whole metasprite
            //int temp_val = 0;
            if (e.KeyChar == (char)Keys.Return)
            {
                update_textbox7();

                e.Handled = true; // prevent ding on return press
            }
        }


        private void textBox7_Leave(object sender, EventArgs e)
        {
            update_textbox7();
        }


        public void update_textbox7()
        { // priority all
            Checkpoint();

            int temp_val = 0;
            string str = textBox7.Text;
            if (str.Length > 1) return; // should be 1 digit
            char testchr = str[0];
            if ((testchr < '0') || (testchr > '3'))
            {
                textBox7.Text = "0";
                MetaspriteArray[selected_meta].priority = 0;
                return;
            }
            //else we have a number between 0-3

            textBox7.Text = str;
            int.TryParse(str, out temp_val);
            MetaspriteArray[selected_meta].priority = temp_val;
        }

        

        private void rebuild_one_item()
        {
            if (listBox2.SelectedIndex < 0) return; // just error checking
            selected_spr = listBox2.SelectedIndex;

            string str = "";
            str = "tile ";
            int tile_sel = MetaspriteArray[selected_meta].tile[selected_spr];
            if (tile_sel < 16) str = str + "0";
            str = str + tile_sel.ToString("X"); //hex
            str = str + "  set=" + MetaspriteArray[selected_meta].set[selected_spr].ToString();
            str = str + "   x=" + MetaspriteArray[selected_meta].rel_x[selected_spr].ToString() +
                "   y=" + MetaspriteArray[selected_meta].rel_y[selected_spr].ToString();
            str = str + "   pal=" + MetaspriteArray[selected_meta].palette[selected_spr].ToString();
            str = str + "   H=" + MetaspriteArray[selected_meta].h_flip[selected_spr].ToString();
            str = str + "   V=" + MetaspriteArray[selected_meta].v_flip[selected_spr].ToString();
            str = str + "   Sz=" + MetaspriteArray[selected_meta].size[selected_spr].ToString();

            listBox2.Items[selected_spr] = str;
        }


        

        private void button10_Click(object sender, EventArgs e)
        { // delete selected tile
            if ((MetaspriteArray[selected_meta].sprite_count < 1) ||
               (listBox2.Items.Count < 1)) return;
            if (listBox2.SelectedItems.Count > 1) // don't allow multi
            {
                MessageBox.Show("Select only one sprite.");
                return;
            }

            Checkpoint();

            if (MetaspriteArray[selected_meta].sprite_count == 1)
            {
                // only 1 item, just delete all
                MetaspriteArray[selected_meta].sprite_count = 0;
                listBox2.Items.Clear();
                listBox2.Refresh();
                selected_spr = 0;
                label19.Text = "00";
            }
            else
            {
                //shift all sprite data up one.
                selected_spr = listBox2.SelectedIndex;
                int max_count = MetaspriteArray[selected_meta].sprite_count;
                int plus_one;
                for (int i = selected_spr; i < 99; i++)
                {
                    plus_one = i + 1;
                    if (plus_one >= max_count) break;
                    MetaspriteArray[selected_meta].tile[i] = MetaspriteArray[selected_meta].tile[plus_one];
                    MetaspriteArray[selected_meta].set[i] = MetaspriteArray[selected_meta].set[plus_one];
                    MetaspriteArray[selected_meta].rel_x[i] = MetaspriteArray[selected_meta].rel_x[plus_one];
                    MetaspriteArray[selected_meta].rel_y[i] = MetaspriteArray[selected_meta].rel_y[plus_one];
                    MetaspriteArray[selected_meta].palette[i] = MetaspriteArray[selected_meta].palette[plus_one];
                    MetaspriteArray[selected_meta].h_flip[i] = MetaspriteArray[selected_meta].h_flip[plus_one];
                    MetaspriteArray[selected_meta].v_flip[i] = MetaspriteArray[selected_meta].v_flip[plus_one];
                    MetaspriteArray[selected_meta].size[i] = MetaspriteArray[selected_meta].size[plus_one];
                }
                // decrease the sprite count
                MetaspriteArray[selected_meta].sprite_count = MetaspriteArray[selected_meta].sprite_count - 1;
            }

            listBox2.ClearSelected(); // fix crash
            rebuild_spr_list();
            
            if(selected_spr < listBox2.Items.Count)
            {
                listBox2.SelectedIndex = selected_spr;
            }
            else
            {
                listBox2.SelectedIndex = listBox2.Items.Count - 1;
            }
            selected_spr = listBox2.SelectedIndex;
            if (selected_spr < 0) selected_spr = 0;
            string str = "";
            if (selected_spr < 10) str = "0";
            str = str + selected_spr.ToString(); //hex
            label19.Text = str;

            update_metatile_image();
            label5.Focus();
        }

        private void button11_Click(object sender, EventArgs e)
        { // delete all tiles from metatile
            Checkpoint();

            MetaspriteArray[selected_meta].sprite_count = 0;
            listBox2.Items.Clear();
            listBox2.Refresh();
            selected_spr = 0;
            label19.Text = "00";
            update_metatile_image();
            label5.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        { // color picker
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                Color tempcolor = colorDialog1.Color;
                int red = tempcolor.R & 0xf8;
                int green = tempcolor.G & 0xf8;
                int blue = tempcolor.B & 0xf8;
                textBox1.Text = red.ToString();
                textBox2.Text = green.ToString();
                textBox3.Text = blue.ToString();
                update_rgb();
                update_box4();
                update_palette();
                common_update2();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        { // main metasprite selection box top
            if (listBox1.SelectedIndex < 0) return;
            if (cancel_index_change) return;

            undo_ready = false;

            selected_meta = listBox1.SelectedIndex;
            string str = "";
            if (selected_meta < 10) str = "0";
            str = str + selected_meta.ToString();
            label10.Text = str;

            textBox6.Text = MetaspriteArray[selected_meta].name;
            
            textBox7.Text = MetaspriteArray[selected_meta].priority.ToString();

            listBox2.Items.Clear(); // fix crash in rebuild.
            rebuild_spr_list();
            if(listBox2.Items.Count >= 1)
            {
                listBox2.SelectedIndex = 0;
            }
            selected_spr = 0;
            label19.Text = "00";
            update_metatile_image();
            label5.Focus();
        }

        private void rebuild_spr_list()
        {
            for(int i = 0; i < 100; i++)
            {
                sel_array[i] = 0;
            }
            foreach (int index in listBox2.SelectedIndices)
            {
                sel_array[index] = 1;
            }
            listBox2.Items.Clear();
            int count = MetaspriteArray[selected_meta].sprite_count;
            int tile_sel;
            string str = "";
            if (count > 0)
            {
                for(int i = 0; i < count; i++)
                {
                    str = "tile ";
                    tile_sel = MetaspriteArray[selected_meta].tile[i];
                    if (tile_sel < 16) str = str + "0";
                    str = str + tile_sel.ToString("X"); //hex
                    str = str + "  set=" + MetaspriteArray[selected_meta].set[i].ToString();
                    str = str + "   x=" + MetaspriteArray[selected_meta].rel_x[i].ToString() +
                        "   y=" + MetaspriteArray[selected_meta].rel_y[i].ToString();
                    str = str + "   pal=" + MetaspriteArray[selected_meta].palette[i].ToString();
                    str = str + "   H=" + MetaspriteArray[selected_meta].h_flip[i].ToString();
                    str = str + "   V=" + MetaspriteArray[selected_meta].v_flip[i].ToString();
                    str = str + "   Sz=" + MetaspriteArray[selected_meta].size[i].ToString();

                    listBox2.Items.Add(str);
                }
                listBox2.Refresh();

                for (int i = 0; i < 100; i++)
                {
                    if(sel_array[i] == 1)
                    {
                        listBox2.SetSelected(i,true);
                    }
                }

                // set the check boxes to the top item
                textBox5.Text = MetaspriteArray[selected_meta].palette[0].ToString();
            }
            else // count is zero
            {
                textBox5.Text = "0";
            }
        }


        private void common_update2()
        {
            
            if (newChild != null)
            {
                newChild.update_tile_box();
            }

            update_tile_image();
            update_metatile_image();
        }

        private void button13_Click(object sender, EventArgs e)
        { // select all
            int how_many = listBox2.Items.Count;
            if (how_many < 1) return;
            for(int i = 0; i < how_many; i++)
            {
                listBox2.SetSelected(i, true);
            }
            rebuild_spr_list();
            label5.Focus();
        }

        private void trackBar1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int val = trackBar1.Value * 8;
                textBox1.Text = val.ToString();

                update_rgb();
                update_box4();

                update_palette();

                common_update2();
                label5.Focus();
            }
        }

        private void trackBar2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int val = trackBar2.Value * 8;
                textBox2.Text = val.ToString();

                update_rgb();
                update_box4();

                update_palette();

                common_update2();
                label5.Focus();
            }
        }

        private void trackBar3_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int val = trackBar3.Value * 8;
                textBox3.Text = val.ToString();

                update_rgb();
                update_box4();

                update_palette();

                common_update2();
                label5.Focus();
            }
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            label5.Focus();
        }

        private void trackBar2_MouseUp(object sender, MouseEventArgs e)
        {
            label5.Focus();
        }


        private void trackBar3_MouseUp(object sender, MouseEventArgs e)
        {
            label5.Focus();
        }

        private void saveTilesInRangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // remember to put the form location as "manual"
            if (newChild4 != null)
            {
                newChild4.BringToFront();
            }
            else
            {
                newChild4 = new Form4();
                newChild4.Owner = this;
                int xx = Screen.PrimaryScreen.Bounds.Width;
                if (this.Location.X + 200 < xx) // set new form location
                {
                    newChild4.Location = new Point(this.Location.X + 100, this.Location.Y + 80);
                }
                else
                {
                    newChild4.Location = new Point(xx - 100, this.Location.Y);
                }

                newChild4.Show();
                //update
            }
        }

        private void loadToSelectedTileToolStripMenuItem_Click(object sender, EventArgs e)
        { // TILES / Load to Selected Tile
            int[] bit1 = new int[8]; // bit planes
            int[] bit2 = new int[8];
            int[] bit3 = new int[8];
            int[] bit4 = new int[8];
            int temp1, temp2, temp3, temp4;
            int[] temp_tiles = new int[0x4000]; // 32 per tile * 512 tiles
            int size_temp_tiles = 0;

            // tile_set 0 or 1
            int offset_tiles_ar = (tile_x * 64) + (tile_y * 1024) + (tile_set * 0x4000);

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Load tiles to the selected tile";
            openFileDialog1.Filter = "Tileset (*.chr)|*.chr|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                
                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();
                if (fs.Length >= 32) // at least one tile.
                {
                    Checkpoint();

                    size_temp_tiles = (int)fs.Length & 0xffe0; // round down
                    if (size_temp_tiles > 0x4000) size_temp_tiles = 0x4000; // max
                    // copy file to the temp array.
                    for (int i = 0; i < size_temp_tiles; i++)
                    {
                        temp_tiles[i] = (byte)fs.ReadByte();
                    }

                    int num_loops;
                    int chr_index = 0;


                    num_loops = size_temp_tiles / 32; // 32 bytes per tile
                    for (int i = 0; i < num_loops; i++)
                    {
                        // get 32 bytes per tile
                        for (int y = 0; y < 8; y++) // get 8 sets of bitplanes
                        {
                            // get the 4 bitplanes for each tile row
                            int y2 = y * 2;
                            bit1[y] = temp_tiles[chr_index + y2];
                            bit2[y] = temp_tiles[chr_index + y2 + 1];
                            bit3[y] = temp_tiles[chr_index + y2 + 16];
                            bit4[y] = temp_tiles[chr_index + y2 + 17];

                            for (int x = 7; x >= 0; x--) // right to left
                            {
                                temp1 = bit1[y] & 1;    // get a bit from each bitplane
                                bit1[y] = bit1[y] >> 1;
                                temp2 = bit2[y] & 1;
                                bit2[y] = bit2[y] >> 1;
                                temp3 = bit3[y] & 1;
                                bit3[y] = bit3[y] >> 1;
                                temp4 = bit4[y] & 1;
                                bit4[y] = bit4[y] >> 1;
                                Tiles.Tile_Arrays[offset_tiles_ar + x] =
                                    (temp4 << 3) + (temp3 << 2) + (temp2 << 1) + temp1;
                            }
                            offset_tiles_ar += 8;
                        }
                        chr_index += 32;

                        //don't go too far, even if more tiles to read
                        if (offset_tiles_ar >= 32768) break; // end of tile array
                    }


                }
                else
                {
                    MessageBox.Show("File size error. Too small.",
                    "File size error", MessageBoxButtons.OK);
                }

                fs.Close();

                common_update2();

                //disable_map_click = 1;  // fix bug, double click causing
                                        // mouse event on tilemap
            }

        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Do_Undo();
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            label5.Focus();
        }

        private void checkBox2_Click(object sender, EventArgs e)
        {
            label5.Focus();
        }

        private void checkBox4_Click(object sender, EventArgs e)
        {
            label5.Focus();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // remember to put the form location as "manual"
            if (newChild3 != null)
            {
                newChild3.BringToFront();
            }
            else
            {
                newChild3 = new Form3();
                newChild3.Owner = this;
                int xx = Screen.PrimaryScreen.Bounds.Width;
                if (this.Location.X + 600 < xx) // set new form location
                {
                    newChild3.Location = new Point(this.Location.X + 500, this.Location.Y + 80);
                }
                else
                {
                    newChild3.Location = new Point(xx - 100, this.Location.Y);
                }

                newChild3.Show();
                //update
            }
        }


        private void getPaletteFromImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // generate a palette from the image
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Filter = "Image Files .png .jpg .bmp .gif)|*.png;*.jpg;*.bmp;*.gif|" + "All Files (*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Bitmap import_bmp = new Bitmap(dlg.FileName);

                    if ((import_bmp.Height < 8) || (import_bmp.Width < 8))
                    {
                        MessageBox.Show("Error. File too small?");
                        import_bmp.Dispose();
                        return;
                    }
                    if ((import_bmp.Height > 128) || (import_bmp.Width > 128))
                    {
                        MessageBox.Show("Error. File too large. 128x128 max.");
                        import_bmp.Dispose();
                        return;
                    }


                    int num_col_to_find, start_offset;
                    int red = 0, blue = 0, green = 0;
                    Color temp_color = import_bmp.GetPixel(0,0);

                    // if we have option "use top left pixel as 0 color"
                    red = temp_color.R & 0xf8;
                    int RememberZeroR = red;
                    green = temp_color.G & 0xf8;
                    int RememberZeroG = green;
                    blue = temp_color.B & 0xf8;
                    int RememberZeroB = blue;


                    num_col_to_find = 16;
                    start_offset = pal_y * 16;


                    image_height = import_bmp.Height;
                    image_width = import_bmp.Width;
                    
                    // copy the bitmap, crop but don't resize
                    // copy pixel by pixel
                    for (int xx = 0; xx < 128; xx++)
                    {
                        for (int yy = 0; yy < 128; yy++)
                        {
                            if ((xx < image_width) && (yy < image_height))
                            {
                                temp_color = import_bmp.GetPixel(xx, yy);
                            }
                            else
                            {
                                temp_color = Color.Gray;
                            }
                            cool_bmp.SetPixel(xx, yy, temp_color);
                        }
                    }



                    int color_found = 0;
                    
                    int temp_var, closest_cnt, added;

                    // default colors

                    // blank the arrays
                    for (int i = 0; i < 65536; i++)
                    {
                        R_Array[i] = 0;
                        G_Array[i] = 0;
                        B_Array[i] = 0;
                        Count_Array[i] = 0;
                    }
                    color_count = 0;

                    Color tempcolor = Color.Black;

                    // read all possible colors from the orig image
                    // removing duplicates, keep track of how many
                    for (int yy = 0; yy < image_height; yy++)
                    {
                        for (int xx = 0; xx < image_width; xx++)
                        {
                            tempcolor = cool_bmp.GetPixel(xx, yy);
                            // speed it up, narrow the possibilities.
                            red = tempcolor.R & 0xf8;
                            blue = tempcolor.G & 0xf8;
                            green = tempcolor.B & 0xf8;
                            tempcolor = Color.FromArgb(red, blue, green);

                            // compare to all other colors, add if not present
                            if (color_count == 0)
                            {
                                Add_Color(tempcolor);
                                continue;
                            }

                            color_found = 0;
                            for (int i = 0; i < color_count; i++)
                            {
                                if ((tempcolor.R == R_Array[i] &&
                                    tempcolor.G == G_Array[i] &&
                                    tempcolor.B == B_Array[i]))
                                { // color match found
                                    Count_Array[i] = Count_Array[i] + 1;
                                    color_found = 1;
                                    break;
                                }
                            }
                            // no color match found
                            if (color_found == 0)
                            {
                                Add_Color(tempcolor);
                            }

                        }
                    }
                    
                    // this mid point algorithm tends avoid extremes
                    // give extra weight to the lowest value and the highest value
                    // first find the darkest and lightest colors
                    int darkest = 999;
                    int darkest_index = 0;
                    int lightest = 0;
                    int lightest_index = 0;
                    for (int i = 0; i < color_count; i++)
                    {
                        added = R_Array[i] + G_Array[i] + B_Array[i];
                        if (added < darkest)
                        {
                            darkest = added;
                            darkest_index = i;
                        }
                        if (added > lightest)
                        {
                            lightest = added;
                            lightest_index = i;
                        }
                    }
                    // give more count to them
                    temp_var = image_width * image_height / 8; // 8 is magic
                    Count_Array[darkest_index] += temp_var;
                    Count_Array[lightest_index] += temp_var;

                    // then reduce to 16 colors, using a mid point merge with
                    // the closest neighbor color

                    int color_count2 = color_count;
                    while (color_count2 > num_col_to_find)
                    {
                        //find the least count
                        int least_index = 0;
                        int least_cnt = 99999;
                        for (int i = 0; i < color_count; i++)
                        {
                            if (Count_Array[i] == 0) continue;
                            if (Count_Array[i] < least_cnt)
                            {
                                least_cnt = Count_Array[i];
                                least_index = i;
                            }
                        }
                        // delete itself
                        Count_Array[least_index] = 0;

                        int closest_index = 0;
                        int closest_val = 999999;
                        r_val = R_Array[least_index];
                        g_val = G_Array[least_index];
                        b_val = B_Array[least_index];
                        int dR = 0, dG = 0, dB = 0;

                        // find the closest to that one
                        for (int i = 0; i < color_count; i++)
                        {
                            if (Count_Array[i] == 0) continue;
                            dR = r_val - R_Array[i];
                            dG = g_val - G_Array[i];
                            dB = b_val - B_Array[i];
                            diff_val = ((dR * dR) + (dG * dG) + (dB * dB));

                            if (diff_val < closest_val)
                            {
                                closest_val = diff_val;
                                closest_index = i;
                            }
                        }

                        closest_cnt = Count_Array[closest_index];

                        // merge closet index with least index, mid point
                        temp_var = (closest_cnt + least_cnt);
                        // the algorithm was (color1 + color2) / 2
                        // but now, multiplied each by their count, div by both counts
                        r_val = (R_Array[least_index] * least_cnt) + (R_Array[closest_index] * closest_cnt);
                        r_val = (int)Math.Round((double)r_val / temp_var);
                        g_val = (G_Array[least_index] * least_cnt) + (G_Array[closest_index] * closest_cnt);
                        g_val = (int)Math.Round((double)g_val / temp_var);
                        b_val = (B_Array[least_index] * least_cnt) + (B_Array[closest_index] * closest_cnt);
                        b_val = (int)Math.Round((double)b_val / temp_var);
                        R_Array[closest_index] = r_val;
                        G_Array[closest_index] = g_val;
                        B_Array[closest_index] = b_val;
                        Count_Array[closest_index] = closest_cnt + least_cnt;

                        color_count2--;

                    }

                    // always palette zero
                    // zero fill the palette, before filling (black)
                    for (int i = 0; i < num_col_to_find; i++)
                    {
                        int j = start_offset + i;
                        Palettes.pal_r[j] = 0;
                        Palettes.pal_g[j] = 0;
                        Palettes.pal_b[j] = 0;
                    }
                    // then go through the array and pull out 16 numbers
                    int findindex = 0;
                    int color_count3 = 0;
                    while (color_count3 < color_count2)
                    {
                        if (Count_Array[findindex] != 0)
                        {
                            SixteenColorIndexes[color_count3] = findindex;
                            color_count3++;
                        }

                        findindex++;
                        if (findindex >= 65536) break;

                    }

                    // then sort by darkness
                    for (int i = 0; i < 16; i++) // zero them
                    {
                        SixteenColorsAdded[i] = 0;
                    }
                    for (int i = 0; i < color_count2; i++) // add them up (rough brightness)
                    {
                        SixteenColorsAdded[i] += R_Array[SixteenColorIndexes[i]];
                        SixteenColorsAdded[i] += G_Array[SixteenColorIndexes[i]];
                        SixteenColorsAdded[i] += B_Array[SixteenColorIndexes[i]];
                    }
                    int temp_val;
                    while (true)
                    {
                        bool sorted = true;
                        for (int i = 0; i < color_count2 - 1; i++) // add them up (rough brightness)
                        {
                            if (SixteenColorsAdded[i] > SixteenColorsAdded[i + 1])
                            {
                                sorted = false;
                                // swap them
                                temp_val = SixteenColorsAdded[i];
                                SixteenColorsAdded[i] = SixteenColorsAdded[i + 1];
                                SixteenColorsAdded[i + 1] = temp_val;
                                temp_val = SixteenColorIndexes[i];
                                SixteenColorIndexes[i] = SixteenColorIndexes[i + 1];
                                SixteenColorIndexes[i + 1] = temp_val;
                            }
                        }
                        if (sorted == true) break;
                    }


                    // then fill the palette with the colors
                    for (int i = 0; i < color_count2; i++)
                    {
                        int j = start_offset + i;
                        Palettes.pal_r[j] = (byte)(R_Array[SixteenColorIndexes[i]] & 0xf8);
                        Palettes.pal_g[j] = (byte)(G_Array[SixteenColorIndexes[i]] & 0xf8);
                        Palettes.pal_b[j] = (byte)(B_Array[SixteenColorIndexes[i]] & 0xf8);
                    }



                    // if checkbox to use top left pixel as transparent, shift that color in place.
                    // review this. could be buggy.
                    if (f3_cb1 == true)
                    {
                        tempcolor = Color.FromArgb(RememberZeroR, RememberZeroG, RememberZeroB);
                        int remove_index = Best_Color(tempcolor, num_col_to_find, start_offset);
                        // we have 1 too many color, remove the one closest to the transparent color
                        // from before... shuffle the lower colors upward 1 slot
                        for (int i = remove_index; i > 0; i--)
                        {
                            int j = start_offset + i;
                            Palettes.pal_r[j] = Palettes.pal_r[j - 1];
                            Palettes.pal_g[j] = Palettes.pal_g[j - 1];
                            Palettes.pal_b[j] = Palettes.pal_b[j - 1];
                        }

                        // insert the zero color at the zero offset
                        Palettes.pal_r[start_offset] = (byte)RememberZeroR;
                        Palettes.pal_g[start_offset] = (byte)RememberZeroG;
                        Palettes.pal_b[start_offset] = (byte)RememberZeroB;
                    }


                    // copy the 0th color to zero
                    Palettes.pal_r[0] = Palettes.pal_r[start_offset];
                    Palettes.pal_g[0] = Palettes.pal_g[start_offset];
                    Palettes.pal_b[0] = Palettes.pal_b[start_offset];
                    // then update the palette image
                    update_palette();

                    //update the boxes
                    rebuild_pal_boxes();

                    common_update2();
                    import_bmp.Dispose();
                }
            }

        }


        public void Add_Color(Color tempcolor)
        {
            R_Array[color_count] = tempcolor.R;
            G_Array[color_count] = tempcolor.G;
            B_Array[color_count] = tempcolor.B;
            Count_Array[color_count] = 1;

            color_count++;
        }


        public int Best_Color(Color temp_color, int num_col, int start_offset)
        {
            int best_index = 0;
            int best_count = 99999;

            for (int i = 0; i < num_col; i++)
            {
                int i2 = start_offset + i;
                int red = Palettes.pal_r[i2] - temp_color.R;
                red = Math.Abs(red);
                int green = Palettes.pal_g[i2] - temp_color.G;
                green = Math.Abs(green);
                int blue = Palettes.pal_b[i2] - temp_color.B;
                blue = Math.Abs(blue);
                int sum = red + green + blue;
                if (sum < best_count)
                {
                    best_count = sum;
                    best_index = i;
                }
            }

            return best_index;
        }



        private void getTilesFromImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // import an image 128x128, generate CHR based on existing palette
            // load to the current tileset

            // load image, generate CHR from it
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Filter = "Image Files .png .jpg .bmp .gif)|*.png;*.jpg;*.bmp;*.gif|" + "All Files (*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    

                    Bitmap import_bmp = new Bitmap(dlg.FileName);

                    if ((import_bmp.Height < 8) || (import_bmp.Width < 8))
                    {
                        MessageBox.Show("Error. File too small?");
                        import_bmp.Dispose();
                        return;
                    }
                    if ((import_bmp.Height > 128) || (import_bmp.Width > 128))
                    {
                        MessageBox.Show("Error. File too large. 128x128 max.");
                        import_bmp.Dispose();
                        return;
                    }

                    Checkpoint();

                    int num_col, start_offset;
                    
                    num_col = 16;
                    start_offset = pal_y * 16;

                    // make sure color zero is correct
                    Palettes.pal_r[start_offset] = Palettes.pal_r[0];
                    Palettes.pal_g[start_offset] = Palettes.pal_g[0];
                    Palettes.pal_b[start_offset] = Palettes.pal_b[0];

                    Color trans_color = Color.FromArgb(Palettes.pal_r[0], Palettes.pal_g[0], Palettes.pal_b[0]);

                    image_height = import_bmp.Height;
                    image_width = import_bmp.Width;

                    Color temp_color;
                    // copy the bitmap, crop but don't resize
                    // copy pixel by pixel
                    for (int xx = 0; xx < 128; xx++)
                    {
                        for (int yy = 0; yy < 128; yy++)
                        {
                            if ((xx < image_width) && (yy < image_height))
                            {
                                temp_color = import_bmp.GetPixel(xx, yy);
                            }
                            else
                            {
                                temp_color = trans_color;
                            }
                            cool_bmp.SetPixel(xx, yy, temp_color);
                        }
                    }


                    int final_y, final_x, best_index, chr_index, tile_num, pixel_num;
                    //int temp_set = 0;
                    int count = 0;

                    // get best color for each pixel
                    // copied to int array, needy_chr_array

                    // was 28.0
                    dither_db = dither_factor / 20.0;

                    dither_adjust = (int)(dither_db * 32.0);
                    int red, green, blue, bayer_val;

                    for (int y = 0; y < 256; y++)
                    {
                        for (int x = 0; x < 256; x++)
                        {
                            if ((x >= image_width) || (y >= image_height))
                            {
                                needy_chr_array[count] = 0;
                            }
                            else
                            {
                                // get the pixel and find its best color
                                temp_color = cool_bmp.GetPixel(x, y);

                                if (dither_factor != 0)
                                {
                                    // add dithering
                                    red = temp_color.R - dither_adjust; // keep it from lightening
                                    green = temp_color.G - dither_adjust;
                                    blue = temp_color.B - dither_adjust;
                                    bayer_val = BAYER_MATRIX[x % 8, y % 8];
                                    bayer_val = (int)((double)bayer_val * dither_db);
                                    red += bayer_val;
                                    red = Math.Max(0, red); // clamp min max
                                    red = Math.Min(255, red);
                                    green += bayer_val;
                                    green = Math.Max(0, green);
                                    green = Math.Min(255, green);
                                    blue += bayer_val;
                                    blue = Math.Max(0, blue);
                                    blue = Math.Min(255, blue);
                                    temp_color = Color.FromArgb(red, green, blue);
                                }

                                best_index = Best_Color(temp_color, num_col, start_offset);
                                needy_chr_array[count] = best_index;
                            }

                            count++;
                        }
                    }

                    // copy image to CHR
                    tile_num = 0;
                    int tile_offset = tile_set * 0x4000;

                    for (int y1 = 0; y1 < 128; y1 += 8) // 16 tiles of 8x8
                    {
                        for (int x1 = 0; x1 < 128; x1 += 8) // ditto
                        {
                            for (int y2 = 0; y2 < 8; y2++) // 8 pixels tall
                            {
                                for (int x2 = 0; x2 < 8; x2++) // 8 pixels wide
                                {
                                    final_x = x1 + x2;
                                    final_y = y1 + y2;

                                    pixel_num = (final_y * 256) + final_x;
                                    // 64 bytes per tile
                                    chr_index = (tile_num * 64) + (y2 * 8) + x2;
                                    chr_index += tile_offset;

                                    Tiles.Tile_Arrays[chr_index] = needy_chr_array[pixel_num];
                                }
                            }
                            tile_num++;
                        }
                    }

                    
                    // redraw everything
                    common_update2();

                    import_bmp.Dispose();
                }
            }


        }



        public void update_tile_image() // redraw the visible tileset
        {
            Color temp_color;
            int temp_tile_num = 0;
            for (int i = 0; i < 16; i++) //tile row = y
            {
                for (int j = 0; j < 16; j++) //tile column = x
                {
                    temp_tile_num = (i * 16) + j;
                    for (int k = 0; k < 8; k++) // pixel row = y
                    {
                        for (int m = 0; m < 8; m++) // pixel column = x
                        {
                            int color = 0;
                            int index = (Form1.tile_set * 256 * 8 * 8) + (temp_tile_num * 8 * 8) + (k * 8) + m;
                            int pal_index = Tiles.Tile_Arrays[index]; // pixel in tile array
                            
                            //{
                                pal_index = pal_index & 0x0f; //sanitize, for my sanity
                                color = (pal_y * 16) + pal_index;
                            //}

                            temp_color = Color.FromArgb(Palettes.pal_r[color], Palettes.pal_g[color], Palettes.pal_b[color]);
                            image_tiles.SetPixel((j * 8) + m, (i * 8) + k, temp_color);
                        }
                    }
                }
            }

            //Bitmap temp_bmp = new Bitmap(256, 256); //resize double size
            using (Graphics g = Graphics.FromImage(temp_bmp))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half; // fix bug, missing half a pixel on top and left
                g.DrawImage(image_tiles, 0, 0, 256, 256);
            } // standard resize of bmp was blurry, this makes it sharp

            //put a white box around the selected tile
            int pos_x = 0; int pos_y = 0;
            for (int i = 0; i < 16; i++)
            {
                pos_y = (tile_y * 16) - 1; // it's doing a weird off by 1 thing
                if (pos_y < 0) pos_y = 0; // so have to adjust by 1, and not == -1
                pos_x = (tile_x * 16) - 1;
                if (pos_x < 0) pos_x = 0;
                temp_bmp.SetPixel(pos_x + i, pos_y, Color.White);
                temp_bmp.SetPixel(pos_x, pos_y + i, Color.White);
                temp_bmp.SetPixel(pos_x + i, pos_y + 15, Color.White);
                temp_bmp.SetPixel(pos_x + 15, pos_y + i, Color.White);
            }
            pictureBox2.Image = temp_bmp;
            pictureBox2.Refresh();
            
        } // END REDRAW TILESET


        private void checkBox6_MouseUp(object sender, MouseEventArgs e)
        { // highlight selected
            update_metatile_image();
            label5.Focus();
        }

        

        public void update_metatile_image()
        {
            Color Fred = Color.FromArgb(Palettes.pal_r[0], Palettes.pal_g[0], Palettes.pal_b[0]);
            // good old Fred
            int add_them = Palettes.pal_r[0] + Palettes.pal_g[0] + Palettes.pal_b[0];
            Color Fred2 = Color.White;
            if (add_them > 384) Fred2 = Color.Black;

            for (int i = 0; i < 160; i++)
            {
                for(int j = 0; j < 160; j++)
                {
                    image_meta.SetPixel(i, j, Fred);
                }
            }

            // loop through all the tiles in current metatile
            // in reverse order and draw them to the image
            int max_sprite = MetaspriteArray[selected_meta].sprite_count;
            if (max_sprite > 0)
            {
                for (int i = 99; i >= 0; i--)
                {
                    if (i >= max_sprite) continue;

                    if (MetaspriteArray[selected_meta].size[i] == 0)
                    { // small
                        switch (spr_size_mode)
                        {
                            default:
                            case SIZES_8_16:
                                draw_8(i);
                                break;
                            case SIZES_8_32:
                                draw_8(i);
                                break;
                            case SIZES_8_64:
                                draw_8(i);
                                break;
                            case SIZES_16_32:
                                draw_16(i);
                                break;
                            case SIZES_16_64:
                                draw_16(i);
                                break;
                            case SIZES_32_64:
                                draw_32(i);
                                break;
                        }
                    }
                    else
                    { // large
                        switch (spr_size_mode)
                        {
                            default:
                            case SIZES_8_16:
                                draw_16(i);
                                break;
                            case SIZES_8_32:
                                draw_32(i);
                                break;
                            case SIZES_8_64:
                                draw_64(i);
                                break;
                            case SIZES_16_32:
                                draw_32(i);
                                break;
                            case SIZES_16_64:
                                draw_64(i);
                                break;
                            case SIZES_32_64:
                                draw_64(i);
                                break;
                        }
                    }
                }
            }
            

            //double the size
            using (Graphics g = Graphics.FromImage(temp_bmp2))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(image_meta, 0, 0, 320, 320);
            } // standard resize of bmp was blurry, this makes it sharp

            //draw grid lines
            if(checkBox3.Checked == true)
            {
                for (int i = 0; i < 320; i++)
                {
                    if (i % 4 < 2)
                    {
                        temp_bmp2.SetPixel(128, i, Fred2);
                        temp_bmp2.SetPixel(i, 128, Fred2);
                    }
                        

                    if (i > 255) continue;
                    if(i%4 == 1)
                    {
                        temp_bmp2.SetPixel(32, i, Color.Gray);
                        temp_bmp2.SetPixel(64, i, Color.Gray);
                        temp_bmp2.SetPixel(96, i, Color.Gray);
                        temp_bmp2.SetPixel(160, i, Color.Gray);
                        temp_bmp2.SetPixel(192, i, Color.Gray);
                        temp_bmp2.SetPixel(224, i, Color.Gray);
                        temp_bmp2.SetPixel(256, i, Color.Gray);

                        temp_bmp2.SetPixel(i, 32, Color.Gray);
                        temp_bmp2.SetPixel(i, 64, Color.Gray);
                        temp_bmp2.SetPixel(i, 96, Color.Gray);
                        temp_bmp2.SetPixel(i, 160, Color.Gray);
                        temp_bmp2.SetPixel(i, 192, Color.Gray);
                        temp_bmp2.SetPixel(i, 224, Color.Gray);
                        temp_bmp2.SetPixel(i, 256, Color.Gray);
                    }
                }
            }

            if(checkBox6.Checked == true) // highlight selected
            {
                int x_min, y_min, sp_size;
                selected_spr = listBox2.SelectedIndex;
                if (selected_spr >= 0)
                {
                    x_min = MetaspriteArray[selected_meta].rel_x[selected_spr] + 64;
                    x_min = x_min * 2;
                    y_min = MetaspriteArray[selected_meta].rel_y[selected_spr] + 64;
                    y_min = y_min * 2;
                    if (MetaspriteArray[selected_meta].size[selected_spr] == 0)
                    {
                        switch (spr_size_mode)
                        {
                            default:
                            case SIZES_8_16:
                                sp_size = 16; // note, all double size
                                break;
                            case SIZES_8_32:
                                sp_size = 16;
                                break;
                            case SIZES_8_64:
                                sp_size = 16;
                                break;
                            case SIZES_16_32:
                                sp_size = 32;
                                break;
                            case SIZES_16_64:
                                sp_size = 32;
                                break;
                            case SIZES_32_64:
                                sp_size = 64;
                                break;
                        }
                    }
                    else
                    {
                        switch (spr_size_mode)
                        {
                            default:
                            case SIZES_8_16:
                                sp_size = 32;
                                break;
                            case SIZES_8_32:
                                sp_size = 64;
                                break;
                            case SIZES_8_64:
                                sp_size = 128;
                                break;
                            case SIZES_16_32:
                                sp_size = 64;
                                break;
                            case SIZES_16_64:
                                sp_size = 128;
                                break;
                            case SIZES_32_64:
                                sp_size = 128;
                                break;
                        }
                    }
                    
                    //now draw the highlight box
                    for (int i = 0; i < sp_size; i++)
                    {
                        if(x_min + i < 319)
                        {
                            temp_bmp2.SetPixel(x_min + i, y_min, Fred2);
                            if(y_min + sp_size < 319)
                            {
                                temp_bmp2.SetPixel(x_min + i, y_min + sp_size, Fred2);
                            }
                            
                        }
                        if (y_min + i < 319)
                        {
                            temp_bmp2.SetPixel(x_min, y_min + i, Fred2);
                            if (x_min + sp_size < 319)
                            {
                                temp_bmp2.SetPixel(x_min + sp_size, y_min + i, Fred2);
                            }
                        }
                    }
                }
            }
            
            pictureBox1.Image = temp_bmp2;
            pictureBox1.Refresh();
        } // end of metatile update

        

        private void draw_8(int sprite_num)
        {
            int local_x, local_y, local_tile, local_pal;

            local_pal = MetaspriteArray[selected_meta].palette[sprite_num];
            local_x = MetaspriteArray[selected_meta].rel_x[sprite_num] + 65;
            local_y = MetaspriteArray[selected_meta].rel_y[sprite_num] + 65;
            local_tile = MetaspriteArray[selected_meta].tile[sprite_num] +
                (MetaspriteArray[selected_meta].set[sprite_num] << 8);

            if (MetaspriteArray[selected_meta].h_flip[sprite_num] == 0)
            {
                if (MetaspriteArray[selected_meta].v_flip[sprite_num] == 0)
                { // standard
                    draw_1_std(local_x, local_y, local_tile, local_pal);
                }
                else
                { // v flipped
                    draw_1_vflip(local_x, local_y, local_tile, local_pal);
                }
            }
            else
            {
                if (MetaspriteArray[selected_meta].v_flip[sprite_num] == 0)
                { // h flipped
                    draw_1_hflip(local_x, local_y, local_tile, local_pal);
                }
                else
                { // h+v flipped
                    draw_1_Hvflip(local_x, local_y, local_tile, local_pal);
                }
            }
        }

        // NOTE, wrapping always goes to same N table
		// and xf wraps backwards to x0 of same x
        // T, T+1, T+16, T+17
        // 1ff, 1f0
        // 10f, 100
        // split lower and upper nibbles and adjust them separately
        private void draw_16(int sprite_num)
        {
            int local_x, local_y, local_tile, local_pal;
            int local_x2, local_y2, low_nibble, upper_nibble;
            int local_set = MetaspriteArray[selected_meta].set[sprite_num] << 8;
            int local_tile2;

            local_pal = MetaspriteArray[selected_meta].palette[sprite_num];
            local_x = MetaspriteArray[selected_meta].rel_x[sprite_num] + 65;
            local_y = MetaspriteArray[selected_meta].rel_y[sprite_num] + 65;
            local_tile = MetaspriteArray[selected_meta].tile[sprite_num] +
                (MetaspriteArray[selected_meta].set[sprite_num] * 256);
            
            for(int loopy = 0; loopy < 2; loopy++) //y
            {
                for(int loopx = 0; loopx < 2; loopx++) //x
                {
                    low_nibble = (local_tile + loopx) & 0x0f;
                    upper_nibble = (local_tile + (loopy * 0x10)) & 0xf0;
                    local_tile2 = low_nibble + upper_nibble + local_set;

                    if (MetaspriteArray[selected_meta].h_flip[sprite_num] == 0)
                    {
                        if (MetaspriteArray[selected_meta].v_flip[sprite_num] == 0)
                        { // standard
                            local_x2 = local_x + (loopx * 8);
                            local_y2 = local_y + (loopy * 8);

                            draw_1_std(local_x2, local_y2, local_tile2, local_pal);
                            
                        }
                        else
                        { // v flipped
                            local_x2 = local_x + (loopx * 8);
                            local_y2 = local_y + 8 - (loopy * 8);

                            draw_1_vflip(local_x2, local_y2, local_tile2, local_pal);
                            
                        }
                    }
                    else
                    {
                        if (MetaspriteArray[selected_meta].v_flip[sprite_num] == 0)
                        { // h flipped
                            local_x2 = local_x + 8 - (loopx * 8);
                            local_y2 = local_y + (loopy * 8);
                            
                            draw_1_hflip(local_x2, local_y2, local_tile2, local_pal);
                            
                        }
                        else
                        { // h+v flipped
                            local_x2 = local_x + 8 - (loopx * 8);
                            local_y2 = local_y + 8 - (loopy * 8);

                            draw_1_Hvflip(local_x2, local_y2, local_tile2, local_pal);
                            
                        }
                    }
                }
            }

            
        }

        private void draw_32(int sprite_num)
        {
            int local_x, local_y, local_tile, local_pal;
            int local_x2, local_y2, low_nibble, upper_nibble;
            int local_set = MetaspriteArray[selected_meta].set[sprite_num] << 8;
            int local_tile2;

            local_pal = MetaspriteArray[selected_meta].palette[sprite_num];
            local_x = MetaspriteArray[selected_meta].rel_x[sprite_num] + 65;
            local_y = MetaspriteArray[selected_meta].rel_y[sprite_num] + 65;
            local_tile = MetaspriteArray[selected_meta].tile[sprite_num] +
                (MetaspriteArray[selected_meta].set[sprite_num] * 256);

            for (int loopy = 0; loopy < 4; loopy++) //y
            {
                for (int loopx = 0; loopx < 4; loopx++) //x
                {
                    low_nibble = (local_tile + loopx) & 0x0f;
                    upper_nibble = (local_tile + (loopy * 0x10)) & 0xf0;
                    local_tile2 = low_nibble + upper_nibble + local_set;

                    if (MetaspriteArray[selected_meta].h_flip[sprite_num] == 0)
                    {
                        if (MetaspriteArray[selected_meta].v_flip[sprite_num] == 0)
                        { // standard
                            local_x2 = local_x + (loopx * 8);
                            local_y2 = local_y + (loopy * 8);

                            draw_1_std(local_x2, local_y2, local_tile2, local_pal);

                        }
                        else
                        { // v flipped
                            local_x2 = local_x + (loopx * 8);
                            local_y2 = local_y + 24 - (loopy * 8);

                            draw_1_vflip(local_x2, local_y2, local_tile2, local_pal);

                        }
                    }
                    else
                    {
                        if (MetaspriteArray[selected_meta].v_flip[sprite_num] == 0)
                        { // h flipped
                            local_x2 = local_x + 24 - (loopx * 8);
                            local_y2 = local_y + (loopy * 8);

                            draw_1_hflip(local_x2, local_y2, local_tile2, local_pal);

                        }
                        else
                        { // h+v flipped
                            local_x2 = local_x + 24 - (loopx * 8);
                            local_y2 = local_y + 24 - (loopy * 8);

                            draw_1_Hvflip(local_x2, local_y2, local_tile2, local_pal);

                        }
                    }
                }
            }
        }

        private void draw_64(int sprite_num)
        {
            int local_x, local_y, local_tile, local_pal;
            int local_x2, local_y2, low_nibble, upper_nibble;
            int local_set = MetaspriteArray[selected_meta].set[sprite_num] << 8;
            int local_tile2;

            local_pal = MetaspriteArray[selected_meta].palette[sprite_num];
            local_x = MetaspriteArray[selected_meta].rel_x[sprite_num] + 65;
            local_y = MetaspriteArray[selected_meta].rel_y[sprite_num] + 65;
            local_tile = MetaspriteArray[selected_meta].tile[sprite_num] +
                (MetaspriteArray[selected_meta].set[sprite_num] * 256);

            for (int loopy = 0; loopy < 8; loopy++) //y
            {
                for (int loopx = 0; loopx < 8; loopx++) //x
                {
                    low_nibble = (local_tile + loopx) & 0x0f;
                    upper_nibble = (local_tile + (loopy * 0x10)) & 0xf0;
                    local_tile2 = low_nibble + upper_nibble + local_set;

                    if (MetaspriteArray[selected_meta].h_flip[sprite_num] == 0)
                    {
                        if (MetaspriteArray[selected_meta].v_flip[sprite_num] == 0)
                        { // standard
                            local_x2 = local_x + (loopx * 8);
                            local_y2 = local_y + (loopy * 8);

                            draw_1_std(local_x2, local_y2, local_tile2, local_pal);

                        }
                        else
                        { // v flipped
                            local_x2 = local_x + (loopx * 8);
                            local_y2 = local_y + 56 - (loopy * 8);

                            draw_1_vflip(local_x2, local_y2, local_tile2, local_pal);

                        }
                    }
                    else
                    {
                        if (MetaspriteArray[selected_meta].v_flip[sprite_num] == 0)
                        { // h flipped
                            local_x2 = local_x + 56 - (loopx * 8);
                            local_y2 = local_y + (loopy * 8);

                            draw_1_hflip(local_x2, local_y2, local_tile2, local_pal);

                        }
                        else
                        { // h+v flipped
                            local_x2 = local_x + 56 - (loopx * 8);
                            local_y2 = local_y + 56 - (loopy * 8);

                            draw_1_Hvflip(local_x2, local_y2, local_tile2, local_pal);

                        }
                    }
                }
            }
        }

        // -------------------------------


        private void draw_1_std(int xx, int yy, int tile, int pal)
        {
            int offset = tile * 8 * 8; // tile 0-511
            for (int j = 0; j < 8; j++)
            {
                for (int i = 0; i < 8; i++)
                {
                    int index = Tiles.Tile_Arrays[offset];
                    offset++;
                    if (index == 0) continue;
                    int temp_color = index + (pal * 16);
                    
                    if((xx + i < 160) && (xx + i >= 0) && (yy + j < 160) && (yy + j >= 0))
                    {
                        image_meta.SetPixel(xx + i, yy + j, Color.FromArgb(Palettes.pal_r[temp_color],
                            Palettes.pal_g[temp_color], Palettes.pal_b[temp_color]));
                    }
                    
                }
            }
        }

        private void draw_1_vflip(int xx, int yy, int tile, int pal)
        {
            int offset = tile * 8 * 8; // tile 0-511
            for (int j = 7; j >= 0; j--)
            {
                for (int i = 0; i < 8; i++)
                {
                    int index = Tiles.Tile_Arrays[offset];
                    offset++;
                    if (index == 0) continue;
                    int temp_color = index + (pal * 16);
                    
                    if ((xx + i < 160) && (xx + i >= 0) && (yy + j < 160) && (yy + j >= 0))
                    {
                        image_meta.SetPixel(xx + i, yy + j, Color.FromArgb(Palettes.pal_r[temp_color],
                            Palettes.pal_g[temp_color], Palettes.pal_b[temp_color]));
                    }

                }
            }
        }

        private void draw_1_hflip(int xx, int yy, int tile, int pal)
        {
            int offset = tile * 8 * 8; // tile 0-511
            for (int j = 0; j < 8; j++)
            {
                for (int i = 7; i >= 0; i--)
                {
                    int index = Tiles.Tile_Arrays[offset];
                    offset++;
                    if (index == 0) continue;
                    int temp_color = index + (pal * 16);
                    
                    if ((xx + i < 160) && (xx + i >= 0) && (yy + j < 160) && (yy + j >= 0))
                    {
                        image_meta.SetPixel(xx + i, yy + j, Color.FromArgb(Palettes.pal_r[temp_color],
                            Palettes.pal_g[temp_color], Palettes.pal_b[temp_color]));
                    }

                }
            }
        }

        private void draw_1_Hvflip(int xx, int yy, int tile, int pal)
        {
            int offset = tile * 8 * 8; // tile 0-511
            for (int j = 7; j >= 0; j--)
            {
                for (int i = 7; i >= 0; i--)
                {
                    int index = Tiles.Tile_Arrays[offset];
                    offset++;
                    if (index == 0) continue;
                    int temp_color = index + (pal * 16);
                    
                    if ((xx + i < 160) && (xx + i >= 0) && (yy + j < 160) && (yy + j >= 0))
                    {
                        image_meta.SetPixel(xx + i, yy + j, Color.FromArgb(Palettes.pal_r[temp_color],
                            Palettes.pal_g[temp_color], Palettes.pal_b[temp_color]));
                    }

                }
            }
        }



    }
}
