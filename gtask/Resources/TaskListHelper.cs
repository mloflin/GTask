using gTask.Model;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;

namespace gTask.Resources
{
    public class TaskListHelper
    {
        private const string ApplicationDataFileName = "TaskListData.txt";

        public static void OnCollectionChanged(Object sender, NotifyCollectionChangedEventArgs e)
        {

        }

        public static async Task SubmitToLocalStorage(List<TaskListItem> list)
        {
            using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(
                                  ApplicationDataFileName,
                                  CreationCollisionOption.ReplaceExisting))
            {
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    sw.Write(JsonConvert.SerializeObject(list));
                }
            }
        }

        /// <summary>
        /// Load the task lists and all associated data from the api
        /// </summary>
        /// <param name="alertWhenNoConnection">Alert the user that we could not connect to the api</param>
        /// <returns>Boolean result</returns>
        public static async Task<List<TaskListItem>> LoadTaskDataFromApi(bool alertWhenNoConnection = false, string specificList = null)
        {
            try
            {
                await LoginHelper.RefreshTokenCodeAwait(true);

                var settings = IsolatedStorageSettings.ApplicationSettings;

                string url = "https://www.googleapis.com/tasks/v1/users/@me/lists?access_token=" + GTaskSettings.AccessToken;
                if (!string.IsNullOrEmpty(specificList))
                {
                    url = "https://www.googleapis.com/tasks/v1/users/@me/lists/" + specificList + "?access_token=" + GTaskSettings.AccessToken;
                }

                // Get the Task Lists
                #region Get Task Lists
                var restRequest = new RestRequest(Method.GET)
                {
                    Resource = url,
                    Timeout = GTaskSettings.RequestTimeout
                };

                //Make the call
                var restResponse = await GTaskSettings.ExecuteRestTask(restRequest, false);

                //Store Lists
                var TaskListObject = JObject.Parse(restResponse.Content);

                // If the limit is exceeded then pop up a message, otherwise lets continue
                if (restResponse.Content.Contains("Limit Exceeded"))
                {
                    MessageBox.Show("[1] Take a screenshot and send it to @MattLoflin or MLoflin.Apps@gmail.com\r\n \r\n" + restResponse.Content.ToString());
                    return null;
                }

                List<TaskListItem> TaskListList = null;
                if (string.IsNullOrEmpty(specificList))
                {
                    TaskListList = TaskListObject["items"].Select(m => new TaskListItem((string)m.SelectToken("title"),
                                    (string)m.SelectToken("id"),
                                    (string)m.SelectToken("kind"),
                                    (string)m.SelectToken("selfLink"),
                                    (string)m.SelectToken("updated"))).ToList();
                }
                else
                {
                    TaskListList = new List<TaskListItem>();
                    TaskListList.Add(new TaskListItem((string)TaskListObject.SelectToken("title"),
                                    (string)TaskListObject.SelectToken("id"),
                                    (string)TaskListObject.SelectToken("kind"),
                                    (string)TaskListObject.SelectToken("selfLink"),
                                    (string)TaskListObject.SelectToken("updated")));
                }

                //Check to see if pageToken exists
                //If so - iterate until empty
                var pageToken = string.Empty;
                if (TaskListObject["nextPageToken"] != null)
                    pageToken = TaskListObject["nextPageToken"].ToString();

                while (pageToken != string.Empty)
                {
                    restRequest = new RestRequest(Method.GET)
                    {
                        Resource =
                            "https://www.googleapis.com/tasks/v1/users/@me/lists?pageToken=" + pageToken + "&access_token=" + GTaskSettings.AccessToken,
                        Timeout = GTaskSettings.RequestTimeout
                    };

                    restResponse = await GTaskSettings.ExecuteRestTask(restRequest, false);
                    TaskListObject = JObject.Parse(restResponse.Content);

                    //Add the new list to the current list
                    if (string.IsNullOrEmpty(specificList))
                    {
                        TaskListList.AddRange(TaskListObject["items"].Select(m => new TaskListItem((string)m.SelectToken("title"),
                                        (string)m.SelectToken("id"),
                                        (string)m.SelectToken("kind"),
                                        (string)m.SelectToken("selfLink"),
                                        (string)m.SelectToken("updated"))));
                    }
                    else
                    {
                        TaskListList.Add(new TaskListItem((string)TaskListObject.SelectToken("title"),
                                        (string)TaskListObject.SelectToken("id"),
                                        (string)TaskListObject.SelectToken("kind"),
                                        (string)TaskListObject.SelectToken("selfLink"),
                                        (string)TaskListObject.SelectToken("updated")));
                    }

                    //reset the pageToken
                    pageToken = string.Empty;
                    if (TaskListObject["nextPageToken"] != null)
                        pageToken = TaskListObject["nextPageToken"].ToString();
                }
                #endregion

                // Get the Task Items
                #region Get Task Items
                // Loop through the task list list and get the tasks for each list
                foreach (TaskListItem list in TaskListList)
                {
                    // Instantiate the task list
                    list.taskList = new List<TaskItem>();

                    //If user wants to see Hidden (Completed) tasks, then display them. Else return current list
                    restRequest.Resource = "https://www.googleapis.com/tasks/v1/lists/" + list.id + "/tasks?access_token=" + GTaskSettings.AccessToken;
                    if (GTaskSettings.HideCompleted == false)
                        restRequest.Resource = "https://www.googleapis.com/tasks/v1/lists/" + list.id + "/tasks?showHidden=True&access_token=" + GTaskSettings.AccessToken;

                    //Make the call
                    restResponse = await GTaskSettings.ExecuteRestTask(restRequest, false);

                    var TaskObject = JObject.Parse(restResponse.Content);
                    if (!restResponse.Content.Contains("Limit Exceeded"))
                    {
                        if (TaskObject != null && TaskObject["items"] != null)
                        {
                            list.taskList = TaskObject["items"].Select(m => new TaskItem((string)m.SelectToken("id"),
                                                        (string)m.SelectToken("kind"),
                                                        ((string)m.SelectToken("title")) ==
                                                        String.Empty
                                                            ? "Empty"
                                                            : (string)
                                                            m.SelectToken("title"),
                                                        (string)m.SelectToken("notes"),
                                                        list.id,
                                                        (string)
                                                        m.SelectToken("position"),
                                                        (string)m.SelectToken("update"),
                                                        (string)m.SelectToken("due"),
                                                        (string)m.SelectToken("deleted"),
                                                        (string)m.SelectToken("hidden"),
                                                        (string)m.SelectToken("status"),
                                                        (string)
                                                        m.SelectToken("selfLink"),
                                                        (string)
                                                        m.SelectToken("completed"),
                                                        (string)
                                                        m.SelectToken("updated")
                                                    )).ToList();
                        }

                        //Check to see if pageToken exists
                        //If so - iterate until empty
                        pageToken = string.Empty;
                        if (TaskObject["nextPageToken"] != null)
                            pageToken = TaskObject["nextPageToken"].ToString();

                        while (pageToken != string.Empty)
                        {
                            //If user wants to see Hidden (Completed) tasks, then display them. Else return current list
                            restRequest.Resource = "https://www.googleapis.com/tasks/v1/lists/" + list.id + "/tasks?pageToken=" + pageToken + "&access_token=" + GTaskSettings.AccessToken;
                            if (GTaskSettings.HideCompleted == false)
                                restRequest.Resource = "https://www.googleapis.com/tasks/v1/lists/" + list.id + "/tasks?pageToken=" + pageToken + "&showHidden=True&access_token=" + GTaskSettings.AccessToken;
                            restResponse = await GTaskSettings.ExecuteRestTask(restRequest, false);
                            TaskObject = JObject.Parse(restResponse.Content);

                            //Add the new list to the current list
                            list.taskList.AddRange(TaskObject["items"].Select(
                                    m => new TaskItem((string)m.SelectToken("id"),
                                                        (string)m.SelectToken("kind"),
                                                        ((string)m.SelectToken("title")) ==
                                                        String.Empty
                                                            ? "Empty"
                                                            : (string)
                                                            m.SelectToken("title"),
                                                        (string)m.SelectToken("notes"),
                                                        (string)m.SelectToken("parent"),
                                                        (string)
                                                        m.SelectToken("position"),
                                                        (string)m.SelectToken("update"),
                                                        (string)m.SelectToken("due"),
                                                        (string)m.SelectToken("deleted"),
                                                        (string)m.SelectToken("hidden"),
                                                        (string)m.SelectToken("status"),
                                                        (string)
                                                        m.SelectToken("selfLink"),
                                                        (string)
                                                        m.SelectToken("completed"),
                                                        (string)
                                                        m.SelectToken("updated")
                                                    )).ToList());

                            //reset the pageToken
                            pageToken = string.Empty;
                            if (TaskObject["nextPageToken"] != null)
                                pageToken = TaskObject["nextPageToken"].ToString();
                        }
                    }
                    else
                    {
                        MessageBox.Show("[2] Take a screenshot and send it to @MattLoflin or MLoflin.Apps@gmail.com\r\n \r\n" + restResponse.Content.ToString());
                    }
                }
                #endregion

                return TaskListList;
            }
            catch (Exception e)
            {
                // Check if an error was returned indicated that we couldnt connect to the api
                if (alertWhenNoConnection && e.ToString().Contains("Error reading JObject from JsonReader"))
                {
                    var googError = MessageBox.Show("Google is having intermittent connectivity issues, please try again in a couple minutes.","Google Connection Issues",MessageBoxButton.OK);
                }

                // Return false
                return null;

                // There was an issue getting the data.
                // There might not have been an internet connection so ignore the error
                // This can be handled more delicately in the future
            }
        } 

        /// <summary>
        /// Retrieves the TaskListItem from the Application storage
        /// </summary>
        /// <returns>TaskListItem</returns>
        public static async Task<List<TaskListItem>> GetTaskListFromApplicationStorage(bool alertWhenDoData = true)
        {
            List<TaskListItem> taskListList = new List<TaskListItem>();
            try
            {
                //check if file exists
                IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                if (storage.FileExists(ApplicationDataFileName))
                {
                    using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(ApplicationDataFileName))
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            taskListList = ((JArray)JsonConvert.DeserializeObject(sr.ReadToEnd())).ToObject<List<TaskListItem>>();
                        }
                    }
                }
            }
            catch (Exception)//e)
            {
                if (GTaskSettings.MsgError && alertWhenDoData)
                {
                    MessageBox.Show("[3] Take a screenshot and send it to @MattLoflin or MLoflin.Apps@gmail.com");
                }
            }

            return taskListList;
        }
        
        //Refreshes the token then calls RetryGetTaskList which gets the latest task lists
        #region Get task Lists
        public static async Task GetTaskList(Action<ObservableCollection<TaskListItem>> callback, bool alertWhenNoConnection = false, bool refresh = false)
        {
            // Sync the latest data between the api and the local storage
            await GTaskSettings.RefreshData(alertWhenNoConnection,null , refresh);

            // Get the actual task list
            await RetryGetTaskList(new ObservableCollection<object> { callback });
        }

        public static async Task RetryGetTaskList(ObservableCollection<object> obj)
        {
            try
            {
                // Get the app settings
                var settings = IsolatedStorageSettings.ApplicationSettings;

                // Create a task list item
                var taskListItem = new ObservableCollection<TaskListItem>();
                taskListItem.CollectionChanged += OnCollectionChanged;

                // Load the task lists from local storage
                List<TaskListItem> TaskListList = await GetTaskListFromApplicationStorage(false);

                // Check if the list is null, if so then return
                if (TaskListList == null) { return; }

                taskListItem.Clear();

                if (GTaskSettings.TaskListSort == 1) //sort by title (1)
                {
                    foreach (var taskList in TaskListList.OrderBy(x => x.title).ToList())
                    {
                        //Store/Update Title
                        settings["Title_" + taskList.id] = taskList.title;
                        Tile_Update(taskList.title, taskList.id);

                        //Add item to list
                        taskListItem.Add(taskList);
                    }

                    var list = obj[0] as Action<ObservableCollection<TaskListItem>>;
                    if (list != null) list(taskListItem);
                }
                else
                {
                    foreach (var taskList in TaskListList.ToList()) //default sort
                    {
                        //Store/Update Title
                        settings["Title_" + taskList.id] = taskList.title;
                        Tile_Update(taskList.title, taskList.id);

                        //Add item to list
                        taskListItem.Add(taskList);
                    }

                    var list = obj[0] as Action<ObservableCollection<TaskListItem>>;
                    if (list != null) list(taskListItem);
                }
            }
            catch
            {
                if (GTaskSettings.MsgError)
                {
                    MessageBox.Show("Houston, we have a problem. I can't retrieve the Task List(s), can you try again?");
                }
            }
        }

        private static void Tile_Update(string curTitle, string ID)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            //Check if a tile exists, if it does proceed to update it
            var tile = ShellTile.ActiveTiles.FirstOrDefault(o => o.NavigationUri.ToString().Contains(ID));
            if (tile != null)
            {
                //No Tile was found, so add one for this page.
                //most displays cut off the 16th charactor, this reduces it to 13 + ... for the Edit page and LiveTile
                string liveTitle = curTitle;
                if (curTitle.Length > 24)
                {
                    liveTitle = curTitle.Substring(0, 21) + "...";
                }

                //Conditional LiveTile Updates depending on the settings (past due w/ no due date, past due w/o no due date, total count
                int count = 0;
                if (GTaskSettings.LiveTileCount > 0)
                {
                    if (GTaskSettings.IncludeNoDueDate)
                    {
                        count = (int)settings["DueCount_" + ID];
                    }
                    else
                    {
                        count = (int)settings["DueNDCount_" + ID];
                    }
                }
                else
                {
                    count = (int)settings["Count_" + ID];
                }
            
                //Update the tile with the latest TileData (e.g. New Title)
                var tileData = new StandardTileData { Title = liveTitle, BackgroundImage = new Uri("/Assets/Icons/202.png", UriKind.Relative), Count = count };
                tile.Update(tileData);
            }
        }
        #endregion

        #region Create List

        public static async Task<TaskListItem> AddTaskList(TaskListItem taskListItem, Action<bool> response)
        {
            return await RetryAddTaskList(new List<object>{taskListItem,response});
        }

        public static async Task<TaskListItem> RetryAddTaskList(IList<object> obj)
        {
            var restRequest = new RestRequest(Method.POST)
            {
                RequestFormat = DataFormat.Json,
                Resource = "https://www.googleapis.com/tasks/v1/users/@me/lists",
                Timeout = GTaskSettings.RequestTimeout
            };
            var ignored = "{title:\"" + ((TaskListItem)obj[0]).title + "\"}";
            restRequest.AddParameter("application/json", ignored, ParameterType.RequestBody);
            
            //Make the call
            var restResponse = await GTaskSettings.ExecuteRestTask(restRequest);
            try
            {
                if (restResponse.StatusCode == HttpStatusCode.OK)
                {
                    if (obj.Count > 1)
                    {
                        var callback = obj[1] as Action<bool>;
                        if (callback != null) callback(true);
                    }

                    var TaskListObject = JObject.Parse(restResponse.Content);

                    var TaskList = new TaskListItem((string)TaskListObject.SelectToken("title"),
                                    (string)TaskListObject.SelectToken("id"),
                                    (string)TaskListObject.SelectToken("kind"),
                                    (string)TaskListObject.SelectToken("selfLink"),
                                    (string)TaskListObject.SelectToken("updated"));

                    // Submit new task list to local storage too
                    List<TaskListItem> list = await TaskListHelper.GetTaskListFromApplicationStorage();

                    TaskListItem newList = new TaskListItem((string)TaskListObject.SelectToken("title"),
                                    (string)TaskListObject.SelectToken("id"),
                                    (string)TaskListObject.SelectToken("kind"),
                                    (string)TaskListObject.SelectToken("selfLink"),
                                    (string)TaskListObject.SelectToken("updated"));

                    // Add this new list to the local list
                    list.Add(newList);
                    // Submit the list to local storage
                    await TaskListHelper.SubmitToLocalStorage(list);

                    return TaskList;
                }
            }
            catch
            {
            }

            return null;
        }

        #endregion

        #region Delete List

        public static async Task<bool> DeleteList(string Id)
        {
            //const string message = "Don't freak out... I wasn't able to Delete the Task List, can you try again?";
            var restClient = new RestClient { Authenticator = new OAuth2UriQueryParameterAuthenticator(GTaskSettings.AccessToken), BaseUrl = "https://www.googleapis.com/tasks/v1/users/@me/lists/" + Id };
            var restRequest = new RestRequest(Method.DELETE)
            {
                    Timeout = GTaskSettings.RequestTimeout
            };

            //Make the call
            var restResponse = await GTaskSettings.ExecuteRestTask(restRequest, true, restClient);

            try
            {
                if(restResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    //success
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        #endregion

        #region Update List

        public static async Task<bool> UpdateList(TaskListItem taskListItem, Action<bool> response)
        {
            return await RetryUpdateList(new List<object> { taskListItem, response });
        }

        public static async Task<bool> RetryUpdateList(IList<object> obj)
        {
            var restRequest = new RestRequest(Method.PUT)
            {
                RequestFormat = DataFormat.Json,
                Resource = ((TaskListItem)obj[0]).selfLink,
                Timeout = GTaskSettings.RequestTimeout
            };
            ((TaskListItem)obj[0]).updated = DateTime.UtcNow.ToString("yyyy-MM-dd'T'hh:mm:ss.00Z");
            restRequest.AddBody(obj[0]);
            //Make the call
            var restResponse = await GTaskSettings.ExecuteRestTask(restRequest);
            try
            {
                if (restResponse.StatusCode == HttpStatusCode.OK)
                {
                    //Success
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        #endregion

        #region Get Task Items in a List

        public static async Task GetTasksForList(string id, Action<ObservableCollection<TaskItem>> callback)
        {
            await RetryGetTasksForList(new ObservableCollection<object> { id, callback });
        }

        private static async Task RetryGetTasksForList(ObservableCollection<object> obj)
        {
            string message = "Ugh.. I can't find the Tasks for this Task List, can you try again?";

            try
            {
                var taskItem = new ObservableCollection<TaskItem>();
                taskItem.CollectionChanged += OnCollectionChanged;

                // Create a list of tasks
                List<TaskItem> tasks = new List<TaskItem>();

                // Load the task lists from local storage
                List<TaskListItem> TaskListList = await GetTaskListFromApplicationStorage();

                // Get the appropriate task list if available
                if (TaskListList != null && TaskListList.Where(x => x.id == obj[0].ToString()).Count() > 0)
                {
                    var taskListItem = (TaskListItem)TaskListList.First(x => x.id == obj[0].ToString());
                    if (taskListItem.taskList != null)
                        tasks.AddRange(taskListItem.taskList);
                }

                taskItem.Clear();

                //Sort tasks by completed first]\//Sort the original list by completed time where Not complete is at the top, complete is then sorted by completion date
                tasks = tasks.OrderBy(x => x.completed != null ? DateTime.Parse(Universal.ConvertToUniversalDate(x.completed)) : DateTime.MinValue).ToList(); 

                //Conditional sorting based on Settings selection
                if (GTaskSettings.TaskSort == 1)
                {
                    if (!GTaskSettings.NoDueDateAtTop)
                    {
                        foreach (var task in tasks.OrderBy(x => x.due != null ? DateTime.Parse(Universal.ConvertToUniversalDate(x.due)) : DateTime.MaxValue).ToList()) //Closest First, No Date at Bottom
                        {
                            //Add task to list
                            taskItem.Add(task);
                        }
                    }
                    else
                    {
                        foreach (var task in tasks.OrderBy(x => x.due != null ? DateTime.Parse(Universal.ConvertToUniversalDate(x.due)) : DateTime.MinValue).ToList()) //Closest First, No Date at Top
                        {
                            //Add task to list
                            taskItem.Add(task);
                        }
                    }
                }
                else if (GTaskSettings.TaskSort == 2)
                {
                    foreach (var task in tasks.OrderBy(x => x.title).ToList()) //Alphabetical
                    {
                        //Add task to list
                        taskItem.Add(task);
                    }
                }
                else
                {
                    foreach (var task in tasks.OrderBy(x => double.Parse(x.position)).ToList()) //Default
                    {
                        //Add task to list
                        taskItem.Add(task);
                    }
                }

                // Remove any completed items if necessary
                //THIS IS REQUIRED TO HIDE COMPLETED WITH OFFLINE MODE
                if (GTaskSettings.HideCompleted)
                {
                    // Remove any hidden/completed tasks
                    List<TaskItem> tempList = taskItem.ToList();
                    tempList.RemoveAll(x => x.hidden == "True");
                    taskItem = new ObservableCollection<TaskItem>(tempList);
                }

                //Set the context to taskItem list
                var list = obj[1] as Action<ObservableCollection<TaskItem>>;
                if (list != null) list(taskItem);

                //Store the Count in Storage, if the liveTile exists then update it's count
                var settings = IsolatedStorageSettings.ApplicationSettings;
                string id = obj[0].ToString();

                //Create a completed list
                var NCtasks = new List<TaskItem>();
                NCtasks.Clear();
                NCtasks.AddRange(tasks.Where(y => y.status != "completed").ToList());

                //Count = Total Count
                //DueCount = Past Due w/ No Date Included
                //DueNDCount = Past Due w/ No Date Excluded
                //string minDT = GTaskSettings.convertDate(DateTime.MinValue.ToString());
                //string maxDT = GTaskSettings.convertDate(DateTime.MaxValue.ToString());
                //string nowDT = GTaskSettings.convertDate(DateTime.Now.ToString()); 

                settings["Count_" + id] = NCtasks.Count();
                settings["DueCount_" + id] = NCtasks.Where(y => (y.due != null ? DateTime.Parse(Universal.ConvertToUniversalDate(y.due)).AddHours(-12) : DateTime.MinValue) <= DateTime.Now).Count(); //Closest Last, No Date Included
                settings["DueNDCount_" + id] = NCtasks.Where(y => (y.due != null ? DateTime.Parse(Universal.ConvertToUniversalDate(y.due)).AddHours(-12) : DateTime.MaxValue) <= DateTime.Now).Count(); //Closest Last, No Date Excluded

                //Update the LiveTile counts if they exist
                var tile = ShellTile.ActiveTiles.FirstOrDefault(y => y.NavigationUri.ToString().Contains(id));
                if (tile != null)
                {
                    //most displays cut off the 16th charactor, this reduces it to 13 + ... for the Edit page and LiveTile
                    string liveTitle = settings["Title_" + id].ToString();
                    if (liveTitle.Length > 24)
                    {
                        liveTitle = liveTitle.Substring(0, 21) + "...";
                    }

                    //Conditional LiveTile Updates depending on the settings (past due w/ no due date, past due w/o no due date, total count
                    int count = 0;
                    if (GTaskSettings.LiveTileCount > 0)
                    {
                        if (GTaskSettings.IncludeNoDueDate)
                        {
                            count = (int)settings["DueCount_" + id];
                        }
                        else
                        {
                            count = (int)settings["DueNDCount_" + id];
                        }
                    }
                    else
                    {
                        count = (int)settings["Count_" + id];
                    }
                    var tileData = new StandardTileData { Title = liveTitle, BackgroundImage = new Uri("/Assets/Icons/202.png", UriKind.Relative), Count = count };
                    tile.Update(tileData);
                }
            }
            catch (Exception )//e)
            {
                if (GTaskSettings.MsgError)
                {
                    MessageBox.Show(message);
                }
            }
        }

        #endregion

        #region Get Specific Task Item

        public static async Task GetSpecificTaskList(string id, Action<TaskListItem> callback, bool refreshLocalStorage = false)
        {
            await RetryGetSpecificTaskList(new List<object> { id, callback }, refreshLocalStorage);
        }

        private static async Task RetryGetSpecificTaskList(IList<object> obj, bool refreshLocalStorage = false)
        {
            const string message = "Hakuna Matata! Can you try that again?";

            try
            {
                // Load the task lists from local storage
                List<TaskListItem> TaskListList = await GetTaskListFromApplicationStorage();

                // Create a tasklist item
                var item = new TaskListItem();

                // Get the appropriate task list if available
                if (TaskListList != null && TaskListList.Where(x => x.id == obj[0].ToString()).Count() > 0)
                {
                    item = (TaskListItem)TaskListList.First(x => x.id == obj[0].ToString());
                }

                var list = obj[1] as Action<TaskListItem>;
                if (list != null) list(item);

                //Store/Update Title
                //No need to update livetile here because this is when it goes into the TaskList
                //The loading sequence updates it if it exists
                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["Title_" + item.id] = item.title;
            }
            catch (Exception)// e)
            {
                if (GTaskSettings.MsgError)
                {
                    MessageBox.Show(message);
                }
            }
        }

        #endregion
    }

}
