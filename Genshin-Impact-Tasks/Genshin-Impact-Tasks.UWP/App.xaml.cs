using Genshin_Impact_Tasks.Models;

using SQLite;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Genshin_Impact_Tasks.UWP
{
    sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            using (var database = new SQLiteConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "git.db"), 
                SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.ProtectionComplete))
            {
                database.CreateTable<SettingTable>();

                #region 테마 초기화
                if (database.Table<SettingTable>().ToList().Where(s => s.Key == "Theme").Count() == 0)
                {
                    var theme = new SettingTable { Key = "Theme", Value = "System" };
                    database.Insert(theme);
                }

                switch (database.Table<SettingTable>().ToList().Where(s => s.Key == "Theme").FirstOrDefault().Value)
                {
                    case "Light":
                        Current.RequestedTheme = ApplicationTheme.Light;
                        break;
                    case "Dark":
                        Current.RequestedTheme = ApplicationTheme.Dark;
                        break;
                }
                #endregion

                #region 자동 시작 초기화
                if (database.Table<SettingTable>().ToList().Where(s => s.Key == "StartupTask").Count() == 0)
                {
                    var st = new SettingTable { Key = "StartupTask", Value = "Off" };
                    database.Insert(st);
                }

                var startupTask = GetStartupTask().Result;

                if (!((startupTask.State == StartupTaskState.Enabled) || (startupTask.State == StartupTaskState.EnabledByPolicy)))
                {
                    var stDb = database.Table<SettingTable>().ToList().Where(s => s.Key == "StartupTask").FirstOrDefault();
                    stDb.Value = "Off";
                    database.Update(stDb);

                    Genshin_Impact_Tasks.App.UseStartupTask = false;
                }
                else
                {
                    var stDb = database.Table<SettingTable>().ToList().Where(s => s.Key == "StartupTask").FirstOrDefault();
                    stDb.Value = "On";
                    database.Update(stDb);

                    Genshin_Impact_Tasks.App.UseStartupTask = true;
                }
                #endregion
            }

            this.Suspending += OnSuspending;
        }

        private async Task<StartupTask> GetStartupTask()
        {
            return await StartupTask.GetAsync("StartupGenshinImpactTasks");
        }

        protected override void OnActivated(IActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated) { }

                Window.Current.Content = rootFrame;
            }

            var staea = e as StartupTaskActivatedEventArgs;
            rootFrame.Navigate(typeof(MainPage), staea.TaskId);
            Window.Current.Activate();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            #if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
            #endif

            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Rg.Plugins.Popup 초기화
                Rg.Plugins.Popup.Popup.Init();

                List<Assembly> assembliesToInclude = (List<Assembly>)Rg.Plugins.Popup.Popup.GetExtraAssemblies();
                assembliesToInclude.Add(typeof(Syncfusion.XForms.UWP.TreeView.SfTreeViewRenderer).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(Syncfusion.XForms.UWP.TabView.SfTabViewRenderer).GetTypeInfo().Assembly);

                Xamarin.Forms.Forms.Init(e, assembliesToInclude);

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }

                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            Window.Current.Activate();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            deferral.Complete();
        }
    }
}
