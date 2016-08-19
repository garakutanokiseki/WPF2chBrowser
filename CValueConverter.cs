using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using System.Diagnostics;

namespace _2chBrowser
{
    /// <summary>
    /// 抽出文字列を色に変換する
    /// </summary>
    [ValueConversion(typeof(SolidColorBrush), typeof(bool))]
    public class VisibleColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bIsVisible = (bool)value;
            if (bIsVisible == true)
                return new SolidColorBrush(Color.FromArgb(0x90, 0x56, 0x8E, 0xFF));
            else
                return new SolidColorBrush(Color.FromArgb(0x00, 0x00, 0x00, 0x00));

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(int), typeof(Visibility))]
    public class StatusVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int status = (int)value;
            string str_type = (string)parameter;

            if (status == 0 && str_type == "normal")return Visibility.Visible;
            if (status == 1 && str_type == "new") return Visibility.Visible;
            if (status == 2 && str_type == "up") return Visibility.Visible;
            if (status == 3 && str_type == "down") return Visibility.Visible;
            if (status == 4 && str_type == "read") return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
