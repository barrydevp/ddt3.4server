@echo off
chcp 65001 >nul
echo ========================================
echo   Starting Gunny Servers
echo ========================================
echo.

REM Check admin
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Đang yêu cầu quyền Administrator...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

echo Đang dọn dẹp process cũ...
echo.

REM Kill old processes by exe name
for %%p in (
    Center.Service.exe
    Fighting.Service.exe
    Road.Service.exe
) do (
    echo - Checking %%p ...
    tasklist | findstr /i "%%p" >nul
    if not errorlevel 1 (
        echo   → Killing %%p
        taskkill /F /IM %%p >nul 2>&1
    ) else (
        echo   → Not running
    )
)

echo.
echo Đang khởi động servers với quyền Admin...
echo.

REM Start Center Server
echo [1/3] Starting Center Server...
start "Center Server" /D "%~dp0Center.Service\bin\Debug" "%~dp0Center.Service\bin\Debug\Center.Service.exe"
timeout /t 2 /nobreak >nul

REM Start Fighting Server
echo [2/3] Starting Fighting Server...
start "Fighting Server" /D "%~dp0Fighting.Service\bin\Debug" "%~dp0Fighting.Service\bin\Debug\Fighting.Service.exe"
timeout /t 2 /nobreak >nul

REM Start Road Server
echo [3/3] Starting Road Server...
start "Road Server" /D "%~dp0Road.Service\bin\Debug" "%~dp0Road.Service\bin\Debug\Road.Service.exe"
timeout /t 2 /nobreak >nul

echo.
echo ========================================
echo   Đã khởi động thành công 3 servers!
echo ========================================
echo   - Center Server
echo   - Fighting Server
echo   - Road Server
echo ========================================
echo.
echo Cửa sổ này sẽ tự động đóng sau 3 giây...
timeout /t 3 /nobreak >nul
exit