namespace BusinessModels;

public class Loan
{
    public int Id { get; set; }

    public required Book Book { get; set; } 

    public required Patron Patron { get; set; }

    public DateTime LoanDate { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public bool IsReturned { get; set; } = false;
}
