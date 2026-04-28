namespace Biblioteka.Models;

public sealed class DashboardStats : ObservableObject
{
    private int _readersCount;
    private int _titlesCount;
    private int _copiesCount;
    private int _availableCopiesCount;
    private int _activeLoansCount;
    private int _debtorsCount;

    public int ReadersCount
    {
        get => _readersCount;
        set => SetProperty(ref _readersCount, value);
    }

    public int TitlesCount
    {
        get => _titlesCount;
        set => SetProperty(ref _titlesCount, value);
    }

    public int CopiesCount
    {
        get => _copiesCount;
        set => SetProperty(ref _copiesCount, value);
    }

    public int AvailableCopiesCount
    {
        get => _availableCopiesCount;
        set => SetProperty(ref _availableCopiesCount, value);
    }

    public int ActiveLoansCount
    {
        get => _activeLoansCount;
        set => SetProperty(ref _activeLoansCount, value);
    }

    public int DebtorsCount
    {
        get => _debtorsCount;
        set => SetProperty(ref _debtorsCount, value);
    }
}
