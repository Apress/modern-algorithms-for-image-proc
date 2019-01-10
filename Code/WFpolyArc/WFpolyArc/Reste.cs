// Reste.cs; 10.08.2018
public class CListPoly
{
  public
  int nArc, nCode, nPolygon, nVert;
  byte[] ByteNew;
  public CPolygon[] Polygon;
  public iVect2[] Vert;
  public CArc[] Arc;
  //public CArc2[] Arc2;

  public CListPoly() { } // Default constructor

  public CListPoly(int narc, int npoly, int nvert) // constructor
  {
    nArc = narc;
    nPolygon = npoly;
    nVert = nvert;
    Polygon = new CPolygon[nPolygon];
    for (int i = 0; i < nPolygon; i++) Polygon[i] = new CPolygon();
    Vert = new iVect2[nVert];
    for (int i = 0; i < nVert; i++) Vert[i] = new iVect2();
    Arc = new CArc[nArc];
    for (int i = 0; i < nArc; i++) Arc[i] = new CArc();

  } //*************** end constructor *********************


  public int Transform(CListLines L)
  // Copies data from L to this and transformd them to the byte array "ByteNew"..
  {
    int i;
    nCode = 0;
    nArc = L.nArc;
    nPolygon = L.nPolygon;
    nVert = L.nVert;
    nCode = 4 * 4;
    for (i = 0; i < nArc; i++) Arc[i] = L.Arc[i];
    nCode += 20 * nArc;
    for (i = 0; i < nPolygon; i++) Polygon[i] = L.Polygon[i];
    nCode += 22 * nPolygon;
    for (i = 0; i < nVert; i++) Vert[i] = L.Vert[i];
    nCode += 8 * nVert;

    // The following code is necessary to avoid "Serialize":
    ByteNew = new byte[nCode + 4]; // 4 bytes for nCode
    for (int ik = 0; ik < nCode + 4; ik++) ByteNew[ik] = 0;
    int j = 0, shift = 0;

    for (i = 0, shift = 0; i < 4; i++, shift += 8)
    {
      ByteNew[j] = (byte)((nCode >> shift) & 255);
      j++;
    }
    for (i = 0, shift = 0; i < 4; i++, shift += 8)
    {
      ByteNew[j] = (byte)((nArc >> shift) & 255);
      j++;
    }

    for (i = 0, shift = 0; i < 4; i++, shift += 8)
    {
      ByteNew[j] = (byte)((nPolygon >> shift) & 255);
      j++;
    }

    for (i = 0, shift = 0; i < 4; i++, shift += 8)
    {
      ByteNew[j] = (byte)((nVert >> shift) & 255);
      j++;
    }


    for (int i2 = 0; i2 < nArc; i2++)
    {
      float div = 1.0F;
      for (i = 0, div = 1.0F; i < 4; i++, div *= 256.0F)
      {
        ByteNew[j] = (byte)(Arc[i2].xb / div);
        j++;
      }
      for (i = 0, div = 1.0F; i < 4; i++, div *= 256.0F)
      {
        ByteNew[j] = (byte)(Arc[i2].yb / div);
        j++;
      }
      for (i = 0, div = 1.0F; i < 4; i++, div *= 256.0F)
      {
        ByteNew[j] = (byte)(Arc[i2].xe / div);
        j++;
      }
      for (i = 0, div = 1.0F; i < 4; i++, div *= 256.0F)
      {
        ByteNew[j] = (byte)(Arc[i2].ye / div);
        j++;
      }
      for (i = 0, div = 1.0F; i < 4; i++, div *= 256.0F)
      {
        ByteNew[j] = (byte)(Arc[i2].rad / div);
        j++;
      }
    }

    int k;
    int jj = j;
    for (int i3 = 0; i3 < nPolygon; i3++)
    {
      for (k = 0, shift = 0; k < 4; k++, shift += 8)
      {
        ByteNew[j] = (byte)((Polygon[i3].firstVert >> shift) & 255);
        j++;
      }
      for (k = 0, shift = 0; k < 4; k++, shift += 8)
      {
        ByteNew[j] = (byte)((Polygon[i3].lastVert >> shift) & 255);
        j++;
      }
      for (k = 0, shift = 0; k < 4; k++, shift += 8)
      {
        ByteNew[j] = (byte)((Polygon[i3].firstArc >> shift) & 255);
        j++;
      }
      for (k = 0, shift = 0; k < 4; k++, shift += 8)
      {
        ByteNew[j] = (byte)((Polygon[i3].lastArc >> shift) & 255);
        j++;
      }
      for (k = 0, shift = 0; k < 4; k++, shift += 8)
      {
        ByteNew[j] = (byte)((Polygon[i3].nCrack >> shift) & 255);
        j++;
      }

      if (Polygon[i3].closed) ByteNew[j] = 1;
      else ByteNew[j] = 0; j++;

      if (Polygon[i3].smooth) ByteNew[j] = 1;
      else ByteNew[j] = 0; j++;
    }
    for (int i4 = 0; i4 < nVert; i4++)
    {
      for (k = 0, shift = 0; k < 4; k++, shift += 8)
      {
        ByteNew[j] = (byte)((Vert[i4].X >> shift) & 255);
        j++;
      }
      for (k = 0, shift = 0; k < 4; k++, shift += 8)
      {
        ByteNew[j] = (byte)((Vert[i4].Y >> shift) & 255);
        j++;
      }
    }
    MessageBox.Show("WFpolyArc, Transform has calculated " + nCode + " bytes of code");
    return nCode;
  } //**************************** end Transform ********************************************

  public void WriteList(string Name, int nCode)
  {
    //FileStream fs = new FileStream("DataFile.dat", FileMode.Create);
    Stream stream = File.Open(Name, FileMode.Create);

    stream.Write(ByteNew, 0, nCode);
    stream.Close();
  } //****************************** end WriteList ********************


} //***************** end class CListPoly *************************


