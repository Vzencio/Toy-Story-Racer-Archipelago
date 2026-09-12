from __future__ import annotations

from typing import TYPE_CHECKING

import logging

logger = logging.getLogger("Client")
from NetUtils import ClientStatus

import worlds._bizhawk as bizhawk
from worlds._bizhawk.client import BizHawkClient

from .Items import item_table
from .Locations import (
    location_table,
    location_key_to_name,
    challenge_key_to_data,
    tower_key_to_name,
    type_key_to_name,
    canonicalize_challenge_type,
)

if TYPE_CHECKING:
    from worlds._bizhawk.context import BizHawkClientContext


# ============================================================
# MEMORY
# ============================================================

MEMORY_DOMAIN = "MainRAM"

STATE_BASE = 0x0D7990
STATE_END = 0x0D79F4
STATE_SIZE = STATE_END - STATE_BASE + 1

SOLDIERS_ADDR = 0x0D79F4


# ============================================================
# CHALLENGES
# ============================================================

CHALLENGE_LAYOUT = {
    "Buzz": {
        "addresses": [0x0D7990, 0x0D7991, 0x0D7992],
        "count": 22,
    },

    "LGM": {
        "addresses": [0x0D7998, 0x0D7999],
        "count": 13,
    },

    "Hamm": {
        "addresses": [0x0D79A0, 0x0D79A1, 0x0D79A2],
        "count": 17,
    },

    "Woody": {
        "addresses": [0x0D79A8, 0x0D79A9, 0x0D79AA],
        "count": 20,
    },

    "RC": {
        "addresses": [0x0D79B0, 0x0D79B1, 0x0D79B2],
        "count": 21,
    },

    "Mr. Potato Head": {
        "addresses": [0x0D79B8, 0x0D79B9, 0x0D79BA],
        "count": 19,
    },

    "Slinky": {
        "addresses": [0x0D79C0, 0x0D79C1, 0x0D79C2],
        "count": 20,
    },

    "Bo Peep": {
        "addresses": [0x0D79C8, 0x0D79C9],
        "count": 16,
    },

    "Rex": {
        "addresses": [0x0D79D0, 0x0D79D1, 0x0D79D2],
        "count": 20,
    },

    "Rocky": {
        "addresses": [0x0D79D8, 0x0D79D9],
        "count": 16,
    },

    "Babyface": {
        "addresses": [0x0D79E0],
        "count": 8,
    },

    "Lenny": {
        "addresses": [0x0D79E8],
        "count": 8,
    },
}


# ============================================================
# CHARACTER UNLOCKS
# ============================================================

CHARACTER_UNLOCKS = {
    "LGM": 0x0D799C,
    "Hamm": 0x0D79A4,
    "Mr. Potato Head": 0x0D79BC,
    "Slinky": 0x0D79C4,
    "Rex": 0x0D79D4,
    "Rocky": 0x0D79DC,
    "Babyface": 0x0D79E4,
    "Lenny": 0x0D79EC,
}


# ============================================================
# HELPERS
# ============================================================

def read_byte(state_block: bytes, address: int) -> int:
    return state_block[address - STATE_BASE]


def challenge_complete(
    state_block: bytes,
    character: str,
    challenge_number: int,
) -> bool:

    layout = CHALLENGE_LAYOUT[character]

    zero_index = challenge_number - 1
    byte_index = zero_index // 8
    bit_index = zero_index % 8

    address = layout["addresses"][byte_index]

    value = read_byte(
        state_block,
        address,
    )

    return bool(
        value & (1 << bit_index)
    )


def tower_complete(
    state_block: bytes,
    character: str,
) -> bool:

    challenge_count = (
        CHALLENGE_LAYOUT[character]["count"]
    )

    return all(
        challenge_complete(
            state_block,
            character,
            challenge_number,
        )

        for challenge_number
        in range(
            1,
            challenge_count + 1,
        )
    )


# ============================================================
# CLIENT
# ============================================================

