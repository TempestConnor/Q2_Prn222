-- Run in a dedicated database for ONE version. Never run against unrelated data.
-- SQL Server syntax. This file creates missing tables and resets only these two tables.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.LookupGroup', N'U') IS NULL
CREATE TABLE dbo.LookupGroup(GroupId int IDENTITY(1,1) PRIMARY KEY, Name nvarchar(50) NOT NULL);
IF OBJECT_ID(N'dbo.Item', N'U') IS NULL
CREATE TABLE dbo.Item(
 ItemId int IDENTITY(1,1) PRIMARY KEY,
 Name nvarchar(50) NOT NULL,
 GroupId int NOT NULL REFERENCES dbo.LookupGroup(GroupId),
 Amount decimal(10,2) NOT NULL CHECK(Amount BETWEEN 0 AND 1000),
 EventDate date NOT NULL,
 Status nvarchar(20) NOT NULL CHECK(Status IN (N'Pending',N'Approved',N'Rejected'))
);
DELETE FROM dbo.Item;
DELETE FROM dbo.LookupGroup;
DBCC CHECKIDENT ('dbo.Item', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.LookupGroup', RESEED, 0) WITH NO_INFOMSGS;
SET IDENTITY_INSERT dbo.LookupGroup ON;
INSERT dbo.LookupGroup(GroupId,Name) VALUES (1,N'North'),(2,N'South'),(3,N'West');
SET IDENTITY_INSERT dbo.LookupGroup OFF;
SET IDENTITY_INSERT dbo.Item ON;
INSERT dbo.Item(ItemId,Name,GroupId,Amount,EventDate,Status) VALUES
 (1,N'Alpha',1,10.00,'20260301',N'Pending'),
 (2,N'Beta',2,20.00,'20260302',N'Approved'),
 (3,N'Alpine',1,30.00,'20260303',N'Pending'),
 (4,N'Delta',2,20.00,'20260304',N'Rejected'),
 (5,N'Echo',2,0.00,'20260305',N'Pending');
SET IDENTITY_INSERT dbo.Item OFF;
-- Explicitly fix the next generated identities independently of prior database history.
DBCC CHECKIDENT ('dbo.Item', RESEED, 5) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.LookupGroup', RESEED, 3) WITH NO_INFOMSGS;
COMMIT TRANSACTION;
