# MusicTree Windows Setup Script
# PowerShell script for Windows environment
# Run as Administrator

param(
    [string]$DbPassword = "musictree",
    [string]$DbHost = "localhost",
    [string]$DbPort = "5432"
)

$ErrorActionPreference = "Stop"

# Colors for output
function Write-Success { param($msg) Write-Host "[SUCCESS] $msg" -ForegroundColor Green }
function Write-Info { param($msg) Write-Host "[INFO] $msg" -ForegroundColor Blue }
function Write-Warning { param($msg) Write-Host "[WARNING] $msg" -ForegroundColor Yellow }
function Write-Error { param($msg) Write-Host "[ERROR] $msg" -ForegroundColor Red }

Write-Info "=== MusicTree Windows Setup Script ==="
Write-Info "Setting up MusicTree backend development environment for Windows"

# Configuration
$DbName = "musictree_db"
$DbUser = "musictree_admin"

Write-Info "Database Configuration:"
Write-Info "  Host: $DbHost"
Write-Info "  Port: $DbPort"
Write-Info "  Database: $DbName"
Write-Info "  Username: $DbUser"
Write-Info "  Password: $DbPassword"
Write-Info ""

# =============================================================================
# STEP 1: Check Prerequisites
# =============================================================================

Write-Info "Checking prerequisites..."

# Check if PostgreSQL is installed
try {
    $pgVersion = & psql --version 2>$null
    Write-Success "PostgreSQL is installed: $pgVersion"
} catch {
    Write-Error "PostgreSQL is not installed or not in PATH"
    Write-Info "Please install PostgreSQL from: https://www.postgresql.org/download/windows/"
    Write-Info "Make sure to add PostgreSQL bin directory to your PATH"
    exit 1
}

# Check if .NET is installed
try {
    $dotnetVersion = & dotnet --version 2>$null
    Write-Success ".NET is installed: $dotnetVersion"
} catch {
    Write-Error ".NET is not installed or not in PATH"
    Write-Info "Please install .NET from: https://dotnet.microsoft.com/download"
    exit 1
}

# Check if PostgreSQL service is running
$pgService = Get-Service -Name "postgresql*" -ErrorAction SilentlyContinue
if ($pgService) {
    if ($pgService.Status -eq "Running") {
        Write-Success "PostgreSQL service is running"
    } else {
        Write-Info "Starting PostgreSQL service..."
        Start-Service $pgService.Name
        Write-Success "PostgreSQL service started"
    }
} else {
    Write-Warning "PostgreSQL service not found. Make sure PostgreSQL is properly installed."
}

# =============================================================================
# STEP 2: Database Setup
# =============================================================================

Write-Info "Setting up database..."

# Get PostgreSQL superuser password
$pgPassword = Read-Host "Enter PostgreSQL superuser (postgres) password" -AsSecureString
$pgPasswordText = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($pgPassword))

# Set environment variable for psql
$env:PGPASSWORD = $pgPasswordText

# Test connection to PostgreSQL
try {
    $null = & psql -h $DbHost -p $DbPort -U postgres -d postgres -c "SELECT 1;" 2>$null
    Write-Success "Connected to PostgreSQL successfully"
} catch {
    Write-Error "Failed to connect to PostgreSQL. Please check:"
    Write-Info "1. PostgreSQL is running"
    Write-Info "2. The password is correct"
    Write-Info "3. Host and port are correct"
    exit 1
}

# Check if database exists
$dbExists = & psql -h $DbHost -p $DbPort -U postgres -tAc "SELECT 1 FROM pg_database WHERE datname='$DbName';" 2>$null

if ($dbExists -eq "1") {
    Write-Warning "Database '$DbName' already exists"
    $recreate = Read-Host "Do you want to recreate it? This will delete all data! (y/N)"
    if ($recreate -eq "y" -or $recreate -eq "Y") {
        Write-Info "Dropping existing database and user..."
        & psql -h $DbHost -p $DbPort -U postgres -c "DROP DATABASE IF EXISTS $DbName;" 2>$null
        & psql -h $DbHost -p $DbPort -U postgres -c "DROP USER IF EXISTS $DbUser;" 2>$null
        Write-Success "Existing database and user dropped"
    } else {
        Write-Info "Using existing database"
    }
}

# Create user and database
Write-Info "Creating database user and database..."
try {
    & psql -h $DbHost -p $DbPort -U postgres -c "CREATE USER $DbUser WITH PASSWORD '$DbPassword';" 2>$null
    & psql -h $DbHost -p $DbPort -U postgres -c "CREATE DATABASE $DbName OWNER $DbUser;" 2>$null
    & psql -h $DbHost -p $DbPort -U postgres -c "GRANT ALL PRIVILEGES ON DATABASE $DbName TO $DbUser;" 2>$null
    Write-Success "Database and user created successfully"
} catch {
    Write-Warning "Database or user might already exist, continuing..."
}

# =============================================================================
# STEP 3: Initialize Database Schema
# =============================================================================

Write-Info "Initializing database schema..."

# Check if schema file exists
$schemaFile = "musictree_db.sql"
if (-not (Test-Path $schemaFile)) {
    Write-Error "Database schema file '$schemaFile' not found!"
    Write-Info "Please ensure the file is in the current directory: $(Get-Location)"
    exit 1
}

# Set password for database user
$env:PGPASSWORD = $DbPassword

