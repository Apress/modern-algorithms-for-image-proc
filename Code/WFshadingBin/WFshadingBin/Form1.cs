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


namespace WFshadingBin
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }
    private Bitmap original_Bitmap;
    private Bitmap Sub_Bitmap;
    private Bitmap Div_Bitmap;
    CImage OrigIm;
    CImage SigmaIm;
    CImage SubIm;
    CImage DivIm;
    CImage GrayIm;
    CImage MeanIm;
    CImage BinIm;
    int width, height, nbyteBmp, nbyteIm, Threshold, Threshold1;
    bool SHADING = false;
    double ScaleX, ScaleY, Scale1;
    int marginX, marginY;
    string OpenImageFile;

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
      OpenFileDialog openFileDialog1 = new OpenFileDialog();
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        try
        {
          original_Bitmap = new Bitmap(openFileDialog1.FileName);
          OpenImageFile = openFileDialog1.FileName;
          pictureBox1.Image = original_Bitmap;
        }
        catch (Exception ex)
        {
          MessageBox.Show("Error: Could not read file from disk. Original error: " +
           ex.Message);
        }
      }
      else return;

      label1.Visible = true;
      label2.Visible = true;
      label5.Visible = true;
      label5.Text = "Opened image:" + OpenImageFile;
      label6.Visible = true;
      button2.Visible = true;
      numericUpDown1.Visible = true;
      numericUpDown2.Visible = true;

      width  = original_Bitmap.Width;
      height = original_Bitmap.Height;
      
      progressBar1.Visible = true;
      progressBar1.Value = 0;

      if (original_Bitmap.PixelFormat == PixelFormat.Format8bppIndexed) nbyteBmp = 1;
      else
        if (original_Bitmap.PixelFormat == PixelFormat.Format24bppRgb) nbyteBmp = 3;
        else
        {
          MessageBox.Show("Pixel format=" + original_Bitmap.PixelFormat + " not used in this project");
          return;
        }



      nbyteIm = BitmapToImage(original_Bitmap, ref OrigIm);
      int N_Bits = nbyteIm * 8;
      SigmaIm = new CImage(width, height, N_Bits); 
      SubIm = new CImage(width, height, N_Bits);
      DivIm = new CImage(width, height, N_Bits);
      GrayIm = new CImage(width, height, 8);
      MeanIm = new CImage(width, height, 8);
      BinIm = new CImage(width, height, 8);

      SigmaIm.SigmaSimpleUni(OrigIm, 1, 30);
      if (OrigIm.N_Bits == 24) GrayIm.ColorToGray(SigmaIm, this);
      else GrayIm.Copy(SigmaIm);

      Threshold1 = -1;
      Threshold = -1;
      ScaleX = (double)pictureBox1.Width / (double)width;
      ScaleY = (double)pictureBox1.Height / (double)height;
      Scale1 = Math.Min(ScaleX, ScaleY);
      marginX = (pictureBox1.Width - (int)(Scale1 * width)) / 2;
      marginY = (pictureBox1.Height - (int)(Scale1 * height)) / 2;
      progressBar1.Visible = false;

      SHADING = false;
    } //****************************** end Open image ***************************



    public int BitmapToImage(Bitmap bmp, ref CImage Image)
    // Converts any bitmap to a color or to a grayscale image.
    {
      int nbyteIm = 1, rv = 0, x, y;
      Color color;

      if (nbyteBmp == 1)  // nbyteBmp is member of "Form1" according to the PixelFormat of "bmp"
      {
        x = 10;
        y = 2;
        color = bmp.GetPixel(x, y);
        if (color.R != color.G) nbyteIm = 3;
        Image = new CImage(bmp.Width, bmp.Height, nbyteIm * 8);

        progressBar1.Visible = true;
        progressBar1.Value = 0;
        for (y = 0; y < bmp.Height; y++) //========================================================
        {
          int jump = bmp.Height / 100;
          if (y % jump == jump - 1) progressBar1.PerformStep();

          for (x = 0; x < bmp.Width; x++) //======================================================
          {
            color = bmp.GetPixel(x, y);
            if (nbyteIm == 3)
            {
              Image.Grid[3 * (x + bmp.Width * y) + 0] = color.B;
              Image.Grid[3 * (x + bmp.Width * y) + 1] = color.G;
              Image.Grid[3 * (x + bmp.Width * y) + 2] = color.R;
            }
            else // nbyteIm == 1:
              Image.Grid[x + bmp.Width * y] = color.R;
          } //================================== end for (x ... ===================================
        } //==================================== end for (y ... =====================================
        rv = nbyteIm;
      }
      else // nbyteBmp == 3 and nbyteIm == 3:
      {
        Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
        IntPtr ptr = bmpData.Scan0;
        int Str = bmpData.Stride;
        int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
        byte[] rgbValues = new byte[bytes];
        System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

        nbyteIm = 3;
        Image = new CImage(bmp.Width, bmp.Height, nbyteIm * 8);
        for (y = 0; y < bmp.Height; y++) //=============================================
        {
          int jump = bmp.Height / 100;
          if (y % jump == jump - 1) progressBar1.PerformStep();
          for (x = 0; x < bmp.Width; x++)
            for (int c = 0; c < nbyteIm; c++)
              Image.Grid[c + nbyteIm * (x + bmp.Width * y)] =
                rgbValues[c + nbyteBmp * x + Math.Abs(bmpData.Stride) * y];
        } //========================= end for (y = 0; ... ============================== 
        rv = nbyteIm;
        bmp.UnlockBits(bmpData);
      }
      return rv;
    } //****************************** end BitmapToImage ****************************************


    public int MessReturn(string s)
    {
      if (MessageBox.Show(s, "Return", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
        return -1;
      return 1;
    }



    private int ImageToBitmapNew(CImage Image, Bitmap bmp, int progPart)
    // Any image and color bitmap.
    {
      Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
      BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
      if (bmp.PixelFormat != PixelFormat.Format24bppRgb)
        if (Image.MessReturn("ImageToBitmapNew: we don't use this pixel format") < 0) return -1;
      IntPtr ptr = bmpData.Scan0;
      int size = bmp.Width * bmp.Height;
      int length = Math.Abs(bmpData.Stride) * bmp.Height;
      byte[] rgbValues = new byte[length];

      progressBar1.Visible = true;
      int nbyteIm = Image.N_Bits / 8;
      for (int y = 0; y < bmp.Height; y++) //=================================================================
      {
        int jump = bmp.Height / progPart;
        if (y % jump == jump - 1) progressBar1.PerformStep();

        for (int x = 0; x < bmp.Width; x++)
        {
          Color color = Color.FromArgb(0, 0, 0); ;
          if (nbyteIm == 3)
          {
            color = Color.FromArgb(Image.Grid[2 + 3 * (x + Image.width * y)],
              Image.Grid[1 + 3 * (x + Image.width * y)], Image.Grid[0 + 3 * (x + Image.width * y)]);
          }
          else
          {
            color = Color.FromArgb(Image.Grid[x + Image.width * y],
              Image.Grid[x + Image.width * y], Image.Grid[x + Image.width * y]);
          }
          rgbValues[3 * x + Math.Abs(bmpData.Stride) * y + 0] = color.B;
          rgbValues[3 * x + Math.Abs(bmpData.Stride) * y + 1] = color.G;
          rgbValues[3 * x + Math.Abs(bmpData.Stride) * y + 2] = color.R;
        } //==================================== end for (int x ... =============================
      }  //===================================== end for (int y ... ===============================
      System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, length);
      bmp.UnlockBits(bmpData);
      return 1;
    } //****************************** end ImageToBitmapNew ****************************************



    public int Round(double x)
    {
      if (x < 0.0) return (int)(x - 0.5);
      return (int)(x + 0.5);
    }
    
    public void CorrectShading()
    { 
      int c, i, x, y;
      int[] color = {0, 0, 0};
      int[] color1 = {0, 0, 0};
      int Lightness=(int)numericUpDown2.Value;
      int hWind = (int)(numericUpDown1.Value * width / 2000);
      MeanIm.FastAverageM(GrayIm, hWind, this); // uses numericUpDown1
      progressBar1.Visible = true;
      progressBar1.Value = 0;
      pictureBox5.Visible = true;

      int[] histoSub = new int[256];
      int[] histoDiv = new int[256];
      for (i = 0; i < 256; i++) histoSub[i] = histoDiv[i] = 0;
      byte lum =  0;
      byte lum1 = 0;
      int jump = height / 17; // width and height are properties of Form1
      for (y = 0; y < height; y++) //==================================================
      {
        if (y % jump == jump - 1) progressBar1.PerformStep();
        for (x = 0; x < width; x++)
        {                               // nbyteIm is member of 'Form1'
          for (c = 0; c < nbyteIm; c++) //==============================================
          {
            color[c] = Round(SigmaIm.Grid[c + nbyteIm * (x + width * y)] * Lightness /
                                    (double)MeanIm.Grid[x + width * y]); // Division
            if (color[c] < 0) color[c] = 0;
            if (color[c] > 255) color[c] = 255;
            DivIm.Grid[c + nbyteIm * (x + width * y)] = (byte)color[c];

            color1[c] = SigmaIm.Grid[c + nbyteIm * (x + width * y)] + Lightness -
                                          MeanIm.Grid[x + width * y]; // Subtraction
            if (color1[c] < 0) color1[c] = 0;
            if (color1[c] > 255) color1[c] = 255;
            SubIm.Grid[c + nbyteIm * (x + width * y)] = (byte)color1[c];
          } //======================= end for (c... ==================================
          if (nbyteIm == 1) 
          {
            lum = (byte)color[0];
            lum1 = (byte)color1[0];
          }
          else 
          {
            lum = SigmaIm.MaxC((byte)color[2], (byte)color[1], (byte)color[0]);
            lum1 = SigmaIm.MaxC((byte)color1[2], (byte)color1[1], (byte)color1[0]);
          }
          histoDiv[lum]++;
          histoSub[lum1]++;
        }
      } //============================ end for (y... ===================================

      // Calculating  MinLight and MaxLight for 'Div':
      int MaxLightDiv, MaxLightSub, MinLightDiv, MinLightSub, Sum = 0;
      for (MinLightDiv = 0; MinLightDiv < 256; MinLightDiv++)
      {
        Sum += histoDiv[MinLightDiv];
        if (Sum > width * height / 100) break;
      }
      Sum = 0;
      for (MaxLightDiv = 255; MaxLightDiv >= 0; MaxLightDiv--)
      {
        Sum += histoDiv[MaxLightDiv];
        if (Sum > width * height / 100) break;
      }

      // Calculating  MinLight and MaxLight for 'Sub':
      Sum = 0;
      for (MinLightSub = 0; MinLightSub < 256; MinLightSub++)
      {
        Sum += histoSub[MinLightSub];
        if (Sum > width * height / 100) break;
      }
      Sum = 0;
      for (MaxLightSub = 255; MaxLightSub >= 0; MaxLightSub--)
      {
        Sum += histoSub[MaxLightSub];
        if (Sum > width * height / 100) break;
      }

      // Calculating LUT for 'Div':
      byte[] LUT = new byte[256];
      for (i = 0; i < 256; i++) 
        if (i <= MinLightDiv) LUT[i] = 0;
        else
          if (i > MinLightDiv && i <= MaxLightDiv)
            LUT[i] = (byte)(255 * (i - MinLightDiv) / (MaxLightDiv - MinLightDiv));
          else LUT[i] = 255;

      // Calculating LUTsub for 'Sub':
      byte[] LUTsub = new byte[256];
      for (i = 0; i < 256; i++) 
        if (i <= MinLightSub) LUTsub[i] = 0;
        else
          if (i > MinLightSub && i <= MaxLightSub)
            LUTsub[i] = (byte)(255 * (i - MinLightSub) / (MaxLightSub - MinLightSub));
          else LUTsub[i] = 255;

      // Calculating contrasted "Div" and "Sub":
      for (i = 0; i < 256; i++) histoDiv[i] = histoSub[i] = 0;
      jump = width * height / 17;
      for (i = 0; i < width * height; i++) //====================================
      {
        if (i % jump == jump - 1) progressBar1.PerformStep();

        for (c = 0; c < nbyteIm; c++)
        {
          DivIm.Grid[c + nbyteIm * i] = LUT[DivIm.Grid[c + nbyteIm * i]];
          SubIm.Grid[c + nbyteIm * i] = LUTsub[SubIm.Grid[c + nbyteIm * i]];
        }
       
        if (nbyteIm == 1) 
        {
          lum = DivIm.Grid[0 + nbyteIm * i];
          lum1 = SubIm.Grid[0 + nbyteIm * i]; 
        }
        else 
        {
          lum = SigmaIm.MaxC(DivIm.Grid[2 + nbyteIm * i], DivIm.Grid[1 + nbyteIm * i],
            DivIm.Grid[0 + nbyteIm * i]);
          lum1 = SigmaIm.MaxC(SubIm.Grid[2 + nbyteIm * i], SubIm.Grid[1 + nbyteIm * i],
            SubIm.Grid[0 + nbyteIm * i]);
        }
        histoDiv[lum]++;
        histoSub[lum1]++;
      } //========================== end for (i = 0; ... ==============================
      
      // Displaying the histograms:
      Bitmap BmpPictBox4 = new Bitmap(pictureBox4.Width, pictureBox4.Height);
      Graphics g4 = Graphics.FromImage(BmpPictBox4);
      pictureBox4.Image = BmpPictBox4;

      Bitmap BmpPictBox5 = new Bitmap(pictureBox5.Width, pictureBox5.Height);
      Graphics g5 = Graphics.FromImage(BmpPictBox5);

      pictureBox5.Image = BmpPictBox5;
      int MaxHisto1 = 0, SecondMax1 = 0;
      int MaxHisto = 0, SecondMax = 0;
      for (i = 0; i < 256; i++)
      {
        if (histoSub[i] > MaxHisto1) MaxHisto1 = histoSub[i];
        if (histoDiv[i] > MaxHisto) MaxHisto = histoDiv[i];
      }
      for (i = 0; i < 256; i++) if (histoSub[i] != MaxHisto1 && histoSub[i] > SecondMax1) SecondMax1 = histoSub[i];
      MaxHisto1 = SecondMax1 * 4 / 3;
      for (i = 0; i < 256; i++) if (histoDiv[i] != MaxHisto && histoDiv[i] > SecondMax) SecondMax = histoDiv[i];
      MaxHisto = SecondMax * 4 / 3;

      Pen redPen = new Pen(Color.Red), yellowPen = new Pen(Color.Yellow),
                          bluePen = new Pen(Color.Blue), greenPen = new Pen(Color.Green);
      SolidBrush whiteBrush = new SolidBrush(Color.White);
      Rectangle Rect1 = new Rectangle(0, 0, pictureBox4.Width, pictureBox4.Height);
      g4.FillRectangle(whiteBrush, Rect1);
      Rectangle Rect = new Rectangle(0, 0, pictureBox5.Width, pictureBox5.Height);
      g5.FillRectangle(whiteBrush, Rect);

      //Drawing the histograms:
      for (i = 0; i < 256; i++)
      {
        g4.DrawLine(redPen, i, pictureBox4.Height - histoSub[i] * 200 / MaxHisto1, i, pictureBox4.Height);
        g5.DrawLine(redPen, i, pictureBox5.Height - histoDiv[i] * 200 / MaxHisto, i, pictureBox5.Height);
      }
      // Vertical lines in histogram:
      for (i = 0; i < 256; i += 50)
      {
        g4.DrawLine(greenPen, i, pictureBox4.Height - 200, i, pictureBox4.Height);
        g5.DrawLine(greenPen, i, pictureBox5.Height - 200, i, pictureBox5.Height);
      }
      pictureBox4.Image = BmpPictBox4;
      pictureBox5.Image = BmpPictBox5;

     } //***************************** end CorrectShading **********************************************



    private void button2_Click(object sender, EventArgs e) // Shading correction
    {
      progressBar1.Value = 0;
      CorrectShading();
      //MessageBox.Show("pictB5.Visible=" + pictureBox5.Visible);
      Sub_Bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
      Div_Bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
      ImageToBitmapNew(SubIm, Sub_Bitmap, 33 );
      ImageToBitmapNew(DivIm, Div_Bitmap, 33 );

      progressBar1.Visible = false;
      pictureBox2.Image = Sub_Bitmap;
      pictureBox3.Image = Div_Bitmap;
      label3.Text = "If shading OK click threshold for 'SubIm'";
      label3.Visible = true;
      label4.Text = "If shading OK click threshold for 'DivIm'";
      label4.Visible = true;
      pictureBox4.Visible = true;
      pictureBox5.Visible = true;
      //MessageBox.Show("After = true; pictB5.Visible=" + pictureBox5.Visible);
      SHADING = true;
    } //*********************************** end Shading correction ****************************

    
    private void Save(SaveFileDialog dialog, string OpenImageFile, Bitmap Bmp)
    {
      string tmpFileName;
      if (dialog.FileName == OpenImageFile)
      {
        tmpFileName = OpenImageFile.Insert(OpenImageFile.IndexOf("."), "$$$");
        if (dialog.FileName.Contains(".jpg"))
          Bmp.Save(tmpFileName, ImageFormat.Jpeg); // saving tmpFile
        else
          if (dialog.FileName.Contains(".bmp")) Bmp.Save(tmpFileName, ImageFormat.Bmp);
          else
          {
            MessageBox.Show("The file " +  dialog.FileName + " has an inappropriate extension. Returning.");
            return;
          }

        original_Bitmap.Dispose();
        File.Replace(tmpFileName, OpenImageFile,
                      OpenImageFile.Insert(OpenImageFile.IndexOf("."), "BackUp"));
        original_Bitmap = new Bitmap(OpenImageFile);
        pictureBox1.Image = original_Bitmap;
      }
      else
      {
        if (dialog.FileName.Contains(".jpg"))
          Bmp.Save(dialog.FileName, ImageFormat.Jpeg);
        else 
          if (dialog.FileName.Contains(".bmp")) Bmp.Save(dialog.FileName, ImageFormat.Bmp);
          else
          {
            MessageBox.Show("The file " + dialog.FileName + " has an inappropriate extension. Returning.");
            return;
          }
      }
      MessageBox.Show("The result image saved under " + dialog.FileName);
    } //********************** end Save ************************************************


    private void button4_Click(object sender, EventArgs e) // Save Div
    {
      if (!SHADING)
      {
        MessageBox.Show("Please press the button 'Shading'");
      }

      SaveFileDialog dialog = new SaveFileDialog();
      if (dialog.ShowDialog() == DialogResult.OK)
      {
        //Save(dialog, OpenImageFile, Div_Bitmap);
      }
      //button4.Visible = false;
      //label4.Visible = false;
    }


    private void pictureBox4_MouseClick(object sender, MouseEventArgs e) // Thresholding Sub
    {
      if (!SHADING)
      {
        MessageBox.Show("Please click the button 'Shading correction'");
        return;
      }
      Threshold1 = e.X;
      Graphics g = pictureBox4.CreateGraphics();
      Pen bluePen = new Pen(Color.Blue);
      g.DrawLine(bluePen, Threshold1, 0, Threshold1, pictureBox4.Height);
      progressBar1.Visible = true;
      progressBar1.Value = 0;
      int nbyte = SubIm.N_Bits / 8;
      int jump = height / 100;
      for (int y = 0; y < height; y++)
      {
        if (y % jump == jump - 1) progressBar1.PerformStep();
        for (int x = 0; x < width; x++)
        {
          int i = x + width * y;
          if (nbyte == 1)
          {
            if (SubIm.Grid[i] > Threshold1) BinIm.Grid[i] = 255;
            else BinIm.Grid[i] = 0;
          }
          else
          {
            if (SubIm.MaxC(SubIm.Grid[2 + 3*i], SubIm.Grid[1 + 3*i], SubIm.Grid[0 + 3*i]) > Threshold1) BinIm.Grid[i] = 255;
            else BinIm.Grid[i] = 0;
          }
          Sub_Bitmap.SetPixel(x, y, Color.FromArgb(BinIm.Grid[i], BinIm.Grid[i], BinIm.Grid[i]));
        }
      }
      pictureBox2.Image = Sub_Bitmap;
      button3.Visible = true;
      label3.Text = "If threshold OK click 'Save subtraction'";
      label3.Visible = true;
      Threshold1 = -1;
      button3.Visible = true;
      progressBar1.Visible = false;

    } //******************************* end pictureBox4_MouseClick ***************************************


    private void pictureBox5_MouseClick(object sender, MouseEventArgs e) // Thresholding DivIm
    {
      if (!SHADING)
      {
        MessageBox.Show("Please click the button 'Shading correction'");
        return;
      }
      Threshold = e.X;
      Graphics g = pictureBox5.CreateGraphics();
      Pen bluePen = new Pen(Color.Blue);
      g.DrawLine(bluePen, Threshold, 0, Threshold, pictureBox5.Height);
      progressBar1.Visible = true;
      progressBar1.Value = 0;
      int nbyte = DivIm.N_Bits / 8;
      int jump = height / 100;
      for (int y = 0; y < height; y++)
      {
        if (y % jump == jump - 1) progressBar1.PerformStep();
        for (int x = 0; x < width; x++)
        {
          int i = x + width * y;
          if (nbyte == 1)
          {
            if (DivIm.Grid[i] > Threshold) BinIm.Grid[i] = 255;
            else BinIm.Grid[i] = 0;
          }
          else
          {
            if (DivIm.MaxC(DivIm.Grid[2 + 3*i], DivIm.Grid[1 + 3*i], DivIm.Grid[0 + 3*i]) > Threshold) BinIm.Grid[i] = 255;
            else BinIm.Grid[i] = 0;
          }
          Div_Bitmap.SetPixel(x, y, Color.FromArgb(BinIm.Grid[i], BinIm.Grid[i], BinIm.Grid[i]));
        }
      }
      pictureBox3.Image = Div_Bitmap;
      button4.Visible = true;
      label4.Text = "If threshold OK click 'Save division'";
      label4.Visible = true;

      Threshold = -1;
      button4.Visible = true;
      progressBar1.Visible = false;
    } //*************************** end pictureBox5_MouseClick ************************************


    private void button3_Click(object sender, EventArgs e) // Save sub
    {
      SaveFileDialog dialog = new SaveFileDialog();
      if (dialog.ShowDialog() == DialogResult.OK)
      {
        Save(dialog, OpenImageFile, Sub_Bitmap);
      }
      button3.Visible = false;
      label3.Visible = false;

    } //****************************** end Save sub ************************************
  } //******************************** end class Form1 ******************************************
} //********************************** end namespace **********************************************
