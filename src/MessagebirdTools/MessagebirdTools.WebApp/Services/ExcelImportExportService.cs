using ClosedXML.Excel;

namespace MessagebirdTools.WebApp.Services;

/// <summary>
/// Service for importing from and exporting to Excel files.
/// </summary>
public interface IExcelImportExportService
{
    /// <summary>
    /// Imports consignees and schedules from an Excel file.
    /// </summary>
    /// <param name="filePath">Path to the Excel file.</param>
    /// <returns>Tuple containing imported consignees and schedules.</returns>
    Task<(List<Consignee> Consignees, List<Schedule> Schedules)> ImportFromExcelAsync(string filePath);

    /// <summary>
    /// Imports consignees and schedules from an Excel file stream.
    /// </summary>
    /// <param name="stream">Stream containing the Excel file.</param>
    /// <returns>Tuple containing imported consignees and schedules.</returns>
    Task<(List<Consignee> Consignees, List<Schedule> Schedules)> ImportFromExcelAsync(Stream stream);

    /// <summary>
    /// Exports consignees and schedules to an Excel file.
    /// </summary>
    /// <param name="filePath">Path where to save the Excel file.</param>
    /// <param name="consignees">Consignees to export.</param>
    /// <param name="schedules">Schedules to export.</param>
    Task ExportToExcelAsync(string filePath, ICollection<Consignee> consignees, ICollection<Schedule> schedules);

    /// <summary>
    /// Exports consignees and schedules to a stream.
    /// </summary>
    /// <param name="consignees">Consignees to export.</param>
    /// <param name="schedules">Schedules to export.</param>
    /// <returns>Memory stream containing the Excel file.</returns>
    Task<MemoryStream> ExportToExcelStreamAsync(ICollection<Consignee> consignees, ICollection<Schedule> schedules);
}

/// <summary>
/// Implementation of IExcelImportExportService.
/// </summary>
public class ExcelImportExportService : IExcelImportExportService
{
    private readonly ILogger<ExcelImportExportService> _logger;

    public ExcelImportExportService(ILogger<ExcelImportExportService> logger)
    {
        _logger = logger;
    }

