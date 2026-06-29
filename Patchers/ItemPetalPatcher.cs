using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace Freedom_Planet_2_Ring_System.Patchers
{
    internal class ItemPetalPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemPetal), "State_Released")]
        static void ReplaceScatteredWithRings(ItemPetal __instance)
        {
            // Only do this if we have the Power Ring item equipped.
            if (FPPlayerPatcher.player != null)
                if (!FPPlayerPatcher.player.powerups.Contains((FPPowerup)Plugin.ringItemID))
                    return;

            // Get this Petal's animator, aborting if we somehow can't find it.
            Animator animator = __instance.GetComponent<Animator>();
            if (animator == null)
                return;

            // If we're not already using the Ring animator, then replace it and the sounds with the Ring ones.
            if (animator.runtimeAnimatorController.name != "Ring Animator")
            {
                animator.runtimeAnimatorController = Plugin.ringsAssetBundle.LoadAsset<RuntimeAnimatorController>("Ring Animator");
                __instance.sfxPetalGet1 = Plugin.ringsAssetBundle.LoadAsset<AudioClip>("ring");
                __instance.sfxPetalGet2 = Plugin.ringsAssetBundle.LoadAsset<AudioClip>("ring");
                __instance.activationMode = FPActivationMode.ALWAYS_ACTIVE;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemPetal), "CollisionCheck")]
        static void ReplaceWithRings(ItemPetal __instance)
        {
            // Only do this if we have the Power Ring item equipped.
            if (FPPlayerPatcher.player != null)
                if (!FPPlayerPatcher.player.powerups.Contains((FPPowerup)Plugin.ringItemID))
                    return;

            // Replicate the code from the original game used by the No Petals Brave Stone.
            bool stageListPosValidated = false;
            ItemCrystal itemCrystal = FPStage.InstantiateFPBaseObject(__instance.pfCrystal, out stageListPosValidated);
            itemCrystal.transform.position = __instance.transform.position;
            FPStage.ForEachBreak();
            FPStage.DestroyStageObject(__instance);
        }
    }
}
