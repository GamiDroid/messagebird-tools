
using System.Net.Http.Json;
using System.Text.Json;

namespace MessagebirdTools.ExcelSchedule;
internal class App
{
    public static async Task RunAsync(string[] args)
    {
        try
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: MessagebirdTools.ExcelSchedule <path_to_excel_file>");
                return;
            }

            var excelFilePath = args[0];

            if (!File.Exists(excelFilePath))
            {
                Console.WriteLine($"The file '{excelFilePath}' does not exist.");
                return;
            }

            // Load the Excel file using ClosedXML

            using var excel = new ExcelReader(excelFilePath);

            if (!excel.ValidateWorksheetsExists(out var errorMessage))
            {
                Console.WriteLine(errorMessage);
                return;
            }

            var apiKey = excel.GetApiKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("API key is not set in the 'Settings' worksheet (Cell B1).");
                return;
            }

            var databaseKey = excel.GetDatabaseKey();
            if (string.IsNullOrEmpty(databaseKey))
            {
                Console.WriteLine("Database key is not set in the 'Settings' worksheet (Cell B2).");
                return;
            }

            var databaseRecordKey = excel.GetDatabaseRecordKey();
            if (string.IsNullOrEmpty(databaseRecordKey))
            {
                Console.WriteLine("Database record key is not set in the 'Settings' worksheet (Cell B3).");
                return;
            }

            var consignees = excel.GetConsignees();
            if (consignees.Count == 0)
            {
                Console.WriteLine("No consignees found in the 'Consignees' worksheet.");
                return;
            }

            var schedules = excel.GetSchedules();
            if (schedules.Count == 0)
            {
                Console.WriteLine("No schedules found in the 'Schedule' worksheet.");
                return;
            }

            var validator = new ImportValidator(consignees, schedules);

            var consigneeErrors = validator.ValidateConsignees();
            if (consigneeErrors.Length > 0)
            {
                Console.WriteLine("Consignee validation errors:");
                foreach (var error in consigneeErrors)
                {
                    Console.WriteLine($"- {error}");
                }
                return;
            }

            var scheduleErrors = validator.ValidateSchedules();
            if (scheduleErrors.Length > 0)
            {
                Console.WriteLine("Schedule validation errors:");
                foreach (var error in scheduleErrors)
                {
                    Console.WriteLine($"- {error}");
                }
                return;
            }

            var jsonData = JsonSerializer.Serialize(new SlaSchedule(consignees, schedules), SlaScheduleJsonContext.Default.SlaSchedule);

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://flows.messagebird.com"),
            };

            httpClient.DefaultRequestHeaders.Add("Authorization", "AccessKey " + apiKey);

            var response = await httpClient.PostAsJsonAsync($"/databases/{databaseKey}", new { Key = databaseRecordKey, Value = jsonData });

            response.EnsureSuccessStatusCode();

            Console.WriteLine($"Database record '{databaseRecordKey}' succesfully is modified.");
            Console.WriteLine($"View at: https://dashboard.messagebird.com/nl/developers/databases/{databaseKey}/edit/document/{databaseRecordKey}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
