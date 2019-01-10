using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WFedgeDetect
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }
    private Bitmap origBmp;
    public Bitmap BmpPictBox1;
    public Bitmap BmpPictBox2;
    public Bitmap BmpPictBox3;
    public CImage OrigIm;  // copy of original image
    CImage SigmaIm;  // local mean
    CImage ExtremIm;  // shading corrected image and the result
    CImage CombIm;  // shading corrected image and the result
    CImage EdgeIm;  // shading corrected image and the result
    int nBit;
    bool OPEN = false;
    int nLoop, denomProg;
    public double Scale1;
    public int marginX, marginY;
    public int Threshold;
    public Graphics g1, g2, g3;
    public bool BmpGraph;
  

    private void button1_Click(object sender, EventArgs e) // Open image
    {
      label4.Visible = false;
      OpenFileDialog openFileDialog1 = new OpenFileDialog();
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        try
        {
          origBmp = new Bitmap(openFileDialog1.FileName);
        }
        catch (Exception ex)
        {
          MessageBox.Show("Error: Could not read file from disk. Error: " + ex.Message);
          return;
        }
      }
      else return;

      byte[] Grid;

      progressBar1.Step = 1;
      denomProg = progressBar1.Maximum / progressBar1.Step;
      nLoop = 2;
      progressBar1.Value = 0;
      progressBar1.Visible = true;
      label2.Visible = false;
      label3.Visible = false;
      label5.Visible = false;
      label6.Text = "Opened image: " + openFileDialog1.FileName;
      label6.Visible = true;

      if (origBmp.PixelFormat == PixelFormat.Format24bppRgb)
      {
        Grid = new byte[3 * origBmp.Width * origBmp.Height]; // color image
        nBit = 24;
        BitmapToGrid(origBmp, Grid);
        OPEN = true;
        BmpGraph = true;
      }
      else
        if (origBmp.PixelFormat == PixelFormat.Format8bppIndexed)
        {
          Grid = new byte[origBmp.Width * origBmp.Height]; // indexed image
          nBit = 8;
          BitmapToGridGet(origBmp, Grid);
          BmpGraph = false;
        }
        else
        {
          MessageBox.Show("Form1: Inappropriate pixel format. Returning.");
          return;
        }
      label4.Text = "Original image";
      label4.Visible = true;
      BmpGraph = false;
      progressBar1.Visible = false;
      BmpPictBox1 = new Bitmap(origBmp.Width, origBmp.Height, PixelFormat.Format24bppRgb);
      BmpPictBox2 = new Bitmap(origBmp.Width, origBmp.Height, PixelFormat.Format24bppRgb);
      BmpPictBox3 = new Bitmap(pictureBox3.Width, pictureBox3.Height);
      pictureBox1.Image = origBmp;
      pictureBox2.Image = BmpPictBox2;
      pictureBox3.Image = BmpPictBox3;
      if (BmpGraph)
      {
        g1 = Graphics.FromImage(BmpPictBox1);
        g2 = Graphics.FromImage(BmpPictBox2);
        g3 = Graphics.FromImage(BmpPictBox3);
      }
      else
      {
        g1 = pictureBox1.CreateGraphics();
        g2 = pictureBox2.CreateGraphics();
        g3 = pictureBox3.CreateGraphics();
      }
 
      OrigIm = new CImage(origBmp.Width, origBmp.Height, nBit, Grid);
      SigmaIm = new CImage(origBmp.Width, origBmp.Height, nBit, Grid);
      ExtremIm = new CImage(origBmp.Width, origBmp.Height, nBit, Grid);
      CombIm = new CImage(1 + 2 * origBmp.Width, 1 + 2 * origBmp.Height, 8); 
      EdgeIm = new CImage(origBmp.Width, origBmp.Height, 8, Grid);
      double ScaleX = (double)pictureBox1.Width / (double)OrigIm.width;
      double ScaleY = (double)pictureBox1.Height / (double)OrigIm.height;
      if (ScaleX < ScaleY) Scale1 = ScaleX;
      else Scale1 = ScaleY;
      marginX = (pictureBox1.Width - (int)(Scale1 * OrigIm.width)) / 2;
      marginY = (pictureBox1.Height - (int)(Scale1 * OrigIm.height)) / 2;
      OPEN = true;
    } //************************************** end Open image *********************************************

 

    private void BitmapToGridGet(Bitmap bmp, byte[] Grid)
    // Assigned both for color and grayscasle images
    {
      progressBar1.Visible = true;
      Color color;
      int nByte = nBit / 8;
      for (int y = 0; y < bmp.Height; y++)
      {
        int y1 = 1 + bmp.Height / 100;
        if (y % y1 == 1) progressBar1.PerformStep();

        for (int x = 0; x < bmp.Width; x++)
        {
          int i = x + bmp.Width * y;
          color = origBmp.GetPixel(x, y);
          for (int c = 0; c < nByte; c++)
          {
            if (c == 0) Grid[nByte * i] = color.B;
            if (c == 1) Grid[nByte * i + 1] = color.G;
            if (c == 2) Grid[nByte * i + 2] = color.R;
          }
        }
      }
      progressBar1.Visible = false;
    } //****************************** end BitmapToGridOld ****************************************



    private void BitmapToGrid(Bitmap bmp, byte[] Grid)
    {
      Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
      BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
      int nbyte;
      switch (bmp.PixelFormat)
      {
        case PixelFormat.Format24bppRgb: nbyte = 3; break;
        case PixelFormat.Format8bppIndexed: nbyte = 1; break;
        default: MessageBox.Show("BitmapToGrid: Inappropriate pixel format=" + bmp.PixelFormat); return;
      }
      IntPtr ptr = bmpData.Scan0;
      int length = Math.Abs(bmpData.Stride) * bmp.Height;
      byte[] rgbValues = new byte[length];
      System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, length);

      progressBar1.Visible = true;
      for (int y = 0; y < bmp.Height; y++)
      {
        int y1 = 1 + bmp.Height / 100;
        if (y % y1 == 1) progressBar1.PerformStep();

        for (int x = 0; x < bmp.Width; x++)
        {
          if (nbyte == 1)  // nbyte is global according to the PixelFormat of "bmp"
          {
            Color color = bmp.Palette.Entries[rgbValues[x + Math.Abs(bmpData.Stride) * y]]; // es war ohne 3*
            Grid[3 * (x + bmp.Width * y) + 0] = color.B;
            Grid[3 * (x + bmp.Width * y) + 1] = color.G;
            Grid[3 * (x + bmp.Width * y) + 2] = color.R;
          }
          else
            for (int c = 0; c < nbyte; c++)
            {
              Grid[c + nbyte * (x + bmp.Width * y)] = rgbValues[c + nbyte * x + Math.Abs(bmpData.Stride) * y];
            }
        }
      }
      bmp.UnlockBits(bmpData);
      progressBar1.Visible = false;
    } //****************************** end BitmapToGrid ****************************************


    public void GridToBitmap(Bitmap bmp, byte[] Grid)
    // Converts color Grid to Bitmap with any format.
    {
      Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
      BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
      int nbyte;
      switch (bmp.PixelFormat)
      {
        case PixelFormat.Format24bppRgb: nbyte = 3; break;
        case PixelFormat.Format8bppIndexed: nbyte = 1; break;
        default: MessageBox.Show("GridToBitmap: Inappropriate pixel format=" + bmp.PixelFormat); return;
      }
      IntPtr ptr = bmpData.Scan0;
      int size = bmp.Width * bmp.Height;
      int length = Math.Abs(bmpData.Stride) * bmp.Height;
      byte[] rgbValues = new byte[length];

      progressBar1.Visible = true;
      int y1 = 1 + nLoop * bmp.Height / 63; 
      for (int y = 0; y < bmp.Height; y++)
      {
        if (((y + 1) % y1) == 0) progressBar1.PerformStep();

        for (int x = 0; x < bmp.Width; x++)
        {
          if (nbyte == 1)  // nbyte is global according to the PixelFormat of "bmp"
          {
            Color color = bmp.Palette.Entries[Grid[3 * (x + bmp.Width * y)]];
            rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 0] = color.B;
            rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 1] = color.G;
            rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 2] = color.R;
          }
          else
            for (int c = 0; c < nbyte; c++)
            {
              rgbValues[c + nbyte * x + Math.Abs(bmpData.Stride) * y] =
                                      Grid[c + nbyte * (x + bmp.Width * y)];
            }
        }
      }
      System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, length);
      bmp.UnlockBits(bmpData);
    } //****************************** end GridToBitmap ****************************************


    private void GridToBitmap(Bitmap bmp, byte[] Grid, int nbyteG)
    // Converts Grid with "nbytesG" bytes per pixel to Bitmap with any format.
    {
      Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
      BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
      int nbyteB;
      switch (bmp.PixelFormat)
      {
        case PixelFormat.Format24bppRgb: nbyteB = 3; break;
        case PixelFormat.Format8bppIndexed: nbyteB = 1; break;
        default: MessageBox.Show("GridToBitmap: Inappropriate pixel format=" + bmp.PixelFormat); return;
      }
      IntPtr ptr = bmpData.Scan0;
      int size = bmp.Width * bmp.Height;
      int length = Math.Abs(bmpData.Stride) * bmp.Height;
      byte[] rgbValues = new byte[length];

      Color color;
      progressBar1.Visible = true;
      for (int y = 0; y < bmp.Height; y++)
      {
        int y1 = 1 + bmp.Height / 100;
        if (y % y1 == 1) progressBar1.PerformStep();

        for (int x = 0; x < bmp.Width; x++)
        {
          if (nbyteB == 1)  // nbyteB is defined by the PixelFormat of "bmp"
          { if (nbyteG == 3)
              color = bmp.Palette.Entries[Grid[3 * (x + bmp.Width * y)]]; // Grid is colore
            else
            { color = bmp.Palette.Entries[Grid[x + bmp.Width * y]];  // Grid is grayscale
              rgbValues[x + Math.Abs(bmpData.Stride) * y] = color.R;
           }
          }
          else // nbyteB == 3
          
            for (int c = 0; c < nbyteB; c++)
            { if (nbyteG == 3)
                rgbValues[c + nbyteB * x + Math.Abs(bmpData.Stride) * y] =
                                      Grid[c + nbyteB * (x + bmp.Width * y)];
               else
                rgbValues[c + nbyteB * x + Math.Abs(bmpData.Stride) * y] =
                        Grid[x + bmp.Width * y];
            }
        }
      }
      System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, length);
      bmp.UnlockBits(bmpData);
    } //****************************** end GridToBitmap ****************************************


    public void GridToBitmapSet(Bitmap bmp, byte[] Grid, int nbyte)
    // The argument "nByte" specifies the type of "Grid"
    {
      if (bmp.PixelFormat != PixelFormat.Format24bppRgb)
      {
        MessageBox.Show("GridToBitmapSet: the pixel format of 'bmp' must be 24");
        return;
      }

      progressBar1.Visible = true;
      int y1 = 1 + nLoop*bmp.Height / denomProg;
      for (int y = 0; y < bmp.Height; y++)
      {
        if (((y +1) % y1) == 0) progressBar1.PerformStep();
        if (nbyte == 3)
        {
          for (int x = 0; x < bmp.Width; x++)
            bmp.SetPixel(x, y, Color.FromArgb(Grid[nbyte * (x + bmp.Width * y) + 2],
                 Grid[nbyte * (x + bmp.Width * y) + 1], Grid[nbyte * (x + bmp.Width * y) + 0]));
        }
        else // nbyte == 1
        {
          for (int x = 0; x < bmp.Width; x++)
          {
            bmp.SetPixel(x, y, Color.FromArgb(Grid[x + bmp.Width * y],
                                   Grid[x + bmp.Width * y], Grid[x + bmp.Width * y]));
          }
        }
      }
    }//****************************** end GridToBitmapSet ****************************************


    public void GridToBitmapOld(Bitmap bmp, byte[] Grid)
    {
      progressBar1.Visible = true;
      int jump, Len = bmp.Height, nStep = 15;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;

      for (int y = 0; y < bmp.Height; y++)
      {
        if (y % jump == jump - 1) progressBar1.PerformStep();

        for (int x = 0; x < bmp.Width; x++)
        {
          bmp.SetPixel(x, y, Color.FromArgb(0, Grid[x + bmp.Width * y],
               Grid[x + bmp.Width * y], Grid[x + bmp.Width * y]));
        }
      }
      progressBar1.Visible = false;
    } //****************************** end GridToBitmapOld ****************************************



    private void button2_Click(object sender, EventArgs e) // Detect edges
    {
      if (OPEN == false)
      {
        MessageBox.Show("Please open an image");
        return;
      }
      progressBar1.Visible = true;
      progressBar1.Value = 0;

      SigmaIm.SigmaSimpleUni(OrigIm, 1, 30, this);
       if (OrigIm.N_Bits == 24)
      {
        ExtremIm.ExtremLightColor(SigmaIm, 2, 1, this);
      }
      else
      {
        ExtremIm.ExtremVar(SigmaIm, 2, this);
      }

      Threshold = (int)numericUpDown1.Value;
      int NX = OrigIm.width;

      int rv;
      rv = CombIm.LabelCellsSign(Threshold, ExtremIm, this);
      rv = CombIm.CleanCombNew(16, this);
      EdgeIm.CracksToPixel(CombIm, this);

      GridToBitmapOld(BmpPictBox2, EdgeIm.Grid);

      radioButton1.Visible = true;
      radioButton2.Visible = true;
      radioButton1.Checked = false;
      radioButton2.Checked = false;
      label5.Visible = true;

      pictureBox2.Refresh(); 
    } // ******************* end Detect edges *****************************************


    private void pictureBox2_MouseClick(object sender, MouseEventArgs e) // DrawComb
    {
      int StandX, StandY;
      if (!radioButton1.Checked && !radioButton2.Checked)
      {
         MessageBox.Show("Please click one of the right radio buttons");
      }
      if (radioButton2.Checked)
      {
        pictureBox1.Image = origBmp;
        radioButton1.Checked = false;
        StandX = (int)((e.X - marginX) / Scale1);
        StandY = (int)((e.Y - marginY) / Scale1);
        label2.Visible = true;
        label3.Visible = true;
        pictureBox3.Visible = true;
        ExtremIm.DrawImageLine(StandY, StandX, Threshold, SigmaIm, CombIm.Grid, this);
      }

      if (radioButton1.Checked)
      {
        StandX = (int)((e.X - marginX) / Scale1);
        StandY = (int)((e.Y - marginY) / Scale1);
        label2.Visible = false;
        label3.Visible = false;
        pictureBox3.Visible = false;
        CombIm.DrawComb(StandX, StandY, this);
      }
      if (BmpGraph)
      {
        pictureBox1.Refresh();
        pictureBox2.Refresh();
      }
    } //***************************** end MouseClick ******************************
  } //****************************** end Form1 *****************************************************
} //******************************** end namespace **************************************************
