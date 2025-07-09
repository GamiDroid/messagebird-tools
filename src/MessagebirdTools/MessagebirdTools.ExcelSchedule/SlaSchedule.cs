namespace MessagebirdTools.ExcelSchedule;

public record SlaSchedule(ICollection<Consignee> Consignees, ICollection<Schedule> Schedules);
