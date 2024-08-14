using System.Collections.Generic;

namespace RarityTiedSpawner {
    public class Settings {
        public bool debug = false;
        public bool trace = false;
        public string excludeTag = "";
        public string dynamicTag = "";
        public Dictionary<string, int> moreCommonTags = new Dictionary<string, int>();
    }
}
