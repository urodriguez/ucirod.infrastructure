SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

IF DB_ID('UciRod.Infrastructure.Logging') IS NULL 
	CREATE DATABASE [UciRod.Infrastructure.Logging]
GO

IF DB_ID('UciRod.Infrastructure.Logging.Hangfire') IS NULL 
	CREATE DATABASE [UciRod.Infrastructure.Logging.Hangfire]
GO

USE [UciRod.Infrastructure.Logging]

BEGIN TRANSACTION

IF NOT EXISTS(SELECT 1 FROM sys.server_principals WHERE name = 'ucirod-infrastructure')
begin
	CREATE LOGIN [ucirod-infrastructure] WITH PASSWORD=N'Uc1R0d-1Nfr4$tructur3', DEFAULT_DATABASE=[master], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
end

IF NOT EXISTS(SELECT 1 FROM sys.database_principals WHERE name = 'ucirod-infrastructure')
begin
	CREATE USER [ucirod-infrastructure] FOR LOGIN [ucirod-infrastructure]
	ALTER USER [ucirod-infrastructure] WITH DEFAULT_SCHEMA=[dbo]
	ALTER ROLE [db_owner] ADD MEMBER [ucirod-infrastructure]
end

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE name = 'Log')
	CREATE TABLE [dbo].[Log](
		[Id] uniqueidentifier NOT NULL,
		[Application] nvarchar(32) NOT NULL,
		[Project] nvarchar(32) NULL,
		[CorrelationId] nvarchar(256) NULL,
		[Text] nvarchar(max) NULL,
		[Type] int NOT NULL,
		[CreationDate] datetime NOT NULL,
		[Environment] varchar(16) NOT NULL
	)

COMMIT TRANSACTION