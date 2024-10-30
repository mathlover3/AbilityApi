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
        // Stores metadata related to ability textures, such as background and icon dimensions.
        public class AbilityTextureMetaData
        {
            public Vector2 BackroundTopLeftCourner { get; set; }
            public Vector2 BackroundSize { get; set; }
            public Vector2 IconTopLeftCourner { get; set; }
            public Vector2 IconSize { get; set; }
            public Vector2 TotalSize { get; set; }
        }
      
        // Holds custom textures for abilities.
        public static List<Texture2D> CustomAbilityTexstures = new();
        public static Dictionary<NamedSprite, List<NamedSprite>> CustomAbilitySpritesWithBackrounds = new();
        public static NamedSpriteList CustomAbilitySpritesWithBackroundList = new();
        public static List<NamedSprite> Sprites = new();
        public static AbilityGrid abilityGrid;

        public static T ConstructInstantAbility<T>(string name) where T : MonoUpdatable
        {
            GameObject parent = new GameObject(name);
            GameObject.DontDestroyOnLoad(parent);

            // Attach an InstantAbility component to the new GameObject.
            InstantAbility ability = parent.AddComponent<InstantAbility>();
            parent.AddComponent<FixTransform>(); // Add transform fix component.
            MonoUpdatable updatable = parent.AddComponent<T>(); // Attach the specified ability type.

            if (updatable == null)
            {
                GameObject.Destroy(parent); // Destroy GameObject if type is invalid.
                throw new MissingReferenceException("Invalid type was fed to ConstructInstantAbility");
            }

            return (T)updatable;
        }

        // Loads an image from the provided path and returns it as a Texture2D.
        public static Texture2D LoadImage(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(data);
            return tex;
        }

        /// <summary>
        /// Adds your NamedSprites to the games many NamedSpriteLists that need them.
        /// Sprite is the NamedSprite for your ability
        /// if theres already a ability with the given name it errors out.
        /// if the assosated gameobjects name isnt the same as the ability name it will be renamed. dont use the same assosated gameobject for multiple abilitys.
        /// </summary>
        /// <returns></returns>
        public static void RegisterNamedSprites(NamedSprite namedSprite, bool isOffensiveAbility)
        {
            if (Sprites.Any(sprite => sprite.name == namedSprite.name))

            if (CustomAbilitySpritesWithBackroundList.sprites == null)
            {
                CustomAbilitySpritesWithBackroundList.sprites = new();
            }
            foreach (NamedSprite sprite in Sprites)
            {
                throw new Exception($"Error: Ability with the name {namedSprite.name} already exists, not creating ability!");
            }

            namedSprite.associatedGameObject.name = namedSprite.name;
            CustomAbilityTexstures.Add(namedSprite.sprite.texture); // Add ability texture to custom textures list.

            // Create overlayed sprites with each background.
            List<NamedSprite> abilitysWithBackrounds = new List<NamedSprite>();
            foreach (var backround in Plugin.BackroundSprites)
            {
                var TextureWithBackround = Api.OverlayBackround(namedSprite.sprite.texture, backround);
                var SpriteWithBackround = Sprite.Create(TextureWithBackround, new Rect(0f, 0f, TextureWithBackround.width, TextureWithBackround.height), new Vector2(0.5f, 0.5f));
                var NamedSpriteWithBackround = new NamedSprite(namedSprite.name, SpriteWithBackround, namedSprite.associatedGameObject, IsOffensiveAbility);
                AbilitysWithBackrounds.Add(NamedSpriteWithBackround);
                CustomAbilitySpritesWithBackroundList.sprites.Add(NamedSpriteWithBackround);
            }

            // Add the main sprite and its background variations to relevant lists.
            CustomAbilitySpritesWithBackrounds.Add(namedSprite, abilitysWithBackrounds);
            Sprites.Add(namedSprite);
        }

        // Overlays the ability texture onto the background texture, returning a combined Texture2D.
        public static Texture2D OverlayBackround(Texture2D ability, Texture2D backround)
        {
            var abilityColorData2D = Texture2DTo2DArray(ability);
            var backroundColorData2D = Texture2DTo2DArray(backround);

            // Determine final image dimensions and calculate overlay positions.
            var endSize = new Vector2Int(Math.Max(ability.width, backround.width), Math.Max(ability.height, backround.height));
            var center = new Vector2Int(endSize.x / 2, endSize.y / 2);
            var abilityBottomLeftCorner = new Vector2Int(center.x - ability.width / 2, center.y - ability.height / 2);
            var backroundBottomLeftCorner = new Vector2Int(center.x - backround.width / 2, center.y - backround.height / 2);

            // Initialize the final image, and the colors should be fully transparent at the start
            var finalImage = new Color32[endSize.x, endSize.y];
            var abilityColorDataOffset = new Color32[endSize.x, endSize.y];
            var backroundColorDataOffset = new Color32[endSize.x, endSize.y];

            // Overlay the ability texture at its calculated position.
            for (int x = 0; x < ability.width; x++)
            {
                for (int y = 0; y < ability.height; y++)
                {
                    abilityColorDataOffset[x + abilityBottomLeftCorner.x, y + abilityBottomLeftCorner.y] = abilityColorData2D[x, y];
                }
            }

            // Overlay the background texture at its calculated position.
            for (int x = 0; x < backround.width; x++)
            {
                for (int y = 0; y < backround.height; y++)
                {
                    backroundColorDataOffset[x + backroundBottomLeftCorner.x, y + backroundBottomLeftCorner.y] = backroundColorData2D[x, y];
                }
            }

            // Combine colors, blending the ability over the background.
            for (int x = 0; x < endSize.x; x++)
            {
                for (int y = 0; y < endSize.y; y++)
                {
                    var abilityColor = abilityColorDataOffset[x, y];
                    var backgroundColor = backroundColorDataOffset[x, y];

                    if (abilityColor.a >= 160) // Full opacity for ability pixels.
                    {
                        finalImage[x, y] = abilityColor;
                    }
                    else if (backgroundColor.a > 50) // Partial opacity blending.
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

        // Blends two colors based on their alpha values.
        private static Color32 MixColor32(Color32 background, Color32 overlay)
        {
            // Calculate the combined alpha by adding the background alpha and the overlay alpha,
            // scale the sum of both alpha values by the opacity of the foreground ( my brain hurts )
            float backgroundAlpha = background.a / 255f;
            float overlayAlpha = overlay.a / 255f;
            float combinedAlpha = backgroundAlpha + overlayAlpha * (1 - backgroundAlpha);
            byte a = (byte)Mathf.Clamp(combinedAlpha * 255, 0, 255);


            // Multiply the Red channel's by their alpha channels, then multiply the sum by the opcaity of the background, and divide it by the combinedAlpha values.
            byte r = (byte)((overlay.r * overlayAlpha + background.r * backgroundAlpha * (1 - overlayAlpha)) / combinedAlpha);
            
            // Multiply the Green channel's by their alpha channels, then multiply the sum by the opcaity of the background, and divide it by the combinedAlpha values.
            byte g = (byte)((overlay.g * overlayAlpha + background.g * backgroundAlpha * (1 - overlayAlpha)) / combinedAlpha);

            // Multiply the Blue channel's by their alpha channels, then multiply the sum by the opcaity of the background, and divide it by the combinedAlpha values.
            byte b = (byte)((overlay.b * overlayAlpha + background.b * backgroundAlpha * (1 - overlayAlpha)) / combinedAlpha);

            return new Color32(r, g, b, a);
        }

        // Converts Texture2D to a 2D array of Color32 for manipulation.
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

        // Converts a 2D array of Color32 back to a Texture2D.
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
