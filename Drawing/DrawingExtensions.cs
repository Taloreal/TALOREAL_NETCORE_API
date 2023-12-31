using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TALOREAL_NETCORE_API.Drawing {

    public static class DrawingExtensions {

        /// <summary>
        /// Converts a 4 int tuple to a 4 byte tuple.
        /// </summary>
        /// <param name="pixel">The 4 integers to convert to bytes.</param>
        /// <returns>The 4 integers as bytes.</returns>
        public static (byte a, byte r, byte g, byte b) AsBytes(this (int a, int r, int g, int b) pixel) => 
            ((byte)(pixel.a & 0xff), (byte)(pixel.r & 0xff),
                (byte)(pixel.g & 0xff), (byte)(pixel.b & 0xff));

        /// <summary>
        /// Converts a 4 byte tuple to a 4 int tuple.
        /// </summary>
        /// <param name="pixel">The 4 bytes to convert to integers.</param>
        /// <returns>The 4 bytes as integers.</returns>
        public static (int a, int r, int g, int b) AsInts(this (byte a, byte r, byte g, byte b) pixel) =>
            (pixel.a, pixel.r, pixel.g, pixel.b);

        /// <summary>
        /// Takes a tuple of 4 bytes and combines them int a single 'pixel' color.
        /// </summary>
        /// <param name="pixel">The colors as bytes.</param>
        /// <returns>The resulting color as an uint.</returns>
        public static uint GetPixelUInt(this (byte a, byte r, byte g, byte b) pixel) => 
            ((uint)pixel.a << 24) + ((uint)pixel.r << 16) + ((uint)pixel.g << 8) + ((uint)pixel.b);

        /// <summary>
        /// Takes a tuple of 4 ints and combines them int a single 'pixel' color.
        /// </summary>
        /// <param name="pixel">The colors as ints.</param>
        /// <returns>The resulting color as an uint.</returns>
        public static uint GetPixelUInt(this (int a, int r, int g, int b) pixel) => 
            pixel.AsBytes().GetPixelUInt();

        /// <summary>
        /// Gets the individual colors (alpha, red, green and blue) from an uint color.
        /// </summary>
        /// <param name="color">The uint containing the colors to extract.</param>
        /// <returns>The resulting color as a tuple of 4 bytes.</returns>
        public static (byte a, byte r, byte g, byte b) GetPixelColors(this uint color) => 
            ((byte)(color >> 24), (byte)(color >> 16 & 0xff),
                (byte)(color >> 8 & 0xff), (byte)(color & 0xff));

        /// <summary>
        /// Creates a bitmap from a uint array containing pixel data.
        /// </summary>
        /// <param name="pixels">The pixel data.</param>
        /// <param name="width">The intended width of the bitmap.</param>
        /// <param name="height">The intended height of the bitmap.</param>
        /// <returns>The resulting bitmap.</returns>
        /// <exception cref="ArgumentException">Method will fail if width or height is 0 or less 
        /// or the number of elements isn't the same as the rectangular area.</exception>
        public static Bitmap GenerateBitmap(this uint[] pixels, int width, int height) {
            int size = width * height;
            if (width <= 0 || height <= 0 || size != pixels.Length) {
                throw new ArgumentException("ERROR: Invalid width and or height specified for the uint array.");
            }

            Bitmap bmp = new(width, height, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, bmp.PixelFormat);
            int byteSize = size * 4;
            byte[] bytes = new byte[byteSize];
            Buffer.BlockCopy(pixels, 0, bytes, 0, byteSize);
            Marshal.Copy(bytes, 0, bitmapData.Scan0, byteSize);
            bmp.UnlockBits(bitmapData);
            return bmp;
        }

        /// <summary>
        /// Creates a bitmap from a uint array containing pixel data.
        /// </summary>
        /// <param name="pixels">The pixel data.</param>
        /// <returns>The resulting bitmap.</returns>
        /// <exception cref="ArgumentException">Method will fail if either dimension's length is 0 or less.</exception>
        public static Bitmap GenerateBitmap(this uint[,] pixels) {
            int width = pixels.GetLength(0), height = pixels.GetLength(1);
            if (width <= 0 || height <= 0) {
                throw new ArgumentException("ERROR: Invalid width and or height specified for the uint array.");
            }
            Bitmap bmp = new(width, height, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, bmp.PixelFormat);
            int byteIndex = 0;
            IntPtr scan0 = bitmapData.Scan0;
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    Marshal.Copy(BitConverter.GetBytes(pixels[x, y]), 0, scan0 + byteIndex, 4);
                    byteIndex += 4;
                }
            }
            bmp.UnlockBits(bitmapData);
            return bmp;
        }
    }
}
