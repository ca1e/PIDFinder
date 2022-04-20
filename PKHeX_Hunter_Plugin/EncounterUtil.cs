using PKHeX.Core;
using System.Collections.Generic;

namespace PKHeX_Hunter_Plugin
{
    internal static class EncounterUtil
    {

        /// <summary>
        ///  species: 
        /// </summary>
        /// <param name="species"></param>
        /// <returns></returns>
        public static IEnumerable<IEncounterInfo> SearchEncounters(int species, int form, PKM pkm, GameVersion version)
        {
            pkm.Species = species;
            pkm.Form = form;
            pkm.SetGender(pkm.GetSaneGender());
            return EncounterMovesetGenerator.GenerateEncounters(pkm, null, version);
        }

        public static EncounterCriteria GetCriteria(ISpeciesForm enc, PKM editor)
        {
            //if any then return EncounterCriteria.Unrestricted;

            var tree = EvolutionTree.GetEvolutionTree(editor, editor.Format);
            bool isInChain = tree.IsSpeciesDerivedFrom(editor.Species, editor.Form, enc.Species, enc.Form);

            var set = new ShowdownSet(editor);
            var criteria = EncounterCriteria.GetCriteria(set, editor.PersonalInfo);
            if (!isInChain)
                criteria = criteria with {Gender = -1}; // Genderless tabs and a gendered enc -> let's play safe.
            return criteria;
        }
    }
}
