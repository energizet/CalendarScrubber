namespace CalendarScrubber.Extensions;

public static class StringExtensions
{
	extension(string str)
	{
		public int GetStableHashCode()
		{
			unchecked
			{
				var hash1 = 0x811c9dc5;
				const int hashingMultiplier = 0x01000193;

				foreach (var c in str)
				{
					hash1 = (hash1 ^ c) * hashingMultiplier;
				}
				return (int)hash1;
			}
		}
	}
}