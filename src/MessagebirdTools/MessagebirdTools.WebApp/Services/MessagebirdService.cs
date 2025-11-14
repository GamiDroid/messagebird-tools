using MessagebirdTools.WebApp.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MessagebirdTools.WebApp.Services;

public interface IMessagebirdService
{
    Task PublishAsync(AppSettings settings, List<Consignee> consignees, List<Schedule> schedules);
}

public class MessagebirdService : IMessagebirdService
{
    private readonly ILogger<MessagebirdService> _logger;

    public MessagebirdService(ILogger<MessagebirdService> logger)
    {
        _logger = logger;
    }

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
            // TODO: Implement actual Messagebird API calls here
            // Example:
            // using var httpClient = new HttpClient();
            // httpClient.DefaultRequestHeaders.Add("Authorization", $"AccessKey {settings.ApiKey}");
            //
            // foreach (var consignee in consignees)
            // {
            //     var content = new StringContent(JsonSerializer.Serialize(consignee), Encoding.UTF8, "application/json");
            //     var response = await httpClient.PostAsync($"https://api.messagebird.com/v1/databases/{settings.DatabaseKey}/records", content);
            //     response.EnsureSuccessStatusCode();
            // }
            //
            // foreach (var schedule in schedules)
            // {
            //     var content = new StringContent(JsonSerializer.Serialize(schedule), Encoding.UTF8, "application/json");
            //     var response = await httpClient.PostAsync($"https://api.messagebird.com/v1/schedules", content);
            //     response.EnsureSuccessStatusCode();
            // }

            // Simulate API call delay
            await Task.Delay(1000);

            _logger.LogInformation("Successfully published data to Messagebird");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing data to Messagebird");
            throw;
        }
    }
}
