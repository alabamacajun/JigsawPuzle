using System;
using System.Drawing;

namespace MinThantSin.OpenSourceGames
{
   public class BezierCurve
   {
      // NOTE : A point's x and y positions are measured from bottom-left corner of the piece.
      // X~RATIO  =     point's x position / piece's width
      // Y~RATIO  =     point's y position / piece's height
      private const double CURVE_TENSION = 1.2;

      public Point[] Points { get; set; }

      public static BezierCurve CreateHorizontal(int length, CurveRatio ratio)
      {
         double curvature = length * CURVE_TENSION;

         int x1, y1;     // First curve's starting point
         int x2, y2;     // First curve's first control point
         int x3, y3;     // First curve's second control point
         int x4, y4;     // First curve's ending point, second curve's starting point
         int x5, y5;     // Second curve's first control point
         int x6, y6;     // Second curve's second control point
         int x7, y7;     // Second curve's ending point

         // First curve (first curve's ending point is (X4, Y4), which is also second curve's end point      
         x1 = 0;
         y1 = 0;

         x2 = x1 + (int)(length * ratio.X2);
         y2 = y1 + (int)(curvature * ratio.Y2);

         x3 = x1 + (int)(length * ratio.X3);
         y3 = y1 - (int)(curvature * ratio.Y3);

         x4 = x1 + (int)(length * ratio.X4);
         y4 = y1 - (int)(curvature * ratio.Y4);

         // Second curve (second curve's ending point is (X4, Y4) )      
         x7 = x1 + length;
         y7 = y1;

         x6 = x7 - (int)(length * ratio.X6);
         y6 = y7 + (int)(curvature * ratio.Y6);

         x5 = x7 - (int)(length * ratio.X5);
         y5 = y7 - (int)(curvature * ratio.Y5);

         BezierCurve curve = new BezierCurve()
         {
            Points = new Point[] 
                {
                    new Point(x1, y1), 
                    new Point(x2, y2),
                    new Point(x3, y3), 
                    new Point(x4, y4),
                    new Point(x5, y5), 
                    new Point(x6, y6),
                    new Point(x7, y7)
                }
         };

         return curve;
      }

      public static BezierCurve CreateVertical(int length, CurveRatio ratio)
      {
         BezierCurve curve = CreateHorizontal(length, ratio);
         curve.Rotate(90);

         int offsetX = 0 - curve.Points[0].X;
         int offsetY = 0 - curve.Points[0].Y;

         return curve.Translate(offsetX, offsetY);
      }

      public BezierCurve Translate(int transX, int transY)
      {
         for (int i = 0; i < this.Points.Length; i++)
         {
            this.Points[i].X += transX;
            this.Points[i].Y += transY;
         }

         return this;
      }

      public BezierCurve FlipHorizontal()
      {
         for (int i = 0; i < this.Points.Length; i++)
         {
            this.Points[i].X *= -1;
         }

         return this;
      }

      public BezierCurve FlipVertical()
      {
         for (int i = 0; i < this.Points.Length; i++)
         {
            this.Points[i].Y *= -1;
         }

         return this;
      }

      // ===============================================
      // Transformation code adapted from C++ source code in the book
      // Direct3D Programming (Kickstart) by Clayton Walnum
      // ===============================================
      public BezierCurve Rotate(int degrees)
      {
         double radians = 6.283185308 / (360 / degrees);
         double cosine = Math.Cos(radians);
         double sine = Math.Sin(radians);

         for (int i = 0; i < this.Points.Length; i++)
         {
            int rotatedX = (int)(this.Points[i].X * cosine - this.Points[i].Y * sine);
            int rotatedY = (int)(this.Points[i].Y * cosine + this.Points[i].X * sine);

            this.Points[i].X = rotatedX;
            this.Points[i].Y = rotatedY;
         }

         return this;
      }
   }
}
