using Freedom_Planet_2_Ring_System.CustomObjectScripts;
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
            // If the Power Ring item is equipped then replace the crystal icon on the HUD with a Ring.
            if (__instance.targetPlayer.powerups.Contains((FPPowerup)Plugin.ringItemID))
                ___hudCrystalIcon.GetComponent<SpriteRenderer>().sprite = Plugin.ringsAssetBundle.LoadAsset<Sprite>("hudRing");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPHudMaster), "Start")]
        private static void ReplaceHUD(FPHudMaster __instance,
                                       ref GameObject ___itemPanel,
                                       ref GameObject ___timerPanel,
                                       ref SpriteRenderer ___hudBaseSprite,
                                       ref SuperTextMesh ___hudGuide,
                                       ref FPHudDigit[] ___hudEnergy,
                                       ref FPHudDigit[] ___hudLifePetals,
                                       ref FPHudDigit[] ___hudShields,
                                       ref GameObject ___energyBarGraphic)
        {
            // Only do this if the config option is enabled and the player has the Power Ring item equipped.
            if (Plugin.configSonicHUD.Value && __instance.targetPlayer.powerups.Contains((FPPowerup)Plugin.ringItemID))
            {
                // Instantiate the HUD and add the script to it.
                GameObject sonicHUD = GameObject.Instantiate(Plugin.ringsAssetBundle.LoadAsset<GameObject>("Sonic HUD"));
                sonicHUD.AddComponent<SonicHUD>();

                // Move various elements of the HUD way off screen.
                ___itemPanel.transform.position = new(2424, 2424);
                ___timerPanel.transform.position = new(2424, 2424);
                ___hudBaseSprite.gameObject.transform.position = new(2424, 2424);
                ___hudGuide.gameObject.transform.position = new(2424, 2424);
                ___energyBarGraphic.transform.position = new(2424, 2424);

                // Hide the sprite renderers for the energy and health bars.
                foreach (var _ in ___hudEnergy) _.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                foreach (var _ in ___hudLifePetals) _.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                foreach (var _ in ___hudShields) _.gameObject.GetComponent<SpriteRenderer>().enabled = false;

                // Move Carol's bike indicator down to beside the life icon.
                __instance.hudBike[0].transform.position = new(80, -280, 0);
            }
        }
    }
}
