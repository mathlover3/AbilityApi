using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using System.Drawing;

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
        public static void RegisterNamedSprites(NamedSprite Sprite)
        {
            foreach (NamedSprite sprite in Sprites)
            {
                if (sprite.name == Sprite.name)
                {
                    throw new Exception($"ERROR: ABILITY WITH NAME {Sprite.name} ALREADY EXSITS! NOT CREATING ABILITY!");
                }
            }
            Sprite.associatedGameObject.name = Sprite.name;
            CustomAbilityTexstures.Add(Sprite.sprite.texture);
            Sprites.Add(Sprite);

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
            //ok now to finaly merge the 2 images
            for (int x = 0; x < EndSize.x; x++)
            {
                for (int y = 0; y < EndSize.y; y++)
                {
                    //if the ability is transparent here replace it with the backround. outerwise replace it with the ability
                    if (AbilityColorDataOffset[x, y].a == 255)
                    {
                        FinalImage[x, y] = AbilityColorDataOffset[x, y];
                    }
                    else
                    {
                        FinalImage[x, y] = BackroundColorDataOffset[x, y];
                    }
                }
            }
            return TwoArrayToTexture2D(FinalImage, EndSize.x, EndSize.y);
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
