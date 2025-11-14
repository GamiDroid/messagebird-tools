using ClosedXML.Excel;
using MessagebirdTools.WebApp.Models;
using Radzen.Blazor;

namespace MessagebirdTools.WebApp.Services;

public interface IExcelService
{
    Task InitializeAsync(string excelPath);
    List<Consignee> GetAllConsignees();
    Consignee AddConsignee(Consignee consignee);
    void UpdateConsignee(Consignee consignee);
    void DeleteConsignee(string key);
    List<Schedule> GetAllSchedules();
    void AddSchedule(Schedule schedule);
    void UpdateSchedule(Schedule schedule);
    void DeleteSchedule(int lineNumber);
    AppSettings GetSettings();
    void UpdateSettings(AppSettings settings);
    Task SaveToExcelAsync();
    bool HasUnsavedChanges { get; }
}

public class ExcelService : IExcelService, IDisposable
{
    private string _excelPath = string.Empty;
    private bool _isInitialized = false;
    private bool _hasUnsavedChanges = false;
    
    // In-memory data storage
    private readonly List<Consignee> _consignees = [];
    private readonly List<Schedule> _schedules = [];
    private AppSettings _settings = new();

    public bool HasUnsavedChanges => _hasUnsavedChanges;

    public async Task InitializeAsync(string excelPath)
    {
        if (_isInitialized)
            return;

        _excelPath = excelPath;

        if (!File.Exists(_excelPath))
        {
            // Create new Excel file with required sheets
            using var workbook = new XLWorkbook();

            // Create Consignees sheet
            var consigneesSheet = workbook.AddWorksheet("Consignees");
            consigneesSheet.Cell("A1").Value = "ID";
            consigneesSheet.Cell("B1").Value = "Name";
            consigneesSheet.Cell("C1").Value = "Email";
            consigneesSheet.Cell("D1").Value = "Phone";

            // Create Schedule sheet
            var scheduleSheet = workbook.AddWorksheet("Schedule");
            scheduleSheet.Cell("A1").Value = "Line Number";
            scheduleSheet.Cell("B1").Value = "From Date";
            scheduleSheet.Cell("C1").Value = "From Time";
            scheduleSheet.Cell("D1").Value = "To Date";
            scheduleSheet.Cell("E1").Value = "To Time";
            scheduleSheet.Cell("F1").Value = "Consignee";

            // Create Settings sheet
            var settingsSheet = workbook.AddWorksheet("Settings");
            settingsSheet.Cell("A1").Value = "API Key";
            settingsSheet.Cell("A2").Value = "Database Key";
            settingsSheet.Cell("A3").Value = "Database Record Key";

            workbook.SaveAs(_excelPath);
        }

        // Load all data into memory
        await LoadDataFromExcelAsync();

        _isInitialized = true;
        _hasUnsavedChanges = false;
    }

