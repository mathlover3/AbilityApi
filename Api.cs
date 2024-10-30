using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using System.Drawing;
using UnityEngine.SocialPlatforms;
using AbilityApi.Internal;

namespace AbilityApi
{
    public class Api
    {
        public class AbilityTextureMetaData
        {
            public Vector2 BackroundTopLeftCourner { get; set; }
            public Vector2 BackroundSize { get; set; }
            public Vector2 IconTopLeftCourner { get; set; }
            public Vector2 IconSize { get; set; }
            public Vector2 TotalSize { get; set; }
        }

        public static List<Texture2D> CustomAbilityTexstures { get; } = new();
        public static Dictionary<NamedSprite, List<NamedSprite>> CustomAbilitySpritesWithBackrounds { get; } = new();
        public static List<NamedSprite> Sprites { get; } = new();
        public static AbilityGrid AbilityGrid { get; set; }

        public static T ConstructInstantAbility<T>(string name) where T : MonoUpdatable
        {
            GameObject parent = new GameObject(name);
            GameObject.DontDestroyOnLoad(parent);

            InstantAbility ability = parent.AddComponent<InstantAbility>();

            parent.AddComponent<FixTransform>();
            MonoUpdatable updatable = parent.AddComponent<T>();

            if (updatable == null)
            {
                GameObject.Destroy(parent);
                throw new MissingReferenceException("Invalid type was fed to ConstructInstantAbility");
            }

            return (T)updatable;
        }

        public static Texture2D LoadImage(string path)
        {
            byte[] data = File.ReadAllBytes(path);

            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(data);

            return tex;
        }

        public static void RegisterNamedSprites(NamedSprite namedSprite, bool isOffensiveAbility)
        {
            if (Sprites.Any(sprite => sprite.name == namedSprite.name))
            {
                throw new Exception($"ERROR: ABILITY WITH NAME {namedSprite.name} ALREADY EXSITS! NOT CREATING ABILITY!");
            }

            namedSprite.associatedGameObject.name = namedSprite.name;
            CustomAbilityTexstures.Add(namedSprite.sprite.texture);

            List<NamedSprite> abilitysWithBackrounds = new List<NamedSprite>();
            foreach (var backround in Plugin.BackroundSprites)
            {
                var textureWithBackround = OverlayBackround(namedSprite.sprite.texture, backround);
                var spriteWithBackround = Sprite.Create(textureWithBackround, new Rect(0f, 0f, textureWithBackround.width, textureWithBackround.height), new Vector2(0.5f, 0.5f));
                var namedSpriteWithBackround = new NamedSprite(namedSprite.name, spriteWithBackround, namedSprite.associatedGameObject, isOffensiveAbility);
                abilitysWithBackrounds.Add(namedSpriteWithBackround);
            }

            CustomAbilitySpritesWithBackrounds.Add(namedSprite, abilitysWithBackrounds);
            Sprites.Add(namedSprite);
        }

        public static Texture2D OverlayBackround(Texture2D ability, Texture2D backround)
        {
            var abilityColorData2D = Texture2DTo2DArray(ability);
            var backroundColorData2D = Texture2DTo2DArray(backround);

            var endSize = new Vector2Int(Math.Max(ability.width, backround.width), Math.Max(ability.height, backround.height));
            var center = new Vector2Int(endSize.x / 2, endSize.y / 2);
            var abilityBottomLeftCorner = new Vector2Int(center.x - ability.width / 2, center.y - ability.height / 2);
            var backroundBottomLeftCorner = new Vector2Int(center.x - backround.width / 2, center.y - backround.height / 2);

            var finalImage = new Color32[endSize.x, endSize.y];
            var abilityColorDataOffset = new Color32[endSize.x, endSize.y];
            var backroundColorDataOffset = new Color32[endSize.x, endSize.y];

            for (int x = 0; x < ability.width; x++)
            {
                for (int y = 0; y < ability.height; y++)
                {
                    abilityColorDataOffset[x + abilityBottomLeftCorner.x, y + abilityBottomLeftCorner.y] = abilityColorData2D[x, y];
                }
            }

            for (int x = 0; x < backround.width; x++)
            {
                for (int y = 0; y < backround.height; y++)
                {
                    backroundColorDataOffset[x + backroundBottomLeftCorner.x, y + backroundBottomLeftCorner.y] = backroundColorData2D[x, y];
                }
            }

            for (int x = 0; x < endSize.x; x++)
            {
                for (int y = 0; y < endSize.y; y++)
                {
                    var abilityColor = abilityColorDataOffset[x, y];
                    var backgroundColor = backroundColorDataOffset[x, y];

                    if (abilityColor.a >= 160)
                    {
                        finalImage[x, y] = abilityColor;
                    }
                    else if (backgroundColor.a > 50)
                    {
                        finalImage[x, y] = MixColor32(backgroundColor, abilityColor);
                    }
                    else
                    {
                        finalImage[x, y] = backgroundColor;
                    }
                }
            }

            return TwoArrayToTexture2D(finalImage, endSize.x, endSize.y);
        }

        private static Color32 MixColor32(Color32 background, Color32 overlay)
        {
            float backgroundAlpha = background.a / 255f;
            float overlayAlpha = overlay.a / 255f;

            float combinedAlpha = backgroundAlpha + overlayAlpha * (1 - backgroundAlpha);
            byte a = (byte)Mathf.Clamp(combinedAlpha * 255, 0, 255);

            byte r = (byte)((overlay.r * overlayAlpha + background.r * backgroundAlpha * (1 - overlayAlpha)) / combinedAlpha);
            byte g = (byte)((overlay.g * overlayAlpha + background.g * backgroundAlpha * (1 - overlayAlpha)) / combinedAlpha);
            byte b = (byte)((overlay.b * overlayAlpha + background.b * backgroundAlpha * (1 - overlayAlpha)) / combinedAlpha);

            return new Color32(r, g, b, a);
        }

        private static Color32[,] Texture2DTo2DArray(Texture2D texture)
        {
            var ColorData1D = texture.GetPixels32();
            Color32[,] ColorData2D = new Color32[texture.width, texture.height];
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    ColorData2D[x, y] = ColorData1D[x + y * texture.width];
                }
            }
            return ColorData2D;
        }
        private static Texture2D TwoArrayToTexture2D(Color32[,] ColorData2D, int width, int height)
        {
            var ColorData1D = new Color32[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    ColorData1D[x + y * width] = ColorData2D[x, y];
                }
            }
            Texture2D copyTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            copyTexture.SetPixels32(ColorData1D);
            copyTexture.Apply();
            return copyTexture;
        }
    }
}

