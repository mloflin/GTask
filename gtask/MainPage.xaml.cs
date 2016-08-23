using gTask.Model;
using gTask.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;

namespace gTask
{
    public partial class MainPage : PhoneApplicationPage
    {
        //App thisApp = Application.Current as App;
        private string _currentId = String.Empty;
        private string _currentTitle = String.Empty;

        private void AdControl_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Ad Error : ({0}) {1}", e.ErrorCode, e.Error);
        }  

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            if (GTaskSettings.IsLoggedIn())
            {
                //Check to see if Reminder should be shown
                LoginHelper.Reminder();
                
                //Create Menu Bar
                BuildLocalizedApplicationBar();

                //hide login button
                btnLogin.Visibility = System.Windows.Visibility.Collapsed;

                //show TaskListBox
                TaskListBox.Visibility = System.Windows.Visibility.Visible;

                //add logout menu item
                ApplicationBarMenuItem appBarLogoutMenuItem = new ApplicationBarMenuItem(AppResources.AppBarLogoutMenuText);
                ApplicationBar.MenuItems.Add(appBarLogoutMenuItem);
                appBarLogoutMenuItem.Click += new EventHandler(Logout_Click);
            }
            else
            {
                //Set/Reset App Variables
                LoginHelper.SetVariables();

                //hide TaskListbox
                TaskListBox.Visibility = System.Windows.Visibility.Collapsed;
            }

            //Set Title
            txtTitle.Text = GTaskSettings.ApplicationTitle;

            //Turnoff Ads if not paid for
            if (GTaskSettings.IsFree)
            {
                AdControl.Visibility = System.Windows.Visibility.Visible;
                TaskListBox.Margin = new Thickness(0, 0, 0, 80);
            }
            else
            {
                AdControl.Visibility = System.Windows.Visibility.Collapsed;
                TaskListBox.Margin = new Thickness(0, 0, 0, 0);
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();

            //Get Data if logged in
            if (GTaskSettings.IsLoggedIn())
            { 
                if (GTaskSettings.AutoRefreshLists)
                {
                    refresh(true,true);
                }
                else
                {
                    refresh();
                }
            }

        }

        private static void SetProgressIndicator(bool isVisible)
        {
            SystemTray.ProgressIndicator.IsIndeterminate = isVisible;
            SystemTray.ProgressIndicator.IsVisible = isVisible;
        }

        #region Menu
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            //Add button
            ApplicationBarIconButton appBarNewListButton = new ApplicationBarIconButton(new Uri("/Assets/add.png", UriKind.Relative));
            appBarNewListButton.Text = AppResources.AppBarAddListButtonText;
            ApplicationBar.Buttons.Add(appBarNewListButton);
            appBarNewListButton.Click += new EventHandler(new_Click);

            //Refresh button
            ApplicationBarIconButton appBarRefreshButton = new ApplicationBarIconButton(new Uri("/Assets/sync.png", UriKind.Relative));
            appBarRefreshButton.Text = AppResources.AppBarRefreshButtonText;
            ApplicationBar.Buttons.Add(appBarRefreshButton);
            appBarRefreshButton.Click += new EventHandler(refresh_Click);

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

