
namespace TALOREAL_NETCORE_API {

	/// <summary>
	/// StringTypeKey was suppose to be a quickway to switch between Settings database's
	/// "frontend" key and "backend" key.
	/// (nonstatic fields deprecated: 2020.)
	/// (nonstatic fields removed: Aug 21st, 2023.)
	/// </summary>
	public static class StringTypeKey {

		private const string Tag = " :1a3b5c7d9: ";


		/// <summary>
		/// Creates the string key for Settings' database.
		/// </summary>
		/// <param name="called">The frontend key.</param>
		/// <param name="ofKind">The type of variable.</param>
		/// <returns>The backend key.</returns>
		public static string GetSettingsKeyCode(string called, Type ofKind) {
			bool containsColon = called.Contains(':');
			if (containsColon == false) {
				return called + Tag + ofKind.FullName;
			}
			throw new Exception("ERROR: Key name cannot contain ':'.");
		}
	}
}
