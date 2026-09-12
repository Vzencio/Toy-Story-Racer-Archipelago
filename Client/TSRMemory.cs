using System;

using Archipelago.Core.Helpers;
using Archipelago.Core.Util;

using PM =
    Archipelago.Core.Util.PlatformMemory.PlatformMemory;


namespace TSRAP.Client;


public sealed class TSRMemory
{
    
    public bool IsToyStoryRacerLoaded()
{
    try
    {
        byte[] expected =
            System.Text.Encoding.ASCII.GetBytes(
                "SLUS_01214"
            );


        byte[] actual =
            Memory.ReadByteArray(
                0x1FEF80,
                expected.Length
            );


        if (
            actual.Length
            != expected.Length
        )
        {
            return false;
        }


        for (
            int i = 0;
            i < expected.Length;
            i++
        )
        {
            if (
                actual[i]
                != expected[i]
            )
            {
                return false;
            }
        }


        return true;
    }
    catch
    {
        return false;
    }
}

    
    public IGameClient GameClient
{
    get;
}


    public int ProcessId =>
        GameClient.ProcId;


    public ulong RamBase
    {
        get;
        private set;
    }


    public bool IsConnected =>
        GameClient.IsConnected
        && GameClient.ProcId != 0
        && RamBase != 0;


    // ========================================================
    // CONSTRUCTOR
    // ========================================================

    public TSRMemory()
{
    GameClient =
        new DuckStationGameClient();
}


    // ========================================================
    // CONNECT
    // ========================================================

    public bool Connect()
    {
        try
        {
            if (
                !GameClient.Connect()
            )
            {
                RamBase =
                    0;

                return false;
            }


            PM.CurrentProcId =
                GameClient.ProcId;


            ulong newRamBase =
                PM.GetDuckstationOffset();


            if (
                newRamBase == 0
            )
            {
                RamBase =
                    0;

                return false;
            }


            RamBase =
                newRamBase;


            PM.GlobalOffset =
                RamBase;


            return true;
        }
        catch
        {
            RamBase =
                0;

            return false;
        }
    }


    // ========================================================
    // REFRESH CONNECTION
    // ========================================================

    public bool RefreshConnection()
    {
        return Connect();
    }


    // ========================================================
    // BASIC MEMORY OPERATIONS
    // ========================================================

    public byte ReadByte(
        ulong address
    )
    {
        return Memory.ReadByte(
            address
        );
    }


    public void WriteByte(
        ulong address,
        byte value
    )
    {
        Memory.WriteByte(
            address,
            value
        );
    }


    // ========================================================
    // STATE VALIDATION
    // ========================================================

    public bool IsStatePlausible()
    {
        try
        {
            byte soldiers =
                ReadSoldiers();


            if (
                soldiers > 200
            )
            {
                return false;
            }


            foreach (
                string character
                in TSRData.CharacterAddresses.Keys
            )
            {
                ulong address =
                    TSRData.CharacterAddresses[
                        character
                    ];


                byte value =
                    ReadByte(
                        address
                    );


                if (
                    value != 0
                    && value != 1
                )
                {
                    return false;
                }
            }


            return true;
        }
        catch
        {
            return false;
        }
    }


    // ========================================================
    // SOLDIERS
    // ========================================================

    public byte ReadSoldiers()
    {
        return ReadByte(
            TSRData.SOLDIERS_ADDR
        );
    }


    public void WriteSoldiers(
        int soldiers
    )
    {
        soldiers =
            Math.Clamp(
                soldiers,
                0,
                200
            );


        WriteByte(
            TSRData.SOLDIERS_ADDR,
            (byte)soldiers
        );
    }


    // ========================================================
    // CHARACTER FLAGS
    // ========================================================

    public void SetCharacterUnlocked(
        string character,
        bool unlocked
    )
    {
        if (
            !TSRData.CharacterAddresses
                .TryGetValue(
                    character,
                    out ulong address
                )
        )
        {
            return;
        }


        WriteByte(
            address,
            unlocked
                ? (byte)1
                : (byte)0
        );
    }


    public bool IsCharacterUnlocked(
        string character
    )
    {
        if (
            !TSRData.CharacterAddresses
                .TryGetValue(
                    character,
                    out ulong address
                )
        )
        {
            return false;
        }


        return
            ReadByte(
                address
            ) != 0;
    }


    // ========================================================
    // CHALLENGE BITS
    // ========================================================

    public bool IsChallengeComplete(
        CharacterData character,
        int challengeIndex
    )
    {
        int byteIndex =
            challengeIndex / 8;


        int bitIndex =
            challengeIndex % 8;


        byte value =
            ReadByte(
                character.Addresses[
                    byteIndex
                ]
            );


        return
            (
                value
                & (
                    1 << bitIndex
                )
            ) != 0;
    }


    public void SetChallengeComplete(
        CharacterData character,
        int challengeIndex,
        bool complete
    )
    {
        int byteIndex =
            challengeIndex / 8;


        int bitIndex =
            challengeIndex % 8;


        ulong address =
            character.Addresses[
                byteIndex
            ];


        byte value =
            ReadByte(
                address
            );


        if (
            complete
        )
        {
            value |=
                (byte)(
                    1 << bitIndex
                );
        }
        else
        {
            value &=
                (byte)~(
                    1 << bitIndex
                );
        }


        WriteByte(
            address,
            value
        );
    }


    public void ClearChallenges(
        CharacterData character
    )
    {
        foreach (
            ulong address
            in character.Addresses
        )
        {
            WriteByte(
                address,
                0
            );
        }
    }


    public byte[] ReadChallengeBytes(
        CharacterData character
    )
    {
        byte[] result =
            new byte[
                character.Addresses.Length
            ];


        for (
            int i = 0;
            i < character.Addresses.Length;
            i++
        )
        {
            result[i] =
                ReadByte(
                    character.Addresses[i]
                );
        }


        return result;
    }
}