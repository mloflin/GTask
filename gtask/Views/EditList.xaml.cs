using gTask.Model;
using gTask.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Windows.Phone.Speech.Recognition;

namespace gTask.Views
{
    public partial class EditList : PhoneApplicationPage
    {
        private static string _id = String.Empty;
        SpeechRecognizerUI recoWithUI;
        private static string _oldtitle = string.Empty;

        public EditList()
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

        #region menu
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
            this.NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        #endregion

        #region Event Handlers

        private void GetResponse(bool obj)
        {        
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
        }

        //Upon loading it pulls in the information of the task list based on the ID
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (NavigationContext.QueryString.ContainsKey("Id"))
            {
                _id = NavigationContext.QueryString["Id"];
                if (!_id.Equals("new"))
                {
                    var s = from p in App.MainViewModel.Tasks
                            where p.id == _id
                            select p;
                    txtbxTitle.Text = s.Single().title;
                }
                else
                {
                    txtPageTitle.Text = txtPageTitle.Text.Replace("Edit", "Add");
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

        //Speech
        private async void speech_Click(object sender, EventArgs eventArgs)
        {
            string message = "Excuse me, what did you say?!";
            string txtbxType = "Title";
            if (GTaskSettings.IsFree)
            {
                GTaskSettings.Upsell();
            }
            else
            {
                try
                {
                    // Create an instance of SpeechRecognizerUI.
                    this.recoWithUI = new SpeechRecognizerUI();
                    recoWithUI.Settings.ReadoutEnabled = false;
                    recoWithUI.Settings.ShowConfirmation = false;

                    recoWithUI.Settings.ListenText = "Listening for Task List Title...";
                    recoWithUI.Settings.ExampleText = "Ex. 'Grocery List'";

                    // Start recognition (load the dictation grammar by default).
                    SpeechRecognitionUIResult recoResult = await recoWithUI.RecognizeWithUIAsync();

                    // Do something with the recognition result.
                    string txtbxText = txtbxTitle.Text;
                    string FinalText = string.Empty;
                    int SelectionStart = txtbxTitle.SelectionStart;
                    int SelectionLength = txtbxTitle.SelectionLength;
                    int SelectionEnd = SelectionStart + SelectionLength;
                    string SpeakResult = (recoResult.RecognitionResult == null) ? string.Empty : recoResult.RecognitionResult.Text;

                    if (SpeakResult == string.Empty) //If nothing in speech result, don't do anything
                        return;

                    FinalText = SpeechHelper.FormatSpeech(SelectionStart, txtbxText, SelectionEnd, SpeakResult, txtbxType);

                    if (FinalText != String.Empty) //Results are returned
                    {
                        if (SelectionLength == 0) //0 means it is an insert
                        {
                            txtbxTitle.Text = txtbxTitle.Text.Insert(SelectionStart, FinalText);
                            txtbxTitle.Select(SelectionStart + FinalText.Length, 0); //Set the cursor location to where the start was previously
                        }
                        else //greater than 0 means it is a replace
                        {
                            txtbxTitle.SelectedText = FinalText;
                            txtbxTitle.Select(SelectionStart + FinalText.Length, 0); //Set the cursor location to where the start was previously
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

        //Cancel to MainPage
        void cancel_Click(object sender, EventArgs eventArgs)
        {
             NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
        }

        //Saves the new or edited task list and navigates back to MainPage
        private async void save_Click(object sender, EventArgs eventArgs)
        {
            SetProgressIndicator(true);
            if (!_id.Equals("new"))
            {
                SystemTray.ProgressIndicator.Text = "Creating Task List";
            }
            else
            {
                SystemTray.ProgressIndicator.Text = "Updating Task List";
            }
            
            
            if (!string.IsNullOrEmpty(txtTitle.Text.Trim()))
            {
                //To prevent multiple requests, disable buttons temporarily
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
                ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
                ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = false;

                if (!_id.Equals("new"))
                {
                    var s = from p in App.MainViewModel.Tasks
                            where p.id == _id
                            select p;
                    var taskList = s.SingleOrDefault();
                    if (taskList != null)
                    {
                        if (taskList.title != txtbxTitle.Text.Trim()) //check that there is a diff before making updates
                        {
                            taskList.title = txtbxTitle.Text.Trim();
                            bool results = await taskList.Update(GetResponse);

                            if (results)
                            {
                                if (GTaskSettings.MsgUpdateTaskList)
                                {
                                    MessageBoxResult n = MessageBox.Show("Complete.", "Update Task List", MessageBoxButton.OK);
                                }
                            }
                            else
                            {
                                // Update the list locally
                                List<TaskListItem> lists = await TaskListHelper.GetTaskListFromApplicationStorage();
                                lists.Where(x => x.id == taskList.id).First().title = txtbxTitle.Text.Trim();
                                lists.Where(x => x.id == taskList.id).First().updated = DateTime.UtcNow.ToString("yyyy-MM-dd'T'hh:mm:ss.00Z"); // DateTime.UtcNow.ToString();

                                // Resubmit the list to local storage
                                await TaskListHelper.SubmitToLocalStorage(lists);

                                MessageBoxResult m = MessageBox.Show("This task list was updated while offline, please sync once back online.", "Offline Mode", MessageBoxButton.OK);
                            }
                        }
                        NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
                    }
                    else //There has been an error
                    {
                        const string message = "Howdy Partner. I can't find the task list, can you try again?";
                        if (GTaskSettings.MsgError)
                        {
                            MessageBox.Show(message);
                        }
                    }
                }
                else
                {
                   TaskListItem results = await TaskListHelper.AddTaskList(
                        new TaskListItem(txtbxTitle.Text.Trim(), String.Empty, String.Empty, String.Empty, DateTime.UtcNow.ToString()), GetResponse);

                   if (results != null)
                   {
                        //await GTaskSettings.RefreshData(false, null, true);
                        //await App.MainViewModel.LoadData(true, true);

                        //Set DataContext to TaskList(s)
                        //DataContext = App.MainViewModel;
                        if (GTaskSettings.MsgCreateTaskList)
                        {
                            MessageBoxResult n = MessageBox.Show("Complete.", "Create Task List", MessageBoxButton.OK);
                        }
                   }
                   else
                   {
                       // Create a random id for temporary purposes
                       string id = Guid.NewGuid().ToString();

                       // Add a setting to flag the list
                       var settings = IsolatedStorageSettings.ApplicationSettings;
                       settings.Add("List_" + id + "_Action", "added");
                       settings.Add("List_" + id + "_Timestamp", DateTime.UtcNow.ToString());

                       // Get local storage
                       List<TaskListItem> list = await TaskListHelper.GetTaskListFromApplicationStorage();

                       TaskListItem newList = new TaskListItem(txtbxTitle.Text.Trim(), id, "tasks#taskList", "https://www.googleapis.com/tasks/v1/users/@me/lists/" + id, DateTime.UtcNow.ToString());

                       // Add this new list to the local list
                       list.Add(newList);

                       // Resubmit the list to local storage
                       await TaskListHelper.SubmitToLocalStorage(list);

                        MessageBoxResult m = MessageBox.Show("This task list was created while offline, please sync once back online.", "Offline Mode", MessageBoxButton.OK);
                    }

                   //Given we don't know the TaskID of this item yet, if you don't return it will create multiple identical task lists
                   NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
                }
            }
            else
            {
                MessageBoxResult m = MessageBox.Show("Please include a Title.", "Title", MessageBoxButton.OK);
            }

            //Turn buttons back on
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = true;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = true;

            SetProgressIndicator(false);
        }

        //On keyback it sends you to MainPage
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            while (NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();
            e.Cancel = true;
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
        }

        #endregion

        private void AdControl_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Ad Error : ({0}) {1}", e.ErrorCode, e.Error);
        }
    }
}