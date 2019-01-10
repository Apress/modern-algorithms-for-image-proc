using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;


namespace WFellipseBikes
{
  public class CQue
  {
    public
      int input, output, Len;
    bool full;
    Point[] Array;
    public CQue(int len) // Constructor
    {
      Len = len;
      input = 0;
      output = 0;
      full = false;
      Array = new Point[Len];
      for (int i = 0; i < Len; i++) Array[i] = new Point();
    }

    public int Put(Point V)
    {
      if (full) return -1;
      Array[input] = V;
      if (input == Len - 1) input = 0;
      else input++;
      return 1;
    }

    public Point Get()
    {
      Point er = new Point(-1, -1);
      if (Empty())
      {
        return er;
      }
      Point iV = new Point();
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


  public class CBox
  {
    public int minX, minY, maxX, maxY;
    public CBox()
    {
      minX = 10000;
      minY = 10000;
      maxX = 0;
      maxY = 0;
    }
  }


  public class CPolygon
  {
    public int firstVert, lastVert, firstArc, lastArc, nCrack; // 20 byte
    public CBox Box; // 16 byte
    public bool closed, smooth; // 1 byte; all together 37 byte
  } //**************** end public class CPolygon ***************************



  public class CelArc
  {
    public int iPol, Circle, Dart, Ori, Sign, Start, nPoints;
    public bool arcOK;
    public double Angle, Mx, MinLong, My, R, EndAz, StartAz;
    public double[] SUM;
    public CelArc() // constructor
    {
      SUM = new double[13];
      arcOK = true;
    }
  }

  public struct Ellipse
  {
    public double a, b, c, d, f;
    public int sumPoints, iGroup;
    public bool good;
  }

  public struct SCircle
  {
    public double Mx, My, R;
    public int index;
    public bool goodEl, goodCirc;
  }


  public class CListLines
  {
    public int nArcs, nPolygon, nVert, MaxPoly, MaxVert, MaxArc; //, nxPS, nyPS,

    public static double PosSectX, PosSectY, NegSectX, NegSectY;
    public static Point Far;

    public Point[] Step; 
    public Point[] Norm; 

    public CPolygon[] Polygon;
    public Point[] Vert;
    public CelArc[] elArc;
    public CQue pQ;
    public int[,] nArcIPC;

    public CListLines() { } // Default constructor

    // constructor
    public CListLines(int maxL2, int maxV, int maxArc, int nx, int ny, int maxArcInPScell, int sizeX, int sizeY) // constructor
    {
      MaxPoly = maxL2;
      MaxVert = maxV;
      MaxArc = maxArc;
      pQ = new CQue(1000); // necessary to find connected components
      nArcs = 0;
      nPolygon = 0;
      Polygon = new CPolygon[MaxPoly];
      for (int i = 0; i < MaxPoly; i++)
      {
        Polygon[i] = new CPolygon();
        Polygon[i].Box = new CBox();
      }
      Vert = new Point[MaxVert];
      for (int i = 0; i < MaxVert; i++) Vert[i] = new Point();
      elArc = new CelArc[MaxArc];
      for (int i = 0; i < MaxArc; i++) elArc[i] = new CelArc();

      Step = new Point[4];
      for (int i = 0; i < 4; i++) Step[i] = new Point();
      Norm = new Point[4];
      for (int i = 0; i < 4; i++) Norm[i] = new Point();

      int[,] iStep = { { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 } };  // * sind konkrete Zahlen
      for (int i = 0; i < 4; i++)
      {
        Step[i].X = iStep[i, 0];
        Step[i].Y = iStep[i, 1];
      }

      int[,] iNorm = { { 0, 1 }, { -1, 0 }, { 0, -1 }, { 1, 0 } };  // * sind konkrete Zahlen
      for (int i = 0; i < 4; i++)
      {
        Norm[i].X = iNorm[i, 0];
        Norm[i].Y = iNorm[i, 1];
      }
    } //*************** end constructor *********************


    public double MinAreaN2(int ia, Point[] P, int Start, int np, ref double radius, ref double x0, ref double y0)
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


    public void SaveArc2(int iPol, int Sign, ref int Start, ref int End)
    {
      elArc[nArcs].arcOK = false;
      elArc[nArcs].iPol = iPol;
      elArc[nArcs].Circle = -1;
      elArc[nArcs].Sign = Sign;
      if (Start < 0 || iPol > 0 && Start == 0)
      {
        MessageBox.Show("SaveArc reports that 'Start' =" + Start + " and returns");
        return;
      }
      elArc[nArcs].Start = Start;
      elArc[nArcs].nPoints = End - Start + 1;
      double radius = 0.0, x0 = 0.0, y0 = 0.0;
      double rv = MinAreaN2(nArcs, Vert, elArc[nArcs].Start, elArc[nArcs].nPoints, ref radius, ref x0, ref  y0);
      Start = End = -1;
      nArcs++;
    }


    public void SaveArc(int iPol, int Sign, ref int Start, ref int End)
    {
      elArc[nArcs].arcOK = false;
      elArc[nArcs].iPol = iPol;
      elArc[nArcs].Circle = -1;
      elArc[nArcs].Sign = Sign;
      if (Start < 0 || iPol > 0 && Start == 0)
      {
        MessageBox.Show("SaveArc reports that 'Start' =" + Start + " and returns");
        return;
      }
      elArc[nArcs].Start = Start;
      elArc[nArcs].nPoints = End - Start + 1;
      double radius = 0.0, x0 = 0.0, y0 = 0.0;
      double rv = MinAreaN2(nArcs, Vert, elArc[nArcs].Start, elArc[nArcs].nPoints, ref radius, ref x0, ref  y0);
      Start = End = -1;
      nArcs++;
    }

    public void SaveArcNew(int iPol, int Start, int End)
    {
      elArc[nArcs].arcOK = false;
      elArc[nArcs].iPol = iPol;
      elArc[nArcs].Circle = -1;
      elArc[nArcs].Sign = 1;
      if (Start < 0 || iPol > 0 && Start == 0)
      {
        if (MessReturn("SaveArcNew reports that 'Start' =" + Start + " and returns") < 0)
          return;
      }
      elArc[nArcs].Start = Start;
      elArc[nArcs].nPoints = End - Start + 1;
      double radius = 0.0, x0 = 0.0, y0 = 0.0;
      double rv = MinAreaN2(nArcs, Vert, elArc[nArcs].Start, elArc[nArcs].nPoints, ref radius, ref x0, ref  y0);
      nArcs++;
    }


    public double ThreePoints(Point[] Vert, int iv2, int iv1, int iv, ref double fmx, ref double fmy)
    // Calculates the curvature and the center of the circle through three points 
    // Vert[iv2], Vert[iv1] and Vert[iv]. Returns the value of the curvature.
    {
      double a, b, c, Curv, d, Det, f1, f2, midlabx, midlaby, midlcdx, midlcdy, SP, VP;
      a = (double)(Vert[iv1].X - Vert[iv2].X);
      b = (double)(Vert[iv1].Y - Vert[iv2].Y);
      c = (double)(Vert[iv].X - Vert[iv1].X);
      d = (double)(Vert[iv].Y - Vert[iv1].Y);
      Det = a * d - b * c;
      if (Math.Abs(Det) < 0.001)
      {
        fmx = fmy = 0;
        return 0.0;
      }
      VP = a * d - b * c;
      SP = a * c + b * d;
      midlabx = (double)(Vert[iv1].X + Vert[iv2].X) * 0.5;
      midlaby = (double)(Vert[iv1].Y + Vert[iv2].Y) * 0.5;
      f1 = a * midlabx + b * midlaby;
      midlcdx = (double)(Vert[iv].X + Vert[iv1].X) * 0.5;
      midlcdy = (double)(Vert[iv].Y + Vert[iv1].Y) * 0.5;
      f2 = c * midlcdx + d * midlcdy;
      fmx = (f1 * d - f2 * b) / Det;
      fmy = (f2 * a - f1 * c) / Det;
      Curv = 1.0 / Math.Sqrt((double)(Vert[iv].X - fmx) * (double)(Vert[iv].X - fmx) +
                              (double)(Vert[iv].Y - fmy) * (double)(Vert[iv].Y - fmy));
      if (Math.Atan2(VP, SP) < 0.0) return -Curv;
      return Curv;
    }

    public double ThreePoints1(Point[] Vert, int iv2, int iv1, int iv, ref double fmx, ref double fmy, Form1 fm1)
    // Calculates the curvature and the center of the circle through three points 
    // Vert[iv2], Vert[iv1] and Vert[iv]. Returns the value of the curvature.
    {
      double a, b, c, Curv, d, Det, f1, f2, midlabx, midlaby, midlcdx, midlcdy, SP, VP;
      a = (double)(Vert[iv1].X - Vert[iv2].X);
      b = (double)(Vert[iv1].Y - Vert[iv2].Y);
      c = (double)(Vert[iv].X - Vert[iv1].X);
      d = (double)(Vert[iv].Y - Vert[iv1].Y);
      Det = a * d - b * c;
      if (Math.Abs(Det) < 0.001)
      {
        fmx = fmy = 0;
        return 0.0;
      }
      VP = a * d - b * c;
      SP = a * c + b * d;
      midlabx = (double)(Vert[iv1].X + Vert[iv2].X) * 0.5;
      midlaby = (double)(Vert[iv1].Y + Vert[iv2].Y) * 0.5;
      f1 = a * midlabx + b * midlaby;
      midlcdx = (double)(Vert[iv].X + Vert[iv1].X) * 0.5;
      midlcdy = (double)(Vert[iv].Y + Vert[iv1].Y) * 0.5;
      f2 = c * midlcdx + d * midlcdy;
      fmx = (f1 * d - f2 * b) / Det;
      fmy = (f2 * a - f1 * c) / Det;
      Curv = 1.0 / Math.Sqrt((double)(Vert[iv].X - fmx) * (double)(Vert[iv].X - fmx) +
                              (double)(Vert[iv].Y - fmy) * (double)(Vert[iv].Y - fmy));
      if (Math.Atan2(VP, SP) < 0.0) return -Curv;
      return Curv;
    }

    public double RadThreePoints(Point[] Vert, int iv2, int iv1, int iv, ref double fmx, ref double fmy)
    // Calculates the curvature radius and the center of the circle through three points 
    // Vert[iv2], Vert[iv1] and Vert[iv]. Returns the value of the curvature radius.
    {
      double a, b, c, d, Det, f1, f2, midlabx, midlaby, midlcdx, midlcdy, Rad;
      a = (double)(Vert[iv1].X - Vert[iv2].X);
      b = (double)(Vert[iv1].Y - Vert[iv2].Y);
      c = (double)(Vert[iv].X - Vert[iv1].X);
      d = (double)(Vert[iv].Y - Vert[iv1].Y);
      Det = a * d - b * c;
      if (Math.Abs(Det) < 0.001)
      {
        fmx = fmy = 0;
        return 0.0;
      }

      midlabx = (double)(Vert[iv1].X + Vert[iv2].X) * 0.5;
      midlaby = (double)(Vert[iv1].Y + Vert[iv2].Y) * 0.5;
      f1 = a * midlabx + b * midlaby;
      midlcdx = (double)(Vert[iv].X + Vert[iv1].X) * 0.5;
      midlcdy = (double)(Vert[iv].Y + Vert[iv1].Y) * 0.5;
      f2 = c * midlcdx + d * midlcdy;
      fmx = (f1 * d - f2 * b) / Det;
      fmy = (f2 * a - f1 * c) / Det;
      Rad = Math.Sqrt((double)(Vert[iv].X - fmx) * (double)(Vert[iv].X - fmx) +
                              (double)(Vert[iv].Y - fmy) * (double)(Vert[iv].Y - fmy));
      return Rad;
    } //***************************** end RadThreePoints ******************************


    public int ThirdPoint(int x1, int y1, int x2, int y2, ref int x3, ref int y3, int dist,
      double fmx, double fmy, double R)
    {
      double a = (double)(dist * dist) / 2.0 / R;
      double kX, kY, length, normX, normY;
      kX = (x2 * (R - a) + fmx * a) / R;
      kY = (y2 * (R - a) + fmy * a) / R;
      normX = y2 - fmy;
      normY = fmx - x2;
      length = Math.Sqrt(normX * normX + normY * normY);
      x3 = (int)(kX - normX * Math.Sqrt((double)(dist * dist) - a * a) / length);
      y3 = (int)(kY - normY * Math.Sqrt((double)(dist * dist) - a * a) / length);
      return 1;
    }

    int Sign(double x)
    {
      if (x > 0.0) return 1;
      if (x == 0.0) return 0;
      return -1;
    }


    public double ParArea(int iv, Point P)
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

    public int CheckComb(Point StartP, Point P, double eps, ref Point Vect)
    // This is the new function for polygonal approximation with the sector method.
    // It replaces the old function "secomb"; 'P' is in standard coord.
    {
       double Length, Sin, Cos, Proj, PosTangX, PosTangY, NegTangX, NegTangY;
      Point Line = new Point();

      if (StartP == P)
      {
        PosSectX = -1000.0;
        return 0;
      }
      Line.X = P.X - StartP.X;
      Line.Y = P.Y - StartP.Y;
      Length = Math.Sqrt(Math.Pow((double)Line.X, 2.0) + Math.Pow((double)Line.Y, 2.0));
      if (Length < eps) return 0;
      
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

    public int TraceApp(CImage Comb, int X, int Y, double eps, ref Point Pterm, ref int dir)
    /* This method traces a line in the image "Comb" with combinatorial coordinates, where the cracks 
      and points of the edges are labeled: bits 0 and 1 of a point contain the label 1 to 3 of the point.
      The label indicates the number of incident edge cracks. Bits 2 to 5 indicate the presence of
      incident cracks of directions 0, 1, 2 and 3 correspondingly. Labeled bit 6 indicates that the point 
      already has been put into the queue; labeled bit 7 indicates that the point shoud not been used any more.
      The crack has only one label 1 in bit 0.
      This function traces the edge from one end or branch point to another while changing the parameter "dir".
      It makes polygonal approximation with precision "eps" and saves STANDARD coordinats in "Vert".
      ----------*/
    {
      int br, Lab, rv = 0;
      bool BP = false, END = false;
      bool atSt_P = false, CHECK = true;
      Point Crack = new Point(), P = new Point(), P1 = new Point(), Pold = new Point(),
        Pstand = new Point(), StartEdge = new Point(), StartLine = new Point(), Vect = new Point();

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

      Vert[nVert].X = Pstand.X;
      Vert[nVert].Y = Pstand.Y;
      nVert++;

      Vect = new Point();
      int CNX = Comb.width;
      int CNY = Comb.height;
      CheckComb(StartEdge, Pstand, eps, ref Vect);
 
      while (true) //====================================================================
      {
        Crack.X = P.X + Step[dir].X;
        Crack.Y = P.Y + Step[dir].Y;
        if (Comb.Grid[Crack.X + CNX * Crack.Y] == 0)
        {
          MessageBox.Show("TraceApp, error: dir=" + dir + " the Crack=(" + Crack.X + "," + Crack.Y +
          ") has label 0;  X=" + X + ", Y=" + Y + "; iCrack=" + iCrack + " dir=" + dir);
          //Comb.DrawComb(Crack.X / 2 - 5, Crack.Y / 2 - 5, Form1 Form());
          MessageBox.Show("Point before Crack Lab=" + (Comb.Grid[P.X + CNX * P.Y] & 7) + " P+0=" + (Comb.Grid[P.X + 2 + CNX * P.Y] & 7) +
          " P+1=" + (Comb.Grid[P.X + CNX * (P.Y + 2)] & 7) + " P+2=" + (Comb.Grid[P.X - 2 + CNX * P.Y] & 7) +
          " P+3=" + (Comb.Grid[P.X + CNX * (P.Y - 2)] & 7) + "P+2 coord=(" + (P.X - 2) + "; " + P.Y + ") Lab(P+2)=" +
          (Comb.Grid[P.X - 2 + CNX * P.Y] & 7) + " Labels of four cracks about P+2=" +
          Comb.Grid[P.X - 2 + 1 + CNX * P.Y] + "; " + Comb.Grid[P.X - 2 + CNX * (P.Y + 1)] + "; " +
          Comb.Grid[P.X - 3 + CNX * P.Y] + "; " + Comb.Grid[P.X - 2 + CNX * (P.Y - 1)]);
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
            MessageBox.Show("TraceApp: Overflow in 'Vert'; X=" + X + " Y=" + Y + " nVert=" + nVert);
            return -1;
          }
          if (CHECK) //-------------------------------------------
          {
            if (br == 1)
            {
              Vert[nVert].X = Pold.X / 2;
              Vert[nVert].Y = Pold.Y / 2;
            }
            else Vert[nVert] = Vect;
            StartEdge = Vert[nVert];
          }
          else // CHECK == false
          {
            if (br == 10)
            {
              Vert[nVert].X = Pold.X / 2;
              Vert[nVert].Y = Pold.Y / 2;
            }
            else Vert[nVert] = P1;
          } //----------------------------- end if (CHECK) ------------------------------


          nVert++;
          br = 0;
        } //------------------ end if (br) --------------------------------------

        atSt_P = (Pstand == StartLine);
        if (atSt_P)
        {
          Pterm.X = P.X; // Pterm is a parameter of TraceApp
          Pterm.Y = P.Y;
          Polygon[nPolygon].lastVert = nVert - 1;
          Polygon[nPolygon].closed = true;
          rv = 2;
          break;
        }

        if (!atSt_P && (BP || END))
        {
          Pterm.X = P.X; // Pterm is a parameter of TraceApp
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
    } //***************************************** end TraceApp ***********************************************


    public int TraceAppNew(CImage Comb, int X, int Y, double eps, ref Point Pterm, ref int dir)
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
      Point Crack, P, P1, Pold, Pstand, StartEdge, StartLine, Vect;
      Crack = new Point();
      P = new Point();
      P1 = new Point();
      Pold = new Point();
      Pstand = new Point();
      StartEdge = new Point();
      StartLine = new Point();
      Vect = new Point();

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
      Vect = new Point();
      int CNX = Comb.width;
      int CNY = Comb.height;
      //CheckComb(StartEdge, Pstand, eps, ref Vect);

      while (true) //====================================================================
      {
        Crack.X = P.X + Step[dir].X;
        Crack.Y = P.Y + Step[dir].Y;
        if (Comb.Grid[Crack.X + CNX * Crack.Y] == 0)
        {
          MessageBox.Show("TraceAppNew, error: dir=" + dir + " the Crack=(" + Crack.X + "," + Crack.Y +
          ") has label 0; Start=(" + X + "; " + Y + "); iCrack=" + iCrack + " P=(" + P.X + "; " + P.Y +
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
            nVert++;
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
    /* Encodes in "CListLines" the lines of the edge component with the point (X, Y) being
      a branch or an end point. Puts the starting point 'Pinp' into the queue and starts
      the 'while' loop. It tests each labeled crack incident with the point 'P' fetched from the queue.
      If the next point of the crack is a branch or an end point, then a short line is saved.
      Otherwise the funktion "TraceAppNew" is called. "TraceAppNew" traces a long line, saves the color
      indices at the sides of the line and ends at the point 'Pterm' with the direction 'DirT'. 
      Then the function "FreqInds" assigns the most frequent from the saved color indices to the line.
      If the point 'Pterm' is a branch point then it is put to the queue.
      "ComponPoly" return when the queue is empty.	---------------*/
    {
      int dir, dirT; //, NX=2*NXP+1, NY=2*NYP+1, NXB=NXP; 
      int LabNext, rv;
      int[] Mark = new int[4]; //{8, 16, 32, 64}, rv; 
      Mark[0] = 8;
      Mark[1] = 16;
      Mark[2] = 32;
      Mark[3] = 64;
      Point Crack = new Point(), P = new Point(), Pinp = new Point(), Pnext = new Point(),
        Pterm = new Point();
      Pinp.X = X;
      Pinp.Y = Y; // comb. coord.
      int CNX = Comb.width;
      int CNY = Comb.height;

      pQ.Put(Pinp);
      while (!pQ.Empty()) //===========================================================================
      {
        P = pQ.Get();
        if ((Comb.Grid[P.X + CNX * P.Y] & 128) != 0) continue;
        for (dir = 0; dir < 4; dir++) //================================================================
        {
          Crack.X = P.X + Step[dir].X;
          Crack.Y = P.Y + Step[dir].Y;
          if (Crack.X < 0 || Crack.X > CNX - 1 || Crack.Y < 0 || Crack.Y > CNY - 1) continue;
          if (Comb.Grid[Crack.X + CNX * Crack.Y] == 1) //---- ------------ -----------
          {
            Pnext.X = Crack.X + Step[dir].X;
            Pnext.Y = Crack.Y + Step[dir].Y;
            LabNext = Comb.Grid[Pnext.X + CNX * Pnext.Y] & 7; // changed on Nov. 1 2015
            if (LabNext >= 3) pQ.Put(Pnext);
            if (LabNext == 2) //-------------------------------------------------------------------------
            {
              Polygon[nPolygon].firstVert = nVert;
              dirT = dir;
              Pterm.X = 0;
              Pterm.Y = 0;

              int nVertRes = nVert;
             // rv = TraceAppNew(Comb, P.X, P.Y, eps, ref Pterm, ref dirT);
              rv = TraceApp(Comb, P.X, P.Y, eps, ref Pterm, ref dirT);

              if (rv < 0)
              {
                MessageBox.Show("ComponPoly, Alarm! TraceApp returned " + rv + ". return -1.");
                return -1;
              }
              if (nPolygon > MaxPoly - 1)
              {
                MessageBox.Show("ComponPoly: Overflow in Polygon; nPolygon=" + nPolygon + " MaxPoly=" + MaxPoly);
                return -1;
              }

              if (Polygon[nPolygon].lastVert - Polygon[nPolygon].firstVert > 1) nPolygon++;
              else nVert = nVertRes;

              if ((Comb.Grid[Pterm.X + CNX * Pterm.Y] & 128) == 0) // '128' is now a label for visited points; Pterm is new
              {
                if (rv >= 3) pQ.Put(Pterm);
              } //------------ end if  ((Comb.Grid[Pterm.X... ----------------------------------------
            } // ------------- end if (LabNest==2) -----------------------------------------------------
            if ((Comb.Grid[P.X + CNX * P.Y] & 7) == 1) break;
          } //--------------- end if (Comb.Grid[Crack.X ...==1) ------------------------------------------
        } //================================== end for (dir ... ==========================================
        Comb.Grid[P.X + CNX * P.Y] |= 128;
      } //==================================== end while ==========================================
      return 1;
    } //************************************** end ComponPoly ************************************************



    public int SearchPoly(ref CImage Comb, double eps, Form1 fm1)
    {
      int cnt1 = 0, cnt2 = 0, Lab, rv = 0, x, y, CNX = Comb.width, CNY = Comb.height;
      if (Step[0].Y < 0) x = Step[0].Y;
      fm1.progressBar1.Visible = true;
      fm1.progressBar1.Step = 1;
      int jump, Len = CNY;
      if (Len > 300) jump = Len / (100 / 6);
      else jump = 2;
      for (y = 0; y < CNY; y += 2)
      {
        if ((y % jump) == jump - 1) fm1.progressBar1.PerformStep();
        for (x = 0; x < CNX; x += 2)
        {
          Lab = Comb.Grid[x + CNX * y] & 7;
          if (Lab == 1 || Lab == 3 || Lab == 4)
          {
            rv = ComponPoly(Comb, x, y, eps);
            cnt1++;
            if (rv < 0)
            {
              MessageBox.Show("SearchPoly, Alarm! ComponPoly returned " + rv);
              return -1;
            }
          }
        }
      }

      // Starting the search for loops:
      for (y = 0; y < CNY; y += 2)
        for (x = 0; x < CNX; x += 2)
        {
          Lab = Comb.Grid[x + CNX * y] & 3;
          if (Lab == 2)
          {
            rv = ComponPoly(Comb, x, y, eps);
            cnt2++;
            if (rv < 0)
            {
              MessageBox.Show("SearchPoly, Alarm! ComponPoly returned " + rv);
              return -1;
            }
          }
        }
      //if (MessReturn("SearchPoly: cnt1=" + cnt1 + " cnt2=" + cnt2) < 0) return -1;
      return nVert;
    } //****************************** end SearchPoly ********************************


    public double ObjectFunc2(double a, double b, double c, double d, double[,] Points, int numbPoints)
    {
      double Sum = 0.0;
      for (int ip = 0; ip < numbPoints; ip++)
      {
        double brake = (Points[ip, 0] - c) * (Points[ip, 0] - c) / a / a +
                (Points[ip, 1] - d) * (Points[ip, 1] - d) / b / b - 1.0;
        Sum += brake * brake;
      }
      return Sum;
    }

    //ObjectFuncPoint(a, b, c, d, Points, startPoint, numbPoints
    public double ObjectFuncPoint(double a, double b, double c, double d, Point[] Points, int startPoint, int numbPoints)
    {
      double Sum = 0.0;
      for (int ip = startPoint; ip < startPoint + numbPoints; ip++)
      {
        double brake = (Points[ip].X - c) * (Points[ip].X - c) / a / a +
                (Points[ip].Y - d) * (Points[ip].Y - d) / b / b - 1.0;
        Sum += brake * brake;
      }
      return Sum;
    }

    public int CheckArc(Point[] Vert, int ia, int ja, ref double aStart, ref double bStart,
                                      ref double cStart, ref double dStart, Form1 fm1)
    {
      double[,] A = new double[4, 4];
      double[,] B = new double[4, 1];
      int[] Iv = new int[4];
      int i, iv = elArc[ia].Start, iv1 = -1, nP = elArc[ia].nPoints - 1, nP1 = 0;

      if (ja < 0)
      {
        for (i = 0; i < 4; i++)
        {
          Iv[i] = iv + i * (nP - 1) / 3;
        }
      }
      else
      {
        iv1 = elArc[ja].Start;
        nP1 = elArc[ja].nPoints - 1;
        for (i = 0; i < 3; i++)
        {
          Iv[i] = iv + i * (nP - 1) / 2;
        }
        Iv[3] = iv1;
      }

      A[0, 0] = -2.0 * Vert[Iv[0]].X; A[0, 1] = Vert[Iv[0]].Y * Vert[Iv[0]].Y;
      A[0, 2] = -2.0 * Vert[Iv[0]].Y; A[0, 3] = 1.0;

      A[1, 0] = -2.0 * Vert[Iv[1]].X; A[1, 1] = Vert[Iv[1]].Y * Vert[Iv[1]].Y;
      A[1, 2] = -2.0 * Vert[Iv[1]].Y; A[1, 3] = 1.0;

      A[2, 0] = -2.0 * Vert[Iv[2]].X; A[2, 1] = Vert[Iv[2]].Y * Vert[Iv[2]].Y;
      A[2, 2] = -2.0 * Vert[Iv[2]].Y; A[2, 3] = 1.0;

      A[3, 0] = -2.0 * Vert[Iv[3]].X; A[3, 1] = Vert[Iv[3]].Y * Vert[Iv[3]].Y;
      A[3, 2] = -2.0 * Vert[Iv[3]].Y; A[3, 3] = 1.0;


      B[0, 0] = -Vert[Iv[0]].X * Vert[Iv[0]].X;
      B[1, 0] = -Vert[Iv[1]].X * Vert[Iv[1]].X;
      B[2, 0] = -Vert[Iv[2]].X * Vert[Iv[2]].X;
      B[3, 0] = -Vert[Iv[3]].X * Vert[Iv[3]].X;

      Gauss_K(A, 4, B, 1);
      double a, b, c, d;
      a = Math.Sqrt(-B[3, 0] + B[0, 0] * B[0, 0] + B[1, 0] * B[2, 0] * B[2, 0] / B[1, 0] / B[1, 0]);
      double a2 = -(B[3, 0] - B[0, 0] * B[0, 0] - B[1, 0] * B[2, 0] * B[2, 0] / B[1, 0] / B[1, 0]);
      double b2 = a2 / -B[1, 0];
      b = Math.Sqrt(b2);
      c = B[0, 0];
      d = B[2, 0] / B[1, 0];
      aStart = a;
      bStart = b;
      cStart = c;
      dStart = d;
      return 1;
    } //****************************** end CheckArc *****************************************

    public void SWAP(ref double a, ref double b)
    {
      double temp = a;
      a = b;
      b = temp;
    }



    public int Gauss_K(double[,] a, int n, double[,] b, int m)
    /* Solves the system of 'n' linear equations with 'n' unknows with
      he matrix "a[n,n]" and different right values "b[,]". The "m" solutions
      will be placed to "b[n, m]". Taken from the book "Numerical Recipes in C",
      1990, page 36 and suited to C#. Returns a -1 in the case of an error,
      or a 1 else. --  */
    {
      int[] indxc, indxr, ipiv;
      int i, icol = 0, irow = 0, j, k, l, ll;
      double big, dum, pivinv;
      // *********************   B E G I N   *******************************
      indxc = new int[n];
      indxr = new int[n];
      ipiv = new int[n];
      for (j = 0; j < n; j++) ipiv[j] = 0;
      for (i = 0; i < n; i++) // ==============================================
      {
        big = 0.0;
        for (j = 0; j < n; j++) //===========================================
        {
          if (ipiv[j] != 1) //-----------------------------------------
            for (k = 0; k < n; k++) // =================================				    
            {
              if (ipiv[k] == 0)
              {
                if (Math.Abs(a[j, k]) >= big) // -------
                {
                  big = Math.Abs(a[j, k]);
                  irow = j;
                  icol = k;
                } // ------- end if (Math.Abs... ------- 
              }
              else
                if (ipiv[k] > 1) return -1;
            } //=============== end for (k... ========================
          // ------------------ end if (ipiv... ------------------------
        } // ==================== end for (j... ============================

        ++(ipiv[icol]);
        if (irow != icol)
        {
          for (l = 0; l < n; l++) SWAP(ref a[irow, l], ref a[icol, l]); // t=a[irow,l]; a[irow,l]=a[icol, l];a[icol,l]=t
          for (l = 0; l < m; l++) SWAP(ref b[irow, l], ref b[icol, l]);
        }
        indxr[i] = irow;
        indxc[i] = icol;
        if (a[icol, icol] == 0.0) return -1;
        
        pivinv = 1.0 / a[icol, icol];
        a[icol, icol] = 1.0;
        for (l = 0; l < n; l++) a[icol, l] *= pivinv;
        for (l = 0; l < m; l++) b[icol, l] *= pivinv;

        for (ll = 0; ll < n; ll++) // ==============================
          if (ll != icol)
          {
            dum = a[ll, icol];
            a[ll, icol] = 0.0;
            for (l = 0; l < n; l++) a[ll, l] -= a[icol, l] * dum;
            for (l = 0; l < m; l++) b[ll, l] -= b[icol, l] * dum;
          }
        //=============== end for (ll... =========================
      } // ============== end for (i...  ==============================

      for (l = n - 1; l >= 0; l--) //=========================== 
      {
        if (indxr[l] != indxc[l])
          for (k = 0; k < n; k++)
            SWAP(ref a[k, indxr[l]], ref a[k, indxc[l]]);
      } // =============== end for (l=n-1;... ==============
      return 1;
    } // ************************* end Gauss_K ****************************


    public int CheckArcPoint(int startPoint, int nPoint, Point[] Points, ref double aStart, ref double bStart,
                                          ref double cStart, ref double dStart, Form1 fm1)
    // Calculates an ellipse while starting from the array 'Points'.
    {
      double[,] A = new double[4, 4];
      double[,] B = new double[4, 1];
      int[] Ip = new int[4];
      int i;


      for (i = 0; i < 4; i++)
      {
        Ip[i] = startPoint + i * (nPoint - 1) / 3;
      }

      A[0, 0] = -2.0 * Points[Ip[0]].X; A[0, 1] = Points[Ip[0]].Y * Points[Ip[0]].Y;
      A[0, 2] = -2.0 * Points[Ip[0]].Y; A[0, 3] = 1.0;

      A[1, 0] = -2.0 * Points[Ip[1]].X; A[1, 1] = Points[Ip[1]].Y * Points[Ip[1]].Y;
      A[1, 2] = -2.0 * Points[Ip[1]].Y; A[1, 3] = 1.0;

      A[2, 0] = -2.0 * Points[Ip[2]].X; A[2, 1] = Points[Ip[2]].Y * Points[Ip[2]].Y;
      A[2, 2] = -2.0 * Points[Ip[2]].Y; A[2, 3] = 1.0;

      A[3, 0] = -2.0 * Points[Ip[3]].X; A[3, 1] = Points[Ip[3]].Y * Points[Ip[3]].Y;
      A[3, 2] = -2.0 * Points[Ip[3]].Y; A[3, 3] = 1.0;


      B[0, 0] = -Points[Ip[0]].X * Points[Ip[0]].X;
      B[1, 0] = -Points[Ip[1]].X * Points[Ip[1]].X;
      B[2, 0] = -Points[Ip[2]].X * Points[Ip[2]].X;
      B[3, 0] = -Points[Ip[3]].X * Points[Ip[3]].X;

      Gauss_K(A, 4, B, 1);
      double a, b, c, d;
      a = Math.Sqrt(-B[3, 0] + B[0, 0] * B[0, 0] + B[1, 0] * B[2, 0] * B[2, 0] / B[1, 0] / B[1, 0]);
      b = a * Math.Sqrt(1.0 / B[1, 0]);
      c = B[0, 0];
      d = B[2, 0] / B[1, 0];
      Pen redPen = new Pen(Color.Red);
      aStart = a;
      bStart = b;
      cStart = c;
      dStart = d;
      return 1;
    } //****************************** end CheckArcPoint *****************************************



    public int MinimumSmart(int numbPoints, double[,] Points, Point[] Vert, int ia, int ja,
                                ref double a, ref double b, ref double c, ref double d, ref double fret, Form1 fm1)
    {
      int i1, i2, i3, i4;
      double[] OptAbcd = new double[5];

      double Dc = 0.0;
      double Dd = 0.0;
      fret = 9999999999999.9;

      double aStart = 0.0, bStart = 0.0, cStart = 0.0, dStart = 0.0;
      CheckArc(Vert, ia, ja, ref aStart, ref bStart, ref cStart, ref dStart, fm1);
     if (!(aStart > 1.0 && bStart > 1.0)) return -1;
      aStart *= 0.9;
      bStart *= 0.9;
      cStart *= 0.9;
      dStart *= 0.9;
      int nStep = 25;

      double aStep = 0.2 * aStart / (double)nStep;
      double bStep = 0.2 * bStart / (double)nStep;
      double cStep = 0.2 * cStart / (double)nStep;
      double dStep = 0.2 * dStart / (double)nStep;

      a = aStart;
      for (i1 = 0; i1 < nStep; i1++)
      {
        b = bStart;
        for (i2 = 0; i2 < nStep; i2++)
        {
          c = cStart;
          for (i3 = 0; i3 < nStep; i3++)
          {
            d = dStart;
            for (i4 = 0; i4 < nStep; i4++)
            {
              if (ObjectFunc2(a, b, c, d, Points, numbPoints) < fret)
              {
                fret = ObjectFunc2(a, b, c, d, Points, numbPoints);
                OptAbcd[1] = a;
                OptAbcd[2] = b;
                OptAbcd[3] = c;
                OptAbcd[4] = d;
              }
              d += dStep;
            }
            c += bStep;
          }
          b += bStep;
        }
        a += aStep;
      }

      double[] OptAbcd2 = new double[5];
      double MultStep = 0.01;
      fret = 1.0;
      for (a = OptAbcd[1] * 0.9; a < OptAbcd[1] * 1.1; a += OptAbcd[1] * MultStep) // es war 0.7
      {
        for (b = OptAbcd[2] * 0.9; b < OptAbcd[2] * 1.2; b += OptAbcd[2] * MultStep)// es war 0.7
        {
          for (c = OptAbcd[3] * 0.9; c < OptAbcd[3] * 1.1; c += OptAbcd[3] * MultStep)
          {
            for (d = OptAbcd[4] * 0.9; d < OptAbcd[4] * 1.1; d += OptAbcd[4] * MultStep)
            {
              if (ObjectFunc2(a, b, c, d, Points, numbPoints) < fret)
              {
                fret = ObjectFunc2(a, b, c, d, Points, numbPoints);
                OptAbcd2[1] = a;
                OptAbcd2[2] = b;
                OptAbcd2[3] = c;
                OptAbcd2[4] = d;
              }
            }
          }
        }
      } //-*/
      a = OptAbcd2[1];
      b = OptAbcd2[2];
      c = OptAbcd2[3];
      d = OptAbcd2[4];
      return 1;
    } //********************** end MinimumSmart ***************************************************



    public double Dist(int ia, int ja) 
    {
      int first, firstJ, iv, iv1, last, lastJ;
      double dist = 0.0, lengthI = 0.0, lengthJ = 0.0, minDist, normXI = 0.0, normYI = 0.0, normXJ = 0.0, normYJ = 0.0, projI, projJ;

      first = elArc[ia].Start;
      last = elArc[ia].Start + elArc[ia].nPoints - 1;
      minDist = 10000.0;
      for (iv = first; iv <= last; iv++) //====================================================
      {
        if (iv < last)
        {
          normXI = Vert[iv + 1].Y - Vert[iv].Y;
          normYI = Vert[iv].X - Vert[iv + 1].X;
          lengthI = Math.Sqrt(normXI * normXI + normYI * normYI);
          normXI /= lengthI;
          normYI /= lengthI;
        }

        firstJ = elArc[ja].Start;
        lastJ = elArc[ja].Start + elArc[ja].nPoints - 1;
        for (iv1 = firstJ; iv1 <= lastJ; iv1++) //====================================================
        {
          if (iv1 < lastJ)
          {
            normXJ = Vert[iv1 + 1].Y - Vert[iv1].Y;
            normYJ = Vert[iv1].X - Vert[iv1 + 1].X;
            lengthJ = Math.Sqrt(normXJ * normXJ + normYJ * normYJ);
            normXJ /= lengthJ;
            normYJ /= lengthJ;
          }

          if (iv < last)
            projJ = (Vert[iv1].X - Vert[iv].X) * (Vert[iv + 1].X - Vert[iv].X) +
                                (Vert[iv1].Y - Vert[iv].Y) * (Vert[iv + 1].Y - Vert[iv].Y);
          else projJ = -1.0;

          if (iv1 < lastJ)
            projI = (Vert[iv].X - Vert[iv1].X) * (Vert[iv1 + 1].X - Vert[iv1].X) +
                                (Vert[iv].Y - Vert[iv1].Y) * (Vert[iv1 + 1].Y - Vert[iv1].Y);
          else projI = -1.0;

          dist = Math.Sqrt((Vert[iv1].X - Vert[iv].X) * (Vert[iv1].X - Vert[iv].X) +
                            (Vert[iv1].Y - Vert[iv].Y) * (Vert[iv1].Y - Vert[iv].Y)); // Euclid iv1, iv
          if (dist < minDist) minDist = dist;

          if (projJ >= 0.0 && projJ <= (lengthI * lengthI))
          {
            dist = (Vert[iv1].X - Vert[iv].X) * normXI + (Vert[iv1].Y - Vert[iv].Y) * normYI;
            if (Math.Abs(dist) < minDist) minDist = Math.Abs(dist);
          }

          if (projI >= 0.0 && projI <= (lengthJ * lengthJ))
          {
            dist = (Vert[iv].X - Vert[iv1].X) * normXJ + (Vert[iv].Y - Vert[iv1].Y) * normYJ;
            if (Math.Abs(dist) < minDist) minDist = Math.Abs(dist);
          }
        }
      }
      return minDist;
    } //**************************** end Dist *************************************+



    public double GetAngle(int ia)
    {
      if (elArc[ia].nPoints == 2) return 0.0;
      int iv, ivEnd;
      iv = elArc[ia].Start;
      ivEnd = iv + elArc[ia].nPoints - 1;
      bool Positive;
      double Angle, Dif, Az1, Az2, Az3;
      Az1 = Math.Atan2((double)Vert[iv].Y - elArc[ia].My, (double)Vert[iv].X - elArc[ia].Mx);
      Az2 = Math.Atan2((double)Vert[iv + 1].Y - elArc[ia].My, (double)Vert[iv + 1].X - elArc[ia].Mx);
      Dif = Az2 - Az1;
      if (Math.Abs(Dif) > 3.14)
        if (Dif < 0) Dif += 2 * Math.PI; // Dif cannot be == 0.0
        else Dif -= 2 * Math.PI;
      Positive = Dif > 0.0;
      Az3 = Math.Atan2((double)Vert[ivEnd].Y - elArc[ia].My, (double)Vert[ivEnd].X - elArc[ia].Mx);
      if (Positive) Angle = Az3 - Az1;
      else Angle = Az1 - Az3;
      if (Angle < 0) Angle += 2 * Math.PI;
      return Angle;
    }

   
    public int MakeArcsTwo(PictureBox picturebox, Form1 fm1)
    /* Searches in polygons the sequences of not too long edges with almost constant angles between
      subsequent edges. Each such sequence will be encoded with its starting point and
      the number of points in it. These valuesare saved in "elArc[]". The difference to the method "MakeArcs3"
      is that here we use no arrays "Start[2]" and "End[2]" but simple variables. The method "MinAreN2" is
      called only in "SaveArc2". -----------*/
    {
      int First, iSignCurv = 1, j, iPol, last, polLen;
      int LenLab = 0;
      if (nPolygon > 0) LenLab = Polygon[nPolygon - 1].lastVert + 100;
      double[] Curv = new double[LenLab];
      int End, Start;
      nArcs = 0;
      Graphics g = picturebox.CreateGraphics();
      int ky = fm1.height;
      fm1.progressBar1.Visible = true;
      fm1.progressBar1.Step = 1;
      int jump, Len = nPolygon;
      if (Len > 300) jump = Len / (100 / 6);
      else jump = 2;

      for (iPol = 0; iPol < nPolygon; iPol++) //==========================================================
      {
        if ((iPol % jump) == jump - 1) fm1.progressBar1.PerformStep();
        First = Polygon[iPol].firstVert;
        last = Polygon[iPol].lastVert;
        polLen = last - First + 1;
        if (polLen < 3) continue;
        End = Start = -1;
        double fmx = 0.0, fmy = 0.0;
        bool bAngle = false, bSign = false, bCurv = false, bEdge = false;
        int iSwitch = 0, iSwitchOld = 4;
        for (j = First; j <= last; j++) //================== single loop ======================================
        {
          if (j == First + 1) Curv[0] = 0.0;
          if (j > First + 1)
          {
            Curv[j - 1] = ThreePoints(Vert, j - 2, j - 1, j, ref fmx, ref fmy);
            if (Curv[j - 1] >= 0.0) iSignCurv = 1;
            else iSignCurv = -1;
            int VP = (Vert[j - 1].X - Vert[j - 2].X) * (Vert[j].Y - Vert[j - 1].Y) -
                    (Vert[j - 1].Y - Vert[j - 2].Y) * (Vert[j].X - Vert[j - 1].X);
            int SP = (Vert[j - 1].X - Vert[j - 2].X) * (Vert[j].X - Vert[j - 1].X) +
                    (Vert[j - 1].Y - Vert[j - 2].Y) * (Vert[j].Y - Vert[j - 1].Y);

            bCurv = Math.Abs(Curv[j - 1]) > 1.0 / 0.024 * fm1.width;

            double ThrAngle = 0.8; // it was 0.8

            bAngle = Math.Abs(Math.Atan2((double)(VP), (double)(SP))) > ThrAngle; // 0.52; // 0.7 is 40 degree
            bEdge = Math.Sqrt((double)(Vert[j].X - Vert[j - 1].X) * (double)(Vert[j].X - Vert[j - 1].X) +
                          (double)(Vert[j].Y - Vert[j - 1].Y) * (double)(Vert[j].Y - Vert[j - 1].Y)) >
                          3.5 * Math.Sqrt(2.0 * 1.7 * 100);
            if (bAngle) iSwitch = 2;
            else iSwitch = 0;
            if (bEdge) iSwitch += 4;
            if (bCurv) iSwitch += 8;
          } //----------------------------- end if (j > First + 1) -----------------------------

          if (j > First + 2) //-----------------------------------------------------------------------------
          {
            bSign = Curv[j - 1] * Curv[j - 2] < 0.0;
            if (bSign)
            {
              iSwitch++;
              End = j - 1;
            }
          } // ------------------------- end if (j > First + 2) --------------------------------------------

          if (j > First)
          {
            if (iSwitch == 0) //-----------------------------------------------------------------
            {
              if (Start == -1)
              {     // iSwitchOld > 0 means that the arc has finished at j - 1;
                if (iSwitchOld == 1) Start = j - 3;
                if (iSwitchOld == 2 || iSwitchOld == 3) Start = j - 2;
                if (iSwitchOld > 3) Start = j - 1;
              }
              if (j == last) End = last;
            }
            else // iSwitch > 0
            {
              End = j - 1;
              if (Start >= 0 && End >= Start + 2) // nPoints >= 3
              {
                Polygon[iPol].firstArc = nArcs;
                SaveArc(iPol, iSignCurv, ref Start, ref End);
                Start = j - 1;
              }
              else
              {
                End = -1;
                Start = j - 1;
              }
            } //------------------------ end if (switch == 0)  ------------------------------------------------
            iSwitchOld = iSwitch;
          } //------------------------------------- end if (j>First+2) ----------------------------------------------------------
          if (j == last)
          {
            if (Start >= 0 && End >= Start + 2) // && iSwitch == 0)
            {
              End = last;

              Polygon[iPol].lastArc = nArcs;
              SaveArc(iPol, iSignCurv, ref Start, ref End);
            }
          }
        } //========================== end for (j... ================================================================
      } //============================== end for (iPol... =================================================================

      int cnt = 0;
      for (int ia = 0; ia < nArcs; ia++) //==========================================================================
      {
        int ivEnd = 0, ivMiddle = 0, ivStart = 0, xEnd, xMid, xStart, yEnd, yMid, yStart;

        ivStart = elArc[ia].Start;
        ivEnd = elArc[ia].Start + elArc[ia].nPoints;
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

          double StartAz = Math.Atan2((double)yStart - elArc[ia].My, (double)xStart - elArc[ia].Mx);
          double EndAz = Math.Atan2((double)yEnd - elArc[ia].My, (double)xEnd - elArc[ia].Mx);

          elArc[ia].Angle = GetAngle(ia);
        } //------------------------------- end if (ivMiddle > 0) -----------------------------------------------
      } //================================= end for (int ia = 0;  ====================================================
      return nArcs;
    } //******************************** end MakeArcsTwo **************************************
    

    public int SortingArcs(int[] SortArcs, ref int maxNP)
    {
      int[] Histo = new int[50];
      int[] nArcsWith = new int[50];
      int i, ia, nP;
      for (i = 0; i < 50; i++) Histo[i] = nArcsWith[i] = 0;

      for (ia = 0; ia < nArcs; ia++)
      {
        Histo[elArc[ia].nPoints]++;
        elArc[ia].arcOK = true;
      }
      int[] Arcs3 = new int[10000];
      int[] Arcs4 = new int[10000];
      int[,] ArcsM = new int[50, 10000];

      maxNP = 0;
      for (ia = 0; ia < nArcs; ia++)
      {
        nP = elArc[ia].nPoints;
        if (nP > maxNP) maxNP = nP;
        if (nP == 3)
        {
          Arcs3[nArcsWith[3]] = ia;
          nArcsWith[3]++;
        }
        if (nP == 4)
        {
          Arcs4[nArcsWith[4]] = ia;
          nArcsWith[4]++;
        }
        if (nP > 4)
        {
          ArcsM[nP, nArcsWith[nP]] = ia;
          nArcsWith[nP]++;
        }
      }

      int k = 0;
      for (nP = maxNP; nP >= 5; nP--)
        for (i = 0; i < nArcsWith[nP]; i++)
        {
          SortArcs[k] = ArcsM[nP, i];
          k++;
        }

      for (i = 0; i < nArcsWith[4]; i++)
      {
        SortArcs[k] = Arcs4[i];
        k++;
      }

      for (i = 0; i < nArcsWith[3]; i++)
      {
        SortArcs[k] = Arcs3[i];
        k++;
      }
      return k;
    }

    public bool PointInBox(Point P, CBox Box)
    {
      if (P.X > Box.minX && P.X < Box.maxX && P.Y > Box.minY && P.Y < Box.maxY) return true;
      return false;
    }


    public int DrawLineSmart(Point P1, Point P2, Form1 fm1)
    {
      Pen redPen = new Pen(Color.Red);
      int x1 = fm1.marginX + (int)(fm1.Scale1 * P1.X);
      int y1 = fm1.marginY + (int)(fm1.Scale1 * P1.Y);
      int x2 = fm1.marginX + (int)(fm1.Scale1 * P2.X);
      int y2 = fm1.marginY + (int)(fm1.Scale1 * P2.Y);
      fm1.g2Bmp.DrawLine(redPen, x1, y1, x2, y2);
      fm1.pictureBox2.Image = fm1.BmpPictBox2;
      return 1;
    }

    public int DrawRectangleSmart(CBox Box, Form1 fm1)
    {
      Pen whitePen = new Pen(Color.White);
      int x1 = fm1.marginX + (int)(fm1.Scale1 * Box.minX);
      int y1 = fm1.marginY + (int)(fm1.Scale1 * Box.minY);
      int width = (int)(fm1.Scale1 * (Box.maxX - Box.minX));
      int height = (int)(fm1.Scale1 * (Box.maxY - Box.minY));
      fm1.g2Bmp.DrawRectangle(whitePen, x1, y1, width, height);
      fm1.pictureBox2.Image = fm1.BmpPictBox2;
      return 1;
    }


    public void DrawArcs(bool Ori, bool Rect, int minNp, Form1 fm1)
    /* Draws the arcs. A basic algorithm for tracing a part of a polygon
      is realized here. The index "i" is the number of a point relativ
      to the first point "First" of the polygon: i == 0 at "First".  --------*/
    {
      int First, ia, iPol, iv, ivStart, lenArc, lenPoly, x, xold, xori, y, yold, yori;
      SolidBrush redBrush = new SolidBrush(Color.Red);
      SolidBrush greenBrush = new SolidBrush(Color.Green);
      SolidBrush blackBrush = new SolidBrush(Color.Black);
      Pen whitePen = new Pen(Color.White);
      Rectangle rect1, rect2;
      rect1 = new Rectangle(0, 0, fm1.width, fm1.height);
      fm1.g2Bmp.FillRectangle(blackBrush, rect1);
      Pen myPen;
      int penWidth = fm1.width / 180;
      myPen = new Pen(Color.Green, penWidth);

      int x2, y2;
      for (ia = 0; ia < nArcs; ia++) //=============================
      {
        iPol = elArc[ia].iPol;
        First = Polygon[iPol].firstVert;
        lenPoly = Polygon[iPol].lastVert - First + 1;
        lenArc = elArc[ia].nPoints;

        if (lenArc < minNp) continue;
        ivStart = elArc[ia].Start;

        xori = Vert[ivStart].X;
        yori = Vert[ivStart].Y;

        // Drawing the arcs:
        if (lenArc >= minNp)
        {
          ivStart = elArc[ia].Start;

          xold = Vert[ivStart].X; // fm1.marginX + (int)(fm1.Scale1 * Vert[ivStart].X);
          yold = Vert[ivStart].Y; // fm1.marginY + (int)(fm1.Scale1 * Vert[ivStart].Y);
          for (iv = ivStart + 1; iv < elArc[ia].Start + elArc[ia].nPoints - 1; iv++) //=====================
          {
            x = Vert[iv].X; // fm1.marginX + (int)(fm1.Scale1 * Vert[iv].X);
            y = Vert[iv].Y; //fm1.marginY + (int)(fm1.Scale1 * Vert[iv].Y);
            // drawing the arcs:
            fm1.g2Bmp.DrawLine(myPen, xold, yold, x, y); // drawing arcs
            fm1.pictureBox2.Image = fm1.BmpPictBox2;
            xold = x; yold = y;
          } //===================== end for (iv ... ==========================================================
        }
        // Vertex in arcs:
        if (lenArc >= minNp && Rect)
          for (iv = elArc[ia].Start; iv <= elArc[ia].Start + lenArc; iv++) //=====================
          {
            x = Vert[iv].X; //fm1.marginX + (int)(fm1.Scale1 * Vert[iv].X);
            y = Vert[iv].Y; //fm1.marginY + (int)(fm1.Scale1 * Vert[iv].Y);
            rect1 = new Rectangle(x, y, 2, 2);
            fm1.g2Bmp.FillRectangle(redBrush, rect1);
          }

        int ivMiddle = elArc[ia].Start + elArc[ia].nPoints / 2;
       
        if (ivMiddle > 0 && Ori) // drawing orientation
        {
          // curvature centers:
          rect1 = new Rectangle(fm1.marginX + (int)(fm1.Scale1 * elArc[ia].Mx),
                                          fm1.marginY + (int)(fm1.Scale1 * elArc[ia].My), 3, 3);
          fm1.g2Bmp.FillRectangle(redBrush, rect1);


          // middle points:
          x2 = Vert[ivMiddle].X; //fm1.marginX + (int)(fm1.Scale1 * Vert[ivMiddle].X);
          y2 = Vert[ivMiddle].Y; //fm1.marginY + (int)(fm1.Scale1 * Vert[ivMiddle].Y);
          rect2 = new Rectangle(x2, y2, 2, 2);
          fm1.g2Bmp.FillRectangle(greenBrush, rect2);

          if (elArc[ia].Mx != 0.0)
            fm1.g2Bmp.DrawLine(whitePen, fm1.marginX + (int)(fm1.Scale1 * elArc[ia].Mx),
              fm1.marginY + (int)(fm1.Scale1 * elArc[ia].My), x2, y2);         
        } //----------------------- end if (lenArc ... ----------------------------------------------
      } //========================= end for (ia... =====================================================
    } // ************************** end DrawArcs ***********************************************************************


    public int GetCircle(int ia, ref double a, ref double b, ref double c, ref double d)
    {
      a = b = elArc[ia].R;
      c = elArc[ia].Mx;
      d = elArc[ia].My;
      return 1;
    }

 
    public void DrawCircle(double r, double c, double d, Form1 fm1)
    {
      int xold, x, yold, y;
      double Step = 0.1;
      Graphics g = fm1.g2Bmp; // pictureBox2.CreateGraphics();
      Pen pen = new Pen(Color.Yellow);
      xold = fm1.marginX + (int)(fm1.Scale1 * (c + r));
      yold = fm1.marginY + (int)(fm1.Scale1 * d);
      for (double t = 0; t < 2 * Math.PI; t += Step)
      {
        x = fm1.marginX + (int)(fm1.Scale1 * (c + r * Math.Cos(t)));
        y = fm1.marginY + (int)(fm1.Scale1 * (d + r * Math.Sin(t)));
        g.DrawLine(pen, xold, yold, x, y);
        xold = x; yold = y;
      }
      fm1.pictureBox2.Image = fm1.BmpPictBox2;
    }

   
    public int DrawOneLongArc(int ia, Form1 fm1)
    // Drawing an arc of many points.
    {
      int x1, x2, y1, y2;
      int iv1 = elArc[ia].Start;
      int iv2 = elArc[ia].Start + elArc[ia].nPoints - 1;
      Graphics g = fm1.g2Bmp; 
      Pen yellowPen = new Pen(Color.Yellow);
      x1 = fm1.marginX + (int)(fm1.Scale1 * Vert[iv1].X);
      y1 = fm1.marginY + (int)(fm1.Scale1 * Vert[iv1].Y);
      for (int iv = iv1 + 1; iv <= iv2; iv++)
      {
        x2 = fm1.marginX + (int)(fm1.Scale1 * Vert[iv].X);
        y2 = fm1.marginY + (int)(fm1.Scale1 * Vert[iv].Y);
        g.DrawLine(yellowPen, x1, y1, x2, y2);
        x1 = x2;
        y1 = y2;
      }
      fm1.pictureBox2.Image = fm1.BmpPictBox2;
      return 1;
    }


    public bool ArcInBox(int ia, CBox Box)
    {
      int iv = elArc[ia].Start, iv1 = elArc[ia].Start + elArc[ia].nPoints / 2,
                                            iv2 = elArc[ia].Start + elArc[ia].nPoints - 1;
      int x = Vert[iv].X, x1 = Vert[iv1].X, x2 = Vert[iv2].X;
      int y = Vert[iv].Y, y1 = Vert[iv1].Y, y2 = Vert[iv2].Y;
      if (x > Box.minX && x < Box.maxX && y > Box.minY && y < Box.maxY &&
          x1 > Box.minX && x1 < Box.maxX && y1 > Box.minY && y1 < Box.maxY &&
          x2 > Box.minX && x2 < Box.maxX && y2 > Box.minY && y2 < Box.maxY) return true;
      return false;
    }


    public int CheckArc2(Point[] Vert, int ia, int ja, ref double a, ref double b,
      ref double c, ref double d, Form1 fm1)
    {
      double[,] A = new double[4, 4];
      double[,] B = new double[4, 1];
      int[] Iv = new int[4];
      int i, iv = elArc[ia].Start, iv1 = 0, nP = elArc[ia].nPoints - 1, nP1 = 0;

      if (ja < 0)
        for (i = 0; i < 4; i++)
        {
          Iv[i] = iv + i * (nP - 1) / 3;
        }
      else
      {
        iv1 = elArc[ja].Start;
        nP1 = elArc[ja].nPoints - 1;
        for (i = 0; i < 3; i++)  Iv[i] = iv + i * (nP - 1) / 2;
        Iv[3] = iv1;
      }


      A[0, 0] = -2.0 * Vert[Iv[0]].X; A[0, 1] = Vert[Iv[0]].Y * Vert[Iv[0]].Y;
      A[0, 2] = -2.0 * Vert[Iv[0]].Y; A[0, 3] = 1.0;

      A[1, 0] = -2.0 * Vert[Iv[1]].X; A[1, 1] = Vert[Iv[1]].Y * Vert[Iv[1]].Y;
      A[1, 2] = -2.0 * Vert[Iv[1]].Y; A[1, 3] = 1.0;

      A[2, 0] = -2.0 * Vert[Iv[2]].X; A[2, 1] = Vert[Iv[2]].Y * Vert[Iv[2]].Y;
      A[2, 2] = -2.0 * Vert[Iv[2]].Y; A[2, 3] = 1.0;

      A[3, 0] = -2.0 * Vert[Iv[3]].X; A[3, 1] = Vert[Iv[3]].Y * Vert[Iv[3]].Y;
      A[3, 2] = -2.0 * Vert[Iv[3]].Y; A[3, 3] = 1.0;


      B[0, 0] = -Vert[Iv[0]].X * Vert[Iv[0]].X;
      B[1, 0] = -Vert[Iv[1]].X * Vert[Iv[1]].X;
      B[2, 0] = -Vert[Iv[2]].X * Vert[Iv[2]].X;
      B[3, 0] = -Vert[Iv[3]].X * Vert[Iv[3]].X;

      Gauss_K(A, 4, B, 1);
      a = Math.Sqrt(-B[3, 0] + B[0, 0] * B[0, 0] + B[1, 0] * B[2, 0] * B[2, 0] / B[1, 0] / B[1, 0]);
      b = a * Math.Sqrt(1.0 / B[1, 0]);
      c = B[0, 0];
      d = B[2, 0] / B[1, 0];
      return 1;
    }  //****************************** end CheckArc2 *****************************************


    public int DrawRedArc(int ia, Form1 fm1)
    // Drawing an arc of three points.
    {
      if (ia < 0 || ia >= nArcs) return -1;
      int x1, x2, y1, y2;
      int iv1 = elArc[ia].Start;
      int iv2 = elArc[ia].Start + elArc[ia].nPoints - 1;
      Graphics g = fm1.g2Bmp; 
      Pen redPen = new Pen(Color.Red, 2.0f);
      x1 = Vert[iv1].X; // fm1.marginX + (int)(fm1.Scale1 * Vert[iv1].X);
      y1 = Vert[iv1].Y; //fm1.marginY + (int)(fm1.Scale1 * Vert[iv1].Y);
      for (int iv = iv1 + 1; iv <= iv2; iv++)
      {
        x2 = Vert[iv].X; //fm1.marginX + (int)(fm1.Scale1 * Vert[iv].X);
        y2 = Vert[iv].Y; //fm1.marginY + (int)(fm1.Scale1 * Vert[iv].Y);
        g.DrawLine(redPen, x1, y1, x2, y2);
        x1 = x2;
        y1 = y2;
      }
      fm1.pictureBox2.Image = fm1.BmpPictBox2;
      return 1;
    }



    public int QualityOfEllipse(int ia, Ellipse Ellipse, int[] SortArcs, Form1 fm1)
    // Returns the sum of the number of points in arcs near the ellipse.
    {
      bool Disp = false;
      int Dif, goodDartIa, locDart, i, iv, ivm, ive, ja, Sum = 0, x, y, xm, ym, xe, ye;
      double angleDart, a = Ellipse.a, b = Ellipse.b, c = Ellipse.c, d = Ellipse.d, r, rc, r1, r2;
      double maxPoints = elArc[ia].nPoints * 6.28 / elArc[ia].Angle;
      r = elArc[ia].R;

      r1 = 0.64 * r * r;
      if (Ellipse.a > r || Ellipse.b > r)
      {
        r2 = 1.7 * r * r;
        rc = 1.2 * r;
      }
      else
      {
        r2 = 1.3 * r * r;
        rc = r;
      }
      int ivStart, ivMid, ivEnd, xMain, yMain;
      ivStart = elArc[ia].Start;
      ivMid = ivStart + elArc[ia].nPoints / 2;
      ivEnd = ivStart + elArc[ia].nPoints - 1;
      x = Vert[ivStart].X;
      y = Vert[ivStart].Y;
      double AngleStart = Math.Atan2(y - d, x - c);
      xe = Vert[ivEnd].X;
      ye = Vert[ivEnd].Y;
      double AngleEnd = Math.Atan2(ye - d, xe - c);
      xMain = Vert[ivMid].X;
      yMain = Vert[ivMid].Y;
      double AngleMid = Math.Atan2(yMain - d, xMain - c);
      double minAngle = Math.Min(AngleStart, AngleEnd);
      double maxAngle = Math.Max(AngleStart, AngleEnd), help;
      bool Plus2PI = false;
      if (minAngle < 0.0 && maxAngle > 0.0 && !(AngleMid >= minAngle && AngleMid < maxAngle))
      {
        Plus2PI = true;
        help = maxAngle;
        maxAngle = minAngle + 2 * Math.PI;
        minAngle = help;
      }
      angleDart = 57.3 * Math.Atan2(yMain - elArc[ia].My, xMain - elArc[ia].Mx) + 15.0;
      if (angleDart < 0.0) angleDart += 360.0;
      goodDartIa = 6 + (int)angleDart / 30;
      if (goodDartIa > 11) goodDartIa -= 12;
      double AngleJa, Fx, Fxe;
      for (i = 0; i < nArcs; i++) //============================================================
      {
        ja = SortArcs[i];
        if (elArc[ja].nPoints < 5) continue;
        iv = elArc[ja].Start;
        ivm = iv + elArc[ja].nPoints / 2;
        ive = iv + elArc[ja].nPoints - 1;
        x = Vert[iv].X;
        y = Vert[iv].Y;
        xm = Vert[ivm].X;
        ym = Vert[ivm].Y;
        xe = Vert[ive].X;
        ye = Vert[ive].Y;
        Fx = (x - c) * (x - c) / a / a + (y - d) * (y - d) / b / b;
        Fxe = (xe - c) * (xe - c) / a / a + (ye - d) * (ye - d) / b / b;

        if (Fx < 0.6 || Fx > 1.67 || Fxe < 0.6 || Fxe > 1.67) continue;

        angleDart = 57.3 * Math.Atan2((ym - d) * a * a, (xm - c) * b * b);
        if (angleDart < 0.0) angleDart += 360.0;
        locDart = (int)angleDart / 30;
        if (locDart > 11) locDart -= 12;
        Dif = Math.Abs(elArc[ja].Dart - locDart);
        if (Dif > 6) Dif = 12 - Dif;

        if (Disp) DrawOneLongArc(ja, fm1);
        if (Dif < 2)
        {
          if (Disp) DrawOneLongArc(ja, fm1);
 
          for (iv = elArc[ja].Start; iv < elArc[ja].Start + elArc[ja].nPoints; iv++)
          {
            x = Vert[iv].X;
            y = Vert[iv].Y;
            AngleJa = Math.Atan2(y - d, x - c);
            if (AngleJa < 0.0 && Plus2PI) AngleJa += 6.28;
            if (!(AngleJa > minAngle && AngleJa < maxAngle)) Sum += elArc[ja].nPoints;
          }
        }
      } //================================= end for (i = 0; ... ===========================
      return Sum;
    }


    public int MakeDarts(Form1 fm1)
    {
      double angleDart;
      int ia, Dart, ivm, xm, ym;
      fm1.progressBar1.Visible = true;
      int jump, Len = nArcs;
      if (Len > 300) jump = Len / (100 / 6);
      else jump = 2;

      for (ia = 0; ia < nArcs; ia++)
      {
        if ((ia % jump) == jump - 1) fm1.progressBar1.PerformStep();
        if (elArc[ia].nPoints < 3) continue;
        ivm = elArc[ia].Start + elArc[ia].nPoints / 2;
        xm = Vert[ivm].X;
        ym = Vert[ivm].Y;
        angleDart = 57.3 * Math.Atan2(ym - elArc[ia].My, xm - elArc[ia].Mx) + 15.0;
        if (angleDart < 0.0) angleDart += 360.0;
        Dart = (int)angleDart / 30;
        if (Dart > 11) Dart -= 12;
        elArc[ia].Dart = Dart;
      }
      return 1;
    } //*********************** end MakeDarts *********************************************

    public int DrawLineSmart(int[] x, int[] y, Form1 fm1)
    {
      Pen whitePen = new Pen(Color.White);
      int x1 = fm1.marginX + (int)(fm1.Scale1 * x[0]);
      int y1 = fm1.marginY + (int)(fm1.Scale1 * y[0]);
      int x2 = fm1.marginX + (int)(fm1.Scale1 * x[1]);
      int y2 = fm1.marginY + (int)(fm1.Scale1 * y[1]);
      fm1.g2Bmp.DrawLine(whitePen, x1, y1, x2, y2);
      fm1.pictureBox2.Image = fm1.BmpPictBox2;
      return 1;
    }

    public double DrawTangent(int ia, ref Point MP, bool disp, Form1 fm1)
    {
      double dyOverDx, Inf = 1000.0;
      int iv, Len = 10;
      int[] x = new int[3]; 
      int[] y = new int[3];
      iv = elArc[ia].Start + elArc[ia].nPoints / 2;
      if (Vert[iv + 1].X - Vert[iv - 1].X == 0) dyOverDx = Inf;
      else dyOverDx = (Vert[iv + 1].Y - Vert[iv - 1].Y) / (Vert[iv + 1].X - Vert[iv - 1].X);
      MP.X = x[2] = (Vert[iv + 1].X + Vert[iv - 1].X) / 2;
      MP.Y = y[2] = (Vert[iv + 1].Y + Vert[iv - 1].Y) / 2;

      if (disp)
      {
        x[0] = x[2] - Len * (Vert[iv - 1].X - x[2]);
        y[0] = y[2] - Len * (Vert[iv - 1].Y - y[2]);
        x[1] = x[2] + Len * (Vert[iv - 1].X - x[2]);
        y[1] = y[2] + Len * (Vert[iv - 1].Y - y[2]);
        DrawLineSmart(x, y, fm1);
      }
      return (double)(Vert[iv - 1].Y - y[2]) / (double)(Vert[iv - 1].X - x[2]);
    }


    public Point PointWithTangent(Ellipse Ellipse, double K, int Dart, Form1 fm1)
    // Returns the point in the ellipse with tangent 'K' and orientation 'Dart'.
    {
      if (K > 100.0) K = 100.0;
      double a = Ellipse.a, b = Ellipse.b, c = Ellipse.c, d = Ellipse.d, f = Ellipse.f, xold, x, yold, y;
      int xDraw, yDraw;
      double angleDart = 0.0, Kc = 0.0, KcOld = 0.0, Step = 0.02; // it wass 0.02
      Graphics g = fm1.g2Bmp; //pictureBox2.CreateGraphics();
      Pen pen = new Pen(Color.White);
      SolidBrush brush = new SolidBrush(Color.Green);
      Rectangle rect;
      xold = (double)fm1.marginX + (fm1.Scale1 * (Ellipse.c + Ellipse.a * Math.Cos(Ellipse.f)));
      yold = (double)fm1.marginY + (fm1.Scale1 * (Ellipse.d - Ellipse.a * Math.Sin(Ellipse.f)));
      Point P = new Point(0, 0);
      int cnt = 0, DartC = 0;
      for (double t = 0; t < 2 * Math.PI; t += Step)
      {
        x = Ellipse.c + Ellipse.a * Math.Cos(t) * Math.Cos(Ellipse.f) + Ellipse.b * Math.Sin(t) * Math.Sin(Ellipse.f);
        xDraw = fm1.marginX + (int)(fm1.Scale1 * x); //(Ellipse.c + Ellipse.a * Math.Cos(t) * Math.Cos(Ellipse.f) + 

        y = Ellipse.d + Ellipse.b * Math.Sin(t) * Math.Cos(Ellipse.f) - Ellipse.a * Math.Cos(t) * Math.Sin(Ellipse.f);
        yDraw = fm1.marginY + (int)(fm1.Scale1 * y); //(Ellipse.d + Ellipse.b * Math.Sin(t) * Math.Cos(Ellipse.f) - 
        if (x == xold) continue;
        Kc = (y - yold) / (x - xold);
        angleDart = 57.3 * Math.Atan2(y - yold, x - xold) - 75.0;
        if (angleDart < 0.0) angleDart += 360.0;
        DartC = (int)angleDart / 30;
        if (DartC > 11) DartC -= 12;
        cnt++;
        if (t > 0 && KcOld < K && K <= Kc && Math.Abs(DartC - Dart) < 3)
        {
          rect = new Rectangle(xDraw, yDraw, 5, 5);
          g.FillRectangle(brush, rect);
          fm1.pictureBox2.Image = fm1.BmpPictBox2;
          P.X = (int)x;
          P.Y = (int)y;
          return P;
        }
        xold = x; yold = y; KcOld = Kc;
      } //================================== end for (double t = 0;  ==============================
      fm1.pictureBox2.Image = fm1.BmpPictBox2;
      return P;
    }


    public int QualityOfEllipseNew(int ia, Ellipse Ellipse, int[] SortArcs, Form1 fm1)
    // Returns the sum of the numbers of points of arcs near the ellipse.
    {
      bool Disp = false; //true; // 
      int Dif, goodDartIa, locDart, i, iv, ivm, ive, ja, Sum = 0, x, y, xm, ym, xe, ye;
      double angleDart, a = Ellipse.a, b = Ellipse.b, c = Ellipse.c, d = Ellipse.d; //, r, rc, r1, r2;
      double maxPoints = elArc[ia].nPoints * 6.28 / elArc[ia].Angle;
      int ivStart, ivMid, ivEnd, xMain, yMain;
      ivStart = elArc[ia].Start;
      ivMid = ivStart + elArc[ia].nPoints / 2;
      ivEnd = ivStart + elArc[ia].nPoints - 1;
      x = Vert[ivStart].X;
      y = Vert[ivStart].Y;
      double AngleStart = Math.Atan2(y - d, x - c);
      xe = Vert[ivEnd].X;
      ye = Vert[ivEnd].Y;
      double AngleEnd = Math.Atan2(ye - d, xe - c);
      xMain = Vert[ivMid].X;
      yMain = Vert[ivMid].Y;
      double AngleMid = Math.Atan2(yMain - d, xMain - c);
      double minAngle = Math.Min(AngleStart, AngleEnd);
      double maxAngle = Math.Max(AngleStart, AngleEnd), help;
      bool Plus2PI = false;
      if (minAngle < 0.0 && maxAngle > 0.0 && !(AngleMid >= minAngle && AngleMid < maxAngle))
      {
        Plus2PI = true;
        help = maxAngle;
        maxAngle = minAngle + 2 * Math.PI;
        minAngle = help;
      }
      angleDart = 57.3 * Math.Atan2(yMain - elArc[ia].My, xMain - elArc[ia].Mx) + 15.0;
      if (angleDart < 0.0) angleDart += 360.0;
      goodDartIa = 6 + (int)angleDart / 30;
      if (goodDartIa > 11) goodDartIa -= 12;
      double AngleJa, Fx, Fxe, Fxm;
      for (i = 0; i < nArcs; i++) //============================================================
      {
        ja = SortArcs[i];
        if (ja == ia || elArc[ja].nPoints < 5) continue;
        iv = elArc[ja].Start;
        ivm = iv + elArc[ja].nPoints / 2;
        ive = iv + elArc[ja].nPoints - 1;
        x = Vert[iv].X;
        y = Vert[iv].Y;

        xm = Vert[ivm].X;
        ym = Vert[ivm].Y;

        xe = Vert[ive].X;
        ye = Vert[ive].Y;
        Fx = (x - c) * (x - c) / a / a + (y - d) * (y - d) / b / b;
        Fxm = (xm - c) * (xm - c) / a / a + (ym - d) * (ym - d) / b / b;
        Fxe = (xe - c) * (xe - c) / a / a + (ye - d) * (ye - d) / b / b;
        if (Fx < 0.7 || Fx > 1.43 || Fxm < 0.7 || Fxm > 1.43 || Fxe < 0.7 || Fxe > 1.43) continue;

        angleDart = 57.3 * Math.Atan2((ym - d) * a * a, (xm - c) * b * b);
        if (angleDart < 0.0) angleDart += 360.0;
        locDart = (int)angleDart / 30;
        if (locDart > 11) locDart -= 12;
        Dif = Math.Abs(elArc[ja].Dart - locDart);
        if (Dif > 6) Dif = 12 - Dif;

        if (Disp) DrawOneLongArc(ja, fm1);
        if (Dif < 2)
        {
          if (Disp) DrawOneLongArc(ja, fm1);

          for (iv = elArc[ja].Start; iv < elArc[ja].Start + elArc[ja].nPoints; iv++)
          {
            x = Vert[iv].X;
            y = Vert[iv].Y;
            AngleJa = Math.Atan2(y - d, x - c);
            if (AngleJa < 0.0 && Plus2PI) AngleJa += 6.28;
            if (!(AngleJa > minAngle && AngleJa < maxAngle)) Sum += elArc[ja].nPoints;
          }
        }
      } //================================= end for (i = 0; ... ===========================
      return Sum;
    }


    public int HelpArcNew(int ia, int[] SortArcs, ref Ellipse Ellipse, int SumStart, Form1 fm1)
    {
      bool disp = false;
      if (disp)  DrawOneLongArc(ia, fm1);
      int Dif, i, ivMid, ivm, ja, xMain, yMain, xm, ym;
      ivMid = elArc[ia].Start + elArc[ia].nPoints / 2;
      xMain = Vert[ivMid].X;
      yMain = Vert[ivMid].Y;
      double angleDart, a = Ellipse.a, b = Ellipse.b, c = Ellipse.c, d = Ellipse.d;
      int goodDartIa;
      angleDart = 57.3 * Math.Atan2(yMain - elArc[ia].My, xMain - elArc[ia].Mx) + 15.0;
      if (angleDart < 0.0) angleDart += 360.0;
      goodDartIa = 6 + (int)angleDart / 30;
      if (goodDartIa > 11) goodDartIa -= 12;

      double R = elArc[ia].R, Mx = elArc[ia].Mx, My = elArc[ia].My;
      if (disp) DrawCircle(elArc[ia].R, elArc[ia].Mx, elArc[ia].My, fm1);
      CBox[] Box = new CBox[2];
      Box[0] = new CBox();
      Box[1] = new CBox();

      Box[1].minX = (int)(Mx - R) - 10;
      if (c - a < Box[1].minX) Box[1].minX -= 20;

      Box[1].maxX = (int)(Mx + R ) + 10;
      if (c + a > Box[1].maxX) Box[1].maxX += 20;

      Box[1].minY = (int)(My - R) - 10;
      if (d - b < Box[1].minY) Box[1].minY -= 20;

      Box[1].maxY = (int)(My + R) + 10;
      if (d + b > Box[1].maxY) Box[1].maxY += 20;


      Box[0].minX = (int)(c - a - 0.3*a);
      if (c - a < Box[0].minX) Box[0].minX -= 20;

      Box[0].maxX = (int)(c + a + 0.3 * a);
      if (c + a > Box[0].maxX) Box[0].maxX += 20;

      Box[0].minY = (int)(d - b - 0.18*b);
      if (d - b < Box[0].minY) Box[0].minY -= 20;

      Box[0].maxY = (int)(d + b + 0.18 * b);
      if (d + b > Box[0].maxY) Box[0].maxY += 20;

      int[] jBest = new int[100];
      int nBest = 0;
      for (int v = 0; v < 2; v++)
      {
        if (disp) DrawRectangleSmart(Box[v], fm1);
        int Dist2 = 0, minDist2 = (int)(1.5 * elArc[ia].R * elArc[ia].R);
        for (i = 0; i < nArcs; i++) //=================================================================
        {
          ja = SortArcs[i];
          if (!ArcInBox(ja, Box[v])) continue;
          if (elArc[ja].nPoints < 3) continue;
          if (disp) DrawOneLongArc(ja, fm1);
          Dif = Math.Abs(elArc[ja].Dart - goodDartIa);
          if (Dif > 6) Dif = 12 - Dif;
          if (Dif > 3) continue;
          ivm = elArc[ja].Start + elArc[ja].nPoints / 2;
          xm = Vert[ivm].X;
          ym = Vert[ivm].Y;
          Dist2 = (xm - xMain) * (xm - xMain) + (ym - yMain) * (ym - yMain);
          if (Dist2 > minDist2)
          {
            jBest[nBest] = ja;
            nBest++;
          }
          if (nBest >= 5) break;
        } //==================================== end for (i = 0; =============================
      } //====================================== end for (int v = 0; =============================
      double Delta = 0.0;
      Ellipse Ellipse1 = new Ellipse();
      int jbestOpt = -1, maxSum = SumStart, Sum = 0;
      for (i = 0; i < nBest; i++) //===========================================================================
      {
        if (disp) DrawRedArc(jBest[i], fm1);
        GetEllipseNew(Vert, elArc[ia].Start, elArc[ia].nPoints, elArc[jBest[i]].Start, elArc[jBest[i]].nPoints, ref Delta, ref Ellipse1.f,
          ref Ellipse1.a, ref Ellipse1.b, ref Ellipse1.c, ref Ellipse1.d);
        Sum = QualityOfEllipseNew(ia, Ellipse1, SortArcs, fm1);
        if (disp) DrawEllipse(Ellipse1, fm1);
         if (!(Ellipse1.a > 5.0 && Ellipse1.b > 5.0) || Ellipse1.d - Ellipse1.b < fm1.height * 2 / 5) Sum = 0;
        else
        {
          if (Sum > maxSum && Math.Abs(Ellipse1.a - Ellipse.a) < 0.9 * Math.Max(Ellipse1.a, Ellipse.a) &&  
              (Math.Abs(Ellipse1.b - Ellipse.b) < 0.5 * Ellipse.b || elArc[ia].nPoints >= 8))
          {
            if (disp) DrawEllipse(Ellipse1, fm1);
            if (disp) DrawRedArc(jBest[i], fm1);

            maxSum = Sum;
            jbestOpt = jBest[i];
            Ellipse = Ellipse1;
          }
        }
      } //=============================== end for (i ... i < nBest; =======================================
      if (disp) DrawRedArc(jbestOpt, fm1);
      Pen pen = new Pen(Color.Red);
      return jbestOpt;
    } //********************************* end HelpArcNew ******************************************************


    public bool CminusCel(Ellipse Ellipse1, Ellipse Ellipse2, Form1 fm1)
    {
      double c1 = Ellipse1.c, c2 = Ellipse2.c, a1 = Ellipse1.a, a2 = Ellipse2.a, b1 = Ellipse1.b, b2 = Ellipse2.b;
      double d1 = Ellipse1.d, d2 = Ellipse2.d;
      bool B0 = Math.Abs(c1 - c2) > 0.9 * (a1 + a2);
      bool B1 = Math.Abs(c1 - c2) < 2.5 * (a1 + a2);
      bool B2 = Math.Abs(d1 - d2) < 0.45 * (b1 + b2);
      bool B3 = d1 - b1 > 0.0;
      bool B4 = d2 - b2 > 0.0;
      bool B5 = a1 / a2 > 0.625;
      bool B6 = a1 / a2 < 1.6;
      bool B7 = b1 / b2 > 0.625;
      bool B8 = b1 / b2 < 1.6;
      if (Math.Abs(c1 - c2) > 0.9 * (a1 + a2) && Math.Abs(c1 - c2) < 2.5 * (a1 + a2) && Math.Abs(d1 - d2) < 0.45 * (b1 + b2) &&
        d1 - b1 > 0.0 && d2 - b2 > 0.0 && a1 / a2 > 0.625 && a1 / a2 < 1.6 &&
         b1 / b2 > 0.625 && b1 / b2 < 1.6) return true;
      return false;
    }


    public bool Position(int ia1, Ellipse Ellipse0, Form1 fm1)
    // Checks the position of the arc "ia1" relative to the Ellipse.
    {
      double c0 = Ellipse0.c, d0 = Ellipse0.d, a0 = Ellipse0.a, b0 = Ellipse0.b;
      int iv1, iv2, iv3, x1, y1, x2, y2, x3, y3;
      if (Math.Abs(elArc[ia1].R) < 0.5 * a0 * a0 / b0 || Math.Abs(elArc[ia1].R) > 2 * b0 * b0 / a0) return false;
      iv1 = elArc[ia1].Start;
      x1 = Vert[iv1].X;
      y1 = Vert[iv1].Y;
      iv2 = elArc[ia1].Start + elArc[ia1].nPoints / 2;
      x2 = Vert[iv2].X;
      y2 = Vert[iv2].Y;
      iv3 = elArc[ia1].Start + elArc[ia1].nPoints - 1;
      x3 = Vert[iv3].X;
      y3 = Vert[iv3].Y;

      if (Math.Abs(c0 - x1) > 2.5 * a0 && Math.Abs(c0 - x1) < 5.5 * a0 && Math.Abs(c0 - x2) > 2.5 * a0 &&
        Math.Abs(c0 - x2) < 5.5 * a0 && Math.Abs(c0 - x3) > 2.5 * a0 && Math.Abs(c0 - x3) < 5.5 * a0 &&
        y1 > d0 - 1.5 * b0 && y1 < d0 + 2 * b0 && y2 > d0 - 1.5 * b0 && y2 < d0 + 2 * b0)
      {
        return true;
      }
      return false;
    }

    public double GoodArc(int ia)
    {
      if (elArc[ia].nPoints < 3) return 0.0;
      int nP = elArc[ia].nPoints, x, y, x1, y1, x2, y2;
      x = Vert[elArc[ia].Start].X;
      y = Vert[elArc[ia].Start].Y;

      x1 = Vert[elArc[ia].Start + nP / 2].X;
      y1 = Vert[elArc[ia].Start + nP / 2].Y;

      x2 = Vert[elArc[ia].Start + nP -1].X;
      y2 = Vert[elArc[ia].Start + nP - 1].Y;
      int VP = (x1 - x) * (y2 - y1) - (x2 - x1) * (y1 - y);
      double Sin = (double)VP / Math.Sqrt((double)((x1 - x) * (x1 - x) + (y1 - y) * (y1 - y))) /
                                Math.Sqrt((double)((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)));
      return Math.Abs(Sin);
    }

 
    public void DrawPolygons(PictureBox pictureBox, CImage EdgeIm, bool DrawRect, bool Inscript, Form1 fm1)
    {
      Graphics g = fm1.g2Bmp; // pictureBox.CreateGraphics();
      Brush blackBrush, smoothBrush, redBrush;
      blackBrush = new SolidBrush(Color.Black);
      redBrush = new SolidBrush(Color.Red);
      smoothBrush = new SolidBrush(Color.Blue);

      Pen penSmooth, penNonSmooth; //, PenDelete;
      penSmooth = new System.Drawing.Pen(Color.Green, 1);
      penNonSmooth = new System.Drawing.Pen(Color.Green, 1);

      double ScaleX = (double)pictureBox.Width / EdgeIm.width;
      double ScaleY = (double)pictureBox.Height / EdgeIm.height;
      double Scale;
      if (ScaleX < ScaleY) Scale = ScaleX;
      else Scale = ScaleY;
      double marginX = (pictureBox.Width - Scale * EdgeIm.width) * 0.5;
      double marginY = (pictureBox.Height - Scale * EdgeIm.height) * 0.5;

      Rectangle rect0, rect1, rect2; // = new Rectangle
      rect0 = new Rectangle(0, 0, 600, 600);
      g.FillRectangle(blackBrush, rect0);
      int ix1, iy1;
      for (int il = 0; il < nPolygon; il++) //=========================
      {
        int first = Polygon[il].firstVert;
        int last = Polygon[il].lastVert;
        bool smooth = Polygon[il].smooth;

        ix1 = fm1.marginX + (int)(fm1.Scale1 * Vert[first].X);
        iy1 = fm1.marginY + (int)(fm1.Scale1 * Vert[first].Y); // "marginY" in "CInscript" deleted
        string s = il.ToString();

        for (int iv = first; iv < Polygon[il].lastVert; iv++) //===
        {
          if (Polygon[il].lastVert - first + 1 < 5) continue;
          float x1 = (float)(Vert[iv].X * Scale + marginX);
          float y1 = (float)(Vert[iv].Y * Scale + marginY);
          float x2 = (float)(Vert[iv + 1].X * Scale + marginX);
          float y2 = (float)(Vert[iv + 1].Y * Scale + marginY);
          rect1 = new Rectangle((int)x1 - 1, (int)y1 - 1, 2, 2);
          rect2 = new Rectangle((int)x2 - 1, (int)y2 - 1, 2, 2);
          if (smooth)
          {
            g.DrawLine(penSmooth, x1, y1, x2, y2);
            if (DrawRect) g.FillRectangle(redBrush, rect1);
          }
          else
          {
            g.DrawLine(penNonSmooth, x1, y1, x2, y2);
            if (DrawRect) g.FillRectangle(redBrush, rect2);
          }
        } //===================== end for (int iv ... ===============
      } //======================= end for (int il ... =================

      // Inscript:
      for (int il = 0; il < nPolygon; il++) //=========================
      {
        int first = Polygon[il].firstVert;
        int last = Polygon[il].lastVert;
        bool smooth = Polygon[il].smooth;
        Inscript = il == 466; // && il < 500;

        ix1 = fm1.marginX + (int)(fm1.Scale1 * Vert[first].X);
        iy1 = fm1.marginY + (int)(fm1.Scale1 * Vert[first].Y);
        string s = il.ToString();

      }

      fm1.pictureBox2.Image = fm1.BmpPictBox2;
    } //************************* end DrawPolygons *************************



    public double SumLengthes(int iv, CBox Box)
    {
      int x, y, x1, y1;
      double Len;
      x = Vert[iv].X;
      y = Vert[iv].Y;
      if (x < Box.minX || x > Box.maxX || y < Box.minY || y > Box.maxY) Len = 0.0;
      else
      {
        x1 = Vert[iv + 1].X;
        y1 = Vert[iv + 1].Y;
        if (2 * Math.Abs(y - y1) < Math.Abs(x - x1))
          Len = Math.Sqrt((x - x1) * (x - x1) + (y - y1) * (y - y1));
        else Len = 0.0;
      }
      return Len;
    }


    public int DarkSpots(CImage CombIm, Ellipse EllipseL, Ellipse EllipseR, Form1 fm1)
    // Combined wth "SegmentsInWheel".
    {
      bool disp = false;

      double aL = EllipseL.a, bL = EllipseL.b, cL = EllipseL.c, dL = EllipseL.d;
      double aR = EllipseR.a, bR = EllipseR.b, cR = EllipseR.c, dR = EllipseR.d;
      if (disp)
      {
        DrawPolygons(fm1.pictureBox2, fm1.EdgeIm, false, false, fm1);
        DrawEllipse(EllipseL, fm1);
        DrawEllipse(EllipseR, fm1);

        // Centers of ellipses:
        Rectangle rect1, rect2;
        rect1 = new Rectangle(fm1.marginX + (int)(fm1.Scale1 * cL), fm1.marginY + (int)(fm1.Scale1 * dL), 3, 3);
        rect2 = new Rectangle(fm1.marginX + (int)(fm1.Scale1 * cR), fm1.marginY + (int)(fm1.Scale1 * dR), 3, 3);
        SolidBrush brush = new SolidBrush(Color.White);
        fm1.g2Bmp.FillRectangle(brush, rect1);
        fm1.g2Bmp.FillRectangle(brush, rect2);
      }

      CBox BoxLeft = new CBox();
      CBox BoxRight = new CBox();
      BoxLeft.minX = (int)(cL - 0.1 * aL);
      BoxLeft.maxX = (int)(cL + 0.9 * aL);
      BoxLeft.minY = (int)(dL - 0.20 * bL);
      BoxLeft.maxY = (int)(dL + 0.40 * bL);

      BoxRight.minX = (int)(cR - 0.9 * aR);
      BoxRight.maxX = (int)(cR + 0.1 * aR);
      BoxRight.minY = (int)(dR - 0.20 * bR);
      BoxRight.maxY = (int)(dR + 0.40 * bR);

      Pen whitePen = new Pen(Color.White);
      int x1, x2, y1, y2;
      x1 = (int)(fm1.Scale1 * BoxLeft.minX);
      x2 = (int)(fm1.Scale1 * BoxLeft.maxX);
      y1 = (int)(fm1.Scale1 * BoxLeft.minY);
      y2 = (int)(fm1.Scale1 * BoxLeft.maxY);
      if (disp)
      {
        Rectangle rectL = new Rectangle(fm1.marginX + x1, fm1.marginY + y1, x2 - x1, y2 - y1);
        fm1.g2Bmp.DrawRectangle(whitePen, rectL);
      }

      x1 = (int)(fm1.Scale1 * BoxRight.minX);
      x2 = (int)(fm1.Scale1 * BoxRight.maxX);
      y1 = (int)(fm1.Scale1 * BoxRight.minY);
      y2 = (int)(fm1.Scale1 * BoxRight.maxY);
      if (disp)
      {
        Rectangle rectR = new Rectangle(fm1.marginX + x1, fm1.marginY + y1, x2 - x1, y2 - y1);
        fm1.g2Bmp.DrawRectangle(whitePen, rectR);
      }
      int[] HistoLeft = new int[256];
      int[] HistoRight = new int[256];
      int gv, i, NX = CombIm.width, x, y, X, Y;

      for (i = 0; i < 256; i++) HistoLeft[i] = HistoRight[i] = 0;

      for (y = BoxLeft.minY; y <= BoxLeft.maxY; y++)
      {
        Y = 2 * y + 1;
        for (x = BoxLeft.minX; x <= BoxLeft.maxX; x++)
        {
          X = 2 * x + 1;
          gv = CombIm.Grid[X + NX * Y];
          HistoLeft[gv]++;
        }
      }

      for (y = BoxRight.minY; y <= BoxRight.maxY; y++)
      {
        Y = 2 * y + 1;
        for (x = BoxRight.minX; x <= BoxRight.maxX; x++)
        {
          X = 2 * x + 1;
          gv = CombIm.Grid[X + NX * Y];
          HistoRight[gv]++;
        }
      }

      int maxHistL = 0, maxHistR = 0, Threshold = 100, SumLeft = 0, SumRight = 0;
      if (disp)
      {
        Rectangle rectHL = new Rectangle(0, 0, 256, 128);
        int x0 = 600 - 256, y0 = 128; ;
        Rectangle rectHR = new Rectangle(x0, 0, 256, 128);
        SolidBrush Brush = new SolidBrush(Color.Black);
        fm1.g2Bmp.FillRectangle(Brush, rectHL);
        fm1.g2Bmp.FillRectangle(Brush, rectHR);
        for (gv = 0; gv < 256; gv++)
        {
          if (HistoLeft[gv] > maxHistL) maxHistL = HistoLeft[gv];
          if (HistoRight[gv] > maxHistR) maxHistR = HistoRight[gv];
        }

        Pen redPen = new Pen(Color.Red);
        for (gv = 0; gv < 256; gv++)
        {
          fm1.g2Bmp.DrawLine(whitePen, gv, y0, gv, y0 - 120 * HistoLeft[gv] / maxHistL);
          fm1.g2Bmp.DrawLine(whitePen, x0 + gv, y0, x0 + gv, y0 - 120 * HistoRight[gv] / maxHistR);
        }
        fm1.g2Bmp.DrawLine(redPen, Threshold, y0, Threshold, 0);
        fm1.g2Bmp.DrawLine(redPen, x0 + Threshold, y0, x0 + Threshold, 0);
        fm1.pictureBox2.Image = fm1.BmpPictBox2;
      }

      for (gv = 0; gv < 256; gv++)
      {
        if (gv < Threshold)
        {
          SumLeft += HistoLeft[gv];
          SumRight += HistoRight[gv];
        }
      }

      double SumL = 0.0, SumR = 0.0;
      int iP, iv, rv = -1;
      for (iP = 0; iP < nPolygon; iP++)
        for (iv = Polygon[iP].firstVert; iv < Polygon[iP].lastVert; iv++)
        {
          SumL += SumLengthes(iv, BoxLeft);
          SumR += SumLengthes(iv, BoxRight);
        }
      if (SumLeft + 6 * (int)SumL < SumRight + 6 * (int)SumR) rv = 2;
      else rv = 0;
      return rv;
    } //***************************** end DarkSpots **********************************************


    public int MakeDrawing(Ellipse Ellipse1, Ellipse Ellipse2, int Dir, Form1 fm1)
    {
      if (Dir < 0) return -1;
      Graphics g = fm1.g2Bmp; //pictureBox2.CreateGraphics();
      DrawEllipse(Ellipse1, fm1);
      DrawEllipse(Ellipse2, fm1);
      fm1.pictureBox2.Refresh();
      double[] ProtX0 = { 0.0, 0.22, 1.10, 1.75, 1.04, 1.17, 2.94, 3.04, 3.16, 3.71, 4.05 };
      double[] ProtY0 = { 0.0, 0.13, 0.17, 0.10, 1.36, 1.06, 1.60, 1.48, 1.32, 0.18, 0.0 };


      double[] ProtX1 = { 0.0, 0.21, 1.33, 1.46, 1.08, 1.13, 3.17, 3.25, 3.33, 3.71, 4.0 };
      double[] ProtY1 = { 0.0, 0.20, 0.20, -0.29, 1.79, 1.08, 2.25, 2.0, 1.54, 0.42, 0.0 };

      int[,] BlockM = { { 3, 4 }, { 6, 9 }, { 0, 5 }, { 5, 7 }, { 3, 8 }, { 1, 2 }, { 0, 3 }, { 9, 10 } };
      int[,] BlockF = { { 3, 4 }, { 6, 9 }, { 0, 5 }, { 2, 7 }, { 3, 8 }, { 1, 2 }, { 0, 3 }, { 9, 10 } };
      double[] A = new double[8];
      double[] B = new double[8];

      double[] PointX = new double[11];
      double[] PointY = new double[11];


      double MultiplX = 0.0, MultiplY = 0.0;
      int margX = fm1.marginX;
      int margY = fm1.marginY;
      double Scale1 = fm1.Scale1;
      Pen whitePen;
      int penWidth = fm1.width / 180;
      whitePen = new Pen(Color.White, penWidth);
      int xLeft, yLeft, yDif, xRight, yRight;
      double yDenom = ProtX1[10] - ProtX1[1];
      fm1.progressBar1.Visible = true;
      fm1.progressBar1.Step = 1;
      int jump, Len = 24 * nPolygon;
      jump = Len / (100 / 6);
      if (Ellipse1.c < Ellipse2.c)
      {
        xLeft = (int)Ellipse1.c;
        yLeft = (int)Ellipse1.d;
        xRight = (int)Ellipse2.c;
        yRight = (int)Ellipse2.d;
        yDif = (int)(Ellipse2.d - Ellipse1.d);
      }
      else
      {
        xLeft = (int)Ellipse2.c;
        yLeft = (int)Ellipse2.d;
        xRight = (int)Ellipse1.c;
        yRight = (int)Ellipse1.d;
        yDif = (int)(Ellipse1.d - Ellipse2.d);
      }
      MultiplX = (double)(xRight - xLeft) / (ProtX1[10] - ProtX1[0]);
      MultiplY = MultiplX * Ellipse1.b / Ellipse1.a;
      for (int k = 0; k < 11; k++) //======================================================
      {
        fm1.progressBar1.PerformStep();

        if (Dir == 0)
        {
          PointX[k] = xLeft + ProtX1[k] * MultiplX;
          PointY[k] = yLeft + yDif * ProtX1[k] / yDenom - ProtY1[k] * MultiplY;
        }
        else
        {
          PointX[k] = xRight - ProtX1[k] * MultiplX;
          PointY[k] = yRight - yDif * ProtX1[k] / yDenom - ProtY1[k] * MultiplY;
        }
      }
      double Root1;
      int halfWidth = (xRight - xLeft) / 50;
      int m = 0;

      for (m = 0; m < 8; m++)
      {
        A[m] = PointY[BlockM[m, 0]] - PointY[BlockM[m, 1]];
        B[m] = PointX[BlockM[m, 1]] - PointX[BlockM[m, 0]];

        Root1 = Math.Sqrt(A[m] * A[m] + B[m] * B[m]);
        A[m] /= Root1;
        B[m] /= Root1;
      } //=============================== end for (m = 0; ... ================================================

      DrawBike(g, whitePen, halfWidth, BlockM, PointX, PointY, fm1);

      fm1.pictureBox2.Image = fm1.BmpPictBox2;
      if (Dir == 0)
        MessageBox.Show(" Bicycle going to right is recognised.");
      else
        MessageBox.Show(" Bicycle going to left is recognised.");

      fm1.progressBar1.Step = fm1.progressBar1.Maximum - fm1.progressBar1.Value;
      fm1.progressBar1.PerformStep();
      return 1;
    } //************************************** end MakeDrawing ****************************************************


    public int FindEllipsesMode(CImage SigmaIm, Ellipse[] ListEllipse, ref int nEllipse, Form1 fm1)
    {
      bool disp = false;
      int[] SortArcs = new int[nArcs];
      int maxNP = 0, k = SortingArcs(SortArcs, ref maxNP);
 
      int i, ia, ia1, i0, i1; 
      nEllipse = 0;
      double a = 0.0, b = 0.0, c = 0.0, d = 0.0; //, fret = 0.0;
      int[,] List = new int[20, 1200];
      int[] nArcList = new int[20];
      SCircle[] Circle = new SCircle[20];
      for (i = 0; i < 20; i++)
      {
        Circle[i] = new SCircle();
        Circle[i].goodCirc = true;
      }
      Ellipse[] smalList = new Ellipse[20];
      for (i = 0; i < 20; i++) smalList[i] = new Ellipse();

      int Sum1 = 0;
      double AnglePerPoint = 0.0, maxPoints = 0.0;

      fm1.progressBar1.Visible = true;
      fm1.progressBar1.Step = 1;
      int jump, Len = nArcs, nStep = 20;
      if (Len > 2 * nStep) jump = Len / nStep;
      else
        jump = 2;
      double Delta = 0.0, f = 0.0, F = 0.0;

      Ellipse Ellipse1 = new Ellipse();
      Ellipse Ellipse2 = new Ellipse();
      int[] Pattern = new int[100000];
      for (i0 = 0; i0 < nArcs; i0++)  //===================================================
      {
        if ((i0 % jump) == jump - 1) fm1.progressBar1.PerformStep();

        ia = SortArcs[i0];
        if (disp)
          DrawRedArc(ia, fm1);

        if (elArc[ia].nPoints <= 5) break;

        GetEllipseNew(Vert, elArc[ia].Start, elArc[ia].nPoints, 0, 0, ref Delta, ref f,
          ref a, ref b, ref c, ref d);
        if ((d - b) < 0.3 * fm1.height || b < 0.028*fm1.height) continue;
        if (disp)
          DrawEllipse(f, a, b, c, d, fm1);

        if (elArc[ia].nPoints > 10 && d + b > fm1.height) return -1;
        if (b < 20.0 || a < 6.0 || d + b > fm1.height || d - 4 * b < 0.0) continue; 

        int jbestOpt = -1;
        Ellipse1.a = a;
        Ellipse1.b = b;
        Ellipse1.c = c;
        Ellipse1.d = d;
        Point P1 = new Point(0, 0);
        Point MP = new Point(0, 0);

        if (a > 5.0 && b > 5.0)
        {
          Sum1 = QualityOfEllipseNew(ia, Ellipse1, SortArcs, fm1);
          AnglePerPoint = elArc[ia].Angle / elArc[ia].nPoints;
          maxPoints = 2 * Math.PI / AnglePerPoint;

          if (b > fm1.height / 4 || elArc[ia].nPoints < 10) Sum1 = 0;
          Pen pen = new Pen(Color.Red);
          if ((elArc[ia].nPoints < 10 || d + b > fm1.height * 2 / 5) && Sum1 < 0.6 * maxPoints)
          {
            jbestOpt = HelpArcNew(ia, SortArcs, ref Ellipse1, Sum1, fm1);

            if (disp)
              DrawEllipse(Ellipse1, fm1);
          }
        }
        for (i1 = i0 + 1; i1 < nArcs; i1++) //======================================================================
        {
          ia1 = SortArcs[i1];
          if (!Position(ia1, Ellipse1, fm1)) continue;
          if (elArc[ia1].nPoints <= 3) // it was 5
          {
            break;
          }

          int iv = elArc[ia].Start, x = Vert[iv].X, y = Vert[iv].Y;
          int iv1 = elArc[ia1].Start, x1 = Vert[iv1].X, y1 = Vert[iv1].Y;
          GetEllipseNew(Vert, elArc[ia1].Start, elArc[ia1].nPoints, 0, 0, ref Delta, ref f,
                                                                    ref a, ref b, ref c, ref d);
          if (!(a > 5.0 && b > 5.0) || Math.Abs(x - x1) < a * 1.5) continue;
          if (disp) DrawEllipse(f, a, b, c, d, fm1);
          Ellipse2.a = a;
          Ellipse2.b = b;
          Ellipse2.c = c;
          Ellipse2.d = d;

          if (disp) DrawRedArc(ia1, fm1);
          double K2 = DrawTangent(ia1, ref MP, disp, fm1);
          if (GoodArc(ia1) < 0.1) continue;

          P1 = PointWithTangent(Ellipse1, K2, elArc[ia1].Dart, fm1);

          if (P1.X == 0 && P1.Y == 0) continue;
          Ellipse2 = Ellipse1;
          Ellipse2.c = MP.X + Ellipse1.c - P1.X;
          Ellipse2.d = MP.Y + Ellipse1.d - P1.Y;
          if (disp) DrawEllipse(Ellipse2, fm1);

          jbestOpt = -1;
          if (Ellipse2.a > 5.0 && Ellipse2.b > 5.0)
          {
            Sum1 = QualityOfEllipseNew(ia1, Ellipse2, SortArcs, fm1);
            AnglePerPoint = elArc[ia1].Angle / elArc[ia1].nPoints;
            maxPoints = 2 * Math.PI / AnglePerPoint;

            Pen pen = new Pen(Color.Red);
            if (!CminusCel(Ellipse1, Ellipse2, fm1))
            {
              jbestOpt = HelpArcNew(ia1, SortArcs, ref Ellipse2, Sum1, fm1);
              if (disp) DrawEllipse(Ellipse2, fm1);
            }
          }

          bool CMINC = CminusCel(Ellipse1, Ellipse2, fm1);

          if (!CMINC) continue;

          ListEllipse[nEllipse] = Ellipse1;
          nEllipse++;
          ListEllipse[nEllipse] = Ellipse2;
          nEllipse++;

          bool Elipse1_is_left = false;
          if (Ellipse1.c < Ellipse2.c) Elipse1_is_left = true;

          int Dir;
          if (Elipse1_is_left)  Dir = DarkSpots(fm1.CombIm, Ellipse1, Ellipse2, fm1);
          else Dir = DarkSpots(fm1.CombIm, Ellipse2, Ellipse1, fm1);
          return Dir;
        } //============================= end for (i1 = 0; ... ==========================================
      } //=============================== end for (i0 = 0; ... ============================================
      MessageBox.Show("FindEllipsesMode: no bike recognized");
      return -1;
    } //********************************* end FindEllipsesMode *********************************************       



    public int MessReturn(string s)
    {
      if (MessageBox.Show(s,
              "Return", MessageBoxButtons.OKCancel) == DialogResult.Cancel) return -1;
      return 1;
    }

   
    public void DrawEllipse(double f, double a, double b, double c, double d, Form1 fm1)
    {
      if (!(a > 5.0 && b > 5.0))
      {
        MessageBox.Show("DrawEllipse: Bad ellipse parameter. Return");
        return;
      }
      int xold, x, yold, y;
      double Step = 0.1;
      Graphics g = fm1.g2Bmp; 
      Pen pen;
      if (fm1.width < 1000) pen = new Pen(Color.White, 1);
      else pen = new Pen(Color.White, 4);
      xold = (int)(c + a * Math.Cos(f)); // fm1.marginX + (int)(fm1.Scale1 * (c + a * Math.Cos(f)));
      yold = (int)(d - a * Math.Sin(f)); // fm1.marginY + (int)(fm1.Scale1 * (d - a * Math.Sin(f)));
      for (double t = 0; t < 2 * Math.PI; t += Step)
      {
        x = (int)(c + a * Math.Cos(t) * Math.Cos(f) + b * Math.Sin(t) * Math.Sin(f));  //fm1.marginX + (int)(fm1.Scale1 * (c + a * Math.Cos(t) * Math.Cos(f) + b * Math.Sin(t) * Math.Sin(f)));
        y = (int)(d + b * Math.Sin(t) * Math.Cos(f) - a * Math.Cos(t) * Math.Sin(f)); //fm1.marginY + (int)(fm1.Scale1 * (d + b * Math.Sin(t) * Math.Cos(f) - a * Math.Cos(t) * Math.Sin(f)));
        g.DrawLine(pen, xold, yold, x, y);
        xold = x; yold = y;
      }
      fm1.pictureBox2.Refresh(); // Image = fm1.BmpPictBox2;
    }



    public void DrawEllipse(Ellipse Ellipse, Form1 fm1)
    {
      if (!(Ellipse.a > 1.0 && Ellipse.b > 1.0)) return;
      int xold, x, yold, y;
      double Step = 0.1;
      Graphics g = fm1.g2Bmp; //pictureBox2.CreateGraphics();
      int penWidth = fm1.width / 180;
      Pen pen = new Pen(Color.White, penWidth);
      xold = (int)(Ellipse.c + Ellipse.a); //fm1.marginX + (int)(fm1.Scale1 * (Ellipse.c + Ellipse.a));
      yold = (int)Ellipse.d; //fm1.marginY + (int)(fm1.Scale1 * Ellipse.d);
      for (double t = 0; t < 2 * Math.PI; t += Step)
      {
        x = (int)(Ellipse.c + Ellipse.a * Math.Cos(t)); //fm1.marginX + (int)(fm1.Scale1 * (Ellipse.c + Ellipse.a * Math.Cos(t)));
        y = (int)(Ellipse.d + Ellipse.b * Math.Sin(t)); //fm1.marginY + (int)(fm1.Scale1 * (Ellipse.d + Ellipse.b * Math.Sin(t)));
        g.DrawLine(pen, xold, yold, x, y);
        xold = x; yold = y;
      }
      fm1.pictureBox2.Refresh(); //Image = fm1.BmpPictBox2;
    }



    public void MakeSums2(Point[] Vert, int iv1, int nPoint1, int iv2, int nPoint2, double[] Sum)
    {
      int nSum = 15;
      for (int j = 0; j < nSum; j++) Sum[j] = 0.0;
      double X = 0.0, Y = 0.0;
      for (int i = iv1; i < iv1 + nPoint1; i++) //===============================
      {
        X = (double)Vert[i].X;
        Y = (double)Vert[i].Y;
        Sum[0]++;
        Sum[1] += Y;
        Sum[2] += X;
        Sum[3] += Y * Y;
        Sum[4] += X * Y;
        Sum[5] += X * X;
        Sum[6] += Y * Y * Y;
        Sum[7] += X * Y * Y;
        Sum[8] += X * X * Y;
        Sum[9] += X * X * X;
        Sum[10] += Y * Y * Y * Y;
        Sum[11] += X * Y * Y * Y;
        Sum[12] += X * X * Y * Y;
        Sum[13] += X * X * X * Y;
        Sum[14] += X * X * X * X;
      } //======================= end for (int i... ==========================

      for (int i = iv2; i < iv2 + nPoint2; i++) //===============================
      {
        X = (double)Vert[i].X;
        Y = (double)Vert[i].Y;
        Sum[0]++;
        Sum[1] += Y;
        Sum[2] += X;
        Sum[3] += Y * Y;
        Sum[4] += X * Y;
        Sum[5] += X * X;
        Sum[6] += Y * Y * Y;
        Sum[7] += X * Y * Y;
        Sum[8] += X * X * Y;
        Sum[9] += X * X * X;
        Sum[10] += Y * Y * Y * Y;
        Sum[11] += X * Y * Y * Y;
        Sum[12] += X * X * Y * Y;
        Sum[13] += X * X * X * Y;
        Sum[14] += X * X * X * X;
      } //======================= end for (int i... ==========================
    } //************************* end MakeSums2 ********************************


    public int GetEllipse2(Point[] Vert, int iv1, int nPoints1, int iv2, int nPoints2, ref double Delta, ref double f,
                                      ref double a, ref double b, ref double c, ref double d, ref double F)
    // Calculates the 15 sums, the 25 A- and 5 B-coefficients and the parameter of the ellipse.
    // Sum[0]=S00; Sum[1]=S01; Sum[2]=S10; Sum[3]=S02; Sum[4]=S11; Sum[5]=S20; Sum[6]=S03; Sum[7]=S12;
    // Sum[8]=S21; Sum[9]=S30; Sum[10]=S04; Sum[11]=S13; Sum[12]=S22; Sum[13]=S31;  Sum[14]=S40;
    {
      double[,] A = new double[5, 5];
      double[,] B = new double[5, 1];

      int nSum = 15;
      double[] Sum = new double[nSum];
      MakeSums2(Vert, iv1, nPoints1, iv2, nPoints2, Sum);
      A[0, 0] = 2.0 * Sum[12]; A[0, 1] = Sum[11]; A[0, 2] = 2.0 * Sum[8]; A[0, 3] = 2.0 * Sum[7]; A[0, 4] = Sum[4];
      A[1, 0] = 2.0 * Sum[11]; A[1, 1] = Sum[10]; A[1, 2] = 2.0 * Sum[7]; A[1, 3] = 2.0 * Sum[6]; A[1, 4] = Sum[3];
      A[2, 0] = 2.0 * Sum[8]; A[2, 1] = Sum[7]; A[2, 2] = 2.0 * Sum[5]; A[2, 3] = 2.0 * Sum[4]; A[2, 4] = Sum[2];
      A[3, 0] = 2.0 * Sum[7]; A[3, 1] = Sum[6]; A[3, 2] = 2.0 * Sum[4]; A[3, 3] = 2.0 * Sum[3]; A[3, 4] = Sum[1];
      A[4, 0] = 2.0 * Sum[4]; A[4, 1] = Sum[3]; A[4, 2] = 2.0 * Sum[2]; A[4, 3] = 2.0 * Sum[1]; A[4, 4] = Sum[0];

      B[0, 0] = -Sum[13]; B[1, 0] = -Sum[12]; B[2, 0] = -Sum[9]; B[3, 0] = -Sum[8]; B[4, 0] = -Sum[5];

      Gauss_K(A, 5, B, 1);                            //    k1               k2                k3
      f = -0.5 * Math.Atan2(2.0 * B[0, 0], 1.0 - B[1, 0]);

      c = (B[0, 0] * B[3, 0] - B[1, 0] * B[2, 0]) / (B[1, 0] - B[0, 0] * B[0, 0]);
      d = (B[0, 0] * B[2, 0] - B[3, 0]) / (B[1, 0] - B[0, 0] * B[0, 0]);


      Delta = B[1, 0] - B[0, 0] * B[0, 0];
      double BigDelta = B[1, 0] * B[4, 0] + B[0, 0] * B[3, 0] * B[2, 0] + B[0, 0] * B[3, 0] * B[2, 0] -
          B[2, 0] * B[1, 0] * B[2, 0] - B[3, 0] * B[3, 0] - B[4, 0] * B[0, 0] * B[0, 0];
      double S = 1.0 + B[1, 0];
      double a2, b2;
      double aprim = (1.0 + B[1, 0] + Math.Sqrt((1.0 - B[1, 0]) * (1.0 - B[1, 0]) + 4.0 * B[0, 0] * B[0, 0])) * 0.5;
      double cprim = (1.0 + B[1, 0] - Math.Sqrt((1.0 - B[1, 0]) * (1.0 - B[1, 0]) + 4.0 * B[0, 0] * B[0, 0])) * 0.5;
      a2 = -BigDelta / aprim / Delta;
      b2 = -BigDelta / cprim / Delta;
      a = Math.Sqrt(a2);
      b = Math.Sqrt(b2);

      double FF = Sum[14] + 2 * B[0, 0] * Sum[13] + B[1, 0] * Sum[12] + 2 * B[2, 0] * Sum[9] + 2 * B[3, 0] * Sum[8] +
        B[4, 0] * Sum[5] + 4 * B[0, 0] * B[0, 0] * Sum[12] + 2 * B[0, 0] * B[1, 0] * Sum[11] +
        4 * B[0, 0] * B[2, 0] * Sum[8] + 4 * B[0, 0] * B[3, 0] * Sum[7] + 2 * B[0, 0] * B[4, 0] * Sum[4] +
        B[1, 0] * B[1, 0] * Sum[10] + 2 * B[1, 0] * B[2, 0] * Sum[7] + 2 * B[1, 0] * B[3, 0] * Sum[6] +
        B[1, 0] * B[4, 0] * Sum[3] + 4 * B[2, 0] * B[2, 0] * Sum[5] + 2 * B[2, 0] * B[3, 0] * Sum[4] +
        2 * B[2, 0] * B[4, 0] * Sum[2] + 4 * B[3, 0] * B[3, 0] * Sum[3] + 2 * B[3, 0] * B[4, 0] * Sum[1] +
        B[4, 0] * B[4, 0] * Sum[0];

      FF = Sum[5] * Sum[5] + 4 * B[0, 0] * B[0, 0] * Sum[4] * Sum[4] + B[1, 0] * B[1, 0] * Sum[3] * Sum[3] + 4 * B[2, 0] * B[2, 0] * Sum[2] * Sum[2] +
        4 * B[3, 0] * B[3, 0] * Sum[1] * Sum[1] + B[4, 0] * B[4, 0] +
        4 * B[0, 0] * Sum[11] + 2 * B[1, 0] * Sum[12] + 4 * B[2, 0] * Sum[9] + 4 * B[3, 0] * Sum[8] + 2 * B[4, 0] * Sum[5] +
        4 * B[0, 0] * B[1, 0] * Sum[11] + 8 * B[0, 0] * B[2, 0] * Sum[8] + 8 * B[0, 0] * B[3, 0] * Sum[7] + 2 * B[0, 0] * B[4, 0] * Sum[4] +
        4 * B[1, 0] * B[2, 0] * Sum[7] + 4 * B[1, 0] * B[3, 0] * Sum[6] + 2 * B[1, 0] * B[4, 0] * Sum[3] +
        8 * B[2, 0] * B[3, 0] * Sum[4] + 4 * B[1, 0] * B[4, 0] * Sum[2] + 4 * B[3, 0] * B[4, 0] * Sum[1];

      F = FF / Sum[0];
      F = Math.Sqrt(F); // / 1000.0;
      if (Delta > 0.0)  return 1;
    
      return -1;
    } //************************************ end GetEllipse2 **********************************************************


    public int GetEllipseNew(Point[] Vert, int iv1, int nPoints1, int iv2, int nPoints2, ref double Delta, ref double f,
                                      ref double a, ref double b, ref double c, ref double d)
    {
      double[,] A = new double[5, 5];
      double[,] B = new double[5, 1];

      int nSum = 15;
      double[] Sum = new double[nSum];
      MakeSums2(Vert, iv1, nPoints1, iv2, nPoints2, Sum);
      A[0, 0] = 2.0 * Sum[12]; 
      A[0, 1] = Sum[11]; 
      A[0, 2] = 2.0 * Sum[8]; 
      A[0, 3] = 2.0 * Sum[7]; 
      A[0, 4] = Sum[4];
      A[1, 0] = 2.0 * Sum[11]; 
      A[1, 1] = Sum[10]; 
      A[1, 2] = 2.0 * Sum[7]; 
      A[1, 3] = 2.0 * Sum[6]; 
      A[1, 4] = Sum[3];
      A[2, 0] = 2.0 * Sum[8]; 
      A[2, 1] = Sum[7]; 
      A[2, 2] = 2.0 * Sum[5]; 
      A[2, 3] = 2.0 * Sum[4]; 
      A[2, 4] = Sum[2];
      A[3, 0] = 2.0 * Sum[7]; 
      A[3, 1] = Sum[6]; 
      A[3, 2] = 2.0 * Sum[4]; 
      A[3, 3] = 2.0 * Sum[3]; 
      A[3, 4] = Sum[1];
      A[4, 0] = 2.0 * Sum[4]; 
      A[4, 1] = Sum[3]; 
      A[4, 2] = 2.0 * Sum[2]; 
      A[4, 3] = 2.0 * Sum[1]; 
      A[4, 4] = Sum[0];

      B[0, 0] = -Sum[13]; 
      B[1, 0] = -Sum[12]; 
      B[2, 0] = -Sum[9]; 
      B[3, 0] = -Sum[8]; 
      B[4, 0] = -Sum[5];

      Gauss_K(A, 5, B, 1);                            

      f = -0.5 * Math.Atan2(2.0 * B[0, 0], 1.0 - B[1, 0]);
      c = (B[0, 0] * B[3, 0] - B[1, 0] * B[2, 0]) / (B[1, 0] - B[0, 0] * B[0, 0]);
      d = (B[0, 0] * B[2, 0] - B[3, 0]) / (B[1, 0] - B[0, 0] * B[0, 0]);

      Delta = B[1, 0] - B[0, 0] * B[0, 0];
      double BigDelta = B[1, 0] * B[4, 0] + B[0, 0] * B[3, 0] * B[2, 0] + B[0, 0] * B[3, 0] * B[2, 0] -
          B[2, 0] * B[1, 0] * B[2, 0] - B[3, 0] * B[3, 0] - B[4, 0] * B[0, 0] * B[0, 0];
      double S = 1.0 + B[1, 0];
      double a2, b2;
      double aprim = (1.0 + B[1, 0] + Math.Sqrt((1.0 - B[1, 0]) * (1.0 - B[1, 0]) + 4.0 * B[0, 0] * B[0, 0])) * 0.5;
      double cprim = (1.0 + B[1, 0] - Math.Sqrt((1.0 - B[1, 0]) * (1.0 - B[1, 0]) + 4.0 * B[0, 0] * B[0, 0])) * 0.5;
      a2 = -BigDelta / aprim / Delta;
      b2 = -BigDelta / cprim / Delta;
      a = Math.Sqrt(a2);
      b = Math.Sqrt(b2);

      if (Delta > 0.0)  return 1;
      
      return -1;
    } //************************************ end GetEllipseNew **********************************************************


    public void DrawEllipsePen(Pen pen, double a, double b, double c, double d, Form1 fm1)
    {
      int xold, x, yold, y;
      double Step = 0.1;
      Graphics g = fm1.g2Bmp; 
      xold = fm1.marginX + (int)(fm1.Scale1 * (c + a));
      yold = fm1.marginY + (int)(fm1.Scale1 * (d));
      for (double t = 0; t < 2 * Math.PI; t += Step)
      {
        x = fm1.marginX + (int)(fm1.Scale1 * (c + a * Math.Cos(t)));
        y = fm1.marginY + (int)(fm1.Scale1 * (d + b * Math.Sin(t)));
        g.DrawLine(pen, xold, yold, x, y);
        xold = x; yold = y;
      }
      Rectangle rect = new Rectangle(fm1.marginX + (int)(fm1.Scale1 * c), fm1.marginY +
        (int)(fm1.Scale1 * d), 4, 4);
      SolidBrush yellowBrush = new SolidBrush(Color.Yellow);
      g.FillRectangle(yellowBrush, rect);
      fm1.pictureBox2.Image = fm1.BmpPictBox2;
    }

    
    public void DrawBike(Graphics g, Pen whitePen, int hW,
                                      int[,] Block, double[] PointX, double[] PointY, Form1 fm1)
    {
      double[] A = new double[8];
      double[] B = new double[8];
      double Root1 = 0.0, Scale1 = 1.0; // fm1.Scale1;
      int maX = 0, maY = 0;
      for (int m = 0; m < 8; m++) //=================================================================================================
      {
        A[m] = PointY[Block[m, 0]] - PointY[Block[m, 1]];
        B[m] = PointX[Block[m, 1]] - PointX[Block[m, 0]];

        Root1 = Math.Sqrt(A[m] * A[m] + B[m] * B[m]);
        A[m] /= Root1;
        B[m] /= Root1;

        g.DrawLine(whitePen, (int)((PointX[Block[m, 0]] - hW * A[m]) * Scale1) + maX,
                              (int)((PointY[Block[m, 0]] - hW * B[m]) * Scale1) + maY, // upper side
                              (int)((PointX[Block[m, 0]] + hW * A[m]) * Scale1) + maX, 
                              (int)((PointY[Block[m, 0]] + hW * B[m]) * Scale1) + maY);

        g.DrawLine(whitePen, (int)((PointX[Block[m, 0]] + hW * A[m]) * Scale1) + maX,
                              (int)((PointY[Block[m, 0]] + hW * B[m]) * Scale1) + maY, // left side
                              (int)((PointX[Block[m, 1]] + hW * A[m]) * Scale1) + maX, 
                              (int)((PointY[Block[m, 1]] + hW * B[m]) * Scale1) + maY);

        g.DrawLine(whitePen, (int)((PointX[Block[m, 1]] + hW * A[m]) * Scale1) + maX,
                              (int)((PointY[Block[m, 1]] + hW * B[m]) * Scale1) + maY, // below side
                              (int)((PointX[Block[m, 1]] - hW * A[m]) * Scale1) + maX, 
                              (int)((PointY[Block[m, 1]] - hW * B[m]) * Scale1) + maY);

        g.DrawLine(whitePen, (int)((PointX[Block[m, 1]] - hW * A[m]) * Scale1) + maX,
                              (int)((PointY[Block[m, 1]] - hW * B[m]) * Scale1) + maY, // right side
                              (int)((PointX[Block[m, 0]] - hW * A[m]) * Scale1) + maX, 
                              (int)((PointY[Block[m, 0]] - hW * B[m]) * Scale1) + maY);
      } //============================ end for (m = 0; ... ====================================================
      fm1.pictureBox2.Refresh(); // Image = fm1.BmpPictBox2;
    } //****************************** end DrawBike ***********************************************************
  }
}
