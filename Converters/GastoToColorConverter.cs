using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using DelCorp.Models;

namespace DelCorp.Converters
{
    public class GastoToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Presupuesto p)
            {
                var total = p.TotalPresupuesto ?? 0m;
                var spent = p.MontoEjePresupuesto ?? 0m;
                return spent > total ? Colors.Red : Colors.Black;
            }
            if (value is decimal spentValue && parameter is decimal totalValue)
            {
                return spentValue > totalValue ? Colors.Red : Colors.Black;
            }
            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
