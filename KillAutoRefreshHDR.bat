@echo off

echo Killing AutoRefreshHDR...
taskkill /IM AutoRefreshHDR.exe /F
timeout /t 2 >nul
exit