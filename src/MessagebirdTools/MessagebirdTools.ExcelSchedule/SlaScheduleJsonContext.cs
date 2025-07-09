using System.Text.Json;
using System.Text.Json.Serialization;

namespace MessagebirdTools.ExcelSchedule;

// Add a partial JsonSerializerContext class for source generation
[JsonSourceGenerationOptions(JsonSerializerDefaults.General, WriteIndented = true)]
[JsonSerializable(typeof(SlaSchedule))]
[JsonSerializable(typeof(Consignee))]
[JsonSerializable(typeof(Schedule))]
internal partial class SlaScheduleJsonContext : JsonSerializerContext;

// Add a partial JsonSerializerContext class for source generation
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, WriteIndented = true)]
[JsonSerializable(typeof(MbDbRow))]
internal partial class MbDbRowJsonContext : JsonSerializerContext;
