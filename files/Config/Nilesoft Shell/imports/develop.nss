menu(mode="multiple" title='&Developpement' pos="middle" sep=sep.bottom image=\uE26E)
{
	menu(mode="single" title='Editeurs' image=\uE17A)
	{
		item(title='VSCode' image=[\uE272, #22A7F2] cmd='code' args='"@sel.path"')
		item(title='Curosr' image cmd='C:\Users\%username%\AppData\Local\Programs\cursor\Cursor.exe' args='"@sel.path"')
		item(title='Windsurf' image cmd='C:\Users\Gildas\AppData\Local\Programs\Windsurf\Windsurf.exe' args='"@sel.path"')
		separator
		item(type='file' mode="single" title='Notepad' image cmd='notepad' args='"@sel.path"')
	}

	menu(mode="multiple" title='dotnet' image=\uE143)
	{
		item(title='run' cmd-line='/K dotnet run' image=\uE149)
		item(title='watch' cmd-line='/K dotnet watch')
		item(title='clean' image=\uE0CE cmd-line='/K dotnet clean')
		separator
		item(title='build debug' cmd-line='/K dotnet build')
		item(title='build release' cmd-line='/K dotnet build -c release /p:DebugType=None')

		menu(mode="multiple" sep=sep.both title='publish' image=\ue11f)
		{
			$publish='dotnet publish -r win-x64 -c release --output publish /*/p:CopyOutputSymbolsToPublishDirectory=false*/'
			item(title='publish sinale file' sep=sep.bottom cmd-line='/K @publish --no-self-contained /p:PublishSingleFile=true')
			item(title='framework-dependent deployment' cmd-line='/K @publish')
			item(title='framework-dependent executable' cmd-line='/K @publish --self-contained false')
			item(title='self-contained deployment' cmd-line='/K @publish --self-contained true')
			item(title='single-file' cmd-line='/K @publish /p:PublishSingleFile=true /p:PublishTrimmed=false')
			item(title='single-file-trimmed' cmd-line='/K @publish /p:PublishSingleFile=true /p:PublishTrimmed=true')
		}
		
		item(title='ef migrations add InitialCreate' cmd-line='/K dotnet ef migrations add InitialCreate')
		item(title='ef database update' cmd-line='/K dotnet ef database update')
		separator
		item(title='help' image=\uE136 cmd-line='/k dotnet -h')
		item(title='version' cmd-line='/k dotnet --info')
	}
}