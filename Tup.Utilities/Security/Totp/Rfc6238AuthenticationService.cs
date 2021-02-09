// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the MIT License, Version 2.0. See License.txt in the project root for license information.
//FROM: https://github.com/aspnet/AspNetIdentity/blob/master/src/Microsoft.AspNet.Identity.Core/Rfc6238AuthenticationService.cs
//FROM: https://raw.githubusercontent.com/aspnet/AspNetIdentity/master/src/Microsoft.AspNet.Identity.Core/Rfc6238AuthenticationService.cs

using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Tup.Utilities.Totp
{
    internal sealed class SecurityToken
    {
        private readonly byte[] _data;

        public SecurityToken(byte[] data)
        {
            _data = (byte[])data.Clone();
        }

        internal byte[] GetDataNoClone()
        {
            return _data;
        }
    }

    /// <summary>
    /// Rfc6238 AuthenticationService
    /// </summary>
    /// <remarks>
    /// 修改: _timestep 从 180s 修改成 30s
    /// </remarks>
    internal static class Rfc6238AuthenticationService
    {
        private static readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly TimeSpan _timestep = TimeSpan.FromSeconds(30);
        private static readonly Encoding _encoding = new UTF8Encoding(false, true);

        private static int ComputeTotp(HashAlgorithm hashAlgorithm, ulong timestepNumber, string modifier)
        {
            // # of 0's = length of pin
            const int mod = 1000000;

            // See https://tools.ietf.org/html/rfc4226
            // We can add an optional modifier
            var timestepAsBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((long)timestepNumber));
            var hash = hashAlgorithm.ComputeHash(ApplyModifier(timestepAsBytes, modifier));

            // Generate DT string
            var offset = hash[hash.Length - 1] & 0xf;
            Debug.Assert(offset + 4 < hash.Length);
            var binaryCode = (hash[offset] & 0x7f) << 24
                             | (hash[offset + 1] & 0xff) << 16
                             | (hash[offset + 2] & 0xff) << 8
                             | (hash[offset + 3] & 0xff);

            return binaryCode % mod;
        }

        private static byte[] ApplyModifier(byte[] input, string modifier)
        {
            if (String.IsNullOrEmpty(modifier))
                return input;

            var modifierBytes = _encoding.GetBytes(modifier);
            var combined = new byte[checked(input.Length + modifierBytes.Length)];
            Buffer.BlockCopy(input, 0, combined, 0, input.Length);
            Buffer.BlockCopy(modifierBytes, 0, combined, input.Length, modifierBytes.Length);
            return combined;
        }

        // More info: https://tools.ietf.org/html/rfc6238#section-4
        private static ulong GetCurrentTimeStepNumber()
        {
            var delta = DateTime.UtcNow - _unixEpoch;
            return (ulong)(delta.Ticks / _timestep.Ticks);
        }

        /// <summary>
        /// 生成 安全码
        /// </summary>
        /// <param name="securityToken"></param>
        /// <param name="modifier"></param>
        /// <returns></returns>
        public static int GenerateCode(SecurityToken securityToken, string modifier = null)
        {
            if (securityToken == null)
                throw new ArgumentNullException(nameof(securityToken));

            // Allow a variance of no greater than 90 seconds in either direction
            var currentTimeStep = GetCurrentTimeStepNumber();
            using (var hashAlgorithm = new HMACSHA1(securityToken.GetDataNoClone()))
            {
                return ComputeTotp(hashAlgorithm, currentTimeStep, modifier);
            }
        }

        /// <summary>
        /// 验证 安全码
        /// </summary>
        /// <param name="securityToken"></param>
        /// <param name="code"></param>
        /// <param name="modifier"></param>
        /// <returns></returns>
        public static bool ValidateCode(SecurityToken securityToken, int code, string modifier = null)
        {
            if (securityToken == null)
                throw new ArgumentNullException(nameof(securityToken));

            // Allow a variance of no greater than 90 seconds in either direction
            var currentTimeStep = GetCurrentTimeStepNumber();
            using (var hashAlgorithm = new HMACSHA1(securityToken.GetDataNoClone()))
            {
                for (var i = -2; i <= 2; i++)
                {
                    var computedTotp = ComputeTotp(hashAlgorithm, (ulong)((long)currentTimeStep + i), modifier);

                    if (computedTotp == code)
                    {
                        return true;
                    }
                }
            }

            // No match
            return false;
        }
    }
}