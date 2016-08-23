using System;
using System.Globalization;
using System.Windows.Data;

namespace gTask.Resources
{
    public class TitleConverter : IValueConverter
    {
        //Takes a title and if it is too long it shortens it and adds "..."
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var formatString = value as string;
 
            if (!string.IsNullOrEmpty(formatString))
            {
                if (formatString.Length > 35)
                {
                    formatString = formatString.Substring(0, 35) + "...";
                }

                return formatString;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return String.Empty;
        }
    }
}
