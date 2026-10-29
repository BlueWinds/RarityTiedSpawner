using BattleTech.Data;
using HarmonyLib;
using HBS.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace RarityTiedSpawner {
    public static class RarityModifications {
        private static Regex REQUIRED_TAG = new Regex("^RTS_(?<requiredTag>.*?)_unit_rarity_(?<number>-?\\d+)$", RegexOptions.Compiled);

        public static int getNumberToAdd(UnitDef_MDD unitDef, TagSet requiredTags) {
            int toAdd = 0;

            foreach (Tag_MDD tag in unitDef.TagSetEntry.Tags) {
                if (RTS.settings.moreCommonTags.ContainsKey(tag.Name)) {
                    toAdd += RTS.settings.moreCommonTags[tag.Name];
                    continue;
                }

                MatchCollection matches = REQUIRED_TAG.Matches(tag.Name);

                if (matches.Count > 0 && requiredTags.Contains(matches[0].Groups["requiredTag"].Value.ToLower())) {
                    toAdd += int.Parse(matches[0].Groups["number"].Value);
                    continue;
                }
            }
            return toAdd;
        }

        [HarmonyPatch(typeof(TagSetQueryExtensions), "GetMatchingUnitDefs")]
        public static class TagSetQueryExtensions_GetMatchingUnitDefs
        {

            private static long numberFetchingMs = 0;
            private static long totalMs = 0;
            
            private static void Postfix(ref List<UnitDef_MDD> __result, TagSet requiredTags) {
                try
                {
                    numberFetchingMs = 0;
                    totalMs = 0;
                    var watch = Stopwatch.StartNew();
                    RTS.l.Log($"Old Length: {__result.Count}");
                    foreach (UnitDef_MDD unitDef in __result.ToArray()) {
                        int toAdd = getNumberToAdd(unitDef, requiredTags);
                        numberFetchingMs += watch.ElapsedMilliseconds;
                      RTS.l.Log($"Adding {unitDef.UnitDefID} to the list {toAdd} extra times.");
                        if (toAdd > 0) {
                            for (int i = 0; i < toAdd; i++) {
                                __result.Add(unitDef);
                            }
                        }
                        totalMs += watch.ElapsedMilliseconds;
                        watch.Restart();
                    }
                    watch.Stop();
                    RTS.l.Log($"New Length: {__result.Count} / Took {numberFetchingMs} ms / {totalMs} ms");
                } catch (Exception e) {
                    RTS.l.LogException(e);
                }
            }
        }
    }

}
