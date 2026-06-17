# MAUI Vlog App — Детальний план розробки

# Основний концепт роботи додтку 

## Гловна задумка
Створення додатку на c# MAUI для легкого стоврення влогу дня у форматі відео 

## Формат 
Додавання 2 секундного відривку відео під час дня , після цього в кінці дня формувалося відео з усіх цих відрізків у одне відео 

### Деталі 
Зробити вертикальний формат для відео , під кожним отривком додавати у верху єкрана окремого відривку влога час в який він був записан білим кольором

## Інтерфейс
### Основні контрастні колори :
1. **#FFA1F9**
2. **#0075FF**
### Основний інтрефейс
В основному інтерфейс білого світлого кольору з конрастними кольорами зазначеними у основних кольорах
З низу по центру основа кнопка зі знаком "+" з градієнтом основних кольорів , який додає к списку двохсекундний відривок
По центру список вже зроблених відрізків за сьогодні , які можно також видалити та продивитись натиснувши на них 
Зверху зліва кнопка VLOG контрастного кольору, яка після того як ти зробив достатньо відрізків зьеднує їх в одне відео 

## ЛОГІКА 
* Робляться відрізки відео по 2 секнди і фіксується час коли їх розпочато записувати в форматі hh:mm 
* При видаленні також видаляеться час з бази данних про цей відривок
* При виведені відрізків на єкран використовувати CollectionView , також використовувати CollectionView.EmptyView при їх відсутності
* Для логіки бази данних , створення відео розробити в окремому файлі 
* Для сторінок створити бізнес логіку viewmodels
* Огранічувати можливість зйомки відрізків на 2 секунди 
* При натисканні на кнопку VLOG , з'єднувати всі відрізки в одне відео та зберігати його в галерею телефону
* .NET 9

> **.NET 9 · Android · Нативні Android API**

---

## Кольорова схема

| Роль | Колір | HEX |
|---|---|---|
| Акцент 1 (градієнт, кнопка «+») | Рожевий | `#FFA1F9` |
| Акцент 2 (кнопка VLOG, акценти) | Синій | `#0075FF` |
| Фон сторінок | Білий | `#FFFFFF` |
| Основний текст | Темний | `#1A1A1A` |

Градієнт кнопки «+»: лінійний зліва направо від `#FFA1F9` до `#0075FF`.

---

## Структура папок проєкту

```
VlogApp/
├── Models/
│   └── ClipEntry.cs
├── Data/
│   └── AppDatabase.cs
├── Services/
│   ├── IVideoRecordService.cs
│   ├── IVideoMergeService.cs
│   ├── IGalleryService.cs
│   └── DatabaseService.cs
├── ViewModels/
│   ├── MainViewModel.cs
│   ├── ClipPreviewViewModel.cs
│   └── ExportViewModel.cs
├── Views/
│   ├── MainPage.xaml
│   ├── MainPage.xaml.cs
│   ├── ClipPreviewPage.xaml
│   └── ClipPreviewPage.xaml.cs
├── Platforms/
│   └── Android/
│       ├── AndroidManifest.xml
│       ├── AndroidVideoRecorder.cs
│       ├── AndroidVideoMerger.cs
│       └── AndroidGalleryService.cs
├── Resources/
│   └── Styles/
│       └── Colors.xaml
├── AppShell.xaml
└── MauiProgram.cs
```

---

## NuGet-пакети

| Пакет | Призначення |
|---|---|
| `CommunityToolkit.Mvvm` | MVVM: `ObservableObject`, `[RelayCommand]`, `[ObservableProperty]` |
| `CommunityToolkit.Maui` | Toast-сповіщення, Popup |
| `CommunityToolkit.Maui.MediaElement` | Відтворення відео через ExoPlayer на Android |
| `sqlite-net-pcl` | Локальна SQLite-база даних |
| `SQLitePCLRaw.bundle_green` | Runtime-залежність для sqlite-net-pcl на Android |

