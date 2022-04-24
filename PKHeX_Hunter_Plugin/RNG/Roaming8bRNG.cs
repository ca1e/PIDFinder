using PKHeX.Core;
using System;

namespace PKHeX_Hunter_Plugin
{
    internal static class Roaming8bRNG
    {
        private const int FlawlessIVs = 3;
        private const int UNSET = -1;

        public static uint Next(uint seed) => (uint)new Xoroshiro128Plus8b(seed).Next();
        
        public static bool TryApplyFromSeed(ref PKM pk, ITrainerID trainer, CheckRule rules, uint seed)
        {
            var xoro = new Xoroshiro128Plus8b(seed);

            // Check PID
            var fakeTID = xoro.NextUInt();
            var pid = xoro.NextUInt();
            var opid = pid;

            pid = GetRevisedPID(fakeTID, pid, trainer);
            var rare = GetShinyXor(opid, fakeTID);

            Span<int> ivs = stackalloc [] { UNSET, UNSET, UNSET, UNSET, UNSET, UNSET };

            var determined = 0;
            while (determined < FlawlessIVs)
            {
                var idx = (int)xoro.NextUInt(6);
                if (ivs[idx] != UNSET) continue;
                ivs[idx] = 31;
                determined++;
            }
            for (var i = 0; i < ivs.Length; i++)
            {
                if (ivs[i] == UNSET)
                {
                    ivs[i] = (int)xoro.NextUInt(32);
                }
            }
            uint ability = xoro.NextUInt(2);
            uint height = xoro.NextUInt(0x81) + xoro.NextUInt(0x80);
            uint weight = xoro.NextUInt(0x81) + xoro.NextUInt(0x80);

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
            if (!rules.CheckShiny((int)rare, 8))
                return false;
            if (!rules.CheckAbility((int)ability))
                return false;

            // fill poke entity
            pk.SetAbilityIndex((int)ability);
            pk.EncryptionConstant = seed;
            pk.PID = pid;
            pk.IV_HP = ivs[0];
            pk.IV_ATK = ivs[1];
            pk.IV_DEF = ivs[2];
            pk.IV_SPA = ivs[3];
            pk.IV_SPD = ivs[4];
            pk.IV_SPE = ivs[5];
            if (pk is IScaledSize s)
            {
                s.HeightScalar = (byte)height;
                s.WeightScalar = (byte)weight;
            }

            // pass the rule check
            return true;
        }

        private static uint GetRevisedPID(uint fakeTID, uint pid, ITrainerID tr)
        {
            var xor = GetShinyXor(pid, fakeTID);
            var newXor = GetShinyXor(pid, (uint)(tr.TID | (tr.SID << 16)));

            var fakeRare = GetRareType(xor);
            var newRare = GetRareType(newXor);

            if (fakeRare == newRare)
                return pid;

            var isShiny = xor < 16;
            if (isShiny)
                return (((uint)(tr.TID ^ tr.SID) ^ (pid & 0xFFFF) ^ (xor == 0 ? 0u : 1u)) << 16) | (pid & 0xFFFF); // force same shiny star type
            return pid ^ 0x1000_0000;
        }

        private static Shiny GetRareType(uint xor) => xor switch
        {
            0 => Shiny.AlwaysSquare,
         < 16 => Shiny.AlwaysStar,
            _ => Shiny.Never,
        };

        private static uint GetShinyXor(uint pid, uint oid)
        {
            var xor = pid ^ oid;
            return (xor ^ (xor >> 16)) & 0xFFFF;
        }
    }
}
