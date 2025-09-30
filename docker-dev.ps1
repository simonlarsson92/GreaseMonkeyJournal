# Grease Monkey Journal Docker Development Environment Script
# PowerShell script for starting the development environment

Write-Host "Starting Grease Monkey Journal Development Environment..." -ForegroundColor Cyan
Write-Host "=========================================================" -ForegroundColor Cyan

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

# Function to test URL accessibility
function Test-Url {
    param([string]$Url)
    try {
        $response = Invoke-WebRequest -Uri $Url -Method HEAD -TimeoutSec 5 -UseBasicParsing
        return $response.StatusCode -eq 200
    }
    catch {
        return $false
    }
}

# Check if Docker is running
if (-not (Test-DockerRunning)) {
    Write-Host "? Docker is not running. Please start Docker Desktop and try again." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "?? Docker is running" -ForegroundColor Green

# Stop any existing containers
Write-Host "?? Stopping existing containers..." -ForegroundColor Yellow
docker-compose down

# Build and start services
Write-Host "?? Building and starting services..." -ForegroundColor Blue
docker-compose up -d --build

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Failed to start services. Check the output above for errors." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Wait for services to be ready
Write-Host "? Waiting for services to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

# Check service health
Write-Host "?? Checking service health..." -ForegroundColor Magenta
Write-Host ""
Write-Host "Container Status:" -ForegroundColor White
docker-compose ps

Write-Host ""
Write-Host "Health Check:" -ForegroundColor White
if (Test-Url "http://localhost:8080/health") {
    Write-Host "? Application is healthy" -ForegroundColor Green
} else {
    Write-Host "? Application health check failed" -ForegroundColor Red
    Write-Host "   Application may still be starting up. Please wait a moment and try again." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "?? Development environment is ready!" -ForegroundColor Green
Write-Host ""
Write-Host "?? Access the application at: " -NoNewline -ForegroundColor White
Write-Host "http://localhost:8080" -ForegroundColor Cyan
Write-Host "?? Health check endpoint: " -NoNewline -ForegroundColor White
Write-Host "http://localhost:8080/health" -ForegroundColor Cyan
Write-Host "???  MariaDB accessible on: " -NoNewline -ForegroundColor White
Write-Host "localhost:3306" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? Useful commands:" -ForegroundColor Yellow
Write-Host "  View logs: " -NoNewline -ForegroundColor White
Write-Host "docker-compose logs -f" -ForegroundColor Gray
Write-Host "  Stop services: " -NoNewline -ForegroundColor White
Write-Host "docker-compose down" -ForegroundColor Gray
Write-Host "  Restart: " -NoNewline -ForegroundColor White
Write-Host "docker-compose restart" -ForegroundColor Gray
Write-Host "  Reset environment: " -NoNewline -ForegroundColor White
Write-Host ".\reset-docker.ps1" -ForegroundColor Gray
Write-Host ""

# Optional: Open browser automatically
$openBrowser = Read-Host "Would you like to open the application in your default browser? (y/N)"
if ($openBrowser -eq 'y' -or $openBrowser -eq 'Y') {
    Write-Host "?? Opening browser..." -ForegroundColor Blue
    Start-Process "http://localhost:8080"
}

Write-Host "Press Enter to exit..." -ForegroundColor DarkGray
Read-Host