using PKHeX.Core;

namespace PKHeX_Hunter_Plugin
{
    internal static class Method1RNG
    {
        public static PkmEntry GenPkm(uint seed, ITrainerID trainer)
        {
            var rng = new Xoroshiro128Plus8b(seed);

            var pidLower = rng.NextUInt();
            var pidUpper = rng.NextUInt();
            var dvLower = rng.NextUInt();
            var dvUpper = rng.NextUInt();

            var pid = combineRNG(pidUpper, pidLower, 16);
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