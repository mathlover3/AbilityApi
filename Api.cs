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
            public Vector2 BackroundTopLeftCourner = new();
            public Vector2 BackroundSize = new();
            public Vector2 IconTopLeftCourner = new();
            public Vector2 IconSize = new();
            public Vector2 TotalSize = new();
        }

        public static List<Texture2D> CustomAbilityTexstures = new();
        public static Dictionary<NamedSprite, List<NamedSprite>> CustomAbilitySpritesWithBackrounds = new();
        public static List<NamedSprite> Sprites = new();
        public static AbilityGrid abilityGrid;
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
        /// <summary>
        /// Adds your NamedSprites to the games many NamedSpriteLists that need them.
        /// Sprite is the NamedSprite for your ability
        /// if theres already a ability with the given name it errors out.
        /// if the assosated gameobjects name isnt the same as the ability name it will be renamed. dont use the same assosated gameobject for multiple abilitys.
        /// </summary>
        /// <returns></returns>
        public static void RegisterNamedSprites(NamedSprite namedSprite, bool IsOffensiveAbility)
        {
            foreach (NamedSprite sprite in Sprites)
            {
                if (sprite.name == namedSprite.name)
                {
                    throw new Exception($"ERROR: ABILITY WITH NAME {namedSprite.name} ALREADY EXSITS! NOT CREATING ABILITY!");
                }
            }
            namedSprite.associatedGameObject.name = namedSprite.name;
            CustomAbilityTexstures.Add(namedSprite.sprite.texture);
            List<NamedSprite> AbilitysWithBackrounds = new List<NamedSprite>();
            foreach (var backround in Plugin.BackroundSprites)
            {
                var TextureWithBackround = Api.OverlayBackround(namedSprite.sprite.texture, backround);
                var SpriteWithBackround = Sprite.Create(TextureWithBackround, new Rect(0f, 0f, TextureWithBackround.width, TextureWithBackround.height), new Vector2(0.5f, 0.5f));
                var NamedSpriteWithBackround = new NamedSprite(namedSprite.name, SpriteWithBackround, namedSprite.associatedGameObject, IsOffensiveAbility);
                AbilitysWithBackrounds.Add(NamedSpriteWithBackround);
            }
            CustomAbilitySpritesWithBackrounds.Add(namedSprite, AbilitysWithBackrounds);
            Sprites.Add(namedSprite);

        }
        public static Texture2D OverlayBackround(Texture2D ability,  Texture2D backround)
        {
            Color32[,] AbilityColorData2D = Texture2DTo2DArray(ability);
            Color32[,] BackroundColorData2D = Texture2DTo2DArray(backround);
            Vector2Int EndSize = new Vector2Int();
            //calculate EndSize
            if (ability.width > backround.width)
            {
                EndSize.x = ability.width;
            }
            else
            {
                EndSize.x = backround.width;
            }
            if (ability.height > backround.height)
            {
                EndSize.y = ability.height;
            }
            else
            {
                EndSize.y = backround.height;
            }
            Vector2Int Center = new Vector2Int(EndSize.x/2, EndSize.y/2);
            Vector2Int AbilityBottomLeftCorner = new Vector2Int(Center.x - (ability.width / 2), Center.y - (ability.height / 2));
            Vector2Int BackroundBottomLeftCorner = new Vector2Int(Center.x - (backround.width / 2), Center.y - (backround.height / 2));
            //the colors should be fully transparent at the start
            Color32[,] FinalImage = new Color32[EndSize.x, EndSize.y];
            Color32[,] AbilityColorDataOffset = new Color32[EndSize.x, EndSize.y];
            Color32[,] BackroundColorDataOffset = new Color32[EndSize.x, EndSize.y];
            //offset the AbilityColorData2D into the AbilityColorDataOffset array. 
            for (int x = 0; x < ability.width; x++)
            {
                for (int y = 0; y < ability.height; y++)
                {
                    AbilityColorDataOffset[x + AbilityBottomLeftCorner.x, y + AbilityBottomLeftCorner.y] = AbilityColorData2D[x, y];
                }
            }
            //offset the BackroundColorData2D into the BackroundColorDataOffset array. 
            for (int x = 0; x < backround.width; x++)
            {
                for (int y = 0; y < backround.height; y++)
                {
                    BackroundColorDataOffset[x + BackroundBottomLeftCorner.x, y + BackroundBottomLeftCorner.y] = BackroundColorData2D[x, y];
                }
            }
            for (int x = 0; x < EndSize.x; x++)
            {
                for (int y = 0; y < EndSize.y; y++)
                {
                    Color32 abilityColor = AbilityColorDataOffset[x, y];
                    Color32 backgroundColor = BackroundColorDataOffset[x, y];


                    if (abilityColor.a >= 160)
                    {
                        FinalImage[x, y] = abilityColor;
                    }

                    else if (backgroundColor.a > 50)
                    {
                        FinalImage[x, y] = MixColor32(backgroundColor, abilityColor);
                    }

                    else
                    {
                        FinalImage[x, y] = backgroundColor;
                    }
                }
            }

            return TwoArrayToTexture2D(FinalImage, EndSize.x, EndSize.y);
        }

        private static Color32 MixColor32(Color32 background, Color32 overlay)
        {
            float backgroundAlpha = background.a / 255f; // uhhh convert to like.. 0->1 i guess.
            float overlayAlpha = overlay.a / 255f; //uh the same thing^

            // Calculate the combined alpha by adding the background alpha and the overlay alpha,
            // scale the sum of both alpha values by the opacity of the foreground ( my brain hurts )
            float combinedAlpha = backgroundAlpha + overlayAlpha * (1 - backgroundAlpha);
            byte a = (byte)Mathf.Clamp(combinedAlpha * 255, 0, 255); // clamp lol

            // Blend the color channels using the alpha values
            // OKAY HERE WE GO WITH COMMENTS
            // Am I really going to copy paste the comment three times?
            // yes.
            // yes I am. deal with it :bblox~3:

            // Multiply the Red channel's by their alpha channels, then multiply the sum by the opcaity of the background, and divide it by the combinedAlpha values.
            byte r = (byte)((overlay.r * overlayAlpha + background.r * backgroundAlpha * (1 - overlayAlpha)) / combinedAlpha);
            // Multiply the Green channel's by their alpha channels, then multiply the sum by the opcaity of the background, and divide it by the combinedAlpha values.
            byte g = (byte)((overlay.g * overlayAlpha + background.g * backgroundAlpha * (1 - overlayAlpha)) / combinedAlpha);
            // Multiply the Blue channel's by their alpha channels, then multiply the sum by the opcaity of the background, and divide it by the combinedAlpha values.
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
