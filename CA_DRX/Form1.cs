using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CA_DRX
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
        //Color[] grainColors;
        List<Color> grainColors = new List<Color>();
        List<Color> DRXColors = new List<Color>();
        bool working, buttonContinue = false;
        int clickedCounter;

        int[,,] tab;
        int[,] pentTab;
        int[,] heksTab;
        double[,] tabX;
        double[,] tabY;

        int[,,] DRXTab;
        bool[,,] isRecrystallized;
        double[,,] density;
        List<double> summaryDensity = new List<double>();
        double A = 86710969050178.5;
        double B = 9.41268203527779;
        double DRXtime = 0.2;
        double xPercentage = 0.999;
        double criticalDensity = 4215840142323.42;
        double criticalDensityCell;
        int idDRX;

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            Point coordinates = me.Location;
            int x = coordinates.X / grainPictureSize;
            int y = coordinates.Y / grainPictureSize;
            if (working == false)
            {
                tab[x, y, 1] = clickedCounter + 1;
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

        private void button3_Click(object sender, EventArgs e)
        {

            DRXTab = new int[width, height, 2];
            isRecrystallized = new bool[width, height, 2];
            density = new double[width, height, 2];

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    for (int k = 0; k < 2; k++)
                    {
                        DRXTab[i, j, k] = tab[i, j, 1];
                        isRecrystallized[i, j, k] = false;
                        density[i, j, k] = 0;
                    }

            DRX();
            int xyz=0;
        }

        private void DRX()
        {
            g = Graphics.FromImage(DrawArea);

            Random rnd = new Random();
            int steps = 0;
            int quantityOfGrains = width * height;
            criticalDensityCell = criticalDensity / quantityOfGrains;
            idDRX = quantityOfGrains + 1;


            for (double t=0.0; t<=DRXtime; t+=0.001)
            {
                steps++;
                summaryDensity.Add(calculateDensity(t));
            }

            //  Every step of DRX
            for (int t = 1; t<steps; t++)
            {
                //  Saving results from last step
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        DRXTab[i, j, 0] = DRXTab[i, j, 1];
                        isRecrystallized[i, j, 0] = isRecrystallized[i, j, 1];
                        density[i, j, 0] = density[i, j, 1];
                    }

                //  Starting new step
                double deltaDensity = summaryDensity[t] - summaryDensity[t - 1];
                double densityToGive = xPercentage * deltaDensity;
                double densityLeftToGive = deltaDensity - densityToGive;
                double densityToGiveEach = densityToGive / quantityOfGrains;

                //  Giving to each cell
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        density[i, j, 1] = density[i, j, 0]+densityToGiveEach;
                    }

                //  Giving additional density
                int recievingAdditional = rnd.Next(quantityOfGrains * 1 / 1000, quantityOfGrains * 10 / 1000);
                //int recievingAdditional = 1;
                double additionalDensity = densityLeftToGive / (recievingAdditional*1.0);
                for (int i=0; i<recievingAdditional; i++)
                {
                    int ii = rnd.Next(width);
                    int jj = rnd.Next(height);

                    int rand = rnd.Next(11);
                    bool received = false;
                    if (isOnEdge(ii,jj) && rand < 8)
                    {
                        density[ii, jj, 1] += additionalDensity;
                        received = true;
                    }
                    else if (rand < 2)
                    {
                        density[ii, jj, 1] += additionalDensity;
                        received = true;
                    }
                    if (received == false) i--;
                }


                //  Grain Growth
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        //density[i, j, 1];
                        if (isOnEdge(i,j) & density[i,j,0]>criticalDensityCell & isRecrystallized[i,j,0]==false)
                        {
                            //  new grain
                            idDRX = idDRX + 1;
                            DRXTab[i, j, 1] = idDRX;
                            density[i, j, 1] = 0;
                            isRecrystallized[i, j, 1] = true;
                            DRXColors.Add(newRndColor());  
                        }
                        else if (shouldRecr(i, j))
                        {
                            //  recrystallizing
                            DRXTab[i, j, 1] = recrValue(i, j);
                            density[i, j, 1] = 0;
                            isRecrystallized[i, j, 1] = true;
                        }

                    }

                //  Visualizing changes
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        if (isRecrystallized[i, j, 0])
                        {
                            int index = DRXTab[i, j, 0] - quantityOfGrains-2;
                            g.FillRectangle(new SolidBrush(DRXColors[index]), i * grainPictureSize, j * grainPictureSize, grainPictureSize, grainPictureSize);
                        }
                    }

                System.Threading.Thread.Sleep(50);
                System.Windows.Forms.Application.DoEvents();
                pictureBox1.Image = DrawArea;
                pictureBox1.Refresh();
            }

        }

        private int recrValue2(int ii, int jj)
        {
            int[] neighbours = getNeighbours(DRXTab, ii, jj, neighbourhood);
            bool[] nei = getNeighboursRecr(isRecrystallized, ii, jj, neighbourhood);
            int winID = 0;
            for (int i=0; i<nei.Length; i++)
            {
                if (nei[i] == true) return neighbours[i];
            }

            return winID;
        }

        private int recrValue(int i, int j)
        {
            int winID = 0;
            int winCount = 0;
            int[] neighbours = getNeighbours(DRXTab, i, j, neighbourhood);
            bool[] nei = getNeighboursRecr(isRecrystallized, i, j, neighbourhood);

            for (int ii=0; ii<neighbours.Length; ii++)
            {
                int count = 0;
                for (int jj=0; jj<neighbours.Length; jj++)
                {
                    if (nei[jj] == true && neighbours[ii]==neighbours[jj])
                    {
                        count++;
                    }
                }

                if (count>winCount)
                {
                    winID = neighbours[ii];
                    winCount = count;
                }
               
            }


            return winID;
        }

        private bool shouldRecr(int i, int j)
        {
            //int [] nei = getNeighbours(DRXTab, i, j, neighbourhood);
            bool[] nei = getNeighboursRecr(isRecrystallized, i, j, neighbourhood);

            bool anyRecr = false;
            //to change for checking neighbours recrystalization
            for (int ii = 0; ii < nei.Length; ii++)
            {
                if (nei[ii]==true) anyRecr = true;
            }
            if (anyRecr == false) return false;

            double [] neiD = getNeighboursDens(density, i, j, neighbourhood);
            for (int ii=0; ii<neiD.Length; ii++)
            {
                if (density[i, j, 0] < neiD[ii]) return false;
            }

            return true;
        }

        private bool isOnEdge(int i, int j)
        {
            int[] nei = getNeighbours(DRXTab, i, j, neighbourhood);
            for (int ii=0; ii<nei.Length; ii++)
            {
                if (DRXTab[i, j, 0] != nei[ii]) return true;
            }
            return false;
        }

        private double calculateDensity(double t)
        {
            double d = 0;
            d = A / B + (1 - A / B) * Math.Exp(-B * t);
            return d;
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
            comboBox2.SelectedIndex = 1;
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
                for (int i = 0; i < grainQuantityHeight; i++)
                {
                    for (int j = 0; j < grainQuantityWidth; j++)
                    {
                        tab[(width / grainQuantityWidth / 2) + j * (width / grainQuantityWidth), (height / grainQuantityHeight / 2) + i * (height / grainQuantityHeight), 1] = counter;
                        counter++;
                    }
                }
            }
            else if (nucleation == 1)
            {
                grainQuantity = Convert.ToInt32(numericUpDown3.Value);
                grainRadius = Convert.ToInt32(numericUpDown4.Value);

                bool impossibility = false;

                for (int g = 0; g < grainQuantity; g++)
                {
                    bool onceAgain = true;

                    int rndQuantity = 0;
                    while (onceAgain == true && impossibility == false)
                    {
                        if (rndQuantity > 100)
                        {
                            MessageBox.Show("Too much points ot too wide radius. \n Grains:" + g, "Impossibility", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                grainQuantity = Convert.ToInt32(numericUpDown3.Value);
                for (int i = 0; i < grainQuantity; i++)
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
                        tabX[i, j] = i * 1.0 + (rnd.NextDouble() - 0.5);
                        tabY[i, j] = j * 1.0 + (rnd.NextDouble() - 0.5);
                    }

            }

            working = true;
            growth();
            working = false;
        }

        private void fillColourTable()
        {
            /*
            Random rnd = new Random();
            grainColors = new Color[grainQuantity];
            for (int i = 0; i < grainQuantity; i++)
            {
                grainColors[i] = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            }
            */
            grainColors.Clear();
            Random rnd = new Random();
            for (int i = 0; i < grainQuantity; i++)
            {
                grainColors.Add(Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256)));
            }
        }

        private Color newRndColor()
        {
            Random rnd = new Random();
            System.Threading.Thread.Sleep(1);
            return Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
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
                        if (tab[i, j, 0] == 0) tab[i, j, 1] = grainValue(getNeighbours(tab,i, j, neighbourhood));
                        else g.FillRectangle(new SolidBrush(grainColors[tab[i, j, 0] - 1]), i * grainPictureSize, j * grainPictureSize, grainPictureSize, grainPictureSize);
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
                    if (tab[i, j, 1] != 0) g.FillRectangle(new SolidBrush(grainColors[tab[i, j, 1] - 1]), i * grainPictureSize, j * grainPictureSize, grainPictureSize, grainPictureSize);
                }
            pictureBox1.Image = DrawArea;
            pictureBox1.Refresh();
        }

        private int[] getNeighbours(int[,,] tab, int i, int j, int neighbourhood)
        {
            int[] toReturn;
            int nextColumn, prevColumn, upperRow, lowerRow;
            if (neighbourhood == 0)
            {
                if (i != width - 1) nextColumn = i + 1;
                else nextColumn = 0;

                if (i != 0) prevColumn = i - 1;
                else prevColumn = width - 1;

                if (j != height - 1) upperRow = j + 1;
                else upperRow = 0;

                if (j != 0) lowerRow = j - 1;
                else lowerRow = height - 1;

                toReturn = new int[4];
                toReturn[0] = tab[nextColumn, j, 0];
                toReturn[1] = tab[prevColumn, j, 0];
                toReturn[2] = tab[i, upperRow, 0];
                toReturn[3] = tab[i, lowerRow, 0];

                if (checkBox1.Checked == false)
                {
                    if (i == width - 1) toReturn[0] = 0;
                    if (i == 0) toReturn[1] = 0;
                    if (j == height - 1) toReturn[2] = 0;
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
                else if (pentTab[i, j] == 1)
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

                if (heksTab[i, j] == 0)
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
                for (int z = 0; z < elementList.Count; z++)
                {
                    toReturn[z] = elementList[z];
                }
                return toReturn;

            }
            toReturn = new int[1];
            toReturn[0] = 0;
            return toReturn;
        }

        private double[] getNeighboursDens(double[,,] tab, int i, int j, int neighbourhood)
        {
            double[] toReturn;
            int nextColumn, prevColumn, upperRow, lowerRow;
            if (neighbourhood == 0)
            {
                if (i != width - 1) nextColumn = i + 1;
                else nextColumn = 0;

                if (i != 0) prevColumn = i - 1;
                else prevColumn = width - 1;

                if (j != height - 1) upperRow = j + 1;
                else upperRow = 0;

                if (j != 0) lowerRow = j - 1;
                else lowerRow = height - 1;

                toReturn = new double[4];
                toReturn[0] = tab[nextColumn, j, 0];
                toReturn[1] = tab[prevColumn, j, 0];
                toReturn[2] = tab[i, upperRow, 0];
                toReturn[3] = tab[i, lowerRow, 0];

                if (checkBox1.Checked == false)
                {
                    if (i == width - 1) toReturn[0] = 0;
                    if (i == 0) toReturn[1] = 0;
                    if (j == height - 1) toReturn[2] = 0;
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

                toReturn = new double[8];
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

                toReturn = new double[5];
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
                else if (pentTab[i, j] == 1)
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

                toReturn = new double[6];
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

                toReturn = new double[6];
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

                toReturn = new double[6];

                if (heksTab[i, j] == 0)
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
                List<double> elementList = new List<double>();
                for (int ii = 0; ii < width; ii++)
                    for (int jj = 0; jj < height; jj++)
                    {
                        double dist = CalculateManhattanDistance(tabX[i, j], tabX[ii, jj], tabY[i, j], tabY[ii, jj]);
                        if (dist < neighbourhoodRadius)
                        {
                            elementList.Add(tab[ii, jj, 0]);
                        }
                    }
                toReturn = new double[elementList.Count];
                for (int z = 0; z < elementList.Count; z++)
                {
                    toReturn[z] = elementList[z];
                }
                return toReturn;

            }
            toReturn = new double[1];
            toReturn[0] = 0;
            return toReturn;
        }

        private bool[] getNeighboursRecr(bool[,,] tab, int i, int j, int neighbourhood)
        {
            bool[] toReturn;
            int nextColumn, prevColumn, upperRow, lowerRow;
            if (neighbourhood == 0)
            {
                if (i != width - 1) nextColumn = i + 1;
                else nextColumn = 0;

                if (i != 0) prevColumn = i - 1;
                else prevColumn = width - 1;

                if (j != height - 1) upperRow = j + 1;
                else upperRow = 0;

                if (j != 0) lowerRow = j - 1;
                else lowerRow = height - 1;

                toReturn = new bool[4];
                toReturn[0] = tab[nextColumn, j, 0];
                toReturn[1] = tab[prevColumn, j, 0];
                toReturn[2] = tab[i, upperRow, 0];
                toReturn[3] = tab[i, lowerRow, 0];

                if (checkBox1.Checked == false)
                {
                    if (i == width - 1) toReturn[0] = false;
                    if (i == 0) toReturn[1] = false;
                    if (j == height - 1) toReturn[2] = false;
                    if (j == 0) toReturn[3] = false;
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

                toReturn = new bool[8];
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
                        toReturn[0] = false;
                        toReturn[4] = false;
                        toReturn[7] = false;
                    }
                    if (i == 0)
                    {
                        toReturn[1] = false;
                        toReturn[5] = false;
                        toReturn[6] = false;
                    }
                    if (j == height - 1)
                    {
                        toReturn[2] = false;
                        toReturn[4] = false;
                        toReturn[6] = false;
                    }
                    if (j == 0)
                    {
                        toReturn[3] = false;
                        toReturn[5] = false;
                        toReturn[7] = false;
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

                toReturn = new bool[5];
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
                            toReturn[0] = false;
                            toReturn[1] = false;
                            toReturn[2] = false;
                        }
                        if (j == height - 1)
                        {
                            toReturn[2] = false;
                            toReturn[4] = false;
                        }
                        if (j == 0)
                        {
                            toReturn[0] = false;
                            toReturn[3] = false;
                        }
                    }
                }
                else if (pentTab[i, j] == 1)
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
                            toReturn[0] = false;
                            toReturn[1] = false;
                            toReturn[2] = false;
                        }
                        if (j == height - 1)
                        {
                            toReturn[2] = false;
                            toReturn[4] = false;
                        }
                        if (j == 0)
                        {
                            toReturn[0] = false;
                            toReturn[3] = false;
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
                            toReturn[2] = false;
                            toReturn[4] = false;
                        }
                        if (i == 0)
                        {
                            toReturn[0] = false;
                            toReturn[3] = false;
                        }
                        if (j == 0)
                        {
                            toReturn[0] = false;
                            toReturn[1] = false;
                            toReturn[2] = false;
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
                            toReturn[2] = false;
                            toReturn[4] = false;
                        }
                        if (i == 0)
                        {
                            toReturn[0] = false;
                            toReturn[3] = false;
                        }
                        if (j == height - 1)
                        {
                            toReturn[0] = false;
                            toReturn[1] = false;
                            toReturn[2] = false;
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

                toReturn = new bool[6];
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
                        toReturn[0] = false;
                        toReturn[4] = false;
                    }
                    if (i == 0)
                    {
                        toReturn[1] = false;
                        toReturn[5] = false;
                    }
                    if (j == height - 1)
                    {
                        toReturn[2] = false;
                        toReturn[4] = false;
                    }
                    if (j == 0)
                    {
                        toReturn[3] = false;
                        toReturn[5] = false;
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

                toReturn = new bool[6];
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
                        toReturn[0] = false;
                        toReturn[5] = false;
                    }
                    if (i == 0)
                    {
                        toReturn[1] = false;
                        toReturn[4] = false;
                    }
                    if (j == height - 1)
                    {
                        toReturn[2] = false;
                        toReturn[4] = false;
                    }
                    if (j == 0)
                    {
                        toReturn[3] = false;
                        toReturn[5] = false;
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

                toReturn = new bool[6];

                if (heksTab[i, j] == 0)
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
                            toReturn[0] = false;
                            toReturn[4] = false;
                        }
                        if (i == 0)
                        {
                            toReturn[1] = false;
                            toReturn[5] = false;
                        }
                        if (j == height - 1)
                        {
                            toReturn[2] = false;
                            toReturn[4] = false;
                        }
                        if (j == 0)
                        {
                            toReturn[3] = false;
                            toReturn[5] = false;
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
                            toReturn[0] = false;
                            toReturn[5] = false;
                        }
                        if (i == 0)
                        {
                            toReturn[1] = false;
                            toReturn[4] = false;
                        }
                        if (j == height - 1)
                        {
                            toReturn[2] = false;
                            toReturn[4] = false;
                        }
                        if (j == 0)
                        {
                            toReturn[3] = false;
                            toReturn[5] = false;
                        }
                    }
                }

                return toReturn;

            }
            else if (neighbourhood == 6)
            {
                List<bool> elementList = new List<bool>();
                for (int ii = 0; ii < width; ii++)
                    for (int jj = 0; jj < height; jj++)
                    {
                        double dist = CalculateManhattanDistance(tabX[i, j], tabX[ii, jj], tabY[i, j], tabY[ii, jj]);
                        if (dist < neighbourhoodRadius)
                        {
                            elementList.Add(tab[ii, jj, 0]);
                        }
                    }
                toReturn = new bool[elementList.Count];
                for (int z = 0; z < elementList.Count; z++)
                {
                    toReturn[z] = elementList[z];
                }
                return toReturn;

            }
            toReturn = new bool[1];
            toReturn[0] = false;
            return toReturn;
        }

        private int grainValue(int[] neighbours)
        {
            int winNumber = 0, winID = 0;
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

