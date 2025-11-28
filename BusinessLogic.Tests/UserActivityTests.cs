using BusinessLogic.InventoryInsights;
using BusinessModels;
using DataStorageContracts;
using Moq;
using Xunit;

namespace BusinessLogic.Tests;

public class UserActivityTests
{
    private readonly Mock<ILoanRepository> _mockLoanRepository;
    private readonly UserActivity _userActivity;

    public UserActivityTests()
    {
        _mockLoanRepository = new Mock<ILoanRepository>();
        _userActivity = new UserActivity(_mockLoanRepository.Object);
    }

    #region GetPatronLoansOrderedByFrequency Tests

    [Fact]
    public async Task GetPatronLoansOrderedByFrequency_WithMultiplePatrons_ReturnsOrderedByLoanCount()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        var patron1 = new Patron { Id = 1, FirstName = "Alice", LastName = "Johnson" };
        var patron2 = new Patron { Id = 2, FirstName = "Bob", LastName = "Smith" };
        var patron3 = new Patron { Id = 3, FirstName = "Carol", LastName = "Williams" };

        var book1 = new Book { Id = 1, Title = "Book 1", NumberOfPages = 300 };
        var book2 = new Book { Id = 2, Title = "Book 2", NumberOfPages = 400 };

        var loans = new List<Loan>
        {
            new Loan { Id = 1, Book = book1, Patron = patron1, LoanDate = new DateTime(2024, 1, 5), DueDate = new DateTime(2024, 1, 19) },
            new Loan { Id = 2, Book = book2, Patron = patron1, LoanDate = new DateTime(2024, 2, 1), DueDate = new DateTime(2024, 2, 15) },
            new Loan { Id = 3, Book = book1, Patron = patron1, LoanDate = new DateTime(2024, 3, 1), DueDate = new DateTime(2024, 3, 15) },
            new Loan { Id = 4, Book = book2, Patron = patron2, LoanDate = new DateTime(2024, 1, 10), DueDate = new DateTime(2024, 1, 24) },
            new Loan { Id = 5, Book = book1, Patron = patron2, LoanDate = new DateTime(2024, 2, 5), DueDate = new DateTime(2024, 2, 19) },
            new Loan { Id = 6, Book = book2, Patron = patron3, LoanDate = new DateTime(2024, 1, 15), DueDate = new DateTime(2024, 1, 29) }
        };

        _mockLoanRepository
            .Setup(r => r.GetLoansByTime(startDate, endDate))
            .ReturnsAsync(loans);

        // Act
        var result = await _userActivity.GetPatronLoansOrderedByFrequency(startDate, endDate);

