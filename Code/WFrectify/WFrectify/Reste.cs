using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WFtest
{
  public class iVect2
  { public
      int X, Y;
      public iVect2(){}
      public iVect2(int x, int y) // constructor
      { this.X=x;
        this.Y=y;
      }

      public static iVect2 operator +(iVect2 a, iVect2 b)
      {
        return new iVect2(a.X + b.X, a.Y + b.Y);
      }

      public static iVect2 operator -(iVect2 a, iVect2 b)
      { 
        return new iVect2(a.X-b.X, a.Y-b.Y);
      }

      public static iVect2 operator -(iVect2 a)
      { 
        return new iVect2(-a.X, -a.Y);
      }
    
      public static bool operator == (iVect2 a, iVect2 b)
      { if (a.X == b.X && a.Y == b.Y) return true;
        return false;
      }

      public static bool operator !=(iVect2 a, iVect2 b)
      {
        if (a.X != b.X || a.Y != b.Y) return true;
        return false;
      }
  } //********************* end public class iVect2 ***************************


  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e) 
    {
      // This is a code to demonstrate using the class iVect2:
      iVect2[] Vert = new iVect2[10];
      iVect2 PS = new iVect2();
      PS.X = 2;
      PS.Y = 3;
      Vert[0] = PS;
      MessageBox.Show("Before: Vert[0]=(" + Vert[0].X + "," + Vert[0].Y + ")");
      PS.X = 19;
      MessageBox.Show("After: Vert[0]=(" + Vert[0].X + "," + Vert[0].Y + ")");

    }
  }
}





// Reste.cs; 19.10.2016.
// Die alte Version aus "Rect_Central", Schleife "for (int X…
      xx = ((double)X - P0X) * Math.Cos(phiY) + width / 2 ;
      yy = ((double)Y - P0Y) * Math.Cos(phiX) + height / 2 ;
      //double Z = F - CX * (xx - width / 2) - CY * (yy - height / 2);
      double Z = F - CX * (xx - width / 2) - CY * (yy - height / 2);

      //fx = width / 2 + F * (xx - width / 2) / (CX * (xx - width / 2) + CY * (yy - height / 2) + F);
      fx = (width / 2 + F * (xx - width / 2) / Z);              // *1.07;
      fy = (height / 2 + F * (yy - height / 2) / Z);               //  - (double) X / 190.9) *1.07;
      x = (int)(fx + 0.5); // -30;
      y = (int)(fy + 0.5); // +70;
      if (X == 0 && Y == 0)
        cof = P0X + P1X;

      //if ((X == 0 || X == Result.width - 1) && (Y == 0 || Y == Result.height - 1))
      if (X == P0X && Y == P0Y)
      {
        MessageBox.Show("X=" + X + " Y=" + Y + " xx=" + xx + " fx=" + fx);
        cof = fx;
      } 
           

