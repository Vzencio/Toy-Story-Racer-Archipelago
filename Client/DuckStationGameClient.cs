using System;
using System.Diagnostics;

using Archipelago.Core.Helpers;


namespace TSRAP.Client;


public sealed class DuckStationGameClient :
    IGameClient
{
    public bool IsConnected
    {
        get;
        set;
    }


    public string ProcessName
    {
        get;
        set;
    } =
        "duckstation";


    public int ProcId
    {
        get
        {
            try
            {
                foreach (
                    Process process
                    in Process.GetProcesses()
                )
                {
                    try
                    {
                        if (
                            process.ProcessName.Contains(
                                "duckstation",
                                StringComparison.OrdinalIgnoreCase
                            )
                            && !process.HasExited
                        )
                        {
                            int pid =
                                process.Id;


                            process.Dispose();


                            return pid;
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch
            {
            }


            return 0;
        }

        set
        {
            // Intentionally ignored.
            // PID is resolved dynamically.
        }
    }


    public bool Connect()
    {
        try
        {
            int pid =
                ProcId;


            if (
                pid == 0
            )
            {
                IsConnected =
                    false;

                return false;
            }


            using Process process =
                Process.GetProcessById(
                    pid
                );


            IsConnected =
                !process.HasExited;


            return IsConnected;
        }
        catch
        {
            IsConnected =
                false;

            return false;
        }
    }
}