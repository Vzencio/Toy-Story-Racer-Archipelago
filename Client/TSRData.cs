using System;
using System.Collections.Generic;

namespace TSRAP.Client;


public static class TSRData
{
    // ========================================================
    // BASIC MEMORY
    // ========================================================

    public const ulong SOLDIERS_ADDR =
        0x0D79F4;


    // ========================================================
    // CHARACTER UNLOCK FLAGS
    // ========================================================

    public static readonly
        Dictionary<string, ulong>
        CharacterAddresses =
        new()
        {
            ["LGM"] = 0x0D799C,
            ["Hamm"] = 0x0D79A4,
            ["Mr. Potato Head"] = 0x0D79BC,
            ["Slinky"] = 0x0D79C4,
            ["Rex"] = 0x0D79D4,
            ["Rocky"] = 0x0D79DC,
            ["Babyface"] = 0x0D79E4,
            ["Lenny"] = 0x0D79EC,
        };


    // ========================================================
    // FIRST CHALLENGE TYPE NAMES
    // ========================================================

    public static readonly
        Dictionary<int, string>
        TypeNames =
        new()
        {
            [2500213] = "First Single Race",
            [2500214] = "First Reverse Race",
            [2500215] = "First Smash Race",
            [2500216] = "First Knockout Race",
            [2500217] = "First Tag Race",
            [2500218] = "First Smash Tag Race",
            [2500219] = "First Collection Challenge",
            [2500220] = "First Countdown Race",
            [2500221] = "First Endurance Challenge",
            [2500222] = "First Lap Trial Challenge",
            [2500223] = "First Super Survival Race",
            [2500224] = "First Survival Race",
            [2500225] = "First Target Challenge",
            [2500226] = "First Knockout Race Tournament",
            [2500227] = "First Race Tournament",
            [2500228] = "First Smash Race Tournament",
        };


    // ========================================================
    // CHARACTER / CHALLENGE DATA
    // ========================================================

