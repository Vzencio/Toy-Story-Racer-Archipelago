from BaseClasses import ItemClassification


ITEM_ID_BASE = 2500000


item_table = {
    "Soldier": ITEM_ID_BASE + 1,

    "Rex": ITEM_ID_BASE + 2,
    "Hamm": ITEM_ID_BASE + 3,
    "Slinky": ITEM_ID_BASE + 4,
    "Mr. Potato Head": ITEM_ID_BASE + 5,
    "LGM": ITEM_ID_BASE + 6,
    "Rocky": ITEM_ID_BASE + 7,
    "Lenny": ITEM_ID_BASE + 8,
    "Babyface": ITEM_ID_BASE + 9,

    "Nothing": ITEM_ID_BASE + 10,
}


item_classification = {
    "Rex": ItemClassification.progression,
    "Hamm": ItemClassification.progression,
    "Slinky": ItemClassification.progression,
    "Mr. Potato Head": ItemClassification.progression,
    "LGM": ItemClassification.progression,
    "Rocky": ItemClassification.progression,
    "Lenny": ItemClassification.progression,
    "Babyface": ItemClassification.progression,

    "Nothing": ItemClassification.filler,
}