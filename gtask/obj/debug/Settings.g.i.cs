﻿#pragma checksum "C:\Users\mloflin\Documents\Visual Studio 2015\Projects\gTask\gTask\Settings.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "29F54311831C19E43D58E364C0CBEB40"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace gTask {
    
    
    public partial class Settings : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal Microsoft.Phone.Controls.Pivot SettingsPivot;
        
        internal System.Windows.Controls.Grid TaskListSettings;
        
        internal System.Windows.DataTemplate TaskListItemTemplate;
        
        internal System.Windows.Controls.ScrollViewer TaskListScrollViewer;
        
        internal Microsoft.Phone.Controls.ListPicker ddlTaskListSort;
        
        internal Microsoft.Phone.Controls.ListPicker ddlLiveTileCount;
        
        internal System.Windows.Controls.CheckBox chkIncludeNoDueDate;
        
        internal System.Windows.Controls.CheckBox chkAutoRefreshList;
        
        internal System.Windows.Controls.Grid TaskView;
        
        internal System.Windows.DataTemplate TaskViewItemTemplate;
        
        internal System.Windows.Controls.ScrollViewer TaskViewScrollViewer;
        
        internal Microsoft.Phone.Controls.ListPicker ddlTaskSort;
        
        internal System.Windows.Controls.CheckBox chkNoDueDateAtTop;
        
        internal System.Windows.Controls.CheckBox chkDisableDragDrop;
        
        internal System.Windows.Controls.CheckBox chkHideNotes;
        
        internal System.Windows.Controls.CheckBox chkHideDueDate;
        
        internal System.Windows.Controls.CheckBox chkAutoClear;
        
        internal System.Windows.Controls.CheckBox chkAutoRefreshTasks;
        
        internal System.Windows.Controls.Grid TaskEdit;
        
        internal System.Windows.DataTemplate TaskEditItemTemplate;
        
        internal System.Windows.Controls.ScrollViewer TaskEditScrollViewer;
        
        internal Microsoft.Phone.Controls.ListPicker ddlDefaultReminder;
        
        internal System.Windows.Controls.CheckBox chkTomorrowIf;
        
        internal Microsoft.Phone.Controls.TimePicker lstTime;
        
        internal Microsoft.Phone.Controls.ListPicker ddlTaskEditTextSize;
        
        internal System.Windows.Controls.Grid Notifications;
        
        internal System.Windows.DataTemplate NotificationItemTemplate;
        
        internal System.Windows.Controls.ScrollViewer NotificationScrollViewer;
        
        internal System.Windows.Controls.TextBlock lblNotifications;
        
        internal System.Windows.Controls.CheckBox chkCreateTaskList;
        
        internal System.Windows.Controls.CheckBox chkUpdateTaskList;
        
        internal System.Windows.Controls.CheckBox chkCreateTask;
        
        internal System.Windows.Controls.CheckBox chkUpdateTask;
        
        internal System.Windows.Controls.CheckBox chkSavedSettings;
        
        internal System.Windows.Controls.Grid About;
        
        internal System.Windows.Controls.TextBlock lblVersion;
        
        internal System.Windows.Controls.TextBlock lblAppTitle;
        
        internal System.Windows.Controls.TextBlock txtAbout;
        
        internal System.Windows.Controls.Button btnBuy;
        
        internal System.Windows.Controls.Grid Feedback;
        
        internal System.Windows.Controls.TextBlock txtFeedback;
        
        internal System.Windows.Controls.Button btnTweet;
        
        internal System.Windows.Controls.Button btnEmail;
        
        internal System.Windows.Controls.Button btnRate;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/gTask;component/Settings.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.SettingsPivot = ((Microsoft.Phone.Controls.Pivot)(this.FindName("SettingsPivot")));
            this.TaskListSettings = ((System.Windows.Controls.Grid)(this.FindName("TaskListSettings")));
            this.TaskListItemTemplate = ((System.Windows.DataTemplate)(this.FindName("TaskListItemTemplate")));
            this.TaskListScrollViewer = ((System.Windows.Controls.ScrollViewer)(this.FindName("TaskListScrollViewer")));
            this.ddlTaskListSort = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("ddlTaskListSort")));
            this.ddlLiveTileCount = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("ddlLiveTileCount")));
            this.chkIncludeNoDueDate = ((System.Windows.Controls.CheckBox)(this.FindName("chkIncludeNoDueDate")));
            this.chkAutoRefreshList = ((System.Windows.Controls.CheckBox)(this.FindName("chkAutoRefreshList")));
            this.TaskView = ((System.Windows.Controls.Grid)(this.FindName("TaskView")));
            this.TaskViewItemTemplate = ((System.Windows.DataTemplate)(this.FindName("TaskViewItemTemplate")));
            this.TaskViewScrollViewer = ((System.Windows.Controls.ScrollViewer)(this.FindName("TaskViewScrollViewer")));
            this.ddlTaskSort = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("ddlTaskSort")));
            this.chkNoDueDateAtTop = ((System.Windows.Controls.CheckBox)(this.FindName("chkNoDueDateAtTop")));
            this.chkDisableDragDrop = ((System.Windows.Controls.CheckBox)(this.FindName("chkDisableDragDrop")));
            this.chkHideNotes = ((System.Windows.Controls.CheckBox)(this.FindName("chkHideNotes")));
            this.chkHideDueDate = ((System.Windows.Controls.CheckBox)(this.FindName("chkHideDueDate")));
            this.chkAutoClear = ((System.Windows.Controls.CheckBox)(this.FindName("chkAutoClear")));
            this.chkAutoRefreshTasks = ((System.Windows.Controls.CheckBox)(this.FindName("chkAutoRefreshTasks")));
            this.TaskEdit = ((System.Windows.Controls.Grid)(this.FindName("TaskEdit")));
            this.TaskEditItemTemplate = ((System.Windows.DataTemplate)(this.FindName("TaskEditItemTemplate")));
            this.TaskEditScrollViewer = ((System.Windows.Controls.ScrollViewer)(this.FindName("TaskEditScrollViewer")));
            this.ddlDefaultReminder = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("ddlDefaultReminder")));
            this.chkTomorrowIf = ((System.Windows.Controls.CheckBox)(this.FindName("chkTomorrowIf")));
            this.lstTime = ((Microsoft.Phone.Controls.TimePicker)(this.FindName("lstTime")));
            this.ddlTaskEditTextSize = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("ddlTaskEditTextSize")));
            this.Notifications = ((System.Windows.Controls.Grid)(this.FindName("Notifications")));
            this.NotificationItemTemplate = ((System.Windows.DataTemplate)(this.FindName("NotificationItemTemplate")));
            this.NotificationScrollViewer = ((System.Windows.Controls.ScrollViewer)(this.FindName("NotificationScrollViewer")));
            this.lblNotifications = ((System.Windows.Controls.TextBlock)(this.FindName("lblNotifications")));
            this.chkCreateTaskList = ((System.Windows.Controls.CheckBox)(this.FindName("chkCreateTaskList")));
            this.chkUpdateTaskList = ((System.Windows.Controls.CheckBox)(this.FindName("chkUpdateTaskList")));
            this.chkCreateTask = ((System.Windows.Controls.CheckBox)(this.FindName("chkCreateTask")));
            this.chkUpdateTask = ((System.Windows.Controls.CheckBox)(this.FindName("chkUpdateTask")));
            this.chkSavedSettings = ((System.Windows.Controls.CheckBox)(this.FindName("chkSavedSettings")));
            this.About = ((System.Windows.Controls.Grid)(this.FindName("About")));
            this.lblVersion = ((System.Windows.Controls.TextBlock)(this.FindName("lblVersion")));
            this.lblAppTitle = ((System.Windows.Controls.TextBlock)(this.FindName("lblAppTitle")));
            this.txtAbout = ((System.Windows.Controls.TextBlock)(this.FindName("txtAbout")));
            this.btnBuy = ((System.Windows.Controls.Button)(this.FindName("btnBuy")));
            this.Feedback = ((System.Windows.Controls.Grid)(this.FindName("Feedback")));
            this.txtFeedback = ((System.Windows.Controls.TextBlock)(this.FindName("txtFeedback")));
            this.btnTweet = ((System.Windows.Controls.Button)(this.FindName("btnTweet")));
            this.btnEmail = ((System.Windows.Controls.Button)(this.FindName("btnEmail")));
            this.btnRate = ((System.Windows.Controls.Button)(this.FindName("btnRate")));
        }
    }
}