    public static readonly
        CharacterData[]
        Characters =
        new[]
        {
            new CharacterData(
                "Woody",
                2500001,
                2500201,
                new ulong[]
                {
                    0x0D79A8,
                    0x0D79A9,
                    0x0D79AA
                },
                new int[]
                {
                    2500216, 2500215, 2500227, 2500224,
                    2500218, 2500213, 2500222, 2500219,
                    2500221, 2500214, 2500217, 2500225,
                    2500219, 2500224, 2500228, 2500214,
                    2500217, 2500214, 2500226, 2500223
                }
            ),

            new CharacterData(
                "Buzz",
                2500021,
                2500202,
                new ulong[]
                {
                    0x0D7990,
                    0x0D7991,
                    0x0D7992
                },
                new int[]
                {
                    2500227, 2500213, 2500216, 2500222,
                    2500215, 2500219, 2500224, 2500221,
                    2500217, 2500224, 2500228, 2500214,
                    2500225, 2500218, 2500214, 2500226,
                    2500219, 2500218, 2500214, 2500228,
                    2500227, 2500220
                }
            ),

            new CharacterData(
                "RC",
                2500043,
                2500203,
                new ulong[]
                {
                    0x0D79B0,
                    0x0D79B1,
                    0x0D79B2
                },
                new int[]
                {
                    2500228, 2500213, 2500213, 2500214,
                    2500213, 2500219, 2500222, 2500216,
                    2500224, 2500214, 2500214, 2500227,
                    2500221, 2500214, 2500215, 2500217,
                    2500219, 2500222, 2500218, 2500224,
                    2500220
                }
            ),

            new CharacterData(
                "Bo Peep",
                2500064,
                2500204,
                new ulong[]
                {
                    0x0D79C8,
                    0x0D79C9
                },
                new int[]
                {
                    2500227, 2500213, 2500218, 2500216,
                    2500224, 2500219, 2500215, 2500222,
                    2500221, 2500217, 2500228, 2500219,
                    2500215, 2500214, 2500214, 2500223
                }
            ),

            new CharacterData(
                "Rex",
                2500080,
                2500205,
                new ulong[]
                {
                    0x0D79D0,
                    0x0D79D1,
                    0x0D79D2
                },
                new int[]
                {
                    2500227, 2500214, 2500214, 2500215,
                    2500214, 2500213, 2500214, 2500219,
                    2500224, 2500222, 2500214, 2500225,
                    2500215, 2500213, 2500228, 2500216,
                    2500218, 2500213, 2500220, 2500223
                }
            ),

            new CharacterData(
                "Hamm",
                2500100,
                2500206,
                new ulong[]
                {
                    0x0D79A0,
                    0x0D79A1,
                    0x0D79A2
                },
                new int[]
                {
                    2500227, 2500214, 2500221, 2500227,
                    2500214, 2500215, 2500227, 2500224,
                    2500216, 2500221, 2500214, 2500219,
                    2500223, 2500227, 2500228, 2500213,
                    2500224
                }
            ),

            new CharacterData(
                "Slinky",
                2500117,
                2500207,
                new ulong[]
                {
                    0x0D79C0,
                    0x0D79C1,
                    0x0D79C2
                },
                new int[]
                {
                    2500214, 2500227, 2500213, 2500214,
                    2500214, 2500219, 2500224, 2500221,
                    2500216, 2500227, 2500218, 2500215,
                    2500214, 2500222, 2500217, 2500225,
                    2500220, 2500224, 2500226, 2500214
                }
            ),

            new CharacterData(
                "Mr. Potato Head",
                2500137,
                2500208,
                new ulong[]
                {
                    0x0D79B8,
                    0x0D79B9,
                    0x0D79BA
                },
                new int[]
                {
                    2500227, 2500228, 2500222, 2500214,
                    2500221, 2500214, 2500216, 2500219,
                    2500214, 2500218, 2500213, 2500215,
                    2500216, 2500217, 2500225, 2500214,
                    2500227, 2500217, 2500224
                }
            ),

            new CharacterData(
                "LGM",
                2500156,
                2500209,
                new ulong[]
                {
                    0x0D7998,
                    0x0D7999
                },
                new int[]
                {
                    2500227, 2500228, 2500219, 2500223,
                    2500221, 2500213, 2500222, 2500216,
                    2500218, 2500219, 2500217, 2500227,
                    2500224
                }
            ),

            new CharacterData(
                "Rocky",
                2500169,
                2500210,
                new ulong[]
                {
                    0x0D79D8,
                    0x0D79D9
                },
                new int[]
                {
                    2500227, 2500224, 2500223, 2500227,
                    2500215, 2500222, 2500221, 2500216,
                    2500219, 2500228, 2500214, 2500218,
                    2500219, 2500213, 2500226, 2500224
                }
            ),

            new CharacterData(
                "Lenny",
                2500185,
                2500211,
                new ulong[]
                {
                    0x0D79E8
                },
                new int[]
                {
                    2500225, 2500227, 2500227, 2500216,
                    2500228, 2500216, 2500213, 2500224
                }
            ),

            new CharacterData(
                "Babyface",
                2500193,
                2500212,
                new ulong[]
                {
                    0x0D79E0
                },
                new int[]
                {
                    2500214, 2500225, 2500214, 2500228,
                    2500227, 2500224, 2500223, 2500216
                }
            ),
        };
}


public sealed class CharacterData
{
    public string Name
    {
        get;
    }


    public int FirstLocationId
    {
        get;
    }


    public int TowerLocationId
    {
        get;
    }


    public ulong[] Addresses
    {
        get;
    }


    public int[] TypeLocationIds
    {
        get;
    }


    public int ChallengeCount =>
        TypeLocationIds.Length;


    public CharacterData(
        string name,
        int firstLocationId,
        int towerLocationId,
        ulong[] addresses,
        int[] typeLocationIds
    )
    {
        Name = name;

        FirstLocationId =
            firstLocationId;

        TowerLocationId =
            towerLocationId;

        Addresses =
            addresses;

        TypeLocationIds =
            typeLocationIds;
    }
}