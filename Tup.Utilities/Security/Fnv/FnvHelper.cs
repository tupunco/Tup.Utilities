using System;
using System.Text;

namespace Tup.Utilities.Fnv
{
    /// <summary>
    /// FNV Helper
    /// </summary>
    public static class FnvHelper
    {
        /// <summary>
        /// FNV1a Hash 32
        /// </summary>
        /// <param name="val">待 Hash 数据</param>
        /// <returns> Bin2Hex(FNVHash(UTF8(val))), UTF8 编码 val 值, Hash 结果 16 进制字符串</returns>
        public static string Fnv1aHash32(this string val)
        {
            return FnvHash("FNV-1a_32", val);
        }

        /// <summary>
        /// FNV1a Hash 64
        /// </summary>
        /// <param name="val">待 Hash 数据</param>
        /// <returns> Bin2Hex(FNVHash(UTF8(val))), UTF8 编码 val 值, Hash 结果 16 进制字符串</returns>
        public static string Fnv1aHash64(this string val)
        {
            return FnvHash("FNV-1a_64", val);
        }

        /// <summary>
        /// FNV Hash
        /// </summary>
        /// <param name="tag">
        ///     FNV-1a_64
        ///     FNV-1a_32
        ///     FNV-1_64
        ///     FNV-1_32
        /// </param>
        /// <param name="val">待 Hash 数据</param>
        /// <returns> Bin2Hex(FNVHash(UTF8(val))), UTF8 编码 val 值, Hash 结果 16 进制字符串</returns>
        public static string FnvHash(string tag, string val)
        {
            if (val.IsEmpty())
                return string.Empty;

            ThrowHelper.ThrowIfNull(tag, "tag");

            FnvHash hashAlgorithm = null;
            switch (tag)
            {
                case "FNV-1a_64":
                    hashAlgorithm = new Fnv1aHash64();
                    break;

                case "FNV-1_64":
                    hashAlgorithm = new Fnv1Hash64();
                    break;

                case "FNV-1a_32":
                    hashAlgorithm = new Fnv1aHash32();
                    break;

                case "FNV-1_32":
                    hashAlgorithm = new Fnv1Hash32();
                    break;

                default:
                    throw new NotImplementedException(tag);
            }

            var bVals = Encoding.UTF8.GetBytes(val);
            using (hashAlgorithm)
            {
                return hashAlgorithm.ComputeHash(bVals).BinaryToHex();
            }
        }
    }
}