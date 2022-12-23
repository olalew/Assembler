using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizing.Services
{
    public class ImageSizeModifierService
    {
        private readonly Bitmap initialBitmap;
        private readonly Bitmap targetBitmap;

        public ImageSizeModifierService(Bitmap bitmap, int width, int height)
        {
            initialBitmap = bitmap;
            targetBitmap = new Bitmap(width, height);
        }

        public Bitmap ExecuteAlgorithm()
        {
            Rectangle initialRectangle = new Rectangle(0, 0, initialBitmap.Width, initialBitmap.Height);
            Rectangle targetRectangle = new Rectangle(0, 0, targetBitmap.Width, targetBitmap.Height);

            System.Drawing.Imaging.BitmapData initBitmapData =
                initialBitmap.LockBits(initialRectangle, System.Drawing.Imaging.ImageLockMode.ReadWrite, initialBitmap.PixelFormat);

            System.Drawing.Imaging.BitmapData targetBitmapData =
               targetBitmap.LockBits(targetRectangle, System.Drawing.Imaging.ImageLockMode.ReadWrite, targetBitmap.PixelFormat);

            // Get the address of the first line.
            IntPtr initBitmapPointer = initBitmapData.Scan0;
            IntPtr targetBitmapPointer = targetBitmapData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int inputBytesCount = Math.Abs(initBitmapData.Stride) * initialBitmap.Height;
            int outputBytesCount = Math.Abs(targetBitmapData.Stride) * targetBitmap.Height;

            byte[] initImageRGB = new byte[inputBytesCount];
            byte[] targetImageRGB = new byte[outputBytesCount];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(initBitmapPointer, initImageRGB, 0, inputBytesCount);

            // Execute Algorithm
            ExecuteOnBytes(initImageRGB, initialBitmap.Width, initialBitmap.Height,
                targetImageRGB, targetBitmap.Width, targetBitmap.Height);

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(targetImageRGB, 0, targetBitmapPointer, outputBytesCount);

            // Unlock the bits.
            initialBitmap.UnlockBits(initBitmapData);
            targetBitmap.UnlockBits(targetBitmapData);

            // Draw the modified image.
            return targetBitmap;
        }

        private static void ExecuteOnBytes(byte[] initArray, int w1, int h1, byte[] targetArray, int w2, int h2)
        {
            int widthRatio = ((w1 << 16) / w2) + 1;
            int heightRatio = ((h1 << 16) / h2) + 1;

            int x2, y2;
            for (int i = 0; i < h2; i++)
            {
                y2 = (i * heightRatio) >> 16;
                for (int j = 0; j < w2; j++)
                {

                    x2 = (j * widthRatio) >> 16;

                    int sourceIndex = ((y2 * w1) + x2) * 4;
                    int targetIndex = ((i * w2) + j) * 4;

                    /*
                        COPY PIXEL FROM ONE ARRAY TO THE OTHER
                     */
                    targetArray[targetIndex] = initArray[sourceIndex];
                    targetArray[targetIndex + 1] = initArray[sourceIndex + 1];
                    targetArray[targetIndex + 2] = initArray[sourceIndex + 2];
                    targetArray[targetIndex + 3] = initArray[sourceIndex + 3];
                }
            }
        }

        private static void ExecuteOnBytesWithAssebmly(byte[] initArray, int w1, int h1, byte[] targetArray, int w2, int h2)
        {
            int x_ratio = ((w1 << 16) / w2) + 1;
            int y_ratio = ((h1 << 16) / h2) + 1;

            int x2, y2;

            int running = h2;
            AutoResetEvent done = new AutoResetEvent(false);

            for (int i = 0; i < h2; i++)
            {
                y2 = (i * y_ratio) >> 16;

                /*
                    TO ASSEMBLY
                    TASK GOES HERE
                */

                _ = ThreadPool.QueueUserWorkItem((param) =>
                {
                    for (int j = 0; j < w2; j++)
                    {

                        x2 = (j * x_ratio) >> 16;

                        int sourceIndex = ((y2 * w1) + x2) * 4;
                        int targetIndex = ((i * w2) + j) * 4;

                        /*
                            COPY PIXEL FROM ONE ARRAY TO THE OTHER
                         */
                        targetArray[targetIndex] = initArray[sourceIndex];
                        targetArray[targetIndex + 1] = initArray[sourceIndex + 1];
                        targetArray[targetIndex + 2] = initArray[sourceIndex + 2];
                        targetArray[targetIndex + 3] = initArray[sourceIndex + 3];
                    }

                    if (0 == Interlocked.Decrement(ref running))
                    {
                        _ = done.Set();
                    }
                }, i);
            }
            done.WaitOne();
        }
    }

    /*

           for (int i = 0; i < h2; i++)
           {
               for (int j = 0; j < w2; j++)
               {
                   int index = ((i * w2) + j) * 4;

                   targetArray[index] = 0xaa;
                   targetArray[index + 1] = 0xaa;
                   targetArray[index + 2] = 0xaa;
                   targetArray[index + 3] = 0xaa;
               }
           }

        */
}
