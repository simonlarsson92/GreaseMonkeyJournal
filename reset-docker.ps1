# Grease Monkey Journal Docker Environment Reset Script
# PowerShell script for completely resetting the Docker environment

Write-Host "Resetting Grease Monkey Journal Docker Environment..." -ForegroundColor Red
Write-Host "====================================================" -ForegroundColor Red

# Function to check if Docker is running
function Test-DockerRunning {
    try {
        $null = docker info 2>$null
        return $true
    }
    catch {
        return $false
    }
}

# Check if Docker is running
if (-not (Test-DockerRunning)) {
    Write-Host "ERROR: Docker is not running. Please start Docker Desktop and try again." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "SUCCESS: Docker is running" -ForegroundColor Green

# Confirm action
Write-Host ""
Write-Host "WARNING: This will completely reset your Docker environment!" -ForegroundColor Yellow
Write-Host "   - Stop all containers" -ForegroundColor Yellow
Write-Host "   - Remove MariaDB volume (all data will be lost)" -ForegroundColor Yellow
Write-Host "   - Rebuild and restart services" -ForegroundColor Yellow
Write-Host ""

$confirm = Read-Host "Are you sure you want to continue? (y/N)"
if ($confirm -ne 'y' -and $confirm -ne 'Y') {
    Write-Host "INFO: Operation cancelled." -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 0
}

# Stop and remove containers
Write-Host "INFO: Stopping and removing existing containers..." -ForegroundColor Yellow
docker-compose down

# Get the actual volume name used by docker-compose
Write-Host "INFO: Finding MariaDB volume..." -ForegroundColor Yellow
$volumeName = ""
try {
    # Get the project name from docker-compose
    $projectName = docker-compose config --format json | ConvertFrom-Json | Select-Object -ExpandProperty name
    if ($projectName) {
        $volumeName = "${projectName}_mariadb_data"
    } else {
        # Fallback: try to find volume with mariadb_data in the name
        $volumes = docker volume ls --format "{{.Name}}" | Where-Object { $_ -like "*mariadb_data*" }
        if ($volumes) {
            $volumeName = $volumes[0]
        }
    }
} catch {
    Write-Host "WARNING: Could not determine project name, trying fallback method..." -ForegroundColor Yellow
    # Fallback: try to find volume with mariadb_data in the name
    $volumes = docker volume ls --format "{{.Name}}" | Where-Object { $_ -like "*mariadb_data*" }
    if ($volumes) {
        $volumeName = $volumes[0]
    }
}

# Remove MariaDB volume to reset database
if ($volumeName) {
    Write-Host "INFO: Removing MariaDB volume '$volumeName' to reset database..." -ForegroundColor Yellow
    try {
        docker volume rm $volumeName 2>$null
        Write-Host "SUCCESS: Volume '$volumeName' removed successfully" -ForegroundColor Green
    } catch {
        Write-Host "INFO: Volume '$volumeName' was not found (this is normal)" -ForegroundColor Blue
    }
} else {
    Write-Host "INFO: No MariaDB volume found to remove (this is normal for first run)" -ForegroundColor Blue
}

# Start services with fresh build
Write-Host "INFO: Starting services with fresh build..." -ForegroundColor Blue
docker-compose up -d --build

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to start services. Check the output above for errors." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Wait for services to be ready
Write-Host "INFO: Waiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check container status
Write-Host "INFO: Checking container status..." -ForegroundColor Magenta
docker-compose ps

Write-Host ""
Write-Host "SUCCESS: Environment reset complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Access the application at: " -NoNewline -ForegroundColor White
Write-Host "http://localhost:8080" -ForegroundColor Cyan
Write-Host "Health check endpoint: " -NoNewline -ForegroundColor White
Write-Host "http://localhost:8080/health" -ForegroundColor Cyan
Write-Host ""

Read-Host "Press Enter to exit"