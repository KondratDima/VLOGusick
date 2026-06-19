using System.Globalization;

namespace VLOGusick.Converters;

// Інвертує bool-значення: true -> false, false -> true.
// Використовується щоб приховати кнопку «+» (IsVisible) саме тоді,
// коли IsRecording = true, і навпаки — показати, коли запис не йде.
public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;

        return value;
    }

    // ConvertBack потрібен лише для двостороннього (TwoWay) біндингу.
    // Тут він не використовується, але метод обов'язковий для інтерфейсу IValueConverter
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;

        return value;
    }
}
