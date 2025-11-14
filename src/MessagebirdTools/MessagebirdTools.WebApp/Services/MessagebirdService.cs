using MessagebirdTools.WebApp.Models;

namespace MessagebirdTools.WebApp.Services;

public interface IMessagebirdService
{
    Task PublishAsync(AppSettings settings, List<Consignee> consignees, List<Schedule> schedules);
}

public class MessagebirdService(
    IMessagebirdClient messagebirdClient,
    ILogger<MessagebirdService> logger) : IMessagebirdService
{
    private readonly IMessagebirdClient _messagebirdClient = messagebirdClient;
    private readonly ILogger<MessagebirdService> _logger = logger;

    public async Task PublishAsync(AppSettings settings, List<Consignee> consignees, List<Schedule> schedules)
    {
        // Validate settings
        if (string.IsNullOrEmpty(settings.ApiKey))
        {
            throw new InvalidOperationException("API Key is required. Please configure it in Settings.");
        }

        if (string.IsNullOrEmpty(settings.DatabaseKey))
        {
            throw new InvalidOperationException("Database Key is required. Please configure it in Settings.");
        }

        if (string.IsNullOrEmpty(settings.DatabaseRecordKey))
        {
            throw new InvalidOperationException("Database Record Key is required. Please configure it in Settings.");
        }

        _logger.LogInformation("Publishing {ConsigneeCount} consignees and {ScheduleCount} schedules to Messagebird",
            consignees.Count, schedules.Count);

        try
        {
            await _messagebirdClient.SaveToDatabaseAsync(
                settings.ApiKey,
                settings.DatabaseKey,
                settings.DatabaseRecordKey,
                consignees,
                schedules);

            _logger.LogInformation("Successfully published data to Messagebird");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing data to Messagebird");
            throw;
        }
    }
}
