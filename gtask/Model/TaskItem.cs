using gTask.Resources;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace gTask.Model
{
    public class TaskItem : INotifyPropertyChanged
     {
        public TaskItem(string id, string kind, string title, string notes, string parentId, string position, string updatedDate, string dueDate, string deleted, string isHidden, string status, string selflink, string completed, string updated)
        {
            this.id = id;
            this.kind = kind;
            this.title = title;
            selfLink = selflink;
            this.position = position;
            this.updated = convertDate(updated);
            this.status = status;
            this.notes = notes;
            this.parent = null;
            due = convertDate(dueDate); 
            this.deleted = deleted;
            hidden = isHidden;
            this.completed = convertDate(completed); 
        }

        public string convertDate (string oldDate)
        {
            string newDate = oldDate;
            if (oldDate != null)
            {
                newDate = Convert.ToDateTime(Universal.ConvertToUniversalDate(oldDate)).ToString("yyyy-MM-dd'T'hh:mm:ss.00Z");
            }
            return newDate;
        }

        private string _title;
        public string title
        {
            get { return _title == String.Empty ? "Empty" : _title; }
            set 
            { 
                _title = value;
                OnPropertyChanged("TaskItem");
            }
        }


        public async Task<bool> Update(Action<bool> response)
        {
            return await TaskHelper.UpdateTask(this, response);
        }

        public string id { get; set; }
        public string kind { get; set; }
        public string selfLink { get; set; }
        private string _completed;
        public string completed
        {
            get
            {
                return _completed;
            }
            set { _completed = value; }
        }

        private string _notes;
        public string notes
        {
            get { return _notes; }
            set { _notes = value; }
        }

        public string parent { get; set; }
        public string position { get; set; }
        public string updated { get; set; }
        public string due { get; set; }
        public string deleted { get; set; }
        public string hidden { get; set; }
        private string _status;

        public TaskItem()
        {
        }

        public string status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged("status");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}