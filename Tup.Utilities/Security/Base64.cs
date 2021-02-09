using System;

namespace Tup.Utilities
{
    #region Base64 TextEncodings

    /// <summary>
    /// Base64 Text Encodings
    /// </summary>
    /// <remarks>
    /// FROM: https://github.com/aspnet/AspNetKatana/tree/master/src/Microsoft.Owin.Security/DataHandler/Encoder
    /// </remarks>
    public static class Base64TextEncodings
    {
        private static readonly IBase64TextEncoder Base64Instance = new Base64TextEncoder();
        private static readonly IBase64TextEncoder Base64UrlInstance = new Base64UrlTextEncoder();

        public static IBase64TextEncoder Base64
        {
            get { return Base64Instance; }
        }

        public static IBase64TextEncoder Base64Url
        {
            get { return Base64UrlInstance; }
        }
    }

    public interface IBase64TextEncoder
    {
        string Encode(byte[] data);

        byte[] Decode(string text);
    }

    /// <summary>
    /// Base64 Url Text Encoder
    /// </summary>
    internal class Base64UrlTextEncoder : IBase64TextEncoder
    {
        public string Encode(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        public byte[] Decode(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Convert.FromBase64String(Pad(text.Replace('-', '+').Replace('_', '/')));
        }

        private static string Pad(string text)
        {
            var padding = 3 - ((text.Length + 3) % 4);
            if (padding == 0)
            {
                return text;
            }
            return text + new string('=', padding);
        }
    }

    /// <summary>
    /// Base64 Text Encoder
    /// </summary>
    internal class Base64TextEncoder : IBase64TextEncoder
    {
        public string Encode(byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        public byte[] Decode(string text)
        {
            return Convert.FromBase64String(text);
        }
    }

    #endregion
}