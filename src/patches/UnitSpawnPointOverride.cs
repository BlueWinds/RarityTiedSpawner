using System;
using System.Collections.Generic;
using BattleTech.Data;
using BattleTech.Framework;
using HarmonyLib;
using HBS.Collections;

namespace RarityTiedSpawner;

[HarmonyPatch(typeof(UnitSpawnPointOverride), "SelectTaggedUnitDef")]
public static class UnitSpawnPointOverride_SelectTaggedUnitDef {
    private static void Prefix(ref bool __runOriginal, UnitSpawnPointOverride __instance, MetadataDatabase mdd, TagSet unitTagSet, 
        TagSet unitExcludedTagSet, string lanceName, string lanceDefId, int unitIndex,
        DateTime? currentDate, TagSet companyTags, ref UnitDef_MDD __result) {
        
        if (!__runOriginal)
        {
            return;
        }
        
        __runOriginal = false;
        
        UnitDef_MDD unitDefMdd = null;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        List<UnitDef_MDD> unitDefMddList = mdd.GetMatchingUnitDefs(unitTagSet, unitExcludedTagSet, true, currentDate, companyTags);
        watch.Stop();
        RTS.l.Log($"MDD matching took {watch.ElapsedMilliseconds} ms");
        int num = 0;
        if (unitDefMddList.Count == 0)
        {
            string message = string.Format("LanceDefID: [{0}]\r\nUnit spawner {1}[{2}]\r\n could not find a unit to match these tags: [{3}] and these excluded tags [{4}]. Picking a random unit marked with unit_fallback tag (currently just a cicada)!", lanceDefId, lanceName, unitIndex,unitTagSet, unitExcludedTagSet);
            UnitSpawnPointOverride.logger.LogError(message);
            unitDefMddList = UnitSpawnPointOverride.GetFallbackUnitList(mdd);
        }
        if (unitDefMddList.Count > 0)
        {
            watch = System.Diagnostics.Stopwatch.StartNew();
            unitDefMddList.Shuffle();
            unitDefMdd = UnitSpawnPointOverride.SelectUnitDefFromList(unitDefMddList);
            watch.Stop();
            RTS.l.Log($"Unit selection/shuffle took {watch.ElapsedMilliseconds} ms");
            if (unitDefMdd != null)
            {
                if (num > 1)
                {
                    string message = string.Format("Unit spawner {0}.[{1}] had to adjust difficulty by +/- [{2}]", lanceName, unitIndex, num);
                    UnitSpawnPointOverride.logger.LogWarning(message);
                }
                string message1 = string.Format("Unit spawner {0}.[{1}] selected this unit: [{2}] match these tags: [{3}] and these excluded tags [{4}]", lanceName, unitIndex, unitDefMdd.UnitDefID, unitTagSet, unitExcludedTagSet);
                UnitSpawnPointOverride.logger.Log(message1);
            }
        }
        if (unitDefMdd == null)
        {
            string message = "Something is really wrong. No units were found. You probably need to rebuild your manifest and/or re collect Lances and Units for the MDD.";
            UnitSpawnPointOverride.logger.LogError( message);
        }
        __result = unitDefMdd;
    }
}