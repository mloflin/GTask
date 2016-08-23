using gTask.Model;
using gTask.Resources;
using gTask.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace gTask.Views
{
    public partial class TaskView : PhoneApplicationPage
    {
        public static string Id = String.Empty;
        public static string curTitle = String.Empty;
        public static string name = String.Empty;

        public TaskView()
        {
            InitializeComponent();    
        }


        private static void SetProgressIndicator(bool isVisible)
        {
            SystemTray.ProgressIndicator.IsIndeterminate = isVisible;
            SystemTray.ProgressIndicator.IsVisible = isVisible;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();

            if (!string.IsNullOrEmpty(Id))
            {
                //Check to see if the Id exists.
                Check_Id(Id);

                if (GTaskSettings.AutoRefreshTasks)
                {
                    refresh(true);
                }
                else
                {
                    refresh();
                }
                    
            }
            else //return to MainPage to select an accurate Task List
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
            }
        }

        #region Menu
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            //Add button
            ApplicationBarIconButton appBarNewTaskButton = new ApplicationBarIconButton(new Uri("/Assets/add.png", UriKind.Relative));
            appBarNewTaskButton.Text = AppResources.AppBarAddTaskButtonText;
            ApplicationBar.Buttons.Add(appBarNewTaskButton);
            appBarNewTaskButton.Click += new EventHandler(new_Click);

            //Refresh button
            ApplicationBarIconButton appBarRefreshButton = new ApplicationBarIconButton(new Uri("/Assets/sync.png", UriKind.Relative));
            appBarRefreshButton.Text = AppResources.AppBarRefreshButtonText;
            ApplicationBar.Buttons.Add(appBarRefreshButton);
            appBarRefreshButton.Click += new EventHandler(refresh_Click);


            if (GTaskSettings.AutoClear == false)
            {
                //clear completed
                ApplicationBarMenuItem appClearBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarCleartMenuText);
                ApplicationBar.MenuItems.Add(appClearBarMenuItem);
                appClearBarMenuItem.Click += new EventHandler(completed_Click); 
            }


            if (GTaskSettings.HideCompleted == true)
            {
                //view completed
                ApplicationBarMenuItem appViewCompletedBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarViewMenuText);
                ApplicationBar.MenuItems.Add(appViewCompletedBarMenuItem);
                appViewCompletedBarMenuItem.Click += new EventHandler(hidden_Click);
            }
            else
            {
                //hide completed
                ApplicationBarMenuItem appHideCompletedBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarHideMenuText);
                ApplicationBar.MenuItems.Add(appHideCompletedBarMenuItem);
                appHideCompletedBarMenuItem.Click += new EventHandler(hidden_Click);
            }

            //view task lists
            ApplicationBarMenuItem appTaskListsMenuItem = new ApplicationBarMenuItem(AppResources.AppBarTaskListMenuText);
            ApplicationBar.MenuItems.Add(appTaskListsMenuItem);
            appTaskListsMenuItem.Click += new EventHandler(tasklists_Click);

            //settings menu item
            ApplicationBarMenuItem appSettingsBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarSettingsMenuText);
            ApplicationBar.MenuItems.Add(appSettingsBarMenuItem);
            appSettingsBarMenuItem.Click += new EventHandler(settings_Click);

            //Turn On/Off Reorder ability
            //If TaskSort = Google Sort -> 1 then Enable, else Disable
            //OR if DisableDragDrop = True
            if (GTaskSettings.TaskSort == 0 && GTaskSettings.DisableDragDrop != true)
            {
                TasksView.IsReorderEnabled = true;
            }
            else
            {
                TasksView.IsReorderEnabled = false;
            }

            //AddPinButton
            AddPinButton();
        }

        void tasklists_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        void settings_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Settings.xaml?p=Task_View", UriKind.Relative));
        }
        #endregion

        //If not logged it - take back to homepage where it will prompt
        //if logged in then pull in list, check to see if "Pinned" and show pin or unpin accordingly
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {           
            //If the user isn't logged in then go to the MainPage
            if (!GTaskSettings.IsLoggedIn())
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
            }
            else
            {
                //Check to see if Reminder should be shown
                LoginHelper.Reminder();
            }



            //If there is an ID set it, else go to MainPage
            if (NavigationContext.QueryString.ContainsKey("Id"))
            {
                Id = NavigationContext.QueryString["Id"];

                //Check to see if the Id exists.
                Check_Id(Id);

                //Set Title
                txtTitle.Text = GTaskSettings.ApplicationTitle;
            }
            else
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
            }

            //Create Menu Bar
            //BuildLocalizedApplicationBar();

            //Turnoff Ads if not paid for
            if (GTaskSettings.IsFree)
            {
                AdControl.Visibility = System.Windows.Visibility.Visible;
                //TasksView.Margin = new Thickness(0, 0, 0, 80);
            }
            else
            {
                AdControl.Visibility = System.Windows.Visibility.Collapsed;
                //TasksView.Margin = new Thickness(0, 0, 0, 0);
            }

        }

        //Adding conditional pin button depending on if it is pinned yet
        private void AddPinButton()
        {
            var tile = ShellTile.ActiveTiles.FirstOrDefault(o => o.NavigationUri.ToString().Contains(Id));
            ApplicationBarIconButton pinButton;

            //If the tile is found then show Pin button, else Unpin
            if (tile == null)
                pinButton = new ApplicationBarIconButton { Text = "Pin to Start", IconUri = new Uri("/Assets/pin.png", UriKind.Relative) };
            else
            {
                //Update LiveTile if it exists
                Tile_Update("Update");
                pinButton = new ApplicationBarIconButton { Text = "Unpin", IconUri = new Uri("/Assets/unpin.png", UriKind.Relative) };
            }

            pinButton.Click -= Pin_Click;
            pinButton.Click += Pin_Click;      

            //Task Page only has 2 buttons by default, if 3+ then remove them to add the pinButton
            if (ApplicationBar.Buttons.Count > 2) { ApplicationBar.Buttons.RemoveAt(2); }
            ApplicationBar.Buttons.Add(pinButton);

        }

        //If the tile already exists, delete it, else pin it
        private void Pin_Click(object sender, EventArgs eventArgs)
        {
            //Start Progress Indicator
            SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "Updating Live Tile";
            
            // Try to find a Tile that has this page's URI.
            var tile = ShellTile.ActiveTiles.FirstOrDefault(o => o.NavigationUri.ToString().Contains(Id));

            if (tile == null)
            {
                //Create or Update LiveTile
                Tile_Update("Create");
            }
            else
            {
                // A Tile was found, so remove it.
                tile.Delete();
            }

            //Remove the button and add it back again as the new state
            ApplicationBar.Buttons.Remove(sender);
            AddPinButton();

            //Stop Progress Indicator
            SetProgressIndicator(false);
        }

        private void Tile_Update(string Type)
        {
            //No Tile was found, so add one for this page.
            //most displays cut off the 16th charactor, this reduces it to 13 + ... for the Edit page and LiveTile
            string liveTitle = curTitle;
            if (curTitle.Length > 24)
            {
                liveTitle = curTitle.Substring(0, 21) + "...";
            }

            var settings = IsolatedStorageSettings.ApplicationSettings;
            //Conditional LiveTile Updates depending on the settings (past due w/ no due date, past due w/o no due date, total count
            int count = 0;
            if (GTaskSettings.LiveTileCount > 0)
            {
                if (GTaskSettings.IncludeNoDueDate)
                {
                    count = (int)settings["DueCount_" + Id];
                }
                else
                {
                    count = (int)settings["DueNDCount_" + Id];
                }
            }
            else
            {
                count = (int)settings["Count_" + Id];
            }

            var tileData = new StandardTileData { Title = liveTitle, BackgroundImage = new Uri("/Assets/Icons/202.png", UriKind.Relative), Count = count };
            if (Type == "Create")
            {
                //Create the tile
                ShellTile.Create(new Uri(("/Views/TaskView.xaml?Id=" + Id + "&Title=" + curTitle + "&From=Tile"), UriKind.Relative), tileData);
                //When a tile is pinned, Start the PeriodicAgent to update counts every hour
                StartPeriodicAgent();

                //#DEBUG Runs in 1500 milliseconds after pinning a tile if not comment out
                if (Debugger.IsAttached)
                {
                    var taskName = "MyTask";
                    ScheduledActionService.LaunchForTest(taskName, TimeSpan.FromMilliseconds(1500));
                }
            }
            else if (Type == "Update")
            {
                //Update the tile with the latest TileData (e.g. New Title)
                var tile = ShellTile.ActiveTiles.FirstOrDefault(o => o.NavigationUri.ToString().Contains(Id));
                tile.Update(tileData);
            }

           
        }
        private void Check_Id(string Id)
        {
            //Check to see if that Id exists in settings
            //Prevents Users from calling with invalid ID's
            var settings = IsolatedStorageSettings.ApplicationSettings;
            string IdSearch;
            settings.TryGetValue<string>("Title_" + Id, out IdSearch);
            if (string.IsNullOrEmpty(IdSearch))
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
            }
        }
        #region Event Handlers

        private void new_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Edit.xaml?Id=new&List=" + Id + "&Title=" + curTitle, UriKind.RelativeOrAbsolute));
        }

        //If I came from a tile then it takes them back, else takes them back to mainpage.xaml
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            while (NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();
            if (!NavigationService.Source.ToString().Contains("From=Tile"))
            {
                e.Cancel = true;
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
            }
        }


        private void completed_Click(object sender, EventArgs e)
        {
            ClearCompleted();
        }

        public async void ClearCompleted()
        {
            //Start Progress Indicator
            SetProgressIndicator(true);

            SystemTray.ProgressIndicator.Text = "Clearing Completed Tasks";

            //Clear Tasks
            await TaskHelper.ClearCompletedTasks(Id);

            //Stop Progress Indicator
            SetProgressIndicator(false);

            //Refresh screen
            refresh();
        }

        private async void hidden_Click(object sender, EventArgs e)
        {
            if (GTaskSettings.HideCompleted == true)    
            {
                MessageBoxResult result = MessageBox.Show("WARNING: If you have a large number of completed tasks, showing Completed Tasks will slow down the performance of the application.\r\n \r\nPress 'OK' to continue or 'Cancel' to return.", "Slow Performance", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    GTaskSettings.HideCompleted = false;
                }
                else
                {
                    return;
                }
            }
            else
            {
                GTaskSettings.HideCompleted = true;

                //Clear the Hidden from
                bool results = await GTaskSettings.RemoveHidden();
                if (!results)
                {
                    MessageBox.Show("There was an error hiding Completed Tasks, please try again.", "Completed Tasks", MessageBoxButton.OK);
                    GTaskSettings.HideCompleted = false;
                }
            }

            //Refresh screen
            refresh(true, true);
        }

        //Calls refresh to refresh page
        private void refresh_Click(object sender, EventArgs e)
        {
            refresh(true);
        }

        /// <summary>
        /// Refresh the data on the page
        /// </summary>
        /// <param name="refreshLocalStorage">Refresh local storage</param>
        public async void refresh(bool refresh = false, bool hidden = false)
        {

            //Start Progress Indicator
            SetProgressIndicator(true);

            if (refresh)
            {
                SystemTray.ProgressIndicator.Text = "Syncing with Google";
            }
            else
            {
                SystemTray.ProgressIndicator.Text = "Getting Data";
            }

            //Check to see if the Id exists.
            Check_Id(Id);

            // Get the settings
            var settings = IsolatedStorageSettings.ApplicationSettings;

            // Check to see if the list currently being viewed was added
            if (settings.Contains("List_" + Id + "_Action") && settings["List_" + Id + "_Action"].ToString() == "added")
            {
                settings["GetNewListId"] = Id;
            }

            //If hidden = true then we need to reload all the data, else we don't
            if (hidden == false)
            {
                await GTaskSettings.RefreshData(true, Id, refresh);
            }
            else
            {
                await GTaskSettings.RefreshData(true, null, refresh);
            }

            if (refresh)
            {
                SystemTray.ProgressIndicator.Text = "Data Synced, Finishing up";
            }
            

            // Update the ID value
            if (settings.Contains("GetNewListId"))
            {
                Id = settings["GetNewListId"].ToString();

                // Remove the setting
                settings.Remove("GetNewListId");
            }

            //Load the Tasks
            await App.TaskViewModel.LoadData(Id);

            //Set DataContext to TaskList(s)
            DataContext = App.TaskViewModel;

            //Update Title
            SetTitle();

            //Stop Progress Indicator
            SetProgressIndicator(false);

            //Build menu bar
            BuildLocalizedApplicationBar();
        }

        //Sets the Title
        public void SetTitle()
        {
            //If ID exists, pull the Title from Storage - else get it from the Redirect URL
            //This catches the case that it was updated between TaskList and TaskView
            var settings = IsolatedStorageSettings.ApplicationSettings;
            if (NavigationContext.QueryString.ContainsKey("Id"))
            {
                curTitle = (string)settings["Title_" + Id];
            }
            else if (!NavigationContext.QueryString.ContainsKey("Title"))
            {
                curTitle = "Task List";//to catch when people use URI and it isn't in Settings
            }
            else
            {
                curTitle = NavigationContext.QueryString["Title"];
            }

            //most displays cut off the 16th charactor, this reduces it to 13 + ... for the Task page
            if (curTitle.Length > 15)
            {
                txtPageTitle.Text = curTitle.Substring(0, 13) + "...";
            }
            else
            {
                txtPageTitle.Text = curTitle;
            }
        }

        //Asks user if they want it deleted, if so it deletes and then refreshes page
        private async void delete_Click(object sender, EventArgs eventArgs)
        {
            MessageBoxResult m = MessageBox.Show("This Task will be permanently deleted.", "Delete Task", MessageBoxButton.OKCancel);
            if (m == MessageBoxResult.OK)
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;

                //Start Progress Indicator
                SetProgressIndicator(true);
                SystemTray.ProgressIndicator.Text = "Deleting Task";

                //Delete Item based on ListID and TaskID
                var currentItem = sender as MenuItem;
                var _currentID = currentItem.Tag.ToString();

                //Wait for the item to be deleted
                bool results = await TaskHelper.DeleteTask(Id, _currentID);

                // Check the results
                if (!results)
                {
                    // Add a setting to indicate the action
                    // Checking to see if Action already exists (e.g. Added), if not -> add 'deleted'
                    // catches scenario where if it is created and deleted offline
                    if (!settings.Contains("Task_" + _currentID + "_Action"))
                    {
                        settings.Add("Task_" + _currentID + "_Action", "deleted");
                        settings.Add("Task_" + _currentID + "_Timestamp", DateTime.UtcNow.ToString());
                    }
                    else
                    {
                        settings.Remove("Task_" + _currentID + "_Action");
                        settings.Remove("Task_" + _currentID + "_Timestamp");
                    }

                }

                //Delete from local storage regardless of result - sync will catch it up
                // Get the list from local storage
                //if (settings.Contains("Task_" + _currentID + "_Action"))
                //{
                //    settings.Remove("Task_" + _currentID + "_Action");
                //    settings.Remove("Task_" + _currentID + "_Timestamp");
                //}

                List<TaskListItem> lists = await TaskListHelper.GetTaskListFromApplicationStorage();

                // Remove the task
                foreach (TaskListItem list in lists)
                {
                    if (list.id == Id)
                    {
                        list.taskList.RemoveAll(x => x.id == _currentID);
                        break;
                    }
                }

                // Resubmit the list to local storage
                await TaskListHelper.SubmitToLocalStorage(lists);
                //}


                //Stop Progress Indicator
                SetProgressIndicator(false);

                refresh();
            }
        }


        #endregion

        //Checks to see if it should be checked or not
        private void CheckBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null)
                return;
        }

        //When you select a text box it takes the ID (currentItem.Tag) and sends you to the Edit.xaml page for editing
        private void Textblock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var currentItem = e.OriginalSource as TextBlock;
            if (currentItem != null)
            {
                NavigationService.Navigate(new Uri(("/Views/Edit.xaml?Id=" + currentItem.Tag + "&List=" + Id), UriKind.RelativeOrAbsolute));
            }
        }

        //Updates the status of the tast
        private async void CheckBox_Tap(object sender, RoutedEventArgs e)
        {
            //Hide the content panel to prevent additional checks
            //TasksView.IsEnabled = false;

            //Start Progress Indicator
            SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "Updating Task";

            var currentItem = e.OriginalSource as CheckBox;
            if (currentItem != null)
            {
                var parentListId = ((TaskViewModel)DataContext).ParentList.id;
                var currentId = currentItem.Tag.ToString();

                var s = from p in App.TaskViewModel.TaskItem
                        where p.id == currentId
                        select p;
                    
                var taskItem = s.First();

                string due = null;
                if (taskItem.due != null)
                {
                    due = Convert.ToDateTime(Universal.ConvertToUniversalDate(taskItem.due)).ToString("yyyy-MM-dd'T'hh:mm:ss.00Z");

                    //due = taskItem.due.ToString("yyyy-MM-dd'T'hh:mm:ss.00Z");// Convert.ToDateTime(taskItem.due).ToString("yyyy-MM-dd'T'hh:mm:ss.00Z");
                }

                bool isChecked = currentItem.IsChecked != null && (bool)currentItem.IsChecked;
                bool results = await TaskHelper.UpdateTaskStatus(parentListId, currentId, due, isChecked);

                List<Model.TaskListItem> localStorageList = await TaskListHelper.GetTaskListFromApplicationStorage(false);

                // Get the specific task
                if (isChecked)
                {
                    localStorageList.First(x => x.id == parentListId).taskList.First(x => x.id == currentId).status = "completed";
                    localStorageList.First(x => x.id == parentListId).taskList.First(x => x.id == currentId).completed = DateTime.UtcNow.ToString("yyyy-MM-dd'T'hh:mm:ss.00Z"); 
                }
                else
                {
                    localStorageList.First(x => x.id == parentListId).taskList.First(x => x.id == currentId).status = "needsAction";
                    localStorageList.First(x => x.id == parentListId).taskList.First(x => x.id == currentId).completed = null;
                }

                // Update the "updated" field to the current time if it was not updated online
                if (!results)
                {
                    localStorageList.First(x => x.id == parentListId).taskList.First(x => x.id == currentId).updated = DateTime.UtcNow.ToString("yyyy-MM-dd'T'hh:mm:ss.00Z"); // DateTime.UtcNow.ToString();
                }

                await TaskListHelper.SubmitToLocalStorage(localStorageList);

                refresh();

                //if (GTaskSettings.AutoClear == true && isChecked == true)
                //{
                //    ClearCompleted();
                //}
            }

            SetProgressIndicator(false);

            //show the content panel to allow checks
            //TasksView.IsEnabled = true;
        }

        #region Scheduler
        private void StartPeriodicAgent()
        {
            var taskName = "MyTask";

            // Obtain a reference to the period task, if one exists
            var oldTask = ScheduledActionService.Find(taskName) as PeriodicTask;

            // If the task already exists and background agents are enabled for the
            // application, you must remove the task and then add it again to update 
            // the schedule
            if (oldTask != null)
            {
                RemoveAgent(taskName);
            }

            PeriodicTask task = new PeriodicTask(taskName);

            // The description is required for periodic agents. This is the string that the user
            // will see in the background services Settings page on the device.
            task.Description = "Updates LiveTile Counts for Pinned Task Lists.";

            // Place the call to Add in a try block in case the user has disabled agents.
            try
            {
                ScheduledActionService.Add(task);     
            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("BNS Error: The action is disabled"))
                {
                    MessageBox.Show("Background agents for this application have been disabled by the user.");
                }

                if (exception.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added."))
                {
                    // No user action required. The system prompts the user when the hard limit of periodic tasks has been reached.
                }
            }
            catch (SchedulerServiceException)
            {
                // No user action required.
            }
        }
        private void RemoveAgent(string name)
        {
            try
            {
                ScheduledActionService.Remove(name);
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region Move

        private void FinishMove(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            try
            {
                //Get parentListId
                var parentListId = ((TaskViewModel)DataContext).ParentList.id;

                //Check that atleast the parentListID and MovedId exist
                if (parentListId != "" && GTaskSettings.MovedId != "")
                {
                    //Make the call
                    MakeMove(parentListId, GTaskSettings.MovedId, GTaskSettings.PrevId);

                    //Reset Variables
                    GTaskSettings.MovedId = string.Empty;
                    GTaskSettings.PrevId = string.Empty;
                }
            }
            catch(Exception)
            {

            }
        }


        private async void MakeMove(string parentListId, string MovedId, string PrevId)
        {
            try
            {
                //Hide the content panel to prevent additional checks
                TasksView.IsEnabled = false;

                //Start Progress Indicator
                SetProgressIndicator(true);
                SystemTray.ProgressIndicator.Text = "Moving Task";
                string results = await TaskHelper.MoveTask(parentListId, MovedId, PrevId);

                SetProgressIndicator(false);

                //Refresh screen
                refresh(!string.IsNullOrEmpty(results));

                //show the content panel to allow checks
                TasksView.IsEnabled = true;

            }
            catch (Exception)
            {

            }
        }

        #endregion

        private void AdControl_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Ad Error : ({0}) {1}", e.ErrorCode, e.Error);
        }
    }
}