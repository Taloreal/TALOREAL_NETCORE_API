using System.Data.Common;
using System.Security.AccessControl;

namespace TALOREAL_NETCORE_API {

    public class AutoSetting<T> {

        /// <summary>
        /// Keeps track of how many what the next autoassigned value's name will be (will be in hex).
        /// </summary>
        private static uint AutoAssignCounter = 0;

        /// <summary>
        /// The characters used to make up the auto assigned names.
        /// </summary>
        private const string HexChars = "0123456789ABCDEF";

        /// <summary>
        /// The name of the value in the database. 
        /// Can't be set exterminally to prevent pointing to another value in the database.
        /// </summary>
        public string Name { get; protected set; } = "";

        /// <summary>
        /// Property for getting and setting what's in the database.
        /// </summary>
        public T? Value {
            get {
                Settings.GetValue(Name, out T? val);
                return val;
            }
            set => Settings.SetValue(Name, value);
        }

        /// <summary>
        /// Creates an AutoSetting object with the specified name. This is useful if you don't want to override what's in the database.
        /// </summary>
        /// <param name="name">The name of the AutoSettings object and the name in the database.</param>
        public AutoSetting(string name) {
            Name = name;
        }

        /// <summary>
        /// Creates a new AutoSettings object with a specified name and value.
        /// </summary>
        /// <param name="name">The name to give the AutoSetting object and use in the Settings Database, if null auto assigned.</param>
        /// <param name="defVal">The starting value of the AutoSetting object, default of type if not specified.</param>
        public AutoSetting(string? name = null, T? defVal = default) {
            if (name == null) {
                name = "";
                uint hexcoder = AutoAssignCounter;
                for (int i = 0; i < 8; i++) {
                    name = HexChars[(int)(hexcoder % 16)] + (i != 0 && i % 2 == 0 ? " " : "") + name;
                    hexcoder /= 16;
                }
                AutoAssignCounter += 1;
            }
            Name = name;
            Value = defVal;
        }

        #region Cast Operators

        /// <summary>
        /// Creates a new AutoSetting object from a name-value pair.
        /// </summary>
        /// <param name="value">The name-value pair to set the AutoSetting to.</param>
        public static implicit operator AutoSetting<T>((string? name, T? value) value) {
            return new AutoSetting<T>(value.name, value.value);
        }

        /// <summary>
        /// Casts a name to an AutoSetting object.
        /// </summary>
        /// <param name="name">The name to set the AutoSetting object to.</param>
        public static implicit operator AutoSetting<T>(string name) {
            return new AutoSetting<T>(name);
        }

        /// <summary>
        /// Gives the name contained in the AutoSetting object.
        /// </summary>
        /// <param name="value">The AutoSetting object to cast.</param>
        public static implicit operator string(AutoSetting<T> value) {
            return value.Name;
        }

        #endregion
    }
}
