using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace FacilityMeltdown.Lang
{
    internal static class LangParser
    {
        internal static Dictionary<string, string> languages { get; private set; }
        internal static Dictionary<string, object> loadedLanguage { get; private set; }

        internal static void Init()
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FacilityMeltdown.Lang.defs.json");
            using StreamReader reader = new StreamReader(stream);
            string result = reader.ReadToEnd();

            languages = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

            SetLanguage("en");
        }

        internal static void SetLanguage(string id)
        {
            MeltdownPlugin.logger.LogInfo($"Loading language: {languages[id]} ({id})");

            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"FacilityMeltdown.Lang.{id}.json");
            using StreamReader reader = new StreamReader(stream);
            string result = reader.ReadToEnd();
            loadedLanguage = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);


            MeltdownPlugin.logger.LogInfo($"Loaded {languages[id]}");
        }

        internal static string GetTranslation(string translation) { 

            if(loadedLanguage.TryGetValue(translation, out var result)) {
                return (string) result;
            }

            if(translation == "lang.missing") {
                // OHNO `lang.missing` is missing!
                MeltdownPlugin.logger.LogError("LANG.MISSING IS MISSING!!!!!  THIS IS BAD!! VERY BAD!!");
                return "lang.missing; <translation_id>";
            }

            return GetTranslation("lang.missing").Replace("<translation_id>", translation);
        }

        internal static string[] GetTranslationSet(string translation)
        {
            if (loadedLanguage.TryGetValue(translation, out var result)) {
                return (string[]) result;
            }

            return new string[] { GetTranslation("lang.missing").Replace("<translation_id>", translation) };
        }
    }
}
