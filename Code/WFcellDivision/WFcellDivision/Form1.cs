using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WFcellDivision
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }

    private Bitmap OrigBmp;

    private Bitmap bmp;
    CImage OrigIm;  // copy of original image
    Color[] Palet;
    int nbyte; 
    
    public double Scale1;
    public int marginX, marginY; 

    private void button1_Click(object sender, EventArgs e) // Open image
    {
      MessageBox.Show("Open the file 'DNA.bmp' in directory 'WFcellDivision'");
      label1.Visible = false;
      OpenFileDialog openFileDialog1 = new OpenFileDialog();
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        try
        {
          OrigBmp = new Bitmap(openFileDialog1.FileName);
        }
        
        catch (Exception ex)
        {
          MessageBox.Show("Error: Could not read file from disk. Original error: " +
           ex.Message);
        }
      }
      else return;

      byte[] Grid;
      Palet = new Color[256];
      for (int k = 0; k < 256; k++)
      {
        Palet[k] = OrigBmp.Palette.Entries[k];
      }

      if (OrigBmp.PixelFormat == PixelFormat.Format24bppRgb)
      {
        Grid = new byte[3 * OrigBmp.Width * OrigBmp.Height]; // color image
        nbyte = 3;
        BitmapToGrid(OrigBmp, Grid);
      }
      else
        if (OrigBmp.PixelFormat == PixelFormat.Format8bppIndexed)
        {
          Grid = new byte[OrigBmp.Width * OrigBmp.Height]; // indexed image
          nbyte = 1;
          BitmapToGrid(OrigBmp, Grid);
        }
        else
        {
        MessageBox.Show("Not suitable pixel format=" + OrigBmp.PixelFormat);
        return;
      }

      bmp = new Bitmap(63, 63, PixelFormat.Format24bppRgb);
      pictureBox1.Image = OrigBmp;
      label1.Text = "Contents of the DNA";
      label1.Visible = true;
      label2.Visible = false;

      OrigIm = new CImage(OrigBmp.Width, OrigBmp.Height, nbyte);
      BitmapToGrid(OrigBmp, OrigIm.Grid);
      double ScaleX = (double)pictureBox1.Width / (double)OrigIm.width;
      double ScaleY = (double)pictureBox1.Height / (double)OrigIm.height;
      if (ScaleX < ScaleY) Scale1 = ScaleX;
      else Scale1 = ScaleY;
      marginX = (pictureBox1.Width - (int)(Scale1 * OrigIm.width)) / 2;
      marginY = (pictureBox1.Height - (int)(Scale1 * OrigIm.height)) / 2;
    } //************************* end Open image **********************************


    public int MessReturn(string s)
    {
      if (MessageBox.Show(s, "Return", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
        return -1;
      return 1;
    }


    public int GrowNew(CCelln[] Org, int NX, int NY, ref int iPair)
    {
      int i, k, x = (NX - 1) / 2, y = (NY - 1) / 2, nCount = 1, X = 31, Y = 31;
      int dir = 0, index, iStep, nStep = 1, nPair = 63, width = 63;
      int[] Array = new int[NX * NY];
      for (i = 0; i < NX * NY; i++) Array[i] = 0;
      int cnt = 0;
      Array[x + NX * y]++;
      Org[x + NX * y].Property = Org[x + NX * y].DNA[X + NX * Y];

      for (iPair = 0; iPair < nPair; iPair++) //==================================
      {
        for (iStep = 0; iStep < 2 * nStep; iStep++) //==============================
        {
          switch (dir)
          {
            case 0: x += 1; X = Org[x + NX * y].X = Org[x - 1 + NX * y].X + 1; // it was 2
                            Y = Org[x + NX * y].Y = Org[x - 1 + NX * y].Y;
              for (k = 0; k < NX * NY; k++) Org[x + NX * y].DNA[k] = Org[x - 1 + NX * y].DNA[k];
              Org[x + NX * y].Property = Org[x + NX * y].DNA[X + NX * Y];
              Array[x + NX * y]++;
                          break;
            case 1: y += 1; X = Org[x + NX * y].X = Org[x + NX * (y - 1)].X;
                            Y = Org[x + NX * y].Y = Org[x + NX * (y - 1)].Y + 1;
              for (k = 0; k < NX * NY; k++) Org[x + NX * y].DNA[k] = Org[x + NX * (y - 1)].DNA[k];
              Org[x + NX * y].Property = Org[x + NX * y].DNA[X + NX * Y];
              Array[x + NX * y]++;
                            break;
            case 2: x -= 1; X = Org[x + NX * y].X = Org[x + 1 + NX * y].X - 1;
                            Y = Org[x + NX * y].Y = Org[x + 1 + NX * y].Y;
                for (k = 0; k < NX * NY; k++) Org[x + NX * y].DNA[k] = Org[x + 1 + NX * y].DNA[k];
                Org[x + NX * y].Property = Org[x + NX * y].DNA[X + NX * Y];
                Array[x + NX * y]++;
                           break;
            case 3: y -= 1; X = Org[x + NX * y].X = Org[x + NX * (y + 1)].X;
                            Y = Org[x + NX * y].Y = Org[x + NX * (y + 1)].Y - 1;
              for (k = 0; k < NX * NY; k++) Org[x + NX * y].DNA[k] = Org[x + NX * (y + 1)].DNA[k];
              Org[x + NX * y].Property = Org[x + NX * y].DNA[X + NX * Y];
              Array[x + NX * y]++;
                           break;
          }
          index = x + width * y;
          if (iStep == nStep - 1)
          {
            dir++;
            if (dir == 4) dir = 0;
          }
        
          cnt += nCount;
          if (cnt > 50 && cnt < 500 && (cnt % 80) == 0 || cnt > 500 && (cnt % 500) == 0) 
          {
            label2.Visible = true;
            if (MessReturn("Animation is running. Click 'CR'") < 0) return -1;
            int gv, xx, yy;
            CImage Image2 = new CImage(63, 63, 1);
            for (int ii = 0; ii < 63 * 63; ii++) Image2.Grid[ii] = 37;
            for (yy = 0; yy < NY; yy++)
              for (xx = 0; xx < NX; xx++)
              {
                i = xx + NX * yy;
                gv = Org[xx + NX * yy].Property;
                if (xx > 30 + 33 * (NY - yy) / 40 || xx < 20 * (48 - NY + yy) / 48) gv = 37;

                Image2.Grid[xx + NX * yy] = (byte)gv;
              }
            ImageToBitmapOld(Image2, Palet, 1, bmp);
            pictureBox2.Image = bmp;

          }
          if (cnt == NX * NY) break;

        } //==================== end for (iStep ... =====================
        dir++;
        if (dir == 4) dir = 0;
        nStep++;
      } //======================= end for (iPair ... ========================
      return 1;
    } //*************************** end GrowNew ************************************


    public int Grow(CCelln[] Org, int NX, int NY, ref int cnt)
    {
      int i, k, x = (NX - 1) / 2, y = (NY - 1) / 2, nCount = 1, X = 31, Y = 31;
      int[] Array = new int[NX * NY];
      for (i = 0; i < NX * NY; i++) Array[i] = 0;
      cnt = 0;
      Array[x + NX * y]++;
      do
      {
        for (i = 0; i < nCount && x < NX - 1; i++)
        {
          x++;
          X = Org[x + NX * y].X = Org[x - 1 + NX * y].X + 1;
          Y = Org[x + NX * y].Y = Org[x - 1 + NX * y].Y;
          for (k = 0; k < NX * NY; k++) Org[x + NX * y].DNA[k] = Org[x - 1 + NX * y].DNA[k];
          Org[x + NX * y].Property = Org[x + NX * y].DNA[X + NX * Y];
          Array[x + NX * y]++;
        }
        cnt += nCount;
        if (cnt > 500 && (cnt % 1000) > 200 && (cnt % 1000) < 500 && false)
        {
          int gv, xx, yy;
          CImage Image2 = new CImage(63, 63, 1);
          for (yy = 0; yy < NY; yy++)
            for (xx = 0; xx < NX; xx++)
            {
              i = xx + NX * yy;
              gv = Org[xx + NX * yy].Property;
              if (xx > 30 + 33 * (NY - yy) / 40 || xx < 20 * (48 - NY + yy) / 48) gv = 37;

              Image2.Grid[x + NX * y] = (byte)gv;
            }
          ImageToBitmapOld(Image2, Palet, 1, bmp);
          pictureBox2.Image = bmp;

        }
        if (cnt == NX * NY) break;

        for (i = 0; i < nCount && y < NY - 1; i++)
        {
          y++;
          X = Org[x + NX * y].X = Org[x + NX * (y - 1)].X;
          Y = Org[x + NX * y].Y = Org[x + NX * (y - 1)].Y + 1;
          for (k = 0; k < NX * NY; k++) Org[x + NX * y].DNA[k] = Org[x + NX * (y - 1)].DNA[k];
          Org[x + NX * y].Property = Org[x + NX * y].DNA[X + NX * Y];
          Array[x + NX * y]++;
        }
        cnt += nCount;
        if (cnt == NX * NY) break;

        nCount++;
        for (i = 0; i < nCount && x > 0; i++)
        {
          x--;
          X = Org[x + NX * y].X = Org[x + 1 + NX * y].X - 1;
          Y = Org[x + NX * y].Y = Org[x + 1 + NX * y].Y;
          for (k = 0; k < NX * NY; k++) Org[x + NX * y].DNA[k] = Org[x + 1 + NX * y].DNA[k];
          Org[x + NX * y].Property = Org[x + NX * y].DNA[X + NX * Y];
          Array[x + NX * y]++;
        }
        cnt += nCount;
        if (cnt == NX * NY) break;

        for (i = 0; i < nCount && y > 0; i++)
        {
          y--;
          X = Org[x + NX * y].X = Org[x + NX * (y + 1)].X;
          Y = Org[x + NX * y].Y = Org[x + NX * (y + 1)].Y - 1;
          for (k = 0; k < NX * NY; k++) Org[x + NX * y].DNA[k] = Org[x + NX * (y + 1)].DNA[k];
          Org[x + NX * y].Property = Org[x + NX * y].DNA[X + NX * Y];
          Array[x + NX * y]++;
        }
        cnt += nCount;
        if (cnt == NX * NY) break;
        nCount++;
      } while (true);
      return 1;
    } //*************************** end Grow ************************************


    private void button2_Click(object sender, EventArgs e) // Start
    {
      int CNX = 63, CNY = 63;
      CCelln[] Org = new CCelln[CNX * CNY];
      for (int q = 0; q < CNX * CNY; q++) Org[q] = new CCelln(CNX, CNY);
      CImage Image1 = new CImage(CNX, CNY, 1);
      Image1.Copy(OrigIm);
      int i, x, y;
      // Initializing "Org":
      for (y = 0; y < CNY; y++)
        for (x = 0; x < CNX; x++)
        {
          Org[x + CNX * y].Property = -1;
          Org[x + CNX * y].X = -1;
          Org[x + CNX * y].Y = -1;
          for (i = 0; i < CNX * CNY; i++) Org[x + CNX * y].DNA[i] = 0;
        }
      // Initializing the central cell:
      x = (CNX - 1) / 2; y = (CNY - 1) / 2; // central cell
      Org[x + CNX * y].Property = OrigIm.Grid[x + CNX * y];
      for (i = 0; i < CNX * CNY; i++) Org[x + CNX * y].DNA[i] = OrigIm.Grid[i];
      Org[x + CNX * y].X = x;
      Org[x + CNX * y].Y = y;

      ImageToBitmapOld(OrigIm, Palet, 1, bmp);
      //pictureBox2.Image = bmp;
      int iPair = -1, gv;
      GrowNew(Org, CNX, CNY, ref iPair);

      for (y=0; y<CNY; y++)
        for (x = 0; x < CNX; x++)
        {
          i = x + CNX * y;
          gv = Org[x + CNX * y].Property;
          if (x > 30 + 33 * (CNY - y) / 40 || x < 20 * (48 - CNY + y) / 48) gv = 37;

          Image1.Grid[x + CNX * y] = (byte)gv;
        }
      ImageToBitmapOld(Image1, Palet, 1, bmp);
      label2.Text = "Adult organism";
      pictureBox2.Image = bmp;
    } //************************* end Start **********************************


    private void ImageToBitmap(CImage Image, int[] Palet, int nbyteIm, Bitmap bmp)
    {
      Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
      BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
      int nbyteBmp = 1;
      switch (bmp.PixelFormat)
      {
        case PixelFormat.Format24bppRgb: nbyteBmp = 3; break;
        case PixelFormat.Format8bppIndexed: nbyteBmp = 1; break;
      }
      IntPtr ptr = bmpData.Scan0;
      int size = bmp.Width * bmp.Height;
      int length = Math.Abs(bmpData.Stride) * bmp.Height;
      byte[] rgbValues = new byte[length];

      Color color;
      for (int y = 0; y < bmp.Height; y++)
      {
        for (int x = 0; x < bmp.Width; x++)
        {
          if (nbyteBmp == 1)  // nbyteBmp is defined by the PixelFormat of "bmp"
          {
            if (nbyteIm == 3)
              color = bmp.Palette.Entries[Image.Grid[3 * (x + bmp.Width * y)]]; // Grid is colore
            else
            {
              color = bmp.Palette.Entries[Image.Grid[x + bmp.Width * y]]; // Grid is grayscale
              rgbValues[x + Math.Abs(bmpData.Stride) * y + 0] = color.B;
              rgbValues[x + Math.Abs(bmpData.Stride) * y + 1] = color.G;
              rgbValues[x + Math.Abs(bmpData.Stride) * y + 2] = color.R;
            }
          }
          else // nbyteBmp==3

            for (int c = 0; c < nbyteBmp; c++)
            {
              if (nbyteIm == 3)
                rgbValues[c + nbyteBmp * x + Math.Abs(bmpData.Stride) * y] =
                                      Image.Grid[c + nbyteBmp * (x + bmp.Width * y)];
              else
              {
                int Col = Palet[Image.Grid[x + bmp.Width * y]];  // Image is indexed
                rgbValues[3 * x + Math.Abs(bmpData.Stride * y) + 0] = (byte)((Col >> 16) & 0XFF);
                rgbValues[3 * x + Math.Abs(bmpData.Stride * y) + 1] = (byte)((Col >> 8) & 0XFF); ;
                rgbValues[3 * x + Math.Abs(bmpData.Stride * y) + 2] = (byte)(Col & 0XFF); ;
              }
            }
        }
      }
      System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, length);
      bmp.UnlockBits(bmpData);
    } //****************************** end ImageToBitmap ****************************************


    private void ImageToBitmapOld(CImage Image, Color[] Palet, int nbyteIm, Bitmap bmp)
    {
      int nbyteBmp = 3;
      switch (bmp.PixelFormat)
      {
        case PixelFormat.Format24bppRgb: nbyteBmp = 3; break;
        case PixelFormat.Format8bppIndexed: nbyteBmp = 1; break;
      }
      int size = bmp.Width * bmp.Height;

      Color color;
      for (int y = 0; y < bmp.Height; y++)
      {
        for (int x = 0; x < bmp.Width; x++)
        {
          if (nbyteBmp == 3)  // nbyteBmp is defined by the PixelFormat of "bmp"
          {
            if (nbyteIm == 3)
              color = bmp.Palette.Entries[Image.Grid[3 * (x + bmp.Width * y)]]; // Grid is colore
            else
            {
              color = Palet[Image.Grid[x + bmp.Width * y]]; // Grid is grayscale
              //ReliaseHdc
              bmp.SetPixel(x, y, color);
            }
          }
          else return;

        }
      }
    } //****************************** end ImageToBitmapOld ****************************************




    private void BitmapToGrid(Bitmap bmp, byte[] Grid)
    {
      Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
      BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
      int nbyteBmp;
      switch (bmp.PixelFormat)
      {
        case PixelFormat.Format24bppRgb: nbyteBmp = 3; break;
        case PixelFormat.Format8bppIndexed: nbyteBmp = 1; break;
        default: MessageBox.Show("BitmapToGrid: Inappropriate pixel format=" + bmp.PixelFormat); return;
      }
      IntPtr ptr = bmpData.Scan0;
      int length = Math.Abs(bmpData.Stride) * bmp.Height;
      byte[] rgbValues = new byte[length];
      System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, length);

      for (int y = 0; y < bmp.Height; y++)
      {
        for (int x = 0; x < bmp.Width; x++)
        {
          if (nbyteBmp == 3)  // nbyte is global according to the PixelFormat of "bmp"
          {
            Color color = bmp.Palette.Entries[rgbValues[x + Math.Abs(bmpData.Stride) * y]]; 
            Grid[3 * (x + bmp.Width * y) + 0] = color.B;
            Grid[3 * (x + bmp.Width * y) + 1] = color.G;
            Grid[3 * (x + bmp.Width * y) + 2] = color.R;
          }
          else
            for (int c = 0; c < nbyte; c++)
            {
              Grid[c + nbyte * (x + bmp.Width * y)] = rgbValues[c + nbyte * x + Math.Abs(bmpData.Stride) * y];
            }
        }
      }
      bmp.UnlockBits(bmpData);
    } //****************************** end BitmapToGrid ****************************************
  }  //************************** end Form1 ****************************************

  public class CImage
  {
    public int width, height, nbyte;
    public byte[] Grid;
    public CImage(int Width, int Height, int Nbyte) // constructor
    {
      width = Width;
      height = Height;
      nbyte = Nbyte;
      Grid = new byte[nbyte*width*height];
    }
    public void Copy(CImage Img)
    {
      for (int i = 0; i < width * height * nbyte; i++ ) Grid[i] = Img.Grid[i];
      return;
    }
  } //***************** end class CImage *********************

  public  class CCelln
  { public
      int X, Y, Property;
      public byte[] DNA;
      public CCelln(int width, int heigh) // constructor
      {
        DNA = new byte[width*heigh];
      }
  }

}
