using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class Form1 : Form
    {
        Bitmap DrawArea;
        Graphics g;
        Pen mypen = new Pen(Brushes.Black);
        Brush aBrush = (Brush)Brushes.Black;

        int time, width, height, selectedRule;
        int[,,] tab;
        int currentStep=0;
        bool buttonStop = false;

        private void WaitNMilliseconds(int time)
        {
            if (time < 1) return;
            DateTime _desired = DateTime.Now.AddMilliseconds(time);
            while (DateTime.Now < _desired)
            {
                System.Threading.Thread.Sleep(1);
                System.Windows.Forms.Application.DoEvents();
            }
        }

        public Form1()
        {
            this.WindowState = FormWindowState.Maximized;
            InitializeComponent();

            DrawArea = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            pictureBox1.Image = DrawArea;

            //  Inserting rules
            comboBox1.Items.Insert(0, "Oscylator");
            comboBox1.Items.Insert(1, "Glider");
            comboBox1.Items.Insert(2, "Niezmienne");
            comboBox1.Items.Insert(3, "Losowe");
            comboBox1.Items.Insert(4, "Wybrane");
            comboBox1.SelectedIndex = 0;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            Point coordinates = me.Location;
            int x = coordinates.X / 10;
            int y = coordinates.Y / 10;
            if (currentStep != time - 1)
            {
                if (tab[x, y, currentStep + 1] == 1) tab[x, y, currentStep + 1] = 0;
                else tab[x, y, currentStep + 1] = 1;
            }
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
            g.Clear(Color.LightGray);

            width = Convert.ToInt32(numericUpDown1.Value);
            height = Convert.ToInt32(numericUpDown3.Value);
            time = Convert.ToInt32(numericUpDown2.Value);
            selectedRule = Convert.ToInt32(comboBox1.SelectedIndex);

            //  Initialising ProgressBar
            progressBar1.Value = 0;
            progressBar1.Maximum = time-1;
            progressBar1.Step = 1;

            tab = new int[width, height, time];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    for (int k =0; k< time; k++)
                        tab[i, j, k] = 0;

            //  Start point
            if (selectedRule == 0)
            {
                tab[width / 2, height / 2, 0] = 1;
                tab[width / 2, height / 2 + 1, 0] = 1;
                tab[width / 2, height / 2 - 1, 0] = 1;
            }
            else if (selectedRule == 1)
            {
                tab[width / 2, height / 2, 0] = 1;
                tab[width / 2, height / 2 + 1, 0] = 1;
                tab[width / 2 + 1, height / 2 + 1, 0] = 1;
                tab[width / 2 - 1, height / 2, 0] = 1;
                tab[width / 2 + 1, height / 2 - 1, 0] = 1;
            }
            else if (selectedRule == 2)
            {
                tab[width / 2, height / 2, 0] = 1;
                tab[width / 2 + 1, height / 2, 0] = 1;
                tab[width / 2, height / 2 - 2, 0] = 1;
                tab[width / 2 + 1, height / 2 - 2, 0] = 1;
                tab[width / 2 - 1, height / 2 - 1, 0] = 1;
                tab[width / 2 + 2, height / 2 - 1, 0] = 1;

            }
            else if (selectedRule == 3)
            {
                Random rnd = new Random();
                for (int i = 0; i < 50; i++)
                {
                    int x = rnd.Next(width);
                    int y = rnd.Next(height);
                    tab[x, y, 0] = 1;
                }
            }
            else if (selectedRule == 4)
            {
                button1.Enabled = false;
                while (buttonStop == false)
                {
                    WaitNMilliseconds(10);
                    for (int i = 0; i <= height; i++)
                    {
                        g.DrawLine(mypen, (width) * 10, (i) * 10, 0, (i) * 10);
                    }
                    for (int i = 0; i <= width; i++)
                    {
                        g.DrawLine(mypen, (i) * 10, (height) * 10, (i) * 10, 0);
                    }
                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            if (tab[i, j, 1] == 1)
                            {
                                g.FillRectangle(aBrush, i * 10, j * 10, 10, 10);
                            }
                        }
                    }
                    pictureBox1.Image = DrawArea;
                    pictureBox1.Refresh();
                }
                buttonStop = false;
            }
            
            gameOn();
            
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private int check(int cell, int [] neighbours)
        {
            int sum = 0;
            for (int i=0; i<8; i++)
            {
                sum += neighbours[i];
            }
            if (sum == 3) return 1;
            else if (cell == 1 & sum == 2) return 1;
            else if (cell == 1 & sum > 3) return 0;
            else if (cell == 1 & sum < 2) return 0;
            return 0;
        }

        private void gameOn()
        {
            button1.Enabled = false;

            for (int k = 0; k < time - 1; k++)
            {
                while (buttonStop == true)
                {
                    WaitNMilliseconds(10);
                }
                currentStep = k;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int[] neighbours = new int[8];

                        //  Checking Periodical Conditions
                        if (checkBox1.Checked == false)
                        {

                            if (i == 0 & j == 0)
                            {
                                neighbours[0] = 0;
                                neighbours[1] = 0;
                                neighbours[2] = 0;
                                neighbours[3] = 0;
                                neighbours[4] = tab[i + 1, j, k];
                                neighbours[5] = 0;
                                neighbours[6] = tab[i, j + 1, k];
                                neighbours[7] = tab[i + 1, j + 1, k];
                            }
                            else if (i == 0 & j == height - 1)
                            {
                                neighbours[0] = 0;
                                neighbours[1] = tab[i, j - 1, k];
                                neighbours[2] = tab[i + 1, j - 1, k];
                                neighbours[3] = 0;
                                neighbours[4] = tab[i + 1, j, k];
                                neighbours[5] = 0;
                                neighbours[6] = 0;
                                neighbours[7] = 0;
                            }
                            else if (i == width - 1 & j == 0)
                            {
                                neighbours[0] = 0;
                                neighbours[1] = 0;
                                neighbours[2] = 0;
                                neighbours[3] = tab[i - 1, j, k];
                                neighbours[4] = 0;
                                neighbours[5] = tab[i - 1, j + 1, k];
                                neighbours[6] = tab[i, j + 1, k];
                                neighbours[7] = 0;
                            }
                            else if (i == width - 1 & j == height - 1)
                            {
                                neighbours[0] = tab[i - 1, j - 1, k];
                                neighbours[1] = tab[i, j - 1, k];
                                neighbours[2] = 0;
                                neighbours[3] = tab[i - 1, j, k];
                                neighbours[4] = 0;
                                neighbours[5] = 0;
                                neighbours[6] = 0;
                                neighbours[7] = 0;
                            }
                            else if (i == 0)
                            {
                                neighbours[0] = 0;
                                neighbours[1] = tab[i, j - 1, k];
                                neighbours[2] = tab[i + 1, j - 1, k];
                                neighbours[3] = 0;
                                neighbours[4] = tab[i + 1, j, k];
                                neighbours[5] = 0;
                                neighbours[6] = tab[i, j + 1, k];
                                neighbours[7] = tab[i + 1, j + 1, k];
                            }
                            else if (i == width - 1)
                            {
                                neighbours[0] = tab[i - 1, j - 1, k];
                                neighbours[1] = tab[i, j - 1, k];
                                neighbours[2] = 0;
                                neighbours[3] = tab[i - 1, j, k];
                                neighbours[4] = 0;
                                neighbours[5] = tab[i - 1, j + 1, k];
                                neighbours[6] = tab[i, j + 1, k];
                                neighbours[7] = 0;
                            }
                            else if (j == 0)
                            {
                                neighbours[0] = 0;
                                neighbours[1] = 0;
                                neighbours[2] = 0;
                                neighbours[3] = tab[i - 1, j, k];
                                neighbours[4] = tab[i + 1, j, k];
                                neighbours[5] = tab[i - 1, j + 1, k];
                                neighbours[6] = tab[i, j + 1, k];
                                neighbours[7] = tab[i + 1, j + 1, k];
                            }
                            else if (j == height - 1)
                            {
                                neighbours[0] = tab[i - 1, j - 1, k];
                                neighbours[1] = tab[i, j - 1, k];
                                neighbours[2] = tab[i + 1, j - 1, k];
                                neighbours[3] = tab[i - 1, j, k];
                                neighbours[4] = tab[i + 1, j, k];
                                neighbours[5] = 0;
                                neighbours[6] = 0;
                                neighbours[7] = 0;
                            }
                            else
                            {
                                neighbours[0] = tab[i - 1, j - 1, k];
                                neighbours[1] = tab[i, j - 1, k];
                                neighbours[2] = tab[i + 1, j - 1, k];
                                neighbours[3] = tab[i - 1, j, k];
                                neighbours[4] = tab[i + 1, j, k];
                                neighbours[5] = tab[i - 1, j + 1, k];
                                neighbours[6] = tab[i, j + 1, k];
                                neighbours[7] = tab[i + 1, j + 1, k];
                            }
                        }
                        else
                        {
                            if (i == 0 & j == 0)
                            {
                                neighbours[0] = tab[width - 1, height - 1, k];
                                neighbours[1] = tab[i, height - 1, k];
                                neighbours[2] = tab[i + 1, height - 1, k];
                                neighbours[3] = tab[width - 1, j, k];
                                neighbours[4] = tab[i + 1, j, k];
                                neighbours[5] = tab[width - 1, j + 1, k];
                                neighbours[6] = tab[i, j + 1, k];
                                neighbours[7] = tab[i + 1, j + 1, k];
                            }
                            else if (i == 0 & j == height - 1)
                            {
                                neighbours[0] = tab[width - 1, j - 1, k];
                                neighbours[1] = tab[i, j - 1, k];
                                neighbours[2] = tab[i + 1, j - 1, k];
                                neighbours[3] = tab[width - 1, j, k];
                                neighbours[4] = tab[i + 1, j, k];
                                neighbours[5] = tab[width - 1, 0, k];
                                neighbours[6] = tab[i, 0, k];
                                neighbours[7] = tab[i + 1, 0, k];
                            }
                            else if (i == width - 1 & j == 0)
                            {
                                neighbours[0] = tab[i - 1, height - 1, k];
                                neighbours[1] = tab[i, height - 1, k];
                                neighbours[2] = tab[0, height - 1, k];
                                neighbours[3] = tab[i - 1, j, k];
                                neighbours[4] = tab[0, j, k];
                                neighbours[5] = tab[i - 1, j + 1, k];
                                neighbours[6] = tab[i, j + 1, k];
                                neighbours[7] = tab[0, j + 1, k];
                            }
                            else if (i == width - 1 & j == height - 1)
                            {
                                neighbours[0] = tab[i - 1, j - 1, k];
                                neighbours[1] = tab[i, j - 1, k];
                                neighbours[2] = tab[0, j - 1, k];
                                neighbours[3] = tab[i - 1, j, k];
                                neighbours[4] = tab[0, j, k];
                                neighbours[5] = tab[i - 1, 0, k];
                                neighbours[6] = tab[i, 0, k];
                                neighbours[7] = tab[0, 0, k];
                            }
                            else if (i == 0)
                            {
                                neighbours[0] = tab[width - 1, j - 1, k];
                                neighbours[1] = tab[i, j - 1, k];
                                neighbours[2] = tab[i + 1, j - 1, k];
                                neighbours[3] = tab[width - 1, j, k];
                                neighbours[4] = tab[i + 1, j, k];
                                neighbours[5] = tab[width - 1, j + 1, k];
                                neighbours[6] = tab[i, j + 1, k];
                                neighbours[7] = tab[i + 1, j + 1, k];
                            }
                            else if (i == width - 1)
                            {
                                neighbours[0] = tab[i - 1, j - 1, k];
                                neighbours[1] = tab[i, j - 1, k];
                                neighbours[2] = tab[0, j - 1, k];
                                neighbours[3] = tab[i - 1, j, k];
                                neighbours[4] = tab[0, j, k];
                                neighbours[5] = tab[i - 1, j + 1, k];
                                neighbours[6] = tab[i, j + 1, k];
                                neighbours[7] = tab[0, j + 1, k];
                            }
                            else if (j == 0)
                            {
                                neighbours[0] = tab[i - 1, height - 1, k];
                                neighbours[1] = tab[i, height - 1, k];
                                neighbours[2] = tab[i + 1, height - 1, k];
                                neighbours[3] = tab[i - 1, j, k];
                                neighbours[4] = tab[i + 1, j, k];
                                neighbours[5] = tab[i - 1, j + 1, k];
                                neighbours[6] = tab[i, j + 1, k];
                                neighbours[7] = tab[i + 1, j + 1, k];
                            }
                            else if (j == height - 1)
                            {
                                neighbours[0] = tab[i - 1, j - 1, k];
                                neighbours[1] = tab[i, j - 1, k];
                                neighbours[2] = tab[i + 1, j - 1, k];
                                neighbours[3] = tab[i - 1, j, k];
                                neighbours[4] = tab[i + 1, j, k];
                                neighbours[5] = tab[i - 1, 0, k];
                                neighbours[6] = tab[i, 0, k];
                                neighbours[7] = tab[i + 1, 0, k];
                            }
                            else
                            {
                                neighbours[0] = tab[i - 1, j - 1, k];
                                neighbours[1] = tab[i, j - 1, k];
                                neighbours[2] = tab[i + 1, j - 1, k];
                                neighbours[3] = tab[i - 1, j, k];
                                neighbours[4] = tab[i + 1, j, k];
                                neighbours[5] = tab[i - 1, j + 1, k];
                                neighbours[6] = tab[i, j + 1, k];
                                neighbours[7] = tab[i + 1, j + 1, k];
                            }
                        }
                        //  Checking rule and marking accordingly to it
                        if (check(tab[i, j, k], neighbours) == 1) tab[i, j, k + 1] = 1;
                    }
                }

                //  Visualizing changes
                g.Clear(Color.LightGray);
                for (int i = 0; i <= height; i++)
                {
                    g.DrawLine(mypen, (width) * 10, (i) * 10, 0, (i) * 10);
                }
                for (int i = 0; i <= width; i++)
                {
                    g.DrawLine(mypen, (i) * 10, (height) * 10, (i) * 10, 0);
                }
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (tab[i, j, k] == 1)
                        {
                            g.FillRectangle(aBrush, i * 10, j * 10, 10, 10);
                        }
                    }
                }
                pictureBox1.Image = DrawArea;
                pictureBox1.Refresh();
                progressBar1.PerformStep();
                WaitNMilliseconds(1000);
            }
            currentStep = 0;
            button1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            buttonStop = !buttonStop;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
