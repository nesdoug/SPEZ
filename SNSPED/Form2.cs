using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SNSPED
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        //global
        Bitmap image_tile_box = new Bitmap(128, 128);

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            //trying to figure out how to send information between forms
            Form1.close_it();
            image_tile_box.Dispose();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            update_tile_box();
        }

        public void update_tile_box()
        {
            Color temp_color;
            for (int i = 0; i < 8; ++i) //row = y
            {
                for (int j = 0; j < 8; ++j) //column = x
                {
                    int color = 0;
                    int index = (Form1.tile_set * 256 * 8 * 8) + (Form1.tile_num * 8 * 8) + (i * 8) + j;
                    int pal_index = Tiles.Tile_Arrays[index]; //pixel in tile array
                    
                    pal_index = pal_index & 0x0f; //sanitize, for my sanity
                    color = (Form1.pal_y * 16) + pal_index;

                    temp_color = Color.FromArgb(Palettes.pal_r[color], Palettes.pal_g[color], Palettes.pal_b[color]);
                    
                    for (int k = 0; k < 15; ++k) // x pixel
                    { //128 / 8 = 16, last one = white line
                        for (int m = 0; m < 15; ++m) // y pixel
                        {
                            image_tile_box.SetPixel((j * 16) + m, (i * 16) + k, temp_color);
                        }
                        if (j == 7)
                        {
                            image_tile_box.SetPixel((j * 16) + 15, (i * 16) + k, temp_color);
                        }
                        else
                        {
                            image_tile_box.SetPixel((j * 16) + 15, (i * 16) + k, Color.White);
                        }
                    }
                    for (int m = 0; m < 16; ++m) //bottom line
                    {
                        if (i == 7)
                        {
                            image_tile_box.SetPixel((j * 16) + m, (i * 16) + 15, temp_color);
                        }
                        else
                        {
                            image_tile_box.SetPixel((j * 16) + m, (i * 16) + 15, Color.White);
                        }

                    }
                }
            }

            pictureBox1.Image = image_tile_box;
            pictureBox1.Refresh();
        }

        // changed from click event, so we can hold down and draw
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int pixel_x = 0;
                int pixel_y = 0;

                var mouseEventArgs = e as MouseEventArgs;
                if (mouseEventArgs != null)
                {
                    pixel_x = mouseEventArgs.X >> 4;
                    pixel_y = mouseEventArgs.Y >> 4;
                }
                if (pixel_x < 0) pixel_x = 0;
                if (pixel_y < 0) pixel_y = 0;
                if (pixel_x > 7) pixel_x = 7;
                if (pixel_y > 7) pixel_y = 7;

                int index = (Form1.tile_set * 256 * 8 * 8) + (Form1.tile_num * 8 * 8) + (pixel_y * 8) + pixel_x;
                int color = 0; // which color is selected in palette
                
                color = Form1.pal_x & 0x0f;
                
                Tiles.Tile_Arrays[index] = color;

                //update tileset picture too
                update_tile_box();
                Form1 f = (this.Owner as Form1);
                f.update_tile_image();
                f.tile_show_num();
            }
        }

        // this is the mostly the same as above, pictureBox1_MouseMove
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            int pixel_x = 0;
            int pixel_y = 0;
            Form1 f = (this.Owner as Form1);

            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs != null)
            {
                pixel_x = mouseEventArgs.X >> 4;
                pixel_y = mouseEventArgs.Y >> 4;
            }
            if (pixel_x < 0) pixel_x = 0;
            if (pixel_y < 0) pixel_y = 0;
            if (pixel_x > 7) pixel_x = 7;
            if (pixel_y > 7) pixel_y = 7;

            int index = (Form1.tile_set * 256 * 8 * 8) + (Form1.tile_num * 8 * 8) + (pixel_y * 8) + pixel_x;

            if (e.Button == MouseButtons.Left)
            {
                int color = 0; // (Form1.pal_y * 16) + Form1.pal_x; // which color is selected in palette
                color = Form1.pal_x & 0x0f;
                Tiles.Tile_Arrays[index] = color;
            }
            else if (e.Button == MouseButtons.Right)
            {
                int color = Tiles.Tile_Arrays[index];
                Form1.pal_x = color;
                f.update_palette();
                f.rebuild_pal_boxes();
            }


            //update tileset picture too
            update_tile_box();

            f.update_tile_image();
            f.tile_show_num();
        }

        private void common_update() // for clicks and drags
        {
            update_tile_box();
            Form1 f = (this.Owner as Form1);
            f.update_tile_image();
            f.tile_show_num();
            f.update_metatile_image();
        }

        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            Form1 f = (this.Owner as Form1);

            if (e.KeyCode == Keys.Left)
            {
                f.Checkpoint();
                Tiles.shift_left();
                common_update();
            }
            else if (e.KeyCode == Keys.Up)
            {
                f.Checkpoint();
                Tiles.shift_up();
                common_update();
            }
            else if (e.KeyCode == Keys.Right)
            {
                f.Checkpoint();
                Tiles.shift_right();
                common_update();
            }
            else if (e.KeyCode == Keys.Down)
            {
                f.Checkpoint();
                Tiles.shift_down();
                common_update();
            }
            else if (e.KeyCode == Keys.NumPad2) // down
            {
                if (Form1.tile_y < 15) Form1.tile_y++;
                Form1.tile_num = (Form1.tile_y * 16) + Form1.tile_x;
                common_update();
            }
            else if (e.KeyCode == Keys.NumPad4) // left
            {
                if (Form1.tile_x > 0) Form1.tile_x--;
                Form1.tile_num = (Form1.tile_y * 16) + Form1.tile_x;
                common_update();
            }
            else if (e.KeyCode == Keys.NumPad6) // right
            {
                if (Form1.tile_x < 15) Form1.tile_x++;
                Form1.tile_num = (Form1.tile_y * 16) + Form1.tile_x;
                common_update();
            }
            else if (e.KeyCode == Keys.NumPad8) // up
            {
                if (Form1.tile_y > 0) Form1.tile_y--;
                Form1.tile_num = (Form1.tile_y * 16) + Form1.tile_x;
                common_update();
            }
            else if (e.KeyCode == Keys.H)
            {
                f.Checkpoint();
                Tiles.tile_h_flip();
                common_update();
            }
            else if (e.KeyCode == Keys.V)
            {
                f.Checkpoint();
                Tiles.tile_v_flip();
                common_update();
            }
            else if (e.KeyCode == Keys.R)
            {
                f.Checkpoint();
                Tiles.tile_rot_cw();
                common_update();
            }
            else if (e.KeyCode == Keys.L)
            {
                f.Checkpoint();
                Tiles.tile_rot_ccw();
                common_update();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                f.Checkpoint();
                Tiles.tile_delete();
                common_update();
            }
            else if (e.KeyCode == Keys.C)
            {
                Tiles.tile_copy();
                common_update();
            }
            else if (e.KeyCode == Keys.P)
            {
                f.Checkpoint();
                Tiles.tile_paste();
                common_update();
            }
            else if (e.KeyCode == Keys.F)
            {
                f.Checkpoint();
                Tiles.tile_fill();
                common_update();
            }
            else if (e.KeyCode == Keys.D1) // number key 1
            {
                f.set1_change(); // change the tileset
            }
            else if (e.KeyCode == Keys.D2)
            {
                f.set2_change();
            }
            else if (e.KeyCode == Keys.Z)
            {
                f.Do_Undo();
            }

        }


        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            Form1 f = (this.Owner as Form1);
            f.update_metatile_image();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Form1 f = (this.Owner as Form1);
                f.Checkpoint();
            }
        }
    }
}
