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
    public partial class Form5 : Form
    {

        public static int start_val, end_val;
        public static bool skipTextChange = false;

        private void Form5_Load(object sender, EventArgs e)
        {
            skipTextChange = true;
            textBox1.Text = Form1.f5_width.ToString();
            textBox2.Text = Form1.f5_height.ToString();
            if(Form1.f5_cb1 == true)
            { // single
                checkBox1.Checked = true;
                checkBox2.Checked = false;
            }
            else
            { // multi
                checkBox1.Checked = false;
                checkBox2.Checked = true;
            }
            
            if(Form1.f5_cb4 == true)
            { // use top left as transparent
                checkBox4.Checked = true;
            }
            else
            { // else use color zero
                checkBox4.Checked = false;
            }
            skipTextChange = false;
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            checkBox1.Checked = true;
            checkBox2.Checked = false;
            Form1.f5_cb1 = true;
        }

        private void checkBox2_Click(object sender, EventArgs e)
        {
            checkBox1.Checked = false;
            checkBox2.Checked = true;
            Form1.f5_cb1 = false;
        }

        

        private void checkBox4_Click(object sender, EventArgs e)
        {
            if (checkBox4.Checked == true)
            { // use top left pixel as transparent
                Form1.f5_cb4 = true;
            }
            else
            {
                Form1.f5_cb4 = false;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        { // width
            if (skipTextChange == true) return;

            string str = textBox1.Text;
            if (str == "") return;

            skipTextChange = true;

            int value = 0;
            int.TryParse(str, out value);
            if (value > 64) value = 64; // max value
            if (value < 0) value = 0; // min value
            str = value.ToString();
            textBox1.Text = str;
            Form1.f5_width = value;
            skipTextChange = false;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        { // height
            if (skipTextChange == true) return;

            string str = textBox2.Text;
            if (str == "") return;

            skipTextChange = true;

            int value = 0;
            int.TryParse(str, out value);
            if (value > 64) value = 64; // max value
            if (value < 0) value = 0; // min value -- allow 0-7 for now
            str = value.ToString();
            textBox2.Text = str;
            Form1.f5_height = value;
            skipTextChange = false;
        }

        private void Form5_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Form1.f5_width < 8)
            {
                Form1.f5_width = 8;
            }

            if (Form1.f5_height < 8)
            {
                Form1.f5_height = 8;
            }

            Form1.close_it5();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true; // prevent ding on return press

                string str = textBox1.Text;
                int value = 0;
                int.TryParse(str, out value);
                if (value > 64) value = 64; // max value
                if (value < 8) value = 8; // min value
                str = value.ToString();
                textBox1.Text = str;
                Form1.f5_width = value;
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true; // prevent ding on return press

                string str = textBox2.Text;
                int value = 0;
                int.TryParse(str, out value);
                if (value > 64) value = 64; // max value
                if (value < 8) value = 8; // min value
                str = value.ToString();
                textBox2.Text = str;
                Form1.f5_height = value;
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        { // lost focus
            string str = textBox1.Text;
            int value = 0;
            int.TryParse(str, out value);
            if (value > 64) value = 64; // max value
            if (value < 8) value = 8; // min value
            str = value.ToString();
            textBox1.Text = str;
            Form1.f5_width = value;
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            string str = textBox2.Text;
            int value = 0;
            int.TryParse(str, out value);
            if (value > 64) value = 64; // max value
            if (value < 8) value = 8; // min value
            str = value.ToString();
            textBox2.Text = str;
            Form1.f5_height = value;
        }

        public Form5()
        {
            InitializeComponent();
        }


    }
}
