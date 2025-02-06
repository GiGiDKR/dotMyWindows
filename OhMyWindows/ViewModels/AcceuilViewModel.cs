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

    [ObservableProperty]
    private string processorName;

    [ObservableProperty]
    private string processorCores;

    [ObservableProperty]
    private string totalMemory;

    [ObservableProperty]
    private string availableMemory;

    [ObservableProperty]
    private string cpuUsage;

    public class DriveInfoWrapper
    {
        public string Name { get; set; }
        public string FreeSpace { get; set; }
        public string TotalSize { get; set; }
    }

    [ObservableProperty]
    private List<DriveInfoWrapper> drives;

    [ObservableProperty]
    private string networkSpeed;

    [ObservableProperty]
    private string networkInterface;

    [ObservableProperty]
    private string operatingSystem;

    public AcceuilViewModel()
    {
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
            ulong totalMemoryInBytes = GetTotalPhysicalMemory();
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
                    driveList.Add(new DriveInfoWrapper
                    {
                        Name = $"Disque {drive.Name}",
                        FreeSpace = FormatBytes(drive.AvailableFreeSpace),
                        TotalSize = FormatBytes(drive.TotalSize)
                    });
                }
            }
            Drives = driveList;
        }
        catch (Exception)
        {
            Drives = new List<DriveInfoWrapper>();
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
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number = number / 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
}
