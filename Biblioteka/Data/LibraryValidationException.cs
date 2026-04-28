namespace Biblioteka.Data;

public sealed class LibraryValidationException : Exception
{
    public LibraryValidationException(string message) : base(message)
    {
    }
}
