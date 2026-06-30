using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace Freedom_Planet_2_Ring_System.Patchers
{
    internal class FPPlayerPatcher
    {
        // Holds a reference to the player's object.
        public static FPPlayer player;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Start")]
        private static void Setup(FPPlayer __instance)
        {
            // Get the player's object.
            player = __instance;

            // Only do the other stuff if we have the Power Ring item equipped.
            if (!player.powerups.Contains((FPPowerup)Plugin.ringItemID) || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Bakunawa_Chase")
                return;

            // Sneak the No Petals Brave Stone into our item set, as manually trying to change the life petals into crystals caused so many problems.
            if (!player.powerups.Contains(FPPowerup.NO_PETALS))
                player.powerups = [.. player.powerups.AddItem(FPPowerup.NO_PETALS)];

            // Set the count of crystals for an extra life to 100 or 200 depending on if the Expensive Stocks Brave Stone is equipped.
            if (__instance.powerups.Contains(FPPowerup.EXPENSIVE_STOCKS))
            {
                __instance.extraLifeCost = 200;
                __instance.crystals = 200;
            }
            else
            {
                __instance.extraLifeCost = 100;
                __instance.crystals = 100;
            }

            // Give us 10 Rings if the config option to do so is enabled.
            if (Plugin.configStartWithRings.Value)
            {
                __instance.totalCrystals = 10;
                __instance.crystals = 90;

                if (FPStage.checkpointEnabled && !Plugin.configStartWithRingsCheckpoint.Value)
                {
                    __instance.totalCrystals = 0;
                    __instance.crystals = 100;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "LateUpdate")]
        static void ReplaceHealth(FPPlayer __instance)
        {
            // Only do this if we have the Power Ring item equipped.
            if (!player.powerups.Contains((FPPowerup)Plugin.ringItemID) ||  UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Bakunawa_Chase")
                return;

            // Remove our health if we have no Rings or give us maximum health if we do.
            if (__instance.totalCrystals <= 0) __instance.health = 0;
            else __instance.health = __instance.healthMax;

            // Stupid hack to kill the BFF2000 if we're out of Rings in it.
            FPBaseObject objRef = null;
            while (FPStage.ForEach(PlayerBFF2000.classID, ref objRef))
            {
                PlayerBFF2000 playerBFF = (PlayerBFF2000)objRef;

                if (playerBFF != null)
                    __instance.health = -1;
            }

            // If we're using the nerf shields option, then force its health down to 1.
            // TODO: This makes the Strong Shields Potion do nothing.
            if (__instance.shieldHealth > 1 && Plugin.configNerfShields.Value == true)
                __instance.shieldHealth = 1;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), "Action_Hurt")]
        static void HighKnockback(FPPlayer __instance)
        {
            // Only do this if we have the Power Ring item equipped.
            if (!player.powerups.Contains((FPPowerup)Plugin.ringItemID) || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Bakunawa_Chase")
                return;

            // Give us some decently high knockback.
            if (__instance.direction == FPDirection.FACING_RIGHT) __instance.hurtKnockbackX = -8;
            if (__instance.direction == FPDirection.FACING_LEFT) __instance.hurtKnockbackX = 8;
            __instance.hurtKnockbackY = 8;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), "State_Hurt")]
        static void DropRings(FPPlayer __instance)
        {
            // Only do this if we have the Power Ring item equipped.
            if (!player.powerups.Contains((FPPowerup)Plugin.ringItemID) || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Bakunawa_Chase")
                return;

            // Check that our timer is 0, which it will be the first time the Hurt state is activated.
            if (__instance.genericTimer == 0)
            {
                // Boost the invincibility time so we actually have a chance to pick up a fallen Ring.
                __instance.invincibilityTime = 120f;
                if (__instance.IsPowerupActive(FPPowerup.PETAL_ARMOR))
                    __instance.invincibilityTime = 240;

                // Strip us of our extra life progress.
                if (__instance.powerups.Contains(FPPowerup.EXPENSIVE_STOCKS))
                    __instance.crystals = 200;
                else
                    __instance.crystals = 100;

                // Play the Ring loss sound.
                __instance.Action_PlaySound(Plugin.ringsAssetBundle.LoadAsset<AudioClip>("ringloss"));

                // Calculate how many Rings we should drop.
                int ringCount = Mathf.Min(__instance.totalCrystals, 32);
                int droppedRings = 0;

                // Loop through and drop up to 32 Rings.
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
                __instance.totalCrystals = 0;
            }
        }
    }
}
