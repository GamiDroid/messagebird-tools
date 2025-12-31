namespace MessagebirdTools.WebApp.Services;

/// <summary>
/// Service for managing schedule data with Messagebird as the single source of truth.
/// </summary>
public interface IScheduleDataService
{
    /// <summary>
    /// Indicates whether the service has been initialized with data.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Indicates whether there are unsaved changes in memory.
    /// </summary>
    bool HasUnsavedChanges { get; }

    /// <summary>
    /// Event triggered when data changes.
    /// </summary>
    event Action? OnDataChanged;

    /// <summary>
    /// Initializes the service and loads data from Messagebird.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Fetches the latest data from Messagebird database.
    /// </summary>
    Task FetchFromMessagebirdAsync();

    /// <summary>
    /// Publishes current in-memory data to Messagebird database.
    /// </summary>
    Task PublishToMessagebirdAsync();

    // Consignee operations
    List<Consignee> GetAllConsignees();
    Consignee AddConsignee(Consignee consignee);
    void UpdateConsignee(Consignee consignee);
    void DeleteConsignee(string key);

    // Schedule operations
    List<Schedule> GetAllSchedules();
    void AddSchedule(Schedule schedule);
    void UpdateSchedule(Schedule schedule);
    void DeleteSchedule(int lineNumber);

    // Bulk operations for import
    void ImportConsignees(ICollection<Consignee> consignees, bool replace = false);
    void ImportSchedules(ICollection<Schedule> schedules, bool replace = false);
}
