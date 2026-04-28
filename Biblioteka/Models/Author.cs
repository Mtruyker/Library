namespace Biblioteka.Models;

public sealed class Author : ObservableObject
{
    private int _authorId;
    private string _fullName = string.Empty;

    public int AuthorId
    {
        get => _authorId;
        set => SetProperty(ref _authorId, value);
    }

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    public override string ToString()
    {
        return FullName;
    }
}
