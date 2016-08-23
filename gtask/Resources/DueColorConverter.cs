using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace gTask.Resources
{
    public class DueColorConverter : IValueConverter
    {
        //Takes a datetime and returns a date
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var id = value as string;
                var s = from p in App.TaskViewModel.TaskItem
                        where p.id == id
                        select p;
                var taskItem = s.SingleOrDefault();
                if (taskItem != null)
                {
                    var formatString = taskItem.due as string;
                    if (!string.IsNullOrEmpty(formatString))
                    {
                        formatString = DateTime.Parse(Universal.ConvertToUniversalDate(formatString)).Date.ToShortDateString();
                        if(taskItem.status == "completed")
                        {
                            return "Gray";
                        }
                        else if (DateTime.Parse(formatString) < DateTime.Parse(DateTime.Now.ToShortDateString()))
                        {
                            return "Red";
                        }
                        else if (formatString == DateTime.Now.ToShortDateString())
                        {
                            return "Green";
                        }
                    }
                    else if (GTaskSettings.NoDueDateAtTop && GTaskSettings.TaskSort == 1) //No Due Date
                    {
                        return "Red";
                    }
                    return Application.Current.Resources["PhoneForegroundBrush"] as Brush; ;
                }
                return Application.Current.Resources["PhoneForegroundBrush"] as Brush; ;
            }
            return Application.Current.Resources["PhoneForegroundBrush"] as Brush; ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return String.Empty;
        }
    }
}
