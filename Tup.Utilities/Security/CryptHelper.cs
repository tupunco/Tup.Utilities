using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using HMACCrypt = System.Security.Cryptography.HMAC;
using MD5Crypt = System.Security.Cryptography.MD5;

namespace Tup.Utilities
{
    /// <summary>
    /// 加密 工具类
    /// </summary>
    public static class CryptHelper
    {
        /// <summary>
        /// UTF8 编码
        /// </summary>
        private static readonly Encoding UTF8Encoding = Encoding.UTF8;

        #region 3DES

        /// <summary>
        /// 3DES 解密数据字符串
        /// </summary>
        /// <param name="base64UrlText"></param>
        /// <param name="sKey"></param>
        /// <returns></returns>
        /// <remarks>
        /// key = sha256(skey, 24)
        /// iv = sub(key, 8)
        /// </remarks>
        public static string TripleDESDecrypt(string base64UrlText, string sKey)
        {
            ThrowHelper.ThrowIfNull(sKey, "sKey");
            if (base64UrlText.IsEmpty())
                return base64UrlText;

            var srcBytes = Base64TextEncodings.Base64Url.Decode(base64UrlText);
            ThrowHelper.ThrowIfNull(srcBytes, "base64UrlText");

            using (var des = new TripleDESCryptoServiceProvider())
            {
                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.PKCS7;

                var shaKey24 = SHA256HashBytes(sKey, des.KeySize / 8); //24 byte
                des.Key = shaKey24;
                des.IV = shaKey24.Length > 8 ? shaKey24.Take(8).ToArray() : shaKey24; // 8 byte

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(srcBytes, 0, srcBytes.Length);
                        cs.FlushFinalBlock();

                        return UTF8Encoding.GetString(ms.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// 3DES 加密字符串数据
        /// </summary>
        /// <param name="text"></param>
        /// <param name="sKey"></param>
        /// <returns>base64Url string</returns>
        /// <remarks>
        /// key = sha256(skey, 24)
        /// iv = sub(key, 8)
        /// </remarks>
        public static string TripleDESEncrypt(string text, string sKey)
        {
            ThrowHelper.ThrowIfNull(sKey, "sKey");
            if (text.IsEmpty())
                return text;

            var srcBytes = UTF8Encoding.GetBytes(text);
            using (var des = new TripleDESCryptoServiceProvider())
            {
                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.PKCS7;

                var shaKey24 = SHA256HashBytes(sKey, des.KeySize / 8); //24 byte
                des.Key = shaKey24;
                des.IV = shaKey24.Length > 8 ? shaKey24.Take(8).ToArray() : shaKey24; // 8 byte

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(srcBytes, 0, srcBytes.Length);
                        cs.FlushFinalBlock();

                        return Base64TextEncodings.Base64Url.Encode(ms.ToArray());
                    }
                }
            }
        }

        #endregion

        #region HMAC

        /// <summary>
        /// HMAC Hash
        /// </summary>
        /// <param name="algorithmName">HMACMD5, HMACSHA1, HMACSHA256, HMACSHA512 ...</param>
        /// <param name="key"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte[] HMAC(string algorithmName, string key, string text)
        {
            ThrowHelper.ThrowIfNull(algorithmName, "algorithmName");
            ThrowHelper.ThrowIfNull(key, "key");
            ThrowHelper.ThrowIfNull(text, "text");

            var bKey = UTF8Encoding.GetBytes(key);
            var buffer = UTF8Encoding.GetBytes(text);
            using (var hashAlgorithm = HMACCrypt.Create(algorithmName))
            {
                hashAlgorithm.Key = bKey;
                return hashAlgorithm.ComputeHash(buffer);
            }
        }

        /// <summary>
        /// HMACSHA256 Hash
        /// </summary>
        /// <param name="key"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte[] HMACSHA256(string key, string text)
        {
            ThrowHelper.ThrowIfNull(key, "key");
            ThrowHelper.ThrowIfNull(text, "text");

            var bKey = UTF8Encoding.GetBytes(key);
            var buffer = UTF8Encoding.GetBytes(text);
            using (var hashAlgorithm = new HMACSHA256(bKey))
            {
                return hashAlgorithm.ComputeHash(buffer);
            }
        }

        /// <summary>
        /// HMACMD5 Hash
        /// </summary>
        /// <param name="key"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte[] HMACMD5(string key, string text)
        {
            ThrowHelper.ThrowIfNull(key, "key");
            ThrowHelper.ThrowIfNull(text, "text");

            var bKey = UTF8Encoding.GetBytes(key);
            var buffer = UTF8Encoding.GetBytes(text);
            using (var hashAlgorithm = new HMACMD5(bKey))
            {
                return hashAlgorithm.ComputeHash(buffer);
            }
        }

        #endregion

        #region SHA256

        /// <summary>
        /// SHA256 Hash
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte[] SHA256Hash(string text)
        {
            ThrowHelper.ThrowIfNull(text, "text");

            var buffer = UTF8Encoding.GetBytes(text);
            using (var hashAlgorithm = SHA256.Create())
            {
                return hashAlgorithm.ComputeHash(buffer); // 32 * 8 = 256
            }
        }

        /// <summary>
        /// Get len SHA256 Bytes
        /// </summary>
        /// <param name="key"></param>
        /// <param name="len">字符串长度: DES:8, 3DSS:24</param>
        /// <returns></returns>
        public static byte[] SHA256HashBytes(string key, int len = 8)
        {
            var shaRes = SHA256Hash(key);
            return shaRes.Take(len).ToArray();
        }

        #endregion

        #region MD5

        /// <summary>
        /// MD5 Hash
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte[] MD5Hash(string text)
        {
            ThrowHelper.ThrowIfNull(text, "text");

            var buffer = UTF8Encoding.GetBytes(text);
            using (var hashAlgorithm = MD5Crypt.Create())
            {
                return hashAlgorithm.ComputeHash(buffer); //16 * 8 = 128
            }
        }

        /// <summary>
        /// MD5 处理, HEX 输出
        /// </summary>
        /// <param name="str">待处理字符</param>
        /// <param name="len">结果位数 8/16/32</param>
        /// <returns>
        ///  0: 0-8
        /// 16: 8-24
        /// </returns>
        public static string MD5(string str, int len = 32)
        {
            if (str.IsEmpty())
                return str;

            var strEncrypt = MD5Hash(str).BinaryToHex();
            if (len == 8)
                strEncrypt = strEncrypt.Substring(0, 8);
            else if (len == 16)
                strEncrypt = strEncrypt.Substring(8, 16);

            return strEncrypt;
        }

        #endregion

        #region SecurityStamp

        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        /// <summary>
        /// 生成一个 Base32 SecurityStamp
        /// </summary>
        /// <param name="len">长度, 取 10 倍数数值</param>
        /// <returns></returns>
        /// <remarks>
        /// https://github.com/aspnet/Identity/blob/master/src/Core/UserManager.cs#L2414
        /// </remarks>
        public static string NewSecurityStamp(int len = 20)
        {
            if (len < 10)
                len = 10;

            byte[] bytes = new byte[10 * (len / 10)];
            _rng.GetBytes(bytes);
            return Base32.ToBase32(bytes);
        }

        /// <summary>
        /// 生成一个 Bytes SecurityStamp
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public static byte[] NewSecurityStamp2(int len = 20)
        {
            if (len < 10)
                len = 10;

            byte[] bytes = new byte[10 * (len / 10)];
            _rng.GetBytes(bytes);
            return bytes;
        }

        /// <summary>
        /// 生成 随机 SecurityStamp
        /// </summary>
        /// <param name="type">
        /// 0/默认: MD5_32, 1: Base64Url, 2: Base32
        /// </param>
        /// <param name="len">长度, base32 取 10 倍数数值</param>
        /// <returns></returns>
        public static string GenSecurityStamp(int type = 0, int len = 30)
        {
            switch (type)
            {
                case 1: //Base64Url
                    return Base64TextEncodings.Base64Url.Encode(NewSecurityStamp2(len));

                case 2: //Base32
                    return NewSecurityStamp(len);

                default: //MD5_32
                    return MD5(NewSecurityStamp(len));
            }
        }

        #endregion
    }
}