using BusinessModels;

namespace BusinessLogicContracts.Dto;

    public class PatronLoans
    {
        public PatronLoans(Patron patron, Loan[] loans)
        {
            Patron = patron;
            Loans = loans;
        }

        public int LoanCount => Loans.Count();
        public Patron Patron { get;}
        public Loan[] Loans { get;}
    }
