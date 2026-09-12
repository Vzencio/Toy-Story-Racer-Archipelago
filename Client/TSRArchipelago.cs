using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Archipelago.Core;

using APLocation =
    Archipelago.Core.Models.Location;


namespace TSRAP.Client;


public sealed class TSRArchipelago :
    IDisposable
{
    private readonly TSRMemory memory;


    private ArchipelagoClient? client;


    private readonly object stateLock =
        new();


    private readonly object locationsLock =
        new();


    private readonly HashSet<string>
        unlockedCharacters =
        new();


    private readonly HashSet<long>
        knownChecked =
        new();


    private readonly Dictionary<
        string,
        byte[]
    >
        previousChallengeState =
        new();


    private CancellationTokenSource?
        cancellation;


    private Task?
        watcherTask;


    private int soldierCount =
        0;


    private int goalTowers =
        1;


    private bool goalSent =
        false;


    // ========================================================
    // CONNECTION / RECOVERY STATE
    // ========================================================

    private string savedHost =
        "";


    private string savedSlot =
        "";


    private string? savedPassword =
        null;


    private int lastProcessId =
        0;


    private ulong lastRamBase =
        0;


    private bool gameAvailable =
        true;


    private bool reconnecting =
        false;


    private DateTime nextReconnectAttempt =
        DateTime.MinValue;


    public int SoldierCount =>
        soldierCount;


    public int GoalTowers =>
        goalTowers;


    public ArchipelagoClient?
        Client =>
        client;


    public TSRArchipelago(
        TSRMemory memory
    )
    {
        this.memory =
            memory;
    }


    // ========================================================
    // INITIAL CONNECT
    // ========================================================

    public async Task<bool>
        ConnectAsync(
            string host,
            string slot,
            string? password
        )
    {
        savedHost =
            host;


        savedSlot =
            slot;


        savedPassword =
            string.IsNullOrWhiteSpace(
                password
            )
                ? null
                : password;


        client =
            new ArchipelagoClient(
                memory.GameClient
            );


        RegisterClientEvents();


        bool connected =
            await ConnectToServerAsync();


        if (!connected)
        {
            return false;
        }


        await SynchronizeEverythingFromServerAsync();


        lastProcessId =
            memory.ProcessId;


        lastRamBase =
            memory.RamBase;


        gameAvailable =
            true;


        StartWatcher();


        return true;
    }


    // ========================================================
    // CLIENT EVENTS
    // ========================================================

    private void RegisterClientEvents()
    {
        if (
            client == null
        )
        {
            return;
        }


        client.Connected +=
            (sender, args) =>
            {
                Console.WriteLine(
                    "Connected to Archipelago."
                );
            };


        client.Disconnected +=
            (sender, args) =>
            {
                Console.WriteLine(
                    "Disconnected from Archipelago."
                );
            };
    }


    // ========================================================
    // SERVER CONNECT / LOGIN
    // ========================================================

    private async Task<bool>
        ConnectToServerAsync()
    {
        if (
            client == null
        )
        {
            return false;
        }


        try
        {
            await client.Connect(
                savedHost,
                "Toy Story Racer"
            );


            if (
                !client.IsConnected
            )
            {
                return false;
            }


            await client.Login(
                savedSlot,
                savedPassword
            );


            if (
                !client.IsLoggedIn
            )
            {
                return false;
            }


            Console.WriteLine(
                $"Logged in as: {savedSlot}"
            );


            return true;
        }
        catch (
            Exception ex
        )
        {
            Console.WriteLine(
                "Archipelago connection error: "
                + ex.Message
            );


            return false;
        }
    }


    // ========================================================
    // FULL SERVER SYNCHRONIZATION
    // ========================================================

    private async Task
        SynchronizeEverythingFromServerAsync()
    {
        await LoadGoalAsync();


        LoadCheckedLocations();


        RestoreChallengesFromServer();


        PrepareItemState();


        RegisterItemHandler();


        await RebuildItemsAsync();


        await RepairDerivedLocationsAsync();


        CheckGoal();


        CaptureChallengeSnapshot();
    }


    // ========================================================
    // AUTOMATIC AP RECONNECTION
    // ========================================================

    private async Task<bool>
        ReconnectArchipelagoAsync()
    {
        if (
            client == null
        )
        {
            return false;
        }


        if (
            client.IsConnected
            && client.IsLoggedIn
        )
        {
            return true;
        }


        if (reconnecting)
        {
            return false;
        }


        if (
            DateTime.UtcNow
            < nextReconnectAttempt
        )
        {
            return false;
        }


        reconnecting =
            true;


        nextReconnectAttempt =
            DateTime.UtcNow
                .AddSeconds(
                    3
                );


        try
        {
            Console.WriteLine(
                "Reconnecting to Archipelago..."
            );


            bool success =
                await ConnectToServerAsync();


            if (!success)
            {
                Console.WriteLine(
                    "Archipelago reconnect failed. "
                    + "Will retry."
                );


                return false;
            }


            Console.WriteLine(
                "Archipelago reconnected."
            );


            await SynchronizeEverythingFromServerAsync();


            Console.WriteLine(
                "Server state synchronized."
            );


            return true;
        }
        catch (
            Exception ex
        )
        {
            Console.WriteLine(
                "Reconnect error: "
                + ex.Message
            );


            return false;
        }
        finally
        {
            reconnecting =
                false;
        }
    }


    // ========================================================
    // GOAL DATA
    // ========================================================

    private async Task
        LoadGoalAsync()
    {
        if (
            client == null
            || !client.IsLoggedIn
        )
        {
            return;
        }


        goalTowers =
            1;


        try
        {
            int currentSlot =
                client.CurrentSession
                    .ConnectionInfo
                    .Slot;


            var slotData =
                await client.CurrentSession
                    .DataStorage
                    .GetSlotDataAsync(
                        currentSlot
                    );


            if (
                slotData.TryGetValue(
                    "goal_towers",
                    out object? goalValue
                )
                && goalValue != null
            )
            {
                if (
                    goalValue
                    is JsonElement json
                )
                {
                    if (
                        json.ValueKind
                        == JsonValueKind.Number
                    )
                    {
                        goalTowers =
                            json.GetInt32();
                    }
                    else
                    {
                        int.TryParse(
                            json.ToString(),
                            out goalTowers
                        );
                    }
                }
                else
                {
                    int.TryParse(
                        goalValue.ToString(),
                        out goalTowers
                    );
                }
            }
        }
        catch
        {
            goalTowers =
                1;
        }


        if (
            goalTowers <= 0
        )
        {
            goalTowers =
                1;
        }


        Console.WriteLine(
            $"Goal: {goalTowers} tower(s)"
        );
    }


    // ========================================================
    // SERVER CHECK STATE
    // ========================================================

    private void
        LoadCheckedLocations()
    {
        if (
            client == null
            || !client.IsLoggedIn
        )
        {
            return;
        }


        lock (
            locationsLock
        )
        {
            knownChecked.Clear();


            foreach (
                long locationId
                in client
                    .CurrentSession
                    .Locations
                    .AllLocationsChecked
            )
            {
                knownChecked.Add(
                    locationId
                );
            }
        }


        Console.WriteLine(
            $"Server already has "
            + $"{knownChecked.Count} checked locations."
        );
    }


    private bool
        IsKnownChecked(
            int locationId
        )
    {
        lock (
            locationsLock
        )
        {
            return
                knownChecked.Contains(
                    locationId
                );
        }
    }


    // ========================================================
    // CHALLENGE RESTORE
    // ========================================================

    private void
        RestoreChallengesFromServer()
    {
        if (
            !memory.IsToyStoryRacerLoaded()
        )
        {
            return;
        }


        Console.WriteLine();
        Console.WriteLine(
            "Restoring challenge state "
            + "from Archipelago..."
        );


        foreach (
            CharacterData character
            in TSRData.Characters
        )
        {
            memory.ClearChallenges(
                character
            );


            for (
                int index = 0;
                index < character.ChallengeCount;
                index++
            )
            {
                int locationId =
                    character.FirstLocationId
                    + index;


                if (
                    !IsKnownChecked(
                        locationId
                    )
                )
                {
                    continue;
                }


                memory.SetChallengeComplete(
                    character,
                    index,
                    true
                );
            }
        }


        Console.WriteLine(
            "Challenge state restored."
        );
    }


    // ========================================================
    // ITEM STATE
    // ========================================================

    private void
        PrepareItemState()
    {
        lock (
            stateLock
        )
        {
            soldierCount =
                0;


            unlockedCharacters.Clear();


            if (
                memory.IsToyStoryRacerLoaded()
            )
            {
                memory.WriteSoldiers(
                    0
                );


                foreach (
                    string character
                    in TSRData
                        .CharacterAddresses
                        .Keys
                )
                {
                    memory.SetCharacterUnlocked(
                        character,
                        false
                    );
                }
            }
        }
    }


    // ========================================================
    // ITEM HANDLER
    // ========================================================

    private void
        RegisterItemHandler()
    {
        if (
            client == null
        )
        {
            return;
        }


        client.ItemManager.ItemReceived +=
            (sender, e) =>
            {
                string itemName =
                    e.Item.Name;


                // ============================================
                // SOLDIER
                // ============================================

                if (
                    itemName
                    == "Soldier"
                )
                {
                    lock (
                        stateLock
                    )
                    {
                        soldierCount++;


                        if (
                            soldierCount > 200
                        )
                        {
                            soldierCount =
                                200;
                        }


                        if (
                            gameAvailable
                            && memory
                                .IsToyStoryRacerLoaded()
                        )
                        {
                            memory.WriteSoldiers(
                                soldierCount
                            );
                        }
                    }


                    Console.WriteLine(
                        $"Received Soldier -> "
                        + $"{soldierCount}"
                    );


                    e.Success =
                        true;


                    return;
                }


                // ============================================
                // CHARACTER
                // ============================================

                if (
                    TSRData
                        .CharacterAddresses
                        .ContainsKey(
                            itemName
                        )
                )
                {
                    lock (
                        stateLock
                    )
                    {
                        unlockedCharacters.Add(
                            itemName
                        );


                        if (
                            gameAvailable
                            && memory
                                .IsToyStoryRacerLoaded()
                        )
                        {
                            memory.SetCharacterUnlocked(
                                itemName,
                                true
                            );
                        }
                    }


                    Console.WriteLine(
                        $"Received character -> "
                        + $"{itemName}"
                    );


                    e.Success =
                        true;


                    return;
                }


                // ============================================
                // NOTHING
                // ============================================

                if (
                    itemName
                    == "Nothing"
                )
                {
                    e.Success =
                        true;


                    return;
                }


                Console.WriteLine(
                    $"Unknown item: {itemName}"
                );


                e.Success =
                    true;
            };
    }


    // ========================================================
    // ITEM REBUILD
    // ========================================================

    private async Task
        RebuildItemsAsync()
    {
        if (
            client == null
            || !client.IsLoggedIn
        )
        {
            return;
        }


        Console.WriteLine();
        Console.WriteLine(
            "Rebuilding received items..."
        );


        await client
            .ItemManager
            .ForceReloadAllItems();


        await client.ReceiveReady();


        Console.WriteLine(
            $"Soldiers restored: "
            + $"{soldierCount}"
        );
    }


    // ========================================================
    // LOCATION SEND
    // ========================================================

    private async Task
        SendLocationAsync(
            int locationId,
            string locationName
        )
    {
        if (
            client == null
            || !client.IsConnected
            || !client.IsLoggedIn
        )
        {
            return;
        }


        lock (
            locationsLock
        )
        {
            if (
                knownChecked.Contains(
                    locationId
                )
            )
            {
                return;
            }


            knownChecked.Add(
                locationId
            );
        }


        try
        {
            await client.SendLocationAsync(
                new APLocation
                {
                    Id =
                        locationId,

                    Name =
                        locationName
                }
            );


            Console.WriteLine(
                $"CHECK -> {locationName}"
            );
        }
        catch (
            Exception ex
        )
        {
            lock (
                locationsLock
            )
            {
                knownChecked.Remove(
                    locationId
                );
            }


            Console.WriteLine(
                $"ERROR sending "
                + $"{locationName}: "
                + ex.Message
            );
        }
    }


    // ========================================================
    // DERIVED CHECKS
    // ========================================================

    private async Task
        ProcessDerivedChecksAsync(
            CharacterData character,
            int challengeIndex
        )
    {
        int typeLocationId =
            character.TypeLocationIds[
                challengeIndex
            ];


        await SendLocationAsync(
            typeLocationId,
            TSRData.TypeNames[
                typeLocationId
            ]
        );


        bool towerComplete =
            true;


        for (
            int i = 0;
            i < character.ChallengeCount;
            i++
        )
        {
            int challengeLocation =
                character.FirstLocationId
                + i;


            if (
                !IsKnownChecked(
                    challengeLocation
                )
            )
            {
                towerComplete =
                    false;

                break;
            }
        }


        if (
            towerComplete
        )
        {
            await SendLocationAsync(
                character.TowerLocationId,
                $"{character.Name} - Tower Complete"
            );
        }
    }


    private async Task
        RepairDerivedLocationsAsync()
    {
        Console.WriteLine();
        Console.WriteLine(
            "Checking derived locations..."
        );


        foreach (
            CharacterData character
            in TSRData.Characters
        )
        {
            for (
                int i = 0;
                i < character.ChallengeCount;
                i++
            )
            {
                int challengeLocation =
                    character.FirstLocationId
                    + i;


                if (
                    IsKnownChecked(
                        challengeLocation
                    )
                )
                {
                    await ProcessDerivedChecksAsync(
                        character,
                        i
                    );
                }
            }
        }
    }


    // ========================================================
    // GOAL
    // ========================================================

    private void
        CheckGoal()
    {
        if (
            goalSent
            || client == null
            || !client.IsConnected
            || !client.IsLoggedIn
        )
        {
            return;
        }


        int completedTowers =
            0;


        foreach (
            CharacterData character
            in TSRData.Characters
        )
        {
            if (
                IsKnownChecked(
                    character.TowerLocationId
                )
            )
            {
                completedTowers++;
            }
        }


        if (
            completedTowers
            >= goalTowers
        )
        {
            Console.WriteLine();
            Console.WriteLine(
                "GOAL COMPLETE!"
            );


            client.SendGoalCompletion();


            goalSent =
                true;
        }
    }


    // ========================================================
    // SNAPSHOT
    // ========================================================

    private void
        CaptureChallengeSnapshot()
    {
        previousChallengeState.Clear();


        if (
            !memory.IsToyStoryRacerLoaded()
        )
        {
            return;
        }


        foreach (
            CharacterData character
            in TSRData.Characters
        )
        {
            previousChallengeState[
                character.Name
            ] =
                memory.ReadChallengeBytes(
                    character
                );
        }
    }


    // ========================================================
    // WATCHER
    // ========================================================

    private void
        StartWatcher()
    {
        cancellation =
            new CancellationTokenSource();


        watcherTask =
            Task.Run(
                () =>
                    WatchLoopAsync(
                        cancellation.Token
                    )
            );
    }


    private async Task
        WatchLoopAsync(
            CancellationToken token
        )
    {
        while (
            !token.IsCancellationRequested
        )
        {
            try
            {
                // ============================================
                // DUCKSTATION PROCESS
                // ============================================

                bool connected =
                    memory.RefreshConnection();


                if (!connected)
                {
                    if (gameAvailable)
                    {
                        Console.WriteLine(
                            "DuckStation connection lost. "
                            + "RAM writes paused."
                        );


                        gameAvailable =
                            false;
                    }


                    await DelaySafely(
                        1000,
                        token
                    );


                    continue;
                }


                // ============================================
                // PROCESS / RAM CHANGED
                // ============================================

                bool processChanged =
                    memory.ProcessId
                    != lastProcessId;


                bool ramChanged =
                    memory.RamBase
                    != lastRamBase;


                if (
                    processChanged
                    || ramChanged
                )
                {
                    Console.WriteLine(
                        "DuckStation instance changed. "
                        + "Reinitializing RAM..."
                    );


                    lastProcessId =
                        memory.ProcessId;


                    lastRamBase =
                        memory.RamBase;


                    gameAvailable =
                        false;
                }


                // ============================================
                // TSR SIGNATURE
                // ============================================

                if (
                    !memory.IsToyStoryRacerLoaded()
                )
                {
                    if (gameAvailable)
                    {
                        Console.WriteLine(
                            "Toy Story Racer is not loaded. "
                            + "RAM writes paused."
                        );


                        gameAvailable =
                            false;
                    }


                    await DelaySafely(
                        500,
                        token
                    );


                    continue;
                }


                // ============================================
                // TSR HAS RETURNED
                // ============================================

                if (!gameAvailable)
                {
                    Console.WriteLine(
                        "Toy Story Racer detected."
                    );


                    // If Archipelago.Core disconnected when
                    // DuckStation disappeared, reconnect first.
                    if (
                        client == null
                        || !client.IsConnected
                        || !client.IsLoggedIn
                    )
                    {
                        bool reconnected =
                            await ReconnectArchipelagoAsync();


                        if (!reconnected)
                        {
                            await DelaySafely(
                                1000,
                                token
                            );


                            continue;
                        }


                        // Full synchronization already happened
                        // inside ReconnectArchipelagoAsync().
                    }
                    else
                    {
                        // AP never disconnected.
                        // Refresh server locations and restore
                        // the current AP state into the new RAM.
                        LoadCheckedLocations();


                        RestoreChallengesFromServer();


                        ForceSoldiers();


                        ForceCharacters();


                        await RepairDerivedLocationsAsync();


                        CaptureChallengeSnapshot();
                    }


                    gameAvailable =
                        true;


                    Console.WriteLine(
                        "Archipelago state restored."
                    );
                }


                // ============================================
                // AP DISCONNECTED WHILE GAME STILL RUNNING
                // ============================================

                if (
                    client == null
                    || !client.IsConnected
                    || !client.IsLoggedIn
                )
                {
                    bool reconnected =
                        await ReconnectArchipelagoAsync();


                    if (!reconnected)
                    {
                        await DelaySafely(
                            1000,
                            token
                        );


                        continue;
                    }


                    gameAvailable =
                        true;
                }


                // ============================================
                // NORMAL SYNC
                // ============================================

                ForceSoldiers();


                ForceCharacters();


                await DetectChallengesAsync();


                CheckGoal();
            }
            catch (
                Exception ex
            )
            {
                Console.WriteLine(
                    "Watcher error: "
                    + ex.Message
                );
            }


            await DelaySafely(
                100,
                token
            );
        }
    }


    // ========================================================
    // SAFE DELAY
    // ========================================================

    private static async Task
        DelaySafely(
            int milliseconds,
            CancellationToken token
        )
    {
        try
        {
            await Task.Delay(
                milliseconds,
                token
            );
        }
        catch (
            OperationCanceledException
        )
        {
        }
    }


    // ========================================================
    // FORCE AP STATE
    // ========================================================

    private void
        ForceSoldiers()
    {
        if (
            !gameAvailable
            || !memory.IsToyStoryRacerLoaded()
        )
        {
            return;
        }


        int expected;


        lock (
            stateLock
        )
        {
            expected =
                soldierCount;
        }


        byte current =
            memory.ReadSoldiers();


        if (
            current
            != expected
        )
        {
            memory.WriteSoldiers(
                expected
            );
        }
    }


    private void
        ForceCharacters()
    {
        if (
            !gameAvailable
            || !memory.IsToyStoryRacerLoaded()
        )
        {
            return;
        }


        foreach (
            string character
            in TSRData
                .CharacterAddresses
                .Keys
        )
        {
            bool unlocked;


            lock (
                stateLock
            )
            {
                unlocked =
                    unlockedCharacters
                        .Contains(
                            character
                        );
            }


            bool current =
                memory.IsCharacterUnlocked(
                    character
                );


            if (
                current
                != unlocked
            )
            {
                memory.SetCharacterUnlocked(
                    character,
                    unlocked
                );
            }
        }
    }


    // ========================================================
    // DETECT NEW CHALLENGES
    // ========================================================

    private async Task
        DetectChallengesAsync()
    {
        if (
            client == null
            || !client.IsConnected
            || !client.IsLoggedIn
            || !gameAvailable
            || !memory.IsToyStoryRacerLoaded()
        )
        {
            return;
        }


        foreach (
            CharacterData character
            in TSRData.Characters
        )
        {
            if (
                !previousChallengeState.TryGetValue(
                    character.Name,
                    out byte[]? previous
                )
                || previous == null
            )
            {
                previousChallengeState[
                    character.Name
                ] =
                    memory.ReadChallengeBytes(
                        character
                    );


                continue;
            }


            for (
                int challengeIndex = 0;
                challengeIndex < character.ChallengeCount;
                challengeIndex++
            )
            {
                int byteIndex =
                    challengeIndex / 8;


                int bitIndex =
                    challengeIndex % 8;


                bool currentComplete =
                    memory.IsChallengeComplete(
                        character,
                        challengeIndex
                    );


                bool previousComplete =
                    (
                        previous[
                            byteIndex
                        ]
                        & (
                            1 << bitIndex
                        )
                    ) != 0;
            

                if (
                    currentComplete
                    && !previousComplete
                )
                {
                    int locationId =
                        character.FirstLocationId
                        + challengeIndex;


                    await SendLocationAsync(
                        locationId,
                        $"{character.Name} - Challenge "
                        + $"{challengeIndex + 1}"
                    );


                    await ProcessDerivedChecksAsync(
                        character,
                        challengeIndex
                    );


                    CheckGoal();
                }


                if (
                    currentComplete
                )
                {
                    previous[
                        byteIndex
                    ] |=
                        (byte)(
                            1 << bitIndex
                        );
                }
                else
                {
                    previous[
                        byteIndex
                    ] &=
                        (byte)~(
                            1 << bitIndex
                        );
                }
            }
        }
    }


    // ========================================================
    // STOP
    // ========================================================

    public async Task
        StopAsync()
    {
        if (
            cancellation != null
        )
        {
            cancellation.Cancel();
        }


        if (
            watcherTask != null
        )
        {
            try
            {
                await watcherTask;
            }
            catch (
                OperationCanceledException
            )
            {
            }
        }


        if (
            client != null
            && client.IsConnected
        )
        {
            client.Disconnect();
        }
    }


    // ========================================================
    // DISPOSE
    // ========================================================

    public void Dispose()
    {
        cancellation?.Cancel();


        client?.Dispose();


        cancellation?.Dispose();
    }
}