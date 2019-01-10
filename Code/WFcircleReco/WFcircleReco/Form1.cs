using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
//using System.File;


// von for_GitHub

namespace WFcircleReco
{
  public partial class Form1 : Form
  {
    public Form1()
    {
        InitializeComponent();
    }
    private Bitmap OrigBmp;
    public Bitmap BmpPictBox1;
    public Bitmap BmpPictBox2;
    public Bitmap ResultBmp; // result of edge detection

    public CImage OrigIm;  // original image
    public CImage SigmaIm;  // Sigma filtered
    public CImage ExtremIm;  // Extrerm filtered
    public CImage CombIm;  // Edge cells in combinatorial coordinates
    public CImage EdgeIm;  // Detected edges
    public Panel PanelWithGrid;
    bool OPEN = false, EDGES = false, POLY = false;
    int nLoop, denomProg;
    int Threshold;
    public Graphics g1, g2;
    public double eps;
    public CListLines L;
    public double Scale1;
    public int marginX, marginY;
    public Label Label4;
    public int width, height;
    public CInscript Ins1, Ins2;
    public string OpenFileName;
    public CCircle[,] Circle;
    public CCircle[] BestCircles1, BestCircles2;
    public int[] nCircle;
    public int optVar1, optVar2, optNcircle1, optNcircle2, nBest;
    public bool radio1_Checked = false, radio2_Checked = false; 


    private void button1_Click(object sender, EventArgs e) // Open image
    {
      if (PanelWithGrid != null) PanelWithGrid.Dispose();

      label3.Visible = false;
      OpenFileDialog openFileDialog1 = new OpenFileDialog();
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
          try
          {
              OrigBmp = new Bitmap(openFileDialog1.FileName);
              OpenFileName = openFileDialog1.FileName;
          }
          catch (Exception ex)
          {
              MessageBox.Show("Error: Could not read file from disk. Original error: " +
                ex.Message);
          }
      }
      else return;

      BmpPictBox2 = new Bitmap(OrigBmp.Width, OrigBmp.Height, PixelFormat.Format24bppRgb);
      Point P = new Point(20, 150);
      label3.Visible = true;
      label3.Location = P;
      label3.Text = "Opened file: " + openFileDialog1.FileName;
      groupBox1.Visible = true;

      label7.Text = "Click one of the radio buttons in 'Areas of radius'";
      label7.Visible = true;

      button3.Visible = false;
      label2.Visible = false;
      numericUpDown2.Visible = false;
      button4.Visible = false;
      button5.Visible = false;


      OrigIm = new CImage(OrigBmp.Width, OrigBmp.Height, 24);

      progressBar1.Maximum = 100;
      progressBar1.Value = 0;
      progressBar1.Step = 1;
      progressBar1.Visible = true;

      double ScaleX = (double)pictureBox2.Width / (double)OrigIm.width;
      double ScaleY = (double)pictureBox2.Height / (double)OrigIm.height;
      if (ScaleX < ScaleY) Scale1 = ScaleX;
      else Scale1 = ScaleY;
      marginX = (pictureBox2.Width - (int)(Scale1 * OrigIm.width)) / 2;
      marginY = (pictureBox2.Height - (int)(Scale1 * OrigIm.height)) / 2;

      denomProg = OrigIm.denomProg = progressBar1.Maximum / progressBar1.Step;
      OrigIm.nLoop = nLoop = 2;
      if (OrigBmp.PixelFormat == PixelFormat.Format8bppIndexed) OrigIm.BitmapToImageOld(OrigBmp, this);
      else
        if (OrigBmp.PixelFormat == PixelFormat.Format24bppRgb) OrigIm.BitmapToImage(OrigBmp, this);
        else
        {
          MessageBox.Show("We do not use this pixel format=" + OrigBmp.PixelFormat);
          return;
        }
      SigmaIm = new CImage(OrigBmp.Width, OrigBmp.Height, 24);

      if (PanelWithGrid != null) PanelWithGrid.Dispose();
      pictureBox1.Image = OrigBmp;
      pictureBox2.Image = BmpPictBox2;

      progressBar1.Step = 1;
      OrigIm.denomProg = progressBar1.Maximum / progressBar1.Step;

      ExtremIm = new CImage(OrigBmp.Width, OrigBmp.Height, 24);
      EdgeIm = new CImage(OrigBmp.Width, OrigBmp.Height, 8);
      CombIm = new CImage(2 * OrigBmp.Width + 1, 2 * OrigBmp.Height + 1, 8);

