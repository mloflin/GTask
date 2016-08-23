using gTask.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace gTask.Model
{
    public class TaskListItem : INotifyPropertyChanged
    {
        public string id { get; set; }
        public string kind { get; set; }
        public string updated { get; set; }
        public string selfLink { get; set; }
        private string _title;
        public string title
        {
            set
            {
                _title = value;
                OnPropertyChanged("Tasks");
            }
            get { return _title; }
        }
        public List<TaskItem> taskList { get; set; }
        
        public event PropertyChangedEventHandler PropertyChanged;

        public TaskListItem(string title, string id, string kind,string selflink, string updated)
        {
            this.id = id;
            this.kind = kind;
            this.selfLink = selflink;
            this.updated = updated;
            _title = title;
            this.taskList = new List<TaskItem>();
        }

        public TaskListItem()
        {
            this.taskList = new List<TaskItem>();
        }

        public async Task<bool> Update(Action<bool> Response)
        {
            bool results = await TaskListHelper.UpdateList(this, Response);
            OnPropertyChanged("Tasks");
            return results;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
