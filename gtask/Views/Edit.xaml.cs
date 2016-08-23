using gTask.Model;
using gTask.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Windows.Phone.Speech.Recognition;

namespace gTask.Views
{
    public partial class Edit : PhoneApplicationPage
    {
        private static string _id = String.Empty;
        private static string _listId = String.Empty;
        SpeechRecognizerUI recoWithUI;
        private TextBox focusedTextbox = null;

        public Edit()
        {
            InitializeComponent();

            //Set Title
            txtTitle.Text = GTaskSettings.ApplicationTitle;

            //Create Menu Bar
            BuildLocalizedApplicationBar();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
        }

        private static void SetProgressIndicator(bool isVisible)
        {
            SystemTray.ProgressIndicator.IsIndeterminate = isVisible;
            SystemTray.ProgressIndicator.IsVisible = isVisible;
        }


        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            //Save button
            ApplicationBarIconButton appBarSaveButton = new ApplicationBarIconButton(new Uri("/Assets/save.png", UriKind.Relative));
            appBarSaveButton.Text = AppResources.AppBarSaveButtonText;
            ApplicationBar.Buttons.Add(appBarSaveButton);
            appBarSaveButton.Click += new EventHandler(save_Click);

            //Cancel button
            ApplicationBarIconButton appBarCancelButton = new ApplicationBarIconButton(new Uri("/Assets/cancel.png", UriKind.Relative));
            appBarCancelButton.Text = AppResources.AppBarCancelButtonText;
            ApplicationBar.Buttons.Add(appBarCancelButton);
            appBarCancelButton.Click += new EventHandler(cancel_Click);

            //Speech Button
            ApplicationBarIconButton appBarSpeechButton = new ApplicationBarIconButton(new Uri("/Assets/speak.png", UriKind.Relative));
            appBarSpeechButton.Text = AppResources.AppBarSpeechButtonText;
            ApplicationBar.Buttons.Add(appBarSpeechButton);
            appBarSpeechButton.Click += new EventHandler(speech_Click);

