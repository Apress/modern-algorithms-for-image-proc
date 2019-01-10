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

namespace PulseNoiseWF
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }
    private Bitmap origBmp;
    private Bitmap Result; // result of processing
    CImage Orig;  // copy of original image
    CImage Work;  // work image
    public Point[] v = new Point[20]; // corners of excluded rectangles, used in CPnoise Sort
    int Number, // number of defined elements "v"
      maxNumber = 8;
    bool Drawn = false, OPEN = false, BMP_Graph;
    public string OpenImageFile;
    public Graphics g;

    private void button1_Click(object sender, EventArgs e) // Open image
    {
      label3.Visible = false;
      label4.Visible = false;
      OpenFileDialog openFileDialog1 = new OpenFileDialog();
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        try
        {
          origBmp = new Bitmap(openFileDialog1.FileName);
          OpenImageFile = openFileDialog1.FileName;
          Number = 0;
        }
        catch (Exception ex)
        {
          MessageBox.Show("Error: Could not read file from disk. Original error: " +
           ex.Message);
        }
      }
      else return;

      label5.Text = "Opened file: " + openFileDialog1.FileName;
      label5.Visible = true;
      label1.Visible = true;
      label2.Visible = true;
      label3.Visible = true;
      label4.Visible = true;
      button3.Visible = true;
      textBox1.Visible = true;
      textBox2.Visible = true;
      progressBar1.Maximum = 100;
      progressBar1.Value = 0;
      progressBar1.Step = 1;
      progressBar1.Visible = true;

      byte[] Grid;
      if (origBmp.PixelFormat == PixelFormat.Format8bppIndexed)
      {
        Grid = new byte[3 * origBmp.Width * origBmp.Height]; 
        BitmapToGridOld(origBmp, Grid);
        BMP_Graph = false;
      }
      else if (origBmp.PixelFormat == PixelFormat.Format24bppRgb)
      {
        Grid = new byte[3 * origBmp.Width * origBmp.Height];
        BitmapToGrid(origBmp, Grid);
        BMP_Graph = true;
      }
      else
      {
        MessageBox.Show("Not suitable pixel format=" + origBmp.PixelFormat);
        return;
      }
      label3.Visible = true;

      progressBar1.Visible = false;

      pictureBox1.Image = origBmp;

      Orig = new CImage(origBmp.Width, origBmp.Height, 24, Grid);
      Work = new CImage(origBmp.Width, origBmp.Height, 24, Grid);

      OPEN = true;
    } //***************************** end Open image *************************

    private void BitmapToGridOld(Bitmap bmp, byte[] Grid)
    { progressBar1.Visible = true;
      Color color;
      for (int y = 0; y < bmp.Height; y++)
      {
        int y1 = 1 + bmp.Height / 100;
        if (y % y1 == 1) progressBar1.PerformStep();

        for (int x = 0; x < bmp.Width; x++)
        {
          int i = x + bmp.Width * y;
          color = origBmp.GetPixel(x, y);
          for (int c = 0; c < 3; c++)
          {
            if (c == 0) Grid[3 * i] = color.B;
            if (c == 1) Grid[3 * i + 1] = color.G;
            if (c == 2) Grid[3 * i + 2] = color.R;
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


    private void GridToBitmap(Bitmap bmp, byte[] Grid)
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

      bool EXPERIM = false;
      if (EXPERIM)
      {
        int[] X = { 3, 6, 7, 3, 6, 7, 6 };
        int[] Y = { 2, 2, 2, 5, 5, 5, 6 };
        for (int i = 0; i < 7; i++)
          for (int c = 0; c < 3; c++)
            Grid[c + 3 * (X[i] + bmp.Width * Y[i])] = 0;
      }

      progressBar1.Visible = true;
      for (int y = 0; y < bmp.Height; y++)
      {
        int y1 = 1 + bmp.Height / 100;
        if (y % y1 == 1) progressBar1.PerformStep();

        for (int x = 0; x < bmp.Width; x++)
        {
          if (nbyte == 1)  // nbyte corresponds to the PixelFormat of "bmp"
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
      progressBar1.Visible = false;
    } //****************************** end GridToBitmap ****************************************

    public int MaxC(int R, int G, int B)
    {
      int max;
      if (R * 0.713 > G) max = (int)(R * 0.713);
      else max = G;
      if (B * 0.527 > max) max = (int)(B * 0.527);
      return max;
    }

    private void button3_Click(object sender, EventArgs e) // Impulse noise
    {
      if (!OPEN)
      {
        MessageBox.Show("Please open an image");
        return;
      }
      Work.Copy(Orig);
      int nbyte = 3;
      Drawn = true;
      Work.DeleteBit0(nbyte);

      int maxLight, minLight;
      int[] histo = new int[256];
      for (int i = 0; i < 256; i++) histo[i] = 0;

      int light, index;
      int y1 = 1 + Work.height / 100;
      for (int y = 0; y < Work.height; y++)
      {
        if (y % y1 == 1) progressBar1.PerformStep();
        for (int x = 0; x < Work.width; x++) //======================================================
        { 
          index = x + y * Work.width; // Index of the pixel (x, y)
          light = MaxC(Work.Grid[3 * index+2] & 254, 
                        Work.Grid[3 * index + 1] & 254, 
                        Work.Grid[3 * index + 0] & 254);
          
          if (light < 0) light = 0;
          if (light > 255) light = 255;
          histo[light]++;
        } //============================ end for (int x=1; .. ========================================
      } //============================== end for (int y=1; .. ========================================
      progressBar1.Visible = false;
      for (maxLight = 255; maxLight > 0; maxLight--) if (histo[maxLight] != 0) break;
      for (minLight = 0; minLight < 256; minLight++) if (histo[minLight] != 0) break;
      CPnoise PN = new CPnoise(histo, 1000, 4000);
      
      PN.Sort(Work, histo, Number, pictureBox1.Width, pictureBox1.Height, this);
      progressBar1.Visible = false;

      int maxSizeD = 0;
      if (textBox1.Text != "") maxSizeD = int.Parse(textBox1.Text);
      int maxSizeL = 0;
      if (textBox2.Text != "") maxSizeL = int.Parse(textBox2.Text);
      PN.DarkNoise(ref Work, minLight, maxLight, maxSizeD, this);
      label4.Visible = true;

      progressBar1.Visible = false;
      Work.DeleteBit0(nbyte);

      PN.LightNoise(ref Work, minLight, maxLight, maxSizeL, this);
      progressBar1.Visible = false;

      Result = new Bitmap(origBmp.Width, origBmp.Height, PixelFormat.Format24bppRgb);

      progressBar1.Visible = true;
      int i1 = 1 + nbyte * origBmp.Width * origBmp.Height / 100;
      for (int i = 0; i < nbyte * origBmp.Width * origBmp.Height; i++)
      {
        if (i % i1 == 1) progressBar1.PerformStep();
        if (Work.Grid[i] == 252 || Work.Grid[i] == 254) Work.Grid[i] = 255;
      }
      progressBar1.Visible = false;
      GridToBitmap(Result, Work.Grid);  // both "GridToBitmap" and "GridToBitmapOld" usable

      pictureBox2.Image = Result;

      g = pictureBox1.CreateGraphics();
      Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Blue);
      for (int n = 0; n < Number; n += 2)
      {
        g.DrawLine(myPen, v[n + 1].X, v[n + 0].Y, v[n + 1].X, v[n + 1].Y);
        g.DrawLine(myPen, v[n + 0].X, v[n + 0].Y, v[n + 1].X, v[n + 0].Y);
        g.DrawLine(myPen, v[n + 0].X, v[n + 0].Y, v[n + 0].X, v[n + 1].Y);
        g.DrawLine(myPen, v[n + 0].X, v[n + 1].Y, v[n + 1].X, v[n + 1].Y);
      }
      progressBar1.Visible = false;
      button2.Visible = true;

    } //***************************** end Impulse noise ***********************



    private void button2_Click(object sender, EventArgs e) // Save image
    {
      SaveFileDialog dialog = new SaveFileDialog();
      if (dialog.ShowDialog() == DialogResult.OK)
      {
        string tmpFileName;
        if (dialog.FileName == OpenImageFile)
        {
          tmpFileName = OpenImageFile.Insert(OpenImageFile.IndexOf("."), "$$$");
          if (dialog.FileName.Contains(".jpg"))
            Result.Save(tmpFileName, ImageFormat.Jpeg); // saving tmpFile
          else 
            if (dialog.FileName.Contains(".bmp")) Result.Save(tmpFileName, ImageFormat.Bmp);
            else
            {
              MessageBox.Show("The file " + dialog.FileName + " has an inappropriate extension. Returning.");
              return;
            }

          origBmp.Dispose();
          File.Replace(tmpFileName, OpenImageFile,
                        OpenImageFile.Insert(OpenImageFile.IndexOf("."), "BackUp"));
          // Replaces the contents of 'OpenImageFile' with the contents of the file 'tmpFileName', 
          // deleting 'tmpFileName', and creating a backup of the 'OpenImageFile'.

          origBmp = new Bitmap(OpenImageFile);
          pictureBox1.Image = origBmp;
        }
        else
        {
          if (dialog.FileName.Contains(".jpg"))
            Result.Save(dialog.FileName, ImageFormat.Jpeg);
          else 
            if (dialog.FileName.Contains(".bmp")) Result.Save(dialog.FileName, ImageFormat.Bmp);
            else
            {
              MessageBox.Show("The file " + dialog.FileName + " has an inappropriate extension. Returning.");
              return;
            }
        }
        MessageBox.Show("The result image saved under " + dialog.FileName);
      }
    } //************************** end save image ***************************************


    private void pictureBox1_MouseClick_1(object sender, MouseEventArgs e)
    {
      Pen myPen;
      
      if (Drawn) 
      { 
        pictureBox1.Image = origBmp;
        Number = 0;
        Drawn = false;
      }

      int X = e.X;
      int Y = e.Y;
      v[Number].X = X;
      v[Number].Y = Y;
      if (Number < maxNumber)
        Number++;
      else
        MessageBox.Show("Number=" + Number + " is too high");

      myPen = new System.Drawing.Pen(System.Drawing.Color.Blue);
      if ((Number & 1) == 0)
        for (int n = 0; n < Number; n += 2)
        {
          g.DrawLine(myPen, v[n + 1].X, v[n + 0].Y, v[n + 1].X, v[n + 1].Y);
          g.DrawLine(myPen, v[n + 0].X, v[n + 0].Y, v[n + 1].X, v[n + 0].Y);
          g.DrawLine(myPen, v[n + 0].X, v[n + 0].Y, v[n + 0].X, v[n + 1].Y);
          g.DrawLine(myPen, v[n + 0].X, v[n + 1].Y, v[n + 1].X, v[n + 1].Y);
        }

    } //***************************** end pictureBox1_MouseClick_1 ***********************
  } //******************************* end Form ********************************************
} //********************************* end namespace ****************************************
