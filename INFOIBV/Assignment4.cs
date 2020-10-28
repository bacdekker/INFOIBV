using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private byte[,] findRegions(byte[,] inputImage)
        {
            int[,] im = new int[inputImage.GetLength(0), inputImage.GetLength(1)];
            int highest = 1;
            for (int x = 1; x < inputImage.GetLength(0) - 1; x++)
            {
                for (int y = 1; y < inputImage.GetLength(1) - 1; y++)
                {
                    if (inputImage[x, y] == 255)
                    {
                        if (im[x, y - 1] != 0 && im[x, y - 1] != 255)
                        {
                            im[x, y] = inputImage[x, y - 1];
                            inputImage[x, y] = inputImage[x, y - 1];
                        }
                        else if (inputImage[x - 1, y] != 0 && im[x - 1, y] != 255)
                        {
                            im[x, y] = inputImage[x - 1, y];
                            inputImage[x, y] = inputImage[x - 1, y];
                        }
                        else if (inputImage[x - 1, y - 1] != 0 && im[x - 1, y - 1] != 255)
                        {
                            im[x, y] = inputImage[x - 1, y - 1];
                            inputImage[x, y] = inputImage[x - 1, y - 1];
                        }
                        else if (inputImage[x + 1, y - 1] != 0 && im[x + 1, y - 1] != 255)
                        {
                            im[x, y] = inputImage[x + 1, y - 1];
                            inputImage[x, y] = inputImage[x + 1, y - 1];
                        }
                        else
                        {
                            im[x, y] = highest;
                            inputImage[x, y] = (byte)highest;
                            highest++;
                        }
                    }
                }
            }

            for (int i = 0; i < 2; i++)
            {
                for (int x = 1; x < im.GetLength(0) - 1; x++)
                {
                    for (int y = 1; y < im.GetLength(1) - 1; y++)
                    {
                        if (im[x, y] > 0)
                        {
                            List<int> neighbours = new List<int> { im[x - 1, y - 1], im[x - 1, y], im[x + 1, y], im[x, y - 1], im[x, y + 1], im[x + 1, y - 1], im[x + 1, y], im[x + 1, y + 1] };
                            neighbours.RemoveAll(q => q == 0);
                            if (neighbours.Any()) im[x, y] = neighbours.Min();
                            else im[x, y] = 0;
                        }
                    }
                }
                for (int x = im.GetLength(0) - 2; x > 0; x--)
                {
                    for (int y = im.GetLength(1) - 2; y > 0; y--)
                    {
                        if (im[x, y] > 0)
                        {
                            List<int> neighbours = new List<int> { im[x - 1, y - 1], im[x - 1, y], im[x + 1, y], im[x, y - 1], im[x, y + 1], im[x + 1, y - 1], im[x + 1, y], im[x + 1, y + 1] };
                            neighbours.RemoveAll(q => q == 0);
                            if (neighbours.Any()) im[x, y] = neighbours.Min();
                            else im[x, y] = 0;
                        }
                    }
                }
            }

            List<int> values = new List<int>();
            for (int x = 0; x < im.GetLength(0); x++)
            {
                for (int y = 0; y < im.GetLength(1); y++)
                {
                    if (!values.Contains(im[x, y])) values.Add(im[x, y]);
                }
            }

            for (int x = 0; x < im.GetLength(0); x++)
            {
                for (int y = 0; y < im.GetLength(1); y++)
                {
                    inputImage[x, y] = (byte)values.FindIndex(q => q == im[x, y]);
                }
            }
            return inputImage;
        }
        
        private byte[,] detectTriangles(byte[,] inputImage, List<Point> corners, bool pointDraw)
        {
            List<List<Point>> shapes = new List<List<Point>>();
            byte[,] regionImage = thresholdImage(inputImage, 180);
            regionImage = findRegions(regionImage);

            byte[,] edgeImage = edgeMagnitude(regionImage, HorizontalKernel(), VerticalKernel());
            regionImage = dilateImage(regionImage, createStructuringElement('c', 25));

            for (int i = 1; i < 255; i++)
            {
                List<Point> temp = new List<Point>();
                for (int x = 0; x < inputImage.GetLength(0); x++)
                {
                    for (int y = 0; y < inputImage.GetLength(1); y++)
                    {
                        if (regionImage[x, y] == i)
                        {
                            if (corners.Contains(new Point(x, y)))
                            {
                                corners.Remove(new Point(x, y));
                                temp.Add(new Point(x, y));
                            }
                        }
                    }
                }
                if (!temp.Any()) continue;
                else shapes.Add(temp);
            }

            for (int i = 0; i < shapes.Count; i++)
            {
                if(pointDraw) inputImage = drawPoints(inputImage, shapes[i], 10, (byte)(i * 20));
                if (isSharkTooth(shapes[i], edgeImage))
                {
                    inputImage = drawBoundingBox(inputImage, findMaximumCorners(shapes[i]).Item1, findMaximumCorners(shapes[i]).Item2);
                }
            }
            return inputImage;
        }

        private double distancePointToLine(Point p1, Point p2, Point notOnLine)
        {
            double distance = Math.Abs((p2.Y - p1.Y) * notOnLine.X - (p2.X - p1.X) * notOnLine.Y + p2.X * p1.Y - p2.Y * p1.X);
            distance /= Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
            return distance;
        }

        private bool isSharkTooth(List<Point> points, byte[,] edgeImage)
        {
            List<Point> pt = new List<Point>();
            foreach (Point p in points)
            {
                pt.Add(new Point(p.X, p.Y));
            }

            if (pt.Count < 3) return false;
            Point mid = findMid(pt);
            Point farthestPoint;
            float farthest;
            List<Point> remove =  new List<Point>();
            List<Point> corners = new List<Point>();

            //Find first point
            farthestPoint = findFarthest(pt, mid);
            farthest = (farthestPoint.X - mid.X) * (farthestPoint.X - mid.X) +
                       (farthestPoint.Y - mid.Y) * (farthestPoint.Y - mid.Y);
            corners.Add(farthestPoint);
            pt.Remove(farthestPoint);

            //Find second point
            Point secondCorner = findFarthest(pt, farthestPoint);
            corners.Add(secondCorner);
            pt.Remove(secondCorner);

            //Find thrid point
            Point thirdCorner = findFarthestPointToLine(pt, farthestPoint, secondCorner);
            corners.Add(thirdCorner);
            pt.Remove(thirdCorner);

            double mul = 0.4;
            if (corners.Count < 3) return false;
            double dist01 =
                Math.Sqrt(Math.Pow(corners[0].X - corners[1].X, 2) + Math.Pow(corners[0].Y - corners[1].Y, 2));
            double dist02 =
                Math.Sqrt(Math.Pow(corners[0].X - corners[2].X, 2) + Math.Pow(corners[0].Y - corners[2].Y, 2));
            double dist12 =
                Math.Sqrt(Math.Pow(corners[1].X - corners[2].X, 2) + Math.Pow(corners[1].Y - corners[2].Y, 2));

            if (dist01 < mul * dist02 || dist02 < mul * dist12)
                return false;

            if (dist02 < mul * dist01 || dist02 < mul * dist12)
                return false;

            if (dist12 < mul * dist01 || dist02 < mul * dist02)
                return false;

            double distanceToLine1 = distancePointToLine(farthestPoint, secondCorner, thirdCorner  );
            double distanceToLine2 = distancePointToLine(farthestPoint, thirdCorner , secondCorner );
            double distanceToLine3 = distancePointToLine(secondCorner , thirdCorner , farthestPoint);

            if (distanceToLine1 < 10 || distanceToLine2 < 10 || distanceToLine3 < 10)
                return false;

            foreach (Point point in pt)
            {
                double d1 = distancePointToLine(farthestPoint, secondCorner, point);
                double d2 = distancePointToLine(farthestPoint, thirdCorner , point);
                double d3 = distancePointToLine(secondCorner , thirdCorner , point);

                if (d1 > 10 && d2 > 10 && d3 > 10)
                    return false;
            }


            double A = area(corners[0], corners[1], corners[2]);
            double A2 = area(mid, corners[1], corners[2]) + area(corners[0], mid, corners[2]) + area(corners[0], corners[1], mid);
            return (Math.Abs(A - A2) < 10);
        }

        private double area(Point p1, Point p2, Point p3)
        {
            return Math.Abs(p1.X * (p2.Y - p3.Y) + p2.X * (p3.Y - p1.Y) + p3.X * (p1.Y - p2.Y)) / 2;
        }
        private Tuple<Point, Point> findMaximumCorners(List<Point> points)
        {
            Point LT = findMid(points);
            Point RB = findMid(points);
            foreach (Point p in points)
            {
                if (p.X > RB.X) RB.X = p.X;
                if (p.X < LT.X) LT.X = p.X;
                if (p.Y > RB.Y) RB.Y = p.Y;
                if (p.Y < LT.Y) LT.Y = p.Y;
            }
            return new Tuple<Point, Point>(LT, RB);
        }

        private Point findMid(List<Point> points)
        {
            int x = 0;
            int y = 0;
            foreach (Point p in points)
            {
                x += p.X;
                y += p.Y;
            }
            x /= points.Count;
            y /= points.Count;
            return new Point(x, y);
        }
        private Point findFarthest(List<Point> points, Point mid)
        {
            Point farthest = mid;
            foreach (Point p in points)
            {
                if ((p.X        - mid.X) * (p.X        - mid.X) + (p.Y        - mid.Y) * (p.Y        - mid.Y) >
                    (farthest.X - mid.X) * (farthest.X - mid.X) + (farthest.Y - mid.Y) * (farthest.Y - mid.Y))
                {
                    farthest = p;
                }
            }
            return farthest;
        }

        private Point findFarthestPointToLine(List<Point> pts, Point p1, Point p2)
        {
            Point farthest = pts[0];
            double distanceFarthest = distancePointToLine(p1, p2, pts[0]);
            for (int i = 1; i < pts.Count; i++)
            {
                double distance = distancePointToLine(p1, p2, pts[i]);
                if (distance > distanceFarthest)
                {
                    farthest = pts[i];
                    distanceFarthest = distance;
                }
            }

            return farthest;
        }

        private byte[,] drawBoundingBox(byte[,] inputImage, Point one, Point two)
        {
            Point three = new Point(one.X, two.Y);
            Point four = new Point(two.X, one.Y);
            inputImage = imposeLines(inputImage, new List<Point>
            {
                one, three,
                three, two,
                two, four,
                one, four
            }, 255, 1.0f);
            return inputImage;
        }

        private byte[,] drawCenterBox(byte[,] inputImage, Point center, Point target)
        {
            return drawBoundingBox(inputImage, new Point(2 * center.X - target.X, 2 * center.Y - target.Y), target);
        }

        private byte[,] drawPoints(byte[,] inputImage, List<Point> points, int size, byte color)
        {
            foreach (Point p in points)
            {
                for (int x = -(size / 2); x < size / 2 + 1; x++)
                {
                    for (int y = -(size / 2); y < size / 2 + 1; y++)
                    {
                        if (x * x + y * y <= (size / 2) * (size / 2))
                        {
                            if (x + p.X >= 0 && x + p.X < inputImage.GetLength(0) && y + p.Y >= 0 && y + p.Y < inputImage.GetLength(1))
                            {
                                inputImage[x + p.X, y + p.Y] = color;
                            }
                        }
                    }
                }
            }
            return inputImage;
        }

        private List<Point> harrisCorner(byte[,] inputImage, byte radius, int threshold, double[,] filter)
        {
            List<Point> points = new List<Point>();
            double[,] A = new double[inputImage.GetLength(0), inputImage.GetLength(1)];
            double[,] B = new double[inputImage.GetLength(0), inputImage.GetLength(1)];
            double[,] C = new double[inputImage.GetLength(0), inputImage.GetLength(1)];
            double[,] pointMap = new double[inputImage.GetLength(0), inputImage.GetLength(1)];

            for (int x = 2; x < inputImage.GetLength(0) - 2; x++)
            {
                for (int y = 2; y < inputImage.GetLength(1) - 2; y++)
                {
                    double sobelx = 0.125f * (-inputImage[x - 1, y - 1] - 2 * inputImage[x - 1, y] - inputImage[x - 1, y + 1] 
                                             + inputImage[x + 1, y - 1] + 2 * inputImage[x + 1, y] + inputImage[x + 1, y + 1]);
                    double sobely = 0.125f * (-inputImage[x - 1, y - 1] - 2 * inputImage[x, y - 1] - inputImage[x + 1, y - 1] 
                                             + inputImage[x - 1, y + 1] + 2 * inputImage[x, y + 1] + inputImage[x + 1, y + 1]);
                    A[x, y] = sobelx * sobelx;
                    B[x, y] = sobely * sobely;
                    C[x, y] = Math.Abs(sobelx * sobely);
                }
            }
            A = convolveImageDouble(A, filter);
            B = convolveImageDouble(B, filter);
            C = convolveImageDouble(C, filter);

            for (int x = 0; x < inputImage.GetLength(0); x++)
            {
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    double val = (A[x, y] * B[x, y] - C[x, y] * C[x, y]) - 0.04f * (A[x, y] + B[x, y]) * (A[x, y] + B[x, y]);
                    if (val > threshold) pointMap[x, y] = val;
                }
            }
            for (int x = 0; x < inputImage.GetLength(0); x++)
            {
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    double curval = pointMap[x, y];
                    if (curval != 0)
                    {
                        for (int dx = -radius; dx < radius; dx++)
                        {
                            for (int dy = -radius; dy < radius; dy++)
                            {
                                if (dx * dx + dy * dy < radius * radius && x + dx >= 0 && x + dx < inputImage.GetLength(0) && y + dy >= 0 && y + dy < inputImage.GetLength(1))
                                {
                                    if (pointMap[x + dx, y + dy] > curval)
                                    {
                                        curval = 0;
                                        break;
                                    }
                                }
                            }
                        }
                        if (curval != 0) points.Add(new Point(x, y));
                    }
                }
            }
            return points;
        }

        private byte[,] sharpenEdges(byte[,] inputImage)
        {
            byte[,] copy = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            sbyte[,] sharpener = contrastSharpener();

            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    int Greyvalue = 0;
                    for (int i = -1; i <= 1; i++)
                    {
                        // Extending the image in in case we encounter borders
                        int xPixel = x + i;
                        if (xPixel >= InputImage.Size.Width)
                            xPixel = InputImage.Size.Width - 1;
                        if (xPixel < 0)
                            xPixel = 0;
                        for (int j = -1; j <= 1; j++)
                        {
                            // Extending the image in case we encounter borders
                            int yPixel = y + j;
                            if (yPixel >= InputImage.Size.Height)
                                yPixel = InputImage.Size.Height - 1;
                            if (yPixel < 0)
                                yPixel = 0;

                            Greyvalue += sharpener[i + 1, j + 1] * inputImage[xPixel, yPixel];
                        }
                    }

                    Greyvalue = Math.Min(Greyvalue, 255);
                    Greyvalue = Math.Max(Greyvalue, 0);
                    copy[x, y] = (byte)Greyvalue;
                }
            }
            return copy;
        }

        public List<Point> puntenToLineSegments(List<Point> pts, byte[,] edgeStrength, byte[,] inputImage)
        {
            List<Punt> punten = Punt.convert(pts);

            for (int i = 0; i < punten.Count; i++)
            {
                for (int j = i + 1; j < punten.Count; j++)
                {
                    punten[i].isConnectedWith(edgeStrength, inputImage, punten[j]);
                }
            }

            List<Point> segmenten = new List<Point>();
            foreach (Punt p in punten)
            {
                Point p1 = new Point(p.px, p.py);
                foreach (Punt q in p.connectedPoints)
                {
                    Point p2 = new Point(q.px, q.py);
                    segmenten.Add(p1);
                    segmenten.Add(p2);
                }
            }

            return segmenten;
        }
        //createGaussianFilterDouble and convolveImageDouble are the same as their byte counterpart, only altered to use doubles instead.
        private double[,] createGaussianFilterDouble(byte size, double sigma)
        {
            if (size % 2 != 1) throw new Exception("size can't be even");

            double[,] filter = new double[size, size];
            double sigmasq = sigma * sigma;
            double sumtot = 0;           //Used for normalisation

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    double tmp = (double)(Math.E / (2 * Math.PI * sigmasq)) - (x ^ 2 + y ^ 2) / (2 * sigmasq);
                    sumtot += tmp;
                    filter[x, y] = tmp;
                }
            }
            sumtot = 1.0f / sumtot; //inverse of total for normalisation

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    filter[x, y] *= sumtot;
                }
            }
            return filter;
        }

        private Double[,] convolveImageDouble(Double[,] inputImage, double[,] filter)
        {
            int[] offset = { filter.GetLength(0) / 2, filter.GetLength(1) / 2 };        //contains the offset used for the borders.
            Double[,] tempImage = new Double[inputImage.GetLength(0) + 2 * offset[0], inputImage.GetLength(1) + 2 * offset[1]];
            Double[,] finalImage = new Double[inputImage.GetLength(0), inputImage.GetLength(1)];

            Double avg = 0;
            for (int x = 0; x < inputImage.GetLength(0); x++)       //gets the average pixel value 
            {
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    avg += inputImage[x, y];
                }
            }

            avg = avg / (inputImage.GetLength(0) * inputImage.GetLength(1));
            for (int x = 0; x < tempImage.GetLength(0); x++)       //fill temp image with average to fill corners.
            {
                for (int y = 0; y < tempImage.GetLength(1); y++)
                {
                    tempImage[x, y] = (byte)avg;
                }
            }
            for (int x = 0; x < inputImage.GetLength(0); x++) //insert image in middle
            {
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    tempImage[x + offset[0], y + offset[1]] = inputImage[x, y];
                }
            }

            //loops over every pixel, and for each pixel applies the filter with another loop. The value of each multiplication is summed and creates a final image.
            for (int x = 0; x < finalImage.GetLength(0); x++)
            {
                for (int y = 0; y < finalImage.GetLength(1); y++)
                {
                    Double val = 0;
                    for (int xf = 0; xf < filter.GetLength(0); xf++)
                    {
                        for (int yf = 0; yf < filter.GetLength(1); yf++)
                        {
                            val += filter[xf, yf] * tempImage[x + xf, y + yf];
                        }
                    }

                    finalImage[x, y] = (byte)val;
                }
            }

            return finalImage;
        }

    }
}