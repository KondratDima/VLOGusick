using Android.Hardware;
using Android.Media;
using VLOGusick.Models;
using VLOGusick.Services;

namespace VLOGusick.Platforms.Android;

// Реалізація запису 2-секундних відеокліпів через нативний Android API.
// Camera (legacy API) відкриває апаратну камеру, MediaRecorder записує
// відео+аудіо потік у файл .mp4. Це найстабільніший зв'язок для MAUI,
// рекомендований для фіксованих коротких записів без preview-екрану.
public class AndroidVideoRecorder : IVideoRecordService
{
    private global::Android.Hardware.Camera? _camera;
    private MediaRecorder? _recorder;

    public async Task<ClipEntry> RecordAsync()
    {
        // 1. Запит дозволів — без них Prepare() кине виняток
        var cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
        var micStatus = await Permissions.RequestAsync<Permissions.Microphone>();

        if (cameraStatus != PermissionStatus.Granted || micStatus != PermissionStatus.Granted)
            throw new UnauthorizedAccessException("Немає дозволу на камеру або мікрофон");

        // 2. Фіксуємо час початку запису у форматі hh:mm — саме цей рядок
        //    потім накладається білим текстом на кадри (Етап 6)
        var recordedAt = DateTime.Now.ToString("HH:mm");

        // 3. Готуємо шлях для файлу кліпу у приватній папці застосунку
        var fileName = $"clip_{DateTime.Now:yyyyMMdd_HHmmssfff}.mp4";
        var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        // 4. Відкриваємо задню камеру (можна змінити на фронтальну за потреби)
        _camera = global::Android.Hardware.Camera.Open();
        _camera.Unlock(); // звільняємо камеру, щоб MediaRecorder міг нею керувати

        _recorder = new MediaRecorder();
        _recorder.SetCamera(_camera);
        _recorder.SetAudioSource(AudioSource.Mic);
        _recorder.SetVideoSource(VideoSource.Camera);
        _recorder.SetOutputFormat(OutputFormat.Mpeg4);
        _recorder.SetVideoEncoder(VideoEncoder.H264);
        _recorder.SetAudioEncoder(AudioEncoder.Aac);
        _recorder.SetVideoSize(720, 1280);     // вертикальне розширення
        _recorder.SetVideoFrameRate(30);
        _recorder.SetOrientationHint(90);      // КЛЮЧОВИЙ рядок — вертикальний формат
        _recorder.SetOutputFile(filePath);

        try
        {
            _recorder.Prepare();
            _recorder.Start();

            // 5. Чекаємо рівно 2 секунди — фіксована тривалість кліпу
            await Task.Delay(2000);
        }
        finally
        {
            // 6. Завжди зупиняємо і звільняємо ресурси, навіть якщо стався збій
            StopAndRelease();
        }

        // 7. Повертаємо готовий запис для збереження в БД (Етап 2)
        return new ClipEntry
        {
            FilePath = filePath,
            RecordedAt = recordedAt,
            CreatedAt = DateTime.Now
        };
    }

    private void StopAndRelease()
    {
        try
        {
            _recorder?.Stop();
        }
        catch
        {
            // Stop() може кинути виняток якщо запис тривав замало —
            // ігноруємо, бо файл у нашому випадку завжди >= 2с
        }
        finally
        {
            _recorder?.Reset();
            _recorder?.Release();
            _recorder = null;

            _camera?.Lock();
            _camera?.Release();
            _camera = null;
        }
    }
}