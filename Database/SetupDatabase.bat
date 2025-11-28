@echo off
REM =============================================
REM Library Database Setup Script
REM Runs DatabaseSetup.sql against SQL Server in Docker
REM =============================================

echo.
echo ========================================
echo Library Database Setup
echo ========================================
echo.

REM Check if sqlcmd is available
where sqlcmd >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: sqlcmd is not found in PATH
    echo Please install SQL Server Command Line Tools
    echo Download from: https://learn.microsoft.com/en-us/sql/tools/sqlcmd-utility
    pause
    exit /b 1
)

REM Check if Docker container is running
docker ps --filter "name=sql1" --format "{{.Names}}" | findstr "sql1" >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: SQL Server container 'sql1' is not running
    echo Please start the container with: docker start sql1
    pause
    exit /b 1
)

echo Waiting for SQL Server to be ready...
timeout /t 3 /nobreak >nul

echo.
echo Executing DatabaseSetup.sql...
echo.

REM Execute the setup script
sqlcmd -S localhost,1433 -U sa -P "Byt1.Byt2.Byt3" -i DatabaseSetup.sql

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Database setup completed successfully!
    echo ========================================
) else (
    echo.
    echo ========================================
    echo ERROR: Database setup failed
    echo ========================================
)

echo.
pause
