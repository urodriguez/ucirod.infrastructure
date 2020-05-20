DECLARE @isDevEnv bit = 1

IF @isDevEnv = 1
	USE [UciRod.Infrastructure.Logging]
ELSE
	USE [UciRod.Infrastructure.Logging-Test]

SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

BEGIN TRANSACTION

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

IF @isDevEnv = 1
begin
	IF DB_ID('UciRod.Infrastructure.Logging.Hangfire') IS NULL 
		CREATE DATABASE [UciRod.Infrastructure.Logging.Hangfire]
end
ELSE 
begin
	IF DB_ID('UciRod.Infrastructure.Logging.Hangfire-Test') IS NULL 
		CREATE DATABASE [UciRod.Infrastructure.Logging.Hangfire-Test]
end

COMMIT TRANSACTION