Die Parameter der Gleichungen am 21.10.2016 um 5:50.
Wahrscheinlich ohne Veränderungen.
      for (int i = 0; i < 4; i++)
      {
        A = B = C = D = 0.0;
        A = (F / (v[i].X - width / 2) - CX);
        B = -CY;
        C = width / 2 * F / (v[i].X - width / 2) - CX * width / 2 - CY * height / 2 + F;
        D = -CX;
        E = (F / (v[i].Y - height / 2) - CY);
        G = height / 2 * F / (v[i].Y - height / 2) - CX * width / 2 - CY * height / 2 + F;
        Det = A * E - B * D;
        xc[i] = (C * E - B * G) / Det;
        yc[i] = (A * G - C * D) / Det;
        zc[i] = F - CX * (xc[i] - width / 2) - CY * (yc[i] - height / 2); // richtig
      }
}

    public void Rect_Uni(Point[] v, bool CUT, ref CImage Result) 
    // Calculates the corners of the rectifyed image and then makes a bilinear transformation
    { int MaxSize, x, y, X, Y;
	    double alphaX, alphaY, betaX, betaY, Correction, F, Height, phiX, phiY, 
		    RedX, RedY, Width, M0X, M1X, M3Y, M2Y;
      M0X=(v[0].X+v[1].X)/2.0; 
      M1X=(v[2].X+v[3].X)/2.0; 
      M3Y=(v[0].Y+v[3].Y)/2.0; 
      M2Y=(v[1].Y+v[2].Y)/2.0;
      RedY=(double)(v[1].Y-v[0].Y)/(double)(v[2].Y-v[3].Y);  
      RedX=(double)(v[3].X-v[0].X)/(double)(v[2].X-v[1].X);  
      if (width > height) MaxSize = width;
      else MaxSize = height;
	    F=1.0*(double)(MaxSize);
	    alphaY=Math.Atan2(F,(double)(width/2-M0X));
	    betaY=Math.Atan2(F,(double)(M1X-width/2));
	    phiY=Math.Atan2(RedY*Math.Sin(betaY)-Math.Sin(alphaY),Math.Cos(alphaY)+RedY*Math.Cos(betaY));
      //double kat = (double)(height / 2 - M3Y);
      
	    alphaX=Math.Atan2(F,(double)(height/2-M3Y));
      betaX = Math.Atan2(F, (double)(M2Y - height / 2));
      phiX = Math.Atan2(RedX * Math.Sin(betaX) - Math.Sin(alphaX), Math.Cos(alphaX) + RedX * Math.Cos(betaX));
	    Width=F*(Math.Cos(alphaY)/Math.Sin(alphaY+phiY)+Math.Cos(betaY)/Math.Sin(betaY-phiY));
	    Height=F*(Math.Cos(alphaX)/Math.Sin(alphaX+phiX)+Math.Cos(betaX)/Math.Sin(betaX-phiX));
      if (Width < 0.0 || Height < 0.0)
      {
        MessageBox.Show("The clicked area does not contain the center of the image");
        return;
      }
      if (width < height) Correction = (double)(width) / 154.0;
	    else Correction=(double)(height)/154.0;

	    if (CUT)
	    { M0X=3.0*Correction; 
		    M3Y=5.0*Correction;
	    }
	    else
	    { M0X=((double)(width)-Width)*0.5; 
		    M3Y=((double)(height)-Height)*0.5;
	    }
	    M1X=M0X+Width;
	    M2Y=M3Y+Height;
      
	    CImage Out;
	    if (CUT)
		    Out=new CImage((int)M1X, (int)M2Y, N_Bits);
	    else
		    Out=new CImage(width, height, N_Bits);
	    Result.Copy(Out, 0); 
      double cof,fx, fy; 
      cof=1.0/((M1X-M0X)*(M2Y-M3Y));
	    // Bilinear transformation:
      for (Y = 0; Y < Result.height; Y++) //=========== over the rectified image ====================
      {
        for (X = 0; X < Result.width; X++) //=====================================================
	      { fx=v[0].X*(double)(X-M1X)*(Y-M2Y) - v[1].X*(double)(X-M1X)*(Y-M3Y) +
	           v[2].X*(double)(X-M0X)*(Y-M3Y) - v[3].X*(double)(X-M0X)*(Y-M2Y);
	        fy=v[0].Y*(double)(X-M1X)*(Y-M2Y) - v[1].Y*(double)(X-M1X)*(Y-M3Y) +
	           v[2].Y*(double)(X-M0X)*(Y-M3Y) - v[3].Y*(double)(X-M0X)*(Y-M2Y);
          x=(int)(fx*cof);
          y = (int)(fy * cof);
          if (x>=0 && x<width && y>=0 && y<height)
            if (N_Bits==24)
            { for (int ic=0; ic<3; ic++)
                Result.Grid[ic+3*X+3*Result.width*(Result.height -1-Y)]=Grid[ic+3*x+3*width*y];
				    }
            else
              Result.Grid[X + Result.width * (Result.height - 1 - Y)] = Grid[x + width * y];
        } //================================ end for (X... ==============================
      } //================================== end for (Y... ================================
    } //************************************ end Rect_Uni *****************************************