/*
public struct Box
{
  public
  int midX, midY, minX, minY, maxX, maxY;
  public Box(int mid, int miy) // constructor
  {
    midX = mid;
    midY = miy;
    minX = 10000;
    minY = 10000;
    maxX = 0;
    maxY = 0;
  }
}

    
public void RecoCarOld(CImage Img, int maxRad, Form1 fm1)
{
  // search for wheels:
  Box[] Box1 = new Box[16];
  for (i = 0; i < 16; i++) Box1[i] = new Box(100,100);
  int[] Clust = new int[nArc];
  for (i = 0; i < nArc; i++) Clust[i] = -1;

  int minRad = 3;
  maxRad = 10;
  double ScaleX, ScaleY, Scale1;
  ScaleX = (double)fm1.pictureBox2.Width / (double)Img.width;
  ScaleY = (double)fm1.pictureBox2.Height / (double)Img.height;
  if (ScaleX < ScaleY) Scale1 = ScaleX;
  else Scale1 = ScaleY;
  int marginX = (fm1.pictureBox2.Width - (int)(Scale1 * Img.width)) / 2;
  int marginY = (fm1.pictureBox2.Height - (int)(Scale1 * Img.height)) / 2;


  Graphics g = fm1.pictureBox2.CreateGraphics();
  Rectangle rect;
  Pen myPen = new Pen(Color.White);
  //rect = new Rectangle(X, Y, (int)(Scale1 * SizeX), (int)(Scale1 * SizeY));  // for "Show"

  double xm = 0, ym = 0;
      
  for (int ia = 0; ia < nArc; ia++)  // learn 0
  { 
    Center(Arc[ia], ref xm, ref ym);
    if (Math.Abs(Arc[ia].rad) < minRad || Math.Abs(Arc[ia].rad) > maxRad) continue;
    if (xm < Box1[0].minX) Box1[0].minX = xm;
    if (xm > Box1[0].maxX) Box1[0].maxX = xm;
    if (ym < Box1[0].minY) Box1[0].minY = ym;
    if (ym > Box1[0].maxY) Box1[0].maxY = ym;
  }
  Box1[0].midX = (Box1[0].minX + Box1[0].maxX) / 2;
  Box1[0].midY = (Box1[0].minY + Box1[0].maxY) / 2;

  for (int ia = 0; ia < nArc; ia++)
  {
    Center(Arc[ia], ref xm, ref ym);

    if (Math.Abs(Arc[ia].rad) < minRad || Math.Abs(Arc[ia].rad) > maxRad) continue;
    if (xm < Box1[0].midX && ym < Box1[0].midY)
    {
      Clust[ia] = 1;
      //Box1[0].minX = xm;
      if (xm < Box1[1].minX) Box1[1].minX = xm;
      if (xm > Box1[1].maxX) Box1[1].maxX = xm;
      if (ym < Box1[1].minY) Box1[1].minY = ym;
      if (ym > Box1[1].maxY) Box1[1].maxY = ym;
    }

    if (xm >= Box1[0].midX && ym < Box1[0].midY)
    {
      Clust[ia] = 2;
      //Box1[0].minX = xm;
      if (xm < Box1[2].minX) Box1[2].minX = xm;
      if (xm > Box1[2].maxX) Box1[2].maxX = xm;
      if (ym < Box1[2].minY) Box1[2].minY = ym;
      if (ym > Box1[2].maxY) Box1[2].maxY = ym;
    }

    if (xm < Box1[0].midX && ym >= Box1[0].midY)
    {
      Clust[ia] = 3;
      //Box1[0].minX = xm;
      if (xm < Box1[3].minX) Box1[3].minX = xm;
      if (xm > Box1[3].maxX) Box1[3].maxX = xm;
      if (ym < Box1[3].minY) Box1[3].minY = ym;
      if (ym > Box1[3].maxY) Box1[3].maxY = ym;
    }

    if (xm >= Box1[0].midX && ym >= Box1[0].midY)
    {
      Clust[ia] = 4;
      //Box1[0].minX = xm;
      if (xm < Box1[4].minX) Box1[4].minX = xm;
      if (xm > Box1[4].maxX) Box1[4].maxX = xm;
      if (ym < Box1[4].minY) Box1[4].minY = ym;
      if (ym > Box1[4].maxY) Box1[4].maxY = ym;
    }

  } //========================== end for (int ia ... ===================
  MessageBox.Show(
  "RecoCar: Box1[1]=("+Box1[1].minX+","+Box1[1].minY+","+Box1[1].maxX+","+Box1[1].maxY+
    ") Box1[2]=("+Box1[2].minX+","+Box1[2].minY+","+Box1[2].maxX+","+Box1[2].maxY+
    ") Box1[3]=("+Box1[3].minX+","+Box1[3].minY+","+Box1[3].maxX+","+Box1[3].maxY+
    ") Box1[4]=("+Box1[4].minX+","+Box1[4].minY+","+Box1[4].maxX+","+Box1[4].maxY+")");
  int sizeX, sizeY;
  for (i = 1; i <= 4; i++)
  {
    sizeX = Box1[i].maxX - Box1[i].minX;
    sizeY = Box1[i].maxY - Box1[i].minY;
    rect = new Rectangle(marginX + (int)(Scale1 * Box1[i].minX), 
            marginY + (int)(Scale1 * Box1[i].minY), sizeX, sizeY);  
    g.DrawRectangle(myPen, rect);
  }
} //***************************** end RecoCarOld *****************************

public int Center(CArc arc, ref double xm, ref double ym) // called in RecoCar
{
  double chord_X = arc.xe - arc.xb;
  double chord_Y = arc.ye - arc.yb;
  double Length = Math.Sqrt(chord_X * chord_X + chord_Y * chord_Y); // Länge der Sehne
  double Lot_x, Lot_y;
  if (arc.rad > 0.0)        // Senkrechte zur Sehne 
  {
    Lot_x = chord_Y; Lot_y = -chord_X;
  }
  else
  {
    Lot_x = -chord_Y; Lot_y = chord_X;
  }

  if (Math.Abs(arc.rad) < Length * 0.5) return -1;

  if (Length < 0.1) return -2;

  // Distance from the center of the chord to the center of the arc: 
  double Lot = Math.Sqrt(4.0 * arc.rad * arc.rad - Length * Length);
  xm = ((arc.xb + arc.xe) / 2 + Lot_x * Lot / 2 / Length);     // center of the arc 
  ym = ((arc.yb + arc.ye) / 2 + Lot_y * Lot / 2 / Length);
  return 1;
}


public int RecoCar(CImage Img, int Size, int minRad, int maxRad, Form1 fm1)
{
  double ScaleX, ScaleY, Scale1;
  ScaleX = (double)fm1.pictureBox2.Width / (double)Img.width;
  ScaleY = (double)fm1.pictureBox2.Height / (double)Img.height;
  if (ScaleX < ScaleY) Scale1 = ScaleX;
  else Scale1 = ScaleY;
  int marginX = (fm1.pictureBox2.Width - (int)(Scale1 * Img.width)) / 2;
  int marginY = (fm1.pictureBox2.Height - (int)(Scale1 * Img.height)) / 2;
  MessageBox.Show("RecoCar: Scale1=" + Scale1 + " marginX=" + marginX + " marginY=" + marginY);

  Graphics g = fm1.pictureBox2.CreateGraphics();
  Rectangle rect, rect1;
  Pen whitePen = new Pen(Color.White);
  Pen redPen = new Pen(Color.Red);
  Pen bluePen = new Pen(Color.Blue);
  //rect = new Rectangle(X, Y, (int)(Scale1 * SizeX), (int)(Scale1 * SizeY));  // for "Show"

  int nx, ny, size = 0;
  double xm = 0.0, ym = 0.0;
  double dSize = (double)Size;
  nx = 1 + Img.width / Size;
  ny = 1 + Img.height / Size;
  int[] Cnt = new int[nx * ny];
  for (int i = 0; i < nx * ny; i++) Cnt[i] = 0;

  for (int ia = 0; ia < nArc; ia++)  // learn 0
  {
    if (Math.Abs(Arc[ia].rad) < minRad || Math.Abs(Arc[ia].rad) > maxRad) continue;
    Center(Arc[ia], ref xm, ref ym);
    if ((int)(xm) / Size >= nx || (int)(ym) / Size >= ny) continue;
    int xx = (int)xm / Size;
    int yy = (int)ym / Size;
    //MessageBox.Show("RecoCar: ia=" + ia + " xm=" + xm + " xx=" + xx + " nx=" + nx + " yy=" + yy + " ny=" + ny);
    if (xx >= 0 && xx < nx && yy >= 0 && yy < ny) Cnt[xx + nx * yy]++;
    if (xx < nx && yy < ny && Cnt[xx + nx * yy] > 0 &&
      (marginX + (int)(Scale1 * xm)) > 0 && (marginX + (int)Scale1 * xm) < fm1.pictureBox2.Width &&
      (marginY + (int)Scale1 * ym) > 0 && (marginY + (int)Scale1 * ym) < fm1.pictureBox2.Height)
    { //MessageBox.Show("RecoCar: ia=" + ia + " Rad=" + Math.Abs(Arc[ia].rad) + "; S*xm=" + (marginX + (int)(Scale1*xm)) + 
      //"; S*ym=" + (marginY + (int)(Scale1*ym)));
      rect1 = new Rectangle(marginX + (int)(Scale1 * xm), marginY + (int)(Scale1 * ym), 5, 5);
      //rect1 = new Rectangle(300, 200, 10, 10);
      g.DrawRectangle(bluePen, rect1);
    }
  }
  double Scale2 = Scale1 * (double)Size;
  for (int y = 0; y < ny; y++)
  {
    g.DrawLine(redPen, marginX + 0, marginY + (int)Scale2 * y, marginX + (int)Scale2 * nx, marginY + (int)Scale2 * y);
    for (int x = 0; x < nx; x++)
    {
      g.DrawLine(redPen, marginX + (int)Scale2 * x, marginY + 0, marginX + (int)Scale2 * x, marginY + (int)Scale2 * ny);
      if (Cnt[x] > 0)
      {
        size = 3 * (1 + (int)Math.Sqrt((double)Cnt[x]));
        rect = new Rectangle(marginX + (int)(Scale2 * x), marginY + (int)(Scale2 * y), size, size);
        //g.DrawRectangle(whitePen, rect);
      }
      //if (false && Cnt[x] > 1)
      //MessageBox.Show("RecoCar: x=" + x + " y=" + y + " Cnt=" + Cnt[x]);
    }
  }

  //IWin32Window Ob=(Win32 HWND) 100;
  int maxSum = 0, optX = -1, optY = -1;
  for (int y = ny / 2; y < ny - 4; y++)
    for (int x = 0; x < nx - 4; x++)
    {
      int Sum = 0;
      for (int k = y; k < y + 4; k++)
        for (int i = x; i < x + 4; i++)
          Sum += Cnt[i + nx * k];
      if (Sum > 0) // && false)
        MessageBox.Show("RecoCar: x=" + x + " y=" + y + " Sum=" + Sum);
      if (Sum > maxSum)
      {
        maxSum = Sum;
        optX = x;
        optY = y;
      }
    }
  MessageBox.Show("RecoCar: maxSum=" + maxSum + " optX=" + optX + " optY=" + optY);
  return 1;
} //************************************* end RecoCar **************************************



public int RecoBicycle(CImage Img, int Size, int minRad, int maxRad, Form1 fm1)
// The method produces the array "Cnt[nx*ny]" and the grid of nx*ny points. The value
// of Cnt[x + nx*y] is the number of arcs whose centers lie in the Size*Size box
// containing the point (x*Size, y*Size).
{
  double ScaleX, ScaleY, Scale1;
  ScaleX = (double)fm1.pictureBox2.Width / (double)Img.width;
  ScaleY = (double)fm1.pictureBox2.Height / (double)Img.height;
  if (ScaleX < ScaleY) Scale1 = ScaleX;
  else Scale1 = ScaleY;
  int marginX = (fm1.pictureBox2.Width - (int)(Scale1 * Img.width)) / 2;
  int marginY = (fm1.pictureBox2.Height - (int)(Scale1 * Img.height)) / 2;
  bool deb = false, GRID = false;
  if (deb) MessageBox.Show("RecoBicycle: Scale1=" + Scale1 + " marginX=" + marginX + " marginY=" + marginY + " Scale1=" + Scale1);

  Graphics g = fm1.pictureBox2.CreateGraphics();
  Rectangle rect, rect1;
  Pen whitePen = new Pen(Color.White);
  Pen redPen = new Pen(Color.Red, 3);
  Pen greenPen = new Pen(Color.Aquamarine);
  Pen yellowPen = new Pen(Color.Yellow);
  //rect = new Rectangle(X, Y, (int)(Scale1 * SizeX), (int)(Scale1 * SizeY));  // for "Show"

  int nx, ny, size = 0;
  double xm = 0.0, ym = 0.0;
  double dSize = (double)Size;
  nx = 1 + Img.width / Size;
  ny = 1 + Img.height / Size;
  int[] Cnt = new int[nx * ny];
  double[] Dxm = new double[nx * ny];
  double[] Dym = new double[nx * ny];
  double[] Drad = new double[nx * ny];
  int i = 0;
  for (i = 0; i < nx * ny; i++)
  {
    Cnt[i] = 0;
    Dxm[i] = 0.0;
    Dym[i] = 0.0;
    Drad[i] = 0.0;
  }

  for (int ia = 0; ia < nArc; ia++)  // learn 0
  {
    if (Math.Abs(Arc[ia].rad) < minRad || Math.Abs(Arc[ia].rad) > maxRad) continue;
    //Center(Arc[ia], ref xm, ref ym);
    xm = Arc[ia].xm;
    ym = Arc[ia].ym;
    if ((int)(xm) / Size >= nx || (int)(ym) / Size >= ny || xm < 0 || ym < 0) continue;
    int xx = (int)xm / Size;
    int yy = (int)ym / Size;
    //if (deb) 
    if (xx >= 0 && xx < nx && yy >= 0 && yy < ny)
    {
      Cnt[xx + nx * yy]++;
      Dxm[xx + nx * yy] += xm;
      Dym[xx + nx * yy] += ym;
      Drad[xx + nx * yy] += Math.Abs(Arc[ia].rad);
      if (deb) MessageBox.Show("--RB: ia=" + ia + " xx=" + xx + " yy=" + yy + " xm=" + (int)xm + " Cnt=" + Cnt[xx + nx * yy]);
    }
    if (Cnt[xx + nx * yy] > 0 && xx > 23 && yy > 23 && deb)
      MessageBox.Show("RecoBicycle: ia=" + ia + " xx=" + xx + " yy=" + yy + " Cnt=" + Cnt[xx + nx * yy] + " xm=" +
        (int)xm + " ym=" + (int)ym + " rad=" + (int)Math.Abs(Arc[ia].rad) + " Dxm=" + (int)Dxm[xx + nx * yy] +
        " Dym=" + (int)Dym[xx + nx * yy] + " Drad=" + (int)Drad[xx + nx * yy]);

    if (xx < nx && yy < ny && Cnt[xx + nx * yy] > 0) // &&
    //(marginX + (int)(Scale1 * xm)) > 0 && (marginX + (int)Scale1 * xm) < fm1.pictureBox2.Width &&
    //(marginY + (int)Scale1 * ym) > 0 && (marginY + (int)Scale1 * ym) < fm1.pictureBox2.Height)
    { //if (deb) 
      if (Cnt[xx + nx * yy] > 1 && xx > 23 && yy > 23 && deb)
        MessageBox.Show("RB: ia=" + ia + " Rad=" + Math.Abs(Arc[ia].rad) + "; xm=" + (int)xm +
        "; ym=" + (int)ym + " xx=" + xx + " yy=" + yy + " Cnt=" + Cnt[xx + nx * yy]);
      size = 3 * (1 + (int)Math.Sqrt((double)Cnt[xx + nx * yy]));
      rect1 = new Rectangle(marginX + (int)(Scale1 * xm), marginY + (int)(Scale1 * ym), size, size);
      //rect1 = new Rectangle(300, 200, 10, 10);
      if (Cnt[xx + nx * yy] > 1) g.DrawRectangle(greenPen, rect1);
    }
  } //============================= end for (int ia ... ==========================================
  double Scale2 = Scale1 * (double)Size;

  for (int y = 0; y < ny; y++)
  {
    if (GRID) g.DrawLine(redPen, marginX + 0, marginY + (int)Scale2 * y, marginX + (int)Scale2 * nx, marginY + (int)Scale2 * y);
    for (int x = 0; x < nx; x++)
    {
      if (GRID) g.DrawLine(redPen, marginX + (int)Scale2 * x, marginY + 0, marginX + (int)Scale2 * x, marginY + (int)Scale2 * ny);
      if (Cnt[x] > 0 && false)
      {
        size = 3 * (1 + (int)Math.Sqrt((double)Cnt[x]));
        rect = new Rectangle(marginX + (int)(Scale2 * x), marginY + (int)(Scale2 * y), size, size);
        //g.DrawRectangle(whitePen, rect);
        Dxm[x] /= Cnt[x];
        Dym[x] /= Cnt[x];
        Drad[x] /= Cnt[x];
      }
      if (Cnt[x] > 1 && x > 23 && y > 23 && deb)
        MessageBox.Show("** RB: x=" + x + " y=" + y + " Cnt=" + Cnt[x] + " Drad=" +
          (int)Drad[x] + " Dxm=(" + (int)Dxm[x] + "," + (int)Dym[x] + ") Cnt=" + Cnt[x]);
    } //======================= end for (int x ... ===========================================
  } //========================= end for (int y ... =============================================
  if (deb) MessageBox.Show("RB: Drad[25,26]=" + (int)Drad[25 + nx * 26] + " Drad[8,25]=" + Drad[8 + nx * 25] +
    " Cnt[25,26]=" + Cnt[25 + nx * 26] + " Cnt[8,25]=" + Cnt[8 + nx * 25]);
  int cnt = 0;
  int[] SumCnt = new int[nx * ny];
  double[] SumXm = new double[nx * ny], SumYm = new double[nx * ny], SumRad = new double[nx * ny];
  for (int k = 0; k < nx * ny; k++)
  {
    SumCnt[k] = 0;
    SumXm[k] = SumYm[k] = SumRad[k] = 0.0;
  }
  int maxSum = 0, maxSum2 = 0, optX = -1, optY = -1, optX2 = -1, optY2 = -1;
  for (int y = 1; y < ny - 1; y++)
    for (int x = 1; x < nx - 1; x++)
    {
      for (int k = -1; k < 2; k++)
        for (i = -1; i < 2; i++) //==========================================
        {
          SumCnt[x] += Cnt[x + i + nx * (y + k)];
          SumXm[x] += Dxm[x + i + nx * (y + k)];
          SumYm[x] += Dym[x + i + nx * (y + k)];
          SumRad[x] += Drad[x + i + nx * (y + k)];
          if (Cnt[x + i + nx * (y + k)] > 0) cnt++;
          if (x > 22 && y > 22 && deb) MessageBox.Show("Rect_xy(" + x + "," + y + ") ik=(" + i +
            "," + k + " Cnt=" + Cnt[x + i + nx * (y + k)] + " SumCnt=" + SumCnt[x]);
        } //====================== end for (i ... =============================


      SumXm[x] /= SumCnt[x];
      SumYm[x] /= SumCnt[x];
      SumRad[x] /= SumCnt[x];
      if (SumCnt[x] > 15 && false)
      {
        size = 3 * (1 + (int)Math.Sqrt((double)SumCnt[x]));
        rect = new Rectangle(marginX + (int)(Scale2 * x), marginY + (int)(Scale2 * y), size, size);
        //g.DrawRectangle(whitePen, rect);
        //MessageBox.Show("RecoBicycle: x=" + x + " y=" + y + " Sum=" + Sum);
      }
      //MessageBox.Show("RecoBicycle: x=" + x + " y=" + y + " Sum=" + Sum + " maxSum=" + maxSum);
      //if (Sum == 0) continue;
      if (SumCnt[x] > maxSum)
      {
        maxSum = SumCnt[x];
        optX = x;
        optY = y;
      }
      if (SumCnt[x] > maxSum2 && Math.Abs(x - optX) > 5)
      {
        maxSum2 = SumCnt[x];
        optX2 = x;
        optY2 = y;
      }
    } //===================== end for (int x ... ========================================

  if (deb) MessageBox.Show("RecoBicycle: maxSum=" + maxSum + " optX=" + optX + " optY=" + optY +
    " Drad=" + (int)Drad[optX + nx * optY] + " Dxm=" + (int)Dxm[optX + nx * optY]);
  if (deb) MessageBox.Show("RecoBicycle: maxSum2=" + maxSum2 + " optX2=" + optX2 + " optY2=" + optY2 +
    " Drad2=" + (int)Drad[optX2 + nx * optY2]);
  int distCenters = 0, Radius1 = 0, Radius2 = 0;
  if (optX > 0 && optX2 > 0)
  {
    distCenters = Math.Abs((int)(SumXm[optX + nx * optY] - SumXm[optX2 + nx * optY2]));
    Radius1 = (int)SumRad[optX + nx * optY];
    Radius2 = (int)SumRad[optX2 + nx * optY2];
    if (deb) MessageBox.Show("RecoBicycle: distCenters=" + distCenters + " Radius1=" + Radius1 + " Radius2=" + Radius2);
    if (deb) MessageBox.Show("RB: optX2=" + optX2 + " optY2=" + optY2 + " left centerX=" + (int)SumXm[optX2 + nx * optY2]);

    double del = 0.2;
    double cos_d = Math.Cos(del); // "del" is the step of the circular moving of the point (x, y)
    double R = Math.Abs(Radius1);
    double sin_d = Math.Sin(del);
    xm = SumXm[optX + nx * optY];
    ym = SumYm[optX + nx * optY];
    double x1 = R;
    double al, xn, yn, y1 = 0.0;
    //if (deb) 
    if (deb) MessageBox.Show("RB: R=" + (int)R + " xm=" + (int)xm + " ym=" + ym + " x1=" + x1 + " y1=" + y1);
    for (al = del; al < 6.28; al += del)       // Drawing an arc step by step 
    {
      xn = x1 * cos_d - y1 * sin_d;
      yn = x1 * sin_d + y1 * cos_d;
      if (deb)
        MessageBox.Show("RB: al=" + al + " x1+xm=" + (int)(x1 + xm) + " y1 + ym =" + (int)(y1 + ym) +
        " xn+xm=" + (int)(xn + xm) + " yn + ym =" + (int)(yn + ym) + " X=" + (marginX + (int)(Scale1 * (x1 + xm) + 0.5)) +
        " Y=" + (marginY + (int)(Scale1 * (y1 + ym) + 0.5)));
      g.DrawLine(redPen, marginX + (int)(Scale1 * (x1 + xm) + 0.5), marginY + (int)(Scale1 * (y1 + ym) + 0.5),
                         marginX + (int)(Scale1 * (xn + xm) + 0.5), marginY + (int)(Scale1 * (yn + ym) + 0.5));
      x1 = xn; y1 = yn;
    }

    xm = SumXm[optX2 + nx * optY2];
    ym = SumYm[optX2 + nx * optY2];
    x1 = R;
    y1 = 0.0;
    //double al, xn, yn, y1 = 0.0;
    //MessageBox.Show("RB: R=" + (int)R + " xm=" + (int)xm + " ym=" + ym + " x1=" + x1 + " y1=" + y1);
    for (al = del; al < 6.28; al += del)       // Drawing an arc step by step 
    {
      xn = x1 * cos_d - y1 * sin_d;
      yn = x1 * sin_d + y1 * cos_d;
      if (deb)
        MessageBox.Show("RB2: al=" + al + " x1+xm=" + (int)(x1 + xm) + " y1 + ym =" + (int)(y1 + ym) +
        " xn+xm=" + (int)(xn + xm) + " yn + ym =" + (int)(yn + ym) + " X=" + (marginX + (int)(Scale1 * (x1 + xm) + 0.5)) +
        " Y=" + (marginY + (int)(Scale1 * (y1 + ym) + 0.5)));
      g.DrawLine(redPen, marginX + (int)(Scale1 * (x1 + xm) + 0.5), marginY + (int)(Scale1 * (y1 + ym) + 0.5),
                         marginX + (int)(Scale1 * (xn + xm) + 0.5), marginY + (int)(Scale1 * (yn + ym) + 0.5));
      x1 = xn; y1 = yn;
    }
  }

  Point P1 = new Point(0, 0),
    P2 = new Point(0, 0), P3 = new Point(0, 0), P4 = new Point(0, 0);
  int rightCenterX;
  int rightCenterY;
  if (SumXm[optX + nx * optY] > SumXm[optX2 + nx * optY2])
  {
    rightCenterX = (int)SumXm[optX + nx * optY];
    rightCenterY = (int)SumYm[optX + nx * optY];
  }
  else
  {
    rightCenterX = (int)SumXm[optX2 + nx * optY2];
    rightCenterY = (int)SumYm[optX2 + nx * optY];
  }

  /* Points as the base of the blocks:
  P0 = (0.00; 0.00); P1 = (0.19; 0.13); P2 = (0.96; 0.17); P3 = (1.52; 0.10); P4 =(0.90; 1.36);
  P5 = (1.02; 1.06); P6 = (2.56; 2.00); P7 = (2.65; 1.85); P8 = (2.75; 1.65); P9 =(3.23; 0.36);
  P10 = (3.52; 0.00); 
  Block0 = (P3, P4); Block1 = (P6, P9); Block2 = (P0, P5); Block3 = (P1, P2); 
  Block4 = (P0, P3); Block5 = (P0, P5); Block6 = (P5, P7); Block7 = (P3, P8);
   * --*

  double[] ProtX = { 0.0, 0.19, 0.96, 1.52, 0.90, 1.02, 2.56, 2.65, 2.75, 3.23, 3.52 };
  double[] ProtY = { 0.0, 0.13, 0.17, 0.10, 1.36, 1.06, 2.00, 1.85, 1.65, 0.36, 0.0 };

  int[,] Block = { { 3, 4 }, { 6, 9 }, { 0, 5 }, { 5, 7 }, { 3, 8 }, { 1, 2 }, { 0, 3 } };
  double[] A = new double[8];
  double[] B = new double[8];

  double[] PointX = new double[11];
  double[] PointY = new double[11];

  int leftCenterX = rightCenterX - distCenters;
  int leftCenterY = rightCenterY;

  for (int k = 0; k < 11; k++)
  {
    PointX[k] = leftCenterX + ProtX[k] * Radius2;
    PointY[k] = leftCenterY - ProtY[k] * Radius2;
  }
  double Root1;
  //double halfWidth = 20.0;
  for (int m = 0; m < 5; m++)
  {
    A[m] = PointY[Block[m, 0]] - PointY[Block[m, 1]];
    B[m] = PointX[Block[m, 1]] - PointX[Block[m, 0]];
    Root1 = Math.Sqrt(A[m] * A[m] + B[m] * B[m]);
    A[m] /= Root1;
    B[m] /= Root1;
    /*
    g.DrawLine(whitePen, (int)((PointX[Block[m, 0]] - halfWidth) * Scale1) + marginX, (int)(PointY[Block[m, 0]] * Scale1) + marginY,
    (int)((PointX[Block[m, 0]] + halfWidth) * Scale1) + marginX, (int)(PointY[Block[m, 0]] * Scale1) + marginY);

    g.DrawLine(whitePen, (int)((PointX[Block[m, 0]] + halfWidth) * Scale1) + marginX, (int)(PointY[Block[m, 0]] * Scale1) + marginY,
    (int)((PointX[Block[m, 1]] + halfWidth) * Scale1) + marginX, (int)(PointY[Block[m, 1]] * Scale1) + marginY);

    g.DrawLine(whitePen, (int)((PointX[Block[m, 1]] + halfWidth) * Scale1) + marginX, (int)(PointY[Block[m, 1]] * Scale1) + marginY,
    (int)((PointX[Block[m, 1]] - halfWidth) * Scale1) + marginX, (int)(PointY[Block[m, 1]] * Scale1) + marginY);

    g.DrawLine(whitePen, (int)((PointX[Block[m, 1]] - halfWidth) * Scale1) + marginX, (int)(PointY[Block[m, 1]] * Scale1) + marginY,
    (int)((PointX[Block[m, 0]] - halfWidth) * Scale1) + marginX, (int)(PointY[Block[m, 0]] * Scale1) + marginY);
     * --*
  }

  for (int m = 5; m <= 6; m++)
  {
    A[m] = PointY[Block[m, 0]] - PointY[Block[m, 1]];
    B[m] = PointX[Block[m, 1]] - PointX[Block[m, 0]];
    Root1 = Math.Sqrt(A[m] * A[m] + B[m] * B[m]);
    A[m] /= Root1;
    B[m] /= Root1;
    /*
    g.DrawLine(whitePen,
      (int)(PointX[Block[m, 0]] * Scale1) + marginX, (int)((PointY[Block[m, 0]] - halfWidth) * Scale1) + marginY,
    (int)(PointX[Block[m, 0]] * Scale1) + marginX, (int)((PointY[Block[m, 0]] + halfWidth) * Scale1) + marginY);

    g.DrawLine(whitePen,
      (int)(PointX[Block[m, 0]] * Scale1) + marginX, (int)((PointY[Block[m, 0]] + halfWidth) * Scale1) + marginY,
    (int)(PointX[Block[m, 1]] * Scale1) + marginX, (int)((PointY[Block[m, 1]] + halfWidth) * Scale1) + marginY);

    g.DrawLine(whitePen,
      (int)(PointX[Block[m, 1]] * Scale1) + marginX, (int)((PointY[Block[m, 1]] + halfWidth) * Scale1) + marginY,
    (int)(PointX[Block[m, 1]] * Scale1) + marginX, (int)((PointY[Block[m, 1]] - halfWidth) * Scale1) + marginY);

    g.DrawLine(whitePen,
      (int)(PointX[Block[m, 1]] * Scale1) + marginX, (int)((PointY[Block[m, 1]] - halfWidth) * Scale1) + marginY,
    (int)(PointX[Block[m, 0]] * Scale1) + marginX, (int)((PointY[Block[m, 0]] - halfWidth) * Scale1) + marginY);
     * --*
  }
  if (deb) MessageBox.Show("***RB: optX=" + optX + " optY=" + optY + " Dm=rightCenter=(" + rightCenterX + "," + rightCenterY + ")" +
   " Drad=" + (int)Drad[optX + nx * optY]);
  /*
  P3.X = rightCenterX - Radius1 / 7;
  P3.Y = rightCenterY - Radius1 / 3;
  P4.X = rightCenterX - Radius1 / 7 - distCenters / 10;
  P4.Y = P3.Y;
  P2.X = rightCenterX - Radius1 / 7 - (rightCenterX - distCenters * 65 / 115) * 5 / 40;
  P2.Y = rightCenterY - distCenters * 65 / 115;
  P1.X = P2.X - (int)(distCenters / 10);
  P1.Y = P2.Y;
  if (deb) MessageBox.Show("distCenters=" + distCenters + " R1=" + Radius1 + " P3=(" + P3.X + "," + P3.Y + ")" +
    " optx*Scale2=" + (int)(optX * Scale2) + " optx2*Scale2=" + (int)(optX2 * Scale2) + " rightCX=" + rightCenterX +
    " rCX - Radius1/5=" + (int)(rightCenterX - Radius1 / 5)); 

  g.DrawLine(redPen, (int)(rightCenterX * Scale1) + marginX, (int)(rightCenterY * Scale1) + marginY,
    (int)((rightCenterX - distCenters) * Scale1) + marginX, (int)(rightCenterY * Scale1) + marginY);
  g.DrawLine(whitePen, (int)(P1.X * Scale1) + marginX, (int)(P1.Y * Scale1) + marginY,
    (int)(P2.X * Scale1) + marginX, (int)(P2.Y * Scale1) + marginY);
  g.DrawLine(whitePen, (int)(P2.X * Scale1) + marginX, (int)(P2.Y * Scale1) + marginY,
    (int)(P3.X * Scale1) + marginX, (int)(P3.Y * Scale1) + marginY);
  g.DrawLine(whitePen, (int)(P3.X * Scale1) + marginX, (int)(P3.Y * Scale1) + marginY,
    (int)(P4.X * Scale1) + marginX, (int)(P4.Y * Scale1) + marginY);
  g.DrawLine(whitePen, (int)(P4.X * Scale1) + marginX, (int)(P4.Y * Scale1) + marginY,
    (int)(P1.X * Scale1) + marginX, (int)(P1.Y * Scale1) + marginY);

  double A1 = (double)(P4.Y - P1.Y);
  double B1 = (double)(P1.X - P4.X);
  double Root = Math.Sqrt(A1 * A1 + B1 * B1);
  A1 = A1 / Root;
  B1 = B1 / Root;
  if (deb) MessageBox.Show("RB: A1=" + A1 + " B1=" + B1 + " sqrt=" + Root);

  double A2 = (double)(P3.Y - P2.Y);
  double B2 = (double)(P2.X - P3.X);
  Root = Math.Sqrt(A2 * A2 + B2 * B2);
  A2 /= Root;
  B2 /= Root;

  double Neig = (P3.X - P2.X) / (double)(P3.Y - P2.Y);
  int[] Save = new int[20];
  int nVert = 0;
  for (int ip = 0; ip < nPolygon; ip++)
    for (int iv = Polygon[ip].firstVert; iv < Polygon[ip].lastVert; iv++)
    {
      double neig = (double)(Vert[iv + 1].X - Vert[iv].X) / (double)(Vert[iv + 1].Y - Vert[iv].Y);
      if (Math.Abs(neig - 0.5) < 0.8 && false)
        MessageBox.Show("RB: ip=" + ip + " neig=" + neig + " Neig=" + Neig);
      //if (Vert[iv].Y >= P1.Y && Vert[iv].Y <= P3.Y && Vert[iv].X >= P1.X && Vert[iv].X <= P3.X &&
      //(Vert[iv + 1].Y - Vert[iv].Y) != 0 && Math.Abs(neig - Neig) < 0.12)
      if (A1 * (Vert[iv].X - P1.X) + B1 * (Vert[iv].Y - P1.Y) > 5.0 && Vert[iv].Y > P1.Y && Vert[iv].Y < P4.Y &&
        A1 * (Vert[iv].X - P1.X) + B1 * (Vert[iv].Y - P1.Y) < 29.0 && Math.Abs(neig - Neig) < 0.12)
      {
        if (A1 * (Vert[iv].X - P1.X) + B1 * (Vert[iv].Y - P1.Y) < 35.0)
        {
          Save[nVert] = iv; nVert++;
        }
        if (deb) MessageBox.Show("RB: found edge (" + Vert[iv].X + ", " + Vert[iv].Y + ")" + " Neig=" + Neig +
          " neig=" + neig + " ip=" + ip);
        g.DrawLine(yellowPen, marginX + (int)(Vert[iv].X * Scale1), marginY + (int)(Vert[iv].Y * Scale1),
          marginX + (int)(Vert[iv + 1].X * Scale1), marginY + (int)(Vert[iv + 1].Y * Scale1));

        //if (deb) 
        MessageBox.Show("++RB: dist(iv)=" + (A1 * (Vert[iv].X - P1.X) + B1 * (Vert[iv].Y - P1.Y)) +
        " Vert=(" + Vert[iv].X + "," + Vert[iv].Y + ")");
      } 
    } //=========================== end for (int iv ... ==============================================  
  if (deb) MessageBox.Show("RB: nVert=" + nVert + " iv=(" + Save[0] + "," + Save[1] + "," + Save[2] + "," + Save[3] + ")");
  int xUp = 10000, xDown = 0;
  int yUp = 10000, yDown = 0;
  for (int ks = 0; ks < nVert; ks++)
  {
    if (Vert[Save[ks]].Y < yUp)
    {
      yUp = Vert[Save[ks]].Y;
      xUp = Vert[Save[ks]].X;
    }
    if (Vert[Save[ks]].Y > yDown)
    {
      yDown = Vert[Save[ks]].Y;
      xDown = Vert[Save[ks]].X;
    }
    g.DrawLine(redPen, marginX + (int)(xUp * Scale1), marginY + (int)(yUp * Scale1),
                       marginX + (int)(xDown * Scale1), marginY + (int)(yDown * Scale1));
  } 
  return 1;
} //************************************* end RecoBicycle **************************************



public void RecoBicycleNew(CImage Img, int Size, int minRad, int maxRad, Form1 fm1)
{
  int nx, ny; //, size = 0;
  bool deb = false;
  double dSize = (double)Size;
  Graphics g = fm1.pictureBox2.CreateGraphics();

  nx = Img.width / Size; // it was 2 + Img.Width / Size
  ny = Img.height / Size;
  int[] CntA = new int[nx * ny];
  int[] CntB = new int[nx * ny];
  int[] CntC = new int[nx * ny];
  int[] CntD = new int[nx * ny];

  int i = 0;
  int[][] IA = new int[nx * ny][];
  for (i = 0; i < nx * ny; i++) IA[i] = new int[200];
  int[][] IB = new int[nx * ny][];
  for (i = 0; i < nx * ny; i++) IB[i] = new int[200];
  int[][] IC = new int[nx * ny][];
  for (i = 0; i < nx * ny; i++) IC[i] = new int[200];
  int[][] ID = new int[nx * ny][];
  for (i = 0; i < nx * ny; i++) ID[i] = new int[200];

  int[][] XA = new int[nx * ny][];
  for (i = 0; i < nx * ny; i++) XA[i] = new int[200];
  int[][] YA = new int[nx * ny][];
  for (i = 0; i < nx * ny; i++) YA[i] = new int[200];

  int[][] XB = new int[nx * ny][];
  for (i = 0; i < nx * ny; i++) XB[i] = new int[200];
  int[][] YB = new int[nx * ny][];
  for (i = 0; i < nx * ny; i++) YB[i] = new int[200];

  int[][] XC = new int[nx * ny][];
  for (i = 0; i < nx * ny; i++) XC[i] = new int[200];
  int[][] YC = new int[nx * ny][];
  for (i = 0; i < nx * ny; i++) YC[i] = new int[200];

  int[][] XD = new int[nx * ny][];
  for (i = 0; i < nx * ny; i++) XD[i] = new int[200];
  int[][] YD = new int[nx * ny][];
  for (i = 0; i < nx * ny; i++) YD[i] = new int[200];


  int[, ,] ArcSys = new int[200, 9, 3];

  /*
  double[] Dxm = new double[nx * ny];
  double[] Dym = new double[nx * ny];
  double[] Drad = new double[nx * ny];
  for (i = 0; i < nx * ny; i++)
  {
    CntA[i] = CntB[i] = 0;
    Dxm[i] = 0.0;
    Dym[i] = 0.0;
    Drad[i] = 0.0;
  }

  //Graphics g = fm1.pictureBox2.CreateGraphics();
  //Rectangle rect, rect1;
  Pen whitePen = new Pen(Color.White, 2);
  Pen redPen = new Pen(Color.Red, 2);

  int maX = fm1.marginX;
  int maY = fm1.marginY;
  double Scale1 = fm1.Scale1;
  int x;
  // Find the arcs C5 to C8 of type "a" and "b":
  for (int ia = 1; ia < nArc; ia++) //=======================================================================
  {
    if (ia == 638 || ia == 141) //&& Arc[ia].xb > 390 && Arc[ia].xb < 430 && Arc[ia].yb > 240 && Arc[ia].yb < 260)
      MessageBox.Show("**RBN0: ia=" + ia + " xbe=" + (int)(Arc[ia].xb + Arc[ia].xe) * 0.5 +
        " m=(" + (int)Arc[ia].xm + "," + Arc[ia].ym + ") rad=" + (int)Arc[ia].rad + " 1/5=" + (Img.height * 1 / 5));

    if (Math.Abs(Arc[ia].rad) < minRad || Math.Abs(Arc[ia].rad) > maxRad || Arc[ia].ym < (float)(Img.height * 1 / 5))
    {
      //if ((int)Math.Abs(Arc[ia].rad) == 56) 
      //MessageBox.Show("R: ia=" + ia + " rad=" + Arc[ia].rad + " ym=" + Arc[ia].ym);
      continue;
    }

    if (Math.Abs(Arc[ia].yb - Arc[ia].ye) < 30.0F && Math.Abs(Arc[ia].xm - (Arc[ia].xb + Arc[ia].xe) * 0.5F) < 30.0F)
    {
      x = (int)Arc[ia].xm / Size;
      if (deb) MessageBox.Show("RBN_A: x=" + x + " TiefZ=" + (int)(Math.Min(Arc[ia].yb, Arc[ia].ye) + minRad) + " ym=" + (int)Arc[ia].ym +
        " HochZ=" + (int)(Math.Min(Arc[ia].yb, Arc[ia].ye) + maxRad) + " ybe=" +
        (int)Math.Min(Arc[ia].yb, Arc[ia].ye) + " rad=" + (int)Arc[ia].rad);

      if (deb) MessageBox.Show("RBN_B: x=" + x + " TiefZ=" + (int)(Math.Max(Arc[ia].yb, Arc[ia].ye) - maxRad) + " ym=" + (int)Arc[ia].ym +
      " HochZ=" + (int)(Math.Max(Arc[ia].yb, Arc[ia].ye) - minRad) + " ybe=" +
      (int)Math.Max(Arc[ia].yb, Arc[ia].ye) + " rad=" + (int)Arc[ia].rad);

      if (deb) MessageBox.Show("CondB: TZ=" + (int)(Math.Max(Arc[ia].yb, Arc[ia].ye) - maxRad) + " < ym=" + (int)Arc[ia].ym + " < HZ=" +
        (int)(Math.Max(Arc[ia].yb, Arc[ia].ye) - minRad));

      if (Arc[ia].ym > Math.Max(Arc[ia].yb, Arc[ia].ye) - maxRad &&   // Cond B: TiefZ = ybe - max; HochZ = ybe - min
          Arc[ia].ym < Math.Max(Arc[ia].yb, Arc[ia].ye) - minRad)
      {
        IB[x][CntB[x]] = ia;
        XB[x][CntB[x]] = (int)Arc[ia].xm;
        YB[x][CntB[x]] = (int)Math.Max(Arc[ia].yb, Arc[ia].ye);
        CntB[x]++;
        if (deb)
          MessageBox.Show("RBN2_B: x=" + x + " m=(" + (int)Arc[ia].xm + "," + (int)Arc[ia].ym + ") be=(" +
            (int)(Arc[ia].xb + Arc[ia].xe) / 2 + "," + (int)Math.Max(Arc[ia].yb, Arc[ia].ye) + ") rad=" + (int)Arc[ia].rad);
      }

      if (deb) MessageBox.Show("CondA: TZ=" + (int)(Math.Min(Arc[ia].yb, Arc[ia].ye) + minRad) + " < ym=" + (int)Arc[ia].ym + " < HZ=" +
        (int)(Math.Min(Arc[ia].yb, Arc[ia].ye) + maxRad));

      if (Arc[ia].ym > Math.Min(Arc[ia].yb, Arc[ia].ye) + minRad && // Cond A: TiefZ = ybe + minRad HochZ = ybe + maxRad
          Arc[ia].ym < Math.Min(Arc[ia].yb, Arc[ia].ye) + maxRad)
      {
        IA[x][CntA[x]] = ia;
        XA[x][CntA[x]] = (int)Arc[ia].xm;
        YA[x][CntA[x]] = (int)Math.Min(Arc[ia].yb, Arc[ia].ye);
        CntA[x]++;
        if (deb)
          MessageBox.Show("RBN2_A: x=" + x + " m=(" + (int)Arc[ia].xm + "," + (int)Arc[ia].ym + ") be=(" +
            (int)(Arc[ia].xb + Arc[ia].xe) / 2 + "," + (int)Math.Min(Arc[ia].yb, Arc[ia].ye) + ") rad=" + (int)Arc[ia].rad);
      }
    }

    if (Math.Abs(Arc[ia].xb - Arc[ia].xe) < 30 && Math.Abs(Arc[ia].ym - (Arc[ia].yb + Arc[ia].ye) * 0.5F) < 15.0)
    {
      x = (int)Math.Min(Arc[ia].xb, Arc[ia].xe) / Size;
      if (deb) MessageBox.Show("CondC: TZ=" + (int)(Math.Min(Arc[ia].xb, Arc[ia].xe) + minRad) +
        " < xm=" + (int)Arc[ia].xm + " < HZ=" + (int)(Math.Min(Arc[ia].xb, Arc[ia].xe) + maxRad));

      if (Arc[ia].xm > Math.Min(Arc[ia].xb, Arc[ia].xe) + minRad && // Cond C: TiefZ = xbe + minRad HochZ = xbe + maxRad
          Arc[ia].xm < Math.Min(Arc[ia].xb, Arc[ia].xe) + maxRad)
      {
        IC[x][CntC[x]] = ia;
        YC[x][CntC[x]] = (int)Arc[ia].ym;
        XC[x][CntC[x]] = (int)Math.Min(Arc[ia].xb, Arc[ia].xe);
        CntC[x]++;
        if (deb)
          MessageBox.Show("RBN2_C: ia=" + ia + " x=" + x + " m=(" + (int)Arc[ia].xm + "," + (int)Arc[ia].ym +
            ") be=(" + (int)Math.Min(Arc[ia].xb, Arc[ia].xe) + "," + (int)((Arc[ia].yb + Arc[ia].ye) * 0.5F) +
            ") XC=" + XC[x][CntC[x]] + " YC=" + YC[x][CntC[x]] + " CntC[x] after =" + CntC[x]);
      }

      if (deb) MessageBox.Show("CondD: TZ=" + (int)(Math.Max(Arc[ia].xb, Arc[ia].xe) - maxRad) +
                " < xm=" + (int)Arc[ia].xm + " < HZ=" + (int)(Math.Min(Arc[ia].xb, Arc[ia].xe) - minRad));

      if (Arc[ia].xm > Math.Max(Arc[ia].xb, Arc[ia].xe) - maxRad && // Cond C: TiefZ = xbe - minRad HochZ = xbe + maxRad
          Arc[ia].xm < Math.Min(Arc[ia].xb, Arc[ia].xe) - minRad)
      {
        ID[x][CntD[x]] = ia;
        YD[x][CntD[x]] = (int)Arc[ia].ym;
        XD[x][CntD[x]] = (int)Math.Max(Arc[ia].xb, Arc[ia].xe);
        CntD[x]++;
        if (deb)
          MessageBox.Show("RBN2_D: ia=" + ia + " x=" + x + " ym=" + Arc[ia].ym + " xbe=" + (int)Math.Max(Arc[ia].xb, Arc[ia].xe));
      }

    }
  } //====================================== end for (int ia ... ======================================

  for (int ii = 0; ii < CntA[13]; ii++)
    MessageBox.Show("+++ ii=" + ii + " IA[13][ii]=" + IA[13][ii]);
  // Groups of arcs:
  int maxDifCoord = 20;
  int[] Delete = new int[100];
  bool found = false;
  int k = 0;
  for (x = 0; x < nx; x++) //===============================================================
  {
    int cnt = 0;
    if ((CntA[x] > 0 || CntB[x] > 0) && deb)
      MessageBox.Show("RBNb: x=" + x + " CntA[x]=" + CntA[x] + " CntB[x]=" + CntB[x]);
    if (CntA[x] > 1)
    {
      for (i = 0; i < CntA[x]; i++) //======================================================
      {
        int X = XA[x][i];
        int Y = YA[x][i];
        k = 0;
        if (i > 0)
          for (int j = 0; j < i; j++)
          {
            if (Math.Abs(XA[x][j] - X) < maxDifCoord && Math.Abs(YA[x][j] - Y) < maxDifCoord)
            {
              found = true;
              if (deb) MessageBox.Show("RBN11: x=" + x + " arc=" + IA[x][j] + " is bad");
              break;
            }
          }

        if (!found) k++;
        if (i > k)
        {
          IA[x][k] = IA[x][i];
          XA[x][k] = XA[x][i];
          YA[x][k] = YA[x][i];
          cnt++;
        }
      } //================================= end for (i ... ========================================
      CntA[x] -= cnt;
      //if (deb) 
      MessageBox.Show("RBN12: x=" + x + " CntA[x]=" + CntA[x]);
    }


    if (CntB[x] > 1)
    {
      found = false;
      cnt = 0;
      for (i = 0; i < CntB[x]; i++) //======================================================
      {
        int X = XB[x][i];
        int Y = YB[x][i];
        k = 0;
        if (i > 0)
          for (int j = 0; j < i; j++)
          {
            if (Math.Abs(XB[x][j] - X) < maxDifCoord && Math.Abs(YB[x][j] - Y) < maxDifCoord)
            {
              found = true;
              if (deb) MessageBox.Show("RBN13: x=" + x + " arc=" + IB[x][j] + " is bad");
              break;
            }
          }

        if (!found) k++;
        if (i > k)
        {
          IB[x][k] = IB[x][i];
          XB[x][k] = XB[x][i];
          YB[x][k] = YB[x][i];
          cnt++;
        }
      } //================================= end for (i ... ========================================
      CntB[x] -= cnt;
      if (deb) MessageBox.Show("RBN14: x=" + x + " CntB[x]=" + CntB[x]);
    }
  } //============================= end for (x ... ======================================================

  // Drawing the found arcs:

  for (x = 0; x < nx; x++) //=================================================================
  {
    if (deb)
      MessageBox.Show("RBN1: x=" + x + " CntA[x]=" + CntA[x] + " CntB[x]=" + CntB[x] +
   " CntC[x]=" + CntC[x] + " CntD[x]=" + CntD[x] + " x*Size=" + (x * Size));
    if (CntA[x] > 0)
      for (i = 0; i < CntA[x]; i++)
      {
        g.DrawLine(whitePen, (int)((XA[x][i] - 10) * Scale1) + maX, (int)(YA[x][i] * Scale1) + maY,
                         (int)((XA[x][i] + 10) * Scale1) + maX, (int)(YA[x][i] * Scale1) + maY);
        g.DrawLine(whitePen, (int)((XA[x][i]) * Scale1) + maX, (int)(YA[x][i] * Scale1) + maY,
                         (int)((XA[x][i]) * Scale1) + maX, (int)((YA[x][i] + 30) * Scale1) + maY);
        if (XA[x][i] > Img.width / 2 && deb) MessageBox.Show("RBN1a: IA[x][i]=" + IA[x][i] + " ym=" + Arc[IA[x][i]].ym);
      }

    if (CntB[x] > 0)
      for (i = 0; i < CntB[x]; i++)
      {
        g.DrawLine(whitePen, (int)((XB[x][i] - 10) * Scale1) + maX, (int)(YB[x][i] * Scale1) + maY,
                         (int)((XB[x][i] + 10) * Scale1) + maX, (int)(YB[x][i] * Scale1) + maY);
        g.DrawLine(whitePen, (int)((XB[x][i]) * Scale1) + maX, (int)(YB[x][i] * Scale1) + maY,
                         (int)((XB[x][i]) * Scale1) + maX, (int)((YB[x][i] - 30) * Scale1) + maY);
      }


    if (deb) MessageBox.Show("RBNx: x=" + x + " CntC[x]=" + CntC[x]);
    if (CntC[x] > 0)
      for (i = 0; i < CntC[x]; i++)
      {
        g.DrawLine(whitePen, (int)((XC[x][i]) * Scale1) + maX, (int)((YC[x][i] - 10) * Scale1) + maY,
                         (int)((XC[x][i]) * Scale1) + maX, (int)((YC[x][i] + 10) * Scale1) + maY);
        g.DrawLine(whitePen, (int)((XC[x][i]) * Scale1) + maX, (int)(YC[x][i] * Scale1) + maY,
                         (int)((XC[x][i] + 30) * Scale1) + maX, (int)((YC[x][i]) * Scale1) + maY);
      }

    if (CntD[x] > 0)
      for (i = 0; i < CntD[x]; i++)
      {
        g.DrawLine(whitePen, (int)((XD[x][i]) * Scale1) + maX, (int)((YD[x][i] - 10) * Scale1) + maY,
                         (int)((XD[x][i]) * Scale1) + maX, (int)((YD[x][i] + 10) * Scale1) + maY);
        g.DrawLine(whitePen, (int)((XD[x][i]) * Scale1) + maX, (int)(YD[x][i] * Scale1) + maY,
                         (int)((XD[x][i] - 30) * Scale1) + maX, (int)((YD[x][i]) * Scale1) + maY);
      }
  } //================================0 end for (x ... =================================================== 

  if (deb)
  {
    for (i = 0; i < CntA[5]; i++)
      MessageBox.Show("x=" + 5 + " i=" + i + " The A-arc " + IA[5][i] + " has XA=" + XA[5][i] + " YA=" + YA[5][i]);

    for (int j = 0; j < CntB[5]; j++)
      MessageBox.Show("x=" + 5 + " j=" + j + " The B-arc " + IB[5][j] + " has XB=" + XB[5][j] + " YB=" + YB[5][j]);
  }

  // Constructing the versions:
  int ia5 = -1, ia6 = -1, ia8 = -1, maxDifX56 = 20, minDifY56 = 2 * minRad, maxDifY56 = 2 * maxRad;
  //int y1 = 0;
  bool foundA = false; // foundB = false;
  int Vers = 0, minXax2 = (int)(4.7 * minRad);
  for (x = 0; x < nx; x++) //=======================================================
  {
    if (deb) MessageBox.Show("RBN2: The memory x=" + x + " contains " + CntA[x] + " A-arcs and " +
        CntB[x] + " B-arcs");
    if (x < (Img.width - minXax2) / Size && CntA[x] > 0) //-------------------------------------------------------
    {
      foundA = true;
      for (i = 0; i < CntA[x]; i++) //==================================
      {
        ia5 = IA[x][i]; // an arc of type 'a'
        foundA = true;
        if (deb)
          MessageBox.Show("RBN3: found ia5=" + ia5 + " at x=" + x + " i=" + i + " IA[x][i]=" + IA[x][i] +
            " XA=" + XA[x][i] + " YA=" + YA[x][i] + " Vers=" + Vers);
        ArcSys[Vers, 5, 0] = IA[x][i];
        ArcSys[Vers, 5, 1] = XA[x][i];
        ArcSys[Vers, 5, 2] = YA[x][i];

        /*
        if (YB[x][i] > ArcSys[Vers, 5, 2] + minRad && YB[x][i] < ArcSys[Vers, 5, 2] + maxRad)
        {
          ArcSys[Vers, 6, 0] = IB[x][i];
          ArcSys[Vers, 6, 1] = XB[x][i];
          ArcSys[Vers, 6, 2] = YB[x][i];
        } 

        for (int x1 = x + (int)(4.0 * minRad / Size); x1 < nx; x1++) //===== search for arc C8 =
        {
          //foundB = false;
          for (int j = 0; j < CntB[x1]; j++) //======================
          {
            // Arc C8:
            if (YB[x1][j] > YA[x][i] + minDifY56 && YB[x1][j] < YA[x][i] + maxDifY56 &&
              Math.Abs(XB[x1][j] - XA[x][i]) < maxDifX56) //----------
            {
              ia8 = IB[x1][j];  // an arc of type 'b' found
              if (deb)
                MessageBox.Show("RBN4: found ia8=" + ia8 + " at x1=" + x1 + " j=" + j + " ia5=" + ia5);
              ArcSys[Vers, 8, 0] = IB[x1][j];
              ArcSys[Vers, 8, 1] = XB[x1][j];
              ArcSys[Vers, 8, 2] = YB[x1][j];

              ArcSys[Vers, 5, 0] = IA[x][i];
              ArcSys[Vers, 5, 1] = XA[x][i];
              ArcSys[Vers, 5, 2] = YA[x][i];

              //foundB = true;
              if (foundA)
              {
                Vers++;
                if (deb) MessageBox.Show("RBNv: Vers=" + Vers + " x=" + x + " i=" + i + " x1=" + x1 + " j=" + j);
              }
            } //---------------------------- end if (Math.Abs(YA ... --------------------------------------------

            /* Arc C7:
            if (Math.Abs(YA[x][i] - YB[x1][j]) < 20 && Math.Abs(YA[x][i] - YB[x1][j]) > minDifY56 &&
              YB[x1][j] > ArcSys[Vers, 5, 2] + minRad && YB[x1][j] < ArcSys[Vers, 5, 2] + maxRad) //----------
            {
              ia8 = IB[x1][j];  // an arc of type 'b' found
              if (deb)
                MessageBox.Show("RBN4: found ia8=" + ia8 + " at x1=" + x1 + " j=" + j + " ia5=" + ia5);
              ArcSys[Vers, 8, 0] = IB[x1][j];
              ArcSys[Vers, 8, 1] = XB[x1][j];
              ArcSys[Vers, 8, 2] = YB[x1][j];

              ArcSys[Vers, 5, 0] = IA[x][i];
              ArcSys[Vers, 5, 1] = XA[x][i];
              ArcSys[Vers, 5, 2] = YA[x][i];

              foundB = true;
              if (foundA)
              {
                Vers++;
                if (deb) MessageBox.Show("RBNv: Vers=" + Vers + " x=" + x + " i=" + i + " x1=" + x1 + " j=" + j);
              }
            } //---------------------------- end if (Math.Abs(YA ... --------------------------------------------
            --*/
