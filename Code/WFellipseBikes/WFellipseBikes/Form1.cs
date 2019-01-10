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

namespace WFellipseBikes
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }
    private Bitmap OrigBmp;
    public Bitmap BmpPictBox2;
    private Bitmap EdgeBmp; // result of edge detection
    CImage OrigIm;  // copy of original image
    CImage HelpIm;  // copy of original image
    CImage SigmaIm;  // Sigma filtered
    CImage ExtremIm;  // Extrerm filtered
    public CImage CombIm;  // Edge cells in combinatorial coordinates
    public CImage EdgeIm;  // Detected edges
    bool OPEN = false; //, EDGES = false;
    int Threshold; 
    public Graphics g2Bmp; 
    public double eps; 
    public CListLines L;

    public double Scale1;
    public int marginX;
    public int marginY;
    public string FileName;
    public int width, height;
    public int Dir;
    Ellipse[] ListEllipse = new Ellipse[100];


    public int MessReturn(string s)
    {
      if (MessageBox.Show(s,
              "Return", MessageBoxButtons.OKCancel) == DialogResult.Cancel) return -1;
      return 1;
    }
    
    private void button1_Click_1(object sender, EventArgs e) // Open image
    {
      button2.Visible = false; 
      label2.Visible = false;
      numericUpDown1.Visible = false;
      button3.Visible = false;
      radioButton1.Visible = false;
      radioButton2.Visible = false;
      OpenFileDialog openFileDialog1 = new OpenFileDialog();
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        try
        {
          OrigBmp = new Bitmap(openFileDialog1.FileName);
        }
        catch (Exception ex)
        {
          MessageBox.Show("Error: Could not read file from disk. Original error: " +
            ex.Message);
        }
      }
      else return;


      Point P = new Point(20, 126);
      label1.Location = P;
      label1.Text = "File " + openFileDialog1.FileName;
      label1.Visible = true;
      button2.Visible = true;
      label2.Visible = true;
      numericUpDown1.Visible = true;
      label3.Visible = true;
      label4.Visible = false;
      label5.Text = "Click 'Detect edges'";
      label5.Visible = true;
      FileName = openFileDialog1.FileName;

      progressBar1.Maximum = 100;
      progressBar1.Value = 0;
      progressBar1.Step = 1;
      progressBar1.Visible = true;

      if (OrigBmp.Width < 300 || OrigBmp.Height < 300)
      {
        MessageBox.Show("The image " + FileName + " is too small: width=" + OrigBmp.Width +
          " height=" + OrigBmp.Height + ". No bicycle recognized.");
        return;
      }

      if (OrigBmp.PixelFormat == PixelFormat.Format8bppIndexed)
      {
        OrigIm = new CImage(OrigBmp.Width, OrigBmp.Height, 8);
        OrigIm.BitmapToImageOld(OrigBmp, this);
      }
      else
        if (OrigBmp.PixelFormat == PixelFormat.Format24bppRgb)
        {
          OrigIm = new CImage(OrigBmp.Width, OrigBmp.Height, 24);
          OrigIm.BitmapToImage(OrigBmp, this);
          
          if (OrigBmp.Width > 1200) // Resize
          {
            HelpIm = new CImage(OrigBmp.Width, OrigBmp.Height, 24);
            int Reduce = 2;
            if (OrigBmp.Width > 2400) Reduce = 4;
            HelpIm.FastAverageUni(OrigIm, Reduce / 2, this);
            width = (OrigBmp.Width + Reduce / 2) / Reduce;
            height = (OrigBmp.Height + Reduce / 2) / Reduce;
            OrigIm = new CImage(width, height, 24);
            OrigIm.Resize(HelpIm, OrigBmp.Width, OrigBmp.Height, 3, Reduce); //, ref width, ref height);
          }
        }
        else
        {
          MessageBox.Show("Form1: Inappropriate pixel format=" + OrigBmp.PixelFormat);
          return;
        }

      width = OrigIm.width;
      height = OrigIm.height;

      double ScaleX = (double)pictureBox2.Width / (double)OrigIm.width;
      double ScaleY = (double)pictureBox2.Height / (double)OrigIm.height;
      if (ScaleX < ScaleY) Scale1 = ScaleX;
      else Scale1 = ScaleY;
      marginX = (pictureBox2.Width - (int)(Scale1 * OrigIm.width)) / 2;
      marginY = (pictureBox2.Height - (int)(Scale1 * OrigIm.height)) / 2;

      
      OrigIm.nLoop = 2;

      SigmaIm = new CImage(OrigIm.width, OrigIm.height, 24);

      pictureBox1.Image = OrigBmp;

      progressBar1.Visible = false;
      progressBar1.Maximum = 100;
      progressBar1.Step = 1;
      OrigIm.denomProg = progressBar1.Maximum / progressBar1.Step;

      ExtremIm = new CImage(OrigIm.width, OrigIm.height, 24);
      EdgeIm = new CImage(OrigIm.width, OrigIm.height, 8);
      CombIm = new CImage(2 * OrigIm.width + 1, 2 * OrigIm.height + 1, 8);

      BmpPictBox2 = new Bitmap(width, height);
      g2Bmp = Graphics.FromImage(BmpPictBox2);
      pictureBox2.Image = BmpPictBox2;

      OPEN = true;
    }

    private void button2_Click_1(object sender, EventArgs e) // Detect edges
    {
      if (!OPEN)
      {
        MessageBox.Show("Please open an image");
        return;
      }
      progressBar1.Visible = true;
      progressBar1.Value = 0;

      if (OrigIm.N_Bits == 24)
      {
        SigmaIm.SigmaColor(OrigIm, 1, 30, this);
        ExtremIm.ExtremVarColor(SigmaIm, 2, this); // it was 2
      }
      else
      {
        SigmaIm.SigmaNewM(OrigIm, 1, 30, this);
        ExtremIm.ExtremVar(SigmaIm, 2, this); // it was 2
      }
      Threshold = (int)numericUpDown1.Value;
      int NX = OrigIm.width;

      CombIm.LabelCellsSign(Threshold, ExtremIm);

      CombIm.CleanCombNew(21, this);
      CombIm.CheckComb(7);

      for (int y = 0; y < OrigIm.height; y++)
        for (int x = 0; x < OrigIm.width; x++)
          if (ExtremIm.N_Bits == 24)
            CombIm.Grid[2 * x + 1 + (2 * NX + 1) * (2 * y + 1)] = ExtremIm.Grid[1 + 3 * (x + NX * y)];
          else
            CombIm.Grid[2 * x + 1 + (2 * NX + 1) * (2 * y + 1)] = SigmaIm.Grid[x + NX * y];

      EdgeIm.CracksToPixel(CombIm);

      EdgeBmp = new Bitmap(OrigIm.width, OrigIm.height, PixelFormat.Format24bppRgb);

      EdgeIm.ImageToBitmapOld(EdgeBmp, this); 

      pictureBox2.Image = EdgeBmp;

      int nBits = CombIm.N_Bits;
      int maxL2 = 40000, maxV = 150000, maxArc = 40000, sizeX = EdgeIm.width / 8, sizeY = EdgeIm.height / 8,
        nx = EdgeIm.width / sizeX + 1,
        ny = EdgeIm.height / sizeY + 1, maxArcInPScell = 1200;
      L = new CListLines(maxL2, maxV, maxArc, nx, ny, maxArcInPScell, sizeX, sizeY);

      eps = 2.0;
      
      switch(width / 500)
      {
        case 0: eps = 1.01; break;
        case 1: eps = 1.05; break;
        case 2: eps = 1.20; break;
        case 3: eps = 1.70; break;
      }

      L.SearchPoly(ref CombIm, eps, this);

      bool RECT = true;

      int nArcs = L.MakeArcsTwo(pictureBox2, this);

      L.MakeDarts(this);

      bool ORI = false;
      RECT = false;
      int nPoint = 1;
      L.DrawArcs(ORI, RECT, nPoint, this);
      label4.Visible = true;

      int nEllipse = 0;
      //int Dir, rv = -1;
      Dir = L.FindEllipsesMode(SigmaIm, ListEllipse, ref nEllipse, this);
      label4.Text = "Recognized bicycle";
      label4.Visible = true;
      label5.Visible = false;
      L.MakeDrawing(ListEllipse[0], ListEllipse[1], Dir, this);
      button3.Visible = true;
      radioButton1.Visible = true;
      radioButton2.Visible = true;
      label5.Text = "Click 'Save result'";
      label5.Visible = true;
      if (MessReturn("Form1 258: Processing finished. Returning.") < 0) return;
    }



    private void button3_Click_1(object sender, EventArgs e) // Save result
    {
      string TextFileName;
      if (radioButton1.Checked)
      {
        int indSlash = -1;
        int indDot = -1;
        for (int i = 0; i < FileName.Length; i++)
        {
          if (FileName[i] == '\\') indSlash = i;
          if (FileName[i] == '.') indDot = i;
        }
        char[] Path1 = new char[100];

        FileName.CopyTo(indSlash + 1, Path1, 0, indDot - indSlash - 1);

        string Name = new String(Path1, 0, indDot - indSlash - 1);
        TextFileName = "C:\\VC_Projects\\TEXT\\Bike_.txt";
        TextFileName = TextFileName.Insert(TextFileName.IndexOf(".txt"), Name);
      }
      else
      {
        SaveFileDialog dialog = new SaveFileDialog();
        MessageBox.Show("Save circles: Do not open an image in the comming window; look for the directory 'TEXT' and click a file " +
                                                                        " or write a name with extension '.txt' in the line below."); 

        if (dialog.ShowDialog() == DialogResult.OK) TextFileName = dialog.FileName;
        else TextFileName = "";
      }

      string[] dir = new string[3];
      dir[0] = "right";
      dir[2] = "left";

      // Create a file to write to:
      using (StreamWriter sw = File.CreateText(TextFileName)) // creating the file
      {
        sw.WriteLine("Bike going to " + dir[Dir] + " has elliptical wheels:");
        sw.WriteLine("First:  a= " + (int)ListEllipse[0].a + "; b= " + (int)ListEllipse[0].b + "; c=" + 
                                            (int)ListEllipse[0].c + "; d=" + (int)ListEllipse[0].d + ".");
        sw.WriteLine("Second: a=" + (int)ListEllipse[1].a + "; b=" + (int)ListEllipse[1].b + "; c=" + 
                                            (int)ListEllipse[1].c + "; d=" + (int)ListEllipse[1].d + ".");
      }

      // Open the file to read from.
      using (StreamReader sr = File.OpenText(TextFileName))
      {
        MessageBox.Show("TextFileName=" + TextFileName + ".  Contents of the saved text file:");
        string s = "";
        while ((s = sr.ReadLine()) != null)
        {
          MessageBox.Show(s);
        }
      }
    } //********************************** end Save result **************************************************************
  }
}
