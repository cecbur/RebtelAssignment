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

-- Create Book table
CREATE TABLE [dbo].[Book] (
    [BookId] INT IDENTITY(1,1) NOT NULL,
    [Title] NVARCHAR(200) NOT NULL,
    [Author] NVARCHAR(100) NULL,
    [ISBN] NVARCHAR(20) NULL,
    [PublicationYear] INT NULL,
    [IsAvailable] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Book] PRIMARY KEY CLUSTERED ([BookId] ASC),
    CONSTRAINT [UQ_Book_ISBN] UNIQUE NONCLUSTERED ([ISBN] ASC)
);
GO

-- Create index on Title for better search performance
CREATE NONCLUSTERED INDEX [IX_Book_Title]
ON [dbo].[Book] ([Title] ASC);
GO

-- Insert sample data
INSERT INTO [dbo].[Book] ([Title], [Author], [ISBN], [PublicationYear], [IsAvailable])
VALUES
    ('Moby Dick', 'Herman Melville', '978-0142437247', 1851, 1),
    ('1984', 'George Orwell', '978-0451524935', 1949, 1),
    ('Pride and Prejudice', 'Jane Austen', '978-0141439518', 1813, 1),
    ('The Great Gatsby', 'F. Scott Fitzgerald', '978-0743273565', 1925, 1),
    ('To Kill a Mockingbird', 'Harper Lee', '978-0061120084', 1960, 0),
    ('The Catcher in the Rye', 'J.D. Salinger', '978-0316769174', 1951, 1),
    ('The Hobbit', 'J.R.R. Tolkien', '978-0547928227', 1937, 1),
    ('Harry Potter and the Philosopher''s Stone', 'J.K. Rowling', '978-0439708180', 1997, 1),
    ('Animal Farm', 'George Orwell', '978-0451526342', 1945, 1),
    ('The Lord of the Rings', 'J.R.R. Tolkien', '978-0544003415', 1954, 0),
    ('Brave New World', 'Aldous Huxley', '978-0060850524', 1932, 1),
    ('The Chronicles of Narnia', 'C.S. Lewis', '978-0066238500', 1950, 1),
    ('Jane Eyre', 'Charlotte Bronte', '978-0141441146', 1847, 1),
    ('Wuthering Heights', 'Emily Bronte', '978-0141439556', 1847, 1),
    ('The Odyssey', 'Homer', '978-0140268867', -800, 1),
    ('Hamlet', 'William Shakespeare', '978-0743477123', 1603, 1);
GO

-- Display created data
SELECT * FROM [dbo].[Book];
GO

PRINT 'Database [Library] created successfully with Book table and sample data!';
GO
