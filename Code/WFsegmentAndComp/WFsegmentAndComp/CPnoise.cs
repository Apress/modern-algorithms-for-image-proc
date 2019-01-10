using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;


namespace WFsegmentAndComp
{
  public class Queue
  {
    public int input, output, Len;
    public bool full;
    public int[] Array;

    public Queue(int len)  // Constructor
    {
      this.Len = len;
      this.input = 0;
      this.output = 0;
      this.full = false;
      this.Array = new int[Len];
    }

    public int Put(int V)
    {
      if (input == Len - 1)
      {
        full = true;
        return -1;
      }
      Array[input] = V;
      input++;
      return 1;
    }

    public int Get()
    {
      if (Empty() == true)
      {
        return -1;
      }
      int i = Array[output];
      if (output == Len - 1) output = 0;
      else output++;
      if (full == true) full = false;
      return i;
    }

    public bool Empty()
    {
      if (input == output && full == false)
      {
        return true;
      }
      return false;
    }
  } //************************ end class Queue *******************************


  public class CPnoise
  {
    public unsafe int[][] Index;
    int[] Comp; int[] nPixel; int MaxSize;
    byte MaskGV, MaskColor;
    public Queue Q1;

    unsafe
    public CPnoise(int[] Histo, int Qlength, int Size)  // Constructor
    {
      this.MaxSize = Size;
      this.Q1 = new Queue(Qlength);
      this.Comp = new int[MaxSize];
      this.nPixel = new int[256];
      int MaxHist = 0;
      MaskGV = 252;
      MaskColor = 254;
      for (int light = 0; light < 256; light++)
      {
        if (Histo[light] > MaxHist) MaxHist = Histo[light];
        nPixel[light] = 0;
      }
      Index = new int[256][];
      for (int light = 0; light < 256; light++) Index[light] = new int[Histo[light] + 1];
    }

    ~CPnoise() { }


    public int Sort(CImage Image, int[] histo, Form1 fm1)
    { 
      Index = new int[256][];
      for (int gw = 0; gw < 256; gw++) Index[gw] = new int[histo[gw] + 1];

      for (int gw = 0; gw < 256; gw++)
      {
        nPixel[gw] = 0;
        for (int j = 0; j < histo[gw] + 1; j++) Index[gw][j] = 0;
      }

      int ind;
      int light;
      bool COLOR;
      if (Image.N_Bits == 24) COLOR = true;
      else COLOR = false;
    
      int Len = Image.height, jump, nStep = 25;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      fm1.progressBar1.Value = 0;
      fm1.progressBar1.Step = 1;
      for (int y = 0; y < Image.height; y++) //============================================================
      {
        if (y % jump == jump - 1) fm1.progressBar1.PerformStep();
        for (int x = 0; x < Image.width; x++) //==========================================================
        { ind = x + y * Image.width;
          if (COLOR) light = MaxC(Image.Grid[3 * ind + 2], Image.Grid[3 * ind + 1],
                                                          Image.Grid[3 * ind + 0]) & 254; 
                                                         
          else light = Image.Grid[ind] & 252;
          if (light < 0) light = 0;
          if (light > 255) light = 255;
          
          int nP =nPixel[light];
          Index[light][nP] = ind;
          if (nPixel[light] < histo[light])
            nPixel[light]++; 
        } //============================ end for (int x = 0; .. ========================================
      } //============================== end for (int y = 0; .. ===========================================
      return 1;
    } //******************************** end Sort *********************************************************


    public int Neighb(CImage Image, int W, int n)
    // Returns the index of the nth neighboor of the pixel with the index W. If the neighboor
    // is outside the grid, then it returns -1.
    {
      int dx, dy, x, y, xn, yn;
      if (n == 4) return -1; 
      yn = y = W / Image.width; 
      xn = x = W % Image.width;
      dx = (n % 3) - 1; 
      dy = n / 3 - 1;
      xn += dx; 
      yn += dy;
      if (xn < 0 || xn >= Image.width || yn < 0 || yn >= Image.height) return -2;
      return xn + Image.width * yn;
    }


    unsafe public int PositionInIndex(int lightNeib, int Nei)
    {
      for (int i = 0; i < nPixel[lightNeib]; i++)
        if (Index[lightNeib][i] == Nei) return i;
      return -1;
    }


    public byte MaxC(byte R, byte G, byte B)
    { byte light = G;
      if (0.713 * R > G) light = (byte)(0.713 * R);
      if (0.527 * B > G) light = (byte)(0.527 * B);
      return light;
    }


