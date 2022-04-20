@echo off

set projname="PKHeX_Hunter_Plugin"

rmdir /s /q publish 

cd %projname%
rmdir /s /q bin
rmdir /s /q obj

dotnet publish %projname%.csproj --nologo -c Release -r win-x64 -f net6.0-windows --no-self-contained -o ..\publish

pause