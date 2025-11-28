-- =============================================
-- Library Database Setup Script for SQL Server LocalDB
-- =============================================

USE master;
GO

-- Drop database if it exists
IF DB_ID('Library') IS NOT NULL
BEGIN
    ALTER DATABASE [Library] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [Library];
END
GO

-- Create Library database
CREATE DATABASE [Library];
GO

USE [Library];
GO

-- Create Author table
CREATE TABLE [dbo].[Author] (
    [AuthorId] INT IDENTITY(1,1) NOT NULL,
    [GivenName] NVARCHAR(100) NULL,
    [Surname] NVARCHAR(100) NOT NULL,
    CONSTRAINT [PK_Author] PRIMARY KEY CLUSTERED ([AuthorId] ASC)
);
GO

-- Create index on Surname for better search performance
CREATE NONCLUSTERED INDEX [IX_Author_Surname]
ON [dbo].[Author] ([Surname] ASC);
GO

-- Create Book table
CREATE TABLE [dbo].[Book] (
    [BookId] INT IDENTITY(1,1) NOT NULL,
    [Title] NVARCHAR(200) NOT NULL,
    [AuthorId] INT NULL,
    [ISBN] NVARCHAR(20) NULL,
    [PublicationYear] INT NULL,
    [NumberOfPages] INT NULL,
    [IsAvailable] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Book] PRIMARY KEY CLUSTERED ([BookId] ASC),
    CONSTRAINT [UQ_Book_ISBN] UNIQUE NONCLUSTERED ([ISBN] ASC),
    CONSTRAINT [FK_Book_Author] FOREIGN KEY ([AuthorId]) REFERENCES [dbo].[Author]([AuthorId])
);
GO

-- Create index on Title for better search performance
CREATE NONCLUSTERED INDEX [IX_Book_Title]
ON [dbo].[Book] ([Title] ASC);
GO

-- Create index on AuthorId for better join performance
CREATE NONCLUSTERED INDEX [IX_Book_AuthorId]
ON [dbo].[Book] ([AuthorId] ASC);
GO

-- Create Patron table
CREATE TABLE [dbo].[Patron] (
    [PatronId] INT IDENTITY(1,1) NOT NULL,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(200) NOT NULL,
    [PhoneNumber] NVARCHAR(20) NULL,
    [MembershipDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [IsActive] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Patron] PRIMARY KEY CLUSTERED ([PatronId] ASC),
    CONSTRAINT [UQ_Patron_Email] UNIQUE NONCLUSTERED ([Email] ASC)
);
GO

-- Create index on LastName for better search performance
CREATE NONCLUSTERED INDEX [IX_Patron_LastName]
ON [dbo].[Patron] ([LastName] ASC);
GO

-- Create Loan table
CREATE TABLE [dbo].[Loan] (
    [LoanId] INT IDENTITY(1,1) NOT NULL,
    [BookId] INT NOT NULL,
    [PatronId] INT NOT NULL,
    [LoanDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [DueDate] DATETIME2 NOT NULL,
    [ReturnDate] DATETIME2 NULL,
    [IsReturned] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Loan] PRIMARY KEY CLUSTERED ([LoanId] ASC),
    CONSTRAINT [FK_Loan_Book] FOREIGN KEY ([BookId]) REFERENCES [dbo].[Book]([BookId]),
    CONSTRAINT [FK_Loan_Patron] FOREIGN KEY ([PatronId]) REFERENCES [dbo].[Patron]([PatronId])
);
GO

