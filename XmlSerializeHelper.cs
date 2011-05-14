using System;
using System.IO;
using System.Xml.Serialization;

namespace Tup.Utilities
{
    /// <summary>
    /// XML 序列化 助手
    /// </summary>
    public static class XmlSerializeHelper
    {
        #region 静态方法
        /// <summary>
        /// XML 序列化某一类型对象到指定的文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="obj"></param>
        /// <exception cref="ArgumentNullException">filePath arg is null</exception>
        /// <exception cref="ArgumentNullException">obj arg is null</exception>
        public static void SerializeToXml<T>(string filePath, T obj)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath", "filePath arg is null");
            if (obj == null)
                throw new ArgumentNullException("obj", "filePath arg is null");

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                xs.Serialize(writer, obj);
            }
        }
        /// <summary>
        /// 从某一 XML 文件反序列化到某一类型对象
        /// </summary>
        /// <param name="filePath">待反序列化的 XML 文件名称</param>
        /// <param name="type">反序列化出的</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">filePath arg is null</exception>
        public static T DeserializeFromXml<T>(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath", "filePath arg is null");
            if (!File.Exists(filePath))
                throw new FileNotFoundException("filePath File not found");

            using (StreamReader reader = new StreamReader(filePath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                return (T)xs.Deserialize(reader);
            }
        }
        #endregion
    }
}
