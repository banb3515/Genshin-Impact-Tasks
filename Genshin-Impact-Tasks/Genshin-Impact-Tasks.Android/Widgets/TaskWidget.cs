using Genshin_Impact_Tasks.Droid.Services;
using Genshin_Impact_Tasks.Models;

using Firebase.Database.Query;
using Firebase.Database;

using SQLite;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;

using Xamarin.Essentials;

namespace Genshin_Impact_Tasks.Droid.Widgets
{
    [BroadcastReceiver(Label = "할 일")]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    [IntentFilter(new string[] { "com.banb.genshin_impact_tasks.ACTION_WIDGET_REFRESH" })]
    [IntentFilter(new string[] { "com.banb.genshin_impact_tasks.ACTION_WIDGET_PREVIOUS" })]
    [IntentFilter(new string[] { "com.banb.genshin_impact_tasks.ACTION_WIDGET_FORWARD" })]
    [IntentFilter(new string[] { "com.banb.genshin_impact_tasks.ACTION_WIDGET_LIST_ITEM" })]
    [MetaData("android.appwidget.provider", Resource = "@xml/task_widget_provider")]
    public class TaskWidget : AppWidgetProvider
    {
        #region Action
        public const string ACTION_WIDGET_REFRESH = "REFRESH";
        public const string ACTION_WIDGET_PREVIOUS = "PREVIOUS";
        public const string ACTION_WIDGET_FORWARD = "FORWARD";
        public const string ACTION_WIDGET_LIST_ITEM = "LIST_ITEM";
        #endregion

        #region Database
        public static SQLiteConnection Database { get; set; }

        public static SQLiteOpenFlags OpenFlags = SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.ProtectionComplete;

        public static readonly string DbFileName = "git.db"; // 데이터베이스 파일 이름

        public static readonly string DbFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DbFileName); // 데이터베이스 파일 경로
        #endregion

        #region Firebase
        public static FirebaseClient Firebase { get; set; } = null;
        public static readonly string FirebaseUrl = "YOUR FIREBASE REALTIME DATABASE URL";
        public static readonly string FirebaseSecretKey = "YOUR FIREBASE SECRET KEY";
        #endregion

        public static List<TaskModel> Items { get; set; }

        private static Toast ToastMsg { get; set; } // Toast

        private static DateTime RecentSyncDate { get; set; } // 최근 동기화 시간

        private static bool Progress { get; set; } = false;

        private static bool Synchronizing { get; set; } = false; // 동기화 작업 중

        private static bool Working { get; set; } = false; // 작업 중

        private static float Opacity { get; set; }

        private static int Theme { get; set; }

        public static int CurrentTask { get; set; } = 0; // 0: Daily, 1: Weekly, 2: OneTime

        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            base.OnUpdate(context, appWidgetManager, appWidgetIds);

