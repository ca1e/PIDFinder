using PKHeX.Core;

namespace PKHeX_Hunter_Plugin
{
    internal static class Method1RNG
    {
        private const int shift = 16;

        public static uint Next(uint seed) => RNG.LCRNG.Next(seed);

        public static bool TryApplyFromSeed(ref PKM pk, ITrainerID trainer, CheckRule rules, uint seed)
        {
            var pidLower = RNG.LCRNG.Next(seed) >> shift;
            var pidUpper = RNG.LCRNG.Advance(seed, 2) >> shift;
            var dvLower = RNG.LCRNG.Advance(seed, 3) >> shift;
            var dvUpper = RNG.LCRNG.Advance(seed, 4) >> shift;

            var pid = combineRNG(pidUpper, pidLower, shift);
            var ivs = dvsToIVs(dvUpper, dvLower);
            var rare = GetShinyXor(pid, trainer);
            var nature = pid % 25;
            var ability = pid & 1;


            // check entity
            if (!rules.CheckHP(ivs[0]))
                return false;
            if (!rules.CheckAtk(ivs[1]))
                return false;
            if (!rules.CheckDef(ivs[2]))
                return false;
            if (!rules.CheckSpA(ivs[3]))
                return false;
            if (!rules.CheckSpD(ivs[4]))
                return false;
            if (!rules.CheckSpe(ivs[5]))
                return false;
            if (!rules.CheckShiny((int)rare, 3))
                return false;
            if (!rules.CheckNature((int)nature))
                return false;
            if (!rules.CheckAbility((int)ability))
                return false;

            // fill poke entity
            pk.SetAbilityIndex((int)ability);
            pk.EncryptionConstant = seed;
            pk.PID = pid; // set PID will also change the nature or gender
            pk.IV_HP = ivs[0];
            pk.IV_ATK = ivs[1];
            pk.IV_DEF = ivs[2];
            pk.IV_SPA = ivs[3];
            pk.IV_SPD = ivs[4];
            pk.IV_SPE = ivs[5];
            pk.RefreshChecksum();

            // pass the rule check
            return true;
        }

        private static uint GetShinyXor(uint pid, ITrainerID tr)
        {
            return ((uint)(tr.TID ^ tr.SID) ^ ((pid >> 16) ^ (pid & 0xFFFF)));
        }

        private static int[] dvsToIVs(uint dvUpper, uint dvLower)
        {
            return new int[]
            {
                (int)(dvLower & 0x1f),
                (int)((dvLower & 0x3E0) >> 5),
                (int)((dvLower & 0x7C00) >> 10),
                (int)((dvUpper & 0x3E0) >> 5),
                (int)((dvUpper & 0x7C00) >> 10),
                (int)(dvUpper & 0x1f),
            };
        }
        private static uint combineRNG(uint upper, uint lower, uint shift)
        {
            return (upper << (int)shift) + lower;
        }
    }
}