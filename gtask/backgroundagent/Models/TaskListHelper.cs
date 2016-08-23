using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.Storage;

namespace BackgroundAgent
{
    public class TaskListHelper
    {
        private const string ApplicationDataFileName = "TaskListData.txt";

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
        public static async Task<List<TaskListItem>> LoadTaskDataFromApi(bool alertWhenNoConnection = false)
        {
            try
            {
                await LoginHelper.RefreshTokenCodeAwait(true);

                var settings = IsolatedStorageSettings.ApplicationSettings;

                // Get the Task Lists
                #region Get Task Lists
                var restRequest = new RestRequest(Method.GET)
                {
                    Resource =
                        "https://www.googleapis.com/tasks/v1/users/@me/lists?access_token=" + GTaskSettings.AccessToken,
                    Timeout = GTaskSettings.RequestTimeout
                };

                //Make the call
                var restResponse = await GTaskSettings.ExecuteRestTask(restRequest, false);

                //Store Lists
                var TaskListObject = JObject.Parse(restResponse.Content);

                var TaskListList = TaskListObject["items"].Select(m => new TaskListItem((string)m.SelectToken("title"),
                                (string)m.SelectToken("id"),
                                (string)m.SelectToken("kind"),
                                (string)m.SelectToken("selfLink"),
                                (string)m.SelectToken("updated"))).ToList();

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
                    TaskListList.AddRange(TaskListObject["items"].Select(m => new TaskListItem((string)m.SelectToken("title"),
                                        (string)m.SelectToken("id"),
                                        (string)m.SelectToken("kind"),
                                        (string)m.SelectToken("selfLink"),
                                        (string)m.SelectToken("updated"))));

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
                        
                    }
                }
                #endregion

                return TaskListList;
            }
            catch (Exception)//e)
            {

                return null;

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
                using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(ApplicationDataFileName))
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        taskListList = ((JArray)JsonConvert.DeserializeObject(sr.ReadToEnd())).ToObject<List<TaskListItem>>();
                    }
                }
            }
            catch (Exception)// e)
            {

            }

            return taskListList;
        }


        #region Create List

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
                if (restResponse.StatusCode == HttpStatusCode.NoContent)
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
    }
}
