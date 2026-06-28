using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace Freedom_Planet_2_Ring_System.Patchers
{
    internal class FPHudMasterPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPHudMaster), "Start")]
        private static void ReplaceHUDIcon(FPHudMaster __instance, ref GameObject ___hudCrystalIcon)
        {
            if (__instance.targetPlayer.powerups.Contains((FPPowerup)Plugin.ringItemID))
                ___hudCrystalIcon.GetComponent<SpriteRenderer>().sprite = Plugin.ringsAssetBundle.LoadAsset<Sprite>("hudRing");
        }
    }
}