> Всі відео-операції (запис, злиття, накладення тексту, збереження в галерею) реалізовані через нативний Android API (`Camera2`, `MediaRecorder`, `MediaCodec`, `MediaExtractor`, `MediaMuxer`, `MediaStore`) без жодних сторонніх NuGet-пакетів.

---

## Етап 1 — Налаштування проєкту і структура

### 1.1 Створення MAUI проєкту (.NET 9)

- Нова MAUI Solution, цільова платформа — Android.
- Видалити iOS/Windows з `.csproj` якщо не потрібні.
- Налаштувати мінімальну версію Android API 21 (Android 5.0) у `AndroidManifest.xml`.

### 1.2 AndroidManifest.xml — дозволи

Файл: `Platforms/Android/AndroidManifest.xml`

Обов'язкові дозволи — без них камера і галерея не відкриються:

```xml
<uses-permission android:name="android.permission.CAMERA"/>
<uses-permission android:name="android.permission.RECORD_AUDIO"/>
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE"
    android:maxSdkVersion="28"/>
<uses-permission android:name="android.permission.READ_MEDIA_VIDEO"/>
```

### 1.3 Кольори і стилі

Файл: `Resources/Styles/Colors.xaml`

```xml
<Color x:Key="Primary">#FFA1F9</Color>
<Color x:Key="Secondary">#0075FF</Color>
<Color x:Key="PageBackground">#FFFFFF</Color>
<Color x:Key="TextPrimary">#1A1A1A</Color>
```

Градієнт для кнопки «+» (задається у XAML):

```xml
<Button.Background>
    <LinearGradientBrush StartPoint="0,0" EndPoint="1,0">
        <GradientStop Color="#FFA1F9" Offset="0"/>
        <GradientStop Color="#0075FF" Offset="1"/>
    </LinearGradientBrush>
</Button.Background>
```

### 1.4 Структура папок

Створити порожні папки: `Models/`, `Data/`, `Services/`, `ViewModels/`, `Views/`, `Platforms/Android/`. Це дозволить одразу дотримуватись архітектури і не переносити файли пізніше.

---

## Етап 2 — Data Layer — модель і база даних

### 2.1 ClipEntry.cs — модель даних

Файл: `Models/ClipEntry.cs`

```csharp
[Table("Clips")]
public class ClipEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Повний шлях до .mp4 файлу на диску
    public string FilePath { get; set; }

    // Час початку запису у форматі "hh:mm" — відображається на відео
    public string RecordedAt { get; set; }

    // Для сортування кліпів у правильному порядку
    public DateTime CreatedAt { get; set; }
}
```

### 2.2 AppDatabase.cs — робота з SQLite

Файл: `Data/AppDatabase.cs`

Клас-синглтон. БД лежить у `FileSystem.AppDataDirectory` — приватна папка додатку, доступна без додаткових дозволів.

Методи:
- `Init()` — асинхронно створює таблицю `Clips` якщо не існує.
- `GetAllAsync()` — всі кліпи відсортовані за `CreatedAt`.
- `SaveAsync(ClipEntry)` — вставка нового запису.
- `DeleteAsync(ClipEntry)` — видалення за `Id`.

```csharp
public class AppDatabase
{
    private SQLiteAsyncConnection _db;

    public async Task Init()
    {
        if (_db is not null) return;
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "vlog.db3");
        _db = new SQLiteAsyncConnection(dbPath);
        await _db.CreateTableAsync<ClipEntry>();
    }

    public async Task<List<ClipEntry>> GetAllAsync()
    {
        await Init();
        return await _db.Table<ClipEntry>()
                        .OrderBy(c => c.CreatedAt)
                        .ToListAsync();
    }

    public async Task SaveAsync(ClipEntry clip)
    {
        await Init();
        await _db.InsertAsync(clip);
    }

    public async Task DeleteAsync(ClipEntry clip)
    {
        await Init();
        await _db.DeleteAsync(clip);
    }
}
```

### 2.3 DatabaseService.cs — сервіс-обгортка

