
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 04/29/2018 18:22:08
-- Generated from EDMX file: C:\Users\nnuda\source\HomeWorks_sem4\Article3000\Article3000\DatabaseModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [DataBase];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Articles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Articles];
GO
IF OBJECT_ID(N'[dbo].[Subscribers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Subscribers];
GO
IF OBJECT_ID(N'[dbo].[Tags]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Tags];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Articles'
CREATE TABLE [dbo].[Articles] (
    [Id] nvarchar(50)  NOT NULL,
    [Author] nvarchar(max)  NOT NULL,
    [Date] nvarchar(max)  NOT NULL,
    [Title] nvarchar(max)  NOT NULL,
    [Data] nvarchar(max)  NOT NULL,
    [Tags] nvarchar(max)  NOT NULL,
    [Num] bigint IDENTITY(1,1) NOT NULL
);
GO

-- Creating table 'Subscribers'
CREATE TABLE [dbo].[Subscribers] (
    [Nickname] nvarchar(50)  NOT NULL,
    [Tags] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Tags'
CREATE TABLE [dbo].[Tags] (
    [Tag] nvarchar(50)  NOT NULL,
    [Number] int  NOT NULL,
    [ArticlesId] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'SubscribersTags'
CREATE TABLE [dbo].[SubscribersTags] (
    [Subscribers_Nickname] nvarchar(50)  NOT NULL,
    [Subscribtions_Tag] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'ArticlesTags'
CREATE TABLE [dbo].[ArticlesTags] (
    [Articles_Id] nvarchar(50)  NOT NULL,
    [SubscribtionTags_Tag] nvarchar(50)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Articles'
ALTER TABLE [dbo].[Articles]
ADD CONSTRAINT [PK_Articles]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Nickname] in table 'Subscribers'
ALTER TABLE [dbo].[Subscribers]
ADD CONSTRAINT [PK_Subscribers]
    PRIMARY KEY CLUSTERED ([Nickname] ASC);
GO

-- Creating primary key on [Tag] in table 'Tags'
ALTER TABLE [dbo].[Tags]
ADD CONSTRAINT [PK_Tags]
    PRIMARY KEY CLUSTERED ([Tag] ASC);
GO

-- Creating primary key on [Subscribers_Nickname], [Subscribtions_Tag] in table 'SubscribersTags'
ALTER TABLE [dbo].[SubscribersTags]
ADD CONSTRAINT [PK_SubscribersTags]
    PRIMARY KEY CLUSTERED ([Subscribers_Nickname], [Subscribtions_Tag] ASC);
GO

-- Creating primary key on [Articles_Id], [SubscribtionTags_Tag] in table 'ArticlesTags'
ALTER TABLE [dbo].[ArticlesTags]
ADD CONSTRAINT [PK_ArticlesTags]
    PRIMARY KEY CLUSTERED ([Articles_Id], [SubscribtionTags_Tag] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Subscribers_Nickname] in table 'SubscribersTags'
ALTER TABLE [dbo].[SubscribersTags]
ADD CONSTRAINT [FK_SubscribersTags_Subscribers]
    FOREIGN KEY ([Subscribers_Nickname])
    REFERENCES [dbo].[Subscribers]
        ([Nickname])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Subscribtions_Tag] in table 'SubscribersTags'
ALTER TABLE [dbo].[SubscribersTags]
ADD CONSTRAINT [FK_SubscribersTags_Tags]
    FOREIGN KEY ([Subscribtions_Tag])
    REFERENCES [dbo].[Tags]
        ([Tag])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_SubscribersTags_Tags'
CREATE INDEX [IX_FK_SubscribersTags_Tags]
ON [dbo].[SubscribersTags]
    ([Subscribtions_Tag]);
GO

-- Creating foreign key on [Articles_Id] in table 'ArticlesTags'
ALTER TABLE [dbo].[ArticlesTags]
ADD CONSTRAINT [FK_ArticlesTags_Articles]
    FOREIGN KEY ([Articles_Id])
    REFERENCES [dbo].[Articles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [SubscribtionTags_Tag] in table 'ArticlesTags'
ALTER TABLE [dbo].[ArticlesTags]
ADD CONSTRAINT [FK_ArticlesTags_Tags]
    FOREIGN KEY ([SubscribtionTags_Tag])
    REFERENCES [dbo].[Tags]
        ([Tag])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ArticlesTags_Tags'
CREATE INDEX [IX_FK_ArticlesTags_Tags]
ON [dbo].[ArticlesTags]
    ([SubscribtionTags_Tag]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------