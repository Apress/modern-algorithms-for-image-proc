using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WFpolyArc
{
  class ConjugGrad
  {
    /* File "\CONJ_S\KonGra_R.C". Entwickelt am 27.12.1996 aus
      "\CONJ\Konj_Gra.C". Es ist eine "reine" Version: Sie enth„lt KEINE
      spezifische Zielfunktion. Diese muá in einem anderen Modul, z.B.
      in "Mit_Sonn.C", definiert werden.
      Die Vektoren bleiben im Format [1..N] und werden von "d_vector()"
      allokiert. Diese ist hier definiert.
      Die Dateien "nrutil.c" und "nrutil.h" werden NICHT benutzt, da sie sehr
      viele Warnungen hervorrufen. Das Modul "\SHAPE\Kov_Util.C" wird auch
      nicht benutzt, weil es zu viel überflüssiges enthält.
      ------------- */
    public double[,] Points;
    public double[] Param;
    //public double[] xicom;
    public int numbPoint;
    public int numbPar;
    //Variables "ncom", "pcom" and "xicom" are defined below and set by the method "dlinmin" in "Conj_Gra"
    public ConjugGrad(int nPo, int nPar, double[,] Po, double[] Par) // Constructor
    {
      numbPoint = nPo;
      numbPar = nPar;
      Points = new double[numbPoint, 2];
      Param = new double[numbPar];
      for (int iPar = 0; iPar < numbPar; iPar++) Param[iPar] = Par[iPar];
      for (int iPo = 0; iPo < numbPoint; iPo++)
      {
        Points[iPo, 0] = Po[iPo, 0];
        Points[iPo, 1] = Po[iPo, 1];
      }
    }

    int ITMAX_B = 100;    // for "dbrent()"
    double CGOLD = 0.3819660;   // for "brent()"
    double ZEPS = 1.0e-10;
    /*
    #define SIGN(a,b) ((b) > 0.0 ? fabs(a) : -fabs(a))
    #define MOV3(a,b,c, d,e,f) (a)=(d);(b)=(e);(c)=(f);
    --*/

    double GOLD = 1.618034;       // for "mnbrak()"
    double GLIMIT = 100.0;
    double TINY = 1.0e-20;
    /*
    #define MAX(a,b) ((a) > (b) ? (a) : (b))
    #define SHFT(a,b,c,d) (a)=(b);(b)=(c);(c)=(d);
    --*/

    double TOL = 2.0e-4; // es war 2.0e-4           // für "dlinmin()"

    //#define ITMAX 400            // fr "Conj_Gra()"; Siehe weiter unten
    double EPS = 1.0e0;    // es war 1.0e-10
    //#define FREEALL free_d_vector(xi,1,n);free_d_vector(h,1,n);free_d_vector(g,1,n);

/* -- Funktionen aus dem aufgabespezifischen Modul (z.B. Mit_Sonn.C) -- 

double func(double *p);

void dfunc(double *p, double *Grad);

/* ---------------- dieses Modul enthält: --------------------------- 

void Print_Tol(FILE *Prot, double ftol);

double *d_vector(int nl, int nh);

void free_d_vector(double *v, int nl, int nh);

void ZeigeVekt(double *xicom, int ncom, char *Tit);

double f1dim(double x);

double df1dim(double x);

void Test_dfunc(void);

double dbrent(double ax, double bx, double cx, double tol, double *xmin);

void mnbrak(double *ax, double *bx, double *cx,
												 double *fa, double *fb, double *fc);

void Set_Param(double ax, double cx);

void Zeige_f1dim(double ax, double bx, double cx, double xmin);

void Zeige_axb(double a, double x, double b, int Iter);

void dlinmin(double *p, double *xi, int n, double *fret);

int Conj_Gra(double *p, int n, double ftol, int *iter, double *fret);

    /* ----------------------- Globale Variablen: ---------------------- */
    int ncom;	/* initialized in "dlinmin()"; "com" bedeutet COMMON=global */
    double[] pcom, xicom; // 26.03.2017: wahrsch. sind "pcom" die Parameter, xicom die part. Abl.

    bool debc = true;     // Variablen für die Anzeige der Zwischenergebnisse
    int ix0=10, iy0=620;
    double masx, masy, offset, x0;
    int ITMAX = 200; // für Conj_Gra(); definiert in "Umb_CS.C"


    public void SHFT(ref double a, ref double b, ref double c, double d)
    {
      (a) = (b); (b) = (c); (c) = (d);
    }

    //#define SIGN(a,b) ((b) > 0.0 ? fabs(a) : -fabs(a))
    //#define MOV3(a,b,c, d,e,f) (a)=(d);(b)=(e);(c)=(f);

    public double SIGN(double a, double b)
    {
      if (b > 0.0) return Math.Abs(a);
      return -Math.Abs(a);
    }

    public void MOV3(ref double a, ref double b, ref double c, ref double d, ref double e,
                                                                                ref double f)
    {
      a = d; b = e; c = f;
    }

    void Print_Tol( double ftol)
    /*Schreibt die Werte der Toleranzen in das Protokol. ---------------- */
    {
      MessageBox.Show("In 'main': ftol=" + ftol + " in 'KonGra_R' TOL=" + TOL + " ; EPS=" + EPS);
    } /* ********************* end Print_Tol **************************** */


/* void nrerror(char error_text[])
//Bringt eine Fehlermeldung im Grafikmodus und h„lt an. ------------- 
{ TextGraphXY(0,0,2,TF,2,error_text,0);
  getch();
  closegraph();
  exit(1);
} /* ********************* end nrerror ****************************** */


/* double *d_vector(int nl, int nh)
  Reserviert Platz fr einen [nl..nh]-Vektor. ------------------------ 
{ double *v;
  /* *********************  B E G I N   *****************************
  v=(double *)malloc( (unsigned) (nh-nl+1)*sizeof(double) );
  if (!v) nrerror("allocation failure in d_vector()");
  return v-nl;
} /* ********************* end d_vektor ***************************** */

    /*
    public void ZeigeVekt(double[] xicom, int ncom, char Tit)
    //Druckt im Grafikmodus die Werte des Vektors "xicom[1..ncom]" mit dem Titel "Tit".

    { int ix, j, y;
      ix=y=0;
      //TextGraphXY(ix,y, 1,255,1,Tit,ncom);
      //w=textwidth("xi[1]=-1.0e-10;  ");
      y=20;
      for (j=1; j<=ncom; j++)
      { if (ix > getmaxx()-w)
	       { ix=0; y+=20;
	       }
	     MessageBox.Show("j=" + j + " xi[j]=" + xi[j] + "xicom[j]=" + xicom[j]);
      }
    } /* ******************** end ZeigeVekt ***************************** */

    //public double func(int numbPoint, double[,] Points, int numbPar, double[] Param)
    public double func(double[] Param)
    { 
      double Sum = 0.0;
      for (int ip = 0; ip < numbPoint; ip++)
      { 
        Sum += ((Points[ip, 0] - Param[0])*(Points[ip, 0] - Param[0])*Param[2] +
               (Points[ip, 1] - Param[1])*(Points[ip, 1] - Param[1])*Param[3] - Param[2]*Param[3]);
      }
      return Sum;
    }

public double f1dim(double x)
/* Gibt den Wert der Funktion "func()" in dem ncom-dimensionalen Punkt
  "pcom[]+x*xicom[]" zurück. ---------------- */
{ int j;
  double f;
  double[] xt = new double[ncom +1];
  for (j = 1; j<=ncom; j++) xt[j] = pcom[j] + x*xicom[j]; // pcom und xicom sind global
  f=func(xt);   // ist in dem aufgabenspezifischen Modul bestimmt
  return f;
} /* ************************ end f1dim ***************************** */



public void Test_dfunc()
/* Testet die Funktion "dfunc()" indem sie die Komponenten des Gradienten
  direct berechnet. ------------- */
{ int j;
  double Dif;
  double[] xt, df;
  /* **************************   B E G I N   *********************** */
  xt = new double[ncom + 1];
  df = new double[ncom + 1];
  dfunc(pcom, df);  //"df" ist der Gradient der Zielfunktion in "pcom[]"
  for (j = 1; j <= ncom; j++) xt[j] = pcom[j];

  for (j = 1; j <= ncom; j++)
  { xt[j]=pcom[j]+0.001;
	 Dif=(func(xt) - func(pcom) )/0.001;

	 if (Math.Abs(Dif - df[j]) > 0.01*Math.Abs(Dif) )
	 { string s=String.Format("j={0}; Dif={1}; df[j]={2}; f0={3}; f1={4}",
							j,	  Dif,		df[j], func(pcom), func(xt));
		MessageBox.Show(s);
	 }
	 xt[j]=pcom[j];
  }
} /* ************************ end Test_dfunc ************************ */


    public double df1dim(double x)
    // Gibt den Wert der Ableitung der Funktion "f1dim()" in dem Punkt "x" zurück.
    {
      bool deb = false;
      int j;
      double df1=0.0;
      double[] xt, df;
      /* **************************   B E G I N   *********************** */
      xt = new double[ncom];
      df = new double[ncom]; 
      for (j = 0; j <= ncom; j++) xt[j]=pcom[j]+x*xicom[j];
      dfunc(xt, df);  //"df" ist der Gradient der Zielfunktion func im Punkt "xt[]"
      if (deb) Test_dfunc();
      for (j = 0; j <= ncom; j++) df1 += df[j]*xicom[j];
      return df1;
    } /* ************************ end df1dim **************************** */


    public double dbrent(double ax, double bx, double cx,	double tol, ref double xmin)
    /* Findet das Minimum der Funktion "f1dim()" nach der paraboloschen
      Methode von Brent. Achtung! Die ursprnglichen Parameter-Funktionen
      "f" und "df" sind aus dem Kopf ausgeschlossen und im K”rper durch
      "f1dim()" und "df1dim()" ersetzt worden. -------------------------- */
    { int iter;
      bool ok1, ok2;
      double a, b, d=0.0, d1, d2, du, dv, dw, dx, e=0.0;
      double fu, fv, fw, fx, olde, tol1, tol2, u, u1, u2, v, w, x, xm;
      /* **************************   B E G I N   *********************** */
      if (debc)
      { string s=String.Format("dbrent: ax={0};  bx={1}; cx={2};",
								     ax,       bx,      cx);
	      MessageBox.Show(s);
      }
      a = (ax < cx ? ax : cx);  // a=min(ax,cx);
      b = (ax > cx ? ax : cx);  // b=max(ax,cx);
      x = w = v = bx;
      fw = fv = fx = f1dim(x);
      dw = dv = dx = df1dim(x);
      for (iter = 1; iter <= ITMAX_B; iter++) 
      { xm = 0.5*(a+b);
	      tol1 = tol*Math.Abs(x)+ZEPS;
	      tol2 = 2.0*tol1;
	      ///* ------
	      if (debc)
	      { string s=String.Format("dbrent: it={0}; x={0};  dx={0};",
				      iter, x,       dx);
	        MessageBox.Show(s);
	      } //------- */
	      if ( Math.Abs(x-xm) <= (tol2-0.5*(b-a)) )
	      {
		      xmin=x;
		      return fx;
	      }
	      if (Math.Abs(e) > tol1) 
        {
		      d1 = 2.0*(b-a);
		      d2 = d1;
		      if (dw != dx)  d1 = (w-x)*dx/(dx-dw);
		      if (dv != dx)  d2 = (v-x)*dx/(dx-dv);
		      u1 = x+d1;
		      u2 = x+d2;
		      ok1 = (a-u1)*(u1-b) > 0.0 && dx*d1 <= 0.0;
		      ok2 = (a-u2)*(u2-b) > 0.0 && dx*d2 <= 0.0;
		      olde=e;
		      e=d;
		      if (ok1 || ok2)
          { if (ok1 && ok2)
				      d=(Math.Abs(d1) < Math.Abs(d2) ? d1 : d2);
			      else 
            if (ok1)
				      d=d1;
			      else
				      d=d2;
			      if (Math.Abs(d) <= Math.Abs(0.5*olde)) 
            {
				      u = x+d;
				      if (u-a < tol2 || b-u < tol2)  d = SIGN(tol1, xm-x);
			      } 
            else 
            {
				      d = 0.5*(e = (dx >= 0.0 ? a-x : b-x));
			      }
		      } 
          else 
          {
			      d=0.5*(e=(dx >= 0.0 ? a-x : b-x));
		      }
	      } 
        else 
        {
		      d=0.5*(e=(dx >= 0.0 ? a-x : b-x));
	      }
	      if (Math.Abs(d) >= tol1) 
        {
		      u = x+d;
		      fu = f1dim(u);
	      } 
        else 
        {
		      u = x+SIGN(tol1,d);
		      fu = f1dim(u);
		      if (fu > fx) 
          {
			      xmin = x;
			      return fx;
		      }
	      }
	      du = df1dim(u);
	      if (fu <= fx) {
		      if (u >= x) a = x; else b = x;
		      MOV3(ref v, ref  fv, ref dv, ref w, ref fw, ref dw);
		      MOV3(ref w, ref fw, ref dw, ref  x, ref fx, ref dx);
		      MOV3(ref x, ref fx, ref dx, ref  u, ref fu, ref du);
	      } else {
		      if (u < x) a = u; else b = u;
		      if (fu <= fw || w == x) {
			      MOV3(ref v, ref fv, ref dv, ref  w, ref fw, ref dw);
			      MOV3(ref w, ref fw, ref dw, ref  u, ref fu, ref du);
		      } else if (fu < fv || v == x || v == w) {
            MOV3(ref v, ref  fv, ref  dv, ref  u, ref  fu, ref  du);
		      }
	      }
	      //if (debc) Zeige_axb(a, x, b, iter);
      }
      MessageBox.Show("Too many iterations in routine DBRENT");
      return fx;
    } /* ************************ end dbrent **************************** */


    public void mnbrak(ref double ax, ref double bx, ref double cx,
								ref double fa, ref double fb, ref double fc)
    /* Klammert das Minimum der Funktion "f1dim()" nach dem Verfahren von
      NR, Seite 297 ein. Anfangend von drei gegebenen Punkten "ax", "bx"
      und "cx" berechnet sie neue drei Punkte und die Werte "fa", "fb"
      und "fc" der Funktion in diesen Punkten.
      Achtung! Die ursprngliche Parameter-Funktion  "func()" ist aus
      dem Kopf ausgeschlossen und im K”rper durch "f1dim()"ersetzt worden.
      -------------------------- */
    { double ulim, u, r, q, fu, dum;
      /* **************************   B E G I N   *********************** */
      fa = f1dim(ax);
      fb = f1dim(bx);
      if (fb > fa) 
      {
	      SHFT(ref dum, ref  ax, ref  bx, dum);
	      SHFT(ref dum, ref  fb, ref  fa, dum);
      }
      cx = (bx) + GOLD*(bx-ax);
      fc = f1dim(cx);
      while (fb > fc) /* ============================== */
      {  r = (bx-ax)*(fb-fc);
	      q = (bx-cx)*(fb-fa);
	      u = (bx)-((bx-cx)*q-(bx-ax)*r)/
		      (2.0*SIGN(Math.Max(Math.Abs(q-r),TINY),q-r));
	      ulim = (bx)+GLIMIT*(cx-bx);
	      if ((bx-u)*(u-cx) > 0.0) {
		      fu = f1dim(u);
		      if (fu < fc) {
			      ax = (bx);
			      bx = u;
			      fa = (fb);
			      fb = fu;
			      return;
		      } else if (fu > fb) {
			      cx = u;
			      fc = fu;
			      return;
		      }
		      u = (cx)+GOLD*(cx-bx);
		      fu = f1dim(u);
	      } else if ((cx-u)*(u-ulim) > 0.0) {
		      fu = f1dim(u);
		      if (fu < fc) 
          {
			      SHFT(ref bx, ref cx, ref u, cx+GOLD*(cx-bx));
			      SHFT(ref fb, ref fc, ref fu, f1dim(u));
		      }
	      } else if ((u-ulim)*(ulim-cx) >= 0.0) {
		      u = ulim;
		      fu = f1dim(u);
	      } else {
		      u = (cx)+GOLD*(cx-bx);
		      fu = f1dim(u);
	      }
	      SHFT(ref ax, ref bx, ref cx, u);
        SHFT(ref fa, ref  fb, ref  fc, fu);
      } /* =========== end while (*fb > *fc) ============= */
    } /* ************************ end mnbrak **************************** */


    /*  void Set_Param(double ax, double cx)
    /* Setzt die globalen Parameter "masx", "masy" u.a. für die Funktionen
      "Zeige_f1dim()" und "Zeige_axb()". ------------------- 
    {
      /* **************************   B E G I N   *********************** 
      offset=log(erZF);          // global
      // masx, masy sind auch global
      masy=(double)(iy0-20)/(log( max( f1dim(ax),max(f1dim(cx),erZF) ) )-offset);

      if (cx>ax)
	     masx=(cx-ax)/(double)(getmaxx()-20);
      else
	     masx=(ax-cx)/(double)(getmaxx()-20);
      x0=min(ax,cx);
    } /* ************************ end Set_Param ************************* */


    /*ÉÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍ»
	      void Zeige_f1dim(double ax, double bx, double cx, double xmin)
    /*ÈÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍ¼
      Zeigt logarithmisch den Verlauf der Funktion "f1dim()" im
      eingeklammerten Bereich. -------------------------- 
    { int deb=0, iax, ibx, icx, imin, ix, kx, xalt, iy, yalt;
      double x;
      /* **************************   B E G I N   ***********************
      setfillstyle(1,50);
      bar(0,0,getmaxx(),600);
      iax=ix0+(int) (0.5+(ax-x0)/masx);
      ibx=ix0+(int) (0.5+(bx-x0)/masx);
      icx=ix0+(int) (0.5+(cx-x0)/masx);
      imin=ix0+(int) (0.5+(xmin-x0)/masx);
      sprintf(str,
      "Zeige_f1dim: masx=%le f1dim(0)=%6.1lf f1dim(0.000 000 01)=%6.1lf",
					     masx,    f1dim(0.),    f1dim(0.00000001));
      TextGraphXY(0,0,1,222,2,str,0); B_STOP

      setcolor(255);
      xalt=ix0-10; yalt=iy0-(int)(0.5+masy*( log(f1dim(x0))-offset ) );
      for (kx=ix0; kx<getmaxx(); kx++)
      { x=x0+masx*(double)kx;
	     ix=ix0+(int) (0.5+(x-x0)/masx); // sonst mit ix==kx war es ungenau
	     iy=iy0-(int)(0.5+masy*( log(f1dim(x))-offset) );
	     if (deb )
	     {	sprintf(str,"ix=%d x=%6.1lf  f1dim=%6.1lf yalt=%d y=%d",
						     ix,   x,        f1dim(x),    yalt,   iy);
		    TextGraphXY(0,0,2,222,2,str,0); B_STOP
	     }
	     line(xalt,yalt,ix,iy);
	     if(ix==iax )
	     { setfillstyle(1,TF+1); bar(ix-2,iy-4,ix+2,iy+4); }
	     if(ix==ibx )
	     { setfillstyle(1,TF+2); bar(ix-2,iy-4,ix+2,iy+4); }
	     if(ix==icx )
	     { setfillstyle(1,TF); bar(ix-2,iy-4,ix+2,iy+4); }
	     if(ix==imin)
	     { setfillstyle(1,255); bar(ix-2,iy-4,ix+2,iy+4); }
	     xalt=ix; yalt=iy;
      }
      delay(1000);
    } /* ******************** end Zeige_f1dim *************************** */

    /*ÉÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍ»
	      void Zeige_axb(double a, double x, double b, int Iter)
    /*ÈÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍ¼
      Zeigt logarithmisch die Werte "a", "x" un "b" aus "dbrent()" mit den
      entsprechneden Werten der Funktion "f1dim()". --------------- 
    { int iax, iay, ibx, iby, ixx, ixy;
      /* **************************   B E G I N   *********************** 
      iax=ix0+(int) (0.5+(a-x0)/masx);
      iay=iy0-(int)(0.5+masy*( log(f1dim(a))-offset) );

      ixx=ix0+(int) (0.5+(x-x0)/masx);
      ixy=iy0-(int)(0.5+masy*( log(f1dim(x))-offset) );

      ibx=ix0+(int) (0.5+(b-x0)/masx);
      iby=iy0-(int)(0.5+masy*( log(f1dim(b))-offset) );

      setfillstyle(1,TF+1); bar(iax-2,iay-2,iax+2,iay+2);
      setfillstyle(1,TF+2); bar(ixx-2,ixy-2,ixx+2,ixy+2);
      setfillstyle(1,TF);   bar(ibx-2,iby-2,ibx+2,iby+2);
      sprintf(str,"Iter=%d; a=%6.2lf; x=%6.2lf; b=%6.2lf",Iter,a,x,b);
      TextGraphXY(0,0,2,255,2,str,0);
      //delay(1000);
      B_STOP
      setfillstyle(1,100); bar(iax-2,iay-2,iax+2,iay+2);
      setfillstyle(1,130); bar(ixx-2,ixy-2,ixx+2,ixy+2);
      setfillstyle(1,160); bar(ibx-2,iby-2,ibx+2,iby+2);
      TextGraphXY(0,0,2,100,2,str,0);
    } /* ******************** end Zeige_axb ***************************** */



    public void dlinmin(double[] p, double[] xi, int n, ref double fret)
    /* Findet das Minimum der Funktion "f1dim()" mit Hilfe ihrer Ableitung,
      die von "df1dim()" geliefert wird. Beide Funktionen werden nur von
      den hier aufgerufenen Funktionen "mnbrak()" und "dbrent()" benutzt.
      Achtung! Die ursprünglichen Parameter-Funktionen "func()" und "dfunc()"
      sind aus dem Kopf ausgeschlossen. Sie werden unmittelbar von "f1dim()"
      und "df1dim()" aufgerufen. -------------------------- */
    { int j;
      double xx, xmin, fx, fb, fa, bx ,ax;
      /* **************************   B E G I N   *********************** */
	    ncom=n;
	    pcom = new double [n]; 
	    xicom = new double [n]; 
	    //nrfunc=func;    Das war die Übergabe der Zeiger auf diese Funktionen
	    //nrdfun=dfunc;   an "f1dim()" und "df1dim()".
	    for ( j = 0; j < n; j++) 
      {
		    pcom[j] = p[j];
		    xicom[j] = xi[j];
	    }
	    ax = 0.0;
	    xx = 1.0;
	    //:::::::::::::::::::::::::::::::::
	    mnbrak(ref ax, ref xx, ref bx, ref fa, ref fx, ref fb);
	    if (debc)
	    { string s=String.Format(
	      "Nach mnbrak: ax={0}; xx={1}; bx={2}; fa={3}; fx={4}; fb={5};",
						     ax,      xx,      bx,      fa,      fx,      fb  );
	      MessageBox.Show(s);
	    }
	    //:::::::::::::::::::::::::::::::::
	    //*fret=dbrent(ax, xx, bx, TOL, &xmin);
	    //if (debc) Set_Param( ax, bx);
	    //if (debc) Zeige_f1dim( ax,  xx,  bx, 0.0);

	    fret=dbrent(ax, xx, bx, TOL, ref xmin);
	    //*fret=K_brent(ax, xx, bx, TOL, &xmin);

	    //if (debc) Zeige_f1dim( ax,  xx,  bx, xmin);
	    if (debc)
	    { string s=String.Format(
	      "Nach dbrent: ax={0}; xx={0}; bx={1}; fa={2}; fx={3}; fb={4}; xm={5};",
					      ax,      xx,      bx,      fa,      fx,      fb,      xmin);
	      MessageBox.Show(s);
	      s=String.Format("TOL={0}; fret={1}",TOL, fret);
	      MessageBox.Show(s);
	    }
	    for (j=1;j<=n;j++) 
      {
		    xi[j] *= xmin;
		    p[j] += xi[j];
	    }
    } /* ************************ end dlinmin**************************** */


    public int Conj_Gra(double[] p, int n, double ftol, ref int iter, ref double fret)
    /* Kommentar vom 23.08.97:
      Realisiert die Minimierung der Funktion "func(p)" bezüglich der "n"
      Unbekannten "p". Die Funktionen "double func(double *p)" und
      "void dfunc(double *p, double *Grad)" sollen außerhalb definiert werden.
      "ftol" ist eine Toleranz für die Abbruchbedingung. Ich habe sie in
      "main()" auf 1.0*e-5 gesetzt. "iter" zeigt am Ende, wieviele Iterationen
      stattgefungen haben, "fret" ist der erreichte Wert des Minimums.
      Gibt eine 1 zurück, wenn die gewünschte Genauigkeit erreicht wurde,
      eine 2, wenn der Gradient gleich Null geworden ist (Problem gelöst)
      und eine 3, wenn zu viele Iterationen bereits abgelaufen sind.
      Für Einzelheiten s. Numerical Recipes, pp. 317ff.

      Alter Kommentar:
      Realisiert die Minimierung der Funktion "func()" nach den H”hen PN[].Z
      der gegebenen Punkte mit der Methode der konjugierten Gradiente (s.
      Numerical Recipes, pp. 317ff). Diese Funktion ist die modifizierte
      "frprmn()". Die Žnderungen:
	      Der Parameter "*p" entspricht den H”hen der Punkte "PN[].Z". Es ist
	      ein [1..n]-Vektor. "n" ist die Anzahl der Punkte. Die Variablen
	      "TRI6 Dr[], POINT_T PN[], POINT_T S, double FGW[], int Anz_D" sind
	      global. Die Vektoren sind [1..n]-Vektoren geblieben.
	      Die Funktionen "func()" und "dfunc()" werden nicht als Parameter
	      bergeben. Sie sind weiter oben definiert. --------------------- */
    { int j, its;
      double gg, gam, fp, dgg;
      double[] g, h, xi;
      //void dlinmin(), nrerror(), free_d_vector();
      /* **************************   B E G I N   *********************** */
      g = new double[n + 1];
      h = new double[n + 1];
      xi = new double[n + 1];

      fp = func(p);    // es war:fp = (*func)(p);
      dfunc(p, xi);  // es war: (*dfunc)(p,xi);
      debc = false;
      //if (debc) ZeigeVekt(xi, n, "In 'Conj_Gra()'");

      for (j = 1;j<=n;j++) {
	      g[j] = -xi[j];
	      xi[j] = h[j] = g[j];
      }
      for (its = 1; its <= ITMAX; its++) // ======= Iterations ==============
      { MessageBox.Show("Iteration=" + its);
	      iter=its;
	      if (debc)
	      { string s=String.Format("Vor 'dlinmin' It={0}; fp={1}; fret={2}; p[1]={3}",
											    its,   fp,   fret,    p[1]);
		      MessageBox.Show(s);
	      }
	      dlinmin(p, xi, n, ref fret);
	      if (debc)
	      { string s=String.Format("Nach 'dlinmin' It={0}; fp={1}; fret={2}; p[1]={3}",
											    its,   fp,   fret,    p[1]);
		      MessageBox.Show(s);
	      }
	      //SaveZF[its] = fret;
        if (2.0 * Math.Abs(fret - fp) <= ftol * (Math.Abs(fret) + Math.Abs(fp) + EPS))
	      { //if (debc) ZeigeVekt(xicom, ncom, "Nach 'dlinmin'");
		      return 1;
	      }
	      fp = func(p);    // es war:fp=(*func)(p);
	      dfunc(p, xi);  // es war: (*dfunc)(p,xi);

	      dgg = gg = 0.0;
	      for (j = 1;j<=n;j++) {
		      gg += g[j]*g[j];
    /*		  dgg += xi[j]*xi[j];	This statement for Fletcher-Reeves */
		      dgg += (xi[j]+g[j])*xi[j];
	      }
	      if (gg == 0.0) return 2; // the gradient is zero, The problem is solved
	      
	      gam = dgg/gg;
	      for (j = 1;j<=n;j++) {
		      g[j] = -xi[j];
		      xi[j] = h[j] = g[j]+gam*h[j];
	      }
      } /* ================== end for (its... =========================== */
      MessageBox.Show("Too many iterations in Conj_Gra");
      return 3;
    } /* ************************ end Conj_Gra ************************** */

  } //*************************** end class ConjugGrad **********************************
} //***************************** end namespace ********************************************
