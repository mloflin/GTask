using RestSharp;
using System;
using System.IO.IsolatedStorage;
using System.Net;
using System.Threading.Tasks;

namespace BackgroundAgent
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


        #endregion

        #region Public Methods

        public static async Task<IRestResponse> ExecuteRestTask(RestRequest request, bool refrestToken = true, RestClient client = null)
        {
            // Execute a rest request with a timeout
            if (refrestToken)
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
        #endregion
    }
}
