from .Challenges import CHALLENGES


LOCATION_ID_BASE = 2500000


# ============================================================
# CHALLENGE TYPES
# ============================================================

CHALLENGE_TYPE_ORDER = [
    "Single Race",
    "Reverse Race",
    "Smash Race",
    "Knockout Race",
    "Tag Race",
    "Smash Tag Race",
    "Collection Challenge",
    "Countdown Race",
    "Endurance Challenge",
    "Lap Trial Challenge",
    "Super Survival Race",
    "Survival Race",
    "Target Challenge",
    "Knockout Race Tournament",
    "Race Tournament",
    "Smash Race Tournament",
]


def canonicalize_challenge_type(raw_type):

    # The game's guide treats Reverse Race as one of the
    # sixteen overall racing types.
    if "(reverse)" in raw_type.lower():
        return "Reverse Race"


    mapping = {

        "Race challenge":
            "Single Race",

        "Smash challenge":
            "Smash Race",

        "Knockout race":
            "Knockout Race",

        "Knockout race challenge":
            "Knockout Race",

        "Tag challenge":
            "Tag Race",

        "Smash tag challenge":
            "Smash Tag Race",

        "Collection challenge":
            "Collection Challenge",

        "Countdown race challenge":
            "Countdown Race",

        "Endurance challenge":
            "Endurance Challenge",

        "Lap trial challenge":
            "Lap Trial Challenge",

        "Super survival race challenge":
            "Super Survival Race",

        "Survival race challenge":
            "Survival Race",

        "Target challenge":
            "Target Challenge",

        "Knockout race tournament challenge":
            "Knockout Race Tournament",

        "Race tournament challenge":
            "Race Tournament",

        "Smash tournament challenge":
            "Smash Race Tournament",
    }


    result = mapping.get(
        raw_type
    )


    if result is None:

        raise ValueError(
            f"Unknown challenge type: "
            f"{raw_type}"
        )


    return result


# ============================================================
# 200 CHALLENGE LOCATIONS
# ============================================================

challenge_location_table = {

    f'{challenge["character"]} - '
    f'{challenge["name"]}':

        LOCATION_ID_BASE + index

    for index, challenge
    in enumerate(
        CHALLENGES,
        start=1,
    )
}


# ============================================================
# 12 TOWER COMPLETE LOCATIONS
# ============================================================

CHARACTERS = [
    "Woody",
    "Buzz",
    "RC",
    "Bo Peep",
    "Rex",
    "Hamm",
    "Slinky",
    "Mr. Potato Head",
    "LGM",
    "Rocky",
    "Lenny",
    "Babyface",
]


tower_location_table = {

    f"{character} - Tower Complete":

        LOCATION_ID_BASE
        + 200
        + index

    for index, character
    in enumerate(
        CHARACTERS,
        start=1,
    )
}


# ============================================================
# 16 FIRST-TYPE LOCATIONS
# ============================================================

type_location_table = {

    f"First {challenge_type}":

        LOCATION_ID_BASE
        + 212
        + index

    for index, challenge_type
    in enumerate(
        CHALLENGE_TYPE_ORDER,
        start=1,
    )
}


# ============================================================
# COMPLETE LOCATION TABLE
# ============================================================

location_table = {
    **challenge_location_table,
    **tower_location_table,
    **type_location_table,
}


# ============================================================
# CHALLENGE LOOKUPS
# ============================================================

location_name_to_challenge = {

    f'{challenge["character"]} - '
    f'{challenge["name"]}':

        challenge

    for challenge
    in CHALLENGES
}


location_key_to_name = {

    (
        challenge["character"],
        challenge["number"],
    ):

        f'{challenge["character"]} - '
        f'{challenge["name"]}'

    for challenge
    in CHALLENGES
}


challenge_key_to_data = {

    (
        challenge["character"],
        challenge["number"],
    ):

        challenge

    for challenge
    in CHALLENGES
}


# ============================================================
# TOWER LOOKUP
# ============================================================

tower_key_to_name = {

    character:
        f"{character} - Tower Complete"

    for character
    in CHARACTERS
}


# ============================================================
# TYPE LOOKUP
# ============================================================

type_key_to_name = {

    challenge_type:
        f"First {challenge_type}"

    for challenge_type
    in CHALLENGE_TYPE_ORDER
}