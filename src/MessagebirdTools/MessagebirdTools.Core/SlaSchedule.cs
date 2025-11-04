namespace MessagebirdTools;

public record SlaSchedule(ICollection<Consignee> Consignees, ICollection<Schedule> Schedule);
