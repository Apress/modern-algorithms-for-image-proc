using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WFsegmentAndComp
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }
    private Bitmap OrigBmp;
    private Bitmap ImpulseBmp;
    private Bitmap SegmentBmp;
    private Bitmap BreadthBmp;
    private Bitmap RootBmp;
    CImage OrigIm;
    CImage ImpulseIm;
    CImage SegmentIm;
    CImageComp BreadthFirIm;
    CImageComp RootIm;
    public string OpenImageFile;
    int nbyte, width, height;
    int nLoop;
    int denomProg;
    bool ORIG = false;
    bool IMPULSE = false;
    bool SEGMENT = false;
    bool BREAD = false;
    bool ROOT = false;
        
    private void button1_Click(object sender, EventArgs e) // Open image
    {
      label1.Visible = false;
      label2.Visible = false;
      label3.Visible = false;
      label4.Visible = false;
      label5.Visible = false;
      label6.Visible = false;
      button2.Visible = false;
      button3.Visible = false;
      button4.Visible = false;
      button5.Visible = false;
      button6.Visible = false;
      button8.Visible = false;
      numericUpDown1.Visible = false;
      numericUpDown2.Visible = false;
      OpenFileDialog openFileDialog1 = new OpenFileDialog();
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        try
        {
          OrigBmp = new Bitmap(openFileDialog1.FileName);
          OpenImageFile = openFileDialog1.FileName;
        }
        catch (Exception ex)
        {
          MessageBox.Show("Open image; Could not read file from disk. Error: " +
            ex.Message);
        }
      }
      else return;

      label1.Visible = true;
      label3.Text = "Opened image:" + openFileDialog1.FileName;
      label3.Visible = true;
      button4.Visible = true;
      label6.Visible = true;
      label6.Text = "Click 'Segment'";
      width = OrigBmp.Width;
      height = OrigBmp.Height;
           
      ORIG = true;

      progressBar1.Maximum = 100;
      progressBar1.Value = 0;
      progressBar1.Step = 1;
      nLoop = 1;
      denomProg = progressBar1.Maximum / progressBar1.Step;

      if (OrigBmp.PixelFormat == PixelFormat.Format8bppIndexed)
      {
        OrigIm = new CImage(width, height, 8);
        BitmapToImageOld(OrigBmp, OrigIm);
      }
      else if (OrigBmp.PixelFormat == PixelFormat.Format24bppRgb)
      {
        OrigIm = new CImage(width, height, 24);
        BitmapToImage(OrigBmp, OrigIm);
      }
      else
      {
        MessageBox.Show("Not suitable pixel format=" + OrigBmp.PixelFormat);
        return;
      }
      pictureBox1.Image = OrigBmp;

      ImpulseIm = new CImage(width, height, 8); //, Grid);
      SegmentIm = new CImage(width, height, 8); 
      BreadthFirIm = new CImageComp(width, height, 8); 
      RootIm = new CImageComp(width, height, 8);
      progressBar1.Value = 0;
      label1.Text = "    Original image   ";
    } //****************************** end Open image ****************************************


    private void button4_Click(object sender, EventArgs e) // Segment
    {
      if (ORIG == false)
      {
        MessageBox.Show("Please click to 'Open image' and open an image");
        return;
      }

      SegmentIm = new CImage(width, height, 8);
      for (int light = 0; light < 256; light++) SegmentIm.Palette[light] = Color.FromArgb(light, light, light);
      SegmentIm.nLoop = 2;
      SegmentIm.denomProg = denomProg;
      if (OrigIm.N_Bits == 24)
        SegmentIm.ColorToGray(OrigIm, this);
      else
        SegmentIm.Copy(OrigIm);
      byte gw = OrigIm.Grid[5];
      for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
          SegmentIm.Grid[x + width * y] /= 24;
          SegmentIm.Grid[x + width * y] *= 24;
        }
      SegmentBmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
      ImageToBitmap(SegmentBmp, SegmentIm);
      label2.Text = "   Segmented image   ";
      label2.Visible = true;
      button2.Visible = true;
      label4.Visible = true;
      label5.Visible = true;
      numericUpDown1.Visible = true;
      numericUpDown2.Visible = true;
      label6.Text = "Click 'Impulse noise'";
      pictureBox2.Image = SegmentBmp;
      progressBar1.Visible = false;
      SEGMENT = true;
      IMPULSE = false;
      BREAD = false;
      ROOT = false;

    } //****************************** end Segment ****************************************


    private void button2_Click(object sender, EventArgs e) // Impulse noise
    {
      if (ORIG == false)
      {
        MessageBox.Show("Please click to 'Open image' and open an image");
        return;
      }

      if (SEGMENT == false)
      {
        MessageBox.Show("Please click to 'Segment' to segment the image");
        return;
      }

      int nbit, nbyte;
      nbit = 8; nbyte = 1;  
      progressBar1.Visible = true;
      progressBar1.Value = 0;

      for (int light = 0; light < 256; light++) ImpulseIm.Palette[light] = Color.FromArgb(light, light, light);

      ImpulseIm.Copy(SegmentIm);
      nLoop = 5; 
      ImpulseIm.nLoop = nLoop;
      progressBar1.Step = 1;

      ImpulseIm.DeleteBit0(nbyte);
     
      // "histo" is necessary for the constructor of CPnoise before "Sort".
      int MaxGV, MinGV;
      int[] histo = new int[256];
      for (int i = 0; i < 256; i++) histo[i] = 0;

      int i1 = nLoop * width * height / denomProg;
      if (nbit == 24)
      {
        int lum;
        for (int i = 0; i < width * height; i++)
        {
          if (i % i1 == 0) progressBar1.PerformStep();
          lum = (int)ImpulseIm.MaxC(ImpulseIm.Grid[3 * i + 2], 
                            ImpulseIm.Grid[3 * i + 1], ImpulseIm.Grid[3 * i + 0]);
          histo[lum]++;
        }
      }
      else
        for (int i = 0; i < width * height; i++)
        {
          if (i % i1 == 0) progressBar1.PerformStep();
          histo[ImpulseIm.Grid[i] & 252]++;
        }

      for (MaxGV = 255; MaxGV > 0; MaxGV--) if (histo[MaxGV] != 0) break;
      for (MinGV = 0; MinGV < 256; MinGV++) if (histo[MinGV] != 0) break; 

      CPnoise PN = new CPnoise(histo, 1000, 4000);

      PN.Sort(ImpulseIm, histo, this);

      int MADS = (int)numericUpDown1.Value;
      int MALS = (int)numericUpDown2.Value;
      progressBar1.Step = 2;

      PN.DarkNoise(ref ImpulseIm, MinGV, MaxGV, MADS, this);

      ImpulseIm.DeleteBit0(nbyte);

      PN.LightNoise(ref ImpulseIm, MinGV, MaxGV, MALS, this);
 
      for (int i = 0; i < nbyte * width * height; i++)
        if ((ImpulseIm.Grid[i] & 3) != 3) ImpulseIm.Grid[i] |= 3;

      ImpulseBmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
      ImageToBitmapOld(ImpulseBmp, ImpulseIm);
      progressBar1.Visible = false;
      pictureBox2.Image = ImpulseBmp;
      label2.Text = "Impulse noise suppressed";
      button3.Visible = true;
      button8.Visible = true;
      button5.Visible = true;
      button6.Visible = true;
      label6.Text = "Click 'Breadth First' or 'Root method'";

      IMPULSE = true;
      BREAD = false;
      ROOT = false;

    } //****************************** end Impulse noise ****************************************


    private void button3_Click(object sender, EventArgs e) // BreadthFirst
    {
      if (ORIG == false)
      {
        MessageBox.Show("Please open an image and click 'Segment' and 'Impulse noise'");
        return;
      }
      if (SEGMENT == false)
      {
        MessageBox.Show("Please click 'Segment' and 'Impulse noise'");
        return;
      }
      if (IMPULSE == false)
      {
        MessageBox.Show("Please click 'Impulse noise'");
        return;
      }
      progressBar1.Value = 0;
      progressBar1.Visible = true;
      progressBar1.Step = 1;
      BreadthFirIm.Copy(ImpulseIm, true);
      int nComp = BreadthFirIm.LabelC(this);
      int nPal = 0;
      BreadthFirIm.MakePalette(ref nPal);
      BreadthBmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
      LabToBitmap(BreadthBmp, BreadthFirIm);
      pictureBox2.Image = BreadthBmp;
      progressBar1.Visible = false;
      BREAD = true;
      label2.Text = nComp + " breadth components  ";
      label6.Text = "Saving 'Breadth' is possible";

    } //****************************** end BreadthFirst ****************************************


    private void button5_Click(object sender, EventArgs e) // Root method
    {
      if (ORIG == false)
      {
        MessageBox.Show("Please open an image and click 'Segment' and 'Impulse noise'");
        return;
      }
      if (SEGMENT == false)
      {
        MessageBox.Show("Please click 'Segment' and 'Impulse noise'");
        return;
      }
      if (IMPULSE == false)
      {
        MessageBox.Show("Please click 'Impulse noise'");
        return;
      }
      RootIm.Copy(ImpulseIm, true);

      progressBar1.Value = 0;
      progressBar1.Visible = true;
      int nComp = RootIm.ComponentsE(this);
      int nPal = 0;
      RootIm.MakePalette(ref nPal);
      progressBar1.Visible = false;
      RootBmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
      LabToBitmap(RootBmp, RootIm);
      progressBar1.Visible = false;
      pictureBox2.Image = RootBmp;
      ROOT = true;
      label2.Text = nComp + " root components   ";
      label6.Text = "Saving 'Root' is possible";
    } //****************************** end Root method ****************************************


    private void button6_Click(object sender, EventArgs e) // Save image of 'Root'
    {
      SaveFileDialog dialog = new SaveFileDialog();
      if (!ROOT)
      {
        MessageBox.Show("Please click the button 'Root method'");
        return;
      }
      dialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*";

      if (dialog.ShowDialog() == DialogResult.OK)
      {
        if (dialog.FileName == OpenImageFile)
        {
          string tmpFileName = OpenImageFile.Insert(OpenImageFile.IndexOf("."), "$$$");
          if (dialog.FileName.Contains(".jpg"))
            RootBmp.Save(tmpFileName, ImageFormat.Jpeg); // saving tmpFile
          else 
            if (dialog.FileName.Contains(".bmp")) RootBmp.Save(tmpFileName, ImageFormat.Bmp);
            else
            {
              MessageBox.Show("The file " + dialog.FileName + " has an inappropriate extension. Returning.");
              return;
            }

          OrigBmp.Dispose();
          File.Replace(tmpFileName, OpenImageFile,
                        OpenImageFile.Insert(OpenImageFile.IndexOf("."), "BackUp"));
          // Replaces the contents of 'OpenImageFile' with the contents of the file 'tmpFileName', 
          // deleting 'tmpFileName', and creating a backup of the 'OpenImageFile'.

          OrigBmp = new Bitmap(OpenImageFile);
          pictureBox1.Image = OrigBmp;
        } 
      }
      else 
      {
        if (dialog.FileName.Contains(".jpg")) RootBmp.Save(dialog.FileName, ImageFormat.Jpeg);
        else 
          if (dialog.FileName.Contains(".bmp")) RootBmp.Save(dialog.FileName, ImageFormat.Bmp);
          else
          {
            MessageBox.Show("The file " + dialog.FileName + " has an inappropriate extension. Returning.");
            return;
          }
      }
    } //****************************** end Save image of 'Root'****************************************


    public byte MaxC(byte R, byte G, byte B)
    {
      byte light = G;
      if (0.713 * R > G) light = (byte)(0.713 * R);
      if (0.527 * B > G) light = (byte)(0.527 * B);
      return light;
    }



    public void BitmapToImage(Bitmap bmp, CImage Image)
    {
      int nbyteBmp, nbyteIm;
      nbyteIm = Image.N_Bits / 8;
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
      int bytes = Math.Abs(bmpData.Stride) * bmp.Height; 
      byte[] rgbValues = new byte[bytes];
      System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

      progressBar1.Visible = true;
      int y1 = 1 + bmp.Height / 100;
        
      for (int y = 0; y < bmp.Height; y++)
      {
        if (y % y1 == 0) progressBar1.PerformStep();

        for (int x = 0; x < bmp.Width; x++)
        {
          if (nbyteBmp == 1)
          {
            Color color = bmp.Palette.Entries[rgbValues[x + Math.Abs(bmpData.Stride) * y]];
            if (nbyteIm == 1)
              Image.Grid[x + bmp.Width * y] = color.R;
            else
            {
              Image.Grid[3 * (x + bmp.Width * y) + 0] = color.B;
              Image.Grid[3 * (x + bmp.Width * y) + 1] = color.G;
              Image.Grid[3 * (x + bmp.Width * y) + 2] = color.R;
            }
          }
          else // nbyteBmp == 3
          {
            if (nbyteIm == 1)
              Image.Grid[x + bmp.Width * y] = rgbValues[2 + nbyte * x + Math.Abs(bmpData.Stride) * y];
            else // nbyteIm == 3
              for (int c = 0; c < nbyteBmp; c++)
                Image.Grid[c + nbyteIm * (x + bmp.Width * y)] =
                  rgbValues[c + nbyteBmp * x + Math.Abs(bmpData.Stride) * y];
          }
        }
      }
      bmp.UnlockBits(bmpData);
    } //****************************** end BitmapToImage ****************************************


    public void BitmapToImageOld(Bitmap bmp, CImage Image)
    {
      int nbyteIm = Image.N_Bits / 8;
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
        if (y % y1 == 0) progressBar1.PerformStep();

        for (int x = 0; x < bmp.Width; x++)
        {
          int i = x + width * y;
          color = bmp.GetPixel(x, y);
          if (nbyteIm == 1)
            Image.Grid[i] = MaxC(color.R, color.G, color.B);
          else // nbyteIm == 3
            for (int c = 0; c < nbyteIm; c++)
            {
              if (c == 0) Image.Grid[nbyteIm * i] = color.B;
              if (c == 1) Image.Grid[nbyteIm * i + 1] = color.G;
              if (c == 2) Image.Grid[nbyteIm * i + 2] = color.R;
            }
        }
      }
    } //****************************** end BitmapToImageOld ****************************************


    public void ImageToBitmap(Bitmap bmp, CImage Image)
    {
      int nbyteBmp = 0, nbyteIm = Image.N_Bits / 8;
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
      progressBar1.Visible = true;
      int Len = bmp.Height, jump, nStep = 50;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (int y = 0; y < bmp.Height; y++)
      {
        if (y % jump == jump - 1) progressBar1.PerformStep();
        for (int x = 0; x < bmp.Width; x++)
        {
          if (nbyteBmp == 1)  // nbyte of bmp;
          {
            if (nbyteIm == 1) // nbyteIm == 1;
            {
              color = (Color)Image.Palette[Image.Grid[x + bmp.Width * y]];
              rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 0] = color.B;
              rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 1] = color.G;
              rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 2] = color.R;
            }
            else // nbyteBmp == 1; nbyteIm == 3
            {
              color = bmp.Palette.Entries[Image.Grid[3 * (x + bmp.Width * y)]];
              rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 0] = color.B;
              rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 1] = color.G;
              rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 2] = color.R;
            }
          }
          else  // nbyteBmp == 3
          {
            if (nbyteIm == 1)
            {
              light = Image.Grid[x + Image.width * y];
              {
                index = 3 * x + Math.Abs(bmpData.Stride) * y; 
                rgbValues[index + 0] = light; // color.B; 
                rgbValues[index + 1] = light; // color.G;
                rgbValues[index + 2] = light; // color.R; 
              }
            }
            else //nbyteIm ==3
            {
              for (int c = 0; c < nbyte; c++)
              {
                rgbValues[c + nbyte * x + Math.Abs(bmpData.Stride) * y] =
                                  Image.Grid[c + nbyteIm * (x + bmp.Width * y)];
              }
            } //------------------ end if (nbyteIm ==1) ------------------------
          } //-------------------- end if (nbyte == 1) ----------------------------
        }
      }
      System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
      bmp.UnlockBits(bmpData);
    } //****************************** end ImageToBitmap ****************************************


    public void ImageToBitmapOld(Bitmap bmp, CImage Image)
    {
      int nbyteIm = Image.N_Bits / 8;
      int light = 0;
      switch (bmp.PixelFormat)
      {
        case PixelFormat.Format24bppRgb: nbyte = 3; break;
        case PixelFormat.Format8bppIndexed: nbyte = 1; break;
        default: MessageBox.Show("ImageToBitmap: Not suitable  pixel format=" + bmp.PixelFormat); return;
      }
      Color color;
      int Len = bmp.Height, jump, nStep = 25;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      for (int y = 0; y < bmp.Height; y++)
      {
        if (y % jump == jump - 1) progressBar1.PerformStep();
        for (int x = 0; x < bmp.Width; x++)
        {
          if (nbyte == 1)  // nbyte of bmp;
          {
            if (nbyteIm == 1) // nbyteIm == 1;
            {
              light = (int)Image.Grid[x + bmp.Width * y];
              bmp.SetPixel(x, y, Color.FromArgb(light, light, light));
            }
            else // nbyte == 1; nbyteIm == 3
            {
              bmp.SetPixel(x, y, Color.FromArgb(Image.Grid[nbyte * (x + width * y) + 2],
                  Image.Grid[nbyte * (x + width * y) + 1], Image.Grid[nbyte * (x + width * y) + 0]));
            }
          }
          else  // nbyte == 3
          {
            if (nbyteIm == 1)
            {
              light = Image.Grid[x + bmp.Width * y];
              color = Color.FromArgb(light, light, light);
              bmp.SetPixel(x, y, Color.FromArgb(light, light, light));
            }
            else //nbyteIm ==3; nbyte == 3
            {
              bmp.SetPixel(x, y, Color.FromArgb(Image.Grid[nbyteIm * (x + width * y) + 2],
                  Image.Grid[nbyteIm * (x + width * y) + 1], Image.Grid[nbyteIm * (x + width * y) + 0]));
            } //------------------ end if (nbyteIm ==1) ------------------------
          } //-------------------- end if (nbyte == 1) ----------------------------
        }
      }
    } //****************************** end ImageToBitmapOld ****************************************/


    public void LabToBitmap(Bitmap bmp, CImageComp Image)
    {
      if (Image.N_Bits != 8)
      {
        MessageBox.Show("LabToBitmap: Not suitable  format of 'Image'; N_Bits=" + Image.N_Bits);
        return;
      }
      switch (bmp.PixelFormat)
      {
        case PixelFormat.Format24bppRgb: nbyte = 3; break;
        case PixelFormat.Format8bppIndexed:
          MessageBox.Show("LabToBitmap: Not suitable  pixel format=" + bmp.PixelFormat);
          return;
        default: MessageBox.Show("LabToBitmap: Not suitable  pixel format=" + bmp.PixelFormat);
          return;
      }
      Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
      BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
 
      IntPtr ptr = bmpData.Scan0;
      int size = bmp.Width * bmp.Height;
      int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
      byte[] rgbValues = new byte[bytes];
      Color color;
      int index = 0;
      int Len = bmp.Height, jump, nStep = 50;
      if (Len > 2 * nStep) jump = Len / nStep;
      else jump = 2;
      progressBar1.Step = 1;
      progressBar1.Visible = true;
      for (int y = 0; y < bmp.Height; y++)
      {
        if (y % jump == jump - 1) progressBar1.PerformStep();
        for (int x = 0; x < bmp.Width; x++)
        {
          color = Image.Palette[Image.Lab[x + bmp.Width * y] & 255];
          index = 3 * x + Math.Abs(bmpData.Stride) * y;
          rgbValues[index + 0] = color.B;
          rgbValues[index + 1] = color.G;
          rgbValues[index + 2] = color.R;
        }
      }
      System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
      bmp.UnlockBits(bmpData);
    } //****************************** end LabToBitmap ***********************


    private void button8_Click(object sender, EventArgs e) // Save image of 'Breadth'
    {
      SaveFileDialog dialog = new SaveFileDialog();
      if (!BREAD)
      {
        MessageBox.Show("Please click the button'Breadth First Search'");
        return;
      }
      dialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*";
      if (dialog.ShowDialog() == DialogResult.OK)
      {
        if (dialog.FileName == OpenImageFile)
        {
          string tmpFileName = OpenImageFile.Insert(OpenImageFile.IndexOf("."), "$$$");
          if (dialog.FileName.Contains("jpg"))
            BreadthBmp.Save(tmpFileName, ImageFormat.Jpeg); // saving tmpFile
          else BreadthBmp.Save(tmpFileName, ImageFormat.Bmp);

          OrigBmp.Dispose();
          File.Replace(tmpFileName, OpenImageFile,
                        OpenImageFile.Insert(OpenImageFile.IndexOf("."), "BackUp"));
          // Replaces the contents of 'OpenImageFile' with the contents of the file 'tmpFileName', 
          // deleting 'tmpFileName', and creating a backup of the 'OpenImageFile'.

          OrigBmp = new Bitmap(OpenImageFile);
          pictureBox1.Image = OrigBmp;
        }
      }
      else 
      {
        if (dialog.FileName.Contains("jpg")) BreadthBmp.Save(dialog.FileName, ImageFormat.Jpeg);
        else BreadthBmp.Save(dialog.FileName, ImageFormat.Bmp);
      }
    } //****************************** end Save image ****************************************

  } //******************************* end Form1 *************************************************
} //********************************* end namespace *************************************************
