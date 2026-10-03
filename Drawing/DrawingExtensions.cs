using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TALOREAL_NETCORE_API.Drawing {

	/// <summary>
	/// Extension methods for working with uint pixel colors, 2D canvases of uints,
	/// bitmaps, and Point/Size arithmetic.
	/// </summary>
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
		/// Sets every element of an array to the same value.
		/// </summary>
		/// <typeparam name="T">The type of element in the array.</typeparam>
		/// <param name="arr">The array to fill.</param>
		/// <param name="value">The value to put in every element.</param>
		public static void Fill<T>(this T[] arr, T value) =>
			arr.For(i => arr[i] = value);

		/// <summary>
		/// Sets every element of a rectangular array to the same value.
		/// </summary>
		/// <typeparam name="T">The type of element in the array.</typeparam>
		/// <param name="arr">The rectangular array to fill.</param>
		/// <param name="value">The value to put in every element.</param>
		public static void Fill<T>(this T[,] arr, T value) {
			for (int x = 0; x < arr.GetLength(0); x++) {
				for (int y = 0; y < arr.GetLength(1); y++) {
					arr[x, y] = value;
				}
			}
		}


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
			if (width > 0 && height > 0 && size == pixels.Length) {
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
			throw new ArgumentException("ERROR: Invalid width and or height specified for the uint array.");
		}

		/// <summary>
		/// Creates a bitmap from a uint array containing pixel data.
		/// </summary>
		/// <param name="pixels">The pixel data.</param>
		/// <returns>The resulting bitmap.</returns>
		/// <exception cref="ArgumentException">Method will fail if either dimension's length is 0 or less.</exception>
		public static Bitmap GenerateBitmap(this uint[,] pixels) {
			int width = pixels.GetLength(0), height = pixels.GetLength(1);
			if (width > 0 && height > 0) {
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
			throw new ArgumentException("ERROR: Invalid width and or height specified for the uint array.");

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
		/// Generates a rectangular array given a specified width, height and a default value.
		/// </summary>
		/// <typeparam name="T">The type of array to generate.</typeparam>
		/// <param name="size">The size (width and height) of the array to generate.</param>
		/// <param name="dft">The default value to fill the array with.</param>
		/// <returns>The newly created and filled array.</returns>
		public static T[,] Generate2DArray<T>(this Size size, T? dft = default) {
			T[,] data = new T[size.Width, size.Height];
			if (dft != null && dft.Equals(default(T)) == false) {
				for (int y = 0; y < size.Height; y++) {
					for (int x = 0; x < size.Width; x++) {
						data[x, y] = dft;
					}
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
		public static List<Point> GetNeighbors(this Point position, Point min, Point max) {
			List<Point> neighbors = new();
			for (int x = position.X - 1; x < position.X + 2; x++) {
				for (int y = position.Y - 1; y < position.Y + 2; y++) {
					if (x >= min.X && y >= min.Y) {
						if (x < max.X && y < max.Y) {
							if (x != position.X || y != position.Y) {
								neighbors.Add(new Point(x, y));
							}
						}
					}
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
		public static List<Point> GetNeighbors(this Point position, uint[,] array, bool excludeSelf = true) {
			List<Point> neighbors = new();
			for (int x = position.X - 1; x < position.X + 2; x++) {
				for (int y = position.Y - 1; y < position.Y + 2; y++) {
					if (array.IsInBounds(new Point(x, y)) == true) {
						if (excludeSelf == false || (x != position.X || y != position.Y)) {
							neighbors.Add(new Point(x, y));
						}
					}
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
		public static double GetSquaredDistance(this Point position, Point goal) =>
			((goal.X - position.X) * (goal.X - position.X)) + ((goal.Y - position.Y) * (goal.Y - position.Y));

		/// <summary>
		/// Draws a line of uints onto a canvas of uints (2D array).
		/// </summary>
		/// <param name="canvas">The 2D uint array to draw on.</param>
		/// <param name="start">The start position of the line.</param>
		/// <param name="end">The end position of the line.</param>
		/// <param name="clr">The uint color to draw the line.</param>
		/// <param name="thickness">How thick should the line be?</param>
		public static void DrawLine(this uint[,] canvas, Point start, Point end, uint clr, int thickness = 1) {
			int length = (int)Math.Round(Math.Sqrt(start.GetSquaredDistance(end)), 0);
			double angle = Math.Atan2(end.Y - start.Y, end.X - start.X);
			for (int i = 0; i <= length; i++) {
				double x = start.X + i * Math.Cos(angle);
				double y = start.Y + i * Math.Sin(angle);
				for (int j = 0; j < thickness; j++) {
					int finalX = (int)Math.Round(x + j * Math.Cos(angle + Math.PI / 2), 0);
					int finalY = (int)Math.Round(y + j * Math.Sin(angle + Math.PI / 2), 0);
					canvas.TrySetValue(new Point(finalX, finalY), clr);
				}
			}
		}

		/// <summary>
		/// Draws a (mostly) horizontal line. (-)
		/// </summary>
		/// <param name="canvas">The 2D uint array to draw on.</param>
		/// <param name="start">The start position of the line.</param>
		/// <param name="end">The end position of the line.</param>
		/// <param name="clr">The uint color to draw the line.</param>
		public static void DrawHorizontalLine(this uint[,] canvas, Point start, Point end, uint clr) {
			int xdif = end.X - start.X, ydif = end.Y - start.Y,
				xabs = Math.Abs(xdif), yabs = Math.Abs(ydif);

			if (xdif != 0 || ydif != 0) { // not at goal.
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
				for (int x = start.X + xintegral, y = start.Y; x != end.X; x += xintegral) {
					int change = decision > 0 ? 1 : 0;
					y += (yintegral * change);
					decision += 2 * (ydif - (xdif * change));
					canvas.TrySetValue(new Point(x, y), clr);
				}
			}
		}

		/// <summary>
		/// Draws a (mostly) vertical lines. (|)
		/// </summary>
		/// <param name="canvas">The 2D uint array to draw on.</param>
		/// <param name="start">The start position of the line.</param>
		/// <param name="end">The end position of the line.</param>
		/// <param name="clr">The uint color to draw the line.</param>
		public static void DrawVerticalLine(this uint[,] canvas, Point start, Point end, uint clr) {
			int xdif = end.X - start.X, ydif = end.Y - start.Y,
				xabs = Math.Abs(xdif), yabs = Math.Abs(ydif);

			if (xdif != 0 || ydif != 0) { // not at goal.
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
				for (int x = start.X, y = start.Y + yintegral; y != end.Y; y += yintegral) {
					int change = decision > 0 ? 1 : 0;
					x += (xintegral * change);
					decision += 2 * (xdif - (ydif * change));
					canvas.TrySetValue(new Point(x, y), clr);
				}
			}
		}

		/// <summary>
		/// Draws the outline of a circle on a canvas of uints.
		/// </summary>
		/// <param name="canvas">The 2D array of uints to draw on.</param>
		/// <param name="pos">The position to draw at.</param>
		/// <param name="radius">The radius of the circle to draw.</param>
		/// <param name="clr">The color to draw the circle.</param>
		public static void DrawOutlinedCircle(this uint[,] canvas, Point pos, int radius, uint clr) {
			int xdif, ydif, distance;
			for (int y = pos.Y - radius - 1; y < pos.Y + radius + 1; y++) {
				for (int x = pos.X - radius - 1; x < pos.X + radius + 1; x++) {
					if (canvas.IsInBounds(new Point(x, y)) == true) {
						xdif = Math.Abs(x - pos.X);
						ydif = Math.Abs(y - pos.Y);
						distance = (xdif * xdif) + (ydif * ydif);
						canvas[x, y] = distance < (radius * radius) + radius &&
							distance > (radius * radius) - radius ?
								clr : canvas[x, y];
					}
				}
			}
		}

		/// <summary>
		/// Draws a filled circle on a canvas of uints.
		/// </summary>
		/// <param name="canvas">The 2D array of uints to draw on.</param>
		/// <param name="pos">The position to draw at.</param>
		/// <param name="radius">The radius of the circle to draw.</param>
		/// <param name="clr">The color to draw the circle.</param>
		public static void DrawFilledCircle(this uint[,] canvas, Point pos, int radius, uint clr) {
			int xdif, ydif, distance;
			int distanceCheck = radius * radius;
			for (int y = pos.Y - radius; y < pos.Y + radius; y++) {
				for (int x = pos.X - radius; x < pos.X + radius; x++) {
					if (canvas.IsInBounds(new Point(x, y)) == true) {
						xdif = Math.Abs(x - pos.X);
						ydif = Math.Abs(y - pos.Y);
						distance = (xdif * xdif) + (ydif * ydif);
						canvas[x, y] = distance <= distanceCheck ? clr : canvas[x, y];
					}
				}
			}
		}

		/// <summary>
		/// Gets the width and height of a rectangular array.
		/// </summary>
		/// <typeparam name="T">The type of element in the array.</typeparam>
		/// <param name="arr">The rectangular array to measure.</param>
		/// <returns>The array's first dimension as the width and its second as the height.</returns>
		public static Size GetSize<T>(this T[,] arr) =>
			new(arr.GetLength(0), arr.GetLength(1));

		/// <summary>
		/// Converts a 1D index into a 2D position, reading row by row.
		/// </summary>
		/// <param name="ndx">The 1D index to convert.</param>
		/// <param name="bounds">The width and height of the 2D area.</param>
		/// <returns>The position, or (-1, -1) if the index is outside the area.</returns>
		public static Point Get2D(this int ndx, Size bounds) {
			int limit = bounds.Width * bounds.Height;
			return new Point(
				(ndx < 0 || ndx >= limit) ? -1 : ndx % bounds.Width,
				(ndx < 0 || ndx >= limit) ? -1 : ndx / bounds.Width);
		}

		/// <summary>
		/// Converts a 2D position into a 1D index, reading row by row.
		/// </summary>
		/// <param name="pos">The 2D position to convert.</param>
		/// <param name="bounds">The width and height of the 2D area.</param>
		/// <returns>The index, or -1 if the position is outside the area.</returns>
		public static int Get1D(this Point pos, Size bounds) =>
			(pos.Y >= bounds.Height || pos.X >= bounds.Width || // too high? too far?
			pos.Y < 0 || pos.X < 0) ? // too low? too close?
				-1 : pos.Y * bounds.Width + pos.X;

		/// <summary>
		/// Converts a size into a point.
		/// </summary>
		/// <param name="size">The size to convert.</param>
		/// <returns>A point with X as the width and Y as the height.</returns>
		public static Point ToPoint(this Size size) =>
			new(size.Width, size.Height);

		/// <summary>
		/// Converts a point into a size.
		/// </summary>
		/// <param name="point">The point to convert.</param>
		/// <returns>A size with the width as X and the height as Y.</returns>
		public static Size ToSize(this Point point) =>
			new(point.X, point.Y);

		/// <summary>
		/// Reverses the byte order of a uint color, so ARGB becomes BGRA.
		/// </summary>
		/// <param name="clr">The color to reverse.</param>
		/// <returns>The color with its four bytes in reverse order.</returns>
		public static uint InvertColor(this uint clr) {
			return ((clr & 0xff000000) >> 24) +
				((clr & 0x00ff0000) >> 8) +
				((clr & 0x0000ff00) << 8) +
				((clr & 0x000000ff) << 24);
		}

		/// <summary>
		/// Blends one uint color over another, using the alpha of the color being applied.
		/// </summary>
		/// <param name="bottom">The color underneath.</param>
		/// <param name="toApply">The color laid on top. Its alpha decides how much of it shows.</param>
		/// <returns>The bottom color when the alpha is 0, the applied color when the alpha is 255,
		/// otherwise every channel (alpha included) mixed by that ratio.</returns>
		public static uint BlendColors(this uint bottom, uint toApply) {
			int alpha = (int)toApply.GetPartialColor(0xff000000, 24);
			uint result = alpha == 0 ? bottom :
				(alpha == 255 ? toApply : 0);
			if (alpha != 0 && alpha != 255) {
				int src, dst, shift;
				float ratio = alpha / 255.0f;
				for (int i = 0; i < 4; i++) {
					shift = i * 8;
					src = (int)toApply.GetPartialColor(255u << shift, shift);
					dst = (int)bottom.GetPartialColor(255u << shift, shift);
					dst = (int)((src - dst) * ratio) + dst;
					result += ((uint)dst << shift);
				}
			}
			return result;
		}

		/// <summary>
		/// Masks part of a uint color and shifts it into place.
		/// </summary>
		/// <param name="baseClr">The color to read from.</param>
		/// <param name="toggles">The bit mask of the part to keep.</param>
		/// <param name="shift">How many bits to shift right. A negative number shifts left.</param>
		/// <returns>The masked and shifted value.</returns>
		public static uint GetPartialColor(this uint baseClr, uint toggles, int shift) {
			uint num = baseClr & toggles;
			num = shift == 0 ? num : // is there a shift?
				(shift < 0 ? // shift left or right?
					num << (shift * -1) :
					num >> shift);
			return num;
		}

		#region Point and Size Arithmetic

		#region Point

		/// <summary>
		/// Adds an offset to a point.
		/// </summary>
		/// <param name="og">The original point.</param>
		/// <param name="offset">The amount to add on each axis.</param>
		/// <returns>The moved point.</returns>
		public static Point Add(this Point og, Point offset) =>
			new(og.X + offset.X, og.Y + offset.Y);

		/// <summary>
		/// Subtracts an offset from a point.
		/// </summary>
		/// <param name="og">The original point.</param>
		/// <param name="offset">The amount to subtract on each axis.</param>
		/// <returns>The moved point.</returns>
		public static Point Minus(this Point og, Point offset) =>
			new(og.X - offset.X, og.Y - offset.Y);

		/// <summary>
		/// Multiplies both axes of a point by a scalar.
		/// </summary>
		/// <param name="og">The original point.</param>
		/// <param name="scalar">The amount to multiply by.</param>
		/// <returns>The scaled point, with any fraction cut off.</returns>
		public static Point Multiply(this Point og, double scalar) =>
			new((int)(og.X * scalar), (int)(og.Y * scalar));

		/// <summary>
		/// Divides both axes of a point by a scalar.
		/// </summary>
		/// <param name="og">The original point.</param>
		/// <param name="scalar">The amount to divide by.</param>
		/// <returns>The scaled point, with any fraction cut off.</returns>
		public static Point Divide(this Point og, double scalar) =>
			new((int)(og.X / scalar), (int)(og.Y / scalar));

		#endregion

		#region Size

		/// <summary>
		/// Adds an offset to a size.
		/// </summary>
		/// <param name="og">The original size.</param>
		/// <param name="offset">The amount to add to the width and height.</param>
		/// <returns>The larger size.</returns>
		public static Size Add(this Size og, Size offset) =>
			new(og.Width + offset.Width, og.Height + offset.Height);

		/// <summary>
		/// Subtracts an offset from a size.
		/// </summary>
		/// <param name="og">The original size.</param>
		/// <param name="offset">The amount to subtract from the width and height.</param>
		/// <returns>The smaller size.</returns>
		public static Size Minus(this Size og, Size offset) =>
			new(og.Width - offset.Width, og.Height - offset.Height);

		/// <summary>
		/// Multiplies the width and height of a size by a scalar.
		/// </summary>
		/// <param name="og">The original size.</param>
		/// <param name="scalar">The amount to multiply by.</param>
		/// <returns>The scaled size, with any fraction cut off.</returns>
		public static Size Multiply(this Size og, double scalar) =>
			new((int)(og.Width * scalar), (int)(og.Height * scalar));

		/// <summary>
		/// Divides the width and height of a size by a scalar.
		/// </summary>
		/// <param name="og">The original size.</param>
		/// <param name="scalar">The amount to divide by.</param>
		/// <returns>The scaled size, with any fraction cut off.</returns>
		public static Size Divide(this Size og, double scalar) =>
			new((int)(og.Width / scalar), (int)(og.Height / scalar));

		#endregion

		#endregion

	}
}
