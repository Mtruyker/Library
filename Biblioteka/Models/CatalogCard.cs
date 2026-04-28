namespace Biblioteka.Models;

public sealed class CatalogCard : ObservableObject
{
    private string _title = "Выберите книгу";
    private string _authorName = "-";
    private string _publisher = "-";
    private string _publishYear = "-";
    private string _genre = "-";
    private string _isbn = "-";
    private int _copyCount;

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

    public string Publisher
    {
        get => _publisher;
        set => SetProperty(ref _publisher, value);
    }

    public string PublishYear
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
}
