using Harmony;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using HBS.Logging;

namespace RarityTiedSpawner {
    public class RTS {
        internal static ILog l;
        internal static string modDir;
        internal static Settings settings;

        public static void Init(string modDirectory, string settingsJSON) {
            modDir = modDirectory;
            l = Logger.GetLogger("RarityTiedSpawner");

            try {
                using (StreamReader reader = new StreamReader($"{modDir}/settings.json")) {
                    string jdata = reader.ReadToEnd();
                    settings = JsonConvert.DeserializeObject<Settings>(jdata);
                }
                l.Log($"Loaded settings from {modDir}/settings.json. Version {typeof(Settings).Assembly.GetName().Version}");
            } catch (Exception e) {
                l.LogException(e);
                settings = new Settings();
            }

            var harmony = HarmonyInstance.Create("blue.winds.RarityTiedSpawner");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
