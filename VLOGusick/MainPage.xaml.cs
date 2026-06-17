namespace VLOGusick
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var db = new VLOGusick.Data.AppDatabase();

            // Зберігаємо тестовий кліп
            var testClip = new VLOGusick.Models.ClipEntry
            {
                FilePath = "/test/path.mp4",
                RecordedAt = "14:30",
                CreatedAt = DateTime.Now
            };
            await db.SaveAsync(testClip);

            // Отримуємо всі кліпи
            var allClips = await db.GetAllAsync();
            System.Diagnostics.Debug.WriteLine($"Кліпів у БД: {allClips.Count}");

            // Видаляємо
            await db.DeleteAsync(testClip);
            var afterDelete = await db.GetAllAsync();
            System.Diagnostics.Debug.WriteLine($"Кліпів після видалення: {afterDelete.Count}");
        }
        
        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}
