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
    public partial class Form1
    {
        private void openSessionToolStripMenuItem_Click(object sender, EventArgs e)
        { // file / open session
            // note, we are ignoring the header, maybe change later
            // all sizes are fixed for now

            byte[] big_array = new byte[58856];
            int temp = 0;
            int[] bit1 = new int[8]; // bit planes
            int[] bit2 = new int[8];
            int[] bit3 = new int[8];
            int[] bit4 = new int[8];

            int temp1 = 0;
            int temp2 = 0;
            int temp3 = 0;
            int temp4 = 0;
            string local_name = "";

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Open an SPZ Session";
            openFileDialog1.Filter = "SPZ File (*.SPZ)|*.SPZ|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();
                if (fs.Length == 58856)
                {
                    for (int i = 0; i < 58856; i++)
                    {
                        big_array[i] = (byte)fs.ReadByte();
                    }

                    if ((big_array[0] == (byte)'S') && (big_array[1] == (byte)'P')
                        && (big_array[2] == (byte)'Z'))
                    {
                        //copy the palette
                        int offset = 16;
                        for (int i = 0; i < 256; i += 2)
                        {
                            int j;
                            temp1 = big_array[offset++];
                            temp2 = big_array[offset++] << 8;
                            temp = temp1 + temp2;
                            if ((i == 0x20) || (i == 0x40) || (i == 0x60) || (i == 0x80) ||
                                (i == 0xa0) || (i == 0xc0) || (i == 0xe0)) temp = 0;
                            // make the left most boxes black, but not the top most
                            j = i / 2;
                            Palettes.pal_r[j] = (byte)((temp & 0x001f) << 3);
                            Palettes.pal_g[j] = (byte)((temp & 0x03e0) >> 2);
                            Palettes.pal_b[j] = (byte)((temp & 0x7c00) >> 7);
                        }

                        // update the numbers in the boxes
                        temp = pal_x + (pal_y * 16);
                        textBox1.Text = Palettes.pal_r[temp].ToString();
                        textBox2.Text = Palettes.pal_g[temp].ToString();
                        textBox3.Text = Palettes.pal_b[temp].ToString();
                        update_box4();

                        //load tiles, 2 x 256 x 4bpp
                        // copy the 4bpp tile sets
                        for (int temp_set = 0; temp_set < 2; temp_set++) // 2 sets
                        {
                            for (int i = 0; i < 256; i++) // 256 tiles
                            {
                                int index = offset + (temp_set * 256 * 32) + (32 * i); // start of current tile
                                for (int y = 0; y < 8; y++) // get 8 sets of bitplanes
                                {
                                    // get the 4 bitplanes for each tile row
                                    int y2 = y * 2; //0,2,4,6,8,10,12,14
                                    bit1[y] = big_array[index + y2];
                                    bit2[y] = big_array[index + y2 + 1];
                                    bit3[y] = big_array[index + y2 + 16];
                                    bit4[y] = big_array[index + y2 + 17];

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
                                        Tiles.Tile_Arrays[(temp_set * 256 * 8 * 8) + (i * 8 * 8) + (y * 8) + x] =
                                            (temp4 << 3) + (temp3 << 2) + (temp2 << 1) + temp1;
                                    }
                                }
                            }
                        }
                        offset += 16384;

                        // load the metasprites main 4 bytes
                        // 100 meta, 100 sprites, x, y, tile, attr
                        // attr = VH-ZpppS Z = size, out of place for SNES
                        // but we're loading the priority separately
                        
                        for(int i = 0; i<100; i++) //meta
                        {
                            for (int j = 0; j < 100; j++) //sprite
                            {
                                temp = (sbyte)big_array[offset++];
                                MetaspriteArray[i].rel_x[j] = temp;
                                temp = (sbyte)big_array[offset++];
                                MetaspriteArray[i].rel_y[j] = temp;
                                temp = big_array[offset++];
                                MetaspriteArray[i].tile[j] = temp;
                                temp = big_array[offset++];

                                MetaspriteArray[i].set[j] = temp & 0x01;
                                MetaspriteArray[i].palette[j] = (temp >> 1) & 0x03;
                                MetaspriteArray[i].h_flip[j] = (temp >> 6) & 0x01;
                                MetaspriteArray[i].v_flip[j] = (temp >> 7) & 0x01;
                                MetaspriteArray[i].size[j] = (temp >> 4) & 0x01; // non standard
                            }
                        }

                        // then all the priority bytes, 1 per meta
                        for (int i = 0; i < 100; i++)
                        {
                            temp = big_array[offset++];
                            MetaspriteArray[i].priority = temp;
                        }
                        // then all the sprite counts, 1 per meta
                        for (int i = 0; i < 100; i++)
                        {
                            temp = big_array[offset++];
                            MetaspriteArray[i].sprite_count = temp;
                        }
                        // then the name of each meta, 20 bytes each
                        byte test_byte;
                        for (int i = 0; i < 100; i++)
                        {
                            local_name = "";
                            for (int j = 0; j < 20; j++)
                            {
                                test_byte = big_array[offset];
                                if((test_byte > 0) && (test_byte < 128)) // blank is zero filled
                                {
                                    local_name = local_name + (char)big_array[offset];
                                }
                                offset++;
                            }
                            MetaspriteArray[i].name = local_name;
                            listBox1.Items[i] = local_name;
                        }
                        //update everything
                        listBox1.SelectedIndex = 0;
                        rebuild_spr_list();
                        if(listBox2.Items.Count > 0)
                        {
                            listBox2.SelectedIndex = 0;
                            textBox5.Text = MetaspriteArray[0].palette[0].ToString();
                        }
                        textBox6.Text = MetaspriteArray[0].name;
                        update_metatile_image();
                        label10.Text = "00";
                        label19.Text = "00";
                        textBox7.Text = MetaspriteArray[0].priority.ToString();

                    }
                    else
                    {
                        MessageBox.Show("Error. Not an SPZ File.");
                    }

                }
                else
                {
                    MessageBox.Show("File size error. Expected 58856 bytes.",
                    "File size error", MessageBoxButtons.OK);
                }

                fs.Close();
                
                update_palette(); 
                common_update2();
            }
        } // end of OPEN SESSION


        private void saveSessionToolStripMenuItem_Click(object sender, EventArgs e)
        { // file / save session
            byte[] big_array = new byte[58856];
            int temp, count;
            int[] bit1 = new int[8]; // bit planes
            int[] bit2 = new int[8];
            int[] bit3 = new int[8];
            int[] bit4 = new int[8];

            big_array[0] = (byte)'S';
            big_array[1] = (byte)'P';
            big_array[2] = (byte)'Z';
            big_array[3] = 1; // SPZ file version
            big_array[4] = 1; // # palettes (of 128 colors)
            big_array[5] = 2; // # 4bpp tilesets
            big_array[6] = 100; // # of metasprites
            big_array[7] = 100; // # of sprites per metasprite
            // I don't use these values currently, but maybe will later.

            for (int i = 8; i < 16; i++)
            {
                big_array[i] = 0;
            }

            count = 16;
            for (int i = 0; i < 128; i++) // palettes
            {
                temp = ((Palettes.pal_r[i] & 0xf8) >> 3) + 
                    ((Palettes.pal_g[i] & 0xf8) << 2) + ((Palettes.pal_b[i] & 0xf8) << 7);
                big_array[count++] = (byte)(temp & 0xff); // little end first
                big_array[count++] = (byte)((temp >> 8) & 0x7f); // 15 bit palette
            }

            for (int temp_set = 0; temp_set < 2; temp_set++) // 2 tilesets 4bpp
            {
                for (int i = 0; i < 256; i++) // 256 tiles
                {
                    int z = (temp_set * 256 * 8 * 8) + (64 * i); // start of current tile
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            temp = Tiles.Tile_Arrays[z + (y * 8) + x];
                            bit1[y] = (bit1[y] << 1) + (temp & 1);
                            bit2[y] = (bit2[y] << 1) + ((temp & 2) >> 1);
                            bit3[y] = (bit3[y] << 1) + ((temp & 4) >> 2);
                            bit4[y] = (bit4[y] << 1) + ((temp & 8) >> 3);
                        }
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        big_array[count++] = (byte)bit1[j];
                        big_array[count++] = (byte)bit2[j];
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        big_array[count++] = (byte)bit3[j];
                        big_array[count++] = (byte)bit4[j];
                    }
                }
            }

            for(int i = 0; i < 100; i++) // meta
            {
                for (int j = 0; j < 100; j++) // sprite
                {
                    big_array[count++] = (byte)MetaspriteArray[i].rel_x[j];
                    big_array[count++] = (byte)MetaspriteArray[i].rel_y[j];
                    big_array[count++] = (byte)MetaspriteArray[i].tile[j];
                    temp = MetaspriteArray[i].set[j];
                    temp += MetaspriteArray[i].palette[j] << 1;
                    temp += MetaspriteArray[i].h_flip[j] << 6;
                    temp += MetaspriteArray[i].v_flip[j] << 7;
                    temp += MetaspriteArray[i].size[j] << 4; // non standard
                    big_array[count++] = (byte)temp;
                    // 4 bytes per sprite
                }
            }

            for (int i = 0; i < 100; i++) // priority
            {
                big_array[count++] = (byte)MetaspriteArray[i].priority;
            }

            for (int i = 0; i < 100; i++) // sprite count
            {
                big_array[count++] = (byte)MetaspriteArray[i].sprite_count;
            }

            string local_name = "";
            byte [] byte_array = new byte[20];
            
            for (int i = 0; i < 100; i++) // name
            {
                local_name = MetaspriteArray[i].name;
                byte_array = Encoding.ASCII.GetBytes(local_name);
                
                for (int j = 0; j < 20; j++)
                {
                    if (j < local_name.Length)
                    {
                        big_array[count] = byte_array[j];
                    }
                    else
                    {
                        big_array[count] = 0; // pad zero
                    }
                    count++;
                    
                }
            }
            

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "SPZ File (*.SPZ)|*.SPZ|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save this Session";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                for (int i = 0; i < 58856; i++)
                {
                    fs.WriteByte(big_array[i]);
                }
                fs.Close();
            }

        } // end of SAVE SESSION


        private void exportImageToolStripMenuItem_Click(object sender, EventArgs e)
        { // file / export image
            // export image pic of the current view
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG|*.png|BMP|*.bmp|JPG|*.jpg|GIF|*.gif";

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string ext = System.IO.Path.GetExtension(sfd.FileName);
                switch (ext)
                {
                    case ".jpg":
                    case ".jpeg":
                        pictureBox1.Image.Save(sfd.FileName, ImageFormat.Jpeg);
                        break;
                    case ".bmp":
                        pictureBox1.Image.Save(sfd.FileName, ImageFormat.Bmp);
                        break;
                    case ".gif":
                        pictureBox1.Image.Save(sfd.FileName, ImageFormat.Gif);
                        break;
                    default:
                        pictureBox1.Image.Save(sfd.FileName, ImageFormat.Png);
                        break;

                }
            }
        }


        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // close the program
            Application.Exit();
        }




        private void loadBinToolStripMenuItem_Click(object sender, EventArgs e)
        { // load 1 metasprite as binary
            // note, if there is already data in the selected meta
            // it doesn't actually clear that data, just the loaded data
            // and the number of sprites var makes it so you can't see the rest

            byte[] b_array = new byte[422]; // 100 * 4 + 2 + 20

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select a Metasprite";
            openFileDialog1.Filter = "Metasprite (*.bin)|*.bin|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();

                if (fs.Length != 422)
                {
                    MessageBox.Show("File size error. Expected 422 bytes.",
                    "File size error", MessageBoxButtons.OK);
                }
                else
                {
                    for (int i = 0; i < 422; i++)
                    {
                        b_array[i] = (byte)fs.ReadByte();
                    }
                    int offset = 0;
                    int temp = 0;
                    for (int i = 0; i < 100; i++)
                    {
                        MetaspriteArray[selected_meta].rel_x[i] = (sbyte)b_array[offset++];
                        MetaspriteArray[selected_meta].rel_y[i] = (sbyte)b_array[offset++];
                        MetaspriteArray[selected_meta].tile[i] = b_array[offset++];
                        temp = b_array[offset++];
                        MetaspriteArray[selected_meta].set[i] = temp & 0x01;
                        MetaspriteArray[selected_meta].palette[i] = (temp>>1) & 0x07;
                        MetaspriteArray[selected_meta].size[i] = (temp >> 4) & 0x01;
                        MetaspriteArray[selected_meta].h_flip[i] = (temp >> 6) & 0x01;
                        MetaspriteArray[selected_meta].v_flip[i] = (temp >> 7) & 0x01;
                    }
                    MetaspriteArray[selected_meta].priority = b_array[offset++];
                    MetaspriteArray[selected_meta].sprite_count = b_array[offset++];
                    string str = "";
                    for(int i = 0; i<20; i++)
                    {
                        temp = b_array[offset++];
                        if (temp == 0) break;
                        str = str + (char)temp;
                    }
                    MetaspriteArray[selected_meta].name = str;
                    listBox1.Items[listBox1.SelectedIndex] = str;
                    textBox6.Text = str;
                    label19.Text = "00";
                    textBox5.Text = MetaspriteArray[selected_meta].palette[0].ToString();
                    textBox7.Text = MetaspriteArray[selected_meta].priority.ToString();
                    rebuild_spr_list();
                    if(listBox2.Items.Count > 0)
                    {
                        listBox2.SelectedIndex = 0;
                    }
                    selected_spr = 0;
                }

                fs.Close();

                update_metatile_image();
            }

        } // end of load metasprite

        private void saveBinToolStripMenuItem_Click(object sender, EventArgs e)
        { // save 1 metasprite as binary
            byte[] b_array = new byte[422]; // 400 + 2 + 20
            int temp, offset;
            offset = 0;
            for(int i = 0; i<100; i++) // sprite
            {
                temp = MetaspriteArray[selected_meta].rel_x[i];
                b_array[offset++] = (byte)temp;
                temp = MetaspriteArray[selected_meta].rel_y[i];
                b_array[offset++] = (byte)temp;
                temp = MetaspriteArray[selected_meta].tile[i];
                b_array[offset++] = (byte)temp;
                temp = MetaspriteArray[selected_meta].set[i];
                temp += MetaspriteArray[selected_meta].palette[i] << 1;
                temp += MetaspriteArray[selected_meta].size[i] << 4; // non standard
                temp += MetaspriteArray[selected_meta].h_flip[i] << 6;
                temp += MetaspriteArray[selected_meta].v_flip[i] << 7;
                b_array[offset++] = (byte)temp;
            }
            temp = MetaspriteArray[selected_meta].priority;
            b_array[offset++] = (byte)temp;
            temp = MetaspriteArray[selected_meta].sprite_count;
            b_array[offset++] = (byte)temp;
            string str = MetaspriteArray[selected_meta].name;
            for (int i = 0; i<20; i++)
            {
                if (i >= str.Length) break;
                temp = MetaspriteArray[selected_meta].name[i];
                b_array[offset++] = (byte)temp;
            }
            while(offset < 422)
            {
                b_array[offset++] = 0; // zero fill to 422
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Metasprite (*.bin)|*.bin|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save 1 Metasprite binary";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                for (int i = 0; i < 422; i++)
                {
                    fs.WriteByte(b_array[i]);
                }
                fs.Close();
            }
        } // end of save metasprite


        private void saveASMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // save all metasprites (that have at least 1 sprite) as txt
            int test_counts = 0;
            for (int i = 0; i < 100; i++)
            {
                if (MetaspriteArray[i].sprite_count > 0) test_counts++;
            }
            if (test_counts == 0)
            {
                MessageBox.Show("Error. All metasprites have 0 sprites. Unable to save.",
                    "Size error", MessageBoxButtons.OK);
                return;
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "ASM File (*.asm)|*.asm|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save a Metasprite as ASM";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.OpenFile()))
                {
                    int temp = 0;
                    byte cast_8bit;
                    string str = "";

                    for(int each_meta = 0; each_meta<100; each_meta++)
                    {
                        if (MetaspriteArray[each_meta].sprite_count < 1) continue;

                        sw.Write("; ");
                        str = MetaspriteArray[each_meta].name;
                        sw.Write(str + "\r\n");
                        sw.Write("; Sprite mode is ");
                        switch (spr_size_mode)
                        {
                            default:
                            case SIZES_8_16:
                                sw.Write("8 x 16\r\n");
                                break;
                            case SIZES_8_32:
                                sw.Write("8 x 32\r\n");
                                break;
                            case SIZES_8_64:
                                sw.Write("8 x 64\r\n");
                                break;
                            case SIZES_16_32:
                                sw.Write("16 x 32\r\n");
                                break;
                            case SIZES_16_64:
                                sw.Write("16 x 64\r\n");
                                break;
                            case SIZES_32_64:
                                sw.Write("32 x 64\r\n");
                                break;
                        }

                        str = each_meta.ToString("X2");
                        sw.Write("Meta_" + str + ":\r\n");

                        int max_spr = MetaspriteArray[each_meta].sprite_count;
                        for (int i = 0; i < max_spr; i++)
                        {
                            sw.Write(".byte $");
                            cast_8bit = (byte)MetaspriteArray[each_meta].rel_x[i];
                            str = cast_8bit.ToString("X2");
                            // X2 = convert int to hex string
                            sw.Write(str + ", $");
                            cast_8bit = (byte)MetaspriteArray[each_meta].rel_y[i];
                            str = cast_8bit.ToString("X2");
                            sw.Write(str + ", $");
                            cast_8bit = (byte)MetaspriteArray[each_meta].tile[i];
                            str = cast_8bit.ToString("X2");
                            sw.Write(str + ", $");
                            temp = MetaspriteArray[each_meta].set[i];
                            temp += MetaspriteArray[each_meta].palette[i] << 1;
                            temp += MetaspriteArray[each_meta].priority << 4;
                            temp += MetaspriteArray[each_meta].h_flip[i] << 6;
                            temp += MetaspriteArray[each_meta].v_flip[i] << 7;
                            cast_8bit = (byte)temp;
                            str = cast_8bit.ToString("X2");
                            sw.Write(str + ", $");
                            temp = MetaspriteArray[each_meta].size[i] << 1;
                            cast_8bit = (byte)temp;
                            str = cast_8bit.ToString("X2");
                            sw.Write(str + "\r\n");

                        }
                        sw.Write(".byte $80 ;end of data\r\n");
                        sw.Write("\r\n\r\n");
                    }
                    
                    sw.Write("\r\n\r\n");
                    sw.Close();
                }
            }
        }

        private void saveCurrentASMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // save current metasprite (if it has at least 1 sprite) as txt
            if(listBox2.Items.Count < 1)
            {
                MessageBox.Show("Error. This metasprite has 0 sprites. Unable to save.",
                    "Size error", MessageBoxButtons.OK);
                return;
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "ASM File (*.asm)|*.asm|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save a Metasprite as ASM";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.OpenFile()))
                {
                    int temp = 0;
                    byte cast_8bit;

                    sw.Write("; ");
                    string str = MetaspriteArray[selected_meta].name;
                    sw.Write(str + "\r\n");
                    sw.Write("; Sprite mode is ");
                    switch (spr_size_mode)
                    {
                        default:
                        case SIZES_8_16:
                            sw.Write("8 x 16\r\n");
                            break;
                        case SIZES_8_32:
                            sw.Write("8 x 32\r\n");
                            break;
                        case SIZES_8_64:
                            sw.Write("8 x 64\r\n");
                            break;
                        case SIZES_16_32:
                            sw.Write("16 x 32\r\n");
                            break;
                        case SIZES_16_64:
                            sw.Write("16 x 64\r\n");
                            break;
                        case SIZES_32_64:
                            sw.Write("32 x 64\r\n");
                            break;
                    }

                    str = selected_meta.ToString("X2");
                    sw.Write("Meta_" + str + ":\r\n");

                    int max_spr = MetaspriteArray[selected_meta].sprite_count;
                    for (int i = 0; i < max_spr; i++)
                    {
                        sw.Write(".byte $");
                        cast_8bit = (byte)MetaspriteArray[selected_meta].rel_x[i];
                        str = cast_8bit.ToString("X2");
                        // X2 = convert int to hex string
                        sw.Write(str + ", $");
                        cast_8bit = (byte)MetaspriteArray[selected_meta].rel_y[i];
                        str = cast_8bit.ToString("X2");
                        sw.Write(str + ", $");
                        cast_8bit = (byte)MetaspriteArray[selected_meta].tile[i];
                        str = cast_8bit.ToString("X2");
                        sw.Write(str + ", $");
                        temp = MetaspriteArray[selected_meta].set[i];
                        temp += MetaspriteArray[selected_meta].palette[i] << 1;
                        temp += MetaspriteArray[selected_meta].priority << 4;
                        temp += MetaspriteArray[selected_meta].h_flip[i] << 6;
                        temp += MetaspriteArray[selected_meta].v_flip[i] << 7;
                        cast_8bit = (byte)temp;
                        str = cast_8bit.ToString("X2");
                        sw.Write(str + ", $");
                        temp = MetaspriteArray[selected_meta].size[i] << 1;
                        cast_8bit = (byte)temp;
                        str = cast_8bit.ToString("X2");
                        sw.Write(str + "\r\n");
                        
                    }
                    sw.Write(".byte $80 ;end of data\r\n");
                    sw.Write("\r\n\r\n");
                    sw.Close();
                }
            }
        }

        

        private void clearAllMetaspritesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();

            for (int i = 0; i < Form1.MAX_METASP; i++) // meta
            {
                for (int j = 0; j < Form1.MAX_SPRITE; j++) // sprite
                {
                    MetaspriteArray[i].tile[j] = 0;
                    MetaspriteArray[i].set[j] = 0;
                    MetaspriteArray[i].rel_x[j] = 0;
                    MetaspriteArray[i].rel_y[j] = 0;
                    MetaspriteArray[i].palette[j] = 0;
                    MetaspriteArray[i].h_flip[j] = 0;
                    MetaspriteArray[i].v_flip[j] = 0;
                    MetaspriteArray[i].size[j] = 0;
                }
                MetaspriteArray[i].sprite_count = 0;
                MetaspriteArray[i].priority = 0;
                MetaspriteArray[i].name = "Metasprite " + i.ToString();
                listBox1.Items[i] = MetaspriteArray[i].name;
            }

            listBox1.SelectedIndex = 0;
            textBox6.Text = MetaspriteArray[0].name;
            update_metatile_image();
        }




        private void load4bppToolStripMenuItem_Click(object sender, EventArgs e)
        { // load arbitrary size tile file
            int[] bit1 = new int[8]; // bit planes
            int[] bit2 = new int[8];
            int[] bit3 = new int[8];
            int[] bit4 = new int[8];
            int temp1 = 0;
            int temp2 = 0;
            int temp3 = 0;
            int temp4 = 0;
            int[] temp_tiles = new int[0x4000];
            int size_temp_tiles = 0;

            // tile_set assumed to be 0-1
            // so offset_tiles_ar = 0, or 4000
            int offset_tiles_ar = 0x4000 * tile_set; // Tile_Arrays is 1 byte per pixel
            int num_sets = 1;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select a 4bpp Tileset";
            openFileDialog1.Filter = "Tileset (*.chr)|*.chr|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();
                if (fs.Length >= 16) // at least one tile.
                {
                    size_temp_tiles = (int)fs.Length & 0xe000; // round down to nearest 2000

                    if (((int)fs.Length & 0x1fff) > 0) // handle weird sizes.
                    {
                        // just bump up to next size.
                        size_temp_tiles = size_temp_tiles + 0x2000;
                    }
                    if (size_temp_tiles < 0x2000) // min, 1 tileset worth.
                    {
                        size_temp_tiles = 0x2000;
                    }
                    if (fs.Length > 0x4000)
                    {
                        size_temp_tiles = 0x4000; // max, 2 tilesets worth.
                    }
                    if (size_temp_tiles == 0x4000)
                    {
                        // full size tiles, fill both sets
                        offset_tiles_ar = 0;
                        num_sets = 2;
                    }

                    // make sure don't try to copy more bytes than exist.
                    int min_size = size_temp_tiles;
                    if (min_size > fs.Length)
                    {
                        min_size = (int)fs.Length;
                    }

                    // copy file to the temp array.
                    for (int i = 0; i < min_size; i++)
                    {
                        temp_tiles[i] = (byte)fs.ReadByte();
                    }


                    for (int temp_set = 0; temp_set < num_sets; temp_set++)
                    {
                        for (int i = 0; i < 256; i++) // 256 tiles
                        {
                            int index = (temp_set * 0x2000) + (32 * i); // start of current tile
                            for (int y = 0; y < 8; y++) // get 8 sets of bitplanes
                            {
                                // get the 4 bitplanes for each tile row
                                int y2 = y * 2; //0,2,4,6,8,10,12,14
                                bit1[y] = temp_tiles[index + y2];
                                bit2[y] = temp_tiles[index + y2 + 1];
                                bit3[y] = temp_tiles[index + y2 + 16];
                                bit4[y] = temp_tiles[index + y2 + 17];

                                int offset = offset_tiles_ar + (temp_set * 256 * 8 * 8) + (i * 8 * 8) + (y * 8);
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
                                    Tiles.Tile_Arrays[offset + x] =
                                        (temp4 << 3) + (temp3 << 2) + (temp2 << 1) + temp1;
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("File size error. Too small.",
                    "File size error", MessageBoxButtons.OK);
                }

                fs.Close();

                common_update2();
            }
        }



        // RLE code.

        private void try_RLE(byte[] out_array, byte[] in_array, int in_size)
        {
            // globals rle_index, rle_index2, rle_count;
            byte byte1, byte2, byte3;
            int old_index = rle_index;
            rle_count = 0;
            while (rle_index < in_size)
            {
                if (rle_count >= 4095) break; // max count
                if (in_array[rle_index - 1] == in_array[rle_index])
                {
                    rle_count++;
                    rle_index++;
                }
                else
                {
                    break;
                }
            }
            if (rle_count > 0) // zero is best here
            {
                if (rle_count > 31) // 2 byte header
                {
                    byte1 = (byte)(((rle_count >> 8) & 0x0f) + 0xd0);
                    byte2 = (byte)(rle_count & 0xff);
                    byte3 = in_array[rle_index - 1];
                    out_array[rle_index2++] = byte1;
                    out_array[rle_index2++] = byte2;
                    out_array[rle_index2++] = byte3;
                }
                else // 1 byte header
                {
                    byte1 = (byte)((rle_count & 0x3f) + 0x40);
                    byte2 = in_array[rle_index - 1];
                    out_array[rle_index2++] = byte1;
                    out_array[rle_index2++] = byte2;
                }
                rle_index++;
            }
            else
            {
                rle_count = 0;
                rle_index = old_index;
            }
        }

        private void try_Plus(byte[] out_array, byte[] in_array, int in_size)
        {
            // globals rle_index, rle_index2, rle_count;
            byte byte1, byte2, byte3;
            int old_index = rle_index;
            int start_value = in_array[rle_index - 1];
            rle_count = 0;
            while (rle_index < in_size)
            {
                if (rle_count >= 255) break; // max count
                if (in_array[rle_index - 1] == in_array[rle_index] - 1)
                {
                    rle_count++;
                    rle_index++;
                }
                else
                {
                    break;
                }
            }
            if (rle_count > 0) // zero is best here.
            {
                if (rle_count > 31) // 2 byte header
                {
                    byte1 = (byte)(((rle_count >> 8) & 0x0f) + 0xe0);
                    byte2 = (byte)(rle_count & 0xff);
                    byte3 = (byte)start_value;
                    out_array[rle_index2++] = byte1;
                    out_array[rle_index2++] = byte2;
                    out_array[rle_index2++] = byte3;
                }
                else // 1 byte header
                {
                    byte1 = (byte)((rle_count & 0x3f) + 0x80);
                    byte2 = (byte)start_value;
                    out_array[rle_index2++] = byte1;
                    out_array[rle_index2++] = byte2;
                }
                rle_index++;
            }
            else
            {
                rle_count = 0;
                rle_index = old_index;
            }
        }

        private void do_Literal(byte[] out_array, byte[] in_array, int in_size)
        {
            // globals rle_index, rle_index2, rle_count;
            byte byte1, byte2, byte3;
            int start_index = rle_index - 1;
            rle_count = 0;
            rle_index++;
            while (rle_index < in_size)
            {
                if (rle_count >= 4094) break; // max count
                if ((in_array[rle_index - 2] == in_array[rle_index - 1]) &&
                    (in_array[rle_index - 1] == in_array[rle_index]))
                { // found a run > 1
                    break;
                }
                if (((in_array[rle_index - 2] == in_array[rle_index - 1] - 1)) &&
                    (in_array[rle_index - 1] == in_array[rle_index] - 1))
                { // found a run > 1
                    break;
                }
                rle_count++;
                rle_index++;
            }
            rle_count--;
            rle_index--;

            int nearend = in_size - rle_index;
            if (nearend < 2)
            { // near the end of the file, dump the rest
                if (nearend == 1)
                {
                    rle_count++;
                    rle_index++;
                }
                rle_count++;
                rle_index++;
            }

            if (rle_count >= 0) // always do
            {
                int count2 = rle_count + 1;


                if (rle_count > 31) // 2 byte header
                {
                    byte1 = (byte)(((rle_count >> 8) & 0x0f) + 0xc0);
                    byte2 = (byte)(rle_count & 0xff);
                    out_array[rle_index2++] = byte1;
                    out_array[rle_index2++] = byte2;
                    for (int i = 0; i < count2; i++)
                    {
                        byte3 = in_array[start_index++];
                        out_array[rle_index2++] = byte3;
                    }

                }
                else // 1 byte header
                {
                    byte1 = (byte)(rle_count & 0x3f);
                    out_array[rle_index2++] = byte1;
                    if (rle_count == 0)
                    {
                        byte2 = in_array[start_index];
                        out_array[rle_index2++] = byte2;
                    }
                    else
                    {
                        for (int i = 0; i < count2; i++)
                        {
                            byte2 = in_array[start_index++];
                            out_array[rle_index2++] = byte2;
                        }
                    }

                }

            }

        }

        private int convert_RLE(byte[] in_array, int in_size)
        {
            byte[] in_array_P = new byte[65536];
            byte[] out_array_P = new byte[65536];
            byte[] out_array_notP = new byte[65536];
            byte[] split_array = new byte[32768];
            byte[] split_array2 = new byte[32768];
            int P_size, notP_size;
            // globals rle_index, rle_index2, rle_count;
            rle_index = 1; // // start at 1, we subtract 1
            rle_index2 = 0;
            rle_count = 0;

            if (in_size < 3) return 0; // minimum to avoid errors


            // try not Planar first

            while (rle_index < in_size)
            {
                try_RLE(out_array_notP, in_array, in_size);
                if (rle_count == 0)
                {
                    try_Plus(out_array_notP, in_array, in_size);
                    if (rle_count == 0)
                    {
                        do_Literal(out_array_notP, in_array, in_size);
                    }
                }
            }

            // do a final literal, if needed
            if (rle_index == in_size)
            {
                out_array_notP[rle_index2++] = 0; // literal of 1
                out_array_notP[rle_index2++] = in_array[in_size - 1]; // the last byte
            }

            // put an end of file marker, non-planar
            out_array_notP[rle_index2++] = 0xf0;
            notP_size = rle_index2;


            // try again, Planar
            // split the array, low bytes in 1 array, high bytes in another
            // planar expects even. If odd, this will pad a zero at the end.
            int half_size = (in_size + 1) / 2;
            in_size = half_size * 2; // should round up even.
            for (int i = 0; i < half_size; i++)
            {
                int j = i * 2;
                int k = j + 1;
                split_array[i] = in_array[j];
                split_array2[i] = in_array[k];
            }
            // combine them into 1 array, so I don't have to modify the code
            for (int i = 0; i < half_size; i++)
            {
                in_array_P[i] = split_array[i];
                int j = i + half_size;
                in_array_P[j] = split_array2[i];
            }

            rle_index = 1;
            rle_index2 = 0;
            rle_count = 0;
            while (rle_index < in_size)
            {
                try_RLE(out_array_P, in_array_P, in_size);
                if (rle_count == 0)
                {
                    try_Plus(out_array_P, in_array_P, in_size);
                    if (rle_count == 0)
                    {
                        do_Literal(out_array_P, in_array_P, in_size);
                    }
                }
            }
            // do a final literal, if needed
            if (rle_index == in_size)
            {
                out_array_P[rle_index2++] = 0; // literal of 1
                out_array_P[rle_index2++] = in_array_P[in_size - 1]; // the last byte
            }

            // put an end of file marker, planar
            out_array_P[rle_index2++] = 0xff;
            P_size = rle_index2;

            // copy best array to global rle_array[]
            // and return the length
            if (notP_size <= P_size)
            { // not planar is best
                for (int i = 0; i < notP_size; i++)
                {
                    rle_array[i] = out_array_notP[i];
                }
                return notP_size;
            }
            else
            { // planar is best
                for (int i = 0; i < P_size; i++)
                {
                    rle_array[i] = out_array_P[i];
                }
                return P_size;
            }

        }




        private void save1SetToolStripMenuItem_Click(object sender, EventArgs e)
        { // save just 256 tiles, 1 set
            int[] bit1 = new int[8]; // bit planes
            int[] bit2 = new int[8];
            int[] bit3 = new int[8];
            int[] bit4 = new int[8];
            int temp;
            byte[] out_array = new byte[8192]; // 256 * 32
            int out_index = 0;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Tileset (*.chr)|*.chr|RLE File (*.rle)|*.rle";
            saveFileDialog1.Title = "Save a 4bpp Tileset";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                for (int i = 0; i < 256; i++) // 256 tiles
                {
                    int z = (tile_set * 256 * 8 * 8) + (64 * i); // start of current tile
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            temp = Tiles.Tile_Arrays[z + (y * 8) + x];
                            bit1[y] = (bit1[y] << 1) + (temp & 1);
                            bit2[y] = (bit2[y] << 1) + ((temp & 2) >> 1);
                            bit3[y] = (bit3[y] << 1) + ((temp & 4) >> 2);
                            bit4[y] = (bit4[y] << 1) + ((temp & 8) >> 3);
                        }
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        out_array[out_index++] = (byte)(bit1[j]);
                        out_array[out_index++] = (byte)(bit2[j]);
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        out_array[out_index++] = (byte)(bit3[j]);
                        out_array[out_index++] = (byte)(bit4[j]);
                    }
                }

                string ext = System.IO.Path.GetExtension(saveFileDialog1.FileName);
                if (ext == ".chr")
                {
                    for (int j = 0; j < 8192; j++)
                    {
                        fs.WriteByte(out_array[j]);
                    }

                    fs.Close();
                }
                else if (ext == ".rle")
                {
                    int rle_length = convert_RLE(out_array, 8192);
                    // global rle_array[] now has our compressed data
                    for (int i = 0; i < rle_length; i++)
                    {
                        fs.WriteByte(rle_array[i]);
                    }

                    float percent = (float)rle_length / 8192;
                    fs.Close();

                    MessageBox.Show(String.Format("RLE size is {0}, or {1:P2}", rle_length, percent));
                }
                else
                { // something went wrong.
                    fs.Close();
                }

            }
        }

        private void save2SetsToolStripMenuItem_Click(object sender, EventArgs e)
        { // save both sets of tiles
            int[] bit1 = new int[8]; // bit planes
            int[] bit2 = new int[8];
            int[] bit3 = new int[8];
            int[] bit4 = new int[8];
            int temp;
            byte[] out_array = new byte[16384]; // 256 * 32 * 2
            int out_index = 0;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Tileset (*.chr)|*.chr|RLE File (*.rle)|*.rle";
            saveFileDialog1.Title = "Save all 4bpp Tilesets";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                for (int temp_set = 0; temp_set < 2; temp_set++)
                {
                    for (int i = 0; i < 256; i++) // 256 tiles
                    {
                        int z = (temp_set * 256 * 8 * 8) + (64 * i); // start of current tile
                        for (int y = 0; y < 8; y++)
                        {
                            for (int x = 0; x < 8; x++)
                            {
                                temp = Tiles.Tile_Arrays[z + (y * 8) + x];
                                bit1[y] = (bit1[y] << 1) + (temp & 1);
                                bit2[y] = (bit2[y] << 1) + ((temp & 2) >> 1);
                                bit3[y] = (bit3[y] << 1) + ((temp & 4) >> 2);
                                bit4[y] = (bit4[y] << 1) + ((temp & 8) >> 3);
                            }
                        }
                        for (int j = 0; j < 8; j++)
                        {
                            out_array[out_index++] = (byte)(bit1[j]);
                            out_array[out_index++] = (byte)(bit2[j]);
                        }
                        for (int j = 0; j < 8; j++)
                        {
                            out_array[out_index++] = (byte)(bit3[j]);
                            out_array[out_index++] = (byte)(bit4[j]);
                        }
                    }
                }

                string ext = System.IO.Path.GetExtension(saveFileDialog1.FileName);
                if (ext == ".chr")
                {
                    for (int j = 0; j < 16384; j++)
                    {
                        fs.WriteByte(out_array[j]);
                    }

                    fs.Close();
                }
                else if (ext == ".rle")
                {
                    int rle_length = convert_RLE(out_array, 16384);
                    // global rle_array[] now has our compressed data
                    for (int i = 0; i < rle_length; i++)
                    {
                        fs.WriteByte(rle_array[i]);
                    }

                    float percent = (float)rle_length / 16384;
                    fs.Close();

                    MessageBox.Show(String.Format("RLE size is {0}, or {1:P2}", rle_length, percent));
                }
                else
                { // something went wrong.
                    fs.Close();
                }
            }
        }

        private void clearAllTilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 32768; i++)
            {
                Tiles.Tile_Arrays[i] = 0;
            }
            common_update2();
        }



        private void loadFullPaletteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] pal_array = new byte[256]; // 128 entries * 2 bytes, little endian
            int temp, max_size;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select a Palette file";
            openFileDialog1.Filter = "Palette files (*.pal)|*.pal|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();
                max_size = (int)fs.Length & 0x1fe; // should be even
                if (fs.Length > 0x100)
                {
                    max_size = 0x100; // handle unusually large
                }
                if (max_size >= 2)
                {
                    for (int i = 0; i < 256; i++)
                    {
                        if (i >= max_size) break;
                        pal_array[i] = (byte)fs.ReadByte();
                    }

                    for (int i = 0; i < 256; i += 2)
                    {
                        if (i >= max_size) break;
                        int j;
                        temp = pal_array[i] + (pal_array[i + 1] << 8);
                        if ((i == 0x20) || (i == 0x40) || (i == 0x60) || (i == 0x80) ||
                            (i == 0xa0) || (i == 0xc0) || (i == 0xe0)) temp = 0;
                        // make the left most boxes black, but not the top most
                        j = i / 2;
                        Palettes.pal_r[j] = (byte)((temp & 0x001f) << 3);
                        Palettes.pal_g[j] = (byte)((temp & 0x03e0) >> 2);
                        Palettes.pal_b[j] = (byte)((temp & 0x7c00) >> 7);
                    }

                    // update the numbers in the boxes
                    temp = pal_x + (pal_y * 16);
                    textBox1.Text = Palettes.pal_r[temp].ToString();
                    textBox2.Text = Palettes.pal_g[temp].ToString();
                    textBox3.Text = Palettes.pal_b[temp].ToString();

                    update_box4();
                    update_palette();
                    common_update2();
                }
                else
                {
                    MessageBox.Show("File size error. Expected 256 bytes.",
                    "File size error", MessageBoxButtons.OK);
                }

                fs.Close();
            }
        } // END OF LOAD FULL PALETTE

        private void load32BytesToolStripMenuItem_Click(object sender, EventArgs e)
        { // load just 1 row palette (16 colors = 32 bytes)
            byte[] pal_array = new byte[32]; // 16 entries * 2 bytes, little endian
            int temp, max_size;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select a Palette file";
            openFileDialog1.Filter = "Palette files (*.pal)|*.pal|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();
                max_size = (int)fs.Length & 0x00fe; // should be even
                if (fs.Length > 0xfe)
                {
                    max_size = 0xfe; // handle unusually large
                }
                if (max_size >= 2)
                {
                    for (int i = 0; i < 32; i++)
                    {
                        if (i >= max_size) break;
                        pal_array[i] = (byte)fs.ReadByte();
                    }

                    for (int i = 0; i < 32; i += 2)
                    {
                        if (i >= max_size) break;
                        int j;
                        temp = pal_array[i] + (pal_array[i + 1] << 8);
                        if ((i == 0) && (pal_y != 0)) continue;
                        // skip the left most boxes, but not the top most

                        j = (i / 2) + (pal_y * 16);
                        Palettes.pal_r[j] = (byte)((temp & 0x001f) << 3);
                        Palettes.pal_g[j] = (byte)((temp & 0x03e0) >> 2);
                        Palettes.pal_b[j] = (byte)((temp & 0x7c00) >> 7);
                    }

                    // update the numbers in the boxes
                    temp = pal_x + (pal_y * 16);
                    textBox1.Text = Palettes.pal_r[temp].ToString();
                    textBox2.Text = Palettes.pal_g[temp].ToString();
                    textBox3.Text = Palettes.pal_b[temp].ToString();

                    update_box4();
                    update_palette();
                    common_update2();
                }
                else
                {
                    MessageBox.Show("File size error. Expected 32 bytes.",
                    "File size error", MessageBoxButtons.OK);
                }

                fs.Close();
            }
        } // END LOAD 32 byte palette

        private void loadPaletteFromRGBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] pal_array = new byte[384]; // 128 entries * 3 colors
            int temp, max_size;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select a Palette file";
            openFileDialog1.Filter = "Palette files (*.pal)|*.pal|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();
                max_size = (int)fs.Length;
                max_size = (max_size / 3) * 3; // should be multiple of 3
                if (fs.Length > 384) max_size = 384; // handle unusually large

                if (max_size >= 3)
                {
                    for (int i = 0; i < 384; i++)
                    {
                        if (i >= max_size) break;
                        pal_array[i] = (byte)fs.ReadByte();
                    }

                    int offset = 0;

                    for (int i = 0; i < 384; i += 3) //128 * 3 color
                    {
                        if (i >= max_size) break;
                        Palettes.pal_r[offset] = (byte)(pal_array[i] & 0xf8);
                        Palettes.pal_g[offset] = (byte)(pal_array[i + 1] & 0xf8);
                        Palettes.pal_b[offset] = (byte)(pal_array[i + 2] & 0xf8);
                        offset++;
                    }

                    // update the numbers in the boxes
                    temp = pal_x + (pal_y * 16);
                    textBox1.Text = Palettes.pal_r[temp].ToString();
                    textBox2.Text = Palettes.pal_g[temp].ToString();
                    textBox3.Text = Palettes.pal_b[temp].ToString();

                    update_box4();
                    update_palette();
                    common_update2();
                }
                else
                {
                    MessageBox.Show("File size error. Expected 3 - 768 bytes.",
                    "File size error", MessageBoxButtons.OK);
                }

                fs.Close();
            }
        } // END PALETTE LOAD FROM RGB

        private void savePaletteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] pal_array = new byte[256]; // 128 entries * 2 bytes, little endian
            int temp;

            for (int i = 0; i < 128; i++)
            {
                temp = ((Palettes.pal_r[i] & 0xf8) >> 3) + ((Palettes.pal_g[i] & 0xf8) << 2) + ((Palettes.pal_b[i] & 0xf8) << 7);
                pal_array[(i * 2)] = (byte)(temp & 0xff); // little end first
                pal_array[(i * 2) + 1] = (byte)((temp >> 8) & 0x7f); // 15 bit palette
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Palette files (*.pal)|*.pal|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save a Palette";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                for (int i = 0; i < 256; i++)
                {
                    fs.WriteByte(pal_array[i]);
                }
                fs.Close();
            }
        }

        private void save32BytesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // save just 1 palette (16 colors = 32 bytes)
            byte[] pal_array = new byte[32]; // 16 entries * 2 bytes, little endian
            int temp;

            for (int i = 0; i < 16; i++)
            {
                int j = i + (pal_y * 16);
                temp = ((Palettes.pal_r[j] & 0xf8) >> 3) + ((Palettes.pal_g[j] & 0xf8) << 2) + ((Palettes.pal_b[j] & 0xf8) << 7);
                pal_array[(i * 2)] = (byte)(temp & 0xff); // little end first
                pal_array[(i * 2) + 1] = (byte)((temp >> 8) & 0x7f); // 15 bit palette
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Palette files (*.pal)|*.pal|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save a Palette";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                for (int i = 0; i < 32; i++)
                {
                    fs.WriteByte(pal_array[i]);
                }
                fs.Close();
            }
        } 

        private void savePaletteAsASMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] pal_array = new byte[256]; // 128 entries * 2 bytes, little endian
            int temp;

            for (int i = 0; i < 128; i++)
            {
                temp = ((Palettes.pal_r[i] & 0xf8) >> 3) + ((Palettes.pal_g[i] & 0xf8) << 2) + ((Palettes.pal_b[i] & 0xf8) << 7);
                pal_array[(i * 2)] = (byte)(temp & 0xff); // little end first
                pal_array[(i * 2) + 1] = (byte)((temp >> 8) & 0x7f); // 15 bit palette
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "ASM File (*.asm)|*.asm|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save a Palette as ASM";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.OpenFile()))
                {
                    int count = 0;
                    string str = "";
                    sw.Write("Palette:\r\n");
                    for (int i = 0; i < 16; i++)
                    {
                        sw.Write("\r\n.byte ");
                        for (int j = 0; j < 8; j++)
                        {
                            str = pal_array[count].ToString("X2"); // convert int to hex string
                            sw.Write("$" + str + ", ");
                            count++;
                            str = pal_array[count].ToString("X2");
                            sw.Write("$" + str);
                            if (j < 7)
                            {
                                sw.Write(", ");
                            }
                            count++;
                        }
                    }
                    sw.Write("\r\n\r\n");
                    sw.Close();
                }
            }
        } // END SAVE PALETTE AS ASM

        private void savePaletteAsRGBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] pal_array = new byte[384]; // 128 entries * 3 = r,g,b
            //int temp;
            int offset = 0;
            for (int i = 0; i < 128; i++)
            {
                pal_array[offset++] = (byte)(Palettes.pal_r[i] & 0xf8);
                pal_array[offset++] = (byte)(Palettes.pal_g[i] & 0xf8);
                pal_array[offset++] = (byte)(Palettes.pal_b[i] & 0xf8);
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Palette (*.pal)|*.pal|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save a Palette";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                for (int i = 0; i < 384; i++)
                {
                    fs.WriteByte(pal_array[i]);
                }
                fs.Close();
            }
        } // END SAVE PAL AS RGB


        private void and16ToolStripMenuItem_Click(object sender, EventArgs e)
        { // 8 16
            and16ToolStripMenuItem.Checked = true;
            and32ToolStripMenuItem.Checked = false;
            and64ToolStripMenuItem.Checked = false;
            and32ToolStripMenuItem1.Checked = false;
            and64ToolStripMenuItem1.Checked = false;
            and64ToolStripMenuItem2.Checked = false;
            spr_size_mode = SIZES_8_16;

            selected_spr = listBox2.SelectedIndex;
            rebuild_spr_list();
            listBox2.SelectedIndex = selected_spr;
            update_metatile_image();

            label20.Text = "Sprite sizes 8 and 16";
        }

        private void and32ToolStripMenuItem_Click(object sender, EventArgs e)
        { // 8 32
            and16ToolStripMenuItem.Checked = false;
            and32ToolStripMenuItem.Checked = true;
            and64ToolStripMenuItem.Checked = false;
            and32ToolStripMenuItem1.Checked = false;
            and64ToolStripMenuItem1.Checked = false;
            and64ToolStripMenuItem2.Checked = false;
            spr_size_mode = SIZES_8_32;

            selected_spr = listBox2.SelectedIndex;
            rebuild_spr_list();
            listBox2.SelectedIndex = selected_spr;
            update_metatile_image();

            label20.Text = "Sprite sizes 8 and 32";
        }

        private void and64ToolStripMenuItem_Click(object sender, EventArgs e)
        { // 8 64
            and16ToolStripMenuItem.Checked = false;
            and32ToolStripMenuItem.Checked = false;
            and64ToolStripMenuItem.Checked = true;
            and32ToolStripMenuItem1.Checked = false;
            and64ToolStripMenuItem1.Checked = false;
            and64ToolStripMenuItem2.Checked = false;
            spr_size_mode = SIZES_8_64;

            selected_spr = listBox2.SelectedIndex;
            rebuild_spr_list();
            listBox2.SelectedIndex = selected_spr;
            update_metatile_image();

            label20.Text = "Sprite sizes 8 and 64";
        }

        private void and32ToolStripMenuItem1_Click(object sender, EventArgs e)
        { // 16 32
            and16ToolStripMenuItem.Checked = false;
            and32ToolStripMenuItem.Checked = false;
            and64ToolStripMenuItem.Checked = false;
            and32ToolStripMenuItem1.Checked = true;
            and64ToolStripMenuItem1.Checked = false;
            and64ToolStripMenuItem2.Checked = false;
            spr_size_mode = SIZES_16_32;

            selected_spr = listBox2.SelectedIndex;
            rebuild_spr_list();
            listBox2.SelectedIndex = selected_spr;
            update_metatile_image();

            label20.Text = "Sprite sizes 16 and 32";
        }

        private void and64ToolStripMenuItem1_Click(object sender, EventArgs e)
        { // 16 64
            and16ToolStripMenuItem.Checked = false;
            and32ToolStripMenuItem.Checked = false;
            and64ToolStripMenuItem.Checked = false;
            and32ToolStripMenuItem1.Checked = false;
            and64ToolStripMenuItem1.Checked = true;
            and64ToolStripMenuItem2.Checked = false;
            spr_size_mode = SIZES_16_64;

            selected_spr = listBox2.SelectedIndex;
            rebuild_spr_list();
            listBox2.SelectedIndex = selected_spr;
            update_metatile_image();

            label20.Text = "Sprite sizes 16 and 64";
        }

        private void and64ToolStripMenuItem2_Click(object sender, EventArgs e)
        { // 32 64
            and16ToolStripMenuItem.Checked = false;
            and32ToolStripMenuItem.Checked = false;
            and64ToolStripMenuItem.Checked = false;
            and32ToolStripMenuItem1.Checked = false;
            and64ToolStripMenuItem1.Checked = false;
            and64ToolStripMenuItem2.Checked = true;
            spr_size_mode = SIZES_32_64;

            selected_spr = listBox2.SelectedIndex;
            rebuild_spr_list();
            listBox2.SelectedIndex = selected_spr;
            update_metatile_image();

            label20.Text = "Sprite sizes 32 and 64";
        }



        private void set1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            set1ToolStripMenuItem.Checked = true;
            set2ToolStripMenuItem.Checked = false;
            tile_set = 0;

            if (newChild != null)
            {
                newChild.update_tile_box();
            }

            label9.Text = "1";
            common_update2();
        }

        private void set2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            set1ToolStripMenuItem.Checked = false;
            set2ToolStripMenuItem.Checked = true;
            tile_set = 1;

            if (newChild != null)
            {
                newChild.update_tile_box();
            }

            label9.Text = "2";
            common_update2();
        }

        private void aboutSNSPEDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("SPEZ = SNES Sprite Editor, by Doug Fraker, 2020.\n\nVersion 1.1");
        }



    }

}