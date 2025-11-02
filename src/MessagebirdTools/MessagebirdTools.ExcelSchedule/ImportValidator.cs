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
            if (schedule.From == DateTimeOffset.MinValue)
            {
                errors.Add($"Schedule 'l{schedule.LineNumber}' has an empty From.");
            }
            if (schedule.To == DateTimeOffset.MinValue)
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

    public string[] ValidateSubsequentSchedules()
    {
        var warnings = new List<string>();

        var orderedSchedules = _schedules.OrderBy(s => s.From).ToList();

        for (var index = 0; index < orderedSchedules.Count; index++)
        {
            if (index == 0) continue;

            var schedule = orderedSchedules[index];
            var previous = orderedSchedules[index - 1];

            if (schedule.From < previous.To)
            {
                warnings.Add($"Schedule 'l{schedule.LineNumber}' starts before the previous schedule ends.");
            }

            var gap = previous.To - schedule.From;
            var maxGap = TimeSpan.FromMinutes(-1);

            if (gap < maxGap)
            {
                warnings.Add($"Schedule 'l{schedule.LineNumber}' has a gap of {gap} from the previous schedule.");
            }
        }

        return [.. warnings];
    }
}
