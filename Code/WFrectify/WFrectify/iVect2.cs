using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WFrectify
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
      return new iVect2(a.X+b.X, a.Y+b.Y);
    }

    public static iVect2 operator -(iVect2 a, iVect2 b)
    { 
      return new iVect2(a.X-b.X, a.Y-b.Y);
    }

    public static iVect2 operator -(iVect2 a)
    { 
      return new iVect2(-a.X, -a.Y);
    }
  } //********************* end public class iVect2 ***************************
 }
