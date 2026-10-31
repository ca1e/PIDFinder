using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PIDFinder.Lib;
using PKHeX.Core;
using Xunit;
using Method1 = PIDFinder.Lib.Method1RNG;
using Overworld = PIDFinder.Lib.Overworld8RNG;
using Roaming = PIDFinder.Lib.Roaming8bRNG;

namespace PIDFinder.Tests.Algorithms
{
    public class GenerationTests
    {
        private const uint Seed = 0x1234ABCD;
        private static PK3 Gen3(ushort species = 25) => new() { Species = species, Version = GameVersion.E, TID16 = 12345, SID16 = 54321 };
        private static PK8 Sword() => new() { Species = 25, Version = GameVersion.SW, Nature = Nature.Adamant, StatAlignment = Nature.Modest, Gender = 1, AbilityNumber = 1, TID16 = 12345, SID16 = 54321 };
        private static PB8 Pearl(ushort species = 488) => new() { Species = species, Version = GameVersion.SP, Nature = Nature.Adamant, StatAlignment = Nature.Modest, Gender = (byte)(species == 488 ? 1 : 2), AbilityNumber = 1, TID16 = 12345, SID16 = 54321 };
        private static int[] IVs(PKM pk) => new[] { pk.IV_HP, pk.IV_ATK, pk.IV_DEF, pk.IV_SPA, pk.IV_SPD, pk.IV_SPE };
        private static void EqualGenerated(PKM expected, PKM actual)
        {
            Assert.Equal(expected.EncryptionConstant, actual.EncryptionConstant);
            Assert.Equal(expected.PID, actual.PID);
            Assert.Equal(expected.ShinyXor, actual.ShinyXor);
            Assert.Equal(IVs(expected), IVs(actual));
            Assert.Equal(expected.AbilityNumber, actual.AbilityNumber);
            Assert.Equal(expected.Ability, actual.Ability);
            Assert.Equal(expected.Nature, actual.Nature);
            Assert.Equal(expected.StatAlignment, actual.StatAlignment);
            Assert.Equal(expected.Gender, actual.Gender);
            if (expected is IScaledSize e && actual is IScaledSize a)
            {
                Assert.Equal(e.HeightScalar, a.HeightScalar);
                Assert.Equal(e.WeightScalar, a.WeightScalar);
            }
        }

        [Fact]
        public void RulesDefaultAnyAndSnapshotOwnsNatureList()
        {
            var rules = new CheckRule();
            Assert.Equal(-1, rules.Ability); Assert.Equal(-1, rules.Gender);
            Assert.True(rules.CheckGender(2));
            rules.Ability = 0; Assert.True(rules.CheckGender(1));
            rules.Gender = 1; rules.Ability = -1; Assert.False(rules.CheckGender(0));
            rules.Natures = new List<int> { 3 };
            var copy = rules.Snapshot();
            rules.Natures[0] = 4; rules.minHP = 20;
            Assert.Equal(3, copy.Natures[0]); Assert.Equal(0u, copy.minHP);
            copy.Natures.Add(5); Assert.Single(rules.Natures);
            Assert.False(copy.CheckAll(new int[6], 20, 8, 3, 0, 0));
        }

        [Theory]
        [InlineData(32u, 32u)] [InlineData(31u, 30u)] [InlineData(0u, 32u)]
        public void InvalidIVBoundsThrow(uint min, uint max)
        {
            Assert.Throws<ArgumentException>(() => new CheckRule { minHP = min, maxHP = max }.Snapshot());
            Assert.Throws<ArgumentException>(() => new CheckRule { minAtk = min, maxAtk = max }.Snapshot());
            Assert.Throws<ArgumentException>(() => new CheckRule { minDef = min, maxDef = max }.Snapshot());
            Assert.Throws<ArgumentException>(() => new CheckRule { minSpA = min, maxSpA = max }.Snapshot());
            Assert.Throws<ArgumentException>(() => new CheckRule { minSpD = min, maxSpD = max }.Snapshot());
            Assert.Throws<ArgumentException>(() => new CheckRule { minSpe = min, maxSpe = max }.Snapshot());
        }

        [Theory]
        [InlineData(5, false)] [InlineData(6, true)] [InlineData(8, true)]
        public void ShinyThresholdChangesAtGen6(int gen, bool shiny)
            => Assert.Equal(shiny, new CheckRule { Shiny = ShinyType.Shiny }.CheckShiny(8, gen));

