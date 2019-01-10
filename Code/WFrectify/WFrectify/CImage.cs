using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace WFrectify
{
  public class CImage
  { 
    public
    byte[] Grid; 
    public
    int width, height, N_Bits, nLoop, denomProg;
		Color[] Palette;

    public CImage(int nx, int ny, int nbits) // constructor
    { 
      width = nx;
      height = ny;
      N_Bits = nbits;
      Palette = new Color[256];
      
      Grid=new byte[width*height*(N_Bits/8)];
    } 

    public CImage(int nx, int ny, int nbits, byte[] img) // constructor
    { 
      width = nx;
      height = ny;
      N_Bits = nbits;
      Palette = new Color[256];
      
      Grid=new byte[width*height*(N_Bits/8)];
      for (int i=0; i<width*height*N_Bits/8; i++) Grid[i]=img[i];
    }

    public void Copy(CImage image, int full)
    {
      width = image.width;
      height = image.height;
      N_Bits = image.N_Bits;
      Grid = new byte[width*height*(N_Bits/8)];
      for (int i = 0; i < 256; i++) Palette[i] = image.Palette[i];

      if (full == 1)
        for (int i = 0; i < width * height * (N_Bits / 8); i++)
          Grid[i] = image.Grid[i];
    }

    public void BitmapToImage(Bitmap bmp, Form1 fm1)
    {
      int nbyteBmp, nbyteIm;
      nbyteIm = N_Bits / 8;
      Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
      BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
      switch (bmp.PixelFormat)
      {
        case PixelFormat.Format24bppRgb: nbyteBmp = 3; break;
        case PixelFormat.Format8bppIndexed: nbyteBmp = 1; break;
        default: MessageBox.Show("BitmapToGrid: Inappropriate pixel format=" + bmp.PixelFormat); return;
      }
      IntPtr ptr = bmpData.Scan0;
      int Str = bmpData.Stride;
      int bytes = Math.Abs(bmpData.Stride) * bmp.Height; // es war ohne *3
      byte[] rgbValues = new byte[bytes];
      System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

      fm1.progressBar1.Visible = true;
      int y1 = bmp.Height / 100;

      for (int y = 0; y < bmp.Height; y++)
      {
        if (y % y1 == 0) fm1.progressBar1.PerformStep();

        for (int x = 0; x < bmp.Width; x++)
        {
          if (nbyteBmp == 1)
          {
            Color color = bmp.Palette.Entries[rgbValues[x + Math.Abs(bmpData.Stride) * y]];
            if (nbyteIm == 1)
              Grid[x + bmp.Width * y] = color.R;
            else
            {
              Grid[3 * (x + bmp.Width * y) + 0] = color.B;
              Grid[3 * (x + bmp.Width * y) + 1] = color.G;
              Grid[3 * (x + bmp.Width * y) + 2] = color.R;
            }
          }
          else // nbyteBmp == 3
          {
            if (nbyteIm == 1)
              Grid[x + bmp.Width * y] = rgbValues[2 + nbyteBmp * x + Math.Abs(bmpData.Stride) * y];
            else // nbyteIm == 3
              for (int c = 0; c < nbyteBmp; c++)
                Grid[c + nbyteIm * (x + bmp.Width * y)] =
                  rgbValues[c + nbyteBmp * x + Math.Abs(bmpData.Stride) * y];
          }
        }
      }
      bmp.UnlockBits(bmpData);
    } //****************************** end BitmapToImage ****************************************

    public byte MaxC(byte R, byte G, byte B)
    {
      byte light = G;
      if (0.713 * R > G) light = (byte)(0.713 * R);
      if (0.527 * B > G) light = (byte)(0.527 * B);
      return light;
    }

    public void BitmapToImageOld(Bitmap bmp, Form1 fm1)
    {
      int nbyteIm = N_Bits / 8;
      if (bmp.PixelFormat != PixelFormat.Format24bppRgb &&
            bmp.PixelFormat != PixelFormat.Format8bppIndexed)
      {
        MessageBox.Show("BitmapToGridOld: Not suitable pixel format=" + bmp.PixelFormat);
        return;
      }

      Color color;
      for (int y = 0; y < bmp.Height; y++)
      {
        int y1 = nLoop * bmp.Height / denomProg;
        if (y % y1 == 0) fm1.progressBar1.PerformStep();

        for (int x = 0; x < bmp.Width; x++)
        {
          int i = x + width * y;
          color = bmp.GetPixel(x, y);
          if (nbyteIm == 1)
            Grid[i] = MaxC(color.R, color.G, color.B);
          else // nbyteIm == 3
            for (int c = 0; c < nbyteIm; c++)
            {
              if (c == 0) Grid[nbyteIm * i] = color.B;
              if (c == 1) Grid[nbyteIm * i + 1] = color.G;
              if (c == 2) Grid[nbyteIm * i + 2] = color.R;
            }
        }
      }
    } //****************************** end BitmapToImageOld ****************************************


    public void ImageToBitmap(Bitmap bmp, Form1 fm1)
    {
      int nbyteBmp = 0, nbyteIm = N_Bits / 8;
      Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
      BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
      switch (bmp.PixelFormat)
      {
        case PixelFormat.Format24bppRgb: nbyteBmp = 3; break;
        case PixelFormat.Format8bppIndexed: nbyteBmp = 1; break;
        default: MessageBox.Show("ImageToBitmap: Not suitable pixel format=" + bmp.PixelFormat); return;
      }
      IntPtr ptr = bmpData.Scan0;
      int size = bmp.Width * bmp.Height;
      int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
      byte[] rgbValues = new byte[bytes];
      byte light = 0; 
      Color color;
      int index = 0;
      fm1.progressBar1.Visible = true;
      int y1 = nLoop * bmp.Height / 100; // denomProg;
      for (int y = 0; y < bmp.Height; y++)
      {
        if (y % y1 == 0) fm1.progressBar1.PerformStep();
        for (int x = 0; x < bmp.Width; x++)
        {
          if (nbyteBmp == 1)  // nbyte of bmp;
          {
            if (nbyteIm == 1) // nbyte of image;
            {
              color = (Color)Palette[Grid[x + bmp.Width * y]];
               rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 0] = color.B;
                rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 1] = color.G;
                rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 2] = color.R;
            }
            else // nbyteBmp == 1; nbyteIm == 3
            {
              color = bmp.Palette.Entries[Grid[3 * (x + bmp.Width * y)]];
              rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 0] = color.B;
              rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 1] = color.G;
              rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 2] = color.R;
            }
          }
          else  // nbyteBmp == 3
          {
            if (nbyteIm == 1)
            {
              light = Grid[x + width * y];
              {
                index = 3 * x + Math.Abs(bmpData.Stride) * y; 
                rgbValues[index + 0] = light; // color.B; 
                rgbValues[index + 1] = light; // color.G;
                rgbValues[index + 2] = light; // color.R; 
              }
            }
            else // nbyteBmp == 3; nbyteIm == 3
            {
              for (int c = 0; c < nbyteBmp; c++)
              {
                rgbValues[c + nbyteBmp * x + Math.Abs(bmpData.Stride) * y] =
                                  Grid[c + nbyteIm * (x + bmp.Width * y)];
              }
            } //------------------ end if (nbyteIm ==1) ------------------------
          } //-------------------- end if (nbyte == 1) ----------------------------
        }
      }
      System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
      bmp.UnlockBits(bmpData);
    } //****************************** end ImageToBitmap ****************************************


    public void ImageToBitmapOld(Bitmap bmp, Form1 fm1)
    {
      int nbyteBmp, nbyteIm = N_Bits / 8;
      int light = 0;
      switch (bmp.PixelFormat)
      {
        case PixelFormat.Format24bppRgb: nbyteBmp = 3; break;
        case PixelFormat.Format8bppIndexed: nbyteBmp = 1; break;
        default: MessageBox.Show("ImageToBitmap: Not suitable  pixel format=" + bmp.PixelFormat); return;
      }

      Color color;
      int y1 = nLoop * bmp.Height / 50; 
      for (int y = 0; y < bmp.Height; y++)
      {
        if ((y + 1) % y1 == 0) fm1.progressBar1.PerformStep();
        for (int x = 0; x < bmp.Width; x++)
        {
          if (nbyteBmp == 1)  // nbyte of bmp;
          {
            if (nbyteIm == 1) // nbyte of image;
            {
              light = (int)Grid[x + bmp.Width * y];
              bmp.SetPixel(x, y, Color.FromArgb(light, light, light));
            }
            else // nbyteBmp == 1; nbyteIm == 3
            {
              bmp.SetPixel(x, y, Color.FromArgb(Grid[nbyteIm * (x + width * y) + 2],
                  Grid[nbyteIm * (x + width * y) + 1], Grid[nbyteIm * (x + width * y) + 0]));
            }
          }
          else  // nbyte == 3
          {
            if (nbyteIm == 1)
            {
              light = Grid[x + bmp.Width * y];
              color = Color.FromArgb(light, light, light);
              bmp.SetPixel(x, y, Color.FromArgb(light, light, light));
            }
            else //nbyteIm ==3; nbyte == 3
            {
              bmp.SetPixel(x, y, Color.FromArgb(Grid[nbyteIm * (x + width * y) + 2],
                  Grid[nbyteIm * (x + width * y) + 1], Grid[nbyteIm * (x + width * y) + 0]));
            } //------------------ end if (nbyteIm ==1) ------------------------
          } //-------------------- end if (nbyte == 1) ----------------------------
        }
      }
    } //****************************** end ImageToBitmapOld ****************************************


    private void Optimization(double F, Point[] v, double CX, double CY, ref double OptCX, ref double OptCY)
    {
      double A = 0.0, B, C, D, Det, E, G;
      double[] xc = new double[4];
      double[] yc = new double[4];
      double[] zc = new double[4];

      double[] xopt = new double[4];
      double[] yopt = new double[4];
      double[] zopt = new double[4];

      double dev1, dev2, dev3, Crit;
      double MinCrit = 10000000.0;
      int OptIterX = -1, OptIterY = -1, IterY;
      OptCX = 0.0;
      OptCY = 0.0;
 
      CX -= 0.40; CY -= 0.40;
      double CX0 = CX, Step = 0.08;
      for (IterY = 0; IterY < 11; IterY++)
      {
        CX = CX0;
        for (int IterX = 0; IterX < 11; IterX++)
        {
          for (int i = 0; i < 4; i++)
          {
            A = B = C = D = 0.0;
            A = (F / (v[i].X - width / 2) + CX); 
            B = CY;
            C = width / 2 * F / (v[i].X - width / 2) + CX * width / 2 + CY * height / 2 + F;
            D = CX;
            E = (F / (v[i].Y - height / 2) + CY);
            G = height / 2 * F / (v[i].Y - height / 2) + CX * width / 2 + CY * height / 2 + F;
            Det = A * E - B * D;
            xc[i] = (C * E - B * G) / Det;
            yc[i] = (A * G - C * D) / Det;
            zc[i] = F - CX * (xc[i] - width / 2) - CY * (yc[i] - height / 2);
          }
          dev1 = dev2 = dev3 = 0.0;

          dev1 = ((xc[0] - xc[1]) * (xc[2] - xc[1]) + (yc[0] - yc[1]) * (yc[2] - yc[1]) +
            (zc[0] - zc[1]) * (zc[2] - zc[1])) /
            Math.Sqrt(Math.Pow((xc[0] - xc[1]), 2.0) + Math.Pow((yc[0] - yc[1]), 2.0) + Math.Pow((zc[0] - zc[1]), 2.0));

          dev2 =
            Math.Sqrt(Math.Pow((xc[3] - xc[2]), 2.0) + Math.Pow((yc[3] - yc[2]), 2.0) + Math.Pow((zc[3] - zc[2]), 2.0)) -
            Math.Sqrt(Math.Pow((xc[0] - xc[1]), 2.0) + Math.Pow((yc[0] - yc[1]), 2.0) + Math.Pow((zc[0] - zc[1]), 2.0));
          dev3 =
          Math.Sqrt(Math.Pow((xc[2] - xc[1]), 2.0) + Math.Pow((yc[2] - yc[1]), 2.0) + Math.Pow((zc[2] - zc[1]), 2.0)) -
          Math.Sqrt(Math.Pow((xc[3] - xc[0]), 2.0) + Math.Pow((yc[3] - yc[0]), 2.0) + Math.Pow((zc[3] - zc[0]), 2.0));

          Crit = Math.Sqrt(Math.Pow(dev1, 2.0) + Math.Pow(dev2, 2.0) + Math.Pow(dev3, 2.0));
 
          if (Crit < MinCrit)
          {
            MinCrit = Crit;
            OptIterX = IterX;
            OptIterY = IterY;
            OptCX = CX;
            OptCY = CY;
            for (int i = 0; i < 4; i++)
            {
              xopt[i] = xc[i];
              yopt[i] = yc[i];
              zopt[i] = zc[i];
            }
          }
          CX += Step;
        } //=========================== end for (int IterX... ========================
        CY += Step;
      } //============================= end for (int IterY... ========================

      CX = OptCX; CY = OptCY;
      CX -= 0.05; CY -= 0.05;
      CX0 = CX;
      double step=0.01;
      for (IterY = 0; IterY < 11; IterY++)
      {
        CX = CX0;
        for (int IterX = 0; IterX < 11; IterX++)
        {
          for (int i = 0; i < 4; i++)
          {
            A = (F / (v[i].X - width / 2) + CX); // all corrected
            B = CY;
            C = width / 2 * F / (v[i].X - width / 2) + CX * width / 2 + CY * height / 2 + F;
            D = CX;
            E = (F / (v[i].Y - height / 2) + CY);
            G = height / 2 * F / (v[i].Y - height / 2) + CX * width / 2 + CY * height / 2 + F;
            Det = A * E - B * D;
            xc[i] = (C * E - B * G) / Det;
            yc[i] = (A * G - C * D) / Det;
            zc[i] = F - CX * (xc[i] - width / 2) - CY * (yc[i] - height / 2);
          }
          dev1 = dev2 = dev3 = 0.0;

          // deviation from a 90° angle:
          dev1 = ((xc[0] - xc[1]) * (xc[2] - xc[1]) + (yc[0] - yc[1]) * (yc[2] - yc[1]) +
            (zc[0] - zc[1]) * (zc[2] - zc[1])) /
            Math.Sqrt(Math.Pow((xc[0] - xc[1]), 2.0) + Math.Pow((yc[0] - yc[1]), 2.0) + Math.Pow((zc[0] - zc[1]), 2.0));

          dev2 =  // difference |pc[3] - pc[2]| - |pc[0] - pc[1]|:
            Math.Sqrt(Math.Pow((xc[3] - xc[2]), 2.0) + Math.Pow((yc[3] - yc[2]), 2.0) + Math.Pow((zc[3] - zc[2]), 2.0)) -
            Math.Sqrt(Math.Pow((xc[0] - xc[1]), 2.0) + Math.Pow((yc[0] - yc[1]), 2.0) + Math.Pow((zc[0] - zc[1]), 2.0));
          dev3 =  // difference |pc[2] - pc[1]| - |pc[3] - pc[0]|:
          Math.Sqrt(Math.Pow((xc[2] - xc[1]), 2.0) + Math.Pow((yc[2] - yc[1]), 2.0) + Math.Pow((zc[2] - zc[1]), 2.0)) -
          Math.Sqrt(Math.Pow((xc[3] - xc[0]), 2.0) + Math.Pow((yc[3] - yc[0]), 2.0) + Math.Pow((zc[3] - zc[0]), 2.0));

          Crit = Math.Sqrt(Math.Pow(dev1, 2.0) + Math.Pow(dev2, 2.0) + Math.Pow(dev3, 2.0));

          if (Crit < MinCrit)
          {
            MinCrit = Crit;
            OptIterX = IterX;
            OptIterY = IterY;
            OptCX = CX;
            OptCY = CY;
            for (int i = 0; i < 4; i++)
            {
              xopt[i] = xc[i];
              yopt[i] = yc[i];
              zopt[i] = zc[i];
            }
          }
          CX += step;
        } //=========================== end for (int IterX... ========================
        CY += step;
      } //============================= end for (int IterY... ========================
    } //************************************ end Optimization *******************************************



    public void Rect_Central(Point[] v, bool CUT, ref CImage Result)
    // Calculates the corners of the rectifyed image and then makes a bilinear transformation
    {
      if (CUT == false)
      {
        MessageBox.Show("Method 'Rect_Central' is not suited for CUT=" + CUT);
        return;
      }
      int MaxSize; 
      double alphaX, alphaY, betaX, betaY, F, Height, phiX, phiY,
        RedX, RedY, Width, M0X, M1X, M3Y, M2Y;

      M0X = (double)v[1].X + (v[0].X - v[1].X) * ((double)height / 2 - v[1].Y) / ((double)v[0].Y - v[1].Y);
      M1X = (double)v[2].X + (v[3].X - v[2].X) * ((double)height / 2 - v[2].Y) / ((double)v[3].Y - v[2].Y);
      M3Y = (double)v[0].Y + (v[3].Y - v[0].Y) * ((double)width / 2 - v[0].X) / ((double)v[3].X - v[0].X);
      M2Y = (double)v[1].Y + (v[2].Y - v[1].Y) * ((double)width / 2 - v[1].X) / ((double)v[2].X - v[1].X);

      RedY = (double)(v[3].Y - v[2].Y) / (double)(v[0].Y - v[1].Y);
      RedX = (double)(v[3].X - v[0].X) / (double)(v[2].X - v[1].X);
      if (width > height) MaxSize = width;
      else MaxSize = height;
      F = 1.0 * (double)(MaxSize);
      alphaY = Math.Atan2(F, (double)(width / 2 - M0X)); // >0
      betaY = Math.Atan2(F, (double)(M1X - width / 2)); // >0
      phiY = Math.Atan2(RedY * Math.Sin(betaY) - Math.Sin(alphaY), Math.Cos(alphaY) + RedY * Math.Cos(betaY));

      alphaX = Math.Atan2(F, (double)(M3Y - height / 2)); //>0
      betaX = Math.Atan2(F, (double)(height / 2 - M2Y)); // >0
      phiX = Math.Atan2(RedX * Math.Sin(betaX) - Math.Sin(alphaX), Math.Cos(alphaX) + RedX * Math.Cos(betaX));
      double P0X = F * Math.Cos(alphaY) / Math.Sin(alphaY - phiY);
      double P1X = F * Math.Cos(betaY) / Math.Sin(betaY + phiY);
      double P0Y = F * Math.Cos(alphaX) / Math.Sin(alphaX + phiX);
      Width = F * (Math.Cos(alphaY) / Math.Sin(alphaY - phiY) + Math.Cos(betaY) / Math.Sin(betaY + phiY));
      Height = F * (Math.Cos(alphaX) / Math.Sin(alphaX - phiX) + Math.Cos(betaX) / Math.Sin(betaX + phiX));     
      
      if (Width < 0.0 || Height < 0.0)
      {
        MessageBox.Show("The clicked area does not contain the center of the image");
        return;
      }
      double CX =  Math.Tan(phiY) + 0.35; 
      double CY = Math.Tan(phiX); 

      CImage Out;
      Out = new CImage((int)Width, (int)Height, N_Bits);
      Result.Copy(Out, 0);     
      double A = 0.0, B, C, D, Det, E, G;
      double[] xc = new double[4];
      double[] yc = new double[4];
      double[] zc = new double[4]; //--*/

      for (int i = 0; i < 4; i++)
      {
        A = B = C = D = 0.0;
        A = (F / (v[i].X - width / 2) + CX);
        B = CY;
        C = width / 2 * F / (v[i].X - width / 2) + CX * width / 2 + CY * height / 2 + F;
        D = CX;
        E = (F / (v[i].Y - height / 2) + CY);
        G = height / 2 * F / (v[i].Y - height / 2) + CX * width / 2 + CY * height / 2 + F;
        Det = A * E - B * D;
        xc[i] = (C * E - B * G) / Det;
        yc[i] = (A * G - C * D) / Det;
        zc[i] = F - CX * (xc[i] - width / 2) - CY * (yc[i] - height / 2); // corrected
      }

      double zz;
      double xp, yp, xp0, xp1, yp0, yp1, xf, yf;
      
      for (int Y = 0; Y < Result.height; Y++) //=========== over the rectified image ====================
      {
        xp0 = xc[1] + (xc[0] - xc[1]) * Y / (Result.height - 1);
        xp1 = xc[2] + (xc[3] - xc[2]) * Y / (Result.height - 1);

        for (int X = 0; X < Result.width; X++) //=====================================================
        {
          yp0 = yc[1] + (yc[2] - yc[1]) * X / (Result.width - 1);
          yp1 = yc[0] + (yc[3] - yc[0]) * X / (Result.width - 1);
          xp = xp0 + (xp1 - xp0) * X / (Result.width - 1); // +100.0;
          yp = yp0 + (yp1 - yp0) * Y / (Result.height - 1);
          zz = F -CX * (xp - width / 2) - CY * (yp - height / 2); // correted
          xf = width/2 + (xp - width/2) * F / (F - CX * (xp - width / 2) - CY * (yp - height / 2));
          yf = height/2 + (yp - height/2) * F / (F - CX * (xp - width / 2) - CY * (yp - height / 2));

          if ((int)xf >= 0 && (int)xf < width && (int)yf >= 0 && (int)yf < height)
            if (N_Bits == 24)
            {
              for (int ic = 0; ic < 3; ic++)
                Result.Grid[ic + 3 * X + 3 * Result.width * Y] = Grid[ic + 3 * (int)xf + 3 * width * (int)yf];
            }
            else
              Result.Grid[X + Result.width * (Result.height - 1 - Y)] = Grid[(int)xf + width * (int)yf];
        } //================================ end for (X... ==============================
      } //================================== end for (Y... ================================
    } //************************************ end Rect_Central *****************************************


    public void Rect_Retain(Point[] v, bool CUT, double Rel, ref CImage Result)
    // Calculates the corners of the rectifyed image and then makes a bilinear transformation
    {
      int MaxSize; 
      double alphaX, alphaY, betaX, betaY, F, Height, phiX, phiY,
        RedX, RedY, Width, M0X, M1X, M3Y, M2Y;

      M0X = (double)v[1].X + (v[0].X - v[1].X) * ((double)height / 2 - v[1].Y) / ((double)v[0].Y - v[1].Y);
      M1X = (double)v[2].X + (v[3].X - v[2].X) * ((double)height / 2 - v[2].Y) / ((double)v[3].Y - v[2].Y);
      M3Y = (double)v[0].Y + (v[3].Y - v[0].Y) * ((double)width / 2 - v[0].X) / ((double)v[3].X - v[0].X);
      M2Y = (double)v[1].Y + (v[2].Y - v[1].Y) * ((double)width / 2 - v[1].X) / ((double)v[2].X - v[1].X);

      RedY = (double)(v[3].Y - v[2].Y) / (double)(v[0].Y - v[1].Y);
      RedX = (double)(v[3].X - v[0].X) / (double)(v[2].X - v[1].X);
      if (width > height) MaxSize = width;
      else MaxSize = height;
      F = 1.0 * (double)(MaxSize);
      alphaY = Math.Atan2(F, (double)(width / 2 - M0X)); 
      betaY = Math.Atan2(F, (double)(M1X - width / 2)); 
      phiY = Math.Atan2(RedY * Math.Sin(betaY) - Math.Sin(alphaY), Math.Cos(alphaY) + RedY * Math.Cos(betaY));

      alphaX = Math.Atan2(F, (double)(M3Y - height / 2)); 
      betaX = Math.Atan2(F, (double)(height / 2 - M2Y)); 
      phiX = Math.Atan2(RedX * Math.Sin(betaX) - Math.Sin(alphaX), Math.Cos(alphaX) + RedX * Math.Cos(betaX));

      double CX = Math.Tan(phiY); // X-coefficient of plane P
      double CY = Math.Tan(phiX); // Y-coefficient of plane

      double OptCX=0.0, OptCY=0.0;
      Optimization(F, v, CX, CY, ref OptCX, ref OptCY);
      CX = OptCX;
      CY = OptCY;

      double A = 0.0, B, C, D, Det, E, G;
      double[] xc = new double[8];
      double[] yc = new double[8];
      double[] zc = new double[8]; 
      
      Point[] w = new Point[8];
      for (int i = 0; i < 4; i++) w[i] = v[i];

      for (int i = 0; i < 4; i++)
      {
        A = (F / (w[i].X - width / 2) + CX);
        B = CY;
        C = width / 2 * F / (w[i].X - width / 2) + CX * width / 2 + CY * height / 2 + F;
        D = CX;
        E = (F / (w[i].Y - height / 2) + CY);
        G = height / 2 * F / (w[i].Y - height / 2) + CX * width / 2 + CY * height / 2 + F;
        Det = A * E - B * D;
        xc[i] = (C * E - B * G) / Det;
        yc[i] = (A * G - C * D) / Det;
        zc[i] = F - CX * (xc[i] - width / 2) - CY * (yc[i] - height / 2); // corrected
      }
      // Transforming the points:
      for (int i = 0; i < 4; i++) // new points:
      {
        xc[i+4] = (int)(width / 2 + Rel * (xc[i] - width / 2));
        yc[i+4] = (int)(height / 2 + Rel * (yc[i] - height / 2));
        zc[i+4] = (int)(F + Rel * (zc[i] - F));
      }

      Width = 
        Math.Sqrt(Math.Pow((xc[7]-xc[4]), 2.0)+Math.Pow((yc[7]-yc[4]), 2.0)+Math.Pow((zc[7]-zc[4]), 2.0));
      Height =
        Math.Sqrt(Math.Pow((xc[4]-xc[5]), 2.0)+Math.Pow((yc[4]-yc[5]), 2.0)+Math.Pow((zc[4]-zc[5]), 2.0));

      CImage Out;
       Out = new CImage((int)Width, (int)Height, N_Bits);
      Result.Copy(Out, 0);

      double zz;
      double xp, yp, xp0, xp1, yp0, yp1, xf, yf;

      for (int Y = 0; Y < Result.height; Y++) //=========== over the rectified image ====================
      {
        xp0 = xc[5] + (xc[4] - xc[5]) * Y / (Result.height - 1);
        xp1 = xc[6] + (xc[7] - xc[6]) * Y / (Result.height - 1);

        for (int X = 0; X < Result.width; X++) //=====================================================
        {
          yp0 = yc[5] + (yc[6] - yc[5]) * X / (Result.width - 1);
          yp1 = yc[4] + (yc[7] - yc[4]) * X / (Result.width - 1);
          xp = xp0 + (xp1 - xp0) * X / (Result.width - 1); // +100.0;
          yp = yp0 + (yp1 - yp0) * Y / (Result.height - 1);
          zz = F - CX * (xp - width / 2) - CY * (yp - height / 2); // correted
          xf = width / 2 + (xp - width / 2) * F / (F - CX * (xp - width / 2) - CY * (yp - height / 2));
          yf = height / 2 + (yp - height / 2) * F / (F - CX * (xp - width / 2) - CY * (yp - height / 2));

          if ((int)xf >= 0 && (int)xf < width && (int)yf >= 0 && (int)yf < height)
            if (N_Bits == 24)
            {
              for (int ic = 0; ic < 3; ic++)
                Result.Grid[ic + 3 * X + 3 * Result.width * Y] =                  
                                    Grid[ic + 3 * (int)xf + 3 * width * (int)yf];
            }
            else
              Result.Grid[X + Result.width * (Result.height - 1 - Y)] = Grid[(int)xf + width * (int)yf];
        } //================================ end for (X... ==============================
      } //================================== end for (Y... ================================

    } //************************************ end Rect_Retain *****************************************


    public void Rect_Optimal(Point[] v, bool CUT, ref CImage Result)
    // Calculates the corners of the rectifyed image and then makes a bilinear transformation
    {
      int MaxSize;
      double alphaX, alphaY, betaX, betaY, F, Height, phiX, phiY,
        RedX, RedY, Width, M0X, M1X, M3Y, M2Y;

      M0X = (double)v[1].X + (v[0].X - v[1].X) * ((double)height / 2 - v[1].Y) / ((double)v[0].Y - v[1].Y);
      M1X = (double)v[2].X + (v[3].X - v[2].X) * ((double)height / 2 - v[2].Y) / ((double)v[3].Y - v[2].Y);
      M3Y = (double)v[0].Y + (v[3].Y - v[0].Y) * ((double)width / 2 - v[0].X) / ((double)v[3].X - v[0].X);
      M2Y = (double)v[1].Y + (v[2].Y - v[1].Y) * ((double)width / 2 - v[1].X) / ((double)v[2].X - v[1].X);

      RedY = (double)(v[3].Y - v[2].Y) / (double)(v[0].Y - v[1].Y);
      RedX = (double)(v[3].X - v[0].X) / (double)(v[2].X - v[1].X);
      if (width > height) MaxSize = width;
      else MaxSize = height;
      F = 1.0 * (double)(MaxSize);
      alphaY = Math.Atan2(F, (double)(width / 2 - M0X)); 
      betaY = Math.Atan2(F, (double)(M1X - width / 2)); 
      phiY = Math.Atan2(RedY * Math.Sin(betaY) - Math.Sin(alphaY), Math.Cos(alphaY) + RedY * Math.Cos(betaY));

      alphaX = Math.Atan2(F, (double)(M3Y - height / 2)); 
      betaX = Math.Atan2(F, (double)(height / 2 - M2Y)); 
      phiX = Math.Atan2(RedX * Math.Sin(betaX) - Math.Sin(alphaX), Math.Cos(alphaX) + RedX * Math.Cos(betaX));
      double P0X = F * Math.Cos(alphaY) / Math.Sin(alphaY - phiY);
      double P1X = F * Math.Cos(betaY) / Math.Sin(betaY + phiY);
      double P0Y = F * Math.Cos(alphaX) / Math.Sin(alphaX + phiX);
      Width = F * (Math.Cos(alphaY) / Math.Sin(alphaY - phiY) + Math.Cos(betaY) / Math.Sin(betaY + phiY));
      Height = F * (Math.Cos(alphaX) / Math.Sin(alphaX - phiX) + Math.Cos(betaX) / Math.Sin(betaX + phiX));

      if (Width < 0.0 || Height < 0.0)
      {
        MessageBox.Show("The clicked area does not contain the center of the image");
        return;
      }

      double OptCX=0.0; 
      double OptCY=0.0; 
      double CX = Math.Tan(phiY); 
      double CY = Math.Tan(phiX); 

      Optimization(F, v, CX, CY, ref OptCX, ref OptCY);
      CX = OptCX;
      CY = OptCY;

      CImage Out;
      if (CUT)
        Out = new CImage((int)Width, (int)Height, N_Bits);
      else
        Out = new CImage(width, height, N_Bits);
      Result.Copy(Out, 0);

      double A = 0.0, B, C, D, Det, E, G;
      double[] xc = new double[4];
      double[] yc = new double[4];
      double[] zc = new double[4]; 

      for (int i = 0; i < 4; i++)
      {
        A = B = C = D = 0.0;
        A = (F / (v[i].X - width / 2) + CX);
        B = CY;
        C = width / 2 * F / (v[i].X - width / 2) + CX * width / 2 + CY * height / 2 + F;
        D = CX;
        E = (F / (v[i].Y - height / 2) + CY);
        G = height / 2 * F / (v[i].Y - height / 2) + CX * width / 2 + CY * height / 2 + F;
        Det = A * E - B * D;
        xc[i] = (C * E - B * G) / Det;
        yc[i] = (A * G - C * D) / Det;
        zc[i] = F - CX * (xc[i] - width / 2) - CY * (yc[i] - height / 2); // corrected
      }

      double zz;
      double xp, yp, xp0, xp1, yp0, yp1, xf, yf;
      for (int Y = 0; Y < Result.height; Y++) //=========== over the rectified image ====================
      {
        xp0 = xc[1] + (xc[0] - xc[1]) * Y / (Result.height - 1);
        xp1 = xc[2] + (xc[3] - xc[2]) * Y / (Result.height - 1);

        for (int X = 0; X < Result.width; X++) //=====================================================
        {
          yp0 = yc[1] + (yc[2] - yc[1]) * X / (Result.width - 1);
          yp1 = yc[0] + (yc[3] - yc[0]) * X / (Result.width - 1);
          xp = xp0 + (xp1 - xp0) * X / (Result.width - 1); 
          yp = yp0 + (yp1 - yp0) * Y / (Result.height - 1);
          zz = F - CX * (xp - width / 2) - CY * (yp - height / 2); // corrected
          xf = width / 2 + (xp - width / 2) * F / (F - CX * (xp - width / 2) - CY * (yp - height / 2));
          yf = height / 2 + (yp - height / 2) * F / (F - CX * (xp - width / 2) - CY * (yp - height / 2));

          if ((int)xp >= 0 && (int)xp < width && (int)yp >= 0 && (int)yp < height)
            if (N_Bits == 24)
            {
              for (int ic = 0; ic < 3; ic++)
                Result.Grid[ic + 3 * X + 3 * Result.width * Y] = Grid[ic + 3 * (int)xf + 3 * width * (int)yf];
            }
            else
              Result.Grid[X + Result.width * (Result.height - 1 - Y)] = Grid[(int)xf + width * (int)yf];
        } //================================ end for (X... ==============================
      } //================================== end for (Y... ================================
    } //************************************ end Rect_Optimal *****************************************

  } //************************************** end public class CImage *****************************************
} //**************************************** end namespace ******************************************************
