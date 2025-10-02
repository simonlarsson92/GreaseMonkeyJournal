#!/bin/bash

# Grease Monkey Journal Docker Development Environment Script
# Shell script for starting the development environment

echo "Starting Grease Monkey Journal Development Environment..."
echo "========================================================="

# Function to check if Docker is running
check_docker_running() {
    if ! docker info >/dev/null 2>&1; then
        echo "ERROR: Docker is not running. Please start Docker and try again."
        echo "Press Enter to exit"
        read
        exit 1
    fi
}

# Function to test URL accessibility
test_url() {
    local url=$1
    if curl -s --head --max-time 5 "$url" | head -n 1 | grep -q "200 OK"; then
        return 0
    else
        return 1
    fi
}

# Check if Docker is running
check_docker_running
echo "SUCCESS: Docker is running"

# Stop any existing containers
echo "INFO: Stopping existing containers..."
docker-compose down

# Build and start services
echo "INFO: Building and starting services..."
docker-compose up -d --build

if [ $? -ne 0 ]; then
    echo "ERROR: Failed to start services. Check the output above for errors."
    echo "Press Enter to exit"
    read
    exit 1
fi

# Wait for services to be ready
echo "INFO: Waiting for services to start..."
sleep 15

# Check service health
echo "INFO: Checking service health..."
echo ""
echo "Container Status:"
docker-compose ps

echo ""
echo "Health Check:"
if test_url "http://localhost:8080/health"; then
    echo "SUCCESS: Application is healthy"
else
    echo "WARNING: Application health check failed"
    echo "   Application may still be starting up. Please wait a moment and try again."
fi

echo ""
echo "SUCCESS: Development environment is ready!"
echo ""
echo "Access the application at: http://localhost:8080"
echo "Health check endpoint: http://localhost:8080/health"
echo "MariaDB accessible on: localhost:3306"
echo ""
echo "Useful commands:"
echo "  View logs: docker-compose logs -f"
echo "  Stop services: docker-compose down"
echo "  Restart: docker-compose restart"
echo "  Reset environment: ./reset-docker.sh"
echo ""

# Optional: Open browser automatically (Linux with xdg-open, macOS with open)
echo -n "Would you like to open the application in your default browser? (y/N): "
read open_browser
if [[ "$open_browser" == "y" || "$open_browser" == "Y" ]]; then
    echo "INFO: Opening browser..."
    if command -v xdg-open > /dev/null; then
        xdg-open "http://localhost:8080"
    elif command -v open > /dev/null; then
        open "http://localhost:8080"
    else
        echo "WARNING: Could not detect browser opener. Please manually navigate to http://localhost:8080"
    fi
fi

echo "Press Enter to exit..."
read