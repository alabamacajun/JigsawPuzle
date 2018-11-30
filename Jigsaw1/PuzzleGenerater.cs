using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing;
using ImageProcessing;

namespace MinThantSin.OpenSourceGames
{
   class PuzzleGenerater
   {
      private const int BottomCurve = 0;
      private const int RightCurve = 1;
      private const int BottomOutty = 2;
      private const int RightOutty = 3;

      public List<PieceCluster> CreateJigsawPuzzle(Size clientSize, Image sourcePicture, int menuStripHeight)
      {
         #region Some validation

         if (sourcePicture == null)
         {
            throw new Exception("Please provide source picture.");
         }

         if (GameSettings.NUM_ROWS <= 1 || GameSettings.NUM_COLUMNS <= 1)
         {
            throw new Exception("GameSettings.NUM_COLUMNS and GameSettings.NUM_ROWS must be at least 2.");
         }

         if (GameSettings.NUM_COLUMNS != GameSettings.NUM_ROWS)
         {
            throw new Exception("GameSettings.NUM_COLUMNS and GameSettings.NUM_ROWS must be the same.");
         }

         #endregion

         #region Make sure the piece size is not too small

         int pieceWidth = sourcePicture.Width / GameSettings.NUM_COLUMNS;
         int pieceHeight = sourcePicture.Height / GameSettings.NUM_ROWS;

         if (pieceWidth < GameSettings.MIN_PIECE_WIDTH || pieceHeight < GameSettings.MIN_PIECE_HEIGHT)
         {
            throw new Exception("The picture is too small. Please select a bigger picture.");
         }

         int lastColPieceWidth = pieceWidth + (sourcePicture.Width % GameSettings.NUM_COLUMNS);
         int lastRowPieceHeight = pieceHeight + (sourcePicture.Height % GameSettings.NUM_ROWS);

         #endregion

         #region Construct jigsaw pieces

         int lastRow = (GameSettings.NUM_ROWS - 1);
         int lastCol = (GameSettings.NUM_COLUMNS - 1);
         var clusters = new List<PieceCluster>();

         Matrix matrix = new Matrix();

         Pen outlinePen = new Pen(Color.Black)
         {
            Width = GameSettings.PIECE_OUTLINE_WIDTH,
            Alignment = PenAlignment.Inset
         };

         var vsets = new List<Vector3>(5);
         vsets.Add(new Vector3(0.9, 1.1, 0.95));
         vsets.Add(new Vector3(1.1, 0.9, 1.05));
         vsets.Add(new Vector3(1.13, 0.95, 1.0));
         vsets.Add(new Vector3(0.9, 0.9, 1.15));
         vsets.Add(new Vector3(0.95, 1.19, 0.95));
         int[, ,] mat = new int[GameSettings.NUM_ROWS, GameSettings.NUM_COLUMNS, 4];

         int pieceID = 0;
         Random rnd = new Random((int)(DateTime.Now.Ticks));

         for (int row = 0; row < GameSettings.NUM_ROWS; row++)
         {
            // Make the top and bottom curves face in the same way or opposite direction.

            for (int col = 0; col < GameSettings.NUM_COLUMNS; col++)
            {
               mat[row, col, RightCurve] = rnd.Next(5);
               mat[row, col, BottomCurve] = rnd.Next(5);
               mat[row, col, RightOutty] = rnd.Next(2);
               mat[row, col, BottomOutty] = rnd.Next(2);

               GraphicsPath figure = new GraphicsPath();

               int offsetX = (col * pieceWidth);
               int offsetY = (row * pieceHeight);
               int horizontalCurveLength = (col == lastCol ? lastColPieceWidth : pieceWidth);
               int verticalCurveLength = (row == lastRow ? lastRowPieceHeight : pieceHeight);
               #region Top

               if (row == 0)
               {
                  int startX = offsetX;
                  int startY = offsetY;
                  int endX = offsetX + horizontalCurveLength;
                  int endY = offsetY;

                  figure.AddLine(startX, startY, endX, endY);
               }
               else
               {
                  var v = vsets[mat[row - 1, col, BottomCurve]];
                  var crTop = new CurveRatio(v.X, v.Y, 2.0 - v.Z);
                  //var crTop = new CurveRatio(v.X, v.Y, v.Z);
                  BezierCurve topCurve = BezierCurve.CreateHorizontal(horizontalCurveLength, crTop);

                  if (mat[row - 1, col, BottomOutty] > 0)
                  {
                     topCurve.FlipVertical();
                  }
                  figure.AddBeziers(topCurve.Translate(offsetX, offsetY).Points);
               }

               #endregion

               #region Right

               if (col == lastCol)
               {
                  int startX = offsetX + lastColPieceWidth;
                  int startY = offsetY;
                  int endX = offsetX + lastColPieceWidth;
                  int endY = offsetY + verticalCurveLength;

                  figure.AddLine(startX, startY, endX, endY);
               }
               else
               {
                  BezierCurve verticalCurve = BezierCurve.CreateVertical(verticalCurveLength, new CurveRatio(vsets[mat[row, col, RightCurve]]));

                  if (mat[row, col, RightOutty] > 0)
                  {
                     verticalCurve.FlipHorizontal();
                  }

                  verticalCurve.Translate(offsetX + pieceWidth, offsetY);
                  figure.AddBeziers(verticalCurve.Points);
               }

               #endregion

               #region Bottom

               if (row == lastRow)
               {
                  int startX = offsetX;
                  int startY = offsetY + lastRowPieceHeight;
                  int endX = offsetX + horizontalCurveLength;
                  int endY = offsetY + lastRowPieceHeight;

                  figure.AddLine(endX, endY, startX, startY);
               }
               else
               {
                  BezierCurve bottomCurve = BezierCurve.CreateHorizontal(horizontalCurveLength, new CurveRatio(vsets[mat[row, col, BottomCurve]]));
                  bottomCurve.FlipHorizontal();

                  if (mat[row, col, BottomOutty] > 0)
                  {
                     bottomCurve.FlipVertical();
                  }

                  bottomCurve.Translate(offsetX + horizontalCurveLength, offsetY + pieceHeight);
                  figure.AddBeziers(bottomCurve.Points);
               }

               #endregion

               #region Left

               if (col == 0)
               {
                  int startX = offsetX;
                  int startY = offsetY;
                  int endX = offsetX;
                  int endY = offsetY + verticalCurveLength;

                  figure.AddLine(endX, endY, startX, startY);
               }
               else
               {
                  var v = vsets[mat[row, col - 1, RightCurve]];
                  var crLeft = new CurveRatio(v.X, v.Y, 2.0 - v.Z);
                  //var crLeft = new CurveRatio(v.X, v.Y, v.Z);
                  BezierCurve verticalCurve = BezierCurve.CreateVertical(verticalCurveLength, crLeft);
                  verticalCurve.FlipVertical();

                  if (mat[row, col - 1, RightOutty] > 0)
                  {
                     verticalCurve.FlipHorizontal();
                  }

                  verticalCurve.Translate(offsetX, offsetY + verticalCurveLength);
                  figure.AddBeziers(verticalCurve.Points);
               }

               #endregion

               #region Jigsaw information

               #region Determine adjacent piece IDs for the current piece

               List<Coordinate> adjacentCoords = new List<Coordinate>
                    {
                        new Coordinate(col, row - 1),
                        new Coordinate(col + 1, row),
                        new Coordinate(col, row + 1),
                        new Coordinate(col - 1, row)
                    };

               List<int> adjacentPieceIDs = DetermineAdjacentPieceIDs(adjacentCoords, GameSettings.NUM_COLUMNS);

               #endregion

               #region Construct piece picture

               Rectangle figureLocation = Rectangle.Truncate(figure.GetBounds());

               // Translate the figure to the origin to draw the picture for individual piece
               matrix.Reset();
               matrix.Translate(0 - figureLocation.X, 0 - figureLocation.Y);
               GraphicsPath translatedFigure = (GraphicsPath)figure.Clone();
               translatedFigure.Transform(matrix);

               Rectangle translatedFigureLocation = Rectangle.Truncate(translatedFigure.GetBounds());

               Bitmap piecePicture = new Bitmap(figureLocation.Width, figureLocation.Height);

               using (Graphics gfx = Graphics.FromImage(piecePicture))
               {
                  gfx.FillRectangle(Brushes.White, 0, 0, piecePicture.Width, piecePicture.Height);
                  gfx.ResetClip();
                  gfx.SetClip(translatedFigure);
                  gfx.DrawImage(sourcePicture, new Rectangle(0, 0, piecePicture.Width, piecePicture.Height),
                          figureLocation, GraphicsUnit.Pixel);

                  if (GameSettings.DRAW_PIECE_OUTLINE)
                  {
                     gfx.SmoothingMode = SmoothingMode.AntiAlias;
                     gfx.DrawPath(outlinePen, translatedFigure);
                  }
               }

               // Wanted to do the "bevel edge" effect but too complicated for me.                    
               Bitmap modifiedPiecePicture = (Bitmap)piecePicture.Clone();
               ImageUtilities.EdgeDetectHorizontal(modifiedPiecePicture);
               ImageUtilities.EdgeDetectVertical(modifiedPiecePicture);
               piecePicture = ImageUtilities.AlphaBlendMatrix(modifiedPiecePicture, piecePicture, 200);

               #endregion

               #region Piece and cluster information

               Piece piece = new Piece
               {
                  ID = pieceID,
                  ClusterID = pieceID,
                  Width = figureLocation.Width,
                  Height = figureLocation.Height,
                  BoardLocation = translatedFigureLocation,
                  SourcePictureLocation = figureLocation,
                  MovableFigure = (GraphicsPath)translatedFigure.Clone(),
                  StaticFigure = (GraphicsPath)figure.Clone(),
                  Picture = (Bitmap)piecePicture.Clone(),
                  AdjacentPieceIDs = adjacentPieceIDs
               };

               PieceCluster cluster = new PieceCluster
               {
                  ID = pieceID,
                  Width = figureLocation.Width,
                  Height = figureLocation.Height,
                  BoardLocation = translatedFigureLocation,
                  SourcePictureLocation = figureLocation,
                  MovableFigure = (GraphicsPath)translatedFigure.Clone(),
                  StaticFigure = (GraphicsPath)figure.Clone(),
                  Picture = (Bitmap)piecePicture.Clone(),
                  Pieces = new List<Piece> { piece }
               };

               #endregion

               clusters.Add(cluster);

               #endregion

               pieceID++;
            }
         }

         //var ratioLeft = new CurveRatio(1.1, 0.9, 1.05);
         //var ratioRight = new CurveRatio(1.1, 0.9, 0.95);
         //var ratioTop = new CurveRatio(0.9, 1.1, 1.0);
         //var ratioBottom = new CurveRatio(0.9, 1.1, 1.0);

         //for (int row = 0; row < GameSettings.NUM_ROWS; row++)
         //{
         //   // Make the top and bottom curves face in the same way or opposite direction.
         //   bool topCurveFlipVertical = (row % 2 == 0);
         //   bool bottomCurveFlipVertical = (row % 2 != 0);

         //   for (int col = 0; col < GameSettings.NUM_COLUMNS; col++)
         //   {
         //      // Make the left and right curves face in the same way or opposite direction.
         //      bool leftCurveFlipHorizontal = (col % 2 != 0);
         //      bool rightCurveFlipHorizontal = (col % 2 == 0);

         //      // Toggle the facing of left and right curves with each row.
         //      if (row % 2 == 0)
         //      {
         //         leftCurveFlipHorizontal = (col % 2 == 0);
         //         rightCurveFlipHorizontal = (col % 2 != 0);
         //      }

         //      // Toggle the facing of top and bottom curves with each row.
         //      topCurveFlipVertical = !topCurveFlipVertical;
         //      bottomCurveFlipVertical = !bottomCurveFlipVertical;

         //      GraphicsPath figure = new GraphicsPath();

         //      int offsetX = (col * pieceWidth);
         //      int offsetY = (row * pieceHeight);
         //      int horizontalCurveLength = (col == lastCol ? lastColPieceWidth : pieceWidth);
         //      int verticalCurveLength = (row == lastRow ? lastRowPieceHeight : pieceHeight);

         //      #region Top

         //      if (row == 0)
         //      {
         //         int startX = offsetX;
         //         int startY = offsetY;
         //         int endX = offsetX + horizontalCurveLength;
         //         int endY = offsetY;

         //         figure.AddLine(startX, startY, endX, endY);
         //      }
         //      else
         //      {
         //         BezierCurve topCurve = BezierCurve.CreateHorizontal(horizontalCurveLength, ratioTop);

         //         if (topCurveFlipVertical)
         //         {
         //            topCurve.FlipVertical();
         //         }

         //         topCurve.Translate(offsetX, offsetY);
         //         figure.AddBeziers(topCurve.Points);
         //      }

         //      #endregion

         //      #region Right

         //      if (col == lastCol)
         //      {
         //         int startX = offsetX + lastColPieceWidth;
         //         int startY = offsetY;
         //         int endX = offsetX + lastColPieceWidth;
         //         int endY = offsetY + verticalCurveLength;

         //         figure.AddLine(startX, startY, endX, endY);
         //      }
         //      else
         //      {
         //         BezierCurve verticalCurve = BezierCurve.CreateVertical(verticalCurveLength, ratioRight);

         //         if (rightCurveFlipHorizontal)
         //         {
         //            verticalCurve.FlipHorizontal();
         //         }

         //         verticalCurve.Translate(offsetX + pieceWidth, offsetY);
         //         figure.AddBeziers(verticalCurve.Points);
         //      }

         //      #endregion

         //      #region Bottom

         //      if (row == lastRow)
         //      {
         //         int startX = offsetX;
         //         int startY = offsetY + lastRowPieceHeight;
         //         int endX = offsetX + horizontalCurveLength;
         //         int endY = offsetY + lastRowPieceHeight;

         //         figure.AddLine(endX, endY, startX, startY);
         //      }
         //      else
         //      {
         //         BezierCurve bottomCurve = BezierCurve.CreateHorizontal(horizontalCurveLength, ratioBottom);
         //         bottomCurve.FlipHorizontal();

         //         if (bottomCurveFlipVertical)
         //         {
         //            bottomCurve.FlipVertical();
         //         }

         //         bottomCurve.Translate(offsetX + horizontalCurveLength, offsetY + pieceHeight);
         //         figure.AddBeziers(bottomCurve.Points);
         //      }

         //      #endregion

         //      #region Left

         //      if (col == 0)
         //      {
         //         int startX = offsetX;
         //         int startY = offsetY;
         //         int endX = offsetX;
         //         int endY = offsetY + verticalCurveLength;

         //         figure.AddLine(endX, endY, startX, startY);
         //      }
         //      else
         //      {
         //         BezierCurve verticalCurve = BezierCurve.CreateVertical(verticalCurveLength, ratioLeft);
         //         verticalCurve.FlipVertical();

         //         if (leftCurveFlipHorizontal)
         //         {
         //            verticalCurve.FlipHorizontal();
         //         }

         //         verticalCurve.Translate(offsetX, offsetY + verticalCurveLength);
         //         figure.AddBeziers(verticalCurve.Points);
         //      }

         //      #endregion

         //      #region Jigsaw information

         //      #region Determine adjacent piece IDs for the current piece

         //      List<Coordinate> adjacentCoords = new List<Coordinate>
         //           {
         //               new Coordinate(col, row - 1),
         //               new Coordinate(col + 1, row),
         //               new Coordinate(col, row + 1),
         //               new Coordinate(col - 1, row)
         //           };

         //      List<int> adjacentPieceIDs = DetermineAdjacentPieceIDs(adjacentCoords, GameSettings.NUM_COLUMNS);

         //      #endregion

         //      #region Construct piece picture

         //      Rectangle figureLocation = Rectangle.Truncate(figure.GetBounds());

         //      // Translate the figure to the origin to draw the picture for individual piece
         //      matrix.Reset();
         //      matrix.Translate(0 - figureLocation.X, 0 - figureLocation.Y);
         //      GraphicsPath translatedFigure = (GraphicsPath)figure.Clone();
         //      translatedFigure.Transform(matrix);

         //      Rectangle translatedFigureLocation = Rectangle.Truncate(translatedFigure.GetBounds());

         //      Bitmap piecePicture = new Bitmap(figureLocation.Width, figureLocation.Height);

         //      using (Graphics gfx = Graphics.FromImage(piecePicture))
         //      {
         //         gfx.FillRectangle(Brushes.White, 0, 0, piecePicture.Width, piecePicture.Height);
         //         gfx.ResetClip();
         //         gfx.SetClip(translatedFigure);
         //         gfx.DrawImage(sourcePicture, new Rectangle(0, 0, piecePicture.Width, piecePicture.Height),
         //                 figureLocation, GraphicsUnit.Pixel);

         //         if (GameSettings.DRAW_PIECE_OUTLINE)
         //         {
         //            gfx.SmoothingMode = SmoothingMode.AntiAlias;
         //            gfx.DrawPath(outlinePen, translatedFigure);
         //         }
         //      }

         //      // Wanted to do the "bevel edge" effect but too complicated for me.                    
         //      Bitmap modifiedPiecePicture = (Bitmap)piecePicture.Clone();
         //      ImageUtilities.EdgeDetectHorizontal(modifiedPiecePicture);
         //      ImageUtilities.EdgeDetectVertical(modifiedPiecePicture);
         //      piecePicture = ImageUtilities.AlphaBlendMatrix(modifiedPiecePicture, piecePicture, 200);

         //      #endregion

         //      #region Piece and cluster information

         //      Piece piece = new Piece
         //      {
         //         ID = pieceID,
         //         ClusterID = pieceID,
         //         Width = figureLocation.Width,
         //         Height = figureLocation.Height,
         //         BoardLocation = translatedFigureLocation,
         //         SourcePictureLocation = figureLocation,
         //         MovableFigure = (GraphicsPath)translatedFigure.Clone(),
         //         StaticFigure = (GraphicsPath)figure.Clone(),
         //         Picture = (Bitmap)piecePicture.Clone(),
         //         AdjacentPieceIDs = adjacentPieceIDs
         //      };

         //      PieceCluster cluster = new PieceCluster
         //      {
         //         ID = pieceID,
         //         Width = figureLocation.Width,
         //         Height = figureLocation.Height,
         //         BoardLocation = translatedFigureLocation,
         //         SourcePictureLocation = figureLocation,
         //         MovableFigure = (GraphicsPath)translatedFigure.Clone(),
         //         StaticFigure = (GraphicsPath)figure.Clone(),
         //         Picture = (Bitmap)piecePicture.Clone(),
         //         Pieces = new List<Piece> { piece }
         //      };

         //      #endregion

         //      clusters.Add(cluster);

         //      #endregion

         //      pieceID++;
         //   }
         //}

         #endregion

         #region Scramble jigsaw pieces

         Random random = new Random();

         int boardWidth = clientSize.Width;
         int boardHeight = clientSize.Height;

         foreach (PieceCluster cluster in clusters)
         {
            int locationX = random.Next(1, boardWidth);
            int locationY = random.Next((menuStripHeight + 1), boardHeight);

            #region Make sure the piece is within client rectangle bounds

            if ((locationX + cluster.Width) > boardWidth)
            {
               locationX = locationX - ((locationX + cluster.Width) - boardWidth);
            }

            if ((locationY + cluster.Height) > boardHeight)
            {
               locationY = locationY - ((locationY + cluster.Height) - boardHeight);
            }

            #endregion

            for (int index = 0; index < cluster.Pieces.Count; index++)
            {
               Piece piece = cluster.Pieces[index];
               piece.BoardLocation = new Rectangle(locationX, locationY, piece.Width, piece.Height);

               matrix.Reset();
               matrix.Translate(locationX, locationY);
               piece.MovableFigure.Transform(matrix);
            }

            // Move the figure for cluster piece
            cluster.BoardLocation = new Rectangle(locationX, locationY, cluster.Width, cluster.Height);

            matrix.Reset();
            matrix.Translate(locationX, locationY);
            cluster.MovableFigure.Transform(matrix);
         }
         return clusters;
      }

      /// <summary>
      /// Returns a Piece ID based on the current row and column (X, Y), 
      /// and the number of columns.
      /// </summary>        
      private List<int> DetermineAdjacentPieceIDs(List<Coordinate> coords, int numColumns)
      {
         List<int> pieceIDs = new List<int>();

         foreach (Coordinate coord in coords)
         {
            if (coord.Y >= 0 && coord.Y < GameSettings.NUM_ROWS)
            {
               if (coord.X >= 0 && coord.X < GameSettings.NUM_COLUMNS)
               {
                  int pieceID = (coord.Y * numColumns) + coord.X;
                  pieceIDs.Add(pieceID);
               }
            }
         }

         return pieceIDs;
      }
         #endregion
   }

}

