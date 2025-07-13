
using ClosedXML.Excel;

namespace MessagebirdTools.ExcelSchedule;
internal sealed class ExcelReader(string pathToExcel) : IDisposable
{
    private readonly XLWorkbook _workbook = new(pathToExcel);

    public bool ValidateWorksheetsExists(out string errorMessage)
    {
        if (!_workbook.Worksheets.Contains("Settings"))
        {
            errorMessage = "The Excel file does not contain a 'Settings' worksheet.";
            return false;
        }

        if (!_workbook.Worksheets.Contains("Consignees"))
        {
            errorMessage = "The Excel file does not contain a 'Consignees' worksheet.";
            return false;
        }

        if (!_workbook.Worksheets.Contains("Schedule"))
        {
            errorMessage = "The Excel file does not contain a 'Schedule' worksheet.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    public string GetApiKey()
    {
        var settingsSheet = _workbook.Worksheet("Settings");
        return settingsSheet.Cell("B1").GetValue<string>();
    }

    public string GetDatabaseKey()
    {
        var settingsSheet = _workbook.Worksheet("Settings");
        return settingsSheet.Cell("B2").GetValue<string>();
    }

    public ICollection<Consignee> GetConsignees()
    {
        var consigneesSheet = _workbook.Worksheet("Consignees");
        var consignees = new List<Consignee>();
        foreach (var row in consigneesSheet.RowsUsed().Skip(1)) // Skip header row
        {
            var consignee = new Consignee(
                Key: row.Cell(1).GetValue<string>(),
                Name: row.Cell(2).GetValue<string>(),
                Email: row.Cell(3).GetValue<string>(),
                Phone: row.Cell(4).GetValue<string>()
            );
            consignees.Add(consignee);
        }
        return consignees;
    }

    public ICollection<Schedule> GetSchedules()
    {
        var scheduleSheet = _workbook.Worksheet("Schedule");
        var schedules = new List<Schedule>();
        foreach (var row in scheduleSheet.RowsUsed().Skip(1)) // Skip header row
        {
            var fromDate = row.Cell(1).GetValue<DateTime>();
            var fromTime = row.Cell(2).GetValue<TimeSpan>();

            var toDate = row.Cell(3).GetValue<DateTime>();
            var toTime = row.Cell(4).GetValue<TimeSpan>();

            var schedule = new Schedule(
                LineNumber: row.RowNumber(),
                // Read dates and times, convert to UTC
                From: new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, fromTime.Hours, fromTime.Minutes, fromTime.Seconds, DateTimeKind.Local).ToUniversalTime(),
                To: new DateTime(toDate.Year, toDate.Month, toDate.Day, toTime.Hours, toTime.Minutes, toTime.Seconds, DateTimeKind.Local).ToUniversalTime(),
                Consignee: row.Cell(5).GetValue<string>()
            );
            schedules.Add(schedule);
        }
        return schedules;
    }

    public void Dispose()
    {
        _workbook.Dispose();
    }
}
