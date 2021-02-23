using System;
using System.Globalization;

using Xamarin.Forms;

namespace Genshin_Impact_Tasks.Converter
{
    public class UseDarkModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? "White" : "Black";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
