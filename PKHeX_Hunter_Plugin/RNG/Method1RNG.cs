using PKHeX.Core;

namespace PKHeX_Hunter_Plugin
{
    internal static class Method1RNG
    {
        private const int shift = 16;

        public static uint Next(uint seed) => RNG.LCRNG.Next(seed);

        public static PkmEntry GenPkm(uint seed, ITrainerID trainer)
        {
            var pidLower = RNG.LCRNG.Next(seed) >> shift;
            var pidUpper = RNG.LCRNG.Advance(seed, 2) >> shift;
            var dvLower = RNG.LCRNG.Advance(seed, 3) >> shift;
            var dvUpper = RNG.LCRNG.Advance(seed, 4) >> shift;

            var pid = combineRNG(pidUpper, pidLower, shift);
            var ivs = dvsToIVs(dvUpper, dvLower);
            var rare = GetShinyXor(pid, trainer);

            return new PkmEntry
            {
                PID = pid,
                ShinyStatus = (int)rare,
                ivs = ivs,
                Nature = pid % 25,
                Ability = pid & 1,
            };
        }

        private static uint GetShinyXor(uint pid, ITrainerID tr)
        {
            return ((uint)(tr.TID ^ tr.SID) ^ ((pid >> 16) ^ (pid & 0xFFFF)));
        }

        private static uint[] dvsToIVs(uint dvUpper, uint dvLower)
        {
            return new uint[]
            {
                dvLower & 0x1f,
                (dvLower & 0x3E0) >> 5,
                (dvLower & 0x7C00) >> 10,
                (dvUpper & 0x3E0) >> 5,
                (dvUpper & 0x7C00) >> 10,
                dvUpper & 0x1f,
            };
        }
        private static uint combineRNG(uint upper, uint lower, uint shift)
        {
            return (upper << (int)shift) + lower;
        }
    }
}