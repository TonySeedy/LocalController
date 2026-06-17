@echo off
:: Batch script to create Task Scheduler for LocalController
:: Run this script as Administrator

set "TASK_NAME=LocalController_AutoStart"
set "APP_PATH=%~dp0LocalController\bin\Debug\net8.0-windows10.0.19041.0\LocalController.exe"

:: Request Admin rights if not already running as Admin
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Administrator rights confirmed.
) else (
    echo Requesting Administrator rights...
    powershell -Command "Start-Process -FilePath '%0' -Verb RunAs"
    exit /b
)

:: Check if the executable exists
if not exist "%APP_PATH%" (
    echo [ERROR] Cannot find LocalController.exe at:
    echo %APP_PATH%
    echo Please make sure you have built the project before running this script.
    echo Or update the APP_PATH in this script to point to your Release build.
    pause
    exit /b 1
)

echo =========================================================
echo Adding LocalController to Task Scheduler
echo Task Name: %TASK_NAME%
echo Application: %APP_PATH%
echo Arguments: --hidden
echo Trigger: At log on, delayed by 1 minute
echo =========================================================
echo.

:: Delete existing task if it exists (to avoid conflicts)
schtasks /query /tn "%TASK_NAME%" >nul 2>&1
if %errorlevel% equ 0 (
    echo Removing existing task...
    schtasks /delete /tn "%TASK_NAME%" /f
)

:: Create the new task
:: /RL HIGHEST: Run with highest privileges (Run as Administrator)
:: /SC ONLOGON: Trigger at logon
:: /DELAY 0001:00: Delay for 1 minute (60 seconds)
schtasks /create /tn "%TASK_NAME%" /tr "\"%APP_PATH%\" --hidden" /sc onlogon /delay 0001:00 /rl highest /f

if %errorlevel% equ 0 (
    echo.
    echo [SUCCESS] Task created successfully!
    echo The application will now start automatically in hidden mode 60 seconds after you log on.
) else (
    echo.
    echo [ERROR] Failed to create task. Please ensure you are running this script as Administrator.
)

echo.
pause
