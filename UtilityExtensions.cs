using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace TALOREAL_NETCORE_API {

    public enum ForLoopDirection { 
        Forward, Backward
    }

    public enum LinkedListSide { 
        Front, Back
    }

    public static class UtilityExtensions {

        #region Array Extensions

        /// <summary>
        /// Serves as a one stop function to get all of type T items from the array.
        /// </summary>
        /// <typeparam name="T">The type to get all items of.</typeparam>
        /// <param name="arr">The array to get the items from.</param>
        /// <returns>A traditionally typed T[] array with indexers.</returns>
        public static T[] AsTypedArray<T>(this Array arr) {
            List<T> asList = new();
            foreach (object? item in arr) {
                if (item is T t && item != null) { 
                    asList.Add(t);
                }
            }
            return asList.ToArray();
        }

        /// <summary>
        /// Removes a specific index from an array and resizes the array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="arr">The array to remove from.</param>
        /// <param name="index">The index to remove from the array.</param>
        /// <returns>The newly resized array.</returns>
        public static T[] RemoveAt<T>(this T[] arr, int index) {
            if (index >= 0 && index < arr.Length) {
                int newIndex = 0;
                T[] copy = new T[arr.Length - 1];
                for (int i = 0; i < arr.Length; i++) {
                    if (i != index) {
                        copy[newIndex] = arr[i];
                        newIndex++;
                    }
                }
                return copy;
            }
            return arr;
        }

        /// <summary>
        /// Gets a subset array from another array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="arr">The original array.</param>
        /// <param name="start">The starting index.</param>
        /// <param name="count">The number of elements to copy.</param>
        /// <returns>The subset array.</returns>
        public static T[] SubArray<T>(this T[] arr, int start, int count = -1) {
            start = (start < 0) ? 0 : start;
            count = (start + count >= arr.Length) ? arr.Length - start : count;
            if (count < 1) { return Array.Empty<T>(); }
            List<T> objs = new(count);
            for (int i = 0; i < count; i++) {
                int pos = i + start;
                objs.Add(arr[pos]);
            }
            return objs.ToArray();
        }

        /// <summary>
        /// Performs a delegate on each element in an array.
        /// </summary>
        /// <typeparam name="T">The type of array to use.</typeparam>
        /// <param name="arr">The array to perform the delegate on.</param>
        /// <param name="toDo">The delegate to perform on each element.</param>
        public static void ForEach<T>(this T[] arr, Action<T, int> toDo) {
            for (int i = 0; i < arr.Length; i++) {
                toDo(arr[i], i);
            }
        }

        /// <summary>
        /// Performs a delegate on each element in an array.
        /// </summary>
        /// <typeparam name="T">The type of array to use.</typeparam>
        /// <param name="arr">The array to perform the delegate on.</param>
        /// <param name="toDo">The delegate to perform on each element.</param>
        public static void ForEach<T>(this T[] arr, Action<T> toDo) {
            for (int i = 0; i < arr.Length; i++) {
                toDo(arr[i]);
            }
        }

        /// <summary>
        /// Performs a delegate for each element in an array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="arr">The array to perform the delegate through.</param>
        /// <param name="toDo">The delegate to perform.</param>
        /// <param name="direction">The direction, either forward or backward, to process the array.</param>
        public static void For<T>(this T[] arr, Action<int> toDo, ForLoopDirection direction = ForLoopDirection.Forward) {
            int start = direction == ForLoopDirection.Forward ? 0 : arr.Length - 1;
            int end = direction == ForLoopDirection.Forward ? arr.Length : -1;
            int iterator = direction == ForLoopDirection.Forward ? 1 : -1;
            for (int i = start; direction == ForLoopDirection.Forward ? i < end : i > end; i += iterator) {
                toDo(i);
            }
        }

        /// <summary>
        /// Creates a copy of an array that is smaller than the original.
        /// </summary>
        /// <typeparam name="T">The type of array to shrink.</typeparam>
        /// <param name="arr">The array to shrink..</param>
        /// <param name="maxSize">The number of elements to copy over.</param>
        /// <param name="shrinkDirection">The direction in which to copy elements. Forward = Prioritize the "front" of the array.
        /// Backward = Prioritize the "back" of the array.</param>
        /// <returns>The new, shrunk, array.</returns>
        public static T[] ShrinkArray<T>(this T[] arr, int maxSize, ForLoopDirection shrinkDirection = ForLoopDirection.Forward) {
            if (arr.Length <= maxSize) { return arr; }
            T[] newArray = new T[maxSize];
            int integral = shrinkDirection == ForLoopDirection.Forward ? 1 : -1;
            int cpi = shrinkDirection == ForLoopDirection.Forward ? 0 : maxSize - 1;

            void copyElement(int ogi) {
                if (cpi > -1 && cpi < newArray.Length) {
                    newArray[cpi] = arr[ogi];
                    cpi += integral;
                }
            }

            arr.For(copyElement, shrinkDirection);
            return newArray;
        }

        /// <summary>
        /// Creates a copy of an array that is bigger than the original.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="arr">The array to grow.</param>
        /// <param name="minSize">The size of the new array.</param>
        /// <param name="growDirection">The direction in which to add the new elements.</param>
        /// <returns>The new, bigger, array.</returns>
        public static T[] GrowArray<T>(this T[] arr, int minSize, ForLoopDirection growDirection = ForLoopDirection.Forward) {
            if (arr.Length >= minSize) { return arr; }

            T[] newArray = new T[minSize];
            int integral = growDirection == ForLoopDirection.Forward ? 1 : -1;
            int cpi = growDirection == ForLoopDirection.Forward ? 0 : minSize - 1;

            void copyElement(int ogi) { 
                newArray[cpi] = arr[ogi]; 
                cpi += integral; 
            }

            arr.For(copyElement, growDirection);
            return newArray;
        }

        /// <summary>
        /// Checks if the elements in two arrays are the same.
        /// </summary>
        /// <typeparam name="T">The types of arrays to check.</typeparam>
        /// <param name="arr">The original array.</param>
        /// <param name="other">The array to check against.</param>
        /// <returns>True if all values are the same, otherwise false.</returns>
        public static bool SameValues<T>(this T[] arr, T[] other) {
            if (arr.Length != other.Length) { return false; }
            for (int i = 0; i < arr.Length; i++) {
                if (arr[i] == null && other[i] == null) { continue; }
                if (arr[i] == null || other[i] == null) { return false; }
                if (arr[i]!.Equals(other[i]) == false) { return false; }
            }
            return true;
        }

        /// <summary>
        /// Create a copy of an array and removes all elements matching a predicate.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="arr">The array to perform the removal on.</param>
        /// <param name="predicate">The condition to use for to check for removal.</param>
        /// <returns>The new array, with the matching elements removed.</returns>
        public static T[] RemoveAllTrue<T>(this T[] arr, Predicate<T> predicate) {
            bool opposite(T arg) { return predicate(arg) == false; }
            return RemoveAllFalse(arr, opposite);
        }

        /// <summary>
        /// Create a copy of an array and removes all elements not matching a predicate.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="arr">The array to perform the removal on.</param>
        /// <param name="predicate">The condition to use for to check for removal.</param>
        /// <returns>The new array, with the non-matching elements removed.</returns>
        public static T[] RemoveAllFalse<T>(this T[] arr, Predicate<T> predicate) {
            int count = 0;
            T[] filtered = new T[arr.Length];
            
            void copyElement(int ogi) {
                if (predicate(arr[ogi]) == true) {
                    filtered[count] = arr[ogi];
                    count += 1;
                }
            }

            arr.For(copyElement, ForLoopDirection.Forward);
            filtered.ShrinkArray(count, ForLoopDirection.Forward);
            return filtered;
        }

        /// <summary>
        /// Checks if the array contains all of the elements of another array.
        /// </summary>
        /// <typeparam name="T">The type of arrays to work with.</typeparam>
        /// <param name="array">The containing array to check.</param>
        /// <param name="subset">The array of elements to check.</param>
        /// <returns>True if all of subset is in the array.</returns>
        public static bool ContainsArray<T>(this T?[] array, T?[] subset) {
            if (array.Length < subset.Length) { return false; }

            int first = 0, last = subset.Length - 1;
            bool[] same = new bool[subset.Length];
            for (int i = 0; i < array.Length; i++) {
                for (int j = first; j <= last; j++) {
                    bool s = array[i] == null && subset[j] == null;
                    if (s == false && (array[i] == null || subset[j] == null)) { continue; }
                    if (s == true || array[i]!.Equals(subset[j])) {
                        same[j] = true;
                        if (j == first) { first += 1; }
                        if (j == last) { last -= 1; }
                        break;
                    }
                }
            }
            for (int i = 0; i < same.Length; i++) {
                if (same[i] == false) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Takes a 2D (rectangular) array and turns it into a 1D array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="arr">The array to transform into a 1D array.</param>
        /// <returns>The resulting 1D array.</returns>
        public static T[] To1DArray<T>(this T[,] arr) =>
            (T[])((Array2DMask<T>)(arr));

        /// <summary>
        /// Takes a 1D array and turns it into a 2D (rectangular) array.
        /// Caution, will take as long as it would moving each element one at a time.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="arr">The array to transform into a 2D array.</param>
        /// <param name="width">The width of the 2D array.</param>
        /// <param name="height">The height of the 2D array.</param>
        /// <returns>The resulting 2D array.</returns>
        public static T[,] To2DArray<T>(this T[] arr, int width, int height) =>
            (T[,])((Array2DMask<T>)(arr, width, height));


        /// <summary>
        /// Checks if a position is within the bounds of an array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The array to access.</param>
        /// <param name="position">The position at which to check.</param>
        /// <returns>True if the position is within the array's bounds.</returns>
        public static bool IsInBounds<T>(this T[,] array, (int x, int y) position) {
            var (min, max) = array.GetBounds();
            return position.x >= min.x && position.x < max.x &&
                position.y >= min.y && position.y < max.y;
        }

        /// <summary>
        /// Checks if a position is within the bounds of an array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The array to access.</param>
        /// <param name="position">The position at which to check.</param>
        /// <returns>True if the position is within the array's bounds.</returns>
        public static bool IsInBounds<T>(this T[] array, int position) => position >= 0 && position < array.Length;

        /// <summary>
        /// Gets the lower and upper bounds of the array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The array to access.</param>
        /// <returns>((0, 0), (array.GetLength(0), array.GetLength(1)))</returns>
        public static ((int x, int y) min, (int x, int y) max) GetBounds<T>(this T[,] array) =>
            ((0, 0), (array.GetLength(0), array.GetLength(1)));

        /// <summary>
        /// Gets the lower and upper bounds of the array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The array to access.</param>
        /// <returns>(0, array.Length)</returns>
        public static (int min, int max) GetBounds<T>(this T[] array) => (0, array.Length);

        /// <summary>
        /// Tries to set the element at position to a new value.
        /// </summary>
        /// <typeparam name="T">The type of element to set.</typeparam>
        /// <param name="array">The array to access.</param>
        /// <param name="position">The position at which to change.</param>
        /// <param name="value">The new value to set in the array.</param>
        /// <returns>True if the position's element was updated.</returns>
        public static bool TrySetValue<T>(this T[,] array, (int x, int y) position, T value) {
            if (array.IsInBounds(position)) {
                array[position.x, position.y] = value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to set the element at position to a new value.
        /// </summary>
        /// <typeparam name="T">The type of element to set.</typeparam>
        /// <param name="array">The array to access.</param>
        /// <param name="position">The position at which to change.</param>
        /// <param name="value">The new value to set in the array.</param>
        /// <returns>True if the position's element was updated.</returns>
        public static bool TrySetValue<T>(this T[] array, int position, T value) {
            if (array.IsInBounds(position)) {
                array[position] = value;
                return true;
            }
            return false;
        }

        #endregion

        #region String Parsing

        /// <summary>
        /// Splits a string by another string.
        /// Emulates the .net Core method. :)
        /// </summary>
        /// <param name="toSplit">The string to split.</param>
        /// <param name="delimiter">The string to split by.</param>
        /// <param name="strOps">Get rid of empty strings?</param>
        /// <returns>The string split into an array of strings.</returns>
        public static string[] Split(this string toSplit, string delimiter, StringSplitOptions strOps = StringSplitOptions.None) {
            if (toSplit.Length < delimiter.Length) { return new string[] { toSplit }; }
            List<string> entries = new();
            string working = "";
            for (int i = 0; i < toSplit.Length; i++) {
                working += toSplit[i];
                if (working.EndsWith(delimiter)) {
                    entries.Add(working.Substring(0, working.Length - delimiter.Length));
                    working = "";
                }
            }
            if (working != "") { entries.Add(working); }
            if (strOps == StringSplitOptions.RemoveEmptyEntries) {
                entries.RemoveAll(s => string.IsNullOrEmpty(s));
            }
            return entries.ToArray();
        }

        /// <summary>
        /// Attempts to parse a bool from a string.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="result">The resulting bool.</param>
        /// <returns>Did the parsing work?</returns>
        public static bool TryParseBool(string str, out bool result) {
            result = false;
            str = str.ToLower();
            if (string.IsNullOrEmpty(str)) { return false; }
            if (str[0] == '0' || str.ToLower().StartsWith("false")) { return true; }
            if (str[0] == '1' || str.ToLower().StartsWith("true")) {
                result = true; return true;
            }
            if (str.StartsWith("no") || str == "n") { return true; }
            if (str.StartsWith("yes") || str.StartsWith("yea") || str == "y") {
                result = true; return true;
            }
            return false;
        }

        /// <summary>
        /// Filters a string based on what characters are allowed.
        /// </summary>
        /// <param name="original">The string to filter.</param>
        /// <param name="filter">The characters allowed in the result.</param>
        /// <param name="areAllowed">Determines if the filter is the allowed or not allowed characters.</param>
        /// <returns>The resulting filtered string.</returns>
        public static string Filter(this string original, string filter, bool areAllowed = true) {
            string filtered = "";
            foreach (char c in original) {
                if (filter.Contains(c) == areAllowed) { 
                    filtered += c;
                }
            }
            return filtered;
        }

        /// <summary>
        /// Converts a string into a unicode formatted byte array (2 bytes/char).
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>A byte array representation of the string.</returns>
        public static byte[] GetBytes(this string str) {
            byte[] arr = new byte[str.Length * 2];
            for (int i = 0; i < str.Length; i++) {
                byte[] one = BitConverter.GetBytes(str[i]);
                arr[(i * 2) + 0] = one[0];
                arr[(i * 2) + 1] = one[1];
            }
            return arr;
        }

        #endregion

        #region Byte Array Extensions

        /// <summary>
        /// Left shifts the bits in a byte array continuously.
        /// </summary>
        /// <param name="data">The bits to left shift.</param>
        /// <param name="shift">The amount to shift the bits over by.</param>
        /// <returns>The newly left shifted bits as a byte array.</returns>
        public static byte[] LeftShift(this byte[] data, short shift) {
            byte[] nData = new byte[data.Length];
            string binary = data.GetBinaryString(false);
            binary = binary.PadRight(shift + binary.Length, '0');
            for (int i = 0; i < nData.Length; i++) {
                nData[i] = Convert.ToByte(binary.Substring(shift + (i * 8), 8), 2);
            }
            return nData;
        }

        /// <summary>
        /// Right shifts the bits in a byte array continuously.
        /// </summary>
        /// <param name="data">The bits to Right shift.</param>
        /// <param name="shift">The amount to shift the bits over by.</param>
        /// <returns>The newly Right shifted bits as a byte array.</returns>
        public static byte[] RightShift(this byte[] data, short shift) {
            byte[] nData = new byte[data.Length];
            string binary = data.GetBinaryString(false);
            binary = binary.PadLeft(shift + binary.Length, '0');
            for (int i = 0; i < nData.Length; i++) {
                nData[i] = Convert.ToByte(binary.Substring(i * 8, 8), 2);
            }
            return nData;
        }

        /// <summary>
        /// Prints the bits of a byte array to a string in base 2.
        /// </summary>
        /// <param name="data">The byte array to print.</param>
        /// <param name="csv">Should the bytes be comma seperated?</param>
        /// <returns>The resulting bits as a string.</returns>
        public static string GetBinaryString(this byte[] data, bool csv = true) {
            string binary = "";
            for (int i = 0; i < data.Length; i++) {
                binary += Convert.ToString(data[i], 2).PadLeft(8, '0') + (csv == true ? ", " : "");
            }
            return binary;
        }

        /// <summary>
        /// Converts a unicode formated (2 bytes/char) byte array to a string.
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>The string representation of the byte array.</returns>
        public static string GetString(this byte[] bytes) {
            string buffer = "";
            for (int i = 0; i < bytes.Length - 1; i += 2) {
                buffer += BitConverter.ToChar(bytes, i);
            }
            return buffer;
        }

        /// <summary>
        /// Reads a char from a byte array starting at a specified index and advances the index by 2.
        /// </summary>
        /// <param name="data">The array to read from.</param>
        /// <param name="readIndex">The current offset inside of the byte array.</param>
        /// <param name="result">The resulting character, if the current offset was invalid than this value is '\0'.</param>
        /// <returns>True if a char was successfully read from the byte array, false otherwise.</returns>
        public static bool ReadCharFromBytes(this byte[] data, ref int readIndex, out char result) {
            result = '\0';
            if (readIndex >= 0 && readIndex <= data.Length - 2) {
                result = BitConverter.ToChar(data, readIndex);
                readIndex += 2;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reads a string from a byte array starting at a specified index and advances the index by the 
        /// expected number of characters * 2 (unicode char length in bytes.)
        /// </summary>
        /// <param name="arr">The byte array to read from.</param>
        /// <param name="curOffset">The current offset inside the byte array.</param>
        /// <param name="expectedLength">The expected length of the string in characters.</param>
        /// <param name="result">The resulting string, if the current offset was invalid, this value is an empty string.</param>
        /// <returns>True if the string was successfully read from the byte array, false otherwise.</returns>
        public static bool ReadStringFromBytes(this byte[] arr, ref int curOffset, int expectedLength, out string result) {
            result = "";
            if (curOffset >= 0 && curOffset <= arr.Length - (expectedLength * 2)) {
                result = arr.SubArray(curOffset, expectedLength * 2).GetString();
                curOffset += expectedLength * 2;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reads an int from a byte array starting at a specified index and advances the index by 4.
        /// </summary>
        /// <param name="data">The array to read from.</param>
        /// <param name="readIndex">The current offset inside of the byte array.</param>
        /// <param name="result">The resulting integer, if the current offset was invalid than this value is int.MinValue.</param>
        /// <returns>True if an int was successfully read from the byte array, false otherwise.</returns>
        public static bool ReadIntFromBytes(this byte[] data, ref int readIndex, out int result) {
            result = int.MinValue;
            if (readIndex >= 0 && readIndex <= data.Length - 4) {
                result = BitConverter.ToInt32(data, readIndex);
                readIndex += 4;
                return true;
            }
            return false;
        }

        #endregion

        #region Randomness Extensions

        /// <summary>
        /// Gets a random bool, true or false.
        /// </summary>
        /// <param name="rand">The Randomizing object.</param>
        /// <returns>The random bool.</returns>
        public static bool NextBool(this Random rand) {
            return rand.Next(0, 2) == 1;
        }

        /// <summary>
        /// Gets a random byte (0-256).
        /// </summary>
        /// <param name="rand">The Randomizing object.</param>
        /// <returns>The random byte.</returns>
        public static byte NextByte(this Random rand) {
            return (byte)rand.Next(0, 256);
        }

        /// <summary>
        /// Gets a random byte between a inclusive lower and exclused upper bounds.
        /// </summary>
        /// <param name="rand">The Randomizing object.</param>
        /// <param name="min">The inclusive lower bounds.</param>
        /// <param name="max">The excluded upper bounds.</param>
        /// <returns>The random byte.</returns>
        public static byte NextByte(this Random rand, byte min, byte max) {
            if (min == max) { return min; }
            if (min > max) {
                (min, max) = (max, min);
            }
            return (byte)rand.Next(min, max);
        }

        /// <summary>
        /// Gets a random short (2 byte integer).
        /// </summary>
        /// <param name="rand">The Randomizing object.</param>
        /// <returns>The random short.</returns>
        public static short NextShort(this Random rand) {
            return (short)rand.Next(short.MinValue, short.MaxValue);
        }

        /// <summary>
        /// Gets a random short (2 byte integer) between an inclusive lower bounds and excluded upper bounds.
        /// </summary>
        /// <param name="rand">The Randomizing object.</param>
        /// <param name="min">The inclusive lower bounds.</param>
        /// <param name="max">The excluded upper bounds.</param>
        /// <returns>The random short.</returns>
        public static short NextShort(this Random rand, short min, short max) {
            if (min == max) { return min; }
            if (min > max) {
                (min, max) = (max, min);
            }
            return (short)rand.Next(min, max);
        }

        /// <summary>
        /// Generates a random Long (8 byte integer).
        /// </summary>
        /// <param name="rand">The Randomizing object.</param>
        /// <param name="allowNegatives">Should negative numbers be generated?</param>
        /// <returns>The random long.</returns>
        public static long NextLong(this Random rand, bool allowNegatives = false) {
            byte[] bytes = new byte[8];
            rand.NextBytes(bytes);
            
            long val = BitConverter.ToInt64(bytes);
            if (allowNegatives == false) { 
                val = val == long.MinValue ? 0 : val;
                val = val < 0 ? val * -1 : val;
            }
            return val;
        }

        /// <summary>
        /// Generates a random Long (8 byte integer) between an inclusive lower bounds and excluded upper bounds.
        /// </summary>
        /// <param name="rand">The Randomizing object.</param>
        /// <param name="min">The inclusive lower bounds.</param>
        /// <param name="max">The excluded upper bounds.</param>
        /// <returns>The random long.</returns>
        public static long NextLong(this Random rand, long min, long max) {
            if (min == max) { return min; }
            if (min > max) { 
                (min, max) = (max, min); 
            }
            (int upper, int lower) maximum = ((int)(max / int.MaxValue), (int)(max % int.MaxValue));
            (int upper, int lower) minimum = ((int)(min / int.MaxValue), (int)(min % int.MaxValue));
            (int upper, int lower) random = (rand.Next(minimum.upper, maximum.upper), rand.Next(minimum.lower, maximum.lower));
            long rn = ((long)random.upper << 31) + random.lower;
            return rn;
        }

        /// <summary>
        /// Generates a random double between an inclusive minimum and an excluded maximum.
        /// </summary>
        /// <param name="rand">The Randomizing object.</param>
        /// <param name="min">The inclusive lower bounds.</param>
        /// <param name="max">The excluded upper bounds.</param>
        /// <returns>The random double.</returns>
        public static double NextDouble(this Random rand, double min, double max) {
            if (min == max) { return min; }
            if (min > max) {
                (min, max) = (max, min);
            }
            return (rand.NextDouble() * (max - min)) + min;
        }

        /// <summary>
        /// Generates a random float.
        /// </summary>
        /// <param name="rand">The Randomizing object.</param>
        /// <returns>The random float.</returns>
        public static float NextFloat(this Random rand) {
            byte[] bytes = new byte[4];
            rand.NextBytes(bytes);
            return BitConverter.ToSingle(bytes);
        }

        /// <summary>
        /// Generates a random float between an inclusive minimum and an excluded maximum.
        /// </summary>
        /// <param name="rand">The Randomizing object.</param>
        /// <param name="min">The inclusive lower bounds.</param>
        /// <param name="max">The excluded upper bounds.</param>
        /// <returns>The random float.</returns>
        public static float NextFloat(this Random rand, float min, float max) {
            return (float)rand.NextDouble(min, max);
        }

        /// <summary>
        /// Generates a random DateTime.
        /// </summary>
        /// <param name="rand">The Randomizing object.</param>
        /// <returns>The random DateTime.</returns>
        public static DateTime NextDateTime(this Random rand) {
            return rand.NextDateTime(DateTime.MinValue, DateTime.MaxValue);
        }

        /// <summary>
        /// Generates a random DateTime between an inclusive minimum and an excluded maximum.
        /// </summary>
        /// <param name="rand">The Randomizing object.</param>
        /// <param name="min">The inclusive lower bounds.</param>
        /// <param name="max">The excluded upper bounds.</param>
        /// <returns>The random DateTime.</returns>
        public static DateTime NextDateTime(this Random rand, DateTime min, DateTime max) {
            if (min == max) { return min; }
            if (min > max) {
                (min, max) = (max, min);
            }
            TimeSpan span = max - min;
            long val = rand.NextLong(0, span.Ticks);
            return min.AddTicks(val);
        }

        #endregion

        #region List Extensions

        /// <summary>
        /// Finds the 0 based indexs of all elements of a list that match a predicate.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to iterate through.</param>
        /// <param name="condition">The condition which must be met.</param>
        /// <returns>All of the 0 based indexs of elements which meet the predicate.</returns>
        public static List<int> FindAllIndexes<T>(this List<T> list, Predicate<T> condition) { 
            List<int> ndxs = new();
            for (int i = 0; i < list.Count; i++) {
                if (condition(list[i])) {
                    ndxs.Add(i);
                }
            }
            return ndxs;
        }

        #endregion

        #region Misc. Interative Extensions

        /// <summary>
        /// Does a specified action a specified number of times.
        /// </summary>
        /// <param name="interations">The number of times to repeat the action.</param>
        /// <param name="toDo">The action to repeat.</param>
        public static void For(this int interations, Action<int> toDo) { 
            for (int i = 0; i < interations; i++) {
                toDo(i);
            }
        }

        /// <summary>
        /// Adds an array of elements to the Queue.
        /// </summary>
        /// <typeparam name="T">The type of elements to add and that are in the queue.</typeparam>
        /// <param name="queue">The queue to add to.</param>
        /// <param name="toAdd">The elements to add.</param>
        public static void AddRange<T>(this Queue<T> queue, T[] toAdd) { 
            toAdd.ForEach(o => queue.Enqueue(o));
        }

        /// <summary>
        /// Adds an array of elements to the stack.
        /// </summary>
        /// <typeparam name="T">The type of elements to add and that are in the stck.</typeparam>
        /// <param name="stack">The stack to add to.</param>
        /// <param name="toAdd">The elements to add.</param>
        public static void AddRange<T>(this Stack<T> stack, T[] toAdd) { 
            toAdd.ForEach(o => stack.Push(o));
        }

        /// <summary>
        /// Adds an array of elements to the linked list.
        /// </summary>
        /// <typeparam name="T">The type of elements to add and that are in the linked list.</typeparam>
        /// <param name="span">The linked list to add to.</param>
        /// <param name="toAdd">The elements to add.</param>
        /// <param name="side">The side of the linked list to add to, front or back.</param>
        public static void AddRange<T>(this LinkedList<T> span, T[] toAdd, LinkedListSide side) { 
            Action<T> 
                addfront = (o => span.AddFirst(o)), 
                addback  = (o => span.AddLast(o));
            toAdd.ForEach(o => (side == LinkedListSide.Front ? addfront : addback)(o));
        }

        /// <summary>
        /// Gets a node from the linked list at a specific index.
        /// </summary>
        /// <typeparam name="T">The type of elements in the linked list.</typeparam>
        /// <param name="span">The linked list to access.</param>
        /// <param name="index">The index to get the node at.</param>
        /// <returns>The node at the specified index.</returns>
        /// <exception cref="InvalidOperationException">The linked list is empty.</exception>
        /// <exception cref="IndexOutOfRangeException">Can't access members beyond the bounds of the linked list.</exception>
        /// <exception cref="NullReferenceException">The returned node is null.</exception>
        public static LinkedListNode<T> GetNodeAt<T>(this LinkedList<T> span, int index) {
            if (span.Count == 0) {
                throw new InvalidOperationException("ERROR: Can not get a node of an empty LinkedList.");
            }
            if (index < 0 || index >= span.Count) {
                throw new IndexOutOfRangeException("ERROR: Invalid index specified.");
            }
            int ndx = 0;
            LinkedListNode<T>? searchNode = span.First;
            while (ndx != index) {
                searchNode = searchNode?.Next;
                ndx++;
            }
            if (searchNode == null) {
                throw new NullReferenceException("ERROR: Invalid node returned.");
            }
            return searchNode;
        }

        /// <summary>
        /// Gets an array of elements from the linked list ranging from the specified index to the count.
        /// </summary>
        /// <typeparam name="T">The type of elements in the linked list.</typeparam>
        /// <param name="span">The linked list to access.</param>
        /// <param name="start">The index at which to start grabbing elements in the linked list.</param>
        /// <param name="count">The number of elements to grab.</param>
        /// <returns>An array of elements from the linked list.</returns>
        /// <exception cref="InvalidOperationException">The linked list is empty.</exception>
        /// <exception cref="IndexOutOfRangeException">Can't access members beyond the bounds of the linked list.</exception>
        /// <exception cref="NullReferenceException">Attempted to go beyond the linked list.</exception>
        public static T[] GetRange<T>(this LinkedList<T> span, int start, int count) {
            if (span.Count == 0) {
                throw new InvalidOperationException("ERROR: Can not get items of an empty LinkedList.");
            }
            if (start < 0 || (start + count) >= span.Count) {
                throw new IndexOutOfRangeException("ERROR: Invalid index range specified.");
            }
            int ndx = 0;
            T[] result = new T[count];
            LinkedListNode<T> searchNode = span.GetNodeAt(start);
            while (ndx < count) {
                result[ndx] = searchNode.Value;
                if ((ndx + 1) < count) {
                    if (searchNode.Next == null) {
                        throw new NullReferenceException("ERROR: Invalid node encountered.");
                    }
                    searchNode = searchNode.Next;
                    ndx += 1;
                }
            }
            return result;
        }

        /// <summary>
        /// Removes all values from a linked list that return true in a given predicate.
        /// </summary>
        /// <typeparam name="T">The type of elements in the linked list.</typeparam>
        /// <param name="span">The linked list to access.</param>
        /// <param name="conditional">The predicate to test each element against.</param>
        public static void RemoveAll<T>(this LinkedList<T> span, Predicate<T> conditional) {
            if (span.Count > 0 && span.First != null) {
                LinkedListNode<T> searchNode = span.First;
                while (searchNode.Next != null) { 
                    while (searchNode.Next != null && conditional(searchNode.Next.Value) == true) {
                        span.Remove(searchNode.Next);
                    }
                    searchNode = searchNode.Next ?? searchNode;
                }
            }
        }
        #endregion

    }
}