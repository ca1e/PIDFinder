#nullable enable

using System;
using System.Linq;
using System.Threading;
using PIDFinder.Lib;
using Xunit;

namespace PIDFinder.Tests
{
    public class ScannerTests
    {
        private static CheckRule Any() => new() { Ability = -1, Gender = -1 };

        [Theory]
        [InlineData(RngKind.Method1)]
        [InlineData(RngKind.Overworld8)]
        [InlineData(RngKind.Roaming8b)]
        public void ManySmallChunksMatchSerialIncludingEveryResultField(RngKind kind)
        {
            var serial = RngScanner.Scan(kind, 1234, 1027, 12345, 54321, Any(), false, 17);
            var parallel = RngScanner.Scan(kind, 1234, 1027, 12345, 54321, Any(), true, 17, 2);
            Assert.Equal(1027, serial.Count);
            Assert.Equal(serial.Select(Fingerprint), parallel.Select(Fingerprint));
        }

        private static string Fingerprint(PkmResult r) =>
            $"{r.Seed}:{r.PID}:{r.EC}:{string.Join(',', r.IVs)}:{r.Nature}:{r.Ability}:{r.ShinyXor}:{r.HeightScalar}:{r.WeightScalar}";

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void LimitKeepsEarliestTraversalMatchesThenSortsNumerically(bool parallel)
        {
            var all = RngScanner.Scan(RngKind.Method1, uint.MaxValue - 4, 23, 1, 2, Any(), false, 3);
            var limited = RngScanner.Scan(RngKind.Method1, uint.MaxValue - 4, 23, 1, 2, Any(), parallel, 3, 2, maxResults: 7);
            Assert.Equal(new uint[] { 0, 1, uint.MaxValue - 4, uint.MaxValue - 3, uint.MaxValue - 2, uint.MaxValue - 1, uint.MaxValue }, limited.Select(r => r.Seed));
            Assert.Equal(23, all.Count);
            Assert.Equal(0u, all[0].Seed);
            Assert.Equal(uint.MaxValue, all[all.Count - 1].Seed);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(16)]
        [InlineData(17)]
        [InlineData(18)]
        [InlineData(90)]
        public void FilteredLimitsMatchUnboundedTraversalPrefix(int limit)
        {
            var rules = Any();
            rules.Ability = 1;
            var all = RngScanner.Scan(RngKind.Method1, 100, 409, 1, 2, rules, false, 17);
            var expected = all.Take(limit).Select(Fingerprint);
            foreach (bool parallel in new[] { false, true })
                Assert.Equal(expected, RngScanner.Scan(RngKind.Method1, 100, 409, 1, 2, rules, parallel, 17, 2, maxResults: limit).Select(Fingerprint));
        }

        [Fact]
        public void ZeroBudgetAndZeroLimitProduceNoResults()
        {
            Assert.Empty(RngScanner.Scan(RngKind.Method1, 0, 0, 0, 0, Any()));
            Assert.Empty(RngScanner.Scan(RngKind.Method1, 0, uint.MaxValue, 0, 0, Any(), maxResults: 0));
            Assert.Null(RngScanner.Search(RngKind.Method1, 0, 0, 0, 0, Any()));
        }

