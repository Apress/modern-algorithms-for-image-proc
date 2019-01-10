using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace WFsegmentAndComp
{
  public class CImage
  {
    public byte[] Grid;
    public Color[] Palette;
    public int width, height, N_Bits;
    public int nLoop, denomProg;

    public CImage() { } // default constructor

    public CImage(int nx, int ny, int nbits) // constructor
    {
      this.width = nx;
      this.height = ny;
      this.N_Bits = nbits;
      Grid = new byte[width * height * (N_Bits / 8)];
      Palette = new Color[256];
    }

    public CImage(int nx, int ny, int nbits, byte[] img) // constructor
    {
      this.width = nx;
      this.height = ny;
      this.N_Bits = nbits;
      Grid = new byte[width * height * (N_Bits / 8)];
      //Palette = new byte[1024];
      Palette = new Color[256];
      for (int i = 0; i < width * height * N_Bits / 8; i++) this.Grid[i] = img[i];
    }

    public CImage(int nx, int ny, int nbits, byte[] img, Color[] Palet) // constructor
    {
      this.width = nx;
      this.height = ny;
      this.N_Bits = nbits;
      Grid = new byte[width * height * (N_Bits / 8)];
      //Palette = new byte[1024];
      Palette = new Color[256];
      for (int i = 0; i < width * height * N_Bits / 8; i++) this.Grid[i] = img[i];
      for (int j = 0; j < 1024; j++) Palette[j] = Palet[j];
    }


    public void Copy(CImage inp)
    {
      if (N_Bits != inp.N_Bits)
      {
        MessageBox.Show("Copy: The formats of input and output are not equal. input.N_Bits="
        + inp.N_Bits + "; ouput.N_Bits=" + N_Bits);
        return;
      }
      width = inp.width;
      height = inp.height;
      N_Bits = inp.N_Bits;
      if (N_Bits == 8)
        for (int i=0; i<256; i++) Palette[i] = inp.Palette[i];
      for (int i = 0; i < width * height * N_Bits / 8; i++)
        Grid[i] = inp.Grid[i];
    }

    public byte MaxC(byte R, byte G, byte B)
    {
      byte light = G;
      if (0.713 * R > light) light = (byte)(0.713 * R);
      if (0.527 * B > light) light = (byte)(0.527 * B);
      return light;
    }


    public int ColorToGray(CImage inp, Form1 fm1)
    /* Transforms the colors of the color image "inp" in luminance=(r+g+b)/3 
    and puts these values to this.Grid. --------- */
    {
      if (N_Bits != 8)
      {
        MessageBox.Show("ColorToGray: The output image has a not suitable format; N_Bits=" + N_Bits);
        return -1;
      }
      if (inp.N_Bits != 24)
      {
        MessageBox.Show("ColorToGray: The input image has a not suitable format; inp.N_Bits=" + inp.N_Bits);
        return -1;
      }
      int x, y;
      if (inp.N_Bits != 24) return -1;
      fm1.progressBar1.Value = 0;
      fm1.progressBar1.Visible = true;
      N_Bits = 8; width = inp.width; height = inp.height;
      Grid = new byte[width * height * 8];
      int y1 = 1 + nLoop * height / denomProg;
      for (y = 0; y < height; y++) //=================================================
      {
        if ((y % y1) == 0) fm1.progressBar1.PerformStep();
        for (x = 0; x < width; x++) // =============================================
        {
          Grid[y * width + x] = (byte)MaxC(inp.Grid[2 + 3 * (x + width * y)],
            inp.Grid[1 + 3 * (x + width * y)], inp.Grid[0 + 3 * (x + width * y)] );
        } // ================= end for (x.  ========================================
      } // =================== end for (x.  ==========================================
      return 1;
    } //********************** end ColorToGray *****************************************


    public int MakePalette(ref int nPal)
    {
      int r, g, b;
      byte Red, Green, Blue;
      int ii = 0;
      for (r = 1; r <= 8; r++) // Colors of the palette
        for (g = 1; g <= 8; g++)
          for (b = 1; b <= 4; b++)
          {
            Red = (byte)(32 * r); if (r == 8) Red = 255;
            Green = (byte)(32 * g); if (g == 8) Green = 255;
            Blue = (byte)(64 * b); if (b == 4) Blue = 255;
            Palette[ii] = Color.FromArgb(Red, Green, Blue);
            ii++;
          }
      nPal = 4 * ii;
      return 1;
    } //************************************ end MakePalette ******************************


    public int DeleteBit0(int nbyte)
    // Sets the bits 0 and 1 of a 8 bit image to 0 and returns a 1.
    // If the image is not an 8 bit one, does nothing and returns -1.
    {
      for (int i = 0; i < width * height; i++)
        if (nbyte == 1)
          Grid[i] &= 252;
        else
        {
          Grid[nbyte * i + 2] = (byte)(Grid[nbyte * i + 2] & 254);
          Grid[nbyte * i + 1] = (byte)(Grid[nbyte * i + 1] & 254);
        }
      return 1;
    }
  }
}