      BmpPictBox1 = new Bitmap(OrigBmp.Width, OrigBmp.Height, PixelFormat.Format24bppRgb);

      g1 = Graphics.FromImage(BmpPictBox1);
      g2 = Graphics.FromImage(BmpPictBox2);
      label5.Visible = false;
      label6.Visible = false;

      progressBar1.Visible = false;
      OPEN = true;
      EDGES = false;
      POLY = false;
      label5.Visible = true;
      label5.Text = "Original image";
    } //**************************** end Open image *************************************


    private void button2_Click(object sender, EventArgs e) //  Edge detection
    {
      if (OPEN == false)
      {
          MessageBox.Show("Please open an image");
          return;
      }
      if (!radioButton1.Checked && !radioButton2.Checked)
      {
        MessageBox.Show("Please click one of the radio buttons on the left side");
        return;
      }

      progressBar1.Visible = true;
      progressBar1.Value = 0;

      progressBar1.Visible = true;
      SigmaIm.SigmaSimpleUni(OrigIm, 1, 30, this);
      ExtremIm.ExtremLightUni(SigmaIm, 3, this);

      Threshold = (int)numericUpDown1.Value;
      int NX = OrigIm.width;

      CombIm.LabelCellsSign(Threshold, ExtremIm, this);

      CombIm.CleanCombNew(81, this);

      int rv = CombIm.CheckComb(this);
      if (rv < 0) Application.Exit();

      EdgeIm.CracksToPixel(CombIm);

      // The image "CombIm" gets the pixel values as lightnes (MaxC) of pixels of "ExtremIm"
      for (int y = 0; y < OrigIm.height; y++)
          for (int x = 0; x < OrigIm.width; x++)
              CombIm.Grid[2 * x + 1 + (2 * NX + 1) * (2 * y + 1)] =
                CombIm.MaxC(ExtremIm.Grid[3 * (x + NX * y) + 2], ExtremIm.Grid[3 * (x + NX * y) + 1], ExtremIm.Grid[3 * (x + NX * y) + 0]);

      EdgeIm.nLoop = nLoop;
      EdgeIm.denomProg = denomProg;
      EdgeIm.ImageToBitmapOld(BmpPictBox2, this);
      pictureBox2.Refresh();

      progressBar1.Visible = false;

      label6.Visible = true;
      label6.Text = "Detected edges";
      button3.Visible = true;
      label2.Visible = true;
      numericUpDown2.Visible = true;
      label7.Text = "Click 'Pollygons'";
      label7.Visible = true;
      if (PanelWithGrid != null) PanelWithGrid.Dispose();

      EDGES = true;
    } //*************************** end edge detection ******************************


    public double TestBestCircles(CCircle[] Circle, int[] BestCircles, int nBest)
    {
      double SumAngle = 0.0;
      for (int i = 0; i < nBest; i++)
      {
          SumAngle += Circle[BestCircles[i]].Angle;
      }
      return SumAngle;
    }


    public int MessReturn(string s)
    {
      if (MessageBox.Show(s,
              "Return", MessageBoxButtons.OKCancel) == DialogResult.Cancel) return -1;
      return 1;
    }


    public double TestCircles(CCircle[,] Circle, int iVar, int[] nCircles)
    {
      bool[] Added = new bool[nCircles[iVar]];
      int i;
      for (i = 0; i < nCircles[iVar]; i++) Added[i] = false;
      double dist = 0.0, SumAngle = 0.0;
      SumAngle = 0.0;
      for (i = 0; i < nCircles[iVar]; i++) //====================================================================
      {
        if (Added[i]) continue;
       for (int j = i + 1; j < nCircles[iVar]; j++) //====================================================================
        {
          dist = Math.Sqrt((Circle[iVar, j].Mx - Circle[iVar, i].Mx) * (Circle[iVar, j].Mx - Circle[iVar, i].Mx) +
            (Circle[iVar, j].My - Circle[iVar, i].My) * (Circle[iVar, j].My - Circle[iVar, i].My));
          if (dist < Circle[iVar, i].R || dist < Circle[iVar, j].R)
          {
            if (Circle[iVar, i].Angle >= Circle[iVar, j].Angle)
            {
              SumAngle += Circle[iVar, i].Angle;
              Circle[iVar, j].good = false;
              Added[i] = true;
            }
            else
            {
              SumAngle += Circle[iVar, j].Angle;
              Circle[iVar, i].good = false;
            }

            nCircles[iVar]--;
          } //-------------------- end if (dist < Circle[iVar, i].R  -----------------------------------
        }  //================================= end for (int j ... ===================================
        if (!Added[i]) SumAngle += Circle[iVar, i].Angle;
      }  //===================================== end for (int i ... ==============================================
      return SumAngle;
    } //********************************** end TestCircles ***************************************