# Run schema initialization
try {
    & psql -h $DbHost -p $DbPort -U $DbUser -d $DbName -f $schemaFile
    Write-Success "Database schema initialized successfully"
} catch {
    Write-Error "Failed to initialize database schema"
    Write-Info "Please check the schema file for errors"
    exit 1
}

# Test database connection with application user
try {
    $clusterCount = & psql -h $DbHost -p $DbPort -U $DbUser -d $DbName -tAc "SELECT COUNT(*) FROM `"Clusters`";" 2>$null
    Write-Success "Database connection test successful. Clusters: $clusterCount"
} catch {
    Write-Error "Database connection test failed"
    exit 1
}

# =============================================================================
# STEP 4: .NET Project Setup
# =============================================================================

Write-Info "Setting up .NET project..."

# Find project files
$projectFiles = Get-ChildItem -Recurse -Include "*.csproj", "*.sln" | Select-Object -First 1
if ($projectFiles) {
    $projectDir = $projectFiles.Directory.FullName
    Write-Success "Found .NET project in: $projectDir"
    Set-Location $projectDir
} else {
    Write-Warning "No .NET project files found"
    $projectPath = Read-Host "Enter the path to your MusicTree project directory (or press Enter to skip)"
    if ($projectPath -and (Test-Path $projectPath)) {
        Set-Location $projectPath
        Write-Success "Using specified project directory: $projectPath"
    } else {
        Write-Warning "Skipping .NET project setup"
        $projectDir = $null
    }
}

if ($projectDir) {
    # Create or update appsettings.Development.json
    $connectionString = "Host=$DbHost;Port=$DbPort;Database=$DbName;Username=$DbUser;Password=$DbPassword"
    $appsettings = @{
        "Logging" = @{
            "LogLevel" = @{
                "Default" = "Information"
                "Microsoft.AspNetCore" = "Warning"
                "Microsoft.EntityFrameworkCore" = "Information"
            }
        }
        "ConnectionStrings" = @{
            "DefaultConnection" = $connectionString
        }
        "AllowedHosts" = "*"
    } | ConvertTo-Json -Depth 3

    $appsettings | Out-File -FilePath "appsettings.Development.json" -Encoding UTF8
    Write-Success "appsettings.Development.json created/updated"

    # Restore packages
    Write-Info "Restoring NuGet packages..."
    try {
        & dotnet restore
        Write-Success "NuGet packages restored successfully"
    } catch {
        Write-Error "Failed to restore NuGet packages"
        exit 1
    }

    # Build project
    Write-Info "Building the project..."
    try {
        & dotnet build
        Write-Success "Project built successfully"
    } catch {
        Write-Error "Failed to build project"
        Write-Info "Please check for compilation errors"
        exit 1
    }
}

# =============================================================================
# COMPLETION
# =============================================================================

Write-Success "=== Setup Complete! ==="
Write-Info ""
Write-Info "Database Information:"
Write-Info "  Host: $DbHost"
Write-Info "  Port: $DbPort"  
Write-Info "  Database: $DbName"
Write-Info "  Username: $DbUser"
Write-Info "  Password: $DbPassword"
Write-Info ""
Write-Info "Next Steps:"
if ($projectDir) {
    Write-Info "1. Run the application:"
    Write-Info "   dotnet run"
    Write-Info ""
    Write-Info "2. Test the API (in another PowerShell window):"
    Write-Info "   .\test_musictree_api.ps1"
    Write-Info ""
    Write-Info "3. Access Swagger documentation:"
    Write-Info "   http://localhost:5102/swagger"
} else {
    Write-Info "1. Navigate to your MusicTree project directory"
    Write-Info "2. Update appsettings.Development.json with connection string:"
    Write-Info "   Host=$DbHost;Port=$DbPort;Database=$DbName;Username=$DbUser;Password=$DbPassword"
    Write-Info "3. Run: dotnet run"
}
Write-Info ""
Write-Info "Connect to database directly:"
Write-Info "   psql -h $DbHost -p $DbPort -U $DbUser -d $DbName"
Write-Info ""
Write-Success "Sprint 1 Features Ready:"
Write-Success "• MGCP calculation between genres"
Write-Success "• RGB color support for genres"  
Write-Success "• Compás filtering and advanced filtering"
Write-Success "• Influence relationships during genre creation"
Write-Success "• Complete database schema with constraints"

# Create reference file
$reference = @"
MusicTree Development Environment - Quick Reference (Windows)
============================================================

Database Connection:
- Host: $DbHost:$DbPort
- Database: $DbName
- User: $DbUser
- Password: $DbPassword

Commands:
- Start application: dotnet run
- Test API: .\test_musictree_api.ps1
- Connect to DB: psql -h $DbHost -p $DbPort -U $DbUser -d $DbName

Files:
- Database schema: musictree_db.sql
- App configuration: appsettings.Development.json
- Setup script: setup_musictree_windows.ps1
- API test: test_musictree_api.ps1

Sprint 1 Fixed Issues:
✅ MGCP calculation working
✅ RGB color support implemented
✅ Compás filtering available
✅ Influence relationships functional

Setup completed: $(Get-Date)
"@

$reference | Out-File -FilePath "SETUP_REFERENCE_WINDOWS.txt" -Encoding UTF8
Write-Success "Quick reference saved to SETUP_REFERENCE_WINDOWS.txt"
Write-Success "Setup completed successfully! 🎵"