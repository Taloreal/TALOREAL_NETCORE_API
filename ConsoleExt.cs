using System.Drawing;
using System.Text;

namespace TALOREAL_NETCORE_API {

	public enum ConsoleTextAlign {
		LeftAligned, Centered, RightAligned
	}

	/// <summary>
	/// Console input and output helpers: typed and range-checked prompts, text and
	/// number prompts that retry until the answer is valid, and writes at a set position.
	/// </summary>
	public static class ConsoleExt {

		private delegate object Parser(string str, out bool worked);
		private delegate bool Ranged(object obj, object min, object max);


		/// <summary>
		/// Used to attempt to convert string input to the requested type.
		/// </summary>
		private readonly static Dictionary<Type, Parser> Converter = new() {
			{ typeof(string),   (string s, out bool p) => { p = true; return s; } },
			{ typeof(bool),     (string s, out bool p) => { p = UtilityExtensions.TryParseBool(s, out bool val); return val; } },
			{ typeof(byte),     (string s, out bool p) => { p = byte.TryParse(s, out byte val); return val; } },
			{ typeof(short),    (string s, out bool p) => { p = short.TryParse(s, out short val); return val; } },
			{ typeof(int),      (string s, out bool p) => { p = int.TryParse(s, out int val); return val; } },
			{ typeof(long),     (string s, out bool p) => { p = long.TryParse(s, out long val); return val; } },
			{ typeof(double),   (string s, out bool p) => { p = double.TryParse(s, out double val); return val; } },
			{ typeof(float),    (string s, out bool p) => { p = float.TryParse(s, out float val); return val; } },
			{ typeof(sbyte),    (string s, out bool p) => { p = sbyte.TryParse(s, out sbyte val); return val; } },
			{ typeof(ushort),   (string s, out bool p) => { p = ushort.TryParse(s, out ushort val); return val; } },
			{ typeof(uint),     (string s, out bool p) => { p = uint.TryParse(s, out uint val); return val; } },
			{ typeof(ulong),    (string s, out bool p) => { p = ulong.TryParse(s, out ulong val); return val; } },
			{ typeof(DateTime), (string s, out bool p) => { p = DateTime.TryParse(s, out DateTime val); return val; } },
		};

		/// <summary>
		/// Used to check if a value is inside expected range.
		/// </summary>
		private readonly static Dictionary<Type, Ranged> InsideOf = new() {

			{ typeof(string), (o, min, max) => {
				return IsType<int>([min, max]) == true && o.GetType() == typeof(string) ?
					((string)o).Length >= (int)min && ((string)o).Length <= (int)max :
					false;
			} },

			{ typeof(bool), (o, min, max) => {
				return IsType<bool>([o]) == true ? true : false;
			} },

			{ typeof(byte), (o, min, max) => {
				return IsType<byte>([o, min, max]) == true ?
					(byte)o >= (byte)min && (byte)o <= (byte)max :
					false;
			} },

			{ typeof(short), (o, min, max) => {
				return IsType<short>([o, min, max]) == true ?
					(short)o >= (short)min && (short)o <= (short)max :
					false;
			} },

			{ typeof(int), (o, min, max) => {
				return IsType<int>([o, min, max]) == true ?
					(int)o >= (int)min && (int)o <= (int)max :
					false;
			} },

			{ typeof(long), (o, min, max) => {
				return IsType<long>([o, min, max]) == true ?
					(long)o >= (long)min && (long)o <= (long)max :
					false;
			} },

			{ typeof(double), (o, min, max) => {
				return IsType<double>([o, min, max]) == true ?
					(double)o >= (double)min && (double)o <= (double)max :
					false;
			} },

			{ typeof(float), (o, min, max) => {
				return IsType<float>([o, min, max]) == true ?
					(float)o >= (float)min && (float)o <= (float)max :
					false;
			} },

			{ typeof(sbyte), (o, min, max) => {
				return IsType<sbyte>([o, min, max]) == true ?
					(sbyte)o >= (sbyte)min && (sbyte)o <= (sbyte)max :
					false;
			} },

			{ typeof(ushort), (o, min, max) => {
				return IsType<ushort>([o, min, max]) == true ?
					(ushort)o >= (ushort)min && (ushort)o <= (ushort)max :
					false;
			} },

			{ typeof(uint), (o, min, max) => {
				return IsType<uint>([o, min, max]) == true ?
					(uint)o >= (uint)min && (uint)o <= (uint)max :
					false;
			} },

			{ typeof(ulong), (o, min, max) => {
				return IsType<ulong>([o, min, max]) == true ?
					(ulong)o >= (ulong)min && (ulong)o <= (ulong)max :
					false;
			} },

			{ typeof(DateTime), (o, min, max) => {
				return IsType<DateTime>([o, min, max]) == true ?
					(DateTime)o >= (DateTime)min && (DateTime)o <= (DateTime)max :
					false;
			} },
		};


