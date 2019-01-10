using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace WFcompressPal
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
    } //--*/
  } //********************* end public struct iVect2 ***************************/


    public class CInscript
    {
        public double Zoom;
        public int marginX, marginY, Width;
        public Color Color1;
        public Graphics g;
        public Pen myPen;
        public CInscript() { } // default constructor
        //CInscript(Zoom, marginX, marginY, 1, Color.White);
        public CInscript(int picBoxInd, double scale, int marx, int mary, int width, Color color, Form1 fm1) // constructor
        {
          Zoom = scale;
          marginX = marx;
          marginY = mary;
          Width = width;
          Color1 = color;
          if (picBoxInd == 1) g = fm1.pictureBox1.CreateGraphics();
          else g = fm1.pictureBox2.CreateGraphics();
          myPen = new Pen(Color1);
        }

        public int Minus(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          int sizeV = 2;
          //iVect2[] Vert = {  {2, 9),  {10, 9) }; // "-"
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(2, 9);
          Vert[1] = new iVect2(10, 9); // "-"
          xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        } //*************************** end Minus ****************************

        public int Plus(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          int sizeV = 5;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(0, 9); Vert[1] = new iVect2(12, 9);
          Vert[2] = new iVect2(6, 9);
          Vert[3] = new iVect2(6, 15); Vert[4] = new iVect2(6, 3); // "+"
          xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        } //*************************** end Minus ****************************


        public int Stop(int x0, int y0)
        {
          int xmax = 0, xold, yold;
          xold = 4 - Width; yold = 18 - Width;
          //x=4; y=0;
          xmax = 4;
          SolidBrush myBrush = new SolidBrush(Color.White);
          Rectangle rect = new
          Rectangle(marginX + x0 + 4 - Width + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold), Width, Width);
          g.FillRectangle(myBrush, rect);
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int Equal(int x0, int y0)
        {
          int x, xmax = 0, xold, y, yold;
          const int sizeV = 4;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(1, 11); Vert[1] = new iVect2(11, 11);
          Vert[2] = new iVect2(1, 7); Vert[3] = new iVect2(11, 7); // "="
          xmax = 10;
          xold = Vert[0].X;
          yold = Vert[0].Y;
          x = Vert[1].X; y = Vert[1].Y;
          g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
            marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
          xold = Vert[2].X;
          yold = Vert[2].Y;
          x = Vert[3].X; y = Vert[3].Y;
          g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
            marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int Null(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 9;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(9, 15); Vert[1] = new iVect2(6, 18);
          Vert[2] = new iVect2(3, 18); Vert[3] = new iVect2(0, 15);
          Vert[4] = new iVect2(0, 3); Vert[5] = new iVect2(3, 0);
          Vert[6] = new iVect2(6, 0); Vert[7] = new iVect2(9, 3);
          Vert[8] = new iVect2(9, 15); // "O"
          xmax = 0;
          xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 7));
        }

        public int F1(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 5;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(3, 18); Vert[1] = new iVect2(7, 18);
          Vert[2] = new iVect2(5, 18); Vert[3] = new iVect2(5, 0);
          Vert[4] = new iVect2(0, 5); // "T"
          xmax = xold = Vert[0].X;
          xmax = 0;
          xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 7));
        }

        public int F2(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 8;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(9, 18); Vert[1] = new iVect2(0, 18);
          Vert[2] = new iVect2(9, 6); Vert[3] = new iVect2(9, 3);
          Vert[4] = new iVect2(6, 0); Vert[5] = new iVect2(3, 0);
          Vert[6] = new iVect2(0, 3); Vert[7] = new iVect2(0, 6); // "2"
          xmax = xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int F3(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 13;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(0, 3); Vert[1] = new iVect2(3, 0);
          Vert[2] = new iVect2(6, 0); Vert[3] = new iVect2(9, 3);
          Vert[4] = new iVect2(9, 6); Vert[5] = new iVect2(6, 9);
          Vert[6] = new iVect2(3, 9); Vert[7] = new iVect2(6, 9);
          Vert[8] = new iVect2(9, 12); Vert[9] = new iVect2(9, 15);
          Vert[10] = new iVect2(6, 18); Vert[11] = new iVect2(3, 18);
          Vert[12] = new iVect2(0, 15); // "3"
          xmax = xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int F4(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 4;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(7, 18); Vert[1] = new iVect2(7, 0);
          Vert[2] = new iVect2(0, 12); Vert[3] = new iVect2(9, 12); // "4"
          xmax = xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int F5(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 9;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(0, 16); Vert[1] = new iVect2(2, 18);
          Vert[2] = new iVect2(6, 18); Vert[3] = new iVect2(9, 15);
          Vert[4] = new iVect2(9, 10); Vert[5] = new iVect2(6, 7);
          Vert[6] = new iVect2(1, 7); Vert[7] = new iVect2(3, 0);
          Vert[8] = new iVect2(9, 0); // "3"
          xmax = xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int F6(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 12;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(9, 0); Vert[1] = new iVect2(5, 2);
          Vert[2] = new iVect2(2, 6); Vert[3] = new iVect2(0, 12);
          Vert[4] = new iVect2(0, 15); Vert[5] = new iVect2(3, 18);
          Vert[6] = new iVect2(6, 18); Vert[7] = new iVect2(9, 15);
          Vert[8] = new iVect2(9, 10); Vert[9] = new iVect2(6, 7);
          Vert[10] = new iVect2(3, 7); Vert[11] = new iVect2(2, 7); // "6"
          xmax = xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int F7(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 4;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(5, 18); Vert[1] = new iVect2(9, 0);
          Vert[2] = new iVect2(2, 0); Vert[3] = new iVect2(2, 2); // "7"
          xmax = xold = Vert[0].X;
          xmax = 0;
          xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 7));
        }

        public int F8(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 13;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(0, 3); Vert[1] = new iVect2(3, 0);
          Vert[2] = new iVect2(6, 0); Vert[3] = new iVect2(9, 3);
          Vert[4] = new iVect2(9, 6); Vert[5] = new iVect2(0, 10);
          Vert[6] = new iVect2(0, 15); Vert[7] = new iVect2(3, 18);
          Vert[8] = new iVect2(6, 18); Vert[9] = new iVect2(9, 15);
          Vert[10] = new iVect2(9, 10); Vert[11] = new iVect2(0, 6);
          Vert[12] = new iVect2(0, 3); // "8"
          xmax = xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;

            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int F9(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 12;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(0, 18); Vert[1] = new iVect2(3, 16);
          Vert[2] = new iVect2(7, 10); Vert[3] = new iVect2(9, 6);
          Vert[4] = new iVect2(9, 3); Vert[5] = new iVect2(6, 0);
          Vert[6] = new iVect2(3, 0); Vert[7] = new iVect2(0, 3);
          Vert[8] = new iVect2(0, 8); Vert[9] = new iVect2(3, 11);
          Vert[10] = new iVect2(6, 11); Vert[11] = new iVect2(7, 10); // "6"
          xmax = xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int A(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          int sizeV = 5;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(0, 18); Vert[1] = new iVect2(6, 0);
          Vert[2] = new iVect2(12, 18); Vert[3] = new iVect2(10, 12);
          Vert[4] = new iVect2(2, 12); // "-"
          xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        } //*************************** end A ****************************



        public int B(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 12;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(0, 18); Vert[1] = new iVect2(0, 0);
          Vert[2] = new iVect2(6, 0); Vert[3] = new iVect2(9, 2);
          Vert[4] = new iVect2(9, 5); Vert[5] = new iVect2(6, 9);
          Vert[6] = new iVect2(0, 9); Vert[7] = new iVect2(6, 9);
          Vert[8] = new iVect2(9, 12); Vert[9] = new iVect2(9, 15);
          Vert[10] = new iVect2(6, 18); Vert[11] = new iVect2(0, 18); // "B"
          xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int C(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 8;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(9, 15); Vert[1] = new iVect2(6, 18);
          Vert[2] = new iVect2(3, 18); Vert[3] = new iVect2(0, 15);
          Vert[4] = new iVect2(0, 3); Vert[5] = new iVect2(3, 0);
          Vert[6] = new iVect2(6, 0); Vert[7] = new iVect2(9, 3); // "T"
          xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int E(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 7;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(11, 18); Vert[1] = new iVect2(0, 18);
          Vert[2] = new iVect2(0, 9); Vert[3] = new iVect2(6, 9);
          Vert[4] = new iVect2(0, 9); Vert[5] = new iVect2(0, 0);
          Vert[6] = new iVect2(11, 0);
          xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int I(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 6;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(0, 18); Vert[1] = new iVect2(10, 18);
          Vert[2] = new iVect2(5, 18); Vert[3] = new iVect2(5, 0);
          Vert[4] = new iVect2(0, 0); Vert[5] = new iVect2(10, 0); // "I"
          xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int K(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 6;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(0, 18); Vert[1] = new iVect2(0, 0);
          Vert[2] = new iVect2(0, 12); Vert[3] = new iVect2(10, 0);
          Vert[4] = new iVect2(5, 6); Vert[5] = new iVect2(10, 18);
          xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int L(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 3;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(0, 0); Vert[1] = new iVect2(0, 18);
          Vert[2] = new iVect2(11, 18); // "L"
          xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int M(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 5;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(0, 18); Vert[1] = new iVect2(0, 0);
          Vert[2] = new iVect2(6, 9); Vert[3] = new iVect2(12, 0);
          Vert[4] = new iVect2(12, 18); //'M'
          xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int N(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 4;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(0, 18); Vert[1] = new iVect2(0, 0);
          Vert[2] = new iVect2(10, 18); Vert[3] = new iVect2(10, 0);
          xmax = xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int R(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          int sizeV = 9;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(0, 18); Vert[1] = new iVect2(0, 0);
          Vert[2] = new iVect2(6, 0); Vert[3] = new iVect2(9, 2);
          Vert[4] = new iVect2(9, 5); Vert[5] = new iVect2(6, 9);
          Vert[6] = new iVect2(0, 9); Vert[7] = new iVect2(6, 9);
          Vert[8] = new iVect2(10, 18); // "-"
          xmax = xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int T(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 4;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(6, 18); Vert[1] = new iVect2(6, 0);
          Vert[2] = new iVect2(0, 0); Vert[3] = new iVect2(10, 0); // "T"
          xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }


        public int U(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 6;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(0, 0); Vert[1] = new iVect2(0, 15);
          Vert[2] = new iVect2(3, 18); Vert[3] = new iVect2(7, 18);
          Vert[4] = new iVect2(10, 15); Vert[5] = new iVect2(10, 0);
          xmax = xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }

        public int W(int x0, int y0)
        {
          int iv, x, xmax = 0, xold, y, yold;
          const int sizeV = 5;
          iVect2[] Vert = new iVect2[sizeV];
          Vert[0] = new iVect2(0, 0); Vert[1] = new iVect2(0, 18);
          Vert[2] = new iVect2(6, 9); Vert[3] = new iVect2(12, 18);
          Vert[4] = new iVect2(12, 0);
          xmax = xold = Vert[0].X;
          yold = Vert[0].Y;
          for (iv = 1; iv < sizeV; iv++)
          {
            x = Vert[iv].X; y = Vert[iv].Y;
            if (x > xmax) xmax = x;
            g.DrawLine(myPen, marginX + x0 + (int)(Zoom * xold), marginY + y0 + (int)(Zoom * yold),
              marginX + x0 + (int)(Zoom * x), marginY + y0 + (int)(Zoom * y));
            xold = x; yold = y;
          }
          return x0 + (int)(Zoom * (xmax + 6));
        }



        public void Write(string Str, int x0, int y0, CInscript Ins)
        {
          int i, len = Str.Length, rv = x0;
          //MessageBox.Show("Write: Str=" + Str + " len=" + len + " x0=" + x0 + " y0=" + y0);
          for (i = 0; i < len; i++) //===============================================
          {
            switch (Str[i])
            {
              case '-': rv = Ins.Minus(rv, y0); break;
              case '+': rv = Ins.Plus(rv, y0); break;
              case '=': rv = Ins.Equal(rv, y0); break;
              case '.': rv = Ins.Stop(rv, y0); break;
              case '0': rv = Ins.Null(rv, y0); break;
              case '1': rv = Ins.F1(rv, y0); break;
              case '2': rv = Ins.F2(rv, y0); break;
              case '3': rv = Ins.F3(rv, y0); break;
              case '4': rv = Ins.F4(rv, y0); break;
              case '5': rv = Ins.F5(rv, y0); break;
              case '6': rv = Ins.F6(rv, y0); break;
              case '7': rv = Ins.F7(rv, y0); break;
              case '8': rv = Ins.F8(rv, y0); break;
              case '9': rv = Ins.F9(rv, y0); break;
              case 'a':
              case 'A': rv = Ins.A(rv, y0); break;
              case 'b':
              case 'B': rv = Ins.B(rv, y0); break;
              case 'c':
              case 'C': rv = Ins.C(rv, y0); break;
              case 'e':
              case 'E': rv = Ins.E(rv, y0); break;
              case 'i':
              case 'I': rv = Ins.I(rv, y0); break;
              case 'k':
              case 'K': rv = Ins.K(rv, y0); break;
              case 'l':
              case 'L': rv = Ins.L(rv, y0); break;
              case 'm':
              case 'M': rv = Ins.M(rv, y0); break;
              case 'n':
              case 'N': rv = Ins.N(rv, y0); break;
              case 'r':
              case 'R': rv = Ins.R(rv, y0); break;
              case 't':
              case 'T': rv = Ins.T(rv, y0); break;
              case 'u':
              case 'U': rv = Ins.U(rv, y0); break;
              case 'w':
              case 'W': rv = Ins.W(rv, y0); break;
            } //::::::::::::::::::::::: end switch :::::::::::::::::::::::::::::::::::
          } //========================= end for (i ... =================================
        } //*************************** end write ****************************************
      }
    
}
