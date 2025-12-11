namespace CalendarScraber.Models;

public class CalendarResponse
{
	public List<CalendarView> Views { get; set; }
}

public class CalendarView
{
	public ItemId ItemId { get; set; }
	public DateTime Start { get; set; }
	public DateTime End { get; set; }
	public string Subject { get; set; }
}

public class ItemId
{
	public string Id { get; set; }
}