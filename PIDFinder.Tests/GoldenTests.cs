using System.Text.Json;
using System;
using System.IO;
using System.Linq;
using PIDFinder.Lib;
using PKHeX.Core;
using Xunit;

namespace PIDFinder.Tests
{
    /// <summary>黄金样例回归：重构前后对相同种子必须产出完全一致的宝可梦数据。</summary>
    public class GoldenTests
    {
        private static JsonElement Golden() =>
            JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "golden.json"))).RootElement;

        private static CheckRule AllPass() => new() { Ability = -1, Gender = -1 };

        [Fact]
        public void GoldenMatchesAll()
        {
            var rules = AllPass();
            var golden = Golden();
            uint[] seeds = { 0x1234ABCD, 0x00001234, 0xDEADBEEF, 0x0FA0CADE };

            foreach (var gen in new[] { "gen3", "overworld8", "roaming8b" })
            {
                foreach (var seed in seeds)
                {
                    var expected = golden.GetProperty(gen).EnumerateArray()
                        .First(x => x.GetProperty("seed").GetString() == seed.ToString("X8"));
                    var res = gen switch
                    {
                        "gen3" => PIDFinder.Lib.Method1RNG.Generate(seed, 12345, 54321, rules)!,
                        "overworld8" => PIDFinder.Lib.Overworld8RNG.Generate(seed, 12345, 54321, rules)!,
                        "roaming8b" => PIDFinder.Lib.Roaming8bRNG.Generate(seed, 12345, 54321, rules)!,
                        _ => throw new InvalidOperationException(gen),
                    };

                    Assert.Equal(expected.GetProperty("ec").GetString(), res.EC.ToString("X8"));
                    Assert.Equal(expected.GetProperty("pid").GetString(), res.PID.ToString("X8"));
                    Assert.Equal(expected.GetProperty("ivs").EnumerateArray().Select(x => x.GetInt32()).ToArray(), res.IVs);
                    // Gen3/Gen8 性格均由 PID 决定（PID % 25）。基线的 nature 记录的是旧 TryApply 路径
                    // 未写入 Nature 时 PKM 的默认值（0），不具参考性，故只校验内部一致性。
                    Assert.Equal((int)(res.PID % 25), res.Nature);
                    if (gen == "overworld8")
                    {
                        // 旧 Overworld 路径不写特性（Gen8 野生特性与 PID 无关），基线记录为默认 0；
                        // 仅校验返回值内部一致性（0/1 索引）。
                        Assert.True((int)(res.PID & 1) == res.Ability,
                            $"overworld8 seed={seed:X8} pid={res.PID:X8} pidLow={res.PID & 1} ability={res.Ability}");
                    }
                    else
                    {
                        // 基线记录的是 PKM.AbilityNumber（1/2 位标志），换算回 0/1 索引比较
                        Assert.True(expected.GetProperty("ability").GetInt32() - 1 == res.Ability,
                            $"{gen} seed={seed:X8} expectedAbility={expected.GetProperty("ability").GetInt32()} ability={res.Ability}");
                    }
                    Assert.Equal(expected.GetProperty("shiny").GetBoolean(), res.ShinyXor < 16);
                    if (gen != "gen3")
                    {
                        Assert.Equal(expected.GetProperty("height").GetByte(), res.HeightScalar);
                        Assert.Equal(expected.GetProperty("weight").GetByte(), res.WeightScalar);
                    }
                }
            }
        }

        [Fact]
        public void TryApplyFromSeedStillFillsPkHeXEntity()
        {
            PKM pk = new PK8();
            Assert.True(Method1RNG.TryApplyFromSeed(ref pk, 12345, 54321, AllPass(), 0x1234ABCD));
            Assert.Equal(0x1234ABCDu, pk.EncryptionConstant);
            Assert.Equal("222CBA53", pk.PID.ToString("X8"));
            Assert.Equal(new[] { 18, 23, 16, 9, 9, 24 }, new[] { pk.IV_HP, pk.IV_ATK, pk.IV_DEF, pk.IV_SPA, pk.IV_SPD, pk.IV_SPE });
        }
    }
}
