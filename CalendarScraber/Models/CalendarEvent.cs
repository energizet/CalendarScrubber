namespace CalendarScraber.Models;

public class CalendarResponse
{
	public List<CalendarView> Views { get; set; } = new(); // Инициализация
}

public class CalendarView
{
	// === Данные из JSON ===
	public ItemIdWrapper ItemId { get; set; } = new(); // Инициализация
    
	public DateTime Start { get; set; }
	public DateTime End { get; set; }
    
	// Инициализация пустой строкой, чтобы убрать Warning CS8618
	public string Subject { get; set; } = string.Empty;
	public string Preview { get; set; } = string.Empty;
	public string Location { get; set; } = string.Empty;
    
	public bool IsCancelled { get; set; }
	public string Status { get; set; } = string.Empty;
	public bool IsRecurring { get; set; }


	// === ВЫЧИСЛЯЕМЫЕ СВОЙСТВА ДЛЯ UI ===

	// 1. Локальное время (для отображения)
	public DateTime LocalStart => Start.ToLocalTime();
	public DateTime LocalEnd => End.ToLocalTime();

	// 2. Форматированный интервал времени (например: "09:00 - 10:15")
	public string TimeRange => $"{LocalStart:HH:mm} - {LocalEnd:HH:mm}";

	// 3. Заголовок (если пустой, пишем заглушку)
	public string DisplaySubject => string.IsNullOrWhiteSpace(Subject) ? "(Без названия)" : Subject;

	// 4. Логика цвета: Серый, если отменено или нет ответа
	public Color UiColor
	{
		get
		{
			// Если событие отменено
			if (IsCancelled) return Colors.Gray;

			// Если статус "Не подтверждено" или "Нет ответа"
			if (Status == "NoResponseReceived" || Status == "Tentative") return Colors.Gray;

			// Иначе - обычный цвет (черный или твой акцентный)
			return Colors.Black; 
		}
	}
    
	// 5. Цвет времени (можно сделать чуть ярче, если событие активно)
	public Color TimeColor => UiColor == Colors.Gray ? Colors.Gray : Color.FromArgb("#512BD4"); // Фиолетовый или серый
}

public class ItemIdWrapper
{
	public string Id { get; set; } = string.Empty;
	public string ChangeKey { get; set; } = string.Empty;
}