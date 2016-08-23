using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;


namespace gTask.Resources
{
    public static class RestClientExtensions
    {
        public static Task<IRestResponse> ExecuteTask(this IRestClient restClient, RestRequest restRequest)
        {
            try
            {
                var tcs = new TaskCompletionSource<IRestResponse>();
                restClient.Timeout = GTaskSettings.RequestTimeout;
                restRequest.Timeout = GTaskSettings.RequestTimeout;
                restClient.ExecuteAsync(restRequest, (restResponse, asyncHandle) =>
                    {
                        if (restResponse.ResponseStatus == ResponseStatus.Error)
                            tcs.SetException(restResponse.ErrorException);
                        else
                            tcs.SetResult(restResponse);
                    }
                );
                return tcs.Task;
            }
            catch (Exception)
            {

            }
            return null;
        }
    }
    public class LoginHelper
    {
        public static void SetLicense()
        {
            //Determine if IsFree based on Title
            Assembly asm = Assembly.GetExecutingAssembly();
            AssemblyTitleAttribute title = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyTitleAttribute));
            if (title.Title.Contains("+"))
            {
                GTaskSettings.IsFree = false;
            }
            else
            {
                GTaskSettings.IsFree = true;
            }
        }
        public static void SetVariables()
        {
            //Set the variables within the apps storage
            GTaskSettings.OAuthUrl = "https://accounts.google.com/o/oauth2/auth";
            GTaskSettings.OAuthTokenUrl = "https://accounts.google.com/o/oauth2/token";
            GTaskSettings.Scope = "https://www.googleapis.com/auth/tasks";
            GTaskSettings.LogOutURL = "https://accounts.google.com/logout";
            GTaskSettings.RedirectUri = "http://localhost";
            GTaskSettings.RequestTimeout = 10000;
            GTaskSettings.LiveTileCount = 1; //Due Not Completed
            GTaskSettings.IncludeNoDueDate = false;

            //###Temporary work around###
            //GTask was maxing out to 5k on GTasks API, I created a new project to hopefully increase the limit
            //The below will default to the new projects and spread it across both
            //LEGACY
            //GTaskSettings.ClientId = "805620880756.apps.googleusercontent.com";
            //GTaskSettings.ClientSecret = "bK-CKNPeYeRzP_mteyqiieV-";but if Random returns 3 (33% chance) it will continue to use the old project
            Random rnd = new Random();
            int random = rnd.Next(1, 3);
            if (random == 1)
            {
                GTaskSettings.ClientId = "624913567698-2fiqlir9ibp4go7imbdoqdgh3s7g7o1e.apps.googleusercontent.com";
                GTaskSettings.ClientSecret = "14vEBy2RPObWiYQU6eySl_hh";
            }
            else
            {
                GTaskSettings.ClientId = "401399692765-nad8brdsi63q3jnp0bl2an2v0r0kog83.apps.googleusercontent.com";
                GTaskSettings.ClientSecret = "wq4xGoGaBX9FsYC8UdfWqM7j";
            }

            //Update|Set License
            SetLicense();

            //When Debugging be able to dynamically set Free/Paid - else it is set by checking the license on Load
            if (Debugger.IsAttached)
            {
                MessageBoxResult m = MessageBox.Show("Debug Trial (OK = Free, Cancel = Paid)", "Debug", MessageBoxButton.OKCancel);
                if (m == MessageBoxResult.OK)
                {
                    GTaskSettings.IsFree = true;
                }
                else
                {
                    GTaskSettings.IsFree = false;
                }
            }

            //Set ApplicationTitle
            if (GTaskSettings.IsFree)
                GTaskSettings.ApplicationTitle = "GTask";
            else
                GTaskSettings.ApplicationTitle = "GTask+";

            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings["Timestamp"] = TimeSpan.FromTicks(DateTime.MinValue.Ticks).TotalSeconds;
        }

        public static async void Reminder()
        {
            //Update License (eg. If someone bought the app)
            SetLicense();

            //Set ApplicationTitle
            if (GTaskSettings.IsFree)
                GTaskSettings.ApplicationTitle = "GTask";
            else
                GTaskSettings.ApplicationTitle = "GTask+";

            //New Feature Popup (increment 1 each time you want it to show up)
            int NumFeature = 9;
            if (GTaskSettings.NewFeatureCount < NumFeature)
            {
                MessageBox.Show("- Offline Mode \r\n- Speech-to-Text \r\n- New Icons \r\n- Auto-Sync\r\n- More Customizations in Settings", "New Features!", MessageBoxButton.OK);

                if (GTaskSettings.IsFree)
                {
                    MessageBox.Show("To get the latest data from Google use the 'Sync' button.\r\n \r\nYou can enable Auto-Sync by upgrading to the Paid version\r\n \r\nSend me any questions, feedback, or issues:\r\n   - Tweet @MattLoflin\r\n   - MLoflin.Apps@gmail.com", "Instructions", MessageBoxButton.OK);
                }
                else
                {
                    MessageBox.Show("To get the latest data from Google use the 'Sync' button OR enable Auto-Sync in settings.\r\n \r\nSend me any questions, feedback, or issues:\r\n   - Tweet @MattLoflin\r\n   - MLoflin.Apps@gmail.com", "Instructions", MessageBoxButton.OK);
                }

                //due to possible data issues with localization changes - force update of Google data
                //Remove LocalStorage (if exists)
                string ApplicationDataFileName = "TaskListData.txt";

                IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                if (storage.FileExists(ApplicationDataFileName))
                {
                    storage.DeleteFile(ApplicationDataFileName);
                }

                //Set First run to true for forced sync
                GTaskSettings.FirstRun = true;


                if (GTaskSettings.IsFree) //if free then turn off 'auto' features
                {
                    GTaskSettings.AutoClear = false;
                    GTaskSettings.AutoRefreshLists = false;
                    GTaskSettings.AutoRefreshTasks = false;
                }

                //If the user already installed the app and HideCompleted = true then remove hidden to speed up processing
                //localStorageList should be null if not used yet
                List<Model.TaskListItem> localStorageList = await TaskListHelper.GetTaskListFromApplicationStorage(false);
                if (GTaskSettings.HideCompleted == true && (localStorageList != null || localStorageList.Count == 0))
                {
                    bool results = await GTaskSettings.RemoveHidden();
                }

                //Check for pinned lists - if pinned then prompt user to re-pin
                bool pinned = false;
                foreach (ShellTile shellTile in ShellTile.ActiveTiles)
                {
                    try
                    {
                        if (shellTile.NavigationUri.ToString().Contains("/Views/TaskView.xaml?Id="))
                        {
                            pinned = true;   
                        }
                    }
                    catch (Exception)// e)
                    {

                    }
                }

                if (pinned)
                {
                    MessageBox.Show("I noticed that you have Task List(s) pinned to the Start Screen.\r\n \r\nPlease Re-pin any Task List(s) to enable updated features.", "Action Required", MessageBoxButton.OK);
                }
                
                //update NewFeatureCount to latest so user doesn't see this again
                GTaskSettings.NewFeatureCount = NumFeature;
            }
            //Only try to do one popup per session - if the new feature popup isn't being used, then do others (if applicable)
            else
            {
                //
                //##RATE ME POPUP##
                //
                int daysElapsed = (int)(DateTime.Now.Subtract(GTaskSettings.ReminderDate).TotalDays);
                if (daysElapsed > 7 && !GTaskSettings.Rated)
                {
                    MessageBoxResult m = MessageBox.Show("Hey! I see you have been using the app for a while now, could you click 'OK' to Rate it or Send me feedback through 'Settings'->'About'?", "Feedback?", MessageBoxButton.OKCancel);
                    if (m == MessageBoxResult.OK)
                    {
                        //Navigate to Rating Page
                        MarketplaceReviewTask oRateTask = new MarketplaceReviewTask();
                        oRateTask.Show();

                        //Set Rated = True (assuming they rated by clicking OK)
                        GTaskSettings.Rated = true;
                    }
                    else
                    {
                        //Set Reminder Date to Today to remind again in 7 days
                        GTaskSettings.ReminderDate = DateTime.Now;
                    }
                }
            }
            //If this was added after someone logged it, the value wouldn't be set until they login again
            //Given the default return is MaxValue - we can catch it and reset it to Today's date
            if (GTaskSettings.ReminderDate == DateTime.MaxValue)
                GTaskSettings.ReminderDate = DateTime.Now;
            
        }


        public static Uri GetLoginUrl()
        {
            return new Uri(string.Format("{0}?client_id={1}&redirect_uri={2}&scope={3}&response_type=code",
                                         GTaskSettings.OAuthUrl,
                                         GTaskSettings.ClientId, GTaskSettings.RedirectUri, GTaskSettings.Scope));
        }

        public static async Task RefreshTokenCodeAwait(bool isRefresh)
        {
            //const string message = "I can't get ahold of Google... can you try again?";
            var secondsElapsed = TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds - GTaskSettings.Timestamp + 60;
            if (secondsElapsed > GTaskSettings.ExpiresIn)
            {
                //Set variables for the body
                var body = String.Format("client_id={0}&client_secret={1}&", GTaskSettings.ClientId, GTaskSettings.ClientSecret);
                var postBody = string.Empty;
                if (isRefresh)
                {
                    postBody = String.Format(body + "refresh_token={0}&grant_type=refresh_token", GTaskSettings.RefreshToken); 
                }
                else
                {
                    postBody =String.Format(body + "code={0}&redirect_uri={1}&grant_type=authorization_code", GTaskSettings.TokenType, GTaskSettings.RedirectUri); 
                }

                //Create the restclient and request
                var restClient = new RestClient();
                var restRequest = new RestRequest(Method.POST)
                {
                    Resource = GTaskSettings.OAuthTokenUrl,
                    Timeout = GTaskSettings.RequestTimeout
                };
                restRequest.AddParameter("application/x-www-form-urlencoded", postBody, ParameterType.RequestBody);
                    
                //Make the call
                var restResponse = await restClient.ExecuteTask(restRequest);
                if (restResponse.Content != "")
                {
                    try
                    {
                        var AuthString = JObject.Parse(restResponse.Content);
                        //Update the token, expires in, and refresh token
                        GTaskSettings.AccessToken = (string)AuthString.SelectToken("access_token");
                        GTaskSettings.ExpiresIn = (int)AuthString.SelectToken("expires_in");
                        JToken jToken;
                        if (AuthString.TryGetValue("refresh_token", out jToken))
                            GTaskSettings.RefreshToken = jToken.Value<string>();
                    }
                    catch
                    {
                        //Don't need anymore since offline mode detects this
                        //if (GTaskSettings.MsgError)
                        //{
                        //MessageBox.Show(message);
                        //}
                    }
                }
            }
        }
    }
}
