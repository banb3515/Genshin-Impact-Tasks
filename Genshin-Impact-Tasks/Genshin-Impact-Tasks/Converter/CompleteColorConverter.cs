using System;
using System.Globalization;

using Xamarin.Forms;

namespace Genshin_Impact_Tasks.Converter
{
    public class CompleteColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool) value) ? "#85CC6F" : "#D3D3D3";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}