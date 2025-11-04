using System.Text.Json;
using System.Text.Json.Serialization;

namespace MessagebirdTools;

// Add a partial JsonSerializerContext class for source generation
[JsonSourceGenerationOptions(JsonSerializerDefaults.General, WriteIndented = true)]
[JsonSerializable(typeof(SlaSchedule))]
[JsonSerializable(typeof(Consignee))]
[JsonSerializable(typeof(Schedule))]
public partial class SlaScheduleJsonContext : JsonSerializerContext;

// Add a partial JsonSerializerContext class for source generation
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, WriteIndented = true)]
[JsonSerializable(typeof(MbDbRow))]
public partial class MbDbRowJsonContext : JsonSerializerContext;
