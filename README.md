# Toy Story Racer Archipelago

Archipelago randomizer support for **Toy Story Racer (PlayStation 1)** using **DuckStation** and a dedicated Windows client.

> Supported game version: **NTSC-U / USA — SLUS-01214**

## What you need

- A legal copy of **Toy Story Racer (USA / SLUS-01214)**.
- [DuckStation](https://www.duckstation.org/)
- [Archipelago](https://archipelago.gg/)
- The latest `toy_story_racer.apworld`
- The latest `TSR Archipelago Client.exe`

The client is currently intended for **Windows x64**.

## Installing the APWorld

1. Download `toy_story_racer.apworld` from the latest GitHub Release.
2. Open the Archipelago Launcher.
3. Use **Install APWorld** and select the downloaded file.
4. Generate a Toy Story Racer template YAML from Archipelago.
5. Edit the YAML to your preference and use it when generating your multiworld.

For general Archipelago setup and multiworld generation, see:
https://archipelago.gg/tutorial/Archipelago/setup_en

## Starting a game

1. Generate and host your Archipelago multiworld.
2. Start DuckStation.
3. Launch **Toy Story Racer (USA / SLUS-01214)**.
4. Open `TSR Archipelago Client.exe`.
5. Enter:
   - **Server** — for example `archipelago.gg:38281`
   - **Slot** — your Toy Story Racer player/slot name
   - **Password** — only if the room uses one
6. Click **Connect**.
7. Once DuckStation, Toy Story Racer, and Archipelago all show as connected/detected, play normally.

The client automatically reconnects if DuckStation is closed and reopened.

## How the randomizer works

Toy Story Racer has 200 challenge Soldiers across 12 characters.

In Archipelago:

- Completed challenges are location checks.
- Soldiers are progression items received through Archipelago.
- The eight normally unlockable characters can also be progression items.
- Character tower completion and first completion of each challenge type are additional checks.
- Your selected goal determines how many character towers must be completed.

The client keeps the in-game Soldier count, character unlocks, and challenge completion state synchronized with the Archipelago server.

## Client features

The client includes:

- Live DuckStation / game / Archipelago connection status
- Soldier counter
- Check counter
- Goal display
- Archipelago message log
- Colored item/player/location messages
- Separate **Hints** tab
- Archipelago chat and commands such as `!hint`
- Automatic reconnection after restarting DuckStation

## Recommended DuckStation graphics settings

These are optional visual improvements and are not required for Archipelago.

Recommended starting point:

- Use a **hardware renderer** such as Vulkan or Direct3D 11/12.
- Set **Internal Resolution** high enough for your display.
  - For a 1080p display, around **5x** internal resolution is a good starting point.
- Enable **PGXP Geometry Correction** to reduce PS1 polygon wobble.
- Texture filtering is optional; use whichever setting looks best to you.
- Keep the original **4:3 aspect ratio** unless you specifically want to experiment with DuckStation's widescreen rendering.

DuckStation notes that higher internal resolution improves rendered 3D geometry, while PGXP can reduce polygon wobble and texture warping.

## Optional 60 FPS patch

A ready-to-use `SLUS-01214.cht` file is included with the release.

The patch uses the following GameShark codes:

```text
800A9134 0001
800A9400 0001
```

60 FPS code credit: **SuperMoonKnight**, published at:
https://gamehacking.org/game/88739

### DuckStation setup for 60 FPS

1. Install or load the `SLUS-01214.cht` patch in DuckStation.
2. Enable **60 FPS** for Toy Story Racer.
3. Set DuckStation's emulated CPU clock to **150%**.

The 150% CPU setting is recommended because the game can slow down when running the 60 FPS patch at the default emulated CPU speed.

The 60 FPS patch is optional and is not required for Archipelago.

## Notes about saves

The Archipelago server is treated as the authoritative randomizer state while connected.

When reconnecting to an existing seed, the client restores:

- completed challenge checks
- received Soldiers
- received character unlocks

Using an old vanilla save does not grant Archipelago checks that the server has not recorded.

## Troubleshooting

**The client cannot find DuckStation**
- Make sure DuckStation is already running.

**DuckStation is detected, but Toy Story Racer is not**
- Make sure the loaded game is the **USA NTSC-U version, SLUS-01214**.

**The client disconnected after closing DuckStation**
- Reopen DuckStation and load Toy Story Racer. The client should reconnect automatically.

**A challenge does not remain completed**
- Confirm the client is connected to the same Archipelago seed/slot and that the game version is SLUS-01214.

## Downloads

Use the **Releases** section of this repository for packaged files:

- `TSR Archipelago Client.exe`
- `toy_story_racer.apworld`
- `SLUS-01214.cht`

Do not download random binaries from mirrors or third-party reuploads.

## Legal

This project does **not** include the Toy Story Racer game, PlayStation BIOS files, or copyrighted game data.

You must provide your own legally obtained game copy and BIOS as required by your emulator setup.

This project is an unofficial fan-made Archipelago integration and is not affiliated with Disney, Pixar, Activision, Traveller's Tales, Sony, DuckStation, or the Archipelago project.

## Credits

- [Archipelago](https://archipelago.gg/)
- [DuckStation](https://www.duckstation.org/)
- [Archipelago.Core](https://github.com/ArsonAssassin/Archipelago.Core)
- 60 FPS code by **SuperMoonKnight** via [GameHacking.org](https://gamehacking.org/game/88739)
- Complete list of content for Toy Story Racer by **dnextreme88** via [gamefaqs.gamespot.com] (https://gamefaqs.gamespot.com/ps/444542-disney-pixar-toy-story-racer/faqs/73747)