        [Fact]
        public void InvalidParametersAreRejectedEvenForZeroCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RngScanner.Scan((RngKind)999, 0, 0, 0, 0, Any()));
            foreach (int chunk in new[] { 0, -1 })
                Assert.Throws<ArgumentOutOfRangeException>(() => RngScanner.Scan(RngKind.Method1, 0, 0, 0, 0, Any(), chunkSize: chunk));
            foreach (int degree in new[] { 0, -2 })
                Assert.Throws<ArgumentOutOfRangeException>(() => RngScanner.Scan(RngKind.Method1, 0, 0, 0, 0, Any(), maxDegreeOfParallelism: degree));
            Assert.Throws<ArgumentOutOfRangeException>(() => RngScanner.Scan(RngKind.Method1, 0, 0, 0, 0, Any(), maxResults: -1));
            Assert.Throws<ArgumentNullException>(() => RngScanner.Scan(RngKind.Method1, 0, 0, 0, 0, null!));
            Assert.Throws<ArgumentOutOfRangeException>(() => RngScanner.Search(RngKind.Method1, 0, 0, 0, 0, Any(), (SeedTraversal)999));
            Assert.Throws<ArgumentOutOfRangeException>(() => RngScanner.Scan(RngKind.Overworld8, 0, 0, 0, 0, Any(), flawlessIVCount: 7));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AlreadyCancelledScanThrowsIncludingEmptyBudget(bool parallel)
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            Assert.ThrowsAny<OperationCanceledException>(() => RngScanner.Scan(RngKind.Method1, 0, 0, 0, 0, Any(), parallel, cancellationToken: cts.Token));
            Assert.ThrowsAny<OperationCanceledException>(() => RngScanner.Search(RngKind.Method1, 0, 0, 0, 0, Any(), cancellationToken: cts.Token));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RunningScanObservesCancellation(bool parallel)
        {
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(25));
            var rules = Any();
            rules.Shiny = ShinyType.Square;
            Assert.ThrowsAny<OperationCanceledException>(() => RngScanner.Scan(RngKind.Method1, 0, uint.MaxValue, 0, 0, rules, parallel, 7, 2, cts.Token));
        }

        [Theory]
        [InlineData(SeedTraversal.NumericIncrement)]
        [InlineData(SeedTraversal.AlgorithmNext)]
        public void SearchReturnsEarliestMatchInExplicitSequence(SeedTraversal traversal)
        {
            var rules = Any();
            uint start = 1234;
            var first = Method1RNG.Generate(start, 1, 2, rules)!;
            rules.Ability = 1 - first.Ability;
            uint seed = start;
            PkmResult? expected = null;
            for (int i = 0; i < 100; i++)
            {
                expected = Method1RNG.Generate(seed, 1, 2, rules);
                if (expected is not null) break;
                seed = traversal == SeedTraversal.NumericIncrement ? unchecked(seed + 1) : Method1RNG.Next(seed);
            }
            Assert.NotNull(expected);
            var actual = RngScanner.Search(RngKind.Method1, start, 100, 1, 2, rules, traversal);
            Assert.NotNull(actual);
            Assert.Equal(Fingerprint(expected!), Fingerprint(actual!));
            Assert.Null(RngScanner.Search(RngKind.Method1, start, 1, 1, 2, rules, traversal));
        }

        [Fact]
        public void HugeRangeWithSmallLimitReturnsWithoutRangeSizedAllocation()
        {
            var result = RngScanner.Scan(RngKind.Method1, 1234, uint.MaxValue, 1, 2, Any(),
                chunkSize: int.MaxValue, maxDegreeOfParallelism: int.MaxValue, maxResults: 1);
            Assert.Single(result);
            Assert.Equal(1234u, result[0].Seed);
        }

        [Fact]
        public void RunningSearchObservesCancellation()
        {
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(25));
            var rules = Any();
            // A deliberately restrictive valid rule keeps the long traversal running
            // until the timer cancels it.
            rules.minHP = rules.minAtk = rules.minDef = rules.minSpA = rules.minSpD = rules.minSpe = 31;
            rules.Shiny = ShinyType.Square;
            Assert.ThrowsAny<OperationCanceledException>(() => RngScanner.Search(
                RngKind.Method1, 0, uint.MaxValue, 0, 0, rules,
                SeedTraversal.AlgorithmNext, cts.Token));
        }

        [Theory]
        [InlineData(RngKind.Overworld8)]
        [InlineData(RngKind.Roaming8b)]
        public void FlawlessCountIsPassedThrough(RngKind kind)
        {
            var result = RngScanner.Search(kind, 1234, 1, 1, 2, Any(), flawlessIVCount: 6);
            Assert.NotNull(result);
            Assert.All(result!.IVs, iv => Assert.Equal(31, iv));
        }
    }
}
