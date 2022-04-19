using System.Runtime.CompilerServices;

namespace PIDFinder
{
    internal class GenericLCRNG
    {
        private uint add, mult;
        public uint Seed { get; set; }
        private int shift;

        public GenericLCRNG(uint seed, uint mult, uint add)
        {
            Seed = seed;

            this.mult = mult;
            this.add = add;
            shift = 16;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Next()
        {
            Seed = Seed * mult + add;
            return Seed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual uint NextUInt() => Next() >> shift;
    }

    class PokeRNG : GenericLCRNG
    {
        public PokeRNG(uint seed)
            : base(seed, 0x41c64e6d, 0x6073)
        {
        }
    }
}
