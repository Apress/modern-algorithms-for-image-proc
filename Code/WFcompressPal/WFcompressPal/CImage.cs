using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace WFcompressPal
{

  public class CQueInd
  {
    public
      int input, output, Len;
    bool full;
    int[] Array;
    public CQueInd() { } // Default constructor
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
      width = nx;
      height = ny;
      N_Bits = nbits;
      Grid = new byte[width * height * (N_Bits / 8)];
    }

    public CImage(int nx, int ny, int nbits, byte[] img) // constructor
    {
      width = nx;
      height = ny;
      N_Bits = nbits;
      denomProg = 100;
      Grid = new byte[width * height * (N_Bits / 8)];
      for (int i = 0; i < width * height * (N_Bits / 8); i++) Grid[i] = img[i];
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

    public int SigmaSimpleUni(CImage Inp, int hWind, int Toleranz, Form1 fm1)
    // Simple sigma filter for both gray value and color images. 
    {
      int[] gvMin = new int[3], gvMax = new int[3], nPix0 = new int[3], Sum = new int[3];
      int c;
      N_Bits = Inp.N_Bits;
      int nbyte = N_Bits / 8;
      fm1.progressBar1.Visible = true;
      fm1.progressBar1.Step = 1;
      fm1.progressBar1.Maximum = 100;
      int jump;
      if (height > 300) jump = height / (100 / 5);
      else jump = 3;
      for (int y = 0; y < height; y++) // ==================================================
      {
        if (y % jump == jump - 1) fm1.progressBar1.PerformStep();
        int gv, y1, yStart = Math.Max(y - hWind, 0), yEnd = Math.Min(y + hWind, height - 1);
        for (int x = 0; x < width; x++) //===============================================
        {
          int x1, xStart = Math.Max(x - hWind, 0), xEnd = Math.Min(x + hWind, width - 1);
          for (c = 0; c < nbyte; c++)
          {
            Sum[c] = 0; nPix0[c] = 0;
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
                  nPix0[c]++;
                }
              }
          int index = nbyte * (x + width * y);
          byte value; 
          for (c = 0; c < nbyte; c++)
          {
            value = (byte)((Sum[c] + nPix0[c] / 2) / nPix0[c]);
            if (nPix0[c] > 0)
              Grid[c + index] = value;
            else Grid[c + nbyte * (x + width * y)] = Inp.Grid[c + nbyte * (x + width * y)];
          }
        } //================== end for (int x... =================================
      } //==================== end for (int y... ===================================
      return 1;
    } //********************** end SigmaSimpleUni **********************************


    public int ExtremVar(CImage Inp, int hWind, Form1 fm1)
    // Extrem filter for grayscale images with variable window size of 2*hWind + 1.
    {
      N_Bits = 8; width = Inp.width; height = Inp.height;
      int gv, y1, yEnd, yStart;
      Grid = new byte[width * height];
      int[] hist = new int[256];
      fm1.progressBar1.Maximum = 100;
      fm1.progressBar1.Value = 0;
      fm1.progressBar1.Visible = true; ;
      int jump;
      if (height > 300) jump = height / 20;
      else jump = 2;
      for (int y = 0; y < height; y++) // =======================================
      {
        if ((y % jump) == jump - 1) fm1.progressBar1.PerformStep();
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


    public int ExtremVarColor(CImage Inp, int hWind, Form1 fm1)
    {	/* The extreme filter for 3 byte color images with variable hWind.
	    The filter finds in the (2*hWind+1)-neighbourhood of the actual pixel (x,y) the color "Color1" which has the greatest 
	    difference form the central color of (x, y). Then it finds a Color2 which has the greatest differnce from
	    Color1. Color1 is assigned to the output pixel if its difference to the cental color is greater than that
	    of Color2. Otherwise Color 2 is assigned. --*/
      int[] CenterColor = new int[3], Color = new int[3], Color1 = new int[3], Color2 = new int[3];
      int c, k, x, y; //, xx=3, yy=1;
      fm1.progressBar1.Maximum = 100;
      fm1.progressBar1.Step = 1;
      fm1.progressBar1.Visible = true; 
      int jump;
      if (height > 300) jump = height / 20;
      else jump = 2;
      for (y = 0; y < height; y++) // =======================================
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
                if (x + i >= 0 && x + i < width) // && (i > 0 || k > 0))
                {
                  int dist = 0;
                  for (c = 0; c < 3; c++)
                  {
                    Color[c] = Inp.Grid[c + 3 * (x + i) + (y + k) * width * 3];
                    dist += (Color[c] - CenterColor[c]) * (Color[c] - CenterColor[c]);
                  }

                  if (dist > MaxDist)
                  {
                    for (c = 0; c < 3; c++) Color1[c] = Color[c];
                    MaxDist = dist;
                  }
                }
              } //=============== end for (int i... ============================
          } //================ end for (int k... ==============================
          MaxDist = -1;
          for (k = -hWind; k <= hWind; k++) //=========================================
          {
            if (y + k >= 0 && y + k < height)
              for (int i = -hWind; i <= hWind; i++) //=====================================
                if (x + i >= 0 && x + i < width) // && (i > 0 || k > 0))
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


    public int MakePalette(CImage Img, int[] Palet, Form1 fm1)
    // Produces a palette of up to 255 elements optimally representig the colors of the image "Img"
    {
      int MaxN = 1000, ResIndex = 11000;
      int[] Sum = new int[3 * MaxN];
      int[] nPix = new int[MaxN]; // "nPix[Index]" is number of pixels in "Img" whose color has the index
      double[] Mean = new double[3 * MaxN];
      int[] Sum0 = new int[3 * ResIndex];
      int[] nPix0 = new int[ResIndex];
      int c, i, jump, nIndex, nIndexOld, n, MaxIndex;
      int[] iChan = new int[3], Div = new int[3], Weight = new int[3];
      int[] NumbInterv = { 11, 12, 9 };

      // Computing minimum and maximum of the color channels:
      int[] Min = { 256, 256, 256 }, Max = { 0, 0, 0 };
      fm1.progressBar1.Visible = true;
      fm1.progressBar1.Step = 1;
      if (Img.width * Img.height > 300) jump = Img.width * Img.height / (100 / 11);
      else jump = 2;
      for (i = 0; i < Img.width * Img.height; i++) //=============================
      {
        if (i % jump == jump - 1) fm1.progressBar1.PerformStep();
        for (c = 0; c < 3; c++)
        {
          if (Img.Grid[3 * i + c] < Min[c]) Min[c] = Img.Grid[3 * i + c];
          if (Img.Grid[3 * i + c] > Max[c]) Max[c] = Img.Grid[3 * i + c];
        }
      } //=============================== end for (i... =============
      int nIndexMin = 218, nIndexMax = 255;
      nIndex = nIndexOld = 0;

      // Changing NumbInterv[] to get nIndex between nIndexMin and nIndexMax:
      do //===================================================================
      {
        Weight[0] = 1;
        Weight[1] = NumbInterv[0] + 1;
        Weight[2] = NumbInterv[0] + Weight[1] * NumbInterv[1] + 1;

        for (i = 0; i < ResIndex; i++) Sum0[i] = 0;

        for (i = 0; i < 3 * MaxN; i++)
        {
          Sum[i] = 0;
          Mean[i] = 0.0;
          nPix[i / 3] = 0;

        }
        for (c = 0; c < 3; c++)
        {
          Div[c] = (int)(0.5 + (double)(Max[c] - Min[c]) / (double)NumbInterv[c]);
          if (Div[c] == 0) Div[c] = 1;
          if ((Max[c] - Min[c]) / Div[c] > NumbInterv[c]) NumbInterv[c]++;
        }
        MaxIndex = Weight[0] * NumbInterv[0] + Weight[1] * NumbInterv[1] + Weight[2] * NumbInterv[2];
        
        int maxIndex = 0;
        if (MaxIndex >= ResIndex)
        {
          MessageBox.Show("MakePalette, Overflow: MaxIndex=" + MaxIndex + " > ResIndex=" + ResIndex
          + "; return -1.");
          return -1;
        }
        for (i = 0; i < ResIndex; i++) nPix0[i] = 0;

        int Index = 0;
        for (i = 0; i < Img.width * Img.height; i++) //====================================
        {
          Index = 0;
          for (c = 0; c < 3; c++)
          {
            iChan[c] = (Img.Grid[3 * i + c] - Min[c]) / Div[c];
            Index += Weight[c] * iChan[c];
          }
          if (Index > maxIndex) maxIndex = Index;

          if (Index > ResIndex - 1)
          {
            MessageBox.Show("MakePalette, Overflow: Index=" + Index + " is too great; return -1.");
            return -1;
          }
          for (c = 0; c < 3; c++) Sum0[3 * Index + c] += Img.Grid[3 * i + c];

          nPix0[Index]++;
        } //====================== end for (i = 0; ... =============================================

        nIndex = 0;
        for (i = 0; i <= MaxIndex; i++) if (nPix0[i] > 0) nIndex++;

        if (nIndexOld > 0 && (nIndex < nIndexMin && nIndexOld > nIndexMax || nIndex > nIndexMax && nIndexOld < nIndexMin))
        {
          if (MessReturn("MakePalette: Alarm! The value nIndexMin=" + nIndexMin + 
            " is too high. return -1.") < 0) return -1;
          return -1;
        }
        int minInd = 0, maxInd = 0;
        if (nIndex < nIndexMin)
        {
          if (NumbInterv[0] <= NumbInterv[1] && NumbInterv[0] <= NumbInterv[2]) minInd = 0;
          else
            if (NumbInterv[1] <= NumbInterv[0] && NumbInterv[1] <= NumbInterv[2]) minInd = 1;
            else
              if (NumbInterv[2] <= NumbInterv[0] && NumbInterv[2] <= NumbInterv[1]) minInd = 2;
          NumbInterv[minInd]++;
        }
           
        if (nIndex > nIndexMax)
        {
          if (NumbInterv[0] >= NumbInterv[1] && NumbInterv[0] >= NumbInterv[2]) maxInd = 0;
          else
            if (NumbInterv[1] >= NumbInterv[0] && NumbInterv[1] >= NumbInterv[2]) maxInd = 1;
            else
              if (NumbInterv[2] >= NumbInterv[0] && NumbInterv[2] >= NumbInterv[1]) maxInd = 2;
          NumbInterv[maxInd]--;
        }

        if (nIndex >= nIndexMin && nIndex <= nIndexMax || NumbInterv[1] > 20) break;
        nIndexOld = nIndex;
      } while (nIndex > nIndexMax || nIndex < nIndexMin); //==================================================

      int[] NewIndex = new int[MaxIndex];

      if (MaxIndex > 300) jump = MaxIndex / (100 / 11);
      else jump = 3;
      for (i = n = 0; i < MaxIndex; i++) //===================================================================
      {
        if (i % jump == jump - 1) fm1.progressBar1.PerformStep();
        if (nPix0[i] > 0) //---------------------------------------------------------------------------
        {
          n++;
          if (n > MaxN - 1)
          {
            MessageBox.Show("MakePalette: Overflow in Sum; n=" + n + "< MaxN=" + MaxN + ". return -1");
            return -1;
          }
          NewIndex[i] = n;
          nPix[n] = nPix0[i];
          for (c = 0; c < 3; c++)
          {
            Sum[3 * n + c] = Sum0[3 * i + c];
            if (nPix[n] > 0) Mean[3 * n + c] = (double)(Sum[3 * n + c]) / (double)(nPix[n]);
            else Mean[3 * n + c] = 0;
          }
        } //------------------------------------- end if (nPix0... ----------------------------------
      } //======================================= end for (i... =======================================

      int MaxNewIndex = n;
      if (Img.width * Img.height > 300) jump = Img.width * Img.height / (100 / 11);
      else jump = 3;

      // Putting NewIndex into "Img.Grid":	
      for (i = 0; i < Img.width * Img.height; i++) //=========================================================
      {
        if (i % jump == jump - 1) fm1.progressBar1.PerformStep();
        int Index = 0;
        for (c = 0; c < 3; c++)
        {
          iChan[c] = (Img.Grid[3 * i + c] - Min[c]) / Div[c];
          Index += Weight[c] * iChan[c];
        }
        if (Index >= MaxIndex) Index = MaxIndex - 1;
        Grid[i] = (byte)NewIndex[Index];
        if (Grid[i] == 0) Grid[i] = (byte)MaxNewIndex;
      } //====================== end for (i=0; ... =============================================

      // Calculating "Palet":
      byte R = 0, G = 0, B = 0;
      if (MaxNewIndex > 300) jump = MaxNewIndex / 20;
      else jump = 2;

      for (n = 0; n <= MaxNewIndex; n++)
      {
        if (n % jump == jump -1) fm1.progressBar1.PerformStep();
        if (n > 0)
        {
          if (Mean[3 * n + 2] < 255.0) R = (byte)Mean[3 * n + 2];
          if (Mean[3 * n + 1] < 255.0) G = (byte)Mean[3 * n + 1];
          if (Mean[3 * n + 0] < 255.0) B = (byte)Mean[3 * n + 0];
          Palet[n] = RGB(R, G, B);
        }
        else
        {
          Palet[n] = 0;
        }

      }
      return 1;
    } //************************** end MakePalette ************************************************************



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

      return (Sign * Dif);
    }


    int ColorDifAbs(byte[] Colp, byte[] Colh)
    // Returns the sum of the absolut differences of the color components divided through 3
    // with the sign of MaxC(Colp) - MaxC(Colh).
    {
      int Dif = 0;
      for (int c = 0; c < 3; c++) Dif += Math.Abs(Colp[c] - Colh[c]);
      return Dif;
    }


    int ColorDifSign(int iColp, int iColh)
    // Returns the sum of the absolut differences of the color components divided through 3
    // with the sign of MaxC(Colp) - MaxC(Colh).
    {
      int Dif = 0;
      //for (int c = 0; c < 3; c++) Dif += Math.Abs(Colp[c] - Colh[c]);
      Dif = Math.Abs((iColp & 0xff) - (iColh & 0xff)) + Math.Abs(((iColp << 8) & 0xff) - ((iColh >> 8) & 0xff)) +
        Math.Abs(((iColp & 0xff) >> 16) - ((iColh >> 16) & 0xff)); 
      int Sign = 0;
      if (MaxC(iColp & 0xff, (iColp >> 8) & 0xff, (iColp >> 16) & 0xff) - 
                MaxC(iColh & 0xff, (iColh >> 8) & 0xff, (iColh >> 16) & 0xff) > 0) Sign = 1;
      else Sign = -1;
      return (Sign * Dif) / 3;
    } 


    public int LabelCellsSign(int th, CImage Image3, Form1 fm1)
    /* Looks in "Image3" (standard coord.) for all pairs of adjacent pixels with signed color  
      differences greater than "th" or less than "-th" and finds the maximum or minimum color  
      differences among pixels of adjacent pairs in the same line or in the same column.
      Labels the extremal cracks in "this" (combinatorial coord.) with "1". The points incident
      to the cracks are also provided with labels indicating the number of incident cracks. 
      This method works both for color and gray value images. ---*/
    {
      int difH, difV, c, maxDif, minDif, nByte, NXB = Image3.width, x, y, xopt, yopt;
      int Inp, jump, State, Contr, xStartP, xStartM, yStartP, yStartM;
      if (Image3.N_Bits == 24) nByte = 3;
      else nByte = 1;
      for (x = 0; x < width * height; x++) Grid[x] = 0;
      byte[] Colorp = new byte[3], Colorh = new byte[3], Colorv = new byte[3];
      maxDif = 0; minDif = 0;
      fm1.progressBar1.Step = 1;
      fm1.progressBar1.Value = 0;
      fm1.progressBar1.Visible = true;
      if (height > 300) jump = height / 2 / 25;
      else jump = 3;
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
          else difV = 3*(Colorp[0] - Colorv[0]);
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

    
    public int LabelCellsSignAr(CImage Pal, short[,] Ar, int th)
    /* Looks in "Image3" (standard coord.) for all pairs of adjacent pixels with signed color  
      differences greater than "th" or less than "-th" and finds the maximum or minimum color  
      differences among pixels of adjacent pairs in the same line or in the same column.
      Labels the extremal cracks in "this" (combinatorial coord.) with "1". The points incident
      to the cracks are also provided with labels indicating the number of incident cracks. 
      This method works both for color and gray value images. ---*/
    {
      int difH, difV, maxDif, minDif, NXB = Pal.width, x, y, xopt, yopt;
      int Inp, State, Contr, xStartP, xStartM, yStartP, yStartM;
      for (x = 0; x < width * height; x++) Grid[x] = 0;
      byte indexP, indexV, indexH;
      maxDif = 0; minDif = 0;
      for (y = 3; y < height; y += 2) //================ vertical cracks =====================
      {
        State = 0;
        xopt = -1; xStartP = xStartM = -1;
        for (x = 3; x < width; x += 2)  //====================================================
        {
          indexV = Pal.Grid[((x - 2) / 2) + NXB * (y / 2)];
          indexP = Pal.Grid[x / 2 + NXB * (y / 2)];

          difV = Ar[indexP, indexV];
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
          indexH = Pal.Grid[x / 2 + NXB * ((y - 2) / 2)];
          indexP = Pal.Grid[x / 2 + NXB * (y / 2)];

          difH = Ar[indexP, indexH];
          if (difH > th) Inp = 1;
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
    } //******************************** end LabelCellsSignAr *****************************************


    public int Smooth(ref CImage Mask, bool Segmentation, Form1 fm1)
    // Calculates the average colors between "gvbeg" and "gvend" and saves them in the image "this".
    // This is a digital automaton with the states S=1 at Mask>0 and S=2 at Mask==0, but S is not used.
    // The variable "mpre" has the value of "Mask" in the previouse pixel.
    {
      int c, cnt, LabMask = 250, msk, mpre, x, xx, xbeg, xend, ybeg, yend, y, yy;
      int[] Col = new int[3], ColBeg = new int[3], ColEnd = new int[3];

      int nComp;
      if (N_Bits == 24) nComp = 3;
      else nComp = 1;

      // Smoothing the borders: Border at y=0:
      y = 0; cnt = 0; xbeg = 0; mpre = 200;
      for (c = 0; c < nComp; c++) ColBeg[c] = Grid[c];

      for (x = 0; x < width; x++) //==========================================================
      {
        msk = Mask.Grid[x + width * y];
        if (mpre > 0 && msk == 0) //----------------------------------------
        {
          cnt = 1; xbeg = x - 1;
          for (c = 0; c < nComp; c++) ColBeg[c] = Grid[nComp * (x - 1 + width * y) + c];
        }
        if (mpre == 0 && msk == 0) //----------------------------------------
        {
          cnt++;
        }
        if (mpre == 0 && msk > 0) //----------------------------------------
        {
          cnt++; xend = x;
          for (c = 0; c < nComp; c++) ColEnd[c] = Grid[nComp * (x + width * y) + c];
          for (xx = xbeg + 1; xx < xend; xx++) //==========================================
          {
            for (c = 0; c < nComp; c++)
              Grid[nComp * (xx + width * y) + c] = (byte)((ColBeg[c] * (xend - xx) + ColEnd[c] * (xx - xbeg)) / cnt);
            Mask.Grid[xx + width * y] = (byte)LabMask;
          } //============== end for (xx... ========================================
        }
        mpre = msk;
      } //=============== end for (x=0; ... ===========================================	

      // Border at y = height-1:
      y = height - 1; cnt = 0; xbeg = 0; mpre = 200;
      for (c = 0; c < nComp; c++) ColBeg[c] = Grid[nComp * width * y + c];
      for (x = 0; x < width; x++) //==========================================================
      {
        msk = Mask.Grid[x + width * y];
        if (mpre > 0 && msk == 0) //----------------------------------------
        {
          cnt = 1; xbeg = x - 1;
          for (c = 0; c < nComp; c++) ColBeg[c] = Grid[nComp * (x - 1 + width * y) + c];
        }
        if (mpre == 0 && msk == 0) //----------------------------------------
        {
          cnt++;
        }
        if (mpre == 0 && msk > 0) //----------------------------------------
        {
          cnt++; xend = x;
          for (c = 0; c < nComp; c++) ColEnd[c] = Grid[nComp * (x + width * y) + c];
          for (xx = xbeg + 1; xx < xend; xx++)
          {
            for (c = 0; c < nComp; c++)
              Grid[nComp * (xx + width * y) + c] = (byte)((ColBeg[c] * (xend - xx) + ColEnd[c] * (xx - xbeg)) / cnt);
            Mask.Grid[xx + width * y] = (byte)LabMask;
          }
        }
        mpre = msk;
      } //=============== end for (x=0; ... ===========================================	


      // Border at x = 0:
      x = 0; cnt = 0; ybeg = 0; mpre = 200;
      for (c = 0; c < nComp; c++) ColBeg[c] = Grid[nComp * (x + width * 0) + c];
      for (y = 0; y < height; y++) //=================================================
      {
        msk = Mask.Grid[x + width * y];
        if (mpre > 0 && msk == 0) //----------------------------------------
        {
          cnt = 1; ybeg = y - 1;
          for (c = 0; c < nComp; c++) ColBeg[c] = Grid[nComp * (x + width * (y - 1)) + c];
        }
        if (mpre == 0 && msk == 0) //----------------------------------------
        {
          cnt++;
        }
        if (mpre == 0 && msk > 0) //----------------------------------------
        {
          cnt++; yend = y;
          for (c = 0; c < nComp; c++) ColEnd[c] = Grid[nComp * (x + width * y) + c];
          for (yy = ybeg + 1; yy < yend; yy++)
          {
            for (c = 0; c < nComp; c++)
            {
              Col[c] = (ColBeg[c] * (yend - yy) + ColEnd[c] * (yy - ybeg)) / cnt;
              Grid[nComp * (x + width * yy) + c] = (byte)Col[c];
            }
            Mask.Grid[x + width * yy] = (byte)LabMask;
          }
        }
        mpre = msk;
      } //=============== end for (y=0; ... ==================================

      // Border at x = width - 1
      x = width - 1; cnt = 0; ybeg = 0; mpre = 200;
      for (c = 0; c < nComp; c++) ColBeg[c] = Grid[nComp * (x + width * 0) + c];
      for (y = 0; y < height; y++) //=================================================
      {
        msk = Mask.Grid[x + width * y];
        if (mpre > 0 && msk == 0) //----------------------------------------
        {
          cnt = 1; ybeg = y - 1;
          for (c = 0; c < nComp; c++) ColBeg[c] = Grid[nComp * (x + width * (y - 1)) + c];
        }
        if (mpre == 0 && msk == 0) //----------------------------------------
        {
          cnt++;
        }
        if (mpre == 0 && msk > 0) //----------------------------------------
        {
          cnt++; yend = y;
          for (c = 0; c < nComp; c++) ColEnd[c] = Grid[nComp * (x + width * y) + c];
          for (yy = ybeg + 1; yy < yend; yy++)
          {
            for (c = 0; c < nComp; c++)
            {
              Col[c] = (ColBeg[c] * (yend - yy) + ColEnd[c] * (yy - ybeg)) / cnt;
              Grid[nComp * (x + width * yy) + c] = (byte)Col[c];
            }
            Mask.Grid[x + width * yy] = (byte)LabMask;
          }
        }
        mpre = msk;
      } //=============== end for (y=0; ... ==================================

      Random rand = new Random();
      int maxNoise = 32, maxExtra = 255 + maxNoise;

      // Smooth on "x":
      int jump;
      if (height > 300) jump = height / 20;
      else jump = 2;
      fm1.progressBar1.Visible = true;
      for (y = 0; y < height; y++) //========================================================
      {
        if ((y % jump) == jump - 1) fm1.progressBar1.PerformStep();
        cnt = 0; xbeg = 0; mpre = 200;
        for (c = 0; c < nComp; c++) ColBeg[c] = Grid[nComp * width * y + c];
        for (x = 0; x < width; x++) //=====================================================
        {
          msk = Mask.Grid[x + width * y];
          if (mpre > 0 && msk == 0) //----------------------------------------
          {
            cnt = 1; xbeg = x - 1;
            for (c = 0; c < nComp; c++) ColBeg[c] = Grid[nComp * (x - 1 + width * y) + c];
          }
          if (mpre == 0 && msk == 0)  cnt++;
          
          if (mpre == 0 && msk > 0) //----------------------------------------
          {
            cnt++; xend = x;
            for (c = 0; c < nComp; c++) ColEnd[c] = Grid[nComp * (x + width * y) + c];
            int[] extra = new int[3];
            for (xx = xbeg + 1; xx < xend; xx++)
            {
              for (c = 0; c < nComp; c++)
              {
                extra[c] = (ColBeg[c] * (xend - xx) + ColEnd[c] * (xx - xbeg)) / cnt + rand.Next(32);
                extra[c] = (extra[c] * 255) / maxExtra;
                Grid[nComp * (xx + width * y) + c] = (byte)extra[c];
              } 
            }
          }
          mpre = msk;
        } //=============== end for (x=0; ... ========================================
      } //================= end for (y=0; ... ==========================================

      // Smooth on "y":
      if (width > 300) jump = width / 20;
      else jump = 2;
      for (x = 0; x < width; x++) //=====================================================
      {
        if ((x % jump) == jump - 1) fm1.progressBar1.PerformStep();
        cnt = 0; ybeg = 0; mpre = 200;
        for (c = 0; c < nComp; c++) ColBeg[c] = Grid[nComp * (x + width * 0) + c];
        for (y = 0; y < height; y++) //=================================================
        {
          msk = Mask.Grid[x + width * y];
          if (mpre > 0 && msk == 0) //----------------------------------------
          {
            cnt = 1; ybeg = y - 1;
            for (c = 0; c < nComp; c++) ColBeg[c] = Grid[nComp * (x + width * (y - 1)) + c];
          }
          if (mpre == 0 && msk == 0) //----------------------------------------
          {
            cnt++;
          }
          if (mpre == 0 && msk > 0) //---------------------------------------------------------------
          {
            cnt++; 
            yend = y; 
            for (c = 0; c < nComp; c++) ColEnd[c] = Grid[nComp * (x + width * y) + c];
            int[] extra1 = new int[3];
             for (yy = ybeg + 1; yy < yend; yy++)
            {
              for (c = 0; c < nComp; c++)
              {
                Col[c] = (Grid[nComp * (x + width * yy) + c] + (ColBeg[c] * (yend - yy) + ColEnd[c] * (yy - ybeg)) / cnt) / 2;
                extra1[c] = Col[c] + rand.Next(32);
                extra1[c] = (extra1[c] * 255) / maxExtra;
                Grid[nComp * (x + width * yy) + c] = (byte)extra1[c];
              }
            }
          }
          mpre = msk;
        } //=============== end for (y=0; ... ===================================================
      } //================= end for (x=0; ... =====================================================

      if (Segmentation) return 0;

      // Solving the Laplace's equation:
      int i;
      double fgv, omega = 1.4 / 4.0, dMaxLap = 0.0, dTH = 1.0;
      double[] dGrid = new double[width * height * nComp];
      double[] Lap = new double[3];
      for (i = 0; i < width * height * nComp; i++) dGrid[i] = (double)Grid[i];
      jump = 2;
      for (int iter = 0; iter < 50; iter++) //===================================================
      {
        if ((iter % jump) == jump -1) fm1.progressBar1.PerformStep();
        for (y = 1; y < height - 1; y++)
          for (x = 1; x < width - 1; x++)
          {
            if (Mask.Grid[x + width * y] == 0 && Math.Abs((x - y)) % 2 == 0)
              for (c = 0; c < nComp; c++)
              {
                Lap[c] = 0.0;
                Lap[c] += dGrid[nComp * (x + width * (y - 1)) + c];
                Lap[c] += dGrid[nComp * (x - 1 + width * y) + c];
                Lap[c] += dGrid[nComp * (x + 1 + width * y) + c];
                Lap[c] += dGrid[nComp * (x + width * (y + 1)) + c];
                Lap[c] -= 4.0 * dGrid[nComp * (x + width * y) + c];
                fgv = dGrid[nComp * (x + width * y) + c] + omega * Lap[c];
                if (fgv > 255.0) fgv = 255.0;
                if (fgv < 0.0) fgv = 0;
                dGrid[nComp * (x + width * y) + c] = fgv;
              }
          }

        for (y = 1; y < height - 1; y++)
          for (x = 1; x < width - 1; x++)
          {
            if (Mask.Grid[x + width * y] == 0 && Math.Abs((x - y)) % 2 == 1)
              for (c = 0; c < nComp; c++)
              {
                Lap[c] = 0.0;
                Lap[c] += dGrid[nComp * (x + width * (y - 1)) + c];
                Lap[c] += dGrid[nComp * (x - 1 + width * y) + c];
                Lap[c] += dGrid[nComp * (x + 1 + width * y) + c];
                Lap[c] += dGrid[nComp * (x + width * (y + 1)) + c];
                Lap[c] -= 4.0 * dGrid[nComp * (x + width * y) + c];
                fgv = dGrid[nComp * (x + width * y) + c] + omega * Lap[c];
                if (fgv > 255.0) fgv = 255.0;
                if (fgv < 0.0) fgv = 0;
                dGrid[nComp * (x + width * y) + c] = fgv; //(int)(fgv);
              }
          }

        dMaxLap = 0.0; // Calculating MaxLap:
        for (y = 1; y < height - 1; y++)
          for (x = 1; x < width - 1; x++) //===================================
          {
            if (Mask.Grid[x + width * y] == 0) //----------------------------
            {
              for (c = 0; c < nComp; c++) //==========================
              {
                Lap[c] = 0.0;
                Lap[c] += dGrid[nComp * (x + width * (y - 1)) + c];
                Lap[c] += dGrid[nComp * (x - 1 + width * y) + c];
                Lap[c] += dGrid[nComp * (x + 1 + width * y) + c];
                Lap[c] += dGrid[nComp * (x + width * (y + 1)) + c];
                Lap[c] -= 4.0 * dGrid[nComp * (x + width * y) + c];
                if (Math.Abs(Lap[c]) > dMaxLap) dMaxLap = Math.Abs(Lap[c]);
              } //================= end for (c=0; =================
            } //------------------- end if (Mask... -----------------
          } //===================== end for (x=1; ... =================
        int ii;
        
        int extra2 = 0;
        for (ii = 0; ii < width * height * nComp; ii++)
        {
          extra2 = (int)dGrid[ii] + rand.Next(32);
          extra2 = (extra2 * 255) / maxExtra;
          Grid[ii] = (byte)extra2; 
        }
        if (dMaxLap < dTH)  break;
      } //------------------------------------ end for (iter... ------------------------------------------
      return 0;
    } //************************************* end Smooth ***********************************************


    public void DrawComb(int StandX, int StandY, Form1 fm1)
    // Draws in the pictureBox1 the cracks of the image "Comb".
    {
      if (StandX < 0) StandX = 0;
      if (StandY < 0) StandY = 0;
      MessageBox.Show("Comb. coord. of the left upper corner =(" + 2 * StandX + "; " + 2 * StandY + ")");
      int PointSize = 3, SizeX = 2 * 75, SizeY = 2 * 75, Step = 4; // Step is the scaled unit of "Comb"
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
            xpB = x - 2 * StandX; // (x, y) are comb. coord. of a horiz. crack
            ypB = y - 2 * StandY; // (xpB, ypB) are coord. in pictureBox
            if (Grid[x + width * y] > 0) //--------------------------------------------------------
            {
              g.DrawLine(whitePen, (xpB - 1) * Step + 1, ypB * Step, (xpB + 1) * Step - 1, ypB * Step); // hor. crack

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



    public void DrawCombPix(int StandX, int StandY, Form1 fm1)
    // Draws in the pictureBox1 the cracks of the image "Comb". "StandX" and "StandY" are the standard
    // coordinates of the left upper corner of the displayed fragment of the image containimg this method.
    {
      bool PIXELS = true;
      CInscript Ins1 = new CInscript(1, 0.3, -5, 30, width, Color.White, fm1);
      if (StandX < 0) StandX = 0;
      if (StandY < 0) StandY = 0;
      MessageBox.Show("Comb. coord. of the left upper corner =(" + 2 * StandX + "; " + 2 * StandY + ")");
      int PointSize = 3, Step = 8, SizeX = 10 * Step, SizeY = 10 * Step; // Step is the scaled unit of "Comb"
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

      int Index = 0, xpB, ypB;
      int NX = Math.Min(width, 2 * (StandX + SizeX));
      int NY = Math.Min(height, 2 * (StandY + SizeY));
      byte Mask = 7;

      for (int y = 2 * StandY; y < NY; y++) //===================================================
      {
        if ((y & 1) == 0) //---------------------------------------------------------------------------
        {
          for (int x = 2 * StandX + 1; x < NX; x += 2) //==== over horizontal cracks ==========
          {
            xpB = x - 2 * StandX; // (x, y) are comb. coord. of a horiz. crack
            ypB = y - 2 * StandY; // (xpB, ypB) are coord. in pictureBox
            if (Grid[x + width * y] > 0) //--------------------------------------------------------
            {
              g.DrawLine(whitePen, (xpB - 1) * Step + 1, ypB * Step, (xpB + 1) * Step - 1, ypB * Step); // hor. crack
              if (PIXELS && false)
              {
                Index = Grid[x + width * (y + 1)]; // pixel under horizontal crack
                string str = Index.ToString();
                Ins1.Write(str, xpB * Step, (ypB - 3) * Step, Ins1); //fm1.Ins1);


                Index = Grid[x + width * (y - 1)]; // above horizontal crack
                string str1 = Index.ToString();
                Ins1.Write(str1, xpB * Step, (ypB - 5) * Step, Ins1);
              }

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
              if (PIXELS && false)
              {
                Index = Grid[x + 1 + width * (y)]; // pixel right of vertical crack
                string str2 = Index.ToString();
                Ins1.Write(str2, (xpB + 1) * Step, (ypB - 4) * Step, Ins1);

                Index = Grid[x - 1 + width * (y)]; // pixel left of vertical crack
                string str3 = Index.ToString();
                Ins1.Write(str3, (xpB - 1) * Step, (ypB - 4) * Step, Ins1);
              }

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

            if (PIXELS)
            {
              Index = Grid[x + 1 + width * (y)]; // pixel right of vertical crack
              string str2 = Index.ToString();
              Ins1.Write(str2, (xpB + 1) * Step, (ypB - 4) * Step, Ins1);

              Index = Grid[x - 1 + width * (y)]; // pixel left of vertical crack
              string str3 = Index.ToString();
              Ins1.Write(str3, (xpB - 1) * Step, (ypB - 4) * Step, Ins1);
            }

          } //========================= end for (int x ... ==========================================
        } //--------------------------- end if ((y & 1) == 0) //---------------------------------------
      } //==================================== end for (int y... ==========================================
    } //************************************** end DrawCombPix *************************************************



    public void DrawImageLine(int Y, int xStart, int th, CImage Sigma, byte[] Grid2, Form1 fm1)
    // This is a method of "ExtremIm".
    {
      if (MessReturn("Left end of the line in standard. coord.=(" + xStart + "; " + Y + ")") < 0) return;
      Graphics g1 = fm1.pictureBox1.CreateGraphics(); // for drawing the tested line in pictBox1
      Graphics g2 = fm1.pictureBox2.CreateGraphics(); // for drawing the tested line in pictBox2
      //Graphics g3 = fm1.pictureBox3.CreateGraphics(); // for drawing the curves
      Pen whitePen = new Pen(Color.White);
      Pen redPen = new Pen(Color.Red, 2);
      Pen thickRedPen = new Pen(Color.Red, 4);
      Pen greenPen = new Pen(Color.LightGreen, 2);
      Pen thickGreenPen = new Pen(Color.LightGreen, 4);
      SolidBrush blackBrush = new SolidBrush(Color.Black);
      Rectangle rect = new Rectangle(0, 0, 1240, 300);
      g1.FillRectangle(blackBrush, rect);
      int divider = 7, xEnd, length = 248, light1 = 0, light2 = 0, Step = 5, x1,
        y01 = 120, y02 = 240, y1, y;
      int nByte = N_Bits / 8;
      if (xStart + length < width) xEnd = xStart + length;
      else xEnd = width;
      int c = 0, Step2 = Step / 2;
      byte[] Colorp = new byte[3], ColorOld = new byte[3];

      // lightness of "ExtremIm":
      for (c = 0; c < 3; c++)
      {
        light1 += Grid[c + nByte * (xStart + width * Y)];
        ColorOld[c] = Grid[c + nByte * (xStart + width * Y)];
      }
      x1 = 0;
      y1 = y01 - light1 / divider;
      g1.DrawLine(whitePen, 20, y01, 20, y01 - th / divider);
      g1.DrawLine(whitePen, 0, y01, xEnd * Step, y01); // level lightness = 0

      // Drawing the curve of "ExtremIm":
      int Dif = 0;
      for (int x = xStart + 1; x < xEnd; x++) //===============================================
      {
        x1 = x - xStart;
        light2 = 0;
        for (c = 0; c < 3; c++)
        {
          light2 += Grid[c + nByte * (x + width * Y)]; // light2 is Extrem at "x"
          Colorp[c] = Grid[c + nByte * (x + width * Y)];
        }
        y = y01 - light2 / divider;
        g1.DrawLine(whitePen, (x1 - 1) * Step, y1, x1 * Step - 1, y1); // y1 is old value of "y"

        if (ColorDifSign(Colorp, ColorOld) > th) // light1 is the old value of light2
        {
          if (Grid2[2 * x + (2 * width + 1) * (2 * Y + 1)] > 0)
            g1.DrawLine(thickRedPen, x1 * Step, y1, x1 * Step, y);
          else
            g1.DrawLine(redPen, x1 * Step, y1, x1 * Step, y);
        }
        else
        {
          Dif = ColorDifSign(Colorp, ColorOld);
          if (Dif > -th && Dif < th)
            g1.DrawLine(whitePen, x1 * Step, y1, x1 * Step, y);
          else // Dif <= -th
          {
            if (Grid2[2 * x + (2 * width + 1) * (2 * Y + 1)] > 0)
              g1.DrawLine(thickGreenPen, x1 * Step, y1, x1 * Step, y);
            else
              g1.DrawLine(greenPen, x1 * Step, y1, x1 * Step, y);
          }
        }
        light1 = light2;
        x1 = x;
        y1 = y;
        for (c = 0; c < 3; c++) ColorOld[c] = Colorp[c];
      }

      // Drawing the curve of "SigmaIm":
      int nByteSigma = Sigma.N_Bits / 8;
      x1 = 0;
      y1 = y02; 
      g1.DrawLine(greenPen, 0, y02, xEnd * Step, y02); // The level "y02"
      for (int x = xStart + 1; x < xEnd; x++) //=============================
      {
        x1 = x - xStart;
        light1 = 0;
        for (c = 0; c < nByteSigma; c++) light1 += Sigma.Grid[c + nByteSigma * (x + width * Y)];
        y = y02 - light1 / divider;
         g1.DrawLine(greenPen, (x1 - 1) * Step, y1, x1 * Step - 1, y1);
        g1.DrawLine(greenPen, x1 * Step, y1, x1 * Step, y);
        x1 = x;
        y1 = y;
      }  //============================= end for (int x ... =================

      // Drawing the chosen line:
      int xx = (int)(xStart * fm1.Scale1) + fm1.marginX;
      int yy = (int)(Y * fm1.Scale1) + fm1.marginY;
      int ex = (int)(xEnd * fm1.Scale1) + fm1.marginX;
      g1.DrawLine(whitePen, xx, yy, ex, yy);
      g2.DrawLine(greenPen, xx, yy, ex, yy);

    } //******************************** end DrawImageLine ***********************

    public byte MaxC(byte R, byte G, byte B)
    {
      byte light = G;
      if (0.713 * R > G) light = (byte)(0.713 * R);
      if (0.527 * B > G) light = (byte)(0.527 * B);
      return light;
    }

    public byte MaxC(int R, int G, int B)
    {
      byte light = (byte)G;
      if (0.713 * R > G) light = (byte)(0.713 * R);
      if (0.527 * B > G) light = (byte)(0.527 * B);
      return light;
    }


    public int DeleteBit0(int nbyte, Form1 fm1)
    // If nbyte == 1, then sets the bits 0 and 1 of each pixel of Grid to 0.
    // If nbyte == 3, then sets the bit 0 (through & 254) of green and red chanels to 0.
    {
      //int jump, Len = width * height, nStep= 3;
      //jump = Len / nStep;
      for (int i = 0; i < width * height; i++)
      {
        //if (i % jump == jump - 1) fm1.progressBar1.PerformStep();
        if (nbyte == 1)
          Grid[i] = (byte)(Grid[i] - (Grid[i] % 4));
        else
        {
          Grid[nbyte * i + 2] = (byte)(Grid[nbyte * i + 2] & 254);
          Grid[nbyte * i + 1] = (byte)(Grid[nbyte * i + 1] & 254);
        }
      }
      return 1;
    } //********************* end DeleteBit0 ************************   

    public void CracksToPixel(CImage Comb, Form1 fm1)
    {
      int NX = Comb.width;
      int NY = Comb.height;
      int nx = NX / 2;
      int ny = NY / 2;
      int dim, jump; 
      for (int i = 0; i < nx * ny; i++) Grid[i] = 0;
      if (NY > 300) jump = NY / 25;
      else jump = 3;
      for (int y = 0; y < NY - 2; y++)
      {
        if (y % jump == jump - 1) fm1.progressBar1.PerformStep();
        for (int x = 0; x < NX - 2; x++)
        {
          dim = (x % 2) + (y % 2);
          if (dim == 1 && Comb.Grid[x + NX * y] > 0) Grid[x / 2 + nx * (y / 2)] = 255;
        }
      }
    } //************************ end CracksToPixel *************************************


    public int Trace(byte[] GridCopy, int P, int[] Index, ref int SumCells, ref int Pterm, ref int dir, int Size)
    /* Traces a branch of a component, saves all cells in "Index"; "SumCells" is the number of saved cells.
     * If "SumCells" becomes greater than "Size", the tracing is interrupted, and the method return -1.
     * Otherwise it returns a positive number. --*/
    {
      int Lab, LabCrack, Mask = 7, rv = 0;
      bool BP = false, END = false;
      bool atSt_P = false; //, deb = false;
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
     * number of traced cells. If "SumCells" becomes greater than "Size", then the saving of cells 
     * is interrupted but the counting in "SumCells" goes on.
     * --*/
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
            if (SumCells <
              Size)
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
      return 1;
    } //*************************** end CheckComb ************************************


    public int ColorToGrayMC(CImage inp, Form1 fm1)
    /* Transforms the colors of the color image "inp" to MaxC(r,g,b) 
    and puts these values to this.Grid. --------- */
    {
      int Light, x, y;
      bool COLOR = true;
      if (inp.N_Bits != 24) COLOR = false;

      fm1.progressBar1.Visible = true;
      N_Bits = 8; width = inp.width; height = inp.height;
      Grid = new byte[width * height * 8];
      int y1 = 1 + height / 100;
      for (y = 0; y < height; y++) //========================================
      {
        if (y % y1 == 1) fm1.progressBar1.PerformStep();
        for (x = 0; x < width; x++) // ====================================
        {
          if (COLOR)
            Light = MaxC(inp.Grid[2 + 3 * (x + width * y)],
                        inp.Grid[1 + 3 * (x + width * y)],
                        inp.Grid[0 + 3 * (x + width * y)]);
          else Light = inp.Grid[x + width * y];
          Grid[y * width + x] = (byte)Light;
        } // ==================== for (x.  ================================
      } // ====================== for (x.  ==================================
      fm1.progressBar1.Visible = false;
      return 1;
    } //************************** end ColorToGrayMC ***************************


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


    public int ConnectShort(CImage Img, int Threshold, Form1 fm1)
    // Filters 'Img" with the average filter, compares the difference of the filtered values
    // in two pixels with the distance of 'dist' pixels with the threshold and connects end points
    // of 'CombIm' with adjacent pixels.
    {
      CImage Work = new CImage(Img.width, Img.height, 8);
      CImage Work1 = new CImage(Img.width, Img.height, 8);
      CImage Work2 = new CImage(Img.width, Img.height, 8);
      int Dif01 = 0, Dif23 = 0, dist = 5, hWind = 5, x = 0, y = 0, xg = 0, yg = 0;
      Graphics g = fm1.pictureBox1.CreateGraphics();
      SolidBrush brushRed = new SolidBrush(Color.Red);
      SolidBrush brushBlue = new SolidBrush(Color.Blue);
      Rectangle rect;
      Work1.ColorToGrayMC(Img, fm1);
      Work.FastAverageM(Work1, hWind, fm1);
      for (x = 0; x < Img.width * Img.height; x++) Work1.Grid[x] = Work2.Grid[x] = 0;
      int[] index0 = new int[3], index1 = new int[3], index2 = new int[3], index3 = new int[3];

      // Preparing Work1 as label;
      for (y = dist; y < Img.height - dist; y++)
        for (x = dist; x < Img.width - dist; x++) //==========================
        {
          index0[0] = x - dist + Img.width * (y - dist); // far point 0
          index1[0] = x + dist + Img.width * (y + dist); // far point 1
          index0[1] = x - 1 + Img.width * (y - 1);       // near point 0
          index1[1] = x + 1 + Img.width * (y + 1);       // near point 1

          index2[0] = x - dist + Img.width * (y + dist);
          index3[0] = x + dist + Img.width * (y - dist);
          index2[1] = x - 1 + Img.width * (y + 1);
          index3[1] = x + 1 + Img.width * (y - 1);
          xg = fm1.marginX + (int)(fm1.Scale1 * x);
          yg = fm1.marginY + (int)(fm1.Scale1 * y);
          rect = new Rectangle(xg, yg, 2, 2);
          Dif01 = Math.Abs(Work.Grid[index0[0]] - Work.Grid[index0[1]] +
            Work.Grid[index1[1]] - Work.Grid[index1[0]]);

          Dif23 = Math.Abs(Work.Grid[index2[0]] - Work.Grid[index2[1]] +
            Work.Grid[index3[1]] - Work.Grid[index3[0]]);

          if (Dif01 > Threshold)
          {
            Work1.Grid[x + Img.width * y] = 255;
            g.FillRectangle(brushRed, rect);
          }
          if (Dif23 > Threshold)
          {
            Work2.Grid[x + Img.width * y] = 255;
            g.FillRectangle(brushBlue, rect);
          }
        }

      int[] dir0 = { 1, 2, 3, 3, 0, 3, 2, 2, 1, 1, 1, 1, 0, 0, 1, 2, 2, 2, 3, 3, 3, 3, 3, 3, 0, 0, 0,
                   3,2,2,2,2,1,1,1,1,1,1,1,1,0,0,0,0,1,2,2,2,2,2,3,3,3,3,3,3,3,3,3,3,0,0,0,0,0};
      int[] dir1 = { 2, 3, 0, 0, 1, 0, 3, 3, 2, 2, 2, 2, 1, 1, 2, 3, 3, 3, 0, 0, 0, 0, 0, 0, 1, 1, 1,
                   0,3,3,3,3,2,2,2,2,2,2,2,2,1,1,1,1,2,3,3,3,3,3,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1};
      int[] dir2 = { 3, 0, 1, 1, 2, 1, 0, 0, 3, 3, 3, 3, 2, 2, 3, 0, 0, 0, 1, 1, 1, 1, 1, 1, 2, 2, 2,
                   1,0,0,0,0,3,3,3,3,3,3,3,3,2,2,2,2,3,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2};
      int[] dir3 = { 0, 1, 2, 2, 3, 2, 1, 1, 0, 0, 0, 0, 3, 3, 0, 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 3,
                   2,1,1,1,1,0,0,0,0,0,0,0,0,3,3,3,3,0,1,1,1,1,1,2,2,2,2,2,2,2,2,2,2,3,3,3,3,3};
      int Len = 65;
      int[,] dir = new int[4, Len];
      for (y = 0; y < 4; y++)
        for (x = 0; x < Len; x++)
          switch (y)
          {
            case 0: dir[y, x] = dir0[x]; break;
            case 1: dir[y, x] = dir1[x]; break;
            case 2: dir[y, x] = dir2[x]; break;
            case 3: dir[y, x] = dir3[x]; break;
          }
      int[] StepX = { 2, 0, -2, 0 };
      int[] StepY = { 0, 2, 0, -2 };
      int X = 0, Y = 0, X1 = 0, Y1 = 0, XX = 0, YY = 0;
      bool FOUND = false, SYNCH = false;

      // Looking for an zero cell:
      for (y = dist; y < Img.height - dist; y++)
        for (x = dist; x < Img.width - dist; x++) //==========================================
        {
           X = 2 * x;
          Y = 2 * y;
          if ((Work1.Grid[x + Img.width * y] > 0 || Work1.Grid[x + Img.width * y] > 0) &&
            Grid[2 * x + 2 * width * y] == 1)
          {
            X = X1 = 2 * x;  // the found zero cell
            Y = Y1 = 2 * y;
            int Dir = 0;
            if (Grid[X + 1 + width * Y] == 1) Dir = 0;
            if (Grid[X + width * (Y + 1)] == 1) Dir = 1;
            if (Grid[X - 1 + width * Y] == 1) Dir = 2;
            if (Grid[X + width * (Y - 1)] == 1) Dir = 3;

             for (int i = 0; i < Len; i++)
            {
              X1 += StepX[dir[Dir, i]];
              Y1 += StepY[dir[Dir, i]];

               // Looking for the second point:
              if (Grid[X1 + width * Y1] > 0)
              {
                XX = X1;
                YY = Y1;
               FOUND = true;
                break;
              }
            }
          }
          if (FOUND) //-----------------------------------------------------------------
          {
            int xMin = Math.Min(X, XX);
            int yMin = Math.Min(Y, YY);
            int xMax = Math.Max(X, XX);
            int yMax = Math.Max(Y, YY);
            if ((X < XX && Y <= YY) || (X >= XX && Y > YY)) SYNCH = true;
            if (X < XX && Y > YY || X > XX && Y < YY) SYNCH = false;

            if (SYNCH) //--------------------------------------------
            {
             for (int iy = yMin + 1; iy <= yMax - 1; iy += 2)
              {
                if (Grid[xMin + width * iy] == 0)
                {
                  Grid[xMin + width * iy] = 1; // vertical crack
                  Grid[xMin + width * (iy - 1)]++; // upper end poni
                  Grid[xMin + width * (iy + 1)]++; // lower end point
                 }
              }
              for (int ix = xMin + 1; ix <= xMax - 1; ix += 2)
              {
                if (Grid[ix + width * yMax] == 0)
                {
                  Grid[ix + width * yMax] = 1; // horizontal crack
                  Grid[ix - 1 + width * yMax]++; // left end poni
                  Grid[ix + 1 + width * yMax]++; // right end point
                }
              }
            }
            else
            {
              for (int iy = yMin + 1; iy <= yMax - 1; iy += 2)
              {
                if (Grid[xMax + width * iy] == 0)
                {
                  Grid[xMax + width * iy] = 1; // vertical crack
                  Grid[xMax + width * (iy - 1)]++; // upper end poni
                  Grid[xMax + width * (iy + 1)]++; // lower end point
               }
              }
              for (int ix = xMin + 1; ix <= xMax - 1; ix += 2)
              {
                if (Grid[ix + width * yMax] == 0)
                {
                  Grid[ix + width * yMax] = 1; // horizontal crack
                  Grid[ix - 1 + width * yMax]++; // left end poni
                  Grid[ix + 1 + width * yMax]++; // right end point
                }
              }
            }  //------------------------ end if (SYNCH) --------------------
            FOUND = false;
          } //------------------------- end if (FOUND) ------------------------------------
        } //============================ end for (x ... =======================================
      return 1;
    } //**************************** end ConnectShort **********************************

 


    public int ExtremLightUni(CImage Inp, int hWind,Form1 fm1)
    {	/* The extreme filter for color or grayscale images with variable hWind.
	    The filter finds in the (2*hWind+1)-neighbourhood of the actual pixel (x,y) the color "Color1" with 
      minimum and the color "Color2" with thge maximum lightness. "Color1" is assigned to the output pixel
      if its lightniss is closer to the lightness of the cetral pixel than the lightness of "Color2". --*/

      byte[] CenterColor = new byte[3], Color = new byte[3], Color1 = new byte[3], Color2 = new byte[3];
      int c, k, nbyte = 3, x; //, xx=3, yy=1;
      if (Inp.N_Bits == 8) nbyte = 1;
      fm1.progressBar1.Visible = true;
      int jump;
      if (height > 300) jump = height / 33;
      else jump = 3;
      for (int y = 0; y < height; y++) // =======================================
      {
        if (y % jump == jump - 1) fm1.progressBar1.PerformStep();
        for (x = 0; x < width; x++) //==============================================
        {
          for (c = 0; c < nbyte; c++) Color2[c] = Color1[c] = Color[c] = CenterColor[c] = 
            Inp.Grid[c + nbyte * (x + y * width)];

          int MinLight = 1000, MaxLight = 0;
          for (k = -hWind; k <= hWind; k++) //=========================================
          {
            if (y + k >= 0 && y + k < height)
              for (int i = -hWind; i <= hWind; i++) //=====================================
              {
                if (x + i >= 0 && x + i < width) // && (i > 0 || k > 0))
                {
                  for (c = 0; c < nbyte; c++) Color[c] = Inp.Grid[c + nbyte * (x + i + (y + k) * width)];
                  int light;
                  if (nbyte == 3) light= MaxC(Color[2], Color[1], Color[0]);
                  else light = Color[0];

                  if (light < MinLight)
                  {
                    MinLight = light;
                    for (c = 0; c < nbyte; c++) Color1[c] = Color[c];
                  }
                  if (light > MaxLight)
                  {
                    MaxLight = light;
                    for (c = 0; c < nbyte; c++) Color2[c] = Color[c];
                  }
                }
              } //=============== end for (int i... ============================
          } //=================== end for (int k... ==============================

          int CenterLight = MaxC(CenterColor[2], CenterColor[1], CenterColor[0]);
          int dist1 = 0, dist2 = 0;
          dist1 = CenterLight - MinLight;
          dist2 = MaxLight - CenterLight;
          if (dist1 < dist2)
            for (c = 0; c < nbyte; c++)  Grid[c + nbyte * (x + y * width)] = Color1[c]; // Min
          else
            for (c = 0; c < nbyte; c++)  Grid[c + nbyte * (x + y * width)] = Color2[c]; // Max

        } //================== end for (int x... ===================================
      } //==================== end for (int y... =====================================
      return 1;
    } //********************** end ExtremLightUni **************************************
  } //************************** end classe CImage **************************************
} //**************************** end namespace ********************************************
