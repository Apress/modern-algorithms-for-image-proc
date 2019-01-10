using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace WFsegmentAndComp
{
  public struct iVect2
  { public
      int X, Y;
      public iVect2(int x, int y)
      { this.X = x;
        this.Y = y;
      }
  }

  public class CQue
  { public
    int input, output, Len;
    bool full;
    int[] Array;

    public CQue(int len)  // Constructor
	  { this.Len = len;
      input=0;
		  output=0;
		  full=false;
		  Array=new int[Len];				 
	  }

    public int Put(int V)
    { if (full) 
    { MessageBox.Show("The Que is full; input=" + input);
      return -1;
    }
    Array[input]=V; 
    if (input==Len-1) input=0;
    else input++;
    return 1;
    }

    public int Get()
    { int er=-1;
    if (Empty() ) 
    { MessageBox.Show("The CQue is empty");
      return er;
    }
    int iV=Array[output]; 
    if (output==Len-1) output=0;
    else output++;
    if (full) full = false;
    return iV;
    }

    public bool Empty()
    { if (input==output && !full) return true;
      return false;
    }
  } //***************** end class CQue *************************


  public class CImageComp
  {
    public byte[] Grid;
    public int[] Lab;
    public Color[] Palette;
    public int width, height, N_Bits, nLoop, denomProg; 

    public CImageComp(){} 

		public CImageComp(int nx, int ny, int nbits) // constructor
    { this.width=nx; 
      this.height=ny; 
      this.N_Bits=nbits;
      Grid=new byte[width*height*(N_Bits/8)];
			Palette=new Color[256];
    } 

    public CImageComp(int nx, int ny, int nbits, byte[] img) // constructor
    { this.width=nx; 
      this.height=ny; 
      this.N_Bits=nbits; 
      Grid=new byte[width*height*(N_Bits/8)];
      Lab=new int[width*height*(N_Bits/8)];
      for (int i=0; i<width*height*N_Bits/8; i++) Grid[i]=img[i];
      Palette = new Color[256];
    } 

		public CImageComp(ref CImageComp inp) // copy-constructor
		{ width=inp.width; 
      height=inp.height; 
      N_Bits=inp.N_Bits;
			Grid=new byte[width*height*(N_Bits/8)];
			Lab=new int[width*height];
			for (int i=0; i<width*height*(N_Bits/8); i++) Grid[i]=inp.Grid[i];
      for (int p = 0; p < 256; p++) Palette[p] = inp.Palette[p];
		}



    public void Copy(CImage inp, bool full)
    // Copies the image "inp" into "this". If full==false then the array "Grid"
    // will be alloced but its content will not be copied.
    { int i, size;
      if (inp.N_Bits==8) size=inp.width*inp.height;
      else size=3*inp.width*inp.height;
      N_Bits=inp.N_Bits; 
      width=inp.width; 
      height=inp.height;
      Grid=new byte[size];
      Lab=new int[size];
      if (full) for (i=0; i<size; i++) Grid[i]=inp.Grid[i];
    }


    public int Root(int k)
    {	for (int i=0; i<1000; i++)
	    {	if (k<0 || k>width*height-1) return -1;

		    if (k==Lab[k]) return k;
		    k=Lab[k];
	    }
	    return -1;
    }


    public int SetEquivalent(int i, int k)
    { int Ri=i, Rk=k;
      while(Ri!=Lab[Ri])
      { Ri=Lab[Ri];
      }
      while(Rk!=Lab[Rk])
      { Rk=Lab[Rk];
      } 

	    if (Ri<0 || Rk<0) return -2;
      if (Ri==Rk) return 1;
	    if (Ri<Rk) Lab[Rk]=Ri; 
	    else Lab[Ri]=Rk; 
	    return 1;
    }


    public int SecondRun()
    // Sets Comp equal to the index of components from 1 to count-1. 
    // Returns the number of components-1. Index=0 is reserved.
    {	int count=1, label;
	    for (int i=0;  i<width*height; i++)
	    {	label=Lab[i];
		    if (label==i)
		    {	
			    Lab[i]=count; 
          count++;
		    }
		    else	Lab[i]=Lab[label];
	    }
	    return count-1;
    }


    public bool EquNaliDir(iVect2 P, int Dir, int ColorP)
    { /* Returns 'true' if the pixel lying in the direction "dir" (0 to 7) 
      from the pixel P is connected with P accordimg to the EquNaLi membership rule. 
      Can be called for any pixel, not only in the neighborhood of a singular point.
      Standard coordinates. "ColorP" is the color of P. --*/
      int cntP=0, cntN=0, col1=-1, col2=-1, colN=-1, x, y;

	    if (N_Bits==24) 
	    { MessageBox.Show("EquNaliDir is not designed for color images; return -1");
		    return false;
	    }
      switch (Dir)
      {
        case 0: if (P.X + 1 + width * P.Y < width * height) colN = Grid[P.X + 1 + width * P.Y];
                else colN = 0;
                return (colN==ColorP);

        case 1: if (P.X<width-1 && P.Y<height-1) colN=Grid[P.X+1+width*(P.Y+1)]; 
						    if (colN!=ColorP) return false;
                if (P.X<width-1) col1=Grid[P.X+1+width*P.Y];
                if (P.Y<height-1) col2=Grid[P.X+width*(P.Y+1)];
	              if (col1!=col2 || col1==ColorP) return true;
                break;
        case 2: if (P.Y<height-1) colN=Grid[P.X+width*(P.Y+1)]; 
                return (colN==ColorP);
        case 3: if (P.X>0 && P.Y<height-1) colN=Grid[P.X-1+width*(P.Y+1)]; 
                if (colN!=ColorP) return false;
                if (P.X>0) col1=Grid[P.X-1+width*P.Y];
                if (P.X>0) col2=Grid[P.X+width*(P.Y+1)];
                if (col1!=col2 || col1==ColorP) return true;
                break;
        case 4: if (P.X>0) colN=Grid[P.X-1+width*P.Y];
						    return (colN==ColorP);
        case 5: if (P.X>0 && P.Y>0) colN=Grid[P.X-1+width*(P.Y-1)]; 
                if (colN!=ColorP) return false;
                if (P.X>0) col1=Grid[P.X-1+width*P.Y];
                if (P.Y>0) col2=Grid[P.X+width*(P.Y-1)];
                if (col1!=col2 || col1==ColorP) return true;
                break;
        case 6: if (P.Y>0) colN=Grid[P.X+width*(P.Y-1)]; 
                return (colN==ColorP);
        case 7: if (P.X<width-1 && P.Y>0) colN=Grid[P.X+1+width*(P.Y-1)]; 
                if (colN!=ColorP) return false;
                if (P.X<width-1) col1=Grid[P.X+1+width*P.Y];
                if (P.Y>0) col2=Grid[P.X+width*(P.Y-1)];
                if (col1!=col2 || col1==ColorP) return true;
                break;
      } //:::::::::::::::::::::: end switch ::::::::::::::::::::::::::::::::::

      int imin, jmin;
      if (Dir==3 || Dir==5) imin=-2; else imin=-1;
      if (Dir==5 || Dir==7) jmin=-2; else jmin=-1;
      for (int j=jmin; j<jmin+4; j++) //============
      { y=P.Y+j;
        if (y<0 || y>=height) continue;
        for (int i=imin; i<imin+4; i++) //=========
        { x=P.X+i;
          if (x<0 || x>=width) continue;
          int col=Grid[x+width*y];
          if (col==ColorP) cntP++;
          if (col==col1) cntN++;
        } //============= end for (int i... =======
      } //=============== end for (int j... =========
      if (cntP==cntN) return (ColorP>col1);
      if (cntP<cntN) return true;
      return false;
    } //***************** end EquNaliDir ****************************
  

    public int ComponentsE(Form1 fm1) 
    { /* Labels connected components of "this" in "this->Lab" and returns 
          the number of components. Uses the EquNaLi connectedness. --*/
	    int Dir, light, i, nComp=0, rv, x1 = 0, y1 = 0;
      bool adjac;
      iVect2 P = new iVect2(0, 0);
	    for (int k = 0; k < width*height; k++) Lab[k] = k;

      int Len = height, jump, nStep = 50;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      fm1.progressBar1.Step = 1;
      for (int y = 0; y < height; y++) //==================================================
	    { 
        if ((y % jump) == jump - 1) fm1.progressBar1.PerformStep();
        for (int x = 0; x < width; x++) //===========================================
		    {	i = x + width*y; // "i" is the index of the actual pixel
			    light=Grid[i];
			    P.X=x; P.Y=y;
			    int nmax;
			    if (y==0) nmax=0;
			    else nmax=3;
			    for (int n=0; n<=nmax; n++) //==== the four preceding neighbors =========
			    { switch(n)
				    { case 0: x1=x-1; y1=y; break;   // West
					    case 1: x1=x-1; y1=y-1; break; // North-West
					    case 2: x1=x;   y1=y-1; break; // North
					    case 3: x1=x+1; y1=y-1; break; // North-East
				    }
				    if (x1<0 || x1>=width) continue;
				    Dir=n+4;
				    int indN=x1+width*y1;
				    int lightNeib=Grid[indN];
				    if (lightNeib!=light) continue;
				    if ((Dir & 1)==0) adjac=true;
				    else
					    adjac=EquNaliDir(P, Dir, light);
				    if (adjac) 	
              rv=SetEquivalent(i, indN); // Sets Lab[i] or Lab[indN] equal to Root				    
			    } //==================== end for (int n ... =============================
        } //====================== end for (int x ... ===============================
      } //======================== end for (int y ... =================================
	    nComp=SecondRun(); // Writes indices of components from 1 to nComp to "Comp".
	    return nComp;	// nComp+1 is the number of indices
    } //***************************** end ComponentsE ***********************************

 
    public int Neighb(int index, int n, int width, int height)
    { int Nei = 0, x0, y0, x, y;
	    y0=index/width; 
      x0=index-width*y0; 
	    switch(n)
	    { case 0: y=y0-1; x=x0-1; 
						    if (x>=0 && x<width && y>=0 && y<height) Nei=index-width-1;
						    else Nei=-1;	break; 
		    case 1: x=x0; y=y0-1;
						    if (x>=0 && x<width && y>=0 && y<height) Nei=index-width;
						    else Nei=-1;	break;
		    case 2: x=x0+1; y=y0-1;
						    if (x>=0 && x<width && y>=0 && y<height) Nei=index-width+1;	
						    else Nei=-1;	 break;
		    case 3: x=x0+1; y=y0;
						    if (x>=0 && x<width && y>=0 && y<height) Nei=index+1;		
						    else Nei=-1;	 break;
		    case 4: x=x0+1; y=y0+1;
						    if (x>=0 && x<width && y>=0 && y<height) Nei=index+width+1;	
						    else Nei=-1;	 break;
		    case 5: x=x0; y=y0+1;
						    if (x>=0 && x<width && y>=0 && y<height) Nei=index+width;		
						    else Nei=-1;	 break;
		    case 6: x=x0-1; y=y0+1;
						    if (x>=0 && x<width && y>=0 && y<height) Nei=index+width-1;		
						    else Nei=-1;	 break;
		    case 7: x=x0-1; y=y0+1;
						    if (x>=0 && x<width && y>=0 && y<height) Nei=index-1;		
						    else Nei=-1;	 break;
	    }
	    return Nei;
    } //*********************** end Neighb *********************************


    public int NeighbEqu(int index, int n, int width, int height)
    { int Nei = 0, x0, y0, x, y;
	    y0=index/width; x0=index-width*y0; 
	    switch(n)
	    { case 0: y=y0; x=x0+1; 
						    if (x>=0 && x<width && y>=0 && y<height) Nei=index+1;
						    else Nei=-1;	 
						    break;
		    case 1: x=x0+1; y=y0+1;
						    if (x>=0 && x<width && y>=0 && y<height) Nei=index+width+1;
						    else Nei=-1;	break;
		    case 2: x=x0; y=y0+1;
						    if (x>=0 && x<width && y>=0 && y<height) Nei=index+width;	
						    else Nei=-1;	 break;
		    case 3: x=x0-1; y=y0+1;
						    if (x>=0 && x<width && y>=0 && y<height) Nei=index+width-1;		
						    else Nei=-1;	 break;
		    case 4: x=x0-1; y=y0;
						    if (x>=0 && x<width && y>=0 && y<height) Nei=index-1;	
						    else Nei=-1;	 break;
		    case 5: x=x0-1; y=y0-1;
						    if (x>=0 && x<width && y>=0 && y<height) Nei=index-width-1;		
						    else Nei=-1;	 break;
		    case 6: x=x0; y=y0-1;
						    if (x>=0 && x<width && y>=0 && y<height) Nei=index-width;		
						    else Nei=-1;	 break;
		    case 7: x=x0+1; y=y0-1;
						    if (x>=0 && x<width && y>=0 && y<height) Nei=index-width+1;		
						    else Nei=-1;	 break;
	    }
	    return Nei;
    }


    public int Breadth_First_Search(int root, int lab)
    { int light, gvP, Index, Len=1000, Nei;
      bool rv;
	    iVect2 P = new iVect2(0, 0);
	    CQue Q = new CQue(Len);
	    light=Grid[root];   
	    if (Lab[root]==0) Lab[root]=lab; 

      Q.Put(root);
	    while(!Q.Empty()) //=============================
	    { Index=Q.Get();
		    gvP=Grid[Index];
		    P.Y=Index/width; P.X=Index-width*P.Y;
		    for (int n=0; n<8; n++) //====================
		    { Nei=NeighbEqu(Index, n, width, height);
			    rv=EquNaliDir(P, n, gvP);
			    if (Nei<0) continue;
			    if (Grid[Nei]==light && Lab[Nei]==0 && rv)
			    { 
				    Lab[Nei]=lab;
				    Q.Put(Nei);
			    }
		    } //============== end for (n... ==============
	    } //================ end while ====================
	    return 1;
    } //****************** end Breadth-First-Search *************


    public int LabelC(Form1 fm1)
    { int index, lab=0; 
	    for (index=0; index<width*height; index++) Lab[index]=0;

      int Len = height, jump, nStep = 50;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      fm1.progressBar1.Step = 1;
	    for (int y = 0; y < height; y++) //========================
      {
        if (y % jump == jump - 1) fm1.progressBar1.PerformStep();
        for (int x = 0; x < width; x++) //=================
		    { index = x + width*y;
			    if (Lab[index] == 0)
			    {	lab++;
				    Breadth_First_Search(index, lab);
			    }
        } //================= end for (int x...============
      } //=================== end for (int x...==============
	    return lab;
    } //********************* end LabelC ***************************

    
    public int MakePalette( ref int nPal)
    {	int r, g, b; 
      byte Red, Green, Blue;
      int ii=0;
      for (r = 1; r <= 8; r++) // Colors of the palette
        for (g = 1; g <= 8; g++)
        for (b = 1; b <= 4; b++)
        { Red=(byte)(32*r);  if (r == 8) Red=255;
          Green = (byte)(32 * g); if (g == 8) Green = 255;
          Blue= (byte)(64*b);  if (b == 4) Blue=255;
          Palette[ii] = Color.FromArgb(Red, Green, Blue);
          ii++;
        }
	    nPal = 4*ii;
	    return 1;
    } //************************************ end MakePalette ******************************
  } //*********************** end class CImageComp ************************************
} //************************* end namespace **********************************************
