using Freedom_Planet_2_Ring_System.CustomObjectScripts;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace Freedom_Planet_2_Ring_System.Patchers
{
    internal class ItemBoxPatcher
    {
        private static readonly GameObject superRingPrefab = Plugin.ringsAssetBundle.LoadAsset<GameObject>("super ring");

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemBox), "State_Idle")]
        static void ReplaceWithSuperRing(ItemBox __instance)
        {
            // Only do this if we have the Power Ring item equipped.
            if (FPPlayerPatcher.player != null)
                if (!FPPlayerPatcher.player.powerups.Contains((FPPowerup)Plugin.ringItemID))
                    return;

            // Check that this item box is a Petal or Crystal one.
            if (__instance.itemType is FPItemBoxTypes.BOX_PETALS or FPItemBoxTypes.BOX_CRYSTALS or FPItemBoxTypes.BOX_CRYSTALRING or FPItemBoxTypes.BOX_REDCRYSTALS or FPItemBoxTypes.BOX_REDCRYSTALRING)
            {
                // Create a Super Ring in this item box's position.
                GameObject superRing = GameObject.Instantiate(superRingPrefab, __instance.transform.position, __instance.transform.rotation);
                superRing.AddComponent<SuperRing>();

                // Destroy this item box.
                FPStage.DestroyStageObject(__instance);
            }
        }
    }
}
