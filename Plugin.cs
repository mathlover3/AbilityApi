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
using UnityEngine.SceneManagement;

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
            /*testAbilityPrefab = Api.ConstructInstantAbility<InstantTestAbility>("A Ability");
            testAbilityTex = Api.LoadImage(Path.Combine(directoryToModFolder, "BlinkTest.png"));
            testSprite = Sprite.Create(testAbilityTex, new Rect(0f, 0f, testAbilityTex.width, testAbilityTex.height), new Vector2(0.5f, 0.5f));
            //dont use the same name multiple times or it will break stuff
            NamedSprite test = new NamedSprite("A Custom Ability", testSprite, testAbilityPrefab.gameObject, true);
            Api.RegisterNamedSprites(test, true);*/
            

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
                    __instance.spriteRen.material.SetVector("_CircleExtents", new Vector4(0, 0, 0 ,0));
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
                Api.AbilityGrid = __instance;
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
        public static class SlimeControllerAwakePatch
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
        [HarmonyPatch(typeof(SlimeController), nameof(SlimeController.DropAbilities))]
        public static class SlimeControllerDropAbilitiesPatch
        {
            public static bool Prefix(SlimeController __instance)
            {
                if (!GameSession.IsInitialized() || GameSessionHandler.HasGameEnded() || __instance.abilities.Count <= 0)
                {
                    return false;
                }
                PlayerHandler.Get().GetPlayer(__instance.playerNumber);
                for (int i = 0; i < __instance.AbilityReadyIndicators.Length; i++)
                {
                    if (__instance.AbilityReadyIndicators[i] != null)
                    {
                        __instance.AbilityReadyIndicators[i].InstantlySyncTransform();
                    }
                }
                int num = Settings.Get().NumberOfAbilities - 1;
                while (num >= 0 && (num >= __instance.AbilityReadyIndicators.Length || __instance.AbilityReadyIndicators[num] == null))
                {
                    num--;
                }
                if (num < 0)
                {
                    return false;
                }
                Vec2 launchDirection = Vec2.NormalizedSafe(Vec2.up + new Vec2(Updater.RandomFix((Fix)(-0.3f), (Fix)0.3f), (Fix)0L));
                DynamicAbilityPickup dynamicAbilityPickup = FixTransform.InstantiateFixed<DynamicAbilityPickup>(__instance.abilityPickupPrefab, __instance.body.position);
                Sprite primarySprite = __instance.AbilityReadyIndicators[num].GetPrimarySprite();
                NamedSprite namedSprite = new();
                if (__instance.abilityIconsFull.IndexOf(primarySprite) != -1)
                {
                    namedSprite = __instance.abilityIconsFull.sprites[__instance.abilityIconsFull.IndexOf(primarySprite)];
                    Debug.Log("droping normal ability");
                }
                else
                {
                    Debug.Log("droping custom ability");
                    namedSprite = Api.CustomAbilitySpritesWithBackroundList.sprites[Api.CustomAbilitySpritesWithBackroundList.IndexOf(primarySprite)];
                }
                if (namedSprite.associatedGameObject == null)
                {
                    namedSprite = __instance.abilityIconsDemo.sprites[__instance.abilityIconsDemo.IndexOf(primarySprite)];
                }
                dynamicAbilityPickup.InitPickup(namedSprite.associatedGameObject, primarySprite, launchDirection);
                return false;
            }

        }
        [HarmonyPatch(typeof(PlayerCollision), nameof(PlayerCollision.SpawnClone))]
        public static class PlayerCollisionPatch
        {
            public static bool Prefix(PlayerCollision __instance, Player player, SlimeController slimeContToRevive, Vec2 targetPosition)
            {
                return false;
            }
            public static void Postfix(PlayerCollision __instance, Player player, SlimeController slimeContToRevive, Vec2 targetPosition, ref SlimeController __result)
            {
                if (player.playersAndClonesStillAlive < Constants.MaxClones + 1)
                {
                    int playersAndClonesStillAlive = player.playersAndClonesStillAlive;
                    player.playersAndClonesStillAlive = playersAndClonesStillAlive + 1;
                    SlimeController slimeController = FixTransform.InstantiateFixed<SlimeController>(__instance.reviveEffectPrefab.emptyPlayerPrefab, targetPosition);
                    slimeController.playerNumber = player.Id;
                    slimeController.GetPlayerSprite().sprite = null;
                    slimeController.GetPlayerSprite().material = player.Color;
                    List<AbilityMonoBehaviour> list = new List<AbilityMonoBehaviour>();
                    for (int i = 0; i < slimeContToRevive.abilities.Count; i++)
                    {
                        int index = slimeContToRevive.abilityIcons.IndexOf(slimeContToRevive.AbilityReadyIndicators[i].GetPrimarySprite());
                        GameObject gameObject = null;
                        if (index != -1)
                        {
                            gameObject = FixTransform.InstantiateFixed(slimeContToRevive.abilityIcons.sprites[index].associatedGameObject, Vec2.zero);
                        }
                        else
                        {
                            int index2 = Api.CustomAbilitySpritesWithBackroundList.IndexOf(slimeContToRevive.AbilityReadyIndicators[i].GetPrimarySprite());

                            gameObject = FixTransform.InstantiateFixed(Api.CustomAbilitySpritesWithBackroundList.sprites[index2].associatedGameObject, Vec2.zero);
                        }
                        gameObject.gameObject.SetActive(false);
                        list.Add(gameObject.GetComponent<AbilityMonoBehaviour>());
                    }
                    slimeController.abilities = list;
                    AbilityReadyIndicator[] array = new AbilityReadyIndicator[3];
                    for (int j = 0; j < slimeContToRevive.AbilityReadyIndicators.Length; j++)
                    {
                        if (!(slimeContToRevive.AbilityReadyIndicators[j] == null))
                        {
                            array[j] = UnityEngine.Object.Instantiate<GameObject>(__instance.reviveEffectPrefab.AbilityReadyIndicators[j]).GetComponent<AbilityReadyIndicator>();
                            array[j].SetSprite(slimeContToRevive.AbilityReadyIndicators[j].GetPrimarySprite(), true);
                            array[j].Init();
                            array[j].SetColor(__instance.reviveEffectPrefab.teamColors.teamColors[player.Team].fill);
                            array[j].GetComponent<FollowTransform>().Leader = slimeController.transform;
                            array[j].gameObject.SetActive(false);
                        }
                    }
                    slimeController.AbilityReadyIndicators = array;
                    slimeController.PrepareToRevive(targetPosition);
                    __result = slimeController;
                    return;
                }
                __result = null;
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
        [HarmonyPatch(typeof(CharacterSelectHandler_online), nameof(CharacterSelectHandler_online.InitPlayer))]
        public static class CharacterSelectHandler_onlinePatch
        {
            public static Player playerToReturn;
            public static bool Prefix(CharacterSelectHandler_online __instance, int id, byte color, byte team, byte ability1, byte ability2, byte ability3, int nrOfAbilities, PlayerColors playerColors)
            {
                NamedSpriteList abilityIcons = SteamManager.instance.abilityIcons;
                Player player = new Player();
                player.Id = id;
                player.Color = playerColors.playerColors[(int)color].playerMaterial;
                player.Team = (int)team;
                Debug.Log($"team is {team}");
                if (nrOfAbilities > 0)
                {
                    if (Api.CustomAbilitySpritesWithBackrounds.ContainsKey(abilityIcons.sprites[(int)ability1]))
                    {
                        player.Abilities.Add(Api.CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[(int)ability1]][team].associatedGameObject);
                        player.AbilityIcons.Add(Api.CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[(int)ability1]][team].sprite);
                    }
                    else
                    {
                        player.Abilities.Add(abilityIcons.sprites[(int)ability1].associatedGameObject);
                        player.AbilityIcons.Add(abilityIcons.sprites[(int)ability1].sprite);
                    }

                }
                if (nrOfAbilities > 1)
                {
                    if (Api.CustomAbilitySpritesWithBackrounds.ContainsKey(abilityIcons.sprites[(int)ability2]))
                    {
                        player.Abilities.Add(Api.CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[(int)ability2]][team].associatedGameObject);
                        player.AbilityIcons.Add(Api.CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[(int)ability2]][team].sprite);
                    }
                    else
                    {
                        player.Abilities.Add(abilityIcons.sprites[(int)ability2].associatedGameObject);
                        player.AbilityIcons.Add(abilityIcons.sprites[(int)ability2].sprite);
                    }
                }
                if (nrOfAbilities > 2)
                {
                    if (Api.CustomAbilitySpritesWithBackrounds.ContainsKey(abilityIcons.sprites[(int)ability3]))
                    {
                        player.Abilities.Add(Api.CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[(int)ability3]][team].associatedGameObject);
                        player.AbilityIcons.Add(Api.CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[(int)ability3]][team].sprite);
                    }
                    else
                    {
                        player.Abilities.Add(abilityIcons.sprites[(int)ability3].associatedGameObject);
                        player.AbilityIcons.Add(abilityIcons.sprites[(int)ability3].sprite);
                    }
                }
                player.IsLocalPlayer = false;
                playerToReturn = player;
                return false;
            }
            public static void Postfix(CharacterSelectHandler_online __instance, ref Player __result)
            {
                __result = playerToReturn;
            }
        }
        [HarmonyPatch(typeof(CharacterSelectHandler), nameof(CharacterSelectHandler.TryStartGame_inner))]
        public static class CharacterSelectHandlerPatch
        {
            public static bool Prefix(CharacterSelectHandler __instance)
            {
                if (CharacterSelectHandler.startButtonAvailable && CharacterSelectHandler.allReadyForMoreThanOneFrame)
                {
                    AudioManager audioManager = AudioManager.Get();
                    if (audioManager != null)
                    {
                        audioManager.Play("startGame");
                    }
                    CharacterSelectHandler.startButtonAvailable = false;
                    List<Player> list = PlayerHandler.Get().PlayerList();
                    list.Clear();
                    int num = 1;
                    NamedSpriteList abilityIcons = SteamManager.instance.abilityIcons;
                    for (int i = 0; i < __instance.characterSelectBoxes.Length; i++)
                    {
                        if (__instance.characterSelectBoxes[i].menuState == CharSelectMenu.ready)
                        {
                            PlayerInit playerInit = __instance.characterSelectBoxes[i].playerInit;
                            Player player = new Player(num, playerInit.team);
                            player.Color = __instance.playerColors[playerInit.color].playerMaterial;
                            player.UsesKeyboardAndMouse = playerInit.usesKeyboardMouse;
                            player.CanUseAbilities = true;
                            player.inputDevice = playerInit.inputDevice;
                            player.Abilities = new List<GameObject>(3);
                            player.AbilityIcons = new List<Sprite>(3);
                            player.Abilities.Add(abilityIcons.sprites[playerInit.ability0].associatedGameObject);
                            //if its a custom ability then use the one that has the backround
                            if (Api.Sprites.Contains(abilityIcons.sprites[playerInit.ability0]))
                            {
                                var AbilityWithBackroundsList = Api.CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[playerInit.ability0]];
                                player.AbilityIcons.Add(AbilityWithBackroundsList[player.Team].sprite);
                            }
                            else
                            {
                                //if its not a custom ability do it normaly
                                player.AbilityIcons.Add(abilityIcons.sprites[playerInit.ability0].sprite);
                            }
                            
                            Settings settings = Settings.Get();
                            if (settings != null && settings.NumberOfAbilities > 1)
                            {
                                player.Abilities.Add(abilityIcons.sprites[playerInit.ability1].associatedGameObject);
                                //if its a custom ability then use the one that has the backround
                                if (Api.Sprites.Contains(abilityIcons.sprites[playerInit.ability1]))
                                {
                                    var AbilityWithBackroundsList = Api.CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[playerInit.ability1]];
                                    player.AbilityIcons.Add(AbilityWithBackroundsList[player.Team].sprite);
                                }
                                else
                                {
                                    //if its not a custom ability do it normaly
                                    player.AbilityIcons.Add(abilityIcons.sprites[playerInit.ability1].sprite);
                                }
                            }
                            Settings settings2 = Settings.Get();
                            if (settings2 != null && settings2.NumberOfAbilities > 2)
                            {
                                player.Abilities.Add(abilityIcons.sprites[playerInit.ability2].associatedGameObject);
                                //if its a custom ability then use the one that has the backround
                                if (Api.Sprites.Contains(abilityIcons.sprites[playerInit.ability2]))
                                {
                                    var AbilityWithBackroundsList = Api.CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[playerInit.ability2]];
                                    player.AbilityIcons.Add(AbilityWithBackroundsList[player.Team].sprite);
                                }
                                else
                                {
                                    //if its not a custom ability do it normaly
                                    player.AbilityIcons.Add(abilityIcons.sprites[playerInit.ability2].sprite);
                                }
                            }
                            player.CustomKeyBinding = playerInit.keybindOverride;
                            num++;
                            list.Add(player);
                        }
                    }
                    GameSession.Init();
                    SceneManager.LoadScene("Level1");
                    if (!WinnerTriangleCanvas.HasBeenSpawned)
                    {
                        SceneManager.LoadScene("winnerTriangle", LoadSceneMode.Additive);
                    }
                }
                return false;
            }
        }
    }
}