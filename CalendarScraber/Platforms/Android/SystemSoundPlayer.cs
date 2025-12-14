using Android.Media;
using CalendarScraber.Services;
using Application = Android.App.Application;

namespace CalendarScraber;

public class SystemSoundPlayer : ISystemSoundPlayer
{
    private MediaPlayer? _mediaPlayer;

    public void Play()
    {
        try
        {
            AppLogger.Log("🔊 SystemSoundPlayer: Запрос на воспроизведение...");
            
            // 1. Получаем URI стандартного звука будильника
            var alertUri = RingtoneManager.GetDefaultUri(RingtoneType.Alarm);

            // Если звука будильника нет (такое бывает), берем звук уведомления
            if (alertUri == null)
            {
                AppLogger.Log("⚠️ Звук будильника не найден, пробуем Notification...");
                alertUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
            }
            
            // Если и его нет (телефон в тотальном беззвучном), берем рингтон звонка
            if (alertUri == null)
            {
                AppLogger.Log("⚠️ Звук уведомления не найден, пробуем Ringtone...");
                alertUri = RingtoneManager.GetDefaultUri(RingtoneType.Ringtone)!;
            }
            
            AppLogger.Log($"🎵 Используемый URI звука: {alertUri}");

            // 2. Настраиваем плеер
            _mediaPlayer = new();
            _mediaPlayer.SetDataSource(Application.Context, alertUri);

            // ВАЖНО: Указываем, что это БУДИЛЬНИК. 
            // Это заставит звук играть через канал Alarm (игнорируя беззвучный режим)
            var audioAttributes = new AudioAttributes.Builder()
                .SetUsage(AudioUsageKind.Alarm) 
                ?.SetContentType(AudioContentType.Sonification)
                ?.Build();

            _mediaPlayer.SetAudioAttributes(audioAttributes);
            
            // Зацикливаем звук
            _mediaPlayer.Looping = true;
            
            _mediaPlayer.Prepare();
            _mediaPlayer.Start();
            
            AppLogger.Log("▶️ SystemSoundPlayer: Воспроизведение началось");
        }
        catch (Exception ex)
        {
            AppLogger.Log($"❌ SystemSoundPlayer Ошибка: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Ошибка проигрывания системного звука: {ex.Message}");
        }
    }

    public void Stop()
    {
        try
        {
            if (_mediaPlayer != null)
            {
                if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Stop();
                    AppLogger.Log("⏹️ SystemSoundPlayer: Звук остановлен");
                }
                _mediaPlayer.Release();
                _mediaPlayer = null;
            }
            else
            {
                AppLogger.Log("SystemSoundPlayer: Нечего останавливать (плеер null)");
            }
        }
        catch (Exception ex)
        {
            AppLogger.Log($"❌ SystemSoundPlayer Ошибка остановки: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Ошибка остановки звука: {ex.Message}");
        }
    }
}