@echo off
chcp 65001 >nul
echo ========================================
echo   Stopping Gunny Servers
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

echo Cửa sổ này sẽ tự động đóng sau 3 giây...
timeout /t 3 /nobreak >nul
exit