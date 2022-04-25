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
        
        /// <summary>
        /// TODO 1/2,not hidden
        /// </summary>
        public int Ability { get; set; }
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

        public int Gender { get; set; }
        public bool CheckGender(int gender)
        {
            if (Ability == -1) return true;
            if(gender == Gender) return true;
            return false;
        }

        public ShinyType Shiny = ShinyType.Any;
        public bool CheckShiny(int shinyxor, int gen = 7)
        {
            var xornum = (gen >= 7 ? 16 : 8);
            return Shiny switch
            {
                ShinyType.Square => shinyxor == 0,
                ShinyType.Star => shinyxor < xornum && shinyxor != 0,
                ShinyType.Shiny => shinyxor < xornum,
                ShinyType.No => shinyxor >= xornum,
                _ => true,
            };
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