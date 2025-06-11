using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using DelCorp.Models;

namespace DelCorp.Converters
{
    public class SaldoRestanteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Presupuesto presupuesto)
            {
                var total = presupuesto.TotalPresupuesto ?? 0m;
                var spent = presupuesto.MontoEjePresupuesto ?? 0m;
                return total - spent;
            }
            return 0m;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
