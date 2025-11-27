@echo off
REM =============================================
REM Setup Script for Library Database
REM This script creates the Library database in SQL Server LocalDB
REM =============================================

echo.
echo ==========================================
echo Library Database Setup
echo ==========================================
echo.

REM Check if LocalDB is installed
sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT @@VERSION" >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: SQL Server LocalDB is not installed or not running.
    echo.
    echo Please install SQL Server LocalDB:
    echo https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb
    echo.
    pause
    exit /b 1
)

echo SQL Server LocalDB detected successfully!
echo.
echo Creating Library database...
echo.

REM Execute the SQL script
sqlcmd -S "(localdb)\mssqllocaldb" -i DatabaseSetup.sql

if %ERRORLEVEL% EQ 0 (
    echo.
    echo ==========================================
    echo Database setup completed successfully!
    echo ==========================================
    echo.
    echo You can now run the LibraryApi application.
    echo.
) else (
    echo.
    echo ==========================================
    echo ERROR: Database setup failed!
    echo ==========================================
    echo.
    echo Please check the error messages above.
    echo.
)

pause
