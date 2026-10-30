using System.Collections.Generic;
using BattleTech.Data;

namespace RarityTiedSpawner;

public class UnitMddCacheEntry
{
    public string requiredTags;
    public string excludedTags;
    public List<UnitDef_MDD> unitDefs;
}