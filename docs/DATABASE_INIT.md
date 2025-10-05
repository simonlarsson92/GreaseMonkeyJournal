# Database Initialization with Environment Variables

## Overview

The Grease Monkey Journal database initialization has been updated to use environment variables from the `.env` file for all user credentials and database configuration. This provides better security and eliminates hardcoded passwords.

## Environment Variables

The following environment variables are used from the `.env` file:

```bash
# Database Configuration
MYSQL_ROOT_PASSWORD=DevPassword123!          # MariaDB root password
MYSQL_DATABASE=greasemonkey_journal          # Database name
MYSQL_USER=appuser                          # Application database user
MYSQL_PASSWORD=AppUserPassword456!          # Application user password
```

## How It Works

### 1. Docker Container Integration

MariaDB Docker container automatically:
- Creates the database specified in `MYSQL_DATABASE`
- Creates the user specified in `MYSQL_USER` with `MYSQL_PASSWORD`
- Sets the root password to `MYSQL_ROOT_PASSWORD`

### 2. Initialization Script

#### `init.sql` (Database Setup)
- Pure SQL script that works with MariaDB's automatic user creation
- Sets proper permissions for the application user
- Handles database and user initialization only
- Schema creation is handled by Entity Framework Core migrations
- Includes comprehensive documentation and verification queries

### 3. Execution Order

1. MariaDB container starts with environment variables
2. Container creates database and users automatically
3. `init.sql` runs to set proper permissions
4. .NET application connects and runs EF Core migrations
5. Application handles table creation and data seeding

## Security Features

- **No Hardcoded Passwords**: All credentials come from environment variables  
- **Least Privilege**: Application user only has access to the application database  
- **Environment File Exclusion**: `.env` files are automatically ignored by Git  
- **Docker Network Isolation**: Database only accessible within Docker network  
- **Root Access Control**: Root user properly configured but restricted  

## Usage Instructions

### 1. Configure Environment Variables

```bash
# Copy the template
cp .env.example .env

# Edit with your secure passwords
# IMPORTANT: Use strong, unique passwords for production!
```

### 2. Start the Application

```bash
# Windows PowerShell
.\docker-dev.ps1

# Linux/macOS  
./docker-dev.sh

# Or manually
docker-compose up -d --build
```

### 3. Verify Installation

The initialization script includes verification queries that show:
- Current database name
- Created users and their roles
- Database readiness for Entity Framework migrations

Check the MariaDB container logs:
```bash
docker-compose logs mariadb
```

## File Structure

```
GreaseMonkeyJournal/
??? .env                    # Environment variables (not in Git)
??? .env.example           # Template file (in Git)
??? init.sql              # Database initialization script
??? docker-compose.yml    # Docker services configuration
??? docs/
    ??? DATABASE_INIT.md  # This documentation
```

## Troubleshooting

### Common Issues

1. **Environment Variables Not Loading**
   - Ensure `.env` file exists in project root
   - Verify variable names match exactly
   - Check for trailing spaces or special characters

2. **Database Connection Errors**
   ```bash
   # Reset Docker environment
   docker-compose down -v
   docker-compose up -d --build
   ```

3. **User Access Issues**
   ```bash
   # Check MariaDB logs
   docker-compose logs mariadb
   
   # Connect to database for debugging
   docker exec -it greasemonkey-mariadb mariadb -u appuser -p greasemonkey_journal
   ```

### Verification Commands

```bash
# Check environment variables being used
docker-compose config

# View initialization logs
docker-compose logs mariadb | grep -i init

# Test database connectivity
docker exec -it greasemonkey-mariadb mariadb -u appuser -p greasemonkey_journal -e "SELECT 1;"
```

## Entity Framework Integration

Since table creation is handled by the .NET application:

### Migration Commands
```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# View migration status
dotnet ef migrations list
```

### Application Startup
The application automatically runs migrations on startup with retry logic for database availability.

## Production Considerations

For production deployment:

1. **Use Docker Secrets** instead of environment variables:
   ```yaml
   secrets:
     db_password:
       file: ./secrets/db_password.txt
   ```

2. **External Database**: Consider using managed database services
3. **Backup Strategy**: Implement regular database backups
4. **Monitoring**: Add database health monitoring
5. **Network Security**: Use proper firewall rules and VPNs

## Migration from Previous Version

If upgrading from the previous hardcoded version:

1. **Backup existing data**:
   ```bash
   docker exec greasemonkey-mariadb mariadb-dump -u root -p greasemonkey_journal > backup.sql
   ```

2. **Update configuration**:
   - Create `.env` file with your passwords
   - Update `docker-compose.yml` if needed

3. **Restart with new configuration**:
   ```bash
   docker-compose down
   docker-compose up -d --build
   ```

4. **Restore data if needed**:
   ```bash
   docker exec -i greasemonkey-mariadb mariadb -u appuser -p greasemonkey_journal < backup.sql
   ```