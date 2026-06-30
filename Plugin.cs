// TODO: The HUD looks a bit goofy in the Lunar Cannon boss.
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using FP2Lib.Item;
using Freedom_Planet_2_Ring_System.Patchers;
using HarmonyLib;
using System.IO;
using UnityEngine;

namespace Freedom_Planet_2_Ring_System
{
    [BepInPlugin("K24_FP2_Rings", "Ring System", "1.0.0")]
    [BepInDependency("000.kuborro.libraries.fp2.fp2lib")]
    public class Plugin : BaseUnityPlugin
    {
        // Asset Bundle for the Ring mod.
        public static AssetBundle ringsAssetBundle;

        // The ID of the Power Ring item.
        public static int ringItemID;

        // Config options.
        public static ConfigEntry<bool> configNerfShields;
        public static ConfigEntry<bool> configStartWithRings;
        public static ConfigEntry<bool> configStartWithRingsCheckpoint;
        public static ConfigEntry<bool> configSonicHUD;

        // Logger.
        public static ManualLogSource consoleLog;

        private void Awake()
        {
            // Set up the logger.
            consoleLog = Logger;

            // Check for the asset bundle.
            if (!File.Exists($@"{Paths.GameRootPath}\mod_overrides\rings.assets"))
            {
                consoleLog.LogError($@"Failed to find the rings.assets file! Please ensure it is correctly located in '{Paths.GameRootPath}\mod_overrides'.");
                return;
            }

            // Load our asset bundle.
            ringsAssetBundle = AssetBundle.LoadFromFile($@"{Paths.GameRootPath}\mod_overrides\rings.assets");

            // Print all the asset names from the asset bundle, as a debug log.
            foreach (string assetName in ringsAssetBundle.GetAllAssetNames())
                consoleLog.LogDebug(assetName);

            // Get the config options.
            configNerfShields = Config.Bind("Misc",
                                            "Nerf Shields",
                                            false,
                                            "Forces Shields to only have one hit point.\r\n" +
                                            "false: Disabled\r\n" +
                                            "true: Enabled");

            configStartWithRings = Config.Bind("Misc",
                                               "Start with Rings",
                                               true,
                                               "Starts stages with 10 Rings, primarily to aid with boss only stages that normally have no Crystal Shards.\r\n" +
                                               "false: Disabled\r\n" +
                                               "true: Enabled");

            configStartWithRingsCheckpoint = Config.Bind("Misc",
                                                         "Start with Rings at Checkpoint",
                                                         true,
                                                         "Makes respawning at Checkpoints also give 10 Rings. This does nothing if the previous option is disabled.\r\n" +
                                                         "false: Disabled\r\n" +
                                                         "true: Enabled");

            configSonicHUD = Config.Bind("Misc",
                                         "Enable Sonic HUD",
                                         false,
                                         "Replaces most elements of the HUD with a recreation of Sonic 1's when the Power Ring item is equipped.\r\n" +
                                         "false: Disabled\r\n" +
                                         "true: Enabled");

            // Create and register the Power Ring item.
            FP2Lib.Item.ItemHandler.RegisterItem("k24.rings.bravestone", "Power Ring", ringsAssetBundle.LoadAsset<Sprite>("braveStoneRing"), "Replaces Crystals and Petals with Rings. Drop all held Rings upon taking damage and lose a life if damaged while holding none.", IAddToShop.Yuni, 0, 1, 0.25f);
            ringItemID = FP2Lib.Item.ItemHandler.GetItemDataByUid("k24.rings.bravestone").itemID;

            // Patch our functions.
            Harmony.CreateAndPatchAll(typeof(FPHudMasterPatcher));
            Harmony.CreateAndPatchAll(typeof(FPPlayerPatcher));
            Harmony.CreateAndPatchAll(typeof(ItemBoxPatcher));
            Harmony.CreateAndPatchAll(typeof(ItemCrystalPatcher));
            Harmony.CreateAndPatchAll(typeof(PlayerBFF2000Patcher));
        }
    }
}
