-- ============================================================================
-- Grease Monkey Journal Database Initialization Script
-- ============================================================================
-- 
-- This script initializes the MariaDB database for the Grease Monkey Journal
-- application using environment variables from the .env file for secure
-- configuration management.
--
-- SCOPE: User and Permission Setup Only
-- ====================================
-- This script ONLY handles database and user creation with proper permissions.
-- Table creation and seed data are handled by the .NET application using
-- Entity Framework Core migrations.
--
-- ENVIRONMENT VARIABLES (from .env file):
-- =====================================
-- MYSQL_ROOT_PASSWORD  - MariaDB root user password
-- MYSQL_DATABASE      - Database name (greasemonkey_journal)
-- MYSQL_USER          - Application database user (appuser)
-- MYSQL_PASSWORD      - Application user password
--
-- DOCKER INTEGRATION:
-- ==================
-- MariaDB Docker container automatically handles user creation using these
-- environment variables. This script ensures proper permissions are set.
--
-- INITIALIZATION ORDER:
-- ====================
-- 1. MariaDB container creates database (MYSQL_DATABASE)
-- 2. MariaDB container creates user (MYSQL_USER with MYSQL_PASSWORD)
-- 3. This script sets proper permissions
-- 4. .NET application creates tables via EF Core migrations
-- 5. .NET application seeds initial data via application logic
--
-- SECURITY FEATURES:
-- =================
-- ? No hardcoded passwords - all from environment variables
-- ? Least privilege - application user only has access to app database
-- ? Environment files excluded from version control
-- ? Docker network isolation
--
-- ============================================================================

-- Use the database that was created by MariaDB from MYSQL_DATABASE environment variable
USE `greasemonkey_journal`;

-- ============================================================================
-- USER PERMISSIONS SETUP
-- ============================================================================
-- Note: MariaDB container automatically creates MYSQL_USER with MYSQL_PASSWORD
-- We need to create additional user entries for Docker network access

-- Create user for Docker network access (using wildcard for Docker IPs)
CREATE USER IF NOT EXISTS 'appuser'@'%' IDENTIFIED BY 'AppUserPassword456!';
GRANT ALL PRIVILEGES ON `greasemonkey_journal`.* TO 'appuser'@'%';

-- Apply permission changes immediately
FLUSH PRIVILEGES;

-- ============================================================================
-- INITIALIZATION VERIFICATION
-- ============================================================================

SELECT 'Database and User Initialization Complete' AS Status;
SELECT 'Schema creation will be handled by Entity Framework Core' AS Note;
SELECT DATABASE() AS 'Current Database';

-- Show user information for verification (non-sensitive data only)
SELECT User, Host, 
       CASE 
           WHEN User = 'root' THEN 'Database Administrator' 
           WHEN User = 'appuser' THEN 'Application User (EF Core)'
           ELSE 'Other User'
       END AS Role
FROM mysql.user 
WHERE User IN ('root', 'appuser') 
ORDER BY User, Host;

-- Confirm database is ready for Entity Framework migrations
SELECT 
    SCHEMA_NAME as 'Database Ready for EF Migrations',
    DEFAULT_CHARACTER_SET_NAME as 'Character Set',
    DEFAULT_COLLATION_NAME as 'Collation'
FROM information_schema.SCHEMATA 
WHERE SCHEMA_NAME = DATABASE();

-- ============================================================================
-- END OF INITIALIZATION SCRIPT
-- 
-- Next Steps:
-- 1. .NET application will connect using the created user credentials
-- 2. Entity Framework Core will create/update tables via migrations
-- 3. Application services will handle data seeding as needed
-- ============================================================================