using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using static Obeliskial_Essentials.Essentials;
using Obeliskial_Essentials;
using System.IO;
using UnityEngine;
using System;
using static Barbarian.Traits;
using static Barbarian.DescriptionFunctions;
using BepInEx.Configuration;

namespace Barbarian
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.stiffmeds.obeliskialessentials")]
    [BepInDependency("com.stiffmeds.obeliskialcontent")]
    [BepInProcess("AcrossTheObelisk.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal int ModDate = int.Parse(DateTime.Today.ToString("yyyyMMdd"));
        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);
        internal static ManualLogSource Log;

        public static ConfigEntry<bool> EnableDebugging { get; set; }


        public static string characterName = "Gork";
        public static string heroName = characterName;

        public static string subclassName = "Barbarian"; // needs caps

        public static string subclassname = subclassName.ToLower();
        public static string debugBase = "Binbin - Testing " + characterName + " ";

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{PluginInfo.PLUGIN_GUID} {PluginInfo.PLUGIN_VERSION} has loaded!");

            EnableDebugging = Config.Bind(new ConfigDefinition(subclassName, "Enable Debugging"), true, new ConfigDescription("Enables debugging logs."));

            // register with Obeliskial Essentials
            RegisterMod(
                _name: PluginInfo.PLUGIN_NAME,
                _author: "binbin",
                _description: "Gork, the Barbarian.",
                _version: PluginInfo.PLUGIN_VERSION,
                _date: ModDate,
                _link: @"https://github.com/binbinmods/Bloodrager",
                _contentFolder: "Barbarian",
                _type: ["content", "hero", "trait"]
            );

            string text = ""; // $"{SpriteText("vitality")}  on this hero increases All Damage by 0.25 per stack\n";
            string cardId = "barbarianslasher";
            // CardDescriptionNew.AddTextToCardDescription(text, CardDescriptionNew.TextLocation.Beginning, cardId);
            text = $"{SpriteText("vitality")}  on this hero increases All Damage by 0.5 per stack\n";
            CardDescriptionNew.AddTextToCardDescription(text, CardDescriptionNew.TextLocation.Beginning, cardId + "rare");

            cardId = "barbariantrait1a";
            text = $"{SpriteText("bleed")}  cannot be removed, prevented, or restricted in any way\n";
            CardDescriptionNew.AddTextToCardDescription(text, CardDescriptionNew.TextLocation.Beginning, cardId);
            CardDescriptionNew.AddTextToCardDescription(text, CardDescriptionNew.TextLocation.Beginning, cardId + "a");
            CardDescriptionNew.AddTextToCardDescription(text, CardDescriptionNew.TextLocation.Beginning, cardId + "b");

            cardId = "barbariantrait3b";
            text = $"{SpriteText("bleed")} received +3\n";
            CardDescriptionNew.AddTextToCardDescription(text, CardDescriptionNew.TextLocation.Beginning, cardId);
            CardDescriptionNew.AddTextToCardDescription(text, CardDescriptionNew.TextLocation.Beginning, cardId + "a");
            CardDescriptionNew.AddTextToCardDescription(text, CardDescriptionNew.TextLocation.Beginning, cardId + "b");



            harmony.PatchAll();
        }

        internal static void LogDebug(string msg)
        {
            if (EnableDebugging.Value)
            {
                Log.LogDebug(debugBase + msg);
            }

        }
        internal static void LogInfo(string msg)
        {
            Log.LogInfo(debugBase + msg);
        }
        internal static void LogError(string msg)
        {
            Log.LogError(debugBase + msg);
        }
    }
}
