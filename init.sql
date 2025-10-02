-- Create database first (database name will be set by MYSQL_DATABASE environment variable)
-- The database is automatically created by MariaDB when MYSQL_DATABASE is set

-- Get the database name from environment variable (MariaDB will have already created it)
-- For this script, we'll use the database that MariaDB creates from MYSQL_DATABASE

-- Use a dynamic approach to get the current database
SET @db_name = (SELECT DATABASE());

-- If no database is selected, use a default
SET @db_name = IFNULL(@db_name, 'greasemonkey_journal');

-- Create some basic tables for your application
-- Grease Monkey Journal specific schema
CREATE TABLE IF NOT EXISTS `users` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `username` VARCHAR(50) UNIQUE NOT NULL,
    `email` VARCHAR(100) UNIQUE NOT NULL,
    `password_hash` VARCHAR(255) NOT NULL,
    `first_name` VARCHAR(50),
    `last_name` VARCHAR(50),
    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `is_active` BOOLEAN DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS `vehicles` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `user_id` INT NOT NULL,
    `make` VARCHAR(50) NOT NULL,
    `model` VARCHAR(50) NOT NULL,
    `year` INT NOT NULL,
    `vin` VARCHAR(17) UNIQUE,
    `license_plate` VARCHAR(20),
    `color` VARCHAR(30),
    `engine_size` VARCHAR(20),
    `fuel_type` ENUM('Gasoline', 'Diesel', 'Electric', 'Hybrid') DEFAULT 'Gasoline',
    `transmission` ENUM('Manual', 'Automatic', 'CVT') DEFAULT 'Automatic',
    `odometer` INT DEFAULT 0,
    `purchase_date` DATE,
    `purchase_price` DECIMAL(10,2),
    `current_value` DECIMAL(10,2),
    `notes` TEXT,
    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `is_active` BOOLEAN DEFAULT TRUE,
    FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE CASCADE,
    INDEX `idx_user_vehicles` (`user_id`),
    INDEX `idx_vin` (`vin`)
);

CREATE TABLE IF NOT EXISTS `maintenance_categories` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(50) NOT NULL UNIQUE,
    `description` TEXT,
    `color` VARCHAR(7) DEFAULT '#007bff', -- Bootstrap primary blue
    `icon` VARCHAR(50) DEFAULT 'wrench',
    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS `maintenance_records` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `vehicle_id` INT NOT NULL,
    `category_id` INT,
    `service_type` VARCHAR(100) NOT NULL,
    `description` TEXT,
    `date_performed` DATE NOT NULL,
    `odometer_reading` INT,
    `cost` DECIMAL(10,2) DEFAULT 0.00,
    `labor_cost` DECIMAL(10,2) DEFAULT 0.00,
    `parts_cost` DECIMAL(10,2) DEFAULT 0.00,
    `service_provider` VARCHAR(100),
    `invoice_number` VARCHAR(50),
    `warranty_until` DATE,
    `next_service_date` DATE,
    `next_service_odometer` INT,
    `notes` TEXT,
    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (`vehicle_id`) REFERENCES `vehicles`(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`category_id`) REFERENCES `maintenance_categories`(`id`) ON DELETE SET NULL,
    INDEX `idx_vehicle_maintenance` (`vehicle_id`),
    INDEX `idx_date_performed` (`date_performed`),
    INDEX `idx_service_type` (`service_type`)
);

CREATE TABLE IF NOT EXISTS `parts` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(100) NOT NULL,
    `part_number` VARCHAR(50),
    `brand` VARCHAR(50),
    `description` TEXT,
    `unit_price` DECIMAL(10,2),
    `supplier` VARCHAR(100),
    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX `idx_part_number` (`part_number`)
);

CREATE TABLE IF NOT EXISTS `maintenance_parts` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `maintenance_record_id` INT NOT NULL,
    `part_id` INT NOT NULL,
    `quantity` INT NOT NULL DEFAULT 1,
    `unit_price` DECIMAL(10,2),
    `total_price` DECIMAL(10,2),
    FOREIGN KEY (`maintenance_record_id`) REFERENCES `maintenance_records`(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`part_id`) REFERENCES `parts`(`id`) ON DELETE CASCADE,
    INDEX `idx_maintenance_parts` (`maintenance_record_id`)
);

CREATE TABLE IF NOT EXISTS `fuel_records` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `vehicle_id` INT NOT NULL,
    `date_filled` DATE NOT NULL,
    `odometer_reading` INT NOT NULL,
    `gallons` DECIMAL(8,3) NOT NULL,
    `price_per_gallon` DECIMAL(6,3),
    `total_cost` DECIMAL(8,2),
    `gas_station` VARCHAR(100),
    `fuel_grade` VARCHAR(20) DEFAULT 'Regular',
    `is_full_tank` BOOLEAN DEFAULT TRUE,
    `notes` TEXT,
    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (`vehicle_id`) REFERENCES `vehicles`(`id`) ON DELETE CASCADE,
    INDEX `idx_vehicle_fuel` (`vehicle_id`),
    INDEX `idx_date_filled` (`date_filled`)
);

-- Insert default maintenance categories
INSERT IGNORE INTO `maintenance_categories` (`name`, `description`, `color`, `icon`) VALUES
('Oil Change', 'Engine oil and filter changes', '#28a745', 'oil-can'),
('Tire Service', 'Tire rotation, replacement, and maintenance', '#fd7e14', 'circle'),
('Brake Service', 'Brake pad, rotor, and fluid service', '#dc3545', 'stop-circle'),
('Engine', 'Engine repairs and maintenance', '#6f42c1', 'engine'),
('Transmission', 'Transmission service and repairs', '#e83e8c', 'gear'),
('Cooling System', 'Radiator, coolant, and temperature control', '#17a2b8', 'thermometer'),
('Electrical', 'Battery, alternator, and electrical system', '#ffc107', 'lightning'),
('Suspension', 'Shocks, struts, and suspension components', '#6c757d', 'arrows-vertical'),
('Exhaust', 'Exhaust system and emissions', '#343a40', 'wind'),
('General Maintenance', 'Routine maintenance and inspections', '#007bff', 'tools');

-- Show current database info for debugging
SELECT DATABASE() as 'Current Database';
SHOW TABLES;

-- Show user info (non-sensitive)
SELECT User, Host FROM mysql.user WHERE User = 'appuser' OR User = 'root';

-- Show table structures for verification
DESCRIBE vehicles;
DESCRIBE maintenance_records;