/*
if (!foundB && x > 0 && CntB[x - 1] > 0) // search 'b' in x1 -1 ---
{ foundB = false;
  for (int j = 0; j < CntB[x - 1]; j++) //========================
  { if (Math.Abs(XA[x][i] - XB[x - 1][j]) < maxDifX &&
        Math.Abs(YA[x][i] - YB[x - 1][j]) < maxDifY &&
        Math.Abs(YA[x][i] - YB[x - 1][j]) > minDifY)
    { ia6 = IB[x - 1][j];
      if (deb) MessageBox.Show("RBN5: found ia6=" + ia6 + " at x -1 =" + (x - 1) + " j=" + j);
      ArcSys[Vers, 6, 0] = IB[x - 1][j];
      foundB = true;
    }
  } //================== end for (int j ... =================================
} //-------------------- end if (!foundB ... ------------------------------------

if (!foundB && x < nx - 1 && CntB[x + 1] > 0) // search 'b' in x1 +1 
{ foundB = false;
  for (int j = 0; j < CntB[x + 1]; j++) //=========================
  { if (Math.Abs(XA[x][i] - XB[x + 1][j]) < maxDifX &&
        Math.Abs(YA[x][i] - YB[x + 1][j]) < maxDifY &&
        Math.Abs(YA[x][i] - YB[x + 1][j]) > minDifY)
    { ia6 = IB[x + 1][j];
      if (deb) MessageBox.Show("RBN6: found ia6=" + ia6 + " at x + 1 =" + (x + 1) + " j=" + j);
      foundB = true;
    }
  } //================== end for (int j ... ==================================
} //-------------------- end if (!foundB ... ------------------------------------
Vers++;
             
} //====================== end for (i ... =======================================
} //------------------------ end if (CntA ... -------------------------------------------
//if (x1 == nx - 1 && ia5 < 0 && ia6 < 0)
if (deb) MessageBox.Show("RBN7: x=" + x + " ia5=" + ia5 + " ia6=" + ia6);
} //========================== end for (int x ... ==========================================
for (x = 0; x < nx; x++)
if ((CntA[x] != 0 || CntB[x] != 0) && deb) MessageBox.Show("RBN8: x=" + x + " CntA=" + CntA[x] + " CntB=" + CntB[x]);

if (deb)
MessageBox.Show("RBN9: Vers=" + Vers + " arc C5 is " + ArcSys[0, 5, 0] + " C8 is " + ArcSys[2, 8, 0]);
//" Vers = 1; arc C5 is " + ArcSys[1, 5, 0] + " C6 is " + ArcSys[1, 6, 0] + " Vers=" + Vers);
for (int iv = 0; iv < Vers; iv++)
{
x = ArcSys[iv, 5, 1]; int y = ArcSys[iv, 5, 2]; int x1 = ArcSys[iv, 8, 1]; int y1 = ArcSys[iv, 8, 2];
if (deb) MessageBox.Show("RBN10: iv=" + iv + " x=" + x + " y=" + y + "x1=" + x1 + " y1=" + y1);
g.DrawLine(whitePen, maX + (int)(Scale1 * x), maY + (int)(Scale1 * y),
maX + (int)(Scale1 * x1), maY + (int)(Scale1 * y1));
}
} //************************************** end RecoBicycleNew ***********************************


public void DrawPoints(Graphics g, Pen whitePen, double Scale1, int maX, int maY, int halfWidth, int[,] Block, double[] PointX, double[] PointY)
{
int m = 0;
for (m = 0; m < 5; m++)
{
g.DrawLine(whitePen, (int)((PointX[Block[m, 0]] - halfWidth) * Scale1) + maX, (int)(PointY[Block[m, 0]] * Scale1) + maY,
(int)((PointX[Block[m, 0]] + halfWidth) * Scale1) + maX, (int)(PointY[Block[m, 0]] * Scale1) + maY);

g.DrawLine(whitePen, (int)((PointX[Block[m, 0]] + halfWidth) * Scale1) + maX, (int)(PointY[Block[m, 0]] * Scale1) + maY,
(int)((PointX[Block[m, 1]] + halfWidth) * Scale1) + maX, (int)(PointY[Block[m, 1]] * Scale1) + maY);

g.DrawLine(whitePen, (int)((PointX[Block[m, 1]] + halfWidth) * Scale1) + maX, (int)(PointY[Block[m, 1]] * Scale1) + maY,
(int)((PointX[Block[m, 1]] - halfWidth) * Scale1) + maX, (int)(PointY[Block[m, 1]] * Scale1) + maY);

g.DrawLine(whitePen, (int)((PointX[Block[m, 1]] - halfWidth) * Scale1) + maX, (int)(PointY[Block[m, 1]] * Scale1) + maY,
(int)((PointX[Block[m, 0]] - halfWidth) * Scale1) + maX, (int)(PointY[Block[m, 0]] * Scale1) + maY);
}

for (m = 5; m <= 6; m++)
{
g.DrawLine(whitePen,
(int)(PointX[Block[m, 0]] * Scale1) + maX, (int)((PointY[Block[m, 0]] - halfWidth) * Scale1) + maY,
(int)(PointX[Block[m, 0]] * Scale1) + maX, (int)((PointY[Block[m, 0]] + halfWidth) * Scale1) + maY);

g.DrawLine(whitePen,
(int)(PointX[Block[m, 0]] * Scale1) + maX, (int)((PointY[Block[m, 0]] + halfWidth) * Scale1) + maY,
(int)(PointX[Block[m, 1]] * Scale1) + maX, (int)((PointY[Block[m, 1]] + halfWidth) * Scale1) + maY);

g.DrawLine(whitePen,
(int)(PointX[Block[m, 1]] * Scale1) + maX, (int)((PointY[Block[m, 1]] + halfWidth) * Scale1) + maY,
(int)(PointX[Block[m, 1]] * Scale1) + maX, (int)((PointY[Block[m, 1]] - halfWidth) * Scale1) + maY);

g.DrawLine(whitePen,
(int)(PointX[Block[m, 1]] * Scale1) + maX, (int)((PointY[Block[m, 1]] - halfWidth) * Scale1) + maY,
(int)(PointX[Block[m, 0]] * Scale1) + maX, (int)((PointY[Block[m, 0]] - halfWidth) * Scale1) + maY);
} //============================ end for (m = 5; ... ====================================================
} //****************************** end DrawPoints ***********************************************************


public double TestWheel(int xCenter, int yCenter, int Radius, Form1 fm1)
{
Graphics g = fm1.pictureBox2.CreateGraphics();
Pen yellowPen = new Pen(Color.Yellow, 2);
bool deb = false;
int maX = fm1.marginX;
int maY = fm1.marginY;
double Scale1 = fm1.Scale1;

int radVectX, radVectY, scaPro, x, x1, y, y1;
double Dist, length, SumLength = 0.0, maxCos = 6.0 / (double)Radius;
for (int ip = 0; ip < nPolygon; ip++) //==========================================
{
int first = Polygon[ip].firstVert;
int last = Polygon[ip].lastVert;
for (int iv = first; iv < last; iv++)
{
x = Vert[iv].X; y = Vert[iv].Y;
x1 = Vert[iv + 1].X; y1 = Vert[iv + 1].Y;
radVectX = x - xCenter;
radVectY = y - yCenter;
scaPro = Math.Abs((x1 - x) * radVectX + (y1 - y) * radVectY);
length = Math.Sqrt((x1 - x) * (x1 - x) + (y1 - y) * (y1 - y));
Dist = Math.Sqrt((x - xCenter) * (x - xCenter) + (y - yCenter) * (y - yCenter));
if (Math.Abs(Radius - Dist) < 20.0 && deb)
MessageBox.Show("TestW: x=" + x + " y=" + y + " length=" + (int)length +
  " Center=(" + xCenter + "," + yCenter + ") Radius - Dist=" + (int)(Radius - Dist));

if (Math.Abs((double)Radius - Dist) < Radius / 3 && scaPro < length * 6.0 && length > 10.0)
{
SumLength += length;
g.DrawLine(yellowPen, maX + (int)(Scale1 * x), maY + (int)(Scale1 * y),
                      maX + (int)(Scale1 * x1), maY + (int)(Scale1 * y1));
if (deb)
  MessageBox.Show("TestW draws x=" + x + " y=" + y + " Radius=" + Radius + " Dist=" + (int)Dist +
    " Center=(" + xCenter + "," + yCenter + ")");
}
}
} //=============================== end for (int ip ==================================

return SumLength;
} //******************************** end TestWheel *****************************************

public double TestWheelNew(int xCenter, int yCenter, int Radius, double Reduct, Form1 fm1)
{
Graphics g = fm1.pictureBox2.CreateGraphics();
Pen yellowPen = new Pen(Color.Yellow, 2);

int maX = fm1.marginX;
int maY = fm1.marginY;
double Scale1 = fm1.Scale1;
bool deb = false;
int x, x1, y, y1;
double radVectX, radVectY, scaPro;
//double xr;
double delX, delY, Dist, length, redLen, SumLength = 0.0, maxCos = 6.0 / (double)Radius;
for (int ip = 0; ip < nPolygon; ip++) //==========================================
{
int first = Polygon[ip].firstVert;
int last = Polygon[ip].lastVert;
for (int iv = first; iv < last; iv++)
{
x = Vert[iv].X; y = Vert[iv].Y;
x1 = Vert[iv + 1].X; y1 = Vert[iv + 1].Y;
radVectX = (x - xCenter) / Reduct;
radVectY = (double)(y - yCenter);
delX = (x1 - x) / Reduct;
delY = (double)(y1 - y);
length = Math.Sqrt((x1 - x) * (x1 - x) + (y1 - y) * (y1 - y));
redLen = Math.Sqrt(delX * delX + delY * delY);
Dist = Math.Sqrt(radVectX * radVectX + radVectY * radVectY);
scaPro = Math.Abs((x1 - x) * radVectX + (y1 - y) * radVectY) / redLen / Dist;
if (Math.Abs(Radius - Dist) < 20.0 && deb)
MessageBox.Show("TestWN: x=" + x + " y=" + y + " length=" + (int)length +
  " Center=(" + xCenter + "," + yCenter + ") Radius - Dist=" + (int)(Radius - Dist));
// scaPro < redLen*Dist*6.0 / Radius
if (Math.Abs((double)Radius - Dist) < Radius / 3 && scaPro < maxCos && length > 10.0)
{
SumLength += length;
g.DrawLine(yellowPen, maX + (int)(Scale1 * x), maY + (int)(Scale1 * y),
                      maX + (int)(Scale1 * x1), maY + (int)(Scale1 * y1));
if (deb)
  MessageBox.Show("TestWN draws x=" + x + " y=" + y + " Radius=" + Radius + " Dist=" + (int)Dist +
    " Center=(" + xCenter + "," + yCenter + ")");
}
}
} //=============================== end for (int ip ==================================

return SumLength;
} //******************************** end TestWheelNew *****************************************


public void RecoBicycleMode(CImage Img, int Size, int minRad, int maxRad, Form1 fm1)
{
Graphics g = fm1.pictureBox2.CreateGraphics();

int nx, ny; //, size = 0;
bool deb = false, drawWhite = true;
double dSize = (double)Size;
nx = 1 + Img.width / Size; // it was 2 + Img.Width / Size
ny = 1 + Img.height / Size;
int[] CntA = new int[nx * ny];
int[] CntB = new int[nx * ny];
int[] CntC = new int[nx * ny];
int[] CntD = new int[nx * ny];

int i = 0;
int size2D = 1000;
int[,] IA = new int[nx * ny, size2D];
int[,] IB = new int[nx * ny, size2D];
int[,] IC = new int[nx * ny, size2D];
int[,] ID = new int[nx * ny, size2D];

int[,] XA = new int[nx * ny, size2D];
int[,] XB = new int[nx * ny, size2D];
int[,] XC = new int[nx * ny, size2D];
int[,] XD = new int[nx * ny, size2D];

int[,] YA = new int[nx * ny, size2D];
int[,] YB = new int[nx * ny, size2D];
int[,] YC = new int[nx * ny, size2D];
int[,] YD = new int[nx * ny, size2D];



int[, ,] ArcSys = new int[size2D, 9, 3];
int[] Radius = new int[size2D];
int[] xLeft = new int[size2D];
int[] yLeft = new int[size2D];
int[] xRight = new int[size2D];
int[] yRight = new int[size2D];
double[] TestLeft = new double[size2D];
double[] TestRight = new double[size2D];
double[] Reduction = new double[size2D];
double[] RedPointX = new double[size2D];
//Graphics g = fm1.pictureBox2.CreateGraphics();
//Rectangle rect, rect1;
Pen whitePen = new Pen(Color.White, 2);
Pen redPen = new Pen(Color.Red, 2);

int maX = fm1.marginX;
int maY = fm1.marginY;
double Scale1 = fm1.Scale1;
int x, y;
for (int k = 0; k < nx * ny; k++) CntA[k] = CntB[k] = CntC[k] = CntD[k] = 0;
//double[] ProtX = { 0.0, 0.19, 0.96, 1.52, 0.90, 1.02, 2.56, 2.65, 2.75, 3.23, 3.52 };
//double[] ProtY = { 0.0, 0.13, 0.17, 0.10, 1.36, 1.06, 2.00, 1.85, 1.65, 0.36, 0.0 };

double[] ProtX = { 0.0, 0.22, 1.10, 1.75, 1.04, 1.17, 2.94, 3.04, 3.16, 3.71, 4.05 };
double[] ProtY = { 0.0, 0.13, 0.17, 0.10, 1.36, 1.06, 1.60, 1.48, 1.32, 0.18, 0.0 };

int[,] Block = { { 3, 4 }, { 6, 9 }, { 0, 5 }, { 5, 7 }, { 3, 8 }, { 1, 2 }, { 0, 3 } };
double[] A = new double[8];
double[] B = new double[8];

double[] PointX = new double[11];
double[] PointY = new double[11];

int OptRad = -1, OptVer = -1, OptDir = -1;
double OptRed = -1.0;
double SumLength = 0.0, maxSum = 0.0;
int Vers = 0;
for (int rad = 0; rad < 6; rad++) //============================================================
{
//if (deb) 
MessageBox.Show("+++ For rad=" + rad + " starts. nArc=" + nArc);
switch (rad)
{
case 0: minRad = 25; maxRad = 42; break;
case 1: minRad = 35; maxRad = 60; break;
case 2: minRad = 50; maxRad = 70; break;
case 3: minRad = 58; maxRad = 110; break;
case 4: minRad = 80; maxRad = 120; break;
case 5: minRad = 100; maxRad = 180; break;
}


// Find the arcs C5 to C8 of type "a" and "b":
for (int ia = 1; ia < nArc; ia++) //=======================================================================
{
if (deb) MessageBox.Show("ia=" + ia + " rad=" + Math.Abs(Arc[ia].rad));
if (Math.Abs(Arc[ia].rad) < minRad || Math.Abs(Arc[ia].rad) > maxRad || Arc[ia].ym < (float)(Img.height * 2 / 5))
continue;
// Condition for Ca and Cb:
if (Math.Abs(Arc[ia].yb - Arc[ia].ye) < 30.0F && Math.Abs(Arc[ia].xm - (Arc[ia].xb + Arc[ia].xe) * 0.5F) < 30.0F)
{
x = (int)Arc[ia].xm / Size;
y = (int)Math.Min(Arc[ia].yb, Arc[ia].ye) / Size;
if (deb)
  MessageBox.Show("RBM2_A: x=" + x + " y=" + y + " m=(" + (int)Arc[ia].xm + "," + (int)Arc[ia].ym + ") be=(" +
    (int)(Arc[ia].xb + Arc[ia].xe) / 2 + "," + (int)Math.Min(Arc[ia].yb, Arc[ia].ye) + ") A.rad=" + (int)Arc[ia].rad +
    " minRad=" + minRad + " maxRad=" + maxRad);
if (Arc[ia].ym > Math.Min(Arc[ia].yb, Arc[ia].ye) + 10 && // Cond A: TiefZ = ybe + minRad HochZ = ybe + maxRad
    Arc[ia].ym < Math.Min(Arc[ia].yb, Arc[ia].ye) + maxRad && x < nx)
{
  IA[x + nx * y, CntA[x + nx * y]] = ia;
  XA[x + nx * y, CntA[x + nx * y]] = (int)Arc[ia].xm;
  YA[x + nx * y, CntA[x + nx * y]] = (int)Math.Min(Arc[ia].yb, Arc[ia].ye);
  CntA[x + nx * y]++;
  if (deb)
    MessageBox.Show("RBM2_A: x=" + x + " m=(" + (int)Arc[ia].xm + "," + (int)Arc[ia].ym + ") be=(" +
      (int)(Arc[ia].xb + Arc[ia].xe) / 2 + "," + (int)Math.Min(Arc[ia].yb, Arc[ia].ye) + ") rad=" + (int)Arc[ia].rad);
}

x = (int)Arc[ia].xm / Size;
y = (int)Math.Max(Arc[ia].yb, Arc[ia].ye) / Size;

if (Arc[ia].ym > Math.Max(Arc[ia].yb, Arc[ia].ye) - maxRad &&   // Cond B: TiefZ = ybe - max; HochZ = ybe - min
    Arc[ia].ym < Math.Max(Arc[ia].yb, Arc[ia].ye) - minRad && x < nx)
{
  IB[x + nx * y, CntB[x + nx * y]] = ia;
  XB[x + nx * y, CntB[x + nx * y]] = (int)Arc[ia].xm;
  YB[x + nx * y, CntB[x + nx * y]] = (int)Math.Max(Arc[ia].yb, Arc[ia].ye);
  CntB[x + nx * y]++;
  if (deb)
    MessageBox.Show("RBM2_B: x=" + x + " m=(" + (int)Arc[ia].xm + "," + (int)Arc[ia].ym + ") be=(" +
      (int)(Arc[ia].xb + Arc[ia].xe) / 2 + "," + (int)Math.Max(Arc[ia].yb, Arc[ia].ye) + ") rad=" + (int)Arc[ia].rad);
}
} //---------------------------------- end if ( 'Condition for CA and CB:' -------------------------------------

// Condition for Cc and Cd:
if (Math.Abs(Arc[ia].xb - Arc[ia].xe) < 30 && Math.Abs(Arc[ia].ym - (Arc[ia].yb + Arc[ia].ye) * 0.5F) < 30.0)
{
x = (int)Math.Min(Arc[ia].xb, Arc[ia].xe) / Size;
y = (int)Arc[ia].ym / Size;
if (deb) MessageBox.Show("CondC: TZ=" + (int)(Math.Min(Arc[ia].xb, Arc[ia].xe) + minRad) +
  " < xm=" + (int)Arc[ia].xm + " < HZ=" + (int)(Math.Min(Arc[ia].xb, Arc[ia].xe) + maxRad));

if (Arc[ia].xm > Math.Min(Arc[ia].xb, Arc[ia].xe) + minRad && // Cond C: TiefZ = xbe + minRad HochZ = xbe + maxRad
    Arc[ia].xm < Math.Min(Arc[ia].xb, Arc[ia].xe) + maxRad && y < ny)
{
  IC[x + nx * y, CntC[x + nx * y]] = ia;
  YC[x + nx * y, CntC[x + nx * y]] = (int)Arc[ia].ym;
  XC[x + nx * y, CntC[x + nx * y]] = (int)Math.Min(Arc[ia].xb, Arc[ia].xe);
  CntC[x + nx * y]++;
  if (deb)
    MessageBox.Show("RBM2_C: ia=" + ia + " x=" + x + " m=(" + (int)Arc[ia].xm + "," + (int)Arc[ia].ym +
      ") be=(" + (int)Math.Min(Arc[ia].xb, Arc[ia].xe) + "," + (int)((Arc[ia].yb + Arc[ia].ye) * 0.5F) +
      ") XC=" + XC[x, CntC[x]] + " YC=" + YC[x, CntC[x]] + " CntC[x] after =" + CntC[x]);
}

if (deb) MessageBox.Show("CondD: TZ=" + (int)(Math.Max(Arc[ia].xb, Arc[ia].xe) - maxRad) +
          " < xm=" + (int)Arc[ia].xm + " < HZ=" + (int)(Math.Min(Arc[ia].xb, Arc[ia].xe) - minRad));

x = (int)Math.Max(Arc[ia].xb, Arc[ia].xe) / Size;
y = (int)Arc[ia].ym / Size;
if (Arc[ia].xm > Math.Max(Arc[ia].xb, Arc[ia].xe) - maxRad && // Cond C: TiefZ = xbe - minRad HochZ = xbe + maxRad
    Arc[ia].xm < Math.Max(Arc[ia].xb, Arc[ia].xe) - minRad && y < ny)
{
  ID[x + nx * y, CntD[x + nx * y]] = ia;
  YD[x + nx * y, CntD[x + nx * y]] = (int)Arc[ia].ym;
  XD[x + nx * y, CntD[x + nx * y]] = (int)Math.Max(Arc[ia].xb, Arc[ia].xe);
  CntD[x + nx * y]++;
  if (deb)
    MessageBox.Show("RBM2_D: ia=" + ia + " x=" + x + " ym=" + Arc[ia].ym + " xbe=" + (int)Math.Max(Arc[ia].xb, Arc[ia].xe));
}

} //------------------------------------ end if ( 'Condition for Cc and Cd:' -----------------------
} //====================================== end for (int ia ... ==========================================================

// Averaging the coordinates in blocks:
for (y = 0; y < ny; y++) //===============================================================
for (x = 0; x < nx; x++) //===============================================================
{
//if (CntA[x + nx * y] > 0 || CntB[x + nx * y] > 0 || CntC[x + nx * y] > 0 || CntD[x + nx * y] > 0) 
//MessageBox.Show("RBM0: rad=" + rad + " x=" + x + " y=" + y + " CntA=" + CntA[x + nx * y] +
//" CntB=" + CntB[x + nx * y] + " CntC=" + CntC[x + nx * y] + " CntD=" + CntD[x + nx * y]);
int SumX = 0, SumY = 0;
if (CntA[x + nx * y] > 1)
{
  for (i = 0; i < CntA[x + nx * y]; i++) //====================
  {
    SumX += XA[x + nx * y, i];
    SumY += YA[x + nx * y, i];
  }
  XA[x + nx * y, 0] = SumX / CntA[x + nx * y];
  YA[x + nx * y, 0] = SumY / CntA[x + nx * y];
  CntA[x + nx * y] = 1;
}

SumX = 0; SumY = 0;
if (CntB[x + nx * y] > 1)
{
  for (i = 0; i < CntB[x + nx * y]; i++) //======================
  {
    SumX += XB[x + nx * y, i];
    SumY += YB[x + nx * y, i];
  }
  XB[x + nx * y, 0] = SumX / CntB[x + nx * y];
  YB[x + nx * y, 0] = SumY / CntB[x + nx * y];
  CntB[x + nx * y] = 1;
}

SumX = 0; SumY = 0;
if (CntC[x + nx * y] > 1)
{
  for (i = 0; i < CntC[x + nx * y]; i++) //======================
  {
    SumX += XC[x + nx * y, i];
    SumY += YC[x + nx * y, i];
  }
  XC[x + nx * y, 0] = SumX / CntC[x + nx * y];
  YC[x + nx * y, 0] = SumY / CntC[x + nx * y];
  CntC[x + nx * y] = 1;
}


SumX = 0; SumY = 0;
if (CntD[x + nx * y] > 1)
{
  for (i = 0; i < CntD[x + nx * y]; i++) //======================
  {
    SumX += XD[x + nx * y, i];
    SumY += YD[x + nx * y, i];
  }
  XD[x + nx * y, 0] = SumX / CntD[x + nx * y];
  YD[x + nx * y, 0] = SumY / CntD[x + nx * y];
  CntD[x + nx * y] = 1;
}

} //============================= end for (x ... ======================================================

if (deb)
{
for (y = 0; y < ny; y++)
for (x = 0; x < nx; x++)
{
  if (CntA[x + nx * y] > 0) MessageBox.Show("RBM0: x=" + x + " y=" + y + " CntA=" + CntA[x + nx * y]);
  if (CntB[x + nx * y] > 0) MessageBox.Show("RBM0: x=" + x + " y=" + y + " CntB=" + CntB[x + nx * y]);
  if (CntC[x + nx * y] > 0) MessageBox.Show("RBM0: x=" + x + " y=" + y + " CntC=" + CntC[x + nx * y]);
  if (CntD[x + nx * y] > 0) MessageBox.Show("RBM0: x=" + x + " y=" + y + " CntD=" + CntD[x + nx * y]);
}
}
//Graphics g = fm1.pictureBox2.CreateGraphics();

// Drawing the found arcs:
for (y = 0; y < ny; y++) //=================================================================
for (x = 0; x < nx; x++) //=================================================================
{
if (deb)
  MessageBox.Show("RBM1: x=" + x + " y=" + y + " CntA[x + nx*y]=" + CntA[x + nx * y] + " CntB[x + nx*y]=" +
    CntB[x + nx * y] + " CntC[x + nx*y]=" + CntC[x + nx * y] + " CntD[x + nx*y]=" + CntD[x + nx * y]);
if (CntA[x + nx * y] > 0)
{
  g.DrawLine(whitePen, (int)((XA[x + nx * y, 0] - 10) * Scale1) + maX, (int)(YA[x + nx * y, 0] * Scale1) + maY,
                    (int)((XA[x + nx * y, 0] + 10) * Scale1) + maX, (int)(YA[x + nx * y, 0] * Scale1) + maY);
  g.DrawLine(whitePen, (int)((XA[x + nx * y, 0]) * Scale1) + maX, (int)(YA[x + nx * y, 0] * Scale1) + maY,
                    (int)((XA[x + nx * y, 0]) * Scale1) + maX, (int)((YA[x + nx * y, 0] + 30) * Scale1) + maY);
  if (XA[x + nx * y, 0] > Img.width / 2 && deb) MessageBox.Show("RBM1a: IA[x + nx*y, 0]=" + IA[x + nx * y, 0] + " ym=" + Arc[IA[x + nx * y, 0]].ym);
}

if (CntB[x + nx * y] > 0)
{
  g.DrawLine(whitePen, (int)((XB[x + nx * y, 0] - 10) * Scale1) + maX, (int)(YB[x + nx * y, 0] * Scale1) + maY,
                    (int)((XB[x + nx * y, 0] + 10) * Scale1) + maX, (int)(YB[x + nx * y, 0] * Scale1) + maY);
  g.DrawLine(whitePen, (int)((XB[x + nx * y, 0]) * Scale1) + maX, (int)(YB[x + nx * y, 0] * Scale1) + maY,
                    (int)((XB[x + nx * y, 0]) * Scale1) + maX, (int)((YB[x + nx * y, 0] - 30) * Scale1) + maY);
}


if (deb) MessageBox.Show("RBMx: x=" + x + " CntC[x]=" + CntC[x + nx * y]);
if (CntC[x + nx * y] > 0)
{
  g.DrawLine(whitePen, (int)((XC[x + nx * y, 0]) * Scale1) + maX, (int)((YC[x + nx * y, 0] - 10) * Scale1) + maY,
                    (int)((XC[x + nx * y, 0]) * Scale1) + maX, (int)((YC[x + nx * y, 0] + 10) * Scale1) + maY);
  g.DrawLine(whitePen, (int)((XC[x + nx * y, 0]) * Scale1) + maX, (int)(YC[x + nx * y, 0] * Scale1) + maY,
                    (int)((XC[x + nx * y, 0] + 30) * Scale1) + maX, (int)((YC[x + nx * y, 0]) * Scale1) + maY);
}

if (CntD[x + nx * y] > 0)
{
  g.DrawLine(whitePen, (int)((XD[x + nx * y, 0]) * Scale1) + maX, (int)((YD[x + nx * y, 0] - 10) * Scale1) + maY,
                    (int)((XD[x + nx * y, 0]) * Scale1) + maX, (int)((YD[x + nx * y, 0] + 10) * Scale1) + maY);
  g.DrawLine(whitePen, (int)((XD[x + nx * y, 0]) * Scale1) + maX, (int)(YD[x + nx * y, 0] * Scale1) + maY,
                    (int)((XD[x + nx * y, 0] - 30) * Scale1) + maX, (int)((YD[x + nx * y, 0]) * Scale1) + maY);
}
} //================================ end for (x ... =================================================== 

if (deb)
{
for (i = 0; i < CntA[5]; i++)
MessageBox.Show("x=" + 5 + " i=" + i + " The A-arc " + IA[5, i] + " has XA=" + XA[5, i] + " YA=" + YA[5, i]);

for (int j = 0; j < CntB[5]; j++)
MessageBox.Show("x=" + 5 + " j=" + j + " The B-arc " + IB[5, j] + " has XB=" + XB[5, j] + " YB=" + YB[5, j]);
}

// Constructing lists of types:
int[,] ListA = new int[size2D, 2];
int[,] ListB = new int[size2D, 2];
int[,] ListC = new int[size2D, 2];
int[,] ListD = new int[size2D, 2];
int cntPairsA = 0, cntPairsB = 0, cntPairsC = 0, cntPairsD = 0;

for (y = 0; y < ny; y++)
for (x = 0; x < nx; x++)
{
if (CntA[x + nx * y] > 0)
{
  ListA[cntPairsA, 0] = x;
  ListA[cntPairsA, 1] = y;
  cntPairsA++;
}
if (CntB[x + nx * y] > 0)
{
  ListB[cntPairsB, 0] = x;
  ListB[cntPairsB, 1] = y;
  cntPairsB++;
}
if (CntC[x + nx * y] > 0)
{
  ListC[cntPairsC, 0] = x;
  ListC[cntPairsC, 1] = y;
  cntPairsC++;
}
if (CntD[x + nx * y] > 0)
{
  ListD[cntPairsD, 0] = x;
  ListD[cntPairsD, 1] = y;
  cntPairsD++;
}
} //============================= end for (x ... ==============================

// Constructing the versions:
int minDifY56 = 2 * minRad, maxDifY56 = 2 * maxRad;
int minXax2 = (int)(4.7 * minRad);
if (deb)
MessageBox.Show("RBM: Constructing versions, rad=" + rad + " OptVer=" + OptVer + " cntPairsA=" + cntPairsA + " cntPairsB=" + cntPairsB);
//if (OptVer < 0)
for (i = 0; i < cntPairsA; i++) //=======================================================
for (int j = 0; j < cntPairsB; j++)
{
int xA = ListA[i, 0];
int yA = ListA[i, 1];
int xB = ListB[j, 0];
int yB = ListB[j, 1];
int Tol = Size;
if (deb) //(xA == 5 || xA == 13) && yA == 8 && deb)
  MessageBox.Show("RBML: rad=" + rad + " i=" + i + " j=" + j + " xA=" + xA + " yA=" + yA + " xB=" + xB + " yB=" + yB +
    " 3.7*minRad/Size=" + (int)(3.7 * minRad / Size) + " 3.7*maxRad/Size=" + (int)(3.7 * maxRad / Size));

if (xB > xA + 3.7 * minRad / Size && xB < xA + 3.7 * maxRad / Size && yB > yA + (2 * minRad + Tol) / Size &&
  yB < yA + (2 * maxRad - Tol) / Size)
{
  ArcSys[Vers, 5, 0] = XA[xA + nx * yA, 0];
  ArcSys[Vers, 5, 1] = YA[xA + nx * yA, 0];
  ArcSys[Vers, 8, 0] = XB[xB + nx * yB, 0];
  ArcSys[Vers, 8, 1] = YB[xB + nx * yB, 0];
  Radius[Vers] = (YB[xB + nx * yB, 0] - YA[xA + nx * yA, 0]) / 2;
  xLeft[Vers] = XA[xA + nx * yA, 0];
  yLeft[Vers] = (YA[xA + nx * yA, 0] + YB[xB + nx * yB, 0]) / 2;
  xRight[Vers] = XB[xB + nx * yB, 0];
  yRight[Vers] = (YA[xA + nx * yA, 0] + YB[xB + nx * yB, 0]) / 2;
  Reduction[Vers] = ((double)XB[xB + nx * yB, 0] - (double)XA[xA + nx * yA, 0]) / (4.0 * Radius[Vers]);
  TestLeft[Vers] = TestWheelNew(xLeft[Vers], yLeft[Vers], Radius[Vers], Reduction[Vers], fm1);
  TestRight[Vers] = TestWheelNew(xRight[Vers], yRight[Vers], Radius[Vers], Reduction[Vers], fm1);
  if (deb)
    MessageBox.Show("RBMa 5,8: rad=" + rad + " i=" + i + " j=" + j + " Radius=" + Radius[Vers] +
      " xLeft=" + xLeft[Vers] + " xRight=" + xRight[Vers] + " yLeft=" + yLeft[Vers] + " yRight=" + yRight[Vers] +
      " Test left wheel=" + TestLeft[Vers] + " Test right wheel=" + TestRight[Vers] + " Vers=" + Vers + " Red=" + Reduction[Vers]);
  //if (TestLeft && TestRight) 
  Vers++;
}

if (xB < xA - 3.7 * minRad / Size && xB >= xA - 3.7 * maxRad / Size &&
                    yB > yA + (2 * minRad + Tol) / Size && yB < yA + (2 * maxRad - Tol) / Size)
{
  ArcSys[Vers, 7, 0] = XA[xA + nx * yA, 0];
  ArcSys[Vers, 7, 1] = YA[xA + nx * yA, 0];
  ArcSys[Vers, 6, 0] = XB[xB + nx * yB, 0];
  ArcSys[Vers, 6, 1] = YB[xA + nx * yB, 0];
  Radius[Vers] = (YB[xB + nx * yB, 0] - YA[xA + nx * yA, 0]) / 2;
  xLeft[Vers] = XB[xB + nx * yB, 0];
  yLeft[Vers] = (YA[xA + nx * yA, 0] + YB[xB + nx * yB, 0]) / 2;
  xRight[Vers] = XA[xA + nx * yA, 0];
  yRight[Vers] = (YA[xA + nx * yA, 0] + YB[xB + nx * yB, 0]) / 2;
  Reduction[Vers] = ((double)XA[xA + nx * yA, 0] - (double)XB[xB + nx * yB, 0]) / (3.7 * Radius[Vers]);
  TestLeft[Vers] = TestWheelNew(xLeft[Vers], yLeft[Vers], Radius[Vers], Reduction[Vers], fm1);
  TestRight[Vers] = TestWheelNew(xRight[Vers], yRight[Vers], Radius[Vers], Reduction[Vers], fm1);
  if (deb)
    MessageBox.Show("RBMb 6,7: rad=" + rad + " i=" + i + " j=" + j + " Radius=" + Radius[Vers] +
    " xLeft=" + xLeft[Vers] + " xRight=" + xRight[Vers] + " yLeft=" + yLeft[Vers] + " yRight=" + yRight[Vers] +
      " Test left wheel=" + TestLeft[Vers] + " Test right wheel=" + TestRight[Vers] + " Vers=" + Vers + " Red=" + Reduction[Vers]);
  Vers++;
}
//if (TestLeft[Vers] && TestRight[Vers]) OptVer = Vers;

} //========================== end for (j ... ==========================================
if (deb)
{
if (Vers > 0)
MessageBox.Show("***** RBMv: rad=" + rad + " Vers-1=" + (Vers - 1) + " Radius=" + Radius[Vers - 1] + " OptVer=" +
  OptVer + " Reductin=" + Reduction[Vers - 1]);
else MessageBox.Show("After constructing versions: rad=" + rad + " Vers=" + Vers);
}


// Testing the versions:
for (int ver = 0; ver < Vers; ver++) //====================================================================
{
if (deb)
MessageBox.Show("rad=" + rad + " for (int ver starts; ver=" + ver + " Vers=" + Vers);
int distCenters = xRight[ver] - xLeft[ver];

int leftCenterX = xLeft[ver];
int leftCenterY = yLeft[ver];

//if (OptVer >= 0 && ver == OptVer)
for (int Dir = 0; Dir < 2; Dir++) // ========== Dir == 0: to right; Dir == 1: to left ===============
{
RedPointX[ver] = (xRight[ver] - xLeft[ver]) / (ProtX[10] * Radius[ver]);
if (deb)
  MessageBox.Show("RBMw 'for Dir' starts: ver=" + ver + " Red=" + RedPointX[ver] + " xRight - xleft=" + (int)(xRight[ver] - xLeft[ver]) +
  " Radius=" + Radius[ver] + " ProtX[10]=" + ProtX[10] + " xLeft=" + xLeft[ver]);
for (int k = 0; k < 11; k++)
{
  if (Dir == 0) PointX[k] = xLeft[ver] + ProtX[k] * Radius[ver] * RedPointX[ver];
  else PointX[k] = xRight[ver] - ProtX[k] * Radius[ver] * RedPointX[ver];
  PointY[k] = yLeft[ver] - ProtY[k] * Radius[ver];
  if (k == 9 && deb)
    MessageBox.Show("RBMm: ver=" + ver + " yLeft=" + yLeft[ver] + " Radius=" + Radius[ver] +
    " PointX[k]=" + PointX[k] + " k=" + k + " ProtX[k]=" + ProtX[k]);
}
double Root1;
double halfWidth = 20.0;
int m = 0;
for (m = 0; m < 7; m++)
{
  A[m] = PointY[Block[m, 0]] - PointY[Block[m, 1]];
  B[m] = PointX[Block[m, 1]] - PointX[Block[m, 0]];
  Root1 = Math.Sqrt(A[m] * A[m] + B[m] * B[m]);
  A[m] /= Root1;
  B[m] /= Root1;
} //=============================== end for (m = 0; ... ================================================

Pen yellowPen = new Pen(Color.Yellow, 2);
SumLength = TestLeft[ver] + TestRight[ver];
//if (ver == OptVer)
if (deb) MessageBox.Show("rad=" + rad + " ver=" + ver + " Dir=" + Dir + " OptVer=" + OptVer + "Starting 'for m'");
for (m = 0; m <= 6; m++) //======================================================================================
{
  if (deb)
    MessageBox.Show("RBM_1: starting 'for (m='" + m);
  double Neig = (PointX[Block[m, 1]] - PointX[Block[m, 0]]) / (double)(PointY[Block[m, 1]] - PointY[Block[m, 0]]);
  if (drawWhite)
    DrawPoints(g, whitePen, Scale1, maX, maY, (int)halfWidth, Block, PointX, PointY);
  double Dist = 0.0, length = 0.0;
  for (int ip = 0; ip < nPolygon; ip++)
    for (int iv = Polygon[ip].firstVert; iv < Polygon[ip].lastVert; iv++)
    {
      length = Math.Sqrt((double)((Vert[iv + 1].X - Vert[iv].X) * (Vert[iv + 1].X - Vert[iv].X) +
                                (Vert[iv + 1].Y - Vert[iv].Y) * (Vert[iv + 1].Y - Vert[iv].Y)));
      if (length < 15.0) continue;
      double neig = (double)(Vert[iv + 1].X - Vert[iv].X) / (double)(Vert[iv + 1].Y - Vert[iv].Y);
      Dist = A[m] * (Vert[iv].X - PointX[Block[m, 0]]) + B[m] * (Vert[iv].Y - PointY[Block[m, 0]]);
      if (deb)
        MessageBox.Show("Dist=" + Dist + " halfWidth=" + halfWidth + " iv=" + iv + " Vert.Y" + Vert[iv].Y + " Max=" +
        Math.Max(PointY[Block[m, 0]], PointY[Block[m, 1]]) + " Min=" + Math.Min(PointY[Block[m, 0]], PointY[Block[m, 1]]));

      if (Math.Abs(Dist) < halfWidth &&
        (m < 5 && Vert[iv].Y < Math.Max(PointY[Block[m, 0]], PointY[Block[m, 1]]) &&
                  Vert[iv].Y > Math.Min(PointY[Block[m, 0]], PointY[Block[m, 1]]) ||
         m >= 5 && Vert[iv].X < Math.Max(PointX[Block[m, 0]], PointX[Block[m, 1]]) &&
                    Vert[iv].X > Math.Min(PointX[Block[m, 0]], PointX[Block[m, 1]])) &&
         Math.Abs(neig - Neig) < 0.12)
      {
        SumLength += Math.Sqrt((double)((Vert[iv + 1].X - Vert[iv].X) * (Vert[iv + 1].X - Vert[iv].X) +
                                (Vert[iv + 1].Y - Vert[iv].Y) * (Vert[iv + 1].Y - Vert[iv].Y)));
        if (deb && A[m] * (Vert[iv].X - PointX[Block[m, 0]]) + B[m] * (Vert[iv].Y - PointY[Block[m, 0]]) < 35.0)
        {
          //Save[nVert] = iv; nVert++;
        }

        if (deb) MessageBox.Show("RBMy: SumLength=" + SumLength + " rad=" + rad + " ver=" + ver + " Dir=" + Dir);


        g.DrawLine(yellowPen, maX + (int)(Vert[iv].X * Scale1), maY + (int)(Vert[iv].Y * Scale1),
          maX + (int)(Vert[iv + 1].X * Scale1), maY + (int)(Vert[iv + 1].Y * Scale1));
        if (deb) MessageBox.Show("RBMxx: rad=" + rad + " ver=" + ver + " Dir=" + Dir + " SumL=" + (int)SumLength);
        if (deb)
          MessageBox.Show("++RB: dist(iv)=" + (A[m] * (Vert[iv].X - PointX[Block[m, 0]]) +
            B[m] * (Vert[iv].Y - PointY[Block[m, 0]])) +
          " Vert=(" + Vert[iv].X + "," + Vert[iv].Y + ")");
      }
    } //=========================== end for (int iv ... ==============================================  
  Pen blackPen = new Pen(Color.Black);
  if (drawWhite) DrawPoints(g, blackPen, Scale1, maX, maY, (int)halfWidth, Block, PointX, PointY);

} //=============================== end for (m ... =====================================================
if (deb)
  MessageBox.Show("RBMq: rad=" + rad + " ver=" + ver + " SumL=" + SumLength + " maxSum=" + maxSum);
if (SumLength > maxSum)
{
  maxSum = SumLength;
  OptRad = rad;
  OptVer = ver;
  OptDir = Dir;
  OptRed = Reduction[ver];
  if (deb)
    MessageBox.Show("++++ RBMx: rad=" + rad + " ver=" + ver + " Dir=" + Dir + " OptVer=" + OptVer +
    " Sum=" + SumLength + " maxSum=" + maxSum);
}

} //================================= end for (int Dir ... =================================================
} //=================================== end for (int ver ... ====================================================
if (deb) MessageBox.Show("The loop 'ver' finished; rad=" + rad);
} //====================================== end for (int rad ... ================================================ 
string[] direct = { "right", "left" };
if (deb)
MessageBox.Show("--- RBMz after 'for rad': OptVer=" + OptVer + " OptDir=" + OptDir + " OptRad=" + OptRad);

if (OptDir >= 0 && OptVer >= 0)
{
//if (deb) 
MessageBox.Show("RBM20: OptRad=" + OptRad + " OptVer= " + OptVer + " OptDir=" + OptDir + " Bicycle going to " +
direct[OptDir] + " is recognised. xLeft=" + xLeft[OptVer] + " yLeft=" + yLeft[OptVer] +
" Radius=" + Radius[OptVer] + " xRight=" + xRight[OptVer] +
" Reduction=" + Reduction[OptVer]);

for (int k = 0; k < 11; k++)
{
if (OptDir == 0) PointX[k] = xLeft[OptVer] + ProtX[k] * Radius[OptVer] * RedPointX[OptVer];
else PointX[k] = xRight[OptVer] - ProtX[k] * Radius[OptVer] * RedPointX[OptVer];
PointY[k] = yLeft[OptVer] - ProtY[k] * Radius[OptVer];
}

int halfWidth = 20, m = 0;
for (m = 0; m < 5; m++)
{
g.DrawLine(whitePen, (int)((PointX[Block[m, 0]] - halfWidth) * Scale1) + maX, (int)(PointY[Block[m, 0]] * Scale1) + maY,
(int)((PointX[Block[m, 0]] + halfWidth) * Scale1) + maX, (int)(PointY[Block[m, 0]] * Scale1) + maY);

g.DrawLine(whitePen, (int)((PointX[Block[m, 0]] + halfWidth) * Scale1) + maX, (int)(PointY[Block[m, 0]] * Scale1) + maY,
(int)((PointX[Block[m, 1]] + halfWidth) * Scale1) + maX, (int)(PointY[Block[m, 1]] * Scale1) + maY);

g.DrawLine(whitePen, (int)((PointX[Block[m, 1]] + halfWidth) * Scale1) + maX, (int)(PointY[Block[m, 1]] * Scale1) + maY,
(int)((PointX[Block[m, 1]] - halfWidth) * Scale1) + maX, (int)(PointY[Block[m, 1]] * Scale1) + maY);

g.DrawLine(whitePen, (int)((PointX[Block[m, 1]] - halfWidth) * Scale1) + maX, (int)(PointY[Block[m, 1]] * Scale1) + maY,
(int)((PointX[Block[m, 0]] - halfWidth) * Scale1) + maX, (int)(PointY[Block[m, 0]] * Scale1) + maY);
}

for (m = 5; m <= 6; m++)
{
g.DrawLine(whitePen,
(int)(PointX[Block[m, 0]] * Scale1) + maX, (int)((PointY[Block[m, 0]] - halfWidth) * Scale1) + maY,
(int)(PointX[Block[m, 0]] * Scale1) + maX, (int)((PointY[Block[m, 0]] + halfWidth) * Scale1) + maY);

g.DrawLine(whitePen,
(int)(PointX[Block[m, 0]] * Scale1) + maX, (int)((PointY[Block[m, 0]] + halfWidth) * Scale1) + maY,
(int)(PointX[Block[m, 1]] * Scale1) + maX, (int)((PointY[Block[m, 1]] + halfWidth) * Scale1) + maY);

g.DrawLine(whitePen,
(int)(PointX[Block[m, 1]] * Scale1) + maX, (int)((PointY[Block[m, 1]] + halfWidth) * Scale1) + maY,
(int)(PointX[Block[m, 1]] * Scale1) + maX, (int)((PointY[Block[m, 1]] - halfWidth) * Scale1) + maY);

g.DrawLine(whitePen,
(int)(PointX[Block[m, 1]] * Scale1) + maX, (int)((PointY[Block[m, 1]] - halfWidth) * Scale1) + maY,
(int)(PointX[Block[m, 0]] * Scale1) + maX, (int)((PointY[Block[m, 0]] - halfWidth) * Scale1) + maY);
} //============================ end for (m = 5; ... ====================================================

for (m = 0; m < 7; m++) //================================================================
{
g.DrawLine(redPen,
(int)(PointX[Block[m, 0]] * Scale1) + maX, (int)(PointY[Block[m, 0]] * Scale1) + maY,
(int)(PointX[Block[m, 0]] * Scale1) + maX, (int)(PointY[Block[m, 0]] * Scale1) + maY);

g.DrawLine(redPen,
(int)(PointX[Block[m, 0]] * Scale1) + maX, (int)((PointY[Block[m, 0]]) * Scale1) + maY,
(int)(PointX[Block[m, 1]] * Scale1) + maX, (int)(PointY[Block[m, 1]] * Scale1) + maY);

g.DrawLine(redPen,
(int)(PointX[Block[m, 1]] * Scale1) + maX, (int)(PointY[Block[m, 1]] * Scale1) + maY,
(int)(PointX[Block[m, 1]] * Scale1) + maX, (int)(PointY[Block[m, 1]] * Scale1) + maY);

g.DrawLine(redPen,
(int)(PointX[Block[m, 1]] * Scale1) + maX, (int)(PointY[Block[m, 1]] * Scale1) + maY,
(int)(PointX[Block[m, 0]] * Scale1) + maX, (int)(PointY[Block[m, 0]] * Scale1) + maY);
} //================================== end for (m = 0; m < 7; m++) ==================================
} //------------------------------------ end if (OptDir ... ----------------------------------------------------

} //************************************** end RecoBicycleMode ******************************************************


public double MinAreaN(iVect2[] P, int np, ref double radius, ref double x0, ref double y0)
/* From 'WFRecoCircles". Calculates the 13 sums and the estimates "x0" and "y0" of the coordinates 
of the center and the estimate
"radius" of the radius of the optimal circle with the minimum deviation from the given
set "P[np]" of points. 
The found values and the 13 sums used for the calculation are assigned to the arc 
with the index "ia". ------------ 
{
double SumX, SumY, SumX2, SumY2, SumXY, SumX3, SumY3, SumX2Y, SumXY2,
  SumX4, SumX2Y2, SumY4;
double a1, a2, b1, b2, c1, c2, Crit, det, fx, fy, mx, my, N, R2;
int ip;
N = (double)(np);
SumX = SumY = SumX2 = SumY2 = SumXY = SumX3 = SumY3 = SumX2Y = SumXY2 =
SumX4 = SumX2Y2 = SumY4 = 0.0;

for (ip = 0; ip < np; ip++) //======= over the set of points ==============
{
fx = (double)P[ip].X; fy = (double)P[ip].Y;
SumX += fx; SumY += fy; SumX2 += fx * fx; SumY2 += fy * fy; SumXY += fx * fy;
SumX3 += fx * fx * fx; SumY3 += fy * fy * fy; SumX2Y += fx * fx * fy; SumXY2 += fx * fy * fy;
SumX4 += fx * fx * fx * fx; SumX2Y2 += fx * fx * fy * fy; SumY4 += fy * fy * fy * fy;
} //=================== end for (ip...) ================================

a1 = 2 * (SumX * SumX - N * SumX2); b1 = 2 * (SumX * SumY - N * SumXY);
a2 = 2 * (SumX * SumY - N * SumXY); b2 = 2 * (SumY * SumY - N * SumY2);
c1 = SumX2 * SumX - N * SumX3 + SumX * SumY2 - N * SumXY2;
c2 = SumX2 * SumY - N * SumY3 + SumY * SumY2 - N * SumX2Y;
det = a1 * b2 - a2 * b1;
if (Math.Abs(det) < 0.00001) return -2.0;

mx = (c1 * b2 - c2 * b1) / det;
my = (a1 * c2 - a2 * c1) / det;
R2 = (SumX2 - 2 * SumX * mx - 2 * SumY * my + SumY2) / N + mx * mx + my * my;
if (R2 <= 0.0) return -1.0;
x0 = mx; y0 = my; radius = Math.Sqrt(R2);
Crit = 0.0;
for (ip = 0; ip < np; ip++) //======= die Punktfolge ====================
{
fx = (double)P[ip].X; fy = (double)P[ip].Y;
Crit += (radius - Math.Sqrt((fx - mx) * (fx - mx) + (fy - my) * (fy - my))) *
  (radius - Math.Sqrt((fx - mx) * (fx - mx) + (fy - my) * (fy - my)));
} //=================== end for (ip...) ==============================

return Math.Sqrt(Crit / (double)np);
} //********************** end MinAreaN *********************************


public double TestWheel2(int xCenter, int yCenter, int Radius, double Reduct,
                  ref int xCent, ref int yCent, ref int Rad, ref double Red, Form1 fm1)
{
Graphics g = fm1.pictureBox2.CreateGraphics();
Pen yellowPen = new Pen(Color.Yellow, 2);

int maX = fm1.marginX;
int maY = fm1.marginY;
double Scale1 = fm1.Scale1;
bool deb = false;
int x, x1, y, y1;
double radVectX, radVectY, scaPro;
//double xr;
double delX, delY, Dist, length, redLen, SumLength = 0.0, maxCos = 6.0 / (double)Radius;
int minX = 100000, maxX = 0, minY = 100000, maxY = 0, number = 0, SumX = 0, SumY = 0;
int XminY = 0, XmaxY = 0;
bool found = false;
iVect2[] P = new iVect2[1000];
int iPk = 0;
for (int ip = 0; ip < nPolygon; ip++) //==========================================
{
int first = Polygon[ip].firstVert;
int last = Polygon[ip].lastVert;
for (int iv = first; iv < last; iv++)
{
x = Vert[iv].X; y = Vert[iv].Y;
x1 = Vert[iv + 1].X; y1 = Vert[iv + 1].Y;
radVectX = (x - xCenter) / Reduct;
radVectY = (double)(y - yCenter);
delX = (x1 - x) / Reduct;
delY = (double)(y1 - y);
length = Math.Sqrt((x1 - x) * (x1 - x) + (y1 - y) * (y1 - y));
redLen = Math.Sqrt(delX * delX + delY * delY);
Dist = Math.Sqrt(radVectX * radVectX + radVectY * radVectY);
scaPro = Math.Abs((x1 - x) * radVectX + (y1 - y) * radVectY) / redLen / Dist;
if (Math.Abs(Radius - Dist) < 20.0 && deb)
MessageBox.Show("TestW2: x=" + x + " y=" + y + " length=" + (int)length + " radVectX=" + radVectX +
  " Center=(" + xCenter + "," + yCenter + ") Radius - Dist=" + (int)(Radius - Dist) + " scapro=" + scaPro +
  " maxCos=" + maxCos);
// scaPro < redLen*Dist*6.0 / Radius

if (Math.Abs((double)Radius - Dist) < 20.0 && scaPro < maxCos && length > 5.0)
{
found = true;
P[iPk].X = x;
P[iPk].Y = y;
iPk++;
SumLength += length;
if (x < minX) minX = x;
if (x > maxX) maxX = x;
if (y < minY)
{
  minY = y;
  XminY = x;
}
if (y > maxY)
{
  maxY = y;
  XmaxY = x;
}
SumX += x;
SumY += y;
number++;
g.DrawLine(yellowPen, maX + (int)(Scale1 * x), maY + (int)(Scale1 * y),
                      maX + (int)(Scale1 * x1), maY + (int)(Scale1 * y1));
if (deb)
  MessageBox.Show("TestWN draws x=" + x + " y=" + y + " Radius=" + Radius + " Dist=" + (int)Dist +
    " Center=(" + xCenter + "," + yCenter + ")");
}
}
} //=============================== end for (int ip ==================================
if (Math.Abs(xCenter - 239) < 20 && false)
MessageBox.Show("TestWheel2: minX=" + minX + " maxX=" + maxX + " xCenter=" + xCenter);
double Crit = 0.0, radius = 0.0, x0 = 0.0, y0 = 0.0;
Crit = MinAreaN(P, iPk, ref radius, ref x0, ref y0);
if (xCenter > 650 && xCenter < 800 && yCenter > 700 && yCenter < 800 && deb)
MessageBox.Show("TestWheel2: Crit=" + Crit + " iPk=" + iPk);
if (number > 0 && found)
{
//xCent = (maxX + minX) / 2; // SumX / number;
//yCent = (maxY + minY) / 2; // SumY / number;
//xCent = SumX / number;
if (Crit > 0.0)
{
yCent = (int)y0;
xCent = (int)x0; // (XminY + XmaxY) / 2;
Rad = (int)Radius;
}
else
{
yCent = (maxY + minY) / 2; ;
xCent = (XminY + XmaxY) / 2;
Rad = (maxY - minY) / 2; 
}

}
else xCent = yCent = 0;
if (found)
{
//Rad = (maxY - minY) / 2;
Red = 1.0; // (double)(maxX - minX) / (double)(2 * Rad);
}
//if (!found) MessageBox.Show("TestWheel2: found=" + found + " RadNew=" + Rad + " maxY=" + maxY + " minY=" + minY);
return SumLength;
} //******************************** end TestWheel2 *****************************************



public void RecoBicycle2(CImage Img, int Size, Form1 fm1)
{
Graphics g = fm1.pictureBox2.CreateGraphics();

int nx, ny; //, size = 0;
bool deb = false, drawWhite = false;
double dSize = (double)Size;
nx = 1 + Img.width / Size; // it was 2 + Img.Width / Size
ny = 1 + Img.height / Size;
int[] CntA = new int[nx * ny];
int[] CntB = new int[nx * ny];
int[] CntC = new int[nx * ny];
int[] CntD = new int[nx * ny];

int i = 0;
int size2D = 10000;
int[,] IA = new int[nx * ny, size2D];
int[,] IB = new int[nx * ny, size2D];
int[,] IC = new int[nx * ny, size2D];
int[,] ID = new int[nx * ny, size2D];

int[,] XA = new int[nx * ny, size2D];
int[,] XB = new int[nx * ny, size2D];
int[,] XC = new int[nx * ny, size2D];
int[,] XD = new int[nx * ny, size2D];

int[,] YA = new int[nx * ny, size2D];
int[,] YB = new int[nx * ny, size2D];
int[,] YC = new int[nx * ny, size2D];
int[,] YD = new int[nx * ny, size2D];

int[] Radius = new int[size2D];
int[] xLeft = new int[size2D];
int[] yLeft = new int[size2D];
int[] xRight = new int[size2D];
int[] yRight = new int[size2D];
int[] radM = new int[size2D];
double[] TestLeft = new double[size2D];
double[] TestRight = new double[size2D];
double[] Reduction = new double[size2D];

int[] RadNewLeft = new int[size2D];
int[] RadNewRight = new int[size2D];
int[] xLeftNew = new int[size2D];
int[] yLeftNew = new int[size2D];
int[] xRightNew = new int[size2D];
int[] yRightNew = new int[size2D];
double[] ReductNew = new double[size2D];

double[] RedPointX = new double[size2D];

Pen whitePen = new Pen(Color.White, 2);
Pen redPen = new Pen(Color.Red, 2);

int maX = fm1.marginX;
int maY = fm1.marginY;
double Scale1 = fm1.Scale1;
int x, y;
for (int k = 0; k < nx * ny; k++) CntA[k] = CntB[k] = CntC[k] = CntD[k] = 0;

double[] ProtX0 = { 0.0, 0.22, 1.10, 1.75, 1.04, 1.17, 2.94, 3.04, 3.16, 3.71, 4.05 };
double[] ProtY0 = { 0.0, 0.13, 0.17, 0.10, 1.36, 1.06, 1.60, 1.48, 1.32, 0.18, 0.0 };


double[] ProtX1 = { 0.0, 0.21, 1.33, 1.46, 1.08, 1.13, 3.17, 3.25, 3.33, 3.71, 4.0 };
double[] ProtY1 = { 0.0, 0.20, 0.20, -0.29, 1.79, 1.08, 2.25, 2.0, 1.54, 0.42, 0.0 };

int[,] BlockM = { { 3, 4 }, { 6, 9 }, { 0, 5 }, { 5, 7 }, { 3, 8 }, { 1, 2 }, { 0, 3 } };
int[,] BlockF = { { 3, 4 }, { 6, 9 }, { 0, 5 }, { 2, 7 }, { 3, 8 }, { 1, 2 }, { 0, 3 } };
double[] A = new double[8];
double[] B = new double[8];

double[] PointX = new double[11];
double[] PointY = new double[11];

int OptRad = -1, OptVer = -1; //, OptDir = -1;
//double OptRed = -1.0;
//double SumLength = 0.0, maxSum = 0.0;
int minRad = 0, maxRad = 0, Vers = 0;
for (int rad = 0; rad < 6; rad++) //============================================================
{
if (deb) MessageBox.Show("For rad=" + rad + " starts. nArc=" + nArc);
switch (rad)
{
case 0: minRad = 25; maxRad = 42; break;
case 1: minRad = 35; maxRad = 60; break;
case 2: minRad = 50; maxRad = 70; break;
case 3: minRad = 58; maxRad = 110; break;
case 4: minRad = 80; maxRad = 120; break;
case 5: minRad = 100; maxRad = 180; break;
}


// Find the arcs C5 to C8 of type "a" and "b":
for (int ia = 1; ia < nArc; ia++) //=======================================================================
{
if (deb) MessageBox.Show("ia=" + ia + " rad=" + Math.Abs(Arc[ia].rad));
if (Math.Abs(Arc[ia].rad) < minRad || Math.Abs(Arc[ia].rad) > maxRad || Arc[ia].ym < (float)(Img.height * 2 / 5))
continue;
// Condition for Ca and Cb:
if (Math.Abs(Arc[ia].yb - Arc[ia].ye) < 30.0F && Math.Abs(Arc[ia].xm - (Arc[ia].xb + Arc[ia].xe) * 0.5F) < 30.0F)
{
x = (int)Arc[ia].xm / Size;
y = (int)Math.Min(Arc[ia].yb, Arc[ia].ye) / Size;
if (deb)
  MessageBox.Show("RBM2_A: x=" + x + " y=" + y + " m=(" + (int)Arc[ia].xm + "," + (int)Arc[ia].ym + ") be=(" +
    (int)(Arc[ia].xb + Arc[ia].xe) / 2 + "," + (int)Math.Min(Arc[ia].yb, Arc[ia].ye) + ") A.rad=" + (int)Arc[ia].rad +
    " minRad=" + minRad + " maxRad=" + maxRad);
if (Arc[ia].ym > Math.Min(Arc[ia].yb, Arc[ia].ye) + 10 && // Cond A: TiefZ = ybe + minRad HochZ = ybe + maxRad
    Arc[ia].ym < Math.Min(Arc[ia].yb, Arc[ia].ye) + maxRad && x < nx)
{
  IA[x + nx * y, CntA[x + nx * y]] = ia;
  XA[x + nx * y, CntA[x + nx * y]] = (int)Arc[ia].xm;
  YA[x + nx * y, CntA[x + nx * y]] = (int)Math.Min(Arc[ia].yb, Arc[ia].ye);
  CntA[x + nx * y]++;
  if (deb)
    MessageBox.Show("RBM2_A: x=" + x + " m=(" + (int)Arc[ia].xm + "," + (int)Arc[ia].ym + ") be=(" +
      (int)(Arc[ia].xb + Arc[ia].xe) / 2 + "," + (int)Math.Min(Arc[ia].yb, Arc[ia].ye) + ") rad=" + (int)Arc[ia].rad);
}

x = (int)Arc[ia].xm / Size;
y = (int)Math.Max(Arc[ia].yb, Arc[ia].ye) / Size;

if (Arc[ia].ym > Math.Max(Arc[ia].yb, Arc[ia].ye) - maxRad &&   // Cond B: TiefZ = ybe - max; HochZ = ybe - min
    Arc[ia].ym < Math.Max(Arc[ia].yb, Arc[ia].ye) - minRad && x < nx)
{
  IB[x + nx * y, CntB[x + nx * y]] = ia;
  XB[x + nx * y, CntB[x + nx * y]] = (int)Arc[ia].xm;
  YB[x + nx * y, CntB[x + nx * y]] = (int)Math.Max(Arc[ia].yb, Arc[ia].ye);
  CntB[x + nx * y]++;
  if (deb)
    MessageBox.Show("RBM2_B: x=" + x + " m=(" + (int)Arc[ia].xm + "," + (int)Arc[ia].ym + ") be=(" +
      (int)(Arc[ia].xb + Arc[ia].xe) / 2 + "," + (int)Math.Max(Arc[ia].yb, Arc[ia].ye) + ") rad=" + (int)Arc[ia].rad);
}
} //---------------------------------- end if ( 'Condition for CA and CB:' -------------------------------------

// Condition for Cc and Cd:
if (Math.Abs(Arc[ia].xb - Arc[ia].xe) < 30 && Math.Abs(Arc[ia].ym - (Arc[ia].yb + Arc[ia].ye) * 0.5F) < 30.0)
{
x = (int)Math.Min(Arc[ia].xb, Arc[ia].xe) / Size;
y = (int)Arc[ia].ym / Size;
if (deb) MessageBox.Show("CondC: TZ=" + (int)(Math.Min(Arc[ia].xb, Arc[ia].xe) + minRad) +
  " < xm=" + (int)Arc[ia].xm + " < HZ=" + (int)(Math.Min(Arc[ia].xb, Arc[ia].xe) + maxRad));

if (Arc[ia].xm > Math.Min(Arc[ia].xb, Arc[ia].xe) + minRad && // Cond C: TiefZ = xbe + minRad HochZ = xbe + maxRad
    Arc[ia].xm < Math.Min(Arc[ia].xb, Arc[ia].xe) + maxRad && y < ny)
{
  IC[x + nx * y, CntC[x + nx * y]] = ia;
  YC[x + nx * y, CntC[x + nx * y]] = (int)Arc[ia].ym;
  XC[x + nx * y, CntC[x + nx * y]] = (int)Math.Min(Arc[ia].xb, Arc[ia].xe);
  CntC[x + nx * y]++;
  if (deb)
    MessageBox.Show("RBM2_C: ia=" + ia + " x=" + x + " m=(" + (int)Arc[ia].xm + "," + (int)Arc[ia].ym +
      ") be=(" + (int)Math.Min(Arc[ia].xb, Arc[ia].xe) + "," + (int)((Arc[ia].yb + Arc[ia].ye) * 0.5F) +
      ") XC=" + XC[x, CntC[x]] + " YC=" + YC[x, CntC[x]] + " CntC[x] after =" + CntC[x]);
}

if (deb) MessageBox.Show("CondD: TZ=" + (int)(Math.Max(Arc[ia].xb, Arc[ia].xe) - maxRad) +
          " < xm=" + (int)Arc[ia].xm + " < HZ=" + (int)(Math.Min(Arc[ia].xb, Arc[ia].xe) - minRad));

x = (int)Math.Max(Arc[ia].xb, Arc[ia].xe) / Size;
y = (int)Arc[ia].ym / Size;
if (Arc[ia].xm > Math.Max(Arc[ia].xb, Arc[ia].xe) - maxRad && // Cond C: TiefZ = xbe - minRad HochZ = xbe + maxRad
    Arc[ia].xm < Math.Max(Arc[ia].xb, Arc[ia].xe) - minRad && y < ny)
{
  ID[x + nx * y, CntD[x + nx * y]] = ia;
  YD[x + nx * y, CntD[x + nx * y]] = (int)Arc[ia].ym;
  XD[x + nx * y, CntD[x + nx * y]] = (int)Math.Max(Arc[ia].xb, Arc[ia].xe);
  CntD[x + nx * y]++;
  if (deb)
    MessageBox.Show("RBM2_D: ia=" + ia + " x=" + x + " ym=" + Arc[ia].ym + " xbe=" + (int)Math.Max(Arc[ia].xb, Arc[ia].xe));
}

} //------------------------------------ end if ( 'Condition for Cc and Cd:' -----------------------
} //====================================== end for (int ia ... ==========================================================

// Averaging the coordinates in blocks:
for (y = 0; y < ny; y++) //===============================================================
for (x = 0; x < nx; x++) //===============================================================
{
//if (CntA[x + nx * y] > 0 || CntB[x + nx * y] > 0 || CntC[x + nx * y] > 0 || CntD[x + nx * y] > 0) 
//MessageBox.Show("RBM0: rad=" + rad + " x=" + x + " y=" + y + " CntA=" + CntA[x + nx * y] +
//" CntB=" + CntB[x + nx * y] + " CntC=" + CntC[x + nx * y] + " CntD=" + CntD[x + nx * y]);
int SumX = 0, SumY = 0;
if (CntA[x + nx * y] > 1)
{
  for (i = 0; i < CntA[x + nx * y]; i++) //====================
  {
    SumX += XA[x + nx * y, i];
    SumY += YA[x + nx * y, i];
  }
  XA[x + nx * y, 0] = SumX / CntA[x + nx * y];
  YA[x + nx * y, 0] = SumY / CntA[x + nx * y];
  CntA[x + nx * y] = 1;
}

SumX = 0; SumY = 0;
if (CntB[x + nx * y] > 1)
{
  for (i = 0; i < CntB[x + nx * y]; i++) //======================
  {
    SumX += XB[x + nx * y, i];
    SumY += YB[x + nx * y, i];
  }
  XB[x + nx * y, 0] = SumX / CntB[x + nx * y];
  YB[x + nx * y, 0] = SumY / CntB[x + nx * y];
  CntB[x + nx * y] = 1;
}

SumX = 0; SumY = 0;
if (CntC[x + nx * y] > 1)
{
  for (i = 0; i < CntC[x + nx * y]; i++) //======================
  {
    SumX += XC[x + nx * y, i];
    SumY += YC[x + nx * y, i];
  }
  XC[x + nx * y, 0] = SumX / CntC[x + nx * y];
  YC[x + nx * y, 0] = SumY / CntC[x + nx * y];
  CntC[x + nx * y] = 1;
}


SumX = 0; SumY = 0;
if (CntD[x + nx * y] > 1)
{
  for (i = 0; i < CntD[x + nx * y]; i++) //======================
  {
    SumX += XD[x + nx * y, i];
    SumY += YD[x + nx * y, i];
  }
  XD[x + nx * y, 0] = SumX / CntD[x + nx * y];
  YD[x + nx * y, 0] = SumY / CntD[x + nx * y];
  CntD[x + nx * y] = 1;
}

} //============================= end for (x ... ======================================================

if (deb)
{
for (y = 0; y < ny; y++)
for (x = 0; x < nx; x++)
{
  if (CntA[x + nx * y] > 0) MessageBox.Show("RBM0: x=" + x + " y=" + y + " CntA=" + CntA[x + nx * y]);
  if (CntB[x + nx * y] > 0) MessageBox.Show("RBM0: x=" + x + " y=" + y + " CntB=" + CntB[x + nx * y]);
  if (CntC[x + nx * y] > 0) MessageBox.Show("RBM0: x=" + x + " y=" + y + " CntC=" + CntC[x + nx * y]);
  if (CntD[x + nx * y] > 0) MessageBox.Show("RBM0: x=" + x + " y=" + y + " CntD=" + CntD[x + nx * y]);
}
}
//Graphics g = fm1.pictureBox2.CreateGraphics();

// Drawing the found arcs:
for (y = 0; y < ny; y++) //=================================================================
for (x = 0; x < nx; x++) //=================================================================
{
if (deb)
  MessageBox.Show("RBM1: x=" + x + " y=" + y + " CntA[x + nx*y]=" + CntA[x + nx * y] + " CntB[x + nx*y]=" +
    CntB[x + nx * y] + " CntC[x + nx*y]=" + CntC[x + nx * y] + " CntD[x + nx*y]=" + CntD[x + nx * y]);
if (CntA[x + nx * y] > 0)
{
  /*
  g.DrawLine(whitePen, (int)((XA[x + nx * y, 0] - 10) * Scale1) + maX, (int)(YA[x + nx * y, 0] * Scale1) + maY,
                    (int)((XA[x + nx * y, 0] + 10) * Scale1) + maX, (int)(YA[x + nx * y, 0] * Scale1) + maY);
  g.DrawLine(whitePen, (int)((XA[x + nx * y, 0]) * Scale1) + maX, (int)(YA[x + nx * y, 0] * Scale1) + maY,
                    (int)((XA[x + nx * y, 0]) * Scale1) + maX, (int)((YA[x + nx * y, 0] + 30) * Scale1) + maY);
               
  if (XA[x + nx * y, 0] > Img.width / 2 && deb) MessageBox.Show("RBM1a: IA[x + nx*y, 0]=" + IA[x + nx * y, 0] + " ym=" + Arc[IA[x + nx * y, 0]].ym);
}

if (CntB[x + nx * y] > 0 && false)
{
  g.DrawLine(whitePen, (int)((XB[x + nx * y, 0] - 10) * Scale1) + maX, (int)(YB[x + nx * y, 0] * Scale1) + maY,
                    (int)((XB[x + nx * y, 0] + 10) * Scale1) + maX, (int)(YB[x + nx * y, 0] * Scale1) + maY);
  g.DrawLine(whitePen, (int)((XB[x + nx * y, 0]) * Scale1) + maX, (int)(YB[x + nx * y, 0] * Scale1) + maY,
                    (int)((XB[x + nx * y, 0]) * Scale1) + maX, (int)((YB[x + nx * y, 0] - 30) * Scale1) + maY);
}


if (deb) MessageBox.Show("RBMx: x=" + x + " CntC[x]=" + CntC[x + nx * y]);
if (CntC[x + nx * y] > 0 && false)
{
  g.DrawLine(whitePen, (int)((XC[x + nx * y, 0]) * Scale1) + maX, (int)((YC[x + nx * y, 0] - 10) * Scale1) + maY,
                    (int)((XC[x + nx * y, 0]) * Scale1) + maX, (int)((YC[x + nx * y, 0] + 10) * Scale1) + maY);
  g.DrawLine(whitePen, (int)((XC[x + nx * y, 0]) * Scale1) + maX, (int)(YC[x + nx * y, 0] * Scale1) + maY,
                    (int)((XC[x + nx * y, 0] + 30) * Scale1) + maX, (int)((YC[x + nx * y, 0]) * Scale1) + maY);
}

if (CntD[x + nx * y] > 0 && false)
{
  g.DrawLine(whitePen, (int)((XD[x + nx * y, 0]) * Scale1) + maX, (int)((YD[x + nx * y, 0] - 10) * Scale1) + maY,
                    (int)((XD[x + nx * y, 0]) * Scale1) + maX, (int)((YD[x + nx * y, 0] + 10) * Scale1) + maY);
  g.DrawLine(whitePen, (int)((XD[x + nx * y, 0]) * Scale1) + maX, (int)(YD[x + nx * y, 0] * Scale1) + maY,
                    (int)((XD[x + nx * y, 0] - 30) * Scale1) + maX, (int)((YD[x + nx * y, 0]) * Scale1) + maY);
}
} //================================ end for (x ... =================================================== 

if (deb)
{
for (i = 0; i < CntA[5]; i++)
MessageBox.Show("x=" + 5 + " i=" + i + " The A-arc " + IA[5, i] + " has XA=" + XA[5, i] + " YA=" + YA[5, i]);

for (int j = 0; j < CntB[5]; j++)
MessageBox.Show("x=" + 5 + " j=" + j + " The B-arc " + IB[5, j] + " has XB=" + XB[5, j] + " YB=" + YB[5, j]);
}

// Constructing lists of types:
int[,] ListA = new int[size2D, 2];
int[,] ListB = new int[size2D, 2];
int[,] ListC = new int[size2D, 2];
int[,] ListD = new int[size2D, 2];
int cntPairsA = 0, cntPairsB = 0, cntPairsC = 0, cntPairsD = 0;

for (y = 0; y < ny; y++)
for (x = 0; x < nx; x++)
{
if (CntA[x + nx * y] > 0)
{
  ListA[cntPairsA, 0] = x;
  ListA[cntPairsA, 1] = y;
  cntPairsA++;
}
if (CntB[x + nx * y] > 0)
{
  ListB[cntPairsB, 0] = x;
  ListB[cntPairsB, 1] = y;
  cntPairsB++;
}
if (CntC[x + nx * y] > 0)
{
  ListC[cntPairsC, 0] = x;
  ListC[cntPairsC, 1] = y;
  cntPairsC++;
}
if (CntD[x + nx * y] > 0)
{
  ListD[cntPairsD, 0] = x;
  ListD[cntPairsD, 1] = y;
  cntPairsD++;
}
} //============================= end for (x ... ==============================

// Constructing the versions:
int minDifY56 = 2 * minRad, maxDifY56 = 2 * maxRad;
int minXax2 = (int)(4.7 * minRad);
if (deb)
MessageBox.Show("RBM: Constructing versions, rad=" + rad + " OptVer=" + OptVer + " cntPairsA=" + cntPairsA + " cntPairsB=" + cntPairsB);
//if (OptVer < 0)
//for (i = 0; i < cntPairsA; i++) //=======================================================
//for (int j = 0; j < cntPairsB; j++)
for (i = cntPairsA - 1; i >= 0 ; i--) //=======================================================
for (int j = cntPairsB - 1; j >= 0 ; j--)
{
int xA = ListA[i, 0];
int yA = ListA[i, 1];
int xB = ListB[j, 0];
int yB = ListB[j, 1];
int Tol = 0;
if (deb) //(xA == 5 || xA == 13) && yA == 8 && deb)
  MessageBox.Show("RBML: rad=" + rad + " i=" + i + " j=" + j + " xA=" + xA + " yA=" + yA + " xB=" + xB + " yB=" + yB +
    " 3.7*minRad/Size=" + (int)(3.7 * minRad / Size) + " 3.7*maxRad/Size=" + (int)(3.7 * maxRad / Size) +
   " 2*minRad /Size=" + (int)(3.7 * minRad / Size) + " 2*maxRad /Size=" + (int)(3.7 * maxRad / Size));

if (xB > xA + 3.7 * minRad / Size && xB < xA + 3.7 * maxRad / Size && yB > yA + (2 * minRad + Tol) / Size &&
  yB < yA + (2 * maxRad - Tol) / Size)
{
  Radius[Vers] = (YB[xB + nx * yB, 0] - YA[xA + nx * yA, 0]) / 2;
  xLeft[Vers] = XA[xA + nx * yA, 0];
  yLeft[Vers] = (YA[xA + nx * yA, 0] + YB[xB + nx * yB, 0]) / 2;
  xRight[Vers] = XB[xB + nx * yB, 0];
  yRight[Vers] = (YA[xA + nx * yA, 0] + YB[xB + nx * yB, 0]) / 2;
  Reduction[Vers] = ((double)xRight[Vers] - (double)xLeft[Vers]) / (3.8 * Radius[Vers]);
  TestLeft[Vers] = TestWheel2(xLeft[Vers], yLeft[Vers], Radius[Vers], Reduction[Vers],
    ref xLeftNew[Vers], ref yLeftNew[Vers], ref RadNewLeft[Vers], ref ReductNew[Vers], fm1);
  TestRight[Vers] = TestWheel2(xRight[Vers], yRight[Vers], Radius[Vers], Reduction[Vers],
    ref xRightNew[Vers], ref yRightNew[Vers], ref RadNewRight[Vers], ref ReductNew[Vers], fm1);
  if (deb)
    MessageBox.Show("RBMa 5,8: rad=" + rad + " i=" + i + " j=" + j + " Radius=" + Radius[Vers] + " RadNewLeft=" + 
      RadNewLeft[Vers] + " RadNewRight=" + RadNewRight + 
      " xLeft=" + xLeft[Vers] + " xLeftNew=" + xLeftNew[Vers] + " xRight=" + xRight[Vers] +
       " xRightNew=" + xRightNew[Vers] + " yLeft=" + yLeft[Vers] + " yLeftNew=" + yLeftNew[Vers] +
       " yRight=" + yRight[Vers] + " yRightNew=" + yRightNew[Vers] +
      " Test left wheel=" + TestLeft[Vers] + " Test right wheel=" + TestRight[Vers] + " Vers=" + Vers + " Red=" + Reduction[Vers]);
  radM[Vers] = rad;
  Vers++;
}

if (xB < xA - 3.7 * minRad / Size && xB >= xA - 3.7 * maxRad / Size &&
                    yB > yA + (2 * minRad + Tol) / Size && yB < yA + (2 * maxRad - Tol) / Size)
{
  Radius[Vers] = (YB[xB + nx * yB, 0] - YA[xA + nx * yA, 0]) / 2;
  xLeft[Vers] = XB[xB + nx * yB, 0];
  yLeft[Vers] = (YA[xA + nx * yA, 0] + YB[xB + nx * yB, 0]) / 2;
  xRight[Vers] = XA[xA + nx * yA, 0];
  yRight[Vers] = (YA[xA + nx * yA, 0] + YB[xB + nx * yB, 0]) / 2;
  Reduction[Vers] = ((double)xRight[Vers] - (double)xLeft[Vers]) / (3.8 * Radius[Vers]);
  TestLeft[Vers] = TestWheel2(xLeft[Vers], yLeft[Vers], Radius[Vers], Reduction[Vers],
    ref xLeftNew[Vers], ref yLeftNew[Vers], ref RadNewLeft[Vers], ref ReductNew[Vers], fm1);
  TestRight[Vers] = TestWheel2(xRight[Vers], yRight[Vers], Radius[Vers], Reduction[Vers],
    ref xRightNew[Vers], ref yRightNew[Vers], ref RadNewRight[Vers], ref ReductNew[Vers], fm1);
  if (deb)
    MessageBox.Show("RBMa 6,7: rad=" + rad + " i=" + i + " j=" + j + " Radius=" + Radius[Vers] + " RadNew=" +
      RadNewLeft[Vers] + " RadNewRight=" + RadNewRight +
      " xLeft=" + xLeft[Vers] + " xLeftNew=" + xLeftNew[Vers] + " xRight=" + xRight[Vers] +
       " xRightNew=" + xRightNew[Vers] + " yLeft=" + yLeft[Vers] + " yLeftNew=" + yLeftNew[Vers] +
       " yRight=" + yRight[Vers] + " yRightNew=" + yRightNew[Vers] +
      " Test left wheel=" + TestLeft[Vers] + " Test right wheel=" + TestRight[Vers] + " Vers=" + Vers + " Red=" + Reduction[Vers]);
  radM[Vers] = rad;
  Vers++;
}
} //========================== end for (j ... ==========================================
if (deb)
{
if (Vers > 0)
MessageBox.Show("***** RBMv: rad=" + rad + " Vers-1=" + (Vers - 1) + " Radius=" + Radius[Vers - 1] + " OptVer=" +
  OptVer + " Reduction=" + Reduction[Vers - 1] + " ReductNew=" + ReductNew[Vers - 1]);
else MessageBox.Show("After constructing versions: rad=" + rad + " Vers=" + Vers);
}
} //============================== end for (int rad ... ========================================================
/*
for (int vv = 0; vv < 288; vv++)
{ 
double Dif = (xRight[vv] - xLeft[vv]) / (double)Radius[vv] - 3.8;
if (Dif > -1.0 && Dif < 1.0)          MessageBox.Show("**** vv=" + vv + " xRight=" + xRight[vv] + " xLeft=" + xLeft[vv] +
  " Radius=" + Radius[vv] + " Dif=" + Dif);
} 

// Testing the versions:
int OptRadius = -1;
int OptRadiusLeft = -1;
int OptRadiusRight = -1;
int OptXLeft = -1;
int OptXLeftNew =  -1;
int OptYLeft =  -1;
int OptYLeftNew = -1;
int OptXRight =  -1;
int OptXRightNew =  -1;
int OptYRight =  -1;
int OptYRightNew = -1;
double maxTestLeft = 0.0, maxTestRight = 0.0;
for (int ver = 0; ver < Vers; ver++) //====================================================================
{
if (deb)
MessageBox.Show(" for (int ver starts; ver=" + ver + " Vers=" + Vers + " TestLeft=" + TestLeft[ver] +
" TestRight=" + TestRight[ver]);
if (TestLeft[ver] > maxTestLeft)
{
maxTestLeft = TestLeft[ver];
OptVer = ver;
OptRad = radM[ver];
OptRadius = Radius[ver];
OptRadiusLeft = RadNewLeft[ver];

OptXLeft = xLeft[ver];
OptXLeftNew = xLeftNew[ver];
OptYLeft = yLeft[ver];
OptYLeftNew = yLeftNew[ver];

OptXRight = xRight[ver];
OptXRightNew = xRightNew[ver];
OptYRight = yRight[ver];
OptYRightNew = yRightNew[ver];
}

if (TestRight[ver] > maxTestRight)
{
maxTestRight = TestRight[ver];
OptVer = ver;
OptRad = radM[ver];
OptRadius = Radius[ver];
OptRadiusRight = RadNewRight[ver];

OptXLeft = xLeft[ver];
OptXLeftNew = xLeftNew[ver];
OptYLeft = yLeft[ver];
OptYLeftNew = yLeftNew[ver];

OptXRight = xRight[ver];
OptXRightNew = xRightNew[ver];
OptYRight = yRight[ver];
OptYRightNew = yRightNew[ver];
}
} //============================ end for (int ver ... ======================================
if (deb)
MessageBox.Show("After for (ver: OptVer=" + OptVer + " optRad=" + OptRad + " OptRadius=" + OptRadius + " OptRadiusLeft=" +
OptRadiusLeft + " OptRadiusRight=" + OptRadiusRight + " OptXLeft=" + OptXLeft + " OptXLeftNew=" + OptXLeftNew +
" OptXRight=" + OptXRight + " OptXRightNew=" + OptXRightNew + " OptYLeft=" + OptYLeft + " OptYLeftNew=" + OptYLeftNew);


      
int OptDir = -1, OptTyp = -1, OptGen = -1, RadiusM = 0;   
double OptRed = -1.0, maxSum = 0.0;
if (OptVer >= 0)
for (int Dir = 0; Dir < 2; Dir++) // ========== Dir == 0: to right; Dir == 1: to left ===============
{
RadiusM = (OptRadiusLeft + OptRadiusRight) /2; // (OptRadiusLeft + OptRadiusRight) / 2;
//RadiusM *= 130;
//RadiusM /= 150;

for (int Typ = 0; Typ < 2; Typ++) // ==========================================================
for (int Gen = 0; Gen < 2; Gen++) //===== Gen == 0: men's; Gen == 1: ladies' ============
{
RedPointX[OptVer] = ReductNew[OptVer]; //(OptXRightNew - OptXLeftNew) / (ProtX[10] * RadiusM);
//else RedPointX[OptVer] = (OptXRightNew - OptXLeftNew) / (ProtX2[10] * RadiusM);
if (deb)
  MessageBox.Show("RBMw 'for Dir' starts: Red=" + RedPointX[OptVer] + " xRight - xleft=" +
    (int)(OptXRightNew - OptXLeftNew) + " RadiusM=" + RadiusM + " ProtX0[10]=" + ProtX0[10] + " xLeftNew=" + OptXLeftNew);

for (int k = 0; k < 11; k++)
{
  if (Dir == 0)
    if (Typ == 0) PointX[k] = OptXLeftNew + ProtX0[k] * RadiusM * RedPointX[OptVer];
    else PointX[k] = OptXLeftNew + ProtX1[k] * RadiusM * RedPointX[OptVer];
  else
    if (Typ == 0) PointX[k] = OptXRightNew - ProtX0[k] * RadiusM * RedPointX[OptVer];
    else PointX[k] = OptXRightNew - ProtX1[k] * RadiusM * RedPointX[OptVer];

  if (Typ == 0) PointY[k] = OptYLeftNew - ProtY0[k] * RadiusM;
  else PointY[k] = OptYLeftNew - ProtY1[k] * RadiusM;

  if (k == 9 && deb)
    MessageBox.Show("RBMm: OptXLeftNew=" + OptXLeftNew + " OptYLeftNew=" + OptYLeftNew + " RadiusM=" + RadiusM +
    " PointY[k]=" + PointY[k] + " k=" + k + " ProtX0[k]=" + ProtX0[k] + " ProtY0[k]=" + ProtY0[k] + " RedPointX=" +
    RedPointX[OptVer]);
}
double Root1;
int halfWidth = Size; // / 2;
int m = 0;
for (m = 0; m < 7; m++)
{
  if (Gen == 0)
  {
    A[m] = PointY[BlockM[m, 0]] - PointY[BlockM[m, 1]];
    B[m] = PointX[BlockM[m, 1]] - PointX[BlockM[m, 0]];
  }
  else
  {
    A[m] = PointY[BlockF[m, 0]] - PointY[BlockF[m, 1]];
    B[m] = PointX[BlockF[m, 1]] - PointX[BlockF[m, 0]];
  }

  Root1 = Math.Sqrt(A[m] * A[m] + B[m] * B[m]);
  A[m] /= Root1;
  B[m] /= Root1;
} //=============================== end for (m = 0; ... ================================================

Pen yellowPen = new Pen(Color.Yellow, 2);
double SumLength = 0.0;

if (deb)
  MessageBox.Show(" Dir=" + Dir + " OptVer=" + OptVer + "Starting 'for m' drawWhite=" + drawWhite);
double Neig = 0.0;
for (m = 0; m <= 6; m++) //======================================================================================
{
  if (deb)
    MessageBox.Show("RBM_1: starting 'for (m='" + m);
  if (Gen == 0)
  {
    Neig = (PointX[BlockM[m, 1]] - PointX[BlockM[m, 0]]) / (double)(PointY[BlockM[m, 1]] - PointY[BlockM[m, 0]]);
    if (drawWhite)
      DrawPoints(g, whitePen, Scale1, maX, maY, halfWidth, BlockM, PointX, PointY);
  }
  else
  {
    Neig = (PointX[BlockF[m, 1]] - PointX[BlockF[m, 0]]) / (double)(PointY[BlockF[m, 1]] - PointY[BlockF[m, 0]]);
    if (drawWhite)
      DrawPoints(g, whitePen, Scale1, maX, maY, halfWidth, BlockF, PointX, PointY);
  }

  double Dist = 0.0, length = 0.0;
  for (int ip = 0; ip < nPolygon; ip++)
    for (int iv = Polygon[ip].firstVert; iv < Polygon[ip].lastVert; iv++)
    {
      length = Math.Sqrt((double)((Vert[iv + 1].X - Vert[iv].X) * (Vert[iv + 1].X - Vert[iv].X) +
                                (Vert[iv + 1].Y - Vert[iv].Y) * (Vert[iv + 1].Y - Vert[iv].Y)));
      if (length < 5.0) continue;
      double neig = (double)(Vert[iv + 1].X - Vert[iv].X) / (double)(Vert[iv + 1].Y - Vert[iv].Y);
      if (Gen == 0)
      {
        Dist = A[m] * (Vert[iv].X - PointX[BlockM[m, 0]]) + B[m] * (Vert[iv].Y - PointY[BlockM[m, 0]]);
        if (deb)
          MessageBox.Show("Dist=" + Dist + " halfWidth=" + halfWidth + " iv=" + iv + " Vert.Y" + Vert[iv].Y + " Max=" +
          Math.Max(PointY[BlockM[m, 0]], PointY[BlockM[m, 1]]) + " Min=" + Math.Min(PointY[BlockM[m, 0]], PointY[BlockM[m, 1]]));

        if (Math.Abs(Dist) < halfWidth &&
          (m < 5 && Vert[iv].Y < Math.Max(PointY[BlockM[m, 0]], PointY[BlockM[m, 1]]) &&
                    Vert[iv].Y > Math.Min(PointY[BlockM[m, 0]], PointY[BlockM[m, 1]]) ||
            m >= 5 && Vert[iv].X < Math.Max(PointX[BlockM[m, 0]], PointX[BlockM[m, 1]]) &&
                      Vert[iv].X > Math.Min(PointX[BlockM[m, 0]], PointX[BlockM[m, 1]])) &&
            Math.Abs(neig - Neig) < 0.12)
        {
          SumLength += length;
          g.DrawLine(redPen, maX + (int)(Vert[iv].X * Scale1), maY + (int)(Vert[iv].Y * Scale1),
            maX + (int)(Vert[iv + 1].X * Scale1), maY + (int)(Vert[iv + 1].Y * Scale1));
          if (deb) MessageBox.Show("RBMxx: Dir=" + Dir + " SumL=" + (int)SumLength);
        }
      }
      else
      {
        Dist = A[m] * (Vert[iv].X - PointX[BlockF[m, 0]]) + B[m] * (Vert[iv].Y - PointY[BlockF[m, 0]]);
        if (deb)
          MessageBox.Show("Dist=" + Dist + " halfWidth=" + halfWidth + " iv=" + iv + " Vert.Y" + Vert[iv].Y + " Max=" +
          Math.Max(PointY[BlockF[m, 0]], PointY[BlockF[m, 1]]) + " Min=" + Math.Min(PointY[BlockF[m, 0]], PointY[BlockF[m, 1]]));

        if (Math.Abs(Dist) < halfWidth &&
          (m < 5 && Vert[iv].Y < Math.Max(PointY[BlockF[m, 0]], PointY[BlockF[m, 1]]) &&
                    Vert[iv].Y > Math.Min(PointY[BlockF[m, 0]], PointY[BlockF[m, 1]]) ||
            m >= 5 && Vert[iv].X < Math.Max(PointX[BlockF[m, 0]], PointX[BlockF[m, 1]]) &&
                      Vert[iv].X > Math.Min(PointX[BlockF[m, 0]], PointX[BlockF[m, 1]])) &&
            Math.Abs(neig - Neig) < 0.12)
        {
          SumLength += length;
          g.DrawLine(redPen, maX + (int)(Vert[iv].X * Scale1), maY + (int)(Vert[iv].Y * Scale1),
            maX + (int)(Vert[iv + 1].X * Scale1), maY + (int)(Vert[iv + 1].Y * Scale1));
          if (deb) MessageBox.Show("RBMxx: Dir=" + Dir + " SumL=" + (int)SumLength);
        }
      }

    } //=========================== end for (int iv ... ==============================================  
  Pen blackPen = new Pen(Color.Black);
  if (drawWhite)
    if (Gen == 0) DrawPoints(g, blackPen, Scale1, maX, maY, halfWidth, BlockM, PointX, PointY);
    else DrawPoints(g, blackPen, Scale1, maX, maY, halfWidth, BlockF, PointX, PointY);

} //=============================== end for (m ... =====================================================
if (deb)
  MessageBox.Show("RBMq: SumL=" + SumLength + " maxSum=" + maxSum);
if (SumLength > maxSum)
{
  maxSum = SumLength;
  OptDir = Dir;
  OptTyp = Typ;
  OptGen = Gen;
  OptRed = ReductNew[OptVer];
  if (deb)
  MessageBox.Show("++++ RBMx: Dir=" + Dir + " OptVer=" + OptVer +
  " Sum=" + SumLength + " maxSum=" + maxSum);
}
} //================================ end for (int Gen ... ====================================================

} //================================= end for (int Dir ... =================================================
if (deb) MessageBox.Show("The loop 'Dir' finished;");
string[] direct = { "right", "left" };
string[] gen = { " Men's", " Ladies'" };
if (deb)
MessageBox.Show("--- RBMz after 'for rad': OptVer=" + OptVer + " OptDir=" + OptDir);
if (OptDir >= 0 && OptVer >= 0)
{
if (deb) 
MessageBox.Show("RBM20: OptVer= " + OptVer + " OptDir=" + OptDir + " OptTyp=" + OptTyp + " OptGen=" + OptGen + gen[OptGen] + " bicycle going to " +
direct[OptDir] + " is recognised. xLeft=" + OptXLeft + " yLeft=" + OptYLeft +
" Radius=" + RadiusM + " xRight=" + OptXRight +
" Reduction=" + Reduction[OptVer]);

for (int k = 0; k < 11; k++)
{
if (OptDir == 0)
if (OptTyp == 0) PointX[k] = OptXLeft + ProtX0[k] * RadiusM * RedPointX[OptVer];
else             PointX[k] = OptXLeft + ProtX1[k] * RadiusM * RedPointX[OptVer];
else
if (OptTyp == 0) PointX[k] = OptXRight - ProtX0[k] * RadiusM * RedPointX[OptVer];
else             PointX[k] = OptXRight - ProtX1[k] * RadiusM * RedPointX[OptVer];

if (OptTyp == 0) PointY[k] = OptYLeft - ProtY0[k] * RadiusM; //[OptVer];
else             PointY[k] = OptYLeft - ProtY1[k] * RadiusM; //[OptVer];
}

int m = 0;
int X0, X1, Y0, Y1;
for (m = 0; m < 5; m++) //====================================================================================
{
if (OptGen == 0)
{
X0 = (int)PointX[BlockM[m, 0]]; X1 = (int)PointX[BlockM[m, 1]];
Y0 = (int)PointY[BlockM[m, 0]]; Y1 = (int)PointY[BlockM[m, 1]];
}
else
{
X0 = (int)PointX[BlockF[m, 0]]; X1 = (int)PointX[BlockF[m, 1]];
Y0 = (int)PointY[BlockF[m, 0]]; Y1 = (int)PointY[BlockF[m, 1]];
}
/*
g.DrawLine(whitePen, (int)((X0 - halfWidth) * Scale1) + maX, (int)(Y0 * Scale1) + maY, // upperline
(int)((X0 + halfWidth) * Scale1) + maX, (int)(Y0 * Scale1) + maY);

g.DrawLine(whitePen, (int)((X0 + halfWidth) * Scale1) + maX, (int)(Y0 * Scale1) + maY, // right line
(int)((X1 + halfWidth) * Scale1) + maX, (int)(Y1 * Scale1) + maY);

g.DrawLine(whitePen, (int)((X1 + halfWidth) * Scale1) + maX, (int)(Y1 * Scale1) + maY,
(int)((X1 - halfWidth) * Scale1) + maX, (int)(Y1 * Scale1) + maY); // lower line

g.DrawLine(whitePen, (int)((X1 - halfWidth) * Scale1) + maX, (int)(Y1 * Scale1) + maY,
(int)((X0 - halfWidth) * Scale1) + maX, (int)(Y0 * Scale1) + maY); // left line
* --*
}

for (m = 5; m <= 6; m++)
{
if (OptGen == 0)
{
X0 = (int)PointX[BlockM[m, 0]]; X1 = (int)PointX[BlockM[m, 1]];
Y0 = (int)PointY[BlockM[m, 0]]; Y1 = (int)PointY[BlockM[m, 1]];
}
else
{
X0 = (int)PointX[BlockF[m, 0]]; X1 = (int)PointX[BlockF[m, 1]];
Y0 = (int)PointY[BlockF[m, 0]]; Y1 = (int)PointY[BlockF[m, 1]];
}
/*
g.DrawLine(whitePen,
(int)(X0 * Scale1) + maX, (int)((Y0 - halfWidth) * Scale1) + maY,
(int)(X0 * Scale1) + maX, (int)((Y0 + halfWidth) * Scale1) + maY); // left line

g.DrawLine(whitePen,
(int)(X0 * Scale1) + maX, (int)((Y0 + halfWidth) * Scale1) + maY, // lower line
(int)(X1 * Scale1) + maX, (int)((Y1 + halfWidth) * Scale1) + maY);

g.DrawLine(whitePen,
(int)(X1 * Scale1) + maX, (int)((Y1 + halfWidth) * Scale1) + maY, // right line
(int)(X1 * Scale1) + maX, (int)((Y1 - halfWidth) * Scale1) + maY);

g.DrawLine(whitePen,
(int)(X1 * Scale1) + maX, (int)((Y1 - halfWidth) * Scale1) + maY, // upper line
(int)(X0 * Scale1) + maX, (int)((Y0 - halfWidth) * Scale1) + maY);
* --*
} //============================ end for (m = 5; ... ====================================================

for (m = 0; m < 7; m++) //================================================================
{
if (OptGen == 0)
{
X0 = (int)PointX[BlockM[m, 0]]; X1 = (int)PointX[BlockM[m, 1]];
Y0 = (int)PointY[BlockM[m, 0]]; Y1 = (int)PointY[BlockM[m, 1]];
}
else
{
X0 = (int)PointX[BlockF[m, 0]]; X1 = (int)PointX[BlockF[m, 1]];
Y0 = (int)PointY[BlockF[m, 0]]; Y1 = (int)PointY[BlockF[m, 1]];
}

/*
g.DrawLine(redPen,
(int)(X0 * Scale1) + maX, (int)(Y0 * Scale1) + maY,
(int)(X0 * Scale1) + maX, (int)(Y0 * Scale1) + maY); --*

g.DrawLine(redPen,
(int)(X0 * Scale1) + maX, (int)((Y0) * Scale1) + maY,
(int)(X1 * Scale1) + maX, (int)(Y1 * Scale1) + maY);

/*
g.DrawLine(redPen,
(int)(X1 * Scale1) + maX, (int)(Y1 * Scale1) + maY,
(int)(X1 * Scale1) + maX, (int)(Y1 * Scale1) + maY);

g.DrawLine(redPen,
(int)(X1 * Scale1) + maX, (int)(Y1 * Scale1) + maY,
(int)(X0 * Scale1) + maX, (int)(Y0 * Scale1) + maY); --*
} //================================== end for (m = 0; m < 7; m++) ==================================
} //------------------------------------ end if (OptDir ... ----------------------------------------------------
     
} //************************************** end RecoBicycle2 ******************************************************
 * --*/