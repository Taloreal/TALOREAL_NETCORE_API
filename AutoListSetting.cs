// File: AutoListSetting.cs
// Namespace: TALOREAL_NETCORE_API
namespace TALOREAL_NETCORE_API {

	/// <summary>
	/// Reports that a stored list changed.
	/// </summary>
	/// <typeparam name="TItem">The element type of the list.</typeparam>
	/// <param name="setting">The AutoListSetting that noticed the change.</param>
	/// <param name="oldValues">The entries before the change; empty on the first write.</param>
	/// <param name="newValues">The entries after the change.</param>
	public delegate void ListChangedHandler<TItem>(AutoListSetting<TItem> setting, TItem[] oldValues, TItem[] newValues);

	/// <summary>
	/// The list counterpart of AutoSetting: a named list that lives in the Settings database,
	/// read through GetList and written through SetList. A missing or unreadable entry reads
	/// as an empty list. Every instance listens to its own name in Settings and raises
	/// Changed, so a subscriber hears about a write made through any instance or through
	/// Settings.SetList directly.
	/// </summary>
	/// <typeparam name="T">The element type; any type Settings supports.</typeparam>
	public class AutoListSetting<T> {

		/// <summary>
		/// Keeps track of what the next auto-assigned name will be (written in hex).
		/// </summary>
		private static uint AutoAssignCounter = 0;

		/// <summary>
		/// The characters used to make up the auto-assigned names.
		/// </summary>
		private const string HexCharacters = "0123456789ABCDEF";


		/// <summary>
		/// Builds the next auto-assigned name: eight hex digits, a space between each pair.
		/// </summary>
		/// <returns>The new name.</returns>
		private static string NextAutoName() {
			string name = "";
			uint remaining = AutoAssignCounter;
			for (int digit = 0; digit < 8; digit++) {
				string separator = digit != 0 && digit % 2 == 0 ? " " : "";
				name = HexCharacters[(int)(remaining % 16)] + separator + name;
				remaining /= 16;
			}
			AutoAssignCounter += 1;
			return name;
		}


		/// <summary>
		/// The name of the list in the database.
		/// Can't be set externally, to prevent pointing at another entry in the database.
		/// </summary>
		public string Name { get; protected set; } = "";

		/// <summary>
		/// Raised after the stored list changes, however the change was made.
		/// </summary>
		public event ListChangedHandler<T>? Changed;

		/// <summary>
		/// A snapshot of the stored list. Reading gives a copy as an array, so changing it
		/// changes nothing in the database; writing replaces the whole stored list. To change
		/// individual entries use Add, AddRange, Remove and RemoveRange.
		/// </summary>
		public T[] Value {
			get { return Load().ToArray(); }
			set { Settings.SetList(Name, value); }
		}

		/// <summary>
		/// How many entries the stored list holds.
		/// </summary>
		public int Count {
			get { return Load().Count; }
		}


		/// <summary>
		/// Creates an AutoListSetting with the specified name, leaving whatever is in the database alone.
		/// </summary>
		/// <param name="name">The name of the list in the database.</param>
		public AutoListSetting(string name) {
			Name = name;
			Settings.ListenTo<List<T>>(Name, OnStoreChanged);
		}

		/// <summary>
		/// Creates an AutoListSetting with a name and a starting list, which is written to the database.
		/// </summary>
		/// <param name="name">The name of the list in the database; auto-assigned when null.</param>
		/// <param name="startingValue">The entries to store now; an empty list when null.</param>
		public AutoListSetting(string? name = null, T[]? startingValue = null) {
			if (name == null) {
				name = NextAutoName();
			}
			Name = name;
			Settings.ListenTo<List<T>>(Name, OnStoreChanged);
			Value = startingValue ?? new T[0];
		}


		/// <summary>
		/// Appends one entry to the stored list.
		/// </summary>
		/// <param name="item">The entry to add.</param>
		public void Add(T item) {
			List<T> values = Load();
			values.Add(item);
			Store(values);
		}

		/// <summary>
		/// Appends several entries to the stored list, in the order given.
		/// </summary>
		/// <param name="items">The entries to add.</param>
		public void AddRange(T[] items) {
			List<T> values = Load();
			values.AddRange(items);
			Store(values);
		}

		/// <summary>
		/// Removes the first entry equal to the one given.
		/// </summary>
		/// <param name="item">The entry to remove.</param>
		/// <returns>True if an entry was removed; false if none matched.</returns>
		public bool Remove(T item) {
			List<T> values = Load();
			bool removed = values.Remove(item);
			if (removed == true) {
				Store(values);
			}
			return removed;
		}

		/// <summary>
		/// Removes a run of entries by position, the same way List.RemoveRange does.
		/// </summary>
		/// <param name="index">The position of the first entry to remove.</param>
		/// <param name="count">How many entries to remove.</param>
		/// <returns>True if the run was removed; false if it did not fit inside the list, in which case nothing changed.</returns>
		public bool RemoveRange(int index, int count) {
			List<T> values = Load();
			bool fits = index >= 0 && count >= 0 && index + count <= values.Count;
			if (fits == true) {
				values.RemoveRange(index, count);
				Store(values);
			}
			return fits;
		}

		/// <summary>
		/// Reads the stored list; empty when nothing is stored.
		/// </summary>
		/// <returns>The stored entries.</returns>
		private List<T> Load() {
			Settings.GetList(Name, out List<T> values);
			return values;
		}

		/// <summary>
		/// Writes the list back to the database.
		/// </summary>
		/// <param name="values">The entries to store.</param>
		private void Store(List<T> values) {
			Settings.SetList(Name, values);
		}

		/// <summary>
		/// Receives the Settings notification for this name and raises Changed with the
		/// entries before and after, as arrays.
		/// </summary>
		/// <param name="key">The Settings key that changed.</param>
		/// <param name="type">The stored type, always List of T here.</param>
		/// <param name="oldValue">The entries before the change, or null on the first write.</param>
		/// <param name="newValue">The entries after the change.</param>
		private void OnStoreChanged(string key, Type type, object? oldValue, object? newValue) {
			ListChangedHandler<T>? handler = Changed;
			if (handler != null) {
				T[] before = CopyEntries(oldValue);
				T[] after = CopyEntries(newValue);
				handler(this, before, after);
			}
		}

		/// <summary>
		/// Copies the entries out of whatever list shape Settings handed over.
		/// </summary>
		/// <param name="entries">A list of T, or null.</param>
		/// <returns>The entries as a new array; empty when there were none.</returns>
		private static T[] CopyEntries(object? entries) {
			T[] copy = new T[0];
			if (entries is IList<T> list) {
				copy = new T[list.Count];
				list.CopyTo(copy, 0);
			}
			return copy;
		}


		/// <summary>
		/// Creates a new AutoListSetting from a name-value pair.
		/// </summary>
		/// <param name="pair">The name and starting entries.</param>
		public static implicit operator AutoListSetting<T>((string? name, T[]? value) pair) {
			return new AutoListSetting<T>(pair.name, pair.value);
		}

		/// <summary>
		/// Casts a name to an AutoListSetting pointing at that entry.
		/// </summary>
		/// <param name="name">The name of the list in the database.</param>
		public static implicit operator AutoListSetting<T>(string name) {
			return new AutoListSetting<T>(name);
		}

		/// <summary>
		/// Gives the name contained in the AutoListSetting.
		/// </summary>
		/// <param name="setting">The AutoListSetting to cast.</param>
		public static implicit operator string(AutoListSetting<T> setting) {
			return setting.Name;
		}
	}
}
