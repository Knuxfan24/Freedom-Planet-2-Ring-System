using UnityEngine;

namespace Freedom_Planet_2_Ring_System.CustomObjectScripts
{
    internal class SuperRing : FPBaseObject
    {
        private FPObjectState state;
        private FPHitBox hbItem;

        private FPPlayer playerClassRef;

        private new void Start()
        {
            // Create the hitbox for this Super Ring.
            hbItem.enabled = true;
            hbItem.visible = true;
            hbItem.left = -32;
            hbItem.top = 32;
            hbItem.right = 32;
            hbItem.bottom = -32;

            // Set this Super Ring to its idle state (the only state it has).
            state = State_Idle;

            // Run the FPBaseObject start.
            base.Start();
        }

        private void Update()
        {
            // Invoke the current state if it isn't null.
            state?.Invoke();
        }

        private void State_Idle()
        {
            // Set up a variable to store a reference to the player object.
            FPBaseObject objRef = null;

            // Loop through every player object in the stage.
            while (FPStage.ForEach(FPPlayer.classID, ref objRef))
            {
                // Store this player in the player reference.
                playerClassRef = (FPPlayer)objRef;

                // Check if the player's touch hitbox is overlapping this Super Ring's hitbox.
                if (FPCollision.CheckOOBB(this, hbItem, objRef, playerClassRef.hbTouch))
                {
                    // Create a sparkle at this Super Ring's position.
                    FPStage.CreateStageObject(Sparkle.classID, position.x, position.y);

                    // Play the Super Ring sound.
                    FPAudio.PlayCollectibleSfx(Plugin.ringsAssetBundle.LoadAsset<AudioClip>("superring"));

                    // Give the player 10 Rings.
                    for (int ringIndex = 0; ringIndex < 10; ringIndex++)  FPSaveManager.AddCrystal(playerClassRef); 

                    // Destroy this Super Ring.
                    FPStage.DestroyStageObject(this);
                }
            }
        }
    }
}