    public int BreadthFirst_D(ref CImage Image, int i, int light, int size)
    // Looks for pixels with gray values <=light composing with the pixel "Index[light][i]"
    // an 8-connected subset. The size of the subset must be less than "size".
    // Instead of labeling the pixels of the subset, indices of pixels of the subset are saved in Comp.
    // Variable "i" is the index of the starting pixel in Index[light][i];
    // Pixels which are put into queue and into Comp[] are labeled in "Image.Grid(green)" by setting Bit 0 to 1.
    // Pixels wich belong to a too big component and having the gray value equal to "light" are
    // labeled in "Image.Grid(red)" by setting Bit 0 to 1. If such a labeled pixel is found in the while loop 
    // then "small" is set to 0. The instruction for breaking the loop is at the end of the loop. 
    {
      int index, Label1, Label2, maxNeib = 8, numbPix = 0, Nei, nextIndex;
      byte lightNeib = 0;
      bool small = true;
      bool COLOR = (Image.N_Bits == 24);
      index = Index[light][i];
      int[] MinBound = new int[3];
      for (int c = 0; c < 3; c++) MinBound[c] = 300;
      for (int p = 0; p < MaxSize; p++) Comp[p] = -1;
      Comp[numbPix] = index; 
      numbPix++;
      if (COLOR)
        Image.Grid[1 + 3 * index] |= 1; // Labeling as in Comp
      else
        Image.Grid[index] |= 1; // Labeling as in Comp
      Q1.input = Q1.output = 0;
      Q1.Put(index); // putting index into the queue
      while (!Q1.Empty()) //=  loop running while queue not empty =======================
      {
        nextIndex = Q1.Get();
        for (int n = 0; n <= maxNeib; n++) // == all neighbors of nextIndex =====================
        {
          Nei = Neighb(Image, nextIndex, n); // the index of the nth neighbor of nextIndex 
          if (Nei < 0) continue; // Nei<0 means outside the image
          if (COLOR)
          {
            Label1 = Image.Grid[1 + 3 * Nei] & 1;
            Label2 = Image.Grid[2 + 3 * Nei] & 1;
            lightNeib = MaxC(Image.Grid[2 + 3 * Nei], Image.Grid[1 + 3 * Nei],
                                                            Image.Grid[0 + 3 * Nei]);
            lightNeib = (byte)(lightNeib & 254); // MaskColor;
          }
          else
          {
            Label1 = Image.Grid[Nei] & 1;
            Label2 = Image.Grid[Nei] & 2;
            lightNeib = (byte)(Image.Grid[Nei] & 252); // MaskGV;
          }
          if (lightNeib == light && Label2 > 0) small = false;

          if (lightNeib <= light) //------------------------------------------------------------
          {
            if (Label1 > 0) continue;
            Comp[numbPix] = Nei; // putting the element with index Nei into Comp
            numbPix++;
            if (COLOR)
              Image.Grid[1 + 3 * Nei] |= 1; // Labeling with "1" as in Comp 
            else
              Image.Grid[Nei] |= 1; // Labeling with "1" as in Comp 
            if (numbPix > size)
            {
              small = false;
              break;
            }
            Q1.Put(Nei);
          }
          else // lightNeib<light
          {
            if (Nei != index) //---------------------------------------------------
            {
              if (COLOR)
              {
                if (lightNeib < MaxC((byte)MinBound[2], (byte) MinBound[1], (byte) MinBound[0]))
                  for (int c = 0; c < 3; c++) MinBound[c] = Image.Grid[c + 3 * Nei];
              }
              else
                if (lightNeib < MinBound[0]) MinBound[0] = lightNeib;
            } //------------------ end if (Nei!=index) --------------------------       
          } //-------------------- end if (lightNeib<=light) and else ------------------------
        } // =================== end for (n=0; .. ====================================
        if (!small) break;
      } // ===================== end while ==============================================

      int lightComp; // lightness of a pixel whose index is contained in "Comp"
      for (int m = 0; m < numbPix; m++) //======================================================
      {
        if (small && MinBound[0] < 300) //--"300" means MinBound was not calculated ---
        {
          if (COLOR)
            for (int c = 0; c < 3; c++) Image.Grid[c + 3 * Comp[m]] = (byte)MinBound[c];
          else
          {
            Image.Grid[Comp[m]] = (byte)MinBound[0];
          }
        }
        else
        {
          if (COLOR)
            lightComp = MaxC(Image.Grid[2 + 3 * Comp[m]], Image.Grid[1 + 3 * Comp[m]],
                                                            Image.Grid[0 + 3 * Comp[m]]) & 254;
          else
            lightComp = Image.Grid[Comp[m]] & 252; // MaskGV;

          if (lightComp == light) 
          {
            if (COLOR) Image.Grid[2 + 3 * Comp[m]] |= 1;
            else Image.Grid[Comp[m]] |= 2;
          }
          else // lightComp != light
          {
            if (COLOR)
            {
              Image.Grid[1 + 3 * Comp[m]] &= (byte)254; // deleting label 1
              Image.Grid[2 + 3 * Comp[m]] &= (byte)254; // deleting label 2
            }
            else
              Image.Grid[Comp[m]] &= 252; // MaskGV == 252; // deleting the labels
          }
        } //-------------------------------- end if (small) and else ----------------
      } //================================== end for (int m=0 .. =======================
      return numbPix;
    } //************************************ end BreadthFirst_D ***************************/


