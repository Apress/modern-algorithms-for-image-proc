using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;



using System.Windows.Forms;


namespace WFrestoreLin
{
  public class CImage
  { public
    byte[] Grid;
    public int width, height, N_Bits;

    public CImage(CImage inp) // copy-constructor
    { this.width = inp.width; 
      this.height=inp.height; 
      this.N_Bits=inp.N_Bits;
      this.Grid=new byte[width*height*(N_Bits/8)];
      for (int i=0; i<width*height*(N_Bits/8); i++) Grid[i]=inp.Grid[i];
    }

    public CImage(int nx, int ny, int nbits) // constructor
    { this.width = nx;
      this.height = ny;
      this.N_Bits = nbits;
		  Grid=new byte[width*height*(N_Bits/8)];
    } 

    public CImage(int nx, int ny, int nbits, byte[] img) // constructor
    { this.width = nx;
      this.height = ny;
      this.N_Bits = nbits;
      Grid=new byte[width*height*(N_Bits/8)];
      for (int i=0; i<width*height*N_Bits/8; i++) Grid[i]=img[i];
    } 

    ~CImage(){}

    public int AverageSmall(CImage Inp)
    // Filters the image "Inp" with the weights "Weight" in a 3x3 window and saves the results
    // in "this.Grid". 
    { N_Bits=Inp.N_Bits; width=Inp.width; height=Inp.height;
	    int[] Weight = {1,2,1,2,4,2,1,2,1};

	    int SumWeight=0, Sum=0;
	    for (int y=0; y<height; y++) //============================================================================
	    {	for (int x=0; x<width; x++) //========================================================================
		    {	if (Inp.N_Bits==8) //----------------------------------------------------------------------
			    { Sum=0; SumWeight=0;
				
				    for (int j=-1; j<2; j++)
				    for (int i=-1; i<2; i++)
				    { int xx=x+i; int yy=y+j;
					    if (xx>=0 && xx<width && yy>=0 && yy<height)
					    { Sum+=Weight[i+1+3*(j+1)]*Inp.Grid[xx+width*yy];
						    SumWeight+=Weight[i+1+3*(j+1)];
				 
						    if (SumWeight<=0)
						    { 
							    return -1;
						    }
					    }
				    }
				    Grid[x+width*y]=(byte)((Sum+SumWeight/2)/SumWeight);
			    } //-------------------------------------- end if (Inp.N_Bits... --------------------------
			    else //Inp.N_Bits==24
          {
            int[] SumC = new int[3];
				    for (int c=0; c<3; c++) SumC[c]=0;

				    for (int j=-1; j<2; j++)
				    for (int i=-1; i<2; i++)
				    { int xx=x+i; int yy=y+j;
					    if (xx>=0 && xx<width && yy>=0 && yy<height)
					    { for (int c=0; c<3; c++) SumC[c]+=Weight[i+1+3*(j+1)]*Inp.Grid[3*(xx+width*yy)+c];
						    SumWeight+=Weight[i+1+3*(j+1)];
				 	      if (SumWeight<=0)   return -1;
						  }
				    }
				    for (int c=0; c<3; c++) Grid[3*(x+width*y)+c]=(byte)((SumC[c]+SumWeight/2)/SumWeight);
			    } //-------------------------------------- end if (Inp.N_Bits... --------------------------
		    } //======================================== end for (x... ====================================
	    } //========================================== end for (y... ======================================
	    return 1;
    } //******************************************** end AverageSmall *************************************

