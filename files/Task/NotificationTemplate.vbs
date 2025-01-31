' Modifiez ce chemin pour spécifier le chemin du script PowerShell
Dim scriptPath
scriptPath = ""

CreateObject("Wscript.Shell").Run "powershell.exe -ExecutionPolicy Bypass -NoProfile -NoLogo -WindowStyle Hidden -File " & scriptPath, 0
