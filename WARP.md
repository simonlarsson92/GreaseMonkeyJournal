# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

**Grease Monkey Journal** is a vehicle maintenance logging web application built with:
- **.NET 9 Blazor Server** - Interactive web UI
- **MariaDB 11.0** - Database (via Entity Framework Core)
- **Docker & Docker Compose** - Containerized development and deployment

The application tracks vehicle maintenance records, service history, fuel consumption, and maintenance reminders.

## Common Development Commands

### Building and Running

```powershell
# Start development environment (builds and runs all services)
.\docker-dev.ps1

# Or manually with Docker Compose
docker-compose up -d --build
```

### Testing

```bash
# Run all tests
dotnet test GreaseMonkeyJournal.sln --verbosity normal

# Run tests for specific project
dotnet test GreaseMonkeyJournal.Tests/GreaseMonkeyJournal.Tests.csproj

# Run tests with coverage
dotnet test GreaseMonkeyJournal.sln --collect:"XPlat Code Coverage"
```

### Building without Docker

```bash
# Restore dependencies
dotnet restore GreaseMonkeyJournal.sln

# Build solution
dotnet build GreaseMonkeyJournal.sln

# Run API project locally
cd GreaseMonkeyJournal.Api
dotnet run
```

### Database Management

```bash
# Add new EF Core migration
cd GreaseMonkeyJournal.Api
dotnet ef migrations add MigrationName

# Remove last migration
dotnet ef migrations remove

# Update database manually (migrations run automatically on app startup)
dotnet ef database update

# View database logs
docker-compose logs mariadb

# Access MariaDB console
docker exec -it greasemonkey-mariadb mariadb -u appuser -p greasemonkey_journal
```

### Docker Operations

```powershell
# View all logs
docker-compose logs -f

# View app logs only
docker-compose logs -f greasemonkeyjournal.api

# Stop all services
docker-compose down

# Reset environment (removes volumes and containers)
.\reset-docker.ps1

# View environment configuration
docker-compose config

# View running containers
docker-compose ps
```

## Architecture

### Project Structure

```
GreaseMonkeyJournal/
├── GreaseMonkeyJournal.Api/          # Main Blazor Server application
│   ├── Components/
│   │   ├── DbContext/                # EF Core DbContext
│   │   ├── Models/                   # Data models (Vehicle, LogEntry, Reminder)
│   │   ├── Services/                 # Business logic services
│   │   ├── Pages/                    # Blazor pages/components
│   │   └── Layout/                   # Layout components
│   ├── Extensions/                   # Service registration extensions
│   ├── Migrations/                   # EF Core migrations
│   └── wwwroot/                      # Static files
└── GreaseMonkeyJournal.Tests/        # xUnit tests (services + components)
```

### Service Layer Architecture

The application follows a **service layer pattern** with interface-based dependency injection:

**Component → Interface → Service → DbContext → Database**

#### Core Services

1. **IVehicleService** / **VehicleService**
   - Manages CRUD operations for vehicles
   - Methods: `GetAllAsync()`, `GetByIdAsync()`, `AddAsync()`, `UpdateAsync()`, `DeleteAsync()`

2. **ILogEntryService** / **LogEntryService**
   - Handles maintenance log entries
   - Methods include vehicle-specific queries: `GetByVehicleIdAsync()`

3. **IReminderService** / **ReminderService**
   - Manages maintenance reminders
   - Has dependency on `ILogEntryService` for completing reminders
   - Special method: `CompleteReminderAsync()` marks reminder complete and creates log entry

#### Dependency Injection

All services are registered using extension methods in `Extensions/ServiceCollectionExtensions.cs`:

```csharp
builder.Services.AddApplicationServices();
```

Services use **Scoped** lifetime (one instance per HTTP request) to align with EF Core DbContext lifecycle.

### Database Architecture

**Entity Framework Core** with code-first migrations manages the database schema.

#### Core Models

- **Vehicle** - Vehicle information (make, model, year, VIN, odometer)
- **LogEntry** - Maintenance records (linked to Vehicle via foreign key)
- **Reminder** - Upcoming maintenance reminders (linked to Vehicle)

#### DbContext

`VehicleLogDbContext` is the single DbContext with three DbSets. Relationships are configured in `OnModelCreating()`.

#### Migrations

- Migrations are in `GreaseMonkeyJournal.Api/Migrations/`
- **Migrations run automatically on application startup** via `Program.cs` with retry logic
- Initial schema created by `20250610125352_InitialCreate.cs`

