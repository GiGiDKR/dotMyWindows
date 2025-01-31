menu(mode="multiple" title='Ouvrir' pos="middle" image=\uE0A4)
{
}

// modify items
// Remove items by identifiers
modify(mode=mode.multiple
	where=this.id(id.restore_previous_versions,id.cast_to_device)
	vis=vis.remove)

modify(type="recyclebin" where=window.is_desktop and this.id==id.empty_recycle_bin pos=1 sep)

modify(find="unpin*" pos="bottom" menu="Pin/Unpin")
modify(find="pin*" pos="top" menu="Pin/Unpin")

modify(where=this.id==id.copy_as_path menu="file manage")
modify(type="dir.back|drive.back" where=this.id==id.customize_this_folder pos=1 sep="top" menu="file manage")

modify(where=str.equals(this.name, ["open in terminal", "open linux shell here"]) || this.id==id.open_powershell_window_here
	pos="bottom" menu="Terminal")

modify(mode=mode.multiple
	where=this.id(
		id.send_to,
		id.share,
		id.create_shortcut,
		id.set_as_desktop_background,
		id.rotate_left,
		id.rotate_right,
		id.map_network_drive,
		id.disconnect_network_drive,
		id.format,
		id.eject,
		id.give_access_to,
		id.include_in_library,
		id.print
	)
	pos="bottom" menu=title.more_options)

modify(find='WizTree' pos=1  menu=title.more_options)
modify(find='Browse with Sononym' pos=2 menu=title.more_options)
modify(find='PowerRename' pos=3 sep="bottom" menu=title.more_options)
modify(find='File Locksmith' pos=4 menu=title.more_options)
modify(find='Take Ownership' pos=5 sep="bottom" menu=title.more_options)
modify(find='Exécuter avec le processeur graphique' pos=8 sep="bottom" menu=title.more_options)
modify(find='compatibilité' pos=9 menu=title.more_options)
modify(find='Synchroniser' pos=10 menu=title.more_options)
modify(find='Glary Utilities' pos=11 menu=title.more_options)
modify(find='Microsoft Defender' pos=12 sep="top" menu=title.more_options)
modify(find='Inclure dans la bibliothèque' pos="bottom" menu=title.more_options)
modify(find='Ouvrir' menu="Ouvrir")
modify(find='Modifier' menu="Ouvrir")
modify(find='lecteur multimédia' menu="Ouvrir")
modify(find='liste de lecture de VLC' sep="top" menu="Ouvrir")
modify(find='Lire avec VLC' sep="bottom" menu="Ouvrir")
modify(find='Ajouter au Centre' pos=5 menu="Envoyer vers")
modify(find='Copier dans' pos=1 sep="top" menu="Gestion fichiers")
modify(find='Déplacer' pos=2 sep="bottom" menu="Gestion fichiers")
modify(find='Ajouter aux Favoris' pos="3"sep="bottom" menu="Gestion fichiers")
