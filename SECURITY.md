# Security Configuration Guide

## Overview
This document outlines the security improvements implemented for the Grease Monkey Journal Docker setup.

## Security Issues Addressed

### Before (Insecure)
- Hardcoded passwords in configuration files
- Root user with remote access (`'root'@'%'`)
- All privileges granted to application user
- Database passwords visible in version control

### After (Secure)
- Environment variables for all sensitive data
- Root access restricted to localhost only
- Minimal privileges for application user
- Passwords excluded from version control
- Strong password requirements documented

## Environment Variables Setup

### Required Files

1. **`.env`** - Your local environment configuration (not committed to Git)
2. **`.env.example`** - Template for team members (committed to Git)

### Environment Variables Reference

| Variable | Purpose | Example | Security Level |
|----------|---------|---------|---------------|
| `MYSQL_ROOT_PASSWORD` | MariaDB root password | `SecureRootPass123!` | Critical |
| `MYSQL_PASSWORD` | Application user password | `AppUserPass456!` | Critical |
| `MYSQL_DATABASE` | Database name | `greasemonkey_journal` | Low |
| `MYSQL_USER` | Application user | `appuser` | Medium |
| `APP_TITLE` | Application title | `Grease Monkey Journal` | Low |

## Database Security Features

### User Privileges
The application user (`appuser`) has minimal required privileges:
- `SELECT` - Read data
- `INSERT` - Add new records
- `UPDATE` - Modify existing records
- `DELETE` - Remove records
- `CREATE` - Create tables (for migrations)
- `DROP` - Drop tables (for migrations)
- `INDEX` - Create/drop indexes
- `ALTER` - Modify table structure

### Root User Security
- Root access restricted to `localhost` only
- No remote root connections allowed
- Root cannot connect from Docker containers or external hosts

### Database Schema Security
- Foreign key constraints prevent orphaned records
- Proper indexing for performance and security
- User isolation through proper table relationships

## Production Security Recommendations

### 1. Password Management
```bash
# Generate strong passwords
openssl rand -base64 32

# Or use a password manager
# Examples: 1Password, Bitwarden, LastPass
```

### 2. Docker Secrets (Production)
Replace environment variables with Docker secrets:

```yaml
# docker-compose.prod.yml
services:
  mariadb:
    secrets:
      - db_root_password
      - db_password
    environment:
      MYSQL_ROOT_PASSWORD_FILE: /run/secrets/db_root_password
      MYSQL_PASSWORD_FILE: /run/secrets/db_password

secrets:
  db_root_password:
    file: ./secrets/db_root_password.txt
  db_password:
    file: ./secrets/db_password.txt
```

### 3. SSL/TLS Configuration
Enable encrypted database connections:

```bash
# Connection string with SSL
server=mariadb;port=3306;database=greasemonkey_journal;user=appuser;password=${MYSQL_PASSWORD};SslMode=Required;
```

### 4. Network Security
- Use Docker networks for service isolation
- Implement reverse proxy (nginx, Traefik)
- Configure firewall rules
- Enable container-to-container encryption

### 5. Monitoring and Auditing
- Enable MariaDB audit plugin
- Monitor failed login attempts
- Log all database connections
- Set up alerting for suspicious activity

## Quick Setup Commands

### Initial Setup
```bash
# 1. Copy environment template
cp .env.example .env

# 2. Generate secure passwords
echo "MYSQL_ROOT_PASSWORD=$(openssl rand -base64 32)" >> .env
echo "MYSQL_PASSWORD=$(openssl rand -base64 32)" >> .env

# 3. Edit other variables as needed
nano .env

# 4. Start the application
docker-compose up -d --build
```

### Rotation of Passwords
```bash
# 1. Stop services
docker-compose down

# 2. Update .env with new passwords
nano .env

# 3. Remove database volume to reset with new passwords
docker volume rm greasemonkeyjournal_mariadb_data

# 4. Restart with new configuration
docker-compose up -d --build
```

## Security Verification

### Check Environment Variables
```bash
# View resolved configuration (passwords will be visible)
docker-compose config

# Check if .env is properly excluded from Git
git status --ignored
```

### Verify Database Security
```bash
# Connect to MariaDB and check users
docker exec -it greasemonkey-mariadb mariadb -u root -p

# In MariaDB console:
SELECT User, Host FROM mysql.user;
SHOW GRANTS FOR 'appuser'@'%';
```

### Test Connections
```bash
# Should work - application user
docker exec -it greasemonkey-mariadb mariadb -u appuser -p greasemonkey_journal

# Should fail - root remote access
mysql -h localhost -P 3306 -u root -p
```

## Security Checklist

- [ ] `.env` file created with strong passwords
- [ ] `.env` excluded from version control
- [ ] Environment variables used in docker-compose.yml
- [ ] Hardcoded passwords removed from configuration files
- [ ] Database user has minimal required privileges  
- [ ] Root remote access disabled
- [ ] Docker secrets configured for production
- [ ] SSL/TLS enabled for database connections
- [ ] Network security configured
- [ ] Monitoring and alerting set up

## Emergency Procedures

### Password Compromise
1. **Immediate Actions**:
   ```bash
   # Stop all services
   docker-compose down
   
   # Change passwords in .env file
   nano .env
   
   # Reset database with new passwords
   docker volume rm greasemonkeyjournal_mariadb_data
   docker-compose up -d --build
   ```

2. **Investigation**:
   - Check access logs
   - Review recent database activity
   - Audit user accounts and permissions

### Security Incident Response
1. **Isolate**: Stop affected containers
2. **Assess**: Determine scope of compromise
3. **Contain**: Change all credentials
4. **Recover**: Restore from secure backups
5. **Learn**: Update security procedures

---

**Security is everyone's responsibility - follow these guidelines to keep your application secure!**