#!/bin/bash

# MusicTree System Setup Script
# Project: MusicTree para X-Tec
# Author: Katharsis0
# Attempt of script for easy system reproduction

set -e # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
DB_NAME="musictree_db"
DB_USER="musictree_admin"
DB_PASSWORD="musictree"
DB_HOST="localhost"
DB_PORT="5432"

echo -e "${BLUE}=== MusicTree System Setup Script ===${NC}"
echo "This script will attempt to set up the MusicTree backend development environment"
echo

# Function to print status
print_status() {
  echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
  echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
  echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if command exists
command_exists() {
  command -v "$1" >/dev/null 2>&1
}

# Check if running as root (not recommended)
if [[ $EUID -eq 0 ]]; then
  print_error "This script should not be run as root for security reasons"
  exit 1
fi

print_status "Starting MusicTree system setup..."

# =============================================================================
# STEP 1: PostgreSQL Setup
# =============================================================================

print_status "Setting up PostgreSQL..."

# Start PostgreSQL service
print_status "Starting PostgreSQL service..."
if command_exists systemctl; then
  sudo systemctl start postgresql
  sudo systemctl enable postgresql
  print_status "PostgreSQL service started and enabled ✓"
else
  print_warning "systemctl not available. Please start PostgreSQL service manually."
fi

# Check if PostgreSQL is running
if ! pg_isready -h $DB_HOST -p $DB_PORT >/dev/null 2>&1; then
  print_error "PostgreSQL is not running on $DB_HOST:$DB_PORT"
  print_error "Please start PostgreSQL service manually"
  exit 1
fi

print_status "PostgreSQL is running ✓"

# =============================================================================
# STEP 2: Database Setup
# =============================================================================

print_status "Setting up MusicTree database..."

# Create database and user (as postgres superuser)
print_status "Creating database and user..."

# Check if database exists
if sudo -u postgres psql -lqt | cut -d \| -f 1 | grep -qw $DB_NAME; then
  print_warning "Database '$DB_NAME' already exists"
  read -p "Do you want to recreate it? This will delete all data! (y/N): " -n 1 -r
  echo
  if [[ $REPLY =~ ^[Yy]$ ]]; then
    sudo -u postgres psql -c "DROP DATABASE IF EXISTS $DB_NAME;"
    sudo -u postgres psql -c "DROP USER IF EXISTS $DB_USER;"
    print_status "Existing database and user dropped"
  else
    print_status "Using existing database"
  fi
fi

# Create user and database
sudo -u postgres psql <<EOF
CREATE USER $DB_USER WITH PASSWORD '$DB_PASSWORD';
CREATE DATABASE $DB_NAME OWNER $DB_USER;
GRANT ALL PRIVILEGES ON DATABASE $DB_NAME TO $DB_USER;
\q
EOF

print_status "Database and user created successfully ✓"

# =============================================================================
# STEP 3: Initialize Database Schema
# =============================================================================

print_status "Initializing database schema..."

# Check if init script exists
INIT_SCRIPT="musictree_db.sql"
if [ ! -f "$INIT_SCRIPT" ]; then
  print_error "Database initialization script '$INIT_SCRIPT' not found!"
  print_error "Please ensure the script is in the current directory"
  exit 1
fi

# Run initialization script
export PGPASSWORD=$DB_PASSWORD
if psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f $INIT_SCRIPT; then
  print_status "Database schema initialized successfully ✓"
else
  print_error "Failed to initialize database schema"
  exit 1
fi

# =============================================================================
# STEP 4: .NET Project Setup
# =============================================================================

print_status "Setting up .NET project..."

# Check if we're in a .NET project directory
if [ ! -f "*.csproj" ] && [ ! -f "*.sln" ]; then
  print_warning "No .NET project files found in current directory"
  read -p "Enter the path to your MusicTree project directory: " PROJECT_DIR
  if [ -d "$PROJECT_DIR" ]; then
    cd "$PROJECT_DIR"
    print_status "Changed to project directory: $PROJECT_DIR"
  else
    print_error "Project directory not found: $PROJECT_DIR"
    exit 1
  fi
fi

# Create appsettings.Development.json if it doesn't exist
if [ ! -f "appsettings.Development.json" ]; then
  print_status "Creating appsettings.Development.json..."
  cat >appsettings.Development.json <<EOF
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=$DB_HOST;Port=$DB_PORT;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASSWORD"
  },
  "AllowedHosts": "*"
}
EOF
  print_status "appsettings.Development.json created ✓"
else
  print_status "appsettings.Development.json already exists"
fi

# Restore NuGet packages
print_status "Restoring NuGet packages..."
if dotnet restore; then
  print_status "NuGet packages restored successfully ✓"
else
  print_error "Failed to restore NuGet packages"
  exit 1
fi

# Build the project
print_status "Building the project..."
if dotnet build; then
  print_status "Project built successfully ✓"
else
  print_error "Failed to build project"
  exit 1
fi

# =============================================================================
# STEP 5: Verification
# =============================================================================

print_status "Verifying setup..."

# Test database connection
print_status "Testing database connection..."
export PGPASSWORD=$DB_PASSWORD
if psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "SELECT COUNT(*) FROM \"Clusters\";" >/dev/null 2>&1; then
  print_status "Database connection test successful ✓"
else
  print_error "Database connection test failed"
  exit 1
fi

# Test .NET application startup (quick test)
print_status "Testing application startup..."
timeout 10s dotnet run --no-build --urls="http://localhost:5000" >/dev/null 2>&1 &
APP_PID=$!
sleep 5

if kill -0 $APP_PID 2>/dev/null; then
  kill $APP_PID
  print_status "Application startup test successful ✓"
else
  print_warning "Application startup test may have failed (check manually)"
fi

# =============================================================================
# STEP 9: Completion and Instructions
# =============================================================================

echo
echo -e "${GREEN}=== Setup Complete! ===${NC}"
echo
print_status "MusicTree development environment is ready!"
echo
echo -e "${BLUE}Database Information:${NC}"
echo "  Host: $DB_HOST"
echo "  Port: $DB_PORT"
echo "  Database: $DB_NAME"
echo "  Username: $DB_USER"
echo "  Password: $DB_PASSWORD"
echo
echo -e "${BLUE}Next Steps:${NC}"
echo "1. Run the application:"
echo "   dotnet run"
echo
echo "2. Access the application:"
echo "   http://localhost:5000"
echo
echo "3. Connect to database directly:"
echo "   psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME"
echo
echo -e "${BLUE}For Sprint 2 Documentation:${NC}"
echo "- Database schema is documented in musictree_init.sql"
echo "- Connection configuration is in appsettings.Development.json"
echo "- This setup script serves as deployment documentation"
echo
echo -e "${YELLOW}Note: Remember to update your connection strings for different environments!${NC}"
echo

# Create a quick reference file
cat >SETUP_REFERENCE.txt <<EOF
MusicTree Development Environment - Quick Reference
==================================================

Database Connection:
- Host: $DB_HOST:$DB_PORT
- Database: $DB_NAME
- User: $DB_USER
- Password: $DB_PASSWORD

Commands:
- Start application: dotnet run
- Connect to DB: psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME  
- Rebuild schema: psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f musictree_init.sql

Files:
- Database schema: musictree_init.sql
- App configuration: appsettings.Development.json
- Setup script: setup_musictree.sh

Setup completed: $(date)
EOF

print_status "Quick reference saved to SETUP_REFERENCE.txt"
print_status "Setup completed successfully! 🎵"
