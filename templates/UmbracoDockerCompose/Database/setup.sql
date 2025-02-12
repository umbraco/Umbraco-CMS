USE [master]
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'UmbracoDb')
    BEGIN
        CREATE DATABASE [umbracoDb]
    END;
GO

USE UmbracoDb;