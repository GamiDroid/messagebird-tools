using System.Text.Json;

namespace MessagebirdTools.WebApp.Services;

public interface IMessagebirdClient
{
    Task SaveToDatabaseAsync(string apiKey, string databaseKey, string databaseRecordKey, ICollection<Consignee> consignees, ICollection<Schedule> schedules);
}

public class MessagebirdClient(HttpClient httpClient) : IMessagebirdClient
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task SaveToDatabaseAsync(string apiKey, string databaseKey, string databaseRecordKey, ICollection<Consignee> consignees, ICollection<Schedule> schedules)
    {
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Add("Authorization", "AccessKey " + apiKey);

        var jsonData = JsonSerializer.Serialize(new SlaSchedule(consignees, schedules), SlaScheduleJsonContext.Default.SlaSchedule);

        var response = await _httpClient.PostAsJsonAsync($"/databases/{databaseKey}", new { Key = databaseRecordKey, Value = jsonData });

        response.EnsureSuccessStatusCode();
    }
}