		/// <summary>
		/// Checks that all elements are of the requested type.
		/// </summary>
		/// <typeparam name="T">The expected type.</typeparam>
		/// <param name="objs">The objects to compare.</param>
		/// <returns>Are all of the objects the expected type?</returns>
		private static bool IsType<T>(object[] objs) {
			// ORIGINAL:
			// for (int i = 0; i < objs.Length; i++) {
			// 	if (objs[i].GetType() != typeof(T)) {
			// 		return false;
			// 	}
			// }
			// return true;
			bool allMatch = true;
			for (int i = 0; i < objs.Length && allMatch == true; i++) {
				if (objs[i].GetType() != typeof(T)) {
					allMatch = false;
				}
			}
			return allMatch;
		}

		/// <summary>
		/// Attempts to get a value from the user.
		/// </summary>
		/// <typeparam name="T">The type of value to get from the user.</typeparam>
		/// <param name="prompt">The prompt to show the user.</param>
		/// <param name="lined">Should the response be on another line?</param>
		/// <param name="min">The minimum value, if any, of the user's input.</param>
		/// <param name="max">The maximum value, if any, of the user's input.</param>
		/// <returns>The user's input in the requested type.</returns>
		public static T ReadValue<T>(string prompt, bool lined = true, T? min = default, T? max = default) {
			bool loop = true;
			T? obj = default;

			if (Converter.ContainsKey(typeof(T)) == true) {
				if (typeof(T) == typeof(string)) {
					Console.Clear();
					Write(prompt, lined);
					obj = (T)(object)(Console.ReadLine() ??
						throw new NullReferenceException("ERROR: Null string read as input."));
					loop = false;
				}
				while (loop == true) {
					Console.Clear();
					Write(prompt, lined);
					string input = Console.ReadLine() ??
						throw new NullReferenceException("ERROR: Null string read as input.");
					obj = (T)Converter[typeof(T)](input, out loop);
					loop = loop == false;
					string err = "ERROR: \"" + input + "\" is not of expected type " + typeof(T).Name;
					if (loop == false && min != null && max != null) {
						if (min.Equals(max) == false && InsideOf[typeof(T)](obj, min, max) == false) {
							err = "ERROR: \"" + input + "\" was outside the expected range " +
								min + " to " + max;
							loop = true;
						}
					}
					if (loop == true) {
						Write(err);
						Console.ReadLine();
					}
				}
				return obj!;
			}
			throw new TypeAccessException("ERROR: Type not supported.");
		}

		/// <summary>
		/// Displays a prompt then waits for user input.
		/// </summary>
		/// <param name="prompt">The prompt to display.</param>
		/// <param name="limit">The maximum number of characters, if any, for the response.</param>
		/// <param name="lined">Should the response be on a different line?</param>
		/// <returns>The user's input.</returns>
		public static string ReadLine(string prompt, int limit = -1, bool lined = true) {
			Write(prompt, lined);
			string resp = Console.ReadLine() ?? throw new NullReferenceException("ERROR: Null string read as input.");
			while (limit != -1 && resp.Length > limit) {
				Write("ERROR: Response was too long. (Limit: " + limit + " chars)", true);
				Write(prompt, lined);
				resp = Console.ReadLine() ?? throw new NullReferenceException("ERROR: Null string read as input.");
			}
			return resp;
		}

