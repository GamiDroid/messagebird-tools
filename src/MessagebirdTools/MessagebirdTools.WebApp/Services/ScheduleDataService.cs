namespace MessagebirdTools.WebApp.Services;

/// <summary>
/// Implementation of IScheduleDataService that uses Messagebird as the single source of truth.
/// </summary>
public class ScheduleDataService : IScheduleDataService
{
    private readonly IMessagebirdClient _messagebirdClient;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<ScheduleDataService> _logger;

    private readonly List<Consignee> _consignees = [];
    private readonly List<Schedule> _schedules = [];
    
    private bool _isInitialized = false;
    private bool _hasUnsavedChanges = false;

    public ScheduleDataService(
        IMessagebirdClient messagebirdClient,
        ISettingsService settingsService,
        ILogger<ScheduleDataService> logger)
    {
        _messagebirdClient = messagebirdClient;
        _settingsService = settingsService;
        _logger = logger;
    }

    public bool IsInitialized => _isInitialized;
    public bool HasUnsavedChanges => _hasUnsavedChanges;
    
    public event Action? OnDataChanged;

    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        try
        {
            await FetchFromMessagebirdAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch data from Messagebird during initialization. Starting with empty data.");
        }

        _isInitialized = true;
    }

    public async Task FetchFromMessagebirdAsync()
    {
        var settings = _settingsService.GetSettings();
        
        if (string.IsNullOrEmpty(settings.ApiKey) ||
            string.IsNullOrEmpty(settings.DatabaseKey) ||
            string.IsNullOrEmpty(settings.DatabaseRecordKey))
        {
            throw new InvalidOperationException("API settings are not configured. Please configure them in Settings first.");
        }

        _logger.LogInformation("Fetching data from Messagebird database...");

        var slaSchedule = await _messagebirdClient.GetFromDatabaseAsync(
            settings.ApiKey,
            settings.DatabaseKey,
            settings.DatabaseRecordKey);

        if (slaSchedule is not null)
        {
            _consignees.Clear();
            _consignees.AddRange(slaSchedule.Consignees);

            _schedules.Clear();
            int lineNumber = 1;
            foreach (var schedule in slaSchedule.Schedule)
            {
                schedule.LineNumber = lineNumber++;
                _schedules.Add(schedule);
            }

            _logger.LogInformation("Loaded {ConsigneeCount} consignees and {ScheduleCount} schedules from Messagebird",
                _consignees.Count, _schedules.Count);
        }
        else
        {
            _logger.LogInformation("No data found in Messagebird database. Starting fresh.");
            _consignees.Clear();
            _schedules.Clear();
        }

        _hasUnsavedChanges = false;
        OnDataChanged?.Invoke();
    }

    public async Task PublishToMessagebirdAsync()
    {
        var settings = _settingsService.GetSettings();
        
        if (string.IsNullOrEmpty(settings.ApiKey) ||
            string.IsNullOrEmpty(settings.DatabaseKey) ||
            string.IsNullOrEmpty(settings.DatabaseRecordKey))
        {
            throw new InvalidOperationException("API settings are not configured. Please configure them in Settings first.");
        }

        _logger.LogInformation("Publishing {ConsigneeCount} consignees and {ScheduleCount} schedules to Messagebird",
            _consignees.Count, _schedules.Count);

        await _messagebirdClient.SaveToDatabaseAsync(
            settings.ApiKey,
            settings.DatabaseKey,
            settings.DatabaseRecordKey,
            _consignees,
            _schedules);

        _hasUnsavedChanges = false;
        _logger.LogInformation("Successfully published data to Messagebird");
        OnDataChanged?.Invoke();
    }

    // Consignee operations
    public List<Consignee> GetAllConsignees()
    {
        return [.. _consignees];
    }

    public Consignee AddConsignee(Consignee consignee)
    {
        _consignees.Add(consignee);
        _hasUnsavedChanges = true;
        OnDataChanged?.Invoke();
        return consignee;
    }

    public void UpdateConsignee(Consignee consignee)
    {
        var index = _consignees.FindIndex(c => c.Key == consignee.Key);
        if (index >= 0)
        {
            _consignees[index] = consignee;
            _hasUnsavedChanges = true;
            OnDataChanged?.Invoke();
        }
    }

    public void DeleteConsignee(string key)
    {
        var consignee = _consignees.FirstOrDefault(c => c.Key == key);
        if (consignee != null)
        {
            _consignees.Remove(consignee);
            _hasUnsavedChanges = true;
            OnDataChanged?.Invoke();
        }
    }

    // Schedule operations
    public List<Schedule> GetAllSchedules()
    {
        return [.. _schedules];
    }

    public void AddSchedule(Schedule schedule)
    {
        var maxLineNumber = _schedules.Any() ? _schedules.Max(s => s.LineNumber) : 0;
        schedule.LineNumber = maxLineNumber + 1;
        
        _schedules.Add(schedule);
        _hasUnsavedChanges = true;
        OnDataChanged?.Invoke();
    }

    public void UpdateSchedule(Schedule schedule)
    {
        var index = _schedules.FindIndex(s => s.LineNumber == schedule.LineNumber);
        if (index >= 0)
        {
            _schedules[index] = schedule;
            _hasUnsavedChanges = true;
            OnDataChanged?.Invoke();
        }
    }

    public void DeleteSchedule(int lineNumber)
    {
        var schedule = _schedules.FirstOrDefault(s => s.LineNumber == lineNumber);
        if (schedule != null)
        {
            _schedules.Remove(schedule);
            _hasUnsavedChanges = true;
            OnDataChanged?.Invoke();
        }
    }

    // Bulk operations for import
    public void ImportConsignees(ICollection<Consignee> consignees, bool replace = false)
    {
        if (replace)
        {
            _consignees.Clear();
        }

        foreach (var consignee in consignees)
        {
            var existingIndex = _consignees.FindIndex(c => c.Key == consignee.Key);
            if (existingIndex >= 0)
            {
                _consignees[existingIndex] = consignee;
            }
            else
            {
                _consignees.Add(consignee);
            }
        }

        _hasUnsavedChanges = true;
        OnDataChanged?.Invoke();
    }

    public void ImportSchedules(ICollection<Schedule> schedules, bool replace = false)
    {
        if (replace)
        {
            _schedules.Clear();
        }

        var maxLineNumber = _schedules.Any() ? _schedules.Max(s => s.LineNumber) : 0;
        
        foreach (var schedule in schedules)
        {
            schedule.LineNumber = ++maxLineNumber;
            _schedules.Add(schedule);
        }

        _hasUnsavedChanges = true;
        OnDataChanged?.Invoke();
    }
}
