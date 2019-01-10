using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace WFellipseBikes
{
  public class CQueInd
  {
    public
      int input, output, Len;
    bool full;
    int[] Array;
    public CQueInd(int len) // Constructor
    {
      Len = len;
      input = 0;
      output = 0;
      full = false;
      Array = new int[Len];
    }

    public int Put(int V)
    {
      if (full) return -1;
      Array[input] = V;
      if (input == Len - 1) input = 0;
      else input++;
      return 1;
    }

    public int Get()
    {
      int er = -1;
      if (Empty())
      {
        return er;
      }
      int iV = new int();
      iV = Array[output];
      if (output == Len - 1) output = 0;
      else output++;
      if (full) full = false;
      return iV;
    }

    public bool Empty()
    {
      if (input == output && full == false) return true;
      return false;
    }
  } //***************************** end public class CQueInd **************************


  public class CImage
  {
    public byte[] Grid;
    public
    int width, height, N_Bits, nLoop, denomProg;
    Color[] Palette;

    public CImage(int nx, int ny, int nbits) // constructor
    {
      width = nx;
      height = ny;
      N_Bits = nbits;
      Palette = new Color[256];
      Grid = new byte[width * height * (N_Bits / 8)];
    }

    public CImage(int nx, int ny, int nbits, byte[] img) // constructor
    {
      width = nx;
      height = ny;
      N_Bits = nbits;
      Palette = new Color[256];

      Grid = new byte[width * height * (N_Bits / 8)];
      for (int i = 0; i < width * height * N_Bits / 8; i++) Grid[i] = img[i];
    }




    public int FastAverageUni(CImage Inp, int hWind, Form1 fm1)
    // Filters the color or gray scale image "Inp" and returns the result in
    // 'Grid' of the calling image.
    {
      int c = 0, nByte = 0;
      if (Inp.N_Bits == 8) nByte = 1;
      else nByte = 3;
      width = Inp.width; height = Inp.height; // elements of the class "Cimage"
      Grid = new byte[nByte * width * height];
      int[] nPixColmn;
      nPixColmn = new int[width];
      for (int i = 0; i < width; i++) nPixColmn[i] = 0;
      int[,] SumColmn;
      SumColmn = new int[width, 3];
      int nPixWind = 0, xout = 0, xsub = 0;
      int[] SumWind = new int[3];
      for (int y = 0; y < height + hWind; y++) //==============================
      {
        int yout = y - hWind, ysub = y - 2 * hWind - 1;
        nPixWind = 0;
        for (c = 0; c < nByte; c++) SumWind[c] = 0;
        int y1 = 1 + (height + hWind) / 100;
        for (int x = 0; x < width + hWind; x++) //============================
        {
          xout = x - hWind;
          xsub = x - 2 * hWind - 1;	// 1. and 2. addition
          if (y < height && x < width) // 3. and 4. addition
          {
            for (c = 0; c < nByte; c++)
              SumColmn[x, c] += Inp.Grid[c + nByte * (x + width * y)];
            nPixColmn[x]++;
          }
          if (ysub >= 0 && x < width)
          {
            for (c = 0; c < nByte; c++)
              SumColmn[x, c] -= Inp.Grid[c + nByte * (x + width * ysub)];
            nPixColmn[x]--;
          }
          if (yout >= 0 && x < width)
          {
            for (c = 0; c < nByte; c++) SumWind[c] += SumColmn[x, c];
            nPixWind += nPixColmn[x];
          }
          if (yout >= 0 && xsub >= 0)
          {
            for (c = 0; c < nByte; c++) SumWind[c] -= SumColmn[xsub, c];
            nPixWind -= nPixColmn[xsub];
          }
          if (xout >= 0 && yout >= 0)
            for (c = 0; c < nByte; c++)
              Grid[c + nByte * (xout + width * yout)] = (byte)(SumWind[c] / nPixWind);
        } //============= end for (int x = 0;  =============================
      } //============== end for (int y = 0;  ==============================
      return 1;
    } //***************** end FastAverageUni ********************************


     public int Resize(CImage Image, int Width, int Height, int nByte, int Reduce)
    //ref int width, ref int height)
    {
      int c, x, y, X, Y;
      byte gv;
      for (y = 0; y < height; y++)
      {
        Y = y * Reduce;
        for (x = 0; x < width; x++)
        {
          X = x * Reduce;
          if (nByte == 1)
          {
            gv = Image.Grid[Y * Width + X];
            for (c = 0; c < 3; c++) Grid[c + 3 * (x + width * y)] = gv;
          }
          else
          {
            for (c = 0; c < 3; c++)
            {
              gv = Image.Grid[c + 3 * (X + Width * Y)];
              Grid[c + 3 * (x + width * y)] = gv;
            }
          }
        }
      }
      return 1;
    } //*********************** end Resize ********************************************+++


    public void Copy(CImage image, int full)
    {
      width = image.width;
      height = image.height;
      N_Bits = image.N_Bits;
      Grid = new byte[width * height * (N_Bits / 8)];
      for (int i = 0; i < 256; i++) Palette[i] = image.Palette[i];

      if (full == 1)
        for (int i = 0; i < width * height * (N_Bits / 8); i++)
          Grid[i] = image.Grid[i];
    }


    public void BitmapToImage(Bitmap bmp, Form1 fm1)
    {
      int nbyteBmp, nbyteIm;
      nbyteIm = N_Bits / 8;
      Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
      BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
      switch (bmp.PixelFormat)
      {
        case PixelFormat.Format24bppRgb: nbyteBmp = 3; break;
        case PixelFormat.Format8bppIndexed: nbyteBmp = 1; break;
        default: MessageBox.Show("BitmapToGrid: Inappropriate pixel format=" + bmp.PixelFormat); return;
      }
      IntPtr ptr = bmpData.Scan0;
      int Str = bmpData.Stride;
      int bytes = Math.Abs(bmpData.Stride) * bmp.Height; // es war ohne *3
      byte[] rgbValues = new byte[bytes];
      System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

      fm1.progressBar1.Value = 0;
      fm1.progressBar1.Step = 1;
      fm1.progressBar1.Visible = true;
      int Len = bmp.Height;
      int jump, nStep = 100;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (int y = 0; y < bmp.Height; y++)
      {
        if (y % jump == jump - 1) fm1.progressBar1.PerformStep();

        for (int x = 0; x < bmp.Width; x++)
        {
          if (nbyteBmp == 1)
          {
            Color color = bmp.Palette.Entries[rgbValues[x + Math.Abs(bmpData.Stride) * y]];
            if (nbyteIm == 1)
              Grid[x + bmp.Width * y] = color.R;
            else
            {
              Grid[3 * (x + bmp.Width * y) + 0] = color.B;
              Grid[3 * (x + bmp.Width * y) + 1] = color.G;
              Grid[3 * (x + bmp.Width * y) + 2] = color.R;
            }
          }
          else // nbyteBmp == 3
          {
            if (nbyteIm == 1)
              Grid[x + bmp.Width * y] = rgbValues[2 + nbyteBmp * x + Math.Abs(bmpData.Stride) * y];
            else // nbyteIm == 3
              for (int c = 0; c < nbyteBmp; c++)
                Grid[c + nbyteIm * (x + bmp.Width * y)] =
                  rgbValues[c + nbyteBmp * x + Math.Abs(bmpData.Stride) * y];
          }
        }
      }
      bmp.UnlockBits(bmpData);
    } //****************************** end BitmapToImage ****************************************


    public byte MaxC(byte R, byte G, byte B)
    {
      byte light = G;
      if (0.713 * R > G) light = (byte)(0.713 * R);
      if (0.527 * B > G) light = (byte)(0.527 * B);
      return light;
    }


    public void BitmapToImageOld(Bitmap bmp, Form1 fm1)
    {
      int nbyteIm = N_Bits / 8;
      if (bmp.PixelFormat != PixelFormat.Format24bppRgb &&
            bmp.PixelFormat != PixelFormat.Format8bppIndexed)
      {
        MessageBox.Show("BitmapToGridOld: Not suitable pixel format=" + bmp.PixelFormat);
        return;
      }

      Color color;
      fm1.progressBar1.Value = 0;
      fm1.progressBar1.Step = 1;
      fm1.progressBar1.Visible = true;
      int jump, Len = bmp.Height, nStep = 100;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (int y = 0; y < bmp.Height; y++)
      {
        if ((y % jump) == jump - 1) fm1.progressBar1.PerformStep();

        for (int x = 0; x < bmp.Width; x++)
        {
          int i = x + width * y;
          color = bmp.GetPixel(x, y);
          if (nbyteIm == 1)
            Grid[i] = MaxC(color.R, color.G, color.B);
          else // nbyteIm == 3
            for (int c = 0; c < nbyteIm; c++)
            {
              if (c == 0) Grid[nbyteIm * i] = color.B;
              if (c == 1) Grid[nbyteIm * i + 1] = color.G;
              if (c == 2) Grid[nbyteIm * i + 2] = color.R;
            }
        }
      }
    } //****************************** end BitmapToImageOld ****************************************


    public void ImageToBitmap(Bitmap bmp, Form1 fm1)
    {
      int nbyteBmp = 0, nbyteIm = N_Bits / 8;
      Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
      BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
      switch (bmp.PixelFormat)
      {
        case PixelFormat.Format24bppRgb: nbyteBmp = 3; break;
        case PixelFormat.Format8bppIndexed: nbyteBmp = 1; break;
        //default: MessageBox.Show("ImageToBitmap: Not suitable pixel format=" + bmp.PixelFormat); return;
      }
      IntPtr ptr = bmpData.Scan0;
      int size = bmp.Width * bmp.Height;
      int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
      byte[] rgbValues = new byte[bytes];
      byte light = 0; //[] Color = new byte[4];
      Color color;
      int index = 0;
      fm1.progressBar1.Visible = true;
      int Len = bmp.Height;
      int jump, nStep = 20;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (int y = 0; y < bmp.Height; y++)
      {
        if (y % jump == jump - 1) fm1.progressBar1.PerformStep();
        for (int x = 0; x < bmp.Width; x++)
        {
          if (nbyteBmp == 1)  // nbyte of bmp;
          {
            if (nbyteIm == 1) // nbyte == 1; nbyteIm == 1;
            {
              color = (Color)Palette[Grid[x + bmp.Width * y]];
              rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 0] = color.B;
              rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 1] = color.G;
              rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 2] = color.R;
            }
            else // nbyteBmp == 1; nbyteIm == 3
            {
              color = bmp.Palette.Entries[Grid[3 * (x + bmp.Width * y)]];
              rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 0] = color.B;
              rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 1] = color.G;
              rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 2] = color.R;
            }
          }
          else  // nbyteBmp == 3
          {
            if (nbyteIm == 1)
            {
              light = Grid[x + width * y];
              {
                index = 3 * x + Math.Abs(bmpData.Stride) * y; // +c;
                rgbValues[index + 0] = light; // color.B; // Color[c];
                rgbValues[index + 1] = light; // color.G;
                rgbValues[index + 2] = light; // color.R; 
              }
            }
            else //nbyteIm ==3
            {
              for (int c = 0; c < nbyteBmp; c++)
              {
                rgbValues[c + nbyteBmp * x + Math.Abs(bmpData.Stride) * y] =
                                  Grid[c + nbyteIm * (x + bmp.Width * y)];
              }
            } //------------------ end if (nbyteIm ==1) ------------------------
          } //-------------------- end if (nbyte == 1) ----------------------------
        }
      }
      System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
      bmp.UnlockBits(bmpData);

    } //****************************** end ImageToBitmap ****************************************


    public void ImageToBitmapOld(Bitmap bmp, Form1 fm1)
    {
      int nbyteBmp, nbyteIm = N_Bits / 8;
      int light = 0;
      switch (bmp.PixelFormat)
      {
        case PixelFormat.Format24bppRgb: nbyteBmp = 3; break;
        case PixelFormat.Format8bppIndexed: nbyteBmp = 1; break;
        default: MessageBox.Show("ImageToBitmap: Not suitable  pixel format=" + bmp.PixelFormat); return;
      }

      Color color;
      int Len = bmp.Height;
      int jump, nStep = 20;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (int y = 0; y < bmp.Height; y++)
      {
        if ((y % jump) == jump - 1) fm1.progressBar1.PerformStep();
        for (int x = 0; x < bmp.Width; x++)
        {
          if (nbyteBmp == 1)  // nbyte of bmp;
          {
            if (nbyteIm == 1) // nbyte == 1; nbyteIm == 1;
            {
              light = (int)Grid[x + bmp.Width * y];
              bmp.SetPixel(x, y, Color.FromArgb(light, light, light));
            }
            else // nbyte == 1; nbyteIm == 3
            {
              bmp.SetPixel(x, y, Color.FromArgb(Grid[nbyteIm * (x + width * y) + 2],
                  Grid[nbyteIm * (x + width * y) + 1], Grid[nbyteIm * (x + width * y) + 0]));
            }
          }
          else  // nbyte == 3
          {
            if (nbyteIm == 1)
            {
              light = Grid[x + bmp.Width * y];
              color = Color.FromArgb(light, light, light);
              bmp.SetPixel(x, y, Color.FromArgb(light, light, light));
            }
            else //nbyteIm ==3; nbyte == 3
            {
              bmp.SetPixel(x, y, Color.FromArgb(Grid[nbyteIm * (x + width * y) + 2],
                  Grid[nbyteIm * (x + width * y) + 1], Grid[nbyteIm * (x + width * y) + 0]));
            } //------------------ end if (nbyteIm ==1) ------------------------
          } //-------------------- end if (nbyte == 1) ----------------------------
        }  //===================== end for (int x ... ================================
      } //======================== end for (int y ... ================================

    } //****************************** end ImageToBitmapOld ****************************************

    public int MessReturn(string s)
    {
      if (MessageBox.Show(s, "Return", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
        return -1;
      return 1;
    }


    public int SigmaSimpleColor(CImage Inp, int hWind, int Toleranz)
    {	// The sigma filter for 3 byte color images. No often repeated multiplications.
      int[] gvMin = new int[3], gvMax = new int[3], nPixel = new int[3], Sum = new int[3];
      int c, hWind3 = hWind * 3, NX3 = width * 3, x3, xyOut;
      N_Bits = Inp.N_Bits;
      for (int y = xyOut = 0; y < height; y++) // ==================================================
      {
        int y1, yStart = Math.Max(y - hWind, 0) * NX3, yEnd = Math.Min(y + hWind, height - 1) * NX3;
        for (int x = x3 = 0; x < width; x++, x3 += 3, xyOut += 3) //====================================
        {
          int x1, xStart = Math.Max(x3 - hWind3, 0), xEnd = Math.Min(x3 + hWind3, NX3 - 3);
          for (c = 0; c < 3; c++)
          {
            Sum[c] = 0; nPixel[c] = 0;
            gvMin[c] = Math.Max(0, Inp.Grid[c + xyOut] - Toleranz);
            gvMax[c] = Math.Min(255, Inp.Grid[c + xyOut] + Toleranz);
          }
          for (y1 = yStart; y1 <= yEnd; y1 += NX3)
            for (x1 = xStart; x1 <= xEnd; x1 += 3)
              for (c = 0; c < 3; c++)
              {
                if (Inp.Grid[c + x1 + y1] >= gvMin[c] && Inp.Grid[c + x1 + y1] <= gvMax[c])
                {
                  Sum[c] += Inp.Grid[c + x1 + y1];
                  nPixel[c]++;
                }
              }
          for (c = 0; c < 3; c++) //================================================
          {
            if (nPixel[c] > 0) Grid[c + xyOut] = (byte)((Sum[c] + nPixel[c] / 2) / nPixel[c]);
            else Grid[c + xyOut] = 0;
          } //================ end for (c... ===================================
        } //================== end for (int x... =================================
      } //==================== end for (int y... ===================================
      return 1;
    } //********************** end SigmaSimpleColor **********************************

    int ColorDifAbs(byte[] Colp, byte[] Colh)
    // Returns the sum of the absolut differences of the color components.
    {
      int Dif = 0;
      for (int c = 0; c < 3; c++) Dif += Math.Abs(Colp[c] - Colh[c]);
      return Dif;
    }

    int ColorDif(byte[] Colh, byte[] Colp)
    // Returns the sum of the differences of the color components.
    {
      int Dif = 0;
      for (int c = 0; c < 3; c++) Dif += (Colh[c] - Colp[c]);
      return Dif;
    }



    public int SigmaNewM(CImage Inp, int hWind, int Toleranz, Form1 fm1)
    // Sigma filter with doubled calculation of the output values for gray value images.
    {
      N_Bits = 8; width = Inp.width; height = Inp.height;
      int gv, y1, yEnd, yStart;
      Grid = new byte[width * height * N_Bits / 8];
      int[] hist = new int[256];
      fm1.progressBar1.Value = 0;
      fm1.progressBar1.Step = 1;
      fm1.progressBar1.Visible = true;
      int Len = height;
      int jump, nStep = 20;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (int y = 0; y < height; y++) // =======================================
      {
        yStart = Math.Max(y - hWind, 0);
        yEnd = Math.Min(y + hWind, height - 1);
        if ((y % jump) == jump - 1) fm1.progressBar1.PerformStep();
        for (int x = 0; x < width; x++) //====================================
        {
          if (x == 0) //-----------------------------------------------
          {
            for (gv = 0; gv < 256; gv++) hist[gv] = 0;
            for (y1 = yStart; y1 <= yEnd; y1++)
              for (int xx = 0; xx <= hWind; xx++) hist[Inp.Grid[xx + y1 * width]]++;
          }
          else
          {
            int x1 = x + hWind, x2 = x - hWind - 1;
            if (x1 < width)
              for (y1 = yStart; y1 <= yEnd; y1++) hist[Inp.Grid[x1 + y1 * width]]++;
            if (x2 >= 0)
              for (y1 = yStart; y1 <= yEnd; y1++)
              {
                hist[Inp.Grid[x2 + y1 * width]]--;
                if (hist[Inp.Grid[x2 + y1 * width]] < 0) return -1;
              }
          } //---------------- end if (x==0) ------------------------
          int Sum = 0, nPixel = 0, prov;
          int gvMin = Math.Max(0, Inp.Grid[x + width * y] - Toleranz),
              gvMax = Math.Min(255, Inp.Grid[x + width * y] + Toleranz);
          for (gv = gvMin; gv <= gvMax; gv++) // Erste Schleife durch das Histogramm
          {
            Sum += gv * hist[gv]; nPixel += hist[gv];
          }
          if (nPixel > 0) prov = (Sum + nPixel / 2) / nPixel;
          else prov = Inp.Grid[x + width * y];  // "prov" ist der provisorische Mittelwert
          // New:		
          Sum = nPixel = 0;
          gvMin = Math.Max(0, prov - Toleranz);
          gvMax = Math.Min(255, prov + Toleranz);
          for (gv = gvMin; gv <= gvMax; gv++) // Zweite Schleife durch das Histogramm
          {
            Sum += gv * hist[gv]; nPixel += hist[gv];
          }
          if (nPixel > 0) Grid[x + width * y] = (byte)((Sum + nPixel / 2) / nPixel);
          else Grid[x + width * y] = Inp.Grid[x + width * y];
        } //================== end for (int x... ======================
      } //==================== end for (int y... ========================
      return 1;
    } //********************** end SigmaNewM **********************************


    public int SigmaColor(CImage Inp, int hWind, int Toleranz, Form1 fm1)
    {	// The sigma filter for color images with 3 bytes per pixel.
      int gv, y1, yEnd, yStart;
      int[] gvMin = new int[3], gvMax = new int[3], nPixel = new int[3], Sum = new int[3];
      int[][] hist = new int[3][];
      for (int i = 0; i < 3; i++) hist[i] = new int[256];
      int c;
      N_Bits = Inp.N_Bits;
      fm1.progressBar1.Value = 0;
      fm1.progressBar1.Visible = true;
      fm1.progressBar1.Step = 1;
      int Len = height;
      int jump, nStep = 20;
      if (Len > 2*nStep) jump = Len / nStep;
      else jump = 2;
      for (int y = 0; y < height; y++) // =======================================
      {
        if ((y % jump) == jump - 1) fm1.progressBar1.PerformStep(); 
        yStart = Math.Max(y - hWind, 0);
        yEnd = Math.Min(y + hWind, height - 1);
        for (int x = 0; x < width; x++) //====================================
        {
          for (c = 0; c < 3; c++)
          {
            if (x == 0) //-----------------------------------------------
            {
              for (gv = 0; gv < 256; gv++) hist[c][gv] = 0;
              for (y1 = yStart; y1 <= yEnd; y1++)
                for (int xx = 0; xx <= hWind; xx++) hist[c][Inp.Grid[c + 3 * xx + y1 * width * 3]]++;
            }
            else
            {
              int x1 = x + hWind, x2 = x - hWind - 1;
              if (x1 < width - 1)
                for (y1 = yStart; y1 <= yEnd; y1++) hist[c][Inp.Grid[c + 3 * x1 + y1 * width * 3]]++;
              if (x2 >= 0)
                for (y1 = yStart; y1 <= yEnd; y1++)
                {
                  hist[c][Inp.Grid[c + 3 * x2 + y1 * width * 3]]--;
                  if (hist[c][Inp.Grid[c + 3 * x2 + y1 * width * 3]] < 0) return -1;
                }
            } //---------------- end if (x==0) ------------------------

            Sum[c] = 0; nPixel[c] = 0;
            gvMin[c] = Math.Max(0, Inp.Grid[c + 3 * x + 3 * width * y] - Toleranz);
            gvMax[c] = Math.Min(255, Inp.Grid[c + 3 * x + 3 * width * y] + Toleranz);
            for (gv = gvMin[c]; gv <= gvMax[c]; gv++)
            {
              Sum[c] += gv * hist[c][gv]; nPixel[c] += hist[c][gv];
            }
            if (nPixel[c] > 0) Grid[c + 3 * x + 3 * width * y] = (byte)((Sum[c] + nPixel[c] / 2) / nPixel[c]);
            else Grid[c + 3 * x + 3 * width * y] = Inp.Grid[c + 3 * x + 3 * width * y];
          } //================ end for (c... ========================
        } //================== end for (int x... ======================
      } //==================== end for (int y... ========================
      return 1;
    } //********************** end SigmaColor **********************************


    public int ExtremVar(CImage Inp, int hWind, Form1 fm1)
    // Extrem filter for gray value images with variable window size of 2*hWind+1.
    {
      N_Bits = 8; width = Inp.width; height = Inp.height;
      int gv, y1, yEnd, yStart;
      Grid = new byte[width * height];
      int[] hist = new int[256];
      int Len = height;
      int jump, nStep = 20;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (int y = 0; y < height; y++) // =======================================
      {
        if ((y % jump) == jump -1) fm1.progressBar1.PerformStep();
        yStart = Math.Max(y - hWind, 0);
        yEnd = Math.Min(y + hWind, height - 1);

        for (int x = 0; x < width; x++) //====================================
        {
          if (x == 0) //-----------------------------------------------
          {
            for (gv = 0; gv < 256; gv++) hist[gv] = 0;
            for (y1 = yStart; y1 <= yEnd; y1++)
              for (int xx = 0; xx <= hWind; xx++) hist[Inp.Grid[xx + y1 * width]]++;
          }
          else
          {
            int x1 = x + hWind, x2 = x - hWind - 1;
            if (x1 < width)
              for (y1 = yStart; y1 <= yEnd; y1++) hist[Inp.Grid[x1 + y1 * width]]++;
            if (x2 >= 0)
              for (y1 = yStart; y1 <= yEnd; y1++)
              {
                hist[Inp.Grid[x2 + y1 * width]]--;
                if (hist[Inp.Grid[x2 + y1 * width]] < 0) return -1;
              }

          } //---------------- end if (x==0) ------------------------
          int gvMin = 0, gvMax = 255;
          for (gv = gvMin; gv <= gvMax; gv++)
            if (hist[gv] > 0) { gvMin = gv; break; }
          for (gv = gvMax; gv >= 0; gv--)
            if (hist[gv] > 0) { gvMax = gv; break; }
          if (Inp.Grid[x + width * y] - gvMin < gvMax - Inp.Grid[x + width * y]) Grid[x + width * y] = (byte)gvMin;
          else Grid[x + width * y] = (byte)gvMax;
        } //================== end for (int x... ======================
      } //==================== end for (int y... ========================
      return 1;
    } //********************** end ExtrmVar **********************************


    public int ExtremLightColor(CImage Inp, int hWind, int th, Form1 fm1)
    {	/* The extreme filter for 3 byte color images with variable hWind.
	    The filter finds in the (2*hWind+1)-neighbourhood of the actual pixel (x,y) the color "Color1" with 
      minimum and the color "Color2" with thge maximum lightness. "Color1" is assigned to the output pixel
      if its lightniss is closer to the lightness of the cetral pixel than the lightnesas of "Color2". --*/

      byte[] CenterColor = new byte[3], Color = new byte[3], Color1 = new byte[3], Color2 = new byte[3];
      int c, k, x; 
      int Len = height;
      int jump, nStep = 20;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (int y = 0; y < height; y++) // =======================================
      {
        if ((y % jump) == jump - 1) fm1.progressBar1.PerformStep();
        for (x = 0; x < width; x++) //==============================================
        {
          for (c = 0; c < 3; c++) Color2[c] = Color1[c] = Color[c] = CenterColor[c] = Inp.Grid[c + 3 * x + y * width * 3];

          int MinLight = 1000, MaxLight = 0;
          for (k = -hWind; k <= hWind; k++) //=========================================
          {
            if (y + k >= 0 && y + k < height)
              for (int i = -hWind; i <= hWind; i++) //=====================================
              {
                if (x + i >= 0 && x + i < width) // && (i > 0 || k > 0))
                {
                  for (c = 0; c < 3; c++) Color[c] = Inp.Grid[c + 3 * (x + i) + 3 * (y + k) * width];
                  int light = MaxC(Color[2], Color[1], Color[0]);

                  if (light < MinLight)
                  {
                    MinLight = light;
                    for (c = 0; c < 3; c++) Color1[c] = Color[c];
                  }
                  if (light > MaxLight)
                  {
                    MaxLight = light;
                    for (c = 0; c < 3; c++) Color2[c] = Color[c];
                  }
                }
              } //=============== end for (int i... ============================
          } //=================== end for (int k... ==============================

          int CenterLight = MaxC(CenterColor[2], CenterColor[1], CenterColor[0]);
          int dist1 = 0, dist2 = 0;
          dist1 = CenterLight - MinLight;
          dist2 = MaxLight - CenterLight;
          if (dist2 - dist1 > 0)
            for (c = 0; c < 3; c++)
              Grid[c + 3 * x + y * width * 3] = Color1[c]; // Min
          else
             for (c = 0; c < 3; c++)
              Grid[c + 3 * x + y * width * 3] = Color2[c]; // Max

        } //================== end for (int x... ===================================
      } //==================== end for (int y... =====================================
      return 1;
    } //********************** end ExtremLightColor **************************************



    public int ExtremNew(CImage Inp, int hWind, Form1 fm1)
    // Extrem filter for gray value and color images with variable window size of 2*hWind+1.
    {
      int xEnd, xStart, yEnd, yStart;
      bool COLOR;
      if (Inp.N_Bits == 24) COLOR = true;
      else COLOR = false;
      byte[] Gray = null;
      if (COLOR)
      {
        Gray = new byte[width * height];
        for (int i = 0; i < width * height; i++)
          Gray[i] = MaxC(Inp.Grid[2 + 3 * i], Inp.Grid[1 + 3 * i], Inp.Grid[0 + 3 * i]);
      }

      int Len = height;
      int jump, nStep = 20;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      int index, maxLum = 0;
      byte[] color = new byte[3];
      for (int y = 0; y < height; y++) // ====================================================
      {
        if ((y % jump) == jump - 1) fm1.progressBar1.PerformStep();
        yStart = Math.Max(y - hWind, 0);
        yEnd = Math.Min(y + hWind, height - 1);
        for (int x = 0; x < width; x++) //================================================
        {
          index = x + width * y;
          xStart = Math.Max(x - hWind, 0);
          xEnd = Math.Min(x + hWind, width - 1);
          maxLum = 0;
          for (int y1 = yStart; y1 <= yEnd; y1++)
          {
            for (int x1 = xStart; x1 <= xEnd; x1++) //===================================
            {
              if (COLOR && Gray[x1 + y1 * width] > maxLum)
              {
                maxLum = Gray[x1 + y1 * width];
                for (int c = 0; c < 3; c++) color[c] =
                  Inp.Grid[c + 3 * (x1 + y1 * width)];
              }
              if (!COLOR && Inp.Grid[x1 + y1 * width] > maxLum)
              {
                maxLum = Inp.Grid[x1 + y1 * width];
              }
            } //========================= end for (int x1 ... =============================
          } //=========================== end for (int x1 ... =============================
          if (COLOR)
          {
            for (int c = 0; c < 3; c++)
              Grid[c + 3 * (x + width * y)] = color[c];
            //MessageBox.Show("ExtremNew: x=" + x + " y=" + y + " color[2]=" + color[2]);
          }
          else Grid[x + width * y] = (byte)maxLum;
        } //================== end for (int x... ======================
      } //==================== end for (int y... ========================
      return 1;
    } //********************** end ExtrmNew **********************************


    public int ExtremVarColor(CImage Inp, int hWind, Form1 fm1)
    {	/* The extreme filter for 3 byte color images with variable hWind.
	    The filter finds in the (2*hWind+1)-neighbourhood of the actual pixel (x,y) the color "Color1" which has the greatest 
	    difference form the central color of (x, y). Then it finds a Color2 which has the greatest differnce from
	    Color1. Color1 is assigned to the output pixel if its difference to the cental color is smaller than that
	    of Color2. Otherwise Color 2 is assigned. --*/
      int[] CenterColor = new int[3], Color = new int[3], Color1 = new int[3], Color2 = new int[3];

      int c, k, x; 

      fm1.progressBar1.Visible = true;
      int Len = height;
      int jump, nStep = 20;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (int y = 0; y < height; y++) // =======================================
      {
        if ((y % jump) == jump - 1) fm1.progressBar1.PerformStep();
        for (x = 0; x < width; x++) //==============================================
        {
          for (c = 0; c < 3; c++) Color2[c] = Color1[c] = Color[c] = CenterColor[c] = Inp.Grid[c + 3 * x + y * width * 3];
          int MaxDist = -1;
          for (k = -hWind; k <= hWind; k++) //=========================================
          {
            if (y + k >= 0 && y + k < height)
              for (int i = -hWind; i <= hWind; i++) //=====================================
              {
                if (x + i >= 0 && x + i < width)
                {
                  int dist = 0;
                  for (c = 0; c < 3; c++)
                  {
                    Color[c] = Inp.Grid[c + 3 * (x + i) + (y + k) * width * 3];
                    dist += (Color[c] - CenterColor[c]) * (Color[c] - CenterColor[c]);
                  }

                  if (dist > MaxDist)
                  {
                    MaxDist = dist;
                    for (c = 0; c < 3; c++) Color1[c] = Color[c];
                  }
                }
              } //=============== end for (int i... ============================
          } //================ end for (int k... ==============================
          MaxDist = -1;
          for (k = -hWind; k <= hWind; k++) //=========================================
          {
            if (y + k >= 0 && y + k < height)
              for (int i = -hWind; i <= hWind; i++) //=====================================
                if (x + i >= 0 && x + i < width)
                {
                  int dist = 0;
                  for (c = 0; c < 3; c++)
                  {
                    Color[c] = Inp.Grid[c + 3 * (x + i) + (y + k) * width * 3];
                    dist += (Color[c] - Color1[c]) * (Color[c] - Color1[c]);
                  }
                  if (dist > MaxDist)
                  {
                    MaxDist = dist;
                    for (c = 0; c < 3; c++) Color2[c] = Color[c];
                  }
                }
            //=============== end for (int i... ============================
          } //================ end for (int k... ==============================
          int dist1 = 0, dist2 = 0;
          for (c = 0; c < 3; c++)
          {
            dist1 += (Color1[c] - CenterColor[c]) * (Color1[c] - CenterColor[c]);
            dist2 += (Color2[c] - CenterColor[c]) * (Color2[c] - CenterColor[c]);
          }
          if (dist1 < dist2) for (c = 0; c < 3; c++) Grid[c + 3 * x + y * width * 3] = (byte)Color1[c];
          else for (c = 0; c < 3; c++) Grid[c + 3 * x + y * width * 3] = (byte)Color2[c];
        } //================== end for (int x... ================================
      } //==================== end for (int y... ==================================
      return 1;
    } //********************** end ExtremVarColor *************************************

    public void CracksToPixel(CImage Comb)
    {
      int NX = Comb.width;
      int NY = Comb.height;
      int nx = NX / 2;
      int ny = NY / 2;
      int dim; //, valG, val2, valComb;
      for (int i = 0; i < nx * ny; i++) Grid[i] = 0;
      for (int y = 0; y < NY - 2; y++)
        for (int x = 0; x < NX - 2; x++)
        {
          dim = (x % 2) + (y % 2);
          if (dim == 1 && Comb.Grid[x + NX * y] > 0) Grid[x / 2 + nx * (y / 2)] = 255;
        }
    } //*************************** end CracksToPixel **************************************


    public int Trace(byte[] GridCopy, int P, int[] Index, ref int SumCells, ref int Pterm, ref int dir, int Size)
    /* Traces a branch of a component, saves all cells in "Index"; "SumCells" is the number of saved cells.
     * If "SumCells" becomes greater than "Size", the tracing is interrupted, and the method return -1.
     * Otherwise it return a positive number.
     * --*/
    {
      int Lab, Mask = 7, rv = 0;
      bool BP = false, END = false;
      bool atSt_P = false; 
      int Crack, StartLine; 
      int[] Step = { 1, width, -1, -width };

      int iCrack = 0;
      StartLine = P;
      int[] Shift = { 0, 2, 4, 6 };

      if (SumCells < Size) Index[SumCells] = P;

      while (true) //====================================================================
      {
        Crack = P + Step[dir];
        if (SumCells < Size) Index[SumCells] = Crack;
        SumCells++;
        if (GridCopy[Crack] == 0)
        {
          int cellY = Crack / width;
          int cellX = Crack - cellY * width;
          int cellY1 = StartLine / width;
          int cellX1 = StartLine - cellY1 * width;
          int cellY2 = P / width;
          int cellX2 = P - cellY1 * width;
           Pterm = P;
          if ((GridCopy[P] & Mask) == 0) return 1;
        }

        P = Crack + Step[dir];
        if (SumCells < Size) Index[SumCells] = P;

        Lab = GridCopy[P] & Mask;
        if (Lab == 2)
        {
          SumCells++;
        }

        switch (Lab)
        {
          case 0: END = true; BP = false; rv = 1; break;
          case 1: END = true; BP = false; rv = 1; break;
          case 2: BP = END = false; break;
          case 3: BP = true; END = false; rv = 3; break;
          case 4: BP = true; END = false; rv = 3; break;
        }
        if (Lab == 2)  GridCopy[P] = 0;

        iCrack++;
        atSt_P = (P == StartLine);
        if (atSt_P)
        {
          Pterm = P; // Pterm is a parameter of TraceApp
          rv = 2;
          break;
        }

        if (!atSt_P && (BP || END))
        {
          Pterm = P; // Pterm is a parameter of Trace
          if (BP) rv = 3;
          else rv = 1;
          break;
        }

        if (!BP && !END) //---------------------------
        {
          Crack = P + Step[(dir + 1) % 4];
          if (GridCopy[Crack] == 1)
          {
            dir = (dir + 1) % 4;
          }
          else
          {
            Crack = P + Step[(dir + 3) % 4];
            if (GridCopy[Crack] == 1) dir = (dir + 3) % 4;
          }
        }
        else break;
      } //======================================= end while ============================================
      return rv;
    } //***************************************** end Trace ***********************************************



    public int ComponClean(byte[] GridCopy, int X, int Y, int[] Index, int Size) // member of CImage
    /* Traces a component starting at (X, Y), saves coordinates of all cells in "Index", "SumCells" is the
       number of traced cells. If "SumCells" becomes greater than "Size", the tracing is interrupted and the
       method returns -1. Otherwize it returns 1.
       --*/
    {
      int dir, dirT;
      int LabNext, Mask = 7, rv;
      int Crack, P, Pinp, Pnext, Pterm = -1; // Indices in "Comb"
      int[] Step = { 1, width, -1, -width };
      CQueInd pQ = new CQueInd(1000); // necessary to find connected components

      Pinp = X + width * Y;
      int SumCells = 0; // Number of cells in the component
      pQ.Put(Pinp);
      while (!pQ.Empty()) //===========================================================================
      {
        P = pQ.Get();
        if (SumCells < Size) Index[SumCells] = P;
        SumCells++;

        if ((GridCopy[P] & 128) != 0) continue;
        // Hier is the label 128 of P equal to zero:
        for (dir = 0; dir < 4; dir++) //================================================================
        {
          Crack = P + Step[dir];
          if (Crack < 0 || Crack > height * width - 1) continue;
          if (GridCopy[Crack] == 1) //---- ------------ -----------
          {
            Pnext = Crack + Step[dir];
            LabNext = GridCopy[Pnext] & Mask; // changed on Nov. 1 2015
             if (LabNext == 3 || LabNext == 4) pQ.Put(Pnext);
            if (LabNext == 2) //-------------------------------------------------------------------------
            {
              dirT = dir;
              rv = Trace(GridCopy, P, Index, ref SumCells, ref Pterm, ref dirT, Size);

              if ((GridCopy[Pterm] & Mask) == 1)
              {
                if (SumCells < Size) Index[SumCells] = Pterm;
                SumCells++;
                GridCopy[Pterm] = 0;
              }
              if ((GridCopy[Pterm] & 128) == 0 && rv >= 3) pQ.Put(Pterm);
            } // ------------- end if (LabNest==2) -----------------------------------------------------
            else
            {
              if (SumCells < Size)  Index[SumCells] = Crack;
             
              SumCells++;
              if (SumCells < Size)
              {
                Index[SumCells] = Pnext;
                 SumCells++;
              }

              GridCopy[Pnext] = 0;
              GridCopy[P] = 0;
              
              if ((GridCopy[Pnext] & Mask) >= 3 && (GridCopy[Pnext] & 128) == 0) SumCells++;
           }
            if ((GridCopy[P] & Mask) == 1)
            {
              GridCopy[P] = 0;
              break; // The only crack with Lab == 1 alredy processed
            }

          } //--------------- end if (GridCopy[Crack  == 1) ------------------------------------------
        } //================================== end for (dir ... ==========================================
        GridCopy[P] |= 128;
      } //==================================== end while ==========================================
      return SumCells;
    } //************************************** end ComponClean ************************************************


    public int CleanCombNew(int Size, Form1 fm1)
    // Delets one crack line at a brunch point and changes the label of this point to two.
    // Delets components containing less than "Size" cells.
    {
      byte Mask = 7;
      int cntSingles, cntCracks, x1, y1;
      int[] Index = new int[Size + 20];
      int SumCells = 0, Lab = 0, rv = 0;

      // Transforming small squares to corners:
      for (int y = 0; y < height; y += 2) //===================================================================
      {
        for (int x = 0; x < width; x += 2) //===================== over points ==============================
        {
          Lab = Grid[x + width * y]; // &Mask;
          if (y < height - 3 && Grid[x + width * y] >= 3 && Grid[x - 2 + width * (y + 2)] >= 3 &&
               Grid[x - 1 + width * y] == 1 && Grid[x - 2 + width * (y + 1)] == 1 &&
               Grid[x + width * (y + 1)] == 1 && Grid[x - 1 + width * (y + 2)] == 1)
          {
            Grid[x + width * y] -= 1; // main point
            Grid[x + width * (y + 1)] = 0; // crack below
            Grid[x + width * (y + 2)] -= 2; // point below
            Grid[x - 1 + width * (y + 2)] = 0; // crack left below
            Grid[x - 2 + width * (y + 2)] -= 1; // second point
          }

          if (y < height - 3 && Grid[x + width * y] >= 3 && Grid[x + 2 + width * (y + 2)] >= 3 &&
               Grid[x + 1 + width * y] == 1 && Grid[x + 2 + width * (y + 1)] == 1 &&
               Grid[x + width * (y + 1)] == 1 && Grid[x + 1 + width * (y + 2)] == 1)
          {
            Grid[x + width * y] -= 1; // main point
            Grid[x + width * (y + 1)] = 0; // crack below
            Grid[x + width * (y + 2)] -= 2; // point below
            Grid[x + 1 + width * (y + 2)] = 0; // crack right below
            Grid[x + 2 + width * (y + 2)] -= 1; // second point
          }

          // Counting cracks and single cracks incident to a branch point. 
          // Deleting single cracks and changing the labels of the brunch points.
          if (Grid[x + width * y] > 2) //---------------------------------------
          {
            cntSingles = cntCracks = 0;
            for (int dir = 0; dir < 4; dir++) //========== over four cracks ============
            {
              switch (dir) //::::::::::::::::::::::::::::::::::::::::::::
              {
                case 0: x1 = x + 1; y1 = y;  // crack to the right
                  if (Grid[x1 + width * y1] == 1) // crack
                  {
                    cntCracks++;
                    if (Grid[x1 + 1 + width * y1] == 1) // end of crack
                    {
                      Grid[x1 + width * y1] = 0;
                      Grid[x1 + 1 + width * y1] = 0;
                      cntSingles++;
                    }
                  }
                  break;

                case 1: x1 = x; y1 = y + 1;
                   if (x1 < width && y1 < height && Grid[x1 + width * y1] == 1)
                  {
                    cntCracks++;
                    if (Grid[x1 + width * (y1 + 1)] == 1)
                    {
                      Grid[x1 + width * y1] = 0;
                      Grid[x1 + width * (y1 + 1)] = 0;
                      cntSingles++;
                    }
                  }
                  break;

                case 2: x1 = x - 1; y1 = y;
                  if (Grid[x1 + width * y1] == 1)
                  {
                    cntCracks++;
                    if (Grid[x1 - 1 + width * y1] == 1)
                    {
                      Grid[x1 + width * y1] = 0;
                      Grid[x1 - 1 + width * y1] = 0;
                      cntSingles++;
                    }
                  }
                  break;

                case 3: x1 = x; y1 = y - 1;
                  if (Grid[x1 + width * y1] == 1)
                  {
                    cntCracks++;
                    if (Grid[x1 + width * (y1 - 1)] == 1)
                    {
                      Grid[x1 + width * y1] = 0;
                      Grid[x1 + width * (y1 - 1)] = 0;
                      cntSingles++;
                    }
                  }
                  break;
              } //:::::::::::::::::::: end switch(dir) ::::::::::::::::::::::::

              if (cntCracks == 3 && cntSingles == 1) Grid[x + width * y] = 2;
              if (cntCracks == 3 && cntSingles == 2) Grid[x + width * y] = 1;
              if (cntCracks == 3 && cntSingles == 3) Grid[x + width * y] = 0;
              if (cntCracks == 4 && cntSingles == 1) Grid[x + width * y] = 3;
              if (cntCracks == 4 && cntSingles == 2) Grid[x + width * y] = 2;
              if (cntCracks == 4 && cntSingles == 3) Grid[x + width * y] = 1;
              if (cntCracks == 4 && cntSingles == 4) Grid[x + width * y] = 0;
            } //============================== end for (int dir... ==================
          } //-------------------------------- end if ((Grid[x ... --------------------
        } //================================== end for (int x... ========================
      } //==================================== end for (int y... ==========================


      byte[] GridCopy = new byte[width * height];
      for (int i = 0; i < width * height; i++) GridCopy[i] = Grid[i];

      SumCells = 0;

      // Deleting connected components of edge which contain end or brunch points and 
      // are less than "Size"
      for (int y = 0; y < height; y += 2)
      {
        for (int x = 0; x < width; x += 2) //=========================================
        {
          Lab = GridCopy[x + width * y] & (Mask | 128);
          if (Lab == 1 || Lab == 3 || Lab == 4) //----------------------------------
          {
            SumCells = ComponClean(GridCopy, x, y, Index, Size);
            if (rv < 0) return -1;
            if (SumCells < 0) return -1;
            if (SumCells < Size)
            {
              for (int i = 0; i < SumCells; i++) Grid[Index[i]] = 0;
            } //-------------------- end if (SumCells <= Size) -----------------------------------
          } //---------------------- end if (if (Lab == 1 || Lab == 3 || Lab == 4) ------------------
        } //======================== end for (int x ... ===============================================
      } //========================== end for (int x ... ===============================================

      // Deleting small components containing no end points and no brunchings
      SumCells = 0;
      for (int y = 0; y < height; y += 2) //===============================================
      {
        for (int x = 0; x < width; x += 2) //=========================================
        {
          Lab = GridCopy[x + width * y] & Mask;
          if (Lab == 2) //----------------------------------
          {
            SumCells = ComponClean(GridCopy, x, y, Index, Size);
            if (SumCells <= Size)
            {
              for (int i = 0; i < SumCells; i++) Grid[Index[i]] = 0;
            }
          } //---------------------- end if (Lab == 2) -------------------------------------------

        } //========================= end for (int x ... ==============================================
      } //=========================== end for (int y ... ===============================================
      // End deleting small components.
      return 1;

      // Deleting branches of two cracks: (19.03.2018)
      bool POINTDELETED = false;
      byte numDeletedCracks = 0;
      for (int y = 0; y < height; y += 2)
        for (int x = 0; x < width; x += 2) //======================================================
        {
          if (Grid[x + width * y] > 2) //--------------------------------------------------
          {
            numDeletedCracks = 0;
            if (Grid[x + 1 + width * y] == 1 &&
                Grid[x + 2 + width * y] == 2) // crack to right with end, dir == 0
            {
              POINTDELETED = false;
              if (Grid[x + 3 + width * y] == 1 && // second crack dir=0
                  Grid[x + 4 + width * y] == 1)
              {
                Grid[x + 1 + width * y] = 0; // right crack
                Grid[x + 2 + width * y] = 0; // end of right crack
                Grid[x + 3 + width * y] = 0; // second crack
                Grid[x + 4 + width * y] = 0;  // second end point--*/
                POINTDELETED = true;
              }

              if (Grid[x + 2 + width * (y + 1)] == 1 && // second crack dir=1
                  Grid[x + 2 + width * (y + 2)] == 1)
              {
                Grid[x + 1 + width * y] = 0; // right crack
                Grid[x + 2 + width * y] = 0; // end of right crack
                Grid[x + 2 + width * (y + 1)] = 0; // second crack
                Grid[x + 2 + width * (y + 2)] = 0;  // end of second crack
                POINTDELETED = true;
              }

              //POINTDELETED = false;
              if (Grid[x + 2 + width * (y - 1)] == 1 && // second crackt dir=3
                  Grid[x + 2 + width * (y - 2)] == 1)
              {
                Grid[x + 1 + width * y] = 0; // right crack
                Grid[x + 2 + width * y] = 0; // end of right crack
                Grid[x + 2 + width * (y - 1)] = 0; // second crack
                Grid[x + 2 + width * (y - 2)] = 0;  // end of second crack
                POINTDELETED = true;
              }
              if (POINTDELETED)
              {
                Grid[x + 1 + width * y] = 0; // right crack
                numDeletedCracks++;
              }
            } //-------------- end of if "crack to right" --------------------
            if (numDeletedCracks > 0) Grid[x + width * y] -= numDeletedCracks;
          } //----------------- end if (Grid[x + width * y] > 2) --------------------


          if (Grid[x + width * y] > 2) //--------------------------------------------------
          {
            numDeletedCracks = 0;
            if (Grid[x + width * (y + 1)] == 1 && // crack down, dir == 1
                Grid[x + width * (y + 2)] == 2) // end of crack down
            {
              POINTDELETED = false;
              if (Grid[x + 1 + width * (y + 2)] == 1 && // second crack dir==0
                  Grid[x + 2 + width * (y + 2)] == 1)   // end of second crack
              {
                Grid[x + width * (y + 1)] = 0; // crack down
                Grid[x + width * (y + 2)] = 0; // end of crack down
                Grid[x + 1 + width * (y + 2)] = 0; // second crack
                Grid[x + 2 + width * (y + 2)] = 0;  // end of second crack
                POINTDELETED = true;
              }

              if (Grid[x + width * (y + 3)] == 1 && // second crack dir==1
                  Grid[x + width * (y + 4)] == 1)   // end of second crack
              {
                Grid[x + width * (y + 1)] = 0; // crack down
                Grid[x + width * (y + 2)] = 0; // end of crack down
                Grid[x + width * (y + 3)] = 0; // second crack
                Grid[x + width * (y + 4)] = 0;  // end of second crack
                POINTDELETED = true;
              }

              if (Grid[x - 1 + width * (y + 2)] == 1 && // second crack dir==2
                  Grid[x - 2 + width * (y + 2)] == 1)   // end of second crack
              {
                Grid[x + width * (y + 1)] = 0; // crack down
                Grid[x + width * (y + 2)] = 0; // end of crack down
                Grid[x - 1 + width * (y + 2)] = 0; // second crack
                Grid[x - 2 + width * (y + 2)] = 0;  // end of second crack
                POINTDELETED = true;
              }
              if (POINTDELETED)
              {
                Grid[x + width * (y + 1)] = 0; // crack down
                numDeletedCracks++;
              }
            } //-------------- end of if "crack down" --------------------
            if (numDeletedCracks > 0) Grid[x + width * y] -= numDeletedCracks;
          } //----------------- end if (Grid[x + width * y] > 2) --------------------


          if (Grid[x + width * y] > 2) //--------------------------------------------------
          {
            numDeletedCracks = 0;
            if (Grid[x - 1 + width * y] == 1 && // crack to left, dir == 2
                Grid[x - 2 + width * y] == 2) // end of crack to left
            {
              //numDeletedCracks = 0;
              POINTDELETED = false;
              if (Grid[x + 2 + width * (y + 1)] == 1 && // second crack dir=1
                  Grid[x + 2 + width * (y + 2)] == 1)
              {
                Grid[x - 1 + width * y] = 0; // left crack
                Grid[x - 2 + width * y] = 0; // end of leftt crack
                Grid[x - 2 + width * (y + 1)] = 0; // second crack
                Grid[x - 2 + width * (y + 2)] = 0;  // end of second crack
                POINTDELETED = true;
              }

              if (Grid[x - 3 + width * y] == 1 && // second crack dir=2
                  Grid[x - 4 + width * y] == 1)
              {
                Grid[x - 1 + width * y] = 0; // left crack
                Grid[x - 2 + width * y] = 0; // end of left crack
                Grid[x - 3 + width * y] = 0; // second crack
                Grid[x - 4 + width * y] = 0;  // second end point
                POINTDELETED = true;
              }

              if (Grid[x - 2 + width * (y - 1)] == 1 && // second crack dir=3
                  Grid[x - 2 + width * (y - 2)] == 1)   // end of second crack
              {
                Grid[x - 1 + width * y] = 0; // left crack
                Grid[x - 2 + width * y] = 0; // end of left crack
                Grid[x - 2 + width * (y - 1)] = 0; // second crack
                Grid[x - 2 + width * (y - 2)] = 0;  // end of second crack
                POINTDELETED = true;
              }
              if (POINTDELETED)
              {
                Grid[x - 1 + width * y] = 0; // left crack
                numDeletedCracks++;
              }
            } //-------------- end of if "crack to right" --------------------
            if (numDeletedCracks > 0) Grid[x + width * y] -= numDeletedCracks;
          } //----------------- end if (Grid[x + width * y] > 2) -----------------


          if (Grid[x + width * y] > 2) //--------------------------------------------------
          {
            numDeletedCracks = 0;
            if (Grid[x + width * (y - 1)] == 1 && // crack upwards, dir == 3
                Grid[x + width * (y - 2)] == 2) // end of crack upwards
            {
              //numDeletedCracks = 0;
              POINTDELETED = false;
              if (Grid[x + 1 + width * (y - 2)] == 1 && // second crack dir==0
                  Grid[x + 2 + width * (y - 2)] == 1)   // end of second crack
              {

                Grid[x + width * (y - 1)] = 0; // crack upwards
                Grid[x + width * (y - 2)] = 0; // end of crack upwards
                Grid[x + 1 + width * (y - 2)] = 0; // second crack
                Grid[x + 2 + width * (y - 2)] = 0;  // end of second crack
                POINTDELETED = true;
              }

              if (Grid[x - 1 + width * (y - 2)] == 1 && // second crack dir==2
                  Grid[x - 2 + width * (y - 2)] == 1)   // end of second crack
              {
                Grid[x + width * (y - 1)] = 0; // crack upwards
                Grid[x + width * (y - 2)] = 0; // end of crack upwards
                Grid[x - 1 + width * (y - 2)] = 0; // second crack
                Grid[x - 2 + width * (y - 2)] = 0;  // end of second crack
                POINTDELETED = true;
              }

              if (Grid[x + width * (y - 3)] == 1 && // second crack dir==3
                  Grid[x + width * (y - 4)] == 1)   // end of second crack
              {
                Grid[x + width * (y - 1)] = 0; // crack upwards
                Grid[x + width * (y - 2)] = 0; // end of crack upwards
                Grid[x + width * (y - 3)] = 0; // second crack
                Grid[x + width * (y - 4)] = 0;  // end of second crack
                POINTDELETED = true;
              }

              if (POINTDELETED)
              {
                Grid[x + width * (y - 1)] = 0; // crack upwards
                numDeletedCracks++;
              }
            } //-------------- end of if "crack upwards" --------------------
            if (numDeletedCracks > 0) Grid[x + width * y] -= numDeletedCracks;
          } //---------------- end if (Grid[x + width * y] > 2) -----------------------
        } //======================= end for (x ... =======================================
      return 1;
    } //************************************** end CleanCombNew ********************************************+



    public int CheckComb(int Mask)
    {
      bool found = false;
      for (int y = 2; y < height - 1; y += 2)
      {
        for (int x = 2; x < width - 1; x += 2)
        {
          int cnt = 0, val = (Grid[x + width * y] & Mask);
          for (int dir = 0; dir < 4; dir++)
          {
            switch (dir)
            {
              case 0: if (Grid[x + 1 + width * y] == 1) cnt++; break;
              case 1: if (Grid[x + width * (y + 1)] == 1) cnt++; break;
              case 2: if (Grid[x - 1 + width * y] == 1) cnt++; break;
              case 3: if (Grid[x + width * (y - 1)] == 1) cnt++; break;
            }
          }
          if (cnt != val)
          {
            found = true;
            MessageBox.Show("CheckComb found an error at x=" + x + " y=" + y + " val of point=" + val + " numb. cracks=" + cnt);
            MessageBox.Show("Crack below has label " + Grid[x + width * (y + 1)]);
            break;
          }
        } //======================= end for (int x ... =========================
        if (found) return -1;
      }  //======================== end for (int y ... ===========================
      return 1;
    } //*************************** end CheckComb ************************************


    int ColorDifSign(byte[] Colp, byte[] Colh)
    // Returns the sum of the absolut differences of the color components divided through 3
    // with the sign of MaxC(Colp) - MaxC(Colh).
    {
      int Dif = 0;
      for (int c = 0; c < 3; c++) Dif += Math.Abs(Colp[c] - Colh[c]);
      int Sign;
      if (MaxC(Colp[2], Colp[1], Colp[0]) - MaxC(Colh[2], Colh[1], Colh[0]) > 0) Sign = 1;
      else Sign = -1;

      return (Sign * Dif) / 3;
    }


    public int LabelCellsSign(int th, CImage Image3)
    /* Looks in "Image3" (standard coord.) for all pairs of adjacent pixels with signed color  
      differences greater than "th" or less than "-th" and finds the maximum or minimum color  
      differences among pixels of adjacent pairs in the same line or in the same column.
      Labels the extremal cracks in "this" (combinatorial coord.) with "1". The points incident
      to the cracks are also provided with labels indicating the number of incident cracks. 
      This method works both for color and gray value images. ---*/
    {
      int difH, difV, c, maxDif, minDif, nByte, NXB = Image3.width, x, y, xopt, yopt;
      int Inp, State, Contr, xStartP, xStartM, yStartP, yStartM;
      if (Image3.N_Bits == 24) nByte = 3;
      else nByte = 1;
      for (x = 0; x < width * height; x++) Grid[x] = 0;
      byte[] Colorp = new byte[3], Colorh = new byte[3], Colorv = new byte[3];
      maxDif = 0; minDif = 0;
      for (y = 3; y < height; y += 2) //================ vertical cracks =====================
      {
        State = 0;
        xopt = -1; xStartP = xStartM = -1;
        for (x = 3; x < width; x += 2)  //====================================================
        {
          for (c = 0; c < nByte; c++)
          {
            Colorv[c] = Image3.Grid[c + nByte * ((x - 2) / 2) + nByte * NXB * (y / 2)];
            Colorp[c] = Image3.Grid[c + nByte * (x / 2) + nByte * NXB * (y / 2)];
          }
          if (nByte == 3) difV = ColorDifSign(Colorp, Colorv);
          else difV = Colorp[0] - Colorv[0];
          if (difV > th) Inp = 1;
          else
            if (difV > -th) Inp = 0;
            else Inp = -1;

          Contr = State * 3 + Inp;
          switch (Contr) //:::::::::::::::::::::::::::::::::::::::::::::::::
          {
            case 4:
              if (x > xStartP && difV > maxDif)
              {
                maxDif = difV;
                xopt = x;
              }
              break;
            case 3:
              Grid[xopt - 1 + width * y] = 1; // vertical crack
              Grid[xopt - 1 + width * (y - 1)]++; // point above
              Grid[xopt - 1 + width * (y + 1)]++; // point below
              State = 0;
              break;
            case 2:
              Grid[xopt - 1 + width * y] = 1; // vertical crack
              Grid[xopt - 1 + width * (y - 1)]++; // point above
              Grid[xopt - 1 + width * (y + 1)]++; // point below
              minDif = difV;
              xopt = x;
              xStartM = x;
              State = -1;
              break;
            case 1: maxDif = difV; xopt = x; xStartP = x; State = 1; break;
            case 0: break;
            case -1: minDif = difV; xopt = x; xStartM = x; State = -1; break;
            case -2:
              Grid[xopt - 1 + width * y] = 1;       // vertical crack
              Grid[xopt - 1 + width * (y - 1)]++; // point above
              Grid[xopt - 1 + width * (y + 1)]++; // point below
              maxDif = difV;
              xopt = x;
              xStartP = x;
              State = 1;
              break;
            case -3:
              Grid[xopt - 1 + width * y] = 1;       // vertical crack
              Grid[xopt - 1 + width * (y - 1)]++; // point above
              Grid[xopt - 1 + width * (y + 1)]++; // point below
              State = 0;
              break;
            case -4:
              if (x > xStartM && difV < minDif)
              {
                minDif = difV;
                xopt = x;
              }
              break;
          }  //:::::::::::::::::::::::::::::: end switch ::::::::::::::::::::::::::::::
        } //============================== end for (x=1;... ====================================
      } //================================ end for (y=1;... ====================================

      for (x = 1; x < width; x += 2) //================== horizontal cracks ==================
      {
        State = 0;
        minDif = 0; yopt = -1; yStartP = yStartM = 0;
        for (y = 3; y < height; y += 2)  //====================================================
        {
          for (c = 0; c < nByte; c++)
          {
            Colorh[c] = Image3.Grid[c + nByte * (x / 2) + nByte * NXB * ((y - 2) / 2)];
            Colorp[c] = Image3.Grid[c + nByte * (x / 2) + nByte * NXB * (y / 2)];
          }
          if (nByte == 3) difH = ColorDifSign(Colorp, Colorh);
          else difH = Colorp[0] - Colorh[0];
          if (difH > th)
            Inp = 1;
          else
            if (difH > -th) Inp = 0;
            else Inp = -1;

          Contr = State * 3 + Inp;
          switch (Contr) //:::::::::::::::::::::::::::::::::::::::::::::::::
          {
            case 4:
              if (y > yStartP && difH > maxDif)
              {
                maxDif = difH;
                yopt = y;
              }
              break;
            case 3:
              Grid[x + width * (yopt - 1)] = 1;     // horizontal crack
              Grid[x - 1 + width * (yopt - 1)]++; // left point
              Grid[x + 1 + width * (yopt - 1)]++; // right point
              State = 0;
              break;
            case 2:
              Grid[x + width * (yopt - 1)] = 1;       // horizontal crack
              Grid[x - 1 + width * (yopt - 1)]++; // left point
              Grid[x + 1 + width * (yopt - 1)]++; // right point
              yopt = y;
              State = -1;
              break;
            case 1: maxDif = difH; yopt = y; yStartP = y; State = 1; break;
            case 0: break;
            case -1: minDif = difH; yopt = y; yStartM = y; State = -1; break;
            case -2:
              Grid[x + width * (yopt - 1)] = 1;       // horizontal crack
              Grid[x - 1 + width * (yopt - 1)]++; // left point
              Grid[x + 1 + width * (yopt - 1)]++; // right point
              yopt = y;
              State = 1;
              break;
            case -3:
              Grid[x + width * (yopt - 1)] = 1; // horizontal crack
              Grid[x - 1 + width * (yopt - 1)]++; // left point
              Grid[x + 1 + width * (yopt - 1)]++; // right point
              State = 0;
              break;
            case -4:
              if (y > yStartM && difH < minDif)
              {
                minDif = difH;
                yopt = y;
              }
              break;
          }  //:::::::::::::::::::::::::::::: end switch ::::::::::::::::::::::::::::::

        } //============================== end for (y=1;... ====================================
      } //================================ end for (x=1;... ====================================
      return 1;
    } //******************************** end LabelCellsSign *****************************************


  }
}
