using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BattleTech.Data;
using HBS.Collections;
using HBS.Logging;
using Newtonsoft.Json;

namespace RarityTiedSpawner.feature;

public class RarityController
{

    private static Regex REQUIRED_TAG = new Regex("^RTS_(?<requiredTag>.*?)_unit_rarity_(?<number>-?\\d+)$", RegexOptions.Compiled);
    
    private static RarityController _instance;

    private RarityCache rarityCache = new RarityCache();
    private List<UnitMddCacheEntry> unitMddCache = new List<UnitMddCacheEntry>();
    private ILog logger;

    public static RarityController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new RarityController();
                _instance.Initialize();
            }
            return _instance;
        }
    }

    private void Initialize()
    {
        logger = RTS.l;
    }

    public void ClearUnitMddCache()
    {
        logger.Log("Clearing unit mdd cache");
        unitMddCache.Clear();
    }

    public List<UnitDef_MDD> GetMatchingUnitDefs(MetadataDatabase mdd, TagSet unitTagSet,
        TagSet unitExcludedTagSet,
        DateTime? currentDate, TagSet companyTags)
    {
        // for some lance slots, the MDDB call can be very expensive (2 seconds or more)
        // cache the results so that if another lance slot in the same contract uses the same tag sets
        // then we don't have to query the MDDB again
        var cached = GetCachedUnitDefs(unitTagSet, unitExcludedTagSet);
        if (cached != null)
        {
            return cached;
        }
        
        var requiredTags = GetCanonicalTagSet(unitTagSet);
        var excludedTags = GetCanonicalTagSet(unitExcludedTagSet);
        logger.Log($"Cache miss for {requiredTags} - {excludedTags}");
        List<UnitDef_MDD> unitDefMddList = mdd.GetMatchingUnitDefs(unitTagSet, unitExcludedTagSet, true, currentDate, companyTags);
        
        unitMddCache.Add(
            new UnitMddCacheEntry()
            {
                requiredTags = requiredTags,
                excludedTags = excludedTags,
                unitDefs = unitDefMddList
            });
        return unitDefMddList;
    }

    private List<UnitDef_MDD> GetCachedUnitDefs(TagSet unitTagSet, TagSet unitExcludedTagSet)
    {
        var unitTagSetString = GetCanonicalTagSet(unitTagSet);
        var unitExcludedTagSetString = GetCanonicalTagSet(unitExcludedTagSet);
        foreach (var unitMddCacheEntry in unitMddCache)
        {
            if (unitMddCacheEntry.requiredTags.Equals(unitTagSetString) &&
                unitMddCacheEntry.excludedTags.Equals(unitExcludedTagSetString))
            {
                return unitMddCacheEntry.unitDefs;
            }
        }

        return null;
    }

    private string GetCanonicalTagSet(TagSet tagSet)
    {
        // represent tag sets as a sorted lower case string this creates a single possible
        // representation that can be quickly checked for equality against other tag sets for cache lookups
        var tags = tagSet.ToList().ConvertAll(t => t.ToLower());
        tags.Sort();
        return string.Join(",", tags);
    }

    public void BuildRarityCache(DataManager dataManager)
    {
        // by pre-computing this we don't have to pay this calculation against a UnitDef MDD's tag set, which
        // is expensive to fetch because it makes multiple MDDB calls to fetch.
        if (rarityCache.Cache.Count > 0) return;
        var units = dataManager.MechDefs.Select(pair => pair.Value);
        logger.Log($"Building rarity cache for {units.Count()} units");
        var watch = System.Diagnostics.Stopwatch.StartNew();
        foreach (var mech in units)
        {
            try
            {
                RarityCacheItem item = new RarityCacheItem();
                foreach (var tag in mech.MechTags)
                {
                    if (RTS.settings.moreCommonTags.ContainsKey(tag))
                    {
                        item.BaseRarity += RTS.settings.moreCommonTags[tag];
                        continue;
                    }
                    MatchCollection matches = REQUIRED_TAG.Matches(tag);
                    if (matches.Count > 0)
                    {
                        item.RarityModifiers[matches[0].Groups["requiredTag"].Value.ToLower()] = int.Parse(matches[0].Groups["number"].Value);
                    }
                }

                rarityCache.Cache[mech.Description.Id] = item;
            }
            catch (Exception e)
            {
                logger.LogException(e);
            }
            
        }
        watch.Stop();
        logger.Log($"Rarity cache built in {watch.ElapsedMilliseconds} ms");
    }

    public int GetRarity(string unitId, IEnumerable<string> tags)
    {
        if (!rarityCache.Cache.ContainsKey(unitId)) return 0;

        var cacheRow = rarityCache.Cache[unitId];

        int modifier = 0;
        foreach (var tag in tags)
        {
            if (cacheRow.RarityModifiers.ContainsKey(tag))
            {
                modifier += cacheRow.RarityModifiers[tag];
            }
        }
        return cacheRow.BaseRarity + modifier;

    }



}