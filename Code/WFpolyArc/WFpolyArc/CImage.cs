using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace WFpolyArc
{
  public class CQueInd
  {
    public int input, output, Len;
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
      int bytes = Math.Abs(bmpData.Stride) * bmp.Height; 
      byte[] rgbValues = new byte[bytes];
      System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

      fm1.progressBar1.Visible = true;
      int y1 = 2 + nLoop*bmp.Height / (1 + denomProg);

      for (int y = 0; y < bmp.Height; y++)
      {
        if (y % y1 == 0) fm1.progressBar1.PerformStep();

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
      int jump;
      if (bmp.Height > 300) jump = bmp.Height / 100;
      else jump = 2;

      for (int y = 0; y < bmp.Height; y++)
      {
        if (y % jump == 0) fm1.progressBar1.PerformStep();

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
      byte light = 0; 
      Color color;
      int index = 0;
      fm1.progressBar1.Visible = true;
      int y1 = 1 + nLoop * bmp.Height / denomProg;
      for (int y = 0; y < bmp.Height; y++)
      {
        if (y % y1 == 0) fm1.progressBar1.PerformStep();
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
                index = 3 * x + Math.Abs(bmpData.Stride) * y; 
                rgbValues[index + 0] = light; // color.B; 
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
      int jump, Len = bmp.Height, nStep = 25;
      if (Len > 2*nStep) jump = Len / nStep;
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
      //fm1.progressBar1.Visible = false;
    } //****************************** end ImageToBitmapOld ****************************************


    int ColorDif(byte[] Colp, byte[] Colh)
    // Returns the sum of the absolut differences of the color components.
    {
      int Dif = 0;
      for (int c = 0; c < 3; c++) Dif += Math.Abs(Colp[c] - Colh[c]);
      return Dif/3;
    }


    
    public int SigmaNewM(ref CImage Inp, int hWind, int Toleranz, Form1 fm1)
    // Sigma filter with doubled calculation of the output values for gray value images.
    {
      N_Bits = 8; width = Inp.width; height = Inp.height;
      int gv, y1, yEnd, yStart;
      Grid = new byte[width * height * N_Bits / 8];
      int[] hist = new int[256];
      int yy = nLoop * height / denomProg;
      for (int y = 0; y < height; y++) // =======================================
      {
        yStart = Math.Max(y - hWind, 0);
        yEnd = Math.Min(y + hWind, height - 1);
        if ((y % yy) == 0) fm1.progressBar1.PerformStep();
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
          for (gv = gvMin; gv <= gvMax; gv++) // First loop through the histogram
          {
            Sum += gv * hist[gv]; nPixel += hist[gv];
          }
          if (nPixel > 0) prov = (Sum + nPixel / 2) / nPixel;
          else prov = Inp.Grid[x + width * y];  // "prov" is the provis. average
          // New:		
          Sum = nPixel = 0;
          gvMin = Math.Max(0, prov - Toleranz);
          gvMax = Math.Min(255, prov + Toleranz);
          for (gv = gvMin; gv <= gvMax; gv++) // Second loop through the histogram
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
      int yy = 5+nLoop * height / denomProg;
      for (int y = 0; y < height; y++) // =======================================
      {
        if ((y % yy) == 0) fm1.progressBar1.PerformStep();
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
              Sum[c] += gv * hist[c][gv];
              nPixel[c] += hist[c][gv];
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
      int yy = nLoop * height / denomProg;
      for (int y = 0; y < height; y++) // =======================================
      {
        if (((y + 1) % yy) == 0) fm1.progressBar1.PerformStep();
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

      int yy = 1 + nLoop * height / denomProg;
      int index, maxLum = 0;
      byte[] color = new byte[3];
      for (int y = 0; y < height; y++) // ====================================================
      {
        yStart = Math.Max(y - hWind, 0);
        yEnd = Math.Min(y + hWind, height - 1);

        for (int x = 0; x < width; x++) //================================================
        { index = x + width*y;
          xStart = Math.Max(x - hWind, 0);
          xEnd = Math.Min(x + hWind, width - 1);
          maxLum = 0;
          for (int y1 = yStart; y1 <= yEnd; y1++)
          { for (int x1 = xStart; x1 <= xEnd; x1++) //===================================
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
          { for (int c=0; c<3; c++)
                Grid[c + 3*(x + width * y)] = color[c];
            //MessageBox.Show("ExtremNew: x=" + x + " y=" + y + " color[2]=" + color[2]);
          }
          else Grid[x + width * y] = (byte)maxLum;
        } //================== end for (int x... ======================
      } //==================== end for (int y... ========================
      return 1;
    } //********************** end ExtrmNew **********************************



    
    public int ExtremVarColor(CImage Inp, int hWind, Form1 fm1)
    {	
      /* The extreme filter for 3 byte color images with variable hWind.
	    The filter finds in the (2*hWind+1)-neighbourhood of the actual pixel (x,y) the color "Color1" which has the greatest 
	    difference form the central color of (x, y). Then it finds a Color2 which has the greatest differnce from
	    Color1. Color1 is assigned to the output pixel if its difference to the cental color is smaller than that
	    of Color2. Otherwise Color 2 is assigned. --*/
      int[] CenterColor = new int[3], Color = new int[3], Color1 = new int[3], Color2 = new int[3];

      int c, k, x; 
      //int yy = 5 + nLoop * height / denomProg;
      int jump, nLine1 = height, nStep = 25;
      if (nLine1 > 2 * nStep) jump = nLine1 / nStep;
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
                if (x + i >= 0 && x + i < width && (i != 0 || k != 0))
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
                if (x + i >= 0 && x + i < width && (i != 0 || k != 0))
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


    public int ExtremColorNew(CImage Inp, Form1 fm1)
    {	/* The extreme filter for 3 byte color images with variable hWind.
	    The filter finds in the (2*hWind+1)-neighbourhood of the actual pixel (x,y) the color "Color1" which has the greatest 
	    difference form the central color of (x, y). Then it finds a Color2 which has the greatest differnce from
	    Color1. Color1 is assigned to the output pixel if its difference to the cental color is smaller than that
	    of Color2. Otherwise Color 2 is assigned. --*/
      int[] CenterColor = new int[3], Color = new int[3], Color1 = new int[3], Color2 = new int[3];

      int c, k, x;
      int hWind = 1;
      for (int y = 0; y < height; y++) // =======================================
      {
        for (x = 0; x < width; x++) //==============================================
        {
          for (c = 0; c < 3; c++) Color2[c] = Color1[c] = Color[c] = CenterColor[c] = Inp.Grid[c + 3 * x + y * width * 3];
          int MaxDist = -1;
          for (k = -hWind; k <= hWind; k++) //=========================================
          {
            if (y + k >= 0 && y + k < height)
              for (int i = -hWind; i <= hWind; i++) //=====================================
              {
                if (x + i >= 0 && x + i < width && (i > 0 || k > 0))
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
                if (x + i >= 0 && x + i < width && (i > 0 || k > 0))
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
          double Sdist1 = 0.0, Sdist2 = 0.0;
          for (c = 0; c < 3; c++)
          {
            dist1 += (Color1[c] - CenterColor[c]) * (Color1[c] - CenterColor[c]);
            dist2 += (Color2[c] - CenterColor[c]) * (Color2[c] - CenterColor[c]);
          }
          Sdist1 = Math.Sqrt((double)dist1);
          Sdist2 = Math.Sqrt((double)dist2);

          if (Sdist1 < Sdist2) for (c = 0; c < 3; c++) Grid[c + 3 * x + y * width * 3] = (byte)Color1[c];
          else for (c = 0; c < 3; c++) Grid[c + 3 * x + y * width * 3] = (byte)Color2[c];
          if (y >= 10 && y <= 14 && x >= 10 && x <= 14)
            MessageBox.Show("x=" + x + ",y=" + y + " Col1=(" + Color1[2] + "," + Color1[1] + "," + Color1[0] + ")" +
              " Col2=(" + Color2[2] + "," + Color2[1] + "," + Color2[0] + ")" +
             " Center=(" + Inp.Grid[2 + 3 * x + y * width * 3] + "," +
             Inp.Grid[1 + 3 * x + y * width * 3] + "," + Inp.Grid[0 + 3 * x + y * width * 3] + ") Sdist1=" +
          (int)Sdist1 + " Sdist2=" + (int)Sdist2);

        } //================== end for (int x... ================================
      } //==================== end for (int y... ==================================
      return 1;
    } //********************** end ExtremColorNew *************************************


    public int ExtremColorOld(ref CImage Inp, Form1 fm1)
    {	/* The extreme filter for 3 byte color images with variable hWind.
	    The filter finds in the (2*hWind+1)-neighbourhood of the actual pixel (x,y) the color "Color1" which has the greatest 
	    difference form the central color of (x, y). Then it finds a Color2 which has the greatest differnce from
	    Color1. Color1 is assigned to the output pixel if its difference to the cental color is smaller than that
	    of Color2. Otherwise Color 2 is assigned. --*/
      int hWind = 1;
      int[] CenterColor = new int[3], Color = new int[3], Color1 = new int[3], Color2 = new int[3];

      int c, ind, i, k, x;

      int[,] K = { { 0, 1, 1 }, { 1, 1, 1 }, { 0, 1, 1 }, { -1, 0, 1 }, { 0, 0, 0 }, { -1, 0, 1 }, { -1, -1, 0 }, 
                 { -1, -1, -1 }, { -1, -1, 0 } };
      int[,] I = { { 1, 0, 1 }, { -1, 0, 1 }, {-1, -1, 0 }, { 1, 1, 1 }, { 0, 0, 0 }, { -1, -1, -1 }, { 0, 1, 1 }, 
                 { -1, 0, 1 }, { -1, 0, -1 } };
      int[] Hline1 = new int[Inp.width];
      int[] Hline2 = new int[Inp.width];
      int yy = 5 + nLoop * height / denomProg;
      for (int y = 0; y < height; y++) // =======================================
      {
        if (((y + 1) % yy) == 0) fm1.progressBar1.PerformStep();
        ind = 0;
        for (x = 0; x < width; x++) //==============================================
        {
          for (c = 0; c < 3; c++) Color2[c] = Color1[c] = Color[c] = CenterColor[c] = Inp.Grid[c + 3 * x + y * width * 3];
          int MaxDist = -1;
          for (k = -hWind; k <= hWind; k++) //=========================================
          {
            if (y + k >= 0 && y + k < height)
              for (i = -hWind; i <= hWind; i++) //=====================================
              {
                if (x + i >= 0 && x + i < width && (i > 0 || k > 0))
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
                    ind = i + 1 + 3 * (k + 1);
                  }
                }
              } //=============== end for (int i... ============================
          } //================ end for (int k... ==============================
          MaxDist = -1;
          for (int j = 0; j < 3; j++) //=========================================
          {
            k = K[ind, j];
            if (y + k >= 0 && y + k < height)
              for (int m = 0; m < 3; m++) //=====================================
              {
                i = I[ind, m];
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
              } //=============== end for (int m... ============================
          } //=================== end for (int j... ==============================
          int dist1 = 0, dist2 = 0;
          for (c = 0; c < 3; c++)
          {
            dist1 += (Color1[c] - CenterColor[c]) * (Color1[c] - CenterColor[c]);
            dist2 += (Color2[c] - CenterColor[c]) * (Color2[c] - CenterColor[c]);
          }
          int Sum = 0;
          if (y > 0 && x >= 5 && x < Inp.width - 6)
          {
            for (int xx = x - 5; xx < x + 5; xx++) Sum += Hline1[xx];
            for (int xx = x - 5; xx < x; xx++) Sum += Hline2[xx];
          }

          if (y == 0 && dist1 < dist2 || y > 0 && Sum < 0)
          {
            for (c = 0; c < 3; c++) Grid[c + 3 * x + y * width * 3] = (byte)Color1[c];
            Hline2[x] = (int)(1.0 * Math.Sqrt((double)dist1));
          }
          else
          {
            for (c = 0; c < 3; c++) Grid[c + 3 * x + y * width * 3] = (byte)Color2[c];
            Hline2[x] = (int)(-1.0 * Math.Sqrt((double)dist2));
          }

          if (y >= 10 && y <= 16 && x >= 12 && x <= 20)
            MessageBox.Show("x=" + x + ",y=" + y + " Col1=(" + Color1[2] + "," + Color1[1] + "," + Color1[0] + ")" +
              " Col2=(" + Color2[2] + "," + Color2[1] + "," + Color2[0] + ")" +
             " Center=(" + Inp.Grid[2 + 3 * x + y * width * 3] + "," +
             Inp.Grid[1 + 3 * x + y * width * 3] + "," + Inp.Grid[0 + 3 * x + y * width * 3] + ") dist1=" +
          dist1 + " dist2=" + dist2 + " Sum=" + Sum);

        } //================== end for (int x... ================================
        for (int ii = 0; ii < Inp.width; ii++) Hline1[ii] = Hline2[ii];
      } //==================== end for (int y... ==================================
      return 1;
    } //********************** end ExtremColorOld *************************************

     

    public void CracksToPixel(CImage Comb)
    {
      int width = Comb.width;
      int height = Comb.height;
      int nx = width / 2;
      int ny = height / 2;
      int dim;
      for (int i = 0; i < nx * ny; i++) Grid[i] = 0;

      //int cnt = 0;
      for (int y = 0; y < height - 2; y++)
        for (int x = 0; x < width - 2; x++)
        {
          dim = (x % 2) + (y % 2);
          if (dim == 1 && Comb.Grid[x + width * y] > 0)
          {
            Grid[x / 2 + nx * (y / 2)] = 255;
          }
        } //======================= end for (int x... ===============
    } //**************************** end CracksToPixel *************************


    public void CleanComb()
    // Changes the 'short' brunch points to points with two incident cracks.
    {
      int cntSingles, cntCracks, x1, y1; 
      for (int y = 0; y < height; y+=2) //===========================================
      {
        for (int x = 0; x < width; x += 2) //==== over points =====================
        {
          if ((Grid[x + width * y] & 7) > 2) //----------------------------------
          {
            cntSingles = cntCracks = 0;
            for (int dir = 0; dir < 4; dir++) //========== over four cracks ===
            {
              switch (dir)
              {
                case 0: x1 = x + 1; y1 = y;  // crack to the right
                  if (Grid[x1 + width * y1] == 1) // crack
                  {
                    cntCracks++;
                    if ((Grid[x1 + 1 + width * y1] & 7) == 1) // end of crack
                    {
                      Grid[x1 + width * y1] = 0;
                      Grid[x1 + 1 + width * y1] = 0;
                      cntSingles++;
                    }
                  }
                  break;

                case 1: x1 = x; y1 = y + 1;
                  if (Grid[x1 + width * y1] == 1)
                  {
                    cntCracks++;
                    if ((Grid[x1 + width * (y1 + 1)] & 7) == 1)
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
                    if ((Grid[x1 - 1 + width * y1] & 7) == 1)
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
                    if ((Grid[x1 + width * (y1 - 1)] & 7) == 1)
                    {
                      Grid[x1 + width * y1] = 0;
                      Grid[x1 + width * (y1 - 1)] = 0;
                      cntSingles++;
                    }
                  }
                  break;
              } //:::::::::::::::::::: end switch(dir) ::::::::::::::
              if (cntCracks == 3 && cntSingles == 1) Grid[x + width * y] = 2;
              if (cntCracks == 4 && cntSingles == 1) Grid[x + width * y] = 3;
              if (cntCracks == 3 && cntSingles == 2) Grid[x + width * y] = 1;
              if (cntCracks == 4 && cntSingles == 2) Grid[x + width * y] = 2;
            } //============================== end for (int dir... ============
          } //-------------------------------- end if ((Grid[x ... --------------
        } //================================== end for (int x... ==================
      } //==================================== end for (int y... ====================
    } //************************************** end CleanComb **************************

 
    public int ColorDif(byte[] Colp, byte[] Colh, int nChan)
    // Returns the sum of the absolut differences of the color components.
    {
      int Dif = 0;
      for (int c = 0; c < nChan; c++) Dif += Math.Abs(Colp[c] - Colh[c]);
      return Dif;
    }

    public bool InSpace(int x1, int y1, int x2, int y2, int NX, int NY)
    {
      if (x1 < 0 || x1 >= NX) return false;
      if (x2 < 0 || x2 >= NX) return false;
      if (y1 < 0 || y1 >= NY) return false;
      if (y2 < 0 || y2 >= NY) return false;
      return true;
    }

    public int ExtremVarColor(CImage Inp, int hWind)
    {
      /* The extreme filter for 3 byte color images with variable hWind.
	    The filter finds in the (2*hWind+1)-neighbourhood of the actual pixel (x,y) the color "Color1" which 
      has the greatest difference form the central color of (x, y). Then it finds a "Color2" which has the 
      greatest differnce from "Color1". "Color1" is assigned to the output pixel if its difference from the 
      cental color is greater than that of "Color2". Otherwise Color 2 is assigned. --*/
      byte[] CenterColor = new byte[3], Color = new byte[3], Color1 = new byte[3], Color2 = new byte[3];
      int c, k, x; 
      for (int y = 0; y < height; y++) // =================================================
      {
        for (x = 0; x < width; x++) //==============================================
        {
          for (c = 0; c < 3; c++) Color2[c] = Color1[c] = Color[c] = CenterColor[c] = Inp.Grid[c + 3 * x + y * width * 3];
          int MaxDist = -1;
          for (k = -hWind; k <= hWind; k++) //=========================================
          {
            if (y + k >= 0 && y + k < height)
              for (int i = -hWind; i <= hWind; i++) //=====================================
              {
                if (x + i >= 0 && x + i < width && ((i != 0) || (k != 0)))
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
                if (x + i >= 0 && x + i < width && ((i != 0) || (k != 0)))
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
          if (dist1 < dist2) for (c = 0; c < 3; c++) Grid[c + 3 * x + y * width * 3] = Color1[c];
          else for (c = 0; c < 3; c++) Grid[c + 3 * x + y * width * 3] = Color2[c];
        } //================== end for (int x... ================================
      } //==================== end for (int y... ==================================
      return 1;
    } //********************** end ExtremVarColor *************************************

    public int MessReturn(string s)
    {
      if (MessageBox.Show(s, "Return", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
        return -1;
      return 1;
    }

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


    public int SigmaSimpleUni(CImage Inp, int hWind, int Toleranz, Form1 fm1)
    // Simple sigma filter for both gray value and color images. 
    {
      int[] gvMin = new int[3], gvMax = new int[3], nPixel = new int[3], Sum = new int[3];
      int c;
      N_Bits = Inp.N_Bits;
      int nbyte = N_Bits / 8;
      fm1.progressBar1.Step = 1;
      fm1.progressBar1.Value = 0;
      fm1.progressBar1.Visible = true;

      int jump, nLine1 = height, nStep = 25;
      if (nLine1 > 2 * nStep) jump = nLine1 / nStep;
      else jump = 2;

      for (int y = 0; y < height; y++) // ==================================================
      {
        if (y % jump == jump - 1) fm1.progressBar1.PerformStep();
        int gv, y1, yStart = Math.Max(y - hWind, 0), yEnd = Math.Min(y + hWind, height - 1);
        for (int x = 0; x < width; x++) //===============================================
        {
          int x1, xStart = Math.Max(x - hWind, 0), xEnd = Math.Min(x + hWind, width - 1);
          for (c = 0; c < nbyte; c++)
          {
            Sum[c] = 0; nPixel[c] = 0;
            gvMin[c] = Math.Max(0, Inp.Grid[c + nbyte * (x + width * y)] - Toleranz);
            gvMax[c] = Math.Min(255, Inp.Grid[c + nbyte * (x + width * y)] + Toleranz);
          }
          for (y1 = yStart; y1 <= yEnd; y1++)
            for (x1 = xStart; x1 <= xEnd; x1++)
              for (c = 0; c < nbyte; c++)
              {
                gv = Inp.Grid[c + nbyte * (x1 + y1 * width)];
                if (gv >= gvMin[c] && gv <= gvMax[c])
                {
                  Sum[c] += gv;
                  nPixel[c]++;
                }
              }
          for (c = 0; c < nbyte; c++)
          {
            if (nPixel[c] > 0)
              Grid[c + nbyte * (x + width * y)] = (byte)((Sum[c] + nPixel[c] / 2) / nPixel[c]);
            else Grid[c + nbyte * (x + width * y)] = Inp.Grid[c + nbyte * (x + width * y)];
          }
        } //================== end for (int x... =================================
      } //==================== end for (int y... ===================================
      return 1;
    } //********************** end SigmaSimpleUni **********************************


    public void DrawComb(int StandX, int StandY, Form1 fm1)
    // Draws in the pictureBox1 the cracks of the image "Comb".
    {
      if (StandX < 0) StandX = 0;
      if (StandY < 0) StandY = 0;
      int PointSize = 3, SizeX = 2 * 75, SizeY = 2 * 75, Step = 4;
      if (width < 50)
      {
        SizeX = width / 2; SizeY = height / 2;
      }
      Graphics g = fm1.pictureBox1.CreateGraphics();
      Pen whitePen;
      whitePen = new Pen(Color.White, 1);
      SolidBrush blackBrush, redBrush, greenBrush, yellowBrush, blueBrush;
      blackBrush = new SolidBrush(Color.Black);
      redBrush = new SolidBrush(Color.Red);
      greenBrush = new SolidBrush(Color.LightGreen);
      yellowBrush = new SolidBrush(Color.Yellow);
      blueBrush = new SolidBrush(Color.Violet);
      Rectangle rect, rect2, point;
      rect = new Rectangle(0, 0, fm1.pictureBox1.Width, fm1.pictureBox1.Height);
      g.FillRectangle(blackBrush, rect);


      Graphics g2 = fm1.pictureBox2.CreateGraphics();
      int X = (int)(fm1.Scale1 * (StandX)) + fm1.marginX;
      int Y = (int)(fm1.Scale1 * (StandY)) + fm1.marginY;
      rect2 = new Rectangle(X, Y, (int)(fm1.Scale1 * (SizeX / 2)), (int)(fm1.Scale1 * (SizeY / 2)));
      g2.DrawRectangle(whitePen, rect2);

      int xpB, ypB;
      int NX = Math.Min(width, 2 * (StandX + SizeX));
      int NY = Math.Min(height, 2 * (StandY + SizeY));
      byte Mask = 7;
      for (int y = 2 * StandY; y < NY; y++) //===================================================
      {
        if ((y & 1) == 0) //---------------------------------------------------------------------------
        {
          for (int x = 2 * StandX + 1; x < NX; x += 2) //==== over horizontal cracks ==========
          {
            xpB = x - 2 * StandX; // cracks in Comb section, odd
            ypB = y - 2 * StandY;         // points in Corb section, even
            if (Grid[x + width * y] > 0) //--------------------------------------------------------
            {
              g.DrawLine(whitePen, (xpB - 1) * Step + 1, ypB * Step, (xpB + 1) * Step - 1, ypB * Step); 

              if ((Grid[x - 1 + width * y] & Mask) > 0) // left point
              {
                point = new Rectangle((xpB - 1) * Step - 1, ypB * Step - 1, PointSize, PointSize);
                switch (Grid[x - 1 + width * y] & Mask)
                {
                  case 1: g.FillRectangle(redBrush, point); break;
                  case 2: g.FillRectangle(greenBrush, point); break;
                  case 3: g.FillRectangle(yellowBrush, point); break;
                  case 4: g.FillRectangle(blueBrush, point); break;
                }
              }

              if ((Grid[x + 1 + width * y] & Mask) > 0) // right point
              {
                point = new Rectangle((xpB + 1) * Step - 1, ypB * Step - 1, PointSize, PointSize);
                switch (Grid[x + 1 + width * y] & Mask)
                {
                  case 1: g.FillRectangle(redBrush, point); break;
                  case 2: g.FillRectangle(greenBrush, point); break;
                  case 3: g.FillRectangle(yellowBrush, point); break;
                  case 4: g.FillRectangle(blueBrush, point); break;
                }
              }
            } //----------------------- end if (Grid[x + width * y] > 0) --------------------------
          } //========================= end for (int x ... ==========================================
        }
        else // (y & 1) != 0
        {
          for (int x = 2 * StandX; x < NX; x += 2) //==== over vertical cracks ================
          {
            xpB = x - 2 * StandX; // even
            ypB = y - 2 * StandY;         // odd
            if (Grid[x + width * y] > 0) //--------------------------------------------------------
            {
              g.DrawLine(whitePen, xpB * Step, (ypB - 1) * Step + 1, xpB * Step, (ypB + 1) * Step - 1);
              if ((Grid[x + width * (y - 1)] & Mask) > 0) // upper point
              {
                point = new Rectangle(xpB * Step - 1, (ypB - 1) * Step - 1, PointSize, PointSize); // upper point
                switch (Grid[x + width * (y - 1)] & Mask)
                {
                  case 1: g.FillRectangle(redBrush, point); break;
                  case 2: g.FillRectangle(greenBrush, point); break;
                  case 3: g.FillRectangle(yellowBrush, point); break;
                  case 4: g.FillRectangle(blueBrush, point); break;
                }
              }

              if ((Grid[x + width * (y + 1)] & Mask) > 0) // lower point
              {
                point = new Rectangle(xpB * Step - 1, (ypB + 1) * Step - 1, PointSize, PointSize); // lower point
                switch (Grid[x + width * (y + 1)] & Mask)
                {
                  case 1: g.FillRectangle(redBrush, point); break;
                  case 2: g.FillRectangle(greenBrush, point); break;
                  case 3: g.FillRectangle(yellowBrush, point); break;
                  case 4: g.FillRectangle(blueBrush, point); break;
                }
              }

            } //-----------------------  end if (Grid[x + width * y] > 0) -------------------------
          } //========================= end for (int x ... ==========================================
        } //--------------------------- end if ((y & 1) == 0) //---------------------------------------
      } //==================================== end for (int y... ==========================================
    } //************************************** end DrawComb *************************************************


    public int LabelCellsSign(int th, CImage Image3, Form1 fm1)
    /* Looks in "Image3" (standard coord.) for all pairs of adjacent pixels with signed color  
      differences greater than "th" or less than "-th" and finds the maximum or minimum color  
      differences among pixels of adjacent pairs in the same line or in the same column.
      Labels the extremal cracks in "this" (combinatorial coord.) with "1". The points incident
      to the cracks are also provided with labels indicating the number of incident cracks. 
      This method works both for color and gray value images. ---*/
    {
      int difH, difV, c, maxDif, minDif, nByte, NXB = Image3.width, x, y, y2, xopt, yopt;
      int Inp, State, Contr, xStartP, xStartM, yStartP, yStartM;
      if (Image3.N_Bits == 24) nByte = 3;
      else nByte = 1;
      for (x = 0; x < width * height; x++) Grid[x] = 0;
      byte[] Colorp = new byte[3], Colorh = new byte[3], Colorv = new byte[3];
      maxDif = 0; minDif = 0;

      int jump, nLine1 = height / 2, nStep = 25;
      if (nLine1 > 2 * nStep) jump = nLine1 / nStep;
      else jump = 2;

      for (y = 3, y2 = 0; y < height; y += 2, y2++) //====== vertical cracks =================
      {
        if (y2 % jump == jump - 1) fm1.progressBar1.PerformStep();
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


    public int Trace(byte[] GridCopy, int P, int[] Index, ref int SumCells, ref int Pterm, ref int dir, int Size)
    /* Used in 'ComponClean' and in 'ComponCleanNew'.
      Traces a branch of a component, saves all cells in "Index"; "SumCells" is the number of saved cells.
      If "SumCells" becomes greater than "Size", the tracing is interrupted, and the method return -1.
      Otherwise it returns a positive number. --*/
    {
      int Lab, LabCrack, Mask = 7, rv = 0;
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

        if (Crack >= 0 && Crack < width * height) LabCrack = GridCopy[Crack];
        else LabCrack = 0;
        if (LabCrack == 0)
        {
          int cellY = Crack / width;
          int cellX = Crack - cellY * width;
          int cellY1 = StartLine / width;
          int cellX1 = StartLine - cellY1 * width;
          int cellY2 = P / width;
          int cellX2 = P - cellY1 * width;
          MessageBox.Show("Trace, error: dir=" + dir + " the Crack=(" + cellX + "; " + cellY +
          ") has label 0;  P=(" + cellX2 + "; " + cellY2 + "; Lab=" + (GridCopy[P] & Mask) + " iCrack=" +
          iCrack + " Start=(" + cellX1 + "; " + cellY1 + ")");
          Pterm = P;
          if ((GridCopy[P] & Mask) == 0) return -1;
        }

        P = Crack + Step[dir];
        if (SumCells < Size) Index[SumCells] = P;

        Lab = GridCopy[P] & Mask;
        if (Lab == 2) SumCells++;

        switch (Lab)
        {
          case 0: END = true; BP = false; rv = 1; break;
          case 1: END = true; BP = false; rv = 1; break;
          case 2: BP = END = false; break;
          case 3: BP = true; END = false; rv = 3; break;
          case 4: BP = true; END = false; rv = 3; break;
        }
        if (Lab == 2) GridCopy[P] = 0;
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
          Pterm = P;
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
    /* Traces a component starting at (X, Y), saves coordinates of cells in "Index", "SumCells" is the
      * number of traced cells. If "SumCells" becomes greater than "Size", then the saving of cells is interrupted
      * but the counting in "SumCells" goes on.--*/
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
            }
            else
            {
              if (SumCells < Size)
              {
                Index[SumCells] = Crack;
              }
              SumCells++;
              if (SumCells < Size)
              {
                Index[SumCells] = Pnext;
                SumCells++;
              }

              GridCopy[Pnext] = 0;
              GridCopy[P] = 0;


              if ((GridCopy[Pnext] & Mask) >= 3 && (GridCopy[Pnext] & 128) == 0) SumCells++;
            } // ------------------------- end if (LabNest==2) -----------------------------------------------------
            if ((GridCopy[P] & Mask) == 1)
            {
              GridCopy[P] = 0;
              break; // The only crack with Lab == 1 alredy processed
            }
          } //--------------- end if (GridCopy[Crack  == 1) ------------------------------------------
        } //================================== end for (dir ... ==========================================
        GridCopy[P] |= 128;
      } //==================================== end while ===================================================
      return SumCells;
    } //************************************** end ComponClean ************************************************
    

    public int CleanCombNew(int Size, Form1 fm1)
    // Delets one crack line at a brunch point and changes the label of this point to two.
    {
      byte Mask = 7;
      int cntSingles, cntCracks, x1, y1;
      int[] Index = new int[Size + 20];
      int SumCells = 0, Lab = 0, rv = 0;

      // Transforming small squares to corners:
      for (int y = 2; y < height - 2; y += 2) //===================================================================
      {
        for (int x = 2; x < width - 2; x += 2) //===================== over points ==============================
        {
          Lab = Grid[x + width * y]; // &Mask;
          if (Grid[x + width * y] >= 3 && Grid[x - 2 + width * (y + 2)] >= 3 &&
                Grid[x - 1 + width * y] == 1 && Grid[x - 2 + width * (y + 1)] == 1 &&
                Grid[x + width * (y + 1)] == 1 && Grid[x - 1 + width * (y + 2)] == 1)
          {
            Grid[x + width * y] -= 1; // main point
            Grid[x + width * (y + 1)] = 0; // crack below
            Grid[x + width * (y + 2)] -= 2; // point below
            Grid[x - 1 + width * (y + 2)] = 0; // crack left below
            Grid[x - 2 + width * (y + 2)] -= 1; // second point
          }

          if (Grid[x + width * y] >= 3 && Grid[x + 2 + width * (y + 2)] >= 3 &&
                Grid[x + 1 + width * y] == 1 && Grid[x + 2 + width * (y + 1)] == 1 &&
                Grid[x + width * (y + 1)] == 1 && Grid[x + 1 + width * (y + 2)] == 1)
          {
            Grid[x + width * y] -= 1; // main point
            Grid[x + width * (y + 1)] = 0; // crack below
            Grid[x + width * (y + 2)] -= 2; // point below
            Grid[x + 1 + width * (y + 2)] = 0; // crack right below
            Grid[x + 2 + width * (y + 2)] -= 1; // second point
          }

          // Counting cracks and single cracks incident to a crotch point. 
          // Deleting single cracks and changing the labels of the crotch points.
          if (Grid[x + width * y] > 2) //---------------------------------------
          {
            cntSingles = cntCracks = 0;
            for (int dir = 0; dir < 4; dir++) //========== over four cracks ======================
            {
              switch (dir) //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
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
                  if (Grid[x1 + width * y1] == 1)
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
              } //:::::::::::::::::::: end switch(dir) ::::::::::::::::::::::::::::::::::::::::::::
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
      // are shorter than "Size"
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

      // Deleting deleting small components containing no end points and no brunchings
      SumCells = 0;
      for (int y = 0; y < height; y += 2)
      {
        for (int x = 0; x < width; x += 2) //=========================================
        {
          Lab = GridCopy[x + width * y] & Mask;
          if (Lab == 2) //----------------------------------
          {
            SumCells = ComponClean(GridCopy, x, y, Index, Size);
            if (SumCells < Size)
            {
              for (int i = 0; i < SumCells; i++) Grid[Index[i]] = 0;
            }
          } //---------------------- end if (Lab == 2) -------------------------------------------

        } //========================= end for (int x ... ==============================================
      }
      return 1;
    } //************************************** end CleanCombNew ********************************************


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
      //MessageBox.Show("CheckComb found no errors");
      return 1;
    } //*************************** end CheckComb ************************************

  } //***************************** end class CImage ********************************************

} //******************************** end namespace WFpolyArc*************************************

