using CalendarScrubber.Services;

namespace CalendarScrubber;

public class PermissionHelper : IPermissionHelper
{
	public Task<bool> CheckStatusAsync(PermissionType type) => Task.FromResult(true);
	public Task RequestPermissionAsync(PermissionType type) => Task.CompletedTask;
	public void OpenAppSettings() { }
}