-- Create indexes for better query performance
CREATE NONCLUSTERED INDEX [IX_Loan_BookId]
ON [dbo].[Loan] ([BookId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Loan_PatronId]
ON [dbo].[Loan] ([PatronId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Loan_IsReturned]
ON [dbo].[Loan] ([IsReturned] ASC);
GO

-- Insert sample author data
INSERT INTO [dbo].[Author] ([GivenName], [Surname])
VALUES
    ('Herman', 'Melville'),
    ('George', 'Orwell'),
    ('Jane', 'Austen'),
    ('F. Scott', 'Fitzgerald'),
    ('Harper', 'Lee'),
    ('J.D.', 'Salinger'),
    ('J.R.R.', 'Tolkien'),
    ('J.K.', 'Rowling'),
    ('Aldous', 'Huxley'),
    ('C.S.', 'Lewis'),
    ('Charlotte', 'Bronte'),
    ('Emily', 'Bronte'),
    (NULL, 'Homer'),
    ('William', 'Shakespeare');
GO

-- Insert sample book data
INSERT INTO [dbo].[Book] ([Title], [AuthorId], [ISBN], [PublicationYear], [NumberOfPages], [IsAvailable])
VALUES
    ('Moby Dick', 1, '978-0142437247', 1851, 635, 1),
    ('1984', 2, '978-0451524935', 1949, 328, 1),
    ('Pride and Prejudice', 3, '978-0141439518', 1813, 432, 1),
    ('The Great Gatsby', 4, '978-0743273565', 1925, 180, 1),
    ('To Kill a Mockingbird', 5, '978-0061120084', 1960, 324, 1),
    ('The Catcher in the Rye', 6, '978-0316769174', 1951, 277, 1),
    ('The Hobbit', 7, '978-0547928227', 1937, 310, 1),
    ('Harry Potter and the Philosopher''s Stone', 8, '978-0439708180', 1997, 223, 1),
    ('Animal Farm', 2, '978-0451526342', 1945, 112, 1),
    ('The Lord of the Rings', 7, '978-0544003415', 1954, 1178, 1),
    ('Brave New World', 9, '978-0060850524', 1932, 268, 1),
    ('The Chronicles of Narnia', 10, '978-0066238500', 1950, 767, 1),
    ('Jane Eyre', 11, '978-0141441146', 1847, 532, 1),
    ('Wuthering Heights', 12, '978-0141439556', 1847, 416, 1),
    ('The Odyssey', 13, '978-0140268867', -800, 541, 1),
    ('Hamlet', 14, '978-0743477123', 1603, 289, 1);
GO

-- Insert sample patron data
INSERT INTO [dbo].[Patron] ([FirstName], [LastName], [Email], [PhoneNumber], [MembershipDate], [IsActive])
VALUES
    ('Alice', 'Johnson', 'alice.johnson@email.com', '555-0101', '2023-01-15', 1),
    ('Bob', 'Smith', 'bob.smith@email.com', '555-0102', '2023-02-20', 1),
    ('Carol', 'Williams', 'carol.williams@email.com', '555-0103', '2023-03-10', 1),
    ('David', 'Brown', 'david.brown@email.com', '555-0104', '2023-04-05', 1),
    ('Emma', 'Davis', 'emma.davis@email.com', NULL, '2023-05-12', 1),
    ('Frank', 'Miller', 'frank.miller@email.com', '555-0106', '2023-06-18', 1),
    ('Grace', 'Wilson', 'grace.wilson@email.com', '555-0107', '2023-07-22', 1),
    ('Henry', 'Moore', 'henry.moore@email.com', '555-0108', '2023-08-30', 0),
    ('Iris', 'Taylor', 'iris.taylor@email.com', NULL, '2023-09-14', 1),
    ('Jack', 'Anderson', 'jack.anderson@email.com', '555-0110', '2023-10-25', 1);
GO

-- Insert sample loan data
INSERT INTO [dbo].[Loan] ([BookId], [PatronId], [LoanDate], [DueDate], [ReturnDate], [IsReturned])
VALUES
    (1, 1, '2024-01-05', '2024-01-19', '2024-01-18', 1),
    (2, 2, '2024-01-10', '2024-01-24', '2024-01-22', 1),
    (3, 3, '2024-01-15', '2024-01-29', NULL, 0),
    (4, 1, '2024-02-01', '2024-02-15', '2024-02-14', 1),
    (5, 4, '2024-02-05', '2024-02-19', NULL, 0),
    (6, 5, '2024-02-10', '2024-02-24', '2024-02-20', 1),
    (7, 6, '2024-02-15', '2024-02-29', NULL, 0),
    (8, 2, '2024-03-01', '2024-03-15', '2024-03-10', 1),
    (9, 7, '2024-03-05', '2024-03-19', NULL, 0),
    (10, 3, '2024-03-10', '2024-03-24', NULL, 0);
GO

-- Display created data
PRINT 'Authors:';
SELECT * FROM [dbo].[Author];
GO

PRINT 'Books:';
SELECT * FROM [dbo].[Book];
GO

PRINT 'Patrons:';
SELECT * FROM [dbo].[Patron];
GO

PRINT 'Loans:';
SELECT * FROM [dbo].[Loan];
GO

PRINT 'Database [Library] created successfully with Author, Book, Patron, and Loan tables with sample data!';
GO
