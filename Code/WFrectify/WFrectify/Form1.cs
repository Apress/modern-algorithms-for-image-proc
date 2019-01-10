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

namespace WFrectify
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }
    public Bitmap OrigBmp;
    private Bitmap ResultBmp; // result of processing
    public Bitmap BmpPictBox1; // for Graphics
    public Bitmap BmpPictBox2; // for Graphics
    CImage OrigIm;  // copy of original image
    CImage ResultIm;  // grayscale image for calculating Mean
    public Point[] v = new Point[4]; // corners of marked rectangle containing the painting
    int Number, // number of defined elements "v" with coordinates of the corners
      maxNumber = 4;
    double Rel = 0.1;
    bool Drawn = false, Indexed = false; 
    double Scale1;
    int marginX, marginY;
    Graphics g1, g2;


    bool CUT = true;
    bool OPEN = false, CLICK = false, CORNERS = false, CUTCHOSEN = false, RECTIFIED = false;
    string OpenImageFile;

    private void button1_Click(object sender, EventArgs e) // Open image
    {
      label1.Visible = false;
      label2.Visible = false;
      label3.Visible = false;

      OpenFileDialog openFileDialog1 = new OpenFileDialog();
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        try
        {
          OrigBmp = new Bitmap(openFileDialog1.FileName);
          OpenImageFile = openFileDialog1.FileName;
          Number = 0;
        }
        catch (Exception ex)
        {
          MessageBox.Show("Error: Could not read file from disk. Original error: " +
           ex.Message);
        }
      }
      else return;

      double ScaleX = (double)pictureBox1.Width / OrigBmp.Width;
      double ScaleY = (double)pictureBox1.Height / OrigBmp.Height;
      if (ScaleX < ScaleY) Scale1 = ScaleX;
      else Scale1 = ScaleY;
      Drawn = false;

      progressBar1.Maximum = 100;
      progressBar1.Value = 0;
      progressBar1.Step = 1;
      progressBar1.Visible = true;

      label1.Text = "Opened image: " + OpenImageFile;
      label1.Visible = true;
 
      OrigIm = new CImage(OrigBmp.Width, OrigBmp.Height, 24);

      OrigIm.denomProg = progressBar1.Maximum / progressBar1.Step;
      OrigIm.nLoop = 2;
      if (OrigBmp.PixelFormat == PixelFormat.Format8bppIndexed)
      {
        OrigIm.BitmapToImageOld(OrigBmp, this);
        Indexed = true;
      }
      else
        if (OrigBmp.PixelFormat == PixelFormat.Format24bppRgb)
        {
          OrigIm.BitmapToImage(OrigBmp, this);
          Indexed = false;
        }
        else
        {
          MessageBox.Show("Inappropriate pixel format=" + OrigBmp.PixelFormat);
          return;
        }

      BmpPictBox1 = new Bitmap(OrigBmp.Width, OrigBmp.Height, PixelFormat.Format24bppRgb);
      if (Indexed)
        OrigIm.ImageToBitmapOld(BmpPictBox1, this);
      else
        OrigIm.ImageToBitmap(BmpPictBox1, this);

      pictureBox1.Image = BmpPictBox1;
      g1 = Graphics.FromImage(BmpPictBox1);


      BmpPictBox2 = new Bitmap(OrigBmp.Width, OrigBmp.Height, PixelFormat.Format24bppRgb);
      pictureBox2.Image = BmpPictBox2;
      g2 = Graphics.FromImage(BmpPictBox2);



      pictureBox1.Image = OrigBmp;

      progressBar1.Visible = false;
      progressBar1.Maximum = 100;
      progressBar1.Step = 1;
      OrigIm.denomProg = progressBar1.Maximum / progressBar1.Step;
      ResultIm = new CImage(OrigBmp.Width, OrigBmp.Height, 24);

      marginX = (pictureBox1.Width - (int)(Scale1 * OrigIm.width)) / 2; // space left of the image
      marginY = (pictureBox1.Height - (int)(Scale1 * OrigIm.height)) / 2; // space above the image
      label2.Visible = true;
      OPEN = true;
      CLICK = false;
   } //********************************* end Open image *********************************


    public void TransformToImage(Point[] v)
    // Transforming the clicked coordinates to image coordinates and correcting with "w[4]"
    {
      iVect2[] w = new iVect2[4];
      int[] iw = { 5, 4, 7, 11, 0, 0, -1, 3 };
      int i;
      for (i = 0; i < 4; i++)
      {
        w[i] = new iVect2();
        w[i].X = iw[2 * i];
        w[i].Y = iw[2 * i + 1];
      }

      for (i = 0; i < 4; i++)
      {
        v[i].X = (int)((v[i].X - marginX) / Scale1) + w[i].X; 
        v[i].Y = (int)((v[i].Y - marginY) / Scale1) + w[i].Y; 
      }
      Pen redPen = new Pen(Color.Red, 3);
      g1.DrawLine(redPen, v[0].X, v[0].Y, v[1].X, v[1].Y);
      g1.DrawLine(redPen, v[1].X, v[1].Y, v[2].X, v[2].Y);
      g1.DrawLine(redPen, v[2].X, v[2].Y, v[3].X, v[3].Y);
      g1.DrawLine(redPen, v[0].X, v[0].Y, v[3].X, v[3].Y); 

    } //********************************** end TransformToImage *******************************


    private void DrawPictureBox()
    // Drawing the white right angle for the quater(Number) of the painting
    {
      SolidBrush myBrush1; 
      Rectangle rect;
      int Size = OrigBmp.Width / 100;
      myBrush1 = new System.Drawing.SolidBrush(Color.Red);

      Pen myPen, penDelete, redPen;
      myPen = new System.Drawing.Pen(Color.White, 2);
      penDelete = new System.Drawing.Pen(Color.Gray);
      redPen = new Pen(Color.Red);
      //MessageBox.Show("In DrawPictureBox: Number=" + Number);
      int nn = 0;
      switch (Number)
      {
        case 0:
          g1.DrawLine(myPen, OrigBmp.Width / 2, OrigBmp.Height / 2, 0, OrigBmp.Height /2); // center, left                        
          g1.DrawLine(myPen, OrigBmp.Width / 2, OrigBmp.Height / 2, OrigBmp.Width / 2, OrigBmp.Height); // center, bottom                           
          pictureBox1.Image = BmpPictBox1; //Refresh();
          //MessageBox.Show("In DrawPictureBox: Number=" + Number + " drawn corner 0 myPen=" + myPen + " OrigBmp.Width=" + OrigBmp.Width);
          break;
        case 1:
          g1.DrawLine(penDelete, OrigBmp.Width / 2, OrigBmp.Height / 2, OrigBmp.Width / 2, OrigBmp.Height); // del. center, bottom                           
          for (int n = 0; n < Number; n++)
          {
            nn = n;
            rect = new Rectangle((int)((v[n].X - marginX) / Scale1) - 2, (int)((v[n].Y - marginY) / Scale1) - 2, Size, Size);
            g1.FillRectangle(myBrush1, rect);
          }
          g1.DrawLine(myPen, OrigBmp.Width / 2, OrigBmp.Height / 2, 0, OrigBmp.Height /2); // center, left                       
          g1.DrawLine(myPen, OrigBmp.Width / 2, OrigBmp.Height / 2, OrigBmp.Width / 2, 0); // center, top
          pictureBox1.Image = BmpPictBox1; //Refresh();
          break;
        case 2:
          g1.DrawLine(penDelete, OrigBmp.Width / 2, OrigBmp.Height / 2, 0, OrigBmp.Height /2); // del. center, left
          for (int n = 0; n < Number; n++)
          {
            nn = n;
            rect = new Rectangle((int)((v[n].X - marginX) / Scale1) - 2, (int)((v[n].Y - marginY) / Scale1) - 2, Size, Size);
            g1.FillRectangle(myBrush1, rect);
          }
          g1.DrawLine(myPen, OrigBmp.Width / 2, OrigBmp.Height / 2, OrigBmp.Width, OrigBmp.Height /2); // center, right
          g1.DrawLine(myPen, OrigBmp.Width / 2, OrigBmp.Height / 2, OrigBmp.Width / 2, 0); // center, top
          pictureBox1.Image = BmpPictBox1; //Refresh();
          break;
        case 3:
          g1.DrawLine(penDelete, OrigBmp.Width / 2, OrigBmp.Height / 2, OrigBmp.Width / 2, 0); // del. center, top 
          for (int n = 0; n < Number; n++)
          {
            rect = new Rectangle((int)((v[n].X - marginX) / Scale1) - 2, (int)((v[n].Y - marginY) / Scale1) - 2, Size, Size);
            g1.FillRectangle(myBrush1, rect);
          }
          g1.DrawLine(myPen, OrigBmp.Width / 2, OrigBmp.Height / 2, OrigBmp.Width, OrigBmp.Height /2); // center, right
          g1.DrawLine(myPen, OrigBmp.Width / 2, OrigBmp.Height / 2, OrigBmp.Width / 2, OrigBmp.Height); // center, bottom
          pictureBox1.Image = BmpPictBox1; //Refresh();
          break;
        case 4:
          for (int n = 0; n < Number; n++)
          {
            rect = new Rectangle((int)((v[n].X - marginX) / Scale1) - 2, (int)((v[n].Y - marginY) / Scale1) - 2, Size, Size);
            g1.FillRectangle(myBrush1, rect);
          }
          g1.DrawLine(penDelete, OrigBmp.Width / 2, OrigBmp.Height / 2, OrigBmp.Width, OrigBmp.Height /2); // del. center, right
          g1.DrawLine(penDelete, OrigBmp.Width / 2, OrigBmp.Height / 2, OrigBmp.Width / 2, OrigBmp.Height); // del. center, bottom
          pictureBox1.Image = BmpPictBox1; //Refresh();
          break;
      }
    } //******************************** end DrawPictureBox *****************************************



    private void pictureBox1_MouseClick(object sender, MouseEventArgs e) // MouseClick
    // Saving the coordinates of the corner[Number].
    {
      if (!OPEN)
      {
        MessageBox.Show("Please click 'Open image' and choose an image");
        return;
      }
      if (!CLICK)
      {
        MessageBox.Show("Please click the button 'Click corners'.");
        return;
      }
      int X = e.X;
      int Y = e.Y;
      if (Number == 0 && (X > pictureBox1.Width / 2 || Y < pictureBox1.Height / 2))
      {
        MessageBox.Show("The first point should be left from center and below the center. Try again.");
        return;
      }

      if (Number == 1 && (X > pictureBox1.Width / 2 || Y > pictureBox1.Height / 2))
      {
        MessageBox.Show("The second point should be left from center and above the center. Try again.");
        return;
      }

      if (Number == 2 && (X < pictureBox1.Width / 2 || Y > pictureBox1.Height / 2))
      {
        MessageBox.Show("The third point should be right from center and above the center. Try again.");
        return;
      }

      if (Number == 3 && (X < pictureBox1.Width / 2 || Y < pictureBox1.Height / 2))
      {
        MessageBox.Show("The fourth point should be right from center and below the center. Try again.");
        return;
      }

      v[Number].X = X;
      v[Number].Y = Y;
      if (Number < maxNumber)
        Number++;
      else
        MessageBox.Show("Number=" + Number + " is too large");

      DrawPictureBox(); // Drawing the white right angle for the quater(Number) of the painting
     
      if (Number == 0)
      {         
        MessageBox.Show("Click to the corners");
        return;
      }

      SolidBrush myBrush1; 
      Rectangle rect; 
      myBrush1 = new System.Drawing.SolidBrush(Color.Red);
      Pen redPen = new Pen(Color.Red);
      for (int n = 0; n < Number; n++)
      {
        rect = new Rectangle(v[n].X-2, v[n].Y-2, 4, 4);
        g1.FillRectangle(myBrush1, rect);
      }

      if (Number == 4)
      {
        TransformToImage(v); // Transforming 'v' to image coordinates and correcting
        Drawn = true;
        CORNERS = true;
      }
    } //**************************** end MouseClick *********************************************



    private void button2_Click(object sender, EventArgs e)  // Straighten
    {
      if (!CLICK)
      {
        MessageBox.Show("Click the button 'Click corners' and clockwise the corners of the painting starting with the lower left one");
        return;
      }

      if (Number != 4 || !CORNERS)
      {
        MessageBox.Show("Click clockwise the corners of the painting starting with the lower-left one");
        return;
      }

      OrigIm.nLoop = 2;
      ResultIm.denomProg = 100;
      ResultIm.nLoop = 2;

      if (radioButton1.Checked)
      {
        CUT = true;
        CUTCHOSEN = true;
      }
      else
        if (radioButton2.Checked) 
        {
          CUT = false;
          CUTCHOSEN = true;
        }

      if (!CUTCHOSEN)
      {
        MessageBox.Show("Decide whether removing the background");
        return;
      }

      if (CUT)
        OrigIm.Rect_Optimal(v, CUT, ref ResultIm);
      else
      {
        if (radioButton3.Checked) Rel = 0.1;
        if (radioButton4.Checked) Rel = 0.15;
        if (radioButton5.Checked) Rel = 0.20;
        OrigIm.Rect_Retain(v, CUT, Rel, ref ResultIm); // 'Rel' is the part of background
      }

      ResultBmp = new Bitmap(ResultIm.width, ResultIm.height, PixelFormat.Format24bppRgb);
      ResultIm.ImageToBitmap(ResultBmp, this);
      progressBar1.Visible = false;

      pictureBox2.Image = ResultBmp;
      BmpPictBox1 = new Bitmap(ResultIm.width, ResultIm.height, PixelFormat.Format24bppRgb);
      label3.Visible = true;

      RECTIFIED = true;
    } //******************************* end Straighten *************************************


    private void button3_Click(object sender, EventArgs e) // Save result
    {
      if (!CLICK)
      {
        MessageBox.Show("Click the button 'Click corners' and clockwise the corners of the painting starting with the lower left one");
        return;
      }
      if (!CORNERS)
      {
        MessageBox.Show("Click clockwise the corners of the painting starting with the lower left one");
        return;
      }
      if (!RECTIFIED)
      {
        MessageBox.Show("Please click the button 'Straighten'");
        return;
      }
      SaveFileDialog dialog = new SaveFileDialog();
      if (dialog.ShowDialog() == DialogResult.OK) 
      {
        string tmpFileName;
        if (dialog.FileName == OpenImageFile)
        {
          tmpFileName = OpenImageFile.Insert(OpenImageFile.IndexOf("."), "$$$");
          if (dialog.FileName.Contains(".jpg"))
            ResultBmp.Save(tmpFileName, ImageFormat.Jpeg); // saving tmpFile
          else 
            if (dialog.FileName.Contains(".bmp")) ResultBmp.Save(tmpFileName, ImageFormat.Bmp);
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
        else
        {
          if (dialog.FileName.Contains(".jpg"))
            ResultBmp.Save(dialog.FileName, ImageFormat.Jpeg);
          else 
            if (dialog.FileName.Contains(".bmp")) ResultBmp.Save(dialog.FileName, ImageFormat.Bmp);
            else
            {
              MessageBox.Show("The file " + dialog.FileName + " has an inappropriate extension. Returning.");
              return;
            }
        }
        MessageBox.Show("The result image saved under " + dialog.FileName);
      }

    } //******************************* end Save result *************************


    private void button4_Click(object sender, EventArgs e) // Click corners
    {
      CLICK = true;
      pictureBox1.Image = OrigBmp;
      Pen myPen = new Pen(Color.White);
      if (Drawn)
      {
        MessageBox.Show("Open an image");
        return;
      }
      if (Drawn) // "Drawn" means all four corners are clicked
      {
        Number = 0;
        Drawn = false;
      }

      Number = 0;
      DrawPictureBox(); // Drawing the white right angle for the quater(0) of the painting
    }
  }
}
