using gTask.Model;
using gTask.Resources;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace gTask.ViewModels
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        public string ID { get; set; }

        #region Private Members

        private static ObservableCollection<TaskItem> _tasks;
        private TaskListItem _parentList = new TaskListItem();
        private bool _isDataLoaded;

        #endregion

        public bool IsDataLoaded
        {
            get { return _isDataLoaded; }
            set
            {
                _isDataLoaded = value;
                OnPropertyChanged("IsDataLoaded");
            }
        }

        //public TaskViewModel(string id)
        //{
        //    SetVariables(id);
        //    LoadData(ID);
        //}

        private void SetVariables(string id)
        {
            TaskItem = null;
            ID = id;
        }

        public TaskViewModel()
        {
            
        }

        public async Task LoadData(string id)
        {
            SetVariables(id);
            await TaskListHelper.GetSpecificTaskList(ID, SetTaskListItem);
        }

        private async void SetTaskListItem(TaskListItem obj)
        {
            ParentList = obj;
            await TaskListHelper.GetTasksForList(ID, SetTasks);
        }

        private void SetTasks(ObservableCollection<TaskItem> obj)
        {
            TaskItem = new ObservableCollection<TaskItem>(obj);
        }

        public TaskListItem ParentList
        {
            get { return _parentList; }
            set
            {
                _parentList = value; 
                OnPropertyChanged("ParentList");
            }
        }

        /// <summary>
        /// A collection for TaskListItem objects.
        /// </summary>
        public ObservableCollection<TaskItem> TaskItem
        {
            get { return _tasks ?? new ObservableCollection<TaskItem>(); }
            set
            {
                _tasks = value;
                if (_tasks != null)
                {
                    OnPropertyChanged("TaskItem");
                    IsDataLoaded = true;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
