namespace Biblioteka.Models;

public sealed class Loan : ObservableObject
{
    private int _loanId;
    private int _readerId;
    private int _copyId;
    private string _readerName = string.Empty;
    private string _ticketNumber = string.Empty;
    private string _bookTitle = string.Empty;
    private string _inventoryNumber = string.Empty;
    private DateTime _issueDate = DateTime.Today;
    private DateTime _dueDate = DateTime.Today.AddDays(14);
    private DateTime? _returnDate;

    public int LoanId
    {
        get => _loanId;
        set => SetProperty(ref _loanId, value);
    }

    public int ReaderId
    {
        get => _readerId;
        set => SetProperty(ref _readerId, value);
    }

    public int CopyId
    {
        get => _copyId;
        set => SetProperty(ref _copyId, value);
    }

    public string ReaderName
    {
        get => _readerName;
        set => SetProperty(ref _readerName, value);
    }

    public string TicketNumber
    {
        get => _ticketNumber;
        set => SetProperty(ref _ticketNumber, value);
    }

    public string BookTitle
    {
        get => _bookTitle;
        set => SetProperty(ref _bookTitle, value);
    }

    public string InventoryNumber
    {
        get => _inventoryNumber;
        set => SetProperty(ref _inventoryNumber, value);
    }

    public DateTime IssueDate
    {
        get => _issueDate;
        set => SetProperty(ref _issueDate, value);
    }

    public DateTime DueDate
    {
        get => _dueDate;
        set => SetProperty(ref _dueDate, value);
    }

    public DateTime? ReturnDate
    {
        get => _returnDate;
        set => SetProperty(ref _returnDate, value);
    }
}
