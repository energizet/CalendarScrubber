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
            // 1. Получаем URI стандартного звука будильника
            var alertUri = RingtoneManager.GetDefaultUri(RingtoneType.Alarm);

            // Если звука будильника нет (такое бывает), берем звук уведомления
            if (alertUri == null)
            {
                alertUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
            }
            
            // Если и его нет (телефон в тотальном беззвучном), берем рингтон звонка
            if (alertUri == null)
            {
                alertUri = RingtoneManager.GetDefaultUri(RingtoneType.Ringtone)!;
            }

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
        }
        catch (Exception ex)
        {
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
                }
                _mediaPlayer.Release();
                _mediaPlayer = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка остановки звука: {ex.Message}");
        }
    }
}