using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using UnityEngine;
using static Barbarian.CustomFunctions;
using static Barbarian.Plugin;
using System.Text;
using System.Text.RegularExpressions;

namespace Barbarian
{
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs
        public static string heroName = "Gork";

        public static string subclassname = "barbarian";

        public static string[] simpleTraitList = ["trait0", "trait1a", "trait1b", "trait2a", "trait2b", "trait3a", "trait3b", "trait4a", "trait4b"];

        public static string[] myTraitList = simpleTraitList.Select(trait => subclassname + trait).ToArray(); // Needs testing

        static string trait0 = myTraitList[0];
        // static string trait1b = myTraitList[1];
        static string trait2a = myTraitList[3];
        static string trait2b = myTraitList[4];
        static string trait4a = myTraitList[7];
        static string trait4b = myTraitList[8];

        public static bool isDamagePreviewActive = false;

        public static bool isCalculateDamageActive = false;
        public static int infiniteProctection = 0;
        public static int infiniteProctectionPowerful = 0;

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
            { // TODO trait 0
                string traitName = traitData.TraitName;
                string traitId = _trait;
                LogDebug($"Handling Trait {traitId}: {traitName}");
                // Insane on you does not reduce damage, increases healing by 1% per charge, and stacks to 200.
                // When you would overheal a character, apply Shield equal to 50% of the amount overhealed (Benefits 50% from Shield bonuses).
                // Done in HealReceivedFinalPrefix and GACM
            }


            else if (_trait == trait2a)
            {
                // Scourge on you increases Damage and Healing by 10% per charge and 
                // increases Block and Shield charges applied by 1 per charge.

                // Handled in GACM/GetTraitAuraCurseModifiers
                string traitName = traitData.TraitName;
                string traitId = _trait;
                LogDebug($"Handling Trait {traitId}: {traitName}");

                // if (IsLivingHero(_character) && _target != null && _target.Alive)
                // {
                //     _character.SetAuraTrait(_character, "scourge", 2);
                //     _target.SetAuraTrait(_character, "scourge", 2);
                //     DisplayTraitScroll(ref _character, traitData);
                // }

            }



            else if (_trait == trait2b)
            { // TODO trait 2b
                string traitName = traitData.TraitName;
                string traitId = _trait;
                LogDebug($"Handling Trait {traitId}: {traitName}");
                // At the start of your turn, reduce your highest cost card by 1 until discarded. Repeat for every 20 Insane on you.
                if (!IsLivingHero(_character))// || _character.GetAuraCharges("insane") < 20)
                {
                    return;
                }

                int nInsane = _character.GetAuraCharges("insane");
                int iterations = Mathf.FloorToInt(nInsane * 0.05f) + 1;
                for (int i = 0; i < iterations; i++)
                {
                    CardData highestCostCard = GetRandomHighestCostCard(Enums.CardType.Spell, heroHand);
                    ReduceCardCost(ref highestCostCard, _character, 1, isPermanent: false);
                }

                // Debating having it half your insane charges first.
                _character.HealAuraCurse(GetAuraCurseData("insane"));
                _character.SetAura(_character, GetAuraCurseData("insane"), Mathf.RoundToInt(nInsane * 0.5f), useCharacterMods: false, canBePreventable: false);

            }

            else if (_trait == trait4a)
            {
                // Scourge +2. Scourge on all characters can stack and only loses 50% of its charges when consumed.
                // Done in GACM

                string traitName = traitData.TraitName;
                string traitId = _trait;
                LogDebug($"Handling Trait {traitId}: {traitName}");

            }

            else if (_trait == trait4b)
            {
                string traitName = traitData.TraitName;
                string traitId = _trait;
                LogDebug($"Handling Trait {traitId}: {traitName}");
                // Insane on you increases mind damage by 3% per charge. When you play a card, suffer 3 Insane, this does not benefit from modifiers. (5x/turn).
                if (CanIncrementTraitActivations(traitId))
                {
                    _character.SetAura(_character, GetAuraCurseData("insane"), 3, useCharacterMods: false);
                    IncrementTraitActivations(traitId);
                }
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
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {
            LogInfo($"GACM {subclassName}");

            Character characterOfInterest = _type == "set" ? _characterTarget : _characterCaster;
            string traitOfInterest;

            switch (_acId)
            {
                // 0: Insane on you does not reduce damage, increases healing by 1% per charge, and stacks to 200
                // item 1a: Scourge on this hero can stack and is lost at the end of turn.
                // 2a: Scourge on this hero increases Damage and Healing by 10% per charge.
                // item 3b: Crack reduces mind resistance by 1%/charge
                // 4a: Scourge on all characters can stack. 
                // 4a: Scourge loses half charges when consumed.
                // 4b: Insane on this hero increases mind damage by 3% per charge.                
                case "scourge":
                    traitOfInterest = trait2a;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.AuraDamageType2 = Enums.DamageType.All;
                        __result.AuraDamageIncreasedPercentPerStack2 = 10.0f;
                        __result.HealDonePercentPerStack = 10;

                    }

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Item, "moontouchedtrait1b", AppliesTo.ThisHero) ||
                        IfCharacterHas(characterOfInterest, CharacterHas.Item, "moontouchedtrait1ba", AppliesTo.ThisHero) ||
                        IfCharacterHas(characterOfInterest, CharacterHas.Item, "moontouchedtrait1bb", AppliesTo.ThisHero))
                    {
                        __result.GainCharges = true;
                        __result.ConsumedAtTurn = true;
                        __result.ConsumedAtTurnBegin = false;
                    }

