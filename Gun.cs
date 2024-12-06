using AbilityApi;
using AbilityApi.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Ability_Api
{
    public abstract class Gun : MonoBehaviour
    {
        public abstract string abilityName { get; }
        public abstract string namespaceName { get; }
        public abstract string playerInAbilitySpriteName { get; }
        public abstract string abilityIconSpriteName { get; }
        public abstract string bulletSpriteName { get; }
        public abstract BoplFixedMath.Fix cooldown { get; }
        public abstract float bulletSpeed { get; }
        public abstract float bulletGravity { get; }
        public abstract string shootSoundEffect { get; }
        public abstract float scale { get; }
        public void SetUpGun()
        {
            var abilityPrefab = Api.ConstructGun<GunAbility>(abilityName, namespaceName, playerInAbilitySpriteName, bulletSpriteName, cooldown, bulletSpeed, bulletGravity, shootSoundEffect, scale);
            abilityPrefab.gameObject.AddComponent<PlayerPhysics>();
            Texture2D abilityTex = Api.LoadImageFromResources(namespaceName, abilityIconSpriteName);
            var iconSprite = Sprite.Create(abilityTex, new Rect(0f, 0f, abilityTex.width, abilityTex.height), new Vector2(0.5f, 0.5f));
            NamedSprite ability = new NamedSprite(abilityName, iconSprite, abilityPrefab.gameObject, true);
            Api.RegisterNamedSprites(ability, true);
        }
    }
}