            //settings menu item
            ApplicationBarMenuItem appSettingsBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarSettingsMenuText);
            ApplicationBar.MenuItems.Add(appSettingsBarMenuItem);
            appSettingsBarMenuItem.Click += new EventHandler(settings_Click);
        }

        void settings_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Settings.xaml?p=Task_Edit", UriKind.Relative));
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            txtDueDate.Value = null;
            txtDueDate.IsEnabled = false;
        }

        private void chkNoReminder_Unchecked(object sender, RoutedEventArgs e)
        {
            txtDueDate.Value = DateTime.Now.Date;
            txtDueDate.IsEnabled = true;
        } 

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                _id = NavigationContext.QueryString.ContainsKey("Id")
                          ? NavigationContext.QueryString["Id"]
                          : String.Empty;
                _listId = NavigationContext.QueryString.ContainsKey("List")
                              ? NavigationContext.QueryString["List"]
                              : String.Empty;
                if (!_id.Equals("new"))
                {
                    var s = from p in App.TaskViewModel.TaskItem
                            where p.id == _id
                            select p;
                    var taskItem = s.SingleOrDefault();
                    if (taskItem != null)
                    {
                        txtbxTitle.Text = taskItem.title;
                        txtNotes.Text = taskItem.notes ?? String.Empty;

                        //Adjust font size if a lot of notes
                        if (GTaskSettings.TextSize != 0)
                        {
                            //large option is 24 which is option 1
                            //24 - 0*2 = 24, next option is 20 (26 - 4*1 = 22)
                            var size = 24 - (GTaskSettings.TextSize * 4);
                            txtNotes.FontSize = size;
                                
                        }

                        SetRemaining();

                        if (!string.IsNullOrEmpty(taskItem.due))
                        {
                            chkNoDueDate.IsChecked = false;
                            txtDueDate.Value = DateTime.Parse(Universal.ConvertToUniversalDate(taskItem.due)).Date; //DateTime.Parse(taskItem.due).Date;
                        }
                        else
                        {
                            chkNoDueDate.IsChecked = true;
                        }
                    }
                }
                else
                {
                    PageTitle.Text = PageTitle.Text.Replace("Edit", "Add");

                    SetRemaining();

                    //Based on settings set DefaultReminder
                    if (GTaskSettings.DefaultReminder == 2) //0 = Today's Date, 1 = Tomorrow's Date, 2 = No Reminder
                    {
                        txtDueDate.Value = null;
                        txtDueDate.IsEnabled = false;
                        chkNoDueDate.IsChecked = true;
                    }
                    else if (GTaskSettings.DefaultReminder == 0) //Today's Date
                    {
                        if (GTaskSettings.DefaultReminderTomorrowIf && DateTime.Now.TimeOfDay > GTaskSettings.DefaultReminderTomorrowIfTime.TimeOfDay)
                        {
                            txtDueDate.Value = DateTime.Now.AddDays(1).Date;
                        }
                        else
                        {
                            txtDueDate.Value = DateTime.Now.Date;
                        }
                    }
                    else //Tomorrow's Date
                    {
                        txtDueDate.Value = DateTime.Now.AddDays(1).Date;
                    }
                }
            }

            //Turnoff Ads if not paid for
            if (GTaskSettings.IsFree)
            {
                AdControl.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                AdControl.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void GetResponse(bool obj)
        {
            NavigationService.Navigate(new Uri("/Views/TaskView.xaml?Id=" + _listId, UriKind.RelativeOrAbsolute));
        }

        private async void speech_Click(object sender, EventArgs eventArgs)
        {
            string message = "Excuse me, what did you say?!";
            string txtbxType = string.Empty;
            if (GTaskSettings.IsFree)
            {
                GTaskSettings.Upsell();
            }
            else
            {


                try
                {
                    //If no textbox is selected, there is no where to put the text
                    if (focusedTextbox == null)
                    {
                        MessageBoxResult o = MessageBox.Show("Please select the text box you want to use and try again.", "Which Text Box?", MessageBoxButton.OK);
                        return;
                    }

                    // Create an instance of SpeechRecognizerUI.
                    this.recoWithUI = new SpeechRecognizerUI();
                    recoWithUI.Settings.ReadoutEnabled = false;
                    recoWithUI.Settings.ShowConfirmation = false;

                    if (focusedTextbox.Name == "txtbxTitle")
                    {
                        recoWithUI.Settings.ListenText = "Listening for Task Title...";
                        recoWithUI.Settings.ExampleText = "Ex. 'Mow the lawn'";
                        txtbxType = "Title";
                    }
                    else
                    {
                        recoWithUI.Settings.ListenText = "Listening for Tasks Notes...";
                        recoWithUI.Settings.ExampleText = "Ex. 'This needs to be done by Tuesday.'";
                        txtbxType = "Notes";
                    }

                    // Start recognition (load the dictation grammar by default).
                    SpeechRecognitionUIResult recoResult = await recoWithUI.RecognizeWithUIAsync();

                    // Do something with the recognition result.
                    string txtbxText = focusedTextbox.Text;
                    string SpeakResult = (recoResult.RecognitionResult == null) ? string.Empty : recoResult.RecognitionResult.Text;
                    string FinalText = string.Empty;
                    int SelectionStart = focusedTextbox.SelectionStart;
                    int SelectionLength = focusedTextbox.SelectionLength;
                    int SelectionEnd = SelectionStart + SelectionLength;

                    if (SpeakResult == string.Empty) //If nothing in speech result, don't do anything
                        return;

                    FinalText = SpeechHelper.FormatSpeech(SelectionStart, txtbxText, SelectionEnd, SpeakResult, txtbxType);

                    if (FinalText != String.Empty) //Results are returned
                    {
                        if (SelectionLength == 0) //0 means it is an insert
                        {
                            focusedTextbox.Text = focusedTextbox.Text.Insert(SelectionStart, FinalText);
                            focusedTextbox.Select(SelectionStart + FinalText.Length, 0); //Set the cursor location to where the start was previously
                        }
                        else //greater than 0 means it is a replace
                        {
                            focusedTextbox.SelectedText = FinalText;
                            focusedTextbox.Select(SelectionStart + FinalText.Length, 0); //Set the cursor location to where the start was previously
                        }
                    }
                }
                catch
                {
                    if (GTaskSettings.MsgError)
                    {
                        MessageBox.Show(message);
                    }
                }
            }
        }

        //Returns to TaskView
        void cancel_Click(object sender, EventArgs eventArgs)
        {
            NavigationService.Navigate(new Uri("/Views/TaskView.xaml?Id=" + _listId, UriKind.RelativeOrAbsolute));
        }

        private async void save_Click(object sender, EventArgs e)
        {
            SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "Saving Task";

            if (!string.IsNullOrEmpty(txtbxTitle.Text.Trim()))
            {
                //Disable buttons
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
                ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
                ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = false;

                if (!_id.Equals("new"))
                {
                    var s = from p in App.TaskViewModel.TaskItem
                            where p.id == _id
                            select p;

                    //Set the variables
                    var taskItem = s.First();

                    string oldNotes = null;
                    string newNotes = null;
                    if (taskItem.notes != null) { oldNotes = taskItem.notes; }
                    if (txtNotes.Text.Trim() != null && txtNotes.Text.Trim() != "") { newNotes = txtNotes.Text.Trim(); }

                    string oldDue = null;
                    string newDue = null;
                    if (taskItem.due != null) { oldDue = Convert.ToDateTime(Universal.ConvertToUniversalDate(taskItem.due)).ToString("yyyy-MM-dd'T'hh:mm:ss.00Z"); }
                    if (txtDueDate.Value.ToString() != null) { newDue = GetReminderDateTime(); }

                    //check for changes before submitting to google
                    if (taskItem.title != txtbxTitle.Text.Trim() || oldNotes != newNotes || oldDue != newDue)
                    {
                        taskItem.title = txtbxTitle.Text.Trim();
                        taskItem.notes = newNotes;
                        taskItem.due = newDue;

                        //Update the task
                        bool result = await taskItem.Update(GetResponse);

                        // Update the task locally
                        List<TaskListItem> lists = await TaskListHelper.GetTaskListFromApplicationStorage();

                        foreach (TaskListItem list in lists)
                        {
                            foreach (TaskItem task in list.taskList.Where(x => x.id == _id))
                            {
                                task.title = taskItem.title;
                                task.notes = taskItem.notes;
                                task.due = taskItem.due;
                                task.updated = DateTime.UtcNow.ToString("yyyy-MM-dd'T'hh:mm:ss.00Z"); //DateTime.UtcNow.ToString();
                            }
                        }

                        // Resubmit the list to local storage
                        await TaskListHelper.SubmitToLocalStorage(lists);

                        if (result)
                        {
                            if (GTaskSettings.MsgUpdateTask)
                            {
                                MessageBoxResult m = MessageBox.Show("Complete.", "Update Task", MessageBoxButton.OK);
                            }
                        }
                        else
                        {
                            MessageBoxResult m = MessageBox.Show("This task was updated while offline, please sync once back online.", "Offline Mode", MessageBoxButton.OK);
                        }
                    }

                    NavigationService.Navigate(new Uri("/Views/TaskView.xaml?Id=" + _listId, UriKind.RelativeOrAbsolute));
                }
                else //if it is new
                {
                    var t = new TaskItem {};
                    t.title = txtbxTitle.Text.Trim();
                    t.notes = txtNotes.Text.Trim();

                    if (!string.IsNullOrEmpty(txtDueDate.Value.ToString()))
                    {
                        t.due = GetReminderDateTime();
                    }
                    else
                    {
                        t.due = null;
                    }

                    //Create the Task
                    TaskItem result = await TaskHelper.AddTask(t, GetResponse, _listId);

                    if (result != null)
                    {
                        // Refresh the data
                        await GTaskSettings.RefreshData(false,_listId,true);
                        if (GTaskSettings.MsgCreateTask)
                        {
                            MessageBoxResult n = MessageBox.Show("Complete.", "Create Task", MessageBoxButton.OK);
                        }
                    }
                    else
                    {
                        // Create a random ID
                        t.id = Guid.NewGuid().ToString();
                        t.status = "needsAction";
                        t.updated = DateTime.UtcNow.ToString("yyyy-MM-dd'T'hh:mm:ss.00Z");
                        t.kind = "tasks#task";
                        t.selfLink = "https://www.googleapis.com/tasks/v1/lists/" + _listId + "/tasks/" + t.id;

                        // Add a setting to flag the list
                        var settings = IsolatedStorageSettings.ApplicationSettings;
                        settings.Add("Task_" + t.id + "_Action", "added");
                        settings.Add("Task_" + t.id + "_Timestamp", DateTime.UtcNow.ToString());

                        // Get local storage
                        List<TaskListItem> list = await TaskListHelper.GetTaskListFromApplicationStorage();

                        // Set the position
                        // Check if there are items in the current list
                        if (list.Where(x => x.id == _listId).First().taskList.Count() > 0)
                        {
                            t.position = (double.Parse(list.Where(x => x.id == _listId).First().taskList.OrderBy(x => x.position).First().position) - 1).ToString();
                        }
                        else
                        {
                            t.position = "10000";
                        }

                        // Add the task to the list
                        list.Where(x => x.id == _listId).First().taskList.Insert(0, t);

                        // Resubmit the list to local storage
                        await TaskListHelper.SubmitToLocalStorage(list);

                        MessageBoxResult m = MessageBox.Show("This task was created while offline, please sync once back online.", "Offline Mode", MessageBoxButton.OK);
                    }

                    //Navigate back to TaskView if "New"
                    //Given we don't know the TaskID of this item yet, if you don't return it will create multiple identical tasks
                    NavigationService.Navigate(new Uri("/Views/TaskView.xaml?Id=" + _listId, UriKind.RelativeOrAbsolute));
                }
            }
            else
            {
                MessageBoxResult o = MessageBox.Show("Well, hello there! Do you mind including a Title?", "Title?", MessageBoxButton.OK);
            }

            //re-enable buttons
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = true;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = true;

            SetProgressIndicator(false);
        }

        private string GetReminderDateTime()
        {
            //Try to convert the date to google in the manner they need.
            string dueDate = null;
            try
            {
                if (txtDueDate.Value != null)
                {
                    dueDate = ((DateTime)txtDueDate.Value).ToString("yyyy-MM-dd'T'hh:mm:ss.00Z");
                    //if (GTaskSettings.MsgError)
                    //{
                    //    MessageBox.Show("Success converting date: " + txtDueDate.Value.ToString() + " to " + dueDate.ToString());
                    //}
                }
                return dueDate;
            }
            catch
            {
                if (GTaskSettings.MsgError)
                {
                    MessageBox.Show("Error converting date: " + txtDueDate.Value.ToString());
                }
            }
            return dueDate;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            while (NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();
            e.Cancel = true;
            NavigationService.Navigate(new Uri("/Views/TaskView.xaml?Id=" + _listId, UriKind.RelativeOrAbsolute));
        }

        private void txtNotes_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            SetRemaining();
        }

        private void SetRemaining()
        {
            //simulate a text limit based on the font size chosen
            int MaxChar = 0;
            switch (GTaskSettings.TextSize)
            {
                case 0:
                    MaxChar = 2500;
                    break;
                case 1:
                    MaxChar = 4200;
                    break;
                case 2:
                    MaxChar = 6800;
                    break;
                case 3:
                    MaxChar = 8200;
                    break;
                default:
                    MaxChar = 0;
                    break;
            }

            int Remaining = MaxChar - txtNotes.Text.Length;
            txtRemaining.Text = "(" + Remaining + "/" + MaxChar + ")";

            //color red if too many characters (over limit of UI)
            if (Remaining < 0)
            {
                txtRemaining.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                // Determine the visibility of the dark background.
                Visibility darkBackgroundVisibility = (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"];

                // Write the theme background value.
                if (darkBackgroundVisibility == Visibility.Visible)
                {
                    txtRemaining.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    txtRemaining.Foreground = new SolidColorBrush(Colors.Black);
                }
            }
        }

        private void AdControl_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Ad Error : ({0}) {1}", e.ErrorCode, e.Error);
        }

        private void txtbxTitle_GotFocus(object sender, RoutedEventArgs e)
        {
            focusedTextbox = (TextBox)sender;
        }

        private void txtNotes_GotFocus(object sender, RoutedEventArgs e)
        {
            focusedTextbox = (TextBox)sender;
        }

        private void txtbxTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            focusedTextbox = null;
        }

        private void txtNotes_LostFocus(object sender, RoutedEventArgs e)
        {
            focusedTextbox = null;
        }
    }
}