            RefreshRemoteViews(context, appWidgetManager, appWidgetIds);
        }

        public override async void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);

            var action = intent.Action;
            var refresh = false;

            #region Database 초기화
            if (Database == null)
            {
                Database = new SQLiteConnection(DbFilePath, OpenFlags);

                Database.CreateTable<DailyTaskTable>();
                Database.CreateTable<WeeklyTaskTable>();
                Database.CreateTable<OneTimeTaskTable>();
            }
            #endregion

            // Toast 초기화
            if (ToastMsg == null)
                ToastMsg = Toast.MakeText(context, "", ToastLength.Long);

            #region 설정 초기화
            var vibration = Database.Table<SettingTable>().ToList().Where(s => s.Key == "Vibration").FirstOrDefault().Value == "On";
            var syncMail = Database.Table<SettingTable>().ToList().Where(s => s.Key == "SyncMail").FirstOrDefault().Value;
            var sync = syncMail != "";
            #endregion

            #region 위젯 업데이트
            if (action.Equals(AppWidgetManager.ActionAppwidgetUpdate))
            {
                Opacity = intent.GetFloatExtra("Opacity", 1.0f);
                Theme = intent.GetIntExtra("Theme", Convert.ToInt32(0x000000));

                refresh = true;
            }
            #endregion

            #region 이전 버튼 클릭
            else if (action.Equals(ACTION_WIDGET_PREVIOUS))
            {
                if (CurrentTask == 0) CurrentTask = 3;

                CurrentTask--;

                refresh = true;
            }
            #endregion

            #region 다음 버튼 클릭
            else if (action.Equals(ACTION_WIDGET_FORWARD))
            {
                if (CurrentTask == 2) CurrentTask = -1;

                CurrentTask++;

                refresh = true;
            }
            #endregion

            #region 새로고침 버튼 클릭
            else if (action.Equals(ACTION_WIDGET_REFRESH) && sync)
            {
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    if (Firebase == null)
                    {
                        var options = new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(FirebaseSecretKey) };
                        Firebase = new FirebaseClient(FirebaseUrl, options);
                    }

                    await AutoSync(context, syncMail);
                }
                else
                {
                    if (Firebase != null)
                    {
                        Firebase.Dispose();
                        Firebase = null;
                    }

                    ShowToast("인터넷에 연결되어 있지 않아 서버에서 데이터를 가져올 수 없습니다.");
                }
            }
            #endregion

            #region 할 일 아이템 탭
            else if (action.Equals(ACTION_WIDGET_LIST_ITEM) && !Working)
            {
                Working = true;

                var itemNo = intent.GetIntExtra("ItemNo", -1);
                if (itemNo == -1)
                {
                    Working = false;
                    return;
                }

                if (vibration) Vibration.Vibrate(TimeSpan.FromMilliseconds(50));

                if (sync && Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    ShowToast("인터넷에 연결되어 있지 않아 완료 상태를 변경할 수 없습니다.\n" +
                        "인터넷 상태를 확인하신 후 다시 시도해주시기 바랍니다.");
                    Working = false;
                    return;
                }

                if ((sync && await CheckChangedServerData(syncMail)) || CheckChangedLocalData())
                {
                    ShowToast("변경된 데이터가 있습니다.\n새로 고침 후 다시 시도해 주시기 바랍니다.");
                    Working = false;
                    return;
                }

                if (sync)
                {
                    Progress = true;

                    Refresh(context);
                }

                switch (CurrentTask)
                {
                    case 0:
                        if (sync)
                        {
                            var update = (await Firebase.Child("UserData").Child(syncMail.Replace('.', '_')).Child("DailyTask").OnceAsync<DailyTaskTable>()).Where(item => item.Object.No == itemNo).FirstOrDefault();
                            update.Object.Status = !update.Object.Status;
                            await Firebase.Child("UserData").Child(syncMail.Replace('.', '_')).Child("DailyTask").Child(update.Key).PutAsync(update.Object);
                            UpdateServerTasksSyncDate(syncMail);
                        }

                        var dailyDb = Database.Table<DailyTaskTable>().ToList().Where(t => t.No == itemNo).FirstOrDefault();
                        dailyDb.Status = !dailyDb.Status;
                        Database.Update(dailyDb);
                        break;
                    case 1:
                        if (sync)
                        {
                            var update = (await Firebase.Child("UserData").Child(syncMail.Replace('.', '_')).Child("WeeklyTask").OnceAsync<WeeklyTaskTable>()).Where(item => item.Object.No == itemNo).FirstOrDefault();
                            update.Object.Status = !update.Object.Status;
                            await Firebase.Child("UserData").Child(syncMail.Replace('.', '_')).Child("WeeklyTask").Child(update.Key).PutAsync(update.Object);
                            UpdateServerTasksSyncDate(syncMail);
                        }

                        var weeklyDb = Database.Table<WeeklyTaskTable>().ToList().Where(t => t.No == itemNo).FirstOrDefault();
                        weeklyDb.Status = !weeklyDb.Status;
                        Database.Update(weeklyDb);
                        break;
                    case 2:
                        if (sync)
                        {
                            var update = (await Firebase.Child("UserData").Child(syncMail.Replace('.', '_')).Child("OneTimeTask").OnceAsync<OneTimeTaskTable>()).Where(item => item.Object.No == itemNo).FirstOrDefault();
                            update.Object.Status = !update.Object.Status;
                            await Firebase.Child("UserData").Child(syncMail.Replace('.', '_')).Child("OneTimeTask").Child(update.Key).PutAsync(update.Object);
                            UpdateServerTasksSyncDate(syncMail);
                        }

                        var oneTimeDb = Database.Table<OneTimeTaskTable>().ToList().Where(t => t.No == itemNo).FirstOrDefault();
                        oneTimeDb.Status = !oneTimeDb.Status;
                        Database.Update(oneTimeDb);
                        break;
                }

                refresh = true;
                Working = false;

                if (sync) Progress = false;
            }
            #endregion

            // 새로고침
            if (refresh || action.Equals(ACTION_WIDGET_REFRESH)) Refresh(context);
        }

        #region RemoteViews 새로고침
        private void RefreshRemoteViews(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            for (int i = 0; i < appWidgetIds.Length; i++)
            {
                var remoteViews = UpdateWidget(context, appWidgetIds[i], appWidgetIds);
                appWidgetManager.UpdateAppWidget(appWidgetIds[i], remoteViews);
            }
        }
        #endregion

        #region 위젯 업데이트
        public static RemoteViews UpdateWidget(Context context, int appWidgetId, int[] appWidgetIds)
        {
            var remoteViews = new RemoteViews(context.PackageName, Resource.Layout.task_widget);

            var intent = new Intent(Intent.ActionMain);
            intent.AddCategory(Intent.CategoryLauncher);
            intent.SetComponent(new ComponentName(context, Java.Lang.Class.FromType(typeof(MainActivity))));

            var pending = PendingIntent.GetActivity(context, 0, intent, 0);

            remoteViews.SetOnClickPendingIntent(Resource.Id.widget_layout, pending);
            remoteViews.SetOnClickPendingIntent(Resource.Id.previous_button, GetPendingSelfIntent(context, ACTION_WIDGET_PREVIOUS));
            remoteViews.SetOnClickPendingIntent(Resource.Id.forward_button, GetPendingSelfIntent(context, ACTION_WIDGET_FORWARD));
            remoteViews.SetOnClickPendingIntent(Resource.Id.refresh_button, GetPendingSelfIntent(context, ACTION_WIDGET_REFRESH));

            var svcIntent = new Intent(context, typeof(TaskWidgetService));
            svcIntent.SetPackage(context.PackageName);
            svcIntent.PutExtra(AppWidgetManager.ExtraAppwidgetId, appWidgetId);
            svcIntent.SetData(Android.Net.Uri.Parse(svcIntent.ToUri(IntentUriType.AndroidAppScheme)));

            var itemIntent = new Intent(context, typeof(TaskWidget));
            itemIntent.PutExtra(AppWidgetManager.ExtraAppwidgetId, appWidgetIds);
            itemIntent.SetData(Android.Net.Uri.Parse(svcIntent.ToUri(IntentUriType.AndroidAppScheme)));
            itemIntent.SetAction(ACTION_WIDGET_LIST_ITEM);

            var itemPending = PendingIntent.GetBroadcast(context, 0, itemIntent, 0);
            remoteViews.SetPendingIntentTemplate(Resource.Id.task_listview, itemPending);

            var task = "";
            switch (CurrentTask)
            {
                case 0:
                    task = context.GetText(Resource.String.daily_task);
                    break;
                case 1:
                    task = context.GetText(Resource.String.weekly_task);
                    break;
                case 2:
                    task = context.GetText(Resource.String.one_time_task);
                    break;
            }

            remoteViews.SetInt(Resource.Id.widget_layout, "setBackgroundColor", (int)(Opacity * 0xFF) << 24 | Theme);
            // 어두운 테마
            if (Theme == Convert.ToInt32(0x000000))
            {
                remoteViews.SetTextColor(Resource.Id.task, Android.Graphics.Color.White);
                remoteViews.SetInt(Resource.Id.divider, "setBackgroundColor", Android.Graphics.Color.White);
            }
            // 밝은 테마
            else
            {
                remoteViews.SetTextColor(Resource.Id.task, Android.Graphics.Color.Black);
                remoteViews.SetInt(Resource.Id.divider, "setBackgroundColor", Android.Graphics.Color.Black);
            }
            remoteViews.SetTextViewText(Resource.Id.task, task);
            remoteViews.SetEmptyView(Resource.Id.task_listview, Resource.Id.empty_view);
            remoteViews.SetRemoteAdapter(Resource.Id.task_listview, svcIntent);
            if (Progress) remoteViews.SetViewVisibility(Resource.Id.progress_layout, Android.Views.ViewStates.Visible);
            else remoteViews.SetViewVisibility(Resource.Id.progress_layout, Android.Views.ViewStates.Gone);

            return remoteViews;
        }
        #endregion

        #region GetPendingSelfIntent
        public static PendingIntent GetPendingSelfIntent(Context context, string action)
        {
            var intent = new Intent(context, typeof(TaskWidget));
            intent.SetAction(action);
            return PendingIntent.GetBroadcast(context, 0, intent, 0);
        }
        #endregion

        #region 로컬 할 일 동기화 날짜 업데이트
        public static async void UpdateLocalTasksSyncDate(string syncMail)
        {
            var server = (await Firebase.Child("UserData").Child(syncMail.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).Where(item => item.Object.Key == "TasksSyncDate").FirstOrDefault();
            var local = Database.Table<SettingTable>().ToList().Where(s => s.Key == "TasksSyncDate").FirstOrDefault();
            local.Value = server.Object.Value;
            Database.Update(local);
        }
        #endregion

        #region 서버 할 일 동기화 날짜 업데이트
        public static async void UpdateServerTasksSyncDate(string syncMail)
        {
            var date = DateTime.Now.ToString();

            var local = Database.Table<SettingTable>().ToList().Where(s => s.Key == "TasksSyncDate").FirstOrDefault();
            local.Value = date;
            Database.Update(local);

            var server = (await Firebase.Child("UserData").Child(syncMail.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).Where(item => item.Object.Key == "TasksSyncDate").FirstOrDefault();
            server.Object.Value = date;
            await Firebase.Child("UserData").Child(syncMail.Replace('.', '_')).Child("Setting").Child(server.Key).PutAsync(server.Object);
        }
        #endregion

        #region 새로고침
        private void Refresh(Context context)
        {
            var appWidgetManager = AppWidgetManager.GetInstance(context);
            var componentName = new ComponentName(context.PackageName, Class.Name);
            var appWidgetIds = appWidgetManager.GetAppWidgetIds(componentName);
            appWidgetManager.NotifyAppWidgetViewDataChanged(appWidgetIds, Resource.Id.task_listview);
            RefreshRemoteViews(context, appWidgetManager, appWidgetIds);
        }
        #endregion

        #region Toast 표시
        private void ShowToast(string msg)
        {
            ToastMsg.SetText($"# 원신 태스크 - 할 일 위젯 #\n{msg}");
            ToastMsg.Show();
        }
        #endregion

        #region 변경된 서버 데이터 확인
        private async Task<bool> CheckChangedServerData(string syncMail)
        {
            var localSyncDateStr = Database.Table<SettingTable>().ToList().Where(s => s.Key == "TasksSyncDate").FirstOrDefault().Value;
            DateTime localSyncDate = DateTime.Now;

            bool pass = false;

            if (localSyncDateStr == "")
                pass = true;
            else
                localSyncDate = Convert.ToDateTime(localSyncDateStr);

            var serverSyncDate = Convert.ToDateTime((await Firebase.Child("UserData").Child(syncMail.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).Where(item => item.Object.Key == "TasksSyncDate").FirstOrDefault().Object.Value);

            return !(!pass && DateTime.Compare(localSyncDate, serverSyncDate) >= 0);
        }
        #endregion

        #region 변경된 로컬 데이터 확인
        private bool CheckChangedLocalData()
        {
            var tmpList = new List<TaskModel>();

            switch (CurrentTask)
            {
                case 0:
                    Database.Table<DailyTaskTable>().ToList().ForEach(task =>
                    { tmpList.Add(new TaskModel { No = task.No, Content = task.Content, IconPath = task.IconPath, Status = task.Status }); });
                    break;
                case 1:
                    Database.Table<WeeklyTaskTable>().ToList().ForEach(task =>
                    { tmpList.Add(new TaskModel { No = task.No, Content = task.Content, IconPath = task.IconPath, Status = task.Status }); });
                    break;
                case 2:
                    Database.Table<OneTimeTaskTable>().ToList().ForEach(task =>
                    { if (!task.Status) Items.Add(new TaskModel { No = task.No, Content = task.Content, IconPath = task.IconPath, Status = task.Status }); });
                    break;
            }

            return !Items.SequenceEqual(tmpList, new TaskComparer());
        }
        #endregion

        #region 동기화
        private async Task AutoSync(Context context, string syncMail)
        {
            try
            {
                if (Synchronizing) return;

                var now = DateTime.Now;
                if (DateTime.Compare(RecentSyncDate.AddSeconds(10), now) > 0)
                {
                    ShowToast($"{Convert.ToInt32((RecentSyncDate.AddSeconds(10) - now).TotalSeconds)}초 후 동기화가 가능합니다.");
                    return;
                }

                Synchronizing = true;
                Progress = true;

                var toast = Toast.MakeText(context, "", ToastLength.Short);
                
                if (!await CheckChangedServerData(syncMail))
                {
                    Synchronizing = false;
                    Progress = false;
                    RecentSyncDate = DateTime.Now;
                    return;
                }

                Refresh(context);

                toast.SetText("# 원신 태스크 - 할 일 위젯 #\n서버에서 데이터를 가져오는 중입니다.");
                toast.Show();

                (await Firebase.Child("UserData").Child(syncMail.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).ToList().ForEach(item =>
                {
                    if (item.Object.Key != "Theme" && item.Object.Key != "Vibration" && item.Object.Key != "SyncMail" && item.Object.Key != "Version")
                        Database.Update(item.Object);
                });
                Database.DropTable<DailyTaskTable>();
                Database.DropTable<WeeklyTaskTable>();
                Database.DropTable<OneTimeTaskTable>();

                Database.CreateTable<DailyTaskTable>();
                Database.CreateTable<WeeklyTaskTable>();
                Database.CreateTable<OneTimeTaskTable>();

                (await Firebase.Child("UserData").Child(syncMail.Replace('.', '_')).Child("DailyTask").OnceAsync<DailyTaskTable>()).ToList().ForEach(item =>
                {
                    Database.Insert(item.Object);
                });

                (await Firebase.Child("UserData").Child(syncMail.Replace('.', '_')).Child("WeeklyTask").OnceAsync<WeeklyTaskTable>()).ToList().ForEach(item =>
                {
                    Database.Insert(item.Object);
                });
                (await Firebase.Child("UserData").Child(syncMail.Replace('.', '_')).Child("OneTimeTask").OnceAsync<OneTimeTaskTable>()).ToList().ForEach(item =>
                {
                    Database.Insert(item.Object);
                });

                UpdateLocalTasksSyncDate(syncMail);

                Synchronizing = false;
                Progress = false;
                RecentSyncDate = DateTime.Now;

                Refresh(context);

                toast.SetText("# 원신 태스크 - 할 일 위젯 #\n동기화가 완료되었습니다.");
                toast.Show();
            }
            catch (Exception ex)
            {
                Toast.MakeText(context, $"#원신 태스크 - 할 일 위젯 #\n동기화 중 오류가 발생하였습니다.\n- {ex.Message}", ToastLength.Long).Show();
            }
        }
        #endregion
    }
}