        [Theory]
        [InlineData(-1)] [InlineData(7)]
        public void InvalidFlawlessCountThrows(int count)
        {
            Assert.Throws<ArgumentException>(() => new GenerationContext(Sword(), RngKind.Overworld8, new(), count));
            Assert.Throws<ArgumentException>(() => Overworld.Generate(Seed, 1, 2, new(), count));
            Assert.Throws<ArgumentException>(() => Roaming.Generate(Seed, 1, 2, new(), count));
        }

        [Fact]
        public void ContextRejectsUnsupportedAndImpossibleInputs()
        {
            Assert.Throws<ArgumentException>(() => new GenerationContext(Sword(), RngKind.Method1, new()));
            Assert.Throws<ArgumentException>(() => new GenerationContext(Pearl(), RngKind.Overworld8, new()));
            var wrongVersion = Sword(); wrongVersion.Version = GameVersion.BD;
            Assert.Throws<ArgumentException>(() => new GenerationContext(wrongVersion, RngKind.Overworld8, new()));
            Assert.Throws<ArgumentException>(() => new GenerationContext(Sword(), RngKind.Roaming8b, new()));
            Assert.Throws<ArgumentException>(() => new GenerationContext(Pearl(25), RngKind.Roaming8b, new()));
            var badGender = Pearl(); badGender.Gender = 0;
            Assert.Throws<ArgumentException>(() => new GenerationContext(badGender, RngKind.Roaming8b, new()));
            Assert.Throws<ArgumentException>(() => new GenerationContext(Pearl(), RngKind.Roaming8b, new() { Ability = 2 }));
            Assert.Throws<ArgumentException>(() => new GenerationContext(Gen3(), RngKind.Method1, new() { Ability = 2 }));
            Assert.Throws<ArgumentException>(() => new GenerationContext(Gen3(81), RngKind.Method1, new() { Gender = 0 }));
            Assert.Throws<ArgumentException>(() => new GenerationContext(Gen3(), RngKind.Method1, new() { Gender = 2 }));
            Assert.Throws<ArgumentException>(() => new GenerationContext(Sword(), RngKind.Overworld8, new() { Ability = 1 }));
            Assert.Throws<ArgumentException>(() => new GenerationContext(Pearl(), RngKind.Roaming8b, new() { Gender = 0 }));
            Assert.Throws<ArgumentException>(() => new GenerationContext(Sword(), RngKind.Overworld8, new() { Natures = new() { 0 } }));
            Assert.Throws<ArgumentException>(() => new GenerationContext(Pearl(), RngKind.Roaming8b, new() { Natures = new() { 0 } }));
            Assert.Throws<ArgumentException>(() => new GenerationContext(Sword(), RngKind.Overworld8,
                new() { maxHP = 30, maxAtk = 30, maxDef = 30, maxSpA = 30, maxSpD = 30, maxSpe = 30 }));
        }

        [Fact]
        public void ContextOwnsTemplateAndRulesAndFailuresHaveNoEffects()
        {
            var pk = Sword(); var original = pk.Clone();
            var rules = new CheckRule { Natures = new() { (int)pk.Nature } };
            var ctx = new GenerationContext(pk, RngKind.Overworld8, rules);
            Assert.True(ctx.TryGenerate(Seed, out var first));
            EqualGenerated(original, pk);
            pk.TID16 = 9; pk.PID = 1; pk.Nature = Nature.Timid;
            rules.Natures.Clear(); rules.minHP = 31; rules.Ability = 2;
            first.PID = 0;
            Assert.True(ctx.TryGenerate(Seed, out var next));
            Assert.Equal(original.TID16, next.TID16); Assert.Equal(original.Nature, next.Nature);
            Assert.NotEqual(first.PID, next.PID);
            Assert.True(new GenerationContext(original, RngKind.Overworld8, new(), 0).TryGenerate(Seed, out var zero));
            var excludedHP = zero.IV_HP == 31 ? 0u : 31u;
            var failed = new GenerationContext(original, RngKind.Overworld8,
                new() { minHP = excludedHP, maxHP = excludedHP }, 0);
            Assert.False(failed.TryGenerate(Seed, out var none)); Assert.Null(none);
            Assert.Equal(0u, original.PID);
        }

