SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

IF DB_ID('UciRod.Infrastructure.Auditing') IS NULL 
	CREATE DATABASE [UciRod.Infrastructure.Auditing]
GO

USE [UciRod.Infrastructure.Auditing]

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

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE name = 'Audit')
	CREATE TABLE [dbo].[Audit](
		[Id] uniqueidentifier NOT NULL,
		[Application] varchar(64) NOT NULL,
		[User] varchar(64) NOT NULL,
		[Changes] varchar(max) NOT NULL,
		[EntityName] varchar(64) NOT NULL,
		[Action] int NOT NULL,
		[CreationDate] smalldatetime NOT NULL,
		[Entity] varchar(max) NULL,
		[EntityId] varchar(64) NULL,
		[Environment] varchar(16) NULL
	)

COMMIT TRANSACTION