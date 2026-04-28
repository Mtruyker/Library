using System.Configuration;
using System.Data;
using Biblioteka.Models;
using Microsoft.Data.SqlClient;

namespace Biblioteka.Data;

public sealed class LibraryRepository
{
    private static readonly HashSet<string> ValidCopyStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "доступна",
        "выдана",
        "потеряна"
    };

    private readonly string _connectionString;

    public LibraryRepository()
    {
        var settings = ConfigurationManager.ConnectionStrings["LibraryDB"];
        if (settings is null || string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            throw new InvalidOperationException(
                "Строка подключения LibraryDB не найдена. Проверьте файл App.config.");
        }

        _connectionString = settings.ConnectionString;
    }

    public List<Reader> GetReaders(string? searchText)
    {
        const string sql = """
                           SELECT ReaderId, FullName, Phone, Address, TicketNumber
                           FROM Readers
                           WHERE @search = N''
                              OR FullName LIKE N'%' + @search + N'%'
                              OR TicketNumber LIKE N'%' + @search + N'%'
                           ORDER BY FullName;
                           """;

        var items = new List<Reader>();
        using var connection = CreateOpenConnection();
        using var command = CreateCommand(connection, sql);
        command.Parameters.Add(StringParameter("@search", 100, searchText));

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new Reader
            {
                ReaderId = reader.GetInt32("ReaderId"),
                FullName = reader.GetString("FullName"),
                Phone = reader.GetStringOrEmpty("Phone"),
                Address = reader.GetStringOrEmpty("Address"),
                TicketNumber = reader.GetString("TicketNumber")
            });
        }

        return items;
    }

    public void SaveReader(Reader reader)
    {
        if (string.IsNullOrWhiteSpace(reader.FullName))
        {
            throw new LibraryValidationException("Укажите ФИО читателя.");
        }

        if (string.IsNullOrWhiteSpace(reader.TicketNumber))
        {
            throw new LibraryValidationException("Укажите номер читательского билета.");
        }

        const string insertSql = """
                                 INSERT INTO Readers (FullName, Phone, Address, TicketNumber)
                                 VALUES (@fullName, @phone, @address, @ticketNumber);
                                 """;

        const string updateSql = """
                                 UPDATE Readers
                                 SET FullName = @fullName,
                                     Phone = @phone,
                                     Address = @address,
                                     TicketNumber = @ticketNumber
                                 WHERE ReaderId = @readerId;
                                 """;

        using var connection = CreateOpenConnection();
        using var command = CreateCommand(connection, reader.ReaderId == 0 ? insertSql : updateSql);
        command.Parameters.Add(StringParameter("@fullName", 100, reader.FullName));
        command.Parameters.Add(StringParameter("@phone", 20, reader.Phone));
        command.Parameters.Add(StringParameter("@address", 200, reader.Address));
        command.Parameters.Add(StringParameter("@ticketNumber", 30, reader.TicketNumber));

        if (reader.ReaderId != 0)
        {
            command.Parameters.Add(IntParameter("@readerId", reader.ReaderId));
        }

        command.ExecuteNonQuery();
    }

    public void DeleteReader(int readerId)
    {
        const string sql = "DELETE FROM Readers WHERE ReaderId = @readerId;";
        using var connection = CreateOpenConnection();
        using var command = CreateCommand(connection, sql);
        command.Parameters.Add(IntParameter("@readerId", readerId));
        command.ExecuteNonQuery();
    }

    public List<Author> GetAuthors()
    {
        const string sql = """
                           SELECT AuthorId, FullName
                           FROM Authors
                           ORDER BY FullName;
                           """;

        var items = new List<Author>();
        using var connection = CreateOpenConnection();
        using var command = CreateCommand(connection, sql);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            items.Add(new Author
            {
                AuthorId = reader.GetInt32("AuthorId"),
                FullName = reader.GetString("FullName")
            });
        }

        return items;
    }

    public void AddAuthor(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new LibraryValidationException("Введите ФИО автора.");
        }

        const string sql = """
                           IF NOT EXISTS (SELECT 1 FROM Authors WHERE FullName = @fullName)
                           BEGIN
                               INSERT INTO Authors (FullName) VALUES (@fullName);
                           END
                           """;

        using var connection = CreateOpenConnection();
        using var command = CreateCommand(connection, sql);
        command.Parameters.Add(StringParameter("@fullName", 100, fullName));
        command.ExecuteNonQuery();
    }

    public List<Book> GetBooks(string? title, string? author, string? genre, int? publishYear)
    {
        const string sql = """
                           SELECT b.BookId,
                                  b.Title,
                                  b.AuthorId,
                                  a.FullName AS AuthorName,
                                  b.Publisher,
                                  b.PublishYear,
                                  b.Genre,
                                  b.ISBN,
                                  COUNT(c.CopyId) AS CopyCount
                           FROM Books b
                           INNER JOIN Authors a ON a.AuthorId = b.AuthorId
                           LEFT JOIN BookCopies c ON c.BookId = b.BookId
                           WHERE (@title = N'' OR b.Title LIKE N'%' + @title + N'%')
                             AND (@author = N'' OR a.FullName LIKE N'%' + @author + N'%')
                             AND (@genre = N'' OR b.Genre LIKE N'%' + @genre + N'%')
                             AND (@publishYear IS NULL OR b.PublishYear = @publishYear)
                           GROUP BY b.BookId, b.Title, b.AuthorId, a.FullName, b.Publisher, b.PublishYear, b.Genre, b.ISBN
                           ORDER BY b.Title;
                           """;

        var items = new List<Book>();
        using var connection = CreateOpenConnection();
        using var command = CreateCommand(connection, sql);
        command.Parameters.Add(StringParameter("@title", 150, title));
        command.Parameters.Add(StringParameter("@author", 100, author));
        command.Parameters.Add(StringParameter("@genre", 50, genre));
        command.Parameters.Add(NullableIntParameter("@publishYear", publishYear));

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new Book
            {
                BookId = reader.GetInt32("BookId"),
                Title = reader.GetString("Title"),
                AuthorId = reader.GetInt32("AuthorId"),
                AuthorName = reader.GetString("AuthorName"),
                Publisher = reader.GetStringOrEmpty("Publisher"),
                PublishYear = reader.GetNullableInt32("PublishYear"),
                Genre = reader.GetStringOrEmpty("Genre"),
                ISBN = reader.GetStringOrEmpty("ISBN"),
                CopyCount = reader.GetInt32("CopyCount")
            });
        }

        return items;
    }

    public void SaveBook(Book book)
    {
        if (string.IsNullOrWhiteSpace(book.Title))
        {
            throw new LibraryValidationException("Укажите название издания.");
        }

        if (book.AuthorId <= 0)
        {
            throw new LibraryValidationException("Выберите автора.");
        }

        if (book.PublishYear is < 0 or > 3000)
        {
            throw new LibraryValidationException("Укажите корректный год издания.");
        }

        const string insertSql = """
                                 INSERT INTO Books (Title, AuthorId, Publisher, PublishYear, Genre, ISBN)
                                 VALUES (@title, @authorId, @publisher, @publishYear, @genre, @isbn);
                                 """;

        const string updateSql = """
                                 UPDATE Books
                                 SET Title = @title,
                                     AuthorId = @authorId,
                                     Publisher = @publisher,
                                     PublishYear = @publishYear,
                                     Genre = @genre,
                                     ISBN = @isbn
                                 WHERE BookId = @bookId;
                                 """;

        using var connection = CreateOpenConnection();
        using var command = CreateCommand(connection, book.BookId == 0 ? insertSql : updateSql);
        command.Parameters.Add(StringParameter("@title", 150, book.Title));
        command.Parameters.Add(IntParameter("@authorId", book.AuthorId));
        command.Parameters.Add(StringParameter("@publisher", 100, book.Publisher));
        command.Parameters.Add(NullableIntParameter("@publishYear", book.PublishYear));
        command.Parameters.Add(StringParameter("@genre", 50, book.Genre));
        command.Parameters.Add(StringParameter("@isbn", 30, book.ISBN));

        if (book.BookId != 0)
        {
            command.Parameters.Add(IntParameter("@bookId", book.BookId));
        }

        command.ExecuteNonQuery();
    }

    public void DeleteBook(int bookId)
    {
        const string sql = "DELETE FROM Books WHERE BookId = @bookId;";
        using var connection = CreateOpenConnection();
        using var command = CreateCommand(connection, sql);
        command.Parameters.Add(IntParameter("@bookId", bookId));
        command.ExecuteNonQuery();
    }

    public List<BookCopy> GetBookCopies(string? searchText, string? status)
    {
        const string sql = """
                           SELECT c.CopyId,
                                  c.BookId,
                                  b.Title,
                                  a.FullName AS AuthorName,
                                  c.InventoryNumber,
                                  c.Location,
                                  c.Status
                           FROM BookCopies c
                           INNER JOIN Books b ON b.BookId = c.BookId
                           INNER JOIN Authors a ON a.AuthorId = b.AuthorId
                           WHERE (@search = N''
                                  OR c.InventoryNumber LIKE N'%' + @search + N'%'
                                  OR c.Location LIKE N'%' + @search + N'%'
                                  OR b.Title LIKE N'%' + @search + N'%'
                                  OR a.FullName LIKE N'%' + @search + N'%')
                             AND (@status = N'' OR c.Status = @status)
                           ORDER BY b.Title, c.InventoryNumber;
                           """;

        var items = new List<BookCopy>();
        using var connection = CreateOpenConnection();
        using var command = CreateCommand(connection, sql);
        command.Parameters.Add(StringParameter("@search", 150, searchText));
        command.Parameters.Add(StringParameter("@status", 30, status));

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new BookCopy
            {
                CopyId = reader.GetInt32("CopyId"),
                BookId = reader.GetInt32("BookId"),
                Title = reader.GetString("Title"),
                AuthorName = reader.GetString("AuthorName"),
                InventoryNumber = reader.GetString("InventoryNumber"),
                Location = reader.GetStringOrEmpty("Location"),
                Status = reader.GetString("Status")
            });
        }

        return items;
    }

    public void SaveBookCopy(BookCopy copy)
    {
        if (copy.BookId <= 0)
        {
            throw new LibraryValidationException("Выберите издание для экземпляра.");
        }

        if (string.IsNullOrWhiteSpace(copy.InventoryNumber))
        {
            throw new LibraryValidationException("Укажите инвентарный номер.");
        }

        if (string.IsNullOrWhiteSpace(copy.Location))
        {
            throw new LibraryValidationException("Укажите местонахождение экземпляра.");
        }

        if (!ValidCopyStatuses.Contains(copy.Status))
        {
            throw new LibraryValidationException("Выберите корректный статус экземпляра.");
        }

        const string insertSql = """
                                 INSERT INTO BookCopies (BookId, InventoryNumber, Location, Status)
                                 VALUES (@bookId, @inventoryNumber, @location, @status);
                                 """;

        const string updateSql = """
                                 UPDATE BookCopies
                                 SET BookId = @bookId,
                                     InventoryNumber = @inventoryNumber,
                                     Location = @location,
                                     Status = @status
                                 WHERE CopyId = @copyId;
                                 """;

        using var connection = CreateOpenConnection();
        using var command = CreateCommand(connection, copy.CopyId == 0 ? insertSql : updateSql);
        command.Parameters.Add(IntParameter("@bookId", copy.BookId));
        command.Parameters.Add(StringParameter("@inventoryNumber", 30, copy.InventoryNumber));
        command.Parameters.Add(StringParameter("@location", 100, copy.Location));
        command.Parameters.Add(StringParameter("@status", 30, copy.Status));

        if (copy.CopyId != 0)
        {
            command.Parameters.Add(IntParameter("@copyId", copy.CopyId));
        }

        command.ExecuteNonQuery();
    }

    public void DeleteBookCopy(int copyId)
    {
        const string sql = "DELETE FROM BookCopies WHERE CopyId = @copyId;";
        using var connection = CreateOpenConnection();
        using var command = CreateCommand(connection, sql);
        command.Parameters.Add(IntParameter("@copyId", copyId));
        command.ExecuteNonQuery();
    }

    public List<BookCopy> GetAvailableCopies()
    {
        return GetBookCopies(string.Empty, "доступна");
    }

    public List<Loan> GetActiveLoans(string? searchText)
    {
        const string sql = """
                           SELECT l.LoanId,
                                  l.ReaderId,
                                  l.CopyId,
                                  r.FullName AS ReaderName,
                                  r.TicketNumber,
                                  b.Title AS BookTitle,
                                  c.InventoryNumber,
                                  l.IssueDate,
                                  l.DueDate,
                                  l.ReturnDate
                           FROM Loans l
                           INNER JOIN Readers r ON r.ReaderId = l.ReaderId
                           INNER JOIN BookCopies c ON c.CopyId = l.CopyId
                           INNER JOIN Books b ON b.BookId = c.BookId
                           WHERE l.ReturnDate IS NULL
                             AND (@search = N''
                                  OR r.FullName LIKE N'%' + @search + N'%'
                                  OR r.TicketNumber LIKE N'%' + @search + N'%'
                                  OR b.Title LIKE N'%' + @search + N'%'
                                  OR c.InventoryNumber LIKE N'%' + @search + N'%')
                           ORDER BY l.DueDate, r.FullName;
                           """;

        var items = new List<Loan>();
        using var connection = CreateOpenConnection();
        using var command = CreateCommand(connection, sql);
        command.Parameters.Add(StringParameter("@search", 150, searchText));
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            items.Add(MapLoan(reader));
        }

        return items;
    }

    public void IssueBook(int readerId, int copyId, DateTime issueDate, DateTime dueDate)
    {
        if (readerId <= 0)
        {
            throw new LibraryValidationException("Выберите читателя.");
        }

        if (copyId <= 0)
        {
            throw new LibraryValidationException("Выберите экземпляр книги.");
        }

        if (dueDate.Date < issueDate.Date)
        {
            throw new LibraryValidationException("Срок возврата не может быть раньше даты выдачи.");
        }

        using var connection = CreateOpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string statusSql = """
                                     SELECT Status
                                     FROM BookCopies
                                     WHERE CopyId = @copyId;
                                     """;
            using var statusCommand = CreateCommand(connection, statusSql, transaction);
            statusCommand.Parameters.Add(IntParameter("@copyId", copyId));
            var status = statusCommand.ExecuteScalar() as string;

            if (status is null)
            {
                throw new LibraryValidationException("Выбранный экземпляр не найден.");
            }

            if (!string.Equals(status, "доступна", StringComparison.OrdinalIgnoreCase))
            {
                throw new LibraryValidationException(
                    "Нельзя выдать выбранный экземпляр. Его статус не 'доступна'.");
            }

            const string updateCopySql = """
                                         UPDATE BookCopies
                                         SET Status = N'выдана'
                                         WHERE CopyId = @copyId
                                           AND Status = N'доступна';
                                         """;
            using var updateCopyCommand = CreateCommand(connection, updateCopySql, transaction);
            updateCopyCommand.Parameters.Add(IntParameter("@copyId", copyId));

            if (updateCopyCommand.ExecuteNonQuery() == 0)
            {
                throw new LibraryValidationException("Экземпляр уже недоступен для выдачи.");
            }

            const string insertLoanSql = """
                                         INSERT INTO Loans (ReaderId, CopyId, IssueDate, DueDate, ReturnDate)
                                         VALUES (@readerId, @copyId, @issueDate, @dueDate, NULL);
                                         """;
            using var insertLoanCommand = CreateCommand(connection, insertLoanSql, transaction);
            insertLoanCommand.Parameters.Add(IntParameter("@readerId", readerId));
            insertLoanCommand.Parameters.Add(IntParameter("@copyId", copyId));
            insertLoanCommand.Parameters.Add(DateParameter("@issueDate", issueDate));
            insertLoanCommand.Parameters.Add(DateParameter("@dueDate", dueDate));
            insertLoanCommand.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void ReturnBook(int loanId, DateTime returnDate)
    {
        if (loanId <= 0)
        {
            throw new LibraryValidationException("Выберите запись выдачи для возврата.");
        }

        using var connection = CreateOpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string getLoanSql = """
                                      SELECT CopyId, IssueDate, ReturnDate
                                      FROM Loans
                                      WHERE LoanId = @loanId;
                                      """;
            using var getLoanCommand = CreateCommand(connection, getLoanSql, transaction);
            getLoanCommand.Parameters.Add(IntParameter("@loanId", loanId));
            using var reader = getLoanCommand.ExecuteReader();

            if (!reader.Read())
            {
                throw new LibraryValidationException("Запись выдачи не найдена.");
            }

            var copyId = reader.GetInt32("CopyId");
            var issueDate = reader.GetDateTime("IssueDate");
            DateTime? existingReturnDate = reader.IsDBNull("ReturnDate")
                ? null
                : reader.GetDateTime("ReturnDate");
            reader.Close();

            if (existingReturnDate.HasValue)
            {
                throw new LibraryValidationException("Эта книга уже была возвращена.");
            }

            if (returnDate.Date < issueDate.Date)
            {
                throw new LibraryValidationException("Дата возврата не может быть раньше даты выдачи.");
            }

            const string updateLoanSql = """
                                         UPDATE Loans
                                         SET ReturnDate = @returnDate
                                         WHERE LoanId = @loanId
                                           AND ReturnDate IS NULL;
                                         """;
            using var updateLoanCommand = CreateCommand(connection, updateLoanSql, transaction);
            updateLoanCommand.Parameters.Add(DateParameter("@returnDate", returnDate));
            updateLoanCommand.Parameters.Add(IntParameter("@loanId", loanId));
            updateLoanCommand.ExecuteNonQuery();

            const string updateCopySql = """
                                         UPDATE BookCopies
                                         SET Status = N'доступна'
                                         WHERE CopyId = @copyId;
                                         """;
            using var updateCopyCommand = CreateCommand(connection, updateCopySql, transaction);
            updateCopyCommand.Parameters.Add(IntParameter("@copyId", copyId));
            updateCopyCommand.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public List<Loan> GetDebtors(DateTime currentDate)
    {
        const string sql = """
                           SELECT l.LoanId,
                                  l.ReaderId,
                                  l.CopyId,
                                  r.FullName AS ReaderName,
                                  r.TicketNumber,
                                  b.Title AS BookTitle,
                                  c.InventoryNumber,
                                  l.IssueDate,
                                  l.DueDate,
                                  l.ReturnDate
                           FROM Loans l
                           INNER JOIN Readers r ON r.ReaderId = l.ReaderId
                           INNER JOIN BookCopies c ON c.CopyId = l.CopyId
                           INNER JOIN Books b ON b.BookId = c.BookId
                           WHERE l.ReturnDate IS NULL
                             AND l.DueDate < @today
                           ORDER BY l.DueDate, r.FullName;
                           """;

        var items = new List<Loan>();
        using var connection = CreateOpenConnection();
        using var command = CreateCommand(connection, sql);
        command.Parameters.Add(DateParameter("@today", currentDate));
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            items.Add(MapLoan(reader));
        }

        return items;
    }

    public CatalogCard GetCatalogCard(int bookId)
    {
        const string sql = """
                           SELECT b.Title,
                                  a.FullName AS AuthorName,
                                  b.Publisher,
                                  b.PublishYear,
                                  b.Genre,
                                  b.ISBN,
                                  COUNT(c.CopyId) AS CopyCount
                           FROM Books b
                           INNER JOIN Authors a ON a.AuthorId = b.AuthorId
                           LEFT JOIN BookCopies c ON c.BookId = b.BookId
                           WHERE b.BookId = @bookId
                           GROUP BY b.Title, a.FullName, b.Publisher, b.PublishYear, b.Genre, b.ISBN;
                           """;

        using var connection = CreateOpenConnection();
        using var command = CreateCommand(connection, sql);
        command.Parameters.Add(IntParameter("@bookId", bookId));
        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            throw new LibraryValidationException("Выбранное издание не найдено.");
        }

        return new CatalogCard
        {
            Title = reader.GetString("Title"),
            AuthorName = reader.GetString("AuthorName"),
            Publisher = reader.GetStringOrEmpty("Publisher", "-"),
            PublishYear = reader.GetNullableInt32("PublishYear")?.ToString() ?? "-",
            Genre = reader.GetStringOrEmpty("Genre", "-"),
            ISBN = reader.GetStringOrEmpty("ISBN", "-"),
            CopyCount = reader.GetInt32("CopyCount")
        };
    }

    public DashboardStats GetDashboardStats(DateTime currentDate)
    {
        const string sql = """
                           SELECT
                               (SELECT COUNT(*) FROM Readers) AS ReadersCount,
                               (SELECT COUNT(*) FROM Books) AS TitlesCount,
                               (SELECT COUNT(*) FROM BookCopies) AS CopiesCount,
                               (SELECT COUNT(*) FROM BookCopies WHERE Status = N'доступна') AS AvailableCopiesCount,
                               (SELECT COUNT(*) FROM Loans WHERE ReturnDate IS NULL) AS ActiveLoansCount,
                               (SELECT COUNT(*) FROM Loans WHERE ReturnDate IS NULL AND DueDate < @today) AS DebtorsCount;
                           """;

        using var connection = CreateOpenConnection();
        using var command = CreateCommand(connection, sql);
        command.Parameters.Add(DateParameter("@today", currentDate));
        using var reader = command.ExecuteReader();
        reader.Read();

        return new DashboardStats
        {
            ReadersCount = reader.GetInt32("ReadersCount"),
            TitlesCount = reader.GetInt32("TitlesCount"),
            CopiesCount = reader.GetInt32("CopiesCount"),
            AvailableCopiesCount = reader.GetInt32("AvailableCopiesCount"),
            ActiveLoansCount = reader.GetInt32("ActiveLoansCount"),
            DebtorsCount = reader.GetInt32("DebtorsCount")
        };
    }

    private SqlConnection CreateOpenConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static SqlCommand CreateCommand(SqlConnection connection, string sql, SqlTransaction? transaction = null)
    {
        return new SqlCommand(sql, connection, transaction);
    }

    private static SqlParameter StringParameter(string name, int size, string? value)
    {
        return new SqlParameter(name, SqlDbType.NVarChar, size)
        {
            Value = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim()
        };
    }

    private static SqlParameter IntParameter(string name, int value)
    {
        return new SqlParameter(name, SqlDbType.Int)
        {
            Value = value
        };
    }

    private static SqlParameter NullableIntParameter(string name, int? value)
    {
        return new SqlParameter(name, SqlDbType.Int)
        {
            Value = value.HasValue ? value.Value : DBNull.Value
        };
    }

    private static SqlParameter DateParameter(string name, DateTime value)
    {
        return new SqlParameter(name, SqlDbType.Date)
        {
            Value = value.Date
        };
    }

    private static Loan MapLoan(SqlDataReader reader)
    {
        return new Loan
        {
            LoanId = reader.GetInt32("LoanId"),
            ReaderId = reader.GetInt32("ReaderId"),
            CopyId = reader.GetInt32("CopyId"),
            ReaderName = reader.GetString("ReaderName"),
            TicketNumber = reader.GetString("TicketNumber"),
            BookTitle = reader.GetString("BookTitle"),
            InventoryNumber = reader.GetString("InventoryNumber"),
            IssueDate = reader.GetDateTime("IssueDate"),
            DueDate = reader.GetDateTime("DueDate"),
            ReturnDate = reader.IsDBNull("ReturnDate") ? null : reader.GetDateTime("ReturnDate")
        };
    }
}

internal static class SqlReaderExtensions
{
    public static int GetInt32(this SqlDataReader reader, string columnName)
    {
        return reader.GetInt32(reader.GetOrdinal(columnName));
    }

    public static int? GetNullableInt32(this SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    public static string GetString(this SqlDataReader reader, string columnName)
    {
        return reader.GetString(reader.GetOrdinal(columnName));
    }

    public static string GetStringOrEmpty(this SqlDataReader reader, string columnName, string fallback = "")
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? fallback : reader.GetString(ordinal);
    }

    public static DateTime GetDateTime(this SqlDataReader reader, string columnName)
    {
        return reader.GetDateTime(reader.GetOrdinal(columnName));
    }

    public static bool IsDBNull(this SqlDataReader reader, string columnName)
    {
        return reader.IsDBNull(reader.GetOrdinal(columnName));
    }
}
