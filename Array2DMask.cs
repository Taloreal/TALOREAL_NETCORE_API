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

        public T[] InternalArray { get; protected set; }
        public int Area => InternalArray.Length;
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public T this[int index] {
            get => InternalArray[index];
            set => InternalArray[index] = value;
        }

        public T this[int x, int y] {
            get => this[y * Width + x];
            set => this[y * Width + x] = value;
        }

        public Array2DMask(int width, int height) {
            if (width < 0 || height < 0) {
                throw new ArgumentException("ERROR: Invalid specified size.");
            }

            Width = width; Height = height;
            InternalArray = new T[width * height];
        }

        public Array2DMask(T[] array, int width, int height) {
            int size = width * height;
            if (width < 0 || height < 0 || size > array.Length) {
                throw new ArgumentException("ERROR: Invalid specified size.");
            }

            Width = width; Height = height;
            InternalArray = array;
        }

        public static implicit operator T[](Array2DMask<T> array) => array.InternalArray;
        public static implicit operator T[,](Array2DMask<T> array) {
            T[,] narr = new T[array.Width, array.Height];
            for (int y = 0; y < array.Height; y++) {
                for (int x = 0; x < array.Width; x++) {
                    narr[x, y] = array[x, y];
                }
            }
            return narr;
        }

        public static implicit operator Array2DMask<T>((T[] array, int width, int height) details) =>
            new(details.array, details.width, details.height);
        public static implicit operator Array2DMask<T>(T[,] array) =>
            (array.Cast<T>().ToArray(), array.GetLength(0), array.GetLength(1));
    }
}
