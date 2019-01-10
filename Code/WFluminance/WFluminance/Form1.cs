using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WFluminance
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }
    // Corrected version, all elements locked
    //                  0    1    2    3    4    5    6    7 : rows in pictureBox1; "7" relates to gray rectangle
    static int[] V = { 242, 160, 174, 122, 124, 122, 242, 121 };
    static int[] R = new int[8]; //{ 0,   160, 174, 122, 0,   0,   0,   121 };
    static int[] G = new int[8]; //{ 0,   0,   0,   122, 124, 122, 0,   121 };
    static int[] B = new int[8]; //{ 242, 160, 0,   0,   0,   122, 242, 121 };
    Bitmap BmpPictBox1, BmpPictBox2;
    Graphics g1, g2;
    int unit, widthForm, widthGray, widthColor, widthGray2, widthColor2, heightColor; // color boxes
    int startColor, startGray; // boxes in pictureBox2
    int labWidth, numCol, numRow; // panel
    Color gray, gray2, gray3;
    int[] Max = new int[8];
    int[] MaxMin = new int[8];
    int[] Mean = new int[8];
    int[] MaxC = new int[8];


    private void MakeColors()
    {
      for (int ii = 0; ii < 8; ii++)
        switch (ii)
        {
          case 0: R[ii] = 0; G[ii] = 0; B[ii] = V[0]; break;
          case 1: R[ii] = V[1]; G[ii] = 0; B[ii] = V[1]; break;
          case 2: R[ii] = V[2]; G[ii] = 0; B[ii] = 0; break;
          case 3: R[ii] = V[3]; G[ii] = V[3]; B[ii] = 0; break;
          case 4: R[ii] = 0; G[ii] = V[4]; B[ii] = 0; break;
          case 5: R[ii] = 0; G[ii] = V[5]; B[ii] = V[5]; break;
          case 6: R[ii] = 0; G[ii] = 0; B[ii] = V[6]; break;
          case 7: R[ii] = V[7]; G[ii] = V[7]; B[ii] = V[7]; break;
        }
    }


    public void MakeMethods()
    {
      for (int k = 0; k < 8; k++)
      {
        Max[k] = Math.Max(R[k], Math.Max(G[k], B[k]));
        int min = Math.Min(R[k], Math.Min(G[k], B[k]));
        MaxMin[k] = (min + Max[k]) / 2;
        Mean[k] = (int)(((0.2126 * R[k]) + (0.7152 * G[k]) + (0.0722 * B[k])));
        MaxC[k] = Math.Max((int)(0.713 * R[k]), Math.Max((int)(1.00 * G[k]), (int)(0.527 * B[k])));
      }
    }


    //    CreateLabelsGrid(   7,          9,         35,           40);
    void CreateLabelsGrid(int numCol, int numRow, int labWidth, int labHeight)
    {
      Controls.Add(panel1);
      int ind;
      Label[,] LabelGrid = new Label[numCol, numRow];

      for (int k = 0; k < 8; k++)
      {
        Max[k] = Math.Max(R[k], Math.Max(G[k], B[k]));
        int min = Math.Min(R[k], Math.Min(G[k], B[k]));
        MaxMin[k] = (min + Max[k]) / 2;
        Mean[k] = (int)(((0.2126 * R[k]) + (0.7152 * G[k]) + (0.0722 * B[k])));
        MaxC[k] = Math.Max((int)(0.713 * R[k]), Math.Max((int)(1.00 * G[k]), (int)(0.527 * B[k])));
 
      }
      for (int x = 0; x < numCol; x++)
        for (int y = 0; y < numRow; y++)
        {
          ind = x + numCol * y;
          LabelGrid[x, y] = new Label();
          LabelGrid[x, y].SetBounds(x * labWidth, y * labHeight + 6, labWidth, labHeight);
          if (y == 0)
            switch (ind)
            {
              case 0: LabelGrid[x, y].Text = "R"; break;
              case 1: LabelGrid[x, y].Text = "G"; break;
              case 2: LabelGrid[x, y].Text = "B"; break;
              case 3: LabelGrid[x, y].Text = "M"; break;
              case 4: LabelGrid[x, y].Text = "L"; break;
              case 5: LabelGrid[x, y].Text = "Y"; break;
              case 6: LabelGrid[x, y].Text = "MC"; break;
            }
          else
          {
            switch (x)
            {
              case 0: LabelGrid[x, y].Text = Convert.ToString(R[y - 1]); break;
              case 1: LabelGrid[x, y].Text = Convert.ToString(G[y - 1]); break;
              case 2: LabelGrid[x, y].Text = Convert.ToString(B[y - 1]); break;
              case 3: LabelGrid[x, y].Text = Convert.ToString(Max[y - 1]); break;
              case 4: LabelGrid[x, y].Text = Convert.ToString(MaxMin[y - 1]); break;
              case 5: LabelGrid[x, y].Text = Convert.ToString(Mean[y - 1]); break;
              case 6: LabelGrid[x, y].Text = Convert.ToString(MaxC[y - 1]); break;
            }
          }
          panel1.Controls.Add(LabelGrid[x, y]);

        }
    } //******************************* end CreateLabelsGrid *************************************


    private void button1_Click(object sender, EventArgs e) // Start box 1
    {
      MakeColors();
      label6.Visible = true;
      int ind = 0;
      BmpPictBox1 = new Bitmap(pictureBox1.Width, pictureBox1.Height, PixelFormat.Format24bppRgb);
      g1 = Graphics.FromImage(BmpPictBox1);
      pictureBox1.Image = BmpPictBox1;
      Color color;
      SolidBrush myBrush1; 
      Rectangle rect;

      widthForm = this.Width;
      unit = widthForm / 100;
      Point P = new Point(12, 9*unit);
      pictureBox1.Location = P;

      P = new Point(36 * unit + 10, 6 * unit);
      panel1.Location = P;

      P = new Point(12, 7 * unit);
      label6.Location = P;
      label6.Text = "Gray = (" + R[7] + ", " + R[7] + ", " + R[7] + ")";

      P = new Point(16 * unit + 15, 7 * unit);
      label7.Location = P;     
      label7.Text = "Change intensity of changed channel";

      widthGray = 5 * pictureBox1.Width / 14;
      widthColor = 9 * pictureBox1.Width / 14;
      heightColor = pictureBox1.Height / 7;
      labWidth = pictureBox1.Width / 11;
      numCol = 7;
      numRow = 9;

      for (int ind1 = 0; ind1 < 7; ind1++) // color boxes
      {
        color = Color.FromArgb(R[ind1], G[ind1], B[ind1]);
        myBrush1 = new System.Drawing.SolidBrush(color);
        rect = new Rectangle(widthGray, heightColor * ind1, widthColor, heightColor);
        g1.FillRectangle(myBrush1, rect);
        pictureBox1.Image = BmpPictBox1;
      }


      ind = 7;  // big gray box
      color = Color.FromArgb(R[ind], G[ind], B[ind]);
      myBrush1 = new System.Drawing.SolidBrush(color);
      rect = new Rectangle(0, 0, widthGray, pictureBox1.Height);
      g1.FillRectangle(myBrush1, rect);
      CreateLabelsGrid(numCol, numRow, labWidth, heightColor);
      panel1.Visible = true;

      pictureBox1.Image = BmpPictBox1;
    } //********************************* end Start box 1 ************************************


    private void numericUpDown1_ValueChanged(object sender, EventArgs e)
    {
      int ind1 = 0;  // blue color
      B[ind1] = (int)numericUpDown1.Value;
      panel1.Controls[19].Text = numericUpDown1.Value.ToString();
      MakeMethods();
      panel1.Controls[28].Text = Max[ind1].ToString();
      panel1.Controls[37].Text = MaxMin[ind1].ToString();
      panel1.Controls[46].Text = Mean[ind1].ToString();
      panel1.Controls[55].Text = MaxC[ind1].ToString();
      Color color = Color.FromArgb(R[ind1], G[ind1], B[ind1]);
      SolidBrush myBrush1 = new System.Drawing.SolidBrush(color);
      Rectangle rect = new Rectangle(widthGray, heightColor * (ind1), widthColor, heightColor);
      g1.FillRectangle(myBrush1, rect);
      pictureBox1.Refresh(); // Image = BmpPictBox1;
    }

    private void numericUpDown2_ValueChanged(object sender, EventArgs e)
    {
      int ind1 = 1;
      B[ind1] = R[ind1] = (int)numericUpDown2.Value;
      panel1.Controls[2].Text = numericUpDown2.Value.ToString();
      panel1.Controls[20].Text = numericUpDown2.Value.ToString();
      MakeMethods();
      panel1.Controls[29].Text = Max[ind1].ToString();
      panel1.Controls[38].Text = MaxMin[ind1].ToString();
      panel1.Controls[47].Text = Mean[ind1].ToString();
      panel1.Controls[56].Text = MaxC[ind1].ToString();
      Color color = Color.FromArgb(R[ind1], G[ind1], B[ind1]);
      SolidBrush myBrush1 = new System.Drawing.SolidBrush(color);
      Rectangle rect = new Rectangle(widthGray, heightColor * (ind1), widthColor, heightColor);
      g1.FillRectangle(myBrush1, rect);
      pictureBox1.Image = BmpPictBox1;

    }

    private void numericUpDown3_ValueChanged(object sender, EventArgs e)
    {
      int ind1 = 2;
      R[ind1] = (int)numericUpDown3.Value;
      panel1.Controls[3].Text = numericUpDown3.Value.ToString();
      MakeMethods();
      panel1.Controls[30].Text = Max[ind1].ToString();
      panel1.Controls[39].Text = MaxMin[ind1].ToString();
      panel1.Controls[48].Text = Mean[ind1].ToString();
      panel1.Controls[57].Text = MaxC[ind1].ToString();
      
      Color color = Color.FromArgb(R[ind1], G[ind1], B[ind1]);
      SolidBrush myBrush1 = new System.Drawing.SolidBrush(color);
      Rectangle rect = new Rectangle(widthGray, heightColor * (ind1), widthColor, heightColor);
      g1.FillRectangle(myBrush1, rect);
      pictureBox1.Image = BmpPictBox1;

    }

    private void numericUpDown4_ValueChanged(object sender, EventArgs e)
    {
      int ind1 = 3;
      R[ind1] = G[ind1] = (int)numericUpDown4.Value;
      panel1.Controls[4].Text = numericUpDown4.Value.ToString();
      panel1.Controls[13].Text = numericUpDown4.Value.ToString();
      MakeMethods();
      panel1.Controls[31].Text = Max[ind1].ToString();
      panel1.Controls[40].Text = MaxMin[ind1].ToString();
      panel1.Controls[49].Text = Mean[ind1].ToString();
      panel1.Controls[58].Text = MaxC[ind1].ToString();
      
      Color color = Color.FromArgb(R[ind1], G[ind1], B[ind1]);
      SolidBrush myBrush1 = new System.Drawing.SolidBrush(color);
      Rectangle rect = new Rectangle(widthGray, heightColor * (ind1), widthColor, heightColor);
      g1.FillRectangle(myBrush1, rect);
      pictureBox1.Image = BmpPictBox1;

    }

    private void numericUpDown5_ValueChanged(object sender, EventArgs e)
    {
      int ind1 = 4;
      G[ind1] = (int)numericUpDown5.Value;
      panel1.Controls[14].Text = numericUpDown5.Value.ToString();
      MakeMethods();
      panel1.Controls[32].Text = Max[ind1].ToString();
      panel1.Controls[41].Text = MaxMin[ind1].ToString();
      panel1.Controls[50].Text = Mean[ind1].ToString();
      panel1.Controls[59].Text = MaxC[ind1].ToString();
      Color color = Color.FromArgb(R[ind1], G[ind1], B[ind1]);
      SolidBrush myBrush1 = new System.Drawing.SolidBrush(color);
      Rectangle rect = new Rectangle(widthGray, heightColor * (ind1), widthColor, heightColor);
      g1.FillRectangle(myBrush1, rect);
      pictureBox1.Image = BmpPictBox1;

    }

    private void numericUpDown6_ValueChanged(object sender, EventArgs e)
    {
      int ind1 = 5;
      G[ind1] = B[ind1] = (int)numericUpDown6.Value;
      panel1.Controls[15].Text = numericUpDown6.Value.ToString();
      panel1.Controls[24].Text = numericUpDown6.Value.ToString();
      MakeMethods();
      panel1.Controls[33].Text = Max[ind1].ToString();
      panel1.Controls[42].Text = MaxMin[ind1].ToString();
      panel1.Controls[51].Text = Mean[ind1].ToString();
      panel1.Controls[60].Text = MaxC[ind1].ToString();
      Color color = Color.FromArgb(R[ind1], G[ind1], B[ind1]);
      SolidBrush myBrush1 = new System.Drawing.SolidBrush(color);
      Rectangle rect = new Rectangle(widthGray, heightColor * (ind1), widthColor, heightColor);
      g1.FillRectangle(myBrush1, rect);

    }

    private void numericUpDown7_ValueChanged(object sender, EventArgs e)
    {
      int ind1 = 6;
      B[ind1] = (int)numericUpDown7.Value;
      panel1.Controls[25].Text = numericUpDown7.Value.ToString();
      MakeMethods();
      panel1.Controls[34].Text = Max[ind1].ToString();
      panel1.Controls[43].Text = MaxMin[ind1].ToString();
      panel1.Controls[52].Text = Mean[ind1].ToString();
      panel1.Controls[61].Text = MaxC[ind1].ToString();
      Color color = Color.FromArgb(R[ind1], G[ind1], B[ind1]);
      SolidBrush myBrush1 = new System.Drawing.SolidBrush(color);
      Rectangle rect = new Rectangle(widthGray, heightColor * (ind1), widthColor, heightColor);
      g1.FillRectangle(myBrush1, rect);
      
    }

    private void numericUpDown8_ValueChanged_1(object sender, EventArgs e)
    {
      int Comp = (int)numericUpDown8.Value;  // lightness of first gray box in pictBox2
      gray = Color.FromArgb(Comp, Comp, Comp);
      SolidBrush myBrush3 = new System.Drawing.SolidBrush(gray);
      Rectangle rect1 = new Rectangle(startGray, 40, widthColor2, 40);
      g2.FillRectangle(myBrush3, rect1);
      pictureBox2.Image = BmpPictBox2;

    }

    private void numericUpDown9_ValueChanged_1(object sender, EventArgs e)
    {
      int Comp = (int)numericUpDown9.Value;
      gray2 = Color.FromArgb(Comp, Comp, Comp);
      SolidBrush myBrush4 = new System.Drawing.SolidBrush(gray2);
      Rectangle rect1 = new Rectangle(startGray, 80, widthColor2, 40);
      g2.FillRectangle(myBrush4, rect1);
      pictureBox2.Image = BmpPictBox2;

    }

    private void numericUpDown10_ValueChanged_1(object sender, EventArgs e)
    {
      int Comp = (int)numericUpDown10.Value;
      gray3 = Color.FromArgb(Comp, Comp, Comp);
      SolidBrush myBrush4 = new System.Drawing.SolidBrush(gray3);
      Rectangle rect1 = new Rectangle(startGray, 120, widthColor2, 40);
      g2.FillRectangle(myBrush4, rect1);
      pictureBox2.Image = BmpPictBox2;

    }

    private void numericUpDown11_ValueChanged_1(object sender, EventArgs e)
    {
      int Comp = (int)numericUpDown11.Value;
      gray3 = Color.FromArgb(Comp, Comp, Comp);
      SolidBrush myBrush4 = new System.Drawing.SolidBrush(gray3);
      Rectangle rect1 = new Rectangle(startGray, 160, widthColor2, 40);
      g2.FillRectangle(myBrush4, rect1);
      pictureBox2.Image = BmpPictBox2;

    }

    private void numericUpDown12_ValueChanged_1(object sender, EventArgs e)
    {
      int Comp = (int)numericUpDown12.Value;
      gray3 = Color.FromArgb(Comp, Comp, Comp);
      SolidBrush myBrush4 = new System.Drawing.SolidBrush(gray3);
      Rectangle rect1 = new Rectangle(startGray, 200, widthColor2, 40);
      g2.FillRectangle(myBrush4, rect1);
      pictureBox2.Image = BmpPictBox2;

    }



    private void button2_Click(object sender, EventArgs e) // Start box 2
    {
      BmpPictBox2 = new Bitmap(pictureBox2.Width, pictureBox2.Height, PixelFormat.Format24bppRgb);
      g2 = Graphics.FromImage(BmpPictBox2);

      startColor = 9 * pictureBox2.Width / 32;
      startGray = 17 * pictureBox2.Width / 32;
      widthColor2 = pictureBox2.Width / 4;
      widthGray2 = pictureBox2.Width / 4;
      heightColor = pictureBox2.Height / 7;
      
      Point P = new Point(54 * unit + 20, 9 * unit);
      pictureBox2.Location = P;
      P = new Point(32 * unit, 28 * unit);
      pictureBox2.Size = (Size)P; 

      P = new Point(57 * unit + startGray + widthColor2, 14 * unit);
      numericUpDown8.Location = P;

      P = new Point(57 * unit + startGray + widthColor2, 18 * unit);
      numericUpDown9.Location = P;

      P = new Point(57 * unit + startGray + widthColor2, 22 * unit);
      numericUpDown10.Location = P;

      P = new Point(57 * unit + startGray + widthColor2, 26 * unit);
      numericUpDown11.Location = P;

      P = new Point(57 * unit + startGray + widthColor2, 30 * unit);
      numericUpDown12.Location = P;

      P = new Point(pictureBox2.Location.X + 30, pictureBox2.Location.Y + 20);
      label9.Location = P;
      label9.Text = "Color lightness";

      P = new Point(pictureBox2.Location.X + 210, pictureBox2.Location.Y + 20);
      label10.Location = P;
      label10.Text = "Change gray lightness";

      for (int i = 0; i < 5; i++)
      {
        P = new Point(pictureBox2.Location.X + 30, 
                          pictureBox2.Location.Y + 55 + heightColor * i);
        switch (i + 1)
        {
          case 1: label1.Location = P; label1.Text = "MC=182"; break;
          case 2: label2.Location = P; label2.Text = "MC=200"; break;
          case 3: label3.Location = P; label3.Text = "MC=125"; break;
          case 4: label4.Location = P; label4.Text = "MC=114"; break;
          case 5: label5.Location = P; label5.Text = "MC=114"; break;
        }
      }


      pictureBox2.Image = BmpPictBox2;

      SolidBrush brush = new SolidBrush(Color.LightGray);
      Rectangle rect = new Rectangle(0, 0, pictureBox2.Width, pictureBox2.Height);
      g2.FillRectangle(brush, rect);

      Color color2 = Color.FromArgb(255, 0, 0); // red box
      SolidBrush myBrush2 = new System.Drawing.SolidBrush(color2);
      rect = new Rectangle(startColor, 40, 80, 40);
      g2.FillRectangle(myBrush2, rect);

      gray = Color.FromArgb(180, 180, 180); // gray box
      myBrush2 = new System.Drawing.SolidBrush(gray);
      rect = new Rectangle(startGray, 40, 80, 40);
      g2.FillRectangle(myBrush2, rect);

      color2 = Color.FromArgb(255, 200, 200);
      myBrush2 = new System.Drawing.SolidBrush(color2);
      rect = new Rectangle(startColor, 80, 80, 40);
      g2.FillRectangle(myBrush2, rect);

      gray2 = Color.FromArgb(182, 182, 182);
      myBrush2 = new System.Drawing.SolidBrush(gray2);
      rect = new Rectangle(startGray, 80, 80, 40);
      g2.FillRectangle(myBrush2, rect);

      color2 = Color.FromArgb(175, 120, 0);
      myBrush2 = new System.Drawing.SolidBrush(color2);
      rect = new Rectangle(startColor, 120, 80, 40);
      g2.FillRectangle(myBrush2, rect);

      gray3 = Color.FromArgb(120, 120, 120);
      myBrush2 = new System.Drawing.SolidBrush(gray3);
      rect = new Rectangle(startGray, 120, 80, 40);
      g2.FillRectangle(myBrush2, rect);

      color2 = Color.FromArgb(160, 0, 222);
      myBrush2 = new System.Drawing.SolidBrush(color2);
      rect = new Rectangle(startColor, 160, 80, 40);
      g2.FillRectangle(myBrush2, rect);

      gray3 = Color.FromArgb(120, 120, 120);
      myBrush2 = new System.Drawing.SolidBrush(gray3);
      rect = new Rectangle(startGray, 160, 80, 40);
      g2.FillRectangle(myBrush2, rect);

      color2 = Color.FromArgb(160, 0, 160);
      myBrush2 = new System.Drawing.SolidBrush(color2);
      rect = new Rectangle(startColor, 200, 80, 40);
      g2.FillRectangle(myBrush2, rect);

      gray3 = Color.FromArgb(120, 120, 120);
      myBrush2 = new System.Drawing.SolidBrush(gray3);
      rect = new Rectangle(startGray, 200, 80, 40);
      g2.FillRectangle(myBrush2, rect);
      pictureBox2.Image = BmpPictBox2;
    } //*************************** end button2_Click ********************************************
  }
}
