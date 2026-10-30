using System.Linq;
using BattleTech;
using BattleTech.Save;
using HarmonyLib;
using RarityTiedSpawner.feature;

namespace RarityTiedSpawner.patches;

[HarmonyPatch(typeof(SimGameState), "Rehydrate", typeof(GameInstanceSave))]
class SimGameState_Rehydrate
{
    public static void Postfix(SimGameState __instance, GameInstanceSave gameInstanceSave)
    {
        RarityController.Instance.BuildRarityCache(__instance.DataManager);
    }
}

[HarmonyPatch(typeof(SimGameState), "_OnAttachUXComplete")]
class SimGameState_OnAttachUXComplete
{
    public static void Postfix(SimGameState __instance)
    {
        RarityController.Instance.BuildRarityCache(__instance.DataManager);
    }
}