using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using static AbilityApi.Api;
using UnityEngine.SceneManagement;
using Ability_Api;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace AbilityApi.Internal
{
    [BepInPlugin("com.AbilityAPITeam.AbilityAPI", "Ability API", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        GameObject BowObject;
        public static BoplBody Arrow;
        public struct CircleEntry
        {
            public Sprite sprite;
            public Vector4 center;
        }
        public static int defaultAbilityCount = 30;
        public static Vector4 defaultExtents = new(0.04882813f, 0.04882813f, 0.08300781f, 0.9287109f);
        public static string directoryToModFolder = "";
        public static GunAbility testAbilityPrefab;
        public static Texture2D testAbilityTex;
        public static Sprite testSprite;
        public static List<Texture2D> BackroundSprites = new();
        public static bool hasDied = false;
        private void Awake()
        {
            Logger.LogInfo("Plugin AbilityApi is loaded!");

            Harmony harmony = new Harmony("com.David_Loves_JellyCar_Worlds.AbilityApi");

            Logger.LogInfo("harmony created");
            harmony.PatchAll();
            Logger.LogInfo("AbilityApi Patch Complete!");
            new Harmony("AbilityApi").PatchAll(Assembly.GetExecutingAssembly());

            directoryToModFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // Load backgrounds
            BackroundSprites.Add(LoadImageFromResources("Ability_Api", "BlueTeam.png"));
            BackroundSprites.Add(LoadImageFromResources("Ability_Api", "OrangeTeam.png"));
            BackroundSprites.Add(LoadImageFromResources("Ability_Api", "GreenTeam.png"));
            BackroundSprites.Add(LoadImageFromResources("Ability_Api", "PinkTeam.png"));
            // This name will automaticly be renamed to whatever the ability name is. 
            /*testAbilityPrefab = Api.ConstructInstantAbility<InstantTestAbility>("A Ability");
            testAbilityTex = Api.LoadImage(Path.Combine(directoryToModFolder, "BlinkTest.png"));
            testSprite = Sprite.Create(testAbilityTex, new Rect(0f, 0f, testAbilityTex.width, testAbilityTex.height), new Vector2(0.5f, 0.5f));
            //dont use the same name multiple times or it will break stuff
            NamedSprite test = new NamedSprite("A Custom Ability", testSprite, testAbilityPrefab.gameObject, true);
            Api.RegisterNamedSprites(test, true);*/
            //GunAbilityTest gun = new GunAbilityTest();
            //gun.SetUpGun();

            GameObject[] array = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
            GameObject[] array2 = array;

            foreach (GameObject val2 in array2)
            {
                if (((UnityEngine.Object)val2).name == "Bow")
                {
                    BowObject = val2;
                    break;
                }
            }
            Component component = BowObject.GetComponent(typeof(BowTransform));
            BowTransform obj = (BowTransform)(object)((component is BowTransform) ? component : null);
            Arrow = (BoplBody)AccessTools.Field(typeof(BowTransform), "Arrow").GetValue(obj);

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
            public static void Postfix(AbilityReadyIndicator __instance, ref Sprite sprite, ref SpriteRenderer ___spriteRen)
            {
                if (CustomAbilityTexstures.Contains(sprite.texture))
                {
                    //basicly set the backround up
                    //AbilityTextureMetaData metadata = Api.CustomAbilityTexstures[sprite.texture];
                    //Vector2 CircleCenter = new Vector2(metadata.BackroundTopLeftCourner.x + (metadata.BackroundSize.x/2), metadata.BackroundTopLeftCourner.y + (metadata.BackroundSize.y / 2));
                    //Vector2 CircleCenterZeroToOne = CircleCenter / metadata.TotalSize;
                    //__instance.spriteRen.material.SetVector("_CircleExtents", new Vector4(1/ metadata.BackroundSize.x, 1 / metadata.BackroundSize.y, CircleCenterZeroToOne.x, CircleCenterZeroToOne.y));
                    __instance.GetSpriteRen().material.SetVector("_CircleExtents", new Vector4(0, 0, 0, 0));
                }

                else
                {
                    __instance.GetSpriteRen().material.SetVector("_CircleExtents", defaultExtents);
                }
            }
        }
        [HarmonyPatch(typeof(AbilityGrid), "Awake")]
        public static class AbilityGridPatch
        {
            public static void Postfix(AbilityGrid __instance)
            {
                abilityGrid = __instance;
                //add the sprites (idk why but awake is called multiple times here
                if (__instance.abilityIcons.sprites.Count == defaultAbilityCount)
                {
                    Debug.Log("adding");
                    __instance.abilityIcons.sprites.AddRange(Sprites);
                }

            }
        }
        [HarmonyPatch(typeof(AchievementHandler), "Awake")]
        public static class AchievementHandlerPatch
        {
            public static void Postfix(AchievementHandler __instance)
            {
                //add the sprites
                if (__instance.abilityIcons.sprites.Count == defaultAbilityCount)
                {
                    __instance.abilityIcons.sprites.AddRange(Sprites);
                }
            }
        }
        [HarmonyPatch(typeof(CharacterStatsList), "TryStartNextLevel_online")]
        public static class CharacterStatsListPatch
        {
            public static void Prefix(CharacterStatsList __instance)
            {
                //add the sprites if they havent already been added (all mods should add there abilitys on awake)
                if (__instance.abilityIcons.sprites.Count == defaultAbilityCount)
                {
                    __instance.abilityIcons.sprites.AddRange(Sprites);
                }

            }
        }
        [HarmonyPatch(typeof(DynamicAbilityPickup), "Awake")]
        public static class DynamicAbilityPickupPatch
        {
            public static void Postfix(DynamicAbilityPickup __instance)
            {
                //add the sprites
                if (__instance.abilityIcons.sprites.Count == defaultAbilityCount)
                {
                    __instance.abilityIcons.sprites.AddRange(Sprites);
                }
            }
        }
        [HarmonyPatch(typeof(MidGameAbilitySelect), "Awake")]
        public static class MidGameAbilitySelectPatch
        {
            public static void Postfix(MidGameAbilitySelect __instance, ref NamedSpriteList ___localAbilityIcons)
            {
                //add the sprites
                if (__instance.AbilityIcons.sprites.Count == defaultAbilityCount)
                {
                    __instance.AbilityIcons.sprites.AddRange(Sprites);
                    ___localAbilityIcons.sprites.AddRange(Sprites);
                }

            }
        }
        [HarmonyPatch(typeof(RandomAbility), "Awake")]
        public static class RandomAbilityPatch
        {
            public static void Postfix(RandomAbility __instance)
            {
                //add the sprites
                if (__instance.abilityIcons.sprites.Count == defaultAbilityCount)
                {
                    __instance.abilityIcons.sprites.AddRange(Sprites);
                }
            }
        }
        [HarmonyPatch(typeof(SelectAbility), "Awake")]
        public static class SelectAbilityPatch
        {
            public static void Postfix(SelectAbility __instance)
            {
                // Add the sprites
                if (__instance.abilityIcons.sprites.Count == defaultAbilityCount)
                {
                    __instance.abilityIcons.sprites.AddRange(Sprites);
                }
            }
        }
        [HarmonyPatch(typeof(SlimeController), "Awake")]
        public static class SlimeControllerAwakePatch
        {
            public static void Postfix(SlimeController __instance)
            {
                //add the sprites
                if (__instance.abilityIconsFull.sprites.Count == defaultAbilityCount)
                {
                    __instance.abilityIconsFull.sprites.AddRange(Sprites);
                }
            }
        }
        [HarmonyPatch(typeof(SlimeController), nameof(SlimeController.AddAdditionalAbility))]
        public class AddMoreAbilityPatch
        {
            public static bool Prefix(SlimeController __instance, Fix[] ___abilityCooldownTimers, AbilityMonoBehaviour ability, PlayerCollision ___playerCollision, Sprite indicatorSprite, GameObject abilityPrefab)
            {
                Debug.Log("Added Additional");
                NamedSpriteList abilityIcons = SteamManager.instance.abilityIcons;
                Player player = PlayerHandler.Get().GetPlayer(__instance.playerNumber);

                if (__instance.abilities.Count == 3)
                {
                    __instance.abilities[2] = ability;
                    PlayerHandler.Get().GetPlayer(__instance.playerNumber).CurrentAbilities[2] = abilityPrefab;
                    //__instance.AbilityReadyIndicators[2].SetSprite(indicatorSprite, true);
                    // If its a custom ability then use the one that has the backround
                    if(ability.GetComponentInParent<DummyAbility>())
                    {
                        //var AbilityWithBackroundsList = CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[___nextAbiltityToChangeOnPickup]];
                        var TextureWithBackround = Api.OverlayBackround(indicatorSprite.texture, BackroundSprites[player.Team]);
                        var SpriteWithBackround = Sprite.Create(TextureWithBackround, new Rect(0f, 0f, TextureWithBackround.width, TextureWithBackround.height), new Vector2(0.5f, 0.5f));
                        __instance.AbilityReadyIndicators[2].SetSprite(SpriteWithBackround, true);
                        Debug.Log("custom");
                    }
                    else
                    {
                        __instance.AbilityReadyIndicators[2].SetSprite(indicatorSprite, true);
                    }
                    __instance.AbilityReadyIndicators[2].ResetAnimation();
                    ___abilityCooldownTimers[2] = (Fix)100000L;
                }
                else if (__instance.abilities.Count > 0 && __instance.AbilityReadyIndicators[0] != null)
                {
                    __instance.abilities.Add(ability);
                    PlayerHandler.Get().GetPlayer(__instance.playerNumber).CurrentAbilities.Add(abilityPrefab);
                    int num = __instance.abilities.Count - 1;
                    if (num >= __instance.AbilityReadyIndicators.Length || __instance.AbilityReadyIndicators[num] == null)
                    {
                        GameObject original = ___playerCollision.reviveEffectPrefab.AbilityReadyIndicators[num];
                        AbilityReadyIndicator[] array = new AbilityReadyIndicator[__instance.abilities.Count];
                        for (int i = 0; i < num; i++)
                        {
                            array[i] = __instance.AbilityReadyIndicators[i];
                        }
                        __instance.AbilityReadyIndicators = array;
                        __instance.AbilityReadyIndicators[num] = UnityEngine.Object.Instantiate<GameObject>(original).GetComponent<AbilityReadyIndicator>();
                        __instance.AbilityReadyIndicators[num].SetSprite(indicatorSprite, true);
                        __instance.AbilityReadyIndicators[num].Init();
                        __instance.AbilityReadyIndicators[num].SetColor(___playerCollision.reviveEffectPrefab.teamColors.teamColors[player.Team].fill);
                        __instance.AbilityReadyIndicators[num].GetComponent<FollowTransform>().Leader = __instance.transform;
                    }
                    __instance.AbilityReadyIndicators[num].SetSprite(indicatorSprite, true);
                    __instance.AbilityReadyIndicators[num].ResetAnimation();
                    ___abilityCooldownTimers[num] = (Fix)100000L;
                    for (int j = 0; j < __instance.abilities.Count; j++)
                    {
                        if (__instance.abilities[j] == null || __instance.abilities[j].IsDestroyed)
                        {
                            return false;
                        }
                        __instance.AbilityReadyIndicators[j].gameObject.SetActive(true);
                        __instance.AbilityReadyIndicators[j].InstantlySyncTransform();
                    }
                }
                else
                {
                    GameObject gameObject = FixTransform.InstantiateFixed(abilityPrefab, Vec2.zero);
                    gameObject.gameObject.SetActive(false);
                    __instance.abilities = new List<AbilityMonoBehaviour>();
                    __instance.abilities.Add(gameObject.GetComponent<AbilityMonoBehaviour>());
                    AbilityReadyIndicator[] array2 = new AbilityReadyIndicator[1];
                    GameObject original2 = ___playerCollision.reviveEffectPrefab.AbilityReadyIndicators[0];
                    Player player2 = PlayerHandler.Get().GetPlayer(__instance.playerNumber);
                    array2[0] = UnityEngine.Object.Instantiate<GameObject>(original2).GetComponent<AbilityReadyIndicator>();
                    array2[0].SetSprite(indicatorSprite, true);
                    array2[0].Init();
                    array2[0].SetColor(___playerCollision.reviveEffectPrefab.teamColors.teamColors[player2.Team].fill);
                    array2[0].GetComponent<FollowTransform>().Leader = __instance.transform;
                    array2[0].gameObject.SetActive(true);
                    array2[0].InstantlySyncTransform();
                    ___abilityCooldownTimers[0] = (Fix)100L;
                    __instance.AbilityReadyIndicators = array2;
                }
                AudioManager.Get().Play("abilityPickup");


                return false;
            }
        }
        [HarmonyPatch(typeof(SlimeController), nameof(SlimeController.DropAbilities))]
        public static class SlimeControllerDropAbilitiesPatch
        {
            public static bool Prefix(SlimeController __instance, ref DynamicAbilityPickup ___abilityPickupPrefab)
            {
                if (!GameSession.IsInitialized() || GameSessionHandler.HasGameEnded() || __instance.abilities.Count <= 0)
                {
                    return false;
                }
                NamedSpriteList abilityIcons = SteamManager.instance.abilityIcons;
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
                
                if(hasDied == false)
                {
                    Vec2 launchDirection = Vec2.NormalizedSafe(Vec2.up + new Vec2(Updater.RandomFix((Fix)(-0.3f), (Fix)0.3f), (Fix)0L));
                    DynamicAbilityPickup dynamicAbilityPickup = FixTransform.InstantiateFixed<DynamicAbilityPickup>(___abilityPickupPrefab, __instance.body.position);
                    Sprite primarySprite = __instance.AbilityReadyIndicators[num].GetPrimarySprite();
                    NamedSprite namedSprite = new();
                    if (__instance.abilityIconsFull.IndexOf(primarySprite) != -1)
                    {
                        namedSprite = __instance.abilityIconsFull.sprites[__instance.abilityIconsFull.IndexOf(primarySprite)];
                        Debug.Log("droping normal ability");
                    }
                    else
                    {
                        Debug.Log("Dropping custom ability");
                        //namedSprite = abilityIcons.sprites[abilityIcons.IndexOf(primarySprite)];
                        namedSprite = CustomAbilitySpritesWithBackroundList.sprites[CustomAbilitySpritesWithBackroundList.IndexOf(primarySprite)];
                        primarySprite = CustomAbilitySpritesWithoutBackroundList.sprites[CustomAbilitySpritesWithBackroundList.IndexOf(primarySprite)].sprite;
                    }
                    if (namedSprite.associatedGameObject == null)
                    {
                        namedSprite = __instance.abilityIconsDemo.sprites[__instance.abilityIconsDemo.IndexOf(primarySprite)];
                    }
                    dynamicAbilityPickup.InitPickup(namedSprite.associatedGameObject, primarySprite, launchDirection);
                }
                if(hasDied)
                {
                    hasDied = false;
                }
                else
                {
                    hasDied = true;
                }
                
                return false;
            }

        }
        [HarmonyPatch(typeof(PlayerCollision), nameof(PlayerCollision.SpawnClone))]
        public static class PlayerCollisionPatch
        {
            public static bool Prefix()
            {
                return false;
            }
            public static void Postfix(PlayerCollision __instance, Player player, SlimeController slimeContToRevive, Vec2 targetPosition, ref SlimeController __result)
            {
                if (player.playersAndClonesStillAlive < Constants.MaxClones + 1)
                {
                    int playersAndClonesStillAlive = player.playersAndClonesStillAlive;
                    if(!player.stillAliveThisRound)
                    {
                        return;
                    }
                    player.playersAndClonesStillAlive = playersAndClonesStillAlive + 1;
                    SlimeController slimeController = FixTransform.InstantiateFixed<SlimeController>(__instance.reviveEffectPrefab.emptyPlayerPrefab, targetPosition);
                    slimeController.playerNumber = player.Id;
                    slimeController.GetPlayerSprite().sprite = null;
                    slimeController.GetPlayerSprite().material = player.Color;
                    List<AbilityMonoBehaviour> list = new();
                    for (int i = 0; i < slimeContToRevive.abilities.Count; i++)
                    {
                        int index = slimeContToRevive.abilityIcons.IndexOf(slimeContToRevive.AbilityReadyIndicators[i].GetPrimarySprite());
                        GameObject gameObject;
                        if (index != -1)
                        {
                            gameObject = FixTransform.InstantiateFixed(slimeContToRevive.abilityIcons.sprites[index].associatedGameObject, Vec2.zero);
                        }
                        else
                        {
                            int index2 = CustomAbilitySpritesWithBackroundList.IndexOf(slimeContToRevive.AbilityReadyIndicators[i].GetPrimarySprite());

                            gameObject = FixTransform.InstantiateFixed(CustomAbilitySpritesWithBackroundList.sprites[index2].associatedGameObject, Vec2.zero);
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
                            array[j] = Instantiate(__instance.reviveEffectPrefab.AbilityReadyIndicators[j]).GetComponent<AbilityReadyIndicator>();
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
        [HarmonyPatch(typeof(GameSessionHandler), "SpawnPlayers")]
        public static class SpawnPlayerPatch
        {
            public static void Prefix()
            {
                hasDied = false;
            }
        }
        [HarmonyPatch(typeof(PlayerCollision), nameof(PlayerCollision.killPlayer))]
        public static class PlayerCollisionPatchOnKill
        {
            public static void Prefix(PlayerCollision __instance, IPlayerIdHolder ___playerIdHolder)
            {
                Player player = PlayerHandler.Get().GetPlayer(___playerIdHolder.GetPlayerId());
                if(player.playersAndClonesStillAlive > 1)
                {
                    player.playersAndClonesStillAlive -= 1;
                }
                
            }
        }
        [HarmonyPatch(typeof(SteamManager), "Awake")]
        public static class SteamManagerPatch
        {
            public static void Postfix(SteamManager __instance, ref NamedSpriteList ___abilityIconsFull)
            {
                // Add the sprites
                if (___abilityIconsFull.sprites.Count == defaultAbilityCount)
                {
                    ___abilityIconsFull.sprites.AddRange(Sprites);
                }
            }
        }
        [HarmonyPatch(typeof(CharacterSelectHandler_online), "InitPlayer")]
        public static class CharacterSelectHandler_onlinePatch
        {
            public static Player playerToReturn;
            public static bool Prefix(int id, byte color, byte team, byte ability1, byte ability2, byte ability3, int nrOfAbilities, PlayerColors playerColors)
            {
                NamedSpriteList abilityIcons = SteamManager.instance.abilityIcons;
                Player player = new Player
                {
                    Id = id,
                    Color = playerColors.playerColors[(int)color].playerMaterial,
                    Team = (int)team
                };

                Debug.Log($"Team is {team}");
                if (nrOfAbilities > 0)
                {
                    if (CustomAbilitySpritesWithBackrounds.ContainsKey(abilityIcons.sprites[ability1]))
                    {
                        player.Abilities.Add(CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[ability1]][team].associatedGameObject);
                        player.AbilityIcons.Add(CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[ability1]][team].sprite);
                    }
                    else
                    {
                        player.Abilities.Add(abilityIcons.sprites[(int)ability1].associatedGameObject);
                        player.AbilityIcons.Add(abilityIcons.sprites[ability1].sprite);
                    }

                }
                if (nrOfAbilities > 1)
                {
                    if (CustomAbilitySpritesWithBackrounds.ContainsKey(abilityIcons.sprites[(int)ability2]))
                    {
                        player.Abilities.Add(CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[(int)ability2]][team].associatedGameObject);
                        player.AbilityIcons.Add(CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[(int)ability2]][team].sprite);
                    }
                    else
                    {
                        player.Abilities.Add(abilityIcons.sprites[(int)ability2].associatedGameObject);
                        player.AbilityIcons.Add(abilityIcons.sprites[(int)ability2].sprite);
                    }
                }
                if (nrOfAbilities > 2)
                {
                    if (CustomAbilitySpritesWithBackrounds.ContainsKey(abilityIcons.sprites[(int)ability3]))
                    {
                        player.Abilities.Add(CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[(int)ability3]][team].associatedGameObject);
                        player.AbilityIcons.Add(CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[(int)ability3]][team].sprite);
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
            public static void Postfix(ref Player __result)
            {
                __result = playerToReturn;
            }
        }
        [HarmonyPatch(typeof(Arrow), "OnCollide")]
        public static class Arrow_OnCollide_Patch
        {
            public static bool Prefix(Arrow __instance)
            {
                if (__instance.gameObject.name == "bullet-api")
                {
                    Updater.DestroyFix(__instance.gameObject);
                    return false;
                }
                return true;
            }
        }
        
        [HarmonyPatch(typeof(CharacterSelectHandler), "TryStartGame_inner")]
        public static class CharacterSelectHandlerPatch
        {
            public static bool Prefix(CharacterSelectHandler __instance, ref bool ___allReadyForMoreThanOneFrame, ref PlayerColors ___playerColors)
            {
                if (CharacterSelectHandler.startButtonAvailable && ___allReadyForMoreThanOneFrame)
                {
                    AudioManager audioManager = AudioManager.Get();
                    if (audioManager != null)
                    {
                        audioManager.Play("startGame");
                    }

                    // Clear the player list
                    CharacterSelectHandler.startButtonAvailable = false;

                    // Create the player list
                    List<Player> list = PlayerHandler.Get().PlayerList();
                    list.Clear();

                    int num = 1;
                    NamedSpriteList abilityIcons = SteamManager.instance.abilityIcons;
                    for (int i = 0; i < __instance.characterSelectBoxes.Length; i++)
                    {
                        if (__instance.characterSelectBoxes[i].menuState == CharSelectMenu.ready)
                        {
                            PlayerInit playerInit = __instance.characterSelectBoxes[i].playerInit;
                            Player player = new(num, playerInit.team)
                            {
                                Color = ___playerColors[playerInit.color].playerMaterial,
                                UsesKeyboardAndMouse = playerInit.usesKeyboardMouse,
                                CanUseAbilities = true,
                                inputDevice = playerInit.inputDevice,
                                Abilities = new List<GameObject>(3),
                                AbilityIcons = new List<Sprite>(3)
                            };

                            player.Abilities.Add(abilityIcons.sprites[playerInit.ability0].associatedGameObject);

                            // If its a custom ability then use the one that has the backround
                            if (Sprites.Contains(abilityIcons.sprites[playerInit.ability0]))
                            {
                                var AbilityWithBackroundsList = CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[playerInit.ability0]];
                                player.AbilityIcons.Add(AbilityWithBackroundsList[player.Team].sprite);
                            }
                            else
                            {
                                // If its not a custom ability do it normaly
                                player.AbilityIcons.Add(abilityIcons.sprites[playerInit.ability0].sprite);
                            }

                            Settings settings = Settings.Get();
                            if (settings != null && settings.NumberOfAbilities > 1)
                            {
                                player.Abilities.Add(abilityIcons.sprites[playerInit.ability1].associatedGameObject);
                                // If its a custom ability then use the one that has the backround
                                if (Sprites.Contains(abilityIcons.sprites[playerInit.ability1]))
                                {
                                    var AbilityWithBackroundsList = CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[playerInit.ability1]];
                                    player.AbilityIcons.Add(AbilityWithBackroundsList[player.Team].sprite);
                                }
                                else
                                {
                                    // If its not a custom ability do it normaly
                                    player.AbilityIcons.Add(abilityIcons.sprites[playerInit.ability1].sprite);
                                }
                            }
                            Settings settings2 = Settings.Get();
                            if (settings2 != null && settings2.NumberOfAbilities > 2)
                            {
                                player.Abilities.Add(abilityIcons.sprites[playerInit.ability2].associatedGameObject);
                                // If its a custom ability then use the one that has the backround
                                if (Sprites.Contains(abilityIcons.sprites[playerInit.ability2]))
                                {
                                    var AbilityWithBackroundsList = CustomAbilitySpritesWithBackrounds[abilityIcons.sprites[playerInit.ability2]];
                                    player.AbilityIcons.Add(AbilityWithBackroundsList[player.Team].sprite);
                                }
                                else
                                {
                                    // If its not a custom ability do it normaly
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
    //public class GunAbilityTest : Gun
    //{
        //public override string abilityName => "Gun";

        //public override string namespaceName => "Ability_Api";

        //public override string playerInAbilitySpriteName => "gun.png";

        //public override string abilityIconSpriteName => "AbilityIcon.png";

        //public override string bulletSpriteName => "bullet.png";

        //public override Fix cooldown => (Fix)10;

        //public override float bulletSpeed => 55;

        //public override float bulletGravity => 3;

        //public override string shootSoundEffect => "explosion";

        //public override float scale => 1.5f;
    //}
}