# Control Save Game Editor

A modern Windows desktop application for editing save files (`persistent.chunk`) from the video game **Control** by Remedy Entertainment. Built with C#, .NET 8, WPF, and Fluent UI styling.

![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-blue)
![Framework](https://img.shields.io/badge/.NET-8.0-purple)
![License](https://img.shields.io/badge/license-MIT-green)

---

## ✨ Features

- **Save Slot Detection:** Automatically locates and reads save files from `%LOCALAPPDATA%\Remedy\Control\` (Epic Games, Steam, GOG).
- **CRC32 Checksum Engine:** Recalculates IEEE 802.3 CRC32 checksums upon saving to avoid corrupted-save warnings.
- **Automatic Backups:** Generates `.bak` backup copies before writing changes.
- **Currency & Stats:**
  - Modify **Source** (in-game currency) with quick-boost buttons (+100k, +1M, 10M).
  - Modify **Ability Points** (+10, +50, Max 100 AP).
  - Displays fast-travel Control Points and collectible lore document counts.
- **Crafting Materials & Resources:**
  - View and adjust quantities for all materials (*Untapped Potential, Astral Blips, Corrupted Samples, Ritual Impulses, Threshold Remnants, House Memories, etc.*).
  - Jukebox Tokens for *Formation* expeditions.
  - 1-click **"Max All Materials (999)"**.
- **Weapon & Player Mods (God Mode):**
  - View individual weapon and player mod multipliers.
  - **⚡ Boost All (+500%)** / **⚡ God Mode (+1000%)** for massive damage, infinite launch energy, and rapid health recovery.
  - 1-click **"Reset Vanilla"** to restore original stats.
- **Outfits & Costumes:**
  - Toggle individual outfits or use **"Unlock All Outfits"** to unlock all 12 game suits (including pre-order and console-exclusive outfits like *Astral Dive* and *Tactical Response*).

---

## 🛠️ Building & Running

### Requirements
- **Windows 10 / 11**
- **.NET 8.0 SDK** (or Visual Studio 2022+ with the *.NET desktop development* workload)

### Build via Command Line
```powershell
# Restore and build solution
dotnet build ControlSaveEditor.sln

# Publish as a standalone single-file .exe
dotnet publish ControlSaveEditor.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

### Visual Studio
1. Open `ControlSaveEditor.sln`.
2. Press `F5` to build and run.

---

## ⚠️ Notes on Saves
Always ensure the game is closed before modifying save files so the game doesn't overwrite your changes upon exiting.