### Environment Configuration

**All sensitive configuration uses environment variables** (never hardcoded):

1. Copy `.env.example` to `.env`
2. Set secure passwords in `.env` file
3. Docker Compose loads variables automatically

Key variables:
- `MYSQL_ROOT_PASSWORD`, `MYSQL_PASSWORD` - Database credentials
- `MYSQL_DATABASE`, `MYSQL_USER` - Database configuration
- `DB_SERVER`, `DB_PORT` - Connection settings
- `ASPNETCORE_ENVIRONMENT` - Environment (Development/Production)

## Blazor Component Conventions

Following patterns from `.github/copilot.md`:

### Component Structure

- Use `@inject` for dependency injection of services
- Separate complex logic into `.razor.cs` code-behind files
- Service injection example:
  ```razor
  @inject IVehicleService VehicleService
  @inject ILogEntryService LogEntryService
  ```

### Key Components

- **Vehicles.razor** - Vehicle list/management
- **VehicleLog.razor** - View maintenance logs for a vehicle
- **VehicleReminders.razor** - Manage reminders for a vehicle
- **CreateVehicle.razor**, **EditVehicle.razor** - Vehicle CRUD
- **AddLogEntryDialog.razor** - Add new maintenance entries

## Testing Approach

Uses **xUnit** with **Moq** for mocking and **Microsoft.EntityFrameworkCore.InMemory** for database tests.

### Test Organization

- **Service Tests** - Unit tests for business logic with mocked dependencies
  - `VehicleServiceTests.cs`, `LogEntryServiceTests.cs`, `ReminderServiceTests.cs`
- **Component Tests** - Blazor component tests
  - `VehiclesTests.cs`, `CreateVehicleTests.cs`, etc.
- **Integration Tests** - Dependency injection and service registration
  - `DependencyInjectionIntegrationTests.cs`

### Testing Pattern

Services are tested in isolation using mocked dependencies:

```csharp
var mockLogEntryService = new Mock<ILogEntryService>();
mockLogEntryService.Setup(x => x.AddAsync(It.IsAny<LogEntry>())).Returns(Task.CompletedTask);
var service = new ReminderService(context, mockLogEntryService.Object);
```

## Coding Standards

Based on `.github/copilot.md`:

- **Naming**: PascalCase for classes/methods, `_camelCase` for private fields
- **Async/Await**: Use for all I/O operations
- **Dependency Injection**: Constructor injection, null parameter validation
- **XML Documentation**: Required on all public interfaces and methods
- **Error Handling**: Null checks with `ArgumentNullException`, meaningful exceptions

### SOLID Principles

- **Single Responsibility**: Each service handles one domain area
- **Dependency Inversion**: Components depend on interfaces, not concrete implementations
- **Interface Segregation**: Focused, specific interfaces for each service

## Important Implementation Notes

### Database Connection Retry Logic

The application includes retry logic in `Program.cs` (10 retries with 5-second delays) to handle database startup delays in Docker environments.

### Health Checks

- Endpoint: `/health`
- Checks: Database connectivity via EF Core health check
- Added via `builder.Services.AddHealthChecks().AddDbContextCheck<VehicleLogDbContext>()`

### Service Relationship

`ReminderService` depends on `ILogEntryService` - when completing a reminder, it creates a corresponding log entry. This dependency is injected through the constructor.

### Security

- No hardcoded credentials (all via environment variables)
- `.env` files excluded from Git
- Database user has minimal required privileges
- Docker network isolation between services

## Development Workflow

1. **Setup**: Copy `.env.example` to `.env` and set passwords
2. **Start Services**: Run `.\docker-dev.ps1` (Windows) or `./docker-dev.sh` (Linux/macOS)
3. **Access App**: Navigate to http://localhost:8080
4. **Database Migrations**: Run automatically on startup (or manually via `dotnet ef`)
5. **Testing**: Run `dotnet test GreaseMonkeyJournal.sln` before commits
6. **Reset Environment**: Use `.\reset-docker.ps1` if database state becomes corrupted

## Common Issues

### "Access denied" database errors
Run `.\reset-docker.ps1` to reset the Docker environment and recreate the database with proper credentials.

### Service won't start
Check logs: `docker-compose logs -f greasemonkeyjournal.api`

### Database connection timeouts
Increase `DB_CONNECTION_TIMEOUT` in `.env` file or check if MariaDB container is healthy: `docker-compose ps`
