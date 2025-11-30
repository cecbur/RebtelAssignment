using DataStorageContracts;
using Moq;

namespace BusinessLogicTests.BookPatterns;

public abstract class BookPatternsTestBase : CommonTestBase
{
    protected readonly Mock<ILoanRepository> MockLoanRepository;
    protected readonly Mock<IBookRepository> MockBookRepository;
    protected readonly BusinessLogic.BookPatterns BookPatterns;

    protected BookPatternsTestBase()
    {
        MockLoanRepository = new Mock<ILoanRepository>();
        MockBookRepository = new Mock<IBookRepository>();
        BookPatterns = new BusinessLogic.BookPatterns(MockLoanRepository.Object, MockBookRepository.Object);
    }
}
