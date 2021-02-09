using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Web;

namespace Tup.Utilities.Totp
{
    /// <summary>
    /// TOTP Helper
    /// </summary>
    /// <remarks>
    /// 参考:
    ///     https://github.com/aspnet/AspNetIdentity/blob/master/src/Microsoft.AspNet.Identity.Core/TotpSecurityStampBasedTokenProvider.cs
    ///     https://github.com/aspnet/Identity/blob/master/src/Core/AuthenticatorTokenProvider.cs
    /// </remarks>
    public static class TotpHelper
    {
        /// <summary>
        /// https://github.com/google/google-authenticator/wiki/Key-Uri-Format
        /// https://github.com/aspnet/Identity/blob/master/src/UI/Areas/Identity/Pages/V4/Account/Manage/EnableAuthenticator.cshtml.cs#L92
        /// </summary>
        private const string AuthenicatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}";

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
        /// 生成 QrCodeUri
        /// </summary>
        /// <param name="issuer">发行人 标识</param>
        /// <param name="userId">用户 标识</param>
        /// <param name="base32SecurityStamp">Base32 安全戳</param>
        /// <returns></returns>
        public static string GetGenerateQrCodeUri(string issuer, string userId, string base32SecurityStamp)
        {
            Func<string, string> ueFunc = HttpUtility.UrlEncode;
            return AuthenicatorUriFormat.Fmt(ueFunc(issuer), ueFunc(userId), ueFunc(base32SecurityStamp));
        }

        /// <summary>
        /// Create SecurityToken
        /// </summary>
        /// <param name="base32SecurityStamp">Base32 安全戳</param>
        /// <returns></returns>
        private static SecurityToken CreateSecurityToken(string base32SecurityStamp)
        {
            return new SecurityToken(Base32.FromBase32(base32SecurityStamp));
        }

        /// <summary>
        /// Get User Modifier`
        /// </summary>
        /// <param name="purpose">用途标识</param>
        /// <param name="modifier"></param>
        /// <returns></returns>
        private static string GetUserModifier(string purpose, string modifier = null)
        {
            if (modifier.IsEmpty())
                return null;

            return "Totp:{0}:{1}".Fmt(purpose, modifier);
        }

        /// <summary>
        /// 验证当前用户 Token
        /// </summary>
        /// <param name="base32SecurityStamp">当前用户 安全戳</param>
        /// <param name="token">当前用户 Token</param>
        /// <param name="purpose">用户标识</param>
        /// <param name="modifier">当前用户</param>
        /// <returns></returns>
        public static bool VerifyToken(string base32SecurityStamp, string token, string purpose = null, string modifier = null)
        {
            ThrowHelper.ThrowIfNull(base32SecurityStamp, "securityStamp");
            ThrowHelper.ThrowIfNull(token, "token");

            int code;
            if (!int.TryParse(token, out code))
                return false;

            var securityToken = CreateSecurityToken(base32SecurityStamp);
            modifier = GetUserModifier(purpose, modifier);
            return securityToken != null && Rfc6238AuthenticationService.ValidateCode(securityToken, code, modifier);
        }

        /// <summary>
        /// 验证当前用户 Token
        /// </summary>
        /// <param name="base32SecurityStamp">当前用户 安全戳</param>
        /// <param name="modifier">当前用户</param>
        /// <returns></returns>
        public static string GenerateToken(string base32SecurityStamp, string purpose = null, string modifier = null)
        {
            ThrowHelper.ThrowIfNull(base32SecurityStamp, "securityStamp");

            var securityToken = CreateSecurityToken(base32SecurityStamp);
            modifier = GetUserModifier(purpose, modifier);
            return Rfc6238AuthenticationService.GenerateCode(securityToken, modifier)
                                               .ToString("D6", CultureInfo.InvariantCulture);
        }
    }
}