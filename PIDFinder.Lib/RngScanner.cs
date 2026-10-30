using PKHeX.Core;

namespace PIDFinder.Lib
{
    /// <summary>宝可梦生成方式。</summary>
    public enum RngKind
    {
        Method1,
        Overworld8,
        Roaming8b,
    }

    /// <summary>
    /// 批量种子扫描器（M3）。种子空间按块分片，并行时各线程独立推进互不加锁，
    /// 结果按种子排序返回，保证与单线程枚举完全一致；parallel=false 时退化为单线程。
    /// </summary>
    public static class RngScanner
    {
        public const int DefaultChunkSize = 1 << 20;

        public static IReadOnlyList<PkmResult> Scan(RngKind kind, uint startSeed, uint count,
            ushort tid, ushort sid, CheckRule rules, bool parallel = true,
            int chunkSize = DefaultChunkSize, int maxDegreeOfParallelism = -1)
        {
            var chunks = (long)Math.Ceiling(count / (double)chunkSize);
            var lists = new List<PkmResult>[chunks];

            void Render(long c)
            {
                lists[c] = new List<PkmResult>(64);
                uint begin = unchecked(startSeed + (uint)(c * chunkSize));
                uint inChunk = (uint)Math.Min(chunkSize, count - (uint)(c * chunkSize));
                for (uint k = 0; k < inChunk; k++)
                {
                    var res = kind switch
                    {
                        RngKind.Method1 => Method1RNG.Generate(unchecked(begin + k), tid, sid, rules),
                        RngKind.Overworld8 => Overworld8RNG.Generate(unchecked(begin + k), tid, sid, rules),
                        RngKind.Roaming8b => Roaming8bRNG.Generate(unchecked(begin + k), tid, sid, rules),
                        _ => throw new ArgumentException(nameof(kind)),
                    };
                    if (res is not null)
                        lists[c].Add(res);
                }
            }

            if (parallel)
            {
                var opts = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };
                Parallel.For(0L, chunks, opts, Render);
            }
            else
            {
                for (long c = 0; c < chunks; c++)
                    Render(c);
            }

            return lists.SelectMany(l => l!).OrderBy(r => r.Seed).ToArray();
        }
    }
}
