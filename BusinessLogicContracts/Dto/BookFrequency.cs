using BusinessModels;

namespace BusinessLogicContracts.Dto;

    public class BookFrequency
    {
        public required Book AssociatedBook { get; set; }
        public double LoansOfThisBookPerLoansOfMainBook { get; set; }
    }