    public int DarkNoise(ref CImage Image, int MinGV, int MaxGV, int MADS, Form1 fm1)
    {
      bool COLOR = (Image.N_Bits == 24);
      int ind3 = 0, Label2, Lum, rv = 0; 
      if (MADS == 0) return 0;
      
      int cnt = 0;
      int Len = Image.width * Image.height, jump, nStep = 25;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      fm1.progressBar1.Step = 1;
      for (int light = MaxGV - 2; light >= MinGV; light--) //=========================================
      {
        for (int i = 0; i < nPixel[light]; i++) //========================================
        {
          cnt++;
          if ((cnt % jump) == jump - 1) fm1.progressBar1.PerformStep();
          ind3 = 3 * Index[light][i];
          if (COLOR)
          {
            Label2 = Image.Grid[2 + ind3] & 1;
            Lum = MaxC((byte)(Image.Grid[2 + ind3] & 254),
                      (byte)(Image.Grid[1 + ind3] & 254), (byte)(Image.Grid[0 + ind3] & 254));
          }
          else
          {
            Label2 = Image.Grid[Index[light][i]] & 2;
            Lum = Image.Grid[Index[light][i]] & 252;
          }

          if (Lum == light && Label2 == 0)
          { rv = BreadthFirst_D(ref Image, i, light, MADS);
          }
        } //============================= end for (int i.. =======================
      } //=============================== end for (int light.. ========================
      return rv;
    } //********************************* end DarkNoise *******************************

   
    public int LightNoise(ref CImage Image, int MinGV, int MaxGV, int MALS, Form1 fm1)
    {
      bool COLOR = (Image.N_Bits == 24);
      int ind3 = 0, Label2, Lum, rv = 0;
      if (MALS == 0) return 0;

      int cnt = 0;
      int Len = Image.width * Image.height, jump, nStep = 25;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      fm1.progressBar1.Step = 1;
      for (int light = MinGV; light <= 255; light++) //=========================================
      {
        int index;
        for (int i = 0; i < nPixel[light]; i++) //========================================
        {
          cnt++;
          if ((cnt % jump) == jump -1) fm1.progressBar1.PerformStep();
          ind3 = 3 * Index[light][i];
          index = Index[light][i];
          if (COLOR)
          {
            Label2 = Image.Grid[2 + 3 * Index[light][i]] & 1;
            Lum = (int) MaxC(Image.Grid[0 + ind3], Image.Grid[1 + ind3], Image.Grid[2 + ind3]);
          }
          else
          {
            Label2 = Image.Grid[Index[light][i]] & 2;
            Lum = Image.Grid[Index[light][i]];
          }

          if (Lum == light && Label2 == 0)
          { 
            rv = BreadthFirst_L(ref Image, i, light, MALS);
          }
        } //============================= end for (int i.. =======================
      } //=============================== end for (int light.. ========================
      return rv;
    } //********************************* end LightNoise ******************************


