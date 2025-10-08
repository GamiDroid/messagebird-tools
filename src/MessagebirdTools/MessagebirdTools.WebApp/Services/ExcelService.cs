using ClosedXML.Excel;
using MessagebirdTools.ExcelSchedule;
using MessagebirdTools.WebApp.Models;
using System.Reflection;
using static MudBlazor.CategoryTypes;

namespace MessagebirdTools.WebApp.Services;

public class ExcelService : IDisposable
{
    private XLWorkbook? _workbook;
    private string _excelPath = string.Empty;
    private bool _isInitialized = false;

    public async Task InitializeAsync(string excelPath)
    {
        _excelPath = excelPath;
        
        if (!File.Exists(_excelPath))
        {
            // Create new Excel file with required sheets
            _workbook = new XLWorkbook();
            
            // Create Consignees sheet
            var consigneesSheet = _workbook.AddWorksheet("Consignees");
            consigneesSheet.Cell("A1").Value = "ID";
            consigneesSheet.Cell("B1").Value = "Name";
            consigneesSheet.Cell("C1").Value = "Email";
            consigneesSheet.Cell("D1").Value = "Phone";
            
            // Create Schedule sheet
            var scheduleSheet = _workbook.AddWorksheet("Schedule");
            scheduleSheet.Cell("A1").Value = "Line Number";
            scheduleSheet.Cell("B1").Value = "From Date";
            scheduleSheet.Cell("C1").Value = "From Time";
            scheduleSheet.Cell("D1").Value = "To Date";
            scheduleSheet.Cell("E1").Value = "To Time";
            scheduleSheet.Cell("F1").Value = "Consignee";
            
            // Create Settings sheet
            var settingsSheet = _workbook.AddWorksheet("Settings");
            settingsSheet.Cell("A1").Value = "API Key";
            settingsSheet.Cell("A2").Value = "Database Key";
            settingsSheet.Cell("A3").Value = "Database Record Key";
            
            await SaveWorkbookAsync();
        }
        else
        {
            await Task.Run(() => _workbook = new XLWorkbook(_excelPath));
            
            // Ensure all required worksheets exist
            if (!_workbook.Worksheets.Contains("Consignees"))
                _workbook.AddWorksheet("Consignees");
                
            if (!_workbook.Worksheets.Contains("Schedule"))
                _workbook.AddWorksheet("Schedule");
                
            if (!_workbook.Worksheets.Contains("Settings"))
                _workbook.AddWorksheet("Settings");
        }
        
        _isInitialized = true;
    }
    
    private async Task SaveWorkbookAsync()
    {
        if (_workbook != null)
        {
            await Task.Run(() => _workbook.SaveAs(_excelPath));
        }
    }

    public List<Consignee> GetAllConsignees()
    {
        if (!_isInitialized || _workbook == null)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");
            
        var consigneesSheet = _workbook.Worksheet("Consignees");
        var consignees = new List<Consignee>();
        
        foreach (var row in consigneesSheet.RowsUsed().Skip(1)) // Skip header row
        {
            var consignee = new Consignee(
                key: row.Cell(1).GetValue<string>(),
                name: row.Cell(2).GetValue<string>(),
                email: row.Cell(3).GetValue<string>(),
                phone: row.Cell(4).GetValue<string>());
            
            consignees.Add(consignee);
        }
        
        return consignees;
    }
    
    public async Task<Consignee> AddPersonnelAsync(Consignee consignee)
    {
        if (!_isInitialized || _workbook == null)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");
            
        var consigneesSheet = _workbook.Worksheet("Consignees");
        var nextRow = consigneesSheet.LastRowUsed().RowNumber() + 1;
        
        consigneesSheet.Cell(nextRow, 1).Value = consignee.Key;
        consigneesSheet.Cell(nextRow, 2).Value = consignee.Name;
        consigneesSheet.Cell(nextRow, 3).Value = consignee.Email;
        consigneesSheet.Cell(nextRow, 4).Value = consignee.Phone;
        
        await SaveWorkbookAsync();
        return consignee;
    }
    
    public async Task UpdateConsigneeAsync(Consignee consignee)
    {
        if (!_isInitialized || _workbook == null)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");
            
        var consigneesSheet = _workbook.Worksheet("Consignees");
        
        foreach (var row in consigneesSheet.RowsUsed().Skip(1)) // Skip header row
        {
            if (row.Cell(1).GetValue<string>() == consignee.Key)
            {
                row.Cell(2).Value = consignee.Name;
                row.Cell(3).Value = consignee.Email;
                row.Cell(4).Value = consignee.Phone;
                break;
            }
        }
        
        await SaveWorkbookAsync();
    }
    
