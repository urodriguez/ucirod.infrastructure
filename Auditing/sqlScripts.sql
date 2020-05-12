DECLARE @isDevEnv bit = 1

IF @isDevEnv = 1
	USE [UciRod.Infrastructure.Auditing]
ELSE
	USE [UciRod.Infrastructure.Auditing-Test]

SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

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