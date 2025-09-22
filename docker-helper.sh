#!/bin/bash

# Vehicle Maintenance Log Docker Helper Script

set -e

case "$1" in
  "build")
    echo "Building Vehicle Maintenance Log Docker image..."
    docker-compose build
    ;;
  "up")
    echo "Starting Vehicle Maintenance Log (Production)..."
    docker-compose up -d
    echo "Application available at: http://localhost:44348"
    echo "Health check at: http://localhost:44348/health"
    ;;
  "up-dev")
    echo "Starting Vehicle Maintenance Log (Development)..."
    docker-compose -f docker-compose.dev.yml up -d
    echo "Application available at: http://localhost:7020"
    echo "Health check at: http://localhost:7020/health"
    ;;
  "down")
    echo "Stopping Vehicle Maintenance Log..."
    docker-compose down
    docker-compose -f docker-compose.dev.yml down 2>/dev/null || true
    ;;
  "logs")
    echo "Showing application logs..."
    docker-compose logs -f vehiclelog-app 2>/dev/null || docker-compose -f docker-compose.dev.yml logs -f vehiclelog-app-dev
    ;;
  "status")
    echo "Checking container status..."
    docker-compose ps 2>/dev/null || docker-compose -f docker-compose.dev.yml ps
    ;;
  "health")
    echo "Checking application health..."
    # Try production port first, then development port
    if curl -f http://localhost:44348/health >/dev/null 2>&1; then
      echo "✅ Production application is healthy (port 44348)"
    elif curl -f http://localhost:7020/health >/dev/null 2>&1; then
      echo "✅ Development application is healthy (port 7020)"
    else
      echo "❌ Application is not healthy on either port"
    fi
    ;;
  "clean")
    echo "Cleaning up containers and images..."
    docker-compose down
    docker-compose -f docker-compose.dev.yml down 2>/dev/null || true
    docker system prune -f
    ;;
  *)
    echo "Vehicle Maintenance Log Docker Helper"
    echo ""
    echo "Usage: $0 {build|up|up-dev|down|logs|status|health|clean}"
    echo ""
    echo "Commands:"
    echo "  build     - Build the Docker image"
    echo "  up        - Start the application (production mode)"
    echo "  up-dev    - Start the application (development mode)"
    echo "  down      - Stop the application"
    echo "  logs      - Show application logs"
    echo "  status    - Show container status"
    echo "  health    - Check application health"
    echo "  clean     - Clean up containers and images"
    echo ""
    exit 1
    ;;
esac
