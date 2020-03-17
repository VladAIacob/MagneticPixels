/*
 * Author: Iacob Vlad Alexandru
 * 
 * Program: It takes an camera input and shows a representation of the received stream
 * in a form of magnetic pixels that sticks to the edge of the objects in the image
 * 
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Threading;
using System.Drawing.Imaging;

namespace MagneticPixels
{
    public partial class Form1 : Form
    {
        //Variables for camera only.
        private FilterInfoCollection videoInput;
        private VideoCaptureDevice camera;

        //Variables for threads.
        private volatile bool working = false;
        private volatile bool running = false;
        private Bitmap workImage;
        private Thread workingThread;

        //Hard-coded filters
        private double[,] gausianfilter5x5 =
        {
            { 1.0/273, 4.0/273,  7.0/273,  4.0/273,  1.0/273 },
            { 4.0/273, 16.0/273, 26.0/273, 16.0/273, 4.0/273 },
            { 7.0/273, 26.0/273, 41.0/273, 26.0/273, 7.0/273 },
            { 4.0/273, 16.0/273, 26.0/273, 16.0/273, 4.0/273 },
            { 1.0/273, 4.0/273,  7.0/273,  4.0/273,  1.0/273 }
        };

        private double[,] gausianfilter3x3 =
        {
            { 1.0/16, 2.0/16, 1.0/16 },
            { 2.0/16, 4.0/16, 2.0/16 },
            { 1.0/16, 2.0/16, 1.0/16 }
        };

        private double[,] sobelKernelX =
        {
            { -1.0, 0.0, 1.0 },
            { -2.0, 0.0, 2.0 },
            { -1.0, 0.0, 1.0 }
        };

        private double[,] sobelKernelY =
        {
            { 1.0,  2.0,  1.0 },
            { 0.0,  0.0,  0.0 },
            {-1.0, -2.0, -1.0 }
        };

        public Form1()
        {
            InitializeComponent();
        }

        //This starts the thread responsible for the "right image".
        private void StartWorkingThread()
        {
            if(!running)
            {
                running = true;
                workingThread = new Thread(new ThreadStart(Work));
                workingThread.Start();
            }
        }

        //This stops the thread.
        private void StopWorkingThread()
        {
            running = false;
            //if (workingThread != null && workingThread.IsAlive)
            //    workingThread.Join();
        }

        //This algorithm returns a matrix that is formed by applying the convolution operator 
        //to the original image and the filter.
        private double[,] ApplyFilter(double [,] matrix, double [,] filter, int filterSize)
        {
            int width = workImage.Width;
            int height = workImage.Height;

            double[,] newMatrix = new double[height, width];
            
            //for each row / column
            for (int row = 0; row < height; row++)
            {
                for(int column = 0; column < width; column++)
                {
                    double value = 0.0;
                    int filterYindex = row - filterSize / 2;
                    //calculate the elementwise multiplication of the original matrix segment and the filter
                    for (int filterRow = 0; filterRow < filterSize; filterRow++)
                    {
                        int filterXindex = column - filterSize / 2;
                        for (int filterColumn = 0; filterColumn < filterSize; filterColumn++)
                        {    
                            //this does not fire if the segment is not the right size. i.e it goes out of bounds.
                            if (filterYindex > 0 && filterXindex > 0 && filterYindex < height
                                && filterXindex < width)
                            {
                                value += matrix[filterYindex, filterXindex] * filter[filterRow, filterColumn];
                            }
                            filterXindex++;
                        }
                        filterYindex++;
                    }
                    newMatrix[row, column] = value;
                }
            }
            return newMatrix;
        }

        //This returns a grayscale image representation from the original 
        private Bitmap MakeGrayscale3(Bitmap original)
        {
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                //these values are the standard coefficients for ARGB to grayscale representation
                ColorMatrix colorMatrix = new ColorMatrix(
                    new float[][]
                    {
                        new float[] {.3f, .3f, .3f, 0, 0},
                        new float[] {.59f, .59f, .59f, 0, 0},
                        new float[] {.11f, .11f, .11f, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0, 0, 0, 0, 1}
                    });

                using (ImageAttributes attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix);
                    g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            return newBitmap;
        }

        //This makes a double[,] from a image.
        private double[,] BitmapToMatrix(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            double[,] matrix = new double[height, width];

            for(int row = 0; row < height; row++)
            {
                for(int column = 0; column < width; column++)
                {
                    matrix[row, column] = image.GetPixel(column, row).B;
                }
            }
            return matrix;
        }

        //This makes a image from a double[,]
        private Bitmap MatrixToBitmap(double [,] matrix)
        {
            int width = workImage.Width;
            int height = workImage.Height;
            int stride = width * 4;
            int[,] data = new int[height, width];

            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    byte color = (byte)matrix[row, column];
                    byte[] bgra = new byte[] { color, color, color, 255 };
                    data[row, column] = BitConverter.ToInt32(bgra, 0);
                }
            }

            Bitmap bitmap;
            unsafe
            {
                fixed (int* intPtr = &data[0, 0])
                {
                    bitmap = new Bitmap(width, height, stride, PixelFormat.Format32bppRgb, new IntPtr(intPtr));
                }
            }
            return bitmap;
        }

        //This makes sure that the returned matrix, matrix1 and matrix2 form a carthesian triple for each element [i,j]. 
        private double[,] MatrixHypot(double [,] matrix1, double [,] matrix2)
        {
            int width = workImage.Width;
            int height = workImage.Height;
            double[,] matrix = new double[height, width];
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    double a = matrix1[row, column];
                    double b = matrix2[row, column];
                    matrix[row, column] = Math.Sqrt(a * a + b * b);
                }
            }
            return matrix;
        }

        //This brings the values of the matrix to [0,255]
        private double[,] NormalizeMatrix(double [,] matrix)
        {
            int width = workImage.Width;
            int height = workImage.Height;

            double max = matrix[0, 0];

            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    double value = matrix[row, column];
                    if (max < value)
                        max = value;
                }
            }

            if (max == 0)
                max = 0.01;

            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    matrix[row, column] = (Math.Abs(matrix[row, column]) / max) * 255;
                }
            }
            return matrix;
        }

        //This is responsible for drawing one bar on the screen
        public void DrawPixel(Graphics drawer, Pen pen, double x, double y, double angle, int size)
        {
            double deltaX = Math.Cos(angle) * size;
            double deltaY = Math.Sin(angle) * size;

            Point p1 = new Point((int)(x + deltaX), (int)(y + deltaY));
            Point p2 = new Point((int)(x - deltaX), (int)(y - deltaY));

            drawer.DrawLine(pen, p1, p2);
        }

        //This draws all the bars on the screen
        private void DrawTargets(Graphics drawer, Pen pen, double [,] matrix,
                                 double [,] angle, int size)
        {
            int width = workImage.Width;
            int height = workImage.Height;

            for(int row = 0; row < height; row += size)
            {
                for(int column = 0; column < width; column += size)
                {
                    if(matrix[row, column] > 10)
                    {
                        DrawPixel(drawer, pen, column, row, angle[row, column], size);
                    }
                }
            }
        }

        //This calculates the angle of the enges.
        private double[,] GetGradient(double [,] matrix, double [,] intensityX, double [,] intensityY)
        {
            int width = workImage.Width;
            int height = workImage.Height;

            double[,] gradient = new double[height, width];

            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    double value = Math.Atan2(intensityY[row, column], intensityX[row, column]);
                    gradient[row, column] = value;           
                }
            }

            return gradient;
        }

        //This is the method called in a separate thread.
        private void Work()
        {
            int width = editedImage.Width;
            int height = editedImage.Height;

            using (Bitmap background = new Bitmap(width, height))
            {
                using (Graphics drawer = Graphics.FromImage(background))
                {
                    Pen pen = new Pen(Color.Black);
                    int size = 5;

                    while (running)
                    {
                        if (working)
                        {
                            drawer.Clear(this.BackColor);

                            double[,] matrix = BitmapToMatrix((Bitmap)MakeGrayscale3(workImage));
                            matrix = ApplyFilter(matrix, gausianfilter5x5, 5);

                            double[,] intensityX = ApplyFilter((double[,])matrix.Clone(), sobelKernelX, 3);
                            double[,] intensityY = ApplyFilter(matrix, sobelKernelY, 3);

                            matrix = MatrixHypot(intensityX, intensityY);

                            matrix = NormalizeMatrix(matrix);

                            double[,] gradient = GetGradient(matrix, intensityX, intensityY);

                            DrawTargets(drawer, pen, matrix, gradient, size);

                            //editedImage.Image = MatrixToBitmap(matrix);  //if you want to look the cannyEdgeImage
                            editedImage.Image = (Bitmap)background.Clone();
                            working = false;
                        }
                    }
                }     
            }          
        }

        //Called after the constructor, it gets the cameras information
        private void Form1_Load(object sender, EventArgs e)
        {
            videoInput = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in videoInput)
                videoSource.Items.Add(device.Name);
            videoSource.SelectedIndex = 0;
        }

        //This starts the camera
        private void startButton_Click(object sender, EventArgs e)
        {
            StartWorkingThread();
            camera = new VideoCaptureDevice(videoInput[videoSource.SelectedIndex].MonikerString);
            camera.NewFrame += new NewFrameEventHandler(camera_NewFrame);
            camera.Start();
        }

        //This method is called wenever a new video frame is captured
        private void camera_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap image = new Bitmap(eventArgs.Frame, originalImage.Width, originalImage.Height);
            originalImage.Image = image;
            if (!working && running)
            {
                workImage = (Bitmap)image.Clone();
                working = true;
            }
                
        }

        //This stops the camera, if it is running
        private void stopButton_Click(object sender, EventArgs e)
        {
            StopWorkingThread();
            if (camera != null && camera.IsRunning)
                camera.Stop();
        }

        //This stops the application from running.
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopWorkingThread();
            if (camera != null && camera.IsRunning)
                camera.Stop();
        }
    }
}
