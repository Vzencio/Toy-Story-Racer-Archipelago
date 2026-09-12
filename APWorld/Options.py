from dataclasses import dataclass

from Options import Choice, PerGameCommonOptions


class Goal(Choice):
    """
    Number of character towers that must be completed
    to finish the Archipelago game.
    """

    display_name = "Goal"

    option_one_tower = 1
    option_three_towers = 3
    option_six_towers = 6
    option_all_towers = 12

    default = 1


@dataclass
class ToyStoryRacerOptions(PerGameCommonOptions):
    goal: Goal