// TODO: Make the BFF2000 magnetise these when I get around to making that work properly.
using UnityEngine;

namespace Freedom_Planet_2_Ring_System.CustomObjectScripts
{
    internal class SuperRing : FPBaseObject
    {
        private FPObjectState state;
        private FPHitBox hbItem;
        private Vector2 start;

        private bool isMagnetic;
        public Vector2 magnetRange;
        private float magnetPos;
        private float magnetAccel;
        private FPPlayer targetPlayer;

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

            magnetRange = new Vector2(200f, 160f);

            // Set this Super Ring to its idle state.
            state = State_Idle;

            // Run the FPBaseObject start.
            base.Start();
        }

        private void Update()
        {
            // Invoke the current state if it isn't null.
            state?.Invoke();
        }

        public void SetMagnetic(FPPlayer player)
        {
            if (!(targetPlayer != null) && !(player == null))
            {
                targetPlayer = player;
                isMagnetic = true;
                if (allowFloatPositions)
                {
                    allowFloatPositions = false;
                    base.transform.parent = null;
                }
                start = position;
                state = State_Magnet;
            }
        }

        private void State_Magnet()
        {
            velocity.x = 0f;
            velocity.y = 0f;
            position.x = start.x * (1f - magnetPos) + targetPlayer.position.x * magnetPos;
            position.y = start.y * (1f - magnetPos) + targetPlayer.position.y * magnetPos;
            float num = Vector2.Distance(position, targetPlayer.position);
            magnetAccel = Mathf.Min(magnetAccel + 0.005f * FPStage.deltaTime, 0.1f);
            magnetPos += magnetAccel / (num * 0.01f) * FPStage.deltaTime;
            CollisionCheck();
        }

        private void State_Idle()
        {
            CollisionCheck();
        }

        private void CollisionCheck()
        {
            // Set up a variable to store a reference to the player object.
            FPBaseObject objRef = null;

            // Loop through every player object in the stage.
            while (FPStage.ForEach(FPPlayer.classID, ref objRef))
            {
                // Store this player in the player reference.
                playerClassRef = (FPPlayer)objRef;

                if (state == new FPObjectState(State_Idle) && (isMagnetic || (playerClassRef.shieldID == 1 && playerClassRef.shieldHealth > 0)) && playerClassRef.position.x > position.x - magnetRange.x && playerClassRef.position.x < position.x + magnetRange.x && playerClassRef.position.y > position.y - magnetRange.y && playerClassRef.position.y < position.y + magnetRange.y)
                {
                    targetPlayer = playerClassRef;
                    if (allowFloatPositions)
                    {
                        allowFloatPositions = false;
                        base.transform.parent = null;
                    }
                    start = position;
                    state = State_Magnet;
                }

                // Check if the player's touch hitbox is overlapping this Super Ring's hitbox.
                if (FPCollision.CheckOOBB(this, hbItem, objRef, playerClassRef.hbTouch))
                {
                    // Create a sparkle at this Super Ring's position.
                    FPStage.CreateStageObject(Sparkle.classID, position.x, position.y);

                    // Play the Super Ring sound.
                    FPAudio.PlayCollectibleSfx(Plugin.ringsAssetBundle.LoadAsset<AudioClip>("superring"));

                    // Give the player 10 Rings.
                    for (int ringIndex = 0; ringIndex < 10; ringIndex++) FPSaveManager.AddCrystal(playerClassRef);

                    // Destroy this Super Ring.
                    FPStage.DestroyStageObject(this);
                }
            }
        }
    }
}
