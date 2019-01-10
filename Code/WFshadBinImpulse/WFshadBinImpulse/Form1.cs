using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace WFshadBinImpulse
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }
    private Bitmap origBmp;
    private Bitmap ShadingBmp; // result of shading correction
    private Bitmap ResultBmp; // result of processing histo
    CImage OrigIm;  // copy of original image
    CImage SigmaIm;  // sigma filtered original image
    CImage GrayIm;  // grayscale image for calculating MeanIm
    CImage MeanIm;  // local mean
    CImage ShadIm;  // shading corrected image and the result
    CImage ImpulseIm;  // shading corrected image and the result
    CImage BinIm;  // shading corrected image and the result
    public Point[] v = new Point[20]; // corners of excluded rectangles, used in CPnoise Sort
    int Number, maxNumber = 12; // number and max. number of defined elements "v"
    bool DIV, Drawn = false, OPEN = false, SHAD = false, BIN = false, CHOICE = false;
    int KIND = 1, marginX, marginY, nbyteIm, nbyteBmp, width, height, Threshold;
    double ScaleX, ScaleY, Scale1;
    String OpenImageFile;

    private void button1_Click_1(object sender, EventArgs e)  // Open image
    {
      pictureBox1.Visible = false;
      pictureBox2.Visible = false;
      pictureBox3.Visible = true;
      groupBox1.Visible = false;

      OpenFileDialog openFileDialog1 = new OpenFileDialog(); 
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        label12.Visible = false;
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
 
      label12.Text = "Opened image: " + OpenImageFile;
      label12.Visible = true;
      label11.Visible = true;
      label6.Visible = true;

      button2.Visible = false;
      button3.Visible = false;
      button4.Visible = false;
      numericUpDown1.Visible = false;
      numericUpDown2.Visible = false;
      numericUpDown4.Visible = false;
      numericUpDown5.Visible = false;
      label1.Visible = false;
      label2.Visible = false;
      label4.Visible = false;
      label5.Visible = false;
      label7.Visible = false;
      label8.Visible = false;
      label9.Visible = false;
      label10.Visible = false;
      pictureBox2.Visible = false;
      pictureBox3.Visible = false;
      
      groupBox1.Visible = true;

      progressBar1.Maximum = 100;
      progressBar1.Value = 0;
      progressBar1.Step = 1;
      progressBar1.Visible = true;

      if (origBmp.PixelFormat == PixelFormat.Format8bppIndexed) nbyteBmp = 1;
      else
        if (origBmp.PixelFormat == PixelFormat.Format24bppRgb) nbyteBmp = 3;
        else
        {
          MessageBox.Show("Pixel format=" + origBmp.PixelFormat + " not used in this project.");
          return;
        }

      nbyteIm = BitmapToImage(origBmp, ref OrigIm);

      pictureBox1.Visible = true;
      pictureBox1.Image = origBmp;

      width = origBmp.Width;
      height = origBmp.Height;

      int N_Bits = nbyteIm * 8;

      SigmaIm = new CImage(width, height, N_Bits);
      SigmaIm.SigmaSimpleUni(OrigIm, 2, 50);
      GrayIm = new CImage(width, height, 8);
      if (nbyteIm == 3) GrayIm.ColorToGray(SigmaIm);
      else GrayIm.Copy(SigmaIm);
      MeanIm = new CImage(width, height, 8);
      ShadIm = new CImage(width, height, N_Bits);
      ImpulseIm = new CImage(width, height, N_Bits);
      BinIm = new CImage(width, height, 8);

      ScaleX = (double)pictureBox1.Width / (double)width;
      ScaleY = (double)pictureBox1.Height / (double)height;
      Scale1 = Math.Min(ScaleX, ScaleY);
      marginX = (pictureBox1.Width - (int)(Scale1 * width)) / 2;
      marginY = (pictureBox1.Height - (int)(Scale1 * height)) / 2;
      ShadingBmp = new Bitmap(origBmp.Width, origBmp.Height, PixelFormat.Format24bppRgb);

      pictureBox2.Visible = false;
      pictureBox3.Visible = false;
      radioButton1.Checked = false;
      radioButton2.Checked = false;
      radioButton3.Checked = false;
      radioButton4.Checked = false;
      progressBar1.Visible = false;

      KIND = -1;
      OPEN = true;
    } //********************************** end Open image *************************************


    public int BitmapToImage(Bitmap bmp, ref CImage Image)
    // Converts any bitmap to a color or to a grayscale image.
    {
      int nbyteIm = 1, rv = 0, x, y;
      Color color;

      if (nbyteBmp == 1)  // nbyteBmp is member of "Form1" according to the PixelFormat of "bmp"
      {
        x = 10;
        y = 2;
        color = bmp.GetPixel(x, y);
        if (color.R != color.G) nbyteIm = 3;
        Image = new CImage(bmp.Width, bmp.Height, nbyteIm * 8);

        progressBar1.Visible = true;
        for (y = 0; y < bmp.Height; y++) //========================================================
        {
          int jump = bmp.Height / 100;
          if (y % jump == jump - 1) progressBar1.PerformStep();

          for (x = 0; x < bmp.Width; x++) //======================================================
          {
            color = bmp.GetPixel(x, y);
            if (nbyteIm == 3)
            {
              Image.Grid[3 * (x + bmp.Width * y) + 0] = color.B;
              Image.Grid[3 * (x + bmp.Width * y) + 1] = color.G;
              Image.Grid[3 * (x + bmp.Width * y) + 2] = color.R;
            }
            else // nbyteIm == 1:
              Image.Grid[x + bmp.Width * y] = color.R;
          } //================================== end for (x ... ===================================
        } //==================================== end for (y ... =====================================
        rv = nbyteIm;
      }
      else // nbyteBmp == 3 and nbyteIm == 3:
      {
        Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
        IntPtr ptr = bmpData.Scan0;
        int Str = bmpData.Stride;
        int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
        byte[] rgbValues = new byte[bytes];
        System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

        nbyteIm = 3;
        Image = new CImage(bmp.Width, bmp.Height, nbyteIm * 8);
        for (y = 0; y < bmp.Height; y++) //=============================================
        {
          int jump = bmp.Height / 100;
          if (y % jump == jump - 1) progressBar1.PerformStep();
          for (x = 0; x < bmp.Width; x++)
            for (int c = 0; c < nbyteIm; c++)
              Image.Grid[c + nbyteIm * (x + bmp.Width * y)] =
                rgbValues[c + nbyteBmp * x + Math.Abs(bmpData.Stride) * y];
        } //========================= end for (y = 0; ... ============================== 
        rv = nbyteIm;
        bmp.UnlockBits(bmpData);
      }
      return rv;
    } //****************************** end BitmapToImage ****************************************


    public int MessReturn(string s)
    {
      if (MessageBox.Show(s, "Return", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
        return -1;
      return 1;
    }

    public int Round(double x)
    {
      if (x < 0.0) return (int)(x - 0.5);
      return (int)(x + 0.5);
    }

    public int MaxC(int R, int G, int B)
    {
      int max;
      if (R * 0.713 > G) max = (int)(R * 0.713);
      else max = G;
      if (B * 0.527 > max) max = (int)(B * 0.527);
      return max;
    }

    public byte MaxC(byte R, byte G, byte B)
    {
      byte max;
      if (R * 0.713 > G) max = (byte)(R * 0.713);
      else max = G;
      if (B * 0.527 > max) max = (byte)(B * 0.527);
      return max;
    }


    public void CorrectShading(bool DIV)
    {
      if (!OPEN)
      {
        MessageBox.Show("Please open an image");
        return;
      }
      int c, i, x, y;
      int[] color = { 0, 0, 0 };
      int Lightness = (int)numericUpDown2.Value;
      int hWind = (int)(numericUpDown1.Value * width / 1000);
      MeanIm.FastAverageM(GrayIm, hWind, this); 
      progressBar1.Visible = true;
      pictureBox2.Visible = true;
      pictureBox3.Visible = true;
      progressBar1.Value = 0;

      int[] histo = new int[256];
      for (i = 0; i < 256; i++) histo[i] = 0;
      byte lum = 0;
      int nbyteIm = SigmaIm.N_Bits / 8;
      int jump = height / 50; // width and height are properties of Form1
      for (y = 0; y < height; y++) //==================================================
      {
        if (y % jump == jump - 1) progressBar1.PerformStep();
        for (x = 0; x < width; x++)
        {                               // nbyteIm is member of 'Form1'
          for (c = 0; c < nbyteIm; c++) //==============================================
          {
            if (DIV)
              color[c] = Round(SigmaIm.Grid[c + nbyteIm * (x + width * y)] * Lightness /
                                      (double)MeanIm.Grid[x + width * y]); // Division
            else
              color[c] = Round(SigmaIm.Grid[c + nbyteIm * (x + width * y)] + Lightness -
                                    (double)MeanIm.Grid[x + width * y]); // Subtraction
            if (color[c] < 0) color[c] = 0;
            if (color[c] > 255) color[c] = 255;
            ShadIm.Grid[c + nbyteIm * (x + width * y)] = (byte)color[c];
          } //======================= end for (c... ==================================
  
          if (nbyteIm == 1) lum = (byte)color[0];
          else lum = (byte)MaxC((byte)color[2], (byte)color[1], (byte)color[0]);
          histo[lum]++;
        }
      } //============================ end for (y... ===================================

      // Calculating  MinLight and MaxLight:
      int MaxLight, MinLight, Sum = 0;
      for (MinLight = 0; MinLight < 256; MinLight++)
      {
        Sum += histo[MinLight];
        if (Sum > width * height / 100) break;
      }
      Sum = 0;
      for (MaxLight = 255; MaxLight >= 0; MaxLight--)
      {
        Sum += histo[MaxLight];
        if (Sum > width * height / 100) break;
      }

      // Calculating LUT:
      byte[] LUT = new byte[256];
      for (i = 0; i < 256; i++) 
        if (i <= MinLight) LUT[i] = 0;
        else
          if (i > MinLight && i <= MaxLight)
            LUT[i] = (byte)(255 * (i - MinLight) / (MaxLight - MinLight));
          else LUT[i] = 255;

      // Calculating contrasted "ShadIm":
      for (i = 0; i < 256; i++) histo[i] = 0;
      jump = width * height / 50;
      for (i = 0; i < width * height; i++) //====================================
      {
        if (i % jump == jump - 1) progressBar1.PerformStep();

        for (c = 0; c < nbyteIm; c++)
          ShadIm.Grid[c + nbyteIm * i] = LUT[ShadIm.Grid[c + nbyteIm * i]];

        if (nbyteIm == 1) lum = ShadIm.Grid[0 + nbyteIm * i];
        else lum = (byte)MaxC(ShadIm.Grid[2 + nbyteIm * i], 
                    ShadIm.Grid[1 + nbyteIm * i], ShadIm.Grid[0 + nbyteIm * i]);
        histo[lum]++;
      } //========================== end for (i = 0; ... ==============================

      // Displaying the histograms and the row sections:
      //pictureBox3.Enabled = true;
      Graphics g3; // = pictureBox3.CreateGraphics();
      Bitmap BmpPictBox3 = new Bitmap(pictureBox3.Width, pictureBox3.Height);
      g3 = Graphics.FromImage(BmpPictBox3);
      pictureBox3.Image = BmpPictBox3;

      int MaxHisto = 0, SecondMax = 0;
      for (i = 0; i < 256; i++) if (histo[i] > MaxHisto) MaxHisto = histo[i];
      for (i = 0; i < 256; i++) if (histo[i] != MaxHisto && histo[i] > SecondMax) SecondMax = histo[i];
      MaxHisto = SecondMax * 4 / 3;
      Pen redPen = new Pen(Color.Red), yellowPen = new Pen(Color.Yellow),
                      bluePen = new Pen(Color.Blue), greenPen = new Pen(Color.LightGreen);
      SolidBrush whiteBrush = new SolidBrush(Color.White);
      Rectangle Rect = new Rectangle(0, 0, pictureBox3.Width, pictureBox3.Height);
      pictureBox3.Visible = true;

      g3.FillRectangle(whiteBrush, Rect);

      for (i = 0; i < 256; i++)
        g3.DrawLine(redPen, i, pictureBox3.Height - 1 - histo[i] * pictureBox3.Height / MaxHisto, i, pictureBox3.Height - 1);
      for (i = 0; i < 256; i += 50)
        g3.DrawLine(greenPen, i, pictureBox3.Height - 200, i, pictureBox3.Height);

      if (Threshold >= 0) g3.DrawLine(bluePen, Threshold, pictureBox3.Height - 200, Threshold, pictureBox3.Height);
      pictureBox3.Visible = true;
      SHAD = true;
    } //***************************** end CorrectShading **********************************************



    private int ImageToBitmapNew(CImage Image, Bitmap bmp)
    // Any image and color bitmap.
    {
      Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
      BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
      if (bmp.PixelFormat != PixelFormat.Format24bppRgb)
        if (MessReturn("ImageToBitmapNew: we don't use this pixel format=" + bmp.PixelFormat) < 0) return -1;
      IntPtr ptr = bmpData.Scan0;
      int size = bmp.Width * bmp.Height;
      int length = Math.Abs(bmpData.Stride) * bmp.Height;
      byte[] rgbValues = new byte[length];

      int nbyteIm = Image.N_Bits / 8;
      for (int y = 0; y < bmp.Height; y++) //=================================================================
      {
        int jump = bmp.Height / 100;
        if (y % jump == jump - 1) progressBar1.PerformStep();

        for (int x = 0; x < bmp.Width; x++)
        {
          Color color = Color.FromArgb(0, 0, 0); ;
          if (nbyteIm == 3)
          {
            color = Color.FromArgb(Image.Grid[2 + 3 * (x + Image.width * y)],
              Image.Grid[1 + 3 * (x + Image.width * y)], Image.Grid[0 + 3 * (x + Image.width * y)]);
          }
          else
          {
            color = Color.FromArgb(Image.Grid[x + Image.width * y],
              Image.Grid[x + Image.width * y], Image.Grid[x + Image.width * y]);
          }
          rgbValues[3 * x + Math.Abs(bmpData.Stride) * y + 0] = color.B;
          rgbValues[3 * x + Math.Abs(bmpData.Stride) * y + 1] = color.G;
          rgbValues[3 * x + Math.Abs(bmpData.Stride) * y + 2] = color.R;
        } //==================================== end for (int x ... =============================
      }  //===================================== end for (int y ... ===============================
      System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, length);
      bmp.UnlockBits(bmpData);
      return 1;
    } //****************************** end ImageToBitmapNew ****************************************


    private void button2_Click(object sender, EventArgs e) // Shading
    {
      if (!OPEN)
      {
        MessageBox.Show("Please open an image");
        return;
      }

      if (radioButton1.Checked)
      {
        button2.Visible = true;
        label1.Visible = true;
        label2.Visible = true;
        label9.Visible = true;
        numericUpDown1.Visible = true;
        numericUpDown2.Visible = true;
        numericUpDown1.Value = 11;  // Window in per mille
        numericUpDown2.Value = 255; // Light
        numericUpDown4.Value = 50;  // Delete dark
        numericUpDown5.Value = 0;   // Delete light

        DIV = true;
        KIND = 1;
        CHOICE = true;
      }
      else
        if (radioButton2.Checked)
        {
          button2.Visible = true;
          label1.Visible = true;
          label2.Visible = true;
          label9.Visible = true;
          numericUpDown1.Visible = true;
          numericUpDown2.Visible = true;
          numericUpDown1.Value = 11;  // Window in per mille
          numericUpDown2.Value = 140; // Light
          numericUpDown4.Value = 0;  // Delete dark
          numericUpDown5.Value = 70;   // Delete light

          DIV = false;
          KIND = 2;
          CHOICE = true;
        }
        else
          if (radioButton3.Checked)
          {
            button2.Visible = true;
            label1.Visible = true;
            label2.Visible = true;
            label9.Visible = true;
            numericUpDown1.Visible = true;
            numericUpDown2.Visible = true;
            numericUpDown1.Value = 500;  // Window in per mille
            numericUpDown2.Value = 128; // Light
            DIV = true;
            KIND = 3;
            CHOICE = true;
          }
      if (radioButton4.Checked)
      {
        button2.Visible = true;
        label1.Visible = true;
        label2.Visible = true;
        label9.Visible = true;
        numericUpDown1.Visible = true;
        numericUpDown2.Visible = true;
        numericUpDown1.Value = 11;  // Window in per mille
        numericUpDown2.Value = 255; // Light
        DIV = false;
        KIND = 4;
        CHOICE = true;
      }

      if (!CHOICE)
      {
        MessageBox.Show("Please choose a radio button in 'groupBox1'");
        label6.Visible = true;
        groupBox1.Visible = true;
        return;
      }
      else
      {
        label1.Visible = true;
        label2.Visible = true;
        label9.Visible = true;
        button2.Visible = true;
        numericUpDown1.Visible = true;
        numericUpDown2.Visible = true;
      }

      Shading_Cor(KIND, this);
      label8.Visible = true;
      label6.Visible = false;
      label7.Visible = false;
      label9.Visible = false;
      label10.Text = "Shading corrected";
      label10.Visible = true;

      groupBox1.Visible = false;
      progressBar1.Visible = false;
      SHAD = true;
    } //******************** end button 2 Shading *******************************************


    private void Shading_Cor(int KIND, Form1 fm1)  
    {   
        int width=OrigIm.width;
        int hWind = (int)numericUpDown1.Value * width / 200;
        GrayIm.ColorToGrayMC(SigmaIm, fm1);
        progressBar1.Value = 0;
        progressBar1.Step = 1;
        progressBar1.Visible = true;
        progressBar1.Maximum = 100;

        hWind = (int)numericUpDown1.Value * OrigIm.width / 200;
        MeanIm.FastAverageM(GrayIm, hWind, fm1);
        int[] histo = new int[256];

        CorrectShading(DIV);
        ImageToBitmapNew(ShadIm, ShadingBmp);

        pictureBox2.Visible = true;
        pictureBox2.Image = ShadingBmp;
    } //****************************** end Shading_Cor ***********************


    private void button3_Click(object sender, EventArgs e) // Impulse Noise
    {
      if (!OPEN)
      {
        MessageBox.Show("Please open an image");
        return;
      }
      if (!SHAD)
      {
        MessageBox.Show("Please ckick the button 'Shading'");
        return;
      }
      if (!BIN)
      {
        MessageBox.Show("Please click in the histogram");
        return;
      }
      ResultBmp = new Bitmap(origBmp.Width, origBmp.Height, PixelFormat.Format24bppRgb);

      ImpulseIm.Copy(BinIm);
      int nbyte = ImpulseIm.N_Bits / 8;
      Drawn = true;
      progressBar1.Visible = true;
      ImpulseIm.DeleteBit0(nbyte, this);

      int maxLi, minLi;
      int[] histo = new int[256];
      for (int i = 0; i < 256; i++) histo[i] = 0;
      if (nbyte == 3)
      {
        int lum;
        int i1 = origBmp.Width * origBmp.Height / 100 + 1;
        for (int i = 0; i < origBmp.Width * origBmp.Height; i++)
        {
          lum = MaxC(ImpulseIm.Grid[3 * i + 2], ImpulseIm.Grid[3 * i + 1], ImpulseIm.Grid[3 * i + 0]) ;
          histo[lum]++;
        }
      }
      else
        for (int i = 0; i < origBmp.Width * origBmp.Height; i++) histo[ImpulseIm.Grid[i]]++;

      for (maxLi = 255; maxLi > 0; maxLi--) if (histo[maxLi] != 0) break;
      for (minLi = 0; minLi < 256; minLi++) if (histo[minLi] != 0) break;
      CPnoise PN = new CPnoise(histo, 1000, 4000);

      PN.Sort(ImpulseIm, histo, Number, pictureBox1.Width, pictureBox1.Height, this);

      int maxDark = (int)numericUpDown4.Value;
      int maxLight = (int)numericUpDown5.Value;

      PN.DarkNoise(ref ImpulseIm, minLi, maxLi, maxDark, this);
      ImpulseIm.DeleteBit0(nbyte, this);

      PN.LightNoise(ref ImpulseIm, minLi, maxLi, maxLight, this);

      for (int i = 0; i < nbyte * origBmp.Width * origBmp.Height; i++)
        if (ImpulseIm.Grid[i] == 252 || ImpulseIm.Grid[i] == 254) ImpulseIm.Grid[i] = 255;

      ImageToBitmapNew(ImpulseIm, ResultBmp);

      pictureBox2.Image = ResultBmp;

      Graphics g = pictureBox1.CreateGraphics();
      Pen myPen = new System.Drawing.Pen(System.Drawing.Color.LightGray);
      for (int n = 0; n < Number; n += 2)
      {
        g.DrawLine(myPen, v[n + 1].X, v[n + 0].Y, v[n + 1].X, v[n + 1].Y);
        g.DrawLine(myPen, v[n + 0].X, v[n + 0].Y, v[n + 1].X, v[n + 0].Y);
        g.DrawLine(myPen, v[n + 0].X, v[n + 0].Y, v[n + 0].X, v[n + 1].Y);
        g.DrawLine(myPen, v[n + 0].X, v[n + 1].Y, v[n + 1].X, v[n + 1].Y);
      }
      progressBar1.Visible = false;

      groupBox1.Visible = true;
      label6.Visible = true;

      label6.Visible = true;
      label8.Visible = true;
      label9.Visible = true;
      label10.Text = "Noise removed";
      label10.Visible = true;
      button4.Visible = true;

      CHOICE = false;
      pictureBox3.Visible = true;
     } //********************************** end Impulse noise *******************************


    private void pictureBox1_MouseClick(object sender, MouseEventArgs e) // small rectangles in OrigIm
    {
      Graphics g = pictureBox1.CreateGraphics();
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
        MessageBox.Show("Number=" + Number + " is too large");

      myPen = new System.Drawing.Pen(System.Drawing.Color.Blue);
      if ((Number & 1) == 0)
        for (int n = 0; n < Number; n += 2)
        {
          g.DrawLine(myPen, v[n + 1].X, v[n + 0].Y, v[n + 1].X, v[n + 1].Y);
          g.DrawLine(myPen, v[n + 0].X, v[n + 0].Y, v[n + 1].X, v[n + 0].Y);
          g.DrawLine(myPen, v[n + 0].X, v[n + 0].Y, v[n + 0].X, v[n + 1].Y);
          g.DrawLine(myPen, v[n + 0].X, v[n + 1].Y, v[n + 1].X, v[n + 1].Y);
        }
    } //***************************** end pictureBox1_MouseClick ***********************


    private void button4_Click(object sender, EventArgs e) // Save result
    {
      SaveFileDialog dialog = new SaveFileDialog(); //Prompts the user to select a location for saving a file. 
      // This class can either open and overwrite an existing file or create a new file.

      dialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*"; 
      if (dialog.ShowDialog() == DialogResult.OK)
      {
        string tmpFileName;
        if (dialog.FileName == OpenImageFile)
        {
          tmpFileName = OpenImageFile.Insert(OpenImageFile.IndexOf("."), "$$$");
          if (dialog.FileName.Contains(".jpg")) ResultBmp.Save(tmpFileName, ImageFormat.Jpeg); // saving tmpFile
          else 
            if (dialog.FileName.Contains(".bmp")) ResultBmp.Save(tmpFileName, ImageFormat.Bmp);
            else
            {
              MessageBox.Show("The file " + dialog.FileName + " has an inappropriate extension. Returning.");
              return;
            }

          origBmp.Dispose();
          File.Replace(tmpFileName, OpenImageFile, OpenImageFile.Insert(OpenImageFile.IndexOf("."), "BackUp"));
          origBmp = new Bitmap(OpenImageFile);
          pictureBox1.Image = origBmp;
        }
        else
        {
          if (dialog.FileName.Contains(".jpg")) ResultBmp.Save(dialog.FileName, ImageFormat.Jpeg);
          else 
            if (dialog.FileName.Contains(".bmp")) ResultBmp.Save(dialog.FileName, ImageFormat.Bmp);
            else
            {
              MessageBox.Show("The file " + dialog.FileName + " has an inappropriate extension. Returning.");
              return;
            }
        }
        MessageBox.Show("The result image saved under " + dialog.FileName);
      }
    } //**************************** end Save result ***********************


    private void pictureBox3_MouseClick(object sender, MouseEventArgs e) // Thresholding
    {
      if (!OPEN)
      {
        MessageBox.Show("Please open an image");
        return;
      }
      if (!SHAD)
      {
        MessageBox.Show("Please click the button 'Shading'");
        return;
      }

      Threshold = e.X;
      Graphics g3 = pictureBox3.CreateGraphics();
      Pen bluePen = new Pen(Color.Blue);
      g3.DrawLine(bluePen, Threshold, pictureBox3.Height, Threshold, pictureBox3.Height - 200); 
      progressBar1.Visible = true;
      progressBar1.Value = 0;
      int nbyte = ShadIm.N_Bits / 8;
      ResultBmp = new Bitmap(ShadIm.width, ShadIm.height, PixelFormat.Format24bppRgb);
      int jump = height / 100;
      for (int y = 0; y < height; y++)
      {
        if (y % jump == jump - 1) progressBar1.PerformStep();
        for (int x = 0; x < width; x++)
        {
          int i = x + width * y;
          if (nbyte == 1)
          {
            if (ShadIm.Grid[i] > Threshold) BinIm.Grid[i] = 255;
            else BinIm.Grid[i] = 0;
          }
          else
          {
            if (MaxC(ShadIm.Grid[2 + 3 * i], ShadIm.Grid[1 + 3 * i], ShadIm.Grid[0 + 3 * i]) > Threshold) BinIm.Grid[i] = 255;
            else BinIm.Grid[i] = 0;
          }
          ResultBmp.SetPixel(x, y, Color.FromArgb(BinIm.Grid[i], BinIm.Grid[i], BinIm.Grid[i]));
        }
      }
      label7.Visible = true;
      label6.Visible = false;
      groupBox1.Visible = false;
      label8.Visible = false;
      CHOICE = false;
      pictureBox2.Image = ResultBmp;
      Threshold = -1;
      progressBar1.Visible = false;
      label10.Text = "Thresholded image";
      label10.Visible = true;
      label4.Visible = true;
      label5.Visible = true;
      button3.Visible = true;
      numericUpDown4.Visible = true;
      numericUpDown5.Visible = true;

      BIN = true;
    } //**************************** end pict3_MouseClick Thresholding ********************

    private void radioButton1_CheckedChanged(object sender, EventArgs e)
    {
      button2.Visible = true;
      label1.Visible = true;
      label2.Visible = true;
      label9.Visible = true;
      numericUpDown1.Visible = true;
      numericUpDown2.Visible = true;
      numericUpDown1.Value = 11;  // Window in per mille
      numericUpDown2.Value = 255; // Light
      numericUpDown4.Value = 50;  // Delete dark
      numericUpDown5.Value = 0;   // Delete light

    }

    private void radioButton2_CheckedChanged(object sender, EventArgs e)
    {
      button2.Visible = true;
      label1.Visible = true;
      label2.Visible = true;
      label9.Visible = true;
      numericUpDown1.Visible = true;
      numericUpDown2.Visible = true;
      numericUpDown1.Value = 11;  // Window in per mille
      numericUpDown2.Value = 140; // Light
      numericUpDown4.Value = 0;  // Delete dark
      numericUpDown5.Value = 70;   // Delete light

    }

    private void radioButton3_CheckedChanged(object sender, EventArgs e)
    {
      button2.Visible = true;
      label1.Visible = true;
      label2.Visible = true;
      label9.Visible = true;
      numericUpDown1.Visible = true;
      numericUpDown2.Visible = true;
      numericUpDown1.Value = 500;  // Window in per mille
      numericUpDown2.Value = 128; // Light

    }

    private void radioButton4_CheckedChanged(object sender, EventArgs e)
    {
      button2.Visible = true;
      label1.Visible = true;
      label2.Visible = true;
      label9.Visible = true;
      numericUpDown1.Visible = true;
      numericUpDown2.Visible = true;
      numericUpDown1.Value = 11;  // Window in per mille
      numericUpDown2.Value = 255; // Light

    } 

  } //****************************** end class Form1 **************************
} //******************************** end namespace ****************************************


