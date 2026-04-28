namespace Biblioteka.Models;

public sealed class Book : ObservableObject
{
    private int _bookId;
    private string _title = string.Empty;
    private int _authorId;
    private string _authorName = string.Empty;
    private string _publisher = string.Empty;
    private int? _publishYear;
    private string _genre = string.Empty;
    private string _isbn = string.Empty;
    private int _copyCount;

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

    public int AuthorId
    {
        get => _authorId;
        set => SetProperty(ref _authorId, value);
    }

    public string AuthorName
    {
        get => _authorName;
        set => SetProperty(ref _authorName, value);
    }

    public string Publisher
    {
        get => _publisher;
        set => SetProperty(ref _publisher, value);
    }

    public int? PublishYear
    {
        get => _publishYear;
        set => SetProperty(ref _publishYear, value);
    }

    public string Genre
    {
        get => _genre;
        set => SetProperty(ref _genre, value);
    }

    public string ISBN
    {
        get => _isbn;
        set => SetProperty(ref _isbn, value);
    }

    public int CopyCount
    {
        get => _copyCount;
        set => SetProperty(ref _copyCount, value);
    }

    public string DisplayTitle => string.IsNullOrWhiteSpace(AuthorName)
        ? Title
        : $"{Title} - {AuthorName}";

    public Book Clone()
    {
        return new Book
        {
            BookId = BookId,
            Title = Title,
            AuthorId = AuthorId,
            AuthorName = AuthorName,
            Publisher = Publisher,
            PublishYear = PublishYear,
            Genre = Genre,
            ISBN = ISBN,
            CopyCount = CopyCount
        };
    }
}
