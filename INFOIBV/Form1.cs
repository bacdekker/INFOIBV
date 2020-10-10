﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap InputImage;
        private Bitmap OutputImage;

        public INFOIBV()
        {
            InitializeComponent();
        }

        /*
         * loadButton_Click: process when user clicks "Load" button
         */
        private void loadImageButton_Click(object sender, EventArgs e)
        {
            if (openImageDialog.ShowDialog() == DialogResult.OK)             // open file dialog
            {
                string file = openImageDialog.FileName;                     // get the file name
                imageFileName.Text = file;                                  // show file name
                if (InputImage != null) InputImage.Dispose();               // reset image
                InputImage = new Bitmap(file);                              // create new Bitmap from file
                //if (InputImage.Size.Height <= 0 || InputImage.Size.Width <= 0 ||
                //    InputImage.Size.Height > 512 || InputImage.Size.Width > 512) // dimension check (may be removed or altered)
                //    MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
                //else
                    pictureBox1.Image = (Image)InputImage;                 // display input image
            }
        }


        /*
         * applyButton_Click: process when user clicks "Apply" button
         */
        private void applyButton_Click(object sender, EventArgs e)
        {
            if (InputImage == null) return;                                 // get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height); // create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)

            // copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                    Image[x, y] = InputImage.GetPixel(x, y);                // set pixel color in array at (x,y)

            // ====================================================================
            // =================== YOUR FUNCTION CALLS GO HERE ====================
            // Alternatively you can create buttons to invoke certain functionality
            // ====================================================================


            byte[,] workingImage = convertToGrayscale(Image);          // convert image to grayscale


            //workingImage = invertImage(workingImage);
            //workingImage = adjustContrast(workingImage);
            //workingImage = convolveImage(workingImage, createGaussianFilter(11, 5f));
            //workingImage = medianFilter(workingImage, 5); // Size needs to be odd
            //workingImage = edgeMagnitude(workingImage, HorizontalKernel(), VerticalKernel());
            //workingImage = thresholdImage(workingImage, 128);
            //workingImage = equalizeImage(workingImage);
            //List<Point> points = traceBoundary(workingImage); 
            //workingImage = erodeImage(workingImage, createStructuringElement('s', 3));
            //workingImage = dilateImage(workingImage, createStructuringElement('s', 3));
            //workingImage = invertImage(openImage(thresholdImage(invertImage(workingImage), 128), createStructuringElement('c', 83)));
            //workingImage = openImage(workingImage, createStructuringElement('s', 9));
            //workingImage = thresholdImage(houghTransformation(thresholdImage(workingImage, 128)), 128);
            //workingImage = invertImage(workingImage);
            //Histogram h = countValues(workingImage);
            //List<Point> peaks = houghPeakFinding(workingImage);
            //Point rThetaPair = peaks[0];
            //List<Point> lineSegments = houghLineDetection(workingImage, rThetaPair, 128, 10, 3);

            // ==================== END OF YOUR FUNCTION CALLS ====================
            // ====================================================================

            // copy array to output Bitmap
            for (int x = 0; x < workingImage.GetLength(0); x++)             // loop over columns
                for (int y = 0; y < workingImage.GetLength(1); y++)         // loop over rows
                {
                    Color newColor = Color.FromArgb(workingImage[x, y], workingImage[x, y], workingImage[x, y]);
                    OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
                }

            pictureBox2.Image = (Image)OutputImage;                         // display output image
        }


        /*
         * saveButton_Click: process when user clicks "Save" button
         */
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // save the output image
        }
        
        #region Assignment1


        /*
         * convertToGrayScale: convert a three-channel color image to a single channel grayscale image
         * input:   inputImage          three-channel (Color) image
         * output:                      single-channel (byte) image
         */

        private byte[,] convertToGrayscale(Color[,] inputImage)
        {
            // create temporary grayscale image of the same size as input, with a single channel
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // process all pixels in the image
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {
                    Color pixelColor = inputImage[x, y];                    // get pixel color
                    byte average = (byte)((pixelColor.R + pixelColor.B + pixelColor.G) / 3); // calculate average over the three channels
                    tempImage[x, y] = average;                              // set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                              // increment progress bar
                }

            progressBar.Visible = false;                                    // hide progress bar

            return tempImage;
        }
        
        private sbyte[,] HorizontalKernel()
        {
            // According to prewitt
            sbyte[,] hKernel = new sbyte[3,3];
            hKernel[0, 0] = -1;
            hKernel[0, 1] = -1;
            hKernel[0, 2] = -1;
            hKernel[1, 0] = 0;
            hKernel[1, 1] = 0;
            hKernel[1, 2] = 0;
            hKernel[2, 0] = 1;
            hKernel[2, 1] = 1;
            hKernel[2, 2] = 1;
            return hKernel;
        }

        private sbyte[,] VerticalKernel()
        {
            sbyte[,] vKernel = new sbyte[3,3];
            vKernel[0, 0] = -1;
            vKernel[1, 0] = -1;
            vKernel[2, 0] = -1;
            vKernel[0, 1] = 0;
            vKernel[1, 1] = 0;
            vKernel[2, 1] = 0;
            vKernel[0, 2] = 1;
            vKernel[1, 2] = 1;
            vKernel[2, 2] = 1;
            return vKernel;
        }

        private int[] makeHistogram(byte[,] inputImage)
        {
            int[] histogram = new int[256];
            for (int x = 0; x < InputImage.Size.Width; x++) // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    histogram[inputImage[x, y]] += 1;
                }

            return histogram;
        }

        private int[] makeHistogramCumulative(int[] histogram)
        {
            int[] cumulativeHistogram = new int[256];
            cumulativeHistogram[0] = histogram[0];
            for (int i = 1; i < histogram.Length; i++)
            {
                cumulativeHistogram[i] = histogram[i] + cumulativeHistogram[i - 1];
            }

            return cumulativeHistogram;
        }

        private byte[,] equalizeImage(byte[,] inputImage)
        {
            int[] histogram = makeHistogram(inputImage);
            int[] cumulativeHistogram = makeHistogramCumulative(histogram);

            int size = InputImage.Size.Width * InputImage.Size.Height;
            
            for(int x = 0; x < InputImage.Size.Width; x++)
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    byte greyValue = inputImage[x, y];
                    byte newValue = (byte)((cumulativeHistogram[greyValue] * 255) / size);
                    inputImage[x, y] = newValue;
                }

            return inputImage;
        }
        
        /*
         * invertImage: invert a single channel (grayscale) image
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image
         */
        private byte[,] invertImage(byte[,] inputImage)
        {
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            byte aMax = 0;
            //for (int x = 0; x < inputImage.GetLength(0); x++)   //gets Highest value in image
            //{
            //    for (int y = 0; y < inputImage.GetLength(1); y++)
            //    {
            //        if (inputImage[x, y] > aMax) aMax = inputImage[x, y];
            //    }
            //}
            aMax = 255;

            for (int x = 0; x < inputImage.GetLength(0); x++)   //applies inversion to each pixel
            {
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    tempImage[x, y] = (byte)(aMax - inputImage[x, y]);
                }
            }
            return tempImage;
        }


        /*
         * adjustContrast: create an image with the full range of intensity values used
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image
         */
        private byte[,] adjustContrast(byte[,] inputImage)
        {
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            byte aHigh = 0;
            byte aLow = 255;
            for (int x = 0; x < inputImage.GetLength(0); x++)       //gets the Highest and lowest values from the image
            {
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    if (inputImage[x, y] > aHigh) aHigh = inputImage[x, y];
                    if (inputImage[x, y] < aLow) aLow = inputImage[x, y];
                }
            }
            if (aHigh == aLow) throw new DivideByZeroException("Highest and lowest values cannot be equal");

            for (int x = 0; x < inputImage.GetLength(0); x++)
            {
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    tempImage[x, y] = (byte)((inputImage[x, y] - aLow) * (255 / (aHigh - aLow)));
                }
            }

            return tempImage;
        }


        /*
         * createGaussianFilter: create a Gaussian filter of specific square size and with a specified sigma
         * input:   size                length and width of the Gaussian filter (only odd sizes)
         *          sigma               standard deviation of the Gaussian distribution
         * output:                      Gaussian filter
         */
        private float[,] createGaussianFilter(byte size, float sigma)
        {
            if (size % 2 != 1) throw new Exception("size can't be even");

            float[,] filter = new float[size, size];
            float sigmasq = sigma * sigma;
            float sumtot = 0;           //Used for normalisation

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float tmp = (float)(Math.E / (2 * Math.PI * sigmasq)) - (x ^ 2 + y ^ 2) / (2 * sigmasq);
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


        /*
         * convolveImage: apply linear filtering of an input image
         * input:   inputImage          single-channel (byte) image
         *          filter              linear kernel
         * output:                      single-channel (byte) image
         */
        private byte[,] convolveImage(byte[,] inputImage, float[,] filter)
        {
            int[] offset = { filter.GetLength(0) / 2, filter.GetLength(1) / 2 };        //contains the offset used for the borders.
            byte[,] tempImage = new byte[inputImage.GetLength(0) + 2 * offset[0], inputImage.GetLength(1) + 2 * offset[1]];
            byte[,] finalImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            int avg = 0;
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
                    tempImage[x, y] = (byte) avg;
                }
            }
            
            for (int x = 0; x < inputImage.GetLength(0); x++)       //stretch top
            {
                for (int y = 0; y < offset[1]; y++)
                {
                    tempImage[x + offset[0], y] = inputImage[x, 0];
                }
            }

            for (int x = 0; x < offset[0]; x++)                     //stretch left
            {
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    tempImage[x, y] = inputImage[0, y];
                }
            }

            for (int x = 0; x < inputImage.GetLength(0); x++)       //stretch bottom
            {
                for (int y = 0; y < offset[1]; y++)
                {
                    tempImage[x + offset[0], y + inputImage.GetLength(1) + offset[1]] = inputImage[x, inputImage.GetLength(1) - 1];
                }
            }

            for (int x = 0; x < offset[0]; x++)
            {
                for (int y = 0; y < inputImage.GetLength(1); y++)   //stretch right
                {
                    tempImage[x + inputImage.GetLength(0) + offset[1], y + offset[0]] = inputImage[inputImage.GetLength(1) - 1, y];
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
                    float val = 0;
                    for (int xf = 0; xf < filter.GetLength(0); xf++)
                    {
                        for (int yf = 0; yf < filter.GetLength(1); yf++)
                        {
                            val += filter[xf, yf] * tempImage[x  + xf, y + yf];
                        }
                    }

                    finalImage[x, y] = (byte)val;
                }
            }

            return finalImage;
        }


        /*
         * medianFilter: apply median filtering on an input image with a kernel of specified size
         * input:   inputImage          single-channel (byte) image
         *          size                length/width of the median filter kernel
         * output:                      single-channel (byte) image
         */
        private byte[,] medianFilter(byte[,] inputImage, byte size)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            if (size % 2 != 1)
                throw new Exception("No definition for even size");

            // I assume that the size is always uneven
            
            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // process all the pixels in the image
            for (int x = 0; x < InputImage.Size.Width; x++) // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++) // loop over rows
                {
                    List<Byte> greyValues = new List<byte>();
                    for (int i = (size - 1 / 2) - size; i <= ((size - 1) / 2); i++)
                    {
                        int Xindex = i + x;
                        if (Xindex < 0)
                            Xindex += InputImage.Size.Width;
                        if (Xindex >= InputImage.Size.Width)
                            Xindex -= InputImage.Size.Width;
                        
                        for (int j = (size - 1 / 2) - size; j <= ((size - 1) / 2); j++)
                        {
                            int Yindex = j + y;
                            if (Yindex < 0)
                                Yindex += InputImage.Size.Height;
                            if (Yindex >= InputImage.Size.Height)
                                Yindex -= InputImage.Size.Height;
                            
                            greyValues.Add(inputImage[Xindex, Yindex]);
                        }
                    }

                    greyValues.Sort();
                    tempImage[x, y] = greyValues[(size - 1) / 2];

                    progressBar.PerformStep(); // increment progress bar
                }
            
            progressBar.Visible = false;      

            return tempImage;
        }

        /*
         * edgeMagnitude: calculate the image derivative of an input image and a provided edge kernel
         * input:   inputImage          single-channel (byte) image
         *          horizontalKernel    horizontal edge kernel
         *          virticalKernel      vertical edge kernel
         * output:                      single-channel (byte) image
         */
        private byte[,] edgeMagnitude(byte[,] inputImage, sbyte[,] horizontalKernel, sbyte[,] verticalKernel)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            
            if(horizontalKernel.GetLength(0) != 3 || horizontalKernel.GetLength(1) != 3)
                throw new Exception("Unexpected horizontal kernel size");
            if(verticalKernel.GetLength(0) != 3 || verticalKernel.GetLength(1) != 3)
                throw new Exception("Unexpected vertical kernel size");
            
            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            for (int x = 0; x < InputImage.Size.Width; x++) // loop over columns
            {
                for (int y = 0; y < InputImage.Size.Height; y++) // loop over rows
                {
                    int DX = 0;
                    int DY = 0;
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

                            DX += horizontalKernel[i + 1, j + 1] * inputImage[xPixel, yPixel];
                            DY += verticalKernel[i + 1, j + 1] * inputImage[xPixel, yPixel];
                        }
                    }

                    tempImage[x, y] = (Byte) (Math.Sqrt(Math.Pow(DX, 2) + Math.Pow(DY, 2)) / 6);
                    progressBar.PerformStep();

                }
            }

            return tempImage;
        }

        /*
         * thresholdImage: threshold a grayscale image
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image with on/off values
         */
        private byte[,] thresholdImage(byte[,] inputImage, int threshold)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            

            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;
            
            
            //Apply threshold
            for (int x = 0; x < inputImage.GetLength(0); x++) // loop over columns
                for (int y = 0; y < inputImage.GetLength(1); y++) // loop over rows
                {
                    if (inputImage[x, y] > threshold)
                        tempImage[x, y] = 255;
                    else
                        tempImage[x, y] = 0;
                }

            return tempImage;
        }
        #endregion

        #region Assignment2
        //All functions for assignment 2 (This comment is added later)

         private byte[,] erodeImage(byte[,] inputImage, List<ElementPoint> strElement)
        {
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            for (int x = 0; x < inputImage.GetLength(0); x++)
            {
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    int lowest = 255;
                    foreach (ElementPoint i in strElement)
                    {
                        if(    x + i.x >= 0 && x + i.x < inputImage.GetLength(0) 
                            && y + i.y >= 0 && y + i.y < inputImage.GetLength(1))
                        {
                            if (inputImage[x + i.x, y + i.y] - i.value < lowest)
                            {
                                lowest = inputImage[x + i.x, y + i.y];
                            }
                        }
                    }
                    if (lowest <= 0) tempImage[x, y] = 0;
                    else tempImage[x, y] = (byte) lowest;
                }
            }
            return tempImage;
        }

        private byte[,] dilateImage(byte[,] inputImage, List<ElementPoint> strElement)
        {
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            for (int x = 0; x < inputImage.GetLength(0); x++)
            {
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    int highest = 0;
                    foreach (ElementPoint i in strElement)
                    {
                        if (    x + i.x >= 0 && x + i.x < inputImage.GetLength(0)
                             && y + i.y >= 0 && y + i.y < inputImage.GetLength(1))
                        {
                            if (inputImage[x + i.x, y + i.y] + i.value > highest)
                            {
                                highest = (inputImage[x + i.x, y + i.y]);
                            }
                        }
                    }
                    if (highest >= 255) tempImage[x, y] = 255;
                    else tempImage[x, y] = (byte)highest;
                }
            }
            return tempImage;
        }
        private byte[,] openImage(byte[,] inputImage, List<ElementPoint> strElement)
        {
            byte[,] tempImage = erodeImage(inputImage, strElement);
            tempImage = dilateImage(tempImage, strElement);
            return tempImage;
        }

        private byte[,] closeImage(byte[,] inputImage, List<ElementPoint> strElement)
        {
            byte[,] tempImage = dilateImage(inputImage, strElement);
            tempImage = erodeImage(tempImage, strElement);
            return tempImage;
        }
        
        struct ElementPoint
        {
            public int x, y;
            public byte value;
        }
        
        private List<ElementPoint> createStructuringElement(char shape, byte size) 
        {
            if (size % 2 != 1) throw new Exception("size can't be even");
            if (size <= 2) throw new Exception("size needs to be at least 3");
            List<ElementPoint> element = new List<ElementPoint>();
            switch (shape)
            {
                case 'p':
                    element.Add(new ElementPoint { x = 0, y = 0, value = 1 });
                    for (int x = -(size / 2); x < size / 2 + 1; x++)
                    {
                        if (x != 0) element.Add(new ElementPoint { x = x, y = 0, value = 1 });
                    }
                    for (int y = -(size / 2); y < size / 2 + 1; y++)
                    {
                        if(y != 0) element.Add(new ElementPoint { x = 0, y = y, value = 1 });
                    }
                    break;
                case 'c':  
                    for (int x = -(size / 2); x < size / 2 + 1; x++)
                    {
                        for (int y = -(size / 2); y < size / 2 + 1; y++)
                        {
                            if (x * x + y + y <= (size / 2) * (size / 2))
                            {
                                element.Add(new ElementPoint { x = x, y = y, value = 1 });
                            }
                        }
                    }
                    break;
                case 's':
                    for (int x =  - (size / 2); x < size / 2 + 1; x++)
                    {
                        for (int y = - (size / 2); y < size / 2 + 1; y++)
                        {
                            element.Add(new ElementPoint { x = x, y = y, value = 1 });
                        }
                    }
                    break;
                default:
                    throw new Exception("shape needs to be either p for plus, or s for square");
            }
            return element;
        }
        private byte[,] andImages(byte[,] I1, byte[,] I2)
        {
            if(I1.GetLength(0) != I2.GetLength(0))
                throw new Exception("The size of the input images does not match in the AND operation");
            if(I1.GetLength(1) != I2.GetLength(1))
                throw new Exception("The size of the input images does not match in the AND operation");
            
            byte[,] result = new byte[I1.GetLength(0), I1.GetLength(1)];
            
            for(int x = 0; x < I1.GetLength(0); x++)
                for (int y = 0; y < I1.GetLength(1); y++)
                {
                    if (I1[x, y] == 0 && I2[x, y] == 0)
                        result[x, y] = 0;
                    else
                        result[x, y] = 255;
                }

            return result;
        }
        
        private byte[,] orImages(byte[,] I1, byte[,] I2)
        {
            if(I1.GetLength(0) != I2.GetLength(0))
                throw new Exception("The size of the input images does not match in the OR operation");
            if(I1.GetLength(1) != I2.GetLength(1))
                throw new Exception("The size of the input images does not match in the OR operation");
            
            byte[,] result = new byte[I1.GetLength(0), I1.GetLength(1)];
            
            for(int x = 0; x < I1.GetLength(0); x++)
                for (int y = 0; y < I1.GetLength(1); y++)
                {
                    if (I1[x, y] == 0 || I2[x, y] == 0)
                        result[x, y] = 0;
                    else
                        result[x, y] = 255;
                }

            return result;
        }

        private Histogram countValues(byte[,] greyScaleImage)
        {
            int[] intensitieCount = new int[256];
            for (int x = 0; x < InputImage.Size.Width; x++) // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++) // loop over rows
                {
                    intensitieCount[greyScaleImage[x, y]] += 1;

                }
            Histogram h = new Histogram();
            h.intensityValues = intensitieCount;

            int uniqueValues = 0;
            
            for (int i = 0; i < 256; i++)
            {
                if (intensitieCount[i] != 0)
                    uniqueValues += 1;
            }

            h.uniqueValues = uniqueValues;
            return h;
        }

        private List<Point> traceBoundary(byte[,] binaryImage)
        {
            Point startingPoint = findStartingPoint(binaryImage);
            
            int dir;
            
            if (startingPoint.X == 0)
                dir = 6;
            else
                dir = 2;

            int xS, yS; // S = Starting point;
            int xT, yT; // T = Successor of the starting point;
            int xP, yP; // P = Previous countour point;
            int xC, yC; // C = Current countour point;

            Point pt = findStartingPoint(binaryImage); //Find a starting point
            List<Point> contour = new List<Point>(); //Declare a list of points

            xS = startingPoint.X; //Declare the starting point
            yS = startingPoint.Y; //Declare the starting point
            contour.Add(pt);

            int[] p = new[] {pt.X, pt.Y};
            
            dir = findNextPoint(dir, p, binaryImage); //Get the next point after the starting point

            xP = xS; //Previous contour point is the starting point
            yP = yS; //Previous contour point is the starting point

            xT = p[0]; //Declare the successor of the starting point
            yT = p[1]; 
            
            xC = p[0]; //Declare the current point
            yC = p[1];

            bool done = xS == xT && yS == yT;

            if(!done)
                contour.Add(new Point(xC, yC));
            
            while (!done)
            {
                if(contour.Count == 40)
                    Console.WriteLine("Hello");
                
                p = new int[]{xC, yC};
                int nDir = (dir + 6) % 8;
                dir = findNextPoint(nDir, p, binaryImage);

                xP = xC; // Save the location of the precious point;
                yP = yC;

                xC = p[0]; // Update the current point
                yC = p[1];

                done = xP == xS && yP == yS || xC == xT && yC == yT;
                if (!done)
                    contour.Add(new Point(p[0], p[1]));
            }


            return contour;
        }

        private Point findStartingPoint(byte[,] binaryImage)
        {
            for (int x = 0; x < binaryImage.GetLength(0); x++) // loop over columns
                for (int y = 0; y < binaryImage.GetLength(1); y++) // loop over rows
                {
                    if(binaryImage[x, y] == 0)
                        return new Point(x, y);
                }
            throw new Exception("No boundry");
        }

        private int findNextPoint(int dir, int[] pt, byte[,] inputImage)
        {
            int[,] dirs = {{1, 0}, {1, 1}, {0, 1}, {-1, 1}, {-1, 0}, {-1, -1}, {0, -1}, {1, -1}};
            for (int i = 0; i < 7; i++)
            {
                int x = pt[0] + dirs[dir,0];
                int y = pt[1] + dirs[dir,1];
                
                //Checking if we sampel out of bounds
                if (x < 0 || x >= inputImage.GetLength(0))
                    continue;
                if (y < 0 || y >= inputImage.GetLength(1))
                    continue;

                if (inputImage[x, y] == 0)
                {
                    dir = (dir + 1) % 8;
                }
                else
                {
                    pt[0] = x;
                    pt[1] = y;
                    break;
                }
            }

            return dir;
        }
        #endregion
        
        // Assignment 3

        public byte[,] houghTransformation(byte[,] binairyImage)
        {
            int lx = binairyImage.GetLength(0);
            int ly = binairyImage.GetLength(1);

            //Declaring these values for faster computation
            int hx = lx / 2; //Half of x and half of y
            int hy = ly / 2;
            
            byte[,] rThetaImage = new byte[180, lx + ly]; // From 100 to -100
            
            for(int x = 0; x < lx; x++)
            {
                for (int y = 0; y < ly; y++)
                {
                    if (binairyImage[x, y] == 255)
                    {
                        int translatedX = x - hx;
                        int translatedY = y - hy;
                        
                        for (int theta = 0; theta < 180; theta += 1)
                        {
                            double r = translatedX * Math.Cos(((double) theta) / (180 * Math.PI)) +
                                       translatedY * Math.Sin(((double) theta) / (180 * Math.PI));
                            rThetaImage[theta / 2, hx + hy + (int) r] += 1;
                        }
                    }
                }    
            }

            return rThetaImage;
        }

        public List<Point> houghPeakFinding(byte[,] inputImage)
        {
            byte[,] rThetaImage = houghTransformation(inputImage);
            
            for (int x = 0; x < rThetaImage.GetLength(0); x++)
            {
                for (int y = 0; y < rThetaImage.GetLength(1); y++)
                {
                    int localValue = rThetaImage[x, y];
                    for (int dx = -1; dx < 2; dx++)
                    {
                        if (x + dx < 0)
                            continue;
                        if (x + dx >= rThetaImage.GetLength(0))
                            continue;
                        for (int dy = -1; dy < 2; dy++)
                        {
                            if (y + dy < 0)
                                continue;
                            if (y + dy >= rThetaImage.GetLength(1))
                                continue;
                            if (rThetaImage[x + dx, y + dy] > localValue)
                                rThetaImage[x, y] = 0;
                        }
                    }
                }
            }

            byte[,] result = thresholdImage(rThetaImage, 200);
            
            List<Point> peaks = new List<Point>();

            for (int x = 0; x < rThetaImage.GetLength(0); x++)
            {
                for (int y = 0; y < rThetaImage.GetLength(1); y++)
                {
                    if(result[x,y] != 0)
                        peaks.Add(new Point(y, 2 * x)); // y = r (distance to the middle of the image) and x is theta / 2
                }
            }

            return peaks;
        }

        public List<Point> houghLineDetection(byte[,] inputImage, Point rThetaPair, int minimumIntensityThreshold, int minimumLength,
            int MaximumGap)
        {
            inputImage = thresholdImage(inputImage, minimumIntensityThreshold);
            
            int r = rThetaPair.X - (inputImage.GetLength(0) / 2) - (inputImage.GetLength(1) / 2) ;
            int theta = rThetaPair.Y;

            int x = inputImage.GetLength(0) / 2 + (int)(r * Math.Cos(((double) theta) / (180 * Math.PI)));
            int y = inputImage.GetLength(1) / 2 + (int)(r * Math.Sin(((double) theta) / (180 * Math.PI)));
            
            double dx = Math.Cos(((double) theta + 90) / (180 * Math.PI));
            double dy = Math.Sin(((double) theta + 90) / (180 * Math.PI));

            int nx = x;
            int ny = x;

            int d = 1;
            
            //Getting the line
            
            List<Point> Line = new List<Point>();
            //Getting the line
            while (nx >= 0 && nx < inputImage.GetLength(0) && ny >= 0 && ny < inputImage.GetLength(1)) 
            {
                Line.Add(new Point(nx, ny));
                d += 1;
                nx = (int)(x + dx * d);
                ny = (int) (y + dy * d);
            }

            Line.Reverse();
            Line.Add(new Point(x, y));
            
            d = -1;
            nx = (int)(x + dx * d);
            ny = (int) (y + dy * d);
            while (nx >= 0 && nx < inputImage.GetLength(0) && ny >= 0 && ny < inputImage.GetLength(1)) 
            {
                Line.Add(new Point(nx, ny));
                d -= 1;
                nx = (int)(x + dx * d);
                ny = (int) (y + dy * d);
            }
            
            int segmentCount = 0;
            int gapCount = 0;
            Point segmentStart = new Point(-1, -1);
            
            List<Point> lineSegments = new List<Point>();
            for (int i = 0; i < Line.Count; i++)
            {
                x = Line[i].X;
                y = Line[i].Y;

                if (inputImage[x, y] == 255 && segmentCount == 0)
                {
                    segmentStart = new Point(x, y);
                    segmentCount++;
                }
                else if (segmentCount != 0 && inputImage[x, y] == 255)
                {
                    segmentCount++;
                    gapCount = 0;
                }
                else if (segmentCount != 0 && inputImage[x, y] == 0)
                {
                    gapCount++;
                    if (gapCount == MaximumGap && segmentCount >= minimumLength) //Maximum gap reached and the segment is long enough
                    {
                        lineSegments.Add(segmentStart);
                        lineSegments.Add(Line[i - MaximumGap]);
                        gapCount = 0;
                        segmentCount = 0;
                    }
                    else if (gapCount == MaximumGap && segmentCount < minimumLength)
                    {
                        gapCount = 0;
                        segmentCount = 0;
                    }
                    else
                    {
                        segmentCount++;
                    }

                }

            }

            return lineSegments;
        }
        
    }
    struct Histogram
    {
        public int[] intensityValues;
        public int uniqueValues;
    }
    
}