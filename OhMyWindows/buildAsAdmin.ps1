param (
		[string]$projectPath
	)

	Start-Process "powershell" -ArgumentList "dotnet build $projectPath" -Verb RunAs