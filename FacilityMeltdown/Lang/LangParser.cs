using FacilityMeltdown.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace FacilityMeltdown.Lang {
    internal static class LangParser {
        internal static Dictionary<string, string> languages { get; private set; }
        internal static Dictionary<string, object> loadedLanguage { get; private set; }
        internal static Dictionary<string, object> defaultLanguage { get; private set; }

        internal static void Init() {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FacilityMeltdown.Lang.defs.json");
            using StreamReader reader = new(stream);
            string result = reader.ReadToEnd();

            languages = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
        }

        internal static Dictionary<string, object> LoadLanguage(string id) {
            using Stream stream = File.Open(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lang", id + ".json"), FileMode.Open);
            using StreamReader reader = new StreamReader(stream);
            string result = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
        }

        internal static void SetLanguage(string id) {
            MeltdownPlugin.logger.LogInfo($"Loading language: {languages[id]} ({id})");

            loadedLanguage = LoadLanguage(id);


            MeltdownPlugin.logger.LogInfo($"Loaded {languages[id]}");
        }

        internal static string GetTranslation(string translation) {

            if(loadedLanguage.TryGetValue(translation, out var result)) {
                return (string)result;
            }

            if(defaultLanguage.TryGetValue(translation, out result)) {
                MeltdownPlugin.logger.LogError($"Falling back to english. for translation: {translation}");
                return (string)result;
            }

            if(translation == "lang.missing") {
                // OHNO `lang.missing` is missing!
                MeltdownPlugin.logger.LogError("LANG.MISSING IS MISSING!!!!!  THIS IS BAD!! VERY BAD!!");
                return "lang.missing; <translation_id>";
            }

            return GetTranslation("lang.missing").Replace("<translation_id>", translation);
        }

        internal static JArray GetTranslationSet(string translation) {

            if(loadedLanguage.TryGetValue(translation, out var result)) {
                MeltdownPlugin.logger.LogInfo(result.GetType());
                return result as JArray;
            }

            if(defaultLanguage.TryGetValue(translation, out result)) {
                MeltdownPlugin.logger.LogError($"Falling back to english. for translation (dialogue): {translation}");
                return result as JArray;
            }

            return new JArray { GetTranslation("lang.missing").Replace("<translation_id>", translation) };
        }
    }
}
