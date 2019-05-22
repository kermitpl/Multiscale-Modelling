using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Grain_Growth
{
    public partial class Form1 : Form
    {
        Bitmap DrawArea;
        Graphics g;
        Pen mypen = new Pen(Brushes.Black);
        Brush aBrush = (Brush)Brushes.Black;

        int width, height, nucleation, grainPictureSize, neighbourhood;
        int grainQuantity, grainQuantityWidth, grainQuantityHeight, grainRadius;
        int neighbourhoodRadius;
        int time = 2;
        Color [] grainColors;
        bool working, buttonContinue = false;
        int clickedCounter;

        int[,,] tab;
        int[,] pentTab;
        int[,] heksTab;
        double[,] tabX;
        double[,] tabY;

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            Point coordinates = me.Location;
            int x = coordinates.X / grainPictureSize;
            int y = coordinates.Y / grainPictureSize;
            if (working == false)
            {
                tab[x, y, 1] = clickedCounter+1;
                clickedCounter++;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            buttonContinue = !buttonContinue;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public Form1()
        {
            this.WindowState = FormWindowState.Maximized;
            InitializeComponent();
            DrawArea = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            pictureBox1.Image = DrawArea;

            //  Inserting rules
            comboBox1.Items.Insert(0, "Jednorodne");
            comboBox1.Items.Insert(1, "Z promieniem");
            comboBox1.Items.Insert(2, "Losowe");
            comboBox1.Items.Insert(3, "Wybrane");
            comboBox1.SelectedIndex = 2;

            comboBox2.Items.Insert(0, "VonNeumann");
            comboBox2.Items.Insert(1, "Moore");
            comboBox2.Items.Insert(2, "Pentagonalne losowe");
            comboBox2.Items.Insert(3, "Heksagonalne lewe");
            comboBox2.Items.Insert(4, "Heksagonalne prawe");
            comboBox2.Items.Insert(5, "Heksagonalne losowe");
            comboBox2.Items.Insert(6, "Z promieniem");
            comboBox2.SelectedIndex = 6;
            MaximizeBox = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox1.Refresh();
            g = Graphics.FromImage(DrawArea);
            g.Clear(Color.LightGray);

            width = Convert.ToInt32(numericUpDown1.Value);
            height = Convert.ToInt32(numericUpDown2.Value);
            nucleation = Convert.ToInt32(comboBox1.SelectedIndex);
            neighbourhood = Convert.ToInt32(comboBox2.SelectedIndex);
            neighbourhoodRadius = Convert.ToInt32(numericUpDown5.Value);
            grainPictureSize = pictureBox1.Width / width;

            tab = new int[width, height, time];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    for (int k = 0; k < time; k++)
                        tab[i, j, k] = 0;

            Random rnd = new Random();

            if (nucleation == 0)
            {
                grainQuantityWidth = Convert.ToInt32(numericUpDown3.Value);
                grainQuantityHeight = Convert.ToInt32(numericUpDown4.Value);
                grainQuantity = grainQuantityHeight * grainQuantityWidth;
                int counter = 1;
                for (int i=0; i<grainQuantityHeight; i++)
                {
                    for (int j=0; j<grainQuantityWidth; j++)
                    {
                        tab[(width / grainQuantityWidth / 2) + j * (width / grainQuantityWidth), (height / grainQuantityHeight / 2) + i * (height / grainQuantityHeight), 1] = counter;
                        counter++;
                    }
                }
            }
            else if (nucleation == 1)
            {
                grainQuantity = Convert.ToInt32(numericUpDown3.Value);
                grainRadius= Convert.ToInt32(numericUpDown4.Value);

                bool impossibility = false;

                for (int g = 0; g < grainQuantity; g++)
                {
                    bool onceAgain = true;
                  
                    int rndQuantity = 0;
                    while (onceAgain == true && impossibility == false)
                    {
                        if (rndQuantity>100)
                        {
                            MessageBox.Show("Too much points ot too wide radius. \n Grains:"+g, "Impossibility", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            impossibility = true;
                            break;
                        }
                        onceAgain = false;
                        int x = rnd.Next(width);
                        int y = rnd.Next(height);
                        for (int i = 0; i < width; i++)
                            for (int j = 0; j < height; j++)
                            {
                                int dist;
                                if (tab[i, j, 1] != 0)
                                {
                                    dist = CalculateManhattanDistance(x, i, y, j);
                                    if (dist < grainRadius) onceAgain = true;
                                }
                            }
                        if (onceAgain == false) tab[x, y, 1] = g + 1;
                        rndQuantity++;
                    }
                    if (impossibility == true)
                    {
                        grainQuantity = g;
                        break;
                    }
                }
            }
            else if (nucleation == 2)
            {
                
                grainQuantity= Convert.ToInt32(numericUpDown3.Value);
                for (int i=0; i<grainQuantity; i++)
                {
                    tab[rnd.Next(width), rnd.Next(height), 1] = i + 1;
                }
            }
            else if (nucleation == 3)
            {
                clickedCounter = 0;
                while (buttonContinue == false)
                {
                    button1.Enabled = false;
                    System.Threading.Thread.Sleep(20);
                    System.Windows.Forms.Application.DoEvents();

                    for (int i = 0; i <= height; i++)
                    {
                        g.DrawLine(mypen, (width) * grainPictureSize, (i) * grainPictureSize, 0, (i) * grainPictureSize);
                    }
                    for (int i = 0; i <= width; i++)
                    {
                        g.DrawLine(mypen, (i) * grainPictureSize, (height) * grainPictureSize, (i) * grainPictureSize, 0);
                    }
                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            if (tab[i, j, 1] != 0)
                            {
                                g.FillRectangle(aBrush, i * grainPictureSize, j * grainPictureSize, grainPictureSize, grainPictureSize);
                            }
                        }
                    }
                    pictureBox1.Image = DrawArea;
                    pictureBox1.Refresh();
                }
                grainQuantity = clickedCounter;
                buttonContinue = false;
                button1.Enabled = true;
            }

            fillColourTable();

            if (neighbourhood == 2)
            {
                pentTab = new int[width, height];
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                        pentTab[i, j] = rnd.Next(4);
            }
            else if (neighbourhood == 5)
            {
                heksTab = new int[width, height];
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                        heksTab[i, j] = rnd.Next(2);
            }
            else if (neighbourhood == 6)
            {
                tabX = new double[width, height];
                tabY = new double[width, height];
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        tabX[i,j] = i*1.0+(rnd.NextDouble()-0.5);
                        tabY[i,j] = j*1.0+(rnd.NextDouble()-0.5);
                    }
                       
            }

            working = true;
            growth();
            working = false;
        }

        private void fillColourTable()
        {
            Random rnd = new Random();
            grainColors = new Color[grainQuantity];
            for (int i=0; i<grainQuantity; i++)
            {
                grainColors[i] = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            }
        }

        private void growth()
        {
            pictureBox1.Image = null;
            pictureBox1.Refresh();
            g = Graphics.FromImage(DrawArea);
            g.Clear(Color.LightGray);


            bool end = false;
            while (end == false)
            {
                g.Clear(Color.LightGray);
                end = true;

                // zapisanie wartosci poprzedniego kroku
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        tab[i, j, 0] = tab[i, j, 1];
                    }

                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        if (tab[i, j, 0] == 0) tab[i, j, 1] = grainValue(getNeighbours(i, j, neighbourhood));
                        else g.FillRectangle(new SolidBrush(grainColors[tab[i,j,0]-1]), i * grainPictureSize, j * grainPictureSize, grainPictureSize, grainPictureSize);
                        if (tab[i, j, 1] == 0) end = false;
                    }
                pictureBox1.Image = DrawArea;
                pictureBox1.Refresh();
                System.Threading.Thread.Sleep(200);
                System.Windows.Forms.Application.DoEvents();
            }

            g.Clear(Color.LightGray);
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    if (tab[i, j, 1] != 0) g.FillRectangle(new SolidBrush(grainColors[tab[i, j, 1]-1]), i * grainPictureSize, j * grainPictureSize, grainPictureSize, grainPictureSize);
                }
            pictureBox1.Image = DrawArea;
            pictureBox1.Refresh();
        }

        private int [] getNeighbours(int i, int j, int neighbourhood)
        {
            int[] toReturn;
            int nextColumn, prevColumn, upperRow, lowerRow;
            if (neighbourhood == 0)
            {
                if (i != width - 1) nextColumn = i + 1;
                else nextColumn = 0;

                if (i != 0) prevColumn = i - 1;
                else prevColumn = width - 1 ;

                if (j != height - 1) upperRow = j + 1;
                else upperRow = 0;

                if (j != 0) lowerRow = j - 1;
                else lowerRow = height - 1 ;

                toReturn = new int[4];
                toReturn[0] =tab[nextColumn, j,0];
                toReturn[1] = tab[prevColumn, j, 0];
                toReturn[2] = tab[i, upperRow, 0];
                toReturn[3] = tab[i, lowerRow, 0];

                if (checkBox1.Checked==false)
                {
                    if (i == width - 1) toReturn[0] = 0;
                    if (i == 0) toReturn[1] = 0;
                    if (j == height- 1) toReturn[2] = 0;
                    if (j == 0) toReturn[3] = 0;
                }
                return toReturn;

            }
            else if (neighbourhood == 1)
            {
                if (i != width - 1) nextColumn = i + 1;
                else nextColumn = 0;

                if (i != 0) prevColumn = i - 1;
                else prevColumn = width - 1;

                if (j != height - 1) upperRow = j + 1;
                else upperRow = 0;

                if (j != 0) lowerRow = j - 1;
                else lowerRow = height - 1;

                toReturn = new int[8];
                toReturn[0] = tab[nextColumn, j, 0];
                toReturn[1] = tab[prevColumn, j, 0];
                toReturn[2] = tab[i, upperRow, 0];
                toReturn[3] = tab[i, lowerRow, 0];
                toReturn[4] = tab[nextColumn, upperRow, 0];
                toReturn[5] = tab[prevColumn, lowerRow, 0];
                toReturn[6] = tab[prevColumn, upperRow, 0];
                toReturn[7] = tab[nextColumn, lowerRow, 0];
                if (checkBox1.Checked == false)
                {
                    if (i == width - 1)
                    {
                        toReturn[0] = 0;
                        toReturn[4] = 0;
                        toReturn[7] = 0;
                    }
                    if (i == 0)
                    {
                        toReturn[1] = 0;
                        toReturn[5] = 0;
                        toReturn[6] = 0;
                    }
                    if (j == height - 1)
                    {
                        toReturn[2] = 0;
                        toReturn[4] = 0;
                        toReturn[6] = 0;
                    }
                    if (j == 0)
                    {
                        toReturn[3] = 0;
                        toReturn[5] = 0;
                        toReturn[7] = 0;
                    }
                }
                return toReturn;
            }
            else if (neighbourhood == 2)
            {
                if (i != width - 1) nextColumn = i + 1;
                else nextColumn = 0;

                if (i != 0) prevColumn = i - 1;
                else prevColumn = width - 1;

                if (j != height - 1) upperRow = j + 1;
                else upperRow = 0;

                if (j != 0) lowerRow = j - 1;
                else lowerRow = height - 1;

                toReturn = new int[5];
                if (pentTab[i, j] == 0)
                {
                    //  pentagonalne lewe
                    toReturn[0] = tab[prevColumn, lowerRow, 0];
                    toReturn[1] = tab[prevColumn, j, 0];
                    toReturn[2] = tab[prevColumn, upperRow, 0];
                    toReturn[3] = tab[i, lowerRow, 0];
                    toReturn[4] = tab[i, upperRow, 0];
                    if (checkBox1.Checked == false)
                    {
                        if (i == 0)
                        {
                            toReturn[0] = 0;
                            toReturn[1] = 0;
                            toReturn[2] = 0;
                        }
                        if (j == height - 1)
                        {
                            toReturn[2] = 0;
                            toReturn[4] = 0;
                        }
                        if (j == 0)
                        {
                            toReturn[0] = 0;
                            toReturn[3] = 0;
                        }
                    }
                }
                else if (pentTab[i,j] == 1)
                {
                    // pentagonalne prawe
                    toReturn[0] = tab[nextColumn, lowerRow, 0];
                    toReturn[1] = tab[nextColumn, j, 0];
                    toReturn[2] = tab[nextColumn, upperRow, 0];
                    toReturn[3] = tab[i, lowerRow, 0];
                    toReturn[4] = tab[i, upperRow, 0];
                    if (checkBox1.Checked == false)
                    {
                        if (i == width - 1)
                        {
                            toReturn[0] = 0;
                            toReturn[1] = 0;
                            toReturn[2] = 0;
                        }
                        if (j == height - 1)
                        {
                            toReturn[2] = 0;
                            toReturn[4] = 0;
                        }
                        if (j == 0)
                        {
                            toReturn[0] = 0;
                            toReturn[3] = 0;
                        }
                    }
                }
                else if (pentTab[i, j] == 2)
                {
                    //  pentagonalne dolne
                    toReturn[0] = tab[prevColumn, lowerRow, 0];
                    toReturn[1] = tab[i, lowerRow, 0];
                    toReturn[2] = tab[nextColumn, lowerRow, 0];
                    toReturn[3] = tab[prevColumn, j, 0];
                    toReturn[4] = tab[nextColumn, j, 0];
                    if (checkBox1.Checked == false)
                    {
                        if (i == width - 1)
                        {
                            toReturn[2] = 0;
                            toReturn[4] = 0;
                        }
                        if (i == 0)
                        {
                            toReturn[0] = 0;
                            toReturn[3] = 0;
                        }
                        if (j == 0)
                        {
                            toReturn[0] = 0;
                            toReturn[1] = 0;
                            toReturn[2] = 0;
                        }
                    }
                }
                else if (pentTab[i, j] == 3)
                {
                    //  pentagonalne gorne
                    toReturn[0] = tab[prevColumn, upperRow, 0];
                    toReturn[1] = tab[i, upperRow, 0];
                    toReturn[2] = tab[nextColumn, upperRow, 0];
                    toReturn[3] = tab[prevColumn, j, 0];
                    toReturn[4] = tab[nextColumn, j, 0];
                    if (checkBox1.Checked == false)
                    {
                        if (i == width - 1)
                        {
                            toReturn[2] = 0;
                            toReturn[4] = 0;
                        }
                        if (i == 0)
                        {
                            toReturn[0] = 0;
                            toReturn[3] = 0;
                        }
                        if (j == height - 1)
                        {
                            toReturn[0] = 0;
                            toReturn[1] = 0;
                            toReturn[2] = 0;
                        }
                    }
                }

                return toReturn;
            }
            else if (neighbourhood == 3)
            {
                if (i != width - 1) nextColumn = i + 1;
                else nextColumn = 0;

                if (i != 0) prevColumn = i - 1;
                else prevColumn = width - 1;

                if (j != height - 1) upperRow = j + 1;
                else upperRow = 0;

                if (j != 0) lowerRow = j - 1;
                else lowerRow = height - 1;

                toReturn = new int[6];
                toReturn[0] = tab[nextColumn, j, 0];
                toReturn[1] = tab[prevColumn, j, 0];
                toReturn[2] = tab[i, upperRow, 0];
                toReturn[3] = tab[i, lowerRow, 0];
                toReturn[4] = tab[nextColumn, upperRow, 0];
                toReturn[5] = tab[prevColumn, lowerRow, 0];
                if (checkBox1.Checked == false)
                {
                    if (i == width - 1)
                    {
                        toReturn[0] = 0;
                        toReturn[4] = 0;
                    }
                    if (i == 0)
                    {
                        toReturn[1] = 0;
                        toReturn[5] = 0;
                    }
                    if (j == height - 1)
                    {
                        toReturn[2] = 0;
                        toReturn[4] = 0;
                    }
                    if (j == 0)
                    {
                        toReturn[3] = 0;
                        toReturn[5] = 0;
                    }
                }
                return toReturn;
            }
            else if (neighbourhood == 4)
            {
                if (i != width - 1) nextColumn = i + 1;
                else nextColumn = 0;

                if (i != 0) prevColumn = i - 1;
                else prevColumn = width - 1;

                if (j != height - 1) upperRow = j + 1;
                else upperRow = 0;

                if (j != 0) lowerRow = j - 1;
                else lowerRow = height - 1;

                toReturn = new int[6];
                toReturn[0] = tab[nextColumn, j, 0];
                toReturn[1] = tab[prevColumn, j, 0];
                toReturn[2] = tab[i, upperRow, 0];
                toReturn[3] = tab[i, lowerRow, 0];
                toReturn[4] = tab[prevColumn, upperRow, 0];
                toReturn[5] = tab[nextColumn, lowerRow, 0];
                if (checkBox1.Checked == false)
                {
                    if (i == width - 1)
                    {
                        toReturn[0] = 0;
                        toReturn[5] = 0;
                    }
                    if (i == 0)
                    {
                        toReturn[1] = 0;
                        toReturn[4] = 0;
                    }
                    if (j == height - 1)
                    {
                        toReturn[2] = 0;
                        toReturn[4] = 0;
                    }
                    if (j == 0)
                    {
                        toReturn[3] = 0;
                        toReturn[5] = 0;
                    }
                }
                return toReturn;
            }
            else if (neighbourhood == 5)
            {
                if (i != width - 1) nextColumn = i + 1;
                else nextColumn = 0;

                if (i != 0) prevColumn = i - 1;
                else prevColumn = width - 1;

                if (j != height - 1) upperRow = j + 1;
                else upperRow = 0;

                if (j != 0) lowerRow = j - 1;
                else lowerRow = height - 1;

                toReturn = new int[6];

                if (heksTab[i,j] == 0)
                {
                    toReturn[0] = tab[nextColumn, j, 0];
                    toReturn[1] = tab[prevColumn, j, 0];
                    toReturn[2] = tab[i, upperRow, 0];
                    toReturn[3] = tab[i, lowerRow, 0];
                    toReturn[4] = tab[nextColumn, upperRow, 0];
                    toReturn[5] = tab[prevColumn, lowerRow, 0];
                    if (checkBox1.Checked == false)
                    {
                        if (i == width - 1)
                        {
                            toReturn[0] = 0;
                            toReturn[4] = 0;
                        }
                        if (i == 0)
                        {
                            toReturn[1] = 0;
                            toReturn[5] = 0;
                        }
                        if (j == height - 1)
                        {
                            toReturn[2] = 0;
                            toReturn[4] = 0;
                        }
                        if (j == 0)
                        {
                            toReturn[3] = 0;
                            toReturn[5] = 0;
                        }
                    }
                }
                else
                {
                    toReturn[0] = tab[nextColumn, j, 0];
                    toReturn[1] = tab[prevColumn, j, 0];
                    toReturn[2] = tab[i, upperRow, 0];
                    toReturn[3] = tab[i, lowerRow, 0];
                    toReturn[4] = tab[prevColumn, upperRow, 0];
                    toReturn[5] = tab[nextColumn, lowerRow, 0];
                    if (checkBox1.Checked == false)
                    {
                        if (i == width - 1)
                        {
                            toReturn[0] = 0;
                            toReturn[5] = 0;
                        }
                        if (i == 0)
                        {
                            toReturn[1] = 0;
                            toReturn[4] = 0;
                        }
                        if (j == height - 1)
                        {
                            toReturn[2] = 0;
                            toReturn[4] = 0;
                        }
                        if (j == 0)
                        {
                            toReturn[3] = 0;
                            toReturn[5] = 0;
                        }
                    }
                }

                return toReturn;

            }
            else if (neighbourhood == 6)
            {
                List<int> elementList = new List<int>();
                for (int ii = 0; ii < width; ii++)
                    for (int jj = 0; jj < height; jj++)
                    {
                        double dist = CalculateManhattanDistance(tabX[i, j], tabX[ii, jj], tabY[i, j], tabY[ii, jj]);
                        if (dist < neighbourhoodRadius)
                        {
                            elementList.Add(tab[ii, jj, 0]);
                        }
                    }
                toReturn = new int[elementList.Count];
                for (int z=0; z<elementList.Count; z++)
                {
                    toReturn[z] = elementList[z];
                }
                return toReturn;
                        
            }
            toReturn = new int[1];
            toReturn[0] = 0;
            return toReturn;
        }

        private int grainValue(int [] neighbours)
        {
            int winNumber = 0, winID=0;
            int[] values = new int[neighbours.Length];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = 0;
                if (neighbours[i] != 0)
                { 
                    for (int j = 0; j < neighbours.Length; j++)
                    {
                        if (neighbours[i] == neighbours[j]) values[i]++;
                    }
                    if (values[i] > winNumber)
                    {
                        winNumber = values[i];
                        winID = neighbours[i];
                    }
                }
            }

            return winID;
        }

        public int CalculateManhattanDistance(int x1, int x2, int y1, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        public double CalculateManhattanDistance(double x1, double x2, double y1, double y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

    }
}
