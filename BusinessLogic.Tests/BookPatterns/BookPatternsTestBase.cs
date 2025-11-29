using DataStorageContracts;
using Moq;

namespace BusinessLogic.Tests.BookPatterns;

public abstract class BookPatternsTestBase : CommonTestBase
{
    protected readonly Mock<ILoanRepository> MockLoanRepository;
    protected readonly BusinessLogic.BookPatterns BookPatterns;

    protected BookPatternsTestBase()
    {
        MockLoanRepository = new Mock<ILoanRepository>();
        BookPatterns = new BusinessLogic.BookPatterns(MockLoanRepository.Object);
    }
}
