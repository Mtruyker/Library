namespace Biblioteka.Models;

public sealed class BookCopy : ObservableObject
{
    private int _copyId;
    private int _bookId;
    private string _title = string.Empty;
    private string _authorName = string.Empty;
    private string _inventoryNumber = string.Empty;
    private string _location = string.Empty;
    private string _status = "доступна";

    public int CopyId
    {
        get => _copyId;
        set => SetProperty(ref _copyId, value);
    }

    public int BookId
    {
        get => _bookId;
        set => SetProperty(ref _bookId, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string AuthorName
    {
        get => _authorName;
        set => SetProperty(ref _authorName, value);
    }

    public string InventoryNumber
    {
        get => _inventoryNumber;
        set => SetProperty(ref _inventoryNumber, value);
    }

    public string Location
    {
        get => _location;
        set => SetProperty(ref _location, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string DisplayLabel => $"{InventoryNumber} | {Title} | {Location}";

    public BookCopy Clone()
    {
        return new BookCopy
        {
            CopyId = CopyId,
            BookId = BookId,
            Title = Title,
            AuthorName = AuthorName,
            InventoryNumber = InventoryNumber,
            Location = Location,
            Status = Status
        };
    }
}
