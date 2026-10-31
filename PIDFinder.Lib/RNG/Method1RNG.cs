using PKHeX.Core;

namespace PIDFinder.Lib
{
    public static class Method1RNG
    {
        private const int shift = 16;

        public static uint Next(uint seed) => LCRNG.Next(seed);

        /// <summary>由种子生成 Gen3 Method 1 结果；不满足筛选条件时返回 null。</summary>
        public static PkmResult Generate(uint seed, ushort tid, ushort sid, CheckRule rules)
        {
            if (rules.Gender != -1)
                throw new System.ArgumentException("Raw Method1 has no species gender context. Use GenerationContext.", nameof(rules));
            var pidLower = LCRNG.Next(seed) >> shift;
            var pidUpper = LCRNG.Advance(seed, 2) >> shift;
            var dvLower = LCRNG.Advance(seed, 3) >> shift;
            var dvUpper = LCRNG.Advance(seed, 4) >> shift;

            var pid = combineRNG(pidUpper, pidLower, shift);
            var rare = GetShinyXor(pid, tid, sid);
            var nature = (int)(pid % 25);
            var ability = (int)(pid & 1);

            // 廉价条件前置：闪光/性格/特性只依赖 PID，失败可跳过 IV 计算与分配
            if (!rules.CheckShiny((int)rare, 3))
                return null;
            if (!rules.CheckNature(nature))
                return null;
            if (!rules.CheckAbility(ability))
                return null;

            var ivs = dvsToIVs(dvUpper, dvLower);

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
            if (!rules.CheckShiny((int)rare, 3))
                return null;
            if (!rules.CheckNature((int)nature))
                return null;
            if (!rules.CheckAbility((int)ability))
                return null;

            return new PkmResult
            {
                Seed = seed,
                EC = pid,
                PID = pid,
                ShinyXor = (int)rare,
                IVs = ivs,
                Nature = nature,
                Ability = ability,
            };
        }

        /// <summary>使用模板训练家 ID 生成独立 PK3；成功替换引用。tid/sid 仅保留签名兼容。</summary>
        public static bool TryApplyFromSeed(ref PKM pk, ushort tid, ushort sid, CheckRule rules, uint seed)
        {
            var context = new GenerationContext(pk, RngKind.Method1, rules);
            if (!context.TryGenerate(seed, out var generated)) return false;
            pk = generated;
            return true;
        }

        /// <summary>将结果写入 PKHeX 实体。</summary>
        public static void ApplyTo(PkmResult res, PKM pk)
        {
            if (pk is not PK3) throw new System.ArgumentException("Method1 requires a native PK3.", nameof(pk));
            pk.PID = res.PID; // PK3 nature and gender are derived getters; EC is also PID.
            pk.RefreshAbility((int)(pk.PID & 1));
            pk.IV_HP = res.IVs[0];
            pk.IV_ATK = res.IVs[1];
            pk.IV_DEF = res.IVs[2];
            pk.IV_SPA = res.IVs[3];
            pk.IV_SPD = res.IVs[4];
            pk.IV_SPE = res.IVs[5];
        }

        private static uint GetShinyXor(uint pid, ushort tid, ushort sid)
        {
            return ((uint)(tid ^ sid) ^ ((pid >> 16) ^ (pid & 0xFFFF)));
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
