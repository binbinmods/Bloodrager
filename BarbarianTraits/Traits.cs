using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using UnityEngine;
using static Barbarian.CustomFunctions;
using static Barbarian.Plugin;
using static Barbarian.DescriptionFunctions;
using System.Text;
using System.Text.RegularExpressions;

namespace Barbarian
{
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs

        public static string[] simpleTraitList = ["trait0", "trait1a", "trait1b", "trait2a", "trait2b", "trait3a", "trait3b", "trait4a", "trait4b"];

        public static string[] myTraitList = simpleTraitList.Select(trait => subclassname + trait).ToArray(); // Needs testing

        static string trait0 = myTraitList[0];
        // static string trait1b = myTraitList[1];
        static string trait2a = myTraitList[3];
        static string trait2b = myTraitList[4];
        static string trait4a = myTraitList[7];
        static string trait4b = myTraitList[8];

        public static int infiniteProctection = 0;
        public static int bleedInfiniteProtection = 0;

        public static string debugBase = "Binbin - Testing " + heroName + " ";

        public static void DoCustomTrait(string _trait, ref Trait __instance)
        {
            // get info you may need
            Enums.EventActivation _theEvent = Traverse.Create(__instance).Field("theEvent").GetValue<Enums.EventActivation>();
            Character _character = Traverse.Create(__instance).Field("character").GetValue<Character>();
            Character _target = Traverse.Create(__instance).Field("target").GetValue<Character>();
            int _auxInt = Traverse.Create(__instance).Field("auxInt").GetValue<int>();
            string _auxString = Traverse.Create(__instance).Field("auxString").GetValue<string>();
            CardData _castedCard = Traverse.Create(__instance).Field("castedCard").GetValue<CardData>();
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            TraitData traitData = Globals.Instance.GetTraitData(_trait);
            List<CardData> cardDataList = [];
            List<string> heroHand = MatchManager.Instance.GetHeroHand(_character.HeroIndex);
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();

            if (_trait == trait0)
            { 
                string traitName = traitData.TraitName;
                string traitId = _trait;
                LogDebug($"Handling Trait {traitId}: {traitName}");
                // trait0:
                // Fury and Sharp are half as effective at increasing your damage. 
                // Bleed on you increases damage by 3%/charge 
                // Done in GACM
            }


            else if (_trait == trait2a)
            {
                // Speed -2. All resistances -40%.  -- Done in JSON
                // Double your current Max HP and your HP gained by Vitality. -- Done in AssignTraitPostfix
                // At the start of combat, gain 2 vitality 

                // Handled in GACM/GetTraitAuraCurseModifiers
                string traitName = traitData.TraitName;
                string traitId = _trait;
                LogDebug($"Handling Trait {traitId}: {traitName}");

                if (IsLivingHero(_character))
                {
                    _character.SetAuraTrait(_character, "vitality", 2);
                }

            }



            else if (_trait == trait2b)
            { // trait 2b:  At the start of your turn, reduce the cost of your highest cost card by 4.
                string traitName = traitData.TraitName;
                string traitId = _trait;
                LogDebug($"Handling Trait {traitId}: {traitName}");
                if(IsLivingHero(_character))
                {
                    CardData highestCostCard = GetRandomHighestCostCard(Enums.CardType.None, heroHand);
                    ReduceCardCost(ref highestCostCard, _character, 4, isPermanent: false);
                }                
            }

            else if (_trait == trait4a)
            {
                // When you apply Vitality to another character, gain that much Vitality (unaffected by modifiers). 
                // +100 Max Vitality charges for you.

                string traitName = traitData.TraitName;
                string traitId = _trait;
                LogDebug($"Handling Trait {traitId}: {traitName}");
                if (IsLivingHero(_character) && _target.Alive && _target != null && _auxInt != 0 && _auxString == "vitality" && _target.SourceName != _character.SourceName)
                {
                    _character.SetAura(_character, GetAuraCurseData("vitality"), _auxInt, fromTrait:true, useCharacterMods: false);
                }

            }

            else if (_trait == trait4b)
            {
                string traitName = traitData.TraitName;
                string traitId = _trait;
                LogDebug($"Handling Trait {traitId}: {traitName}");
                // Bleed +1 for every 2 Sharp on you. -- Done in GetTraitAuraCurseModifiersPostfix
                // +200 Max Bleed charges for everyone. -- Done in GACM
                
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), "DoTrait")]
        public static bool DoTrait(Enums.EventActivation _theEvent, string _trait, Character _character, Character _target, int _auxInt, string _auxString, CardData _castedCard, ref Trait __instance)
        {
            if ((UnityEngine.Object)MatchManager.Instance == (UnityEngine.Object)null)
                return false;
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            if (Content.medsCustomTraitsSource.Contains(_trait) && myTraitList.Contains(_trait))
            {
                DoCustomTrait(_trait, ref __instance);
                return false;
            }
            return true;
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GlobalAuraCurseModificationByTraitsAndItems")]
        [HarmonyPriority(Priority.Last)]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {
            LogInfo($"GACM {subclassName}");

            Character characterOfInterest = _type == "set" ? _characterTarget : _characterCaster;
            string traitOfInterest;
            string enchantmentOfInterest;
            switch (_acId)
            {
                // trait0:
                // Fury and Sharp are half as effective at increasing your damage. 
                // Bleed on you increases damage by 2%/charge 

                // enchantment1a: Bleed cannot be removed, prevented, or restricted

                // 4a: +100 max vit on this hero
                // 4b: +200 max bleed globally
                case "bleed":
                    enchantmentOfInterest = "barbariantrait1a";
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Enchantment, enchantmentOfInterest, AppliesTo.Global))
                    {
                        __result.Preventable = false;
                        __result.Removable = false;
                        __result.MaxCharges = -1;
                        __result.MaxMadnessCharges = -1;
                    }
                    traitOfInterest = trait4b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Global))
                    {
                        __result.MaxMadnessCharges += 200;
                    }

                    break;
                case "vitality":
                    traitOfInterest = trait4b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.MaxMadnessCharges += 100;
                    }
                    break;

                case "fury":
                    traitOfInterest = trait0;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.AuraDamageIncreasedPercentPerStack *= 0.5f;
                    }                    
                    break;
                case "sharp":
                    traitOfInterest = trait0;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.AuraDamageIncreasedPerStack *= 0.5f;
                        __result.AuraDamageIncreasedPerStack2 *= 0.5f;
                        __result.AuraDamageIncreasedPerStack3 *= 0.5f;
                        __result.AuraDamageIncreasedPerStack4 *= 0.5f;                        
                    }
                    break;
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Hero), nameof(Hero.AssignTrait))]
        public static void AssignTraitPostfix(
            ref Hero __instance,
            bool __result,
            string traitName)
        {
            LogDebug("AssignTraitPostfix");
            string traitOfInterest = trait2a;
            if(__result && traitName == traitOfInterest)
            {
                LogDebug("AssignTraitPostfix - Doubling Max HP");
                int toAdd = __instance.GetMaxHP();
                __instance.ModifyMaxHP(toAdd);
            }
           
        }



        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), nameof(Character.BeginTurn))]
        public static void BeginTurnPrefix(ref Character __instance)
        {

            infiniteProctection = 0;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.GetTraitAuraCurseModifiers))]
        public static void GetTraitAuraCurseModifiersPostfix(ref Character __instance, ref Dictionary<string, int> __result)
        {
            LogDebug("GetTraitAuraCurseModifiersPostfix");
            string traitOfInterest = trait4b;
            if (!IsLivingHero(__instance) || !__instance.HaveTrait(traitOfInterest))
            {
                return;
            }

            // int nInsane = __instance.GetAuraCharges("insane");
            int nToIncrease = Mathf.RoundToInt(__instance.GetAuraCharges("sharp") * 0.5f);
            // int nToIncrease = Mathf.FloorToInt(nInsane * 0.1f);
            if (nToIncrease <= 0)
            {
                return;
            }
            if (!__result.ContainsKey("bleed"))
            {
                __result["bleed"] = 0;
            }
            __result["bleed"] += nToIncrease;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CardData), nameof(CardData.SetDescriptionNew))]

        public static void SetDescriptionNewPostfix(ref CardData __instance, bool forceDescription = false, Character character = null, bool includeInSearch = true)
        {
            // LogInfo("executing SetDescriptionNewPostfix");
            if (__instance == null)
            {
                LogDebug("Null Card");
                return;
            }

            StringBuilder stringBuilder1 = new StringBuilder();
            string currentDescription = Globals.Instance.CardsDescriptionNormalized[__instance.Id];
            stringBuilder1.Append(currentDescription);
            string enchantId = "barbariantrait1a";
            if (__instance.Id == enchantId || __instance.Id == enchantId+"a" || __instance.Id == enchantId+"b")
            {
                string textToAdd = $"{SpriteText("bleed")}  cannot be removed, prevented, or restricted in any way\n";
                stringBuilder1.Insert(0, textToAdd);
            }
            enchantId = "barbariantrait3b";
            if (__instance.Id == enchantId || __instance.Id == enchantId+"a" || __instance.Id == enchantId+"b")
            {
                string textToAdd = $"{SpriteText("bleed")}  received +3\n";
                stringBuilder1.Insert(0, textToAdd);
            }

            BinbinNormalizeDescription(ref __instance, stringBuilder1);
        }   


        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), nameof(Character.SetEvent))]
        public static void SetEventPrefix(ref Character __instance,
                                            Enums.EventActivation theEvent,
                                            ref int auxInt,
                                            Character target = null,
                                            string auxString = "")
        {

            if (theEvent == Enums.EventActivation.CharacterAssign || theEvent == Enums.EventActivation.DestroyItem || __instance == null || theEvent == Enums.EventActivation.None)
            {
                return;
            }

            // Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            // NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();
            // string eventString = Enum.GetName(typeof(Enums.EventActivation), theEvent);
            LogDebug("SetEventPrefix");
            string enchantId = "barbariantrait3b";
            if (theEvent == Enums.EventActivation.AuraCurseSet && IsLivingHero(target) && CharacterHaveEnchantment(target,enchantId) && auxString == "bleed")
            {
                // trait3a: Bleed Received +3";
                int n = 3;

                // needs to run every other time this is called
                bleedInfiniteProtection++;
                if (bleedInfiniteProtection % 2 == 1 && bleedInfiniteProtection < 100)
                {
                    LogDebug("Handling ");
                    target.SetAura(__instance, GetAuraCurseData("bleed"), n, useCharacterMods: false);
                }

            }

        }             

    }
}

