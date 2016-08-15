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

}
