using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinThantSin.OpenSourceGames
{
   public class CurveRatio
   {
      // NOTE : A point's x and y positions are measured from bottom-left corner of the piece.
      // X~RATIO  =     point's x position / piece's width
      // Y~RATIO  =     point's y position / piece's height
      private const double X2RATIO = 0.760869565217391;
      private const double Y2RATIO = 0.183946488294314;

      private const double X3RATIO = 0.0802675585284281;
      private const double Y3RATIO = 0.150501672240803;

      private const double X4RATIO = 0.5;

      public double X2
      {
         protected set;
         get;
      }
      public double X3
      {
         protected set;
         get;
      }
      public double X4
      {
         protected set;
         get;
      }
      public double X5
      {
         protected set;
         get;
      }
      public double X6
      {
         protected set;
         get;
      }

      public double Y2
      {
         protected set;
         get;
      }
      public double Y3
      {
         protected set;
         get;
      }
      public double Y4
      {
         protected set;
         get;
      }
      public double Y5
      {
         protected set;
         get;
      }
      public double Y6
      {
         protected set;
         get;
      }

      public CurveRatio(Vector3 v)
      {
         Set(v.X, v.Y, v.Z);
      }

      public CurveRatio(double vari1, double vari2, double vari4)
      {
         Set(vari1, vari2, vari4);
      }

      public void Set(double vari1, double vari2, double vari4)
      {
         X2 = X2RATIO * vari1;
         Y2 = Y2RATIO * vari2;
         X3 = X3RATIO * vari1;
         Y3 = Y3RATIO * vari2;
         X4 = X4RATIO * vari4;
         Y4 = Y2;
         Y4 = Y2;
         X6 = X2;
         Y6 = Y2;
         X5 = X3;
         Y5 = Y3;
      }
   }
}
