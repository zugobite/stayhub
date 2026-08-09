-- ============================================
-- StayHub
-- 002-drop-database.sql
-- Drops StayHubDB for a clean re-run of the migrations
-- ============================================
-- Destructive. Run this only when you want the next application start to
-- rebuild the schema and reseed the demonstration data from scratch.
-- ============================================

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'StayHubDB')
BEGIN
    ALTER DATABASE StayHubDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE StayHubDB;
    PRINT 'StayHubDB dropped.';
END
GO
