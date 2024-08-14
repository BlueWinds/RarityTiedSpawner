using BattleTech.Data;
using Harmony;
using HBS.Collections;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RarityTiedSpawner {
    public static class RarityModifications {
        private class TagCache {
            private HashSet<string> NonTagCachhe;
            private Dictionary<string, int> MoreCommonTags;
            private Dictionary<string, Regex> GenericTags;
            private Dictionary<string, int> NumberStrings;
            private Dictionary<string, int> NegativeTags;
            private Dictionary<string, int> PositiveTags;
            private Dictionary<string, List<Tag_MDD>> MechTagCache;

            private static Regex EndNumberPattern = new Regex(@"-?\d+$", RegexOptions.Compiled);
            private static Regex NegativePattern = new Regex($"^{RTS.settings.excludeTag}_.+{RTS.settings.dynamicTag}_-?\\d+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            private static Regex PositiveTagPattern = new Regex($"^.+_{RTS.settings.dynamicTag}_-?\\d+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            private static Regex TagPattern = new Regex($"^{RTS.settings.dynamicTag}_-?\\d+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            private static Regex DynamicTagPattern = new Regex($"{RTS.settings.dynamicTag}_-?\\d+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            public long timeUsed = 0;

            private static TagCache _instance;
            public static TagCache Instance {
                get {
                    if (_instance == null) {
                        _instance = new TagCache();
                    }
                    return _instance;
                }
            }

            private TagCache() {
                NonTagCachhe = new HashSet<string>();
                MoreCommonTags = RTS.settings.moreCommonTags;
                GenericTags = new Dictionary<string, Regex>();
                NumberStrings = new Dictionary<string, int> {
                    { "0", 0 },
                    { "1", 1 },
                    { "2", 2 },
                    { "3", 3 },
                    { "4", 4 },
                    { "5", 5 },
                    { "6", 6 },
                    { "7", 7 },
                    { "8", 8 },
                    { "9", 9 },
                    { "10", 10 },
                    { "11", 11 },
                    { "12", 12 },
                    { "13", 13 },
                    { "14", 14 },
                    { "15", 15 },
                    { "16", 16 },
                    { "17", 17 },
                    { "18", 18 },
                    { "19", 19 },
                    { "20", 20 },
                };
                PositiveTags = new Dictionary<string,int>();
                NegativeTags = new Dictionary<string, int>();
                MechTagCache = new Dictionary<string, List<Tag_MDD>>();
            }

            public int GetNumberToAdd(UnitDef_MDD unitDef, TagSet requiredTags, TagSet excludedTags) {
                int toAdd = 0;

                if (!MechTagCache.ContainsKey(unitDef.UnitDefID)) {
                    MechTagCache.Add(unitDef.UnitDefID, unitDef.TagSetEntry.Tags);
                }
                foreach (Tag_MDD tag in MechTagCache[unitDef.UnitDefID]) {
                    if (MoreCommonTags.ContainsKey(tag.Name)) {
                        toAdd += MoreCommonTags[tag.Name];
                        continue;
                    }
                    if (TagPattern.IsMatch(tag.Name)) {
                        var numString = EndNumberPattern.Match(tag.Name).Value;
                        var num = 0;
                        if (NumberStrings.ContainsKey(numString)) {
                            num = NumberStrings[numString];
                        } else {
                            num = int.Parse(numString);
                            NumberStrings.Add(numString, num);
                        }
                        MoreCommonTags.Add(tag.Name, num);
                        toAdd += MoreCommonTags[tag.Name];
                        continue;
                    }
                    if (!DynamicTagPattern.IsMatch(tag.Name)) {
                        MoreCommonTags.Add(tag.Name, 0);
                        continue;
                    }
                    var negativeAdd = 0;
                    foreach (var negativeTag in excludedTags) {
                        if (!NegativePattern.IsMatch(tag.Name)) {
                            break;
                        }
                        if (!GenericTags.ContainsKey(negativeTag)) {
                            GenericTags.Add(negativeTag, new Regex(Regex.Escape(negativeTag), RegexOptions.Compiled | RegexOptions.IgnoreCase));
                        }
                        if (!GenericTags[negativeTag].IsMatch(tag.Name)) {
                            continue;
                        }
                        if (NegativeTags.ContainsKey(tag.Name)) {
                            negativeAdd = NegativeTags[tag.Name];
                            break;
                        } else {
                            var numString = EndNumberPattern.Match(tag.Name).Value;
                            var num = 0;
                            if (NumberStrings.ContainsKey(numString)) {
                                num = NumberStrings[numString];
                            } else {
                                num = int.Parse(numString);
                            }
                            NegativeTags.Add(tag.Name, num);
                            negativeAdd = NegativeTags[tag.Name];
                            break;
                        }
                    }
                    if (negativeAdd > 0) {
                        toAdd += negativeAdd;
                        continue;
                    }
                    var positiveAdd = 0;
                    foreach (var positiveTag in requiredTags) {
                        if (!PositiveTagPattern.IsMatch(tag.Name)) {
                            break;
                        }
                        if (!GenericTags.ContainsKey(positiveTag)) {
                            GenericTags.Add(positiveTag, new Regex(Regex.Escape(positiveTag), RegexOptions.Compiled | RegexOptions.IgnoreCase));
                        }
                        if (!GenericTags[positiveTag].IsMatch(tag.Name)) {
                            continue;
                        }
                        if (PositiveTags.ContainsKey(tag.Name)) {
                            positiveAdd = PositiveTags[tag.Name];
                            break;
                        } else {
                            var numString = EndNumberPattern.Match(tag.Name).Value;
                            var num = 0;
                            if (NumberStrings.ContainsKey(numString)) {
                                num = NumberStrings[numString];
                            } else {
                                num = int.Parse(numString);
                            }
                            PositiveTags.Add(tag.Name, num);
                            positiveAdd = PositiveTags[tag.Name];
                            break;
                        }
                    }
                    if (positiveAdd > 0) {
                        toAdd += positiveAdd;
                        continue;
                    }
                }
                return toAdd;
            }
        }

        [HarmonyPatch(typeof(TagSetQueryExtensions), "GetMatchingUnitDefs")]
        public static class TagSetQueryExtensions_GetMatchingUnitDefs {
            private static void Postfix(ref List<UnitDef_MDD> __result, TagSet requiredTags, TagSet excludedTags, TagSet companyTags) {
                try {
                    RTS.modLog.Info?.Write($"Old Length: {__result.Count}");
                    foreach (UnitDef_MDD unitDef in __result.ToArray()) {
                        int toAdd = TagCache.Instance.GetNumberToAdd(unitDef, requiredTags, excludedTags);
                        if (toAdd > 0) {
                            RTS.modLog.Info?.Write($"Possible unit: {unitDef.UnitDefID}. Adding {toAdd} to list.");
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
    
}
