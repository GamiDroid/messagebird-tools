using System.Text.Json;

namespace MessagebirdTools.WebApp.Services;

public interface IMessagebirdClient
{
    Task SaveToDatabaseAsync(string apiKey, string databaseKey, string databaseRecordKey, ICollection<Consignee> consignees, ICollection<Schedule> schedules);
    Task<SlaSchedule?> GetFromDatabaseAsync(string apiKey, string databaseKey, string databaseRecordKey, CancellationToken cancellationToken = default);
}

// TODO: Refactor to use a http client factory that creates clients with base address and default headers
// from settings service
public class MessagebirdClient(HttpClient httpClient) : IMessagebirdClient
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task SaveToDatabaseAsync(string apiKey, string databaseKey, string databaseRecordKey, ICollection<Consignee> consignees, ICollection<Schedule> schedules)
    {
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Add("Authorization", "AccessKey " + apiKey);

        var jsonData = JsonSerializer.Serialize(new SlaSchedule(consignees, schedules), SlaScheduleJsonContext.Default.SlaSchedule);

        var response = await _httpClient.PostAsJsonAsync($"/databases/{databaseKey}", new DbRecord { Key = databaseRecordKey, Value = jsonData });

        response.EnsureSuccessStatusCode();
    }

    public async Task<SlaSchedule?> GetFromDatabaseAsync(string apiKey, string databaseKey, string databaseRecordKey, CancellationToken cancellationToken)
    {
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Add("Authorization", "AccessKey " + apiKey);

        var response = await _httpClient.GetAsync($"/databases/{databaseKey}/{databaseRecordKey}", cancellationToken);

        response.EnsureSuccessStatusCode();

        var dbRecord = await response.Content.ReadFromJsonAsync<DbRecord>(cancellationToken);
        
        if (dbRecord is null)
        {
            return null;
        }

        var slaSchedule = JsonSerializer.Deserialize(dbRecord.Value, SlaScheduleJsonContext.Default.SlaSchedule);
        return slaSchedule;
    }

    private sealed record DbRecord
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
