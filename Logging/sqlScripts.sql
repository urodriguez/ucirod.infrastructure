DECLARE @isDevEnv bit = 1

IF COL_LENGTH('dbo.[Log]', 'Environment') IS NULL
	alter table dbo.[Log] add Environment varchar(16) NOT NULL

alter table dbo.[Log] alter column CorrelationId nvarchar(256)

If @isDevEnv = 1
Begin
	IF DB_ID('UciRod.Infrastructure.Logging.Hangfire') IS NULL 
		CREATE DATABASE [UciRod.Infrastructure.Logging.Hangfire]
End
Else 
Begin
	IF DB_ID('UciRod.Infrastructure.Logging.Hangfire-Test') IS NULL 
		CREATE DATABASE [UciRod.Infrastructure.Logging.Hangfire-Test]
End