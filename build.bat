@echo off

rmdir /s /q publish 

cd PIDFinder
rmdir /s /q bin
rmdir /s /q obj

dotnet publish PIDFinder.csproj --nologo -c Release -r win-x64 -f net6.0-windows -p:PublishSingleFile=true --no-self-contained -o ..\publish

pause