using PKHeX.Core;
using System;

namespace PIDFinder.Lib
{
    public static class Roaming8bRNG
    {
        private const int FlawlessIVs = 3;
        private const int UNSET = -1;

        public static uint Next(uint seed) => (uint)new Xoroshiro128Plus8b(seed).Next();
        
        /// <summary>由种子生成 BDSP 游走生成结果；不满足筛选条件时返回 null。</summary>
        public static PkmResult Generate(uint seed, ushort tid, ushort sid, CheckRule rules)
        {
            var xoro = new Xoroshiro128Plus8b(seed);

            // Check PID
            var fakeTID = xoro.NextUInt();
            var pid = xoro.NextUInt();
            var opid = pid;
            pid = GetRevisedPID(fakeTID, pid, tid, sid);
            var rare = GetShinyXor(opid, fakeTID);

            Span<int> ivs = stackalloc[] { UNSET, UNSET, UNSET, UNSET, UNSET, UNSET };

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
                return null;
            if (!rules.CheckAtk(ivs[1]))
                return null;
            if (!rules.CheckDef(ivs[2]))
                return null;
            if (!rules.CheckSpA(ivs[3]))
                return null;
            if (!rules.CheckSpD(ivs[4]))
                return null;
            if (!rules.CheckSpe(ivs[5]))
                return null;
            if (!rules.CheckShiny((int)rare, 8))
                return null;
            if (!rules.CheckAbility((int)ability))
                return null;

            return new PkmResult
            {
                Seed = seed,
                EC = seed,
                PID = pid,
                ShinyXor = (int)rare,
                IVs = ivs.ToArray(),
                Nature = (int)(pid % 25),
                Ability = (int)ability,
                HeightScalar = (byte)height,
                WeightScalar = (byte)weight,
            };
        }

        /// <summary>由种子在 PKHeX 实体上应用 BDSP 游走结果（保持原有写入副作用顺序）。</summary>
        public static bool TryApplyFromSeed(ref PKM pk, ushort tid, ushort sid, CheckRule rules, uint seed)
        {
            var res = Generate(seed, tid, sid, rules);
            if (res is null)
                return false;

            pk.SetAbilityIndex(res.Ability);
            pk.EncryptionConstant = res.EC;
            pk.PID = res.PID;
            pk.IV_HP = res.IVs[0];
            pk.IV_ATK = res.IVs[1];
            pk.IV_DEF = res.IVs[2];
            pk.IV_SPA = res.IVs[3];
            pk.IV_SPD = res.IVs[4];
            pk.IV_SPE = res.IVs[5];
            if (pk is IScaledSize s)
            {
                s.HeightScalar = res.HeightScalar;
                s.WeightScalar = res.WeightScalar;
            }

            return true;
        }

        private static uint GetRevisedPID(uint fakeTID, uint pid, ushort tid, ushort sid)
        {
            var xor = GetShinyXor(pid, fakeTID);
            var newXor = GetShinyXor(pid, (uint)(tid | (sid << 16)));

            var fakeRare = GetRareType(xor);
            var newRare = GetRareType(newXor);

            if (fakeRare == newRare)
                return pid;

            var isShiny = xor < 16;
            if (isShiny)
                return (((uint)(tid ^ sid) ^ (pid & 0xFFFF) ^ (xor == 0 ? 0u : 1u)) << 16) | (pid & 0xFFFF); // force same shiny star type
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
