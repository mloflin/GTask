using System;
using System.Globalization;
using System.Windows.Data;

namespace gTask.Resources
{
    public class TaskStatusConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        /// <param name="value">The source data being passed to the target.</param><param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param><param name="parameter">An optional parameter to be used in the converter logic.</param><param name="culture">The culture of the conversion.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var input = value as string;

            //If someone has completed a task in Gmail and the "AutoClear" == True, then clear the completed
            if (input == "completed" && GTaskSettings.AutoClear == true && GTaskSettings.HideCompleted != false)
            {
                ClearCompleted();
            }

            return input != null && !input.Equals("needsAction");
        }
        public async void ClearCompleted()
        {
            var parentListId = App.TaskViewModel.ParentList.id;
            var parentListTitle = App.TaskViewModel.ParentList.title;
                
            if (parentListId != null)
            {
                //Clear Tasks
                await TaskHelper.ClearCompletedTasks(parentListId);
            }

            if (parentListId != null && parentListTitle != null)
            {
                //Load the Tasks
                await App.TaskViewModel.LoadData(parentListId);
            }
        }
           

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        /// <param name="value">The target data being passed to the source.</param><param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param><param name="parameter">An optional parameter to be used in the converter logic.</param><param name="culture">The culture of the conversion.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var input = value is bool && (bool)value;
            return input ? "completed" : "needsAction";
        }

        #endregion
    }
}

