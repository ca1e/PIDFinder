namespace PIDFinder
{
    public static class Roaming8bRNG
    {
        private const int Species = 488;
        private const int FlawlessIVs = 3;
        private const uint UNSET = 255;
        public static PKM GenPkm(uint seed, TSVID trainer)
        {
            var xoro = new Xoroshiro128Plus8b(seed);
            var fakeTID = xoro.NextUInt();
            var pid = xoro.NextUInt();
            var opid = pid;

            pid = GetRevisedPID(fakeTID, pid, trainer);
            var rare = GetShinyXor(opid, fakeTID);
            var shiny = GetRareType(rare);

            var ivs = new uint[6] { UNSET , UNSET, UNSET, UNSET, UNSET, UNSET };

            var determined = 0;
            while (determined < FlawlessIVs)
            {
                var idx = xoro.NextUInt(6);
                if (ivs[idx] != UNSET) continue;
                ivs[idx] = 31;
                determined++;
            }
            for(var i = 0;i< ivs.Length; i++) {
                if(ivs[i] == UNSET)
                {
                    ivs[i] = xoro.NextUInt(32);
                }
            }
            uint ability = xoro.NextUInt(2);
            uint height = xoro.NextUInt(0x81) + xoro.NextUInt(0x80);
            uint weight = xoro.NextUInt(0x81) + xoro.NextUInt(0x80);

            return new PKM { PID = pid, EC = seed,
                ShinyStatus = shiny,
                ivs = ivs, 
                Ability = ability,
                Height = height,
                Weight = weight
            };
        }

        private static uint GetRevisedPID(uint fakeTID, uint pid, TSVID tr)
        {
            var xor = GetShinyXor(pid, fakeTID);
            var newXor = GetShinyXor(pid, tr.NTID | (tr.NSID << 16));

            var fakeRare = GetRareType(xor);
            var newRare = GetRareType(newXor);

            if (fakeRare == newRare)
                return pid;

            var isShiny = xor < 16;
            if (isShiny)
                return (((uint)(tr.NTID ^ tr.NSID) ^ (pid & 0xFFFF) ^ (xor == 0 ? 0u : 1u)) << 16) | (pid & 0xFFFF); // force same shiny star type
            return pid ^ 0x1000_0000;
        }

        private static Shiny GetRareType(uint xor) => xor switch
        {
            0 => Shiny.AlwaysSquare,
            < 16 => Shiny.AlwaysStar,
            _ => Shiny.Never,
        };

        private static uint GetShinyNum(uint tid, uint sid, uint pid)
        {
            var oid = (sid << 16) | tid;
            return GetShinyXor(pid, oid);
        }

        private static uint GetShinyXor(uint pid, uint oid)
        {
            var xor = pid ^ oid;
            return (xor ^ (xor >> 16)) & 0xFFFF;
        }
    }
}