    public int Smooth(ref CImage Mask, Form1 fm1)
    // Calculates the average colors between "gvbeg" and "gvend" and saves them in the image "this".
    // This is a digital automaton with the states S=1 at Mask>0 and S=2 at Mask==0, but S is not used.
    // The variable "mpre" has the value of "Mask" in the previouse pixel.
    {
      int c, cnt, LabMask = 250, msk, mpre, x, xx, xbeg, xend, ybeg, yend, y, yy;
      int[] Col = new int[3], ColBeg = new int[3], ColEnd = new int[3];

      int nbyte;
      if (N_Bits == 24) nbyte = 3;
      else nbyte = 1;

      // Smoothing the borders:
      // Border at y=0:
      y = 0; cnt = 0; xbeg = 0; mpre = 200;
      for (c = 0; c < nbyte; c++) ColBeg[c] = Grid[c];

      int jump, Len = width, nStep = 4;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (x = 0; x < width; x++) //==========================================================
      {
        if ((x % jump) == jump - 1) fm1.progressBar1.PerformStep();
        msk = Mask.Grid[x + width * y];
        if (mpre > 0 && msk == 0) //----------------------------------------
        {
          cnt = 1; xbeg = x - 1;
          for (c = 0; c < nbyte; c++) ColBeg[c] = Grid[nbyte * (x - 1 + width * y) + c];
        }
        if (mpre == 0 && msk == 0) //----------------------------------------
        {
          cnt++;
        }
        if (mpre == 0 && msk > 0) //----------------------------------------
        {
          cnt++; xend = x;
          for (c = 0; c < nbyte; c++) ColEnd[c] = Grid[nbyte * (x + width * y) + c];
          for (xx = xbeg + 1; xx < xend; xx++) //==========================================
          {
            for (c = 0; c < nbyte; c++)
              Grid[nbyte * (xx + width * y) + c] = (byte)((ColBeg[c] * (xend - xx) + ColEnd[c] * (xx - xbeg)) / cnt);
            Mask.Grid[xx + width * y] = (byte)LabMask;
          } //============== end for (xx... ========================================
        }
        mpre = msk;
      } //=============== end for (x=0; ... ===========================================	

      // Border at y=height-1:
      y = height - 1; cnt = 0; xbeg = 0; mpre = 200;
      for (c = 0; c < nbyte; c++) ColBeg[c] = Grid[nbyte * width * y + c];

      Len = width;
      nStep = 4;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (x = 0; x < width; x++) //==========================================================
      {
        if ((x % jump) == jump - 1) fm1.progressBar1.PerformStep();
        msk = Mask.Grid[x + width * y];
        if (mpre > 0 && msk == 0) //----------------------------------------
        {
          cnt = 1; xbeg = x - 1;
          for (c = 0; c < nbyte; c++) ColBeg[c] = Grid[nbyte * (x - 1 + width * y) + c];
        }
        if (mpre == 0 && msk == 0) //----------------------------------------
        {
          cnt++;
        }
        if (mpre == 0 && msk > 0) //----------------------------------------
        {
          cnt++; xend = x;
          for (c = 0; c < nbyte; c++) ColEnd[c] = Grid[nbyte * (x + width * y) + c];
          for (xx = xbeg + 1; xx < xend; xx++)
          {
            for (c = 0; c < nbyte; c++)
              Grid[nbyte * (xx + width * y) + c] = (byte)((ColBeg[c] * (xend - xx) + ColEnd[c] * (xx - xbeg)) / cnt);
            Mask.Grid[xx + width * y] = (byte)LabMask;
          }
        }
        mpre = msk;
      } //=============== end for (x=0; ... ===========================================	


      // Border at x=0
      x = 0; cnt = 0; ybeg = 0; mpre = 200;
      for (c = 0; c < nbyte; c++) ColBeg[c] = Grid[nbyte * (x + width * 0) + c];

      Len = height;
      nStep = 4;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (y = 0; y < height; y++) //=================================================
      {
        if ((y % jump) == jump - 1) fm1.progressBar1.PerformStep();
        msk = Mask.Grid[x + width * y];
        if (mpre > 0 && msk == 0) //----------------------------------------
        {
          cnt = 1; ybeg = y - 1;
          for (c = 0; c < nbyte; c++) ColBeg[c] = Grid[nbyte * (x + width * (y - 1)) + c];
        }
        if (mpre == 0 && msk == 0) //----------------------------------------
        {
          cnt++;
        }
        if (mpre == 0 && msk > 0) //----------------------------------------
        {
          cnt++; yend = y;
          for (c = 0; c < nbyte; c++) ColEnd[c] = Grid[nbyte * (x + width * y) + c];
          for (yy = ybeg + 1; yy < yend; yy++)
          {
            for (c = 0; c < nbyte; c++)
            {
              Col[c] = (ColBeg[c] * (yend - yy) + ColEnd[c] * (yy - ybeg)) / cnt;
              Grid[nbyte * (x + width * yy) + c] = (byte)Col[c];
            }
            Mask.Grid[x + width * yy] = (byte)LabMask;
          }
        }
        mpre = msk;
      } //=============== end for (y=0; ... ==================================

      // Border at x=width-1
      x = width - 1; cnt = 0; ybeg = 0; mpre = 200;
      for (c = 0; c < nbyte; c++) ColBeg[c] = Grid[nbyte * (x + width * 0) + c];
      Len = height;
      nStep = 4;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (y = 0; y < height; y++) //=================================================
      {
        if ((y % jump) == jump - 1) fm1.progressBar1.PerformStep();
        msk = Mask.Grid[x + width * y];
        if (mpre > 0 && msk == 0) //----------------------------------------
        {
          cnt = 1; ybeg = y - 1;
          for (c = 0; c < nbyte; c++) ColBeg[c] = Grid[nbyte * (x + width * (y - 1)) + c];
        }
        if (mpre == 0 && msk == 0) //----------------------------------------
        {
          cnt++;
        }
        if (mpre == 0 && msk > 0) //----------------------------------------
        {
          cnt++; yend = y;
          for (c = 0; c < nbyte; c++) ColEnd[c] = Grid[nbyte * (x + width * y) + c];
          for (yy = ybeg + 1; yy < yend; yy++)
          {
            for (c = 0; c < nbyte; c++)
            {
              Col[c] = (ColBeg[c] * (yend - yy) + ColEnd[c] * (yy - ybeg)) / cnt;
              Grid[nbyte * (x + width * yy) + c] = (byte)Col[c];
            }
            Mask.Grid[x + width * yy] = (byte)LabMask;
          }
        }
        mpre = msk;
      } //=============== end for (y=0; ... ==================================

      // End smoothin border; Smooth on "x":
      Len = height;
      nStep = 4;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (y = 0; y < height; y++) //========================================================
      {
        if ((y % jump) == jump - 1) fm1.progressBar1.PerformStep();
        cnt = 0; xbeg = 0; mpre = 200;
        for (c = 0; c < nbyte; c++) ColBeg[c] = Grid[nbyte * width * y + c];
        for (x = 0; x < width; x++) //=====================================================
        {
          msk = Mask.Grid[x + width * y];
          if (mpre > 0 && msk == 0) //----------------------------------------
          {
            cnt = 1; xbeg = x - 1;
            for (c = 0; c < nbyte; c++) ColBeg[c] = Grid[nbyte * (x - 1 + width * y) + c];
          }
          if (mpre == 0 && msk == 0) //----------------------------------------
          {
            cnt++;
          }
          if (mpre == 0 && msk > 0) //----------------------------------------
          {
            cnt++; xend = x;
            for (c = 0; c < nbyte; c++) ColEnd[c] = Grid[nbyte * (x + width * y) + c];
            for (xx = xbeg + 1; xx < xend; xx++)
            {
              for (c = 0; c < nbyte; c++)
                Grid[nbyte * (xx + width * y) + c] = (byte)((ColBeg[c] * (xend - xx) + ColEnd[c] * (xx - xbeg)) / cnt);
            }
          }
          mpre = msk;
        } //=============== end for (x=0; ... ========================================
      } //================= end for (y=0; ... ==========================================

      // Smooth on "y":
      Len = width;
      nStep = 4;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (x = 0; x < width; x++) //=====================================================
      {
        if ((x % jump) == jump - 1) fm1.progressBar1.PerformStep();
        cnt = 0; ybeg = 0; mpre = 200;
        for (c = 0; c < nbyte; c++) ColBeg[c] = Grid[nbyte * (x + width * 0) + c];
        for (y = 0; y < height; y++) //=================================================
        {
          msk = Mask.Grid[x + width * y];
          if (mpre > 0 && msk == 0) //----------------------------------------
          {
            cnt = 1; ybeg = y - 1;
            for (c = 0; c < nbyte; c++) ColBeg[c] = Grid[nbyte * (x + width * (y - 1)) + c];
          }
          if (mpre == 0 && msk == 0) //----------------------------------------
          {
            cnt++;
          }
          if (mpre == 0 && msk > 0) //---------------------------------------------------------------
          {
            cnt++; yend = y; for (c = 0; c < nbyte; c++) ColEnd[c] = Grid[nbyte * (x + width * y) + c];
            for (yy = ybeg + 1; yy < yend; yy++)
            {
              for (c = 0; c < nbyte; c++)
              {
                Col[c] = (Grid[nbyte * (x + width * yy) + c] + (ColBeg[c] * (yend - yy) + ColEnd[c] * (yy - ybeg)) / cnt) / 2;
                Grid[nbyte * (x + width * yy) + c] = (byte)Col[c];
              }
            }
          }
          mpre = msk;
        } //=============== end for (y=0; ... ===================================================
      } //================= end for (x=0; ... =====================================================

      // Solving the Laplace's equation:
      int i;
      double fgv, omega = 1.4 / 4.0, dMaxLap = 0.0, dTH = 1.0;
      double[] dGrid = new double[width * height * nbyte];
      double[] Lap = new double[3];
      for (i = 0; i < width * height * nbyte; i++) dGrid[i] = (double)Grid[i];

      Len = 50;
      nStep = 6;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (int iter = 0; iter < 50; iter++) //-----------------------------------------------------------------------------
      { 
        if ((iter % jump) == jump - 1) fm1.progressBar1.PerformStep();
        for (y = 1; y < height - 1; y++)
          for (x = 1; x < width - 1; x++)
          {
            if (Mask.Grid[x + width * y] == 0 && Math.Abs((x - y)) % 2 == 0)
              for (c = 0; c < nbyte; c++)
              {
                Lap[c] = 0.0;
                Lap[c] += dGrid[nbyte * (x + width * (y - 1)) + c];
                Lap[c] += dGrid[nbyte * (x - 1 + width * y) + c];
                Lap[c] += dGrid[nbyte * (x + 1 + width * y) + c];
                Lap[c] += dGrid[nbyte * (x + width * (y + 1)) + c];
                Lap[c] -= 4.0 * dGrid[nbyte * (x + width * y) + c];
                fgv = dGrid[nbyte * (x + width * y) + c] + omega * Lap[c];
                if (fgv > 255.0) fgv = 255.0;
                if (fgv < 0.0) fgv = 0;
                dGrid[nbyte * (x + width * y) + c] = fgv;
              }
          }
        // Smooth at Math.Abs((x - y)) % 2 == 1
        for (y = 1; y < height - 1; y++)
          for (x = 1; x < width - 1; x++)
          {
            if (Mask.Grid[x + width * y] == 0 && Math.Abs((x - y)) % 2 == 1)
              for (c = 0; c < nbyte; c++)
              {
                Lap[c] = 0.0;
                Lap[c] += dGrid[nbyte * (x + width * (y - 1)) + c];
                Lap[c] += dGrid[nbyte * (x - 1 + width * y) + c];
                Lap[c] += dGrid[nbyte * (x + 1 + width * y) + c];
                Lap[c] += dGrid[nbyte * (x + width * (y + 1)) + c];
                Lap[c] -= 4.0 * dGrid[nbyte * (x + width * y) + c];
                fgv = dGrid[nbyte * (x + width * y) + c] + omega * Lap[c];
                if (fgv > 255.0) fgv = 255.0;
                if (fgv < 0.0) fgv = 0;
                dGrid[nbyte * (x + width * y) + c] = fgv; //(int)(fgv);
              }
          }

        dMaxLap = 0.0; // Calculating MaxLap:
        for (y = 1; y < height - 1; y++)
          for (x = 1; x < width - 1; x++) //===================================
          {
            if (Mask.Grid[x + width * y] == 0) //----------------------------
            {
              for (c = 0; c < nbyte; c++) //==========================
              {
                Lap[c] = 0.0;
                Lap[c] += dGrid[nbyte * (x + width * (y - 1)) + c];
                Lap[c] += dGrid[nbyte * (x - 1 + width * y) + c];
                Lap[c] += dGrid[nbyte * (x + 1 + width * y) + c];
                Lap[c] += dGrid[nbyte * (x + width * (y + 1)) + c];
                Lap[c] -= 4.0 * dGrid[nbyte * (x + width * y) + c];
                if (Math.Abs(Lap[c]) > dMaxLap) dMaxLap = Math.Abs(Lap[c]);
              } //================= end for (c=0; =================
            } //------------------- end if (Mask... -----------------
          } //===================== end for (x=1; ... =================

        int ii;
        for (ii = 0; ii < width * height * nbyte; ii++) Grid[ii] = (byte)dGrid[ii];

        if (dMaxLap < dTH)  break;
      } //------------------------------------ end for (iter... ------------------------------------------

      return 0;
    } //************************************* end Smooth ***********************************************

  }
}
