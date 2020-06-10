@echo off

net session >nul 2>&1
if %errorLevel% == 0 (
    echo Success: Administrator mode confirmed.
) else (
    echo Failure: This command must be run as Administrator mode.
    goto :end
)

:start
set powerShellScriptPath=%~dp0PowerShellScripts\iis-initializeApp.ps1
PowerShell -ExecutionPolicy RemoteSigned -File %powerShellScriptPath%

:end
pause