using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AK_1D
{
    public partial class Form1 : Form
    {
        Bitmap DrawArea;
        Graphics g;
        Pen mypen = new Pen(Brushes.Black);
        Brush aBrush = (Brush)Brushes.Black;

        public Form1()
        {
            InitializeComponent();

            DrawArea = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            pictureBox1.Image = DrawArea;

            //Inserting rules
            comboBox1.Items.Insert(0, 30);
            comboBox1.Items.Insert(1, 60);
            comboBox1.Items.Insert(2, 90);
            comboBox1.Items.Insert(3, 120);
            comboBox1.Items.Insert(4, 225);
            comboBox1.SelectedIndex = 0;
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox1.Refresh();
            g = Graphics.FromImage(DrawArea);
            g.Clear(Color.White);

            int width = Convert.ToInt32(numericUpDown1.Value);
            int time = Convert.ToInt32(numericUpDown2.Value);
            int selectedRule = Convert.ToInt32(comboBox1.SelectedItem);
            int [] ruleValues = convertDecToBin(selectedRule);

            int[,] tab = new int[width, time];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < time; j++)
                    tab[i, j] = 0;

            //  Start point
            tab[width / 2,0] = 1;

            for (int j=0; j<time-1; j++)
            {
                for (int i=0;i<width; i++)
                {
                    int a=0, b=0, c=0;

                    //  Checking Periodical Conditions
                    if (checkBox1.Checked == false)
                    {
                        if (i == 0)
                        {
                            a = 0;
                            b = tab[i, j];
                            c = tab[i + 1, j];
                        }
                        else if (i == width - 1)
                        {
                            a = tab[i - 1, j];
                            b = tab[i, j];
                            c = 0;
                        }
                        else
                        {
                            a = tab[i - 1, j];
                            b = tab[i, j];
                            c = tab[i + 1, j];
                        }
                    }
                    else
                    {
                        if (i == 0)
                        {
                            a = tab[width - 1,j] ;
                            b = tab[i, j];
                            c = tab[i + 1, j];
                        }
                        else if (i == width - 1)
                        {
                            a = tab[i - 1, j];
                            b = tab[i, j];
                            c = tab[0,j];
                        }
                        else
                        {
                            a = tab[i - 1, j];
                            b = tab[i, j];
                            c = tab[i + 1, j];
                        }
                    }
                    // Checking rule and marking accordingly to it
                    if (checkRule(a, b, c, ruleValues) == 1) tab[i, j + 1] = 1;
                }
            }

            //  Visualising changes
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < time - 1; j++)
                {
                    if (tab[i, j] == 1) g.FillRectangle(aBrush, i, j, 1, 1);
                }
            }
            

            pictureBox1.Image = DrawArea;
            pictureBox1.Refresh();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private int check30(int a, int b, int c)
        {
            if (a == 1 & b == 1 & c == 1) return 0;
            if (a == 1 & b == 1 & c == 0) return 0;
            if (a == 1 & b == 0 & c == 1) return 0;
            if (a == 1 & b == 0 & c == 0) return 1;
            if (a == 0 & b == 1 & c == 1) return 1;
            if (a == 0 & b == 1 & c == 0) return 1;
            if (a == 0 & b == 0 & c == 1) return 1;
            if (a == 0 & b == 0 & c == 0) return 0;
            return 0;
        }

        private int checkRule(int a, int b, int c, int [] values)
        {
            if (a == 1 & b == 1 & c == 1) return values[0];
            if (a == 1 & b == 1 & c == 0) return values[1];
            if (a == 1 & b == 0 & c == 1) return values[2];
            if (a == 1 & b == 0 & c == 0) return values[3];
            if (a == 0 & b == 1 & c == 1) return values[4];
            if (a == 0 & b == 1 & c == 0) return values[5];
            if (a == 0 & b == 0 & c == 1) return values[6];
            if (a == 0 & b == 0 & c == 0) return values[7];
            return 0;
        }

        private int [] convertDecToBin(int value)
        {
            string binary = Convert.ToString(value, 2).PadLeft(8, '0');
            int [] tab1 = new int [8];
            for (int i = 0; i < 8; i++)
                tab1[i] = Convert.ToInt32(binary[i])-48;
            return tab1;
        }
    }
}
