using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RarityTiedSpawner {
    public class Settings {
        public bool debug = false;
        public bool trace = false;
        public Dictionary<string, int> moreCommonTags = new Dictionary<string, int>();
    }
}
