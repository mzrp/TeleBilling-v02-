
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 04/08/2019 11:25:19
-- Generated from EDMX file: C:\Users\rj\source\TeleBilling(v02)\TeleBilling(v02)\Models\DBModels.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [TeleBillingDB];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_CSVFileSupplier]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CSVFileSet] DROP CONSTRAINT [FK_CSVFileSupplier];
GO
IF OBJECT_ID(N'[dbo].[FK_CSVFileUser]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CSVFileSet] DROP CONSTRAINT [FK_CSVFileUser];
GO
IF OBJECT_ID(N'[dbo].[FK_AgreementUser]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AgreementSet] DROP CONSTRAINT [FK_AgreementUser];
GO
IF OBJECT_ID(N'[dbo].[FK_CSVFileType]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CSVFileSet] DROP CONSTRAINT [FK_CSVFileType];
GO
IF OBJECT_ID(N'[dbo].[FK_AgreementCSVFile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AgreementSet] DROP CONSTRAINT [FK_AgreementCSVFile];
GO
IF OBJECT_ID(N'[dbo].[FK_CSVFileInvoiceRecords]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[InvoiceRecordsSet] DROP CONSTRAINT [FK_CSVFileInvoiceRecords];
GO
IF OBJECT_ID(N'[dbo].[FK_CSVFileZoneRecords]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ZoneRecordsSet] DROP CONSTRAINT [FK_CSVFileZoneRecords];
GO
IF OBJECT_ID(N'[dbo].[FK_AgreementZoneRecords]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ZoneRecordsSet] DROP CONSTRAINT [FK_AgreementZoneRecords];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[AgreementSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AgreementSet];
GO
IF OBJECT_ID(N'[dbo].[UserSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserSet];
GO
IF OBJECT_ID(N'[dbo].[SupplierSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SupplierSet];
GO
IF OBJECT_ID(N'[dbo].[CSVFileSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CSVFileSet];
GO
IF OBJECT_ID(N'[dbo].[ZoneRecordsSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ZoneRecordsSet];
GO
IF OBJECT_ID(N'[dbo].[InvoiceRecordsSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[InvoiceRecordsSet];
GO
IF OBJECT_ID(N'[dbo].[TypeSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TypeSet];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'AgreementSet'
CREATE TABLE [dbo].[AgreementSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Customer_cvr] nvarchar(max)  NOT NULL,
    [Customer_name] nvarchar(max)  NOT NULL,
    [Subscriber_range_start] nvarchar(max)  NOT NULL,
    [Subscriber_range_end] nvarchar(max)  NOT NULL,
    [Description] nvarchar(max)  NOT NULL,
    [Status] bit  NOT NULL,
    [Date] datetime  NOT NULL,
    [UserId] int  NOT NULL,
    [CSVFileId] int  NOT NULL
);
GO

-- Creating table 'UserSet'
CREATE TABLE [dbo].[UserSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Role] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'SupplierSet'
CREATE TABLE [dbo].[SupplierSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'CSVFileSet'
CREATE TABLE [dbo].[CSVFileSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Date] datetime  NOT NULL,
    [SupplierId] int  NOT NULL,
    [UserId] int  NOT NULL,
    [TypeId] int  NOT NULL
);
GO

-- Creating table 'ZoneRecordsSet'
CREATE TABLE [dbo].[ZoneRecordsSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Call_price] decimal(10,4)  NOT NULL,
    [Minute_price] decimal(10,4)  NOT NULL,
    [Country_code] nvarchar(max)  NULL,
    [CSVFileId] int  NULL,
    [AgreementId] int  NULL
);
GO

-- Creating table 'InvoiceRecordsSet'
CREATE TABLE [dbo].[InvoiceRecordsSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Id_call] nvarchar(max)  NOT NULL,
    [Time] datetime  NOT NULL,
    [Subscriber] nvarchar(max)  NOT NULL,
    [Aprefix] nvarchar(max)  NOT NULL,
    [Destination] nvarchar(max)  NOT NULL,
    [Invoice_group] nvarchar(max)  NOT NULL,
    [Prefix] nvarchar(max)  NOT NULL,
    [Pbx] nvarchar(max)  NOT NULL,
    [Direction] nvarchar(max)  NOT NULL,
    [Volume_time_secs] nvarchar(max)  NOT NULL,
    [Price] nvarchar(max)  NOT NULL,
    [Free] nvarchar(max)  NOT NULL,
    [Forward] nvarchar(max)  NOT NULL,
    [Servingnetwork] nvarchar(max)  NOT NULL,
    [Reason] nvarchar(max)  NOT NULL,
    [Billed] nvarchar(max)  NOT NULL,
    [ZoneName] nvarchar(max)  NOT NULL,
    [RPBilled] nvarchar(max)  NOT NULL,
    [CSVFileId] int  NOT NULL
);
GO

-- Creating table 'TypeSet'
CREATE TABLE [dbo].[TypeSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'AgreementSet'
ALTER TABLE [dbo].[AgreementSet]
ADD CONSTRAINT [PK_AgreementSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'UserSet'
ALTER TABLE [dbo].[UserSet]
ADD CONSTRAINT [PK_UserSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'SupplierSet'
ALTER TABLE [dbo].[SupplierSet]
ADD CONSTRAINT [PK_SupplierSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'CSVFileSet'
ALTER TABLE [dbo].[CSVFileSet]
ADD CONSTRAINT [PK_CSVFileSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ZoneRecordsSet'
ALTER TABLE [dbo].[ZoneRecordsSet]
ADD CONSTRAINT [PK_ZoneRecordsSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'InvoiceRecordsSet'
ALTER TABLE [dbo].[InvoiceRecordsSet]
ADD CONSTRAINT [PK_InvoiceRecordsSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'TypeSet'
ALTER TABLE [dbo].[TypeSet]
ADD CONSTRAINT [PK_TypeSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [SupplierId] in table 'CSVFileSet'
ALTER TABLE [dbo].[CSVFileSet]
ADD CONSTRAINT [FK_CSVFileSupplier]
    FOREIGN KEY ([SupplierId])
    REFERENCES [dbo].[SupplierSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CSVFileSupplier'
CREATE INDEX [IX_FK_CSVFileSupplier]
ON [dbo].[CSVFileSet]
    ([SupplierId]);
GO

-- Creating foreign key on [UserId] in table 'CSVFileSet'
ALTER TABLE [dbo].[CSVFileSet]
ADD CONSTRAINT [FK_CSVFileUser]
    FOREIGN KEY ([UserId])
    REFERENCES [dbo].[UserSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CSVFileUser'
CREATE INDEX [IX_FK_CSVFileUser]
ON [dbo].[CSVFileSet]
    ([UserId]);
GO

-- Creating foreign key on [UserId] in table 'AgreementSet'
ALTER TABLE [dbo].[AgreementSet]
ADD CONSTRAINT [FK_AgreementUser]
    FOREIGN KEY ([UserId])
    REFERENCES [dbo].[UserSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AgreementUser'
CREATE INDEX [IX_FK_AgreementUser]
ON [dbo].[AgreementSet]
    ([UserId]);
GO

-- Creating foreign key on [TypeId] in table 'CSVFileSet'
ALTER TABLE [dbo].[CSVFileSet]
ADD CONSTRAINT [FK_CSVFileType]
    FOREIGN KEY ([TypeId])
    REFERENCES [dbo].[TypeSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CSVFileType'
CREATE INDEX [IX_FK_CSVFileType]
ON [dbo].[CSVFileSet]
    ([TypeId]);
GO

-- Creating foreign key on [CSVFileId] in table 'AgreementSet'
ALTER TABLE [dbo].[AgreementSet]
ADD CONSTRAINT [FK_AgreementCSVFile]
    FOREIGN KEY ([CSVFileId])
    REFERENCES [dbo].[CSVFileSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AgreementCSVFile'
CREATE INDEX [IX_FK_AgreementCSVFile]
ON [dbo].[AgreementSet]
    ([CSVFileId]);
GO

-- Creating foreign key on [CSVFileId] in table 'InvoiceRecordsSet'
ALTER TABLE [dbo].[InvoiceRecordsSet]
ADD CONSTRAINT [FK_CSVFileInvoiceRecords]
    FOREIGN KEY ([CSVFileId])
    REFERENCES [dbo].[CSVFileSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CSVFileInvoiceRecords'
CREATE INDEX [IX_FK_CSVFileInvoiceRecords]
ON [dbo].[InvoiceRecordsSet]
    ([CSVFileId]);
GO

-- Creating foreign key on [CSVFileId] in table 'ZoneRecordsSet'
ALTER TABLE [dbo].[ZoneRecordsSet]
ADD CONSTRAINT [FK_CSVFileZoneRecords]
    FOREIGN KEY ([CSVFileId])
    REFERENCES [dbo].[CSVFileSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CSVFileZoneRecords'
CREATE INDEX [IX_FK_CSVFileZoneRecords]
ON [dbo].[ZoneRecordsSet]
    ([CSVFileId]);
GO

-- Creating foreign key on [AgreementId] in table 'ZoneRecordsSet'
ALTER TABLE [dbo].[ZoneRecordsSet]
ADD CONSTRAINT [FK_AgreementZoneRecords]
    FOREIGN KEY ([AgreementId])
    REFERENCES [dbo].[AgreementSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AgreementZoneRecords'
CREATE INDEX [IX_FK_AgreementZoneRecords]
ON [dbo].[ZoneRecordsSet]
    ([AgreementId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------