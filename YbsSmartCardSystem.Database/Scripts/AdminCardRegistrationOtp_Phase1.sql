USE [YbsSmartCard]
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_CardRegistrationOtp]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Tbl_CardRegistrationOtp](
    [OtpId] [int] IDENTITY(1,1) NOT NULL,
    [PhoneNumber] [nvarchar](20) NOT NULL,
    [OtpCodeHash] [nvarchar](500) NOT NULL,
    [Purpose] [nvarchar](50) NOT NULL,
    [ExpiresAt] [datetime] NOT NULL,
    [VerifiedAt] [datetime] NULL,
    [AttemptCount] [int] NOT NULL DEFAULT 0,
    [MaxAttemptCount] [int] NOT NULL DEFAULT 5,
    [CreatedByUserId] [int] NOT NULL,
    [CreatedDate] [datetime] NOT NULL DEFAULT GETDATE(),
    [DeleteFlag] [bit] NOT NULL DEFAULT 0,
PRIMARY KEY CLUSTERED 
(
    [OtpId] ASC
)
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_CardRegistrationOtp]') AND name = N'IX_Tbl_CardRegistrationOtp_PhoneNumber')
BEGIN
CREATE NONCLUSTERED INDEX [IX_Tbl_CardRegistrationOtp_PhoneNumber] ON [dbo].[Tbl_CardRegistrationOtp]
(
    [PhoneNumber] ASC
)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_CardRegistrationOtp]') AND name = N'IX_Tbl_CardRegistrationOtp_ExpiresAt')
BEGIN
CREATE NONCLUSTERED INDEX [IX_Tbl_CardRegistrationOtp_ExpiresAt] ON [dbo].[Tbl_CardRegistrationOtp]
(
    [ExpiresAt] ASC
)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_CardRegistrationOtp]') AND name = N'IX_Tbl_CardRegistrationOtp_CreatedByUserId')
BEGIN
CREATE NONCLUSTERED INDEX [IX_Tbl_CardRegistrationOtp_CreatedByUserId] ON [dbo].[Tbl_CardRegistrationOtp]
(
    [CreatedByUserId] ASC
)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_Card]') AND name = N'IX_Tbl_Card_CardNum')
BEGIN
CREATE UNIQUE NONCLUSTERED INDEX [IX_Tbl_Card_CardNum] ON [dbo].[Tbl_Card]
(
    [CardNum] ASC
)
END
GO
