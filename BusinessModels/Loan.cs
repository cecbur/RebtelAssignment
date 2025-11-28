namespace BusinessModels;

public class Loan
{
    public int Id { get; set; }

    public Book? Book { get; set; }

    public Patron? Patron { get; set; }

    public DateTime LoanDate { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public bool IsReturned { get; set; } = false;
}