    public int BreadthFirst_L(ref CImage Image, int i, int light, int size)
    // Looks for pixels with gray values >=light composing with the pixel "Index[light][i]"
    // an 8-connected subset. The size of the subset must be less than "size".
    // Instead of labeling the pixels of the subset, indices of pixels of the subset are saved in Comp.
    // Variable "i" is the index of the starting pixel in Index[light][i];
    // Pixels which are put into queue and into Comp[] are labeled in "Image.Grid(green)" by setting Bit 0 to 1.
    // Pixels wich belong to a too big component and having the gray value equal to "light" are
    // labeled in "Image.Grid(red)" by setting Bit 0 to 1. If such a labeled pixel is found in the while loop
    // then "small" is set to 0. The insruction for breaking the loop is at the end of the loop. 
    {
      int lightNeib, index, Label1, Label2, maxNeib = 8, Nei, nextIndex;
      bool small = true;
      int[] MaxBound = new int[3];
      bool COLOR = (Image.N_Bits == 24);
      index = Index[light][i];
      for (int c = 0; c < 3; c++) MaxBound[c] = -255;
      for (int p = 0; p < size; p++) Comp[p] = -1;
      int numbPix = 0;
      Comp[numbPix] = index; 
      numbPix++;
      if (COLOR == true)
        Image.Grid[1 + 3 * index] |= 1; // Labeling as in Comp
      else
        Image.Grid[index] |= 1; // Labeling as in Comp
      Q1.input = Q1.output = 0;
      Q1.Put(index); // putting index into the queue

      while (!Q1.Empty()) //=== loop running while queue not empty ========================
      {
        nextIndex = Q1.Get();
        for (int n = 0; n <= maxNeib; n++) //======== all neighbors of nextIndex ================
        {
          Nei = Neighb(Image, nextIndex, n); // the index of the nth neighbor of nextIndex 
          if (Nei < 0) continue; // Nei<0 means outside the image
          if (COLOR)
          {
            Label1 = Image.Grid[1 + 3 * Nei] & 1;
            Label2 = Image.Grid[2 + 3 * Nei] & 1;
            lightNeib = MaxC(Image.Grid[2 + 3 * Nei], Image.Grid[1 + 3 * Nei], Image.Grid[0 + 3 * Nei]) & MaskColor;
          }
          else
          {
            Label1 = Image.Grid[Nei] & 1;
            Label2 = Image.Grid[Nei] & 2;
            lightNeib = Image.Grid[Nei] & MaskGV;
          }
          if (lightNeib == light && Label2 > 0) small = false;

          if (lightNeib >= light) //------------------------------------------------------------
          {
            if (Label1 > 0) continue;
            Comp[numbPix] = Nei; // putting the element with index Nei into Comp
            numbPix++;
            if (COLOR)
              Image.Grid[1 + 3 * Nei] |= 1; // Labeling with "1" as in Comp 
            else
              Image.Grid[Nei] |= 1; // Labeling with "1" as in Comp 

            if (numbPix > size)
            {
              small = false;
              break;
            }
            Q1.Put(Nei);
          }
          else // lightNeib<light
          {
            if (Nei != index) //-----------------------------------------------------
            {
              if (COLOR)
              {
                if (lightNeib > MaxC((byte)MaxBound[2], (byte)MaxBound[1], (byte)MaxBound[0]))
                {
                  for (int c = 0; c < 3; c++) MaxBound[c] = (Image.Grid[c + 3 * Nei] & MaskColor);

                }
              }
              else
              {
                if (lightNeib > MaxBound[0]) MaxBound[0] = lightNeib;
              }
            } //------------------ end if (Nei!=index) ----------------------------       
          } //-------------------- end if (lightNeib<=light) and else ------------------------
        } // =================== end for (n=0; .. ====================================
        if (!small) break;
      } // ===================== end while ==============================================
      int lightComp, nChanged = 0;
      for (int m = 0; m < numbPix; m++) //========================================================
      {
        if (small && MaxBound[0] >= 0) //it was >-255; ----"-1" means MaxBound was not calculated ---------
        {
          if (COLOR == true)
            for (int c = 0; c < 3; c++) Image.Grid[c + 3 * Comp[m]] = (byte)MaxBound[c];
          else
          {
            Image.Grid[Comp[m]] = (byte)MaxBound[0];
            nChanged++;
          }
        }
        else
        {
          if (COLOR == true)
            lightComp = MaxC(Image.Grid[2 + 3 * Comp[m]], Image.Grid[1 + 3 * Comp[m]],
                                                                        Image.Grid[0 + 3 * Comp[m]]) & MaskColor;
          else
            lightComp = Image.Grid[Comp[m]] & MaskGV;

          if (lightComp == light) //------------------------------------------------------ 
          {
            if (COLOR == true) Image.Grid[2 + 3 * Comp[m]] |= 1;
            else Image.Grid[Comp[m]] |= 2;
          }
          else
          {
            if (COLOR == true)
            {
              Image.Grid[1 + 3 * Comp[m]] &= (byte)MaskColor; // deleting label 1
              Image.Grid[2 + 3 * Comp[m]] &= (byte)MaskColor; // deleting label 2
            }
            else
              Image.Grid[Comp[m]] &= (byte)MaskGV; // deleting the labels
          } //----------------------- end if (lightComp==light) and else ---------------
        } //------------------------- end if (small && MaxBound[0]>0) and else ----------
      } //=========================== end for (int m=0 .. ================================
      return nChanged; // numbPix;
    } //************************************ end BreadthFirst_L *****************************
  } //************************************** end public class CPnoise ************************
} //**************************************** end namespace ************************************
