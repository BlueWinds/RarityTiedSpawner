using System;
using System.Collections.Generic;
using Harmony;
using Localize;
using BattleTech;
using BattleTech.Data;

namespace RarityTiedSpawner {
    [HarmonyPatch(typeof(TagSetQueryExtensions), "GetMatchingUnitDefs")]

    public static class TagSetQueryExtensions_GetMatchingUnitDefs {
        private static Dictionary<UnitDef_MDD, int> numberToAddCache = new Dictionary<UnitDef_MDD, int>();
        private static int numberToAdd(UnitDef_MDD unitDef) {
            if (numberToAddCache.ContainsKey(unitDef)) {
                return numberToAddCache[unitDef];
            }

            Settings s = RTS.settings;
            int toAdd = 0;

            foreach (Tag_MDD tag in unitDef.TagSetEntry.Tags) {
                if (s.moreCommonTags.ContainsKey(tag.Name)) {
                    toAdd += s.moreCommonTags[tag.Name];
                }
            }

            numberToAddCache[unitDef] = toAdd;
            return toAdd;
        }

        private static void Postfix(ref List<UnitDef_MDD> __result) {
            try {
                RTS.modLog.Info?.Write($"Old Length: {__result.Count}");
                foreach (UnitDef_MDD unitDef in __result.ToArray()) {
                    int toAdd = numberToAdd(unitDef);

                    if (toAdd > 0) {
                        RTS.modLog.Info?.Write($"Possible unit: {unitDef.FriendlyName}. Adding {toAdd} to list.");
                        for (int i = 0; i < toAdd; i++) {
                            __result.Add(unitDef);
                        }
                    }
                }
                RTS.modLog.Info?.Write($"New Length: {__result.Count}");
            } catch (Exception e) {
                RTS.modLog.Error?.Write(e);
            }
        }
    }
}
