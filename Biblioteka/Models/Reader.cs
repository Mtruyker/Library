namespace Biblioteka.Models;

public sealed class Reader : ObservableObject
{
    private int _readerId;
    private string _fullName = string.Empty;
    private string _phone = string.Empty;
    private string _address = string.Empty;
    private string _ticketNumber = string.Empty;

    public int ReaderId
    {
        get => _readerId;
        set => SetProperty(ref _readerId, value);
    }

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public string TicketNumber
    {
        get => _ticketNumber;
        set => SetProperty(ref _ticketNumber, value);
    }

    public Reader Clone()
    {
        return new Reader
        {
            ReaderId = ReaderId,
            FullName = FullName,
            Phone = Phone,
            Address = Address,
            TicketNumber = TicketNumber
        };
    }
}