Файл: `Services/DatabaseService.cs`

Інжектований сервіс для ViewModels. Делегує виклики до `AppDatabase`. Ізолює ViewModels від деталей SQLite — якщо треба змінити БД, міняємо тільки цей файл.

> **Перевірка перед переходом далі:** зберегти тестовий кліп, отримати, видалити — переконатись що CRUD працює.

---

## Етап 3 — Запис відео — Camera2 API (Android)

### 3.1 IVideoRecordService.cs — інтерфейс

Файл: `Services/IVideoRecordService.cs`

```csharp
public interface IVideoRecordService
{
    Task<ClipEntry> RecordAsync();
}
```

ViewModels знають тільки інтерфейс — не Android-код. Це дозволяє у майбутньому додати iOS без змін у ViewModel.

### 3.2 AndroidVideoRecorder.cs — реалізація

Файл: `Platforms/Android/AndroidVideoRecorder.cs`

Використовує нативний `MediaRecorder` (Camera2 API). Кроки виконання:

1. Запитати дозволи `CAMERA` і `RECORD_AUDIO` через `Permissions.RequestAsync()` — якщо відмова, кинути виняток з поясненням.
2. Зафіксувати час початку `DateTime.Now` у форматі `hh:mm`.
3. Ініціалізувати `MediaRecorder`:
   - Відеоджерело: `Camera`
   - Аудіоджерело: `Mic`
   - Формат виводу: `Mpeg4`
   - Відеокодек: `H264`
   - Аудіокодек: `Aac`
   - Розширення файлу: `.mp4`
   - `SetOrientationHint(90)` — **обов'язково** для вертикального (портретного) формату відео
4. Вказати шлях збереження: `FileSystem.AppDataDirectory/clip_{timestamp}.mp4`
5. Викликати `Prepare()` і `Start()`.
6. Через рівно **2000 мс** (`await Task.Delay(2000)`) викликати `Stop()` і `Release()`.
7. Повернути `ClipEntry` з шляхом до файлу і зафіксованим часом.

### 3.3 Обмеження 2 секунди

Використати `await Task.Delay(2000)` між `Start()` і `Stop()`. Під час запису:
- Кнопка «+» відключається (`IsEnabled = false` через `IsRecording = true` у ViewModel).
- Показати індикатор запису (наприклад, пульсуюче червоне коло або напис «Запис...»).
- Після закінчення — автоматично розблокувати UI.

---

## Етап 4 — MVVM — MainViewModel і MainPage

### 4.1 MainViewModel.cs

Файл: `ViewModels/MainViewModel.cs`

Успадковує `ObservableObject` з `CommunityToolkit.Mvvm`.

```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly IVideoRecordService _videoRecordService;
    private readonly DatabaseService _databaseService;
    private readonly IVideoMergeService _videoMergeService;

    [ObservableProperty]
    private ObservableCollection<ClipEntry> clips = new();

    [ObservableProperty]
    private bool isRecording;

    public MainViewModel(
        IVideoRecordService videoRecordService,
        DatabaseService databaseService,
        IVideoMergeService videoMergeService)
    {
        _videoRecordService = videoRecordService;
        _databaseService = databaseService;
        _videoMergeService = videoMergeService;
    }

    // Завантажити кліпи з БД при старті
    public async Task LoadClipsAsync()
    {
        var list = await _databaseService.GetAllAsync();
        Clips = new ObservableCollection<ClipEntry>(list);
    }

    // Записати 2-секундний кліп, зберегти в БД і додати до списку
    [RelayCommand]
    private async Task RecordClipAsync()
    {
        IsRecording = true;
        try
        {
            var clip = await _videoRecordService.RecordAsync();
            await _databaseService.SaveAsync(clip);
            Clips.Insert(0, clip);
        }
        finally
        {
            IsRecording = false;
        }
    }

    // Видалити кліп з диска і з БД
    [RelayCommand]
    private async Task DeleteClipAsync(ClipEntry clip)
    {
        if (File.Exists(clip.FilePath))
            File.Delete(clip.FilePath);
        await _databaseService.DeleteAsync(clip);
        Clips.Remove(clip);
    }

    // Зшити всі кліпи в один влог
    [RelayCommand]
    private async Task ExportVlogAsync()
    {
        if (Clips.Count == 0) return;
        await _videoMergeService.MergeAsync(Clips.ToList());
    }
}
```

