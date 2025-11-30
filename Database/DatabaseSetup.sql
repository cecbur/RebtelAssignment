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
    [Id] INT IDENTITY(1,1) NOT NULL,
    [GivenName] NVARCHAR(100) NULL,
    [Surname] NVARCHAR(100) NOT NULL,
    CONSTRAINT [PK_Author] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Create index on Surname for better search performance
CREATE NONCLUSTERED INDEX [IX_Author_Surname]
ON [dbo].[Author] ([Surname] ASC);
GO

-- Create Book table
CREATE TABLE [dbo].[Book] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Title] NVARCHAR(200) NOT NULL,
    [AuthorId] INT NULL,
    [ISBN] NVARCHAR(20) NULL,
    [PublicationYear] INT NULL,
    [NumberOfPages] INT NULL,
    [IsAvailable] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Book] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_Book_ISBN] UNIQUE NONCLUSTERED ([ISBN] ASC),
    CONSTRAINT [FK_Book_Author] FOREIGN KEY ([AuthorId]) REFERENCES [dbo].[Author]([Id])
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
    [Id] INT IDENTITY(1,1) NOT NULL,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(200) NOT NULL,
    [PhoneNumber] NVARCHAR(20) NULL,
    [MembershipDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [IsActive] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Patron] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_Patron_Email] UNIQUE NONCLUSTERED ([Email] ASC)
);
GO

-- Create index on LastName for better search performance
CREATE NONCLUSTERED INDEX [IX_Patron_LastName]
ON [dbo].[Patron] ([LastName] ASC);
GO

-- Create Loan table
CREATE TABLE [dbo].[Loan] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [BookId] INT NOT NULL,
    [PatronId] INT NOT NULL,
    [LoanDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [DueDate] DATETIME2 NOT NULL,
    [ReturnDate] DATETIME2 NULL,
    [IsReturned] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Loan] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Loan_Book] FOREIGN KEY ([BookId]) REFERENCES [dbo].[Book]([Id]),
    CONSTRAINT [FK_Loan_Patron] FOREIGN KEY ([PatronId]) REFERENCES [dbo].[Patron]([Id])
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
-- Creating borrowing patterns: Multiple patrons borrowing similar books
-- Pattern 1: Classic literature fans (Books 1, 2, 3 - Moby Dick, 1984, Pride & Prejudice)
-- Pattern 2: Harry Potter series fans (Books 4, 5 - Harry Potter books)
-- Pattern 3: Science fiction fans (Books 6, 7, 8)
INSERT INTO [dbo].[Loan] ([BookId], [PatronId], [LoanDate], [DueDate], [ReturnDate], [IsReturned])
VALUES
    -- Original loans
    (1, 1, '2024-01-05', '2024-01-19', '2024-01-18', 1),
    (2, 2, '2024-01-10', '2024-01-24', '2024-01-22', 1),
    (3, 3, '2024-01-15', '2024-01-29', '2024-01-28', 1),
    (4, 1, '2024-02-01', '2024-02-15', '2024-02-14', 1),
    (5, 4, '2024-02-05', '2024-02-19', '2024-02-18', 1),
    (6, 5, '2024-02-10', '2024-02-24', '2024-02-20', 1),
    (7, 6, '2024-02-15', '2024-02-29', '2024-02-28', 1),
    (8, 2, '2024-03-01', '2024-03-15', '2024-03-10', 1),
    (9, 7, '2024-03-05', '2024-03-19', '2024-03-18', 1),
    (10, 3, '2024-03-10', '2024-03-24', '2024-03-22', 1),

    -- Classic literature pattern: Patrons 1, 2, 3 all borrow books 1, 2, 3
    (2, 1, '2024-03-20', '2024-04-03', '2024-04-01', 1),  -- Patron 1 borrows book 2
    (3, 1, '2024-04-05', '2024-04-19', '2024-04-18', 1),  -- Patron 1 borrows book 3
    (1, 2, '2024-03-25', '2024-04-08', '2024-04-07', 1),  -- Patron 2 borrows book 1
    (3, 2, '2024-04-10', '2024-04-24', '2024-04-23', 1),  -- Patron 2 borrows book 3
    (1, 3, '2024-04-01', '2024-04-15', '2024-04-14', 1),  -- Patron 3 borrows book 1
    (2, 3, '2024-04-15', '2024-04-29', '2024-04-28', 1),  -- Patron 3 borrows book 3

    -- Harry Potter fans: Patrons 1, 4, 8 all borrow books 4 and 5
    (5, 1, '2024-04-20', '2024-05-04', '2024-05-03', 1),  -- Patron 1 borrows book 5
    (4, 4, '2024-04-22', '2024-05-06', '2024-05-05', 1),  -- Patron 4 borrows book 4
    (5, 4, '2024-04-22', '2024-05-06', '2024-05-05', 1),  -- Patron 4 borrows book 5
    (4, 8, '2024-04-25', '2024-05-09', '2024-05-08', 1),  -- Patron 8 borrows book 4
    (5, 8, '2024-05-10', '2024-05-24', '2024-05-23', 1),  -- Patron 8 borrows book 5

    -- Science fiction pattern: Patrons 2, 5, 6 all borrow books 6, 7, 8
    (7, 2, '2024-05-01', '2024-05-15', '2024-05-14', 1),  -- Patron 2 borrows book 7
    (8, 5, '2024-05-05', '2024-05-19', '2024-05-18', 1),  -- Patron 5 borrows book 8
    (7, 5, '2024-05-20', '2024-06-03', '2024-06-02', 1),  -- Patron 5 borrows book 7
    (6, 6, '2024-05-10', '2024-05-24', '2024-05-23', 1),  -- Patron 6 borrows book 6
    (8, 6, '2024-05-25', '2024-06-08', '2024-06-07', 1);  -- Patron 6 borrows book 8
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