        [Theory]
        [InlineData(25)] [InlineData(81)] [InlineData(29)] [InlineData(32)] [InlineData(280)]
        public void NativePK3UsesFinalPidAbilityAndGender(ushort species)
        {
            var template = Gen3(species);
            for (uint seed = 0; seed < 20; seed++)
            {
                var ctx = new GenerationContext(template, RngKind.Method1, new());
                Assert.True(ctx.TryGenerate(seed, out var result));
                Assert.IsType<PK3>(result);
                var state = LCRNG.Next(seed); var lo = state >> 16;
                state = LCRNG.Next(state); var pid = (state & 0xFFFF0000) | lo;
                Assert.Equal(pid, result.PID); Assert.Equal(pid, result.EncryptionConstant);
                Assert.Equal((Nature)(pid % 25), result.Nature);
                Assert.Equal(EntityGender.GetFromPID(species, pid), result.Gender);
                var expected = template.Clone(); expected.PID = pid; expected.RefreshAbility((int)(pid & 1));
                Assert.Equal(expected.Ability, result.Ability); Assert.Equal(expected.AbilityNumber, result.AbilityNumber);
                Assert.Equal((uint)(template.TID16 ^ template.SID16 ^ (pid >> 16) ^ (pid & 0xFFFF)), result.ShinyXor);
                var matching = new CheckRule { Ability = result.AbilityNumber == 2 ? 1 : 0, Gender = result.Gender };
                Assert.True(new GenerationContext(template, RngKind.Method1, matching).TryGenerate(seed, out _));
            }
            Assert.Equal(0u, template.PID);
        }

        [Theory]
        [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)] [InlineData(6)]
        public void OverworldMatchesPKHeXIndependentImplementation(int flawless)
        {
            // PKHeX's seed entrypoint is private; invoke that actual implementation rather than copying its sequence.
            var method = typeof(PKHeX.Core.Overworld8RNG).GetMethod("TryApplyFromSeed", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);
            foreach (uint seed in new uint[] { 0, Seed, 0xDEADBEEF, 0x80000000 })
            {
                var template = Sword(); template.RefreshAbility(1);
                var expected = (PK8)template.Clone();
                Assert.True((bool)method.Invoke(null, new object[] { expected, EncounterCriteria.Unrestricted, Shiny.Random, flawless, seed }));
                var context = new GenerationContext(template, RngKind.Overworld8, new(), flawless);
                Assert.True(context.TryGenerate(seed, out var result));
                EqualGenerated(expected, result);
                Assert.True(IVs(result).Count(v => v == 31) >= flawless);
                Assert.True(PKHeX.Core.Overworld8RNG.ValidateOverworldEncounter(result, seed, Shiny.Random, flawless));
                Assert.True(result.ChecksumValid);
            }
        }

