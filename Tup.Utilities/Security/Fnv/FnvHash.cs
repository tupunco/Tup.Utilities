using System;
using System.Security.Cryptography;

namespace Tup.Utilities.Fnv
{
    //FROM: https://gist.github.com/RobThree/25d764ea6d4849fdd0c79d15cda27d61

    /// <summary>
    /// HNV Hash 算法
    /// </summary>
    public abstract class FnvHash : HashAlgorithm
    {
        protected const uint FNV32_PRIME = 16777619;
        protected const uint FNV32_OFFSETBASIS = 2166136261;

        protected const ulong FNV64_PRIME = 1099511628211;
        protected const ulong FNV64_OFFSETBASIS = 14695981039346656037;

        public FnvHash(int hashSize)
        {
            this.HashSizeValue = hashSize;
            this.Initialize();
        }
    }

    public class Fnv1Hash32 : FnvHash
    {
        private uint _hash;

        public Fnv1Hash32()
            : base(32) { }

        public override void Initialize()
        {
            _hash = FNV32_OFFSETBASIS;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            for (int i = 0; i < cbSize; i++)
                _hash = (_hash * FNV32_PRIME) ^ array[ibStart + i];
        }

        protected override byte[] HashFinal()
        {
            return BitConverter.GetBytes(_hash);
        }
    }

    public class Fnv1Hash64 : FnvHash
    {
        private ulong _hash;

        public Fnv1Hash64()
            : base(64) { }

        public override void Initialize()
        {
            _hash = FNV64_OFFSETBASIS;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            for (int i = 0; i < cbSize; i++)
                _hash = (_hash * FNV64_PRIME) ^ array[ibStart + i];
        }

        protected override byte[] HashFinal()
        {
            return BitConverter.GetBytes(_hash);
        }
    }

    public class Fnv1aHash32 : FnvHash
    {
        private uint _hash;

        public Fnv1aHash32()
            : base(32) { }

        public override void Initialize()
        {
            _hash = FNV32_OFFSETBASIS;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            for (int i = 0; i < cbSize; i++)
                _hash = (_hash ^ array[ibStart + i]) * FNV32_PRIME;
        }

        protected override byte[] HashFinal()
        {
            return BitConverter.GetBytes(_hash);
        }
    }

    public class Fnv1aHash64 : FnvHash
    {
        private ulong _hash;

        public Fnv1aHash64()
            : base(64) { }

        public override void Initialize()
        {
            _hash = FNV64_OFFSETBASIS;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            for (int i = 0; i < cbSize; i++)
                _hash = (_hash ^ array[ibStart + i]) * FNV64_PRIME;
        }

        protected override byte[] HashFinal()
        {
            return BitConverter.GetBytes(_hash);
        }
    }
}