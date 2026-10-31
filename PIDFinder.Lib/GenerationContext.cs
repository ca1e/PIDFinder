using System;
using PKHeX.Core;

namespace PIDFinder.Lib
{
    /// <summary>
    /// Isolated raw seed-correlation generation using the template's trainer IDs.
    /// This does not initialize or promise legality of a complete encounter.
    /// Overworld preserves template nature, ability and gender; BDSP supports Synchronize only.
    /// </summary>
    public sealed class GenerationContext
    {
        private readonly PKM template;
        private readonly CheckRule rules;
        private readonly CheckRule generationRules;
        private readonly int flawlessIVCount;
        public RngKind Kind { get; }

        public GenerationContext(PKM template, RngKind kind, CheckRule rules, int flawlessIVCount = 3)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            if ((uint)flawlessIVCount > 6)
                throw new ArgumentException("Flawless IV count must be in 0..6.", nameof(flawlessIVCount));
            if (!Enum.IsDefined(kind)) throw new ArgumentException("Unknown RNG kind.", nameof(kind));
            this.template = template.Clone();
            this.rules = rules.Snapshot();
            this.flawlessIVCount = flawlessIVCount;
            Kind = kind;
            ValidateTemplate();
            generationRules = this.rules.Snapshot();
            // Gender needs a species/template; Gen8 nature was checked once above.
            generationRules.Gender = -1;
            if (kind != RngKind.Method1) generationRules.Natures = null;
            // Method1 raw PID parity is not necessarily the final ability slot of a PK3.
            // Overworld ability is preserved by the template and was checked above.
            if (kind is RngKind.Method1 or RngKind.Overworld8) generationRules.Ability = -1;
        }

        private void ValidateTemplate()
        {
            if (template.Species == 0 || template.Species > template.MaxSpeciesID)
                throw new ArgumentException("Template species is not supported by its format.", nameof(template));
            switch (Kind)
            {
                case RngKind.Method1:
                    if (template is not PK3)
                        throw new ArgumentException("Method1 requires a native PK3 template.", nameof(template));
                    if (rules.Ability == 2 || (rules.Ability == 1 && !((PK3)template).PersonalInfo.HasSecondAbility))
                        throw new ArgumentException("The requested ability slot is unavailable in this PK3.", nameof(rules));
                    if (!template.PersonalInfo.IsDualGender && !rules.CheckGender(template.Gender))
                        throw new ArgumentException("The species has a fixed gender incompatible with the rule.", nameof(rules));
                    if (template.PersonalInfo.IsDualGender && rules.Gender == 2)
                        throw new ArgumentException("A dual-gender species cannot be genderless.", nameof(rules));
                    break;
                case RngKind.Overworld8:
                    if (template is not PK8 || template.Version is not (GameVersion.SW or GameVersion.SH))
                        throw new ArgumentException("Overworld8 requires PK8 and Sword/Shield.", nameof(template));
                    ValidatePreservedNature();
                    if (!rules.CheckAbility(GetAbilityIndex(template)) || !rules.CheckGender(template.Gender))
                        throw new ArgumentException("Overworld8 preserves template ability and gender; rules conflict with them.", nameof(rules));
                    if (template.Gender > 2 || (!template.PersonalInfo.IsDualGender &&
                        template.Gender != EntityGender.GetFromPIDAndRatio(0, template.PersonalInfo.Gender)))
                        throw new ArgumentException("Template gender is incompatible with the species.", nameof(template));
                    break;
                case RngKind.Roaming8b:
                    if (template is not PB8 || template.Version is not (GameVersion.BD or GameVersion.SP))
                        throw new ArgumentException("Roaming8b requires PB8 and BD/SP.", nameof(template));
                    if (template.Form != 0 || template.Species is not ((int)Species.Mesprit or (int)Species.Cresselia))
                        throw new ArgumentException("Synchronize roaming supports only Mesprit and Cresselia.", nameof(template));
                    var gender = template.Species == (int)Species.Cresselia ? 1 : 2;
                    if (template.Gender != gender)
                        throw new ArgumentException("The roaming template must have its species' fixed gender.", nameof(template));
                    if (!rules.CheckGender(gender) || rules.Ability == 2)
                        throw new ArgumentException("Roaming has fixed gender and ordinary ability slots only.", nameof(rules));
                    ValidatePreservedNature();
                    break;
            }
            // A guaranteed perfect stat must fit at least this many rule maxima.
            if (Kind != RngKind.Method1)
            {
                var possible = (rules.maxHP == 31 ? 1 : 0) + (rules.maxAtk == 31 ? 1 : 0)
                    + (rules.maxDef == 31 ? 1 : 0) + (rules.maxSpA == 31 ? 1 : 0)
                    + (rules.maxSpD == 31 ? 1 : 0) + (rules.maxSpe == 31 ? 1 : 0);
                if (possible < flawlessIVCount)
                    throw new ArgumentException("IV maxima cannot satisfy the guaranteed flawless count.", nameof(rules));
            }
        }

        private void ValidatePreservedNature()
        {
            if ((uint)template.Nature >= 25)
                throw new ArgumentException("Template nature must be in 0..24.", nameof(template));
            if (!rules.CheckNature((int)template.Nature))
                throw new ArgumentException("This mode preserves template nature; the nature rule excludes it.", nameof(rules));
        }

        internal static int GetAbilityIndex(PKM pk) => pk.AbilityNumber switch
        {
            1 => 0, 2 => 1, 4 => 2,
            _ => throw new ArgumentException("Template ability number must be 1, 2 or 4.", nameof(pk)),
        };

        public bool TryGenerate(uint seed, out PKM result)
        {
            result = null;
            if (Kind == RngKind.Roaming8b && seed == 0x7FFFFFFF) return false;
            var raw = Kind switch
            {
                RngKind.Method1 => Method1RNG.Generate(seed, template.TID16, template.SID16, generationRules),
                RngKind.Overworld8 => Overworld8RNG.Generate(seed, template.TID16, template.SID16, generationRules, flawlessIVCount),
                RngKind.Roaming8b => Roaming8bRNG.Generate(seed, template.TID16, template.SID16, generationRules, flawlessIVCount),
                _ => throw new InvalidOperationException(),
            };
            if (raw == null) return false;
            var pk = template.Clone();
            if (Kind == RngKind.Method1)
                Method1RNG.ApplyTo(raw, pk);
            else
            {
                pk.EncryptionConstant = raw.EC;
                pk.PID = raw.PID;
                pk.IV_HP = raw.IVs[0]; pk.IV_ATK = raw.IVs[1]; pk.IV_DEF = raw.IVs[2];
                pk.IV_SPA = raw.IVs[3]; pk.IV_SPD = raw.IVs[4]; pk.IV_SPE = raw.IVs[5];
                if (Kind == RngKind.Roaming8b) pk.RefreshAbility(raw.Ability);
                if (pk is IScaledSize size)
                {
                    size.HeightScalar = raw.HeightScalar;
                    size.WeightScalar = raw.WeightScalar;
                }
            }
            if (!rules.CheckAll(raw.IVs, (int)pk.ShinyXor, Kind == RngKind.Method1 ? 3 : 8,
                (int)pk.Nature, GetAbilityIndex(pk), pk.Gender)) return false;
            pk.RefreshChecksum();
            result = pk;
            return true;
        }
    }
}
