using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WFshadBinImpulse
{
  class CPnoise
  {
    unsafe
    public int[][] Index; // saving all pixels of the image ordered by lightness
    int[] Comp; // contains indices of pixels of a connected component
    int[] nPixel; // number of pixels with certain lightness in the image
    int MaxSize;  // admissible size of a component
    Queue Q1;

    unsafe
    public CPnoise(int[] Histo, int Qlength, int Size)  // Constructor
    {
      this.MaxSize = Size;
      this.Q1 = new Queue(Qlength); // necessary to find connected components
      this.Comp = new int[MaxSize];
      this.nPixel = new int[256]; // 256 is the number of lightness values
      for (int light = 0; light < 256; light++) nPixel[light] = 0;
      Index = new int[256][];
      for (int light = 0; light < 256; light++) Index[light] = new int[Histo[light] + 2];
    }


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


    public int MaxC(int R, int G, int B)
    {
      int max;
      if (R * 0.713 > G) max = (int)(R * 0.713);
      else max = G;
      if (B * 0.527 > max) max = (int)(B * 0.527);
      return max;
    }


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
      for (light = 0; light < 256; light++) nPixel[light] = 0;
      for (light = 0; light < 256; light++)
        for (int light1 = 0; light1 < histo[light]; light1++)
          Index[light][light1] = 0;

      int y1 = 1 + Image.height / 100;
      for (int y = 0; y < Image.height; y++) //===============================================================
      {
        for (int x = 0; x < Image.width; x++) //============================================================
        { 
          Condition = false;
          for (int k = 0; k < Number; k += 2)
            Condition = Condition || getCond(k, x, y, marginX, marginY, Scale, fm1);
          if (Condition) 
            continue; 
          i = x + y * Image.width; // Index of the pixel (x, y)
          if (COLOR) 
            light = MaxC(Image.Grid[3 * i+2] & 254, Image.Grid[3 * i + 1] & 254, Image.Grid[3 * i + 0] & 254);
          else light = Image.Grid[i] & 252;
          if (light < 0) light = 0;
          if (light > 255) light = 255;
          Index[light][nPixel[light]] = i; // record of the index "i" of a pixel with lightness "light"
          if (nPixel[light] < histo[light]) nPixel[light]++;
        } //============================ end for (int x=1; .. ========================================
      } //============================== end for (int y=1; .. ========================================
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


    unsafe public int PositionInIndex(int lightNeb, int Neib)
    {
      for (int i = 0; i < nPixel[lightNeb]; i++)
        if (Index[lightNeb][i] == Neib) return i;
      return -1;
    }


    private bool TestComp(int[] Comp, int numbPix, int width)
    {
      int ind, minX=10000, maxX=0, minY=10000, maxY=0, x, y, xs, ys;
      for (int p = 0; p < numbPix; p++)
      {
        ind = Comp[p];
        ys = ind / width; // standard coordinates
        xs = ind - ys * width;
        if (xs < minX) minX = xs;
        if (xs > maxX) maxX = xs;
        if (ys < minY) minY = ys;
        if (ys > maxY) maxY = ys;
      }
      int widthC=2*(maxX-minX+1)+5, heightC=2*(maxY-minY+1)+5;
      CImage Combin=new CImage(widthC, heightC, 8);
      for (int i = 0; i < widthC * heightC; i++) Combin.Grid[i] = 0;

      for (int p = 0; p < numbPix; p++) // Labeling pixels of 'Comp' in 'Combin'
      {
        ind = Comp[p];
        y = (ind / width - minY) * 2 + 3;
        x = (ind - (ind / width) * width - minX) * 2 + 3;
        Combin.Grid[x + widthC * y] = 1;
      }

      for (y = 2; y < heightC - 2; y++) //===============================================
      {
        if ((y & 1) == 0)
        {
          for (x = 3; x < widthC - 3; x += 2)
            if (Combin.Grid[x + widthC * (y - 1)] != Combin.Grid[x + widthC * (y + 1)])
              Combin.Grid[x + widthC * y] = 1; // Labeling a horizontal crack
        }
        else
        {
          for (x = 2; x < widthC - 2; x += 2)
            if (Combin.Grid[x - 1 + widthC * y] != Combin.Grid[x + 1 + widthC * y])
              Combin.Grid[x + widthC * y] = 1; // Labeling a vertical crack
        }
      } //================================= end for (y = 2 ... ==========================

      double Perim = 0.0, Corner = 1.414;
      bool rv, found = false;
      int xStart, xEnd;
      for (y = 2; y < heightC - 2; y++) //=================================================
      {
        if ((y & 1) == 0) //-----------
        { xStart = 3; 
          xEnd = widthC - 3;
        }
        else
        { xStart = 2; 
          xEnd = widthC -2;
        } //-------- end if (( ... ----

        for (x = xStart; x < xEnd; x += 2) //=========== testing cracks =================
        { if (Combin.Grid[x + widthC * y] != 1) continue;
          found = false;
          for (int neib = 0; neib<4; neib++) //===== testing four neighbour cracks ====
          { switch(neib) //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            { case 0: 
                if (Combin.Grid[x - 1 + widthC * (y - 1)] == 1)
                { Combin.Grid[x + widthC * y] = 2; //Labeled with 2 a used
                  Combin.Grid[x - 1 + widthC * (y - 1)] = 2; //Labeled with 2 a used
                  Perim += Corner;
                  found = true;
                }
                break;

              case 1: 
                if (Combin.Grid[x - 1 + widthC * (y + 1)] == 1)
                { Combin.Grid[x + widthC * y] = 2; //Labeled with 2 a used
                  Combin.Grid[x - 1 + widthC * (y + 1)] = 2; //Labeled with 2 a used
                  Perim += Corner;
                  found = true;
                }
                break;
              
              case 2: 
                if (Combin.Grid[x + 1 + widthC * (y - 1)] == 1)
                { Combin.Grid[x + widthC * y] = 2; //Labeled with 2 a used
                  Combin.Grid[x + 1 + widthC * (y - 1)] = 2; //Labeled with 2 a used
                  Perim += Corner;
                  found = true;
                }
                break;

              case 3: 
                if (Combin.Grid[x + 1 + widthC * (y + 1)] == 1)
                { Combin.Grid[x + widthC * y] = 2; //Labeled with 2 a used
                  Combin.Grid[x + 1 + widthC * (y + 1)] = 2; //Labeled with 2 a used
                  Perim += Corner;
                  found = true;
                }
                break;
            } //:::::::::::::::::::::::::::::::: end switch :::::::::::::::::::::::::
            if (found) break;
          } //================================== end for (int neib... =================
          if (!found)
          {
            Combin.Grid[x + widthC * y] = 2; //Labeled with 2 a used
            Perim += 1.0;
            //break;
          }
        } //==================================== end for (x... ==========================
      } //====================================== end for (y = 2 ... =======================
      rv = (numbPix < 20 && Perim*Perim / (double) numbPix <18.0) || 2.0 * numbPix / Perim > 2.5;
      return rv;
    } //**************************************** end TestComp *******************************


    private int BreadthFirst_D(ref CImage Image, int i, int light, int maxSize)
    /* Looks for pixels with lightness <=light composing with the pixel "Index[light][i]"
       an 8-connected subset. The size of the subset must be less than "maxSize".
       Instead of labeling the pixels of the subset, indices of pixels of the subset are saved in Comp.
       Variable "index" is the index of the starting pixel in Index[light][i];
       Pixels which are put into queue and into Comp[] are labeled in "Image.Grid(green)" by setting Bit 0 to 1.
       Pixels which belong to a too big component and having the gray value equal to "light" are
       labeled in "Image.Grid(red)" by setting Bit 0 to 1. If such a labeled pixel is found in the while loop 
       then "small" is set to 0. The instruction for breaking the loop is at the end of the loop. --*/
    {
      int lightNeb, // lightness of the neighbor
          index, Label1, Label2, maxNeib, // maxNeib is the maximum number of neighbors of a pixel
          Neib, // the index of a neighbor
          nextIndex, // index of the next pixel in the queue
          numbPix; // number of pixel indices in "Comp"
      bool small; 
      bool COLOR = (Image.N_Bits == 24);
      index = Index[light][i];
      int[] MinBound = new int[3]; // color of a pixel with minimum lightness among pixels near the subset
      for (int c = 0; c < 3; c++) MinBound[c] = 300;
      for (int p = 0; p < MaxSize; p++) Comp[p] = -1;
      numbPix = 0;
      maxNeib = 8; // maximum number of neighbors
      small = true;
      Comp[numbPix] = index; 
      numbPix++;
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
            lightNeb = MaxC(Image.Grid[2 + 3 * Neib], Image.Grid[1 + 3 * Neib],
                                                            Image.Grid[0 + 3 * Neib]) & 254; // MaskColor;
          }
          else
          {
            Label1 = Image.Grid[Neib] & 1;
            Label2 = Image.Grid[Neib] & 2;
            lightNeb = Image.Grid[Neib] & 252; // MaskGV;
          }
          if (lightNeb == light && Label2 > 0) small = false;

          if (lightNeb <= light) //------------------------------------------------------------
          {
            if (Label1 > 0) continue;
            Comp[numbPix] = Neib; // putting the element with index Neib into Comp
            numbPix++;
            if (COLOR)
              Image.Grid[1 + 3 * Neib] |= 1; // Labeling with "1" as in Comp 
            else
              Image.Grid[Neib] |= 1; // Labeling with "1" as in Comp 
            if (numbPix > maxSize)
            {
              small = false;
              break;
            }
            Q1.Put(Neib);
          }
          else // lightNeb > light
          {
            if (Neib != index) //-----------------------------------------------------
            {
              if (COLOR)
              {
                if (lightNeb < MaxC(MinBound[2], MinBound[1], MinBound[0]) )
                  for (int c = 0; c < 3; c++) MinBound[c] = Image.Grid[c + 3 * Neib];
              }
              else
                if (lightNeb < MinBound[0]) MinBound[0] = lightNeb;
            } //------------------ end if (Neib != index) ----------------------------       
          } //-------------------- end if (lightNeb<=light) and else ----------------------
        } // ===================== end for (n=0; .. ======================================
        if (small == false) break;
      } // ===================== end while ==============================================

      int lightComp; // lightness of a pixel whose index is contained in "Comp"
      for (int m = 0; m < numbPix; m++) //======================================================
      {
        if (small && MinBound[0] < 300) //--"300" means MinBound was not calculated ---
        {
          if (COLOR)
            for (int c = 0; c < 3; c++) Image.Grid[c + 3 * Comp[m]] = (byte)MinBound[c];
          else
            Image.Grid[Comp[m]] = (byte)MinBound[0];
        }
        else
        {
          if (COLOR)
            lightComp = MaxC(Image.Grid[2 + 3 * Comp[m]], Image.Grid[1 + 3 * Comp[m]],
                                                            Image.Grid[0 + 3 * Comp[m]]) & 254;
          else
            lightComp = Image.Grid[Comp[m]] & 252; // MaskGV;

          if (lightComp == light) //----------------------------------------------------------------
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
              Image.Grid[Comp[m]] &= 252; // (byte)MaskGV; // deleting the labels
          } //------------------------------ end if (bric == light) and else ------------------
        } //-------------------------------- end if (small != false) and else -----------------
      } //================================== end for (int m=0 .. ================================
      return numbPix;
    } //************************************ end BreadthFirst_D ************************************


    public int DarkNoise(ref CImage Image, int minLight, int maxLight, int maxSize, Form1 fm1)
    {
      bool COLOR = (Image.N_Bits == 24);
      int Label2, Lum, rv = 0, ind3 = 0; // index multilied with 3
      fm1.progressBar1.Maximum = 100;
      fm1.progressBar1.Step = 1;
      fm1.progressBar1.Visible = false;
      fm1.progressBar1.Visible = true;

      fm1.progressBar1.Value = 0;
      if (maxSize == 0)
      {
        fm1.progressBar1.Step = 50;
        fm1.progressBar1.PerformStep();
        return 1;
      }

      int jump = Image.width * Image.height / 50;
      for (int light = maxLight - 2; light >= minLight; light--) //=========================================
      {
        for (int i = 0; i < nPixel[light]; i++) //========================================
        {
          ind3 = 3 * Index[light][i];
          if ((Index[light][i] % jump) == jump - 1) fm1.progressBar1.PerformStep();
          if (COLOR)
          {
            Label2 = Image.Grid[2 + ind3] & 1;
            Lum = MaxC(Image.Grid[2 + ind3] & 254, Image.Grid[1 + ind3] & 254, Image.Grid[0 + ind3] & 254);
          }
          else
          {
            Label2 = Image.Grid[Index[light][i]] & 2;
            Lum = Image.Grid[Index[light][i]] & 252;
          }
          if (Lum == light && Label2 == 0)
          {
            rv = BreadthFirst_D(ref Image, i, light, maxSize);
          }

        } //============================= end for (int i.. =======================
      } //=============================== end for (int light.. ========================
      return rv;
   } //********************************* end DarkNoise *******************************


    public int LightNoise(ref CImage Image, int minLight, int maxLight, int maxSize, Form1 fm1)
    {
      bool COLOR = (Image.N_Bits == 24);
      int Label2, Lum, rv = 0, ind3 = 0; // index multiplied with 3
      fm1.progressBar1.Minimum = 0;
      fm1.progressBar1.Maximum = 100;
      fm1.progressBar1.Step = 1;
      fm1.progressBar1.Visible = true;
      if (maxSize == 0)
      {
        fm1.progressBar1.Step = 50;
        fm1.progressBar1.PerformStep();
        return 1;
      }

      int jump = Image.width * Image.height / 50;
      for (int light = minLight; light <= maxLight; light++) //=========================================
      {
        int index;
        for (int i = 0; i <= nPixel[light]; i++) //========================================
        {
          ind3 = 3 * Index[light][i];
          index = Index[light][i];
          if ((index % jump) == jump - 1) fm1.progressBar1.PerformStep();

          if (COLOR)
          {
            Label2 = Image.Grid[2 + 3 * Index[light][i]] & 1;
            Lum = MaxC(Image.Grid[2 + ind3] & 254,Image.Grid[1 + ind3] & 254,Image.Grid[0 + ind3]);
          }
          else
          {
            Label2 = Image.Grid[Index[light][i]] & 2;
            Lum = Image.Grid[Index[light][i]];
          }
          if (Lum == light && Label2 == 0)
            rv = BreadthFirst_L(ref Image, i, light, maxSize);
        } //============================= end for (int i.. =======================
      } //=============================== end for (int light.. ========================
     return rv;
    } //********************************* end LightNoise ******************************


    private int BreadthFirst_L(ref CImage Image, int i, int light, int maxSize)
    // Looks for pixels with gray values >=light composing with the pixel "Index[light][i]"
    // an 8-connected subset. The size of the subset must be less than "maxSize".
    // Instead of labeling the pixels of the subset, indices of pixels of the subset are saved in Comp.
    // Variable "i" is the index of the starting pixel in Index[light][i];
    // Pixels which are put into queue and into Comp[] are labeled in "Image.Grid(green)" by setting Bit 0 to 1.
    // Pixels wich belong to a too big component and having the gray value equal to "light" are
    // labeled in "Image.Grid(red)" by setting Bit 0 to 1. If such a labeled pixel is found in the while loop
    // then "small" is set to 0. The insruction for breaking the loop is at the end of the loop. 
    {
      int lightNeb, index, Label1, Label2, MaskBri = 252, MaskColor = 254, maxNeib = 8, Neib, nextIndex;
      bool small = true;
      int[] MaxBound = new int[3];
      bool COLOR = (Image.N_Bits == 24);
      index = Index[light][i];
      for (int c = 0; c < 3; c++) MaxBound[c] = -255;
      for (int p = 0; p < MaxSize; p++) Comp[p] = -1;
      int numbPix = 0;
      Comp[numbPix] = index; 
      numbPix++;
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
          if (Neib < 0) continue; // Neib < 0 means outside the image
          if (COLOR)
          {
            Label1 = Image.Grid[1 + 3 * Neib] & 1;
            Label2 = Image.Grid[2 + 3 * Neib] & 1;
            lightNeb = MaxC(Image.Grid[2 + 3 * Neib], Image.Grid[1 + 3 * Neib],
                                                            Image.Grid[0 + 3 * Neib]) & 254; // MaskColor;
          }
          else
          {
            Label1 = Image.Grid[Neib] & 1;
            Label2 = Image.Grid[Neib] & 2;
            lightNeb = Image.Grid[Neib] & MaskBri;
          }
          if (lightNeb == light && Label2 > 0) small = false;

          if (lightNeb >= light) //------------------------------------------------------------
          {
            if (Label1 > 0) continue;
            Comp[numbPix] = Neib; // putting the element with index Neib into Comp
            numbPix++;
            if (COLOR)
              Image.Grid[1 + 3 * Neib] |= 1; // Labeling with "1" as in Comp 
            else
              Image.Grid[Neib] |= 1; // Labeling with "1" as in Comp 

            if (numbPix > maxSize)
            {
              small = false;
              break;
            }
            Q1.Put(Neib);
          }
          else // lightNeb < light
          {
            if (Neib != index) //-----------------------------------------------------
            {
              if (COLOR)
              {
                if (lightNeb > MaxC(MaxBound[2], MaxBound[1], MaxBound[0]) )
                   for (int c = 0; c < 3; c++) MaxBound[c] = (Image.Grid[c + 3 * Neib] & MaskColor);
              }
              else
                if (lightNeb > MaxBound[0]) MaxBound[0] = lightNeb;
            } //------------------ end if (Neib!=index) ----------------------------       
          } //-------------------- end if (lightNeb<=light) and else ------------------------
        } // =================== end for (n=0; .. ====================================
        if (small == false) break;
      } // ===================== end while ==============================================
      int lightComp, // lightness of a pixel whose index is contained in "Comp"
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
            lightComp = MaxC(Image.Grid[2 + 3 * Comp[m]], Image.Grid[1 + 3 * Comp[m]],
                                                                        Image.Grid[0 + 3 * Comp[m]]) & MaskColor;
          else
            lightComp = Image.Grid[Comp[m]] & MaskBri;

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
              Image.Grid[Comp[m]] &= (byte)MaskBri; // deleting the labels
          } //----------------------- end if (lightComp==light) and else ---------------
        } //------------------------- end if (small && MaxBound[0]>0) and else ----------
      } //=========================== end for (int m=0 .. ================================
      return nChanged; // numbPix;
    } //************************************ end BreadthFirst_L *****************************

  } //************************************** end class CPnoise ********************************
} //**************************************** end namespace PulseNoiseWF *************************