        // Assert
        Assert.Equal(3, result.Length);
        Assert.Equal(patron1.Id, result[0].Patron.Id);
        Assert.Equal(3, result[0].LoanCount);
        Assert.Equal(patron2.Id, result[1].Patron.Id);
        Assert.Equal(2, result[1].LoanCount);
        Assert.Equal(patron3.Id, result[2].Patron.Id);
        Assert.Equal(1, result[2].LoanCount);
    }

    [Fact]
    public async Task GetPatronLoansOrderedByFrequency_WithNoLoans_ReturnsEmptyArray()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        _mockLoanRepository
            .Setup(r => r.GetLoansByTime(startDate, endDate))
            .ReturnsAsync(new List<Loan>());

        // Act
        var result = await _userActivity.GetPatronLoansOrderedByFrequency(startDate, endDate);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPatronLoansOrderedByFrequency_WithSinglePatron_ReturnsSinglePatronLoans()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        var patron = new Patron { Id = 1, FirstName = "Alice", LastName = "Johnson" };
        var book = new Book { Id = 1, Title = "Book 1", NumberOfPages = 300 };

        var loans = new List<Loan>
        {
            new Loan { Id = 1, Book = book, Patron = patron, LoanDate = new DateTime(2024, 1, 5), DueDate = new DateTime(2024, 1, 19) },
            new Loan { Id = 2, Book = book, Patron = patron, LoanDate = new DateTime(2024, 2, 1), DueDate = new DateTime(2024, 2, 15) }
        };

        _mockLoanRepository
            .Setup(r => r.GetLoansByTime(startDate, endDate))
            .ReturnsAsync(loans);

        // Act
        var result = await _userActivity.GetPatronLoansOrderedByFrequency(startDate, endDate);

        // Assert
        Assert.Single(result);
        Assert.Equal(patron.Id, result[0].Patron.Id);
        Assert.Equal(2, result[0].LoanCount);
    }

    [Fact]
    public async Task GetPatronLoansOrderedByFrequency_WithEqualLoanCounts_MaintainsStableOrder()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        var patron1 = new Patron { Id = 1, FirstName = "Alice", LastName = "Johnson" };
        var patron2 = new Patron { Id = 2, FirstName = "Bob", LastName = "Smith" };

        var book = new Book { Id = 1, Title = "Book 1", NumberOfPages = 300 };

        var loans = new List<Loan>
        {
            new Loan { Id = 1, Book = book, Patron = patron1, LoanDate = new DateTime(2024, 1, 5), DueDate = new DateTime(2024, 1, 19) },
            new Loan { Id = 2, Book = book, Patron = patron1, LoanDate = new DateTime(2024, 2, 1), DueDate = new DateTime(2024, 2, 15) },
            new Loan { Id = 3, Book = book, Patron = patron2, LoanDate = new DateTime(2024, 1, 10), DueDate = new DateTime(2024, 1, 24) },
            new Loan { Id = 4, Book = book, Patron = patron2, LoanDate = new DateTime(2024, 2, 5), DueDate = new DateTime(2024, 2, 19) }
        };

        _mockLoanRepository
            .Setup(r => r.GetLoansByTime(startDate, endDate))
            .ReturnsAsync(loans);

        // Act
        var result = await _userActivity.GetPatronLoansOrderedByFrequency(startDate, endDate);

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal(2, result[0].LoanCount);
        Assert.Equal(2, result[1].LoanCount);
    }

    #endregion

    #region GetPagesPerDayByUser Tests

    [Fact]
    public async Task GetPagesPerDayByUser_WithCompletedLoans_CalculatesCorrectPagesPerDay()
    {
        // Arrange
        var patron1 = new Patron { Id = 1, FirstName = "Alice", LastName = "Johnson" };
        var patron2 = new Patron { Id = 2, FirstName = "Bob", LastName = "Smith" };

        var book1 = new Book { Id = 1, Title = "Book 1", NumberOfPages = 300 };
        var book2 = new Book { Id = 2, Title = "Book 2", NumberOfPages = 200 };

        var loans = new List<Loan>
        {
            // Patron1: 300 pages in 10 days = 30 pages/day
            new Loan
            {
                Id = 1,
                Book = book1,
                Patron = patron1,
                LoanDate = new DateTime(2024, 1, 1),
                DueDate = new DateTime(2024, 1, 15),
                ReturnDate = new DateTime(2024, 1, 11),
                IsReturned = true
            },
            // Patron2: 200 pages in 5 days = 40 pages/day
            new Loan
            {
                Id = 2,
                Book = book2,
                Patron = patron2,
                LoanDate = new DateTime(2024, 1, 5),
                DueDate = new DateTime(2024, 1, 19),
                ReturnDate = new DateTime(2024, 1, 10),
                IsReturned = true
            }
        };

        _mockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await _userActivity.GetPagesPerDayByUser();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey(patron1));
        Assert.True(result.ContainsKey(patron2));
        Assert.Equal(30.0, result[patron1]!.Value);
        Assert.Equal(40.0, result[patron2]!.Value);
    }

    [Fact]
    public async Task GetPagesPerDayByUser_WithOverlappingLoans_MergesTimeIntervalsCorrectly()
    {
        // Arrange
        var patron = new Patron { Id = 1, FirstName = "Alice", LastName = "Johnson" };

        var book1 = new Book { Id = 1, Title = "Book 1", NumberOfPages = 300 };
        var book2 = new Book { Id = 2, Title = "Book 2", NumberOfPages = 200 };

        var loans = new List<Loan>
        {
            // First loan: Jan 1 to Jan 11 (10 days)
            new Loan
            {
                Id = 1,
                Book = book1,
                Patron = patron,
                LoanDate = new DateTime(2024, 1, 1),
                DueDate = new DateTime(2024, 1, 15),
                ReturnDate = new DateTime(2024, 1, 11),
                IsReturned = true
            },
            // Second loan: Jan 5 to Jan 15 (overlaps with first by 6 days)
            // Merged time: Jan 1 to Jan 15 (14 days)
            new Loan
            {
                Id = 2,
                Book = book2,
                Patron = patron,
                LoanDate = new DateTime(2024, 1, 5),
                DueDate = new DateTime(2024, 1, 20),
                ReturnDate = new DateTime(2024, 1, 15),
                IsReturned = true
            }
        };

        _mockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await _userActivity.GetPagesPerDayByUser();

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey(patron));
        // Total pages: 300 + 200 = 500
        // Total time (merged): 14 days
        // Pages per day: 500 / 14 ≈ 35.71
        Assert.Equal(500.0 / 14.0, result[patron]!.Value, precision: 2);
    }

    [Fact]
    public async Task GetPagesPerDayByUser_WithNonOverlappingLoans_SumsTimeCorrectly()
    {
        // Arrange
        var patron = new Patron { Id = 1, FirstName = "Alice", LastName = "Johnson" };

        var book1 = new Book { Id = 1, Title = "Book 1", NumberOfPages = 300 };
        var book2 = new Book { Id = 2, Title = "Book 2", NumberOfPages = 200 };

        var loans = new List<Loan>
        {
            // First loan: Jan 1 to Jan 10 (9 days)
            new Loan
            {
                Id = 1,
                Book = book1,
                Patron = patron,
                LoanDate = new DateTime(2024, 1, 1),
                DueDate = new DateTime(2024, 1, 15),
                ReturnDate = new DateTime(2024, 1, 10),
                IsReturned = true
            },
            // Second loan: Jan 20 to Jan 25 (5 days)
            // Total time: 9 + 5 = 14 days
            new Loan
            {
                Id = 2,
                Book = book2,
                Patron = patron,
                LoanDate = new DateTime(2024, 1, 20),
                DueDate = new DateTime(2024, 1, 30),
                ReturnDate = new DateTime(2024, 1, 25),
                IsReturned = true
            }
        };

        _mockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await _userActivity.GetPagesPerDayByUser();

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey(patron));
        // Total pages: 300 + 200 = 500
        // Total time: 14 days
        // Pages per day: 500 / 14 ≈ 35.71
        Assert.Equal(500.0 / 14.0, result[patron]!.Value, precision: 2);
    }

    [Fact]
    public async Task GetPagesPerDayByUser_WithIncompleteLoans_SkipsThoseLoans()
    {
        // Arrange
        var patron = new Patron { Id = 1, FirstName = "Alice", LastName = "Johnson" };

        var book1 = new Book { Id = 1, Title = "Book 1", NumberOfPages = 300 };
        var book2 = new Book { Id = 2, Title = "Book 2", NumberOfPages = 200 };

        var loans = new List<Loan>
        {
            // Completed loan
            new Loan
            {
                Id = 1,
                Book = book1,
                Patron = patron,
                LoanDate = new DateTime(2024, 1, 1),
                DueDate = new DateTime(2024, 1, 15),
                ReturnDate = new DateTime(2024, 1, 11),
                IsReturned = true
            },
            // Incomplete loan (no return date)
            new Loan
            {
                Id = 2,
                Book = book2,
                Patron = patron,
                LoanDate = new DateTime(2024, 1, 5),
                DueDate = new DateTime(2024, 1, 19),
                ReturnDate = null,
                IsReturned = false
            }
        };

        _mockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await _userActivity.GetPagesPerDayByUser();

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey(patron));
        // Only the completed loan counts: 300 pages in 10 days = 30 pages/day
        Assert.Equal(30.0, result[patron]!.Value);
    }

    [Fact]
    public async Task GetPagesPerDayByUser_WithBooksWithoutPageCount_SkipsThoseLoans()
    {
        // Arrange
        var patron = new Patron { Id = 1, FirstName = "Alice", LastName = "Johnson" };

        var book1 = new Book { Id = 1, Title = "Book 1", NumberOfPages = 300 };
        var book2 = new Book { Id = 2, Title = "Book 2", NumberOfPages = null };

        var loans = new List<Loan>
        {
            // Loan with page count
            new Loan
            {
                Id = 1,
                Book = book1,
                Patron = patron,
                LoanDate = new DateTime(2024, 1, 1),
                DueDate = new DateTime(2024, 1, 15),
                ReturnDate = new DateTime(2024, 1, 11),
                IsReturned = true
            },
            // Loan without page count
            new Loan
            {
                Id = 2,
                Book = book2,
                Patron = patron,
                LoanDate = new DateTime(2024, 1, 5),
                DueDate = new DateTime(2024, 1, 19),
                ReturnDate = new DateTime(2024, 1, 10),
                IsReturned = true
            }
        };

        _mockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await _userActivity.GetPagesPerDayByUser();

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey(patron));
        // Only the loan with page count: 300 pages in 10 days = 30 pages/day
        Assert.Equal(30.0, result[patron]!.Value);
    }

    [Fact]
    public async Task GetPagesPerDayByUser_WithNoCompletedLoans_ExcludesPatron()
    {
        // Arrange
        var patron1 = new Patron { Id = 1, FirstName = "Alice", LastName = "Johnson" };
        var patron2 = new Patron { Id = 2, FirstName = "Bob", LastName = "Smith" };

        var book1 = new Book { Id = 1, Title = "Book 1", NumberOfPages = 300 };
        var book2 = new Book { Id = 2, Title = "Book 2", NumberOfPages = 200 };

        var loans = new List<Loan>
        {
            // Patron1 with completed loan
            new Loan
            {
                Id = 1,
                Book = book1,
                Patron = patron1,
                LoanDate = new DateTime(2024, 1, 1),
                DueDate = new DateTime(2024, 1, 15),
                ReturnDate = new DateTime(2024, 1, 11),
                IsReturned = true
            },
            // Patron2 with incomplete loan
            new Loan
            {
                Id = 2,
                Book = book2,
                Patron = patron2,
                LoanDate = new DateTime(2024, 1, 5),
                DueDate = new DateTime(2024, 1, 19),
                ReturnDate = null,
                IsReturned = false
            }
        };

        _mockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await _userActivity.GetPagesPerDayByUser();

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey(patron1));
        Assert.False(result.ContainsKey(patron2));
    }

    [Fact]
    public async Task GetPagesPerDayByUser_WithNoLoans_ReturnsEmptyDictionary()
    {
        // Arrange
        _mockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(new List<Loan>());

        // Act
        var result = await _userActivity.GetPagesPerDayByUser();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPagesPerDayByUser_WithMultiplePatrons_CalculatesIndependently()
    {
        // Arrange
        var patron1 = new Patron { Id = 1, FirstName = "Alice", LastName = "Johnson" };
        var patron2 = new Patron { Id = 2, FirstName = "Bob", LastName = "Smith" };
        var patron3 = new Patron { Id = 3, FirstName = "Carol", LastName = "Williams" };

        var book = new Book { Id = 1, Title = "Book 1", NumberOfPages = 400 };

        var loans = new List<Loan>
        {
            // Patron1: 400 pages in 10 days = 40 pages/day
            new Loan
            {
                Id = 1,
                Book = book,
                Patron = patron1,
                LoanDate = new DateTime(2024, 1, 1),
                DueDate = new DateTime(2024, 1, 15),
                ReturnDate = new DateTime(2024, 1, 11),
                IsReturned = true
            },
            // Patron2: 400 pages in 20 days = 20 pages/day
            new Loan
            {
                Id = 2,
                Book = book,
                Patron = patron2,
                LoanDate = new DateTime(2024, 1, 1),
                DueDate = new DateTime(2024, 1, 25),
                ReturnDate = new DateTime(2024, 1, 21),
                IsReturned = true
            },
            // Patron3: 400 pages in 5 days = 80 pages/day
            new Loan
            {
                Id = 3,
                Book = book,
                Patron = patron3,
                LoanDate = new DateTime(2024, 1, 10),
                DueDate = new DateTime(2024, 1, 20),
                ReturnDate = new DateTime(2024, 1, 15),
                IsReturned = true
            }
        };

        _mockLoanRepository
            .Setup(r => r.GetAllLoans())
            .ReturnsAsync(loans);

        // Act
        var result = await _userActivity.GetPagesPerDayByUser();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(40.0, result[patron1]!.Value);
        Assert.Equal(20.0, result[patron2]!.Value);
        Assert.Equal(80.0, result[patron3]!.Value);
    }

    #endregion
}
