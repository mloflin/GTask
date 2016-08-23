using gTask.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace gTask
{
    public partial class Settings : PhoneApplicationPage
    {
        private string callingPage;
        public Settings()
        {
            InitializeComponent();

            var settings = IsolatedStorageSettings.ApplicationSettings;
            //Create List for Task List Sort
            List<TaskListSort> tls = new List<TaskListSort>();
            tls.Add(new TaskListSort() { Name = "Google Sort" });
            tls.Add(new TaskListSort() { Name = "Alphabetical" });
            this.ddlTaskListSort.ItemsSource = tls;

            //Create List for Task Sort
            List<TaskSort> ts = new List<TaskSort>();
            ts.Add(new TaskSort() { Name = "Google Sort (Drag & Drop)" });
            ts.Add(new TaskSort() { Name = "Due Date" });
            ts.Add(new TaskSort() { Name = "Alphabetical" });
            this.ddlTaskSort.ItemsSource = ts;

            //Create List for LiveTile Count
            List<LiveTileCount> ltc = new List<LiveTileCount>();
            ltc.Add(new LiveTileCount() { Name = "Total Uncompleted Tasks" });
            ltc.Add(new LiveTileCount() { Name = "Due Uncompleted Tasks" });
            this.ddlLiveTileCount.ItemsSource = ltc;

            //Create List for REminder
            List<Reminders> dr = new List<Reminders>();
            dr.Add(new Reminders() { Name = "Today's Date" });
            dr.Add(new Reminders() { Name = "Tomorrow's Date" });
            dr.Add(new Reminders() { Name = "No Reminder" });
            this.ddlDefaultReminder.ItemsSource = dr;

            //Create List for Task Edit Text Size
            List<TextSize> tets = new List<TextSize>();
            tets.Add(new TextSize() { Name = "24px [2,500] (Default)" });
            tets.Add(new TextSize() { Name = "20px [4,200]" });
            tets.Add(new TextSize() { Name = "16px [6,800]" });
            tets.Add(new TextSize() { Name = "12px [8,200]" });
            this.ddlTaskEditTextSize.ItemsSource = tets;

            //If TaskListSort, TaskSort, and NoDueDateAtTop exist in Settings, Set them when the page loads
            if (settings.Contains("TaskListSort"))
            {
                ddlTaskListSort.SelectedIndex = GTaskSettings.TaskListSort;
            }
            if (settings.Contains("TaskSort"))
            {
                ddlTaskSort.SelectedIndex = GTaskSettings.TaskSort; //Automatically calls checkDateSort when index changed
            }
            if (settings.Contains("DisableDragDrop"))
            {
                chkDisableDragDrop.IsChecked = (bool)(GTaskSettings.DisableDragDrop);
            }
            if (settings.Contains("NoDueDateAtTop"))
            {
                chkNoDueDateAtTop.IsChecked = (bool)(GTaskSettings.NoDueDateAtTop);
            }
            if (settings.Contains("LiveTileCount"))
            {
                ddlLiveTileCount.SelectedIndex = GTaskSettings.LiveTileCount;
            }
            if (settings.Contains("IncludeNoDueDate"))
            {
                chkIncludeNoDueDate.IsChecked = (bool)(GTaskSettings.IncludeNoDueDate);
            }
            if (settings.Contains("DefaultReminder"))
            {
                ddlDefaultReminder.SelectedIndex = GTaskSettings.DefaultReminder;
                if (ddlDefaultReminder.SelectedIndex == 0)
                {
                    chkTomorrowIf.Visibility = System.Windows.Visibility.Visible;
                    if (settings.Contains("DefaultReminderTomorrowIf"))
                        chkTomorrowIf.IsChecked = GTaskSettings.DefaultReminderTomorrowIf;
                    lstTime.Visibility = System.Windows.Visibility.Visible;
                    if (settings.Contains("DefaultReminderTomorrowIfTime"))
                        lstTime.Value = GTaskSettings.DefaultReminderTomorrowIfTime;
                }
            }
            if (settings.Contains("HideNotes"))
            {
                chkHideNotes.IsChecked = (bool)(GTaskSettings.HideNotes);
            }
            if (settings.Contains("HideDueDate"))
            {
                chkHideDueDate.IsChecked = (bool)(GTaskSettings.HideDueDate);
            }
            if (settings.Contains("AutoClear"))
            {
                chkAutoClear.IsChecked = (bool)(GTaskSettings.AutoClear);
            }
            if (settings.Contains("AutoRefreshLists"))
            {
                chkAutoRefreshList.IsChecked = (bool)(GTaskSettings.AutoRefreshLists);
            }
            if (settings.Contains("AutoRefreshTasks"))
            {
                chkAutoRefreshTasks.IsChecked = (bool)(GTaskSettings.AutoRefreshTasks);
            }
            if (settings.Contains("TextSize"))
            {
                ddlTaskEditTextSize.SelectedIndex = GTaskSettings.TextSize;
            }

                chkCreateTaskList.IsChecked = (bool)(GTaskSettings.MsgCreateTaskList);
                chkUpdateTaskList.IsChecked = (bool)(GTaskSettings.MsgUpdateTaskList);
                chkCreateTask.IsChecked = (bool)(GTaskSettings.MsgCreateTask);
                chkUpdateTask.IsChecked = (bool)(GTaskSettings.MsgUpdateTask);
                chkSavedSettings.IsChecked = (bool)(GTaskSettings.MsgSavedSettings);
                //chkError.IsChecked = (bool)(GTaskSettings.MsgError);

            //Create Menu Bar
            BuildLocalizedApplicationBar();

            //Set Version
            lblVersion.Text = "Version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            //Set about block
            lblAppTitle.Text = "The " + GTaskSettings.ApplicationTitle + " App is a mobile version of the Google Tasks add on in Gmail that I created to enable quick and simple tracking of my day-to-day tasks.";

            //Set btnRate Title
            btnRate.Content = "Rate " + GTaskSettings.ApplicationTitle;

            //Hide buy button if paid for
            if (GTaskSettings.IsFree)
            {
                btnBuy.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                btnBuy.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        public class TaskListSort
        {
            public string Name
            {
                get;
                set;
            }
        }
        public class TaskSort
        {
            public string Name
            {
                get;
                set;
            }
        }
        public class LiveTileCount
        {
            public string Name
            {
                get;
                set;
            }
        }
        public class TextSize
        {
            public string Name
            {
                get;
                set;
            }
        }
        
        public class Reminders
        {
            public string Name
            {
                get;
                set;
            }
        }

        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            //Save button
            ApplicationBarIconButton appBarNewButton = new ApplicationBarIconButton(new Uri("/Assets/save.png", UriKind.Relative));
            appBarNewButton.Text = AppResources.AppBarSaveButtonText;
            ApplicationBar.Buttons.Add(appBarNewButton);
            appBarNewButton.Click += new EventHandler(save_Click);

            //Cancel button
            ApplicationBarIconButton appBarCancelButton = new ApplicationBarIconButton(new Uri("/Assets/cancel.png", UriKind.Relative));
            appBarCancelButton.Text = AppResources.AppBarCancelButtonText;
            ApplicationBar.Buttons.Add(appBarCancelButton);
            appBarCancelButton.Click += new EventHandler(cancel_Click);
        }

        //Returns to MainPage
        void cancel_Click(object sender, EventArgs eventArgs)
        {
            //Return to the last page
            NavigationService.GoBack();
        }

        void save_Click(object sender, EventArgs e)
        {
            //Save the drop down values to App Settings
            GTaskSettings.TaskListSort = (int)(ddlTaskListSort.SelectedIndex);
            GTaskSettings.TaskSort = (int)(ddlTaskSort.SelectedIndex);
            GTaskSettings.DisableDragDrop = (bool)(chkDisableDragDrop.IsChecked);
            GTaskSettings.NoDueDateAtTop = (bool)(chkNoDueDateAtTop.IsChecked);
            GTaskSettings.LiveTileCount = (int)(ddlLiveTileCount.SelectedIndex);
            GTaskSettings.IncludeNoDueDate = (bool)(chkIncludeNoDueDate.IsChecked);
            GTaskSettings.DefaultReminder = (int)(ddlDefaultReminder.SelectedIndex);
            GTaskSettings.DefaultReminderTomorrowIf = (bool)(chkTomorrowIf.IsChecked);
            GTaskSettings.DefaultReminderTomorrowIfTime = (DateTime)(lstTime.Value);
            GTaskSettings.HideNotes = (bool)(chkHideNotes.IsChecked);
            GTaskSettings.HideDueDate = (bool)(chkHideDueDate.IsChecked);
            GTaskSettings.AutoClear = (bool)(chkAutoClear.IsChecked);
            GTaskSettings.AutoRefreshLists = (bool)(chkAutoRefreshList.IsChecked);
            GTaskSettings.AutoRefreshTasks = (bool)(chkAutoRefreshTasks.IsChecked);
            GTaskSettings.TextSize = (int)(ddlTaskEditTextSize.SelectedIndex);

            GTaskSettings.MsgCreateTaskList = (bool)(chkCreateTaskList.IsChecked);
            GTaskSettings.MsgUpdateTaskList = (bool)(chkUpdateTaskList.IsChecked);
            GTaskSettings.MsgCreateTask = (bool)(chkCreateTask.IsChecked);
            GTaskSettings.MsgUpdateTask = (bool)(chkUpdateTask.IsChecked);
            GTaskSettings.MsgSavedSettings = (bool)(chkSavedSettings.IsChecked);
            //GTaskSettings.MsgError = (bool)(chkError.IsChecked);

            if (GTaskSettings.MsgSavedSettings)
            {
                MessageBoxResult n = MessageBox.Show("Complete.", "Update Settings", MessageBoxButton.OK);
            }

            //Return to the last page
            NavigationService.GoBack();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (GTaskSettings.IsFree)
            {
                btnBuy.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                btnBuy.Visibility = System.Windows.Visibility.Collapsed;
            }

            btnRate.Content = "Rate " + GTaskSettings.ApplicationTitle;
                            
            //Auto Select which Pivot to start on based on the page the use is coming from
            if (NavigationContext.QueryString.ContainsKey("p"))
            {
                callingPage = NavigationContext.QueryString["p"];
                if (callingPage == "Task_View")
                {
                    SettingsPivot.SelectedIndex = 1;
                }
                else if (callingPage == "Task_Edit")
                {
                    SettingsPivot.SelectedIndex = 2;
                }
                else
                {
                    SettingsPivot.SelectedIndex = 0;
                }
            }
        }

        private void btnRate_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask oRateTask = new MarketplaceReviewTask();
            oRateTask.Show();
        }


        private void btnBuy_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceDetailTask _marketPlaceDetailTask = new MarketplaceDetailTask();
            _marketPlaceDetailTask.ContentIdentifier = "b4a8de82-cd83-40b0-b2e6-31eb95a0100f"; //GTASK+ Link
            _marketPlaceDetailTask.Show();
        }

        private async void btnTweet_Click(object sender, RoutedEventArgs e)
        {
            // The URI to launch
            string uriToLaunch = @"twitter://";
            var uri = new Uri(uriToLaunch);

            // Launch the URI
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);

            if (success)
            {
                // URI launched
            }
            else
            {
                // URI launch failed
                await Windows.System.Launcher.LaunchUriAsync(new Uri("https://twitter.com/intent/tweet?text=@MattLoflin"));
            }
        }

        private void btnEmail_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.Subject = GTaskSettings.ApplicationTitle + " Feedback";
            emailComposeTask.To = "MLoflin.Apps@gmail.com";
            emailComposeTask.Show();
        }

        

        #region CheckBoxes
        private void ddlTaskSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkTaskSortOptions();
        }

        private void ddlLiveTileCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkLiveTileCount();
        }

        //Checks to see if the TaskSort is a "Date" oriented sort, if so show the NoDueDateAtTop Checkbox
        private void checkTaskSortOptions()
        {
            //If Google Sort (Drag & Drop) then allow user to disable Drag & Drop
            if (ddlTaskSort.SelectedIndex == 0)
            {
                chkDisableDragDrop.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                chkDisableDragDrop.Visibility = System.Windows.Visibility.Collapsed;
            }

            //If date sort give option to show No Due Date items at the top
            if (ddlTaskSort.SelectedIndex == 1) //Date Sort
            {
                chkNoDueDateAtTop.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                chkNoDueDateAtTop.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        //Checks to see if the TaskSort is a "Date" oriented sort, if so show the NoDueDateAtTop Checkbox
        private void checkLiveTileCount()
        {

            if (ddlLiveTileCount.SelectedIndex > 0) //Past Due = 1
            {
                chkIncludeNoDueDate.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                chkIncludeNoDueDate.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void ddlDefaultReminder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddlDefaultReminder.SelectedIndex == 0)
            {
                chkTomorrowIf.Visibility = System.Windows.Visibility.Visible;
                lstTime.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                chkTomorrowIf.Visibility = System.Windows.Visibility.Collapsed;
                lstTime.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public void chkAutoRefreshLists_Click(object sender, RoutedEventArgs e)
        {
            if (GTaskSettings.IsFree)
            {
                GTaskSettings.Upsell();
                chkAutoRefreshList.IsChecked = false;
            }
        }


        public void chkAutoRefreshTasks_Click(object sender, RoutedEventArgs e)
        {
            if (GTaskSettings.IsFree)
            {
                GTaskSettings.Upsell();
                chkAutoRefreshTasks.IsChecked = false;
            }
                
        }

        public void chkAutoClear_Click(object sender, RoutedEventArgs e)
        {
            if (GTaskSettings.IsFree)
            {
                GTaskSettings.Upsell();
                chkAutoClear.IsChecked = false;
            }

        }
        #endregion
    }

}