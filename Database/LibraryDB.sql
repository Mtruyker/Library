IF DB_ID(N'LibraryDB') IS NULL
BEGIN
    CREATE DATABASE LibraryDB;
END
GO

USE LibraryDB;
GO

IF OBJECT_ID(N'dbo.Loans', N'U') IS NOT NULL DROP TABLE dbo.Loans;
IF OBJECT_ID(N'dbo.BookCopies', N'U') IS NOT NULL DROP TABLE dbo.BookCopies;
IF OBJECT_ID(N'dbo.Books', N'U') IS NOT NULL DROP TABLE dbo.Books;
IF OBJECT_ID(N'dbo.Readers', N'U') IS NOT NULL DROP TABLE dbo.Readers;
IF OBJECT_ID(N'dbo.Authors', N'U') IS NOT NULL DROP TABLE dbo.Authors;
GO

CREATE TABLE dbo.Readers
(
    ReaderId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Readers PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Address NVARCHAR(200) NULL,
    TicketNumber NVARCHAR(30) NOT NULL CONSTRAINT UQ_Readers_TicketNumber UNIQUE
);
GO

CREATE TABLE dbo.Authors
(
    AuthorId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Authors PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL CONSTRAINT UQ_Authors_FullName UNIQUE
);
GO

CREATE TABLE dbo.Books
(
    BookId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Books PRIMARY KEY,
    Title NVARCHAR(150) NOT NULL,
    AuthorId INT NOT NULL,
    Publisher NVARCHAR(100) NULL,
    PublishYear INT NULL,
    Genre NVARCHAR(50) NULL,
    ISBN NVARCHAR(30) NULL,
    CONSTRAINT FK_Books_Authors FOREIGN KEY (AuthorId) REFERENCES dbo.Authors (AuthorId),
    CONSTRAINT CK_Books_PublishYear CHECK (PublishYear IS NULL OR PublishYear BETWEEN 0 AND 3000)
);
GO

CREATE TABLE dbo.BookCopies
(
    CopyId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BookCopies PRIMARY KEY,
    BookId INT NOT NULL,
    InventoryNumber NVARCHAR(30) NOT NULL CONSTRAINT UQ_BookCopies_InventoryNumber UNIQUE,
    Location NVARCHAR(100) NOT NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_BookCopies_Status DEFAULT N'доступна',
    CONSTRAINT FK_BookCopies_Books FOREIGN KEY (BookId) REFERENCES dbo.Books (BookId),
    CONSTRAINT CK_BookCopies_Status CHECK (Status IN (N'доступна', N'выдана', N'потеряна'))
);
GO

CREATE TABLE dbo.Loans
(
    LoanId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Loans PRIMARY KEY,
    ReaderId INT NOT NULL,
    CopyId INT NOT NULL,
    IssueDate DATE NOT NULL,
    DueDate DATE NOT NULL,
    ReturnDate DATE NULL,
    CONSTRAINT FK_Loans_Readers FOREIGN KEY (ReaderId) REFERENCES dbo.Readers (ReaderId),
    CONSTRAINT FK_Loans_BookCopies FOREIGN KEY (CopyId) REFERENCES dbo.BookCopies (CopyId),
    CONSTRAINT CK_Loans_Dates CHECK (DueDate >= IssueDate)
);
GO

CREATE UNIQUE INDEX UX_Loans_ActiveCopy
    ON dbo.Loans (CopyId)
    WHERE ReturnDate IS NULL;
GO

CREATE INDEX IX_Books_Title ON dbo.Books (Title);
CREATE INDEX IX_Authors_FullName ON dbo.Authors (FullName);
CREATE INDEX IX_BookCopies_Status ON dbo.BookCopies (Status);
CREATE INDEX IX_Loans_DueDate ON dbo.Loans (DueDate);
GO
