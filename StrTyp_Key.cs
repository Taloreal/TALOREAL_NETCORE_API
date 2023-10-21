using System.Xml.Serialization;

namespace TALOREAL_NETCORE_API {

    /// <summary>
    /// StrTyp_Key was suppose to be a quickway to switch between Settings database's
    /// "frontend" key and "backend" key. 
    /// (nonstatic fields deprecated: 2020.)
    /// (nonstatic fields removed: Aug 21st, 2023.)
    /// </summary>
    [Serializable] public static class StrTyp_Key {

        [XmlIgnore] private const string TAG = " :1a3b5c7d9: ";

        /// <summary>
        /// Creates the string key for Settings' database.
        /// </summary>
        /// <param name="called">The frontend key.</param>
        /// <param name="ofKind">The type of variable.</param>
        /// <returns>The backend key.</returns>
        public static string Get_STKCode(string called, Type ofKind) {
            if (called.Contains(':')) { 
                throw new Exception("ERROR: Key name cannot contain ':'."); 
            }
            return called + TAG + ofKind.FullName;
        }
    }
}
