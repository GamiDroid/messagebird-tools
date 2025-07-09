namespace MessagebirdTools.ExcelSchedule;
internal class ImportValidator(ICollection<Consignee> consignees, ICollection<Schedule> schedules)
{
    private readonly ICollection<Consignee> _consignees = consignees;
    private readonly ICollection<Schedule> _schedules = schedules;

    public string[] ValidateConsignees()
    {
        var errors = new List<string>();

        // check unique keys
        var keys = new HashSet<string>();
        foreach (var consignee in _consignees)
        {
            if (!keys.Add(consignee.Key))
            {
                errors.Add($"Consignee '{consignee.Name}' has a duplicate Key: '{consignee.Key}'.");
            }
        }

        foreach (var consignee in _consignees)
        {
            if (string.IsNullOrWhiteSpace(consignee.Key))
            {
                errors.Add($"Consignee '{consignee.Name}' has an empty Key.");
            }
            if (string.IsNullOrWhiteSpace(consignee.Email) || !consignee.Email.Contains("@"))
            {
                errors.Add($"Consignee '{consignee.Name}' has an invalid Email.");
            }
            if (string.IsNullOrWhiteSpace(consignee.Phone))
            {
                errors.Add($"Consignee '{consignee.Name}' has an empty Phone.");
            }
        }

        return [.. errors];
    }

    public string[] ValidateSchedules()
    {
        var errors = new List<string>();

        foreach (var schedule in _schedules)
        {
            if (string.IsNullOrWhiteSpace(schedule.Consignee))
            {
                errors.Add($"Schedule 'l{schedule.LineNumber}' has an empty Consignee.");
            }
            if (schedule.From == DateTime.MinValue)
            {
                errors.Add($"Schedule 'l{schedule.LineNumber}' has an empty From.");
            }
            if (schedule.To == DateTime.MinValue)
            {
                errors.Add($"Schedule 'l{schedule.LineNumber}' has an empty To.");
            }
            if (schedule.Consignee == null || !_consignees.Any(c => c.Key == schedule.Consignee))
            {
                errors.Add($"Schedule 'l{schedule.LineNumber}' references an invalid Consignee Key: '{schedule.Consignee}'.");
            }
        }

        return [.. errors];
    }
}
