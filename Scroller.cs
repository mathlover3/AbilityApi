using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Rendering;

namespace AbilityApi
{
    internal class Scroller
    {
        [HarmonyPatch(typeof(AbilityGrid), nameof(AbilityGrid.Awake))]
        public static class AbilityGridPatch
        {
            public static void Postfix(AbilityGrid __instance)
            {
                // ERWER's Code  
                // Set variables..

                GameObject mask_viewport = new GameObject("viewport_mask");
                mask_viewport.AddComponent<RectTransform>();

                Transform transform = __instance.gameObject.transform;

                mask_viewport.transform.SetParent(transform, false);

                Transform bgTransform = transform.Find("border");
                transform.Find("selectionCircle").gameObject.SetActive(false); // removes the circle lol.
                /* fix this later, you'd need to offset the selection cirlce by using the scroll amount. (which is normalized) */
                
               
                if (transform.Find("scroller_content") == null)
                {

                    RectTransform mask = bgTransform.GetComponent<RectTransform>();
                    RectTransform scrollRect = /*__instance.gameObject*/mask_viewport.GetComponent<RectTransform>();

                    // Set the ScrollRect's RectTransform properties
                    scrollRect.sizeDelta = mask.sizeDelta;
                    scrollRect.anchorMin = mask.anchorMin;
                    scrollRect.anchorMax = mask.anchorMax;
                    scrollRect.pivot = mask.pivot;

                    // Create content GameObject
                    GameObject content = new GameObject("scroller_content");
                    RectMask2D masker = mask_viewport.gameObject.AddComponent<RectMask2D>();

                    masker.rectTransform.sizeDelta = new Vector2(mask.sizeDelta.x * 2, mask.sizeDelta.y-110f);
                    masker.rectTransform.anchorMin = mask.anchorMin;
                    masker.rectTransform.anchorMax = mask.anchorMax;
                    masker.rectTransform.pivot = mask.pivot;

                    Vector2 offset = new Vector2(0, -120); // slightly move down so it doesn't overlap the abiltiy name text.
                    masker.rectTransform.anchoredPosition += offset;


                    RectTransform contentRectTransform = content.AddComponent<RectTransform>();
                    ScrollRect scroll = content.AddComponent<ScrollRect>();

                    content.transform.SetParent(mask_viewport.transform, false);

                    // Find grid entries and set their parent to content
                    var gridEntries = transform.GetComponentsInChildren<Transform>()
                        .Where(child => child.gameObject.name == "AbilityGridEntry(Clone)")
                        .Select(child => child.gameObject);

                    foreach (GameObject gridEntry in gridEntries)
                    {
                        gridEntry.transform.SetParent(content.transform, false);
                    }

                    content.SetActive(true);

                    // Set the ScrollRect's content to the new content RectTransform
                    scroll.content = contentRectTransform; // Reference the RectTransform of the content
                    scroll.viewport = transform.GetComponent<RectTransform>();
                    scroll.scrollSensitivity = -5;
                    scroll.horizontal = false;
                    scroll.movementType = ScrollRect.MovementType.Elastic;
                    scroll.elasticity = 0.5f;
                }

            }
        }
    }
}
