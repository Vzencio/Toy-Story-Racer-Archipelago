# Toy Story Racer Archipelago Setup

## Requirements

- Archipelago 0.6.7 or newer
- BizHawk
- Toy Story Racer NTSC-U / SLUS-01214
- Toy Story Racer Archipelago APWorld
- Toy Story Racer Archipelago Lua connector

## Install the APWorld

Open the Archipelago Launcher.

Select:

Install APWorld

Choose:

toy_story_racer.apworld

Restart the Archipelago Launcher after installation.

## Generate a YAML

Open:

Generate Template Options

Archipelago will create template YAML files for installed games.

Find the Toy Story Racer template and copy it into the Players folder.

Configure your player name and goal.

Example:

    name: Player1
    game: Toy Story Racer

    Toy Story Racer:
      progression_balancing: 50
      accessibility: full
      goal: one_tower

Available goals:

- one_tower
- three_towers
- six_towers
- all_towers

## Generate a Seed

Place the configured YAML in the Archipelago Players folder.

Open the Archipelago Launcher and select:

Generate

The generated multiworld ZIP will be placed in the output folder.

## Playing

Start the generated multiworld server or connect to an online Archipelago room.

Launch the Toy Story Racer client.

Start Toy Story Racer in BizHawk.

Load the Toy Story Racer Lua connector.

Once both the Archipelago server and BizHawk connector are connected, play normally.