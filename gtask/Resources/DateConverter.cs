using System;
using System.Globalization;
using System.Windows.Data;

namespace gTask.Resources
{
    public class DateConverter : IValueConverter
    {
        //Takes a datetime and returns a date
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var formatString = value as string;
            //If user wants date hidden return empty
            if (GTaskSettings.HideDueDate)
                return String.Empty;
            try
            {
                //Check if Null or Empty
                if (string.IsNullOrEmpty(formatString))
                {
                    return "No Due Date";
                }

                //Try to convert it to ShortDateString if "." is not included (assuming US Version)
                formatString = Universal.ConvertToUniversalDate(formatString);

                //Try to remove 00:00:00 and 12:00:00 from string and just return it
                formatString = formatString.Replace(" 00:00:00", "");
                formatString = formatString.Replace(" 12:00:00", "");
                return formatString;
            }
            catch
            {
                //Return if error
                //MessageBox.Show(message);
                return "Invalid Due Date";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return String.Empty;
        }

    }
}
