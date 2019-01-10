using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.IO;

namespace WFcompressPal
{
  // Classes for the lists of lines:

  public class CQue
  {
    public
      int input, output, Len;
    bool full;
    iVect2[] Array;
    public CQue() { }// Default onstructor
    public CQue(int len) // Constructor
    {
      this.Len = len;
      this.input = 0;
      this.output = 0;
      this.full = false;
      this.Array = new iVect2[Len];
    }

    public int Put(iVect2 V)
    {
      if (full)
      {
        return -1;
      }
      Array[input] = V;
      if (input == Len - 1) input = 0;
      else input++;
      return 1;
    }

    public iVect2 Get()
    {
      iVect2 er = new iVect2(-1, -1);
      if (Empty())
      {
        return er;
      }
      iVect2 iV = Array[output];
      if (output == Len - 1) output = 0;
      else output++;
      if (full) full = false;
      return iV;
    }

    public bool Empty()
    {
      if (input == output && full == false)
      {
        return true;
      }
      return false;
    }
  } //***************************** end public class CQue **************************


  public class CLine // Line in a color image; size=14 bytes
  {
    public int EndByte; // 4 bytes
    public ushort x, y, nCrack;  // 6 bytes
    public byte Ind0, Ind1, Ind2, Ind3; // 4 bytes
  } //**************** end public class CLine ***************************


  public class CCrack // Short line in a gray value image; size=6 bytes
  {
    public ushort x, y;	// 4 bytes; in bit 15 of "x" and "y" is "dir" encoded
    public byte Ind0, Ind1; // 2 bytes
  } //**************** end public class CCrack ***************************************

  public class CListLines
  {
    unsafe
    public
    int nLine1, nLine2, nByte, MaxLine1, MaxLine2, MaxByte, CNX, CNY, nBits3;

    public iVect2[] Step;
    public iVect2[] Norm;

    public CCrack[] Line1;
    public CLine[] Line;
    public byte[] Byte, IndPos, IndNeg;
    public CQue pQ;

    public CListLines() { } // Default constructor

    public CListLines(int max1, int max2, int maxB, int cnx, int cny, int nbits3) // constructor
    {
      this.MaxLine1 = max1;
      this.MaxLine2 = max2;
      this.MaxByte = maxB;
      this.CNX = cnx;
      this.CNY = cny;
      this.nBits3 = nbits3;
      this.pQ = new CQue(1000); // necessary to find connected components
      this.nLine1 = 0;
      this.nLine2 = 0;
      this.nByte = 0;
      this.Line1 = new CCrack[MaxLine1];
      for (int i = 0; i < MaxLine1; i++) Line1[i] = new CCrack();
      this.Line = new CLine[MaxLine2];
      for (int i = 0; i < MaxLine2; i++) Line[i] = new CLine();
      this.Step = new iVect2[4];
      for (int i = 0; i < 4; i++) Step[i] = new iVect2();
      this.Norm = new iVect2[4];
      for (int i = 0; i < 4; i++) Norm[i] = new iVect2();

      Step[0].X = 1; Step[0].Y = 0;
      Step[1].X = 0; Step[1].Y = 1;
      Step[2].X = -1; Step[2].Y = 0;
      Step[3].X = 0; Step[3].Y = -1;

      Norm[0].X = 0; Norm[0].Y = 1;
      Norm[1].X = -1; Norm[1].Y = 0;
      Norm[2].X = 0; Norm[2].Y = -1;
      Norm[3].X = 1; Norm[3].Y = 0;

      this.Line[0].EndByte = 0;
      this.Byte = new byte[MaxByte];
      this.IndPos = new byte[MaxByte];
      this.IndNeg = new byte[MaxByte];
    } //*************** end constructor *********************/


    public int SearchLin(ref CImage Image, Form1 fm1)
    {
      int Lab, rv, x, y;
      for (x = 0; x < MaxByte; x++) Byte[x] = 0;
      if (Step[0].Y < 0) x = Step[0].Y;
      fm1.progressBar1.Value = 0;
      int jump;
      if (CNY > 300) jump = CNY / 100;
      else jump = 2;

      for (y = 0; y < CNY; y += 2)
      {
        if ((y % jump) == jump - 1) fm1.progressBar1.PerformStep();
        for (x = 0; x < CNX; x += 2)
        {
          Lab = Image.Grid[x + CNX * y] & 3;
          if (Lab == 1 || Lab == 3)
          {
            rv = ComponLin(Image.Grid, x, y);
            if (rv < 0)
            {
              MessageBox.Show("SearchLin, Alarm! ComponLin returned " + rv);
              return -1;
            }
          }
        }
      }

      // Starting the search for loops:
      for (y = 0; y < CNY; y += 2)
        for (x = 0; x < CNX; x += 2)
        {
          Lab = Image.Grid[x + CNX * y] & 3;
          if (Lab == 2)
          {
            rv = ComponLin(Image.Grid, x, y);
            if (rv < 0)
            {
              MessageBox.Show("SearchLin, Alarm! ComponLin returned " + rv);
              return -1;
            }
          }
        }
      nByte++;
      return nByte;
    } //****************************** end SearchLin ********************************


