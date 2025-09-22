# Docker Setup for Vehicle Maintenance Log

This application is configured to run in Docker containers using an external database.

## Prerequisites

- Docker and Docker Compose installed
- External MariaDB/MySQL database running and accessible
- Connection strings configured in `appsettings.json` and `appsettings.Development.json`

## Configuration

The application reads database connection strings from:
- **Production**: `appsettings.json`
- **Development**: `appsettings.Development.json`

Make sure your connection strings are properly configured before running the containers.

## Running the Application

### Production Mode
```bash
# Build and run the application
docker-compose up --build

# Run in detached mode
docker-compose up -d --build

# View logs
docker-compose logs -f vehiclelog-app

# Stop the application
docker-compose down
```

### Development Mode
```bash
# Build and run in development mode
docker-compose -f docker-compose.dev.yml up --build

# Run in detached mode
docker-compose -f docker-compose.dev.yml up -d --build

# View logs
docker-compose -f docker-compose.dev.yml logs -f vehiclelog-app-dev

# Stop the application
docker-compose -f docker-compose.dev.yml down
```

## Accessing the Application

- **Production Application**: http://localhost:44348
- **Development Application**: http://localhost:7020
- **Production Health Check**: http://localhost:44348/health
- **Development Health Check**: http://localhost:7020/health

## Health Checks

The application includes built-in health checks that monitor:
- Application responsiveness
- Database connectivity (through EF Core)

## Logs

Application logs are stored in the `./logs` directory on the host machine.

## Environment Variables

The following environment variables can be configured:

- `ASPNETCORE_ENVIRONMENT`: Set to `Development` or `Production`
- `ASPNETCORE_URLS`: URLs the application listens on (default: `http://+:8080`)

## Database Migrations

The application automatically runs pending Entity Framework migrations on startup, ensuring your database schema is up to date.

## Troubleshooting

1. **Connection Issues**: Verify your database connection strings are correct and the database is accessible from the Docker container
2. **Port Conflicts**: Ensure ports 44348 (production) and 7020 (development) are not being used by another application
3. **Health Check Failures**: Check application logs for database connection issues

## Building for Different Architectures

```bash
# Build for multiple architectures (if needed)
docker buildx build --platform linux/amd64,linux/arm64 -t vehiclelog-app .
```
