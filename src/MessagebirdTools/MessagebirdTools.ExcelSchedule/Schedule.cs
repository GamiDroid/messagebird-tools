using System.Text.Json.Serialization;

namespace MessagebirdTools.ExcelSchedule;

public record Schedule(
    [property: JsonIgnore] int LineNumber,
    DateTime From,
    DateTime To,
    string Consignee
);