from BaseClasses import (
    Item,
    ItemClassification,
    Location,
    Region,
    Tutorial,
)

from worlds.AutoWorld import (
    World,
    WebWorld,
)

# Import required so the class registers itself
# with Archipelago's generic BizHawk Client.
from .Client import ToyStoryRacerClient

from .Items import (
    item_table,
    item_classification,
)

from .Locations import (
    location_table,
    location_name_to_challenge,
    tower_key_to_name,
    type_key_to_name,
    canonicalize_challenge_type,
)

from .Options import (
    ToyStoryRacerOptions,
)


# ============================================================
# CHARACTER REQUIREMENTS
# ============================================================

STARTING_CHARACTERS = {
    "Woody",
    "Buzz",
    "RC",
    "Bo Peep",
}


CHARACTER_ITEMS = {
    "Rex": "Rex",
    "Hamm": "Hamm",
    "Slinky": "Slinky",
    "Mr. Potato Head": "Mr. Potato Head",
    "LGM": "LGM",
    "Rocky": "Rocky",
    "Lenny": "Lenny",
    "Babyface": "Babyface",
}


# ============================================================
# CLASSES
# ============================================================

class ToyStoryRacerItem(Item):
    game = "Toy Story Racer"


class ToyStoryRacerLocation(Location):
    game = "Toy Story Racer"


# ============================================================
# WEB
# ============================================================

class ToyStoryRacerWebWorld(WebWorld):

    game_info_languages = [
        "en"
    ]

    tutorials = [
        Tutorial(
            "Multiworld Setup Guide",
            "A guide to setting up Toy Story Racer for Archipelago.",
            "English",
            "setup_en.md",
            "setup/en",
            ["Cristian"],
        )
    ]


# ============================================================
# WORLD
# ============================================================

