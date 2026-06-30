using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Freedom_Planet_2_Ring_System.Patchers
{
    public static class ItemCrystalPatcher
    {
        public static Dictionary<ItemCrystal, float> terrainDelay = [];
        public static Dictionary<ItemCrystal, float> vanishTimer = [];

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemCrystal), "CollisionCheck")]
        static void RingVisuals(ref Animator ___animator, ItemCrystal __instance)
        {
            // Only do this if we have the Power Ring item equipped.
            if (FPPlayerPatcher.player != null && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Bakunawa_Chase")
                if (!FPPlayerPatcher.player.powerups.Contains((FPPowerup)Plugin.ringItemID))
                    return;

            // If we're not already using the Ring animator, then replace it and the sound with the Ring ones.
            if (___animator.runtimeAnimatorController.name != "Ring Animator")
            {
                ___animator.runtimeAnimatorController = Plugin.ringsAssetBundle.LoadAsset<RuntimeAnimatorController>("Ring Animator");
                __instance.sfxCrystalGet = Plugin.ringsAssetBundle.LoadAsset<AudioClip>("ring");
                __instance.activationMode = FPActivationMode.ALWAYS_ACTIVE;
            }
        }

        public static void State_Dropped() { }

        public static void State_Despawned() { }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemCrystal), "Update")]
        static void RingDrop(ref Animator ___animator, ItemCrystal __instance)
        {
            // Check if we're in our dummy dropped state.
            if (__instance.state == State_Dropped)
            {
                // If we're not already using the Ring animator, then replace it and the sound with the Ring ones.
                if (___animator.runtimeAnimatorController.name != "Ring Animator")
                {
                    ___animator.runtimeAnimatorController = Plugin.ringsAssetBundle.LoadAsset<RuntimeAnimatorController>("Ring Animator");
                    __instance.sfxCrystalGet = Plugin.ringsAssetBundle.LoadAsset<AudioClip>("ring");
                    __instance.activationMode = FPActivationMode.ALWAYS_ACTIVE;
                }

                // Reduce our y velocity to apply gravity.
                __instance.velocity.y -= 0.5f * FPStage.deltaTime;

                // Move this dropepd Ring based on our velocity and the game's delta timer.
                __instance.position.x += __instance.velocity.x * FPStage.deltaTime;
                __instance.position.y += __instance.velocity.y * FPStage.deltaTime;

                // Increment this Ring's vanish timer based on the game's delta timer.
                vanishTimer[__instance] += 1 * FPStage.deltaTime;

                // Make this Ring blink in the last 64 frames of its existence.
                if (vanishTimer[__instance] >= 192)
                {
                    if (System.Math.Floor(vanishTimer[__instance]) % 2 == 0) __instance.GetComponent<SpriteRenderer>().enabled = false;
                    else __instance.GetComponent<SpriteRenderer>().enabled = true;
                }

                // Destroy this Ring if its been active for 256 frames.
                if (vanishTimer[__instance] >= 256)
                {
                    FPStage.DestroyStageObject(__instance);
                    vanishTimer.Remove(__instance);
                    terrainDelay.Remove(__instance);
                    __instance.state = State_Despawned;
                    return;
                }

                // Handle this Ring's bouncing.
                CollisionHack(__instance);
            }
        }

        public static void CollisionHack(ItemCrystal crystal)
        {
            // Code borrowed from the ProjectileBasic's collision handling.
            if (terrainDelay[crystal] >= 0f)
            {
                float num = crystal.angle;
                crystal.angle = 0f;
                FPHitBox rectA = new()
                {
                    enabled = true,
                    left = -1f,
                    right = 1f,
                    bottom = -12f,
                    top = 12f
                };
                if (FPCollision.CheckTerrainOOBB(crystal, rectA))
                {
                    crystal.velocity.y *= -1f;
                    terrainDelay[crystal] = -6f;
                }
                crystal.angle = num;
            }
            if (terrainDelay[crystal] >= 0f)
            {
                float num2 = crystal.angle;
                crystal.angle = 0f;
                FPHitBox rectA2 = new()
                {
                    enabled = true,
                    left = -12f,
                    right = 12f,
                    bottom = -1f,
                    top = 1f
                };
                if (FPCollision.CheckTerrainOOBB(crystal, rectA2))
                {
                    crystal.velocity.x *= -1f;
                    terrainDelay[crystal] = -6f;
                }
                crystal.angle = num2;
            }
            if (terrainDelay[crystal] < 0f)
            {
                terrainDelay[crystal] = Mathf.Min(terrainDelay[crystal] + FPStage.deltaTime, 0f);
            }

            FPHitBox hbItem = new()
            {
                left = -16f,
                top = 16f,
                right = 16f,
                bottom = -16f,
                enabled = true,
                visible = true
            };

            if (vanishTimer[crystal] >= 64)
            {
                FPBaseObject objRef = null;
                while (FPStage.ForEach(FPPlayer.classID, ref objRef))
                {
                    FPPlayer fPPlayer = (FPPlayer)objRef;
                    if (FPCollision.CheckOOBB(crystal, hbItem, objRef, fPPlayer.hbTouch))
                    {
                        FPSaveManager.AddCrystal(fPPlayer);
                        if (crystal.redCrystal && fPPlayer.health < fPPlayer.healthMax && !fPPlayer.IsPowerupActive(FPPowerup.ONE_HIT_KO))
                        {
                            fPPlayer.health += 0.1f;
                            FPStage.CreateStageObject(Heart.classID, crystal.position.x + UnityEngine.Random.Range(-24f, 24f), crystal.position.y + UnityEngine.Random.Range(-24f, 24f));
                        }
                        crystal.Action_Get();
                        vanishTimer.Remove(crystal);
                        terrainDelay.Remove(crystal);
                        crystal.state = State_Despawned;
                    }
                }
            }
        }
    }
}