### 4.2 MainPage.xaml — інтерфейс

Файл: `Views/MainPage.xaml`

Структура сторінки (зверху вниз):

```xml
<ContentPage Background="White">
  <Grid RowDefinitions="Auto,*,Auto" Padding="16">

    <!-- Верхня панель: кнопка VLOG зліва -->
    <Button Grid.Row="0"
            Text="VLOG"
            TextColor="#0075FF"
            BackgroundColor="Transparent"
            BorderColor="#0075FF"
            BorderWidth="1"
            HorizontalOptions="Start"
            Command="{Binding ExportVlogCommand}"/>

    <!-- Центральна зона: список кліпів -->
    <CollectionView Grid.Row="1"
                    ItemsSource="{Binding Clips}"
                    Margin="0,16">

      <!-- Показується коли список порожній -->
      <CollectionView.EmptyView>
        <VerticalStackLayout HorizontalOptions="Center"
                             VerticalOptions="Center">
          <Label Text="Ще немає кліпів."
                 HorizontalOptions="Center"
                 TextColor="Gray"/>
          <Label Text="Натисни + щоб почати!"
                 HorizontalOptions="Center"
                 TextColor="Gray"/>
        </VerticalStackLayout>
      </CollectionView.EmptyView>

      <!-- Шаблон одного кліпу -->
      <CollectionView.ItemTemplate>
        <DataTemplate x:DataType="models:ClipEntry">
          <Grid ColumnDefinitions="*,Auto" Padding="12"
                BackgroundColor="#F9F9F9" Margin="0,4">

            <!-- Час запису — натиск відкриває перегляд -->
            <Label Grid.Column="0"
                   Text="{Binding RecordedAt}"
                   FontSize="16" FontAttributes="Bold"
                   VerticalOptions="Center">
              <Label.GestureRecognizers>
                <TapGestureRecognizer Command="{Binding Source={RelativeSource AncestorType={x:Type viewmodels:MainViewModel}}, Path=OpenPreviewCommand}"
                                      CommandParameter="{Binding .}"/>
              </Label.GestureRecognizers>
            </Label>

            <!-- Кнопка видалення -->
            <Button Grid.Column="1"
                    Text="✕"
                    TextColor="Red"
                    BackgroundColor="Transparent"
                    Command="{Binding Source={RelativeSource AncestorType={x:Type viewmodels:MainViewModel}}, Path=DeleteClipCommand}"
                    CommandParameter="{Binding .}"/>
          </Grid>
        </DataTemplate>
      </CollectionView.ItemTemplate>
    </CollectionView>

    <!-- Кнопка «+» знизу по центру з градієнтом -->
    <Button Grid.Row="2"
            Text="+"
            TextColor="White"
            FontSize="32"
            WidthRequest="72"
            HeightRequest="72"
            CornerRadius="36"
            HorizontalOptions="Center"
            IsEnabled="{Binding IsRecording, Converter={StaticResource InvertBool}}"
            Command="{Binding RecordClipCommand}">
      <Button.Background>
        <LinearGradientBrush StartPoint="0,0" EndPoint="1,0">
          <GradientStop Color="#FFA1F9" Offset="0"/>
          <GradientStop Color="#0075FF" Offset="1"/>
        </LinearGradientBrush>
      </Button.Background>
    </Button>

  </Grid>
</ContentPage>
```

---

## Етап 5 — Перегляд кліпів — ClipPreviewPage

### 5.1 ClipPreviewViewModel.cs

Файл: `ViewModels/ClipPreviewViewModel.cs`

