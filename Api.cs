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

        public static Dictionary<Texture2D, AbilityTextureMetaData> CustomAbilityTexstures = new();
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
        /// Sprite is the sprite alits with your sprite and the backround
        /// if theres already a ability with the given name it errors out.
        /// if the assosated gameobjects name isnt the same as the ability name it will be renamed. dont use the same assosated gameobject for multiple abilitys.
        /// Json is the json thats generated with texsture packer. this is needed so we know where your ability sprite is and where the backround is.
        /// BackroundName is the name of the file that the backround was in when packing it. normaly its BaseCircle.png unless you renamed it.
        /// </summary>
        /// <returns></returns>
        public static void RegisterNamedSprites(NamedSprite Sprite, string json, string BackroundName)
        {
            foreach (NamedSprite sprite in Sprites)
            {
                if (sprite.name == Sprite.name)
                {
                    throw new Exception($"ERROR: ABILITY WITH NAME {Sprite.name} ALREADY EXSITS! NOT CREATING ABILITY!");
                }
            }
            var dict = MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
            if (dict == null)
            {
                throw new InvalidDataException("ERROR: INVALID JSON");
            }
            Vector2 TotalSize = new();
            try
            {
                TotalSize.x = (float)Convert.ToDouble(((Dictionary<string, object>)((Dictionary<string, object>)dict["meta"])["size"])["w"]);
                TotalSize.y = (float)Convert.ToDouble(((Dictionary<string, object>)((Dictionary<string, object>)dict["meta"])["size"])["h"]);
                dict = dict["frames"] as Dictionary<string, object>;
            }
            catch (Exception e)
            {
                throw new InvalidDataException($"ok you know what im not gonna make a difrent error for ever difrent way a json can be invalid. this json is invalid err {e}");
            }
            if (!dict.ContainsKey(BackroundName))
            {
                throw new InvalidDataException($"ERROR: JSON DOESNT CONTANE BACKROUND {BackroundName}");
            }
            Dictionary<string, object> BackroundNameData;
            try
            {
                //idk a better way of checking its the correct type.
                BackroundNameData = dict[BackroundName] as Dictionary<string, object>;
            }
            catch (Exception e)
            {
                throw new InvalidDataException("ERROR: BACKROUND DOESNT CONTANE A DICTIONARY"); 
            }
            if (!BackroundNameData.ContainsKey("frame"))
            {
                throw new InvalidDataException($"ERROR: BACKROUND DOESNT CONTANE \"frame\"");
            }
            try
            {
                Dictionary<string, object> BackroundFrame = BackroundNameData["frame"] as Dictionary<string, object>;
                AbilityTextureMetaData metaData = new();
                metaData.BackroundTopLeftCourner.x = (float)Convert.ToDouble(BackroundFrame["x"]) ;
                metaData.BackroundTopLeftCourner.y = (float)Convert.ToDouble(BackroundFrame["y"]);
                metaData.BackroundSize.x = (float)Convert.ToDouble(BackroundFrame["w"]);
                metaData.BackroundSize.y = (float)Convert.ToDouble(BackroundFrame["h"]);
                //remove the backround so the only one left is the icon data
                dict.Remove(BackroundName);
                var DictList = dict.ToList();
                var IconKeyValuePair = DictList[0];
                var IconData = IconKeyValuePair.Value as Dictionary<string, object>;
                var IconFrame = IconData["frame"] as Dictionary<string, object>;
                metaData.IconTopLeftCourner.x = (float)Convert.ToDouble(IconFrame["x"]);
                metaData.IconTopLeftCourner.y = (float)Convert.ToDouble(IconFrame["y"]);
                metaData.IconSize.x = (float)Convert.ToDouble(IconFrame["w"]);
                metaData.IconSize.y = (float)Convert.ToDouble(IconFrame["h"]);
                metaData.TotalSize = TotalSize;
                Sprite.associatedGameObject.name = Sprite.name;
                CustomAbilityTexstures.Add(Sprite.sprite.texture, metaData);
                Sprites.Add(Sprite);
            }
            catch (Exception e)
            {
                throw new InvalidDataException($"ok you know what im not gonna make a difrent error for ever difrent way a json can be invalid. this json is invalid err {e}");
            }

        }
    }
}