    public async Task DeleteConsigneeAsync(string key)
    {
        if (!_isInitialized || _workbook == null)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");
            
        var consigneesSheet = _workbook.Worksheet("Consignees");
        
        foreach (var row in consigneesSheet.RowsUsed().Skip(1)) // Skip header row
        {
            if (row.Cell(1).GetValue<string>() == key)
            {
                row.Delete();
                break;
            }
        }
        
        await SaveWorkbookAsync();
    }
    
    public async Task<List<Schedule>> GetAllSchedulesAsync()
    {
        if (!_isInitialized || _workbook == null)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");
            
        var scheduleSheet = _workbook.Worksheet("Schedule");
        var schedules = new List<Schedule>();
        
        foreach (var row in scheduleSheet.RowsUsed().Skip(1)) // Skip header row
        {
            var fromDate = row.Cell(2).GetValue<DateTime>();
            var fromTime = row.Cell(3).GetValue<TimeSpan>();
            
            var toDate = row.Cell(4).GetValue<DateTime>();
            var toTime = row.Cell(5).GetValue<TimeSpan>();

            var schedule = new Schedule(
                lineNumber: row.RowNumber(),
                from: new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, fromTime.Hours, fromTime.Minutes, fromTime.Seconds),
                to: new DateTime(toDate.Year, toDate.Month, toDate.Day, toTime.Hours, toTime.Minutes, toTime.Seconds),
                consignee: row.Cell(6).GetValue<string>());

            schedules.Add(schedule);
        }
        
        return schedules;
    }
    
    public async Task AddScheduleAsync(Schedule schedule)
    {
        if (!_isInitialized || _workbook == null)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");
            
        var scheduleSheet = _workbook.Worksheet("Schedule");
        var nextRow = scheduleSheet.LastRowUsed().RowNumber() + 1;
        
        scheduleSheet.Cell(nextRow, 1).Value = nextRow - 1; // Line number
        scheduleSheet.Cell(nextRow, 2).Value = schedule.From.Date;
        scheduleSheet.Cell(nextRow, 3).Value = schedule.From.TimeOfDay;
        scheduleSheet.Cell(nextRow, 4).Value = schedule.To.Date;
        scheduleSheet.Cell(nextRow, 5).Value = schedule.To.TimeOfDay;
        scheduleSheet.Cell(nextRow, 6).Value = schedule.Consignee;

        await SaveWorkbookAsync();
    }
    
    public async Task UpdateScheduleAsync(Schedule schedule)
    {
        if (!_isInitialized || _workbook == null)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");
            
        var scheduleSheet = _workbook.Worksheet("Schedule");
        
        foreach (var row in scheduleSheet.RowsUsed().Skip(1)) // Skip header row
        {
            if (row.RowNumber() == schedule.LineNumber)
            {
                row.Cell(2).Value = schedule.From.Date;
                row.Cell(3).Value = schedule.From.TimeOfDay;
                row.Cell(4).Value = schedule.To.Date;
                row.Cell(5).Value = schedule.To.TimeOfDay;
                row.Cell(6).Value = schedule.Consignee;
                break;
            }
        }
        
        await SaveWorkbookAsync();
    }
    
    public async Task DeleteScheduleAsync(int lineNumber)
    {
        if (!_isInitialized || _workbook == null)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");
            
        var scheduleSheet = _workbook.Worksheet("Schedule");
        
        foreach (var row in scheduleSheet.RowsUsed().Skip(1)) // Skip header row
        {
            if (row.RowNumber() == lineNumber)
            {
                row.Delete();
                break;
            }
        }
        
        await SaveWorkbookAsync();
    }
    
    public AppSettings GetSettings()
    {
        if (!_isInitialized || _workbook == null)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");
            
        var settingsSheet = _workbook.Worksheet("Settings");

        var settings = new AppSettings
        {
            ApiKey = settingsSheet.Cell("B1").GetValue<string>(),
            DatabaseKey = settingsSheet.Cell("B2").GetValue<string>(),
            DatabaseRecordKey = settingsSheet.Cell("B3").GetValue<string>()
        };
        
        return settings;
    }
    
    public async Task SaveSettingsAsync(AppSettings settings)
    {
        if (!_isInitialized || _workbook == null)
            throw new InvalidOperationException("Excel service not initialized. Call InitializeAsync first.");
            
        var settingsSheet = _workbook.Worksheet("Settings");
        
        settingsSheet.Cell("B1").Value = settings.ApiKey;
        settingsSheet.Cell("B2").Value = settings.DatabaseKey;
        settingsSheet.Cell("B3").Value = settings.DatabaseRecordKey;
        
        await SaveWorkbookAsync();
    }

    public void Dispose()
    {
        _workbook?.Dispose();
    }
}