@echo off
setlocal
pushd "%~dp0"
dotnet publish "PIDFinder\PIDFinder.csproj" --nologo -c Release -r win-x64 -f net10.0-windows -p:PublishSingleFile=true --no-self-contained -o "publish\standalone"
set "buildResult=%errorlevel%"
popd
exit /b %buildResult%