        //Add new task list
        void new_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/EditList.xaml?Id=new", UriKind.RelativeOrAbsolute));
        }

        //launch google login page
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Login.xaml", UriKind.RelativeOrAbsolute));
        }

        //dynamic controls based on logged in/out
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            
        }

        //Visits Google Logout page, clears the local memory, and then redirects to Mainpage
        void Logout_Click(object sender, EventArgs e)
        {
            MessageBoxResult m = MessageBox.Show("This will Log you out of Google and remove any personal settings and LiveTiles.", "Logout", MessageBoxButton.OKCancel);
            if (m == MessageBoxResult.OK)
            {
                this.NavigationService.Navigate(new Uri("/Login.xaml", UriKind.Relative));
            }
        }

        //calls LoadData which is in MainViewModel which then calls TaskListHelper
        void refresh_Click(object sender, EventArgs e)
        {
            refresh(true, true);
        }

        /// <summary>
        /// Refresh the data on the page.
        /// </summary>
        /// <param name="alertWhenNoConnection">Pass a true value to alert the user when no connection
        /// to the API is available</param>
        public async void refresh(bool alertWhenNoConnection = false, bool refresh = false)
        {
            SetProgressIndicator(true);
            if (refresh || GTaskSettings.FirstRun)
            {
                
                SystemTray.ProgressIndicator.Text = "Syncing with Google";
            }
            else
            {
                SystemTray.ProgressIndicator.Text = "Getting Data";
            }
            

            try
            {
                //Load the TaskLists
                // Pass a true value to alert the user that we werent able to get a connection
                // since they specifically requested a refresh of the data
                await App.MainViewModel.LoadData(alertWhenNoConnection, refresh);

                //Set DataContext to TaskList(s)
                DataContext = App.MainViewModel;
            }
            catch (Exception) //e)
            {
                if (GTaskSettings.MsgError)
                {
                    MessageBox.Show("There was an error getting the Task List(s). Please Try Again.");
                }
                
            }

            SetProgressIndicator(false);
            
        }

        //Edit Task List
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var currentItem = sender as MenuItem;
            _currentId = currentItem.Tag.ToString();
            _currentTitle = currentItem.Name.ToString();
            if (currentItem != null)
                NavigationService.Navigate(new Uri("/Views/EditList.xaml?Id=" + _currentId, UriKind.RelativeOrAbsolute));
        }

        //Delete Task List
        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult m = MessageBox.Show("This Task List and Tasks will be permanently deleted.", "Delete Task List", MessageBoxButton.OKCancel);
            if (m == MessageBoxResult.OK)
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                
                //Start Progress Indicator
                SetProgressIndicator(true);
                SystemTray.ProgressIndicator.Text = "Deleting Task List";

                //Get the ID of the TaskList 
                var currentItem = sender as MenuItem;
                _currentId = currentItem.Tag.ToString();

                //Wait for the item to be deleted
                bool results = await TaskListHelper.DeleteList(_currentId);

                //Remove any Stored Variables
                if (settings.Contains("Count_" + _currentId))
                    settings.Remove("Count_" + _currentId);
                if (settings.Contains("DueCount_" + _currentId))
                    settings.Remove("DueCount_" + _currentId);
                if (settings.Contains("DueNDCount_" + _currentId))
                    settings.Remove("DueNDCount_" + _currentId);
                if (settings.Contains("Title_" + _currentId))
                    settings.Remove("Title_" + _currentId);

                // If No results add queue item to delete currentID
                if (!results)
                {
                    // Add a setting to indicate the action
                    // Checking to see if Action already exists (e.g. Added), if not -> add 'deleted'
                    // catches scenario where if it is created and deleted offline
                    if (!settings.Contains("List_" + _currentId + "_Action"))
                    {
                        // Add a setting to indicate the action
                        settings.Add("List_" + _currentId + "_Action", "deleted");
                        settings.Add("List_" + _currentId + "_Timestamp", DateTime.UtcNow.ToString());
                    }
                    else
                    {
                        settings.Remove("List_" + _currentId + "_Action");
                        settings.Remove("List_" + _currentId + "_Timestamp");
                    }
                }

                //Delete from local storage regardless of result - sync will catch it up
                // Get the list from local storage
                //if (settings.Contains("List_" + _currentId + "_Action"))
                //{
                //    settings.Remove("List_" + _currentId + "_Action");
                //    settings.Remove("List_" + _currentId + "_Timestamp");
                //}

                // Get the list from local storage
                List<TaskListItem> lists = await TaskListHelper.GetTaskListFromApplicationStorage();

                // Remove the item
                lists.RemoveAll(x => x.id == _currentId);

                // Resubmit the list to local storage
                await TaskListHelper.SubmitToLocalStorage(lists);
                

                //Stop Progress Indicator
                SetProgressIndicator(false);

                //Refresh screen
                refresh();
            }
        }

        //Opens the task list
        private void TaskList_Click(object sender, RoutedEventArgs e)
        {
            var currentItem = sender as Button;
            if (currentItem != null)
            {
                _currentId = currentItem.Tag.ToString();
                _currentTitle = currentItem.Content.ToString();
                NavigationService.Navigate(new Uri("/Views/TaskView.xaml?Id=" + _currentId + "&Title=" + _currentTitle, UriKind.RelativeOrAbsolute));
            }
        }

        //Can't go back anymore
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            while (NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();
        }

        private void CheckBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null)
                return;
        }

        #endregion


    }
}