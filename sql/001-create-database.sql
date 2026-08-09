-- ============================================
-- StayHub
-- 001-create-database.sql
-- Creates the StayHubDB database
-- ============================================
-- The schema itself is owned by EF Core migrations, which run automatically on
-- application startup. This script only guarantees the database exists so the
-- migration has somewhere to apply.
-- ============================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'StayHubDB')
BEGIN
    CREATE DATABASE StayHubDB;
    PRINT 'StayHubDB database created successfully.';
END
ELSE
BEGIN
    PRINT 'StayHubDB already exists; no action taken.';
END
GO
