using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PulseNoiseWF
{
  class CImage
  { public Byte[] Grid;
    public int width, height, nBits;
 
    public CImage(int nx, int ny, int nbits) // constructor
    {
        this.width = nx;
        this.height = ny;
        this.nBits = nbits;
        Grid = new byte[width * height * (nBits / 8)];
    }

    public CImage(int nx, int ny, int nbits, byte[] img) // constructor
    {
        this.width = nx;
        this.height = ny;
        this.nBits = nbits;
        this.Grid = new byte[width * height * (nBits / 8)];
        for (int i = 0; i < width * height * nBits / 8; i++) this.Grid[i] = img[i];
    }

    public void Copy(CImage inp)
    {
        width = inp.width;
        height = inp.height;
        nBits = inp.nBits;
        for (int i = 0; i < width * height * nBits / 8; i++)
            Grid[i] = inp.Grid[i];
    }

    public int ColorToGray(CImage inp, Form1 fm1)
    /* Transforms the colors of the color image "inp" in lightness=(r+g+b)/3 
       and puts these values to this.Grid. --------- */
    {
      int c, sum, x, y;
      if (inp.nBits != 24) return -1;
      fm1.progressBar1.Value = 0;
      fm1.progressBar1.Step = 1;
      fm1.progressBar1.Visible = true;
      nBits = 8; width = inp.width; height = inp.height;
      Grid = new byte[width * height * 8];
      for (y = 0; y < height; y++) //=========================
      {
        fm1.progressBar1.PerformStep();
        for (x = 0; x < width; x++) // =====================
        {
          sum = 0;
          for (c = 0; c < 3; c++) sum += inp.Grid[c + 3 * (x + width * y)];
          Grid[y * width + x] = (byte)(sum / 3);
        } // ========== for (x.  ====================
      }
      fm1.progressBar1.Visible = false;
      return 1;
    } //********************** end ColorToGray **********************


    public int ColorToGray(CImage inp)
    /* Transforms the colors of the color image "inp" in lightness=(r+g+b)/3 
       and puts these values to this.Grid. --------- */
    {
      int sum, x, y, c;
      if (inp.nBits != 24) return -1;
      nBits = 8; width = inp.width; height = inp.height;
      Grid = new byte[width * height * 8];
      for (y = 0; y < height; y++) //=========================
      {
        for (x = 0; x < width; x++) // =====================
        {
          sum = 0;
          for (c = 0; c < 3; c++) sum += inp.Grid[c + 3 * (x + width * y)];
          Grid[y * width + x] = (byte)(sum / 3);
        } // ========== for (x.  ====================
      }
      return 1;
    } //********************** end ColorToGray **********************

    public int DeleteBit0(int nbyte)
    // If "this" is a 8 bit image, then sets the bits 0 and 1 of each pixel to 0.
    // If it is a 24 bit one, then sets the bit 0 of green and red chanels to 0.
    { for (int i = 0; i < width * height; i++)
        if (nbyte == 1)
            Grid[i] = (byte)(Grid[i] - (Grid[i] % 4));
        else
        {
            Grid[nbyte * i + 2] = (byte)(Grid[nbyte * i + 2] & 254);
            Grid[nbyte * i + 1] = (byte)(Grid[nbyte * i + 1] & 254);
        }
        return 1;
    } //********************* end DeleteBit0 ************************   

  } //************************ end class CImage ************************

} //************************** end namespace ******************************
