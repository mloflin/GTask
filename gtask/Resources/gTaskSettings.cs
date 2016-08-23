using gTask.Model;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace gTask.Resources
{
    public class GTaskSettings
    {
        #region Public Properties

        public static bool IsFree
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("IsFree") ? true : (bool)(settings["IsFree"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["IsFree"] = value;
            }
        }
        public static bool Rated
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("Rated") ? false : (bool)(settings["Rated"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["Rated"] = value;
            }
        }
        public static int NewFeatureCount
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return (int)(!settings.Contains("NewFeatureCount") ? 0 : settings["NewFeatureCount"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["NewFeatureCount"] = value;
            }
        }

        public static bool FirstRun
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("FirstRun") ? true : (bool)(settings["FirstRun"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["FirstRun"] = value;
            }
        }
        public static bool HideNotes
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("HideNotes") ? false : (bool)(settings["HideNotes"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["HideNotes"] = value;
            }
        }
        public static bool HideDueDate
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("HideDueDate") ? false : (bool)(settings["HideDueDate"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["HideDueDate"] = value;
            }
        }

        public static bool HideCompleted
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("HideCompleted") ? true : (bool)(settings["HideCompleted"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["HideCompleted"] = value;
            }
        }

        public static bool AutoClear
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("AutoClear") ? false : (bool)(settings["AutoClear"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["AutoClear"] = value;
            }
        }
        public static bool AutoRefreshLists
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("AutoRefreshLists") ? false : (bool)(settings["AutoRefreshLists"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["AutoRefreshLists"] = value;
            }
        }
        public static bool AutoRefreshTasks
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("AutoRefreshTasks") ? false : (bool)(settings["AutoRefreshTasks"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["AutoRefreshTasks"] = value;
            }
        }

        public static string OAuthUrl
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("OAuthUrl") ? string.Empty : settings["OAuthUrl"].ToString();
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["OAuthUrl"] = value;
            }
        }
        public static string OAuthTokenUrl
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("OAuthTokenUrl") ? string.Empty : settings["OAuthTokenUrl"].ToString();
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["OAuthTokenUrl"] = value;
            }
        }
        public static string Scope
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("Scope") ? string.Empty : settings["Scope"].ToString();
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["Scope"] = value;
            }
        }
        public static string LogOutURL
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("LogOutURL") ? string.Empty : settings["LogOutURL"].ToString();
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["LogOutURL"] = value;
            }
        }
        public static string ClientId
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("ClientId") ? string.Empty : settings["ClientId"].ToString();
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["ClientId"] = value;
            }
        }
        public static string RedirectUri
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("RedirectUri") ? string.Empty : settings["RedirectUri"].ToString();
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["RedirectUri"] = value;
            }
        }
        public static string ClientSecret
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("ClientSecret") ? string.Empty : settings["ClientSecret"].ToString();
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["ClientSecret"] = value;
            }
        }
        public static int RequestTimeout
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["RequestTimeout"] = 3000;
                return (int)(settings["RequestTimeout"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["RequestTimeout"] = value;
            }
        }
        public static DateTime ReminderDate
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("ReminderDate") ? DateTime.MaxValue : (DateTime)settings["ReminderDate"];
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["ReminderDate"] = value;
            }
        }
        public static int TextSize
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return (int)(!settings.Contains("TextSize") ? 0 : settings["TextSize"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["TextSize"] = value;
            }
        }
        public static string ApplicationTitle
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("ApplicationTitle") ? string.Empty : settings["ApplicationTitle"].ToString();
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["ApplicationTitle"] = value;
            }
        }
        public static int TaskListSort
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return (int)(!settings.Contains("TaskListSort") ? 0 : settings["TaskListSort"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["TaskListSort"] = value;
            }
        }
        public static int TaskSort
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return (int)(!settings.Contains("TaskSort") ? 0 : settings["TaskSort"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["TaskSort"] = value;
            }
        }
        public static bool DisableDragDrop
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("DisableDragDrop") ? false : (bool)(settings["DisableDragDrop"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["DisableDragDrop"] = value;
            }
        }
        public static bool NoDueDateAtTop
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("NoDueDateAtTop") ? false : (bool)(settings["NoDueDateAtTop"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["NoDueDateAtTop"] = value;
            }
        }
        public static int LiveTileCount
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return (int)(!settings.Contains("LiveTileCount") ? 1 : settings["LiveTileCount"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["LiveTileCount"] = value;
            }
        }
        public static bool IncludeNoDueDate
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("IncludeNoDueDate") ? false : (bool)(settings["IncludeNoDueDate"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["IncludeNoDueDate"] = value;
            }
        }
        public static int DefaultReminder
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return (int)(!settings.Contains("DefaultReminder") ? 0 : settings["DefaultReminder"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["DefaultReminder"] = value;
            }
        }
        public static bool DefaultReminderTomorrowIf
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("DefaultReminderTomorrowIf") ? false : (bool)(settings["DefaultReminderTomorrowIf"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["DefaultReminderTomorrowIf"] = value;
            }
        }
        public static DateTime DefaultReminderTomorrowIfTime
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("DefaultReminderTomorrowIfTime") ? DateTime.Parse("19:00:00") : (DateTime)settings["DefaultReminderTomorrowIfTime"];
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["DefaultReminderTomorrowIfTime"] = value;
            }
        }
        public static int ExpiresIn
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return (int)(!settings.Contains("ExpiresIn") ? 0 : settings["ExpiresIn"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["ExpiresIn"] = value;
            }
        }
        public static string TokenType
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("TokenType") ? string.Empty : settings["TokenType"].ToString();
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["TokenType"] = value;
            }
        }
        public static string AccessToken
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("AccessToken") ? string.Empty : settings["AccessToken"].ToString();
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["AccessToken"] = value;
                Timestamp = TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
            }
        }
        public static double Timestamp
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return (double)(!settings.Contains("Timestamp") ? 0 : settings["Timestamp"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["Timestamp"] = value;
            }
        }
        public static string RefreshToken
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("RefreshToken") ? string.Empty : settings["RefreshToken"].ToString();
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["RefreshToken"] = value;
            }
        }
        public static string MovedId
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("MovedId") ? string.Empty : settings["MovedId"].ToString();
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["MovedId"] = value;
            }
        }
        public static string PrevId
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("PrevId") ? string.Empty : settings["PrevId"].ToString();
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["PrevId"] = value;
            }
        }
        public static bool MsgError
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("MsgError") ? true : (bool)(settings["MsgError"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["MsgError"] = value;
            }
        }
        public static bool MsgSavedSettings
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("MsgSavedSettings") ? true : (bool)(settings["MsgSavedSettings"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["MsgSavedSettings"] = value;
            }
        }
        public static bool MsgUpdateTask
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("MsgUpdateTask") ? true : (bool)(settings["MsgUpdateTask"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["MsgUpdateTask"] = value;
            }
        }
        public static bool MsgUpdateTaskList
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("MsgUpdateTaskList") ? true : (bool)(settings["MsgUpdateTaskList"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["MsgUpdateTaskList"] = value;
            }
        }
        public static bool MsgCreateTask
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("MsgCreateTask") ? true : (bool)(settings["MsgCreateTask"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["MsgCreateTask"] = value;
            }
        }
        public static bool MsgCreateTaskList
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                return !settings.Contains("MsgCreateTaskList") ? true : (bool)(settings["MsgCreateTaskList"]);
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["MsgCreateTaskList"] = value;
            }
        }

        #endregion

        #region Public Methods

        public static void ClearSession()
        {
            TokenType = RefreshToken = AccessToken = String.Empty;
            IsolatedStorageSettings.ApplicationSettings.Clear();
        }

        public static bool IsLoggedIn()
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            return settings.Contains("Timestamp") && settings.Contains("AccessToken") && !string.IsNullOrEmpty(TokenType) && !string.IsNullOrEmpty(AccessToken) && !string.IsNullOrEmpty(RefreshToken);
        }

        public static async Task<bool> RefreshData(bool alertWhenNoConnection = false, string specificList = null, bool refresh = false)
        {
            try
            {
                // Create a list to hold the local data
                List<Model.TaskListItem> localStorageList = await TaskListHelper.GetTaskListFromApplicationStorage(false);
                if (localStorageList == null) { localStorageList = new List<Model.TaskListItem>(); }

                // Create a list to hold the data from the api
                // If no data locally then populate it, else if AutoRefreshLists or AutoRefreshTasks = true then get data
                // Goal is to only get Google data when needed 

                // Create a final list to hold the results for local storage
                List<Model.TaskListItem> finalList = new List<Model.TaskListItem>();

                // Get the settings
                var settings = IsolatedStorageSettings.ApplicationSettings;

                // Sync w/ google?
                bool googleSync = false;

                List<Model.TaskListItem> googleApiList = null;
                if (AutoRefreshLists || AutoRefreshTasks || localStorageList.Count == 0 || refresh || GTaskSettings.FirstRun == true)
                {
                    googleApiList = await TaskListHelper.LoadTaskDataFromApi(alertWhenNoConnection, null);// specificList);
                }
                else
                {
                    //if (specificList != null)
                    //{
                    //    localStorageList.RemoveAll(x => x.id != specificList);
                    //    finalList = localStorageList;
                    //    goto finish;
                    //}
                    //else
                    //{
                        finalList = localStorageList;
                        goto finish;
                    //}
                    
                }
                
                // If the google api list returns null then we must assume that we can't get a connection, so error out
                if (googleApiList == null)
                {
                    return false;
                }
                googleSync = true;

                // Get the last sync date
                DateTime lastSyncDate = DateTime.Parse("1/1/1901");
                if (settings.Contains("LastSyncDate"))
                {
                    DateTime.TryParse(settings["LastSyncDate"].ToString(), out lastSyncDate);
                }

                // Loop through the two lists and check if any changes need to be synced
                foreach (Model.TaskListItem lSList in localStorageList)
                {
                    // Create a boolean to determine if we need to reorder tasks
                    bool reOrderTasks = false;

                    // Look for a corresponding list in the API
                    if (googleApiList.Where(x => x.id == lSList.id).Count() > 0)
                    {
                        // Get the corresponding google api list
                        Model.TaskListItem gAList = googleApiList.Where(x => x.id == lSList.id).First();

                        // Remove this list from the google api list to narrow things down
                        googleApiList.RemoveAll(x => x.id == gAList.id);

                        // Check if the titles are the same
                        if (lSList.title != gAList.title)
                        {
                            // Check which one was more recently updated
                            if (DateTime.Parse(lSList.updated) > DateTime.Parse(gAList.updated))
                            {
                                // Set the new title for the ga list
                                gAList.title = lSList.title;

                                // Update the list in the api
                                await TaskListHelper.RetryUpdateList(new List<object> { gAList });
                            }
                            else if (DateTime.Parse(lSList.updated) <= DateTime.Parse(gAList.updated))
                            {
                                // Set the new title for the ls list
                                lSList.title = gAList.title;
                            }
                        }

                        // Create a final TaskListtem
                        Model.TaskListItem finalTaskListItem = new Model.TaskListItem();

                        //Convert to universal
                        string lSListupdated = Universal.ConvertToUniversalDate(lSList.updated);
                        string gAListupdated = Universal.ConvertToUniversalDate(gAList.updated);
                        //string lSListupdatedFormat = (DateTime)lSListupdated.ToString("MM/dd/yyyy hh:mm:ss");

                        // Set the necessary properties
                        finalTaskListItem.updated = (DateTime.Parse(lSListupdated) > DateTime.Parse(gAListupdated)) ? lSList.updated : gAList.updated;
                        finalTaskListItem.title = lSList.title;
                        finalTaskListItem.id = lSList.id;
                        finalTaskListItem.selfLink = gAList.selfLink;
                        finalTaskListItem.kind = gAList.kind;

                        // The lists are the same now but lets compare the individual tasks
                        #region Tasks
                        foreach (Model.TaskItem lSItem in lSList.taskList)
                        {
                            if (lSItem == null) { continue; }

                            // Check if there is a corresponding task in the gAList
                            if (gAList.taskList.Where(x => x.id == lSItem.id).Count() > 0)
                            {
                                // Get the task from the gAList
                                Model.TaskItem gAItem = gAList.taskList.Where(x => x.id == lSItem.id).First();

                                // If the positions do not match then we need to update them all
                                if (lSItem.position != gAItem.position)
                                {
                                    reOrderTasks = true;
                                }
                                
                                // Remove the gAItem from the gAList to narrow things down
                                gAList.taskList.RemoveAll(x => x.id == gAItem.id);

                                //Get due dates and compare formatted correctly
                                //local vars
                                string lSItemdue = Universal.ConvertToUniversalDate(lSItem.due);
                                string lSItemupdated = Universal.ConvertToUniversalDate(lSItem.updated);
                                string lSItemcompleted = Universal.ConvertToUniversalDate(lSItem.completed);
                                //string lSItemnotes = lSItem.notes;
                                //string lSItemtitle = lSItem.title;

                                //google vars
                                string gAItemdue = Universal.ConvertToUniversalDate(gAItem.due);
                                string gAItemupdated = Universal.ConvertToUniversalDate(gAItem.updated);
                                string gAItemcompleted = Universal.ConvertToUniversalDate(gAItem.completed);
                                //string gAItemnotes = gAItem.notes;
                                //string gAItemtitles = gAItem.title;

                                // Now we check the properties of the item, get the more recently updated one
                                if (DateTime.Parse(lSItemupdated) > DateTime.Parse(gAItemupdated) && (lSItemcompleted != gAItemcompleted
                                    || lSItem.deleted != gAItem.deleted || lSItemdue != gAItemdue
                                    || lSItem.hidden != gAItem.hidden || lSItem.kind != gAItem.kind
                                    || lSItem.notes != gAItem.notes || lSItem.parent != gAItem.parent
                                    || lSItem.position != gAItem.position || lSItem.status != gAItem.status
                                    || lSItem.title != gAItem.title))
                                {
                                    // The local storage item was updated more recently, submit it to the api
                                    await TaskHelper.RetryUpdateTask(new List<object> { lSItem });

                                    // Check if we need ot update the status specifically
                                    if (lSItem.status != gAItem.status)
                                    {
                                        string due = null;
                                        if (lSItemdue != null)
                                        {
                                            due = Convert.ToDateTime(Universal.ConvertToUniversalDate(lSItem.due)).ToString("yyyy-MM-dd'T'hh:mm:ss.00Z");
                                        }
                                        bool isChecked = false;
                                        if (lSItem.status == "completed") { isChecked = true; }

                                        await TaskHelper.UpdateTaskStatus(lSList.id, lSItem.id, lSItemdue, isChecked);
                                    }

                                    // Add the lSItem to the final list
                                    finalTaskListItem.taskList.Add(lSItem);
                                }
                                else
                                {
                                    // The item was updated more recently on a remote device, simply add it to the final list
                                    finalTaskListItem.taskList.Add(gAItem);
                                }
                            }
                            else
                            {
                                // There was no corresponding item in the list from the google api
                                // Check if the item was added while in offline mode
                                if (settings.Contains("Task_" + lSItem.id + "_Action") && settings.Contains("Task_" + lSItem.id + "_Timestamp")
                                    && settings["Task_" + lSItem.id + "_Action"].ToString() == "added"
                                    && DateTime.Parse(settings["Task_" + lSItem.id + "_Timestamp"].ToString()) > lastSyncDate)
                                {
                                    // Submit the item to the google api
                                    Model.TaskItem result = await TaskHelper.RetryAddTask(new List<object> { lSItem, null, lSList.id });

                                    // Add the item to the final list
                                    finalTaskListItem.taskList.Add(result);

                                    // Continue to the next one
                                    continue;
                                }

                                // At this point we can assume the item was deleted on a remote device, no need to add it to the final list
                            }

                            // Remove any settings associated with the task
                            settings.Remove("Task_" + lSItem.id + "_Action");
                            settings.Remove("Task_" + lSItem.id + "_Timestamp");
                        }

                        // Loop through what is left in the google api tasks
                        foreach (Model.TaskItem task in gAList.taskList)
                        {
                            // Check if this task was deleted locally
                            if (settings.Contains("Task_" + task.id + "_Action") && settings["Task_" + task.id + "_Action"].ToString() == "deleted"
                                && DateTime.Parse(settings["Task_" + task.id + "_Timestamp"].ToString()) >= DateTime.Parse(task.updated))
                            {
                                // Delete the list in the api
                                await TaskHelper.DeleteTask(gAList.id, task.id);
                            }
                            else
                            {
                                finalTaskListItem.taskList.Add(task);
                            }

                            // Remove any settings
                            settings.Remove("Task_" + task.id + "_Action");
                            settings.Remove("Task_" + task.id + "_Timestamp");
                        }
                        #endregion

                        finalList.Add(finalTaskListItem);
                    }
                    else
                    {
                        // There was no corresponding list in the list from the google api
                        // Check if the list was added while in offline mode
                        if (settings.Contains("List_" + lSList.id + "_Action") && settings.Contains("List_" + lSList.id + "_Timestamp")
                                    && settings["List_" + lSList.id + "_Action"].ToString() == "added"
                                    && DateTime.Parse(settings["List_" + lSList.id + "_Timestamp"].ToString()) > lastSyncDate)
                        {
                            // Submit the list to the google api
                            Model.TaskListItem results = await TaskListHelper.RetryAddTaskList(new List<object> { lSList });

                            // Add the list to the final list
                            if (results != null)
                            {
                                // Check if we need to update an ID value
                                if (settings.Contains("GetNewListId") && settings["GetNewListId"].ToString() == lSList.id)
                                {
                                    settings["GetNewListId"] = results.id;
                                }

                                // Add the tasks
                                #region Tasks
                                foreach (Model.TaskItem lSItem in lSList.taskList)
                                {
                                    // Submit the item to the google api
                                    Model.TaskItem task = await TaskHelper.RetryAddTask(new List<object> { lSItem, null, results.id });

                                    // Add the item to the final list
                                    results.taskList.Add(task);

                                    // Remove any settings associated with the task
                                    settings.Remove("Task_" + lSItem.id + "_Action");
                                    settings.Remove("Task_" + lSItem.id + "_Timestamp");
                                }
                                #endregion

                                finalList.Add(results);
                            }
                            else
                            {
                                MessageBox.Show("Uh oh! There was an error when syncing the tasks. Please try again soon.");

                                return false;
                            }
                        }

                        // At this point we can assume the list was deleted on a remote device and we don't have to add it to the final list
                    }

                    // Get the final task list
                    if (finalList.Count > 0)
                    {
                        Model.TaskListItem finalTaskList = finalList.Last();
                        finalTaskList.taskList = finalTaskList.taskList.OrderBy(x => double.Parse(x.position)).ToList();

                        // Loop through the tasks
                        //if ((GTaskSettings.TaskSort == 0 && GTaskSettings.DisableDragDrop != true) //only move if settings are enabled
                        if (reOrderTasks) //only move if reOrderTasks is detected
                        {
                            for (int i = 0; i < finalTaskList.taskList.Count; i++)
                            {
                                string position = "";
                                if (i == 0)
                                {
                                    position = await TaskHelper.MoveTask(finalTaskList.id, finalTaskList.taskList[i].id, null);
                                }
                                else
                                {
                                    position = await TaskHelper.MoveTask(finalTaskList.id, finalTaskList.taskList[i].id, finalTaskList.taskList[i - 1].id);
                                }

                                // Update the position
                                finalList.Last().taskList.First(x => x.id == finalTaskList.taskList[i].id).position = position;
                            }
                        }
                    }

                    // Remove any settings
                    settings.Remove("List_" + lSList.id + "_Action");
                    settings.Remove("List_" + lSList.id + "_Timestamp");
                }

                // Loop through what is left in the google api list
                foreach (Model.TaskListItem list in googleApiList)
                {
                    // Check if this list was deleted locally
                    if (settings.Contains("List_" + list.id + "_Action") && settings["List_" + list.id + "_Action"].ToString() == "deleted"
                        && DateTime.Parse(settings["List_" + list.id + "_Timestamp"].ToString()) >= DateTime.Parse(list.updated))
                    {
                        // Delete the list in the api
                        await TaskListHelper.DeleteList(list.id);
                    }
                    else
                    {
                        finalList.Add(list);
                    }

                    // Remove any settings
                    settings.Remove("List_" + list.id + "_Action");
                    settings.Remove("List_" + list.id + "_Timestamp");
                }

                // Create a temp list
                List<string> keyStrList = settings.Select(x => x.Key.ToString()).ToList<string>();

                // Remove any remaining settings
                foreach (string keyStr in keyStrList)
                {
                    if (keyStr.Contains("List_") || keyStr.Contains("Task_"))
                    {
                        settings.Remove(keyStr);
                    }
                }

                
                // Save the final list to local storage
                //if (specificList == null) //only commit to storage if it isn't for a specific list
                //{
                    await TaskListHelper.SubmitToLocalStorage(finalList);
                //}

                finish:

                // Update any live tiles
                // Loop through the live tiles
                foreach (ShellTile shellTile in ShellTile.ActiveTiles)
                {
                    try
                    {
                        if (shellTile.NavigationUri.ToString().Contains("/Views/TaskView.xaml?Id="))
                        {
                            // Get the ID
                            string id = shellTile.NavigationUri.ToString().Substring(shellTile.NavigationUri.ToString().IndexOf("?Id=")).Replace("?Id=", "");
                            if (id.Contains("&")) { id = id.Substring(0, id.IndexOf("&")); }

                            // Check if there is a corresponding list
                            if (finalList.Where(x => x.id == id).Count() > 0)
                            {
                                // Get the list
                                Model.TaskListItem list = finalList.Where(x => x.id == id).First();

                                // Get the title
                                string title = list.title;
                                if (title.Length > 24)
                                {
                                    title = title.Substring(0, 21) + "...";
                                }

                                //Create a completed list
                                var NCtasks = new List<TaskItem>();
                                NCtasks.Clear();
                                NCtasks.AddRange(list.taskList.Where(y => y.status != "completed").ToList());

                                // Update the tile accordingly
                                int count = 0;
                                if ((int)settings["LiveTileCount"] > 0)
                                {
                                    if ((bool)settings["IncludeNoDueDate"])
                                    {
                                        
                                        count = NCtasks.Where(y => (y.due != null ? DateTime.Parse(y.due).AddHours(-12) : DateTime.MinValue) <= DateTime.Now).Count();
                                        settings["DueCount_" + list.id] = count;
                                    }
                                    else
                                    {
                                        count = NCtasks.Where(y => (y.due != null ? DateTime.Parse(y.due).AddHours(-12) : DateTime.MaxValue) <= DateTime.Now).Count();
                                        settings["DueNDCount_" + list.id] = count;
                                    }
                                }
                                else
                                {
                                    count = NCtasks.Count;
                                    settings["Count_" + list.id] = count;
                                }

                                //most displays cut off the 16th charactor, this reduces it to 13 + ... for the Edit page and LiveTile
                                var tileData = new StandardTileData { Title = title, BackgroundImage = new Uri("/Assets/Icons/202.png", UriKind.Relative), Count = count };
                                shellTile.Update(tileData);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // Do nothing
                        Console.WriteLine(e.ToString());
                    }
                }

                // Set the last sync date
                if (googleSync)
                    settings["LastSyncDate"] = DateTime.UtcNow;

                //If you've made it this far on your first attempt them you FirstRun is complete
                GTaskSettings.FirstRun = false;

                // Return true
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine(e.StackTrace.ToString());

                return false;
            }
        }

        public static async Task<bool> RemoveHidden()
        {
            try {
                // Grap the local task lists to remove all the Hidden tasks
                List<TaskListItem> lists = await TaskListHelper.GetTaskListFromApplicationStorage();

                foreach (TaskListItem list in lists)
                {
                    list.taskList.RemoveAll(x => x.hidden == "True");
                }

                // Resubmit the list to local storage
                await TaskListHelper.SubmitToLocalStorage(lists);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<IRestResponse> ExecuteRestTask(RestRequest request, bool refreshToken = true, RestClient client = null)
        {
            // Execute a rest request with a timeout
            if (refreshToken)
            {
                await LoginHelper.RefreshTokenCodeAwait(true);
            }

            var restClient = client;
            if (client == null)
            {
                restClient = new RestClient { Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(GTaskSettings.AccessToken) };
            }

            var task = restClient.ExecuteTask(request);
            if (await Task.WhenAny(task, Task.Delay(GTaskSettings.RequestTimeout)) == task)
            {
                return task.Result;
            }
            else
            {
                RestResponse response = new RestResponse();
                response.StatusCode = HttpStatusCode.BadRequest;  
                return response;
            }
        }

        public static bool Upsell()
        {
            //goal of this is to tell users the option is only available for buyers
            MessageBoxResult msgResult = MessageBox.Show("This feature is only available in the paid version. Would you like to upgrade for $2.99?", "Upgrade?", MessageBoxButton.OKCancel);
            if (msgResult == MessageBoxResult.OK)
            {
                MarketplaceDetailTask _marketPlaceDetailTask = new MarketplaceDetailTask();
                _marketPlaceDetailTask.ContentIdentifier = "b4a8de82-cd83-40b0-b2e6-31eb95a0100f"; //GTASK+ Link
                _marketPlaceDetailTask.Show();
            }

            return true;
        }
        #endregion
    }
}
