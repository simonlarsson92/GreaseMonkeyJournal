-- Create database first
CREATE DATABASE IF NOT EXISTS `database`;

-- Drop users if they exist to avoid conflicts
DROP USER IF EXISTS 'appuser'@'%';
DROP USER IF EXISTS 'appuser'@'localhost';

-- Create the application user with explicit permissions
CREATE USER 'appuser'@'%' IDENTIFIED BY 'yourpassword';
CREATE USER 'appuser'@'localhost' IDENTIFIED BY 'yourpassword';

-- Grant full privileges to the application user
GRANT ALL PRIVILEGES ON `database`.* TO 'appuser'@'%';
GRANT ALL PRIVILEGES ON `database`.* TO 'appuser'@'localhost';

-- Also ensure root can connect from anywhere (for debugging)
GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' IDENTIFIED BY 'yourpassword' WITH GRANT OPTION;

-- Flush privileges
FLUSH PRIVILEGES;

-- Show users for debugging
SELECT User, Host FROM mysql.user WHERE User IN ('root', 'appuser');