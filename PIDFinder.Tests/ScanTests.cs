using System.Collections.Generic;
using PIDFinder.Lib;
using PKHeX.Core;
using Xunit;

namespace PIDFinder.Tests
{
    /// <summary>并行扫描与单线程扫描必须产出完全一致的结果（方案 C / M3 行为锚点）。</summary>
    public class ScanTests
    {
        public static IEnumerable<object[]> Kinds()
        {
            yield return new object[] { RngKind.Method1 };
            yield return new object[] { RngKind.Overworld8 };
            yield return new object[] { RngKind.Roaming8b };
        }

        [Theory]
        [MemberData(nameof(Kinds))]
        public void ParallelScanMatchesSerial(RngKind kind)
        {
            var rules = new CheckRule { Ability = -1, Gender = -1 };
            uint start = 0x1000_0000, count = 60_000;

            var serial = RngScanner.Scan(kind, start, count, 12345, 54321, rules, parallel: false);
            var parallel = RngScanner.Scan(kind, start, count, 12345, 54321, rules, parallel: true);

            Assert.Equal(serial.Count, parallel.Count);
            for (int i = 0; i < serial.Count; i++)
            {
                Assert.Equal(serial[i].Seed, parallel[i].Seed);
                Assert.Equal(serial[i].PID, parallel[i].PID);
                Assert.Equal(serial[i].EC, parallel[i].EC);
                Assert.Equal(serial[i].IVs, parallel[i].IVs);
                Assert.Equal(serial[i].Nature, parallel[i].Nature);
                Assert.Equal(serial[i].Ability, parallel[i].Ability);
                Assert.Equal(serial[i].ShinyXor, parallel[i].ShinyXor);
            }
        }
    }
}
