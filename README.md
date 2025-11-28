# Grease Monkey Journal

A comprehensive vehicle maintenance logging application built with .NET 9 Blazor Server and MariaDB.

## Overview

Grease Monkey Journal is a web-based application designed to help vehicle owners track and manage their maintenance records, service history, and upcoming reminders. Built with modern web technologies, it provides an intuitive interface for logging maintenance activities and monitoring vehicle health.

## Technology Stack

- **Frontend**: Blazor Server (.NET 9)
- **Backend**: ASP.NET Core 9
- **Database**: MariaDB 11.0
- **ORM**: Entity Framework Core
- **Containerization**: Docker & Docker Compose
- **Health Monitoring**: Built-in health checks

## Prerequisites

- [Docker](https://www.docker.com/get-started) (version 20.10 or later)
- [Docker Compose](https://docs.docker.com/compose/install/) (version 1.29 or later)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (for local development)
- [PowerShell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell) (for Windows development script)

## Security Setup (IMPORTANT)

### Environment Variables Configuration
This application uses environment variables for secure configuration management:

1. **Copy the environment template**:
   ```bash
   cp .env.example .env
   ```

2. **Update the `.env` file** with your secure passwords:
   ```env
   MYSQL_ROOT_PASSWORD=your_secure_root_password_here
   MYSQL_DATABASE=greasemonkey_journal
   MYSQL_USER=appuser
   MYSQL_PASSWORD=your_secure_app_password_here
   ```

3. **Generate strong passwords** - Use a password manager or generator to create secure passwords.

### Security Features
- **No hardcoded passwords** - All sensitive data uses environment variables  
- **Minimal database privileges** - Application user has only necessary permissions  
- **Root access restricted** - Database root user cannot connect remotely  
- **Docker network isolation** - Services communicate through isolated network  
- **Environment files excluded** - `.env` files are automatically ignored by Git  

## Quick Start with Docker

### 1. Clone the Repository
```bash
git clone https://github.com/simonlarsson92/GreaseMonkeyJournal.git
cd GreaseMonkeyJournal
```

### 2. Setup Environment Variables
```bash
# Copy the template
cp .env.example .env

# Edit the .env file with your secure passwords
# Use strong, unique passwords for production
```

### 3. Start the Application
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

**Or manually:**
```bash
docker-compose up -d --build
```

### 4. Access the Application
- **Web Application**: http://localhost:8080
- **Health Check**: http://localhost:8080/health
- **Database**: MariaDB accessible on localhost:3306

## Database Schema

The application includes a comprehensive schema for vehicle maintenance tracking:

### Core Tables
- **users** - Application users and authentication
- **vehicles** - Vehicle information (make, model, year, VIN, etc.)
- **maintenance_records** - Service records and maintenance history
- **maintenance_categories** - Predefined service categories
- **parts** - Parts catalog and inventory
- **maintenance_parts** - Parts used in maintenance records  
- **fuel_records** - Fuel consumption and cost tracking

### Features Supported
- Multi-user support with user isolation
- Comprehensive vehicle profiles
- Detailed maintenance history
- Parts tracking and costs
- Fuel consumption analysis
- Service reminders and scheduling
- Categorized maintenance types

## Docker Architecture

### Services

| Service | Container Name | Ports | Description |
|---------|----------------|-------|-------------|
| **greasemonkeyjournal.api** | greasemonkeyjournal-greasemonkeyjournal.api-1 | 8080:8080 | Main Blazor Server application |
| **mariadb** | greasemonkey-mariadb | 3306:3306 | MariaDB database server |
| **seq** | greasemonkey-seq | 5341:80, 5342:5341 | Seq log aggregation and analysis |

### Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `MYSQL_ROOT_PASSWORD` | MariaDB root password | `SecureRootPassword123!` |
| `MYSQL_DATABASE` | Database name | `greasemonkey_journal` |
| `MYSQL_USER` | Application database user | `appuser` |
| `MYSQL_PASSWORD` | Application user password | `SecureAppPassword456!` |
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core environment | `Development` |
| `APP_TITLE` | Application title | `Grease Monkey Journal` |
| `SEQ_URL` | Seq server URL (internal) | `http://seq:5341` |
| `SEQ_API_KEY` | Seq API key (optional) | `` |

### Docker Network
- **Network Name**: `greasemonkey-network`
- **Type**: Bridge network
- **Purpose**: Enables secure communication between containers

### Volumes
- **mariadb_data**: Persistent storage for MariaDB data
- **seq_data**: Persistent storage for Seq logs and configuration
- **init.sql**: Database initialization script with schema and security setup

## Database Configuration

### Connection Details (Development)
- **Server**: mariadb (Docker internal) / localhost (external)
- **Port**: 3306
- **Database**: greasemonkey_journal
- **User**: appuser
- **Password**: Set via environment variables

### Database Security
- Application user has limited privileges: `SELECT, INSERT, UPDATE, DELETE, CREATE, DROP, INDEX, ALTER`
- Root user access restricted to localhost only
- No remote root access allowed
- Strong password requirements enforced

## Logging with Seq

The application uses **Serilog** with **Seq** for centralized structured logging, making it easy to diagnose issues, track exceptions, and monitor application health.

### What is Seq?

Seq is a log aggregation and analysis tool designed for structured logging. It provides:
- **Powerful search and filtering** across all log events
- **Real-time log streaming** as events occur
- **Exception tracking** with full stack traces
- **Query-based dashboards** for monitoring
- **Alerting** on error patterns (Pro feature)

### Accessing Seq

Once your development environment is running:

1. **Seq Web UI**: http://localhost:5341
2. **No authentication required** in development (configured with `SEQ_FIRSTRUN_NOAUTHENTICATION=True`)

### Log Levels

The application uses these log levels:

- **Debug**: Method entry/exit, detailed flow (Development only)
- **Information**: Business events (vehicle created, log entry added, reminder completed)
- **Warning**: Validation failures, retry attempts, non-critical issues
- **Error**: Exceptions, failed operations
- **Critical**: Application startup failures, database unavailable

### Structured Logging

All logs include structured properties for easy querying:

```csharp
// Example log statement with structured properties
_logger.LogInformation("Added vehicle with ID: {VehicleId} for user {UserId}", vehicleId, userId);
```

**Common properties:**
- `VehicleId` - Vehicle entity ID
- `LogEntryId` - Log entry ID
- `ReminderId` - Reminder ID
- `MachineName` - Server/container name
- `EnvironmentName` - Development/Production

### Querying Logs in Seq

**Examples:**

```sql
-- Find all errors in the last hour
@Level = 'Error' and @Timestamp > Now()-1h

-- Find logs for a specific vehicle
VehicleId = 5

-- Find all database constraint violations
@Exception like '%DbUpdateException%'

-- Find slow operations (over 1 second)
Elapsed > 1000

-- Find all operations for VehicleService
SourceContext = 'GreaseMonkeyJournal.Api.Components.Services.VehicleService'
```

### Log Retention

By default, Seq stores logs indefinitely in the development environment. For production:

1. Configure retention policies in Seq UI: **Settings** → **Retention**
2. Set appropriate retention periods based on compliance requirements
3. Consider archiving important logs to external storage

### Viewing Logs

**Console Logs (Docker)**
```bash
# View all application logs
docker-compose logs -f greasemonkeyjournal.api

# View Seq container logs
docker-compose logs -f seq
```

**Seq Web UI**
1. Navigate to http://localhost:5341
2. Use the search bar to filter logs
3. Click on any log event for detailed information
4. Use the "Signals" feature to create reusable queries

## Development Setup

### Docker Development (Recommended)
```bash
# Setup environment
cp .env.example .env
# Edit .env with your passwords

# Start development environment
.\docker-dev.ps1  # Windows
./docker-dev.sh   # Linux/macOS

# View logs
docker-compose logs -f greasemonkeyjournal.api

# Reset environment (if needed)
.\reset-docker.ps1  # Windows
./reset-docker.sh   # Linux/macOS
```

### Local Development (without Docker)
1. **Start MariaDB** (ensure it's running on localhost:3306)
2. **Update Connection String** in `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "MariaDbConnection": "server=localhost;port=3306;database=greasemonkey_journal;user=appuser;password=your_password;"
     }
   }
   ```
3. **Run the Application**:
   ```bash
   cd GreaseMonkeyJournal.Api
   dotnet run
   ```

## Health Monitoring

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
      "status": "Healthy"
    }
  }
}
```

## Troubleshooting

### Common Issues

#### Environment Variable Errors
If you encounter configuration errors:
1. **Check your `.env` file exists**:
   ```bash
   ls -la .env
   ```
2. **Verify environment variables are set**:
   ```bash
   docker-compose config
   ```
3. **Recreate from template**:
   ```bash
   cp .env.example .env
   # Edit with your values
   ```

#### Database Connection Errors
If you encounter "Access denied" errors:
1. **Reset the Docker environment**:
   ```bash
   .\reset-docker.ps1  # Windows
   ./reset-docker.sh   # Linux/macOS
   ```
2. **Check environment variables**:
   ```bash
   docker-compose config
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
docker rm -f greasemonkey-mariadb greasemonkey-seq
docker-compose up -d --build
```

#### Seq Not Receiving Logs
If Seq is running but not showing logs:
1. **Check Seq container is running**:
   ```bash
   docker-compose ps seq
   ```
2. **Verify Seq URL in environment**:
   ```bash
   docker-compose config | grep SEQ_URL
   ```
3. **Check application logs for Serilog errors**:
   ```bash
   docker-compose logs greasemonkeyjournal.api | grep -i serilog
   ```
4. **Restart the application**:
   ```bash
   docker-compose restart greasemonkeyjournal.api
   ```

### Useful Commands

```bash
# View environment variables being used
docker-compose config

