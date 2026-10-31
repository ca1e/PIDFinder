using System;
using System.Collections.Generic;

namespace PIDFinder.Lib
{
    public sealed class CheckRule
    {
        public uint minHP { get; set; }
        public uint maxHP { get; set; } = 31;
        public bool CheckHP(int HP) => (HP >= minHP && HP <= maxHP);

        public uint minAtk { get; set; }
        public uint maxAtk { get; set; } = 31;
        public bool CheckAtk(int Atk) => (Atk >= minAtk && Atk <= maxAtk);

        public uint minDef { get; set; }
        public uint maxDef { get; set; } = 31;
        public bool CheckDef(int Def) => (Def >= minDef && Def <= maxDef);

        public uint minSpA { get; set; }
        public uint maxSpA { get; set; } = 31;
        public bool CheckSpA(int SpA) => (SpA >= minSpA && SpA <= maxSpA);

        public uint minSpD { get; set; }
        public uint maxSpD { get; set; } = 31;
        public bool CheckSpD(int SpD) => (SpD >= minSpD && SpD <= maxSpD);

        public uint minSpe { get; set; }
        public uint maxSpe { get; set; } = 31;
        public bool CheckSpe(int Spe) => (Spe >= minSpe && Spe <= maxSpe);
        
        /// <summary>Any=-1, ordinary slots=0/1, hidden slot=2.</summary>
        public int Ability { get; set; } = -1;
        public bool CheckAbility(int ability)
        {
            if (Ability == -1) return true;
            if (ability == Ability) return true;
            return false;
        }

        public List<int> Natures { get; set; }
        private bool AnyNature => Natures == null || Natures.Count == 0;
        public bool CheckNature(int nature)
        {
            if(AnyNature)return true;
            if (Natures.Contains(nature)) return true;
            return false;
        }

        public int Gender { get; set; } = -1;
        public bool CheckGender(int gender)
        {
            if (Gender == -1) return true;
            if(gender == Gender) return true;
            return false;
        }

        public ShinyType Shiny = ShinyType.Any;
        public bool CheckShiny(int shinyxor, int gen = 7)
        {
            var xornum = (gen >= 6 ? 16 : 8);
            return Shiny switch
            {
                ShinyType.Square => shinyxor == 0,
                ShinyType.Star => shinyxor < xornum && shinyxor != 0,
                ShinyType.Shiny => shinyxor < xornum,
                ShinyType.No => shinyxor >= xornum,
                _ => true,
            };
        }

        /// <summary>验证输入并创建独立快照；调用方可以私有持有快照而继续保留可变规则 API。</summary>
        public CheckRule Snapshot()
        {
            ValidateRange(minHP, maxHP, nameof(minHP));
            ValidateRange(minAtk, maxAtk, nameof(minAtk));
            ValidateRange(minDef, maxDef, nameof(minDef));
            ValidateRange(minSpA, maxSpA, nameof(minSpA));
            ValidateRange(minSpD, maxSpD, nameof(minSpD));
            ValidateRange(minSpe, maxSpe, nameof(minSpe));
            if (Ability < -1 || Ability > 2)
                throw new ArgumentException("Ability must be -1, 0, 1 or 2.", nameof(Ability));
            if (Gender < -1 || Gender > 2)
                throw new ArgumentException("Gender must be -1, 0, 1 or 2.", nameof(Gender));
            if (!Enum.IsDefined(Shiny))
                throw new ArgumentException("Unknown shiny rule.", nameof(Shiny));
            var copy = (CheckRule)MemberwiseClone();
            copy.Natures = Natures == null ? null : new List<int>(Natures);
            if (copy.Natures != null && copy.Natures.Exists(n => n < 0 || n >= 25))
                throw new ArgumentException("Natures must be in 0..24.", nameof(Natures));
            return copy;
        }

        private static void ValidateRange(uint min, uint max, string name)
        {
            if (min > 31 || max > 31 || min > max)
                throw new ArgumentException("IV bounds must be in 0..31 with min <= max.", name);
        }

        /// <summary>检查包含性别的完整实体属性。</summary>
        public bool CheckAll(int[] ivs, int shinyXor, int gen, int nature, int ability, int gender)
            => CheckAll(ivs, shinyXor, gen, nature, ability) && CheckGender(gender);

        /// <summary>检查无性别上下文的原始算法结果；有实体时使用包含 gender 的重载。</summary>
        public bool CheckAll(int[] ivs, int shinyXor, int gen, int nature, int ability)
        {
            return CheckHP(ivs[0]) && CheckAtk(ivs[1]) && CheckDef(ivs[2])
                && CheckSpA(ivs[3]) && CheckSpD(ivs[4]) && CheckSpe(ivs[5])
                && CheckShiny(shinyXor, gen) && CheckNature(nature) && CheckAbility(ability);
        }
    }

    public enum AbilityType : byte
    {
        Any,
        Ability1,
        Ability2,
        AbilityHidden,
    }

    public enum GenderType : byte
    {
        Any,
        Male,
        Female,
        Genderless,
    }

    public enum ShinyType : byte
    {
        Any,
        No,
        Shiny,
        Star,
        Square,
    }
}