using System.Collections.Generic;

namespace RarityTiedSpawner;

public class RarityCacheItem
{
    public int BaseRarity;
    public Dictionary<string, int> RarityModifiers = new Dictionary<string, int>();
}