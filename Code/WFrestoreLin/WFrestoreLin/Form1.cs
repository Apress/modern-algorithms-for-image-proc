using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Xml.Serialization;



namespace WFrestoreLin
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }
    string OpenFileName;

    public struct iVect2
    {
      public
      int X, Y;
      public iVect2(int x, int y) // constructor
      {
        this.X = x;
        this.Y = y;
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
    } //********************* end class iVect2 ***************************

    
    class CLine // Line in a color image; size=14 bytes
    {
      public int EndByte; // 4 bytes
      public ushort x, y, nCrack;  // 6 bytes
      public byte Ind0, Ind1, Ind2, Ind3; // 4 bytes
    } //**************** end class CLine ***************************


    class CCrack // Short line in a gray value image; size=6 bytes
    {
      public ushort x, y;	// 4 bytes; in bit 15 of "x" and "y" is "dir" encoded
      public byte Ind0, Ind1; // 2 bytes
    } //**************** end class CCrack ***************************************


    public class CListCode // The class for the final list
    {
      public
      int width, height, nBits, nLine1, nLine2, nByte;
      int[] Palette;
      byte[] Corner;
      CCrack[] Line1;
      CLine[] Line2;
      byte[] Byte;
      iVect2[] Step;

        
      public CListCode(byte[] ByteNew, Form1 fm1) // Constructor
      {
        int j = 0;
        this.width = ByteNew[j]; j++;
        this.width |= ByteNew[j] << 8; j++;
        this.width |= ByteNew[j] << 16; j++;
        this.width |= ByteNew[j] << 24; j++;

        this.height = ByteNew[j]; j++;
        this.height |= ByteNew[j] << 8; j++;
        this.height |= ByteNew[j] << 16; j++;
        this.height |= ByteNew[j] << 24; j++;

        this.nBits = ByteNew[j]; j++;
        this.nBits |= ByteNew[j] << 8; j++;
        this.nBits |= ByteNew[j] << 16; j++;
        this.nBits |= ByteNew[j] << 24; j++;
        int b = j;
        this.nLine1 = ByteNew[j]; j++;
        this.nLine1 |= ByteNew[j] << 8; j++;
        this.nLine1 |= ByteNew[j] << 16; j++;
        this.nLine1 |= ByteNew[j] << 24; j++;

        this.nLine2 = ByteNew[j]; j++;
        this.nLine2 |= ByteNew[j] << 8; j++;
        this.nLine2 |= ByteNew[j] << 16; j++;
        this.nLine2 |= ByteNew[j] << 24; j++;

        this.nByte = ByteNew[j]; j++;
        this.nByte |= ByteNew[j] << 8; j++;
        this.nByte |= ByteNew[j] << 16; j++;
        this.nByte |= ByteNew[j] << 24; j++;

        int m = j + 100;
        this.Palette = new int[256];
        for (int ip = 0; ip < 256; ip++)
        {
          this.Palette[ip] = ByteNew[j]; j++;
          this.Palette[ip] |= ByteNew[j] << 8; j++;
          this.Palette[ip] |= ByteNew[j] << 16; j++;
          this.Palette[ip] |= ByteNew[j] << 24; j++;
        }

        this.Corner = new byte[4];
        for (int i1 = 0; i1 < 4; i1++)
          this.Corner[i1] = ByteNew[j + i1];
        j += 4;

        this.Line1 = new CCrack[nLine1];
        for (int i1 = 0; i1 < nLine1; i1++) this.Line1[i1] = new CCrack();

        fm1.progressBar1.Visible = true;
        int denomProg = fm1.progressBar1.Maximum / fm1.progressBar1.Step;
        int Sum = nLine1 + nLine2 + nByte;
        int k = Sum / denomProg;
        for (int i1 = 0; i1 < nLine1; i1++)
        {
          if ((i1 % k) == 0) fm1.progressBar1.PerformStep();
          this.Line1[i1].x = (ushort)ByteNew[j]; j++;
          this.Line1[i1].x |= (ushort)(ByteNew[j]<<8); j++;
          this.Line1[i1].y = (ushort)ByteNew[j]; j++;
          this.Line1[i1].y |= (ushort)(ByteNew[j]<<8); j++;
          this.Line1[i1].Ind0 = ByteNew[j]; j++;
          this.Line1[i1].Ind1 = ByteNew[j]; j++;
        }

        this.Line2 = new CLine[nLine2];
        for (int i2 = 0; i2 < nLine2; i2++) this.Line2[i2] = new CLine();
          
        for (int i2 = 0; i2 < nLine2; i2++)
        {
          if ((i2 % k) == 0) fm1.progressBar1.PerformStep();
          this.Line2[i2].EndByte = (int)ByteNew[j]; j++;
          this.Line2[i2].EndByte |= (int)(ByteNew[j]<<8); j++;
          this.Line2[i2].EndByte |= (int)(ByteNew[j]<<16); j++;
          this.Line2[i2].EndByte |= (int)(ByteNew[j]<<24); j++;
          this.Line2[i2].x = (ushort)ByteNew[j]; j++;
          this.Line2[i2].x |= (ushort)(ByteNew[j]<<8); j++;
          this.Line2[i2].y = (ushort)ByteNew[j]; j++;
          this.Line2[i2].y |= (ushort)(ByteNew[j]<<8); j++;
          this.Line2[i2].nCrack = (ushort)ByteNew[j]; j++;
          this.Line2[i2].nCrack |= (ushort)(ByteNew[j]<<8); j++;
          this.Line2[i2].Ind0 = ByteNew[j]; j++;
          this.Line2[i2].Ind1 = ByteNew[j]; j++;
          this.Line2[i2].Ind2 = ByteNew[j]; j++;
          this.Line2[i2].Ind3 = ByteNew[j]; j++;
        }

        int p = j;
        this.Byte = new byte[nByte];
        for (int i3 = 0; i3 < nByte; i3++)
        {
          if ((i3 % k) == 0) fm1.progressBar1.PerformStep();
          this.Byte[i3] = ByteNew[j + i3];
        }
        fm1.progressBar1.Visible = false;
        j += nByte;

        this.Step = new iVect2[4];
        for (int i = 0; i < 4; i++) Step[i] = new iVect2();
        this.Step[0].X = 1; this.Step[0].Y = 0;
        this.Step[1].X = 0; this.Step[1].Y = 1;
        this.Step[2].X = -1; this.Step[2].Y = 0;
        this.Step[3].X = 0; this.Step[3].Y = -1;
      } //***************************** end constructor **************************************


      public int Restore(ref CImage Image, ref CImage Mask, Form1 fm1)
      { int dir, nbyte, x, y;
	      byte LabMask=250;
	      if (nBits==24) nbyte=3;
	      else nbyte=1;

        fm1.progressBar1.Value = 0;
        fm1.progressBar1.Visible = true;
        for (int i = 0; i < width * height * (nBits / 8); i++) Image.Grid[i] = 0;
        
	      for (int i=0; i<width*height; i++) Mask.Grid[i]=0;

	      if (nBits==24)
	      {	for (int c=0; c<nbyte; c++) 
          { Image.Grid[c]=(byte)((Palette[Corner[0]]>>8*(2-c)) & 0XFF); // left below
		        Image.Grid[nbyte*(width-1)+c]=(byte)((Palette[Corner[1]]>>8*(2-c)) & 0XFF); // right below
		        Image.Grid[nbyte*width*height-nbyte+c]=(byte)((Palette[Corner[2]]>>8*(2-c)) & 0XFF); // right on top
		        Image.Grid[nbyte*width*(height-1)+c]=(byte)((Palette[Corner[3]]>>8*(2-c)) & 0XFF); // left on top
          }
	      }
	      else
	      { Image.Grid[0]=Corner[0];
	        Image.Grid[width-1]=Corner[1];
	        Image.Grid[width*height-1]=Corner[2];
	        Image.Grid[0+width*(height-1)]=Corner[3];
	      }
	      Mask.Grid[0]=Mask.Grid[width-1]=Mask.Grid[width*height-1]=Mask.Grid[width*(height-1)]=LabMask;

	      // Short lines:
        fm1.progressBar1.Maximum = 100;
        fm1.progressBar1.Step = 1;
        fm1.progressBar1.Value = 0;
        int jump, Len = nLine1, nStep = 20;
        if (Len > 2 * nStep) jump = Len / nStep;
        else jump = 2;
        for (int il = 0; il < nLine1; il++) //=====================================================
        {
          if ((il % jump) == jump - 1) fm1.progressBar1.PerformStep();
          dir=((Line1[il].x>>14) & 2) | (Line1[il].y>>15);
		      x=Line1[il].x & 0X7FFF;			y=Line1[il].y & 0X7FFF;
		      if (nBits==24)
		      { switch(dir)
            { case 0: if (y > 0)
              {
                for (int c = 0; c < nbyte; c++)
                {
                  int Index = Line1[il].Ind1;
                  byte col = (byte)(Palette[Index] >> 8 * c);
                  Image.Grid[nbyte * (x + width * y) + 2 - c] = col;
                  Image.Grid[nbyte * (x + width * (y - 1)) + 2 - c] = (byte)((Palette[Line1[il].Ind0] >> 8 * c) & 0XFF);
                }
                Mask.Grid[x + width * y] = Mask.Grid[x + width * (y - 1)] = LabMask;
              }
								      break;
              case 1: for (int c = 0; c < nbyte; c++) 
								      { Image.Grid[nbyte*(x+width*y)+2-c]=(byte)((Palette[Line1[il].Ind0]>>8*c) & 0XFF);
									      Image.Grid[nbyte*(x-1+width*y)+2-c]=(byte)((Palette[Line1[il].Ind1]>>8*c) & 0XFF);
								      }
								      Mask.Grid[x+width*y]=Mask.Grid[x-1+width*y]=LabMask;
								      break;
              case 2: for (int c = 0; c < nbyte; c++) 
								      { Image.Grid[nbyte*(x-1+width*y)+2-c]=(byte)((Palette[Line1[il].Ind0]>>8*c) & 0XFF);
									      Image.Grid[nbyte*(x-1+width*(y-1))+2-c]=(byte)((Palette[Line1[il].Ind1]>>8*c) & 0XFF);
								      }
								      Mask.Grid[x-1+width*y]=Mask.Grid[x-1+width*(y-1)]=LabMask;
								      break;
              case 3: for (int c = 0; c < nbyte; c++) 
								      { Image.Grid[nbyte*(x+width*(y-1))+2-c]=(byte)((Palette[Line1[il].Ind1]>>8*c) & 0XFF);
									      Image.Grid[nbyte*(x-1+width*(y-1))+2-c]=(byte)((Palette[Line1[il].Ind0]>>8*c) & 0XFF);
								      }
								      Mask.Grid[x+width*(y-1)]=Mask.Grid[x-1+width*(y-1)]=LabMask;
								      break;
            } //::::::::::::: end switch ::::::::::::::::::::::::::::::::::::::::::
		      }
		      else
		      { switch(dir)
			      {	case 0: Image.Grid[x+width*y]=Line1[il].Ind1;
								      Image.Grid[x+width*(y-1)]=Line1[il].Ind0;
								      Mask.Grid[x+width*y]=Mask.Grid[x+width*(y-1)]=LabMask;
								      break;
			 	      case 1: Image.Grid[x+width*y]=Line1[il].Ind0;
								      Image.Grid[x-1+width*y]=Line1[il].Ind1;
								      Mask.Grid[x+width*y]=Mask.Grid[x-1+width*y]=LabMask;
								      break;
			 	      case 2: Image.Grid[x-1+width*y]=Line1[il].Ind0;
								      Image.Grid[x-1+width*(y-1)]=Line1[il].Ind1;
								      Mask.Grid[x-1+width*y]=Mask.Grid[x-1+width*(y-1)]=LabMask;
								      break;
			 	      case 3: Image.Grid[x+width*(y-1)]=Line1[il].Ind1;
								      Image.Grid[x-1+width*(y-1)]=Line1[il].Ind0;
								      Mask.Grid[x+width*(y-1)]=Mask.Grid[x-1+width*(y-1)]=LabMask;
								      break;
			      } //::::::::::::: end switch ::::::::::::::::::::::::::::::::::::::::::
          } //--------------- end if (nBits==24) ------------------------------------------
	      } //================= end for (il < nLine1 ==========================================
 
        int first, last;
	      int[] Shift = new int[]{0,2,4,6};
        Len = nLine2;
        nStep = 20;
        if (Len > 2 * nStep) jump = Len / nStep;
        else jump = 2;
	      for (int il = 0; il < nLine2; il++) //====================================================================================================
        {
          if ((il % jump) == jump - 1) fm1.progressBar1.PerformStep(); 
          if (il==0) first=0;
		      else first=Line2[il-1].EndByte+1;

		      last=Line2[il].EndByte;
		      x=Line2[il].x;
		      y=Line2[il].y;
		      int iByte=first, iShift=0;
          iVect2 P = new iVect2(), PixelP = new iVect2(), PixelN = new iVect2(); // comb. coordinates
          
		      byte[] ColN = new byte[3], ColP = new byte[3];
          byte[] ColStartN = new byte[3], ColStartP = new byte[3], 
                    ColLastN = new byte[3], ColLastP = new byte[3]; // Colors
		      for (int c=0; c<3; c++) 
            ColN[c]=ColP[c]=ColStartN[c]=ColStartP[c]=ColLastN[c]=ColLastP[c]=0;

		      if (nBits==24)
		      { for (int c=0; c<nbyte; c++)
			      { ColStartN[2-c]=(byte)((Palette[Line2[il].Ind0]>>8*c) & 255);
				      ColStartP[2-c]=(byte)((Palette[Line2[il].Ind1]>>8*c) & 255);
				      ColLastN[2-c]= (byte)((Palette[Line2[il].Ind2]>>8*c) & 255);
				      ColLastP[2-c]= (byte)((Palette[Line2[il].Ind3]>>8*c) & 255);
			      }
		      }
		      else
		      { ColStartN[0]=Line2[il].Ind0;
		        ColStartP[0]=Line2[il].Ind1;
		        ColLastN[0]=Line2[il].Ind2;
		        ColLastP[0]=Line2[il].Ind3;
		      }
	        P.X=Line2[il].x; P.Y=Line2[il].y;
	
		      int nCrack=Line2[il].nCrack;
		      int xx, yy;		
          // Interpolation:
		      for (int iC=0; iC<nCrack; iC++) //=======================================================
		      { dir=(Byte[iByte] & (3<<Shift[iShift]))>>Shift[iShift];
			      switch(dir) // Standard coordinates
			      {	case 0: PixelP=P; PixelN=P+Step[3];	break;
			 	      case 1: PixelP=P+Step[2]; PixelN=P;	break;
			 	      case 2: PixelP=P+Step[2]+Step[3]; PixelN=P+Step[2];	break;
			 	      case 3: PixelP=P+Step[3]; PixelN=P+Step[2]+Step[3]; break;
			      }
			      if (PixelP.Y<0 || PixelN.Y<0 || PixelP.Y>height-1 || PixelN.Y>height-1)
			      { MessageBox.Show("Restore: Bad 'PixelP' or 'PixelN'. This means 'Byte' is bad. iByte=" + 
              iByte + "; dir=" + dir + "; Byte=" + Byte[iByte]);
              MessageBox.Show("Restore: PixelP=(" + PixelP.X + "," + PixelP.Y + "); PixelN=(" + PixelN.X + ","
              + PixelN.Y + "); P=(" + P.X +"," + P.Y + ")");
			      }
			      for (int c=0; c<nbyte; c++) //===================================================
			      {	ColN[c]=(byte)((ColLastN[c]*iC+ColStartN[c]*(nCrack-iC-1))/(nCrack-1)); // Interpolation
				      ColP[c]=(byte)((ColLastP[c]*iC+ColStartP[c]*(nCrack-iC-1))/(nCrack-1));
			      } //========================== end for (c... ================================

		        // Assigning colors to intermediate pixels of a line:
			      xx=PixelP.X; yy=PixelP.Y;
			      if (xx+width*yy > width*height-1 || xx+width*yy < 0)
            {
              MessageBox.Show("Restore: Bad 'xx,yy'=" + (xx+width*yy) + "; This means 'Byte' is bad.");
			      }
			
			      if (xx+width*yy<width*height && xx+width*yy>=0) 
			      { for (int c=0; c<nbyte; c++) Image.Grid[c+nbyte*xx+nbyte*width*yy]=ColP[c]; // Assertion
				      Mask.Grid[xx+width*yy]=LabMask;
			      }
			      xx=PixelN.X; yy=PixelN.Y;
            if (xx + width * yy > width * height - 1 || xx + width * yy < 0) return -1;
			
			      if (xx+width*yy<width*height && xx+width*yy>=0) 
			      { for (int c=0; c<nbyte; c++) Image.Grid[c+nbyte*xx+nbyte*width*yy]=ColN[c];
				      Mask.Grid[xx+width*yy]=LabMask;
			      }
			      P=P+Step[dir];

			      iShift++;
			      if (iShift==4)
			      { iShift=0; 
				      iByte++;
			      }
		      } //=============================== end for (iC... ==============================
	      } //================================= end for (il < nLine2 ===============================================
	      Mask.Grid[0]=Mask.Grid[width-1]=Mask.Grid[width*height-1]=Mask.Grid[width*(height-1)]=LabMask;
        return 1;
      } //*********************** end Restore **********************************************************************
    } //************************* end class CListCode ****************************************************************

    CListCode LiCod;

    private void button1_Click(object sender, EventArgs e) // Open file
    {
      button2.Visible = false;
      OpenFileDialog openFileDialog1 = new OpenFileDialog();
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        label1.Visible = false;
        label2.Visible = false;
        Stream stream = File.Open(openFileDialog1.FileName, FileMode.Open);
        OpenFileName = openFileDialog1.FileName;
        int indDot = -1;
        for (int i = 0; i < OpenFileName.Length; i++)
        {
          if (OpenFileName[i] == '.') indDot = i;  // Position of the last '.'
        }
        if (OpenFileName[indDot + 1] != 'd' && OpenFileName[indDot + 1] != 'D' ||
            OpenFileName[indDot + 2] != 'a' && OpenFileName[indDot + 2] != 'A' ||
            OpenFileName[indDot + 3] != 't' && OpenFileName[indDot + 3] != 'T')
        {
          MessageBox.Show("WFrestoreLin: Wrong extension; open a file with extension 'dat'");
          return;
        }
 
        int nCode = 0;
        byte[] bCode = new byte[4];

        int cnt=stream.Read(bCode, 0, 4);
        
        nCode = bCode[0];
        nCode |= bCode[1] << 8;
        nCode |= bCode[2] << 16;
        nCode |= bCode[3] << 24; 

        byte[] ByteNew = new byte[nCode];
        stream.Read(ByteNew, 0, nCode - 4);
        LiCod = new CListCode(ByteNew, this);
        label1.Text = "Opened file: " + OpenFileName;
        label1.Visible = true;
        button2.Visible = true;        
        MessageBox.Show("The file " + openFileDialog1.FileName + " has been opened and read. Click 'Restore'");
      }
      else return;



    } //****************************** end Open file **************************************


    public void GridToBitmapNew(Bitmap bmp, CImage Image)
    {
      int nbyteI=Image.N_Bits/8;
      if (bmp.PixelFormat != PixelFormat.Format24bppRgb)
      {
        MessageBox.Show("GridToBitmapNew: Inappropriate pixel format=" + bmp.PixelFormat);
        return;
      }

      int jump, Len = bmp.Height, nStep = 30;
      if (Len > 2*nStep) jump = Len / nStep;
      else jump = 2;
      for (int y = 0; y < bmp.Height; y++)
      {
        if ((y % jump) == jump - 1) progressBar1.PerformStep();
        for (int x = 0; x < bmp.Width; x++)
        { if (nbyteI==3)
            bmp.SetPixel(x, y, Color.FromArgb(Image.Grid[nbyteI * (x + bmp.Width * y) + 2],
               Image.Grid[nbyteI * (x + bmp.Width * y) + 1], Image.Grid[nbyteI * (x + bmp.Width * y) + 0]));
          else
            bmp.SetPixel(x, y, Color.FromArgb(Image.Grid[x + bmp.Width * y],
               Image.Grid[x + bmp.Width * y], Image.Grid[x + bmp.Width * y]));

        }
      }
      progressBar1.Visible = false;
    } //****************************** end GridToBitmapNew ****************************************


    private void GridToBitmap(Bitmap bmp, byte[] Grid)
    {
      Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
      BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
      int nbyte;
      switch (bmp.PixelFormat)
      {
        case PixelFormat.Format24bppRgb: nbyte = 3; break;
        case PixelFormat.Format8bppIndexed: nbyte = 1; break;
        default: MessageBox.Show("GridToBitmap: Inappropriate pixel format=" + bmp.PixelFormat); return;
      }
      IntPtr ptr = bmpData.Scan0;
      int size = bmp.Width * bmp.Height;
      int length = Math.Abs(bmpData.Stride) * bmp.Height;
      byte[] rgbValues = new byte[length];

      progressBar1.Visible = true;
      for (int y = 0; y < bmp.Height; y++)
      {
        int y1 = 1 + bmp.Height / 100;
        if (y % y1 == 1) progressBar1.PerformStep();

        for (int x = 0; x < bmp.Width; x++)
        {
          if (nbyte == 1)  // nbyte is global according to the PixelFormat of "bmp"
          {
            Color color = bmp.Palette.Entries[Grid[3 * (x + bmp.Width * y)]];
            rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 0] = color.B;
            rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 1] = color.G;
            rgbValues[3 * (x + Math.Abs(bmpData.Stride) * y) + 2] = color.R;
          }
          else
            for (int c = 0; c < nbyte; c++)
            {
              rgbValues[c + nbyte * x + Math.Abs(bmpData.Stride) * y] =
                                      Grid[c + nbyte * (x + bmp.Width * y)];
            }
        }
      }
      System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, length);
      bmp.UnlockBits(bmpData);
    } //****************************** end GridToBitmap ****************************************

    Bitmap RestoreBMP;
    private void button2_Click(object sender, EventArgs e) // Restore
    {
      int nBits = LiCod.nBits;
      int width = LiCod.width;
      int height = LiCod.height;
      CImage RestoreIm = new CImage(width, height, nBits);
      CImage Mask = new CImage(width, height, 8);

      LiCod.Restore(ref RestoreIm, ref Mask, this);

      RestoreIm.Smooth(ref Mask, this);
      RestoreBMP = new Bitmap(width, height, PixelFormat.Format24bppRgb);
      GridToBitmapNew(RestoreBMP, RestoreIm);

      progressBar1.Visible = false;
      pictureBox1.Image = RestoreBMP;
      button2.Visible = false;
      label2.Visible = true;
    } //******************************** end Restore ********************************

    private void button3_Click(object sender, EventArgs e) // Save image
    {
      MessageBox.Show("The restored image will be saved in the chosen directory;" +
        " you should tip a name with the extention 'bmp' or 'jpg'.");
      SaveFileDialog dialog = new SaveFileDialog();
      if (dialog.ShowDialog() == DialogResult.OK)
      {
        if (dialog.FileName.Contains(".jpg"))
          RestoreBMP.Save(dialog.FileName, ImageFormat.Jpeg);
        else 
          if (dialog.FileName.Contains(".bmp")) RestoreBMP.Save(dialog.FileName, ImageFormat.Bmp);
          else
          {
            MessageBox.Show("The file " + dialog.FileName + " has an inappropriate extension. Returning.");
            return;
          }
        MessageBox.Show("The restored image is saved as " + dialog.FileName);
      }

    } //****************************** end Save image ************************************

  }
}
