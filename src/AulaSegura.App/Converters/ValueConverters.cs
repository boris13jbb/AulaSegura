using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AulaSegura.App.Converters;

/// <summary>
/// Convierte string vacío a Visibility.Collapsed
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}

/// <summary>
/// Convierte booleano a string basado en parámetro
/// </summary>
public class BoolToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string param)
        {
            var parts = param.Split('|');
            if (parts.Length == 2)
            {
                return boolValue ? parts[0] : parts[1];
            }
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}

/// <summary>
/// Converts boolean to Visibility (true = Visible, false = Collapsed)
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // Optional parameter to invert logic
            bool invert = parameter is string param && param.ToLower() == "invert";
            
            if (invert)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}

/// <summary>
/// Convierte bool a pincel de primer plano. Parámetro opcional: "#4CAF50|#F44336" (valor true | valor false).
/// </summary>
public sealed class BoolToForegroundBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var trueHex = "#4CAF50";
        var falseHex = "#F44336";
        if (parameter is string p && p.Contains('|', StringComparison.Ordinal))
        {
            var parts = p.Split('|', 2);
            if (parts.Length == 2)
            {
                trueHex = parts[0].Trim();
                falseHex = parts[1].Trim();
            }
        }

        var pick = value is true ? trueHex : falseHex;
        try
        {
            return (Brush)(new BrushConverter().ConvertFromString(pick) ?? Brushes.Gray);
        }
        catch
        {
            return Brushes.Gray;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        Binding.DoNothing;
}
