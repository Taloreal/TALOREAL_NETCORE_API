using System.Runtime.InteropServices;

namespace TALOREAL_NETCORE_API {

    public enum ForLoopDirection { 
        Forward, Backward
    }

    public static class Extensions {

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

            bool opposite(T arg) { 
                return predicate(arg) == false; 
            }

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
    }
}