using DataStorageContracts;
using Moq;

namespace BusinessLogic.Tests.PatronActivity;

public abstract class PatronActivityTestBase : CommonTestBase
{
    protected readonly Mock<ILoanRepository> MockLoanRepository;
    protected readonly BusinessLogic.PatronActivity PatronActivity;

    protected PatronActivityTestBase()
    {
        MockLoanRepository = new Mock<ILoanRepository>();
        PatronActivity = new BusinessLogic.PatronActivity(MockLoanRepository.Object);
    }
}
