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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Remoting.Messaging;
using System.Reflection;

namespace WFpolyArc
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }
    private Bitmap OrigBmp;
    private Bitmap ResBmp;
    private Bitmap EdgeBmp; 
    public Bitmap BmpPictBox2;
    CImage OrigIm;  // copy of original image
    CImage SigmaIm;  // Sigma filtered
    CImage ExtremIm;  // Extrerm filtered
    CImage CombIm;  // Edge cells in combinatorial coordinates
    CImage EdgeIm;  // Detected edges
    public Panel PanelWithGrid;
    bool OPEN = false, EDGES = false, POLY = false;
    //int nLoop, denomProg; 
    int Threshold;
    double eps; 
    CListLines L;
    public double Scale1;
    public int marginX, marginY;
    public Graphics g2Bmp;
    bool Resized, Indexed;
    string FileName;
    public int WidthG, HeightG;


    private void button1_Click(object sender, EventArgs e) // Open image
    {
      label1.Visible = false;
      label2.Visible = false;
      label3.Visible = false;
      label4.Visible = false;
      label5.Visible = false;
      label8.Visible = false;
      button2.Visible = false;
      button3.Visible = false;
      button5.Visible = false;
      button8.Visible = false;
      numericUpDown1.Visible = false;
      numericUpDown8.Visible = false;

      OpenFileDialog openFileDialog1 = new OpenFileDialog();
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        try
        {
          OrigBmp = new Bitmap(openFileDialog1.FileName);
          FileName = openFileDialog1.FileName;
        }
        catch (Exception ex)
        {
          MessageBox.Show("Error: Could not read file from disk. Original error: " +
           ex.Message);
        }
      }
      else return;

      label1.Visible = true;
      label2.Visible = true;
      label2.Text = "Opened image: " + openFileDialog1.FileName;
      label5.Visible = true;
      label5.Text = "Click 'Detect edges'";
      button8.Visible = true;
      numericUpDown1.Visible = true;

      progressBar1.Maximum = 100;
      progressBar1.Value = 0;
      progressBar1.Step = 1;
      progressBar1.Visible = true;

      if (OrigBmp.PixelFormat == PixelFormat.Format8bppIndexed) Indexed = true;
      else Indexed = false;

      Resized = false;
      if (OrigBmp.Width < 500 && ! Indexed)
      {
        ResBmp = new Bitmap(2 * OrigBmp.Width, 2 * OrigBmp.Height, OrigBmp.PixelFormat);
        Resized = true;
        for (int y = 0; y < ResBmp.Height; y++)
          for (int x = 0; x < ResBmp.Width; x++)
            ResBmp.SetPixel(x, y, OrigBmp.GetPixel(x / 2, y / 2));
      }
      else ResBmp = new Bitmap(OrigBmp.Width, OrigBmp.Height, OrigBmp.PixelFormat); //Format24bppRgb);
      
      if (Resized)
      { OrigIm = new CImage(ResBmp.Width, ResBmp.Height, 24);
        if (ResBmp.PixelFormat == PixelFormat.Format8bppIndexed)
          OrigIm.BitmapToImageOld(ResBmp, this);
        else
          if (ResBmp.PixelFormat == PixelFormat.Format24bppRgb)
            OrigIm.BitmapToImage(ResBmp, this);
          else
          {
            MessageBox.Show("Form1: Inappropriate pixel format=" + ResBmp.PixelFormat);
            return;
          }
      }
      else
      { OrigIm = new CImage(OrigBmp.Width, OrigBmp.Height, 24);
        if (OrigBmp.PixelFormat == PixelFormat.Format8bppIndexed)
          OrigIm.BitmapToImageOld(OrigBmp, this);
        else
          if (OrigBmp.PixelFormat == PixelFormat.Format24bppRgb)
            OrigIm.BitmapToImage(OrigBmp, this);
          else
          {
            MessageBox.Show("Inappropriate pixel format=" + OrigBmp.PixelFormat);
            return;
          }
      }

      pictureBox1.Image = OrigBmp;
      label3.Visible = true;

      progressBar1.Visible = false;
      progressBar1.Maximum = 100;
      progressBar1.Step = 1;
 
      if (Resized)
      { WidthG = ResBmp.Width; HeightG = ResBmp.Height;
      }
      else
      { WidthG = OrigBmp.Width; HeightG = OrigBmp.Height;
      }
      
      SigmaIm = new CImage(WidthG, HeightG, 24);
      ExtremIm = new CImage(WidthG, HeightG, 24);
      EdgeIm = new CImage(WidthG, HeightG, 8);
      CombIm = new CImage(2 * WidthG + 1, 2 * HeightG + 1, 8);
      OPEN = true;

      EDGES = false;
      POLY = false;

      double ScaleX, ScaleY;
      ScaleX = (double)pictureBox2.Width / (double)OrigIm.width;
      ScaleY = (double)pictureBox2.Height / (double)OrigIm.height;
      if (ScaleX < ScaleY) Scale1 = ScaleX;
      else Scale1 = ScaleY;

      marginX = (pictureBox2.Width - (int)(Scale1 * OrigIm.width)) / 2;
      marginY = (pictureBox2.Height - (int)(Scale1 * OrigIm.height)) / 2;
      BmpPictBox2 = new Bitmap(ResBmp.Width, ResBmp.Height, PixelFormat.Format24bppRgb);

      g2Bmp = Graphics.FromImage(BmpPictBox2);

      pictureBox2.Image = BmpPictBox2;
    } //****************************** end Open image ***********************************


    public int MessReturn(string s)
    {
      if (MessageBox.Show(s,
              "Return", MessageBoxButtons.OKCancel) == DialogResult.Cancel) return -1;
      return 1;
    }


    private void numericUpDown1_ValueChanged(object sender, EventArgs e) // Change threshold
    {
      Threshold = (int)numericUpDown1.Value;
    } //******************************* end Change threshold *********************************


    private void button2_Click(object sender, EventArgs e) // Polygons
    {
      int nBits = CombIm.N_Bits;
      if (!EDGES)
      {
        MessageBox.Show("Press the button 'Edge detection' ");
        return;
      }
      int maxL2 = 40000, maxV = 40000, maxArc=40000;
      L = new CListLines(maxL2, maxV, maxArc);
 
      
      int NX = OrigIm.width;
      for (int y = 0; y < OrigIm.height; y++)
        for (int x = 0; x < OrigIm.width; x++)
          CombIm.Grid[2 * x + 1 + (2 * NX + 1) * (2 * y + 1)] = ExtremIm.Grid[x + NX * y]; 


      //CombIm.nLoop = nLoop;
      //CombIm.denomProg = denomProg;

      eps = (double) numericUpDown8.Value;
      L.SearchPoly(ref CombIm, eps, this);
      MessageBox.Show("Form1: eps=" + eps + " The list contains " + L.nPolygon + " polygons.");
      L.CheckSmooth(0.9, 0.5); 
      L.DrawPolygons(pictureBox2, OrigIm, this);
      label4.Text = "Polygons";
      label4.Visible = true;
      label5.Text = "Click 'Arcs'";
      button3.Visible = true;

      POLY = true;
    }  //*********************** end Polygons ***********************************************



    private void button5_Click(object sender, EventArgs e) // Save 
    {
      int indSlash = -1;
      int indDot = -1;
      for (int i = 0; i < FileName.Length; i++)
      {
        if (FileName[i] == '\\') indSlash = i;  // Position of the last '\'
        if (FileName[i] == '.') indDot = i;  // Position of the last '.'
      }
      char[] Path1 = new char[100];

      FileName.CopyTo(indSlash + 1, Path1, 0, indDot - indSlash - 1); //Path1 must be char[], no string

      string Name = new String(Path1, 0, indDot - indSlash - 1);
      // Takes indDot-IS-1 characters starting at the beginning of 'Path1'

      string SS = "C:\\VC_Projects\\TEXT\\.txt"; // New path for the text file

      SS = SS.Insert(SS.IndexOf(".txt"), Name); //sets Name for ".txt"

      using (StreamWriter sw = File.CreateText(SS)) // making the file
      {
        sw.WriteLine("List of " + L.nPolygon + " polygons");
        sw.WriteLine("First number - index of polygon followed by indices of first and last vertex, first and last arc.");
        for (int ip = 0; ip < L.nPolygon; ip++) //==========================================================================
        {
          sw.WriteLine("Polygon " + ip + " has firstVert=" + L.Polygon[ip].firstVert + "; lastVert=" +
            L.Polygon[ip].lastVert + "; firstArc=" + L.Polygon[ip].firstArc + "; lastArc=" + L.Polygon[ip].lastArc);
          int i = 0, iv = L.Polygon[ip].firstVert;
          char[] CH = { 'V', 'e', 'r', 't', 'i', 'c', 'e', 's', ':' };
          string Line = new String(CH);
          i = 9;
          do
          {
            Line = Line.Insert(i, " (");
            i+=2;
            Line = Line.Insert(i, L.Vert[iv].X.ToString());
            i += L.Vert[iv].X.ToString().Length;
            Line = Line.Insert(i, "; ");
            i += 2;
            Line = Line.Insert(i, L.Vert[iv].Y.ToString());
            i += L.Vert[iv].Y.ToString().Length;
            Line = Line.Insert(i, ")");
            i++;              
            iv++;
          } while (iv <= L.Polygon[ip].lastVert);
          sw.WriteLine(Line);
        } //==================================== end for (int ip ... =====================================================

        sw.WriteLine(" ");
        sw.WriteLine("List of " + L.nArc + " arcs");
        sw.WriteLine("First number - index of arc followed by coordinates of end points, those of curvature center and by radius");

        for (int ia = 0; ia < L.nArc; ia++)
        {
          sw.WriteLine("ia=" + ia + " (xb=" + Math.Round(L.Arc[ia].xb, 1) + "; yb=" + Math.Round(L.Arc[ia].yb, 1) + "; xe=" + 
            Math.Round(L.Arc[ia].xe, 1) + "; ye=" + Math.Round(L.Arc[ia].ye, 1) + "; xm=" + Math.Round(L.Arc[ia].xm, 1) +
            "; xm=" + Math.Round(L.Arc[ia].ym, 1) + "; rad=" + Math.Round(L.Arc[ia].rad, 1) + ")");
        }
        if (MessReturn("List of " + L.nPolygon + " polygons and " + L.nArc + " arcs saved in the text file " + SS) < 0) return;
      } //------------------------------ end using (StreamWriter sw = File.CreateText(SS)) --------------------------------------

      //Open the file to read from.
      using (StreamReader sr = File.OpenText(SS))
      {
        if (MessReturn("Contents of the saved text file: (can be interrupted)") < 0) return;
        string s = "";
        while ((s = sr.ReadLine()) != null)
        {
          if (MessReturn(s) < 0) return;
        }
      } 
    } //****************** end Save result ***********************************************


    private void button3_Click(object sender, EventArgs e) // Arcs
    {
      if (!EDGES)
      {
        MessageBox.Show("Please change the 'Threshold for edge detction' and click the button 'Polygons'");
        return;
      }
      if (!POLY)
      {
        MessageBox.Show("Please click the button 'Polygons'");
        return;
      }
      L.FindArcs(pictureBox2, EdgeIm, eps, this);
      label4.Text = "Detected arcs";
      label4.Visible = true;
      label5.Text = "you can click 'Save'";
      button5.Visible = true;


    }


    private void numericUpDown8_ValueChanged(object sender, EventArgs e) // epsilon
    {
      eps = (double) numericUpDown8.Value;
    }


    private void button8_Click(object sender, EventArgs e) // Detect edges
    {
      if (OPEN == false)
      {
        MessageBox.Show("Please open an image");
        return;
      }
      progressBar1.Visible = true;
      progressBar1.Value = 0;

      SigmaIm.SigmaSimpleUni(OrigIm, 1, 30, this);

      if (OrigIm.N_Bits == 24) ExtremIm.ExtremVarColor(SigmaIm, 2, this);
      else ExtremIm.ExtremVar(SigmaIm, 2, this);

      Threshold = (int)numericUpDown1.Value;
 
      CombIm.LabelCellsSign(Threshold, ExtremIm, this);
      CombIm.CleanCombNew(21, this);

      EdgeIm.CracksToPixel(CombIm);

      EdgeBmp = new Bitmap(OrigIm.width, OrigIm.height, PixelFormat.Format24bppRgb);
      EdgeIm.ImageToBitmapOld(BmpPictBox2, this);

      pictureBox2.Image = BmpPictBox2;
      label4.Text = "Detected edges";
      label4.Visible = true;
      label5.Text = "Click 'Polygons'";
      label5.Visible = true;
      label8.Visible = true;
      button2.Visible = true;
      numericUpDown8.Visible = true;
      progressBar1.Visible = false;
      EDGES = true;
    } //*********************************** end Detect edges ************************************


    private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
    // If you click to pictureBox2 after having detected edges you wiil see a fragment of the egdes.
    {
      int StandX = (int)((e.X - marginX)/Scale1);
      int StandY = (int)((e.Y - marginY)/Scale1);
      MessageBox.Show("Mouse; standard coord. of upper left corner: StandX=" + StandX + " StandY=" + StandY);
      CombIm.DrawComb(StandX, StandY, this); 
    } //************************************ end MouseClick *****************************************
  } //************************************** end Form1 ***********************************************
} //**************************************** end namespace ********************************************
