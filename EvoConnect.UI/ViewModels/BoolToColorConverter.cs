using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace EvoConnect.UI.ViewModels
{
    public class BoolToColorConverter : IValueConverter
    {
        public static readonly BoolToColorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isRunning)
            {
                if (targetType == typeof(Color) || targetType == typeof(Color?))
                {
                    return isRunning ? Colors.Green : Colors.Gray;
                }
                else if (targetType == typeof(IBrush))
                {
                    return isRunning ? Brushes.Green : Brushes.Gray;
                }
            }

            return Colors.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}