using PKHeX.Core;
using System.Collections.Generic;

namespace PKHeX_Hunter_Plugin
{
    internal static class EncounterUtil
    {

        /// <summary>
        ///  species: 481/488
        /// </summary>
        /// <param name="species"></param>
        /// <returns></returns>
        public static IEnumerable<IEncounterInfo> SearchEncounters(PKM pkm, GameVersion version, int species)
        {
            pkm.Species = species;
            pkm.Form = 0;
            pkm.SetGender(pkm.GetSaneGender());
            return EncounterMovesetGenerator.GenerateEncounters(pkm, null, version);
        }

        public static EncounterCriteria GetCriteria(PKM editor)
        {
            //return EncounterCriteria.Unrestricted;
            var set = new ShowdownSet(editor);
            return EncounterCriteria.GetCriteria(set, editor.PersonalInfo);
        }
    }
}