Приймає `ClipEntry` через Shell navigation query parameter. Надає `Source` (шлях до файлу) для `MediaElement` і `RecordedAt` для відображення часу над відео.

```csharp
[QueryProperty(nameof(ClipId), "clipId")]
public partial class ClipPreviewViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private string videoSource;

    [ObservableProperty]
    private string recordedAt;

    public string ClipId
    {
        set => LoadClipAsync(int.Parse(value));
    }

    private async void LoadClipAsync(int id)
    {
        var clip = await _databaseService.GetByIdAsync(id);
        VideoSource = clip.FilePath;
        RecordedAt = clip.RecordedAt;
    }
}
```

### 5.2 ClipPreviewPage.xaml

Файл: `Views/ClipPreviewPage.xaml`

```xml
<ContentPage Background="White">
  <Grid RowDefinitions="Auto,*,Auto" Padding="16">

    <!-- Час запису зверху -->
    <Label Grid.Row="0"
           Text="{Binding RecordedAt}"
           FontSize="20" FontAttributes="Bold"
           HorizontalOptions="Center"
           TextColor="#1A1A1A"/>

    <!-- Відеоплеєр — використовує ExoPlayer на Android -->
    <toolkit:MediaElement Grid.Row="1"
                          Source="{Binding VideoSource}"
                          ShouldAutoPlay="True"
                          ShouldShowPlaybackControls="True"/>

    <!-- Кнопка повернення -->
    <Button Grid.Row="2"
            Text="← Назад"
            TextColor="#0075FF"
            BackgroundColor="Transparent"
            Command="{Binding GoBackCommand}"/>
  </Grid>
</ContentPage>
```

`MediaElement` з `CommunityToolkit.Maui.MediaElement` використовує ExoPlayer під капотом на Android — найнадійніший відеоплеєр для локальних файлів.

### 5.3 Навігація

При тапі на кліп у `CollectionView`:

```csharp
await Shell.Current.GoToAsync($"clipPreview?clipId={clip.Id}");
```

`ClipPreviewPage` зареєстрована в `AppShell.xaml`:

```csharp
Routing.RegisterRoute("clipPreview", typeof(ClipPreviewPage));
```

---

## Етап 6 — Злиття відео — MediaExtractor + MediaMuxer

### 6.1 IVideoMergeService.cs — інтерфейс

Файл: `Services/IVideoMergeService.cs`

```csharp
public interface IVideoMergeService
{
    Task<string> MergeAsync(List<ClipEntry> clips, IProgress<int> progress = null);
}
```

Повертає шлях до готового файлу влогу. `IProgress<int>` — для відображення прогресу у UI.

### 6.2 AndroidVideoMerger.cs — злиття без перекодування

Файл: `Platforms/Android/AndroidVideoMerger.cs`

Використовує нативні `MediaExtractor` і `MediaMuxer` — жодних сторонніх NuGet.

Алгоритм злиття:

1. Зібрати шляхи кліпів відсортованих за `CreatedAt`.
2. Для кожного кліпу викликати `TextOverlayAsync(clip)` — перекодувати з накладеним текстом (деталі у 6.3).
3. Для кожного перекодованого кліпу:
   - `MediaExtractor.SetDataSource(filePath)`
   - Обійти всі треки `GetTrackFormat(i)` і `SelectTrack(i)`
   - `MediaMuxer.AddTrack(format)` — зареєструвати відео і аудіо трек
4. `MediaMuxer.Start()`.
5. Читати `ByteBuffer` через `MediaExtractor.ReadSampleData()` і писати у `MediaMuxer.WriteSampleData()` — поки `MediaExtractor.Advance()` повертає `true`.
6. Після всіх кліпів — `MediaMuxer.Stop()` і `Release()`.

