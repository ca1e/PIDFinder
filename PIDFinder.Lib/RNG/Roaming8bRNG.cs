using PKHeX.Core;
using System;

namespace PIDFinder.Lib
{
    public static class Roaming8bRNG
    {
        private const int UNSET = -1;

        public static uint Next(uint seed) => (uint)new Xoroshiro128Plus8b(seed).Next();
        
        /// <summary>由种子生成 BDSP 游走生成结果；不满足筛选条件时返回 null。</summary>
        public static PkmResult Generate(uint seed, ushort tid, ushort sid, CheckRule rules, int flawlessIVCount = 3)
        {
            if (rules.Gender != -1 || rules.Natures is { Count: > 0 })
                throw new ArgumentException("Raw Synchronize roaming has no nature or gender context. Use GenerationContext.", nameof(rules));
            if (rules.Ability == 2)
                throw new ArgumentException("Roaming has ordinary ability slots only.", nameof(rules));
            if ((uint)flawlessIVCount > 6)
                throw new ArgumentException("Flawless IV count must be in 0..6.", nameof(flawlessIVCount));
            if (seed == 0x7FFFFFFF) return null;
            var xoro = new Xoroshiro128Plus8b(seed);

            // Check PID
            var fakeTID = xoro.NextUInt();
            var pid = xoro.NextUInt();
            pid = GetRevisedPID(fakeTID, pid, tid, sid);
            var rare = GetShinyXor(pid, (uint)(tid | (sid << 16)));

            Span<int> ivs = stackalloc[] { UNSET, UNSET, UNSET, UNSET, UNSET, UNSET };

            var determined = 0;
            while (determined < flawlessIVCount)
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
                Nature = -1, // Synchronize branch: nature belongs to the template.
                Ability = (int)ability,
                HeightScalar = (byte)height,
                WeightScalar = (byte)weight,
            };
        }

        /// <summary>同步模式，使用模板训练家 ID 生成独立 PB8；成功替换引用。tid/sid 仅保留签名兼容。</summary>
        public static bool TryApplyFromSeed(ref PKM pk, ushort tid, ushort sid, CheckRule rules, uint seed)
        {
            var context = new GenerationContext(pk, RngKind.Roaming8b, rules);
            if (!context.TryGenerate(seed, out var generated)) return false;
            pk = generated;
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
