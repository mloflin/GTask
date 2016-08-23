using gTask.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;

namespace gTask
{
    public partial class Login : PhoneApplicationPage
    {
        public Login()
        {
            InitializeComponent();

            //Login or Logout depending on current state
            if (GTaskSettings.IsLoggedIn())
            {
                try
                {


                    webBrowserGoogleLogin.Navigate(new Uri(GTaskSettings.LogOutURL, UriKind.RelativeOrAbsolute));

                    //Remove Stored Information
                    GTaskSettings.ClearSession();

                    //Remove LocalStorage (if exists)
                    string ApplicationDataFileName = "TaskListData.txt";

                    IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                    if (storage.FileExists(ApplicationDataFileName))
                    {
                        storage.DeleteFile(ApplicationDataFileName);
                    }

                    //Remove LiveTiles
                    List<ShellTile> listST = new List<ShellTile>();
                    foreach (ShellTile shellTile in ShellTile.ActiveTiles)
                    {
                        if (shellTile.NavigationUri.ToString().Contains("/Views/TaskView.xaml?Id="))
                        {
                            shellTile.Delete();
                        }
                    }

                    MessageBoxResult msgbox = MessageBox.Show("You have successfully logged out.");
                    Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute)));
                }
                catch (Exception)
                {
                    MessageBox.Show("[4] Take a screenshot and send it to @MattLoflin or MLoflin.Apps@gmail.com");
                }
            }
            else
            {
                webBrowserGoogleLogin.Navigate(LoginHelper.GetLoginUrl());
            }
        }

        private void webBrowserGoogleLogin_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            webBrowserGoogleLogin.Visibility = Visibility.Visible;
        }

        private async void webBrowserGoogleLogin_Navigating(object sender, NavigatingEventArgs e)
        {
            if (e.Uri.Query.ToString().Contains("code="))
            {                
                e.Cancel = true;

                //Set the Token Type
                var TokenType = e.Uri.Query.Substring(6, e.Uri.Query.Length - 6);
                GTaskSettings.TokenType = TokenType;

                //minimize Google Browser
                webBrowserGoogleLogin.Visibility = System.Windows.Visibility.Collapsed;

                //Call Google to get the real token (not refresh)
                await LoginHelper.RefreshTokenCodeAwait(false);

                //Set ReminderDate for Rating
                GTaskSettings.ReminderDate = DateTime.Now;

                //Navigate back home
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));

            }
        }
    }
}