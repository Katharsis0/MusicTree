$DB_NAME = "musictree_db"
$DB_USER = "musictree_admin"
$DB_PASSWORD = "musictree"
$DB_PORT = 5432

Write-Host "=== MusicTree Setup ==="

# Test PostgreSQL
try {
    psql --version
} catch {
    Write-Error "PostgreSQL not found. Install it manually or via Chocolatey."
    exit 1
}

# Run .sql schema
$env:PGPASSWORD = $DB_PASSWORD
psql -h localhost -p $DB_PORT -U $DB_USER -d $DB_NAME -f "musictree_db.sql"
