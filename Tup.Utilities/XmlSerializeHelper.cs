using System.IO;
using System.Xml.Serialization;

namespace Tup.Utilities
{
    /// <summary>
    ///     XML 序列化 助手
    /// </summary>
    public static class XmlSerializeHelper
    {
        #region 静态方法

        /// <summary>
        ///     XML 序列化某一类型对象到指定的文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="obj"></param>
        public static void SerializeToXml<T>(string filePath, T obj)
        {
            ThrowHelper.ThrowIfNull(filePath, "filePath");
            ThrowHelper.ThrowIfNull(filePath, "obj");

            using (var writer = new StreamWriter(filePath))
            {
                var xs = new XmlSerializer(typeof(T));
                xs.Serialize(writer, obj);
            }
        }

        /// <summary>
        ///     从某一 XML 文件反序列化到某一类型对象
        /// </summary>
        /// <param name="filePath">待反序列化的 XML 文件名称</param>
        /// <returns></returns>
        public static T DeserializeFromXml<T>(string filePath)
        {
            ThrowHelper.ThrowIfNull(filePath, "filePath");
            ThrowHelper.ThrowIfFalse(File.Exists(filePath), "filePath");

            using (var reader = new StreamReader(filePath))
            {
                var xs = new XmlSerializer(typeof(T));
                return (T)xs.Deserialize(reader);
            }
        }

        #endregion 静态方法
    }
}