    public double TestCircles1(CCircle[,] Circle, int iVar, int nCircles)
    {
      double dist = 0.0, SumAngle = 0.0;
      for (int i = 0; i < nCircles; i++)
      {
        SumAngle = Circle[iVar, i].Angle;
        for (int j = i + 1; j < nCircles; j++)
        {
          dist = Math.Sqrt((Circle[iVar, j].Mx - Circle[iVar, i].Mx) * (Circle[iVar, j].Mx - Circle[iVar, i].Mx) +
            (Circle[iVar, j].My - Circle[iVar, i].My) * (Circle[iVar, j].My - Circle[iVar, i].My));
          if (dist < Circle[iVar, i].R || dist < Circle[iVar, j].R)
          {
            if (Circle[iVar, i].Angle >= Circle[iVar, j].Angle)
            {
                Circle[iVar, j].good = false;
                SumAngle += Circle[iVar, i].Angle;
            }
            else
            {
                Circle[iVar, i].good = false;
                SumAngle += Circle[iVar, j].Angle;
            }
          }
          else SumAngle += Circle[iVar, i].Angle;
        }
      }
      return SumAngle;
    }


    private void button3_Click(object sender, EventArgs e) // Polygons
    {
      int nBits = CombIm.N_Bits;
      if (!EDGES)
      {
          MessageBox.Show("Click edge detection");
          return;
      }
      if (!radioButton1.Checked && !radioButton2.Checked)
      {
          MessageBox.Show("Choose a radio button with 'Radii'");
          return;
      }
      OrigIm.ImageToBitmapOld(BmpPictBox1, this);

      int maxL2 = 100000, maxV = 220000, maxArc = 40000;
      L = new CListLines(maxL2, maxV, maxArc);

      int marginX = (pictureBox2.Width - (int)(Scale1 * OrigIm.width)) / 2;
      int marginY = (pictureBox2.Height - (int)(Scale1 * OrigIm.height)) / 2;

      int PicBoxInd = 1;
      if (radioButton1.Checked)
      {
        Ins1 = new CInscript(PicBoxInd, 0.6, 0, 0, OrigIm.width, Color.Yellow, this);
        radio1_Checked = true;
      }
      else
        if (radioButton2.Checked)
        {
          Ins1 = new CInscript(PicBoxInd, 0.8, 0, 0, OrigIm.width, Color.Yellow, this);
          radio2_Checked = true;
        }

      PicBoxInd = 2;
      Ins2 = new CInscript(PicBoxInd, 0.5, 0, 0, OrigIm.width, Color.White, this);

      CombIm.nLoop = nLoop;
      CombIm.denomProg = denomProg;

                    //  0   1  2   3   4   5    6    7
      int[] minRad = { 10, 14, 23, 35, 53, 80, 120, 180 };
      int[] maxRad = { 20, 30, 46, 70, 106, 160, 240, 360 };
      double[] ProtEps = { 1.05, 1.15, 1.23, 1.35, 1.50, 1.67, 1.85, 2.0 };

      double maxAngle1 = 0.0, maxAngle2 = 0.0;
      optVar1 = optVar2 = -1;
      optNcircle1 = optNcircle2 = 0;
      int nVar = 8;

      int maxCircle = 2000;
      nBest = 0;
      nCircle = new int[nVar];

      Circle = new CCircle[nVar, maxCircle]; // declared in Form1
      for (int iv = 0; iv < nVar; iv++)
          for (int i = 0; i < maxCircle; i++) Circle[iv, i] = new CCircle();

      BestCircles1 = new CCircle[6000];
      for (int i = 0; i < 600; i++) BestCircles1[i] = new CCircle();
      BestCircles2 = new CCircle[600];
      for (int i = 0; i < 600; i++) BestCircles2[i] = new CCircle();
      int nArcs = 0, rv = 1;

      int iVarStart = 0, iVarEnd = 7;

      if (radioButton1.Checked) iVarEnd = 4;
      else
      if (radioButton2.Checked) iVarStart = 3;
        else MessageBox.Show("Chose a radio button with 'Radii'");
      progressBar1.Value = 0;
      progressBar1.Visible = true;
      progressBar1.Step = 20;

      for (int iVar = iVarStart; iVar <= iVarEnd; iVar++) //===========================================
      {
        progressBar1.PerformStep();
          eps = ProtEps[iVar];
        numericUpDown2.Value = (decimal)eps;
        rv = L.SearchPoly(ref CombIm, eps, this);
        if (rv < 0) break;
        double maxSinus = 0.7, maxProportion = 0.2;
        L.CheckSmooth(maxSinus, maxProportion);
        rv = L.DrawPolygons(pictureBox2, EdgeIm, this);
        EDGES = false;
        POLY = true;

        nArcs = L.MakeArcs3(pictureBox2, minRad[iVar], maxRad[iVar], eps, this);
        if (nArcs < 0) break;
        nBest = 0;
        nCircle[iVar] = 0;
        int[] iBestCircles1 = new int[100];
        nCircle[iVar] = L.MakeCirclesEl(Circle, iVar, ref nCircle[iVar], minRad[iVar], maxRad[iVar],
                                                                    iBestCircles1, ref nBest);

        if (nCircle[iVar] < 0) break;
        double SumAngle;
        //if (nCircle[iVar] > 0) SumAngle = TestCircles(Circle, iVar, nCircle[iVar]);
        if (nCircle[iVar] > 0) SumAngle = TestCircles(Circle, iVar, nCircle);
        else SumAngle = 0.0;

        if (radioButton1.Checked)
        {
          int cnt = 0;
          if (SumAngle > maxAngle1)
          {
              cnt = 0;
              for (int iCircle = 0; iCircle < nCircle[iVar]; iCircle++)
              {
                  if (Circle[iVar, iCircle].good)
                  {
                      BestCircles1[cnt] = Circle[iVar, iCircle];
                      cnt++;
                  }
              }
              maxAngle1 = SumAngle;
              optVar1 = iVar;
              optNcircle1 = cnt;
          }
        }

        if (radioButton2.Checked)
        {
          int cnt = 0;
          if (SumAngle > maxAngle2)
          {
              cnt = 0;
              for (int iCircle = 0; iCircle < nCircle[iVar]; iCircle++)
              {
                  if (Circle[iVar, iCircle].good)
                  {
                      BestCircles2[cnt] = Circle[iVar, iCircle];
                      cnt++;
                  }
              }
              maxAngle2 = SumAngle;
              optVar2 = iVar;
              optNcircle2 = cnt;
            }
          }
      } //=========================== end for (int iVar... ==================================
      if (rv < 0) return;
      progressBar1.Visible = false;
      if (radioButton1.Checked)
      {
        nBest = L.ShowBestCircles3(BestCircles1, optNcircle1, 2.0, this);
        MessageBox.Show("Form1: Number of recognized circles is " + nBest);
      }

      if (radioButton2.Checked)
      {
        nBest = L.ShowBestCircles3(BestCircles2, optNcircle2, 1.0, this);
        MessageBox.Show("Form1: Number of recognized circles is " + nBest);
      }

      label5.Visible = true;
      label5.Text = "Recognized circles";
      label6.Visible = true;
      label6.Text = "Polygons";

      button4.Visible = true;
      button5.Visible = true;
      groupBox2.Visible = true;


    } //*************************** end Polygons *******************************


