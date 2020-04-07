IF COL_LENGTH('dbo.[Log]', 'Environment') IS NULL
	alter table dbo.[Log] add Environment varchar(16) NOT NULL
GO
