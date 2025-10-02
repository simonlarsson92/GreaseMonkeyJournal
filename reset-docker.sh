#!/bin/bash

# Grease Monkey Journal Docker Environment Reset Script
# Shell script for completely resetting the Docker environment

echo -e "\033[31mResetting Grease Monkey Journal Docker Environment...\033[0m"
echo -e "\033[31m====================================================\033[0m"

# Function to check if Docker is running
check_docker_running() {
    if ! docker info >/dev/null 2>&1; then
        echo -e "\033[31mERROR: Docker is not running. Please start Docker and try again.\033[0m"
        echo "Press Enter to exit"
        read
        exit 1
    fi
}

# Check if Docker is running
check_docker_running
echo -e "\033[32mSUCCESS: Docker is running\033[0m"

# Confirm action
echo ""
echo -e "\033[33mWARNING: This will completely reset your Docker environment!\033[0m"
echo -e "\033[33m   - Stop all containers\033[0m"
echo -e "\033[33m   - Remove MariaDB volume (all data will be lost)\033[0m"
echo -e "\033[33m   - Rebuild and restart services\033[0m"
echo ""

echo -n "Are you sure you want to continue? (y/N): "
read confirm
if [[ "$confirm" != "y" && "$confirm" != "Y" ]]; then
    echo -e "\033[33mINFO: Operation cancelled.\033[0m"
    echo "Press Enter to exit"
    read
    exit 0
fi

# Stop and remove containers
echo -e "\033[33mINFO: Stopping and removing existing containers...\033[0m"
docker-compose down

# Remove MariaDB volume to reset database
echo -e "\033[33mINFO: Removing MariaDB volume to reset database...\033[0m"
volume_name="greasemonkeyjournal_mariadb_data"
if docker volume rm "$volume_name" >/dev/null 2>&1; then
    echo -e "\033[32mSUCCESS: Volume '$volume_name' removed successfully\033[0m"
else
    echo -e "\033[34mINFO: Volume '$volume_name' was not found (this is normal)\033[0m"
fi

# Start services with fresh build
echo -e "\033[34mINFO: Starting services with fresh build...\033[0m"
docker-compose up -d --build

if [ $? -ne 0 ]; then
    echo -e "\033[31mERROR: Failed to start services. Check the output above for errors.\033[0m"
    echo "Press Enter to exit"
    read
    exit 1
fi

# Wait for services to be ready
echo -e "\033[33mINFO: Waiting for services to be ready...\033[0m"
sleep 10

# Check container status
echo -e "\033[35mINFO: Checking container status...\033[0m"
docker-compose ps

echo ""
echo -e "\033[32mSUCCESS: Environment reset complete!\033[0m"
echo ""
echo -e "\033[37mAccess the application at: \033[36mhttp://localhost:8080\033[0m"
echo -e "\033[37mHealth check endpoint: \033[36mhttp://localhost:8080/health\033[0m"
echo ""

echo "Press Enter to exit"
read