namespace MessagebirdTools.ExcelSchedule;

public class Consignee
{
    public Consignee()
    {
    }

    public Consignee(string key, string name, string email, string phone)
    {
        Key = key;
        Name = name;
        Email = email;
        Phone = phone;
    }

    public string Key { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}