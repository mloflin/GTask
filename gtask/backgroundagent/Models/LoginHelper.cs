using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace BackgroundAgent
{
    public static class RestClientExtensions
    {
        public static Task<IRestResponse> ExecuteTask(this IRestClient restClient, RestRequest restRequest)
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
            });
            return tcs.Task;
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
                    postBody = String.Format(body + "code={0}&redirect_uri={1}&grant_type=authorization_code", GTaskSettings.TokenType, GTaskSettings.RedirectUri);
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
                }
            }
        }
    }
}
