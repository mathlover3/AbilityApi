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
    internal class scroller
    {
        [HarmonyPatch(typeof(AbilityGrid), nameof(AbilityGrid.Awake))]
        public static class AbilityGridPatch
        {
            public static void Postfix(AbilityGrid __instance)
            {
                // ERWER's Code  
                // Set variables..

                Transform transform = __instance.gameObject.transform;
                Transform bgTransform = transform.Find("bg");

                
               
                if (transform.Find("scroller_content") == null)
                {

                    RectTransform mask = bgTransform.GetComponent<RectTransform>();
                    RectTransform scrollRect = __instance.gameObject.GetComponent<RectTransform>();

                    // Set the ScrollRect's RectTransform properties
                    scrollRect.sizeDelta = mask.sizeDelta;
                    scrollRect.anchorMin = mask.anchorMin;
                    scrollRect.anchorMax = mask.anchorMax;
                    scrollRect.pivot = mask.pivot;

                    // Create content GameObject
                    GameObject content = new GameObject("scroller_content");
                    RectTransform contentRectTransform = content.AddComponent<RectTransform>();
                    ScrollRect scroll = content.AddComponent<ScrollRect>();

                    content.transform.SetParent(transform, false);

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
                    scroll.elasticity = 1;
                }

            }
        }
    }
}
