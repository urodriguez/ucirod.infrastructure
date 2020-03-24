IF COL_LENGTH('dbo.[Audit]', 'Entity') IS NULL
	alter table audit add Entity varchar(max)
GO

IF COL_LENGTH('dbo.[Audit]', 'EntityId') IS NULL
	alter table audit add EntityId varchar(64)
GO

IF COL_LENGTH('dbo.[Audit]', 'Environment') IS NULL
	alter table audit add Environment varchar(16)
GO

IF COL_LENGTH('dbo.[Log]', 'Environment') IS NULL
	alter table dbo.[Log] add Environment varchar(16) NOT NULL
GO
