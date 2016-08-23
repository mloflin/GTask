using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BackgroundAgent
{
    public class TaskHelper
    {
        private static string errorMSG = String.Empty;

        #region Delete

        public static async Task<bool> DeleteTask(string TaskListID, string Id)
        {
            //const string message = "Oops, I can't seem to Delete this Task, can you try again?";
            var restClient = new RestClient { Authenticator = new OAuth2UriQueryParameterAuthenticator(GTaskSettings.AccessToken), BaseUrl = "https://www.googleapis.com/tasks/v1/lists/" + TaskListID + "/tasks/" + Id, Timeout = GTaskSettings.RequestTimeout };
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

        #region Update

        public static async Task<bool> UpdateTask(TaskItem taskItem, Action<bool> response)
        {
            return await RetryUpdateTask(new List<object> { taskItem, response });
        }

        public static async Task<bool> RetryUpdateTask(IList<object> obj)
        {
           //const string message = "Whoops, I wasn't able to Update the Task,  can you try again?";

            var restRequest = new RestRequest(Method.PUT)
            {
                RequestFormat = DataFormat.Json,
                Resource = ((TaskItem)obj[0]).selfLink,
                Timeout = GTaskSettings.RequestTimeout
            };
            // Set the updated date for formatting purposes
            ((TaskItem)obj[0]).updated = DateTime.UtcNow.ToString("yyyy-MM-dd'T'hh:mm:ss.00Z");
            restRequest.AddBody(obj[0]);

            //Make the call
            var restResponse = await GTaskSettings.ExecuteRestTask(restRequest);
            try
            {
                if (restResponse.StatusCode == HttpStatusCode.OK)
                {
                    //success
                    return true;
                }
            }
            catch (Exception)// e)
            {
            }

            return false;
        }

        #endregion

        #region Add

        public static async Task<TaskItem> RetryAddTask(IList<object> obj)
        {
            //const string message = "Golly Gee, There was an error Creating the Task, can you try it again?";
            var restRequest = new RestRequest(Method.POST)
            {
                RequestFormat = DataFormat.Json,
                Resource = String.Format("https://www.googleapis.com/tasks/v1/lists/{0}/tasks", obj[2]),
                Timeout = GTaskSettings.RequestTimeout
            };
            string dueDate = string.Empty;
            if (!string.IsNullOrEmpty(((TaskItem)obj[0]).due))
            {
                TaskItem t = ((TaskItem)obj[0]);
                dueDate = ",due: \"" + t.due + "\"";
            }
            var info = "{title:\"" + ((TaskItem)obj[0]).title + "\",notes:\"" + ((TaskItem)obj[0]).notes + "\"" + dueDate + "}";
            info = info.Replace("\r", "\\n");
            restRequest.AddParameter("application/json", info, ParameterType.RequestBody);

            //Make the call
            var restResponse = await GTaskSettings.ExecuteRestTask(restRequest);
            try
            {
                if (restResponse.StatusCode == HttpStatusCode.OK)
                {
                    //Success
                    var m = JObject.Parse(restResponse.Content);
                    TaskItem newTask = new TaskItem((string)m.SelectToken("id"),
                                                        (string)m.SelectToken("kind"),
                                                        ((string)m.SelectToken("title")) ==
                                                        String.Empty
                                                            ? "Empty"
                                                            : (string)
                                                            m.SelectToken("title"),
                                                        ((TaskItem)obj[0]).notes,
                                                        obj[2].ToString(),
                                                        (string)
                                                        m.SelectToken("position"),
                                                        (string)m.SelectToken("updated"),
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
                                                    );

                    return newTask;
                }
            }
            catch
            {
            }

            return null;
        }

        #endregion

        #region Update a Task's Status

        public static async Task<bool> UpdateTaskStatus(string parentListId, string currentTaskId, string due, bool isChecked)
        {
            //const string message = "Fiddlesticks.. I wasn't able to Update the Task,  can you try again?";
            var restRequest = new RestRequest(Method.PUT)
            {
                RequestFormat = DataFormat.Json,
                Resource = String.Format("https://www.googleapis.com/tasks/v1/lists/{0}/tasks/{1}", parentListId, currentTaskId),
                Timeout = GTaskSettings.RequestTimeout
            };
            var check = (bool)isChecked ? "completed" : "needsAction";
            var dueDate = ",due: \"" + due + "\"";
            var param = string.Empty;

            //Conditional on if there is a due date or not, if there isn't it sets it to No Due Date automatically
            //if there is we send it to retain the date an item was completed

            if (due != null)
            {
                param = "{id:\"" + currentTaskId + "\",status:\"" + check + "\"" + dueDate + "}";
            }
            else
            {
                param = "{id:\"" + currentTaskId + "\",status:\"" + check + "\"}";
            }
            restRequest.AddParameter("application/json", param, ParameterType.RequestBody);

            //Make the call
            var restResponse = await GTaskSettings.ExecuteRestTask(restRequest);
            try
            {
                if (restResponse.StatusCode == HttpStatusCode.OK)
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

        #region Move a Task

        public static async Task<string> MoveTask(string TaskListID, string Id, string PrevID)
        {
            //const string message = "I'm having issues moving this task, can you try again?";

            var restRequest = new RestRequest(Method.POST)
            {
                Timeout = GTaskSettings.RequestTimeout
            };

            if (PrevID == "")
            {
                restRequest.RequestFormat = DataFormat.Json;
                restRequest.Resource = String.Format("https://www.googleapis.com/tasks/v1/lists/" + TaskListID + "/tasks/" + Id + "/move?");
            }
            else
            {
                restRequest.RequestFormat = DataFormat.Json;
                restRequest.Resource = String.Format("https://www.googleapis.com/tasks/v1/lists/" + TaskListID + "/tasks/" + Id + "/move?previous=" + PrevID);
            }

            restRequest.AddParameter("application/json", ParameterType.RequestBody);

            //Make the call
            var restResponse = await GTaskSettings.ExecuteRestTask(restRequest);

            try
            {
                if (restResponse.StatusCode == HttpStatusCode.OK)
                {
                    // Get the new position from the response
                    var m = JObject.Parse(restResponse.Content);
                    string position = (string)m.SelectToken("position");

                    //success
                    return position;
                }
            }
            catch
            {
            }

            // Get the locally stored list
            List<TaskListItem> TaskListList = await TaskListHelper.GetTaskListFromApplicationStorage(false);

            // Check that the task list and tasks are available in local storage
            if (TaskListList.Where(x => x.id == TaskListID).Count() == 0 ||
                TaskListList.Where(x => x.id == TaskListID).First().taskList.Where(x => x.id == Id).Count() == 0)
            {
                return null;
            }

            if (string.IsNullOrEmpty(PrevID))
            {
                // The item has been moved to the first position
                TaskListList.Where(x => x.id == TaskListID).First().taskList.Where(x => x.id == Id).First().position = "0";
                TaskListList.Where(x => x.id == TaskListID).First().taskList.Where(x => x.id == Id).First().updated = DateTime.UtcNow.ToString("yyyy-MM-dd'T'hh:mm:ss.00Z"); // DateTime.UtcNow.ToString();
            }
            else
            {
                // Check that the prev id item is available in local storage
                if (TaskListList.Where(x => x.id == TaskListID).First().taskList.Where(x => x.id == PrevID).Count() == 0)
                {
                    return null;
                }

                // get the position of the prev id item
                double position = double.Parse(TaskListList.Where(x => x.id == TaskListID).First().taskList.Where(x => x.id == PrevID).First().position);

                // Set the position of the item to 1+ the position of the previous item
                TaskListList.Where(x => x.id == TaskListID).First().taskList.Where(x => x.id == Id).First().position = (position + 1).ToString();
                TaskListList.Where(x => x.id == TaskListID).First().taskList.Where(x => x.id == Id).First().updated = DateTime.UtcNow.ToString("yyyy-MM-dd'T'hh:mm:ss.00Z"); // DateTime.UtcNow.ToString();
            }

            //submit the task list to local storage
            await TaskListHelper.SubmitToLocalStorage(TaskListList);

            return null;
        }

        #endregion
    }
}