    private void button5_Click(object sender, EventArgs e) // Print circles
    {
      PanelWithGrid = new Panel();
      if (radioButton1.Checked) L.PrintCircles(BestCircles1, optNcircle1, this);
      else L.PrintCircles(BestCircles2, optNcircle2, this);
    }


    private void pictureBox2_MouseClick(object sender, MouseEventArgs e) // Click for "DrawComb"
    {
      int StandX = (int)((double)(e.X - marginX) / Scale1);
      int StandY = (int)((double)(e.Y - marginY) / Scale1);
      CombIm.DrawComb(StandX, StandY, false, this);
    } //*************************** end Click for "DrawComb" **********************************


    private void button4_Click(object sender, EventArgs e) // Save circles
    {
      if (!POLY)
      {
        MessageBox.Show("Click the button 'Polygons'");
        return;
      }
      string TextFileName;

      if (!radioButton3.Checked && !radioButton4.Checked)
      {
        if (MessReturn("Save circles: please click one of the the radio buttons at right side and click 'Save circles' again.") < 0) return;
        return;
      }

      if (radioButton3.Checked) // automatic
      {
          int indSlash = -1;
          int indDot = -1;
          for (int i = 0; i < OpenFileName.Length; i++)
          {
              if (OpenFileName[i] == '\\') indSlash = i;  // Position of the last '\'
              if (OpenFileName[i] == '.') indDot = i;  // Position of the last '.'
          }
          char[] Path1 = new char[100];

          OpenFileName.CopyTo(indSlash + 1, Path1, 0, indDot - indSlash - 1); //Path1 must be char[], no string
          //Copies indDot-indSlash-1 charakters from the pos. (indSlash+1) in 'OpenFileName' to 'Path1'


          string Name = new String(Path1, 0, indDot - indSlash - 1);
          // Takes indDot-IS-1 characters starting at the beginning of 'Path1'

          TextFileName = "C:\\VC_Projects\\TEXT\\CirclesOf_.txt"; // New path for the text file

          TextFileName = TextFileName.Insert(TextFileName.IndexOf(".txt"), Name); //sets Name for ".txt"
      }
      else   // radioButton4.Checked; defined:
      {
        SaveFileDialog dialog = new SaveFileDialog();
        MessageBox.Show("Save circles: Do not open an image in the comming window; look for the directory 'TEXT' and click a file " +
          " or write a name with extension '.txt' in the line below."); 
              
        if (dialog.ShowDialog() == DialogResult.OK)
        {
          TextFileName = dialog.FileName;
        }
        else TextFileName = "";
      } //---------------------------- end if (radioButton3.Checked) -------------------------------------
 
      // Create a file to write to:
      using (StreamWriter sw = File.CreateText(TextFileName)) // creating the file
      {
        if (radioButton3.Checked)
        {
          sw.WriteLine("List of " + nBest + " recognized circles");
          if (radioButton1.Checked)
            for (int ic = 0; ic < nBest; ic++)
            {
                sw.WriteLine("Circle " + ic + " Center (Mx=" + Math.Round(BestCircles1[ic].Mx, 1) + "; My=" +
                  Math.Round(BestCircles1[ic].My, 1) + "); Radius R=" + Math.Round(BestCircles1[ic].R, 1) + ")");
            }
          else // radioButton2.Checked
            for (int ic = 0; ic < nBest; ic++)
            {
              sw.WriteLine("Circle " + ic + " Center (Mx=" + Math.Round(BestCircles2[ic].Mx, 1) + "; My=" +
                Math.Round(BestCircles2[ic].My, 1) + "); Radius R=" + Math.Round(BestCircles2[ic].R, 1) + ")");
            }
          if (MessReturn("List of " + nBest + " circles saved in the text file " + TextFileName) < 0) return;
          sw.Close();
        }
        else // radioButton4.Checked
        {
          sw.WriteLine("List of " + nBest + " recognized circles");
          if (radioButton1.Checked)
            for (int ic = 0; ic < nBest; ic++)
            {
                sw.WriteLine("Circle " + ic + " Center (Mx=" + Math.Round(BestCircles1[ic].Mx, 1) + "; My=" +
                  Math.Round(BestCircles1[ic].My, 1) + "); Radius R=" + Math.Round(BestCircles1[ic].R, 1) + ".");
            }
          else // radioButton2.Checked
            for (int ic = 0; ic < nBest; ic++)
            {
              sw.WriteLine("Circle " + ic + " Center (Mx=" + Math.Round(BestCircles2[ic].Mx, 1) + "; My=" +
                Math.Round(BestCircles2[ic].My, 1) + "); Radius R=" + Math.Round(BestCircles2[ic].R, 1) + ".");
            }

          sw.Close();
          if (MessReturn("List of " + nBest + " circles saved in the text file " + TextFileName) < 0) return;
        }
 
        // Open the file to read from.
        using (StreamReader sr = File.OpenText(TextFileName))
        {
          if (MessReturn("Contents of the saved text file:") < 0) return;
          string s = "";
          while ((s = sr.ReadLine()) != null)
          {
              if (MessReturn(s) < 0) return;
          }
        }
      } //*************************** end using ****************************************
    } //**************************** end Save circles ***********************************

    private void radioButton1_CheckedChanged(object sender, EventArgs e)
    {
      button2.Visible = true;
      label1.Visible = true;
      numericUpDown1.Visible = true;
      label7.Text = "Click 'Detect edges'";
      label7.Visible = true;

    }

    private void radioButton2_CheckedChanged(object sender, EventArgs e)
    {
      button2.Visible = true;
      label1.Visible = true;
      numericUpDown1.Visible = true;
      label7.Text = "Click 'Detect edges'";
      label7.Visible = true;

    } 
  } //****************************** end Form1 *******************************************
} //******************************** end namespace ****************************************
