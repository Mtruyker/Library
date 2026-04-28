using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Biblioteka.Data;
using Biblioteka.Models;
using Microsoft.Data.SqlClient;

namespace Biblioteka;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly LibraryRepository? _repository;

    private DashboardStats _dashboardStats = new();
    private Reader _readerForm = new();
    private Reader? _selectedReader;
    private string _readerSearchText = string.Empty;
    private Book _bookForm = new();
    private Book? _selectedBook;
    private string _bookPublishYearInput = string.Empty;
    private string _bookTitleSearch = string.Empty;
    private string _bookAuthorSearch = string.Empty;
    private string _bookGenreSearch = string.Empty;
    private string _bookYearSearch = string.Empty;
    private string _newAuthorName = string.Empty;
    private BookCopy _copyForm = new() { Status = "доступна" };
    private BookCopy? _selectedCopy;
    private string _copySearchText = string.Empty;
    private string _selectedCopyStatusFilter = "Все";
    private int _issueReaderId;
    private int _issueCopyId;
    private DateTime? _issueDate = DateTime.Today;
    private DateTime? _dueDate = DateTime.Today.AddDays(14);
    private string _loanSearchText = string.Empty;
    private Loan? _selectedLoanForReturn;
    private DateTime? _returnDate = DateTime.Today;
    private int _selectedCatalogBookId;
    private CatalogCard _currentCatalogCard = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        try
        {
            _repository = new LibraryRepository();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка конфигурации", MessageBoxButton.OK, MessageBoxImage.Error);
            Loaded += (_, _) => Close();
            return;
        }

        Loaded += MainWindow_Loaded;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<Reader> Readers { get; } = new();
    public ObservableCollection<Reader> ReaderLookup { get; } = new();
    public ObservableCollection<Author> Authors { get; } = new();
    public ObservableCollection<Book> Books { get; } = new();
    public ObservableCollection<Book> BookLookup { get; } = new();
    public ObservableCollection<BookCopy> Copies { get; } = new();
    public ObservableCollection<BookCopy> AvailableCopies { get; } = new();
    public ObservableCollection<Loan> ActiveLoans { get; } = new();
    public ObservableCollection<Loan> Debtors { get; } = new();
    public ObservableCollection<Book> BooksReport { get; } = new();
    public ObservableCollection<BookCopy> AvailableCopiesReport { get; } = new();
    public ObservableCollection<Loan> IssuedBooksReport { get; } = new();
    public ObservableCollection<Loan> DebtorsReport { get; } = new();
    public ObservableCollection<string> CopyStatusFilterOptions { get; } = new(new[] { "Все", "доступна", "выдана", "потеряна" });
    public ObservableCollection<string> CopyStatusEditOptions { get; } = new(new[] { "доступна", "выдана", "потеряна" });

    public DashboardStats DashboardStats
    {
        get => _dashboardStats;
        set => SetProperty(ref _dashboardStats, value);
    }

    public Reader ReaderForm
    {
        get => _readerForm;
        set => SetProperty(ref _readerForm, value);
    }

    public Reader? SelectedReader
    {
        get => _selectedReader;
        set
        {
            if (!SetProperty(ref _selectedReader, value))
            {
                return;
            }

            ReaderForm = value?.Clone() ?? new Reader();
        }
    }

    public string ReaderSearchText
    {
        get => _readerSearchText;
        set => SetProperty(ref _readerSearchText, value);
    }

    public Book BookForm
    {
        get => _bookForm;
        set => SetProperty(ref _bookForm, value);
    }

    public Book? SelectedBook
    {
        get => _selectedBook;
        set
        {
            if (!SetProperty(ref _selectedBook, value))
            {
                return;
            }

            BookForm = value?.Clone() ?? new Book();
            BookPublishYearInput = value?.PublishYear?.ToString() ?? string.Empty;
        }
    }

    public string BookPublishYearInput
    {
        get => _bookPublishYearInput;
        set => SetProperty(ref _bookPublishYearInput, value);
    }

    public string BookTitleSearch
    {
        get => _bookTitleSearch;
        set => SetProperty(ref _bookTitleSearch, value);
    }

    public string BookAuthorSearch
    {
        get => _bookAuthorSearch;
        set => SetProperty(ref _bookAuthorSearch, value);
    }

    public string BookGenreSearch
    {
        get => _bookGenreSearch;
        set => SetProperty(ref _bookGenreSearch, value);
    }

    public string BookYearSearch
    {
        get => _bookYearSearch;
        set => SetProperty(ref _bookYearSearch, value);
    }

    public string NewAuthorName
    {
        get => _newAuthorName;
        set => SetProperty(ref _newAuthorName, value);
    }

    public BookCopy CopyForm
    {
        get => _copyForm;
        set => SetProperty(ref _copyForm, value);
    }

    public BookCopy? SelectedCopy
    {
        get => _selectedCopy;
        set
        {
            if (!SetProperty(ref _selectedCopy, value))
            {
                return;
            }

            CopyForm = value?.Clone() ?? CreateEmptyCopy();
        }
    }

    public string CopySearchText
    {
        get => _copySearchText;
        set => SetProperty(ref _copySearchText, value);
    }

    public string SelectedCopyStatusFilter
    {
        get => _selectedCopyStatusFilter;
        set => SetProperty(ref _selectedCopyStatusFilter, value);
    }

    public int IssueReaderId
    {
        get => _issueReaderId;
        set => SetProperty(ref _issueReaderId, value);
    }

    public int IssueCopyId
    {
        get => _issueCopyId;
        set => SetProperty(ref _issueCopyId, value);
    }

    public DateTime? IssueDate
    {
        get => _issueDate;
        set => SetProperty(ref _issueDate, value);
    }

    public DateTime? DueDate
    {
        get => _dueDate;
        set => SetProperty(ref _dueDate, value);
    }

    public string LoanSearchText
    {
        get => _loanSearchText;
        set => SetProperty(ref _loanSearchText, value);
    }

    public Loan? SelectedLoanForReturn
    {
        get => _selectedLoanForReturn;
        set => SetProperty(ref _selectedLoanForReturn, value);
    }

    public DateTime? ReturnDate
    {
        get => _returnDate;
        set => SetProperty(ref _returnDate, value);
    }

    public int SelectedCatalogBookId
    {
        get => _selectedCatalogBookId;
        set => SetProperty(ref _selectedCatalogBookId, value);
    }

    public CatalogCard CurrentCatalogCard
    {
        get => _currentCatalogCard;
        set => SetProperty(ref _currentCatalogCard, value);
    }

    private LibraryRepository Repository => _repository
                                           ?? throw new InvalidOperationException("Подключение к данным не инициализировано.");

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            RefreshAllDataInternal();
            ResetReaderForm();
            ResetBookForm();
            ResetCopyForm();
        });
    }

    private void RefreshAllDataInternal()
    {
        LoadReaderLookup();
        LoadReadersGrid();
        LoadAuthors();
        LoadBookLookup();
        LoadBooksGrid();
        LoadCopiesGrid();
        LoadAvailableCopies();
        LoadActiveLoans();
        LoadDebtorsGrid();
        LoadReports();
        LoadDashboard();
        UpdateCatalogCardAfterRefresh();
    }

    private void LoadReaderLookup()
    {
        UpdateCollection(ReaderLookup, Repository.GetReaders(string.Empty));
    }

    private void LoadReadersGrid()
    {
        UpdateCollection(Readers, Repository.GetReaders(ReaderSearchText));
    }

    private void LoadAuthors()
    {
        UpdateCollection(Authors, Repository.GetAuthors());
    }

    private void LoadBookLookup()
    {
        UpdateCollection(BookLookup, Repository.GetBooks(string.Empty, string.Empty, string.Empty, null));
    }

    private void LoadBooksGrid()
    {
        UpdateCollection(
            Books,
            Repository.GetBooks(BookTitleSearch, BookAuthorSearch, BookGenreSearch, ParseSearchYear()));
    }

    private void LoadCopiesGrid()
    {
        UpdateCollection(
            Copies,
            Repository.GetBookCopies(CopySearchText, NormalizeStatusFilter(SelectedCopyStatusFilter)));
    }

    private void LoadAvailableCopies()
    {
        UpdateCollection(AvailableCopies, Repository.GetAvailableCopies());
    }

    private void LoadActiveLoans()
    {
        UpdateCollection(ActiveLoans, Repository.GetActiveLoans(LoanSearchText));
    }

    private void LoadDebtorsGrid()
    {
        UpdateCollection(Debtors, Repository.GetDebtors(DateTime.Today));
    }

    private void LoadReports()
    {
        UpdateCollection(BooksReport, Repository.GetBooks(string.Empty, string.Empty, string.Empty, null));
        UpdateCollection(AvailableCopiesReport, Repository.GetAvailableCopies());
        UpdateCollection(IssuedBooksReport, Repository.GetActiveLoans(string.Empty));
        UpdateCollection(DebtorsReport, Repository.GetDebtors(DateTime.Today));
    }

    private void LoadDashboard()
    {
        DashboardStats = Repository.GetDashboardStats(DateTime.Today);
    }

    private void UpdateCatalogCardAfterRefresh()
    {
        if (SelectedCatalogBookId > 0 && BookLookup.Any(book => book.BookId == SelectedCatalogBookId))
        {
            CurrentCatalogCard = Repository.GetCatalogCard(SelectedCatalogBookId);
            return;
        }

        CurrentCatalogCard = new CatalogCard();
    }

    private void RefreshAfterMutation()
    {
        RefreshAllDataInternal();
        ReturnDate = DateTime.Today;
        IssueDate = DateTime.Today;
        DueDate = DateTime.Today.AddDays(14);
    }

    private void ExecuteUiAction(Action action, string? successMessage = null)
    {
        try
        {
            action();

            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                MessageBox.Show(successMessage, "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (LibraryValidationException ex)
        {
            MessageBox.Show(ex.Message, "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (SqlException ex)
        {
            MessageBox.Show(MapSqlException(ex), "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Ошибка конфигурации", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Произошла непредвиденная ошибка: {ex.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static string MapSqlException(SqlException exception)
    {
        return exception.Number switch
        {
            2 or 53 or 4060 => "Не удалось подключиться к SQL Server или базе LibraryDB. Проверьте строку подключения и наличие базы данных.",
            547 => "Операцию выполнить нельзя: запись используется в связанных таблицах.",
            2601 or 2627 => "Операция нарушает уникальность данных. Проверьте номер билета, инвентарный номер или автора.",
            _ => $"Ошибка SQL Server: {exception.Message}"
        };
    }

    private void ResetReaderForm()
    {
        SelectedReader = null;
        ReaderForm = new Reader();
    }

    private void ResetBookForm()
    {
        SelectedBook = null;
        BookForm = new Book();
        BookPublishYearInput = string.Empty;

        if (Authors.Count > 0)
        {
            BookForm.AuthorId = Authors[0].AuthorId;
        }
    }

    private void ResetCopyForm()
    {
        SelectedCopy = null;
        CopyForm = CreateEmptyCopy();

        if (BookLookup.Count > 0)
        {
            CopyForm.BookId = BookLookup[0].BookId;
        }
    }

    private static BookCopy CreateEmptyCopy()
    {
        return new BookCopy
        {
            Status = "доступна"
        };
    }

    private int? ParseSearchYear()
    {
        if (string.IsNullOrWhiteSpace(BookYearSearch))
        {
            return null;
        }

        if (int.TryParse(BookYearSearch.Trim(), out var year))
        {
            return year;
        }

        throw new LibraryValidationException("Поле поиска 'Год' должно содержать целое число.");
    }

    private int? ParseBookFormYear()
    {
        if (string.IsNullOrWhiteSpace(BookPublishYearInput))
        {
            return null;
        }

        if (int.TryParse(BookPublishYearInput.Trim(), out var year))
        {
            return year;
        }

        throw new LibraryValidationException("Год издания должен быть целым числом.");
    }

    private static string NormalizeStatusFilter(string? value)
    {
        return string.Equals(value, "Все", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : value?.Trim() ?? string.Empty;
    }

    private static void UpdateCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();

        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private bool ConfirmDelete(string entityName)
    {
        return MessageBox.Show(
                   $"Удалить выбранную запись '{entityName}'?",
                   "Подтверждение удаления",
                   MessageBoxButton.YesNo,
                   MessageBoxImage.Question)
               == MessageBoxResult.Yes;
    }

    private void OpenReadersTab_Click(object sender, RoutedEventArgs e) => MainTabControl.SelectedIndex = 1;
    private void OpenBooksTab_Click(object sender, RoutedEventArgs e) => MainTabControl.SelectedIndex = 2;
    private void OpenCopiesTab_Click(object sender, RoutedEventArgs e) => MainTabControl.SelectedIndex = 3;
    private void OpenIssueTab_Click(object sender, RoutedEventArgs e) => MainTabControl.SelectedIndex = 4;
    private void OpenReturnTab_Click(object sender, RoutedEventArgs e) => MainTabControl.SelectedIndex = 5;
    private void OpenDebtorsTab_Click(object sender, RoutedEventArgs e) => MainTabControl.SelectedIndex = 6;

    private void RefreshAllData_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(RefreshAllDataInternal);
    }

    private void SearchReaders_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(LoadReadersGrid);
    }

    private void ResetReadersSearch_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            ReaderSearchText = string.Empty;
            LoadReadersGrid();
        });
    }

    private void RefreshReaders_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(LoadReadersGrid);
    }

    private void NewReader_Click(object sender, RoutedEventArgs e)
    {
        ResetReaderForm();
    }

    private void SaveReader_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            Repository.SaveReader(ReaderForm);
            RefreshAfterMutation();
            ResetReaderForm();
        });
    }

    private void DeleteReader_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            if (ReaderForm.ReaderId == 0)
            {
                throw new LibraryValidationException("Выберите читателя для удаления.");
            }

            if (!ConfirmDelete(ReaderForm.FullName))
            {
                return;
            }

            Repository.DeleteReader(ReaderForm.ReaderId);
            RefreshAfterMutation();
            ResetReaderForm();
        });
    }

    private void SearchBooks_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(LoadBooksGrid);
    }

    private void ResetBooksSearch_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            BookTitleSearch = string.Empty;
            BookAuthorSearch = string.Empty;
            BookGenreSearch = string.Empty;
            BookYearSearch = string.Empty;
            LoadBooksGrid();
        });
    }

    private void RefreshBooks_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            LoadAuthors();
            LoadBooksGrid();
        });
    }

    private void NewBook_Click(object sender, RoutedEventArgs e)
    {
        ResetBookForm();
    }

    private void AddAuthor_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            var authorName = NewAuthorName.Trim();
            Repository.AddAuthor(authorName);
            LoadAuthors();
            var createdAuthor = Authors.FirstOrDefault(x => string.Equals(x.FullName, authorName, StringComparison.OrdinalIgnoreCase));

            if (createdAuthor is not null)
            {
                BookForm.AuthorId = createdAuthor.AuthorId;
            }

            NewAuthorName = string.Empty;
        }, "Автор добавлен в справочник.");
    }

    private void SaveBook_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            BookForm.PublishYear = ParseBookFormYear();
            Repository.SaveBook(BookForm);
            RefreshAfterMutation();
            ResetBookForm();
        });
    }

    private void DeleteBook_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            if (BookForm.BookId == 0)
            {
                throw new LibraryValidationException("Выберите издание для удаления.");
            }

            if (!ConfirmDelete(BookForm.Title))
            {
                return;
            }

            Repository.DeleteBook(BookForm.BookId);
            RefreshAfterMutation();
            ResetBookForm();
        });
    }

    private void SearchCopies_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(LoadCopiesGrid);
    }

    private void ResetCopiesSearch_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            CopySearchText = string.Empty;
            SelectedCopyStatusFilter = "Все";
            LoadCopiesGrid();
        });
    }

    private void RefreshCopies_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            LoadBookLookup();
            LoadCopiesGrid();
        });
    }

    private void NewCopy_Click(object sender, RoutedEventArgs e)
    {
        ResetCopyForm();
    }

    private void SaveCopy_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            Repository.SaveBookCopy(CopyForm);
            RefreshAfterMutation();
            ResetCopyForm();
        });
    }

    private void DeleteCopy_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            if (CopyForm.CopyId == 0)
            {
                throw new LibraryValidationException("Выберите экземпляр для удаления.");
            }

            if (!ConfirmDelete(CopyForm.InventoryNumber))
            {
                return;
            }

            Repository.DeleteBookCopy(CopyForm.CopyId);
            RefreshAfterMutation();
            ResetCopyForm();
        });
    }

    private void IssueBook_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            if (!IssueDate.HasValue)
            {
                throw new LibraryValidationException("Укажите дату выдачи.");
            }

            if (!DueDate.HasValue)
            {
                throw new LibraryValidationException("Укажите срок возврата.");
            }

            Repository.IssueBook(IssueReaderId, IssueCopyId, IssueDate.Value, DueDate.Value);
            RefreshAfterMutation();
            IssueReaderId = 0;
            IssueCopyId = 0;
        }, "Выдача успешно оформлена.");
    }

    private void RefreshLoansAndAvailability_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            LoadAvailableCopies();
            LoadActiveLoans();
            LoadCopiesGrid();
            LoadDebtorsGrid();
            LoadReports();
            LoadDashboard();
        });
    }

    private void SearchLoans_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(LoadActiveLoans);
    }

    private void ResetLoansSearch_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            LoanSearchText = string.Empty;
            LoadActiveLoans();
        });
    }

    private void ReturnBook_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            if (SelectedLoanForReturn is null)
            {
                throw new LibraryValidationException("Выберите запись о выдаче для возврата.");
            }

            if (!ReturnDate.HasValue)
            {
                throw new LibraryValidationException("Укажите дату возврата.");
            }

            Repository.ReturnBook(SelectedLoanForReturn.LoanId, ReturnDate.Value);
            RefreshAfterMutation();
            SelectedLoanForReturn = null;
        }, "Возврат успешно оформлен.");
    }

    private void RefreshDebtors_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            LoadDebtorsGrid();
            LoadReports();
            LoadDashboard();
        });
    }

    private void RefreshReports_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(LoadReports);
    }

    private void ShowCatalogCard_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            if (SelectedCatalogBookId <= 0)
            {
                throw new LibraryValidationException("Выберите книгу для формирования каталожной карточки.");
            }

            CurrentCatalogCard = Repository.GetCatalogCard(SelectedCatalogBookId);
        });
    }
}
