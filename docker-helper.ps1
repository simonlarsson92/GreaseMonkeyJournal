# Vehicle Maintenance Log Docker Helper Script (PowerShell)

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("build", "up", "up-dev", "down", "logs", "status", "health", "clean")]
    [string]$Command
)

switch ($Command) {
    "build" {
        Write-Host "Building Vehicle Maintenance Log Docker image..." -ForegroundColor Green
        docker-compose build
    }
    "up" {
        Write-Host "Starting Vehicle Maintenance Log (Production)..." -ForegroundColor Green
        docker-compose up -d
        Write-Host "Application available at: http://localhost:44348" -ForegroundColor Yellow
        Write-Host "Health check at: http://localhost:44348/health" -ForegroundColor Yellow
    }
    "up-dev" {
        Write-Host "Starting Vehicle Maintenance Log (Development)..." -ForegroundColor Green
        docker-compose -f docker-compose.dev.yml up -d
        Write-Host "Application available at: http://localhost:7020" -ForegroundColor Yellow
        Write-Host "Health check at: http://localhost:7020/health" -ForegroundColor Yellow
    }
    "down" {
        Write-Host "Stopping Vehicle Maintenance Log..." -ForegroundColor Green
        docker-compose down
        try { docker-compose -f docker-compose.dev.yml down } catch { }
    }
    "logs" {
        Write-Host "Showing application logs..." -ForegroundColor Green
        try {
            docker-compose logs -f vehiclelog-app
        } catch {
            docker-compose -f docker-compose.dev.yml logs -f vehiclelog-app-dev
        }
    }
    "status" {
        Write-Host "Checking container status..." -ForegroundColor Green
        try {
            docker-compose ps
        } catch {
            docker-compose -f docker-compose.dev.yml ps
        }
    }
    "health" {
        Write-Host "Checking application health..." -ForegroundColor Green
        # Try production port first, then development port
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:44348/health" -UseBasicParsing
            if ($response.StatusCode -eq 200) {
                Write-Host "✅ Production application is healthy (port 44348)" -ForegroundColor Green
            }
        } catch {
            try {
                $response = Invoke-WebRequest -Uri "http://localhost:7020/health" -UseBasicParsing
                if ($response.StatusCode -eq 200) {
                    Write-Host "✅ Development application is healthy (port 7020)" -ForegroundColor Green
                }
            } catch {
                Write-Host "❌ Application is not healthy on either port" -ForegroundColor Red
            }
        }
    }
    "clean" {
        Write-Host "Cleaning up containers and images..." -ForegroundColor Green
        docker-compose down
        try { docker-compose -f docker-compose.dev.yml down } catch { }
        docker system prune -f
    }
}