class ToyStoryRacerWorld(World):

    game = "Toy Story Racer"

    web = ToyStoryRacerWebWorld()

    options_dataclass = (
        ToyStoryRacerOptions
    )

    item_name_to_id = (
        item_table
    )

    location_name_to_id = (
        location_table
    )


    # ========================================================
    # REGIONS
    # ========================================================

    def create_regions(self):

        menu = Region(
            "Menu",
            self.player,
            self.multiworld,
        )


        for (
            location_name,
            location_id,
        ) in location_table.items():

            location = (
                ToyStoryRacerLocation(
                    self.player,
                    location_name,
                    location_id,
                    menu,
                )
            )


            menu.locations.append(
                location
            )


        self.multiworld.regions.append(
            menu
        )


    # ========================================================
    # ITEMS
    # ========================================================

    def create_items(self):

        # ----------------------------------------------------
        # 200 Soldiers
        # ----------------------------------------------------

        for _ in range(200):

            self.multiworld.itempool.append(
                ToyStoryRacerItem(
                    "Soldier",
                    ItemClassification.progression,
                    item_table["Soldier"],
                    self.player,
                )
            )


        # ----------------------------------------------------
        # 8 unlockable characters
        # ----------------------------------------------------

        for item_name in [
            "Rex",
            "Hamm",
            "Slinky",
            "Mr. Potato Head",
            "LGM",
            "Rocky",
            "Lenny",
            "Babyface",
        ]:

            self.multiworld.itempool.append(
                self.create_item(
                    item_name
                )
            )


        # ----------------------------------------------------
        # 20 filler
        # ----------------------------------------------------

        for _ in range(20):

            self.multiworld.itempool.append(
                self.create_item(
                    "Nothing"
                )
            )


    # ========================================================
    # CREATE ITEM
    # ========================================================

    def create_item(
        self,
        name,
    ):

        return ToyStoryRacerItem(
            name,
            item_classification[name],
            item_table[name],
            self.player,
        )


    # ========================================================
    # CHARACTER ACCESS
    # ========================================================

    def can_use_character(
        self,
        state,
        character,
    ):

        if character in STARTING_CHARACTERS:
            return True


        required_item = (
            CHARACTER_ITEMS.get(
                character
            )
        )


        if required_item is None:
            return False


        return state.has(
            required_item,
            self.player,
        )


    # ========================================================
    # CHALLENGE ACCESS
    # ========================================================

    def can_access_challenge(
        self,
        state,
        character,
        required_soldiers,
    ):

        return (
            self.can_use_character(
                state,
                character,
            )

            and

            state.count(
                "Soldier",
                self.player,
            )
            >= required_soldiers
        )


    # ========================================================
    # RULES
    # ========================================================

    def set_rules(self):

        # ----------------------------------------------------
        # Individual Challenges
        # ----------------------------------------------------

        for (
            location_name,
            challenge,
        ) in (
            location_name_to_challenge.items()
        ):

            location = (
                self.multiworld.get_location(
                    location_name,
                    self.player,
                )
            )


            character = (
                challenge[
                    "character"
                ]
            )

            required_soldiers = (
                challenge[
                    "required_soldiers"
                ]
            )


            location.access_rule = (
                lambda state,
                character=character,
                required_soldiers=required_soldiers:

                    self.can_access_challenge(
                        state,
                        character,
                        required_soldiers,
                    )
            )


        # ----------------------------------------------------
        # Tower Requirements
        # ----------------------------------------------------

        tower_requirements = {}


        for challenge in (
            location_name_to_challenge.values()
        ):

            character = (
                challenge[
                    "character"
                ]
            )

            soldiers = (
                challenge[
                    "required_soldiers"
                ]
            )


            tower_requirements[
                character
            ] = max(
                tower_requirements.get(
                    character,
                    0,
                ),
                soldiers,
            )


        # ----------------------------------------------------
        # Tower Complete Locations
        # ----------------------------------------------------

        for (
            character,
            tower_location_name,
        ) in (
            tower_key_to_name.items()
        ):

            location = (
                self.multiworld.get_location(
                    tower_location_name,
                    self.player,
                )
            )


            required_soldiers = (
                tower_requirements[
                    character
                ]
            )


            location.access_rule = (
                lambda state,
                character=character,
                required_soldiers=required_soldiers:

                    self.can_access_challenge(
                        state,
                        character,
                        required_soldiers,
                    )
            )


        # ----------------------------------------------------
        # First Challenge Type Candidates
        # ----------------------------------------------------

        type_candidates = {

            challenge_type: []

            for challenge_type
            in type_key_to_name
        }


        for challenge in (
            location_name_to_challenge.values()
        ):

            challenge_type = (
                canonicalize_challenge_type(
                    challenge[
                        "type"
                    ]
                )
            )


            type_candidates[
                challenge_type
            ].append(
                (
                    challenge[
                        "character"
                    ],

                    challenge[
                        "required_soldiers"
                    ],
                )
            )


        # ----------------------------------------------------
        # First Challenge Type Locations
        # ----------------------------------------------------

        for (
            challenge_type,
            location_name,
        ) in (
            type_key_to_name.items()
        ):

            location = (
                self.multiworld.get_location(
                    location_name,
                    self.player,
                )
            )


            candidates = tuple(
                type_candidates[
                    challenge_type
                ]
            )


            location.access_rule = (
                lambda state,
                candidates=candidates:

                    any(
                        self.can_access_challenge(
                            state,
                            character,
                            required_soldiers,
                        )

                        for (
                            character,
                            required_soldiers,
                        )

                        in candidates
                    )
            )


        # ----------------------------------------------------
        # Goal
        # ----------------------------------------------------

        tower_names = tuple(
            tower_key_to_name.values()
        )


        required_towers = int(
            self.options.goal.value
        )


        self.multiworld.completion_condition[
            self.player
        ] = (
            lambda state,
            tower_names=tower_names,
            required_towers=required_towers:

                sum(
                    1

                    for tower_name
                    in tower_names

                    if state.can_reach_location(
                        tower_name,
                        self.player,
                    )
                )
                >= required_towers
        )


    # ========================================================
    # SLOT DATA
    # ========================================================

    def fill_slot_data(self):

        return {
            "goal_towers": int(
                self.options.goal.value
            ),
        }