using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VLOGusick.Models;
using VLOGusick.Services;

namespace VLOGusick.ViewModels;

// partial — обов'язково, бо [ObservableProperty] і [RelayCommand]
// генерують код через Source Generators у partial-частині класу
public partial class MainViewModel : ObservableObject
{
    private readonly IVideoRecordService _videoRecordService;
    private readonly DatabaseService _databaseService;

    // [ObservableProperty] генерує публічну властивість Clips
    // з автоматичним викликом OnPropertyChanged при зміні
    [ObservableProperty]
    private ObservableCollection<ClipEntry> clips = new();

    // Прапорець — true поки йде запис.
    // Прив'язується до IsEnabled кнопки «+» (інвертований)
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RecordClipCommand))] // 
    private bool isRecording;

    public MainViewModel(IVideoRecordService videoRecordService, DatabaseService databaseService)
    {
        _videoRecordService = videoRecordService;
        _databaseService = databaseService;
    }

    // Завантаження кліпів з БД при відкритті сторінки.
    // Викликається з OnAppearing у MainPage.xaml.cs
    public async Task LoadClipsAsync()
    {
        var list = await _databaseService.GetAllAsync();
        Clips = new ObservableCollection<ClipEntry>(list);
    }

    // [RelayCommand(CanExecute = ...)] — кнопка «+» заблокована під час запису.
    // Метод RecordClipAsync → генерується команда RecordClipCommand
    [RelayCommand(CanExecute = nameof(CanRecord))]
    private async Task RecordClipAsync()
    {
        IsRecording = true;
        try
        {
            var clip = await _videoRecordService.RecordAsync();
            await _databaseService.SaveAsync(clip);

            // Додаємо на початок — останній знятий кліп буде зверху списку
            Clips.Insert(0, clip);
        }
        catch (UnauthorizedAccessException)
        {
            // Користувач відмовив у дозволі — показуємо повідомлення
            await Shell.Current.DisplayAlert(
                "Дозвіл відхилено",
                "Надайте доступ до камери та мікрофону в налаштуваннях телефону",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Помилка запису",
                $"Не вдалось записати кліп: {ex.Message}",
                "OK");
        }
        finally
        {
            // Завжди знімаємо прапорець запису — навіть якщо стався збій
            IsRecording = false;
        }
    }

    // Метод CanExecute — кнопка «+» активна тільки коли не йде запис
    private bool CanRecord() => !IsRecording;

    // CommandParameter — передається конкретний ClipEntry з CollectionView.
    // Метод DeleteClipAsync(clip) → генерується команда DeleteClipCommand
    [RelayCommand]
    private async Task DeleteClipAsync(ClipEntry clip)
    {
        bool confirmed = await Shell.Current.DisplayAlert(
            "Видалити кліп?",
            $"Кліп {clip.RecordedAt} буде видалено назавжди",
            "Видалити",
            "Скасувати");

        if (!confirmed) return;

        // Видаляємо файл з диска
        if (File.Exists(clip.FilePath))
            File.Delete(clip.FilePath);

        // Видаляємо запис з БД
        await _databaseService.DeleteAsync(clip);

        // Видаляємо з колекції — UI оновиться автоматично
        Clips.Remove(clip);
    }

    // Навігація на сторінку перегляду кліпу (Етап 5)
    // Передаємо Id кліпу як query parameter
    [RelayCommand]
    private async Task OpenPreviewAsync(ClipEntry clip)
    {
        await Shell.Current.GoToAsync($"clipPreview?clipId={clip.Id}");
    }
}
