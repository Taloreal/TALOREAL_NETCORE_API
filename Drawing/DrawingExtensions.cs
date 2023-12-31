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

        /// <summary>
        /// Generates an array of uints given a specified number of elements and a default color.
        /// </summary>
        /// <param name="size">The number of elements to put in the array.</param>
        /// <param name="clr">The color to fill the array with.</param>
        /// <returns>The newly created and filled array.</returns>
        public static uint[] Generate1DArray(int size, uint clr = 0xff000000) {
            uint[] data = new uint[size];
            for (int i = 0; i < size; i++) {
                data[i] = clr;
            }
            return data;
        }

        /// <summary>
        /// Generates an array of uints given a specified width, height and a default color.
        /// </summary>
        /// <param name="width">The width of the array to generate.</param>
        /// <param name="height">The height of the array to generate.</param>
        /// <param name="clr">The color to fill the array with.</param>
        /// <returns>The newly created and filled array.</returns>
        public static uint[,] Generate2DArray(int width, int height, uint clr = 0xff000000) {
            uint[,] data = new uint[width, height];
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    data[x, y] = clr;
                }
            }
            return data;
        }

        /// <summary>
        /// Gets a list of neighboring points (x and y) inside of a 2D bounds.
        /// </summary>
        /// <param name="position">The originating position.</param>
        /// <param name="min">The lower bounds of positions to count as neighbors.</param>
        /// <param name="max">The upper bounds of positions to count as neighbors.</param>
        /// <returns>The list of neighbors to the point.</returns>
        public static List<(int x, int y)> GetNeighbors(this (int x, int y) position, (int x, int y) min, (int x, int y) max) {
            List<(int x, int y)> neighbors = new();
            for (int x = position.x - 1; x < position.x + 2; x++) {
                for (int y = position.y - 1; y < position.y + 2; y++) {
                    if (x < min.x || y < min.y) continue;
                    if (x >= max.x || y >= max.y) continue;
                    if (x == position.x && y == position.y) continue;
                    neighbors.Add((x, y));
                }
            }
            return neighbors;
        }

        /// <summary>
        /// Gets a list of neighboring points (x and y) inside of a 2D array.
        /// </summary>
        /// <param name="position">The originating position.</param>
        /// <param name="array">The array to apply the bounds of.</param>
        /// <param name="excludeSelf">Should we skip the originating position?</param>
        /// <returns>The list of neighbors to the point.</returns>
        public static List<(int x, int y)> GetNeighbors(this (int x, int y) position, uint[,] array, bool excludeSelf = true) {
            List<(int x, int y)> neighbors = new();
            for (int x = position.x - 1; x < position.x + 2; x++) {
                for (int y = position.y - 1; y < position.y + 2; y++) {
                    if (array.IsInBounds((x, y)) == false) continue;
                    if (excludeSelf == true && x == position.x && y == position.y) continue;
                    neighbors.Add((x, y));
                }
            }
            return neighbors;
        }

        /// <summary>
        /// Gets the distance to a point squared (because it's faster and just as easy to use).
        /// </summary>
        /// <param name="position">The originating position.</param>
        /// <param name="goal">The distant point.</param>
        /// <returns>The distance to the distant point.</returns>
        public static double GetSquaredDistance(this (int x, int y) position, (int x, int y) goal) =>
            ((goal.x - position.x) * (goal.x - position.x)) + ((goal.y - position.y) * (goal.y - position.y));

        /// <summary>
        /// Draws a line of uints onto a canvas of uints (2D array).
        /// </summary>
        /// <param name="canvas">The 2D uint array to draw on.</param>
        /// <param name="start">The start position of the line.</param>
        /// <param name="end">The end position of the line.</param>
        /// <param name="clr">The uint color to draw the line.</param>
        /// <param name="thickness">How thick should the line be?</param>
        public static void DrawLine(this uint[,] canvas, (int x, int y) start, (int x, int y) end, uint clr, int thickness = 1) {
            int xdif = end.x - start.x, ydif = end.y - start.y,
                xabs = Math.Abs(xdif), yabs = Math.Abs(ydif);
            bool horizontal = xabs > yabs;

            thickness = Math.Clamp(thickness, 1, int.MaxValue);
            for (int i = 0; i < thickness; i++) {
                int xoffset = horizontal == true ? 0 :
                    (int)Math.Round(i / 2.0, 0) * (i % 2 == 0 ? -1 : 1);
                int yoffset = horizontal == false ? 0 :
                    (int)Math.Round(i / 2.0, 0) * (i % 2 == 0 ? -1 : 1);
                canvas.DrawHorizontalLine(
                    (start.x + xoffset, start.y + yoffset),
                    (end.x + xoffset, end.y + yoffset), clr);
            }
        }

        /// <summary>
        /// Draws a (mostly) horizontal line. (-)
        /// </summary>
        /// <param name="canvas">The 2D uint array to draw on.</param>
        /// <param name="start">The start position of the line.</param>
        /// <param name="end">The end position of the line.</param>
        /// <param name="clr">The uint color to draw the line.</param>
        public static void DrawHorizontalLine(this uint[,] canvas, (int x, int y) start, (int x, int y) end, uint clr) {
            int xdif = end.x - start.x, ydif = end.y - start.y,
                xabs = Math.Abs(xdif), yabs = Math.Abs(ydif);

            if (xdif == 0 && ydif == 0) { return; } // at goal.
            if (xabs < yabs) { // bigger rise than run 
                canvas.DrawVerticalLine(start, end, clr);
                return;
            }
            // xdif can't be 0 in this context now because
            // if it were we'd be at the goal or on a vertical line.
            // therefore no divide by 0 potential.
            int xintegral = xdif / xabs; // will be -1 or +1, which way is x going?
            // we need to check ydif for 0 because no guarantee from the conditions above.
            int yintegral = ydif == 0 ? 0 : ydif / yabs; // -1/+1, which way is y going?
            int decision = 2 * ydif + xdif;

            canvas.TrySetValue(start, clr);
            for (int x = start.x + xintegral, y = start.y; x != end.x; x += xintegral) {
                int change = decision > 0 ? 1 : 0;
                y += (yintegral * change);
                decision += 2 * (ydif - (xdif * change));
                canvas.TrySetValue((x, y), clr);
            }
        }

        /// <summary>
        /// Draws a (mostly) vertical lines. (|)
        /// </summary>
        /// <param name="canvas">The 2D uint array to draw on.</param>
        /// <param name="start">The start position of the line.</param>
        /// <param name="end">The end position of the line.</param>
        /// <param name="clr">The uint color to draw the line.</param>
        public static void DrawVerticalLine(this uint[,] canvas, (int x, int y) start, (int x, int y) end, uint clr) {
            int xdif = end.x - start.x, ydif = end.y - start.y,
                xabs = Math.Abs(xdif), yabs = Math.Abs(ydif);

            if (xdif == 0 && ydif == 0) { return; } // at goal.
            if (xabs > yabs) { // bigger run than rise 
                canvas.DrawHorizontalLine(start, end, clr);
                return;
            }
            // ydif can't be 0 in this context now because
            // if it were we'd be at the goal or on a vertical line.
            // therefore no divide by 0 potential.
            int yintegral = ydif / yabs; // will be -1 or +1, which way is y going?
            // we need to check xdif for 0 because no guarantee from the conditions above.
            int xintegral = xdif == 0 ? 0 : xdif / xabs; // -1/+1, which way is x going?
            int decision = 2 * xdif + ydif;

            canvas.TrySetValue(start, clr);
            for (int x = start.x, y = start.y + yintegral; y != end.y; y += yintegral) {
                int change = decision > 0 ? 1 : 0;
                x += (xintegral * change);
                decision += 2 * (xdif - (ydif * change));
                canvas.TrySetValue((x, y), clr);
            }
        }
    }
}
