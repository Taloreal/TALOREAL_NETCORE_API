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
        private static readonly SerializableDictionary<string, string> Database = new();

        /// <summary>
        /// Events that occur when a value in Database changes.
        /// </summary>
        private readonly static Dictionary<string, Listener?> OnChanged = new();

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
        static Settings() { LoadSettings(); }

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
                    success = success && fileBuffer.ReadIntFromBytes(ref index, out count);
                    for (int i = 0; i < count; i++) {
                        success = success && fileBuffer.ReadIntFromBytes(ref index, out keySize);
                        success = success && fileBuffer.ReadIntFromBytes(ref index, out valueSize);
                        success = success && fileBuffer.ReadStringFromBytes(ref index, keySize, out key);
                        success = success && fileBuffer.ReadStringFromBytes(ref index, valueSize, out value);
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
                            + "valuSize = " + valueSize + "\r\n\t, "
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
                if (File.Exists("Settings.TAL")) {
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
        /// <param name="KEY">The needed StrTyp_Key string needed for the database.</param>
        /// <returns>A value determining if the key/type is in the database.</returns>
        private static bool IsGoodKey(string keyname, Type type, out string KEY) {
            KEY = StrTyp_Key.Get_STKCode(keyname, type);
            return Converter.ContainsKey(type)
                && Database.ContainsKey(KEY);
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
            if (!IsGoodKey(key, typeof(T), out string KEY)) { return false; }
            try {
                result = (T?)Converter[typeof(T)](Database[KEY], out bool worked);
                return worked;
            }
            catch { return false; }
        }

        /// <summary>
        /// Sets a value in the database.
        /// </summary>
        /// <typeparam name="T">The value type to save.</typeparam>
        /// <param name="key">The key to save to the database.</param>
        /// <param name="obj">THe object to save to the database.</param>
        /// <param name="onChange">An optional event to happen when the value is changed.</param>
        /// <returns>A value determining if the key/value were saved in the database.</returns>
        public static bool SetValue<T>(string key, T? obj, Listener? onChange = null) {
            bool nullptr = key == null || key == "";
            bool noConvert = !Converter.ContainsKey(typeof(T));
            if (nullptr || noConvert) { return false; }
            string STKey = StrTyp_Key.Get_STKCode(key!, typeof(T));
            if (onChange != null) { ListenTo<T>(key!, onChange); }

            bool inDatabase = GetValue<T>(key!, out T? old);
            RemoveOld<T>(key!);
            Database.Add(STKey, obj == null ? "" : (obj!.ToString() 
                ?? throw new NullReferenceException("ERROR: Null string representation.")));

            if (Autosave) { SaveSettings(); }
            if (inDatabase && OnChanged.ContainsKey(STKey)) {
                (OnChanged[STKey] ?? throw new NullReferenceException("ERROR: Listening delegate null."))(key!, typeof(T), old, obj);
            }
            return true;
        }

        /// <summary>
        /// Adds an event to listen for changes to the database.
        /// </summary>
        /// <typeparam name="T">The type of value to be monitored.</typeparam>
        /// <param name="key">The key of the value to be monitored.</param>
        /// <param name="onChange">The method to call when change happens.</param>
        public static void ListenTo<T>(string key, Listener onChange) {
            string STKey = StrTyp_Key.Get_STKCode(key, typeof(T));
            if (OnChanged.ContainsKey(STKey)) {
                OnChanged[STKey] += onChange;
            }
            else { OnChanged.Add(STKey, onChange); }
        }

        /// <summary>
        /// Unsubscribes a method from listening to the database.
        /// </summary>
        /// <typeparam name="T">The type of value in the database.</typeparam>
        /// <param name="key">The key to stop listening to.</param>
        /// <param name="onChange">The method to unsubscribe.</param>
        public static void Mute<T>(string key, Listener onChange) {
            string STKey = StrTyp_Key.Get_STKCode(key, typeof(T));
            if (!OnChanged.ContainsKey(STKey)) { return; }
            OnChanged[STKey] -= onChange;
            if (OnChanged[STKey] == null) { 
                OnChanged.Remove(STKey); 
            }
        }

        /// <summary>
        /// Removes an old value from the database.
        /// </summary>
        /// <typeparam name="T">The type of value to be removed.</typeparam>
        /// <param name="key">The key to be removed from the database.</param>
        private static void RemoveOld<T>(string key) {
            bool goodkey = IsGoodKey(key, typeof(T), out key);
            if (goodkey && Database.ContainsKey(key)) {
                Database.Remove(key);
            }
        }

        /// <summary>
        /// Removes a value from the database. And unsubscribes it's events.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="key">The key in the database.</param>
        /// <returns>A value determining if success/failure.</returns>
        public static bool RemoveValue<T>(string key) {
            if (!IsGoodKey(key, typeof(T), out key)) { return false; }
            if (Database.ContainsKey(key)) {
                Database.Remove(key);
                if (Autosave) { SaveSettings(); }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Empties the database.
        /// </summary>
        /// <param name="save">Should the database be saved after emptying?</param>
        public static void Clear(bool save = true) {
            Database.Clear();
            if (Autosave && save) { 
                SaveSettings(); 
            }
        }
    }
}