                    traitOfInterest = trait4a;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Global))
                    {
                        __result.GainCharges = true;
                        __result.AuraConsumed = 0;
                        __result.ConsumeAll = false;
                    }
                    break;

                case "crack":
                    // traitOfInterest = trait4b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Item, "moontouchedtrait3b", AppliesTo.Monsters) ||
                        IfCharacterHas(characterOfInterest, CharacterHas.Item, "moontouchedtrait3ba", AppliesTo.Monsters) ||
                        IfCharacterHas(characterOfInterest, CharacterHas.Item, "moontouchedtrait3bb", AppliesTo.Monsters))
                    {
                        __result = __instance.GlobalAuraCurseModifyResist(__result, Enums.DamageType.Mind, 0, -1.0f);
                    }
                    break;

                case "insane":
                    traitOfInterest = trait0;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        // __result.AuraDamageType3 = Enums.DamageType.Mind;
                        // __result.AuraDamageIncreasedPercentPerStack3 = 2.0f;
                        __result.AuraDamageType = Enums.DamageType.None;
                        __result.AuraDamageIncreasedPercentPerStack = 0;
                        __result.HealDonePercentPerStack = 1;
                        __result.MaxCharges = 200;
                        __result.MaxMadnessCharges = 200;
                    }

                    traitOfInterest = trait4b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.AuraDamageType3 = Enums.DamageType.Mind;
                        __result.AuraDamageIncreasedPercentPerStack3 = 3.0f;
                    }
                    break;
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.HealReceivedFinal))]
        public static void HealReceivedFinalPostfix(Character __instance, int __result, int heal, bool isIndirect = false)
        {
            LogDebug("HealReceivedFinalPostfix");
            if (infiniteProctection > 100)
                return;
            if (isDamagePreviewActive || isCalculateDamageActive)
                return;
            if (MatchManager.Instance == null)
                return;
            if (!IsLivingHero(__instance) || MatchManager.Instance.GetHeroHeroActive() == null)
                return;

            infiniteProctection++;

            // MatchManager.Instance.cast
            Hero activeHero = MatchManager.Instance.GetHeroHeroActive();
            LogDebug("Inf " + infiniteProctection);
            LogDebug("Active Hero: " + activeHero.SubclassName);
            LogDebug("Targeted/Instanced Hero: " + __instance.SubclassName);
            if (__result >= 0 && activeHero.HaveTrait(trait0) && IsLivingHero(__instance) && IsLivingHero(activeHero))
            {
                int amountOverhealed = __result - __instance.GetHpLeftForMax();
                if (amountOverhealed <= 0) { return; }
                int amountToShield = Mathf.RoundToInt((amountOverhealed + activeHero.AuraCurseModification["shield"]) * 0.5f);
                __instance.SetAura(activeHero, GetAuraCurseData("shield"), amountToShield, useCharacterMods: false);
            }
        }

       



        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "EndTurn")]
        public static void EndTurnPostfix(
            ref Character __instance,
            int __state)
        {
            
           
        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "BeginTurn")]
        public static void BeginTurnPostFix2(
            ref Character __instance,
            int __state)
        {
            // string whenToConsume = "EndTurn";
            // Character character = __instance;
            // LogDebug($"BeginTurnPostfix - Applying {__state} Scourge");
            // if (__state > 0 && character != null && character.Alive)
            // {            
            //     // Globals.Instance.WaitForSeconds(1.5f);
            //     character.SetAura(character, GetAuraCurseData("scourge"), __state, useCharacterMods: false, canBePreventable: false);
            //     LogDebug($"BeginTurnPostfix - Applied {__state} Scourge");
            // }
            AuraCurseData scourge = GetAuraCurseData("scourge");
            bool hasWaning = IsLivingHero(__instance) && (AtOManager.Instance.CharacterHaveItem(__instance.SubclassName,"moontouchedtrait1b") ||AtOManager.Instance.CharacterHaveItem(__instance.SubclassName,"moontouchedtrait1ba") ||AtOManager.Instance.CharacterHaveItem(__instance.SubclassName,"moontouchedtrait1bb") );
            if (!hasWaning && __instance != null && __instance.Alive && __instance.GetAuraCharges("scourge") > 0)
            {
                int nToApply = Mathf.RoundToInt(__instance.GetAuraCharges("scourge") * 0.5f);
                LogDebug($"BeginTurnPostfix - Character {__instance.SourceName} Has {Mathf.RoundToInt(__instance.GetAuraCharges("scourge"))} Scourge, Applying {nToApply} Scourge");
                __instance.HealAuraCurse(scourge);
                __instance.SetAura(__instance, scourge, nToApply, useCharacterMods: false, canBePreventable: false);
            }
        }





        [HarmonyPrefix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPostfix()
        {
            isDamagePreviewActive = false;
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
            string traitOfInterest = trait4a;
            if (!IsLivingHero(__instance) || !__instance.HaveTrait(traitOfInterest))
            {
                return;
            }

            // int nInsane = __instance.GetAuraCharges("insane");
            int nToIncrease = __instance.GetAuraCharges("insane");
            // int nToIncrease = Mathf.FloorToInt(nInsane * 0.1f);
            if (nToIncrease <= 0)
            {
                return;
            }
            if (!__result.ContainsKey("shield"))
            {
                __result["shield"] = 0;
            }
            __result["shield"] += nToIncrease;

            if (!__result.ContainsKey("block"))
            {
                __result["block"] = 0;
            }
            __result["block"] += nToIncrease;
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

            if (__instance.Id == "moontouchedtrait1b" || __instance.Id == "moontouchedtrait1ba" || __instance.Id == "moontouchedtrait1bb")
            {
                string waningMoonText = $"{SpriteText("scourge")} on this hero stacks and is lost at the end of turn\n";
                stringBuilder1.Insert(0, waningMoonText);
            }

            if (__instance.Id == "moontouchedtrait3b" || __instance.Id == "moontouchedtrait3ba" || __instance.Id == "moontouchedtrait3bb")
            {
                string crackHeadsText = $"{SpriteText("crack")} on enemies reduces {SpriteText("mind")} resistance by 1% per charge\n";
                stringBuilder1.Insert(0, crackHeadsText);
            }

            // if (__instance.Id == "flashheal" || __instance.Id == "flashheala" || __instance.Id == "flashhealb")
            // {
            //     string textToAdd = $"Testing\n";
            //     stringBuilder1.Insert(0, textToAdd);
            // }

            BinbinNormalizeDescription(ref __instance, stringBuilder1);
        }

        public static void BinbinNormalizeDescription(ref CardData __instance, StringBuilder stringBuilder)
        {
            stringBuilder.Replace("<c>", "<color=#5E3016>");
            stringBuilder.Replace("</c>", "</color>");
            stringBuilder.Replace("<nb>", "<nobr>");
            stringBuilder.Replace("</nb>", "</nobr>");
            stringBuilder.Replace("<br1>", "<br><line-height=15%><br></line-height>");
            stringBuilder.Replace("<br2>", "<br><line-height=30%><br></line-height>");
            stringBuilder.Replace("<br3>", "<br><line-height=50%><br></line-height>");
            string descriptionNormalized = stringBuilder.ToString();
            descriptionNormalized = Regex.Replace(descriptionNormalized, "[,][ ]*(<(.*?)>)*(.)", (MatchEvaluator)(m => m.ToString().ToLower()));
            descriptionNormalized = Regex.Replace(descriptionNormalized, "<br>\\w", (MatchEvaluator)(m => m.ToString().ToUpper()));
            Globals.Instance.CardsDescriptionNormalized[__instance.Id] = stringBuilder.ToString();
            __instance.DescriptionNormalized = descriptionNormalized;
            Traverse.Create(__instance).Field("description").SetValue(descriptionNormalized);
            Traverse.Create(__instance).Field("descriptionNormalized").SetValue(descriptionNormalized);
        }

        public static string SpriteText(string sprite)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string text = sprite.ToLower().Replace(" ", "");
            switch (text)
            {
                case "block":
                case "card":
                    stringBuilder.Append("<space=.2>");
                    break;
                case "piercing":
                    stringBuilder.Append("<space=.4>");
                    break;
                case "bleed":
                    stringBuilder.Append("<space=.1>");
                    break;
                case "bless":
                    stringBuilder.Append("<space=.1>");
                    break;
                default:
                    stringBuilder.Append("<space=.3>");
                    break;
            }
            stringBuilder.Append(" <space=-.2>");
            stringBuilder.Append("<size=+.1><sprite name=");
            stringBuilder.Append(text);
            stringBuilder.Append("></size>");
            switch (text)
            {
                case "bleed":
                    stringBuilder.Append("<space=-.4>");
                    break;
                case "card":
                    stringBuilder.Append("<space=-.2>");
                    break;
                case "powerful":
                case "fury":
                    stringBuilder.Append("<space=-.1>");
                    break;
                default:
                    stringBuilder.Append("<space=-.2>");
                    break;
                case "reinforce":
                case "fire":
                    break;
            }
            return stringBuilder.ToString();
        }

    }
}

