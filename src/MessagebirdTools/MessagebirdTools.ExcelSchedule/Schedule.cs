using System.Text.Json.Serialization;

namespace MessagebirdTools.ExcelSchedule;

public class Schedule
{
    public Schedule()
    {
    }

    public Schedule(int lineNumber, DateTimeOffset from, DateTimeOffset to, string consignee)
    {
        LineNumber = lineNumber;
        From = from;
        To = to;
        Consignee = consignee;
    }

    [JsonIgnore]
    public int LineNumber { get; set; }
    public DateTimeOffset From { get; set; }
    public DateTimeOffset To { get; set; }
    public string? Consignee { get; set; }
}