```csharp
public async Task<string> MergeAsync(List<ClipEntry> clips, IProgress<int> progress = null)
{
    var outputPath = Path.Combine(
        FileSystem.CacheDirectory,
        $"vlog_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");

    // Крок 1: накласти текст на кожен кліп
    var processedPaths = new List<string>();
    for (int i = 0; i < clips.Count; i++)
    {
        var processed = await AddTextOverlayAsync(clips[i]);
        processedPaths.Add(processed);
        progress?.Report((i + 1) * 50 / clips.Count);
    }

    // Крок 2: зшити всі оброблені кліпи
    await Task.Run(() => MuxVideos(processedPaths, outputPath));
    progress?.Report(100);

    return outputPath;
}

private void MuxVideos(List<string> inputPaths, string outputPath)
{
    using var muxer = new MediaMuxer(outputPath, MuxerOutputType.Mpeg4);
    long timeOffsetUs = 0;

    foreach (var path in inputPaths)
    {
        var extractor = new MediaExtractor();
        extractor.SetDataSource(path);

        var trackMap = new Dictionary<int, int>();
        for (int i = 0; i < extractor.TrackCount; i++)
        {
            var format = extractor.GetTrackFormat(i);
            extractor.SelectTrack(i);
            trackMap[i] = muxer.AddTrack(format);
        }

        if (timeOffsetUs == 0) muxer.Start();

        var buffer = ByteBuffer.Allocate(1024 * 1024);
        var info = new MediaCodec.BufferInfo();

        while (true)
        {
            int size = extractor.ReadSampleData(buffer, 0);
            if (size < 0) break;

            info.Offset = 0;
            info.Size = size;
            info.PresentationTimeUs = extractor.SampleTime + timeOffsetUs;
            info.Flags = (MediaCodecBufferFlags)extractor.SampleFlags;

            muxer.WriteSampleData(trackMap[extractor.SampleTrackIndex], buffer, info);
            extractor.Advance();
        }

        timeOffsetUs += GetDurationUs(path);
        extractor.Release();
    }

    muxer.Stop();
}
```

> **Важливо:** `MediaMuxer` підтримує лише один відеотрек і один аудіотрек у вихідному файлі. Для цього проєкту цього достатньо.

### 6.3 Накладення тексту "hh:mm" на кліп

Перед злиттям кожен кліп перекодовується з накладеним білим текстом часу.

Алгоритм для одного кліпу:

1. `MediaExtractor` — відкрити кліп, знайти відеотрек.
2. `MediaCodec` (decoder) — декодувати відеотрек у сирі кадри (через `Surface` або `ImageReader`).
3. На кожен кадр (`Bitmap`):
   - Створити `Canvas` поверх `Bitmap`
   - `Paint`: `Color = White`, `TextSize = 52f`, `AntiAlias = true`, `Typeface = Bold`
   - `Canvas.DrawText(clip.RecordedAt, x: 20f, y: 70f, paint)` — білий текст зверху зліва
4. `MediaCodec` (encoder) — закодувати оброблений кадр назад у H264.
5. Аудіотрек — скопіювати без змін через `MediaExtractor` напряму у вихідний файл.
6. Повернути шлях до обробленого кліпу.

```csharp
private async Task<string> AddTextOverlayAsync(ClipEntry clip)
{
    var outputPath = Path.Combine(
        FileSystem.CacheDirectory,
        $"overlay_{clip.Id}.mp4");

    await Task.Run(() =>
    {
        // ... MediaCodec decode → Canvas.DrawText → MediaCodec encode
        // Аудіо: MediaExtractor → без змін → MediaMuxer
    });

    return outputPath;
}
```

---

## Етап 7 — Збереження в галерею — MediaStore

### 7.1 IGalleryService.cs — інтерфейс

Файл: `Services/IGalleryService.cs`

```csharp
public interface IGalleryService
{
    Task SaveToGalleryAsync(string filePath, string fileName);
}
```

### 7.2 AndroidGalleryService.cs

Файл: `Platforms/Android/AndroidGalleryService.cs`

Два варіанти залежно від версії Android:

**Android 10+ (API 29+) — через ContentResolver і MediaStore:**