    public async Task<(List<Consignee> Consignees, List<Schedule> Schedules)> ImportFromExcelAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            using var workbook = new XLWorkbook(filePath);
            return ImportFromWorkbook(workbook);
        });
    }

    public async Task<(List<Consignee> Consignees, List<Schedule> Schedules)> ImportFromExcelAsync(Stream stream)
    {
        return await Task.Run(() =>
        {
            using var workbook = new XLWorkbook(stream);
            return ImportFromWorkbook(workbook);
        });
    }

    private (List<Consignee> Consignees, List<Schedule> Schedules) ImportFromWorkbook(XLWorkbook workbook)
    {
        var consignees = new List<Consignee>();
        var schedules = new List<Schedule>();

        // Import Consignees
        if (workbook.Worksheets.Contains("Consignees"))
        {
            var consigneesSheet = workbook.Worksheet("Consignees");
            foreach (var row in consigneesSheet.RowsUsed().Skip(1)) // Skip header row
            {
                var consignee = new Consignee(
                    key: row.Cell(1).GetValue<string>(),
                    name: row.Cell(2).GetValue<string>(),
                    email: row.Cell(3).GetValue<string>(),
                    phone: row.Cell(4).GetValue<string>());

                consignees.Add(consignee);
            }
            _logger.LogInformation("Imported {Count} consignees from Excel", consignees.Count);
        }
        else
        {
            _logger.LogWarning("Excel file does not contain a 'Consignees' worksheet");
        }

        // Import Schedules
        if (workbook.Worksheets.Contains("Schedule"))
        {
            var scheduleSheet = workbook.Worksheet("Schedule");
            int lineNumber = 1;
            foreach (var row in scheduleSheet.RowsUsed().Skip(1)) // Skip header row
            {
                var fromDate = row.Cell(1).GetValue<DateTime>();
                var fromTime = row.Cell(2).GetValue<TimeSpan>();

                var toDate = row.Cell(3).GetValue<DateTime>();
                var toTime = row.Cell(4).GetValue<TimeSpan>();

                var schedule = new Schedule(
                    lineNumber: lineNumber++,
                    from: new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, fromTime.Hours, fromTime.Minutes, fromTime.Seconds, DateTimeKind.Local).ToUniversalTime(),
                    to: new DateTime(toDate.Year, toDate.Month, toDate.Day, toTime.Hours, toTime.Minutes, toTime.Seconds, DateTimeKind.Local).ToUniversalTime(),
                    consignee: row.Cell(5).GetValue<string>());

                schedules.Add(schedule);
            }
            _logger.LogInformation("Imported {Count} schedules from Excel", schedules.Count);
        }
        else
        {
            _logger.LogWarning("Excel file does not contain a 'Schedule' worksheet");
        }

        return (consignees, schedules);
    }

    public async Task ExportToExcelAsync(string filePath, ICollection<Consignee> consignees, ICollection<Schedule> schedules)
    {
        await Task.Run(() =>
        {
            using var workbook = CreateWorkbook(consignees, schedules);
            workbook.SaveAs(filePath);
        });

        _logger.LogInformation("Exported {ConsigneeCount} consignees and {ScheduleCount} schedules to {FilePath}",
            consignees.Count, schedules.Count, filePath);
    }

    public async Task<MemoryStream> ExportToExcelStreamAsync(ICollection<Consignee> consignees, ICollection<Schedule> schedules)
    {
        return await Task.Run(() =>
        {
            var stream = new MemoryStream();
            using var workbook = CreateWorkbook(consignees, schedules);
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        });
    }

    private XLWorkbook CreateWorkbook(ICollection<Consignee> consignees, ICollection<Schedule> schedules)
    {
        var workbook = new XLWorkbook();

        // Create Consignees sheet
        var consigneesSheet = workbook.AddWorksheet("Consignees");
        consigneesSheet.Cell("A1").Value = "ID";
        consigneesSheet.Cell("B1").Value = "Name";
        consigneesSheet.Cell("C1").Value = "Email";
        consigneesSheet.Cell("D1").Value = "Phone";

        // Style header row
        var headerRow = consigneesSheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 2;
        foreach (var consignee in consignees)
        {
            consigneesSheet.Cell(row, 1).Value = consignee.Key;
            consigneesSheet.Cell(row, 2).Value = consignee.Name;
            consigneesSheet.Cell(row, 3).Value = consignee.Email;
            consigneesSheet.Cell(row, 4).Value = consignee.Phone;
            row++;
        }

        consigneesSheet.Columns().AdjustToContents();

        // Create Schedule sheet
        var scheduleSheet = workbook.AddWorksheet("Schedule");
        scheduleSheet.Cell("A1").Value = "From Date";
        scheduleSheet.Cell("B1").Value = "From Time";
        scheduleSheet.Cell("C1").Value = "To Date";
        scheduleSheet.Cell("D1").Value = "To Time";
        scheduleSheet.Cell("E1").Value = "Consignee";

        // Style header row
        var scheduleHeaderRow = scheduleSheet.Row(1);
        scheduleHeaderRow.Style.Font.Bold = true;
        scheduleHeaderRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        row = 2;
        foreach (var schedule in schedules)
        {
            var localFrom = schedule.From.LocalDateTime;
            var localTo = schedule.To.LocalDateTime;

            scheduleSheet.Cell(row, 1).Value = localFrom.Date;
            scheduleSheet.Cell(row, 1).Style.NumberFormat.Format = "yyyy-MM-dd";
            scheduleSheet.Cell(row, 2).Value = localFrom.TimeOfDay;
            scheduleSheet.Cell(row, 2).Style.NumberFormat.Format = "HH:mm";
            scheduleSheet.Cell(row, 3).Value = localTo.Date;
            scheduleSheet.Cell(row, 3).Style.NumberFormat.Format = "yyyy-MM-dd";
            scheduleSheet.Cell(row, 4).Value = localTo.TimeOfDay;
            scheduleSheet.Cell(row, 4).Style.NumberFormat.Format = "HH:mm";
            scheduleSheet.Cell(row, 5).Value = schedule.Consignee;
            row++;
        }

        scheduleSheet.Columns().AdjustToContents();

        return workbook;
    }
}
