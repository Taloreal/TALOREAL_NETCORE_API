namespace TALOREAL_NETCORE_API {

    public class AutoSetting<T> {

        public string Name { get; protected set; } = "";

        public T? Value {
            get {
                Settings.GetValue(Name, out T? val);
                return val;
            }
            set => Settings.SetValue(Name, value);
        }

        public AutoSetting(string name, T? defVal = default) { 
            Name = name;
            Value = defVal;
        }

    }
}
