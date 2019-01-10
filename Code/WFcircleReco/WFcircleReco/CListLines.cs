using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace WFcircleReco
{
  public struct iVect2
  {
    public
      int X, Y;
    public iVect2(int x, int y) // constructor
    {
      X = x;
      Y = y;
    }

    public static iVect2 operator +(iVect2 a, iVect2 b)
    {
      return new iVect2(a.X + b.X, a.Y + b.Y);
    }

    public static iVect2 operator -(iVect2 a, iVect2 b)
    {
      return new iVect2(a.X - b.X, a.Y - b.Y);
    }

    public static iVect2 operator -(iVect2 a)
    {
      return new iVect2(-a.X, -a.Y);
    }

    public static bool operator ==(iVect2 a, iVect2 b) // is used in 'CheckComb'
    {
      if (a.X == b.X && a.Y == b.Y) return true;
      return false;
    }

    public static bool operator !=(iVect2 a, iVect2 b)
    {
      if (a.X != b.X || a.Y != b.Y) return true;
      return false;
    } 
  } //********************* end public struct iVect2 ***************************/


  public class CQue
  {
    public
      int input, output, Len;
    bool full;
    iVect2[] Array;
    public CQue() { } // Default constructor
    public CQue(int len) // Constructor
    {
      Len = len;
      input = 0;
      output = 0;
      full = false;
      Array = new iVect2[Len];
      for (int i = 0; i < Len; i++) Array[i] = new iVect2();
    }

    public int Put(iVect2 V)
    {
      if (full) return -1;
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
      iVect2 iV = new iVect2();
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
  } //***************************** end public class CQue **************************


  public class CPolygon
  {
    public int firstVert, lastVert, firstArc, lastArc, nCrack; // 20 byte
    public bool closed, smooth; // 2 byte; all together 22 byte
  } //**************** end public class CPolygon ***************************


  public struct CArc
  {
    public float xb, yb, xe, ye, xm, ym, rad;  // 20 byte
  }




  public class CSetting
  {
    public double MaxCrit, Eps, MinAngle;
    public int MinRad, MaxRad;
    public CSetting(double maxcrit, double eps, int minrad, int maxrad, double minangle) // constructor
    {
      MaxCrit = maxcrit;
      Eps = eps;
      MinRad = minrad;
      MaxRad = maxrad;
      MinAngle = minangle;
    }
  }

  public class ARC
  {
    public int iPol, Sign, Start, nPoints;
    public bool Suitable, inCircle;
    public double Angle, Mx, MinLong, My, R, EndAz, StartAz;
    public double[] SUM = new double[13];
  }

  public class CelArc
  {
    public int iPol, Circle, Ori, Sign, Start, nPoints;
    public bool arcOK;
    public double Angle, Mx, MinLong, My, R, EndAz, StartAz;
    public double[] SUM;
    public CelArc() // constructor
    {
      SUM = new double[13];
    }
  }

  public class CCircle
  {
    public double Mx, My, R;
    public double Angle;
    public int sign;
    public bool good;
    public CCircle() // default constructor
    {
      Mx = 0.0;
      My = 0.0;
      R = 0.0;
      sign = 1;
      good = true;  
    } 

    public CCircle(int mx, int my, int r, int Sign) // constructor
    {
      Mx = mx;
      My = my;
      R = r;
      sign = Sign;
    } //******************* end constructor ********************************


    public void SetPar(double mX, double mY, double r, double angle, int Sign)
    {
      Mx = mX;
      My = mY;
      R = r;
      sign = Sign;
      Angle = angle;
    }
  } //******************* end class CCircle ********************************


  public class CListLines
  {
    public
    int nArc, nPolygon, nVert, MaxPoly, MaxVert, MaxArc;

    public static double PosSectX, PosSectY, NegSectX, NegSectY;
    public iVect2 Far;

    public iVect2[] Step;
    public iVect2[] Norm;

    public CPolygon[] Polygon;
    public CSetting Set;
    public iVect2[] Vert;
    public CArc[] Arc;
    public CelArc[] elArc;
    public CQue pQ;

    public CListLines() { } // Default constructor

    public CListLines(int maxL2, int maxV, int maxArc) // constructor
    {
      MaxPoly = maxL2;
      MaxVert = maxV;
      MaxArc = maxArc;
      pQ = new CQue(1000); // necessary to find connected components
      nArc = 0;
      nPolygon = 0;
      Polygon = new CPolygon[MaxPoly];
      int i = 0;
      for (i = 0; i < MaxPoly; i++) Polygon[i] = new CPolygon();
       Set = new CSetting(6.0, 1.1, 100, 180, 2.0); // constructor

      Vert = new iVect2[MaxVert];
      for (i = 0; i < MaxVert; i++) Vert[i] = new iVect2();
      Arc = new CArc[MaxArc];
      for (i = 0; i < MaxArc; i++) Arc[i] = new CArc();
      elArc = new CelArc[MaxArc];
      for (i = 0; i < MaxArc; i++) elArc[i] = new CelArc();
      Step = new iVect2[4];
      for (i = 0; i < 4; i++) Step[i] = new iVect2();
      Norm = new iVect2[4];
      for (i = 0; i < 4; i++) Norm[i] = new iVect2();

      Step[0].X = 1; Step[0].Y = 0;
      Step[1].X = 0; Step[1].Y = 1;
      Step[2].X = -1; Step[2].Y = 0;
      Step[3].X = 0; Step[3].Y = -1;

      Norm[0].X = 0; Norm[0].Y = 1;
      Norm[1].X = -1; Norm[1].Y = 0;
      Norm[2].X = 0; Norm[2].Y = -1;
      Norm[3].X = 1; Norm[3].Y = 0;

    } //*************** end constructor *********************


    public void CheckSmooth(double maxSin, double maxProportion)
    // Checks all polygons. A polygon is smooth if the realtive number of large
    // angles between its edges is less than "maxProportion".
    {
      int first = 0, last = 0, numKnick, sign = 1;
      double SinusAngle = 0.0, Len1 = 0.0, Len2 = 0.0; // lengthes of two vectors
      for (int ip = 0; ip < nPolygon; ip++) //=========================================================
      {
        if (ip == 0) first = 0;
        else first = Polygon[ip - 1].lastVert + 1;
        last = Polygon[ip].lastVert;
        numKnick = 0;
        for (int iv = first; iv < last; iv++) //=====================================================
        {
          Len1 = Len2;
          Len2 = Math.Sqrt((Vert[iv + 1].X - Vert[iv].X) * (Vert[iv + 1].X - Vert[iv].X) +
                            (Vert[iv + 1].Y - Vert[iv].Y) * (Vert[iv + 1].Y - Vert[iv].Y));
          if (iv == first) continue;
          SinusAngle = ((Vert[iv].X - Vert[iv - 1].X) * (Vert[iv + 1].Y - Vert[iv].Y) -
                      (Vert[iv + 1].X - Vert[iv].X) * (Vert[iv].Y - Vert[iv - 1].Y)) / Len1 / Len2;
          if (iv == first + 1)
          {
            if (SinusAngle < 0.0) sign = -1;
            else sign = 1;
          }
          if (SinusAngle * sign > maxSin) numKnick++;
        } //========================= end for (int iv ... ===========================================
        if ((double)numKnick / (double)(last - first + 1) > maxProportion) Polygon[ip].smooth = false;
        else Polygon[ip].smooth = true;
      } //=========================== end for (int ip ... =============================================
    } //****************************** end CheckSmooth **************************************************


    public void PrintCircles(CCircle[] Circle, int nCircle, Form1 fm1)
    // Prints parameters of 'maxNumber' arcs ('maxNumber' is calculated here) with arc 
    // numbers starting with 'from' and containing at least 'minPoints' pointss.
    {
      int locationX = 0; int locationY = 150;  //int numRow = 46;
      int labWidth = 140; int labHeight = 15; //int lab2Height = 11;

      fm1.PanelWithGrid = new Panel();
      int panelWidth = fm1.pictureBox1.Width;
      int panelHeight = fm1.pictureBox1.Height;
      fm1.PanelWithGrid.SetBounds(locationX, locationY - 5, panelWidth, panelHeight);

      fm1.PanelWithGrid.BorderStyle = BorderStyle.Fixed3D;
      fm1.Controls.Add(fm1.PanelWithGrid);
      fm1.PanelWithGrid.BringToFront();
      Label[] longLabel = new Label[52];

      int number = 0, maxNumber = 50;

      int y = 0;
      number = 0;
      for (int ic = 0; ic < nCircle; ic++) //===============================================
      {
        number++;
        if (number > maxNumber) break;

        longLabel[y] = new Label();
        fm1.PanelWithGrid.Controls.Add(longLabel[y]);
        longLabel[y].SetBounds(0 * labWidth, y * labHeight, 750, labHeight);
        longLabel[y].Text = "Circle " + ic + ": Center=(" + Math.Round(Circle[ic].Mx, 1) +
          "; " + Math.Round(Circle[ic].My, 1) + ") Radius=" + Math.Round(Circle[ic].R, 1) +
          " Angle=" + Math.Round(Circle[ic].Angle, 1) + " good=" + Circle[ic].good;
        y++;
      } //============================== end for (int ia... ====================================
    } //******************************** end PrintCircless *******************************************


    public int SearchPoly(ref CImage Comb, double eps, Form1 fm1)
    {
      int Lab = 0, rv = 0, x, y, CNX = Comb.width, CNY = Comb.height;
      for (y = 0; y < CNY; y += 2) //=====================================================
      {
        for (x = 0; x < CNX; x += 2) //=================================================
        {
          Lab = Comb.Grid[x + CNX * y] & 135;
          if (Lab == 1 || Lab == 3 || Lab == 4)
          {
            rv = ComponPoly(Comb, x, y, eps);
            if (rv < 0)
            {
              MessageBox.Show("SearchPoly1, Alarm! ComponPoly returned " + rv);
              return -1;
            }
          }
        } //============================= end for (x ... ====================
      } //=============================== end for (y ... =======================

      for (y = 0; y < CNY; y += 2)
        for (x = 0; x < CNX; x += 2)
        {
          Lab = Comb.Grid[x + CNX * y] & 7;
          if (Lab == 2)
          {
            rv = ComponPoly(Comb, x, y, eps);
            if (rv < 0)
            {
              MessageBox.Show("SearchPoly2, Alarm! ComponPoly returned " + rv);
              return -1;
            }
          }
        }
      return nVert;
    } //****************************** end SearchPoly ********************************


    int CheckComb(iVect2 StartP, iVect2 P, double eps, ref iVect2 Vect)
    // This is the new function for polygonal approximation with the sector method.
    // It replaces the old function "secomb"; 'P' is in standard coord.
    {
      double Length, Sin, Cos, Proj, PosTangX, PosTangY, NegTangX, NegTangY;
      iVect2 Line = new iVect2();

      if (StartP == P)
      {
        PosSectX = -1000.0;
        return 0;
      }
      Line.X = P.X - StartP.X;
      Line.Y = P.Y - StartP.Y;
      Length = Math.Sqrt(Math.Pow((double)Line.X, 2.0) + Math.Pow((double)Line.Y, 2.0));
      if (Length < eps)  return 0;

      if (PosSectX == -1000.0)
      {
        Sin = eps / Length;
        Cos = Math.Sqrt(1.0 - Sin * Sin);
        PosSectX = Line.X * Cos - Line.Y * Sin;
        PosSectY = Line.X * Sin + Line.Y * Cos;
        NegSectX = Line.X * Cos + Line.Y * Sin;
        NegSectY = -Line.X * Sin + Line.Y * Cos;
        Far = P;
        return 0;
      }

      Proj = (double)((Far.X - StartP.X) * (P.X - Far.X) + (Far.Y - StartP.Y) * (P.Y - Far.Y));
      if (Proj < -eps * Math.Sqrt(Math.Pow((double)(Far.X - StartP.X), 2.0) + Math.Pow((double)(Far.Y - StartP.Y), 2.0)))
      {
        Vect = Far;
        PosSectX = -1000.0;
        return 2;
      }
      if (Proj >= 0.0) Far = P;

      if (PosSectX * Line.Y - PosSectY * Line.X > 0.0 || NegSectX * Line.Y - NegSectY * Line.X < 0.0)
      {
        PosSectX = -1000.0;
        return 1;
      }


      if (PosSectX != -1000.0 && Length > eps)
      {
        Sin = eps / Length; Cos = Math.Sqrt(1 - Sin * Sin);
        PosTangX = Line.X * Cos - Line.Y * Sin;
        PosTangY = Line.X * Sin + Line.Y * Cos;
        NegTangX = Line.X * Cos + Line.Y * Sin;
        NegTangY = -Line.X * Sin + Line.Y * Cos;

        if (NegSectX * NegTangY - NegSectY * NegTangX > 0.0)
        {
          NegSectX = NegTangX;
          NegSectY = NegTangY;
        }
        if (PosSectX * PosTangY - PosSectY * PosTangX < 0.0)
        {
          PosSectX = PosTangX;
          PosSectY = PosTangY;
        }
      }
      return 0;
    } //*************************** end CheckComb ************************************


    public double ParArea(int iv, iVect2 P)
    // Returns the area of the parallelogram spanning three points 
    // Vert[iv - 2], Vert[iv - 1] and Vert[iv].
    {
      if (iv < 2) return -1.0;
      double a = 0.0, b = 0.0, c = 0.0, d = 0.0;
      a = (double)(Vert[iv - 1].X - Vert[iv - 2].X);
      b = (double)(Vert[iv - 1].Y - Vert[iv - 2].Y);
      c = (double)(P.X / 2 - Vert[iv - 1].X);
      d = (double)(P.Y / 2 - Vert[iv - 1].Y);
      return a * d - b * c;
    }




    public int TraceAppNew(CImage Comb, int X, int Y, double eps, ref iVect2 Pterm, ref int dir)
    /* This method traces a line in the image "Comb" with combinatorial coordinates, where the cracks 
      and points of the edges are labeled: bits 0, 1 and 2 of a point contain the label 1 to 4 of the point.
      The label indicates the number of incident edge cracks. Bits 3 to 6 are not used. Labeled bit 7 
      indicates that the point shoud not been used any more. The crack has only one label 1 in bit 0.
      This function traces the edge from one end or branch point to another one while changing the parameter "dir".
      It makes polygonal approximation with precision "eps" and saves STANDARD coordinats in "Vert".
      ----------*/
    {
      int br, Lab, rv = 0;
      bool BP = false, END = false;
      bool atSt_P = false; // CHECK = true;
      iVect2 Crack, P, P1, Pold, Pstand, StartEdge, StartLine, Vect;
      Crack = new iVect2();
      P = new iVect2();
      P1 = new iVect2();
      Pold = new iVect2();
      Pstand = new iVect2();
      StartEdge = new iVect2();
      StartLine = new iVect2();
      Vect = new iVect2();

      int iCrack = 0;
      P.X = X; P.Y = Y;
      Pstand.X = X / 2;
      Pstand.Y = Y / 2;
      P1.X = Pold.X = P.X;
      P1.Y = Pold.Y = P.Y;
      StartEdge.X = X / 2;
      StartEdge.Y = Y / 2;
      StartLine.X = X / 2;
      StartLine.Y = Y / 2;

      int[] Shift = { 0, 2, 4, 6 }; 

      int StartVert = nVert;
      Vert[nVert].X = Pstand.X;
      Vert[nVert].Y = Pstand.Y;
      nVert++;
      Vect = new iVect2();
      int CNX = Comb.width;
      int CNY = Comb.height;
      CheckComb(StartEdge, Pstand, eps, ref Vect);

      while (true) //====================================================================
      {
        Crack.X = P.X + Step[dir].X;
        Crack.Y = P.Y + Step[dir].Y;
        if (Comb.Grid[Crack.X + CNX * Crack.Y] == 0)
        {
          MessageBox.Show("TraceAppNew, error: dir=" + dir + " the Crack=(" + Crack.X + "," + Crack.Y +
          ") has label 0; Start=(" + X + "; " + Y + "); iCrack=" + iCrack + " P=(" + P.X + "; "  + P.Y + 
          " Lab(P)=" + (Comb.Grid[P.X + CNX * P.Y] & 7) + "). return -1");

          MessageBox.Show("Lab(Start)=" + (Comb.Grid[X + CNX * Y] & 7) + " P+Step[0]=" + (Comb.Grid[P.X + 1 + CNX * P.Y] & 7) +
          " +Step[1]=" + (Comb.Grid[P.X + CNX * (P.Y + 1)] & 7) + " +Step[2]=" + (Comb.Grid[P.X - 1 + CNX * P.Y] & 7) +
          " +Step[3]=" + (Comb.Grid[P.X + CNX * (P.Y - 1)] & 7));
        }
        P.X = P1.X = Crack.X + Step[dir].X;
        P.Y = P1.Y = Crack.Y + Step[dir].Y;
        Pstand.X = P.X / 2;
        Pstand.Y = P.Y / 2;

        br = CheckComb(StartEdge, Pstand, eps, ref Vect);

        Lab = Comb.Grid[P.X + CNX * P.Y] & 7; // changed on Nov. 1
        switch (Lab)
        {
          case 1: END = true; BP = false; rv = 1; break;
          case 2: BP = END = false; break;
          case 3: BP = true; END = false; rv = 3; break;
          case 4: BP = true; END = false; rv = 4; break;
        }
        if (Lab == 2) Comb.Grid[P.X + CNX * P.Y] = 0; // deleting all labels of P
        iCrack++;

 
        if (br > 0) //--------------------------------------------------------------
        {
          if (nVert >= MaxVert - 1)
          {
            MessageBox.Show("TraceAppNew: Overflow in 'Vert'; X=" + X + " Y=" + Y + " nVert=" + nVert);
            return -1;
          }

          if (br == 1)
          {
            if (nVert > (StartVert + 1) && ParArea(nVert, Pold) == 0.0)
            {
              Vert[nVert - 1].X = Pold.X / 2;
              Vert[nVert - 1].Y = Pold.Y / 2;
            }
            else
            {
              Vert[nVert].X = Pold.X / 2;
              Vert[nVert].Y = Pold.Y / 2;
            }
          }
          else
          {
            if (nVert > (StartVert + 1) && ParArea(nVert, Pold) == 0.0) Vert[nVert - 1] = Vect;
            else Vert[nVert] = Vect;
          } //-------------------- end if (br == 1) -------------------------------

          if (nVert > (StartVert + 1) && ParArea(nVert, Pold) == 0.0)
          {
            StartEdge = Vert[nVert - 1];
          }
          else
          {
            StartEdge = Vert[nVert];
            if (StartEdge.X > 0 || StartEdge.Y > 0) nVert++;
          }
          br = 0;
        } //------------------ end if (br > 0) --------------------------------------

        atSt_P = (Pstand == StartLine);
        if (atSt_P)
        {
          Pterm.X = P.X; // Pterm is a parameter of TraceAppNew
          Pterm.Y = P.Y;
          Polygon[nPolygon].lastVert = nVert - 1;
          Polygon[nPolygon].closed = true;
          rv = 2;
          break;
        }

        if (!atSt_P && (BP || END))
        {
          Pterm.X = P.X; // Pterm is a parameter of TraceAppNew
          Pterm.Y = P.Y;
          Vert[nVert].X = Pstand.X;
          Vert[nVert].Y = Pstand.Y;

          Polygon[nPolygon].lastVert = nVert;
          Polygon[nPolygon].closed = false;
          nVert++;
          if (BP) rv = 3;
          else rv = 1;
          break;
        }

        if (!BP && !END) //---------------------------
        {
          Crack.X = P.X + Step[(dir + 1) % 4].X;
          Crack.Y = P.Y + Step[(dir + 1) % 4].Y;
          if (Comb.Grid[Crack.X + CNX * Crack.Y] == 1)
          {
            dir = (dir + 1) % 4;
          }
          else
          {
            Crack.X = P.X + Step[(dir + 3) % 4].X;
            Crack.Y = P.Y + Step[(dir + 3) % 4].Y;
            if (Comb.Grid[Crack.X + CNX * Crack.Y] == 1) dir = (dir + 3) % 4;
          }
        }
        else break;
        Pold.X = P.X;
        Pold.Y = P.Y;
      } //======================================= end while ============================================
      Polygon[nPolygon].nCrack = iCrack;
      return rv;
    } //***************************************** end TraceAppNew ***********************************************


    public int ComponPoly(CImage Comb, int X, int Y, double eps)
    /* Encodes in "CArcs" the polygones of the edge component with the point (X, Y) being
      a branch or an end point. Puts the starting point 'Pinp' into the queue and starts
      the 'while' loop. It tests each labeled crack incident to the point 'P' fetched from the queue.
      If the next point of the crack is a branch or an end point, then the crack is being ignorred.
      Otherwise the funktion "TraceAppNew" is called. "TraceAppNew" traces the edge until the next end or
      branch point and calulates the verices of the approximating polygon. The tracing ends at the 
      point 'Pterm' with the direction 'DirT'. If the point 'Pterm' is a branch point then it is put 
      to the queue. "ComponPoly" returns when the queue is empty.	---------------*/
    {
      int dir, dirT;
      int LabNext, rv;
      iVect2 Crack, P, Pinp, PixN, PixP, Pnext, Pterm;
      Crack = new iVect2();
      P = new iVect2();
      Pinp = new iVect2();
      Pnext = new iVect2();
      PixN = new iVect2();
      PixP = new iVect2();
      Pterm = new iVect2();
      Pinp.X = X;
      Pinp.Y = Y; // comb. coord.
      int CNX = Comb.width;
      int CNY = Comb.height;
      int ValC, ValN, ValP;
      bool closed = (Comb.Grid[Pinp.X + CNX * Pinp.Y] & 7) == 2;

      if (closed) //-------------------------------------------------------------
      {
        for (dir = 0; dir < 4; dir++) //========================================================================
        {
          Crack = Pinp + Step[dir];
          PixP = Crack + Norm[dir];
          PixN = Crack - Norm[dir];
          ValC = Comb.Grid[Crack.X + CNX * Crack.Y];
          ValN = Comb.Grid[PixN.X + CNX * PixN.Y];
          ValP = Comb.Grid[PixP.X + CNX * PixP.Y];
          if (ValC == 1 && ValP <= ValN)
          {
            Polygon[nPolygon].firstVert = nVert;
            dirT = dir;
            rv = TraceAppNew(Comb, Pinp.X, Pinp.Y, eps, ref Pterm, ref dirT);

            if (rv < 0)
            {
              MessageBox.Show("ComponPoly, Alarm! TraceAppNew returned " + rv + ". return -1.");
              return -1;
            }
            if (nPolygon > MaxPoly - 1)
            {
              MessageBox.Show("ComponPoly: Overflow in Polygon; nPolygon=" + nPolygon + " MaxPoly=" + MaxPoly);
              return -1;
            }
            else nPolygon++;
            break;
          } //--------------------------- end if (ValC == 1 ------------------------------------
        } //============================= end for (dir... =========================================
        return 1;
      }  //-------------------------------- end if (closed) -----------------------------------------
          
      pQ.Put(Pinp);
      while (!pQ.Empty()) //===========================================================================
      {
        P = pQ.Get();
        if ((Comb.Grid[P.X + CNX * P.Y] & 128) != 0) continue;

        for (dir = 0; dir < 4; dir++) //================================================================
        {
          Crack = P + Step[dir];
          if (Crack.X < 0 || Crack.X > CNX - 1 || Crack.Y < 0 || Crack.Y > CNY - 1) continue;
          if (Comb.Grid[Crack.X + CNX * Crack.Y] == 1) //---- ------------ -----------
          {
            Pnext.X = Crack.X + Step[dir].X;
            Pnext.Y = Crack.Y + Step[dir].Y;
            LabNext = Comb.Grid[Pnext.X + CNX * Pnext.Y] & 7; // changed on Nov. 1 2015

            if (LabNext >= 3) pQ.Put(Pnext);
            //Crack = Pinp + Step[dir];
            PixP = Crack + Norm[dir];
            PixN = Crack - Norm[dir];
            ValC = Comb.Grid[Crack.X + CNX * Crack.Y];
            ValN = Comb.Grid[PixN.X + CNX * PixN.Y];
            ValP = Comb.Grid[PixP.X + CNX * PixP.Y];
 
            if (ValP > ValN) continue;

            if (LabNext == 2) //-------------------------------------------------------------------------
            {
              Polygon[nPolygon].firstVert = nVert;
              dirT = dir;
              Pterm = new iVect2();
              rv = TraceAppNew(Comb, P.X, P.Y, eps, ref Pterm, ref dirT);

              if (rv < 0)
              {
                MessageBox.Show("ComponPoly, Alarm! TraceAppNew returned " + rv + ". return -1.");
                return -1;
              }
              if (nPolygon > MaxPoly - 1)
              {
                MessageBox.Show("ComponPoly: Overflow in Polygon; nPolygon=" + nPolygon + " MaxPoly=" + MaxPoly);
                return -1;
              }
              else nPolygon++;
              if ((Comb.Grid[Pterm.X + CNX * Pterm.Y] & 128) == 0 && rv >= 3) pQ.Put(Pterm);
            } // ------------- end if (LabNest==2) -----------------------------------------------------
            if ((Comb.Grid[P.X + CNX * P.Y] & 7) == 1) break; // from point a single direction
          } //--------------- end if (Comb.Grid[Crack.X ...==1) ------------------------------------------
        } //================================== end for (dir ... ==========================================
        Comb.Grid[P.X + CNX * P.Y] |= 128;
      } //==================================== end while ==========================================
      return 1;
    } //************************************** end ComponPoly ************************************************


    public int DrawPolygons(PictureBox pictureBox, CImage EdgeIm, Form1 fm1)
    {
      bool INSCRIPT = false;
      Brush blackBrush, yellowBrush;
      blackBrush = new System.Drawing.SolidBrush(Color.Black);
      yellowBrush = new System.Drawing.SolidBrush(Color.Yellow);

      Pen greenPen = new System.Drawing.Pen(Color.Green, 2);

      double Scale1 = fm1.Scale1;
      bool Rect_on = true;
      Rectangle rect0, rect1, rect2;
      rect0 = new Rectangle(0, 0, (int)((pictureBox.Width - fm1.marginX) / fm1.Scale1),
        (int)((pictureBox.Height - fm1.marginY) / fm1.Scale1));
      fm1.g2.FillRectangle(blackBrush, rect0);
      fm1.pictureBox2.Refresh();
      int x1 = 0, x2 = 0, y1 = 0, y2 = 0;
      for (int iPol = 0; iPol < nPolygon; iPol++) //=========================
      {
        int first = Polygon[iPol].firstVert;
        int lenPoly = Polygon[iPol].lastVert - first + 1;
        if (lenPoly < 3) continue;
        bool smooth = Polygon[iPol].smooth;
        int x = Vert[first].X;
        int y = Vert[first].Y;
        String s = iPol.ToString();
        String s1 = nPolygon.ToString();
        if (INSCRIPT && lenPoly > 5)
          fm1.Ins2.Write(s, (int)(Scale1 * x), (int)(Scale1 * y), fm1.Ins2);
        for (int iv = first; iv < Polygon[iPol].lastVert; iv++) //==========
        {
          x1 = Vert[iv].X; 
          y1 = Vert[iv].Y; 
          x2 = Vert[iv + 1].X; 
          y2 = Vert[iv + 1].Y; 

          rect1 = new Rectangle(x1 - 1, y1 - 1, 2, 2);
          rect2 = new Rectangle(x2 - 1, y2 - 1, 2, 2);
          fm1.g2.DrawLine(greenPen, x1, y1, x2, y2);
          if (Rect_on) fm1.g2.FillRectangle(yellowBrush, rect1);
        } //===================== end for (int iv ... ======================
        if (Polygon[iPol].closed)
        {
          x1 = Vert[first].X; 
          y1 = Vert[first].Y; 
          fm1.g2.DrawLine(greenPen, x1, y1, x2, y2);
        }
        rect2 = new Rectangle((int)x2 - 1, (int)y2 - 1, 2, 2);
        if (Rect_on) fm1.g2.FillRectangle(yellowBrush, rect2);
      } //======================= end for (int iPol ... =================
      fm1.pictureBox2.Refresh();
      return 1;
    } //************************* end DrawPolygons *************************



    public int DrawArc(CArc arc, double fmag, int XS, int YS, Pen pen, bool DispRad, Graphics g, double fmx, double fmy)
    /* Draws an arc with radius R through (x1, y1) and (x2, y2)
    with the sacle "fmag". Draws also the radii at the ends of the arc if R < maxRad.
    (XS, YS) are the coordinates of the upper left corner of the pictureBox.
    The radius R has a sign: "+" means that the center lies left from the vector (P2 - P1), 
      "-" means right. */
    {
      double AbsR, al, del = 0.1, angle, R = 0.0, chord_X, chord_Y, Length,
        sin_d, cos_d, xm = 0, ym = 0, x = 0, y = 0, xn = 0, yn = 0;
      int maxRad = 120;
      Color F_Line = Color.Green;
      bool bar_on;

      bar_on = fmag >= 2.0;        // switch on of the 3x3 rectangles for points 

      if (arc.rad == 0.0 && false)  // curvature = 0.0 is coded with R = 0.0; it is a segment 
      {
        g.DrawLine(pen, XS + (int)(fmag * arc.xb + 0.5), YS + (int)(fmag * arc.yb + 0.5),
            XS + (int)(fmag * arc.xe + 0.5), YS + (int)(fmag * arc.ye + 0.5));
        return 0;
      }

      R = arc.rad;
      AbsR = R;
      if (R < 0.0) AbsR = -R;

      chord_X = arc.xe - arc.xb; chord_Y = arc.ye - arc.yb; // the chord

      Length = Math.Sqrt(chord_X * chord_X + chord_Y * chord_Y); // Length of the chord

      xm = arc.xm; 
      ym = arc.ym; 

      Pen linePen = new System.Drawing.Pen(Color.Blue);
      if (AbsR < maxRad + 50.0 && AbsR > 30.0 && DispRad)   // Radius vectors at the end points of the arc 
      {
        if (R > 0) linePen = new System.Drawing.Pen(Color.Yellow);
        else linePen = new System.Drawing.Pen(Color.Violet);
        g.DrawLine(linePen, XS + (int)(fmag * xm + 0.5), YS + (int)(fmag * ym + 0.5),
                          XS + (int)(fmag * arc.xb + 0.5), YS + (int)(fmag * arc.yb + 0.5));
        g.DrawLine(linePen, XS + (int)(fmag * xm + 0.5), YS + (int)(fmag * ym + 0.5),
                          XS + (int)(fmag * arc.xe + 0.5), YS + (int)(fmag * arc.ye + 0.5));
      }

      angle = 2.0 * Math.Asin(Length / 2.0 / AbsR);       // angle of the arc 

      if (R > 0) linePen = new System.Drawing.Pen(Color.Red);

      del = 0.1;
      cos_d = Math.Cos(del); // "del" is the step of the circular moving of the point (x, y)
      if (R < 0.0) sin_d = -Math.Sin(del);
      else sin_d = Math.Sin(del);

      if (angle < 2.0 * del) // The for-loop with 'al' is not used in this case
      {
        g.DrawLine(linePen, XS + (int)(fmag * arc.xb + 0.5), YS + (int)(fmag * arc.yb + 0.5),
                                    XS + (int)(fmag * arc.xe + 0.5), YS + (int)(fmag * arc.ye + 0.5));
        xn = yn = 0;
      }
      else
      {
        x = arc.xb - xm;
        y = arc.yb - ym;
        for (al = del; al < angle; al += del)       // Drawing an arc step by step 
        {
          xn = x * cos_d - y * sin_d;
          yn = x * sin_d + y * cos_d;
          g.DrawLine(linePen, XS + (int)(fmag * (x + xm) + 0.5), YS + (int)(fmag * (y + ym) + 0.5),
                                    XS + (int)(fmag * (xn + xm) + 0.5), YS + (int)(fmag * (yn + ym) + 0.5));
          x = xn; y = yn;
        }
        // The last segmewnt to the end of the arc:    
        int yyb = YS + (int)(fmag * (yn + ym) + 0.5);
        int xxb = XS + (int)(fmag * (xn + xm) + 0.5);
        int yy = YS + (int)(fmag * arc.ye + 0.5);
        int xx = XS + (int)(fmag * arc.xe + 0.5);
      }
      if (bar_on)
      {
        Brush myBrush = new System.Drawing.SolidBrush(Color.Violet);
        int size = 4;
        Rectangle rect = new Rectangle(XS + (int)(fmag * arc.xb + 0.5), YS + (int)(fmag * arc.yb + 0.5), size, size);
        g.FillRectangle(myBrush, rect);
        rect = new Rectangle(XS + (int)(fmag * arc.xe + 0.5), YS + (int)(fmag * arc.ye + 0.5), size, size);
        g.FillRectangle(myBrush, rect);
      }
      return 0;
    } // ********************** end DrawArc ********************************* 


    public int Curvature(int x1, int y1, int x2, int y2, int x3, int y3,
          double eps, ref CArc arc)
    /* Calculates the curvatur 'k' of an arc lying in the tolerance tube
      around the given two polygon edges [(x1,y1), (x2,y2)] and 
      [(x2,y2), (x3,y3)] and having the outher boundary of the tube as its
      tangent. The tangency point should not be farther as the half length
      of the shorter edge from (x2,y2). The radius of the arc should be as
      large as possible. */
    {
      //bool deb = false;
      double a1, a2, lp,  // Variables for calculating the projections
      dx1, dy1, dx2, dy2, len1, len2,    // Inkrements, lengths of edges 
      cosgam, // cosine of the outer angle betwwen the edges
      sinbet, cosbet,    // cosine and sine of the half angle 
      strip = 0.6,       // correcture of the deviation 
      k, kru1, kru2;     // curvature for long and short edges

      dx1 = (double)(x2 - x1); dy1 = (double)(y2 - y1);     // first edge 
      len1 = Math.Sqrt(dx1 * dx1 + dy1 * dy1);
      dx2 = (double)(x3 - x2); dy2 = (double)(y3 - y2);     // second edge 
      len2 = Math.Sqrt(dx2 * dx2 + dy2 * dy2);
      if ((len1 == 0.0) || (len2 == 0.0)) return (-1);

      cosgam = (dx1 * dx2 + dy1 * dy2) / len1 / len2;      // angle between the edges 
      if (Math.Abs(cosgam) <= 1.0)
        sinbet = Math.Sqrt((1.0 - cosgam) * 0.5);           // half angle 
      else sinbet = 0.0;
      cosbet = Math.Sqrt((1.0 + cosgam) * 0.5);

      kru1 = (1.0 - cosbet) / (eps - strip);    // longe edges, it is important
                                                // that the arc goes throught the vertex
      double min_len = len1;
      if (len2 < min_len) min_len = len2;
      if (min_len != 0.0 && cosbet != 0.0)
        kru2 = 2.0 * sinbet / cosbet / min_len;    // short edges, it is important
      else kru2 = 0.0;                       // that the tangency is in the midle of the shortest edge

      if ((Math.Abs(kru1) > Math.Abs(kru2)) && kru1 != 0.0)
      {
        if (cosbet != 0.0 && kru1 != 0.0)
          lp = sinbet / cosbet / kru1;        // distance of the point of tangency from (x2, y2)
        else lp = 100.0;
        k = kru1;    // first curvature 
      }
      else
      {
        lp = min_len / 2.0;
        k = kru2 * 0.95;    // second curvature
      }

      if (dx1 * dy2 - dy1 * dx2 > 0.0) k = -k;      // the sign 

      // the first edge is devided in relation a1 : a2 
      a1 = lp / len1;
      a2 = 1.0 - a1;

      if (k != 0.0) arc.rad = 1.0F / (float)k;
      else arc.rad = 0.0F;

      arc.xb = (float)(x1 * a1 + x2 * a2);   // first tangency (begin of the arc) 
      arc.yb = (float)(y1 * a1 + y2 * a2);

      a1 = lp / len2; a2 = 1.0 - a1;
      arc.xe = (float)(x3 * a1 + x2 * a2);   // second tangency (end of the arc) 
      arc.ye = (float)(y3 * a1 + y2 * a2);

      double AbsR, R, chord_X, chord_Y, Length, Lot_x, Lot_y;
      R = arc.rad;
      AbsR = R;
      if (R < 0.0) AbsR = -R;

      chord_X = arc.xe - arc.xb; chord_Y = arc.ye - arc.yb; // the chord
      Length = Math.Sqrt(chord_X * chord_X + chord_Y * chord_Y); // Length of the chord

      if (R < 0.0)        // 'Lot' is orthogonal to chord 
      {
        Lot_x = chord_Y; Lot_y = -chord_X;
      }
      else
      {
        Lot_x = -chord_Y; Lot_y = chord_X;
      }

      if (2 * AbsR < Length) return -1;
      if (Length < 0.1) return -2;

      double Lot = Math.Sqrt(4 * R * R - Length * Length);
      arc.xm = (float)((arc.xb + arc.xe) / 2 - Lot_x * Lot / 2 / Length);
      arc.ym = (float)((arc.yb + arc.ye) / 2 - Lot_y * Lot / 2 / Length);

      if ((arc.xe == arc.xb) && (arc.ye == arc.yb))
      { // can be a closed line of three points ?? 
        String s = String.Format("Curvature: Bad bounds: P1=({0}, {1}); P2=({2}, {3}); P3=({4}, {5})",
          x1, y1, x2, y2, x3, y3);
        return -2;
      }
      return 0;
    } // *********************** end Curvature ****************************************************** 

 

    public struct Box
    {
      public
      int midX, midY, minX, minY, maxX, maxY;
      public Box(int mid, int miy) // constructor
      {
        midX = mid;
        midY = miy;
        minX = 10000;
        minY = 10000;
        maxX = 0;
        maxY = 0;
      }
    }

    public double ThreePoints(iVect2[] Vert, int iv1, int iv2, int iv3, ref double fmx, ref double fmy)
    // Calculates the curvature and the center of the circle through three points 
    // Vert[iv1], Vert[iv2] and Vert[iv3]. Returns the value of the curvature.
    {
      double a, b, c, Curv, d, Det, f1, f2, midlabx, midlaby, midlcdx, midlcdy, SP, VP;
      a = (double)(Vert[iv2].X - Vert[iv1].X);
      b = (double)(Vert[iv2].Y - Vert[iv1].Y);
      c = (double)(Vert[iv3].X - Vert[iv2].X);
      d = (double)(Vert[iv3].Y - Vert[iv2].Y);
      Det = a * d - b * c;
      if (Math.Abs(Det) < 0.001)
      {
        fmx = fmy = 0;
        return 0.0;
      }
      VP = a * d - b * c;
      SP = a * c + b * d;
      midlabx = (double)(Vert[iv2].X + Vert[iv1].X) * 0.5;
      midlaby = (double)(Vert[iv2].Y + Vert[iv1].Y) * 0.5;
      f1 = a * midlabx + b * midlaby;
      midlcdx = (double)(Vert[iv3].X + Vert[iv2].X) * 0.5;
      midlcdy = (double)(Vert[iv3].Y + Vert[iv2].Y) * 0.5;
      f2 = c * midlcdx + d * midlcdy;
      fmx = (f1 * d - f2 * b) / Det;
      fmy = (f2 * a - f1 * c) / Det;
      Curv = 1.0 / Math.Sqrt((double)(Vert[iv3].X - fmx) * (double)(Vert[iv3].X - fmx) +
        (double)(Vert[iv3].Y - fmy) * (double)(Vert[iv3].Y - fmy));
      if (Math.Atan2(VP, SP) < 0.0) return -Curv;
      return Curv;
    }




    public double MinAreaN(iVect2[] P, int np, ref double radius, ref double x0, ref double y0)
    /* From 'WFRecoCircles". Calculates the 13 sums and the estimates "x0" and "y0" of the coordinates 
        of the center and the estimate
      "radius" of the radius of the optimal circle with the minimum deviation from the given
      set "P[np]" of points. 
      The found values and the 13 sums used for the calculation are assigned to the arc 
      with the index "ia". ------------ */
    {
      double SumX, SumY, SumX2, SumY2, SumXY, SumX3, SumY3, SumX2Y, SumXY2,
              SumX4, SumX2Y2, SumY4;
      double a1, a2, b1, b2, c1, c2, Crit, det, fx, fy, mx, my, N, R2;
      int ip;
      N = (double)(np);
      SumX = SumY = SumX2 = SumY2 = SumXY = SumX3 = SumY3 = SumX2Y = SumXY2 =
      SumX4 = SumX2Y2 = SumY4 = 0.0;

      for (ip = 0; ip < np; ip++) //======= over the set of points ==============
      {
        fx = (double)P[ip].X; fy = (double)P[ip].Y;
        SumX += fx; SumY += fy; SumX2 += fx * fx; SumY2 += fy * fy; SumXY += fx * fy;
        SumX3 += fx * fx * fx; SumY3 += fy * fy * fy; SumX2Y += fx * fx * fy; SumXY2 += fx * fy * fy;
        SumX4 += fx * fx * fx * fx; SumX2Y2 += fx * fx * fy * fy; SumY4 += fy * fy * fy * fy;
      } //=================== end for (ip...) ================================

      a1 = 2 * (SumX * SumX - N * SumX2); b1 = 2 * (SumX * SumY - N * SumXY);
      a2 = 2 * (SumX * SumY - N * SumXY); b2 = 2 * (SumY * SumY - N * SumY2);
      c1 = SumX2 * SumX - N * SumX3 + SumX * SumY2 - N * SumXY2;
      c2 = SumX2 * SumY - N * SumY3 + SumY * SumY2 - N * SumX2Y;
      det = a1 * b2 - a2 * b1;
      if (Math.Abs(det) < 0.00001) return -2.0;

      mx = (c1 * b2 - c2 * b1) / det;
      my = (a1 * c2 - a2 * c1) / det;
      R2 = (SumX2 - 2 * SumX * mx - 2 * SumY * my + SumY2) / N + mx * mx + my * my;
      if (R2 <= 0.0) return -1.0;
      x0 = mx; y0 = my; radius = Math.Sqrt(R2);
      Crit = 0.0;
      for (ip = 0; ip < np; ip++) //======= die Punktfolge ====================
      {
        fx = (double)P[ip].X; fy = (double)P[ip].Y;
        Crit += (radius - Math.Sqrt((fx - mx) * (fx - mx) + (fy - my) * (fy - my))) *
              (radius - Math.Sqrt((fx - mx) * (fx - mx) + (fy - my) * (fy - my)));
      } //=================== end for (ip...) ==============================

      return Math.Sqrt(Crit / (double)np);
    } //********************** end MinAreaN *********************************



    public int MessReturn(string s)
    {
      if (MessageBox.Show(s,
              "Return", MessageBoxButtons.OKCancel) == DialogResult.Cancel) return -1;
      return 1;
    }


    public double MinAreaN2(int ia, iVect2[] P, int Start, int np, ref double radius, ref double x0, ref double y0)
    /* Calculates the estimates "x0" and "y0" of the coordinates of the center and the estimate
    "radius" of the radius of the optimal circle with the minimum deviation from the given
    set "P[np]" of points. 
    The found values and the 13 sums used for the calculation are assigned to the arc 
    with the index "ia". ------------ */
    {
    double SumX, SumY, SumX2, SumY2, SumXY, SumX3, SumY3, SumX2Y, SumXY2,
            SumX4, SumX2Y2, SumY4;
    double a1, a2, b1, b2, c1, c2, Crit, det, fx, fy, mx, my, N, R2;
    int ip;
    N = (double)np;
    SumX = SumY = SumX2 = SumY2 = SumXY = SumX3 = SumY3 = SumX2Y = SumXY2 =
    SumX4 = SumX2Y2 = SumY4 = 0.0;

    for (ip = Start; ip < Start + np; ip++) //======= over the set of points ==============
    {
      fx = (double)P[ip].X; fy = (double)P[ip].Y;
      SumX += fx; SumY += fy; SumX2 += fx * fx; SumY2 += fy * fy; SumXY += fx * fy;
      SumX3 += fx * fx * fx; SumY3 += fy * fy * fy; SumX2Y += fx * fx * fy; SumXY2 += fx * fy * fy;
      SumX4 += fx * fx * fx * fx; SumX2Y2 += fx * fx * fy * fy; SumY4 += fy * fy * fy * fy;
    } //=================== end for (ip...) ================================

    a1 = 2 * (SumX * SumX - N * SumX2); b1 = 2 * (SumX * SumY - N * SumXY);
    a2 = 2 * (SumX * SumY - N * SumXY); b2 = 2 * (SumY * SumY - N * SumY2);
    c1 = SumX2 * SumX - N * SumX3 + SumX * SumY2 - N * SumXY2;
    c2 = SumX2 * SumY - N * SumY3 + SumY * SumY2 - N * SumX2Y;
    det = a1 * b2 - a2 * b1;
    if (Math.Abs(det) < 0.00001) return -1.0;

    mx = (c1 * b2 - c2 * b1) / det;
    my = (a1 * c2 - a2 * c1) / det;
    R2 = (SumX2 - 2 * SumX * mx - 2 * SumY * my + SumY2) / N + mx * mx + my * my;
    if (R2 <= 0.0) return -1.0;
    x0 = mx; y0 = my; radius = Math.Sqrt(R2);
    elArc[ia].Mx = mx;
    elArc[ia].My = my;
    elArc[ia].R = radius;
    elArc[ia].nPoints = np;
    Crit = 0.0;
    for (ip = Start; ip < Start + np; ip++) //======= die Punktfolge ====================
    {
      fx = (double)P[ip].X; fy = (double)P[ip].Y;
      Crit += (radius - Math.Sqrt((fx - mx) * (fx - mx) + (fy - my) * (fy - my))) *
            (radius - Math.Sqrt((fx - mx) * (fx - mx) + (fy - my) * (fy - my)));
    } //=================== end for (ip...) ==============================
    elArc[ia].SUM[0] = N; elArc[ia].SUM[1] = SumX; elArc[ia].SUM[2] = SumY; elArc[ia].SUM[3] = SumX2;
    elArc[ia].SUM[4] = SumY2; elArc[ia].SUM[5] = SumXY; elArc[ia].SUM[6] = SumX3;
    elArc[ia].SUM[7] = SumY3; elArc[ia].SUM[8] = SumX2Y; elArc[ia].SUM[9] = SumXY2;
    elArc[ia].SUM[10] = SumX4; elArc[ia].SUM[11] = SumX2Y2; elArc[ia].SUM[12] = SumY4;

    return Math.Sqrt(Crit / (double)np);
    } //********************** end MinAreaN2 *********************************/


    public void SaveArc(int iPol, int Sign, ref int Start, ref int End)
    {
      //int deb = 0;
      elArc[nArc].arcOK = false;
      elArc[nArc].iPol = iPol;
      elArc[nArc].Circle = -1;
      elArc[nArc].Sign = Sign;
      elArc[nArc].Start = Start;
      elArc[nArc].nPoints = End - Start + 1;
      Start = End = -1;
      nArc++;
    }




    public void SaveArc2(int iPol, int Sign, ref int Start, ref int End)
    {
      elArc[nArc].arcOK = true;
      elArc[nArc].iPol = iPol;
      elArc[nArc].Circle = -1;
      elArc[nArc].Sign = Sign;
      if (Start < 0 || iPol > 0 && Start == 0)
      {
        MessageBox.Show("SaveArc2 reports that 'Start' =" + Start + " and returns");
        return;
      }
      elArc[nArc].Start = Start;
      elArc[nArc].nPoints = End - Start + 1;
      double radius = 0.0, x0 = 0.0, y0 = 0.0;
      double rv = MinAreaN2(nArc, Vert, elArc[nArc].Start, elArc[nArc].nPoints, ref radius, ref x0, ref  y0);
      Start = End = -1;
      nArc++;
    } //*********************************** end SaveArc2 ********************************************************

 
    public void SaveArc3(int iPol, int Sign, ref int Start, ref int End)
    {
      elArc[nArc].arcOK = true;
      elArc[nArc].iPol = iPol;
      elArc[nArc].Circle = -1;
      elArc[nArc].Sign = Sign;
      if (Start < 0 || iPol > 0 && Start == 0)
      {
        MessageBox.Show("SaveArc3 reports that 'Start' =" + Start + " and returns");
        return;
      }
      elArc[nArc].Start = Start;
      elArc[nArc].nPoints = End - Start + 1;
      double radius = 0.0, x0 = 0.0, y0 = 0.0;
      iVect2[] Point = new iVect2[elArc[nArc].nPoints];
      for (int i = 0; i < elArc[nArc].nPoints; i++) Point[i] = new iVect2();
      if (End <= Polygon[iPol].lastVert)
        for (int i = 0; i < elArc[nArc].nPoints; i++) Point[i] = Vert[Start + i];
      else
      {
        for (int i = 0; i < elArc[nArc].nPoints - 1; i++) Point[i] = Vert[Start + i];
        Point[elArc[nArc].nPoints - 1] = Vert[Polygon[iPol].firstVert];
      }
      double rv = MinAreaN2(nArc, Point, 0, elArc[nArc].nPoints, ref radius, ref x0, ref  y0);
      Start = End = -1;
      nArc++;
    } //*********************************** end SaveArc3 *****************************************

    public bool Permit(int Start, int End)
    // Permits an arc if it contains more than 3 non collinear points.
    {
      //if (End - Start > 2) return true;
      if (End - Start == 1) return false;
      int delX, delX1, delY, delY1, ivMid = Start + 1;
      delX = Vert[ivMid].X - Vert[Start].X;
      delY = Vert[ivMid].Y - Vert[Start].Y;
      delX1 = Vert[End].X - Vert[Start].X;
      delY1 = Vert[End].Y - Vert[Start].Y;
      if (delX * delY1 == delX1 * delY) return false;
      return true;
    }

    public int MakeArcs3(PictureBox picturebox, int minRad, int maxRad, double eps, Form1 fm1)
    /* Searches in polygons for sequences of not too long edges with almost constant angles between
      subsequent edges. Each such sequence will be encoded with its starting point and
      the number of points in it. These values are saved in "elArc[]". -----------*/
    {
      bool INSCRIPT = false;
      int first, j, iPol, last, polLen;
      double[] curv = new double[3];
      curv[0] = curv[1] = curv[2] = 0.0;
      int[] End = new int[2], Start = new int[2], arcSign = new int[2];
      int P = 0; // Pold = -1;
      nArc = 0;
      double length = 0.0;
      bool bAngle = false, bCurv = false, bEdge = false, bSign = false;
      double ThrAngle = 1.5, ThrCurv = 2.0 / (double)minRad;
      if (minRad < 15) ThrCurv = 0.53;
      double ThrEdge = 3.5 * Math.Sqrt(2.0 * eps * maxRad);

      for (iPol = 0; iPol < nPolygon; iPol++) //==========================================================
      {
        first = Polygon[iPol].firstVert;
        if (INSCRIPT)
        {
          string s = iPol.ToString();
          fm1.Ins2.Write(s, Vert[first].X, Vert[first].Y, fm1.Ins2);
        }
        last = Polygon[iPol].lastVert;
        int PolyEnd;
        if (Polygon[iPol].closed) PolyEnd = last + 1;
        else PolyEnd = last;
        polLen = last - first + 1;
        if (polLen < 3) continue;
        for (int p = 0; p < 2; p++) End[p] = Start[p] = -1;
        double Angle, fmx = 0.0, fmy = 0.0;
        int iSwitch = 0, iSwitchOld = 0, j1 = 0;

        for (j = first; j <= PolyEnd; j++) //================== single loop ======================================
        {
          if (j <= last) j1 = j; // for closed polygons
          else j1 = first;
          length = 0.0;

          if (j >= first + 1) //----------------------------------------------
          {
            length = Math.Sqrt((double)(Vert[j].X - Vert[j - 1].X) * (double)(Vert[j].X - Vert[j - 1].X) +
                          (double)(Vert[j].Y - Vert[j - 1].Y) * (double)(Vert[j].Y - Vert[j - 1].Y));
            bEdge = (length > ThrEdge);
            if (bEdge) iSwitch = 4;
            else iSwitch = 0;

            if (j > first + 1) //----------------------------------------------
            {
              curv[0] = curv[1]; // curve[0] is necessary vor the case curve[1] == 0.0
              curv[1] = curv[2];
              curv[2] = ThreePoints(Vert, j - 2, j - 1, j1, ref fmx, ref fmy);
              if (curv[2] >= 0.0) P = 1;
              else P = 0;
              if (j == first + 3 && ((iSwitch & 6) > 0 || (iSwitchOld & 6) > 0 ) ) Start[P] = Start[1 - P] = -1;
              int VP = (Vert[j - 1].X - Vert[j - 2].X) * (Vert[j1].Y - Vert[j - 1].Y) -
                        (Vert[j - 1].Y - Vert[j - 2].Y) * (Vert[j1].X - Vert[j - 1].X);
              int SP = (Vert[j - 1].X - Vert[j - 2].X) * (Vert[j1].X - Vert[j - 1].X) +
                        (Vert[j - 1].Y - Vert[j - 2].Y) * (Vert[j1].Y - Vert[j - 1].Y);
              Angle = Math.Abs(Math.Atan2((double)(VP), (double)(SP)));
              bAngle = Angle > ThrAngle;  // 0.7 is 40 degree
              bCurv = Math.Abs(curv[2]) > ThrCurv; 
              if (bAngle || bCurv) iSwitch |= 2;

              if (j > first + 2) //-----------------------------------------------------------------------------
              {
                bSign = curv[2] * curv[1] < 0.0;
                if (j > first + 3 && curv[1] == 0.0)
                {
                  bSign = curv[2] * curv[0] < 0.0;
                  iSwitch = iSwitchOld;
                }
                if (bSign) iSwitch |= 1;
              } // ------------------------- end if (j > first + 2) --------------------------------------------
 
              
              if (j == first + 2) //--------------------------
              {
                if ((iSwitchOld & 4) == 0 && (iSwitch & 6) == 0) Start[P] = first;
                else Start[P] = Start[1 - P] = first + 1;
              } //---------- end if (j == first + 2) ----------
              if (j == first + 3 && (iSwitch & 1) == 0) Start[1 - P] = -1; // only 1 - P

              if (j > first + 3 && iSwitch < 2 && (iSwitchOld & 4) == 0 && Start[P] < 0) Start[P] = j - 2;

              if (Start[P] >= 0 && j == Start[P] + 2)
                if (curv[2] > 0) arcSign[P] = 1;
                else arcSign[P] = -1;

              if (Start[1 - P] >= 0 && j == Start[1 - P] + 2)
                if (curv[2] > 0) arcSign[1 - P] = 1;
                else arcSign[1 - P] = -1;

              if (iSwitchOld < 2)
              {
                if ((iSwitch & 1) == 0 && iSwitch > 1) End[P] = End[1 - P] = j - 1;
                 else
                 if (iSwitch != 0) End[P] = End[1 - P] = j - 1;
                 
              }

              // Saving the Arc:
              if (Start[P] >= 0 && End[P] >= Start[P]) 
              {
                if (Permit(Start[P], End[P]))
                  SaveArc3(iPol, arcSign[P], ref Start[P], ref End[P]);
                else Start[P] = End[P] = -1;
              }

              if (Start[1 - P] >= 0 && End[1 - P] >= Start[1 - P])
              {
                if (Permit(Start[1 - P], End[1 - P]))
                  SaveArc3(iPol, arcSign[1 - P], ref Start[1 - P], ref End[1 - P]);
                else Start[1 - P] = End[1 - P] = -1;
              }

              if ((iSwitch == 1 && iSwitchOld < 4 || iSwitch == 0 && (iSwitchOld & 6) == 2) && 
                                                                Start[P] < j - 2) Start[P] = j - 2;
            } //------------------------ end if (j > first + 1)  ------------------------------------------------
          } //-------------------------- end if (j >= first + 1) ----------------------------------------------------------

          iSwitchOld = iSwitch;

          if (j == PolyEnd)
          {
            if (Start[P] >= 0 && iSwitch == 0)
            {
              End[P] = PolyEnd;
              SaveArc3(iPol, arcSign[P], ref Start[P], ref End[P]);
            }
          }
        } //========================== end for (j... ================================================================
      } //============================== end for (iPol... =================================================================

      // Checking the arcs for long edges:
      length = 0;
      for (int ia = 0; ia < nArc; ia++) //========================================================================
      {
        int ivStart = elArc[ia].Start;
        int iPol1 = elArc[ia].iPol;
        int last1 = Polygon[iPol1].lastVert;
        int lenPoly = last1 - Polygon[iPol1].firstVert + 1;

        for (int iv1 = ivStart + 1; iv1 < ivStart + elArc[ia].nPoints; iv1++) //=========================
        {
          int iv = -1, ivM1 = -1, ivM2 = -1;
          if (iv1 > last1)
          {
            iv = Polygon[iPol1].firstVert; // closed polygon
            ivM1 = iv1 - 1;
            ivM2 = iv1 - 2;
          }
          else
          {
            iv = iv1;
            ivM1 = iv1 - 1;
            ivM2 = iv1 - 2;
          }
          length = Math.Sqrt((double)(Vert[iv].X - Vert[ivM1].X) * (double)(Vert[iv].X - Vert[ivM1].X) +
                        (double)(Vert[iv].Y - Vert[ivM1].Y) * (double)(Vert[iv].Y - Vert[ivM1].Y));
          bEdge = (length > ThrEdge);
        }

        for (int iv = ivStart + 2; iv < ivStart + elArc[ia].nPoints; iv++) //=======================
        {
          int iv1;
          if (iv > last1) iv1 = Polygon[iPol1].firstVert;
          else iv1 = iv;
          int VP = (Vert[iv - 1].X - Vert[iv - 2].X) * (Vert[iv1].Y - Vert[iv - 1].Y) -
                    (Vert[iv - 1].Y - Vert[iv - 2].Y) * (Vert[iv1].X - Vert[iv - 1].X);
          int SP = (Vert[iv - 1].X - Vert[iv - 2].X) * (Vert[iv1].X - Vert[iv - 1].X) +
                    (Vert[iv - 1].Y - Vert[iv - 2].Y) * (Vert[iv1].Y - Vert[iv - 1].Y);

          double Angle = Math.Abs(Math.Atan2((double)(VP), (double)(SP)));
          bAngle = Angle > ThrAngle; // 0.52; // 0.7 is 40 degree
        } //================================ end for (int iv1  =======================================
      } //================================== end for (int ia ...========================================


      // Calculation of the orientation and of the angle:
      int cnt = 0;
      for (int ia = 0; ia < nArc; ia++) //==================================================================
      {
        int ivEnd = 0, ivMiddle = 0, ivStart = 0, xEnd, xMid, xStart, yEnd, yMid, yStart;
        int iPol1 = elArc[ia].iPol;
        int last1 = Polygon[iPol1].lastVert;
        int lenPoly = last1 - Polygon[iPol1].firstVert + 1;
        ivStart = elArc[ia].Start;
        ivEnd = elArc[ia].Start + elArc[ia].nPoints - 1;
        if (ivEnd > last1) ivEnd -= lenPoly;
        ivMiddle = (ivStart + ivEnd) / 2;
        if (ivMiddle > 0)
        {
          xStart = Vert[ivStart].X; yStart = Vert[ivStart].Y;
          xEnd = Vert[ivEnd].X; yEnd = Vert[ivEnd].Y;
          xMid = Vert[ivMiddle].X; yMid = Vert[ivMiddle].Y;
          double oriAngle;
          if (elArc[ia].nPoints > 2)
            oriAngle = Math.Atan2((double)yMid - elArc[ia].My, (double)xMid - elArc[ia].Mx) + Math.PI / 8.0;
          else
            oriAngle = Math.Atan2((double)(yEnd - yStart), (double)(xStart - xEnd)) + Math.PI / 8.0;
          if (oriAngle < 0.0) oriAngle += 2.0 * Math.PI;
          int Ori = (int)(oriAngle / (0.25 * Math.PI));
          elArc[ia].Ori = Ori;

          // Calculating the angle:
          double Perim = 0.0;
          for (int iv = ivStart + 1; iv < ivStart + elArc[ia].nPoints - 1; iv++)
          {
            Perim += Math.Sqrt((Vert[iv].X - Vert[iv - 1].X) * (Vert[iv].X - Vert[iv - 1].X) +
                                (Vert[iv].Y - Vert[iv - 1].Y) * (Vert[iv].Y - Vert[iv - 1].Y));
          }

          elArc[ia].Angle = Perim / elArc[ia].R;

          if (elArc[ia].nPoints >= 7 && elArc[ia].Angle > Math.PI * 0.2) cnt++;
        } //------------------------------ end if ( Middle -----------------------------------------------------
      } //================================ end for (int ia  ======================================================
      return nArc;
    } //******************************** end MakeArcs3 **************************************


    public double MinArea2(double[] SUM, ref double radius, ref double x0, ref double y0)
    /* Calculates the estimates "x0" and "y0" of the coordinates of the center and the estimate
      "radius" of the radius of the optimal circle with the minimum deviation from a given
      set of points. The coordinates of the points and their products are contained in the 
      array "SUM". Returns the value of the deviation. ------------ */
    { // N,SuX,SuY,SuX2,SuY2,SuXY,SuX3,SuY3,SuX2Y,SuXY2,SuX4,SuX2Y2,SuY4;
      // 0	1		2		3			4		 5 		6		7			8		  9		 10		11		 12
      double a1, a2, b1, b2, c1, c2, det, mx, my, N, R2;
      double Crit, Mx, My, Mx2, My2, Mx3, My3;
      //*********************   B E G I N   ********************************
      N = SUM[0];
      a1 = 2 * (SUM[1] * SUM[1] - N * SUM[3]); b1 = 2 * (SUM[1] * SUM[2] - N * SUM[5]);
      a2 = 2 * (SUM[1] * SUM[2] - N * SUM[5]); b2 = 2 * (SUM[2] * SUM[2] - N * SUM[4]);
      c1 = SUM[3] * SUM[1] - N * SUM[6] + SUM[1] * SUM[4] - N * SUM[9];
      c2 = SUM[3] * SUM[2] - N * SUM[7] + SUM[2] * SUM[4] - N * SUM[8];
      det = a1 * b2 - a2 * b1;
      if (Math.Abs(det) < 0.00001) return -2;

      mx = (c1 * b2 - c2 * b1) / det;
      my = (a1 * c2 - a2 * c1) / det;

      R2 = ((SUM[3] - 2 * SUM[1] * mx - 2 * SUM[2] * my + SUM[4]) / N) + mx * mx + my * my;
      if (R2 <= 0.0) return -1;
      x0 = mx; y0 = my; 
      radius = Math.Sqrt(R2);
      Mx = mx; My = my; 
      Mx2 = mx * mx; 
      My2 = my * my;
      Mx3 = Mx2 * mx; 
      My3 = My2 * my;

      Crit = 0.1 + SUM[10] + 2.0 * SUM[11] + SUM[12] - 4.0 * Mx * (SUM[6] + SUM[9]) - 4.0 * My * (SUM[7] + SUM[8]) +
      (6.0 * Mx2 - 2.0 * R2 + 2.0 * My2) * SUM[3] + 8.0 * Mx * My * SUM[5] + (6.0 * My2 - 2.0 * R2 + 2.0 * Mx2) * SUM[4]
      - 4.0 * (Mx3 - R2 * Mx + Mx * My2) * SUM[1] - 4.0 * (My3 - R2 * My + Mx2 * My) * SUM[2] +
      N * (R2 * R2 + Mx2 * Mx2 + 2.0 * Mx2 * My2 + My2 * My2 - 2.0 * R2 * (Mx2 + My2));
      if (Crit < 0.0) return 100.0;
      return Math.Sqrt(Crit / N) / radius / 2.0;
    } //***************************** end MinArea2 ****************************************************************+

 
    public int MakeCirclesEl(CCircle[,] Circle, int iVar, ref int nCircle, int minRad, int maxRad, 
                                                    int[] BestCircles, ref int nBest)
    /* Tests all arcs while ignoring bad arcs and arcs already assigned to a circle.
      Finds without the pseudoraster all "neighbour" arcs whose center lies close to the center 
      of the arc being tested. If the tested arc has no neighbours and its angle is greater than
      "MinAngle" then the circle assigned to it by "FindCirclesB" is saved in the array "Circle". 
      Otherwise a circle is being calculated for the group of neighboring arcs by means of "MeanArea2". 
      If the criterium is less than "minCrit" then the calculated circle is saved in "Circle". 
      "MakeCirclesEl" fills the array "Circle". ------*/
    {
      int iCircle = 0;
      int iMX, iMY, SignArc, SignCircle;
      const int nSum = 13;
      const int nArcsKr = 10;
      int[] iArcKr = new int[nArcsKr];
      double Crit, MD, mx = 0.0, my = 0.0, R, SumAngle = 0;
      double[] SUM = new double[13];
      double maxCrit = Set.MaxCrit;
      double MinAngle = 0.5;
      for (int ia = 0; ia < nArc; ia++) //==================================================================
      {
        SignCircle = SignArc = elArc[ia].Sign;
        if (elArc[ia].Circle >= 0 || elArc[ia].nPoints < 3 || elArc[ia].Angle < MinAngle) continue;
        double MX = elArc[ia].Mx;
        double MY = elArc[ia].My;
        R = elArc[ia].R;
        SignArc = elArc[ia].Sign; // sign of the curvature
        MD = R * 0.5;
        iMX = (int)(MX);
        iMY = (int)(MY);
        bool ready = false;
        int cntArcs = 0;
        int jaOK = -1;
        for (int iis = 0; iis > nSum; iis++) SUM[iis] = 0.0;
        SumAngle = 0.0;
        iArcKr[0] = ia; // iArcKr saves the indices of the arcs passing to a group
          cntArcs = 1;
        jaOK = ia;
        for (int ja = 0; ja < nArc; ja++) //====================================
        {
          if (ja == ia || elArc[ja].nPoints < 3 || elArc[ja].Sign != SignCircle) continue;

          // Testing whether ja passes to ia:
          if (elArc[ja].Circle < 0 && //elArc[ja].arcOK &&
                            Math.Abs(elArc[ja].Mx - MX) < MD &&
                            Math.Abs(elArc[ja].My - MY) < MD &&
                            elArc[ja].R / R > 0.5 && elArc[ja].R / R < 2.1 &&
                            cntArcs < nArcsKr - 1) //------------------------------------------------
          {
            SumAngle += elArc[ja].Angle;
            if (SumAngle > 6.28)
            {
              SumAngle -= elArc[ja].Angle;
              ready = true;
            }

            iArcKr[cntArcs++] = ja;
            jaOK = ja;
          } //----------------- end if (ja... -----------------------------------------------
            if (ready) break;
        } //============== end for (int ja ... ================================================

        if (cntArcs == 1 && jaOK == ia && elArc[ia].nPoints >= 3)
        { 
          if (elArc[ia].Angle < MinAngle || elArc[ia].R < minRad || elArc[ia].R > maxRad) continue;
          Circle[iVar, iCircle].SetPar(MX, MY, elArc[ia].R, elArc[ia].Angle, SignCircle);
          elArc[ia].Circle = iCircle;
          iCircle++; // iCircle is the running number of a circle
        }
        else // cntArcs > 1
        {
          int ka = -1;
          for (int iis = 0; iis < nSum; iis++) SUM[iis] = 0.0;
          for (int i = 0; i < cntArcs; i++)
          {
            ka = iArcKr[i];
            for (int iis = 0; iis < nSum; iis++)  SUM[iis] += elArc[ka].SUM[iis];
            SumAngle += elArc[ka].Angle;
          }
          if (SumAngle > 6.28) SumAngle = 6.28;

          Crit = MinArea2(SUM, ref R, ref mx, ref my);
          if (Crit < 0.0) return -1;
              
          if (elArc[ka].nPoints >= 3 && R > minRad && R < maxRad && SumAngle > MinAngle) //-------------------------------------------------------
          {
            Circle[iVar, iCircle].SetPar(mx, my, R, SumAngle, SignCircle);
            for (int i = 0; i < cntArcs; i++)
            {
              int ja = iArcKr[i];
              elArc[ja].Circle = iCircle;
            }
            iCircle++;
          } // ----------- end if (R > minRad && R < maxRad) ------------------------------------
        } //-------------- end if (cntArcs == 1 && jaOK == ia) ------------------------------------
      } //================ end for (int ia... ===========================================================
      nCircle = iCircle;  
      return nCircle;
    } //****************** end MakeCirclesEl ****************************************************************


    public int DrawAcircle(CCircle[] Circle, int ic, Form1 fm1)
    {
      double alpha = 0.0, del = 0.1, xm, ym; // = Circle[ic].Mx, ym = Circle[ic].My;
      int Sign = Circle[ic].sign;
      Graphics g = fm1.g1;
      Pen redPen = new Pen(Color.Red, 2);
      double Scale = fm1.Scale1;
      double cos_d = Math.Cos(del), sin_d = Math.Sin(del), x = 0, y = Circle[ic].R, xn, yn;
      x = 0.0;
      y = Circle[ic].R;

      xm = Circle[ic].Mx;
      ym = Circle[ic].My;

      string s = ic.ToString();
      fm1.Ins1.Write(s, (int)(x + xm), (int)(ym - y), fm1.Ins1);
      for (alpha = 0.0; alpha < 6.28; alpha += del)       // stepwise drawing 
      {
        xn = x * cos_d - y * sin_d;
        yn = x * sin_d + y * cos_d;
        g.DrawLine(redPen, (float)(x + xm), (float)(y + ym), (float)(xn + xm), (float)(yn + ym));
        x = xn; y = yn;
      }
      fm1.pictureBox1.Image = fm1.BmpPictBox1;
      return 1;
    } //**************************** end DrawAcircle ************************************


    public int DrawAcircleCreate(CCircle[] Circle, int ic, Form1 fm1)
    {
      double alpha = 0.0, del = 0.1, xm, ym; // = Circle[ic].Mx, ym = Circle[ic].My;
      int Sign = Circle[ic].sign;
      Graphics g = fm1.g1;
      Pen redPen = new Pen(Color.Red, 2);
      double Scale = fm1.Scale1;
      double cos_d = Math.Cos(del), sin_d = Math.Sin(del), x = 0, y = Circle[ic].R, xn, yn;
      x = 0.0;
      y = Scale * Circle[ic].R;

      xm = fm1.marginX + Scale * Circle[ic].Mx;
      ym = fm1.marginY + Scale * Circle[ic].My;

      string s = ic.ToString();
      fm1.Ins1.Write(s, (int)(x + xm), (int)(ym - y), fm1.Ins1);
      for (alpha = 0.0; alpha < 6.28; alpha += del)       // stepwise drawing 
      {
        xn = x * cos_d - y * sin_d;
        yn = x * sin_d + y * cos_d;
        g.DrawLine(redPen, (float)(x + xm), (float)(y + ym), (float)(xn + xm), (float)(yn + ym));
        x = xn; y = yn;
      }
      fm1.pictureBox1.Image = fm1.BmpPictBox1;
      return 1;
    } //**************************** end DrawAcircle ************************************



    public int ShowBestCircles3(CCircle[] Circle, int nBest, double minAngle, Form1 fm1)
    {
      int cnt = 0;
      for (int ib = 0; ib < nBest; ib++)
      {
        if (Circle[ib].Angle > minAngle)
        {
          DrawAcircle(Circle, ib, fm1);
          cnt++;
        }
      }
      return cnt;
    }
  }
}
