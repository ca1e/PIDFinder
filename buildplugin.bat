@echo off
setlocal

dotnet build "%~dp0PKHeX_Hunter_Plugin\PKHeX_Hunter_Plugin.csproj" --nologo -c Release -f net10.0-windows
if errorlevel 1 exit /b %errorlevel%

set "pluginout=%~dp0publish\plugins\PIDFinder"
if not exist "%pluginout%" mkdir "%pluginout%"
copy /y "%~dp0PKHeX_Hunter_Plugin\bin\Release\net10.0-windows\PKHeX_Hunter_Plugin.dll" "%pluginout%\" >nul
if errorlevel 1 exit /b %errorlevel%
copy /y "%~dp0PKHeX_Hunter_Plugin\bin\Release\net10.0-windows\PIDFinder.Lib.dll" "%pluginout%\" >nul
if errorlevel 1 exit /b %errorlevel%
echo Plugin ready: "%pluginout%"
echo Copy the PIDFinder folder into the plugins folder beside PKHeX.exe.
exit /b 0
