using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TALOREAL_NETCORE_API {

	/// <summary>
	/// This class is meant to act as a 2D mask for a 1D array.
	/// Essentially giving all the flexibility of a 2d array (math wise) with the simplistity of a 1D array.
	/// </summary>
	public class Array2DMask<T> {

		/// <summary>
		/// The flat 1D array backing this mask.
		/// </summary>
		public T[] InternalArray { get; protected set; }

		/// <summary>
		/// The total number of elements in the mask (Width * Height).
		/// </summary>
		public int Area => InternalArray.Length;

		/// <summary>
		/// How many x-index positions this mask has.
		/// </summary>
		public int Width { get; protected set; }

		/// <summary>
		/// How many y-index positions this mask has.
		/// </summary>
		public int Height { get; protected set; }


		/// <summary>
		/// Creates an empty mask of the given size, backed by a freshly allocated 1D array.
		/// </summary>
		/// <param name="width">The mask's Width.</param>
		/// <param name="height">The mask's Height.</param>
		public Array2DMask(int width, int height) {
			if (width >= 0 && height >= 0) {
				Width = width;
				Height = height;
				InternalArray = new T[width * height];
				return;
			}
			throw new ArgumentException("ERROR: Invalid specified size.");
		}

		/// <summary>
		/// Wraps an existing 1D array as a mask of the given size.
		/// </summary>
		/// <param name="array">The 1D array to wrap. Must be at least width * height long.</param>
		/// <param name="width">The mask's Width.</param>
		/// <param name="height">The mask's Height.</param>
		public Array2DMask(T[] array, int width, int height) {
			int size = width * height;
			if (width >= 0 && height >= 0 && size <= array.Length) {
				Width = width;
				Height = height;
				InternalArray = array;
				return;
			}
			throw new ArgumentException("ERROR: Invalid specified size.");
		}


		/// <summary>
		/// Gets or sets an element by its flat position in the underlying 1D array.
		/// </summary>
		/// <param name="index">The flat position, 0 to Area - 1.</param>
		public T this[int index] {
			get => InternalArray[index];
			set => InternalArray[index] = value;
		}

		/// <summary>
		/// Gets or sets an element by its 2D position, mapped to the underlying 1D array as x * Height + y.
		/// </summary>
		/// <param name="x">The x position.</param>
		/// <param name="y">The y position.</param>
		public T this[int x, int y] {
			get => this[x * Height + y];
			set => this[x * Height + y] = value;
		}


		/// <summary>
		/// Gives direct access to the mask's underlying 1D array.
		/// </summary>
		/// <param name="array">The mask to unwrap.</param>
		public static implicit operator T[](Array2DMask<T> array) => array.InternalArray;

		/// <summary>
		/// Copies a mask's elements into a new 2D array of the same Width and Height.
		/// </summary>
		/// <param name="array">The mask to convert.</param>
		public static implicit operator T[,](Array2DMask<T> array) {
			T[,] newbie = new T[array.Width, array.Height];
			for (int x = 0; x < array.Width; x++) {
				for (int y = 0; y < array.Height; y++) {
					newbie[x, y] = array[x, y];
				}
			}
			return newbie;
		}

		/// <summary>
		/// Wraps a 1D array as a mask, given its intended Width and Height.
		/// </summary>
		/// <param name="details">The 1D array, Width, and Height to build the mask from.</param>
		public static implicit operator Array2DMask<T>((T[] array, int width, int height) details) =>
			new(details.array, details.width, details.height);

		/// <summary>
		/// Flattens a 2D array into a new mask, one element at a time in the same order the array
		/// itself would enumerate (equivalent to array.Cast&lt;T&gt;().ToArray()).
		/// </summary>
		/// <param name="array">The 2D array to flatten.</param>
		public static implicit operator Array2DMask<T>(T[,] array) {
			int width = array.GetLength(0);
			int height = array.GetLength(1);
			T[] flatArray = new T[width * height];
			int flatIndex = 0;
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					flatArray[flatIndex] = array[x, y];
					flatIndex++;
				}
			}
			return (flatArray, width, height);
		}
	}
}
