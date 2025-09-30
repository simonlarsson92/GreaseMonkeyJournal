#!/bin/bash
echo "Starting Grease Monkey Journal Development Environment..."
echo "========================================================="

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "? Docker is not running. Please start Docker Desktop and try again."
    exit 1
fi

echo "?? Docker is running"

# Stop any existing containers
echo "?? Stopping existing containers..."
docker-compose down

# Build and start services
echo "?? Building and starting services..."
docker-compose up -d --build

# Wait for services to be ready
echo "? Waiting for services to start..."
sleep 15

# Check service health
echo "?? Checking service health..."
echo ""
echo "Container Status:"
docker-compose ps

echo ""
echo "Health Check:"
curl -s http://localhost:8080/health > /dev/null && echo "? Application is healthy" || echo "? Application health check failed"

echo ""
echo "?? Development environment is ready!"
echo ""
echo "?? Access the application at: http://localhost:8080"
echo "?? Health check endpoint: http://localhost:8080/health"
echo "???  MariaDB accessible on: localhost:3306"
echo ""
echo "?? Useful commands:"
echo "  View logs: docker-compose logs -f"
echo "  Stop services: docker-compose down"
echo "  Restart: docker-compose restart"
echo ""