    public int TraceLin(byte[] CGrid, int X, int Y, ref iVect2 Pterm, ref int dir)
    /* This method traces a line in the image "Image" with combinatorial coordinates, where the cracks 
      and points of the edges are labeled: bits 0, 1 and 2 of a point contain the label 1 to 4 of the point.
      The label indicates the number of incident edge cracks. Labeled bit 6 indicates that the point 
      already has been put into the queue; labeled bit 7 indicates that the point shoud not been used any more.
      The crack has only one label 1 in bit 0.
      This method traces the edge from one end or branch point to another while changing the parameter "dir".
      ----------*/
    {
      bool atSt_P = false, BP = false, END = false;
      int rv = 0;
      iVect2 Crack, P = new iVect2(0, 0), PixelP, PixelN, StartPO;
      int iShift = -1, iCrack = 0, Lab, Mask = 7;
      P.X = X; P.Y = Y;
      StartPO = P;
      if (nLine2 == 0) nByte = 0;
      else nByte = Line[nLine2 - 1].EndByte + 1;

      int[] Shift = { 0, 2, 4, 6 };
      while (true) //====================================================================
      {
        Crack = P + Step[dir];
        if (Crack.Y == -1 && dir == 3)
        {
          Pterm = P;
          break;
        }
        P = Crack + Step[dir];
        Lab = CGrid[P.X + CNX * P.Y] & Mask;
        switch (Lab)
        {
          case 1: END = true; BP = false; rv = 1; break;
          case 2: BP = END = false; break;
          case 3: BP = true; END = false; rv = 3; break;
          case 4: BP = true; END = false; rv = 3; break;
        }
        PixelP = Crack + Norm[dir];
        PixelN = Crack - Norm[dir];
        IndPos[iCrack] = CGrid[PixelP.X + CNX * PixelP.Y];
        IndNeg[iCrack] = CGrid[PixelN.X + CNX * PixelN.Y];

        if (Lab == 2) CGrid[P.X + CNX * P.Y] = 0;

        iShift++;
        iCrack++;
        if (iShift == 4)
        {
          iShift = 0;
          if (nByte < MaxByte - 1) nByte++;
          else
          {
            return -1;
          }
        }
        Byte[nByte] |= (byte)(dir << Shift[iShift]);

        if (P.X == StartPO.X && P.Y == StartPO.Y) atSt_P = true;
        else atSt_P = false;
        if (atSt_P)
        {
          Pterm = P;
          rv = 2;
          break;
        }

        if (!BP && !END) //---------------------------
        {
          Crack = P + Step[(dir + 1) % 4];
          if (CGrid[Crack.X + CNX * Crack.Y] == 1)
          {
            dir = (dir + 1) % 4;
          }
          else
          {
            Crack = P + Step[(dir + 3) % 4];
            if (CGrid[Crack.X + CNX * Crack.Y] == 1)
            {
              dir = (dir + 3) % 4;
            }
          }
        }
        else
        {
          Pterm = P;
          break;
        } //----------------- end if (!BP && !END) --------------------
      } //======================================= end while ============================================
      Line[nLine2].EndByte = nByte;
      Line[nLine2].nCrack = (ushort)iCrack;

      return rv;
    } //***************************************** end TraceLin ***********************************************


