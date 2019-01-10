using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace WFedgeDetect
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
    public Byte[] Grid;
    public int width, height, N_Bits;
    public int nLoop, denomProg;

    public CImage() { } // default constructor

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
      for (int i = 0; i < width * height * (N_Bits / 8); i++) this.Grid[i] = img[i];
    }

    public int RGB(byte rot, byte gruen, byte blau)
    {
      int color = rot | (gruen << 8) | (blau << 16);
      return color;
    }


    public void Copy(CImage inp)
    {
      width = inp.width;
      height = inp.height;
      N_Bits = inp.N_Bits;
      for (int i = 0; i < width * height * N_Bits / 8; i++)
        Grid[i] = inp.Grid[i];
    }

    public int MaxC(int R, int G, int B)
    {
      int max;
      if (R * 0.713 > G) max = (int)(R * 0.713);
      else max = G;
      if (B * 0.527 > max) max = (int)(B * 0.527);
      return max;
    }

    public int ColorToGrayMC(CImage inp, Form1 fm1)
    /* Transforms the colors of the color image "inp" to MaxC(r,g,b) 
    and puts these values to this.Grid. --------- */
    {
      int Light, x, y;
      if (inp.N_Bits != 24) return -1;

      fm1.progressBar1.Value = 0;
      
      fm1.progressBar1.Visible = true;
      N_Bits = 8; width = inp.width; height = inp.height;
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
    } //************************** end ColorToGrayMC ***************************


    public int DeleteBit0(int nbyte)
    // If "this" is a 8 bit image, then sets the bits 0 and 1 of each pixel to 0.
    // If it is a 24 bit one, then sets the bit 0 of green and red chanels to 0.
    {
      for (int i = 0; i < width * height; i++)
        if (nbyte == 1)
          Grid[i] = (byte)(Grid[i] - (Grid[i] % 4));
        else
        {
          Grid[nbyte * i + 2] = (byte)(Grid[nbyte * i + 2] & 254);
          Grid[nbyte * i + 1] = (byte)(Grid[nbyte * i + 1] & 254);
        }
      return 1;
    } //********************* end DeleteBit0 ************************   



    public int SigmaNewM(CImage Inp, int hWind, int Toleranz, Form1 fm1)
    // Sigma filter with doubled calculation of the output values for gray value images.
    {
      N_Bits = 8; width = Inp.width; height = Inp.height;
      int gv, y1, yEnd, yStart;
      Grid = new byte[width * height * N_Bits / 8];
      int[] hist = new int[256];
      int yy = 1 + nLoop * height / denomProg;
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
          else prov = Inp.Grid[x + width * y];  // "prov" is the provisional average
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
      int yy = 1 + nLoop * height / denomProg;
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
              Sum[c] += gv * hist[c][gv]; nPixel[c] += hist[c][gv];
            }
            if (nPixel[c] > 0) Grid[c + 3 * x + 3 * width * y] = (byte)((Sum[c] + nPixel[c] / 2) / nPixel[c]);
            else Grid[c + 3 * x + 3 * width * y] = Inp.Grid[c + 3 * x + 3 * width * y];
          } //================ end for (c... ========================
        } //================== end for (int x... ======================
      } //==================== end for (int y... ========================
      return 1;
    } //********************** end SigmaColor **********************************


    public int SigmaSimpleUni(CImage Inp, int hWind, int Toleranz, Form1 fm1)
    // Simple sigma filter for both gray value and color images. 
    {
      int[] gvMin = new int[3], gvMax = new int[3], nPixel = new int[3], Sum = new int[3];
      int c;
      N_Bits = Inp.N_Bits;
      int nbyte = N_Bits / 8;
      int jump, Len = height, nStep = 20;
      if (Len > 2*nStep) jump = Len / nStep;
      else jump = 2;
      fm1.progressBar1.Step = 1;
      fm1.progressBar1.Value = 0;
      fm1.progressBar1.Visible = true;
      for (int y = 0; y < height; y++) // ==================================================
      {
        if ((y % jump) == jump - 1) fm1.progressBar1.PerformStep();
        int gv, y1, yStart = Math.Max(y - hWind, 0), yEnd = Math.Min(y + hWind, height - 1);
        for (int x = 0; x < width; x++) //===============================================
        {
          int x1, xStart = Math.Max(x - hWind, 0), xEndB = Math.Min(x + hWind, width - 1);
          for (c = 0; c < nbyte; c++)
          {
            Sum[c] = 0; nPixel[c] = 0;
            gvMin[c] = Math.Max(0, Inp.Grid[c + nbyte * (x + width * y)] - Toleranz);
            gvMax[c] = Math.Min(255, Inp.Grid[c + nbyte * (x + width * y)] + Toleranz);
          }
          for (y1 = yStart; y1 <= yEnd; y1++)
            for (x1 = xStart; x1 <= xEndB; x1++)
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


    public int ExtremVar(CImage Inp, int hWind, Form1 fm1)
    // Extrem filter for gray value images with variable window size of 2*hWind+1.
    {
      N_Bits = 8; width = Inp.width; height = Inp.height;
      int gv, y1, yEnd, yStart;
      Grid = new byte[width * height];
      int[] hist = new int[256];
      int jump, Len = height, nStep = 20;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (int y = 0; y < height; y++) // ==============================================
      {
        if ((y % jump) == jump - 1) fm1.progressBar1.PerformStep();
        yStart = Math.Max(y - hWind, 0);
        yEnd = Math.Min(y + hWind, height - 1);

        for (int x = 0; x < width; x++) //============================================
        {
          if (x == 0) //------------------------------------------------------------
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

          } //---------------- end if (x==0) ---------------------------------------
          int gvMin = 0, gvMax = 255;
          for (gv = gvMin; gv <= gvMax; gv++)
            if (hist[gv] > 0) { gvMin = gv; break; }
          for (gv = gvMax; gv >= 0; gv--)
            if (hist[gv] > 0) { gvMax = gv; break; }
          if (Inp.Grid[x + width * y] - gvMin < gvMax - Inp.Grid[x + width * y]) 
               Grid[x + width * y] = (byte)gvMin;
          else Grid[x + width * y] = (byte)gvMax;
        } //================== end for (int x... =====================================
      } //==================== end for (int y... =======================================
      return 1;
    } //********************** end ExtrmVar *********************************************


    public int ExtremVarColor(CImage Inp, int hWind, Form1 fm1)
    {	/* The extreme filter for 3 byte color images with variable hWind.
	    The filter finds in the (2*hWind+1)-neighbourhood of the actual pixel (x,y) the color "Color1" which has 
      the greatest euclidean difference fromm the color of the central pixel (x, y). Then it finds a Color2 
      which has the greatest euclidean differnce from Color1. Color1 is assigned to the output pixel if its 
      difference from the color of the central pixel of the input image is less than that
	    of Color2. Otherwise Color 2 is assigned. --*/
      int[] CenterColor = new int[3], Color = new int[3], Color1 = new int[3], Color2 = new int[3];
      int c, k, x; 
      int yy = 1 + nLoop * height / denomProg;
      for (int y = 0; y < height; y++) // =======================================
      {
        if (((y + 1) % yy) == 0) fm1.progressBar1.PerformStep();
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
          } //=================== end for (int k... ==============================
          MaxDist = -1;
          for (k = -hWind; k <= hWind; k++) //====================================
          {
            if (y + k >= 0 && y + k < height)
              for (int i = -hWind; i <= hWind; i++) //==========================
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
            //=============== end for (int i... ================================
          } //================ end for (int k... =================================
          int dist1 = 0, dist2 = 0;
          for (c = 0; c < 3; c++)
          {
            dist1 += (Color1[c] - CenterColor[c]) * (Color1[c] - CenterColor[c]);
            dist2 += (Color2[c] - CenterColor[c]) * (Color2[c] - CenterColor[c]);
          }
          if (dist1 < dist2) for (c = 0; c < 3; c++)
              Grid[c + 3 * x + y * width * 3] = (byte)Color1[c];
          else for (c = 0; c < 3; c++)
              Grid[c + 3 * x + y * width * 3] = (byte)Color2[c];

        } //================== end for (int x... ===================================
      } //==================== end for (int y... =====================================
      return 1;
    } //********************** end ExtremVarColor **************************************


    public int ExtremDifColor(CImage Inp, int hWind, Form1 fm1)
    {	/* The extreme filter for 3 byte color images with variable hWind.
	    The filter finds in the (2*hWind+1)-neighbourhood of the actual pixel (x,y) the color "Color1" which 
      is darker than the color of the central pixel (x, y) and has 
      the greatest absolut "ColorDifSign" from the color of the central pixel (x, y). Then it finds a Color2 
      which lighter than the color of the central pixel (x, y) and has the greatest absolut "ColorDifSign"
      from Color1. Color1 is assigned to the output pixel if its 
      difference from the color of the central pixel of the input image is less than that
	    of Color2. Otherwise Color 2 is assigned. --*/

      byte[] CenterColor = new byte[3], Color = new byte[3], Color1 = new byte[3], Color2 = new byte[3];
      int c, k, x; 
      int yy = 1 + nLoop * height / denomProg;
      for (int y = 0; y < height; y++) // =======================================
      {
        if (((y + 1) % yy) == 0) fm1.progressBar1.PerformStep();
        for (x = 0; x < width; x++) //==============================================
        {
          for (c = 0; c < 3; c++) Color2[c] = Color1[c] = Color[c] = CenterColor[c] = Inp.Grid[c + 3 * x + y * width * 3];
          int MinDist = 1000;
          for (k = -hWind; k <= hWind; k++) //=========================================
          {
            if (y + k >= 0 && y + k < height)
              for (int i = -hWind; i <= hWind; i++) //=====================================
              {
                if (x + i >= 0 && x + i < width) // && (i > 0 || k > 0))
                {
                  for (c = 0; c < 3; c++) Color[c] = Inp.Grid[c + 3 * (x + i) + (y + k) * width * 3];
                  int dist = ColorDifSign(Color, CenterColor);
 
                  if (dist < MinDist)
                  {
                    MinDist = dist;
                    for (c = 0; c < 3; c++) Color1[c] = Color[c];
                  }
                }
              } //=============== end for (int i... ============================
          } //=================== end for (int k... ==============================

          int MaxDist = -1;
          for (k = -hWind; k <= hWind; k++) //====================================
          {
            if (y + k >= 0 && y + k < height)
              for (int i = -hWind; i <= hWind; i++) //==========================
                if (x + i >= 0 && x + i < width) 
                {
                  for (c = 0; c < 3; c++) Color[c] = Inp.Grid[c + 3 * (x + i) + (y + k) * width * 3];
                  int dist = ColorDifSign(Color, Color1);
                  if (dist > MaxDist)
                  {
                    MaxDist = dist;
                    for (c = 0; c < 3; c++) Color2[c] = Color[c];
                  }
                }
            //=============== end for (int i... ================================
          } //================ end for (int k... =================================
          int dist1 = 0, dist2 = 0;
          dist1 = Math.Abs(ColorDifSign(Color1, CenterColor));
          dist2 = Math.Abs(ColorDifSign(Color1, CenterColor));
          if (dist1 < dist2) for (c = 0; c < 3; c++)
              Grid[c + 3 * x + y * width * 3] = Color1[c];
          else for (c = 0; c < 3; c++)
              Grid[c + 3 * x + y * width * 3] = Color2[c];

        } //================== end for (int x... ===================================
      } //==================== end for (int y... =====================================
      return 1;
    } //********************** end ExtremNewColor **************************************


    public int ExtremLightColor(CImage Inp, int hWind, int th, Form1 fm1)
    {	/* The extreme filter for 3 byte color images with variable hWind.
	    The filter finds in the (2*hWind+1)-neighbourhood of the actual pixel (x,y) the color "Color1" with 
      minimum and the color "Color2" with thge maximum lightness. "Color1" is assigned to the output pixel
      if its lightniss is closer to the lightness of the cetral pixel than the lightnesas of "Color2". --*/
      
      byte[] CenterColor = new byte[3], Color = new byte[3], Color1 = new byte[3], Color2 = new byte[3];
      int c, k, x; 

      int jump, Len = height, nStep = 20;
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
                if (x + i >= 0 && x + i < width) 
                {
                  for (c = 0; c < 3; c++) Color[c] = Inp.Grid[c + 3 * (x + i) + 3*(y +k) * width];
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
          if (dist2 - dist1 > th) 
            for (c = 0; c < 3; c++)
              Grid[c + 3 * x + y * width * 3] = Color1[c]; // Min
          else
            if (dist2 - dist1 < th && dist2 - dist1 > -th) 
              for (c = 0; c < 3; c++)
                Grid[c + 3 * x + y * width * 3] = CenterColor[c];
            else // dist2 - dist1 < -th
              for (c = 0; c < 3; c++)
                Grid[c + 3 * x + y * width * 3] = Color2[c]; // Max

        } //================== end for (int x... ===================================
      } //==================== end for (int y... =====================================
      return 1;
    } //********************** end ExtremLightColor **************************************


    public int ExtremVarColorExp(CImage Inp, int hWind1)
    {	/* The extreme filter for 3 byte color images with variable hWind.
	    The filter finds in the (2*hWind+1)-neighbourhood of the actual pixel (x,y) the color "Color1" which has 
      the greatest distance form the central color of (x, y). Then it finds a Color2 which lies opposite to 
      Color1 and has the greatest distance from Color1. Color1 is assigned to the output pixel if its 
      difference to the cental color is greater than that of Color2. Otherwise Color 2 is assigned. --*/
      int hWind = 1;
      int[] CenterColor = new int[3], Color = new int[3], Color1 = new int[3], Color2 = new int[3];
      int c, k, x, x1, x2, y, y1, y2; 

      x1 = y1 = -1;
      for (y = 0; y < height; y++) // ======================================================
      {
        for (x = 0; x < width; x++) //=====================================================
        {
          for (c = 0; c < 3; c++) Color2[c] = Color1[c] = Color[c] = CenterColor[c] = 
            Inp.Grid[c + 3 * x + y * width * 3];
          int MaxDist = -1;
          for (k = -hWind; k <= hWind; k++) //=============================================
          {
            if (y + k >= 0 && y + k < height)
              for (int i = -hWind; i <= hWind; i++) //====================================
              {
                if (x + i >= 0 && x + i < width && (i > 0 || k > 0)) //------------------
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
                    x1 = x + i; y1 = y + k; // location of Color1
                  }
                } //------------- end if (x + i >= 0  ----------------------------------- 
              } //=============== end for (int i... ======================================
          } //=================== end for (int k... ========================================
          MaxDist = -1;
          for (k = -hWind; k <= hWind; k++) //=========================================
          {
            if (y + k >= 0 && y + k < height)
              for (int i = -hWind; i <= hWind; i++) //===================================
                if (x + i >= 0 && x + i < width && (i > 0 || k > 0) && 
                  Math.Abs(x + i - x1) + Math.Abs(y + k - y1) > 2) //------------------
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
                    x2 = x + i; y2 = y + k;
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
    } //********************** end ExtremVarColorExp *************************************


    int ColorDifAbs(byte[] Colp, byte[] Colh)
    // Returns the sum of the absolut differences of the color components divided through 3.
    {
      int Dif = 0;
      for (int c = 0; c < 3; c++) Dif += Math.Abs(Colp[c] - Colh[c]);
      return Dif / 3;
    }

    int ColorDif(byte[] Colh, byte[] Colp)
    // Returns the sum of the differences of the color components.
    {
      int Dif = 0;
      for (int c = 0; c < 3; c++) Dif += (Colh[c] - Colp[c]);
      return Dif / 3;
    }

    public int LabelCells(int th, CImage Image3)
    /* Looks in "Image3" (standard coord.) for all pairs of adjacent pixels with color differences greater
	     than "th" and finds the maximum color differencew among adjacent pairs in the same line or in the same column.
       Labels the corresponding cracks in "this" (combinatorial coord.) with "1". The points 
       incident with the cracks are also provided with labels indicating the number and locations of 
       incident cracks. This method works both for color and gray value images. ---*/
    {
      int difH, difV, c, Lab = 1, maxDif, nByte, NXB = Image3.width, rv, val, x, y, x1, y1, xopt, yopt;
      int[] Mark = { 4, 8, 16, 32 }; // labels for points incident to labeled cracks
      if (Image3.N_Bits == 24) nByte = 3;
      else nByte = 1;
      for (x = 0; x < width * height; x++) Grid[x] = 0;

      byte[] Colorp = new byte[3], Colorh = new byte[3], Colorv = new byte[3];
      for (y = 1; y < height; y += 2)
      {
        for (x = 1; x < width; x += 2)
        {
          if (x >= 65 && x < 69 && y == 1 && false) MessageBox.Show("x=" + x + " y=" + y);
          if (x >= 3) //-------------- vertical cracks: Math.Abs.dif{(x/2, y/2)-((x-2)/2, y/2)} ---------------------------------
          {
            for (c = 0; c < nByte; c++)
            {
              Colorv[c] = Image3.Grid[c + nByte * ((x - 2) / 2) + nByte * NXB * (y / 2)];
              Colorp[c] = Image3.Grid[c + nByte * (x / 2) + nByte * NXB * (y / 2)]; ;
            }
            if (nByte == 3) difV = ColorDif(Colorp, Colorv);
            else difV = Math.Abs(Colorp[0] - Colorv[0]);
            if (x > 55 && x < 71 && y == 1 && false)
            {
              rv = MessReturn("x=" + x + " y=" + y + " difV=" + difV + " th=" + th);
              if (rv < 0) return -1;
            }
            if (difV > th)
            {
              maxDif = difV;
              xopt = x;
              for (x1 = x + 2; x1 < width; x1 += 2) //============================================
              {
                for (c = 0; c < nByte; c++)
                {
                  Colorv[c] = Image3.Grid[c + nByte * ((x1 - 2) / 2) + nByte * NXB * (y / 2)];
                  Colorp[c] = Image3.Grid[c + nByte * (x1 / 2) + nByte * NXB * (y / 2)]; ;
                }
                if (nByte == 3)
                  difV = ColorDif(Colorp, Colorv);
                else
                  difV = Math.Abs(Colorp[0] - Colorv[0]);
                if (difV > th)
                {
                  if (difV > maxDif)
                  {
                    maxDif = difV;
                    xopt = x1;
                  }
                }
                else break;
              } //============================ end for (x1 ... =====================================
              Grid[xopt - 1 + width * y] = (byte)Lab; // vertical crack
              val = Grid[xopt - 1 + width * (y - 1)] & 3;
              if (val < 3) val++; // point abow
              Grid[xopt - 1 + width * (y - 1)] &= 252;
              Grid[xopt - 1 + width * (y - 1)] |= (byte)(val | Mark[1]);  // now 8

              val = Grid[xopt - 1 + width * (y + 1)] & 3; // point below
              if (val < 3) val++;
              Grid[xopt - 1 + width * (y + 1)] &= 252;
              Grid[xopt - 1 + width * (y + 1)] |= (byte)(val | Mark[3]); // now 32
            } //-------------------------- end if (difV > th) -----------------------------------------
          } //---------------------------- end if (x>=3) ---------------------------------------------------------------

          if (y >= 3) //------------ horizontal cracks: Math.Abs.dif{(x/2, y/2)-(x/2, (y-2)/2)} ------------------------
          {
            for (c = 0; c < nByte; c++)
            {
              Colorh[c] = Image3.Grid[c + nByte * (x / 2) + nByte * NXB * ((y - 2) / 2)];
              Colorp[c] = Image3.Grid[c + nByte * (x / 2) + nByte * NXB * (y / 2)];
            }
            if (nByte == 3) difH = ColorDif(Colorp, Colorh);
            else difH = Math.Abs(Colorp[0] - Colorh[0]);
            if (difH > th)
            {
              maxDif = difH;
              yopt = y;
              for (y1 = y + 2; y1 < height; y1 += 2) //============================================
              {
                for (c = 0; c < nByte; c++)
                {
                  Colorh[c] = Image3.Grid[c + nByte * (x / 2) + nByte * NXB * ((y1 - 2) / 2)];
                  Colorp[c] = Image3.Grid[c + nByte * (x / 2) + nByte * NXB * (y1 / 2)]; ;
                }
                if (nByte == 3) difH = ColorDif(Colorp, Colorh);
                else difH = Math.Abs(Colorp[0] - Colorh[0]);
                if (difH > th)
                {
                  if (difH > maxDif)
                  {
                    maxDif = difH;
                    yopt = y1;
                  }
                }
                else break;
              } //============================ end for (y1 ... =====================================

              Grid[x + width * (yopt - 1)] = (byte)Lab; // horizontal crack
              val = Grid[x - 1 + width * (yopt - 1)] & 3;
              if (val < 3) val++; // left point 
              Grid[x - 1 + width * (yopt - 1)] &= 252;
              Grid[x - 1 + width * (yopt - 1)] |= (byte)(val | Mark[0]);  // now 4
              val = Grid[x + 1 + width * (yopt - 1)] & 3; // right point
              if (val < 3) val++;
              Grid[x + 1 + width * (yopt - 1)] &= 252;
              Grid[x + 1 + width * (yopt - 1)] |= (byte)(val | Mark[2]); // now 16
            }
          } //---------------------------- end if (y>=3) -------------------------------------
        } //============================== end for (x=1;... ====================================
      } //================================ end for (y=1;... ====================================
      return 1;
    } //******************************** end LabelCells *****************************************


    public void CracksToPixel(CImage Comb, Form1 fm1)
    {
      int NX = Comb.width;
      int NY = Comb.height;
      int nx = NX / 2;
      int ny = NY / 2;
      int dim; 
      for (int i = 0; i < nx * ny; i++) Grid[i] = 0;

      int jump, Len = NY - 2, nStep = 20;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;

      for (int y = 0; y < NY - 2; y++)
      {
        if (y % jump == jump - 1) fm1.progressBar1.PerformStep();
        for (int x = 0; x < NX - 2; x++)
        {
          dim = (x % 2) + (y % 2);
          if (dim == 1 && Comb.Grid[x + NX * y] > 0) Grid[x / 2 + nx * (y / 2)] = 255;
        }
      }
    } //*************************** end CracksToPixel **************************************


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


    public void DrawComb(int StandX, int StandY, Form1 fm1)
    // Draws in the pictureBox1 the cracks of the image "Comb".
    {
      if (StandX < 0) StandX = 0;
      if (StandY < 0) StandY = 0;
      int SizeX = 2 * 75, SizeY = 2 * 75, Step = 4;
      if (width < 50)
      {
        SizeX = width / 2; SizeY = height / 2;
      }
      Graphics g = fm1.pictureBox1.CreateGraphics();
      Pen whitePen; //, PenDelete;
      whitePen = new Pen(Color.White, 1);
      SolidBrush blackBrush, redBrush, greenBrush, yellowBrush, blueBrush;
      blackBrush = new SolidBrush(Color.Black);
      redBrush = new SolidBrush(Color.Red);
      greenBrush = new SolidBrush(Color.LightGreen);
      yellowBrush = new SolidBrush(Color.Yellow);
      blueBrush = new SolidBrush(Color.Blue);
      Rectangle rect, rect2, point, point2;
      rect = new Rectangle(0, 0, fm1.pictureBox1.Width, fm1.pictureBox1.Height);
      g.FillRectangle(blackBrush, rect);


      Graphics g2 = fm1.pictureBox2.CreateGraphics();
      int X = (int)(fm1.Scale1 * (StandX)) + fm1.marginX;
      int Y = (int)(fm1.Scale1 * (StandY)) + fm1.marginY;
      rect2 = new Rectangle(X, Y, (int)(fm1.Scale1 * (SizeX / 2)), (int)(fm1.Scale1 * (SizeY / 2)));
      MessageBox.Show("DrawComb: left upper corner in Comb=(" + 2 * StandX + ";" + 2 * StandY + ")");
      g2.DrawRectangle(whitePen, rect2);

      int xpB, ypB;
      int NX = Math.Min(width, 2 * (StandX + SizeX));
      int NY = Math.Min(height, 2 * (StandY + SizeY));
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
              g.DrawLine(whitePen, (xpB - 1) * Step + 1, ypB * Step, (xpB + 1) * Step - 1, ypB * Step); // crack

              if ((Grid[x - 1 + width * y] & 7) > 0) // left point
              {
                point = new Rectangle((xpB - 1) * Step - 1, ypB * Step - 1, 2, 2);
                point2 = new Rectangle((xpB - 1) * Step - 1, ypB * Step - 1, 3, 3);
                switch (Grid[x - 1 + width * y] & 7)
                {
                  case 1: g.FillRectangle(redBrush, point); break;
                  case 2: g.FillRectangle(greenBrush, point); break;
                  case 3: g.FillRectangle(yellowBrush, point); break;
                  case 4: g.FillRectangle(blueBrush, point2); break;
                }
              }

              if ((Grid[x + 1 + width * y] & 7) > 0) // right point
              {
                point = new Rectangle((xpB + 1) * Step - 1, ypB * Step - 1, 2, 2);
                point2 = new Rectangle((xpB + 1) * Step - 1, ypB * Step - 1, 3, 3);
                switch (Grid[x + 1 + width * y] & 7)
                {
                  case 1: g.FillRectangle(redBrush, point); break;
                  case 2: g.FillRectangle(greenBrush, point); break;
                  case 3: g.FillRectangle(yellowBrush, point); break;
                  case 4: g.FillRectangle(blueBrush, point2); break;
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
              if ((Grid[x + width * (y - 1)] & 7) > 0) // upper point
              {
                point = new Rectangle(xpB * Step - 1, (ypB - 1) * Step - 1, 2, 2); // upper point
                point2 = new Rectangle(xpB * Step - 1, (ypB - 1) * Step - 1, 3, 3); // upper point
                switch (Grid[x + width * (y - 1)] & 7)
                {
                  case 1: g.FillRectangle(redBrush, point); break;
                  case 2: g.FillRectangle(greenBrush, point); break;
                  case 3: g.FillRectangle(yellowBrush, point); break;
                  case 4: g.FillRectangle(blueBrush, point2); break;
                }
              }

              if ((Grid[x + width * (y + 1)] & 7) > 0) // lower point
              {
                point = new Rectangle(xpB * Step - 1, (ypB + 1) * Step - 1, 2, 2); // lower point
                point2 = new Rectangle(xpB * Step - 1, (ypB + 1) * Step - 1, 3, 3); // lower point
                switch (Grid[x + width * (y + 1)] & 7)
                {
                  case 1: g.FillRectangle(redBrush, point); break;
                  case 2: g.FillRectangle(greenBrush, point); break;
                  case 3: g.FillRectangle(yellowBrush, point); break;
                  case 4: g.FillRectangle(blueBrush, point2); break;
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
      int difH, difV, c, maxDif, minDif, nByte, NXB = Image3.width, x, y, xopt, yopt;
      int Inp, State, Contr, xStartP, xStartM, yStartP, yStartM;
      if (Image3.N_Bits == 24) nByte = 3;
      else nByte = 1;
      for (x = 0; x < width * height; x++) Grid[x] = 0;
      byte[] Colorp = new byte[3], Colorh = new byte[3], Colorv = new byte[3];
      maxDif = 0; minDif = 0;
      int jump, Len = height / 2, nStep = 10;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (y = 3; y < height; y += 2) //================ vertical cracks =====================
      {
        if (y % jump == jump - 1) fm1.progressBar1.PerformStep();
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
              Grid[x - 1 + width * (yopt - 1)]++;  // left point
              Grid[x + 1 + width * (yopt - 1)]++;  // right point
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


    public int Dist(int P, Point EP)
    {
      int PY = P / width;
      int PX = P - PY * width;
      return Math.Abs(PX - EP.X) + Math.Abs(PX - EP.X);
    }


    public int Trace(byte[] GridCopy, int P, int[] Index, ref int SumCells, ref int Pterm, ref int dir, 
      int Size, bool SPEC, int maxDist, Point EP)
    /* Traces a branch of a component, saves all cells in "Index"; "SumCells" is the number of saved cells.
     * If "SumCells" becomes greater than "Size", the tracing is interrupted, and the method returns -1.
     * Otherwise it returns a positive number. --*/
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
        Lab = GridCopy[P] & Mask;

        if (SPEC)
        {
          if (Dist(P, EP) < maxDist)
          {
            Index[SumCells] = P;
            if (Lab == 2) SumCells++;
          }
        }
        else
        {
          if (SumCells < Size) Index[SumCells] = P;
          if (Lab == 2) SumCells++;
        }

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



    public int ComponClean(byte[] GridCopy, int X, int Y, int[] Index, int Size, bool SPEC, int maxDist, Point EP) 
    /* Traces a component starting at (X, Y), saves coordinates of cells in "Index", "SumCells" is the
     * number of traced cells. If "SumCells" becomes greater than "Size", then the saving of cells is interrupted
     * but the counting in "SumCells" goes on.
     * --*/
    {
      int dir, dirT;
      int LabNext, Mask = 7, rv;
      int Crack, P, Pinp, Pnext, Pterm = -1; // Attention! This are the indices in "Comb"
      int[] Step = { 1, width, -1, -width }; // one step in direction"dir"
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
            LabNext = GridCopy[Pnext] & Mask; 
            if (LabNext == 3 || LabNext == 4) pQ.Put(Pnext);
            if (LabNext == 2) //-------------------------------------------------------------------------
            {
              dirT = dir; 
              rv = Trace(GridCopy, P, Index, ref SumCells, ref Pterm, ref dirT, Size, SPEC, maxDist, EP);

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
      int cntSingles, cntCracks, x1, y1, x, y;
      int[] Index = new int[10000];
      int SumCells = 0, Lab = 0; 

      // Transforming small squares to corners:
      for (y = 2; y < height - 2; y += 2) //===================================================================
      {
        for (x = 2; x < width - 2; x += 2) //===================== over points ==============================
        {
          Lab = Grid[x + width * y]; 
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
        } //================================== end for (x... ========================
      } //==================================== end for (y... ==========================
      byte[] GridCopy = new byte[width * height];
      for (int i = 0; i < width * height; i++) GridCopy[i] = Grid[i];

      for (int  i = 0; i < width * height; i++) GridCopy[i] = Grid[i];

      SumCells = 0;

      // Deleting connected components of edge which contain end or brunch points and 
      // are shorter than "Size"
      Point EP = new Point(0, 0);

      for (y = 0; y < height; y += 2)
      {
        for (x = 0; x < width; x += 2) //=========================================
        {
          Lab = GridCopy[x + width * y] & (Mask | 128);
          if (Lab == 1 || Lab == 3 || Lab == 4) //----------------------------------
          {
            SumCells = ComponClean(GridCopy, x, y, Index, Size, false, 20, EP);
             if (SumCells < 0) return -1;
            if (SumCells < Size)
            {
              for (int  i = 0; i < SumCells; i++) Grid[Index[i]] = 0;
            } //-------------------- end if (SumCells <= Size) -----------------------------------
          } //---------------------- end if (if (Lab == 1 || Lab == 3 || Lab == 4) ------------------
        } //======================== end for (x ... ===============================================
      } //========================== end for (x ... ===============================================

      // Deleting small components containing no end points and no brunchings
      SumCells = 0;
      for (y = 0; y < height; y += 2)
      {
        for (x = 0; x < width; x += 2) //=========================================
        {
          Lab = GridCopy[x + width * y] & Mask;
          if (Lab == 2) //----------------------------------
          {
            SumCells = ComponClean(GridCopy, x, y, Index, Size, false, 20, EP);
            if (SumCells < Size)
            {
              for (int  i = 0; i < SumCells; i++) Grid[Index[i]] = 0;
            }
          } //---------------------- end if (Lab == 2) -------------------------------------------

        } //========================= end for (x ... ==============================================
      } //=========================== end for (y ... ================================================

      return 1;
    } //************************************** end CleanCombNew ********************************************



    public int FastAverageM(CImage Inp, int hWind, Form1 fm1)
    // Filters the gray scale image "Inp" and returns the result in 'Grid' of the
    // calling image.
    {
      if (Inp.N_Bits != 8)
      {
        MessageBox.Show("FastAverageM cannot process an image with " + Inp.N_Bits +
          " bits per pixel");
        return -1;
      }
      width = Inp.width; height = Inp.height;
      Grid = new byte[width * height];
      int[] SumColmn; int[] nPixColmn;
      SumColmn = new int[width];
      nPixColmn = new int[width];
      for (int i = 0; i < width; i++) SumColmn[i] = nPixColmn[i] = 0;

      int nPixWind = 0, SumWind = 0;
      int y2 = 1 + height / 100;
      for (int y = 0; y < height + hWind; y++) //==========================================
      {
        if (y % y2 == 1) fm1.progressBar1.PerformStep();
        int yout = y - hWind, ysub = y - 2 * hWind - 1;
        SumWind = 0; nPixWind = 0;

        int y1 = 1 + (height + hWind) / 100;
        for (int x = 0; x < width + hWind; x++) //=======================================
        {
          int xout = x - hWind, xsub = x - 2 * hWind - 1;	// 1. and 2. addition
          if (y < height && x < width)
          {
            SumColmn[x] += Inp.Grid[x + width * y];
            nPixColmn[x]++; 	// 3. and 4. addition
          }
          if (ysub >= 0 && x < width)
          {
            SumColmn[x] -= Inp.Grid[x + width * ysub];
            nPixColmn[x]--;
          }
          if (yout >= 0 && x < width)
          {
            SumWind += SumColmn[x];
            nPixWind += nPixColmn[x];
          }
          if (yout >= 0 && xsub >= 0)
          {
            SumWind -= SumColmn[xsub];
            nPixWind -= nPixColmn[xsub];
          }
          if (xout >= 0 && yout >= 0)
            Grid[xout + width * yout] = (byte)((SumWind + nPixWind / 2) / nPixWind);
        } //============================= end for (int x = 0; ===========================
      } //=============================== end for (int x = 0; =============================
      return 1;
    } //********************************* end FastAverageM **********************************


    public int FastAverageUni(CImage Inp, int hWind)
    // Filters the color or grayscale image "Inp" and returns the result in
    // 'Grid' of the calling image. Each of the three color channels is being averaged.
    {
      int c = 0, nByte = 0;
      if (Inp.N_Bits == 8) nByte = 1;
      else nByte = 3;
      width = Inp.width; height = Inp.height;
      Grid = new byte[nByte * width * height];
      int[] nPixColmn;  
      nPixColmn = new int[width];
      for (int i = 0; i < width; i++) nPixColmn[i] = 0;

      int[,] SumColmn;
      SumColmn = new int[width, 3];

      int nPixWind = 0, xout = 0, xsub = 0;
      int[] SumWind = new int[3];

      for (int y = 0; y < height + hWind; y++) //================================================
      {
        int yout = y - hWind, ysub = y - 2 * hWind - 1;
        nPixWind = 0;
        for (c = 0; c < nByte; c++) SumWind[c] = 0;

        int y1 = 1 + (height + hWind) / 100;
        for (int x = 0; x < width + hWind; x++) //============================================
        {
          xout = x - hWind;
          xsub = x - 2 * hWind - 1;	// 1. and 2. addition

          if (y < height && x < width) // 3. and 4. addition
          {
            for (c = 0; c < nByte; c++) SumColmn[x, c] += Inp.Grid[c + nByte * (x + width * y)];
            nPixColmn[x]++;
          }
          if (ysub >= 0 && x < width)
          {
            for (c = 0; c < nByte; c++) SumColmn[x, c] -= Inp.Grid[c + nByte * (x + width * ysub)];
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
              Grid[c + nByte * (xout + width * yout)] =
                            (byte)((SumWind[c] + nPixWind / 2) / nPixWind);

        } //============================ end for (int x = 0;  =================================
      } //============================== end for (int y = 0;  ===================================
      return 1;
    } //******************************** end FastAverageUni ***************************************


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



    public void DrawImageLine(int Y, int xStart, int th, CImage Sigma, byte[] Grid2, Form1 fm1)
    // This is a method of "ExtremIm".
    {
      Graphics g1 = fm1.pictureBox1.CreateGraphics();
      Graphics g2 = fm1.pictureBox2.CreateGraphics();
      Graphics g3 = fm1.pictureBox3.CreateGraphics();
      Pen whitePen = new Pen(Color.White);
      Pen redPen = new Pen(Color.Red);
      Pen thickRedPen = new Pen(Color.Red, 4);
      Pen greenPen = new Pen(Color.LightGreen);
      Pen thickBluePen = new Pen(Color.LightGreen, 4);
      SolidBrush blackBrush = new SolidBrush(Color.Black);
      Rectangle rect = new Rectangle(0, 0, fm1.pictureBox3.Width, 300);
      int divider, xEnd, Step = 4, length = fm1.pictureBox3.Width / Step, light1 = 0, light2 = 0, x1,
        y01 = 120, y02 = 240, y1, y;

      int nByte = N_Bits / 8;
      if (nByte == 3) divider = 8;
      else divider = 3;
      MessageBox.Show("Line " + Y + " starting with xStart=" + xStart);
      g3.FillRectangle(blackBrush, rect);
      if (xStart + length < width) xEnd = xStart + length;
      else xEnd = width;
      int Step2 = Step / 2;

      for (int c = 0; c < 3; c++) light1 += Grid[c + nByte * (xStart + width * Y)];
      x1 = 0;
      y1 = y01 - light1 / divider;
      g3.DrawLine(whitePen, 20, y01, 20, y01 - th / divider);
      g3.DrawLine(whitePen, 0, y01, xEnd * Step, y01);
      for (int x = xStart + 1; x < xEnd; x++) //===============================================
      {
        x1 = (x - xStart);
        light2 = 0;
        for (int c = 0; c < nByte; c++) light2 += Grid[c + nByte * (x + width * Y)];
        y = y01 - light2 / divider;
        g3.DrawLine(whitePen, (x1 - 1) * Step, y1, x1 * Step - 1, y1);

        if (light2 - light1 > th)
        {
          if (Grid2[2 * x + (2 * width + 1) * (2 * Y + 1)] > 0)
            g3.DrawLine(thickRedPen, x1 * Step, y1, x1 * Step, y);
          else
            g3.DrawLine(redPen, x1 * Step, y1, x1 * Step, y);
        }
        else
        {
          if (light2 - light1 > -th && light2 - light1 < th)
            g3.DrawLine(whitePen, x1 * Step, y1, x1 * Step, y);
          else
          {
            if (Grid2[2 * x + (2 * width + 1) * (2 * Y + 1)] > 0)
              g3.DrawLine(thickBluePen, x1 * Step, y1, x1 * Step, y);
            else
              g3.DrawLine(greenPen, x1 * Step, y1, x1 * Step, y);
          }
        }
        light1 = light2;
        x1 = x;
        y1 = y;
      }

      // Drawing the curve of "SigmaIm":
      int nByteSigma = Sigma.N_Bits / 8;
      for (int c = 0; c < nByteSigma; c++) light1 += Sigma.Grid[c + nByteSigma * (xStart + width * Y)];
      x1 = 0;
      y1 = y02; // -light1 / divider;
      g3.DrawLine(whitePen, 0, y02, xEnd * Step, y02);
      for (int x = xStart + 1; x < xEnd; x++) //=============================
      {
        //x1 = (int)((x - xStart) * fm1.Scale1 * 0.8);
        x1 = x - xStart;
        light1 = 0;
        for (int c = 0; c < nByteSigma; c++) light1 += Sigma.Grid[c + nByteSigma * (x + width * Y)];
        y = y02 - light1 / divider;
        g3.DrawLine(whitePen, (x1 - 1) * Step, y1, x1 * Step - 1, y1);
        g3.DrawLine(whitePen, x1 * Step, y1, x1 * Step, y);
        x1 = x;
        y1 = y;
      }  //============================= end for (int x ... =================
      int xx = (int)(xStart * fm1.Scale1) + fm1.marginX;
      int yy = (int)(Y * fm1.Scale1) + fm1.marginY;
      int ex = (int)(xEnd * fm1.Scale1) + fm1.marginX;
      //int ey = (int)(Y * fm1.Scale1) + fm1.marginY;
      g1.DrawLine(greenPen, xx, yy, ex, yy);
      g2.DrawLine(greenPen, xx, yy, ex, yy);

    } //******************************** end DrawImageLine ***********************



    public void DrawImageLineED(int Y, int xStart, int th, CImage Sigma, byte[] Grid2, Form1 fm1)
    // This is a method of "ExtremIm".
    {
      Graphics g1 = fm1.g1; // pictureBox1.CreateGraphics(); // for drawing the tested line in pictBox1
      Graphics g2 = fm1.g2; // pictureBox2.CreateGraphics(); // for drawing the tested line in pictBox2
      Graphics g3 = fm1.g3; // pictureBox3.CreateGraphics(); // for drawing the curves
      Pen whitePen = new Pen(Color.White);
      Pen redPen = new Pen(Color.Red, 2);
      Pen thickRedPen = new Pen(Color.Red, 4);
      Pen greenPen = new Pen(Color.LightGreen, 2);
      Pen thickGreenPen = new Pen(Color.LightGreen, 4);
      SolidBrush blackBrush = new SolidBrush(Color.Black);
      Rectangle rect = new Rectangle(0, 0, 1224, 279);
      g3.FillRectangle(blackBrush, rect);
      int divider = 3, xEndB, length, light1 = 0, light2 = 0, Step = 2, x1, 
        y01 = 120, y02 = 240, y1, y;
      length = fm1.pictureBox3.Width / Step;
      int nByte = N_Bits / 8;
      int c = 0, Step2 = Step / 2;
      byte[] Colorp = new byte[3], ColorOld = new byte[3];
      MessageBox.Show("BmpGraph=" + fm1.BmpGraph + " length=" + length + " Step=" + Step);

      if (fm1.OrigIm.N_Bits == 24) fm1.GridToBitmap(fm1.BmpPictBox1, fm1.OrigIm.Grid);
      else fm1.GridToBitmapOld(fm1.BmpPictBox1, fm1.OrigIm.Grid);
      fm1.pictureBox1.Image = fm1.BmpPictBox1;

      int lengthB, xStartB, YB;
      if (fm1.BmpGraph)
      {
        lengthB = (int)(length / fm1.Scale1);
        xStartB = (int)((xStart - fm1.marginX) / fm1.Scale1);
        YB = (int)((Y - fm1.marginY) / fm1.Scale1);
      }
      else
      {
        lengthB = length;
        xStartB = xStart;
        YB = Y;
      }
      if (xStartB + lengthB < width) xEndB = xStartB + lengthB;
      else xEndB = (int)(width / fm1.Scale1);

      // lightness of "ExtremIm":
      light1 = MaxC(Grid[c + nByte * (xStartB + width * YB)],
        Grid[c + nByte * (xStartB + width * YB)], Grid[c + nByte * (xStartB + width * YB)]);
      
      for (c = 0; c < 3; c++)  ColorOld[c] = Grid[c + nByte * (xStartB + width * YB)];
      
      x1 = 0;
      y1 = y01 - light1 / divider;

      g3.DrawLine(whitePen, 20, y01, 20, y01 - th / 3 / divider); 
      g3.DrawLine(whitePen, 0, y01, xEndB * Step, y01); // level lightness = 0

      // Drawing the curve of "ExtremIm":
      int Dif = 0;
      for (int x = xStartB + 1; x < xEndB; x++) //===============================================
      {
        x1 = x - xStartB;
        light2 = MaxC(Grid[2 + nByte * (x + width * YB)], Grid[1 + nByte * (x + width * YB)],
          Grid[0 + nByte * (x + width * YB)]);
        
        for (c = 0; c < 3; c++)   Colorp[c] = Grid[c + nByte * (x + width * YB)];
       
        y = y01 - light2 / divider;
        g3.DrawLine(whitePen, (x1 - 1) * Step, y1, x1 * Step - 1, y1); // y1 is old value of "y"
        if (ColorDifSign(Colorp, ColorOld) > th) 
        {
          if (Grid2[2 * x + (2 * width + 1) * (2 * YB + 1)] > 0)
            g3.DrawLine(thickRedPen, x1 * Step, y1, x1 * Step, y);
          else
            g3.DrawLine(redPen, x1 * Step, y1, x1 * Step, y);
        }
        else
        {
          Dif = ColorDifSign(Colorp, ColorOld);
          if (Dif > -th && Dif < th)
              g3.DrawLine(whitePen, x1 * Step, y1, x1 * Step, y);
          else // Dif < -th
          {
            if (Grid2[2 * x + (2 * width + 1) * (2 * YB + 1)] > 0)
              g3.DrawLine(thickGreenPen, x1 * Step, y1, x1 * Step, y);
            else
              g3.DrawLine(greenPen, x1 * Step, y1, x1 * Step, y);
          }
        }
        x1 = x;
        y1 = y;
        for (c = 0; c < 3; c++) ColorOld[c] = Colorp[c];
      } //============================ end for ( int x ... ===========================

      // Drawing the curve of "SigmaIm":
      int nByteSigma = Sigma.N_Bits / 8;
      x1 = 0;
      y1 = y02; 
      g3.DrawLine(greenPen, 0, y02, xEndB * Step, y02); // The level "y02"
      
      for (int x = xStartB + 1; x < xEndB; x++) //=============================
      {
        x1 = x - xStartB;
        light1 = 0;
        if (fm1.BmpGraph) light1 = MaxC(Sigma.Grid[2 + nByteSigma * (x + width * YB)],
          Sigma.Grid[1 + nByteSigma * (x + width * YB)], Sigma.Grid[0 + nByteSigma * (x + width * YB)]);
        else light1 = Sigma.Grid[x + width * YB];
        y = y02 - light1 / divider;
        g3.DrawLine(greenPen, (x1 - 1) * Step, y1, x1 * Step - 1, y1);
        g3.DrawLine(greenPen, x1 * Step, y1, x1 * Step, y);
        x1 = x;
        y1 = y;
      }  //============================= end for (int x ... =================
      int xx = xStart;
      int yy;
      if (fm1.BmpGraph) yy = (int)((Y - fm1.marginY) / fm1.Scale1);
      else yy = Y;
      int ex = Math.Min(xStart + length, width);
      g1.DrawLine(greenPen, xx, yy, ex, yy);
      g2.DrawLine(greenPen, xx, yy, ex, yy);
      if (fm1.BmpGraph)
      {
        fm1.pictureBox1.Image = fm1.BmpPictBox1;
        fm1.pictureBox2.Image = fm1.BmpPictBox2;
        fm1.pictureBox3.Image = fm1.BmpPictBox3;
      }
    } //******************************** end DrawImageLine ***********************

  }
}
