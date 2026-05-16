@echo off
setlocal

set "APPDIR=%LOCALAPPDATA%\DTCDesk"
if not exist "%APPDIR%" mkdir "%APPDIR%"

xcopy "%~dp0*" "%APPDIR%\" /E /I /Y >nul
del "%APPDIR%\install.cmd" >nul 2>nul

powershell -NoProfile -ExecutionPolicy Bypass -Command "$appDir=Join-Path $env:LOCALAPPDATA 'DTCDesk'; $exe=Join-Path $appDir 'DTCDesk.exe'; $desktop=[Environment]::GetFolderPath('DesktopDirectory'); $startMenu=Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs'; $shell=New-Object -ComObject WScript.Shell; foreach($path in @((Join-Path $desktop 'DTCDesk.lnk'), (Join-Path $startMenu 'DTCDesk.lnk'))) { $shortcut=$shell.CreateShortcut($path); $shortcut.TargetPath=$exe; $shortcut.WorkingDirectory=$appDir; $shortcut.IconLocation=$exe; $shortcut.Save() }"

start "" "%APPDIR%\DTCDesk.exe"
endlocal