    public int ComponLin(byte[] CGrid, int X, int Y)
    /* Encodes in "CListLines" the lines of the edge component with the point (X, Y) being
      a branch or an end point. Puts the starting point 'Pinp' into the queue and starts
      the 'while' loop. It tests each labeled crack incident with the point 'P' fetched from the queue.
      If the next point of the crack is a branch or an end point, then a short line is saved.
      Otherwise the funktion "TraceLin" is called. "TraceLin" traces a long line, saves the color
      indices at the sides of the line and ends at the point 'Pterm' with the direction 'DirT'. 
      Then the method "FreqInds" assigns the most frequent from the saved color indices to the line.
      If the point 'Pterm' is a branch point then it is put to the queue.
      "ComponLin" returns when the queue is empty.	---------------*/
    {
      int dir, dirT; 
      int LabNext, Mask = 7, rv;
      iVect2 Crack, P, Pinp, PixelN, PixelP, Pnext, Pterm = new iVect2(0, 0);
      Pinp = new iVect2(X, Y); // comb. coord.

      pQ.Put(Pinp);
      while (pQ.Empty() == false) //===========================================================================
      {
        P = pQ.Get();

        for (dir = 0; dir < 4; dir++) //================================================================
        {
          Crack = P + Step[dir];
          if (Crack.X < 0 || Crack.X > CNX - 1 || Crack.Y < 0 || Crack.Y > CNY - 1) continue;

          if (CGrid[Crack.X + CNX * Crack.Y] == 1) //---- ------------ -----------
          {
            PixelP = Crack + Norm[dir]; PixelN = Crack - Norm[dir];
            Pnext = Crack + Step[dir];
            LabNext = CGrid[Pnext.X + CNX * Pnext.Y] & Mask; //Ind0
            if (LabNext == 1 || LabNext == 3 || LabNext == 4)
            {
              Line1[nLine1].x = (ushort)(P.X / 2); 
              Line1[nLine1].y = (ushort)(P.Y / 2);
              Line1[nLine1].Ind0 = CGrid[PixelN.X + CNX * PixelN.Y]; // correct: Ind0=PixelN
              Line1[nLine1].Ind1 = CGrid[PixelP.X + CNX * PixelP.Y];
              if (nLine1 > MaxLine1 - 2)
              {
                MessageBox.Show("ComponLin: Overflow in Line1; return -1");
                return -1;
              }
              else nLine1++;
            }
            if (LabNext == 3 || LabNext == 4) pQ.Put(Pnext);
            if (LabNext == 2) //-------------------------------------------------------------------------
            {
              Line[nLine2].x = (ushort)(P.X / 2); 
              Line[nLine2].y = (ushort)(P.Y / 2); // transformed to standard coord.
              dirT = dir;

              rv = TraceLin(CGrid, P.X, P.Y, ref Pterm, ref dirT);  // NXP, 
              if (nBits3 == 24) FreqInds(nLine2);
              else AverageGray(nLine2);
              if (rv < 0)
              {
                return -1;
              }
              if (nLine2 > MaxLine2 - 2)
              {
                MessageBox.Show("ComponLin: Overflow in Line; return -1");
                return -1;
              }
              else nLine2++;
              if ((CGrid[Pterm.X + CNX * Pterm.Y] & 64) == 0) // '64' is a label for visited points; Pterm is new
              {
                if (rv == 3) //------------------------------------------------------
                {
                  CGrid[Pterm.X + CNX * Pterm.Y] |= 64;
                  pQ.Put(Pterm);
                }
              } //------------ end if  ((CGrid[Pterm.X... ----------------------------------------
            } // ------------- end if (LabNest==2) -----------------------------------------------------
            if ((CGrid[P.X + CNX * P.Y] & Mask) == 1) break;
          } //--------------- end if (CGrid[Crack.X ...==1) ------------------------------------------
        } //================================== end for (dir ... ==========================================
        CGrid[P.X + CNX * P.Y] = 0;
      } //==================================== end while ==================================================
      return 1;
    } //************************************** end ComponLin ************************************************


    public int FreqInds(int il)
    // Finds the most frequent indices in the two halfs of the arrays CListLines.IndPos and CListLines.IndNeg
    // and assigns these indices to the 'Ind*'-elements of the line 'il'.
    {
      int i, ind, j, k, N_Crack;
      bool found;
      int[] IndexN, CounterN, IndexP, CounterP;
      int[] nIndP = { 0, 0 };
      int[] nIndN = { 0, 0 };
      N_Crack = Line[il].nCrack;
      IndexN = new int[2 * N_Crack];
      IndexP = new int[2 * N_Crack];
      CounterN = new int[2 * N_Crack];
      CounterP = new int[2 * N_Crack];
      for (ind = 0; ind < N_Crack; ind++) CounterN[ind] = CounterN[ind + N_Crack] = CounterP[ind] = CounterP[ind + N_Crack] = 0;

      for (i = 0; i < Line[il].nCrack; i++) //==================================
      {
        ind = IndPos[i];
        if (i < Line[il].nCrack / 2) k = 0;
        else k = 1;
        if (nIndP[k] == 0) //--------------------------------------------------
        {
          IndexP[nIndP[k] + N_Crack * k] = ind;
          CounterP[nIndP[k] + N_Crack * k]++;
          nIndP[k]++; // number of different colors saved in IndexP[k]
        }
        else
        {
          found = false;
          for (j = 0; j < nIndP[k]; j++) //===================================
          {
            if (ind == IndexP[j + N_Crack * k])
            {
              CounterP[j + N_Crack * k]++; found = true;
              break;
            }
          } //====================== end for (j... ====================
          if (!found)
          {
            IndexP[nIndP[k] + N_Crack * k] = ind;
            CounterP[nIndP[k] + N_Crack * k]++;
            nIndP[k]++; // number of different colors saved in IndexP[k]
          }
        } //------------------------- end if (nIndP==0) --------------------
        ind = IndNeg[i];
        if (nIndN[k] == 0) //--------------------------------------------------
        {
          IndexN[nIndN[k] + N_Crack * k] = ind;
          CounterN[nIndN[k] + N_Crack * k]++;
          nIndN[k]++; // number of different colors saved in IndexN
        }
        else
        {
          found = false;
          for (j = 0; j < nIndN[k]; j++) //===================================
          {
            if (ind == IndexN[j + N_Crack * k])
            {
              CounterN[j + N_Crack * k]++; found = true;
              break;
            }
          } //====================== end for (j... ====================
          if (!found)
          {
            IndexN[nIndN[k] + N_Crack * k] = ind;
            CounterN[nIndN[k] + N_Crack * k]++;
            nIndN[k]++; // number of different colors saved in IndexP
          }
        } //------------------------ end if (nIndN==0) -------------------- 
      } //========================== end for (i... ==========================

      int iOpt = -1; int MaxCount = 0;
      for (k = 0; k < 2; k++) //==============================================================
      {
        iOpt = -1; MaxCount = 0;
        if (nIndP[k] == 1) //-----------------------------------------------------
          if (k == 0) Line[il].Ind1 = (byte)IndexP[N_Crack * k];
          else Line[il].Ind3 = (byte)IndexP[N_Crack * k];
        else
        {
          for (i = 0; i < nIndP[k]; i++)
            if (CounterP[i + N_Crack * k] > MaxCount)
            {
              MaxCount = CounterP[i + N_Crack * k];
              iOpt = i;
            }
          if (k == 0) Line[il].Ind1 = (byte)IndexP[iOpt + N_Crack * k];
          else Line[il].Ind3 = (byte)IndexP[iOpt + N_Crack * k];
        }

        iOpt = -1; MaxCount = 0;
        if (nIndN[k] == 1) //-----------------------------------------------------
          if (k == 0) Line[il].Ind0 = (byte)IndexN[N_Crack * k];
          else Line[il].Ind2 = (byte)IndexN[N_Crack * k];
        else
        {
          for (i = 0; i < nIndN[k]; i++)
            if (CounterN[i + N_Crack * k] > MaxCount)
            {
              MaxCount = CounterN[i + N_Crack * k];
              iOpt = i;
            }
          if (k == 0) Line[il].Ind0 = (byte)IndexN[iOpt + N_Crack * k];
          else Line[il].Ind2 = (byte)IndexN[iOpt + N_Crack * k];
        } //----------------- end if (nIndN==1) ---------------------------------
      } //=================== end for (k=0; ... ======================================
      return 1;
    } //******************* end FreqInds **************************************************


