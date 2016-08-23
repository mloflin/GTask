using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;

namespace BackgroundAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {

        /// ScheduledAgent constructor, initializes the UnhandledException handler
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        protected async override void OnInvoke(ScheduledTask task)
        {
            #region NewCode
            try
            {
                // Create a list to hold the local data
                List<TaskListItem> localStorageList = await TaskListHelper.GetTaskListFromApplicationStorage(false);
                if (localStorageList == null) { localStorageList = new List<TaskListItem>(); }

                // Create a list to hold the data from the api
                // If no data locally then populate it, else if AutoRefreshLists or AutoRefreshTasks = true then get data
                // Goal is to only get Google data when needed 

                // Create a final list to hold the results for local storage
                List<TaskListItem> finalList = new List<TaskListItem>();

                // Get the settings
                var settings = IsolatedStorageSettings.ApplicationSettings;

                // Sync w/ google?
                bool googleSync = false;

                List<TaskListItem> googleApiList = null;
                //if (AutoRefreshLists || AutoRefreshTasks || localStorageList.Count == 0 || refresh || GTaskSettings.FirstRun == true)
                //{
                    googleApiList = await TaskListHelper.LoadTaskDataFromApi();
                //}
                //else
                //{
                //    if (specificList != null)
                //    {
                //        localStorageList.RemoveAll(x => x.id != specificList);
                //        finalList = localStorageList;
                //        goto finish;
                //        //googleApiList = localStorageList;
                //    }
                //    else
                //    {
                //        finalList = localStorageList;
                //        goto finish;
                //        //googleApiList = localStorageList;
                //    }

                //}

                // If the google api list returns null then we must assume that we can't get a connection, so error out
                if (googleApiList == null)
                {
                    return;// false;
                }
                googleSync = true;

                // Get the last sync date
                DateTime lastSyncDate = DateTime.Parse("1/1/1901");
                if (settings.Contains("LastSyncDate"))
                {
                    DateTime.TryParse(settings["LastSyncDate"].ToString(), out lastSyncDate);
                }

                // Loop through the two lists and check if any changes need to be synced
                foreach (TaskListItem lSList in localStorageList)
                {
                    // Create a boolean to determine if we need to reorder tasks
                    bool reOrderTasks = false;

                    // Look for a corresponding list in the API
                    if (googleApiList.Where(x => x.id == lSList.id).Count() > 0)
                    {
                        // Get the corresponding google api list
                        TaskListItem gAList = googleApiList.Where(x => x.id == lSList.id).First();

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
                        TaskListItem finalTaskListItem = new TaskListItem();

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
                        foreach (TaskItem lSItem in lSList.taskList)
                        {
                            if (lSItem == null) { continue; }

                            // Check if there is a corresponding task in the gAList
                            if (gAList.taskList.Where(x => x.id == lSItem.id).Count() > 0)
                            {
                                // Get the task from the gAList
                                TaskItem gAItem = gAList.taskList.Where(x => x.id == lSItem.id).First();

                                // If the positions do not match then we need to update them all
                                if (lSItem.position != gAItem.position)
                                {
                                    reOrderTasks = true;
                                }

                                // Remove the gAItem from the gAList to narrow things down
                                gAList.taskList.RemoveAll(x => x.id == gAItem.id);

                                //Get due dates and compare formatted correctly
                                string lSItemdue = Universal.ConvertToUniversalDate(lSItem.due);
                                string gAItemdue = Universal.ConvertToUniversalDate(gAItem.due);
                                string lSItemupdated = Universal.ConvertToUniversalDate(lSItem.updated);
                                string gAItemupdated = Universal.ConvertToUniversalDate(gAItem.updated);
                                string lSItemcompleted = Universal.ConvertToUniversalDate(lSItem.completed);
                                string gAItemcompleted = Universal.ConvertToUniversalDate(gAItem.completed);


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

                                        await TaskHelper.UpdateTaskStatus(lSList.id, lSItem.id, due, isChecked);
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
                                    TaskItem result = await TaskHelper.RetryAddTask(new List<object> { lSItem, null, lSList.id });

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
                        foreach (TaskItem Temptask in gAList.taskList)
                        {
                            // Check if this task was deleted locally
                            if (settings.Contains("Task_" + Temptask.id + "_Action") && settings["Task_" + Temptask.id + "_Action"].ToString() == "deleted"
                                && DateTime.Parse(settings["Task_" + Temptask.id + "_Timestamp"].ToString()) >= DateTime.Parse(Temptask.updated))
                            {
                                // Delete the list in the api
                                await TaskHelper.DeleteTask(gAList.id, Temptask.id);
                            }
                            else
                            {
                                finalTaskListItem.taskList.Add(Temptask);
                            }

                            // Remove any settings
                            settings.Remove("Task_" + Temptask.id + "_Action");
                            settings.Remove("Task_" + Temptask.id + "_Timestamp");
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
                            TaskListItem results = await TaskListHelper.RetryAddTaskList(new List<object> { lSList });

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
                                foreach (TaskItem lSItem in lSList.taskList)
                                {
                                    // Submit the item to the google api
                                    TaskItem Temptask = await TaskHelper.RetryAddTask(new List<object> { lSItem, null, results.id });

                                    // Add the item to the final list
                                    results.taskList.Add(Temptask);

                                    // Remove any settings associated with the task
                                    settings.Remove("Task_" + lSItem.id + "_Action");
                                    settings.Remove("Task_" + lSItem.id + "_Timestamp");
                                }
                                #endregion

                                finalList.Add(results);
                            }
                            else
                            {
                                //MessageBox.Show("Uh oh! There was an error when syncing the tasks. Please try again soon.");

                                //return false;
                            }
                        }

                        // At this point we can assume the list was deleted on a remote device and we don't have to add it to the final list
                    }

                    // Get the final task list
                    TaskListItem finalTaskList = finalList.Last();
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

                    // Remove any settings
                    settings.Remove("List_" + lSList.id + "_Action");
                    settings.Remove("List_" + lSList.id + "_Timestamp");
                }

                // Loop through what is left in the google api list
                foreach (TaskListItem list in googleApiList)
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

            //finish:

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
                                TaskListItem list = finalList.Where(x => x.id == id).First();

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
                                        count = NCtasks.Where(y => (y.due != null ? DateTime.Parse(y.due) : DateTime.MinValue) <= DateTime.Now).Count();
                                        settings["DueCount_" + list.id] = count;
                                    }
                                    else
                                    {
                                        count = NCtasks.Where(y => (y.due != null ? DateTime.Parse(y.due) : DateTime.MaxValue) <= DateTime.Now).Count();
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine(e.StackTrace.ToString());
            }

            #endregion
            NotifyComplete();
        }
    }
}