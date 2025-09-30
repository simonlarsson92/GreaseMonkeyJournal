# Grease Monkey Journal

A comprehensive vehicle maintenance logging application built with .NET 9 Blazor Server and MariaDB.

## ?? Overview

Grease Monkey Journal is a web-based application designed to help vehicle owners track and manage their maintenance records, service history, and upcoming reminders. Built with modern web technologies, it provides an intuitive interface for logging maintenance activities and monitoring vehicle health.

## ??? Technology Stack

- **Frontend**: Blazor Server (.NET 9)
- **Backend**: ASP.NET Core 9
- **Database**: MariaDB 11.0
- **ORM**: Entity Framework Core
- **Containerization**: Docker & Docker Compose
- **Health Monitoring**: Built-in health checks

## ?? Prerequisites

- [Docker](https://www.docker.com/get-started) (version 20.10 or later)
- [Docker Compose](https://docs.docker.com/compose/install/) (version 1.29 or later)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (for local development)
- [PowerShell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell) (for Windows development script)

## ?? Quick Start with Docker

### 1. Clone the Repository
```bash
git clone https://github.com/simonlarsson92/GreaseMonkeyJournal.git
cd GreaseMonkeyJournal
```

### 2. Start the Application
Run the development script for your platform:

**Windows (PowerShell):**
```powershell
.\docker-dev.ps1
```

**Linux/macOS:**
```bash
chmod +x docker-dev.sh
./docker-dev.sh
```

**Reset Environment (if needed):**
```bash
# Windows
reset-docker.bat

# Linux/macOS
./reset-docker.sh
```

**Or manually:**
```bash
docker-compose up -d --build
```

### 3. Access the Application
- **Web Application**: http://localhost:8080
- **Health Check**: http://localhost:8080/health
- **Database**: MariaDB accessible on localhost:3306

## ?? Docker Architecture

### Services

| Service | Container Name | Ports | Description |
|---------|----------------|-------|-------------|
| **greasemonkeyjournal.api** | greasemonkeyjournal-greasemonkeyjournal.api-1 | 8080:8080 | Main Blazor Server application |
| **mariadb** | greasemonkey-mariadb | 3306:3306 | MariaDB database server |

### Docker Network
- **Network Name**: `greasemonkey-network`
- **Type**: Bridge network
- **Purpose**: Enables secure communication between containers

### Volumes
- **mariadb_data**: Persistent storage for MariaDB data
- **init.sql**: Database initialization script with user permissions

## ??? Database Configuration

### Connection Details
- **Server**: mariadb (Docker internal) / localhost (external)
- **Port**: 3306
- **Database**: database
- **User**: appuser
- **Password**: yourpassword

### Database Initialization
The application automatically:
1. Creates the database schema
2. Sets up user permissions
3. Runs Entity Framework migrations
4. Initializes required tables

## ?? Development Setup

### Local Development (without Docker)
1. **Start MariaDB** (ensure it's running on localhost:3306)
2. **Update Connection String** in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "MariaDbConnection": "server=localhost;port=3306;database=database;user=root;password=yourpassword;"
     }
   }
   ```
3. **Run the Application**:
   ```bash
   cd GreaseMonkeyJournal.Api
   dotnet run
   ```

### Docker Development
For development with Docker, use the development scripts:
```bash
# Windows (PowerShell)
.\docker-dev.ps1

# Linux/macOS
./docker-dev.sh

# View logs
docker-compose logs -f greasemonkeyjournal.api

# Stop services
docker-compose down
```

## ?? Health Monitoring

The application includes comprehensive health checks:
- **Database Connectivity**: Verifies MariaDB connection
- **Application Health**: Monitors overall application status
- **Endpoint**: `/health`

### Health Check Response
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.123",
  "entries": {
    "GreaseMonkeyJournal.Api.Components.DbContext.VehicleLogDbContext": {
      "data": {},
      "description": null,
      "duration": "00:00:00.123",
      "status": "Healthy"
    }
  }
}
```

## ?? Project Structure

```
GreaseMonkeyJournal/
??? GreaseMonkeyJournal.Api/          # Main Blazor application
?   ??? Components/                   # Blazor components and pages
?   ?   ??? DbContext/               # Entity Framework context
?   ?   ??? Models/                  # Data models
?   ?   ??? Pages/                   # Razor pages
?   ??? Extensions/                  # Service extensions
?   ??? Properties/                  # Launch settings
?   ??? wwwroot/                     # Static files
?   ??? Dockerfile                   # Application container
??? GreaseMonkeyJournal.Tests/       # Unit tests
??? docker-compose.yml               # Docker services configuration
??? docker-dev.ps1                   # Windows PowerShell dev script
??? docker-dev.sh                    # Linux/macOS dev script
??? init.sql                         # Database initialization
??? reset-docker.sh/.bat            # Environment reset scripts
??? README.md                        # This file
```

## ?? Troubleshooting

### Common Issues

#### Database Connection Errors
If you encounter "Access denied" errors:
1. **Reset the Docker environment**:
   ```bash
   ./reset-docker.sh  # or reset-docker.bat on Windows
   ```
2. **Check container status**:
   ```bash
   docker-compose ps
   ```
3. **View logs**:
   ```bash
   docker-compose logs mariadb
   docker-compose logs greasemonkeyjournal.api
   ```

#### Container Conflicts
If containers fail to start due to name conflicts:
```bash
# Remove conflicting containers
docker rm -f greasemonkey-mariadb
docker-compose up -d
```

#### Volume Issues
To completely reset the database:
```bash
docker-compose down -v  # Removes volumes
docker-compose up -d --build
```

#### PowerShell Execution Policy (Windows)
If you can't run the PowerShell script:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Useful Commands

```bash
# View running containers
docker-compose ps

# Follow application logs
docker-compose logs -f greasemonkeyjournal.api

# Access MariaDB console
docker exec -it greasemonkey-mariadb mariadb -u appuser -pyourpassword database

# Rebuild and restart services
docker-compose up -d --build

# Stop all services
docker-compose down

# Clean up everything (containers, networks, volumes)
docker-compose down -v --remove-orphans
```

## ?? Security Considerations

- **Default Passwords**: Change default passwords in production
- **Database Access**: Database user has minimal required permissions
- **Network Isolation**: Services communicate through isolated Docker network
- **HTTPS**: Enabled for development environment

## ?? Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test with Docker: `.\docker-dev.ps1` or `./docker-dev.sh`
5. Submit a pull request

## ?? License

This project is licensed under the MIT License - see the LICENSE file for details.

## ?? Support

If you encounter issues:
1. Check the [Troubleshooting](#??-troubleshooting) section
2. Review Docker logs: `docker-compose logs`
3. Create an issue in the GitHub repository

---

**Made with ?? for vehicle enthusiasts and maintenance professionals**