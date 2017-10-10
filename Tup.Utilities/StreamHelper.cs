using System.IO;

namespace Tup.Utilities
{
    /// <summary>
    /// 流处理 工具类
    /// </summary>
    public static class StreamHelper
    {
        /// <summary>
        /// Read a stream into a byte array
        /// </summary>
        /// <param name="input">Stream to read</param>
        /// <returns>byte[]</returns>
        public static byte[] ReadAsBytes(this Stream input)
        {
            if (input == null)
                return null;

            byte[] buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Copy Stream
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="buffer"></param>
        public static void CopyTo(this Stream source, Stream destination, byte[] buffer = null)
        {
            ThrowHelper.ThrowIfNull(source, "source");
            ThrowHelper.ThrowIfNull(destination, "destination");

            if (buffer == null)
                buffer = new byte[1024];

            ThrowHelper.ThrowIfTrue(buffer.Length < 128, "Buffer is too small-buffer.Length < 128");

            var num = 0;
            var buffLen = buffer.Length;
            while ((num = source.Read(buffer, 0, buffLen)) > 0)
            {
                destination.Write(buffer, 0, num);
            }
            destination.Flush();
        }
    }
}