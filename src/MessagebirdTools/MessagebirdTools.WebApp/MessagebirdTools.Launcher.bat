@echo off
echo Starting MessageBird Tools Web Application...

:: Find the web app dll
set WEBAPP_DLL=MessagebirdTools.WebApp.dll
set WEBAPP_URL=http://localhost:5000

:: Check current directory
if exist "%~dp0%WEBAPP_DLL%" (
    set WEBAPP_PATH=%~dp0%WEBAPP_DLL%
    goto :found
)

:: Check publish directory
if exist "%~dp0publish\%WEBAPP_DLL%" (
    set WEBAPP_PATH=%~dp0publish\%WEBAPP_DLL%
    goto :found
)

:: Check bin\Debug directory
if exist "%~dp0bin\Debug\net8.0\%WEBAPP_DLL%" (
    set WEBAPP_PATH=%~dp0bin\Debug\net8.0\%WEBAPP_DLL%
    goto :found
)

:: Check bin\Release directory
if exist "%~dp0bin\Release\net8.0\%WEBAPP_DLL%" (
    set WEBAPP_PATH=%~dp0bin\Release\net8.0\%WEBAPP_DLL%
    goto :found
)

echo Error: Unable to find the web application. Please ensure it is built correctly.
pause
exit /b 1

:found
echo Found web app at: %WEBAPP_PATH%

:: Start the web server in a new window
start "MessageBird Tools Server" cmd /c "dotnet "%WEBAPP_PATH%""

:: Wait a moment for the server to initialize
echo Waiting for server to start...
timeout /t 3 /nobreak > nul

:: Open the browser
echo Opening browser...
start "" "%WEBAPP_URL%"

echo.
echo Server is running. Close all related windows to shut down the application.