using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WFshadBinImpulse
{
  class CPnoise
  {
    unsafe
    public int[][] Index; // saving all pixels of the image ordered by lightness
    int[] Comp; // contains indices of pixels of a connected component
    int[] nPixel; // number of pixels with certain lightness in the image
    int maxSize;  // admissible size of a component
    Queue Q1;

    unsafe
    public CPnoise(int[] Histo, int Qlength, int Size)  // Constructor
    {
      this.maxSize = Size;
      this.Q1 = new Queue(Qlength); // necessary to find connected components
      this.Comp = new int[maxSize];
      this.nPixel = new int[256]; // 256 is the number of lightness values
      for (int light = 0; light < 256; light++) nPixel[light] = 0;
      Index = new int[256][];
      for (int light = 0; light < 256; light++) Index[light] = new int[Histo[light] + 1];
    }

    ~CPnoise() { }

    public bool getCond(int i, int x, int y, double marginX, double marginY, double Scale, Form1 fm1)
    { // Calculates bounds of the rectangle defined by global "fm1.v" and returns the condition
      // that the point (x, y) lies inside the rectangle.
      double fxmin = (fm1.v[i].X - marginX) / Scale; // "marginX" is the space of pictureBox1 left of image (may be 0)
      int xmin = (int)fxmin;

      double fxmax = (fm1.v[i + 1].X - marginX) / Scale; // Scale is the scale of the presentation of image
      int xmax = (int)fxmax;

      double fymin = (fm1.v[i].Y - marginY) / Scale; // "marginY" is the space of pictureBox1 above the image  (may be 0)
      int ymin = (int)fymin;

      double fymax = (fm1.v[i + 1].Y - marginY) / Scale;
      int ymax = (int)fymax;
      bool Condition = (y >= ymin && y <= ymax && x >= xmin && x <= xmax);
      return Condition;
    } //******************************* end getCond **********************************


    public int Sort(CImage Image, int[] histo, int Number, int picBox1Width, int picBox1Height, Form1 fm1)
    {
      int light, i;
      double ScaleX = (double)picBox1Width / (double)Image.width;
      double ScaleY = (double)picBox1Height / (double)Image.height;
      double Scale; // Scale of the presentation of the image in "pictureBox1"
      if (ScaleX < ScaleY) Scale = ScaleX;
      else Scale = ScaleY;
      bool COLOR;
      if (Image.N_Bits == 24) COLOR = true;
      else COLOR = false;
      double marginX = (double)(picBox1Width - Scale * Image.width) * 0.5; // space left of the image
      double marginY = (double)(picBox1Height - Scale * Image.height) * 0.5; // space above the image
      bool Condition = false; // Condition for skipping pixel (x, y) if it lies in one of the global rectangles "fm1.v"
      
      fm1.progressBar1.Value = 0;
      fm1.progressBar1.Step = 1;
      fm1.progressBar1.Visible = true;
      fm1.progressBar1.Maximum = 100;

      int y1 = 1 + Image.height / 100;
      for (int y = 1; y < Image.height; y++)
      {
        if (y % y1 == 1) fm1.progressBar1.PerformStep();
        for (int x = 1; x < Image.width; x++) //============================================================
        {
          Condition = false;
          for (int k = 0; k < Number; k += 2)
            Condition = Condition || getCond(k, x, y, marginX, marginY, Scale, fm1);
          if (Condition) continue;

          i = x + y * Image.width; // Index of the pixel (x, y)
          if (COLOR) light = ((Image.Grid[3 * i] & 254) + (Image.Grid[3 * i + 1] & 254) + (Image.Grid[3 * i + 2] & 254)) / 3;
          else light = Image.Grid[i] & 252;
          if (light < 0) light = 0;
          if (light > 255) light = 255;
          Index[light][nPixel[light]] = i; // record of the index "i" of a pixel with lightness "light"
          if (nPixel[light] < histo[light])
            nPixel[light]++;
        } //============================ end for (int x=1; .. ========================================
      }
      return 1;
    } //******************************** end Sort *********************************************************

    public int Neighb(CImage Image, int W, int n)
    // Returns the index of the nth neighboor of the pixel W. If the neighboor
    // is outside the grid, then it returns -1.
    {
      int dx, dy, x, y, xn, yn;
      if (n == 4) return -1; // "n==4" means Neigb==W
      yn = y = W / Image.width; xn = x = W % Image.width;
      dx = (n % 3) - 1; dy = n / 3 - 1;
      xn += dx; yn += dy;
      if (xn < 0 || xn >= Image.width || yn < 0 || yn >= Image.height) return -2;
      return xn + Image.width * yn;
    }


    unsafe public int PositionInIndex(int lightNeib, int Neib)
    {
      for (int i = 0; i < nPixel[lightNeib]; i++)
        if (Index[lightNeib][i] == Neib) return i;
      return -1;
    }

    private int Lumi(byte R, byte G, byte B) // lightness of a pixel with the color (R, G, B)
    {
      return (int)((R + G + B) / 3);
    }

    private int BreadthFirst_D(ref CImage Image, int i, int light, int maxDark)
    /* Looks for pixels with lightness <=light composing with the pixel "Index[light][i]"
       an 8-connected subset. The size of the subset must be less than "maxDark".
       Instead of labeling the pixels of the subset, indices of pixels of the subset are saved in Comp.
       Variable "index" is the index of the starting pixel in Index[light][i];
       Pixels which are put into queue and into Comp[] are labeled in "Image.Grid(green)" by setting Bit 0 to 1.
       Pixels which belong to a too big component and having the gray value equal to "light" are
       labeled in "Image.Grid(red)" by setting Bit 0 to 1. If such a labeled pixel is found in the while loop 
       then "small" is set to 0. The instruction for breaking the loop is at the end of the loop. --*/
    {
      int lightNeib, // lightness of the neighbor
          index, Label1, Label2, maxNeib, // maxNeib is the maximum number of neighbors of a pixel
          Neib, // the index of a neighbor
          nextIndex, // index of the next pixel in the queue
          numbPix; // number of pixel indices in "Comp"
      bool small; // equals "true" 
      bool COLOR = (Image.N_Bits == 24);
      index = Index[light][i];
      int[] MinBound = new int[3]; // color of a pixel with minimum lightness among pixels near the subset
      for (int c = 0; c < 3; c++) MinBound[c] = 300;
      for (int p = 0; p < maxDark; p++) Comp[p] = -1;
      numbPix = 0;
      maxNeib = 8; // maximum number of neighbors
      small = true;
      Comp[numbPix] = index; numbPix++;
      if (COLOR)
        Image.Grid[1 + 3 * index] |= 1; // Labeling as in Comp (Label1)
      else
        Image.Grid[index] |= 1; // Labeling as in Comp
      Q1.input = Q1.output = 0;
      Q1.Put(index); // putting index into the queue
      while (Q1.Empty() == 0) //=  loop running while queue not empty =======================
      {
        nextIndex = Q1.Get();
        for (int n = 0; n <= maxNeib; n++) // == all neighbors of nextIndex =====================
        {
          Neib = Neighb(Image, nextIndex, n); // the index of the nth neighbor of nextIndex 
          if (Neib < 0) continue; // Neib<0 means outside the image
          if (COLOR)
          {
            Label1 = Image.Grid[1 + 3 * Neib] & 1;
            Label2 = Image.Grid[2 + 3 * Neib] & 1;
            lightNeib = Lumi(Image.Grid[0 + 3 * Neib], Image.Grid[1 + 3 * Neib],
                                                            Image.Grid[2 + 3 * Neib]) & 254; // MaskColor;
          }
          else
          {
            Label1 = Image.Grid[Neib] & 1;
            Label2 = Image.Grid[Neib] & 2;
            lightNeib = Image.Grid[Neib] & 252; // MaskGV;
          }
          if (lightNeib == light && Label2 > 0) small = false;

          if (lightNeib <= light) //------------------------------------------------------------
          {
            if (Label1 > 0) continue;
            Comp[numbPix] = Neib; // putting the element with index Neib into Comp
            numbPix++;
            if (COLOR)
              Image.Grid[1 + 3 * Neib] |= 1; // Labeling with "1" as in Comp 
            else
              Image.Grid[Neib] |= 1; // Labeling with "1" as in Comp 
            if (numbPix == maxDark)
            {
              small = false;
              break;
            }
            Q1.Put(Neib);
          }
          else // lightNeib<light
          {
            if (Neib != index) //---------------------------------------------------
            {
              if (COLOR)
              {
                if (lightNeib < (MinBound[0] + MinBound[1] + MinBound[2]) / 3)
                  for (int c = 0; c < 3; c++) MinBound[c] = Image.Grid[c + 3 * Neib];
              }
              else
                if (lightNeib < MinBound[0]) MinBound[0] = lightNeib;
            } //------------------ end if (Neib!=index) --------------------------       
          } //-------------------- end if (lightNeib<=light) and else ------------------------
        } // =================== end for (n=0; .. ====================================
        if (small == false) break;
      } // ===================== end while ==============================================
      int lightC; // lightness of a pixel whose index is contained in "Comp"
      for (int m = 0; m < numbPix; m++) //======================================================
      {
        if (small != false && MinBound[0] < 300) //--"300" means MinBound was not calculated ---
        {
          if (COLOR)
            for (int c = 0; c < 3; c++) Image.Grid[c + 3 * Comp[m]] = (byte)MinBound[c];
          else
            Image.Grid[Comp[m]] = (byte)MinBound[0];
        }
        else
        {
          if (COLOR)
            lightC = Lumi(Image.Grid[3 * Comp[m]], Image.Grid[1 + 3 * Comp[m]],
                                                            Image.Grid[2 + 3 * Comp[m]]) & 254;
          else
            lightC = Image.Grid[Comp[m]] & 252; // MaskGV;

          if (lightC == light) //----------------------------------------------------------------
          {
            if (COLOR) Image.Grid[2 + 3 * Comp[m]] |= 1;
            else Image.Grid[Comp[m]] |= 2;
          }
          else // lightC!=light
          {
            if (COLOR)
            {
              Image.Grid[1 + 3 * Comp[m]] &= (byte)254; // deleting label 1
              Image.Grid[2 + 3 * Comp[m]] &= (byte)254; // deleting label 2
            }
            else
              Image.Grid[Comp[m]] &= 252; // (byte)MaskGV; // deleting the labels
          } //------------------------------ end if (lightc == light) and else ------------------
        } //-------------------------------- end if (small != false) and else -----------------
      } //================================== end for (int m=0 .. ================================
      return numbPix;
    } //************************************ end BreadthFirst_D ************************************

    public int DarkNoise(ref CImage Image, int minLi, int maxLi, int maxDark, Form1 fm1)
    {
      bool COLOR = (Image.N_Bits == 24);
      int ind3 = 0, // index multilied with 3
          Label2, Lum, rv = 0;
      if (maxDark == 0) return 0;
      fm1.progressBar1.Visible = true;
      int light1 = (maxLi - minLi + 1) / 100;
      if (light1 == 0) light1 = 2;
      for (int light = maxLi - 2; light >= minLi; light--) //=========================================
      {
        //if (light % light1 == 1) fm1.progressBar1.PerformStep();
        for (int i = 0; i < nPixel[light]; i++) //========================================
        {
          ind3 = 3 * Index[light][i];
          if (COLOR)
          {
            Label2 = Image.Grid[2 + ind3] & 1;
            Lum = ((Image.Grid[0 + ind3] & 254) + (Image.Grid[1 + ind3] & 254) + (Image.Grid[2 + ind3] & 254)) / 3;
          }
          else
          {
            Label2 = Image.Grid[Index[light][i]] & 2;
            Lum = Image.Grid[Index[light][i]] & 252;
          }
          if (Lum == light && Label2 == 0)
          {
            rv = BreadthFirst_D(ref Image, i, light, maxDark);
          }

        } //============================= end for (int i.. =======================
      } //=============================== end for (int light.. ========================
      return rv;
    } //********************************* end DarkNoise *******************************


    public int LightNoise(ref CImage Image, int minLi, int maxLi, int maxLight, Form1 fm1)
    {
      bool COLOR = (Image.N_Bits == 24);
      int ind3 = 0, // index multiplied with 3
          Label2, Lum, rv = 0;
      if (maxLight == 0) return 0;
      
      fm1.progressBar1.Minimum = 0;
      fm1.progressBar1.Maximum = 100;
      fm1.progressBar1.Step = 1;
      fm1.progressBar1.Value = 0;
      fm1.progressBar1.Visible = true;

      int light1 = 1 + (255 - minLi + 1) / 100;
      if (light1 == 0) light1 = 1;
      for (int light = minLi; light <= 255; light++) //=========================================
      {
        int index;
         if (light % light1 == 1) fm1.progressBar1.PerformStep();
        for (int i = 0; i < nPixel[light]; i++) //========================================
        {
          ind3 = 3 * Index[light][i];
          index = Index[light][i];
          if (COLOR)
          {
            Label2 = Image.Grid[2 + 3 * Index[light][i]] & 1;
            Lum = ((Image.Grid[0 + ind3] & 254) + (Image.Grid[1 + ind3] & 254) + (Image.Grid[2 + ind3])) / 3;
          }
          else
          {
            Label2 = Image.Grid[Index[light][i]] & 2;
            Lum = Image.Grid[Index[light][i]];
          }
          if (Lum == light && Label2 == 0)
            rv = BreadthFirst_L(ref Image, i, light, maxLi);
        } //============================= end for (int i.. =======================
      } //=============================== end for (int light.. ========================
      fm1.progressBar1.Visible = false;
      return rv;
    } //********************************* end LightNoise ******************************


    private int BreadthFirst_L(ref CImage Image, int i, int light, int maxSize)
    // Looks for pixels with lightness >=light composing with the pixel "Index[light][i]"
    // an 8-connected subset. The size of the subset must be less than "maxSize".
    // Instead of labeling the pixels of the subset, indices of pixels of the subset are saved in Comp.
    // Variable "i" is the index of the starting pixel in Index[light][i];
    // Pixels which are put into queue and into Comp[] are labeled in "Image.Grid(green)" by setting Bit 0 to 1.
    // Pixels belonging to a too big component and having the gray value equal to "light" are
    // labeled in "Image.Grid(red)" by setting Bit 0 to 1. If such a labeled pixel is found in the while loop
    // then "small" is set to 0. The insruction for breaking the loop is at the end of the loop. 
    {
      int lightNeib, index, Label1, Label2, maxLight = 252, MaskColor = 254, maxNeib = 8, Neib, nextIndex;
      bool small = true;
      int[] MaxBound = new int[3];
      bool COLOR = (Image.N_Bits == 24);
      index = Index[light][i];
      for (int c = 0; c < 3; c++) MaxBound[c] = -255;
      for (int p = 0; p < maxSize; p++) Comp[p] = -1;
      int numbPix = 0;
      Comp[numbPix] = index; numbPix++;
      if (COLOR == true)
        Image.Grid[1 + 3 * index] |= 1; // Labeling as in Comp
      else
        Image.Grid[index] |= 1; // Labeling as in Comp
      Q1.input = Q1.output = 0;
      Q1.Put(index); // putting index into the queue

      while (Q1.Empty() == 0) //=== loop running while queue not empty ========================
      {
        nextIndex = Q1.Get();
        for (int n = 0; n <= maxNeib; n++) //======== all neighbors of nextIndex ================
        {
          Neib = Neighb(Image, nextIndex, n); // the index of the nth neighbor of nextIndex 
          if (Neib < 0) continue; // Neib<0 means outside the image
          if (COLOR)
          {
            Label1 = Image.Grid[1 + 3 * Neib] & 1;
            Label2 = Image.Grid[2 + 3 * Neib] & 1;
            lightNeib = Lumi(Image.Grid[0 + 3 * Neib], Image.Grid[1 + 3 * Neib], Image.Grid[2 + 3 * Neib]) & MaskColor;
          }
          else
          {
            Label1 = Image.Grid[Neib] & 1;
            Label2 = Image.Grid[Neib] & 2;
            lightNeib = Image.Grid[Neib] & maxLight;
          }
          if (lightNeib == light && Label2 > 0) small = false;

          if (lightNeib >= light) //------------------------------------------------------------
          {
            if (Label1 > 0) continue;
            Comp[numbPix] = Neib; // putting the element with index Neib into Comp
            numbPix++;
            if (COLOR)
              Image.Grid[1 + 3 * Neib] |= 1; // Labeling with "1" as in Comp 
            else
              Image.Grid[Neib] |= 1; // Labeling with "1" as in Comp 

            if (numbPix == maxSize)
            {
              small = false;
              break;
            }
            Q1.Put(Neib);
          }
          else // lightNeib<light
          {
            if (Neib != index) //-----------------------------------------------------
            {
              if (COLOR)
              {
                if (lightNeib > (MaxBound[0] + MaxBound[1] + MaxBound[2]) / 3)
                {
                  for (int c = 0; c < 3; c++) MaxBound[c] = (Image.Grid[c + 3 * Neib] & MaskColor);

                }
              }
              else
              {
                if (lightNeib > MaxBound[0]) MaxBound[0] = lightNeib;
              }
            } //------------------ end if (Neib!=index) ----------------------------       
          } //-------------------- end if (lightNeib<=light) and else ------------------------
        } // =================== end for (n=0; .. ====================================
        if (small == false) break;
      } // ===================== end while ==============================================
      int lightC, // lightness of a pixel whose index is contained in "Comp"
          nChanged = 0; // number of pixels whose lightness was changed
      for (int m = 0; m < numbPix; m++) //========================================================
      {
        if (small == true && MaxBound[0] >= 0) //it was >-255; ----"-1" means MaxBound was not calculated ---------
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
            lightC = Lumi(Image.Grid[0 + 3 * Comp[m]], Image.Grid[1 + 3 * Comp[m]],
                                                                        Image.Grid[2 + 3 * Comp[m]]) & MaskColor;
          else
            lightC = Image.Grid[Comp[m]] & maxLight;

          if (lightC == light) //------------------------------------------------------ 
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
              Image.Grid[Comp[m]] &= (byte)maxLight; // deleting the labels
          } //----------------------- end if (lightC==light) and else ---------------
        } //------------------------- end if (small && MaxBound[0]>0) and else ----------
      } //=========================== end for (int m=0 .. ================================
      return nChanged; // numbPix;
    } //************************************ end BreadthFirst_L *****************************

  }
}
