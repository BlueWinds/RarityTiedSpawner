using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BattleTech.Data;
using HarmonyLib;
using HBS.Collections;
using RarityTiedSpawner.feature;

namespace RarityTiedSpawner.patches;

[HarmonyPatch(typeof(TagSetQueryExtensions), "GetMatchingUnitDefs")]
public static class TagSetQueryExtensions_GetMatchingUnitDefs
{
    private static void Postfix(ref List<UnitDef_MDD> __result, TagSet requiredTags) {
        try
        {
            var watch = Stopwatch.StartNew();
            var tags = requiredTags.ToList().ConvertAll(t => t.ToLower()).ToArray();
            RTS.l.Log($"Old Length: {__result.Count}");
            foreach (UnitDef_MDD unitDef in __result.ToArray()) {
                int toAdd = RarityController.Instance.GetRarity(unitDef.UnitDefID, tags);
                RTS.l.Log($"Adding {unitDef.UnitDefID} to the list {toAdd} extra times.");
                if (toAdd > 0) {
                    for (int i = 0; i < toAdd; i++) {
                        __result.Add(unitDef);
                    }
                }
            }
            watch.Stop();
            RTS.l.Log($"New Length: {__result.Count} / Took {watch.ElapsedMilliseconds} ms");
        } catch (Exception e) {
            RTS.l.LogException(e);
        }
    }
}