#nullable enable

namespace PIDFinder.Lib
{
    /// <summary>宝可梦生成方式。</summary>
    public enum RngKind { Method1, Overworld8, Roaming8b }

    /// <summary>候选种子按数值递增，或按所选算法的 Next 推进。</summary>
    public enum SeedTraversal { NumericIncrement, AlgorithmNext }

    /// <summary>有限预算的 raw 种子相关性扫描，不提供遭遇合法性保证。</summary>
    public static class RngScanner
    {
        public const int DefaultChunkSize = 1 << 20;
        // Bound speculative results even if callers request very large chunks.
        private const int MaxBufferedChunkSize = 4096;

        /// <summary>
        /// 扫描 count 个数值递增种子（模 2^32 回绕）。保留遍历顺序的前 maxResults 个匹配，
        /// 最后按 Seed 数值排序；串并行结果一致。chunkSize 是每块的大小上限。
        /// 调用方须在所属线程捕获 UI 规则，且不得在 Snapshot 期间修改规则。
        /// </summary>
        public static IReadOnlyList<PkmResult> Scan(RngKind kind, uint startSeed, uint count,
            ushort tid, ushort sid, CheckRule rules, bool parallel = true,
            int chunkSize = DefaultChunkSize, int maxDegreeOfParallelism = -1,
            CancellationToken cancellationToken = default, int maxResults = int.MaxValue,
            int flawlessIVCount = 3)
        {
            Validate(kind, rules, flawlessIVCount);
            if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize));
            if (maxDegreeOfParallelism != -1 && maxDegreeOfParallelism <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));
            if (maxResults < 0) throw new ArgumentOutOfRangeException(nameof(maxResults));
            var snapshot = rules.Snapshot();
            cancellationToken.ThrowIfCancellationRequested();
            if (count == 0 || maxResults == 0) return Array.Empty<PkmResult>();

            var results = new List<PkmResult>();
            int blockSize = Math.Min(chunkSize, MaxBufferedChunkSize);
            int workers = parallel ? (maxDegreeOfParallelism == -1 ? Environment.ProcessorCount : maxDegreeOfParallelism) : 1;
            // No allocation proportional to count, chunk count, or an unchecked degree request.
            workers = Math.Min(workers, Math.Max(1, Environment.ProcessorCount));
            long offset = 0;
            while (offset < count && results.Count < maxResults)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int batchCount = (int)Math.Min(workers, ((long)count - offset + blockSize - 1) / blockSize);
                int remaining = maxResults - results.Count;
                var blocks = new List<PkmResult>[batchCount];
                long batchStart = offset;
                void Render(int block)
                {
                    var matches = new List<PkmResult>();
                    long begin = batchStart + (long)block * blockSize;
                    long end = Math.Min(begin + blockSize, count);
                    for (long index = begin; index < end && matches.Count < remaining; index++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var result = Generate(kind, unchecked(startSeed + (uint)index), tid, sid, snapshot, flawlessIVCount);
                        if (result is not null) matches.Add(result);
                    }
                    blocks[block] = matches;
                }
                if (parallel)
                    Parallel.For(0, batchCount, new ParallelOptions
                    {
                        MaxDegreeOfParallelism = workers,
                        CancellationToken = cancellationToken,
                    }, Render);
                else
                    Render(0);

                foreach (var block in blocks)
                {
                    foreach (var result in block)
                    {
                        if (results.Count == maxResults) break;
                        results.Add(result);
                    }
                    if (results.Count == maxResults) break;
                }
                offset += (long)batchCount * blockSize;
            }
            cancellationToken.ThrowIfCancellationRequested();
            results.Sort((a, b) => a.Seed.CompareTo(b.Seed));
            cancellationToken.ThrowIfCancellationRequested();
            return results;
        }

        /// <summary>按显式遍历策略检查至多 count 个候选，返回遍历中首个匹配；无匹配返回 null。</summary>
        public static PkmResult? Search(RngKind kind, uint startSeed, uint count,
            ushort tid, ushort sid, CheckRule rules,
            SeedTraversal traversal = SeedTraversal.NumericIncrement,
            CancellationToken cancellationToken = default, int flawlessIVCount = 3)
        {
            Validate(kind, rules, flawlessIVCount);
            if (!Enum.IsDefined(typeof(SeedTraversal), traversal))
                throw new ArgumentOutOfRangeException(nameof(traversal));
            var snapshot = rules.Snapshot();
            cancellationToken.ThrowIfCancellationRequested();
            uint seed = startSeed;
            for (uint index = 0; index < count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = Generate(kind, seed, tid, sid, snapshot, flawlessIVCount);
                if (result is not null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return result;
                }
                seed = traversal == SeedTraversal.NumericIncrement ? unchecked(seed + 1) : kind switch
                {
                    RngKind.Method1 => Method1RNG.Next(seed),
                    RngKind.Overworld8 => Overworld8RNG.Next(seed),
                    RngKind.Roaming8b => Roaming8bRNG.Next(seed),
                    _ => throw new ArgumentOutOfRangeException(nameof(kind)),
                };
            }
            cancellationToken.ThrowIfCancellationRequested();
            return null;
        }

        private static void Validate(RngKind kind, CheckRule rules, int flawlessIVCount)
        {
            if (!Enum.IsDefined(typeof(RngKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
            ArgumentNullException.ThrowIfNull(rules);
            if (flawlessIVCount < 0 || flawlessIVCount > 6)
                throw new ArgumentOutOfRangeException(nameof(flawlessIVCount));
        }

        private static PkmResult? Generate(RngKind kind, uint seed, ushort tid, ushort sid,
            CheckRule rules, int flawlessIVCount) => kind switch
        {
            RngKind.Method1 => Method1RNG.Generate(seed, tid, sid, rules),
            RngKind.Overworld8 => Overworld8RNG.Generate(seed, tid, sid, rules, flawlessIVCount),
            RngKind.Roaming8b => Roaming8bRNG.Generate(seed, tid, sid, rules, flawlessIVCount),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }
}
