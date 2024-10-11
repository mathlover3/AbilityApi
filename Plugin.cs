using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using UnityEngine.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System;
using static AbilityApi.Api;

namespace AbilityApi.Internal
{
    [BepInPlugin("com.David_Loves_JellyCar_Worlds.AbilityApi", "AbilityApi", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public struct CircleEntry
        {
            public Sprite sprite;
            public Vector4 center;
        }
        public static int defaultAbilityCount = 30;
        public static Vector4 defaultExtents = new(0.04882813f, 0.04882813f, 0.08300781f, 0.9287109f);
        public static string directoryToModFolder = "";
        public static InstantTestAbility testAbilityPrefab;
        public static Texture2D testAbilityTex;
        public static Sprite testSprite;
        public static List<Texture2D> BackroundSprites = new();
        private void Awake()
        {
            Logger.LogInfo("Plugin AbilityApi is loaded!");

            Harmony harmony = new Harmony("com.David_Loves_JellyCar_Worlds.AbilityApi");

            Logger.LogInfo("harmany created");
            harmony.PatchAll();
            Logger.LogInfo("AbilityApi Patch Compleate!");
            new Harmony("AbilityApi").PatchAll(Assembly.GetExecutingAssembly());

            directoryToModFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //load backrounds
            BackroundSprites.Add(Api.LoadImage(Path.Combine(directoryToModFolder, "BlueTeam.png")));
            BackroundSprites.Add(Api.LoadImage(Path.Combine(directoryToModFolder, "OrangeTeam.png")));
            BackroundSprites.Add(Api.LoadImage(Path.Combine(directoryToModFolder, "GreenTeam.png")));
            BackroundSprites.Add(Api.LoadImage(Path.Combine(directoryToModFolder, "PinkTeam.png")));
            //this name will automaticly be renamed to whatever the ability name is. 
            testAbilityPrefab = Api.ConstructInstantAbility<InstantTestAbility>("BlinkAbility");
            testAbilityTex = Api.LoadImage(Path.Combine(directoryToModFolder, "BlinkTest.png"));
            testAbilityTex = Api.OverlayBackround(testAbilityTex, BackroundSprites[0]);
            testSprite = Sprite.Create(testAbilityTex, new Rect(0f, 0f, testAbilityTex.width, testAbilityTex.height), new Vector2(0.5f, 0.5f));
            //dont use the same name multiple times or it will break stuff
            NamedSprite test = new NamedSprite("BlinkAbility", testSprite, testAbilityPrefab.gameObject, true);
            Api.RegisterNamedSprites(test);

        }


        /*[HarmonyPatch(typeof(ClassToPatch))]
        public class myPatches
        {
            [HarmonyPatch("FuncToPatch")]
            [HarmonyPrefix]
            private static void FuncName_PlugenName_Plug(ClassToPatch __instance )
            {
                //insert code here. and remeber the __instance
            }
        }*/
        [HarmonyPatch(typeof(AbilityReadyIndicator), nameof(AbilityReadyIndicator.SetSprite))]
        public static class SpriteCirclePatch
        {
            public static void Postfix(AbilityReadyIndicator __instance, ref Sprite sprite)
            {
                if (Api.CustomAbilityTexstures.Contains(sprite.texture))
                {
                    //basicly set the backround up
                    //AbilityTextureMetaData metadata = Api.CustomAbilityTexstures[sprite.texture];
                    //Vector2 CircleCenter = new Vector2(metadata.BackroundTopLeftCourner.x + (metadata.BackroundSize.x/2), metadata.BackroundTopLeftCourner.y + (metadata.BackroundSize.y / 2));
                    //Vector2 CircleCenterZeroToOne = CircleCenter / metadata.TotalSize;
                    //__instance.spriteRen.material.SetVector("_CircleExtents", new Vector4(1/ metadata.BackroundSize.x, 1 / metadata.BackroundSize.y, CircleCenterZeroToOne.x, CircleCenterZeroToOne.y));
                    __instance.spriteRen.material.SetVector("_CircleExtents", new Vector4(1, 1, 1 ,1));
                }

                else
                {
                    __instance.spriteRen.material.SetVector("_CircleExtents", defaultExtents);
                }
            }
        }
        [HarmonyPatch(typeof(AbilityGrid), nameof(AbilityGrid.Awake))]
        public static class AbilityGridPatch
        {
            public static void Postfix(AbilityGrid __instance)
            {
                Api.abilityGrid = __instance;
                //add the sprites (idk why but awake is called multiple times here
                if (__instance.abilityIcons.sprites.Count == defaultAbilityCount)
                {
                    Debug.Log("adding");
                    __instance.abilityIcons.sprites.AddRange(Api.Sprites);
                }

            }
        }
        [HarmonyPatch(typeof(AchievementHandler), nameof(AchievementHandler.Awake))]
        public static class AchievementHandlerPatch
        {
            public static void Postfix(AchievementHandler __instance)
            {
                //add the sprites
                if (__instance.abilityIcons.sprites.Count == defaultAbilityCount)
                {
                    __instance.abilityIcons.sprites.AddRange(Api.Sprites);
                }
            }
        }
        [HarmonyPatch(typeof(CharacterStatsList), nameof(CharacterStatsList.TryStartNextLevel_online))]
        public static class CharacterStatsListPatch
        {
            public static void Prefix(CharacterStatsList __instance)
            {
                //add the sprites if they havent already been added (all mods should add there abilitys on awake)
                if (__instance.abilityIcons.sprites.Count == defaultAbilityCount)
                {
                    __instance.abilityIcons.sprites.AddRange(Api.Sprites);
                }
                
            }
        }
        [HarmonyPatch(typeof(DynamicAbilityPickup), nameof(DynamicAbilityPickup.Awake))]
        public static class DynamicAbilityPickupPatch
        {
            public static void Postfix(DynamicAbilityPickup __instance)
            {
                //add the sprites
                if (__instance.abilityIcons.sprites.Count == defaultAbilityCount)
                {
                    __instance.abilityIcons.sprites.AddRange(Api.Sprites);
                }
            }
        }
        [HarmonyPatch(typeof(MidGameAbilitySelect), nameof(MidGameAbilitySelect.Awake))]
        public static class MidGameAbilitySelectPatch
        {
            public static void Postfix(MidGameAbilitySelect __instance)
            {
                //add the sprites
                if (__instance.AbilityIcons.sprites.Count == defaultAbilityCount)
                {
                    Debug.Log("adding mid ability select");
                    __instance.AbilityIcons.sprites.AddRange(Api.Sprites);
                    __instance.localAbilityIcons.sprites.AddRange(Api.Sprites);
                }

            }
        }
        [HarmonyPatch(typeof(RandomAbility), nameof(RandomAbility.Awake))]
        public static class RandomAbilityPatch
        {
            public static void Postfix(RandomAbility __instance)
            {
                //add the sprites
                if (__instance.abilityIcons.sprites.Count == defaultAbilityCount)
                {
                    __instance.abilityIcons.sprites.AddRange(Api.Sprites);
                }
            }
        }
        [HarmonyPatch(typeof(SelectAbility), nameof(SelectAbility.Awake))]
        public static class SelectAbilityPatch
        {
            public static void Postfix(SelectAbility __instance)
            {
                //add the sprites
                if (__instance.abilityIcons.sprites.Count == defaultAbilityCount)
                {
                    __instance.abilityIcons.sprites.AddRange(Api.Sprites);
                }
            }
        }
        [HarmonyPatch(typeof(SlimeController), nameof(SlimeController.Awake))]
        public static class SlimeControllerPatch
        {
            public static void Postfix(SlimeController __instance)
            {
                //add the sprites
                if (__instance.abilityIconsFull.sprites.Count == defaultAbilityCount)
                {
                    __instance.abilityIconsFull.sprites.AddRange(Api.Sprites);
                }
            }
        }
        [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.Awake))]
        public static class SteamManagerPatch
        {
            public static void Postfix(SteamManager __instance)
            {
                //add the sprites
                if (__instance.abilityIconsFull.sprites.Count == defaultAbilityCount)
                {
                    __instance.abilityIconsFull.sprites.AddRange(Api.Sprites);
                }
            }
        }
        [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.ChangePlayerAbilites))]
        public static class SteamManagerPatch2
        {
            public static void Prefix(SteamManager __instance, Player player, byte ability1, byte ability2, byte ability3, int nrOfAbilities, NamedSpriteList abilityIcons)
            {
                Debug.Log($"abilitys: {ability1}, {ability2}, {ability3}");
            }
        }
    }
    }