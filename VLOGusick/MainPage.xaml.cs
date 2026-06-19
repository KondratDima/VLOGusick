using VLOGusick.ViewModels;

namespace VLOGusick;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    // ViewModel вводиться через DI — MAUI автоматично підставить
    // зареєстрований екземпляр з MauiProgram.cs
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;

        // Прив'язуємо ViewModel до сторінки — всі Binding у XAML
        // будуть шукати властивості і команди саме в цьому об'єкті
        BindingContext = viewModel;
    }

    // Завантажуємо кліпи кожного разу коли сторінка стає видимою.
    // OnAppearing — це правильне місце, а не конструктор,
    // бо при поверненні з ClipPreviewPage список теж оновиться
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadClipsAsync();
    }
}
