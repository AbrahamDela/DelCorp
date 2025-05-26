using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace DelCorp.Converters
{
    public class NonNullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Generalmente no se necesita para bindings unidireccionales como IsVisible
            throw new NotImplementedException();
        }
    }
}