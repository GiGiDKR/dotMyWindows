﻿using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using System;
using System.Management;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Security.Principal;

namespace OhMyWindows.ViewModels;

public partial class AcceuilViewModel : ObservableRecipient
{
	private readonly DispatcherQueue dispatcherQueue;
    private readonly DispatcherQueueTimer timer;
    private readonly List<PerformanceCounter> performanceCounters;

	private string _processorName;
	public string ProcessorName
	{
		get => _processorName;
		set => SetProperty(ref _processorName, value);
	}

	private string _processorCores;
	public string ProcessorCores
	{
		get => _processorCores;
		set => SetProperty(ref _processorCores, value);
	}

	private string _totalMemory;
	public string TotalMemory
	{
		get => _totalMemory;
		set => SetProperty(ref _totalMemory, value);
	}

	private string _availableMemory;
	public string AvailableMemory
	{
		get => _availableMemory;
		set => SetProperty(ref _availableMemory, value);
	}

	private string _cpuUsage;
	public string CpuUsage
	{
		get => _cpuUsage;
		set => SetProperty(ref _cpuUsage, value);
	}

	public class DriveInfoWrapper
	{
		public string? Name { get; set; }
		public string? FreeSpace { get; set; }
		public string? TotalSize { get; set; }
		public string? FormattedInfo { get; set; }
        public double DiskUsageValue { get; set; }
	}

	private List<DriveInfoWrapper> _drives;
	public List<DriveInfoWrapper> Drives
	{
		get => _drives;
		set => SetProperty(ref _drives, value);
	}

	private string _networkSpeed;
	public string NetworkSpeed
	{
		get => _networkSpeed;
		set => SetProperty(ref _networkSpeed, value);
	}

	private string _networkInterface;
	public string NetworkInterface
	{
		get => _networkInterface;
		set => SetProperty(ref _networkInterface, value);
	}

	private string _operatingSystem;
	public string OperatingSystem
	{
		get => _operatingSystem;
		set => SetProperty(ref _operatingSystem, value);
	}

	private string _computerName;
	public string ComputerName
	{
		get => _computerName;
		set => SetProperty(ref _computerName, value);
	}

    private string _operatingSystemVersion;
    public string OperatingSystemVersion
    {
        get => _operatingSystemVersion;
        set => SetProperty(ref _operatingSystemVersion, value);
    }

    private string _operatingSystemArchitecture;
    public string OperatingSystemArchitecture
    {
        get => _operatingSystemArchitecture;
        set => SetProperty(ref _operatingSystemArchitecture, value);
    }

    private string _currentDateTime;
    public string CurrentDateTime
    {
        get => _currentDateTime;
        set => SetProperty(ref _currentDateTime, value);
    }

    private string _processorBaseFrequency;
    public string ProcessorBaseFrequency
    {
        get => _processorBaseFrequency;
        set => SetProperty(ref _processorBaseFrequency, value);
    }

    private string _logicalProcessorsCount;
    public string LogicalProcessorsCount
    {
        get => _logicalProcessorsCount;
        set => SetProperty(ref _logicalProcessorsCount, value);
    }

    private string _memoryUsagePercentage;
    public string MemoryUsagePercentage
    {
        get => _memoryUsagePercentage;
        set => SetProperty(ref _memoryUsagePercentage, value);
    }

    private double _memoryUsageValue;
    public double MemoryUsageValue
    {
        get => _memoryUsageValue;
        set => SetProperty(ref _memoryUsageValue, value);
    }

    private double _cpuUsageValue;
    public double CpuUsageValue
    {
        get => _cpuUsageValue;
        set => SetProperty(ref _cpuUsageValue, value);
    }

	public AcceuilViewModel()
	{
		_processorName = string.Empty;
		_processorCores = string.Empty;
		_totalMemory = string.Empty;
		_availableMemory = string.Empty;
		_cpuUsage = string.Empty;
		_drives = new List<DriveInfoWrapper>();
		_networkSpeed = string.Empty;
		_networkInterface = string.Empty;
		_operatingSystem = string.Empty;
		_computerName = string.Empty;
        _operatingSystemVersion = string.Empty;
        _operatingSystemArchitecture = string.Empty;
        _currentDateTime = string.Empty;
        _processorBaseFrequency = string.Empty;
        _logicalProcessorsCount = string.Empty;
        _memoryUsagePercentage = string.Empty;
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

    private void GetProcessorInfo()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                ProcessorBaseFrequency = $"{obj["MaxClockSpeed"]} MHz";
                LogicalProcessorsCount = $"Processeurs Logiques : {obj["NumberOfLogicalProcessors"]?.ToString() ?? "Inconnu"}";
            }
        }
        catch (Exception)
        {
            ProcessorBaseFrequency = "Inconnu";
            LogicalProcessorsCount = "Inconnu";
        }
    }

    private void GetCurrentDateTime()
    {
        CurrentDateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
    }

	private void GetComputerInfo()
	{
		ComputerName = Environment.MachineName;
        OperatingSystemVersion = Environment.OSVersion.VersionString;
        OperatingSystemArchitecture = Environment.Is64BitOperatingSystem ? "x64" : "x86";
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
			GetComputerInfo();
            GetProcessorInfo();
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
			ComputerName = "Inconnu";
            ProcessorBaseFrequency = "Inconnu";
            LogicalProcessorsCount = "Inconnu";

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
                    var diskUsageValue = (double)usedSpace / drive.TotalSize * 100;
					driveList.Add(new DriveInfoWrapper
					{
						Name = $"Disque {drive.Name}",
						FreeSpace = FormatBytes(drive.AvailableFreeSpace),
						TotalSize = FormatBytes(drive.TotalSize),
						FormattedInfo = $"{driveLetter}: = {FormatBytes(usedSpace)}/{FormatBytes(drive.TotalSize)}",
                        DiskUsageValue = diskUsageValue
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

    private void GetMemoryUsagePercentage()
    {
        try
        {
            if (performanceCounters.Count >= 2)
            {
                var totalMemoryInBytes = GetTotalPhysicalMemory();
                var availableMemoryInMB = performanceCounters[1].NextValue();
                var usedMemoryInMB = (totalMemoryInBytes / (1024 * 1024)) - availableMemoryInMB;
                var usagePercentage = (usedMemoryInMB / (totalMemoryInBytes / (1024 * 1024))) * 100;
                MemoryUsagePercentage = $"Utilisation Mémoire : {usagePercentage:F1}%";
                MemoryUsageValue = (double)usagePercentage;
            }
        }
        catch (Exception)
        {
            MemoryUsagePercentage = "Erreur de lecture";
        }
    }

	private void Timer_Tick(DispatcherQueueTimer sender, object args)
	{
		try
		{
            GetCurrentDateTime();
            GetMemoryUsagePercentage();
			if (performanceCounters.Count >= 3)
			{
                var cpuUsage = performanceCounters[0].NextValue();
				CpuUsage = $"Utilisation CPU : {cpuUsage:F1}%";
                CpuUsageValue = (double)cpuUsage;
				AvailableMemory = $"Mémoire Disponible : {performanceCounters[1].NextValue():F0} Mo";
				NetworkSpeed = $"Débit Réseau : {FormatBytes((long)performanceCounters[2].NextValue())}/s";
			}
		}
		catch (Exception)
		{
			CpuUsage = "Erreur de lecture";
			AvailableMemory = "Erreur de lecture";
			NetworkSpeed = "Erreur de lecture";
            CurrentDateTime = "Erreur de lecture";
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
