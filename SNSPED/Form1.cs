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

        public static void close_it()
        {
            newChild = null;
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

        public static Bitmap image_meta = new Bitmap(160, 160);
        public static Bitmap image_tiles = new Bitmap(128, 128);
        public static Bitmap image_pal = new Bitmap(256, 256);
        public static Bitmap temp_bmp = new Bitmap(256, 256); //double size
        public static Bitmap temp_bmp2 = new Bitmap(320, 320); //double size
        public static Bitmap temp_bmp3 = new Bitmap(256, 256); //double size
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
            str = hex_char(tile_y) + hex_char(tile_x);
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

            common_update2();
            label5.Focus();
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
        }


        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        { // palette changer for a tile
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;
            if (listBox2.SelectedIndex < 0) return;

            if (e.KeyChar == (char)Keys.Return)
            {
                string str = textBox5.Text;
                if (str.Length > 1) return; // should be 1 digit
                char testchr = str[0];
                if ((testchr < '0') || (testchr > '7'))
                {
                    return;
                }
                else
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
                e.Handled = true; // prevent ding on return press
            }
        }


        private void button2_Click(object sender, EventArgs e)
        { // h flip
            //flip the selected tile, unless select all, then flip all.
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;
            if (listBox2.SelectedItems.Count < 1) return;
            
            int x_least, x_most, temp1, temp2, temp3;
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
        }

        private void button3_Click(object sender, EventArgs e)
        { // v flip
            //flip the selected tile, unless select all, then flip all.
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;
            if (listBox2.SelectedItems.Count < 1) return;
            
            int y_least, y_most, temp1, temp2, temp3;
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
        }

        private void button12_Click(object sender, EventArgs e)
        { // resize
            //resize the selected tile, unless select all, then resize all.
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;
            if (listBox2.SelectedItems.Count < 1) return;
            
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
        }





        private void button4_Click(object sender, EventArgs e)
        { // nudge left
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;
            if (listBox2.SelectedItems.Count < 1) return;
            int temp_x;
            
            int num_sprites = listBox2.Items.Count;
            if (num_sprites < 1) return;
            int test = 0;
            
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
        }

        private void button5_Click(object sender, EventArgs e)
        { // nudge right
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;
            if (listBox2.SelectedItems.Count < 1) return;
            int temp_x;
            
            int num_sprites = listBox2.Items.Count;
            if (num_sprites < 1) return;
            int test = 0;
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
        }

        private void button6_Click(object sender, EventArgs e)
        { // nudge up
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;
            if (listBox2.SelectedItems.Count < 1) return;
            int temp_y;
            
            int num_sprites = listBox2.Items.Count;
            if (num_sprites < 1) return;
            int test = 0;
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
        }

        private void button7_Click(object sender, EventArgs e)
        { // nudge down
            if (MetaspriteArray[selected_meta].sprite_count == 0) return;
            if (listBox2.SelectedItems.Count < 1) return;
            int temp_y;
            
            int num_sprites = listBox2.Items.Count;
            if (num_sprites < 1) return;
            int test = 0;
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
                e.IsInputKey = true;
                Tiles.shift_left();
            }
            else if (e.KeyCode == Keys.Up)
            {
                e.IsInputKey = true;
                Tiles.shift_up();
            }
            else if (e.KeyCode == Keys.Right)
            {
                e.IsInputKey = true;
                Tiles.shift_right();
            }
            else if (e.KeyCode == Keys.Down)
            {
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
                Tiles.tile_h_flip();
            }
            else if (e.KeyCode == Keys.V)
            {
                Tiles.tile_v_flip();
            }
            else if (e.KeyCode == Keys.R)
            {
                Tiles.tile_rot_cw();
            }
            else if (e.KeyCode == Keys.L)
            {
                Tiles.tile_rot_ccw();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                Tiles.tile_delete();
            }
            else if (e.KeyCode == Keys.C)
            {
                Tiles.tile_copy();
            }
            else if (e.KeyCode == Keys.P)
            {
                Tiles.tile_paste();
            }
            else if (e.KeyCode == Keys.F)
            {
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
            else if (e.KeyCode == Keys.E)
            { // clear selected to color
                Palettes.pal_r[selection] = 0;
                Palettes.pal_g[selection] = 0;
                Palettes.pal_b[selection] = 0;
                update_palette();
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
                MetaspriteArray[selected_meta].name = textBox6.Text;

                cancel_index_change = true;
                // I think it was calling the index changed event
                listBox1.Items[selected_meta] = textBox6.Text;
                cancel_index_change = false;

                e.Handled = true; // prevent ding on return press
            }
        }

        private void textBox7_KeyPress(object sender, KeyPressEventArgs e)
        { // priority for whole metasprite
            int temp_val = 0;
            if (e.KeyChar == (char)Keys.Return)
            {
                string str = textBox7.Text;
                if (str.Length > 1) return; // should be 1 digit
                char testchr = str[0];
                if((testchr < '0') || (testchr > '3'))
                {
                    textBox7.Text = "0";
                    MetaspriteArray[selected_meta].priority = 0;
                    return;
                }
                //else we have a number between 0-3
                
                textBox7.Text = str;
                int.TryParse(str, out temp_val);
                MetaspriteArray[selected_meta].priority = temp_val;

                e.Handled = true; // prevent ding on return press
            }
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
        }

        private void button11_Click(object sender, EventArgs e)
        { // delete all tiles from metatile
            MetaspriteArray[selected_meta].sprite_count = 0;
            listBox2.Items.Clear();
            listBox2.Refresh();
            selected_spr = 0;
            label19.Text = "00";
            update_metatile_image();
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
        {
            int how_many = listBox2.Items.Count;
            if (how_many < 1) return;
            for(int i = 0; i < how_many; i++)
            {
                listBox2.SetSelected(i, true);
            }
            rebuild_spr_list();
            
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
            //temp_bmp.Dispose(); //crashes the program ?
        } // END REDRAW TILESET


        private void checkBox6_MouseUp(object sender, MouseEventArgs e)
        { // highlight selected
            update_metatile_image();
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
                int x_min, y_min, temp1, sp_size;
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