# View running containers
docker-compose ps

# Follow application logs
docker-compose logs -f greasemonkeyjournal.api

# Access MariaDB console (using environment variables)
docker exec -it greasemonkey-mariadb mariadb -u appuser -p greasemonkey_journal

# Rebuild and restart services
docker-compose up -d --build

# Stop all services
docker-compose down

# Clean up everything (containers, networks, volumes)
docker-compose down -v --remove-orphans
```

## Production Deployment

For production environments:

1. **Use Docker Secrets** instead of environment variables:
   ```yaml
   secrets:
     db_password:
       file: ./secrets/db_password.txt
   ```

2. **Enable HTTPS** with proper certificates
3. **Use external database** with backup/replication
4. **Implement proper logging** and monitoring
5. **Set up reverse proxy** (nginx, Traefik, etc.)
6. **Configure firewall rules** and network security

## Project Structure

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
??? .env.example                     # Environment variables template
??? .env                             # Environment variables (not in Git)
??? docker-compose.yml               # Docker services configuration
??? docker-dev.ps1                   # Windows PowerShell dev script
??? docker-dev.sh                    # Linux/macOS dev script
??? init.sql                         # Database initialization
??? reset-docker.ps1                 # Windows PowerShell reset script
??? reset-docker.sh                  # Linux/macOS reset script
??? README.md                        # This file
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test with Docker: `.\docker-dev.ps1` or `./docker-dev.sh`
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

If you encounter issues:
1. Check the [Troubleshooting](#troubleshooting) section
2. Review Docker logs: `docker-compose logs`
3. Verify environment variables: `docker-compose config`
4. Create an issue in the GitHub repository

---

**Made with care for vehicle enthusiasts and maintenance professionals**