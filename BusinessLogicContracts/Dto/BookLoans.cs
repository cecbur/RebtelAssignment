using BusinessModels;

namespace BusinessLogicContracts.Dto;

    public class BookLoans
    {
        public required Book Book { get; set; }
        public int LoanCount { get; set; }
    }
