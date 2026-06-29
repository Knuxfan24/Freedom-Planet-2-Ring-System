using Freedom_Planet_2_Ring_System.Patchers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Freedom_Planet_2_Ring_System.CustomObjectScripts
{
    internal class SonicHUD : MonoBehaviour
    {
        // Arrays holding the digits for the various parts of the HUD.
        private FPHudDigit[] energy = new FPHudDigit[3];
        private FPHudDigit[] timer = new FPHudDigit[3];
        private FPHudDigit[] rings = new FPHudDigit[4];
        private FPHudDigit[] lives = new FPHudDigit[3];

        // Timer to control the Rings label blinking.
        private float ringTimer = 0f;

        // Get the ID of Sonic from FP2Lib.
        private readonly FPCharacterID sonicID = (FPCharacterID)FP2Lib.Player.PlayerHandler.GetPlayableCharaByUid("k24.sonic").id;

        private void Start()
        {
            // Get the various FPHudDigit components on the HUD.
            energy[0] = this.transform.GetChild(0).GetChild(0).GetComponent<FPHudDigit>();
            energy[1] = this.transform.GetChild(0).GetChild(1).GetComponent<FPHudDigit>();
            energy[2] = this.transform.GetChild(0).GetChild(2).GetComponent<FPHudDigit>();

            timer[0] = this.transform.GetChild(1).GetChild(0).GetComponent<FPHudDigit>();
            timer[1] = this.transform.GetChild(1).GetChild(2).GetComponent<FPHudDigit>();
            timer[2] = this.transform.GetChild(1).GetChild(3).GetComponent<FPHudDigit>();

            rings[0] = this.transform.GetChild(2).GetChild(0).GetComponent<FPHudDigit>();
            rings[1] = this.transform.GetChild(2).GetChild(1).GetComponent<FPHudDigit>();
            rings[2] = this.transform.GetChild(2).GetChild(2).GetComponent<FPHudDigit>();
            rings[3] = this.transform.GetChild(2).GetComponent<FPHudDigit>();

            lives[0] = this.transform.GetChild(3).GetChild(0).GetComponent<FPHudDigit>();
            lives[1] = this.transform.GetChild(3).GetChild(1).GetComponent<FPHudDigit>();
            lives[2] = this.transform.GetChild(3).GetComponent<FPHudDigit>();
        }

        private void Update()
        {
            // Only do any of this if the player exists.
            if (FPPlayerPatcher.player == null)
                return;

            DisplayEnergy();
            DisplayTime();
            DisplayRings();
            DisplayLives();
        }

        private void DisplayEnergy()
        {
            // Split the energy count into three individual ints.
            List<int> energyDigits = [.. ((int)FPPlayerPatcher.player.energy).ToString().PadLeft(3, '0').Select(digit => int.Parse(digit.ToString()))];

            // Loop through each digit in the energy count.
            for (int digitIndex = 0; digitIndex < energyDigits.Count; digitIndex++)
            {
                // Set this digit's sprite to the value plus 1 (as 0 is a blank sprite).
                energy[digitIndex].SetDigitValue(energyDigits[digitIndex] + 1);

                // If we have less than 100 energy, then the first two digits if they're ever 0.
                if (digitIndex < 2 && energyDigits[digitIndex] == 0 && FPPlayerPatcher.player.energy < 100)
                    energy[digitIndex].SetDigitValue(0);
            }
        }

        private void DisplayTime()
        {
            // Don't bother updating the timer if we've reached 10 minutes.
            if (FPStage.currentStage.minutes >= 10)
                return;

            // Set the minute counter.
            timer[0].SetDigitValue(FPStage.currentStage.minutes + 1);

            // Split the seconds count into two individual ints.
            List<int> secondsDigits = [.. ((int)FPStage.currentStage.seconds).ToString().PadLeft(2, '0').Select(digit => int.Parse(digit.ToString()))];

            // Loop through the two digits in the second count and set its corrosponding digit's sprite to the value plus 1 (as 0 is a blank sprite).
            for (int digitIndex = 0; digitIndex < secondsDigits.Count; digitIndex++)
                timer[digitIndex + 1].SetDigitValue(secondsDigits[digitIndex] + 1);
        }

        private void DisplayRings()
        {
            // Split the total crystals count into three individual ints.
            List<int> ringDigits = [.. FPPlayerPatcher.player.totalCrystals.ToString().PadLeft(3, '0').Select(digit => int.Parse(digit.ToString()))];

            // Loop through the digits in the ring count and set its corrosponding digit's sprite to the value plus 1 (as 0 is a blank sprite).
            for (int digitIndex = 0; digitIndex < ringDigits.Count; digitIndex++)
                rings[digitIndex].SetDigitValue(ringDigits[digitIndex] + 1);

            // Hide the first and second digits if we don't have enough rings for them to be larger than 0.
            if (FPPlayerPatcher.player.totalCrystals < 100) rings[0].SetDigitValue(0);
            if (FPPlayerPatcher.player.totalCrystals < 10) rings[1].SetDigitValue(0);

            // Check if we have no Rings.
            if (FPPlayerPatcher.player.totalCrystals == 0)
            {
                // Increment the Ring Timer.
                ringTimer += 1 * FPStage.deltaTime;

                // If the Ring Timer is less than 8, then swap to the red label, else swap to the normal one.
                if (ringTimer < 8) rings[3].SetDigitValue(1);
                else rings[3].SetDigitValue(0);

                // Loop the timer back down if its reached 16.
                if (ringTimer >= 16)
                    ringTimer -= 16;

            }

            // If we have Rings, then reset the timer and label.
            else
            {
                ringTimer = 0;
                rings[3].SetDigitValue(0);
            }
        }

        private void DisplayLives()
        {
            // Split the life count (incremented by 1, as the Sonic 1 HUD shouldn't display 0) into two individual ints.
            List<int> livesDigits = [.. (FPPlayerPatcher.player.lives + 1).ToString().PadLeft(2, '0').Select(digit => int.Parse(digit.ToString()))];

            // Loop through the digits in the life count and set its corrosponding digit's sprite to the value plus 1 (as 0 is a blank sprite).
            for (int digitIndex = 0; digitIndex < livesDigits.Count; digitIndex++)
                lives[digitIndex].SetDigitValue(livesDigits[digitIndex] + 1);

            // If we have less than 9 lives (which displays as 10) then hide the first digit.
            if ((FPPlayerPatcher.player.lives + 1) < 10) lives[0].SetDigitValue(0);

            // Swap the life icon depending on the character.
            if (FPSaveManager.character == sonicID) lives[2].SetDigitValue(5);
            else switch (FPSaveManager.character)
            {
                case FPCharacterID.LILAC: lives[2].SetDigitValue(0); break;
                case FPCharacterID.CAROL: case FPCharacterID.BIKECAROL: lives[2].SetDigitValue(1); break;
                case FPCharacterID.MILLA: lives[2].SetDigitValue(2); break;
                case FPCharacterID.NEERA: lives[2].SetDigitValue(3); break;
                default: lives[2].SetDigitValue(4); break;
            }

        }
    }
}
