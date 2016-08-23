using gTask.Model;
using gTask.Resources;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace gTask.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            // No longer need
            //LoadData();
        }

        #region Private Members

        private static ObservableCollection<TaskListItem> _tasks;
        private bool _isDataLoaded;

        #endregion

        /// <summary>
        /// A collection for TaskListItem objects.
        /// </summary>
        public ObservableCollection<TaskListItem> Tasks
        {
            get { return _tasks; }
            set
            {
                _tasks = value;
                if (_tasks.Count > 0)
                {
                    OnPropertyChanged("Tasks");
                    IsDataLoaded = true;
                }
            }
        }

        public bool IsDataLoaded
        {
            get { return _isDataLoaded; }
            set
            {
                _isDataLoaded = value;
                OnPropertyChanged("IsDataLoaded");
            }
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public async Task LoadData(bool alertWhenNoConnection = false, bool refresh = false)
        {
            await TaskListHelper.GetTaskList(SetTaskList, alertWhenNoConnection, refresh);
        }

        public void SetTaskList(ObservableCollection<TaskListItem> obj)
        {
            Tasks = obj;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}