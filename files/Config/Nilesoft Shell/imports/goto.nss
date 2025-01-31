menu(type='*' where=window.is_taskbar||sel.count mode=mode.multiple title=title.go_to sep=sep.both image=\uE14A)
{
	menu(title='Repertoire' image=\uE1F4)
	{
		item(title='Windows' image=inherit cmd=sys.dir)
		item(title='System' image=inherit cmd=sys.bin)
		item(title='Program Files' image=inherit cmd=sys.prog)
		item(title='Program Files x86' image=inherit cmd=sys.prog32)
		item(title='ProgramData' image=inherit cmd=sys.programdata)
		item(title='Applications' image=inherit cmd='shell:appsfolder')
		item(title='Users' image=inherit cmd=sys.users)
		separator
		//item(title='@user.name@@@sys.name' vis=label)
		item(title='Desktop' image=inherit cmd=user.desktop)
		item(title='Downloads' image=inherit cmd=user.downloads)
		item(title='Pictures' image=inherit cmd=user.pictures)
		item(title='Documents' image=inherit cmd=user.documents)
		item(title='Startmenu' image=inherit cmd=user.startmenu)
		item(title='Profile' image=inherit cmd=user.dir)
		item(title='AppData' image=inherit cmd=user.appdata)
		item(title='Temp' image=inherit cmd=user.temp)
	}
	item(title=title.control_panel image=\uE0F3 cmd='shell:::{5399E694-6CE5-4D6C-8FCE-1D8870FDCBA0}')
	item(title='God Mode' image=\uE0F3 cmd='shell:::{ED7BA470-8E54-465E-825C-99712043E01C}')
	item(title=title.run image=\uE14B cmd='shell:::{2559a1f3-21d7-11d4-bdaf-00c04f60b9f0}')
	menu(where=sys.ver.major >= 10 title=title.settings sep=sep.before image=\uE0F3)
	{
		// https://docs.microsoft.com/en-us/windows/uwp/launch-resume/launch-settings-app
		item(title='Systeme' image=inherit cmd='ms-settings:')
		item(title='A propos' image=inherit cmd='ms-settings:about')
		item(title='Compte' image=inherit cmd='ms-settings:yourinfo')
		item(title='Informations système' image=inherit cmd-line='/K systeminfo')
		item(title='Recherche' cmd='search-ms:' image=inherit)
		item(title='USB' image=inherit cmd='ms-settings:usb')
		item(title='Mise à jour' image=inherit cmd='ms-settings:windowsupdate')
		item(title='Windows Defender' image=inherit cmd='ms-settings:windowsdefender')
		menu(title='Applications' image=inherit)
		{
			item(title='Applications installées' image=inherit cmd='ms-settings:appsfeatures')
			item(title='Applications par défaut' image=inherit cmd='ms-settings:defaultapps')
			item(title='Fonctionnalités facultatives' image=inherit cmd='ms-settings:optionalfeatures')
			item(title='Démarrage' image=inherit cmd='ms-settings:startupapps')
		}
		menu(title='Personalisation' image=inherit)
		{
			item(title='Personalisation' image=inherit cmd='ms-settings:personalization')
			item(title='Ecran de verrouillage' image=inherit cmd='ms-settings:lockscreen')
			item(title='Fond écran' image=inherit cmd='ms-settings:personalization-background')
			item(title='Couleurs' image=inherit cmd='ms-settings:colors')
			item(title='Themes' image=inherit cmd='ms-settings:themes')
			item(title='Menu démarrer' image=inherit cmd='ms-settings:personalization-start')
			item(title='Barre des taches' image=inherit cmd='ms-settings:taskbar')
		}
		menu(title='Reseau' image=inherit)
		{
			item(title='Status' image=inherit cmd='ms-settings:network-status')
			item(title='Ethernet' image=inherit cmd='ms-settings:network-ethernet')
			item(title='Connexions' image=inherit cmd='shell:::{7007ACC7-3202-11D1-AAD2-00805FC1270E}')
		}
	}
}