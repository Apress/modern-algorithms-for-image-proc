using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WFpiecewiseLinear
{
  class CImage
  {
    public Byte[] Grid;
    public int NX, NY, N_Bits;

    public CImage(int nx, int ny, int nbits) // constructor
    {
        this.NX = nx;
        this.NY = ny;
        this.N_Bits = nbits;
        Grid = new byte[NX * NY * (N_Bits / 8)];
    }

    public CImage(int nx, int ny, int nbits, byte[] img) // constructor
    {
        this.NX = nx;
        this.NY = ny;
        this.N_Bits = nbits;
        this.Grid = new byte[NX * NY * (N_Bits / 8)];
        for (int i = 0; i < NX * NY * N_Bits / 8; i++) this.Grid[i] = img[i];
    }

    public void Copy(CImage inp)
    {
        NX = inp.NX;
        NY = inp.NY;
        N_Bits = inp.N_Bits;
        for (int i = 0; i < NX * NY * N_Bits / 8; i++)
            Grid[i] = inp.Grid[i];
    }

    public int ColorToGray(CImage inp, Form1 fm1)
    /* Transforms the colors of the color image "inp" in luminance=(r+g+b)/3 
    and puts these values to this.Grid. --------- */
    {
      int c, sum, x, y;
      if (inp.N_Bits != 24) return -1;
      N_Bits = 8; NX = inp.NX; NY = inp.NY;
      Grid = new byte[NX * NY * 8];
      for (y = 0; y < NY; y++) //=========================
      {
        for (x = 0; x < NX; x++) // =====================
        {
          sum = 0;
          for (c = 0; c < 3; c++) sum += inp.Grid[c + 3 * (x + NX * y)];
          Grid[y * NX + x] = (byte)(sum / 3);
        } // ========== for (x.  ====================
      }
      return 1;
    } //********************** end ColorToGray **********************


    public int ColorToGray(CImage inp)
    /* Transforms the colors of the color image "inp" in luminance=(r+g+b)/3 
    and puts these values to this.Grid. --------- */
    {
      int c, sum, x, y;
      if (inp.N_Bits != 24) return -1;
      N_Bits = 8; NX = inp.NX; NY = inp.NY;
      Grid = new byte[NX * NY * 8];
      for (y = 0; y < NY; y++) //=========================
      {
        for (x = 0; x < NX; x++) // =====================
        {
          sum = 0;
          for (c = 0; c < 3; c++) sum += inp.Grid[c + 3 * (x + NX * y)];
          Grid[y * NX + x] = (byte)(sum / 3);
        } // ========== for (x.  ====================
      }
      return 1;
    } //********************** end ColorToGray **********************


    public byte MaxC(byte R, byte G, byte B)
    {
      int max;
      if (0.713 * R > G) max = (int)(0.713 * R);
      else max = G;
      if (0.527 * B > max) max = (int)(0.527 * B);
      return (byte)max;
    }

 

    public int ColorToGrayMC(CImage inp, Form1 fm1)
    /* Transforms the colors of the color image "inp" in lightness=MaxC(R, G, B) 
    and puts these values to this.Grid. --------- */
    {
      int gv, x, y;
      if (inp.N_Bits != 24) return -1;
      N_Bits = 8; NX = inp.NX; NY = inp.NY;
      Grid = new byte[NX * NY * 8];
      for (y = 0; y < NY; y++) //=========================
      {
        for (x = 0; x < NX; x++) // =====================
        {
          gv = MaxC(inp.Grid[2 + 3 * (x + NX * y)],
                    inp.Grid[1 + 3 * (x + NX * y)],
                    inp.Grid[0 + 3 * (x + NX * y)]);
          Grid[y * NX + x] = (byte)gv;
        } // ========== for (x.  ====================
      }
       return 1;
    } //********************** end ColorToGrayMC **********************


  }
}
