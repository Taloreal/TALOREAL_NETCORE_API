using System.Xml.Serialization;

namespace TALOREAL_NETCORE_API {

    /// <summary>
    /// A dictionary that can be saved as a XML file.
    /// </summary>
    [XmlRoot("dictionary")] public class SerializableDictionary<TKey, TValue> :
        Dictionary<TKey, TValue>, IXmlSerializable where TKey : notnull where TValue : notnull {

        #region IXmlSerializable Members

        /// <summary>
        /// No XMLScheme info to be had.
        /// </summary>
        /// <returns>null</returns>
        public System.Xml.Schema.XmlSchema? GetSchema() { return null; }

        /// <summary>
        /// Reads the dictionary's XML in.
        /// </summary>
        public void ReadXml(System.Xml.XmlReader reader) {
            XmlSerializer keySerializer = new(typeof(TKey));
            XmlSerializer valueSerializer = new(typeof(TValue));
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty) { return; }
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement) {
                reader.ReadStartElement("item");
                reader.ReadStartElement("key");
                TKey key = (TKey?)keySerializer.Deserialize(reader) ?? throw new NullReferenceException("ERROR: The key cannot be null.");
                reader.ReadEndElement();
                reader.ReadStartElement("value");
                TValue value = (TValue?)valueSerializer.Deserialize(reader) ?? throw new NullReferenceException("ERROR: The value cannot be null.");
                reader.ReadEndElement();
                this.Add(key, value);
                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        /// <summary>
        /// Writes the dictionary to XML.
        /// </summary>
        public void WriteXml(System.Xml.XmlWriter writer) {
            XmlSerializer keySerializer = new(typeof(TKey));
            XmlSerializer valueSerializer = new(typeof(TValue));
            foreach (TKey key in this.Keys) {
                writer.WriteStartElement("item");
                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();
                writer.WriteStartElement("value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }
        #endregion
    }
}