        [Theory]
        [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)] [InlineData(6)]
        public void SynchronizeRoamingMatchesPKHeXIndependentImplementation(int flawless)
        {
            foreach (ushort species in new ushort[] { 481, 488 })
            foreach (uint seed in new uint[] { 0, Seed, 0xDEADBEEF, 0x80000000 })
            {
                var template = Pearl(species); var expected = (PB8)template.Clone();
                Assert.True(PKHeX.Core.Roaming8bRNG.TryApplyFromSeed(expected, EncounterCriteria.Unrestricted, Shiny.Random, flawless, seed));
                Assert.True(new GenerationContext(template, RngKind.Roaming8b, new(), flawless).TryGenerate(seed, out var result));
                EqualGenerated(expected, result);
                Assert.True(IVs(result).Count(v => v == 31) >= flawless);
                Assert.True(PKHeX.Core.Roaming8bRNG.ValidateRoamingEncounter(result, Shiny.Random, flawless));
                Assert.True(result.ChecksumValid);
                var raw = Roaming.Generate(seed, template.TID16, template.SID16, new(), flawless);
                Assert.Equal((int)result.ShinyXor, raw.ShinyXor); Assert.Equal(-1, raw.Nature);
            }
        }

        [Fact]
        public void Gen8PreservesIndependentNatureAndOverworldAbility()
        {
            var a = Sword(); var b = Sword(); b.Nature = Nature.Timid; b.RefreshAbility(1);
            Assert.True(new GenerationContext(a, RngKind.Overworld8, new()).TryGenerate(Seed, out var ra));
            Assert.True(new GenerationContext(b, RngKind.Overworld8, new()).TryGenerate(Seed, out var rb));
            Assert.Equal(ra.PID, rb.PID); Assert.NotEqual(ra.Nature, rb.Nature); Assert.NotEqual(ra.AbilityNumber, rb.AbilityNumber);
            Assert.Equal(-1, Overworld.Generate(Seed, 1, 2, new()).Nature);
            Assert.Equal(-1, Overworld.Generate(Seed, 1, 2, new()).Ability);
            Assert.False(new GenerationContext(Pearl(), RngKind.Roaming8b, new()).TryGenerate(0x7FFFFFFF, out var none));
            Assert.Null(none); Assert.Null(Roaming.Generate(0x7FFFFFFF, 1, 2, new()));
        }

        [Fact]
        public void RawResultsRejectRulesThatNeedTemplateContext()
        {
            Assert.Throws<ArgumentException>(() => Method1.Generate(Seed, 1, 2, new() { Gender = 0 }));
            Assert.Throws<ArgumentException>(() => Overworld.Generate(Seed, 1, 2, new() { Gender = 0 }));
            Assert.Throws<ArgumentException>(() => Overworld.Generate(Seed, 1, 2, new() { Ability = 0 }));
            Assert.Throws<ArgumentException>(() => Overworld.Generate(Seed, 1, 2, new() { Natures = new() { 0 } }));
            Assert.Throws<ArgumentException>(() => Roaming.Generate(Seed, 1, 2, new() { Gender = 1 }));
            Assert.Throws<ArgumentException>(() => Roaming.Generate(Seed, 1, 2, new() { Natures = new() { 0 } }));
            Assert.Throws<ArgumentException>(() => Roaming.Generate(Seed, 1, 2, new() { Ability = 2 }));
            var raw = Method1.Generate(Seed, 1, 2, new());
            Assert.Equal(raw.PID, raw.EC);
        }

        [Theory]
        [InlineData(RngKind.Method1, 0, ShinyType.Square)]
        [InlineData(RngKind.Method1, 1, ShinyType.Star)]
        [InlineData(RngKind.Method1, 8, ShinyType.No)]
        [InlineData(RngKind.Overworld8, 0, ShinyType.Square)]
        [InlineData(RngKind.Overworld8, 1, ShinyType.Star)]
        [InlineData(RngKind.Overworld8, 16, ShinyType.No)]
        public void ShinyFilteringUsesFinalTemplateTrainer(RngKind kind, ushort xor, ShinyType shiny)
        {
            PKM template = kind == RngKind.Method1 ? Gen3() : Sword();
            uint pid = kind == RngKind.Method1 ? Method1.Generate(Seed, 0, 0, new()).PID
                : Overworld.Generate(Seed, 0, 0, new()).PID;
            template.SID16 = (ushort)(template.TID16 ^ (pid >> 16) ^ (pid & 0xFFFF) ^ xor);
            var context = new GenerationContext(template, kind, new() { Shiny = shiny });
            Assert.True(context.TryGenerate(Seed, out var result));
            Assert.Equal((uint)xor, result.ShinyXor); Assert.Equal(template.ID32, result.ID32);
            var rejected = shiny == ShinyType.No ? ShinyType.Shiny : ShinyType.No;
            Assert.False(new GenerationContext(template, kind, new() { Shiny = rejected }).TryGenerate(Seed, out var none));
            Assert.Null(none);
        }

        [Theory]
        [InlineData(ShinyType.Square)] [InlineData(ShinyType.Star)] [InlineData(ShinyType.No)]
        public void RoamingRevisedPidReportsActualXorAndFiltersShiny(ShinyType shiny)
        {
            uint seed;
            for (seed = 0; seed < 1_000_000; seed++)
            {
                var xoro = new Xoroshiro128Plus8b(seed);
                uint value = xoro.NextUInt() ^ xoro.NextUInt();
                uint fakeXor = (value ^ (value >> 16)) & 0xFFFF;
                if (shiny == ShinyType.Square ? fakeXor == 0 : shiny == ShinyType.Star ? fakeXor > 0 && fakeXor < 16 : fakeXor >= 16)
                    break;
            }
            Assert.True(seed < 1_000_000);
            var template = Pearl();
            Assert.True(new GenerationContext(template, RngKind.Roaming8b, new() { Shiny = shiny }).TryGenerate(seed, out var result));
            Assert.True(new CheckRule { Shiny = shiny }.CheckShiny((int)result.ShinyXor, 8));
            Assert.Equal((int)result.ShinyXor, Roaming.Generate(seed, template.TID16, template.SID16, new()).ShinyXor);
            var opposite = shiny == ShinyType.No ? ShinyType.Shiny : ShinyType.No;
            Assert.False(new GenerationContext(template, RngKind.Roaming8b, new() { Shiny = opposite }).TryGenerate(seed, out _));
        }

        [Fact]
        public void LegacyAdapterUsesTemplateTrainerAndDoesNotMutateSource()
        {
            PKM pk = Gen3(); var source = pk;
            Assert.True(Method1.TryApplyFromSeed(ref pk, 1, 2, new(), Seed));
            Assert.NotSame(source, pk); Assert.Equal(0u, source.PID);
            Assert.Equal(source.ID32, pk.ID32);
            Assert.Equal(Method1.Generate(Seed, source.TID16, source.SID16, new()).ShinyXor, (int)pk.ShinyXor);
        }
    }
}
