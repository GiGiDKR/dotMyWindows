﻿using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using System;
using System.Management;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

namespace OhMyWindows.ViewModels;

public partial class AcceuilViewModel : ObservableRecipient
{
    private readonly DispatcherQueue dispatcherQueue;
    private readonly DispatcherQueueTimer timer;
    private readonly List<PerformanceCounter> performanceCounters;

    private string processorName;
    public string ProcessorName
    {
        get => processorName;
        set => SetProperty(ref processorName, value);
    }

    private string processorCores;
    public string ProcessorCores
    {
        get => processorCores;
        set => SetProperty(ref processorCores, value);
    }

    private string totalMemory;
    public string TotalMemory
    {
        get => totalMemory;
        set => SetProperty(ref totalMemory, value);
    }

    private string availableMemory;
    public string AvailableMemory
    {
        get => availableMemory;
        set => SetProperty(ref availableMemory, value);
    }

    private string cpuUsage;
    public string CpuUsage
    {
        get => cpuUsage;
        set => SetProperty(ref cpuUsage, value);
    }

    public class DriveInfoWrapper
    {
        public string? Name { get; set; }
        public string? FreeSpace { get; set; }
        public string? TotalSize { get; set; }
        public string? FormattedInfo { get; set; }
    }

    private List<DriveInfoWrapper> drives;
    public List<DriveInfoWrapper> Drives
    {
        get => drives;
        set => SetProperty(ref drives, value);
    }

    private string networkSpeed;
    public string NetworkSpeed
    {
        get => networkSpeed;
        set => SetProperty(ref networkSpeed, value);
    }

    private string networkInterface;
    public string NetworkInterface
    {
        get => networkInterface;
        set => SetProperty(ref networkInterface, value);
    }

    private string operatingSystem;
    public string OperatingSystem
    {
        get => operatingSystem;
        set => SetProperty(ref operatingSystem, value);
    }

    public AcceuilViewModel()
    {
        processorName = string.Empty;
        processorCores = string.Empty;
        totalMemory = string.Empty;
        availableMemory = string.Empty;
        cpuUsage = string.Empty;
        drives = new List<DriveInfoWrapper>();
        networkSpeed = string.Empty;
        networkInterface = string.Empty;
        operatingSystem = string.Empty;
        dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        performanceCounters = new List<PerformanceCounter>();
        InitializePerformanceCounters();
        
        timer = dispatcherQueue.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += Timer_Tick;
        
        GetStaticSystemInfo();
        GetDriveInfo();
        
        timer.Start();
    }

    private void InitializePerformanceCounters()
    {
        try
        {
            performanceCounters.Add(new PerformanceCounter("Processor", "% Processor Time", "_Total"));
            performanceCounters.Add(new PerformanceCounter("Memory", "Available MBytes"));
            performanceCounters.Add(new PerformanceCounter("Network Interface", "Bytes Total/sec", GetMainNetworkInterface()));
        }
        catch (Exception)
        {
            // Gérer les erreurs d'initialisation des compteurs
        }
    }

    private string GetMainNetworkInterface()
    {
        try
        {
            var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            foreach (var ni in interfaces)
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || 
                     ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
                {
                    NetworkInterface = ni.Description;
                    return ni.Description;
                }
            }
        }
        catch (Exception)
        {
            NetworkInterface = "Interface réseau inconnue";
        }
        return "_Total";
    }

    private void GetStaticSystemInfo()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                ProcessorName = obj["Name"]?.ToString() ?? "Inconnu";
                ProcessorCores = $"Cœurs : {obj["NumberOfCores"]?.ToString() ?? "Inconnu"}";
                break;
            }

            OperatingSystem = RuntimeInformation.OSDescription;
            var totalMemoryInBytes = GetTotalPhysicalMemory();
            TotalMemory = $"Mémoire Totale : {FormatBytes((long)(totalMemoryInBytes & 0x7FFFFFFFFFFFFFFF))}";
        }
        catch (Exception)
        {
            ProcessorName = "Impossible d'obtenir les informations du processeur";
            ProcessorCores = "Inconnu";
            OperatingSystem = "Impossible d'obtenir les informations du système";
            TotalMemory = "Impossible d'obtenir les informations de mémoire";
        }
    }

    private void GetDriveInfo()
    {
        try
        {
            var driveList = new List<DriveInfoWrapper>();
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    var usedSpace = drive.TotalSize - drive.AvailableFreeSpace;
                    var driveLetter = drive.Name.TrimEnd('\\', ':');
                    driveList.Add(new DriveInfoWrapper
                    {
                        Name = $"Disque {drive.Name}",
                        FreeSpace = FormatBytes(drive.AvailableFreeSpace),
                        TotalSize = FormatBytes(drive.TotalSize),
                        FormattedInfo = $"{driveLetter}: = {FormatBytes(usedSpace)}/{FormatBytes(drive.TotalSize)}"
                    });
                }
            }
            Drives = driveList;
        }
        catch (Exception)
        {
            Drives = [];
        }
    }

    private void Timer_Tick(DispatcherQueueTimer sender, object args)
    {
        try
        {
            if (performanceCounters.Count >= 3)
            {
                CpuUsage = $"Utilisation CPU : {performanceCounters[0].NextValue():F1}%";
                AvailableMemory = $"Mémoire Disponible : {performanceCounters[1].NextValue():F0} Mo";
                NetworkSpeed = $"Débit Réseau : {FormatBytes((long)performanceCounters[2].NextValue())}/s";
            }
        }
        catch (Exception)
        {
            CpuUsage = "Erreur de lecture";
            AvailableMemory = "Erreur de lecture";
            NetworkSpeed = "Erreur de lecture";
        }
    }

    private ulong GetTotalPhysicalMemory()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                return Convert.ToUInt64(obj["TotalPhysicalMemory"]);
            }
        }
        catch
        {
            return 0;
        }
        return 0;
    }

    private string FormatBytes(long bytes)
    {
        string[] suffixes = { "o", "Ko", "Mo", "Go", "To" };
        var counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
}