    public int AverageGray(int il)
    // Finds the average gray value ih the two halfs of the arrays CListLines.IndPos and CListLines.IndNeg
    // and assigns these values to the 'Ind*'-elements of the line 'il'.
    {
      int i, SumPos = 0, SumNeg = 0, nPix = 0;

      if (Line[il].nCrack == 2)
      {
        Line[il].Ind0 = IndNeg[0];
        Line[il].Ind2 = IndNeg[1];
        Line[il].Ind1 = IndPos[0];
        Line[il].Ind3 = IndPos[1];
      }
      else
        if (Line[il].nCrack == 3)
        {
          Line[il].Ind0 = (byte)IndNeg[0];
          Line[il].Ind2 = (byte)((IndNeg[1] + IndNeg[2]) / 2);
          Line[il].Ind1 = (byte)IndPos[0];
          Line[il].Ind3 = (byte)((IndPos[1] + IndPos[2]) / 2);
        }
      if (Line[il].nCrack < 4) return 1;


      for (i = 0; i < Line[il].nCrack / 2; i++)
      {
        SumPos += IndPos[i];
        SumNeg += IndNeg[i];
        nPix++;
      }
      Line[il].Ind0 = (byte)(SumNeg / nPix);
      Line[il].Ind1 = (byte)(SumPos / nPix);

      SumPos = SumNeg = nPix = 0;
      for (i = Line[il].nCrack / 2; i < Line[il].nCrack; i++)
      {
        SumPos += IndPos[i];
        SumNeg += IndNeg[i];
        nPix++;
      }
      Line[il].Ind2 = (byte)(SumNeg / nPix);
      Line[il].Ind3 = (byte)(SumPos / nPix);
      return 1;
    } //******************* end AverageGray **************************************************
  } //**************************************** end public class CListLines *******************************************


  public class CListCode // The class for the final list
  {
    public
      int width, height, nBits, nLine1, nLine2, nByte;
    int[] Param;
    int[] Palette;
    byte[] Corner;
    CCrack[] Line1;
    CLine[] Line;
    byte[] Byte;
    byte[] ByteNew;
    iVect2[] Step;
    int nCodeAfterLine2;

    public CListCode() // Constructor
    {
      this.Step = new iVect2[4];
      for (int i = 0; i < 4; i++) Step[i] = new iVect2();
      this.Step[0].X = 1; this.Step[0].Y = 0;
      this.Step[1].X = 0; this.Step[1].Y = 1;
      this.Step[2].X = -1; this.Step[2].Y = 0;
      this.Step[3].X = 0; this.Step[3].Y = -1;
    }