		/// <summary>True for a char that renders two columns wide in a monospace console (CJK script, fullwidth forms, Hangul) rather than one.</summary>
		private static bool IsWideChar(char c) =>
			(c >= 'ᄀ' && c <= 'ᇿ')   // Hangul Jamo
			|| (c >= '⺀' && c <= '〾') // CJK radicals, symbols/punctuation (incl. 「」、。)
			|| (c >= 'ぁ' && c <= '㏿') // Hiragana, Katakana, CJK compat
			|| (c >= '㐀' && c <= '䶿') // CJK unified ideographs extension A
			|| (c >= '一' && c <= '鿿') // CJK unified ideographs
			|| (c >= 'ꥠ' && c <= '꥿') // Hangul Jamo extended-A
			|| (c >= '가' && c <= '힣') // Hangul syllables
			|| (c >= '豈' && c <= '﫿') // CJK compatibility ideographs
			|| (c >= '＀' && c <= '｠') // fullwidth forms
			|| (c >= '￠' && c <= '￦'); // fullwidth signs

		/// <summary>
		/// Writes a prompt to the screen either with a new line or not.
		/// </summary>
		/// <param name="prompt">The prompt to display.</param>
		/// <param name="lined">Should a new line be made?</param>
		public static void Write(string prompt, bool lined = true) {
			Action toDo = lined == true ?
				() => Console.WriteLine(prompt) :
				() => Console.Write(prompt);
			toDo();
		}

		/// <summary>
		/// Writes a prompt onto the console at a specific location and then reset the cursor position back to where it was.
		/// </summary>
		/// <param name="prompt">The prompt to display.</param>
		/// <param name="pos">The position to print the prompt.</param>
		/// <param name="lined">Should a newline character be printed as well?</param>
		public static void WriteAtPosition(string prompt, Point pos, bool lined = false) {
			(int left, int top) = Console.GetCursorPosition();
			Console.SetCursorPosition(pos.X, pos.Y);
			Write(prompt, lined);
			Console.SetCursorPosition(left, top);
		}

		/// <summary>
		/// Waits for the user to press enter.
		/// </summary>
		/// <param name="action">The prompt to wait for.</param>
		public static void WaitForEnter(string action) {
			Console.WriteLine();
			Console.WriteLine("Press enter to " + action + "...");
			Console.ReadLine();
		}

		/// <summary>
		/// Shows the user some options and prompts for a choice.
		/// </summary>
		/// <param name="prompt">The prompt for the user.</param>
		/// <param name="options">The options for the user.</param>
		/// <param name="sel">The user's selection.</param>
		/// <param name="numberOptions">Should each of the options be numbered?</param>
		public static void ReadChoice(string prompt, string[] options, out int sel, bool numberOptions = true) {
			for (int i = 0; i < options.Length; i++) {
				Console.WriteLine((numberOptions == true ?
					((i + 1) + ". ") : "") + options[i]);
			}
			sel = ReadInteger(prompt, 1, options.Length);
		}

		/// <summary>
		/// Gets an integer input from the user.
		/// If min and max are set equal range does not matter,
		/// otherwise the integer must be within range.
		/// </summary>
		/// <param name="prompt">The string to prompt the user with.</param>
		/// <param name="min">The minimum input.</param>
		/// <param name="max">The maximum input.</param>
		/// <returns>An integer within the set range.</returns>
		public static int ReadInteger(string prompt, int min = -1, int max = -1) {
			bool notRanged = min == max;
			Console.Write(prompt);
			bool isInt = int.TryParse(Console.ReadLine(), out int input);
			bool inRange = notRanged || (input >= min && input <= max);
			while (isInt == false || inRange == false) {
				string add = notRanged == true ? "." : " " + min + " to " + max + ".";
				string err = "ERROR: Must enter a number" + add;
				Console.WriteLine(err);
				Console.Write(prompt);
				isInt = int.TryParse(Console.ReadLine(), out input);
				inRange = notRanged || (input >= min && input <= max);
			}
			return input;
		}

		/// <summary>
		/// Prompts the user for input and puts it inside input.
		/// </summary>
		/// <param name="prompt">The prompt for the user.</param>
		/// <param name="input">The input the user enters.</param>
		public static void ReadString(string prompt, ref string input) {
			Console.Write(prompt);
			input = Console.ReadLine() ?? throw new NullReferenceException("ERROR: Null string read as input.");
			bool empty = string.IsNullOrEmpty(input);
			bool white = string.IsNullOrWhiteSpace(input);
			while (empty == true || white == true) {
				Console.WriteLine("ERROR: Must enter text.");
				Console.Write(prompt);
				input = Console.ReadLine() ?? throw new NullReferenceException("ERROR: Null string read as input.");
				empty = string.IsNullOrEmpty(input);
				white = string.IsNullOrWhiteSpace(input);
			}
		}
	}
}	
