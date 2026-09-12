using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Media;
using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Archipelago.MultiClient.Net.MessageLog.Messages;

using TSRAP.Client;


namespace TSRAP.GUI.ViewModels;


// ============================================================
// COLORED LOG PART
// ============================================================

public sealed class LogPartViewModel
{
    public string Text
    {
        get;
    }


    public IBrush Foreground
    {
        get;
    }


    public LogPartViewModel(
        string text,
        IBrush foreground
    )
    {
        Text =
            text;


        Foreground =
            foreground;
    }
}


// ============================================================
// LOG LINE
// ============================================================

public sealed class LogLineViewModel
{
    public ObservableCollection<LogPartViewModel>
        Parts
    {
        get;
    } =
        new();


    public LogLineViewModel()
    {
    }


    public LogLineViewModel(
        string text,
        IBrush color
    )
    {
        Parts.Add(
            new LogPartViewModel(
                text,
                color
            )
        );
    }
}


// ============================================================
// MAIN VIEW MODEL
// ============================================================

public partial class MainViewModel :
    ViewModelBase
{
    private TSRMemory?
        memory;


    private TSRArchipelago?
        archipelago;


    private DispatcherTimer?
        statusTimer;


    private bool
        messageHandlerRegistered =
        false;


    // ========================================================
    // STATUS
    // ========================================================

    [ObservableProperty]
    private string duckStationStatus =
        "Disconnected";


    [ObservableProperty]
    private string gameStatus =
        "Not detected";


    [ObservableProperty]
    private string archipelagoStatus =
        "Disconnected";


    // ========================================================
    // CONNECTION INPUT
    // ========================================================

    [ObservableProperty]
    private string server =
        "";


    [ObservableProperty]
    private string slot =
        "";


    [ObservableProperty]
    private string password =
        "";


    // ========================================================
    // GAME STATE
    // ========================================================

    [ObservableProperty]
    private int soldiers =
        0;


    [ObservableProperty]
    private int checks =
        0;


    [ObservableProperty]
    private int goalTowers =
        1;


    // ========================================================
    // UI STATE
    // ========================================================

    [ObservableProperty]
    private bool isConnecting =
        false;


    [ObservableProperty]
    private string commandText =
        "";


    public ObservableCollection<LogLineViewModel>
        LogLines
    {
        get;
    } =
        new();


    public ObservableCollection<LogLineViewModel>
        HintLines
    {
        get;
    } =
        new();


    // ========================================================
    // CONNECT
    // ========================================================

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (
    IsConnecting
    || archipelago != null
)
{
    return;
}


        IsConnecting =
            true;


        try
        {
            AddClientLog(
                "Connecting to DuckStation..."
            );


            memory =
                new TSRMemory();


            if (
                !memory.Connect()
            )
            {
                DuckStationStatus =
                    "Disconnected";


                GameStatus =
                    "Not detected";


                AddErrorLog(
                    "DuckStation was not found."
                );


                return;
            }


            DuckStationStatus =
                "Connected";


            AddClientLog(
                $"DuckStation detected. PID: "
                + $"{memory.ProcessId}"
            );


            // ================================================
            // TSR
            // ================================================

            if (
                !memory.IsToyStoryRacerLoaded()
            )
            {
                GameStatus =
                    "Not detected";


                AddErrorLog(
                    "Toy Story Racer "
                    + "SLUS-01214 is not loaded."
                );


                return;
            }


            GameStatus =
                "Detected";


            AddClientLog(
                "Toy Story Racer detected."
            );


            // ================================================
            // INPUT
            // ================================================

            if (
                string.IsNullOrWhiteSpace(
                    Server
                )
                || string.IsNullOrWhiteSpace(
                    Slot
                )
            )
            {
                AddErrorLog(
                    "Server and Slot are required."
                );


                return;
            }


            // ================================================
            // ARCHIPELAGO
            // ================================================

            ArchipelagoStatus =
                "Connecting";


            AddClientLog(
                "Connecting to Archipelago..."
            );
            Password =
    "";


            archipelago =
                new TSRArchipelago(
                    memory
                );


            bool connected =
                await archipelago.ConnectAsync(
                    Server.Trim(),
                    Slot.Trim(),
                    Password
                );


            if (
                !connected
            )
            {
                ArchipelagoStatus =
                    "Disconnected";


                AddErrorLog(
                    "Could not connect "
                    + "to Archipelago."
                );


                return;
            }


            RegisterMessageHandler();


            ArchipelagoStatus =
                "Connected";


            Soldiers =
                archipelago.SoldierCount;


            GoalTowers =
                archipelago.GoalTowers;


            UpdateChecks();


            StartStatusTimer();


            AddClientLog(
                "Connected to Archipelago."
            );
        }
        catch (
            Exception ex
        )
        {
            ArchipelagoStatus =
                "Disconnected";


            AddErrorLog(
                ex.Message
            );
        }
        finally
        {
            IsConnecting =
                false;
        }
    }


    // ========================================================
    // ARCHIPELAGO MESSAGE HANDLER
    // ========================================================

    private void RegisterMessageHandler()
    {
        if (
            archipelago?.Client == null
            || messageHandlerRegistered
        )
        {
            return;
        }


        archipelago.Client.MessageReceived +=
            (sender, e) =>
            {
                Dispatcher.UIThread.Post(
                    () =>
                    {
                        AddArchipelagoMessage(
                            e.Message
                        );
                    }
                );
            };


        messageHandlerRegistered =
            true;
    }


    // ========================================================
    // ARCHIPELAGO MESSAGE
    // ========================================================

    private void AddArchipelagoMessage(
        LogMessage message
    )
    {
        LogLineViewModel line =
            ConvertMessage(
                message
            );


        // Every AP message still appears in the main log.
        LogLines.Add(
            line
        );


        // Relevant hints ALSO appear in the dedicated Hints tab.
        if (
            message
            is HintItemSendLogMessage hint
            && hint.IsRelatedToActivePlayer
        )
        {
            HintLines.Add(
                ConvertMessage(
                    message
                )
            );
        }


        TrimLogs();
    }


    // ========================================================
    // CONVERT AP COLORS DIRECTLY TO AVALONIA
    // ========================================================

    private static LogLineViewModel
        ConvertMessage(
            LogMessage message
        )
    {
        LogLineViewModel line =
            new();


        foreach (
            var part
            in message.Parts
        )
        {
            var apColor =
                part.Color;


            Color avaloniaColor =
                Color.FromRgb(
                    apColor.R,
                    apColor.G,
                    apColor.B
                );


            line.Parts.Add(
                new LogPartViewModel(
                    part.Text,
                    new SolidColorBrush(
                        avaloniaColor
                    )
                )
            );
        }


        return line;
    }


    // ========================================================
    // SEND CHAT / COMMAND
    // ========================================================

    [RelayCommand]
    private async Task SendCommandAsync()
    {
        string text =
            CommandText.Trim();


        if (
            string.IsNullOrWhiteSpace(
                text
            )
        )
        {
            return;
        }


        if (
            archipelago?.Client == null
            || !archipelago.Client.IsConnected
            || !archipelago.Client.IsLoggedIn
        )
        {
            AddErrorLog(
                "Not connected to Archipelago."
            );


            return;
        }


        try
        {
            CommandText =
                "";


            await archipelago.Client
                .SendMessage(
                    text
                );
        }
        catch (
            Exception ex
        )
        {
            AddErrorLog(
                "Could not send message: "
                + ex.Message
            );
        }
    }


    // ========================================================
    // STATUS TIMER
    // ========================================================

    private void StartStatusTimer()
    {
        if (
            statusTimer != null
        )
        {
            return;
        }


        statusTimer =
            new DispatcherTimer
            {
                Interval =
                    TimeSpan.FromMilliseconds(
                        250
                    )
            };


        statusTimer.Tick +=
            (sender, args) =>
            {
                UpdateLiveState();
            };


        statusTimer.Start();
    }


    private void UpdateLiveState()
    {
        // ================================================
        // DUCKSTATION
        // ================================================

        if (
            memory != null
            && memory.IsConnected
        )
        {
            DuckStationStatus =
                "Connected";
        }
        else
        {
            DuckStationStatus =
                "Disconnected";
        }


        // ================================================
        // TSR
        // ================================================

        if (
            memory != null
            && memory.IsConnected
            && memory.IsToyStoryRacerLoaded()
        )
        {
            GameStatus =
                "Detected";
        }
        else
        {
            GameStatus =
                "Not detected";
        }


        // ================================================
        // ARCHIPELAGO
        // ================================================

        if (
            archipelago?.Client != null
            && archipelago.Client.IsConnected
            && archipelago.Client.IsLoggedIn
        )
        {
            ArchipelagoStatus =
                "Connected";
        }
        else
        {
            ArchipelagoStatus =
                "Disconnected";
        }


        // ================================================
        // GAME VALUES
        // ================================================

        if (
            archipelago != null
        )
        {
            Soldiers =
                archipelago.SoldierCount;


            GoalTowers =
                archipelago.GoalTowers;
        }


        UpdateChecks();
    }


    // ========================================================
    // CHECK COUNT
    // ========================================================

    private void UpdateChecks()
    {
        try
        {
            if (
                archipelago?.Client == null
                || !archipelago.Client.IsLoggedIn
            )
            {
                return;
            }


            Checks =
                archipelago.Client
                    .CurrentSession
                    .Locations
                    .AllLocationsChecked
                    .Count;
        }
        catch
        {
        }
    }


    // ========================================================
    // LOCAL LOGS
    // ========================================================

    private void AddClientLog(
        string message
    )
    {
        AddLocalLine(
            "[CLIENT] "
            + message,
            Brushes.LightGray
        );
    }


    private void AddErrorLog(
        string message
    )
    {
        AddLocalLine(
            "[ERROR] "
            + message,
            Brushes.IndianRed
        );
    }


    private void AddLocalLine(
        string message,
        IBrush color
    )
    {
        Dispatcher.UIThread.Post(
            () =>
            {
                LogLines.Add(
                    new LogLineViewModel(
                        $"[{DateTime.Now:HH:mm:ss}] "
                        + message,
                        color
                    )
                );


                TrimLogs();
            }
        );
    }


    // ========================================================
    // KEEP MEMORY UNDER CONTROL
    // ========================================================

    private void TrimLogs()
    {
        const int maxLogLines =
            1000;


        const int maxHintLines =
            300;


        while (
            LogLines.Count
            > maxLogLines
        )
        {
            LogLines.RemoveAt(
                0
            );
        }


        while (
            HintLines.Count
            > maxHintLines
        )
        {
            HintLines.RemoveAt(
                0
            );
        }
    }
    public async Task ShutdownAsync()
{
    statusTimer?.Stop();


    if (
        archipelago != null
    )
    {
        try
        {
            await archipelago.StopAsync();
        }
        catch
        {
        }


        archipelago.Dispose();

        archipelago =
            null;
    }
}
}