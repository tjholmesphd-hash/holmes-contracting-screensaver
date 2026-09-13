@echo off
setlocal
set DEST=%LOCALAPPDATA%\OmarchyScreensaver
mkdir "%DEST%" >nul 2>&1
copy /Y "%~dp0screensaver.html" "%DEST%\screensaver.html" >nul
copy /Y "%~dp0OmarchyScreensaver.cs" "%DEST%\OmarchyScreensaver.cs" >nul
set CSC=
if exist "%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if exist "%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe" if "%CSC%"=="" set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
if "%CSC%"=="" (
  echo Could not find the .NET C# compiler. Windows 10/11 should include it.
  pause
  exit /b 1
)
"%CSC%" /nologo /target:winexe /out:"%DEST%\OmarchyScreensaver.scr" /r:System.Windows.Forms.dll /r:System.Drawing.dll "%DEST%\OmarchyScreensaver.cs"
if errorlevel 1 (
  echo Compile failed.
  pause
  exit /b 1
)
set TIMEOUT=600
for /f "tokens=3" %%A in ('reg query "HKCU\Control Panel\Desktop" /v ScreenSaveTimeOut 2^>nul') do set TIMEOUT=%%A
if "%TIMEOUT%"=="" set TIMEOUT=600
reg add "HKCU\Control Panel\Desktop" /v SCRNSAVE.EXE /t REG_SZ /d "%DEST%\OmarchyScreensaver.scr" /f >nul
reg add "HKCU\Control Panel\Desktop" /v ScreenSaveActive /t REG_SZ /d "1" /f >nul
reg add "HKCU\Control Panel\Desktop" /v ScreenSaveTimeOut /t REG_SZ /d "%TIMEOUT%" /f >nul
rundll32.exe user32.dll,UpdatePerUserSystemParameters
echo.
echo Holmes Contracting is now your Windows screensaver.
echo It starts after idle. Mouse or any key exits, same as Omarchy.
echo.
echo Opening Screen Saver settings so you can set the wait time...
rundll32.exe desk.cpl,InstallScreenSaver "%DEST%\OmarchyScreensaver.scr"