    public CListCode(int nx, int ny, int nbits, CListLines L) // Constructor
    {
      this.width = nx; //nLine1, nLine2, nByte;
      this.height = ny;
      this.nBits = nbits;

      this.nLine1 = L.nLine1;
      this.Line1 = new CCrack[nLine1];
      for (int i = 0; i < nLine1; i++) Line1[i] = new CCrack();

      this.nLine2 = L.nLine2;
      this.Line = new CLine[nLine2];
      for (int i = 0; i < nLine2; i++) Line[i] = new CLine();

      this.nByte = L.nByte;
      this.Byte = new byte[nByte];

      this.Corner = new byte[4];
      this.Param = new int[6];
      this.Param[0] = nx;
      this.Param[1] = ny;
      this.Param[2] = nbits;
      this.Param[3] = L.nLine1;
      this.Param[4] = L.nLine2;
      this.Param[5] = L.nByte;

      this.Step = new iVect2[4];
      for (int i = 0; i < 4; i++) Step[i] = new iVect2();
      this.Step[0].X = 1; this.Step[0].Y = 0;
      this.Step[1].X = 0; this.Step[1].Y = 1;
      this.Step[2].X = -1; this.Step[2].Y = 0;
      this.Step[3].X = 0; this.Step[3].Y = -1;
    }

    ~CListCode() { } // Default destructor
 
    public int Transform(int nx, int ny, int nbits, int[] Palet, CImage Image, CListLines L, Form1 fm1)
    // Transforms the provisional list "L" to an object of the class "CListCode".
    {
      int i, ib, il, nCode = 0;
      width = nx; height = ny; nBits = nbits;
      nCode += 3 * 4;
      nLine1 = L.nLine1; nLine2 = L.nLine2; nByte = L.nByte;
      nCode += 3 * 4;
      nCodeAfterLine2 = nCode;
      Palette = new int[256];
      for (i = 0; i < 256; i++) Palette[i] = Palet[i];

      nCode += 256 * 4;
      Corner[0] = Image.Grid[1 + (2 * width + 1) * 1]; // this is a gray value or a palette index
      Corner[1] = Image.Grid[2 * width - 1 + (2 * width + 1) * 1];
      Corner[2] = Image.Grid[2 * width - 1 + (2 * width + 1) * (2 * height - 1)];
      Corner[3] = Image.Grid[1 + (2 * width + 1) * (2 * height - 1)];

      nCode += 4;
      int jump;
      if (nLine1 > 300) jump = nLine1 / 33;
      else jump = 2;
      fm1.progressBar1.Step = 1;
      fm1.progressBar1.Value = 0;
      fm1.progressBar1.Visible = true;
      for (il = 0; il < nLine1; il++)
      {
        Line1[il] = L.Line1[il];
        if ((il % jump) == jump - 1) fm1.progressBar1.PerformStep();
      }

      nCode += nLine1 * 6; // "6" is the sizeof(CCrack);

      if (nLine2 > 300) jump = nLine2  / 33;
      else jump = 2;

      for (il = 0; il < nLine2; il++)
      {
        Line[il] = L.Line[il];
        if ((il % jump) == jump - 1) fm1.progressBar1.PerformStep();
      }

      nCode += nLine2 * 14; // sizeof(CLine);

      if (nByte > 300) jump = nByte / 34;
      else jump = 2;
      for (ib = 0; ib < nByte; ib++)
      {
        Byte[ib] = L.Byte[ib];
        if ((ib % jump) == jump - 1) fm1.progressBar1.PerformStep();
      }

      nCode += nByte;

      // The following code is necessary to avoid "Serialize":
      ByteNew = new byte[nCode + 4];
      for (int ik = 0; ik < nCode + 4; ik++) ByteNew[ik] = 0;
      int j = 0;
      ByteNew[j] = (byte)(nCode & 255); j++;
      ByteNew[j] = (byte)((nCode >> 8) & 255); j++;
      ByteNew[j] = (byte)((nCode >> 16) & 255); j++;
      ByteNew[j] = (byte)((nCode >> 24) & 255); j++;

      ByteNew[j] = (byte)(nx & 255); j++;
      ByteNew[j] = (byte)((nx >> 8) & 255); j++;
      ByteNew[j] = (byte)((nx >> 16) & 255); j++;
      ByteNew[j] = (byte)((nx >> 24) & 255); j++;

      ByteNew[j] = (byte)(ny & 255); j++;
      ByteNew[j] = (byte)((ny >> 8) & 255); j++;
      ByteNew[j] = (byte)((ny >> 16) & 255); j++;
      ByteNew[j] = (byte)((ny >> 24) & 255); j++;

      ByteNew[j] = (byte)(nbits & 255); j++;
      ByteNew[j] = (byte)((nbits >> 8) & 255); j++;
      ByteNew[j] = (byte)((nbits >> 16) & 255); j++;
      ByteNew[j] = (byte)((nbits >> 24) & 255); j++;

      ByteNew[j] = (byte)(nLine1 & 255); j++;
      ByteNew[j] = (byte)((nLine1 >> 8) & 255); j++;
      ByteNew[j] = (byte)((nLine1 >> 16) & 255); j++;
      ByteNew[j] = (byte)((nLine1 >> 24) & 255); j++;

      ByteNew[j] = (byte)(nLine2 & 255); j++;
      ByteNew[j] = (byte)((nLine2 >> 8) & 255); j++;
      ByteNew[j] = (byte)((nLine2 >> 16) & 255); j++;
      ByteNew[j] = (byte)((nLine2 >> 24) & 255); j++;

      ByteNew[j] = (byte)(nByte & 255); j++;
      ByteNew[j] = (byte)((nByte >> 8) & 255); j++;
      ByteNew[j] = (byte)((nByte >> 16) & 255); j++;
      ByteNew[j] = (byte)((nByte >> 24) & 255); j++;

      for (int ii = 0; ii < 256; ii++)
      {
        ByteNew[j] = (byte)(Palet[ii] & 255); j++;
        ByteNew[j] = (byte)((Palet[ii] >> 8) & 255); j++;
        ByteNew[j] = (byte)((Palet[ii] >> 16) & 255); j++;
        ByteNew[j] = (byte)((Palet[ii] >> 24) & 255); j++;
      }

      for (int i1 = 0; i1 < 4; i1++) ByteNew[j + i1] = Corner[i1];
      j += 4;

      for (int i2 = 0; i2 < nLine1; i2++)
      {
        ByteNew[j] = (byte)(L.Line1[i2].x & 255); j++;
        ByteNew[j] = (byte)((L.Line1[i2].x >> 8) & 255); j++;
        ByteNew[j] = (byte)(L.Line1[i2].y & 255); j++;
        ByteNew[j] = (byte)((L.Line1[i2].y >> 8) & 255); j++;
        ByteNew[j] = L.Line1[i2].Ind0; j++;
        ByteNew[j] = L.Line1[i2].Ind1; j++;
      }

      for (int i3 = 0; i3 < nLine2; i3++)
      {
        ByteNew[j] = (byte)(L.Line[i3].EndByte & 255); j++;
        ByteNew[j] = (byte)((L.Line[i3].EndByte >> 8) & 255); j++;
        ByteNew[j] = (byte)((L.Line[i3].EndByte >> 16) & 255); j++;
        ByteNew[j] = (byte)((L.Line[i3].EndByte >> 248) & 255); j++;
        ByteNew[j] = (byte)(L.Line[i3].x & 255); j++;
        ByteNew[j] = (byte)((L.Line[i3].x >> 8) & 255); j++;
        ByteNew[j] = (byte)(L.Line[i3].y & 255); j++;
        ByteNew[j] = (byte)((L.Line[i3].y >> 8) & 255); j++;
        ByteNew[j] = (byte)(L.Line[i3].nCrack & 255); j++;
        ByteNew[j] = (byte)((L.Line[i3].nCrack >> 8) & 255); j++;
        ByteNew[j] = L.Line[i3].Ind0; j++;
        ByteNew[j] = L.Line[i3].Ind1; j++;
        ByteNew[j] = L.Line[i3].Ind2; j++;
        ByteNew[j] = L.Line[i3].Ind3; j++;
      }

      for (int i4 = 0; i4 < nByte; i4++) ByteNew[j + i4] = L.Byte[i4];
      j += nByte;
      fm1.progressBar1.Visible = false;
      return nCode;
    } //**************************** end Transform ********************************************


