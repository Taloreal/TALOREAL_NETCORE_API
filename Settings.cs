using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace TALOREAL_NETCORE_API {

	/// <summary>
	/// Represents a change from oldval to newval in the database.
	/// </summary>
	/// <param name="key">The key of the changed value.</param>
	/// <param name="type">The type of object in the database.</param>
	/// <param name="oVal">The old value.</param>
	/// <param name="nVal">The new value.</param>
	public delegate void Listener(string key, Type type, object? oVal, object? nVal);

	/// <summary>
	/// A class to make getting and setting variables that persist between executions of programs easier.
	/// </summary>
	public static class Settings {

		/// <summary>
		/// A string key/value database.
		/// </summary>
		private static readonly SerializableDictionary<string, string> Database = [];

		/// <summary>
		/// Events that occur when a value in Database changes.
		/// </summary>
		private readonly static Dictionary<string, Listener?> OnChanged = [];

		/// <summary>
		/// Represents a conversion from a string to some other type.
		/// </summary>
		/// <param name="str">The string input.</param>
		/// <param name="worked">A value indicating success/failure.</param>
		/// <returns>The converted object.</returns>
		private delegate object? Parser(string str, out bool worked);


		/// <summary>
		/// A collection of conversion definitions.
		/// </summary>
		private readonly static Dictionary<Type, Parser> Converter = new() {
			{ typeof(string),   (string s, out bool p) =>
				{ p = true; return s; } },
			{ typeof(bool),     (string s, out bool p) =>
				{ p = UtilityExtensions.TryParseBool(s, out bool val); return val; } },
			{ typeof(byte),     (string s, out bool p) =>
				{ p = byte.TryParse(s, out byte val); return val; } },
			{ typeof(short),    (string s, out bool p) =>
				{ p = short.TryParse(s, out short val); return val; } },
			{ typeof(int),      (string s, out bool p) =>
				{ p = int.TryParse(s, out int val); return val; } },
			{ typeof(long),     (string s, out bool p) =>
				{ p = long.TryParse(s, out long val); return val; } },
			{ typeof(float),    (string s, out bool p) =>
				{ p = float.TryParse(s, out float val); return val; } },
			{ typeof(double),   (string s, out bool p) =>
				{ p = double.TryParse(s, out double val); return val; } },
			{ typeof(decimal),  (string s, out bool p) =>
				{ p = decimal.TryParse(s, out decimal val); return val; } },
			{ typeof(sbyte),    (string s, out bool p) =>
				{ p = sbyte.TryParse(s, out sbyte val); return val; } },
			{ typeof(ushort),   (string s, out bool p) =>
				{ p = ushort.TryParse(s, out ushort val); return val; } },
			{ typeof(uint),     (string s, out bool p) =>
				{ p = uint.TryParse(s, out uint val); return val; } },
			{ typeof(ulong),    (string s, out bool p) =>
				{ p = ulong.TryParse(s, out ulong val); return val; } },
			{ typeof(DateTime), (string s, out bool p) =>
				{ p = DateTime.TryParse(s, out DateTime val); return val; } },
		};


		/// <summary>
		/// Will the database save for each change?
		/// </summary>
		public static bool Autosave = true;


		/// <summary>
		/// Automatically loads the previously saved database.
		/// </summary>
		static Settings() {
			LoadSettings();
		}


		/// <summary>
		/// Loads a previously saved database.
		/// </summary>
		/// <returns>A value determining success/failure.</returns>
		public static bool LoadSettings() {
			Clear(false);
			// NEW IMPLEMENTATION
			try {
				byte[] fileBuffer = File.ReadAllBytes("Settings.TAL");
				int fileLength = fileBuffer.Length;
				if (fileLength > 4) {
					bool success = true;
					string key = "", value = "";
					int index = 0, count = 0, keySize = 0, valueSize = 0;
					success = success == true && fileBuffer.ReadIntFromBytes(ref index, out count);
					for (int i = 0; i < count; i++) {
						success = success == true &&
							fileBuffer.ReadIntFromBytes(ref index, out keySize);
						success = success == true &&
							fileBuffer.ReadIntFromBytes(ref index, out valueSize);
						success = success == true &&
							fileBuffer.ReadStringFromBytes(ref index, keySize, out key);
						success = success == true &&
							fileBuffer.ReadStringFromBytes(ref index, valueSize, out value);
						if (success == true) {
							Database.Add(key, value);
							continue;
						}
						throw new Exception("ERROR: File format error.\r\n\t"
							+ "file_length = " + fileLength + "\r\n\t, "
							+ "file_index = " + index + "\r\n\t, "
							+ "count = " + count + "\r\n\t, "
							+ "index = " + i + "\r\n\t, "
							+ "keySize = " + keySize + "\r\n\t, "
							+ "valueSize = " + valueSize + "\r\n\t, "
							+ "key = " + key + "\r\n\t, "
							+ "value = " + value);
					}
					return true;
				}
			}
			catch {
#warning "Need log file implementation"
				// TODO: Implement a log file for the exception.
			}
			return false;
		}

		/// <summary>
		/// Saves the database to the local file system.
		/// </summary>
		/// <returns>A value determining success/failure.</returns>
		public static bool SaveSettings() {
			// NEW IMPLEMENTATION
			try {
				BinaryWriter bw = new(File.Create("temp.TAL"));
				bw.Write(Database.Count);
				foreach (KeyValuePair<string, string> entry in Database) {
					bw.Write(entry.Key.Length);
					bw.Write(entry.Value.Length);
					bw.Write(entry.Key.GetBytes());
					bw.Write(entry.Value.GetBytes());
				}
				bw.Close();
				if (File.Exists("Settings.TAL") == true) {
					File.Delete("Settings.TAL");
				}
				File.Move("temp.TAL", "Settings.TAL");
				return true;
			}
			catch {
#warning "Need log file implementation"
				// TODO: Implement a log file for the exception.
			}
			return false;
		}

		/// <summary>
		/// Checks if a provided key is in the database.
		/// </summary>
		/// <param name="keyname">The keyname to check.</param>
		/// <param name="type">The desired value type.</param>
		/// <param name="KEY">The needed StringTypeKey string needed for the database.</param>
		/// <returns>A value determining if the key/type is in the database.</returns>
		private static bool IsGoodKey(string keyname, Type type, out string KEY) {
			KEY = StringTypeKey.GetSettingsKeyCode(keyname, type);
			return Converter.ContainsKey(type) == true
				&& Database.ContainsKey(KEY) == true;
		}

		/// <summary>
		/// Gets a value from the database.
		/// </summary>
		/// <typeparam name="T">The type of value to get/convert to.</typeparam>
		/// <param name="key">The key to look up in the database.</param>
		/// <param name="result">The outputted value.</param>
		/// <returns>A value determining if something was fetched from the database.</returns>
		public static bool GetValue<T>(string key, out T? result) {
			result = default;
			if (IsGoodKey(key, typeof(T), out string KEY) == true) {
				try {
					result = (T?)Converter[typeof(T)](Database[KEY], out bool worked);
					return worked;
				}
				catch { }
			}
			return false; 
		}

		/// <summary>
		/// Sets a value in the database.
		/// </summary>
		/// <typeparam name="T">The value type to save.</typeparam>
		/// <param name="key">The key to save to the database.</param>
		/// <param name="obj">The object to save to the database.</param>
		/// <param name="onChange">An optional event to happen when the value is changed.</param>
		/// <returns>A value determining if the key/value were saved in the database.</returns>
		public static bool SetValue<T>(string key, T? obj, Listener? onChange = null) {
			bool nullptr = key == null || key == "";
			bool noConvert = Converter.ContainsKey(typeof(T)) == false;
			if (nullptr == false && noConvert == false) {
				string STKey = StringTypeKey.GetSettingsKeyCode(key!, typeof(T));
				if (onChange != null) {
					ListenTo<T>(key!, onChange);
				}

				bool inDatabase = GetValue<T>(key!, out T? old);
				RemoveOld<T>(key!);
				Database.Add(STKey, obj == null ? "" : (obj!.ToString()
					?? throw new NullReferenceException("ERROR: Null string representation.")));

				if (Autosave == true) {
					SaveSettings();
				}
				if (inDatabase == true && OnChanged.TryGetValue(STKey, out Listener? ev) == true) {
					Listener toCall = ev ??
						throw new NullReferenceException("ERROR: Null listener reference.");
					toCall(key!, typeof(T), old, obj);
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets a list of values from the database. The list must have been saved with SetList
		/// under the same key and element type.
		/// </summary>
		/// <typeparam name="T">The element type; any type GetValue supports.</typeparam>
		/// <param name="key">The key to look up in the database.</param>
		/// <param name="result">The decoded list. Empty when nothing was fetched.</param>
		/// <returns>True if a list was found and every element converted; false otherwise.</returns>
		public static bool GetList<T>(string key, out List<T> result) {
			result = new List<T>();
			bool worked = false;
			bool nullptr = key == null || key == "";
			bool noConvert = Converter.ContainsKey(typeof(T)) == false;
			if (nullptr == false && noConvert == false) {
				string STKey = StringTypeKey.GetSettingsKeyCode(key!, typeof(List<T>));
				if (Database.TryGetValue(STKey, out string? encoded) == true) {
					worked = TryDecodeList(encoded, out List<string> parts);
					Parser parse = Converter[typeof(T)];
					int index = 0;
					while (worked == true && index < parts.Count) {
						object? value = parse(parts[index], out bool parsed);
						worked = parsed == true && value != null;
						if (worked == true) {
							result.Add((T)value!);
						}
						index += 1;
					}
				}
			}
			if (worked == false) {
				result.Clear();
			}
			return worked;
		}

		/// <summary>
		/// Sets a list of values in the database under one key. Each element is stored in the
		/// same string form a scalar of its type would be, and the whole list is kept as one
		/// hex-encoded value, so an element may contain any character.
		/// </summary>
		/// <typeparam name="T">The element type; any type SetValue supports.</typeparam>
		/// <param name="key">The key to save to the database.</param>
		/// <param name="values">The list to save.</param>
		/// <param name="onChange">An optional event to happen when the list is changed. Unlike SetValue, a list fires its listeners on the first write too, with an empty old list.</param>
		/// <returns>A value determining if the key/list were saved in the database.</returns>
		public static bool SetList<T>(string key, IList<T> values, Listener? onChange = null) {
			bool nullptr = key == null || key == "";
			bool noConvert = Converter.ContainsKey(typeof(T)) == false;
			if (nullptr == false && noConvert == false && values != null) {
				string STKey = StringTypeKey.GetSettingsKeyCode(key!, typeof(List<T>));
				if (onChange != null) {
					ListenTo<List<T>>(key!, onChange);
				}

				GetList<T>(key!, out List<T> old);   // empty when this is the first write
				Database.Remove(STKey);
				Database.Add(STKey, EncodeList(values));

				if (Autosave == true) {
					SaveSettings();
				}
				if (OnChanged.TryGetValue(STKey, out Listener? ev) == true) {
					Listener toCall = ev ??
						throw new NullReferenceException("ERROR: Null listener reference.");
					toCall(key!, typeof(List<T>), old, values);
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Encodes a list as hex, two characters per byte. Each element is written as a
		/// 4-byte little-endian length followed by its UTF-8 bytes, so no separator or
		/// escaping is needed.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="values">The list to encode.</param>
		/// <returns>The hex string. Empty for an empty list.</returns>
		private static string EncodeList<T>(IList<T> values) {
			List<byte> bytes = new();
			foreach (T value in values) {
				string text = value == null ? "" : (value.ToString() ?? "");
				byte[] data = Encoding.UTF8.GetBytes(text);
				int length = data.Length;
				bytes.Add((byte)(length & 0xFF));
				bytes.Add((byte)((length >> 8) & 0xFF));
				bytes.Add((byte)((length >> 16) & 0xFF));
				bytes.Add((byte)((length >> 24) & 0xFF));
				bytes.AddRange(data);
			}

			StringBuilder hex = new(bytes.Count * 2);
			foreach (byte current in bytes) {
				hex.Append(current.ToString("X2"));
			}
			return hex.ToString();
		}

		/// <summary>
		/// Decodes a string written by EncodeList back into its element strings.
		/// </summary>
		/// <param name="encoded">The hex string.</param>
		/// <param name="parts">The element strings, in order. Empty on failure.</param>
		/// <returns>True if the whole string decoded cleanly; false on any malformed byte or length.</returns>
		private static bool TryDecodeList(string encoded, out List<string> parts) {
			parts = new List<string>();
			bool worked = encoded.Length % 2 == 0;
			byte[] bytes = new byte[encoded.Length / 2];
			int position = 0;
			while (worked == true && position < bytes.Length) {
				string pair = encoded.Substring(position * 2, 2);
				worked = byte.TryParse(pair, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytes[position]);
				position += 1;
			}

			int offset = 0;
			while (worked == true && offset < bytes.Length) {
				worked = offset + 4 <= bytes.Length;
				if (worked == true) {
					int length = bytes[offset]
						| (bytes[offset + 1] << 8)
						| (bytes[offset + 2] << 16)
						| (bytes[offset + 3] << 24);
					offset += 4;
					worked = length >= 0 && offset + length <= bytes.Length;
					if (worked == true) {
						parts.Add(Encoding.UTF8.GetString(bytes, offset, length));
						offset += length;
					}
				}
			}
			if (worked == false) {
				parts.Clear();
			}
			return worked;
		}

		/// <summary>
		/// Adds an event to listen for changes to the database.
		/// </summary>
		/// <typeparam name="T">The type of value to be monitored.</typeparam>
		/// <param name="key">The key of the value to be monitored.</param>
		/// <param name="onChange">The method to call when change happens.</param>
		public static void ListenTo<T>(string key, Listener onChange) {
			string STKey = StringTypeKey.GetSettingsKeyCode(key, typeof(T));
			if (OnChanged.TryAdd(STKey, onChange) == false) {
				OnChanged[STKey] += onChange;
			}
		}

		/// <summary>
		/// Unsubscribes a method from listening to the database.
		/// </summary>
		/// <typeparam name="T">The type of value in the database.</typeparam>
		/// <param name="key">The key to stop listening to.</param>
		/// <param name="onChange">The method to unsubscribe.</param>
		public static void Mute<T>(string key, Listener onChange) {
			string STKey = StringTypeKey.GetSettingsKeyCode(key, typeof(T));
			if (OnChanged.ContainsKey(STKey) == true) {
				OnChanged[STKey] -= onChange;
				if (OnChanged[STKey] == null) {
					OnChanged.Remove(STKey);
				}
			}
		}

		/// <summary>
		/// Removes an old value from the database.
		/// </summary>
		/// <typeparam name="T">The type of value to be removed.</typeparam>
		/// <param name="key">The key to be removed from the database.</param>
		private static void RemoveOld<T>(string key) {
			bool goodkey = IsGoodKey(key, typeof(T), out key);
			if (goodkey == true && Database.ContainsKey(key) == true) {
				Database.Remove(key);
			}
		}

		/// <summary>
		/// Removes a value from the database.
		/// Listeners on the key stay subscribed;
		///		use Mute to remove them.
		/// </summary>
		/// <typeparam name="T">The type of value.</typeparam>
		/// <param name="key">The key in the database.</param>
		/// <returns>A value determining if success/failure.</returns>
		public static bool RemoveValue<T>(string key) {
			if (IsGoodKey(key, typeof(T), out key) == true) {
				bool removed = Database.Remove(key);
				if (removed == true && Autosave == true) {
					SaveSettings();
				}
				return removed;
			}
			return false;
		}

		/// <summary>
		/// Empties the database.
		/// </summary>
		/// <param name="save">Should the database be saved after emptying?</param>
		public static void Clear(bool save = true) {
			Database.Clear();
			if (Autosave == true && save == true) {
				SaveSettings();
			}
		}
	}
}
