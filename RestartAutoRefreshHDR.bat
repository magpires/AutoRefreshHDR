@echo off

taskkill /IM AutoRefreshHDR.exe /F
echo Restarting AutoRefreshHDR...
timeout /t 2 >nul
start "" "AutoRefreshHDR.exe"
exit