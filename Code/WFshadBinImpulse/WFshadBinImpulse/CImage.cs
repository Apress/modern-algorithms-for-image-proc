using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;



namespace WFshadBinImpulse
{
  public class CImage
  {
    public Byte[] Grid;
    public int width, height, N_Bits;
 
    public CImage(int nx, int ny, int nbits) // constructor
    {
        this.width = nx;
        this.height = ny;
        this.N_Bits = nbits;
        Grid = new byte[width * height * (N_Bits / 8)];
    }

    public CImage(int nx, int ny, int nbits, byte[] img) // constructor
    {
        this.width = nx;
        this.height = ny;
        this.N_Bits = nbits;
        this.Grid = new byte[width * height * (N_Bits / 8)];
        for (int i = 0; i < width * height * N_Bits / 8; i++) this.Grid[i] = img[i];
    }

 
    public void Copy(CImage inp)
    {
        width = inp.width;
        height = inp.height;
        N_Bits = inp.N_Bits;
        for (int i = 0; i < width * height * N_Bits / 8; i++)
            Grid[i] = inp.Grid[i];
    }

    int MaxC(int R, int G, int B)
    {
      int max;
      if (R * 0.713 > G) max = (int)(R * 0.713);
      else max = G;
      if (B * 0.527 > max) max = (int)(B * 0.527);
      return max;
    }

    public int SigmaSimpleUni(CImage Inp, int hWind, int Toleranz)
    // Simple sigma filter for both gray value and color images. 
    {
      int[] gvMin = new int[3], gvMax = new int[3], nPixel = new int[3], Sum = new int[3];
      int c;
      N_Bits = Inp.N_Bits;
      int nbyte = N_Bits / 8;
      for (int y = 0; y < height; y++) // ==================================================
      {
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


    public int ColorToGrayMC(CImage inp, Form1 fm1)
    /* Transforms the colors of the color image "inp" in luminance=(r+g+b)/3 
    and puts these values to this.Grid. --------- */
    {
      int Light, x, y;
      if (inp.N_Bits != 24) return -1;
          
      fm1.progressBar1.Value = 0;
      fm1.progressBar1.Step = 1;
      fm1.progressBar1.Visible = true;
      int width = inp.width, height = inp.height;
      Grid = new byte[width * height * 8];
      int y1 = 1 + height / 100;
      for (y = 0; y < height; y++) //========================================
      {
        if (y % y1 == 1) fm1.progressBar1.PerformStep();
        for (x = 0; x < width; x++) // ====================================
        {
          Light = MaxC(inp.Grid[2 + 3 * (x + width * y)],
                        inp.Grid[1 + 3 * (x + width * y)],
                        inp.Grid[0 + 3 * (x + width * y)]);
          Grid[y * width + x] = (byte)Light;
        } // ==================== for (x.  ================================
      } // ====================== for (x.  ==================================
      fm1.progressBar1.Visible = false;
      return 1;
    } //************************** end ColorToGray ***************************


    public int ColorToGray(CImage inp)
    /* Transforms the colors of the color image "inp" in luminance=(r+g+b)/3 
    and puts these values to this.Grid. --------- */
    {
      int c, sum, x, y;
      if (inp.N_Bits != 24) return -1;
      N_Bits = 8; width = inp.width; height = inp.height;
      Grid = new byte[width * height * 8];
      for (y = 0; y < height; y++) //=========================
      {
        for (x = 0; x < width; x++) // =====================
        {
          sum = 0;
          for (c = 0; c < 3; c++) sum += inp.Grid[c + 3 * (x + width * y)];
          Grid[y * width + x] = (byte)(sum / 3);
        } // ========== for (x.  ====================
      }
      return 1;
    } //********************** end ColorToGray **********************

    public int FastAverageM(CImage Inp, int hWind, Form1 fm1)
    // Filters the gray value image "Inp" and returns the result as *this."
    {
      if (Inp.N_Bits != 8)
      {
          MessageBox.Show("FastAverageM cannot process an image with " + Inp.N_Bits + " bits per pixel");
          return -1;
      }
      N_Bits = 8; width = Inp.width; height = Inp.height;
      Grid = new byte[width * height];
      int[] ColSum; int[] nC;
      ColSum = new int[width];
      nC = new int[width];
      for (int i = 0; i < width; i++) ColSum[i] = nC[i] = 0;
      
      int nS = 0, Sum = 0;
      for (int y = 0; y < height + hWind; y++)
      {
          int yout = y - hWind, ysub = y - 2 * hWind - 1;
          Sum = 0; nS = 0;
          for (int x = 0; x < width + hWind; x++)
          {
              int xout = x - hWind, xsub = x - 2 * hWind - 1;	// 1. and 2. addition
              if (y < height && x < width) { ColSum[x] += Inp.Grid[x + width * y]; nC[x]++; }	// 3. and 4. addition
              if (ysub >= 0 && x < width) { ColSum[x] -= Inp.Grid[x + width * ysub]; nC[x]--; }
              if (yout >= 0 && x < width) { Sum += ColSum[x]; nS += nC[x]; }
              if (yout >= 0 && xsub >= 0) { Sum -= ColSum[xsub]; nS -= nC[xsub]; }
              if (xout >= 0 && yout >= 0) Grid[xout + width * yout] = (byte)((Sum + nS / 2) / nS);
          }
      }
      return 1;
    } //***************************** end FastAverageM ************************************************


    public int DeleteBit0(int nbyte, Form1 fm1)
    // If the image is an 8 bit image (nbyte==1) then it sets the bits 0 and 1 to 0.
    // Otherwise it sets the bits 0 in the red and green channels of the image.
    {
      int i1 = 1 + width * height / 100;
      for (int i = 0; i < width * height; i++)
      {
        if (nbyte == 1)
          Grid[i] = (byte)(Grid[i] - (Grid[i] % 4));
        else
        {
          Grid[nbyte * i + 2] = (byte)(Grid[nbyte * i + 2] & 254); // red channel
          Grid[nbyte * i + 1] = (byte)(Grid[nbyte * i + 1] & 254); // green channel
        }
      }
      return 1;
    } //*********************** end DeleteBit0 ************************************************

  } //************************* end class CImage *************************************************

} //*************************** end class CImage ***************************************************