```csharp
public async Task SaveToGalleryAsync(string filePath, string fileName)
{
    var values = new ContentValues();
    values.Put(MediaStore.IMediaColumns.DisplayName, fileName);
    values.Put(MediaStore.IMediaColumns.MimeType, "video/mp4");
    values.Put(MediaStore.IMediaColumns.RelativePath, "Movies/VlogApp");

    var resolver = Android.App.Application.Context.ContentResolver;
    var uri = resolver.Insert(MediaStore.Video.Media.ExternalContentUri, values);

    using var output = resolver.OpenOutputStream(uri);
    using var input = File.OpenRead(filePath);
    await input.CopyToAsync(output);
}
```

**Android 9 і нижче — через File.Copy:**

```csharp
var moviesDir = Android.OS.Environment
    .GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMovies);
var destPath = Path.Combine(moviesDir.AbsolutePath, "VlogApp", fileName);
Directory.CreateDirectory(Path.GetDirectoryName(destPath));
File.Copy(filePath, destPath, overwrite: true);
```

> **Дозвіл `READ_MEDIA_VIDEO`** для Android 13+ потрібно запитати динамічно перед збереженням через `Permissions.RequestAsync<Permissions.Media>()`.

Після успішного збереження показати Toast:

```csharp
await Toast.Make("Влог збережено в галерею!").Show();
```

---

## Етап 8 — DI, Shell і фінальний збір

### 8.1 MauiProgram.cs — реєстрація DI

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Singletons — один екземпляр на весь час роботи додатку
        builder.Services.AddSingleton<AppDatabase>();
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<IVideoRecordService, AndroidVideoRecorder>();
        builder.Services.AddSingleton<IVideoMergeService, AndroidVideoMerger>();
        builder.Services.AddSingleton<IGalleryService, AndroidGalleryService>();

        // Transients — новий екземпляр при кожній навігації
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<ClipPreviewViewModel>();
        builder.Services.AddTransient<ClipPreviewPage>();

        return builder.Build();
    }
}
```

### 8.2 AppShell.xaml — навігація

```xml
<Shell>
    <ShellContent Title="Vlog" ContentTemplate="{DataTemplate views:MainPage}"/>
</Shell>
```

У `AppShell.xaml.cs`:

```csharp
public AppShell()
{
    InitializeComponent();
    Routing.RegisterRoute("clipPreview", typeof(ClipPreviewPage));
}
```

### 8.3 Фінальний UI-полірінг і тестування

Перевірити на реальному Android-пристрої:

- [ ] Градієнт `#FFA1F9 → #0075FF` на кнопці «+»
- [ ] Колір кнопки VLOG — `#0075FF`
- [ ] `CollectionView.EmptyView` з підказкою при порожньому списку
- [ ] Кнопка «+» вимкнена під час запису (`IsRecording = true`)
- [ ] Індикатор запису («Запис 2 сек...») поки кліп пишеться
- [ ] Перегляд кліпу при тапі
- [ ] Видалення кліпу — файл і запис у БД видаляються
- [ ] Прогрес-бар під час злиття кліпів
- [ ] Білий текст `hh:mm` у верхньому куті кожного кліпу у готовому влозі
- [ ] Вертикальний (портретний) формат фінального відео
- [ ] Toast «Влог збережено в галерею!» після збереження
- [ ] Відео з'являється у галереї телефону в папці `Movies/VlogApp`

---

## Підсумок стеку

| Шар | Технологія |
|---|---|
| UI | MAUI ContentPage / XAML |
| MVVM | CommunityToolkit.Mvvm |
| База даних | sqlite-net-pcl + SQLitePCLRaw.bundle_green |
| Перегляд відео | CommunityToolkit.Maui.MediaElement (ExoPlayer) |
| Запис відео | Camera2 API / MediaRecorder — нативний Android |
| Накладення тексту | MediaCodec + Android Canvas — нативний Android |
| Злиття відео | MediaExtractor + MediaMuxer — нативний Android |
| Збереження в галерею | MediaStore / ContentResolver — нативний Android |
| Toast | CommunityToolkit.Maui |

