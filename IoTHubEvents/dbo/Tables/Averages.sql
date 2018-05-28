CREATE TABLE [dbo].[Averages]
(
	[Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	DeviceId NVARCHAR(64) NOT NULL,
	[Timestamp] datetimeoffset(7) NOT NULL,
	Value decimal(18,3) NOT NULL, 
    [Count] INT NULL
)