class ToyStoryRacerClient(BizHawkClient):

    game = "Toy Story Racer"
    system = "PSX"
    patch_suffix = None


    def __init__(self):

        # Identifies which AP seed/slot the RAM was last
        # synchronized against.
        self.sync_key = None

        # "replace":
        # AP is source of truth. Used when entering a new seed.
        #
        # "merge":
        # Used when reconnecting to the same seed so offline
        # progress is not destroyed.
        self.pending_sync_mode = None


    # ========================================================
    # ROM VALIDATION
    # ========================================================

    async def validate_rom(
        self,
        ctx: "BizHawkClientContext",
    ) -> bool:

        if ctx.rom_hash != "6637E179":
            return False

        ctx.game = self.game
        ctx.items_handling = 0b111
        ctx.want_slot_data = True
        ctx.watcher_timeout = 0.125

        self.sync_key = None
        self.pending_sync_mode = None

        logger.info(
            "Toy Story Racer NTSC-U detected."
        )

        return True


        # ----------------------------------------------------
        # Temporary alpha validation.
        #
        # All eight Toy Story Racer character unlock bytes
        # should be boolean flags.
        #
        # Once we obtain BizHawk's exact ROM hash from the
        # first test, this will be replaced with exact hash
        # validation for SLUS-01214.
        # ----------------------------------------------------

        for address in (
            CHARACTER_UNLOCKS.values()
        ):

            value = read_byte(
                state_block,
                address,
            )

            if value not in (0, 1):
                return False


        soldiers = read_byte(
            state_block,
            SOLDIERS_ADDR,
        )


        if soldiers > 200:
            return False


        ctx.game = self.game

        ctx.items_handling = 0b111

        ctx.want_slot_data = True

        ctx.watcher_timeout = 0.125


        # A newly validated ROM starts without AP state
        # synchronized.
        self.sync_key = None

        self.pending_sync_mode = None


        logger.info(
            "Toy Story Racer NTSC-U detected."
        )

        logger.info(
            f"BizHawk ROM hash: "
            f"{ctx.rom_hash}"
        )


        return True


    # ========================================================
    # SERVER PACKAGES
    # ========================================================

    def on_package(
        self,
        ctx: "BizHawkClientContext",
        cmd: str,
        args: dict,
    ) -> None:

        if cmd != "Connected":
            return


        current_key = (
            ctx.server_seed_name,
            ctx.auth,
        )


        if (
            self.sync_key is not None
            and self.sync_key == current_key
        ):

            # Same seed/slot:
            # preserve progress performed while disconnected.
            self.pending_sync_mode = "merge"


        else:

            # New seed/slot:
            # Archipelago becomes source of truth.
            self.pending_sync_mode = "replace"


        self.sync_key = current_key


        goal_towers = int(
            ctx.slot_data.get(
                "goal_towers",
                1,
            )
        )


        logger.info(
            f"Toy Story Racer goal: "
            f"{goal_towers} tower(s)."
        )


    # ========================================================
    # AP CHALLENGE STATE
    # ========================================================

    def build_ap_challenge_bytes(
        self,
        ctx: "BizHawkClientContext",
        character: str,
    ) -> list[int]:

        layout = (
            CHALLENGE_LAYOUT[
                character
            ]
        )


        values = [
            0
            for _ in layout["addresses"]
        ]


        for challenge_number in range(
            1,
            layout["count"] + 1,
        ):

            location_name = (
                location_key_to_name[
                    (
                        character,
                        challenge_number,
                    )
                ]
            )


            location_id = (
                location_table[
                    location_name
                ]
            )


            if (
                location_id
                not in ctx.checked_locations
            ):
                continue


            zero_index = (
                challenge_number - 1
            )

            byte_index = (
                zero_index // 8
            )

            bit_index = (
                zero_index % 8
            )


            values[
                byte_index
            ] |= (
                1 << bit_index
            )


        return values


    # ========================================================
    # FIND LOCAL CHECKS
    # ========================================================

    def get_local_checks(
        self,
        state_block: bytes,
    ) -> set[int]:

        checks = set()


        for (
            character,
            layout,
        ) in CHALLENGE_LAYOUT.items():

            for challenge_number in range(
                1,
                layout["count"] + 1,
            ):

                if not challenge_complete(
                    state_block,
                    character,
                    challenge_number,
                ):
                    continue


                # --------------------------------------------
                # Normal challenge
                # --------------------------------------------

                location_name = (
                    location_key_to_name[
                        (
                            character,
                            challenge_number,
                        )
                    ]
                )


                checks.add(
                    location_table[
                        location_name
                    ]
                )


                # --------------------------------------------
                # First Challenge Type
                # --------------------------------------------

                challenge_data = (
                    challenge_key_to_data[
                        (
                            character,
                            challenge_number,
                        )
                    ]
                )


                challenge_type = (
                    canonicalize_challenge_type(
                        challenge_data[
                            "type"
                        ]
                    )
                )


                type_location_name = (
                    type_key_to_name[
                        challenge_type
                    ]
                )


                checks.add(
                    location_table[
                        type_location_name
                    ]
                )


            # ------------------------------------------------
            # Tower Complete
            # ------------------------------------------------

            if tower_complete(
                state_block,
                character,
            ):

                tower_location_name = (
                    tower_key_to_name[
                        character
                    ]
                )


                checks.add(
                    location_table[
                        tower_location_name
                    ]
                )


        return checks


    # ========================================================
    # SYNC CHALLENGES FROM AP
    # ========================================================

    async def sync_challenges(
        self,
        ctx: "BizHawkClientContext",
        state_block: bytes,
        merge: bool,
    ) -> None:

        writes = []


        for (
            character,
            layout,
        ) in CHALLENGE_LAYOUT.items():

            ap_values = (
                self.build_ap_challenge_bytes(
                    ctx,
                    character,
                )
            )


            for (
                byte_index,
                address,
            ) in enumerate(
                layout["addresses"]
            ):

                current_value = (
                    read_byte(
                        state_block,
                        address,
                    )
                )


                challenges_remaining = (
                    layout["count"]
                    - (
                        byte_index
                        * 8
                    )
                )


                valid_bits = min(
                    8,
                    challenges_remaining,
                )


                valid_mask = (
                    (1 << valid_bits)
                    - 1
                )


                # Preserve bits outside the actual
                # challenge bitfield.
                preserved_bits = (
                    current_value
                    & (
                        ~valid_mask
                        & 0xFF
                    )
                )


                if merge:

                    desired_valid_bits = (
                        (
                            current_value
                            & valid_mask
                        )
                        |
                        ap_values[
                            byte_index
                        ]
                    )


                else:

                    desired_valid_bits = (
                        ap_values[
                            byte_index
                        ]
                    )


                desired_value = (
                    preserved_bits
                    |
                    desired_valid_bits
                )


                if (
                    desired_value
                    != current_value
                ):

                    writes.append(
                        (
                            address,
                            [desired_value],
                            MEMORY_DOMAIN,
                        )
                    )


        if writes:

            await bizhawk.write(
                ctx.bizhawk_ctx,
                writes,
            )


    # ========================================================
    # ENFORCE AP INVENTORY
    # ========================================================

    async def enforce_inventory(
        self,
        ctx: "BizHawkClientContext",
        state_block: bytes,
    ) -> None:

        received_item_ids = [
            network_item.item

            for network_item
            in ctx.items_received
        ]


        # ----------------------------------------------------
        # Soldiers
        # ----------------------------------------------------

        ap_soldiers = sum(
            1

            for item_id
            in received_item_ids

            if item_id
            == item_table["Soldier"]
        )


        # The game has 200 Soldier progression levels.
        ap_soldiers = min(
            ap_soldiers,
            200,
        )


        writes = []


        current_soldiers = (
            read_byte(
                state_block,
                SOLDIERS_ADDR,
            )
        )


        if (
            current_soldiers
            != ap_soldiers
        ):

            writes.append(
                (
                    SOLDIERS_ADDR,
                    [ap_soldiers],
                    MEMORY_DOMAIN,
                )
            )


        # ----------------------------------------------------
        # Characters
        # ----------------------------------------------------

        received_set = set(
            received_item_ids
        )


        for (
            character,
            address,
        ) in CHARACTER_UNLOCKS.items():

            unlocked = (
                item_table[
                    character
                ]
                in received_set
            )


            desired_value = (
                1
                if unlocked
                else 0
            )


            current_value = (
                read_byte(
                    state_block,
                    address,
                )
            )


            if (
                current_value
                != desired_value
            ):

                writes.append(
                    (
                        address,
                        [desired_value],
                        MEMORY_DOMAIN,
                    )
                )


        if writes:

            await bizhawk.write(
                ctx.bizhawk_ctx,
                writes,
            )


    # ========================================================
    # GOAL
    # ========================================================

    async def check_goal(
        self,
        ctx: "BizHawkClientContext",
        state_block: bytes,
    ) -> None:

        if ctx.finished_game:
            return


        completed_towers = sum(
            1

            for character
            in CHALLENGE_LAYOUT

            if tower_complete(
                state_block,
                character,
            )
        )


        required_towers = int(
            ctx.slot_data.get(
                "goal_towers",
                1,
            )
        )


        if (
            completed_towers
            < required_towers
        ):
            return


        ctx.finished_game = True


        await ctx.send_msgs(
            [
                {
                    "cmd":
                        "StatusUpdate",

                    "status":
                        ClientStatus.CLIENT_GOAL,
                }
            ]
        )


        logger.info(
            f"GOAL COMPLETED: "
            f"{completed_towers}/"
            f"{required_towers} towers."
        )


    # ========================================================
    # GAME WATCHER
    # ========================================================

    async def game_watcher(
        self,
        ctx: "BizHawkClientContext",
    ) -> None:

        # Don't modify RAM until we're authenticated to
        # an actual Toy Story Racer slot.
        if (
            ctx.server is None
            or ctx.server.socket.closed
            or ctx.slot is None
            or ctx.slot_data is None
        ):
            return


        try:

            state_block = (
                await bizhawk.read(
                    ctx.bizhawk_ctx,
                    [
                        (
                            STATE_BASE,
                            STATE_SIZE,
                            MEMORY_DOMAIN,
                        )
                    ],
                )
            )[0]


            # =================================================
            # INITIAL / RECONNECT SYNCHRONIZATION
            # =================================================

            if (
                self.pending_sync_mode
                is not None
            ):

                merge = (
                    self.pending_sync_mode
                    == "merge"
                )


                # Reconnecting to the SAME seed:
                # first send anything completed while offline.
                if merge:

                    local_checks = (
                        self.get_local_checks(
                            state_block
                        )
                    )


                    await ctx.check_locations(
                        local_checks
                    )


                # New seed:
                # overwrite TSR challenge completion with
                # AP's checked_locations.
                #
                # Same seed reconnect:
                # merge AP state with local offline progress.
                await self.sync_challenges(
                    ctx,
                    state_block,
                    merge,
                )


                self.pending_sync_mode = None


                # Re-read after synchronization.
                state_block = (
                    await bizhawk.read(
                        ctx.bizhawk_ctx,
                        [
                            (
                                STATE_BASE,
                                STATE_SIZE,
                                MEMORY_DOMAIN,
                            )
                        ],
                    )
                )[0]


                mode_name = (
                    "merged"
                    if merge
                    else "restored"
                )


                logger.info(
                    f"Challenge state "
                    f"{mode_name} with "
                    f"Archipelago."
                )


            # =================================================
            # LOCATIONS
            # =================================================

            local_checks = (
                self.get_local_checks(
                    state_block
                )
            )


            newly_sent = (
                await ctx.check_locations(
                    local_checks
                )
            )


            if newly_sent:

                for location_id in sorted(
                    newly_sent
                ):

                    location_name = (
                        ctx.location_names.lookup_in_game(
                             location_id,
                            "Toy Story Racer",
                        )
                    )

                    logger.info(
                        f"Location check sent: "
                        f"{location_name}"
                    )


            # =================================================
            # ITEMS
            # =================================================

            await self.enforce_inventory(
                ctx,
                state_block,
            )


            # =================================================
            # GOAL
            # =================================================

            await self.check_goal(
                ctx,
                state_block,
            )


        except (
            bizhawk.RequestFailedError,
            bizhawk.ConnectorError,
            bizhawk.SyncError,
        ):

            # The generic BizHawk client will reconnect and
            # call us again.
            return