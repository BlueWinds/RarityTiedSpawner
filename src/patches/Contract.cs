using BattleTech;
using HarmonyLib;
using RarityTiedSpawner.feature;

namespace RarityTiedSpawner.patches;

[HarmonyPatch(typeof(Contract), "CompleteContract")]
public static class Contract_CompleteContract
{
    static void Postfix(Contract __instance)
    {
        // probably not needed but can't hurt in case a mod 
        // selects and spawns a unit mid-combat. 
        // Normally this doesn't happen as even for units spawned halfway through a mission
        // are selected ahead of time, but better safe than sorry
        RarityController.Instance.ClearUnitMddCache();
    }
}