using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using AndroidX.Core.App;
using CalendarScrubber.Services;
using Application = Android.App.Application;
using Uri = Android.Net.Uri;

namespace CalendarScrubber;

public class PermissionHelper : IPermissionHelper
{
    public async Task<bool> CheckStatusAsync(PermissionType type)
    {
        var context = Application.Context;

        switch (type)
        {
            case PermissionType.Notifications:
                // Для Android 13+ (API 33)
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    return NotificationManagerCompat.From(context)!.AreNotificationsEnabled();
                }
                return true; // На старых версиях обычно включено по умолчанию

            case PermissionType.ExactAlarms:
                // Для Android 12+ (API 31)
                if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                {
                    var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService)!;
                    return alarmManager.CanScheduleExactAlarms();
                }
                return true;

            case PermissionType.Overlay:
                // Для Android 6+ (API 23)
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    return Settings.CanDrawOverlays(context);
                }
                return true;

            case PermissionType.FullScreenIntent:
                // Для Android 14+ (API 34)
                if (Build.VERSION.SdkInt >= BuildVersionCodes.UpsideDownCake)
                {
                    var notifManager = (NotificationManager)context.GetSystemService(Context.NotificationService)!;
                    return notifManager.CanUseFullScreenIntent();
                }
                return true;

            case PermissionType.BatteryOptimization:
                // Проверяем, находится ли приложение в "Белом списке" (Игнорирует оптимизацию)
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    var pm = (PowerManager)context.GetSystemService(Context.PowerService)!;
                    return pm.IsIgnoringBatteryOptimizations(context.PackageName);
                }
                return true;
            case PermissionType.AutoStart:
            case PermissionType.ShowOnLockScreen:
                // К сожалению, программно проверить этот статус НЕВОЗМОЖНО.
                // Android не дает API для чтения настроек MIUI/ColorOS/EMUI.
                // Мы всегда возвращаем false, чтобы кнопка была активна,
                // или можно вести свой флаг в Preferences (нажал ли юзер кнопку).
                return false;

            default:
                return false;
        }
    }

    public async Task RequestPermissionAsync(PermissionType type)
    {
        var context = Application.Context;
        Intent? intent = null;

        try
        {
            switch (type)
            {
                case PermissionType.Notifications:
                    // Используем MAUI хелпер для стандартных пермиссий
                    await Permissions.RequestAsync<Permissions.PostNotifications>();
                    return;

                case PermissionType.ExactAlarms:
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                    {
                        intent = new(Settings.ActionRequestScheduleExactAlarm);
                        intent.SetData(Uri.Parse("package:" + context.PackageName));
                    }
                    break;

                case PermissionType.Overlay:
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                    {
                        intent = new(Settings.ActionManageOverlayPermission);
                        intent.SetData(Uri.Parse("package:" + context.PackageName));
                    }
                    break;

                case PermissionType.FullScreenIntent:
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.UpsideDownCake)
                    {
                        intent = new(Settings.ActionManageAppUseFullScreenIntent);
                        intent.SetData(Uri.Parse("package:" + context.PackageName));
                    }
                    break;
                
                case PermissionType.BatteryOptimization:
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                    {
                        // Запрос на добавление в белый список
                        intent = new(Settings.ActionRequestIgnoreBatteryOptimizations);
                        intent.SetData(Uri.Parse("package:" + context.PackageName));
                    }
                    break;
                case PermissionType.AutoStart:
                    AutoStartOpen(context); // Вызываем специальный метод
                    return; // Выходим, так как метод сам запускает Activity
                case PermissionType.ShowOnLockScreen:
                    // Открываем страницу "О приложении", так как галочка спрятана внутри
                    OpenAppSettings(); 
                    return;
            }

            if (intent != null)
            {
                intent.AddFlags(ActivityFlags.NewTask);
                context.StartActivity(intent);
            }
        }
        catch (Exception ex)
        {
            AppLogger.Log($"❌ Ошибка запроса разрешения {type}: {ex.Message}");
            // Если специфический интент упал, открываем общие настройки
            OpenAppSettings();
        }
    }

    public void OpenAppSettings()
    {
        try
        {
            var intent = new Intent(Settings.ActionApplicationDetailsSettings);
            intent.SetData(Uri.Parse("package:" + Application.Context.PackageName));
            intent.AddFlags(ActivityFlags.NewTask);
            Application.Context.StartActivity(intent);
        }
        catch (Exception ex)
        {
            AppLogger.Log($"❌ Не удалось открыть настройки: {ex.Message}");
        }
    }
    
    private void AutoStartOpen(Context context)
{
    // Список известных компонентов для настроек автозапуска
    var intents = new List<Intent>
    {
        // Xiaomi / Redmi / POCO
        new Intent().SetComponent(new("com.miui.securitycenter", "com.miui.permcenter.autostart.AutoStartManagementActivity")),
        new Intent().SetComponent(new("com.miui.securitycenter", "com.miui.powercenter.autostart.AutoStartManagementActivity")),
        
        // Huawei / Honor
        new Intent().SetComponent(new("com.huawei.systemmanager", "com.huawei.systemmanager.startupmgr.ui.StartupNormalAppListActivity")),
        new Intent().SetComponent(new("com.huawei.systemmanager", "com.huawei.systemmanager.optimize.process.ProtectActivity")),
        
        // Oppo
        new Intent().SetComponent(new("com.coloros.safecenter", "com.coloros.safecenter.permission.startup.StartupAppListActivity")),
        new Intent().SetComponent(new("com.oppo.safe", "com.oppo.safe.permission.startup.StartupAppListActivity")),
        
        // Vivo
        new Intent().SetComponent(new("com.vivo.permissionmanager", "com.vivo.permissionmanager.activity.BgStartUpManagerActivity")),
        
        // Samsung (Обычно в разделе батареи, но попробуем специфичные)
        new Intent().SetComponent(new("com.samsung.android.lool", "com.samsung.android.sm.ui.battery.BatteryActivity")),
        
        // Asus
        new Intent().SetComponent(new("com.asus.mobilemanager", "com.asus.mobilemanager.entry.FunctionActivity")),
        
        // One Plus
        new Intent().SetComponent(new("com.oneplus.security", "com.oneplus.security.chainlaunch.view.ChainLaunchAppListActivity"))
    };

    var success = false;
    foreach (var intent in intents)
    {
        try
        {
            intent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
            success = true;
            break; // Если открылось - выходим
        }
        catch 
        { 
            // Игнорируем ошибки, пробуем следующий вариант
        }
    }

    if (!success)
    {
        // Если ничего не подошло - открываем обычные настройки приложения
        OpenAppSettings();
    }
}
}