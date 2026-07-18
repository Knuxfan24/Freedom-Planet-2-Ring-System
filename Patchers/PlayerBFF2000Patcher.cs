// TODO: This is janky as fuck and some times just doesn't work.
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace Freedom_Planet_2_Ring_System.Patchers
{
    internal class PlayerBFF2000Patcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerBFF2000), "HealthUpdate")]
        static void DropRings(PlayerBFF2000 __instance, ref float ___flinchInvulnTime)
        {
            // Only do this if we have the Power Ring item equipped.
            if (FPPlayerPatcher.player != null)
            {
                if (!FPPlayerPatcher.player.powerups.Contains((FPPowerup)Plugin.ringItemID))
                    return;
            }
            else return;

            // Check if we've taken damage.
            if (__instance.weakPoint.health < __instance.weakPoint.initialHealth)
            {
                // Check that we've got any Rings.
                if (FPPlayerPatcher.player.totalCrystals > 0)
                {
                    // Play the Ring loss sound.
                    FPAudio.PlaySfx(Plugin.ringsAssetBundle.LoadAsset<AudioClip>("ringloss"));

                    // Calculate if we should drop 1 or 2 Rings.
                    int ringCount = Mathf.Min(FPPlayerPatcher.player.totalCrystals, 2);
                    int droppedRings = 0;

                    // Loop through and drop up to 2 Rings.
                    float ringAngle;
                    for (ringAngle = 0f; ringAngle < 720; ringAngle += 720 / ringCount)
                    {
                        // Create a crystal and set it to our custom dummy Dropped state.
                        ItemCrystal itemCrystal = (ItemCrystal)FPStage.CreateStageObject(ItemCrystal.classID, __instance.position.x + Mathf.Cos((__instance.transform.eulerAngles.z + ringAngle) * ((float)Math.PI / 180f)) * 6f, __instance.position.y + Mathf.Sin((__instance.transform.eulerAngles.z + ringAngle) * ((float)Math.PI / 180f)) * 6f);
                        itemCrystal.state = ItemCrystalPatcher.State_Dropped;

                        // Apply velocity to this crystal, the amount depending on if this is the first or last half.
                        if (droppedRings > ringCount / 2)
                        {
                            itemCrystal.velocity.x = Mathf.Cos((__instance.transform.eulerAngles.z + ringAngle) * ((float)Math.PI / 180f)) * 8;
                            itemCrystal.velocity.y = Mathf.Sin((__instance.transform.eulerAngles.z + ringAngle) * ((float)Math.PI / 180f)) * 8;
                        }
                        else
                        {
                            itemCrystal.velocity.x = Mathf.Cos((__instance.transform.eulerAngles.z + ringAngle) * ((float)Math.PI / 180f)) * 4;
                            itemCrystal.velocity.y = Mathf.Sin((__instance.transform.eulerAngles.z + ringAngle) * ((float)Math.PI / 180f)) * 4;
                        }

                        // Increment the dropped count.
                        droppedRings++;

                        // Add the new crystal to the terrainDelay and vanishTimer sets, removing them if they already exist in it.
                        if (ItemCrystalPatcher.terrainDelay.ContainsKey(itemCrystal)) ItemCrystalPatcher.terrainDelay.Remove(itemCrystal);
                        if (ItemCrystalPatcher.vanishTimer.ContainsKey(itemCrystal)) ItemCrystalPatcher.vanishTimer.Remove(itemCrystal);
                        ItemCrystalPatcher.terrainDelay.Add(itemCrystal, 0);
                        ItemCrystalPatcher.vanishTimer.Add(itemCrystal, 0);
                    }

                    // Remove our crystals.
                    FPPlayerPatcher.player.totalCrystals -= 2;

                    // Give the player some invincibility.
                    __instance.weakPoint.invincibility = ___flinchInvulnTime / 4;

                    // Make sure we don't have a negative crystal count.
                    if (FPPlayerPatcher.player.totalCrystals < 0)
                        FPPlayerPatcher.player.totalCrystals = 0;

                    // Reset the weakpoint health.
                    __instance.weakPoint.health = __instance.weakPoint.initialHealth;
                }
                else
                {
                    FPPlayerPatcher.player.health = -1f;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerBFF2000), "CrystalMagnet")]
        static void SuperRingMagnet()
        {
            // TODO: Implement this.
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerBFF2000), "State_Flinch")]
        static void StopFlinch(ref float ___genericTimer) => ___genericTimer = 999;
    }
}