    public int Restore(ref CImage Image, ref CImage Mask, Form1 fm1)
    // Calculates the colors at the borders of the image and along the lines. Constructs the image Mask.
    {
      int c, dir, i, il, LabMask = 250, nComp, x, y;
      if (nBits == 24) nComp = 3;
      else nComp = 1;
      for (i = 0; i < width * height * nBits / 8; i++) Image.Grid[i] = 0;
      for (i = 0; i < width * height; i++) Mask.Grid[i] = 0;

      // Setting the corners
      if (nBits == 24)
      {
        for (c = 0; c < nComp; c++) Image.Grid[c] = (byte)((Palette[Corner[0]] >> 8 * (2 - c)) & 0XFF); // left below
        for (c = 0; c < nComp; c++) Image.Grid[nComp * (width - 1) + c] = (byte)((Palette[Corner[1]] >> 8 * (2 - c)) & 0XFF); // right below
        for (c = 0; c < nComp; c++) Image.Grid[nComp * width * height - nComp + c] = (byte)((Palette[Corner[2]] >> 8 * (2 - c)) & 0XFF); // right on top
        for (c = 0; c < nComp; c++) Image.Grid[nComp * width * (height - 1) + c] = (byte)((Palette[Corner[3]] >> 8 * (2 - c)) & 0XFF); // left on top
      }
      else
      {
        Image.Grid[0] = Corner[0];
        Image.Grid[width - 1] = Corner[1];
        Image.Grid[width * height - 1] = Corner[2];
        Image.Grid[0 + width * (height - 1)] = Corner[3];
      }
      Mask.Grid[0] = Mask.Grid[width - 1] = Mask.Grid[width * height - 1] = Mask.Grid[width * (height - 1)] = (byte)LabMask;

      // Short lines:
      fm1.progressBar1.Value = 0;
      fm1.progressBar1.Step = 1;
      fm1.progressBar1.Visible = true;
      int jump, nStep = 20;
      if (nLine1 > 2*nStep) jump = nLine1 / nStep;
      else jump = 2;

      for (il = 0; il < nLine1; il++) //=====================================================
      {
        if ((il % jump) == jump - 1) fm1.progressBar1.PerformStep();
        dir = ((Line1[il].x >> 14) & 2) | (Line1[il].y >> 15);
        x = Line1[il].x & 0X7FFF; y = Line1[il].y & 0X7FFF;
        if (nBits == 24)
        {
          switch (dir)
          {
            case 0: if (y > 0)
              {
                for (c = 0; c < nComp; c++)
                {
                  Image.Grid[nComp * (x + width * y) + 2 - c] = (byte)((Palette[Line1[il].Ind1] >> 8 * c) & 0XFF);
                  int ind = Line1[il].Ind0;
                  byte col = (byte)((Palette[ind] >> 8 * c) & 0XFF);
                  Image.Grid[nComp * (x + width * (y - 1)) + 2 - c] = col; //(byte)((Palette[Line1[il].Ind0]>>8*c) & 0XFF);
                }
                Mask.Grid[x + width * y] = Mask.Grid[x + width * (y - 1)] = (byte)LabMask;
              }
              break;
            case 1: for (c = 0; c < nComp; c++)
              {
                Image.Grid[nComp * (x + width * y) + 2 - c] = (byte)((Palette[Line1[il].Ind0] >> 8 * c) & 0XFF);
                Image.Grid[nComp * (x - 1 + width * y) + 2 - c] = (byte)((Palette[Line1[il].Ind1] >> 8 * c) & 0XFF);
              }
              Mask.Grid[x + width * y] = Mask.Grid[x - 1 + width * y] = (byte)LabMask;
              break;
            case 2: for (c = 0; c < nComp; c++)
              {
                Image.Grid[nComp * (x - 1 + width * y) + 2 - c] = (byte)((Palette[Line1[il].Ind0] >> 8 * c) & 0XFF);
                Image.Grid[nComp * (x - 1 + width * (y - 1)) + 2 - c] = (byte)((Palette[Line1[il].Ind1] >> 8 * c) & 0XFF);
              }
              Mask.Grid[x - 1 + width * y] = Mask.Grid[x - 1 + width * (y - 1)] = (byte)LabMask;
              break;
            case 3: for (c = 0; c < nComp; c++)
              {
                Image.Grid[nComp * (x + width * (y - 1)) + 2 - c] = (byte)((Palette[Line1[il].Ind1] >> 8 * c) & 0XFF);
                Image.Grid[nComp * (x - 1 + width * (y - 1)) + 2 - c] = (byte)((Palette[Line1[il].Ind0] >> 8 * c) & 0XFF);
              }
              Mask.Grid[x + width * (y - 1)] = Mask.Grid[x - 1 + width * (y - 1)] = (byte)LabMask;
              break;
          }
        }
        else
        {
          switch (dir)
          {
            case 0: Image.Grid[x + width * y] = Line1[il].Ind1;
              Image.Grid[x + width * (y - 1)] = Line1[il].Ind0;
              Mask.Grid[x + width * y] = Mask.Grid[x + width * (y - 1)] = (byte)LabMask;
              break;
            case 1: Image.Grid[x + width * y] = Line1[il].Ind0;
              Image.Grid[x - 1 + width * y] = Line1[il].Ind1;
              Mask.Grid[x + width * y] = Mask.Grid[x - 1 + width * y] = (byte)LabMask;
              break;
            case 2: Image.Grid[x - 1 + width * y] = Line1[il].Ind0;
              Image.Grid[x - 1 + width * (y - 1)] = Line1[il].Ind1;
              Mask.Grid[x - 1 + width * y] = Mask.Grid[x - 1 + width * (y - 1)] = (byte)LabMask;
              break;
            case 3: Image.Grid[x + width * (y - 1)] = Line1[il].Ind1;
              Image.Grid[x - 1 + width * (y - 1)] = Line1[il].Ind0;
              Mask.Grid[x + width * (y - 1)] = Mask.Grid[x - 1 + width * (y - 1)] = (byte)LabMask;
              break;
          }
        }
      } //================= end for (il... nLine1 ... ==========================================

      int cnt = 0, first, last;
      int[] Shift = { 0, 2, 4, 6 }; // Interpolation along the sides of long lines.

      // Long lines:
      if (nLine2 > 300) jump = nLine2 / 20;
      else jump = 2;
      for (il = 0; il < nLine2; il++) //============================================================
      {
        if ((il % jump) == jump - 1) fm1.progressBar1.PerformStep();
        if (il == 0) first = 0;
        else first = Line[il - 1].EndByte + 1;
        last = Line[il].EndByte;
        x = Line[il].x;
        y = Line[il].y;
        int iByte = first, iShift = 0;
        iVect2 V0 = new iVect2(0, 0);
        iVect2 P = V0, P1, PixelP = V0, PixelN = V0; // comb. coordinates

        byte[] ColN = new byte[3], ColP = new byte[3], ColStartN = new byte[3],
                  ColStartP = new byte[3], ColLastN = new byte[3], ColLastP = new byte[3];
        for (c = 0; c < 3; c++) ColN[c] = ColP[c] = ColStartN[c] = ColStartP[c] = ColLastN[c] = ColLastP[c] = 0;

        if (nBits == 24)
        {
          for (c = 0; c < nComp; c++)
          {
            ColStartN[2 - c] = (byte)((Palette[Line[il].Ind0] >> 8 * c) & 255);
            ColStartP[2 - c] = (byte)((Palette[Line[il].Ind1] >> 8 * c) & 255);
            ColLastN[2 - c] = (byte)((Palette[Line[il].Ind2] >> 8 * c) & 255);
            ColLastP[2 - c] = (byte)((Palette[Line[il].Ind3] >> 8 * c) & 255);
          }
        }
        else
        {
          ColStartN[0] = Line[il].Ind0;
          ColStartP[0] = Line[il].Ind1;
          ColLastN[0] = Line[il].Ind2;
          ColLastP[0] = Line[il].Ind3;
        }
 
        P.X = Line[il].x; P.Y = Line[il].y;
        int nCrack = Line[il].nCrack;
        int iC, xx, yy;		// Interpolation along a line:
        for (iC = 0; iC < nCrack; iC++) //=======================================================
        {
          dir = (Byte[iByte] & (3 << Shift[iShift])) >> Shift[iShift];
          switch (dir) // Standard coordinates
          {
            case 0: PixelP = P; PixelN = P + Step[3]; break;
            case 1: PixelP = P + Step[2]; PixelN = P; break;
            case 2: PixelP = P + Step[2] + Step[3]; PixelN = P + Step[2]; break;
            case 3: PixelP = P + Step[3]; PixelN = P + Step[2] + Step[3]; break;
          }
          for (c = 0; c < nComp; c++) //===================================================
          {
            ColN[c] = (byte)((ColLastN[c] * iC + ColStartN[c] * (nCrack - iC - 1)) / (nCrack - 1)); // Interpolation
            ColP[c] = (byte)((ColLastP[c] * iC + ColStartP[c] * (nCrack - iC - 1)) / (nCrack - 1));
          } //========================== end for (c... ================================

          // Assigning colors to intermediate pixels of a line:
          xx = PixelP.X; yy = PixelP.Y;

          if (xx + width * yy < width * height && xx + width * yy >= 0)
          {
            for (c = 0; c < nComp; c++) Image.Grid[c + nComp * xx + nComp * width * yy] = ColP[c]; // Assertion
            Mask.Grid[xx + width * yy] = (byte)LabMask;
          }
          xx = PixelN.X; yy = PixelN.Y;

          if (xx + width * yy < width * height && xx + width * yy >= 0)
          {
            for (c = 0; c < nComp; c++) Image.Grid[c + nComp * xx + nComp * width * yy] = ColN[c];
            Mask.Grid[xx + width * yy] = (byte)LabMask;
          }
          P1 = P;
          P = P + Step[dir];

          iShift++;
          if (iShift == 4)
          {
            iShift = 0;
            iByte++;
          }
        } //=============================== end for (iC... ==============================
        int nZero = 0;
        if (P.X > 0 && P.Y > 0 && P.X < width && P.Y < height)
        {
          if (Image.Grid[nComp * (P.X + width * P.Y) + 0] == 0) nZero++;
          if (Image.Grid[nComp * (P.X - 1 + width * P.Y) + 0] == 0) nZero++;
          if (Image.Grid[nComp * (P.X - 1 + width * (P.Y - 1)) + 0] == 0) nZero++;
          if (Image.Grid[nComp * (P.X + width * (P.Y - 1)) + 0] == 0) nZero++;
          if (nZero == 2) cnt++; 
        }
      } //================================= end for (il... nLine2 ... ===========================

      Mask.Grid[0] = Mask.Grid[width - 1] = Mask.Grid[width * height - 1] = Mask.Grid[width * (height - 1)] = (byte)LabMask;
      //fm1.progressBar1.Visible = false;
      return 1;
    } //*********************** end Restore **********************************************************


    public void WriteCode(string Name, int nCode)
    {
      Stream stream = File.Open(Name, FileMode.Create);
      long rv = 0;
      stream.Write(ByteNew, 0, nCode);
      rv = stream.Length;
      stream.Close();
    } //****************************** end WriteCode ********************
  } //************************* end public class CListCode ****************************************************
}
