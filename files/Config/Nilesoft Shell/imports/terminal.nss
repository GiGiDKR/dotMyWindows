menu(type='*' where=(sel.count or wnd.is_taskbar or wnd.is_edit) title=title.terminal sep=sep.top image=icon.run_with_powershell)
{
	$tip_run_admin=["\xE1A7 Press SHIFT key to run " + this.title + " as administrator", tip.warning, 1.0]
	$has_admin=key.shift() or key.rbutton()
	
	//item(title=title.command_prompt tip=tip_run_admin admin=has_admin image cmd-prompt=`/K TITLE Command Prompt &ver& PUSHD "@sel.dir"`)
	item(where=package.exists("WindowsTerminal") title='Terminal' tip=tip_run_admin admin=has_admin image='@package.path("WindowsTerminal")\WindowsTerminal.exe' cmd="wt.exe" arg=`-d "@sel.path\."`)
	item(title='Powershell' admin=has_admin tip=tip_run_admin image cmd-pwsh=`-noexit -command Set-Location -Path '@sel.dir'`)
	item(title='Git Bash' image='C:\Program Files\Git\git-bash.exe' cmd="C:/Program Files/Git/bin/bash.exe" arg="-i -l")
	item(title='WSL' image cmd="wsl")
}