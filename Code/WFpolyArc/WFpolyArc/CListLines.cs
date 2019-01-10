using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;



namespace WFpolyArc
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

    public static bool operator ==(iVect2 a, iVect2 b) // used in 'CheckComb'
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

  public struct CArc2
  {
    public int Start, nPoints;
    public float Mx, My, R;
  }



  public class CListLines
  {
    public
    int nArc, nPolygon, nVert, MaxPoly, MaxVert, MaxArc;

    public static double PosSectX, PosSectY, NegSectX, NegSectY;
    public static iVect2 Far;

    public iVect2[] Step;
    public iVect2[] Norm;

    public CPolygon[] Polygon;
    public iVect2[] Vert;
    public CArc[] Arc;
    public CArc2[] Arc2;
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
      Vert = new iVect2[MaxVert];
      for (i = 0; i < MaxVert; i++) Vert[i] = new iVect2();
      Arc = new CArc[MaxArc];
      for (i = 0; i < MaxArc; i++) Arc[i] = new CArc();
      Arc2 = new CArc2[MaxArc];
      for (i = 0; i < MaxArc; i++) Arc2[i] = new CArc2();
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


    public int MessReturn(string s)
    {
      if (MessageBox.Show(s,
              "Return", MessageBoxButtons.OKCancel) == DialogResult.Cancel) return -1;
      return 1;
    }


    public void CheckSmooth(double minSin, double maxProportion)
    {
      int first, last, numKnick, sign = 1;
      double SinAngle = 0.0, Len1 = 0.0, Len2 = 0.0;
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
          SinAngle = ((Vert[iv].X - Vert[iv - 1].X) * (Vert[iv + 1].Y - Vert[iv].Y) -
                     (Vert[iv + 1].X - Vert[iv].X) * (Vert[iv].Y - Vert[iv - 1].Y)) / Len1 / Len2;
          if (iv == first + 1)
          {
            if (SinAngle < 0.0) sign = -1;
            else sign = 1;
          }
          if (SinAngle * sign > minSin) numKnick++;
        } //========================= end for (int iv ... ===========================================
        if ((double)numKnick / (double)(last - first + 1) > maxProportion) Polygon[ip].smooth = false;
        else Polygon[ip].smooth = true;
      } //=========================== end for (int ip ... =============================================
    } //****************************** end CheckSmooth **************************************************


    public int SearchPoly(ref CImage Comb, double eps, Form1 fm1)
    {
      int Lab, rv = 0, x, y, y2, CNX = Comb.width, CNY = Comb.height;
      fm1.progressBar1.Value = 0;
      fm1.progressBar1.Step = 1;
      fm1.progressBar1.Visible = true;
      int jump, Len = CNY / 2, nStep = 100;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (y = 0, y2 = 0; y < CNY; y += 2, y2++)
      {
        if ((y2 % jump) == jump - 1) fm1.progressBar1.PerformStep();
        for (x = 0; x < CNX; x += 2)
        {
          Lab = Comb.Grid[x + CNX * y] & 135;
          if (Lab == 1 || Lab == 3 || Lab == 4)
          {
            rv = ComponPoly(Comb, x, y, eps);
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
          Lab = Comb.Grid[x + CNX * y] & 7;
          if (Lab == 2)
          {
            rv = ComponPoly(Comb, x, y, eps);
            if (rv < 0)
            {
              MessageBox.Show("SearchPoly, Alarm! ComponPoly returned " + rv);
              return -1;
            }
          }
        }
      //fm1.progressBar1.Visible = false;
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



    public int TraceApp(CImage Comb, int X, int Y, double eps, ref iVect2 Pterm, ref int dir)
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

      int[] Shift = new int[4];
      Shift[0] = 0;
      Shift[1] = 2;
      Shift[2] = 4;
      Shift[3] = 6;

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
          MessageBox.Show("TraceApp, error: dir=" + dir + " the Crack=(" + Crack.X + "," + Crack.Y +
          ") has label 0;  X=" + X + ", Y=" + Y + "; iCrack=" + iCrack + " return -1");
          MessageBox.Show("Point before Crack Lab=" + (Comb.Grid[P.X + CNX * P.Y] & 7) + " P+0=" + (Comb.Grid[P.X + 1 + CNX * P.Y] & 7) +
          " P+1=" + (Comb.Grid[P.X + CNX * (P.Y + 1)] & 7) + " P+2=" + (Comb.Grid[P.X - 1 + CNX * P.Y] & 7) +
          " P+3=" + (Comb.Grid[P.X + CNX * (P.Y - 1)] & 7));
        }
        P.X = P1.X = Crack.X + Step[dir].X;
        P.Y = P1.Y = Crack.Y + Step[dir].Y;
        Pstand.X = P.X / 2;
        Pstand.Y = P.Y / 2;

        br = CheckComb(StartEdge, Pstand, eps, ref Vect);

        Lab = Comb.Grid[P.X + CNX * P.Y] & 7; 
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

          br = 0;
          nVert++;
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



    public int ComponPoly(CImage Comb, int X, int Y, double eps)
    /* Encodes in "CListLines" the polygones of the edge component with the point (X, Y) being
      a branch or an end point. Puts the starting point 'Pinp' into the queue and starts
      the 'while' loop. It tests each labeled crack incident to the point 'P' fetched from the queue.
      If the next point of the crack is a branch or an end point, then crack is being ignorred.
      Otherwise the funktion "TraceApp" is called. "TraceApp" traces the edge until the next end or
      branch point and calulates the verices of the approximating polygon. The tracing ends at the 
      point 'Pterm' with the direction 'DirT'. If the point 'Pterm' is a branch point then it is put 
      to the queue. "ComponPoly" returns when the queue is empty.	---------------*/
    {
      int dir, dirT;
      int LabNext, rv;
      iVect2 Crack, P, Pinp, Pnext, Pterm;
      Crack = new iVect2();
      P = new iVect2();
      Pinp = new iVect2();
      Pnext = new iVect2();
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
          if (Comb.Grid[Crack.X + CNX * Crack.Y] == 1) //--------------------------------------
          {
            Pnext.X = Crack.X + Step[dir].X;
            Pnext.Y = Crack.Y + Step[dir].Y;
            LabNext = Comb.Grid[Pnext.X + CNX * Pnext.Y] & 7; 

            if (LabNext == 3) pQ.Put(Pnext);
            if (LabNext == 2) //--------------------------------------------------------------
            {
              Polygon[nPolygon].firstVert = nVert;
              dirT = dir;
              Pterm = new iVect2();

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
              else nPolygon++;
              if ((Comb.Grid[Pterm.X + CNX * Pterm.Y] & 128) == 0 && rv >= 3) pQ.Put(Pterm);
            } // ------------- end if (LabNest==2) -----------------------------------------------------
            if ((Comb.Grid[P.X + CNX * P.Y] & 7) == 1) break;
          } //--------------- end if (Comb.Grid[Crack.X ...==1) ------------------------------------------
        } //================================== end for (dir ... ==========================================
        Comb.Grid[P.X + CNX * P.Y] |= 128;
      } //==================================== end while ==========================================
      return 1;
    } //************************************** end ComponPoly ************************************************



    public void DrawPolygons(PictureBox pictureBox, CImage EdgeIm, Form1 fm1)
    {
      bool Rect_on = true;
      Brush smoothBrush, nonBrush, delBrush;
      nonBrush = new System.Drawing.SolidBrush(Color.Yellow);
      smoothBrush = new System.Drawing.SolidBrush(Color.Yellow);
      delBrush = new SolidBrush(Color.Black);

      Pen greenPen;
      int penWidth = fm1.WidthG / 200;
      greenPen = new System.Drawing.Pen(Color.Green, penWidth);

      double ScaleX = (double)pictureBox.Width / EdgeIm.width;
      double ScaleY = (double)pictureBox.Height / EdgeIm.height;
      double Scale1;
      if (ScaleX < ScaleY) Scale1 = ScaleX;
      else Scale1 = ScaleY;
      double marginX = (pictureBox.Width - Scale1 * EdgeIm.width) * 0.5;
      double marginY = (pictureBox.Height - Scale1 * EdgeIm.height) * 0.5;
      Rectangle rect1, rect2, rect0;

      rect0 = new Rectangle(0, 0, fm1.WidthG, fm1.HeightG);
      fm1.g2Bmp.FillRectangle(delBrush, rect0);
      int x1, y1, x2, y2;
      for (int il = 0; il < nPolygon; il++) //=========================
      {
        int first = Polygon[il].firstVert;
        int last = Polygon[il].lastVert;
        bool smooth = Polygon[il].smooth, closed = Polygon[il].closed;

        for (int iv = first; iv < last; iv++) //===
        {
          if (Polygon[il].lastVert - first + 1 < 2) continue;
          x1 = Vert[iv].X; 
          y1 = Vert[iv].Y; 
          x2 = Vert[iv + 1].X; 
          y2 = Vert[iv + 1].Y; 

          rect1 = new Rectangle(x1 - 1, y1 - 1, 2, 2);
          rect2 = new Rectangle(x2 - 1, y2 - 1, 2, 2);
          fm1.g2Bmp.DrawLine( greenPen, x1, y1, x2, y2);
          if (Rect_on) fm1.g2Bmp.FillRectangle(smoothBrush, rect1);
        } //===================== end for (int iv ... ===============
        if (Polygon[il].closed)
        {
          x1 = Vert[first].X;
          y1 = Vert[first].Y;
          x2 = Vert[last].X;
          y2 = Vert[last].Y;
          fm1.g2Bmp.DrawLine(greenPen, x1, y1, x2, y2);
          rect2 = new Rectangle(x2 - 1, y2 - 1, 2, 2);
          if (Rect_on) fm1.g2Bmp.FillRectangle(smoothBrush, rect2);
        }
      } //======================= end for (int il ... =================
      fm1.pictureBox2.Image = fm1.BmpPictBox2;
    } //************************* end DrawPolygons *************************


    public int DrawArc(CArc arc, bool DispRad, Form1 fm1, int ip)
    /* Draws an arc with radius arc.rad going through (arc.xb, arc.yb) and (arc.xe, arc.ye)
    with the scale "fm1.Scale1". Draws also the radii at the ends of the arc if arc.rad < maxRad.
    Starts at the point (arc.xb, arc.yb) undependently of the tracing direction of the polygon.
    */
    {
      bool bar_on;
      double AbsR, al, del = 0.1, angle = 0.0, R = 0.0, chord_X, chord_Y, Length, fmag = fm1.Scale1,
        sin_d, cos_d, x = 0, y = 0, xn = 0, yn = 0, fmx = arc.xm, fmy = arc.ym, Scale1 = fm1.Scale1;
      int maxRad = 200, marginX = fm1.marginX, marginY = fm1.marginY, ixb, iyb, ixe, iye, ixm, iym;
      int penWidth = fm1.WidthG / 800;
      if (penWidth == 0) penWidth = 1;
      Pen arcPen = new Pen(Color.Red, penWidth);
      Pen posLinePen = new Pen(Color.Yellow, penWidth);
      Pen negLinePen = new Pen(Color.Violet, penWidth);

      bar_on = fmag >= 2.0; // switches on the filling of the 3x3 rectangles for points 

      R = arc.rad;
      AbsR = R;
      if (R < 0.0) AbsR = -R;

      chord_X = arc.xe - arc.xb; 
      chord_Y = arc.ye - arc.yb; // the chord

      Length = Math.Sqrt(chord_X * chord_X + chord_Y * chord_Y); // Length of the chord

      ixb = (int)arc.xb; // marginX + (int)(Scale1 * arc.xb);
      iyb = (int)arc.yb; // marginY + (int)(Scale1 * arc.yb);
      ixe = (int)arc.xe; // marginX + (int)(Scale1 * arc.xe);
      iye = (int)arc.ye; // marginY + (int)(Scale1 * arc.ye);
      ixm = (int)arc.xm; // marginX + (int)(Scale1 * arc.xm);
      iym = (int)arc.ym; // marginY + (int)(Scale1 * arc.ym); 

      Pen linePen; // = new System.Drawing.Pen(Color.Blue);
      if (AbsR < maxRad && AbsR > 30.0 && DispRad)   // Radius vectors at the end ppoints of the arc 
      {
        if (R > 0) linePen = posLinePen;
        else linePen = negLinePen;

        fm1.g2Bmp.DrawLine(linePen, ixm, iym, ixb, iyb);
        fm1.g2Bmp.DrawLine(linePen, ixm, iym, ixe, iye);
      }

      angle = 2.0 * Math.Asin(Length / 2.0 / AbsR);       // angle of the arc 

      if (R > 0) linePen = new Pen(Color.Red);

      del = 0.1;
      cos_d = Math.Cos(del); // "del" is the step of the circular moving of the point (x, y)
      if (R < 0.0) sin_d = -Math.Sin(del);
      else sin_d = Math.Sin(del);

      if (angle < 2.0 * del) // The for-loop with 'al' is not used in this case
      {
        fm1.g2Bmp.DrawLine(arcPen, ixb, iyb, ixe, iye);
        xn = yn = 0;
      }
      else
      {
        x = arc.xb - arc.xm;
        y = arc.yb - arc.ym;
        for (al = 0.0; al < angle; al += del)       // Drawing an arc step by step 
        {
          xn = x * cos_d + y * sin_d;
          yn = -x * sin_d + y * cos_d;

          fm1.g2Bmp.DrawLine(arcPen, (int)(arc.xm + x + 0.5), (int)(arc.ym + y + 0.5),
                                    (int)(arc.xm + xn + 0.5), (int)(arc.ym + yn + 0.5));
          x = xn; y = yn;
        }
      }
      if (bar_on)
      {
        Brush myBrush = new System.Drawing.SolidBrush(Color.Violet);
        int size = 4;
        Rectangle rect = new Rectangle((int)arc.xb, (int)arc.yb, size, size);
        fm1.g2Bmp.FillRectangle(myBrush, rect);
        rect = new Rectangle((int)arc.xe, (int)arc.ye, size, size);
        fm1.g2Bmp.FillRectangle(myBrush, rect);
      }
      fm1.pictureBox2.Image = fm1.BmpPictBox2;
      return 0;
    } // ********************** end DrawArc ********************************* 



    public int Curvature(int x1, int y1, int x2, int y2, int x3, int y3,
          double eps, ref CArc arc)
    /* Calculates the curvature 'k' of an arc lying in the tolerance tube
      around the given two polygon edges [(x1,y1), (x2,y2)] and 
      [(x2,y2), (x3,y3)] and having the outer boundary of the tube as its
      tangent. The tangency point should be not farther as the half length
      of the shorter edge from (x2,y2). The radius of the arc should be as
      large as possible. */
    {
      //bool deb = false;
      double a1, a2, lp,  // Variables for calculating the projections
      dx1, dy1, dx2, dy2, len1, len2,    // Inkrements, lengths of edges 
      cosgam, // cosine of the outer angle between the edges
      sinbet, cosbet,    // cosine and sine of the half angle 
      strip = 0.6,       // correcture of the deviation 
      k, cru1, cru2;     // curvature for long and short edges

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

      cru1 = (1.0 - cosbet) / (eps - strip);    // long edge, it is important
      // that the arc goes throught the vertex
      double min_len = len1;
      if (len2 < min_len) min_len = len2;
      if (min_len != 0.0 && cosbet != 0.0)
        cru2 = 2.0 * sinbet / cosbet / min_len;    // short edge, it is important
      else cru2 = 0.0;                       // that the tangency is in the midle of the shortest edge

      if ((Math.Abs(cru1) > Math.Abs(cru2)) && cru1 != 0.0)
      {
        if (cosbet != 0.0 && cru1 != 0.0)
          lp = sinbet / cosbet / cru1;        // distance of the point of tangency from (x2, y2)
        else lp = 100.0;
        k = cru1;    // first curvature 
      }
      else
      {
        lp = min_len / 2.0;
        k = cru2 * 0.95;    // second curvature
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
      { // can a closed line of three points exist?? 
        return -2;
      }
      return 0;
    } // *********************** end Curvature ****************************************************** 


    public int FindArcs(PictureBox pictureBox, CImage EdgeIm, double eps, Form1 fm1)
    /* The method calculates the parametrs of the arcs contained in
      the polygons. Fills the array 'Arc[]' of structures "CArc". Shows the contents 
      of this array graphically. */
    {
      bool disp = true;
      int j, ip, first, last, Len, rv, x1, y1, x2, y2, x3, y3; 
      nArc = 0;
      Pen linePen = new System.Drawing.Pen(Color.LightBlue);
      int marginX = fm1.marginX;
      int marginY = fm1.marginY;
      double Scale1 = fm1.Scale1;

      fm1.progressBar1.Value = 0;
      fm1.progressBar1.Visible = true;
      fm1.progressBar1.Step = 1;
      int jump, Len1 = nPolygon, nStep = 100;
      if (Len1 > 2 * nStep) jump = Len1 / nStep;
      else jump = 2;
      for (ip = 0; ip < nPolygon; ip++) // =========================== 
      {
        if (ip % jump == jump - 1) fm1.progressBar1.PerformStep();
        int cntArc = 0;
        Polygon[ip].firstArc = -1;
        Polygon[ip].lastArc = -2;
        first = Polygon[ip].firstVert;
        last = Polygon[ip].lastVert;

        if (last > first + 1) // ------- are there sufficient many vertices?----------- 
        {
          if (disp) // here are three points of a polygon calculated but not drawn
          {
            x1 = (int)(Scale1 * Vert[first].X + 0.5);  // starting point
            y1 = (int)(Scale1 * Vert[first].Y + 0.5);
          }
          x2 = Vert[first].X;        // will become the first point of the arc
          y2 = Vert[first].Y;
          x3 = Vert[first + 1].X;      // second point
          y3 = Vert[first + 1].Y;

          Polygon[ip].firstArc = nArc;

          // Points from the second one until the one before the last
          Len = last - first + 1;
          for (j = 2; j <= Len; j++) // ============================= 
          {
            x1 = x2; y1 = y2;
            x2 = x3; y2 = y3;
            x3 = Vert[first + j % Len].X;
            y3 = Vert[first + j % Len].Y;

            CArc arc = new CArc();
            // 'Curvature' calculates and saves parameters of an arc in 'arc'.
            rv = Curvature(x1, y1, x2, y2, x3, y3, eps, ref arc);

            if (rv < 0) continue;
            if (Math.Abs(arc.xb - arc.xe) < 1.0 && Math.Abs(arc.yb - arc.ye) < 1.0 || Math.Abs(arc.rad) < 2.0) continue;

            // The arc is saved in the array "Arc" of arcs:
            Arc[nArc] = arc;
            if (cntArc == 0) Polygon[ip].firstArc = nArc;

            cntArc++;
            nArc++;
            if (disp) //-------------------------------------------------------------
            {
              rv = DrawArc(arc, true, fm1, ip);
              if (rv < 0) return -1;
             } //---------------------- end if (disp) ---------------------------------------------------------
            if (j == Len - 1) Polygon[ip].lastArc = nArc - 1;
          } // ======================= end for (j... ========================================================= 
        } // ------------------------- end if (last > first+1 ) -----------------------------------------------
      } // =========================== end for (ip... ========================================================== 
      fm1.pictureBox2.Image = fm1.BmpPictBox2;
      fm1.progressBar1.Visible = false;
      return 0;
    } // ***************************** end FindArcs **************************************************************



    public void CurvatureNew(int x1, int y1, int x2, int y2, int x3, int y3, double eps, ref CArc arc)
    {
      double chordX, chordY, ortX, ortY, Length, dist, VectProd;
      chordX = x2 - x1;
      chordY = y2 - y1;
      Length = Math.Sqrt(chordX * chordX + chordY * chordY);
      arc.rad = (float)(Length * Length / eps / 8.0);
      arc.xb = (float)x1;
      arc.yb = (float)y1;
      arc.xe = (float)x2;
      arc.ye = (float)y2;
      dist = Math.Sqrt(arc.rad * arc.rad - Length * Length / 4.0);
      VectProd = (x2 - x1) * (y3 - y2) - (x3 - x2) * (y2 - y1);
      if (VectProd < 0)
      {
        ortX = chordY;
        ortY = -chordX;
      }
      else
      {
        ortX = -chordY;
        ortY = chordX;
      }

      arc.xm = (float)((x1 + x2) / 2 + ortX * dist / Length);
      arc.ym = (float)((y1 + y2) / 2 + ortY * dist / Length);
    }
  } //************************** end CListLines *************************************
} //**************************** end namespace ***********************************************
