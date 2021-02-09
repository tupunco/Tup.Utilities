using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Tup.Utilities
{
    /// <summary>
    /// 支持 XML 序列化的泛型 Dictionary 类
    /// http://www.cnblogs.com/facebuyer/archive/2009/05/04/1448511.html
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public SerializableDictionary()
            : base()
        {
        }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary)
            : base(dictionary)
        {
        }

        public SerializableDictionary(IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
        }

        public SerializableDictionary(int capacity)
            : base(capacity)
        {
        }

        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer)
            : base(capacity, comparer)
        {
        }

        protected SerializableDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #region IXmlSerializable Members

        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// 从对象的XML表示形式生成该对象
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));
            if (reader.IsEmptyElement || !reader.Read())
            {
                return;
            }

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item"); //cjb:2009-05-04

                //reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                //reader.ReadEndElement();

                //reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                //reader.ReadEndElement();

                reader.ReadEndElement();

                this.Add(key, value);
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        /// <summary>
        /// 将对象转换为其XML表示形式
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));
            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");

                //writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                //writer.WriteEndElement();

                //writer.WriteStartElement("value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                //writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }

        #endregion
    }
}