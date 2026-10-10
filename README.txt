=============================================================
 CONTROL - ULTIMATE SAVE GAME EDITOR (v1.0.0)
=============================================================

A modern, standalone Windows desktop utility for editing save files
(persistent.chunk) from Control by Remedy Entertainment.

GitHub / Source Code: https://github.com/nr275/ModTools

-------------------------------------------------------------
 HOW TO USE:
-------------------------------------------------------------
1. IMPORTANT: Make sure the game is CLOSED before editing saves.
2. Run "ControlSaveEditor.exe".
3. Select your save slot from the top dropdown (saves from your
   local Remedy folder are auto-detected with timestamps).
   - If using a custom location, click "Browse File...".
4. Choose what you want to edit:
   - Currency & Stats: Source, Ability Points.
   - Crafting Materials: 1-click "Max All Materials (999)" or edit individual counts.
   - Mods & God Mode: Customize stat multipliers, or click "+500%" / "+1000% God Mode".
   - Outfits: Toggle individual suits or click "Unlock All Outfits" (unlocks all 12).
5. Click "Save & Apply All Changes" at the bottom right.
6. Launch Control and enjoy!

-------------------------------------------------------------
 SAFETY & BACKUPS:
-------------------------------------------------------------
- An automatic backup copy (.bak) of your original save is created
  in your save folder the first time you save changes.
- To revert at any time, delete "persistent.chunk" and rename
  "persistent.chunk.bak" back to "persistent.chunk".
- Save files are re-hashed with standard IEEE 802.3 CRC32 checksums
  to ensure the game loads them without corruption errors.

-------------------------------------------------------------
 COMPATIBILITY:
-------------------------------------------------------------
- Works with Epic Games Store, Steam, and GOG PC versions.
- Compatible with Windows 10 & Windows 11 (64-bit).

-------------------------------------------------------------
 AI DISCLOSURE:
-------------------------------------------------------------
This utility was created with the assistance of Google Antigravity (Gemini)
for binary reverse-engineering and UI development. 
The complete C# source code is 100% open-source at:
https://github.com/nr275/ModTools
