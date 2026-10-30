using BattleTech.UI;
using HarmonyLib;
using RarityTiedSpawner.feature;

namespace RarityTiedSpawner.patches;

[HarmonyPatch(typeof(Briefing), "SetStartButtonActive")]
public static class Briefing_SetStartButtonActive
{
    static void Postfix(Briefing __instance, bool isActive)
    {
        if (!isActive) return;
        // units have been selected by this point, clear the mdd cache to free memory for combat
        RarityController.Instance.ClearUnitMddCache();
    }
}