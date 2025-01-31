// Configuration du menu de la barre des tâches
menu(type="taskbar" vis=key.shift() pos=0 title=app.name image=\uE249)
{
    item(title="Configuration" image=\uE10A cmd='"@app.cfg"')
    item(title="Gestionnaire" image=\uE0F3 admin cmd='"@app.exe"')
    item(title="Répértoire" image=\uE0E8 cmd='"@app.dir"')
    item(title="Version\t"+@app.ver vis=label col=1)
    item(title="Docs" image=\uE1C4 cmd='https://nilesoft.org/docs')
    item(title="Donner" image=\uE1A7 cmd='https://nilesoft.org/donate')
}

// Menu principal de la barre des tâches
menu(where=@(this.count == 0) type='taskbar' image=icon.settings expanded=true)
{
    // Sous-menu Applications
    menu(title='Apps' image=\uE254)
    {
        // Applications système
        item(title='Paint' image=\uE116 cmd='mspaint')
        item(title='Calculatrice' image=\ue1e7 cmd='calc.exe')
        item(title='Registre' image cmd='regedit.exe')
        
        // Applications installées
        item(vis=key.shift() title='VSCode' image cmd='C:\Users\%username%\AppData\Local\Programs\Microsoft VS Code\Code.exe')
        item(vis=key.shift() title='Cursor' image sep="bottom" cmd='C:\Users\%username%\AppData\Local\Programs\cursor\Cursor.exe')
        item(title='Edge' image cmd='@sys.prog32\Microsoft\Edge\Application\msedge.exe')
    }

    // Sous-menu Affichage
    menu(title='Affichage' image=\uE1FB)
    {
        item(title='En cascade' cmd=command.cascade_windows)
        item(title='Horizontal' cmd=command.Show_windows_stacked)
        item(title="Vértical" cmd=command.Show_windows_side_by_side)
        separator
        item(title='Minimiser' cmd=command.minimize_all_windows)
        item(title='Restaurer' cmd=command.restore_all_windows)
    }

    // Raccourcis système
    item(title=title.desktop image=icon.desktop cmd=command.toggle_desktop)
    item(title=title.settings sep=top image=icon.settings(auto, image.color1) cmd='ms-settings:')
    item(title='Barre des taches' image=inherit cmd='ms-settings:taskbar')
    item(title='Gestionnaire des taches' sep=bottom image=icon.task_manager cmd='taskmgr.exe')

    // Outils personnalisés
    item(title="Eteindre l'écran" image=\uE1FB sep=both cmd='C:\Tools\MonitorOff\MonitorOff.exe')
    item(title="Redemarrer l'explorateur" image=\uE254 cmd='C:\Tools\Rexplorer\Rexplorer.exe')
}