    private async Task LoadDataFromExcelAsync()
    {
        await Task.Run(() =>
        {
            using var workbook = new XLWorkbook(_excelPath);

            // Ensure all required worksheets exist
            if (!workbook.Worksheets.Contains("Consignees"))
                workbook.AddWorksheet("Consignees");

            if (!workbook.Worksheets.Contains("Schedule"))
                workbook.AddWorksheet("Schedule");

            if (!workbook.Worksheets.Contains("Settings"))
                workbook.AddWorksheet("Settings");

            // Load Consignees
            _consignees.Clear();
            var consigneesSheet = workbook.Worksheet("Consignees");
            foreach (var row in consigneesSheet.RowsUsed().Skip(1))
            {
                var consignee = new Consignee(
                    key: row.Cell(1).GetValue<string>(),
                    name: row.Cell(2).GetValue<string>(),
                    email: row.Cell(3).GetValue<string>(),
                    phone: row.Cell(4).GetValue<string>());

                _consignees.Add(consignee);
            }

            // Load Schedules
            _schedules.Clear();
            var scheduleSheet = workbook.Worksheet("Schedule");
            foreach (var row in scheduleSheet.RowsUsed().Skip(1))
            {
                var fromDate = row.Cell(1).GetValue<DateTime>();
                var fromTime = row.Cell(2).GetValue<TimeSpan>();

                var toDate = row.Cell(3).GetValue<DateTime>();
                var toTime = row.Cell(4).GetValue<TimeSpan>();

                var schedule = new Schedule(
                    lineNumber: row.RowNumber(),
                    from: new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, fromTime.Hours, fromTime.Minutes, fromTime.Seconds),
                    to: new DateTime(toDate.Year, toDate.Month, toDate.Day, toTime.Hours, toTime.Minutes, toTime.Seconds),
                    consignee: row.Cell(5).GetValue<string>());

                _schedules.Add(schedule);
            }

            // Load Settings
            var settingsSheet = workbook.Worksheet("Settings");
            _settings = new AppSettings
            {
                ApiKey = settingsSheet.Cell("B1").GetValue<string>(),
                DatabaseKey = settingsSheet.Cell("B2").GetValue<string>(),
                DatabaseRecordKey = settingsSheet.Cell("B3").GetValue<string>()
            };
        });
    }

    public async Task SaveToExcelAsync()
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");

        await Task.Run(() =>
        {
            using var workbook = new XLWorkbook(_excelPath);

            // Save Consignees
            var consigneesSheet = workbook.Worksheet("Consignees");
            consigneesSheet.Clear();
            consigneesSheet.Cell("A1").Value = "ID";
            consigneesSheet.Cell("B1").Value = "Name";
            consigneesSheet.Cell("C1").Value = "Email";
            consigneesSheet.Cell("D1").Value = "Phone";

            int row = 2;
            foreach (var consignee in _consignees)
            {
                consigneesSheet.Cell(row, 1).Value = consignee.Key;
                consigneesSheet.Cell(row, 2).Value = consignee.Name;
                consigneesSheet.Cell(row, 3).Value = consignee.Email;
                consigneesSheet.Cell(row, 4).Value = consignee.Phone;
                row++;
            }

            // Save Schedules
            var scheduleSheet = workbook.Worksheet("Schedule");
            scheduleSheet.Clear();
            scheduleSheet.Cell("A1").Value = "From Date";
            scheduleSheet.Cell("B1").Value = "From Time";
            scheduleSheet.Cell("C1").Value = "To Date";
            scheduleSheet.Cell("D1").Value = "To Time";
            scheduleSheet.Cell("E1").Value = "Consignee";

            row = 2;
            foreach (var schedule in _schedules)
            {
                scheduleSheet.Cell(row, 1).Value = schedule.From.Date;
                scheduleSheet.Cell(row, 2).Value = schedule.From.TimeOfDay;
                scheduleSheet.Cell(row, 3).Value = schedule.To.Date;
                scheduleSheet.Cell(row, 4).Value = schedule.To.TimeOfDay;
                scheduleSheet.Cell(row, 5).Value = schedule.Consignee;
                row++;
            }

            // Save Settings
            var settingsSheet = workbook.Worksheet("Settings");
            settingsSheet.Cell("B1").Value = _settings.ApiKey;
            settingsSheet.Cell("B2").Value = _settings.DatabaseKey;
            settingsSheet.Cell("B3").Value = _settings.DatabaseRecordKey;

            workbook.Save();
        });

        _hasUnsavedChanges = false;
    }

    public List<Consignee> GetAllConsignees()
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");

        return [.. _consignees];
    }

    public Consignee AddConsignee(Consignee consignee)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");

        _consignees.Add(consignee);
        _hasUnsavedChanges = true;
        return consignee;
    }

    public void UpdateConsignee(Consignee consignee)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");

        var index = _consignees.FindIndex(c => c.Key == consignee.Key);
        if (index >= 0)
        {
            _consignees[index] = consignee;
            _hasUnsavedChanges = true;
        }
    }

    public void DeleteConsignee(string key)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");

        var consignee = _consignees.FirstOrDefault(c => c.Key == key);
        if (consignee != null)
        {
            _consignees.Remove(consignee);
            _hasUnsavedChanges = true;
        }
    }

    public List<Schedule> GetAllSchedules()
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");

        return [.. _schedules];
    }

    public void AddSchedule(Schedule schedule)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");

        // Auto-generate line number
        var maxLineNumber = _schedules.Any() ? _schedules.Max(s => s.LineNumber) : 0;
        schedule.LineNumber = maxLineNumber + 1;
        
        _schedules.Add(schedule);
        _hasUnsavedChanges = true;
    }

    public void UpdateSchedule(Schedule schedule)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");

        var index = _schedules.FindIndex(s => s.LineNumber == schedule.LineNumber);
        if (index >= 0)
        {
            _schedules[index] = schedule;
            _hasUnsavedChanges = true;
        }
    }

    public void DeleteSchedule(int lineNumber)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");

        var schedule = _schedules.FirstOrDefault(s => s.LineNumber == lineNumber);
        if (schedule != null)
        {
            _schedules.Remove(schedule);
            _hasUnsavedChanges = true;
        }
    }

    public AppSettings GetSettings()
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");

        return _settings;
    }

    public void UpdateSettings(AppSettings settings)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");

        _settings = settings;
        _hasUnsavedChanges = true;
    }

    public void Dispose()
    {
        // No longer need to dispose workbook as we don't keep it open
    }
}