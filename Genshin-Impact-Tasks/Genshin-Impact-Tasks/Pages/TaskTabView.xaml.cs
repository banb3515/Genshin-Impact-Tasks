using Genshin_Impact_Tasks.Models;
using Genshin_Impact_Tasks.Popups;

using Firebase.Database.Query;

using Rg.Plugins.Popup.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Genshin_Impact_Tasks.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TaskTabView : ContentView
    {
        public static TaskTabView Instance { get; set; }

        public static bool TimerRepeat { get; set; } = true;

        public static bool Synchronizing { get; set; } = false; // 동기화 진행 중

        private static bool Working { get; set; } = false; // 작업 중

        private string CurrentTaskList { get; set; } = "Daily"; // 현재 할 일 목록

        private List<TaskModel> Items { get; set; }

        public TaskTabView()
        {
            try
            {
                InitializeComponent();

                if (Device.RuntimePlatform == Device.Android)
                    RefreshTasksViewButton.IsVisible = false;

                Init();

                Instance = this;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }

        #region 초기화
        private async void Init()
        {
            try
            {
                TaskListRefresh();

                await AutoSync();
                await DateChange();
                await TaskAutoDelete();

                // 날짜 변경 감지
                Device.StartTimer(TimeSpan.FromSeconds(2), OnDetectDateChange);

                // 완료된 할 일 자동 삭제
                Device.StartTimer(TimeSpan.FromSeconds(5), OnTaskAutoDelete);

                // 데이터 변경 자동 감지
                if (Device.RuntimePlatform == Device.Android)
                    Device.StartTimer(TimeSpan.FromSeconds(1), OnDetectDataChange);

                // 자동 동기화
                if (App.AutoSync)
                    Device.StartTimer(TimeSpan.FromSeconds(5), OnAutoSync);

                if (App.UseDarkMode)
                    Divider.Color = Color.White;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 날짜 변경 타이머
        private bool OnDetectDateChange()
        {
            try
            {
                if (!TimerRepeat) return true;

                _ = DateChange();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }

            return true;
        }
        #endregion

        #region 완료된 할 일 자동 삭제 타이머
        private bool OnTaskAutoDelete()
        {
            try
            {
                if (!TimerRepeat) return true;

                _ = TaskAutoDelete();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }

            return true;
        }
        #endregion

        #region 데이터 변경 감지 타이머
        private bool OnDetectDataChange()
        {
            try
            {
                if (!TimerRepeat) return true;

                _ = DetectDataChange();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }

            return true;
        }
        #endregion

        #region 자동 동기화 타이머
        public bool OnAutoSync()
        {
            try
            {
                if (!TimerRepeat || Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    if (TasksRefreshView.IsRefreshing)
                        TasksRefreshView.IsRefreshing = false;

                    if (!RefreshTasksViewButton.IsEnabled)
                        RefreshTasksViewButton.IsEnabled = true;
                    return true;
                }

                _ = AutoSync();

                if (TasksRefreshView.IsRefreshing)
                    TasksRefreshView.IsRefreshing = false;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }

            return true;
        }
        #endregion

        #region 날짜 변경
        public async Task DateChange()
        {
            try
            {
                if (App.AutoSync && Connectivity.NetworkAccess != NetworkAccess.Internet) return;

                var date = Convert.ToDateTime(App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "Date").FirstOrDefault().Value);

                DateTime current;
                if (DateTime.Now.Hour >= 5) current = DateTime.Now;
                else current = DateTime.Now.AddDays(-1);

                #region 일일 반복 초기화
                if (current.ToString("yyyyMMdd") != date.ToString("yyyyMMdd"))
                {
                    if (App.AutoSync)
                    {
                        var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).Where(item => item.Object.Key == "Date").FirstOrDefault();
                        update.Object.Value = current.ToString();
                        await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("Setting").Child(update.Key).PutAsync(update.Object);
                        App.UpdateServerTasksSyncDate();
                    }

                    var dateDb = App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "Date").FirstOrDefault();
                    dateDb.Value = current.ToString();
                    App.Database.Update(dateDb);

                    var loading = new LoadingPopup("일일 반복 할 일을 초기화하는 중입니다.");
                    await PopupNavigation.Instance.PushAsync(loading);

                    foreach (var t in App.Database.Table<DailyTaskTable>().ToList())
                    {
                        if (App.AutoSync)
                        {
                            var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("DailyTask").OnceAsync<DailyTaskTable>()).Where(item => item.Object.No == t.No).FirstOrDefault();
                            update.Object.Status = false;
                            await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("DailyTask").Child(update.Key).PutAsync(update.Object);
                        }

                        t.Status = false;
                        App.Database.Update(t);
                    }

                    await PopupNavigation.Instance.RemovePageAsync(loading);

                    #region 주간 반복 초기화
                    if (current.DayOfWeek == DayOfWeek.Monday)
                    {
                        loading = new LoadingPopup("주간 반복 할 일을 초기화하는 중입니다.");
                        await PopupNavigation.Instance.PushAsync(loading);

                        foreach (var t in App.Database.Table<WeeklyTaskTable>().ToList())
                        {
                            if (App.AutoSync)
                            {
                                var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("WeeklyTask").OnceAsync<WeeklyTaskTable>()).Where(item => item.Object.No == t.No).FirstOrDefault();
                                update.Object.Status = false;
                                await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("WeeklyTask").Child(update.Key).PutAsync(update.Object);
                            }

                            t.Status = false;
                            App.Database.Update(t);
                        }

                        await PopupNavigation.Instance.RemovePageAsync(loading);
                    }
                    #endregion                   

                    TaskListRefresh();
                }
                #endregion
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 완료된 할 일 자동 삭제
        public async Task TaskAutoDelete()
        {
            try
            {
                var tadDay = Convert.ToInt32(App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "TaskAutoDelete").FirstOrDefault().Value);

                if (App.AutoSync && Connectivity.NetworkAccess != NetworkAccess.Internet) return;

                if (tadDay > 0)
                {
                    var currentTime = DateTime.Now;

                    bool delete = false;
                    var loading = new LoadingPopup($"완료된지 {tadDay}일이 지난 할 일을 삭제 중입니다.");

                    foreach (var task in App.Database.Table<OneTimeTaskTable>().ToList())
                    {
                        if (task.Status)
                        {
                            var completeTime = Convert.ToDateTime(task.CompleteTime);
                            completeTime = completeTime.AddDays(tadDay);

                            if (DateTime.Compare(completeTime, currentTime) < 0)
                            {
                                if (!delete)
                                {
                                    delete = true;
                                    await PopupNavigation.Instance.PushAsync(loading);
                                }

                                if (App.AutoSync)
                                {
                                    var del = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("OneTimeTask").OnceAsync<OneTimeTaskTable>()).Where(item => item.Object.No == task.No).FirstOrDefault();
                                    await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("OneTimeTask").Child(del.Key).DeleteAsync();
                                    App.UpdateServerTasksSyncDate();
                                }

                                App.Database.Delete(task);
                            }
                        }
                    }

                    if (delete)
                    {
                        TaskListRefresh();
                        await PopupNavigation.Instance.RemovePageAsync(loading);
                    }
                }
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 데이터 변경 감지
        public async Task DetectDataChange()
        {
            try
            {
                var tmpList = new List<TaskModel>();

                switch (CurrentTaskList)
                {
                    case "Daily":
                        App.Database.Table<DailyTaskTable>().ToList().ForEach(task =>
                        { tmpList.Add(new TaskModel { No = task.No, Content = task.Content, IconPath = task.IconPath, Status = task.Status }); });
                        break;
                    case "Weekly":
                        App.Database.Table<WeeklyTaskTable>().ToList().ForEach(task =>
                        { tmpList.Add(new TaskModel { No = task.No, Content = task.Content, IconPath = task.IconPath, Status = task.Status }); });
                        break;
                    case "OneTime":
                        switch (OTTCompleteListButton.Text)
                        {
                            case "완료 보기":
                                App.Database.Table<OneTimeTaskTable>().ToList().ForEach(task =>
                                { if (!task.Status) tmpList.Add(new TaskModel { No = task.No, Content = task.Content, IconPath = task.IconPath, Status = task.Status }); });
                                break;
                            case "미완료 보기":
                                App.Database.Table<OneTimeTaskTable>().ToList().ForEach(task =>
                                { if (task.Status) tmpList.Add(new TaskModel { No = task.No, Content = task.Content, IconPath = task.IconPath, Status = task.Status }); });
                                break;
                        }
                        break;
                }

                if (!Items.SequenceEqual(tmpList, new TaskComparer()))
                {
                    var loading = new LoadingPopup("변경된 데이터를 적용 중입니다.");
                    await PopupNavigation.Instance.PushAsync(loading);

                    TaskListRefresh();

                    await PopupNavigation.Instance.RemovePageAsync(loading);
                }
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 자동 동기화
        public async Task AutoSync()
        {
            try
            {
                if (Synchronizing) return;

                if (!App.AutoSync || Connectivity.NetworkAccess != NetworkAccess.Internet) return;

                Synchronizing = true;
                
                if (!await CheckChangedServerData())
                {
                    Synchronizing = false;
                    return;
                }

                var loading = new LoadingPopup("동기화 중입니다.");
                await PopupNavigation.Instance.PushAsync(loading);

                (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).ToList().ForEach(item =>
                {
                    if (item.Object.Key != "Theme" && item.Object.Key != "Vibration" && item.Object.Key != "SyncMail" && item.Object.Key != "Version")
                        App.Database.Update(item.Object);
                });
                App.Database.DropTable<DailyTaskTable>();
                App.Database.DropTable<WeeklyTaskTable>();
                App.Database.DropTable<OneTimeTaskTable>();

                App.Database.CreateTable<DailyTaskTable>();
                App.Database.CreateTable<WeeklyTaskTable>();
                App.Database.CreateTable<OneTimeTaskTable>();

                (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("DailyTask").OnceAsync<DailyTaskTable>()).ToList().ForEach(item =>
                {
                    App.Database.Insert(item.Object);
                });

                (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("WeeklyTask").OnceAsync<WeeklyTaskTable>()).ToList().ForEach(item =>
                {
                    App.Database.Insert(item.Object);
                });
                (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("OneTimeTask").OnceAsync<OneTimeTaskTable>()).ToList().ForEach(item =>
                {
                    App.Database.Insert(item.Object);
                });

                App.UpdateLocalTasksSyncDate();

                TaskListRefresh();
                await PopupNavigation.Instance.RemovePageAsync(loading);

                Synchronizing = false;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 할 일 목록 새로고침
        public void TaskListRefresh()
        {
            try
            {
                Items = new List<TaskModel>();
                TaskEmptyText.IsVisible = true;
                TasksView.ItemsSource = null;

                switch (CurrentTaskList)
                {
                    case "Daily":
                        App.Database.Table<DailyTaskTable>().ToList().ForEach(task =>
                        { Items.Add(new TaskModel { No = task.No, Content = task.Content, IconPath = task.IconPath, Status = task.Status }); });
                        break;
                    case "Weekly":
                        App.Database.Table<WeeklyTaskTable>().ToList().ForEach(task =>
                        { Items.Add(new TaskModel { No = task.No, Content = task.Content, IconPath = task.IconPath, Status = task.Status }); });
                        break;
                    case "OneTime":
                        switch (OTTCompleteListButton.Text)
                        {
                            case "완료 보기":
                                App.Database.Table<OneTimeTaskTable>().ToList().ForEach(task =>
                                { if (!task.Status) Items.Add(new TaskModel { No = task.No, Content = task.Content, IconPath = task.IconPath, Status = task.Status }); });
                                break;
                            case "미완료 보기":
                                App.Database.Table<OneTimeTaskTable>().ToList().ForEach(task =>
                                { if (task.Status) Items.Add(new TaskModel { No = task.No, Content = task.Content, IconPath = task.IconPath, Status = task.Status }); });
                                break;
                        }
                        break;
                }

                TasksView.ItemsSource = Items;

                if (Items.Count > 0) TaskEmptyText.IsVisible = false;

                if (TasksRefreshView.IsRefreshing)
                    TasksRefreshView.IsRefreshing = false;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 할 일 아이템 탭 시
        private async void TasksView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                if (Working) return;

                Working = true;

                // 안드로이드, 진동 0.05초 동안 울림
                if (App.UseVibration && Device.RuntimePlatform == Device.Android)
                    Vibration.Vibrate(TimeSpan.FromMilliseconds(50));

                if (App.AutoSync && Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await App.Current.MainPage.DisplayAlert("할 일",
                        "인터넷에 연결되어 있지 않아 완료 상태를 변경할 수 없습니다.\n" +
                        "인터넷 상태를 확인하신 후 다시 시도해주시기 바랍니다.", "확인");
                    Working = false;
                    return;
                }

                if (App.AutoSync && await CheckChangedServerData())
                {
                    await App.Current.MainPage.DisplayAlert("할 일", 
                        "서버에 변경된 데이터가 있습니다.\n" +
                        "새로 고침 후 다시 시도해 주시기 바랍니다.", "확인");
                    Working = false;
                    return;
                }

                var loading = new LoadingPopup("");
                await PopupNavigation.Instance.PushAsync(loading);

                var idx = e.ItemIndex;

                switch (CurrentTaskList)
                {
                    case "Daily":
                        if (App.AutoSync)
                        {
                            var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("DailyTask").OnceAsync<DailyTaskTable>()).Where(item => item.Object.No == Items[idx].No).FirstOrDefault();
                            update.Object.Status = !update.Object.Status;
                            await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("DailyTask").Child(update.Key).PutAsync(update.Object);
                            App.UpdateServerTasksSyncDate();
                        }

                        var dailyDb = App.Database.Table<DailyTaskTable>().ToList().Where(t => t.No == Items[idx].No).FirstOrDefault();
                        dailyDb.Status = !dailyDb.Status;
                        App.Database.Update(dailyDb);
                        break;
                    case "Weekly":
                        if (App.AutoSync)
                        {
                            var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("WeeklyTask").OnceAsync<WeeklyTaskTable>()).Where(item => item.Object.No == Items[idx].No).FirstOrDefault();
                            update.Object.Status = !update.Object.Status;
                            await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("WeeklyTask").Child(update.Key).PutAsync(update.Object);
                            App.UpdateServerTasksSyncDate();
                        }

                        var weeklyDb = App.Database.Table<WeeklyTaskTable>().ToList().Where(t => t.No == Items[idx].No).FirstOrDefault();
                        weeklyDb.Status = !weeklyDb.Status;
                        App.Database.Update(weeklyDb);
                        break;
                    case "OneTime":
                        var now = DateTime.Now.ToString();

                        switch (OTTCompleteListButton.Text)
                        {
                            case "완료 보기":
                                if (App.AutoSync)
                                {
                                    var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("OneTimeTask").OnceAsync<OneTimeTaskTable>()).Where(item => item.Object.No == Items[idx].No).FirstOrDefault();
                                    update.Object.Status = !update.Object.Status;
                                    update.Object.CompleteTime = now;
                                    await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("OneTimeTask").Child(update.Key).PutAsync(update.Object);
                                    App.UpdateServerTasksSyncDate();
                                }

                                var oneTimeDb = App.Database.Table<OneTimeTaskTable>().ToList().Where(t => t.No == Items[idx].No).FirstOrDefault();
                                oneTimeDb.Status = !oneTimeDb.Status;
                                oneTimeDb.CompleteTime = now;
                                App.Database.Update(oneTimeDb);
                                break;
                            case "미완료 보기":
                                if (App.AutoSync)
                                {
                                    var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("OneTimeTask").OnceAsync<OneTimeTaskTable>()).Where(item => item.Object.No == Items[idx].No).FirstOrDefault();
                                    update.Object.Status = !update.Object.Status;
                                    update.Object.CompleteTime = "";
                                    await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("OneTimeTask").Child(update.Key).PutAsync(update.Object);
                                    App.UpdateServerTasksSyncDate();
                                }

                                var cOneTimeDb = App.Database.Table<OneTimeTaskTable>().ToList().Where(t => t.No == Items[idx].No).FirstOrDefault();
                                cOneTimeDb.Status = !cOneTimeDb.Status;
                                cOneTimeDb.CompleteTime = "";
                                App.Database.Update(cOneTimeDb);
                                break;
                        }
                        break;
                }

                TaskListRefresh();
                await PopupNavigation.Instance.RemovePageAsync(loading);

                Working = false;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 할 일 목록 이동
        private async void NavigateTaskList_Clicked(object sender, EventArgs e)
        {
            try
            {
                var task = (sender as Button).BindingContext as string;

                if (CurrentTaskList == task) return;

                CurrentTaskList = task;

                OTTCompleteListButton.IsEnabled = false;
                EnableTaskButton(false);

                _ = Title.FadeTo(0, 500, Easing.SpringOut);

                if (task != "OneTime" && OTTCompleteListButton.IsVisible)
                    _ = OTTCompleteListButton.FadeTo(0, 500, Easing.SpringOut);

                await TasksView.FadeTo(0, 300, Easing.SpringOut);
                TasksView.IsVisible = false;

                switch (task)
                {
                    case "Daily":
                        Title.Text = "일일 반복";
                        TaskEmptyText.Text = "할 일이 없습니다.";
                        _ = Title.FadeTo(1, 150, Easing.CubicIn);
                        OTTCompleteListButton.IsVisible = false;
                        break;
                    case "Weekly":
                        Title.Text = "주간 반복";
                        TaskEmptyText.Text = "할 일이 없습니다.";
                        _ = Title.FadeTo(1, 150, Easing.CubicIn);
                        OTTCompleteListButton.IsVisible = false;
                        break;
                    case "OneTime":
                        if (OTTCompleteListButton.Text == "완료 보기")
                        {
                            Title.Text = "할 일 [미완료]";
                            TaskEmptyText.Text = "할 일이 없습니다.";
                        }
                        else
                        {
                            Title.Text = "할 일 [완료]";
                            TaskEmptyText.Text = "완료된 할 일이 없습니다.";
                        }
                        OTTCompleteListButton.IsVisible = true;
                        OTTCompleteListButton.IsEnabled = true;
                        _ = OTTCompleteListButton.FadeTo(1, 160, Easing.CubicIn);
                        _ = Title.FadeTo(1, 150, Easing.CubicIn);
                        break;
                }

                TaskListRefresh();

                TasksView.IsVisible = true;
                await TasksView.FadeTo(1, 150, Easing.CubicIn);

                EnableTaskButton(true);
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 할 일 목록 이동 버튼 활성화
        // 할 일 목록 이동 버튼을 활성화하거나 비활성화합니다.
        private void EnableTaskButton(bool enable = true)
        {
            try
            {
                DailyTasksButton.IsEnabled = enable;
                WeeklyTasksButton.IsEnabled = enable;
                OneTimeTasksButton.IsEnabled = enable;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 더 보기 버튼 클릭 시
        private async void MoreButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (App.AutoSync && Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await App.Current.MainPage.DisplayAlert("할 일",
                        "인터넷에 연결되어 있지 않아 수정 또는 삭제를 할 수 없습니다.\n" +
                        "인터넷 상태를 확인하신 후 다시 시도해주시기 바랍니다.", "확인");
                    return;
                }

                if (App.AutoSync && await CheckChangedServerData())
                {
                    await App.Current.MainPage.DisplayAlert("할 일",
                        "서버에 변경된 데이터가 있습니다.\n" +
                        "새로 고침 후 다시 시도해 주시기 바랍니다.", "확인");
                    return;
                }

                var answer = await App.Current.MainPage.DisplayActionSheet("더 보기", "취소", null, "아이콘 변경", "내용 편집", "삭제");

                if (answer == "취소") return;

                var loading = new LoadingPopup("변경 중입니다.");
                var loadingFlag = false;

                var no = Convert.ToInt32((sender as ImageButton).CommandParameter.ToString());

                var idx = Items.FindIndex(t => t.No == no);
                var content = Items[idx].Content;

                switch (answer)
                {
                    #region 아이콘 변경
                    case "아이콘 변경":
                        var popup = new SelectIconPopup();

                        popup.OnClosed += async (s, arg) =>
                        {
                            if (arg.IconPath != null)
                            {
                                if (App.AutoSync && Connectivity.NetworkAccess != NetworkAccess.Internet)
                                {
                                    await App.Current.MainPage.DisplayAlert("할 일",
                                        "인터넷에 연결되어 있지 않아 아이콘을 변경할 수 없습니다.\n" +
                                        "인터넷 상태를 확인하신 후 다시 시도해주시기 바랍니다.", "확인");
                                    return;
                                }

                                loadingFlag = true;
                                await PopupNavigation.Instance.PushAsync(loading);

                                switch (CurrentTaskList)
                                {
                                    case "Daily":
                                        if (App.AutoSync)
                                        {
                                            var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("DailyTask").OnceAsync<DailyTaskTable>()).Where(item => item.Object.No == Items[idx].No).FirstOrDefault();
                                            update.Object.IconPath = arg.IconPath;
                                            await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("DailyTask").Child(update.Key).PutAsync(update.Object);
                                            App.UpdateServerTasksSyncDate();
                                        }

                                        var dailyDb = App.Database.Table<DailyTaskTable>().ToList().Where(t => t.No == Items[idx].No).FirstOrDefault();
                                        dailyDb.IconPath = arg.IconPath;
                                        App.Database.Update(dailyDb);
                                        break;
                                    case "Weekly":
                                        if (App.AutoSync)
                                        {
                                            var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("WeeklyTask").OnceAsync<WeeklyTaskTable>()).Where(item => item.Object.No == Items[idx].No).FirstOrDefault();
                                            update.Object.IconPath = arg.IconPath;
                                            await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("WeeklyTask").Child(update.Key).PutAsync(update.Object);
                                            App.UpdateServerTasksSyncDate();
                                        }

                                        var weeklyDb = App.Database.Table<WeeklyTaskTable>().ToList().Where(t => t.No == Items[idx].No).FirstOrDefault();
                                        weeklyDb.IconPath = arg.IconPath;
                                        App.Database.Update(weeklyDb);
                                        break;
                                    case "OneTime":
                                        switch (OTTCompleteListButton.Text)
                                        {
                                            case "완료 보기":
                                                if (App.AutoSync)
                                                {
                                                    var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("OneTimeTask").OnceAsync<OneTimeTaskTable>()).Where(item => item.Object.No == Items[idx].No).FirstOrDefault();
                                                    update.Object.IconPath = arg.IconPath;
                                                    await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("OneTimeTask").Child(update.Key).PutAsync(update.Object);
                                                    App.UpdateServerTasksSyncDate();
                                                }

                                                var oneTimeDb = App.Database.Table<OneTimeTaskTable>().ToList().Where(t => t.No == Items[idx].No).FirstOrDefault();
                                                oneTimeDb.IconPath = arg.IconPath;
                                                App.Database.Update(oneTimeDb);
                                                break;
                                            case "미완료 보기":
                                                if (App.AutoSync)
                                                {
                                                    var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("OneTimeTask").OnceAsync<OneTimeTaskTable>()).Where(item => item.Object.No == Items[idx].No).FirstOrDefault();
                                                    update.Object.IconPath = arg.IconPath;
                                                    await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("OneTimeTask").Child(update.Key).PutAsync(update.Object);
                                                    App.UpdateServerTasksSyncDate();
                                                }

                                                var cOneTimeDb = App.Database.Table<OneTimeTaskTable>().ToList().Where(t => t.No == Items[idx].No).FirstOrDefault();
                                                cOneTimeDb.IconPath = arg.IconPath;
                                                App.Database.Update(cOneTimeDb);
                                                break;
                                        }
                                        break;
                                }

                                TaskListRefresh();
                                await PopupNavigation.Instance.RemovePageAsync(loading);
                            }
                        };

                        await PopupNavigation.Instance.PushAsync(popup);
                        break;
                    #endregion

                    #region 내용 편집
                    case "내용 편집":
                        var newContent = await App.Current.MainPage.DisplayPromptAsync("내용 편집", "변경할 내용을 입력해주세요.", accept: "확인", cancel: "취소", placeholder: content);

                        if (string.IsNullOrWhiteSpace(newContent)) return;

                        if (App.AutoSync && Connectivity.NetworkAccess != NetworkAccess.Internet)
                        {
                            await App.Current.MainPage.DisplayAlert("할 일",
                                "인터넷에 연결되어 있지 않아 내용 편집을 할 수 없습니다.\n" +
                                "인터넷 상태를 확인하신 후 다시 시도해주시기 바랍니다.", "확인");
                            return;
                        }

                        loadingFlag = true;
                        await PopupNavigation.Instance.PushAsync(loading);

                        newContent = newContent.Trim();

                        switch (CurrentTaskList)
                        {
                            case "Daily":
                                if (App.AutoSync)
                                {
                                    var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("DailyTask").OnceAsync<DailyTaskTable>()).Where(item => item.Object.No == Items[idx].No).FirstOrDefault();
                                    update.Object.Content = newContent;
                                    await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("DailyTask").Child(update.Key).PutAsync(update.Object);
                                    App.UpdateServerTasksSyncDate();
                                }

                                var dailyDb = App.Database.Table<DailyTaskTable>().ToList().Where(t => t.No == Items[idx].No).FirstOrDefault();
                                dailyDb.Content = newContent;
                                App.Database.Update(dailyDb);
                                break;
                            case "Weekly":
                                if (App.AutoSync)
                                {
                                    var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("WeeklyTask").OnceAsync<WeeklyTaskTable>()).Where(item => item.Object.No == Items[idx].No).FirstOrDefault();
                                    update.Object.Content = newContent;
                                    await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("WeeklyTask").Child(update.Key).PutAsync(update.Object);
                                    App.UpdateServerTasksSyncDate();
                                }

                                var weeklyDb = App.Database.Table<WeeklyTaskTable>().ToList().Where(t => t.No == Items[idx].No).FirstOrDefault();
                                weeklyDb.Content = newContent;
                                App.Database.Update(weeklyDb);
                                break;
                            case "OneTime":
                                switch (OTTCompleteListButton.Text)
                                {
                                    case "완료 보기":
                                        if (App.AutoSync)
                                        {
                                            var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("OneTimeTask").OnceAsync<OneTimeTaskTable>()).Where(item => item.Object.No == Items[idx].No).FirstOrDefault();
                                            update.Object.Content = newContent;
                                            await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("OneTimeTask").Child(update.Key).PutAsync(update.Object);
                                            App.UpdateServerTasksSyncDate();
                                        }

                                        var oneTimeDb = App.Database.Table<OneTimeTaskTable>().ToList().Where(t => t.No == Items[idx].No).FirstOrDefault();
                                        oneTimeDb.Content = newContent;
                                        App.Database.Update(oneTimeDb);
                                        break;
                                    case "미완료 보기":
                                        if (App.AutoSync)
                                        {
                                            var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("OneTimeTask").OnceAsync<OneTimeTaskTable>()).Where(item => item.Object.No == Items[idx].No).FirstOrDefault();
                                            update.Object.Content = newContent;
                                            await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("OneTimeTask").Child(update.Key).PutAsync(update.Object);
                                            App.UpdateServerTasksSyncDate();
                                        }

                                        var cOneTimeDb = App.Database.Table<OneTimeTaskTable>().ToList().Where(t => t.No == Items[idx].No).FirstOrDefault();
                                        cOneTimeDb.Content = newContent;
                                        App.Database.Update(cOneTimeDb);
                                        break;
                                }
                                break;
                        }
                        break;
                    #endregion

                    #region 삭제
                    case "삭제":
                        var remove = await App.Current.MainPage.DisplayAlert("삭제", $"[{content}] (을)를 삭제합니다.\n삭제된 항목은 복구할 수 없습니다.", "삭제", "취소");

                        if (!remove) return;

                        if (App.AutoSync && Connectivity.NetworkAccess != NetworkAccess.Internet)
                        {
                            await App.Current.MainPage.DisplayAlert("할 일",
                                "인터넷에 연결되어 있지 않아 삭제할 수 없습니다.\n" +
                                "인터넷 상태를 확인하신 후 다시 시도해주시기 바랍니다.", "확인");
                            return;
                        }

                        loadingFlag = true;
                        await PopupNavigation.Instance.PushAsync(loading);

                        switch (CurrentTaskList)
                        {
                            case "Daily":
                                if (App.AutoSync)
                                {
                                    var del = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("DailyTask").OnceAsync<DailyTaskTable>()).Where(item => item.Object.No == Items[idx].No).FirstOrDefault();
                                    await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("DailyTask").Child(del.Key).DeleteAsync();
                                    App.UpdateServerTasksSyncDate();
                                }

                                var dailyDb = App.Database.Table<DailyTaskTable>().ToList().Where(t => t.No == Items[idx].No).FirstOrDefault();
                                App.Database.Delete(dailyDb);
                                break;
                            case "Weekly":
                                if (App.AutoSync)
                                {
                                    var del = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("WeeklyTask").OnceAsync<WeeklyTaskTable>()).Where(item => item.Object.No == Items[idx].No).FirstOrDefault();
                                    await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("WeeklyTask").Child(del.Key).DeleteAsync();
                                    App.UpdateServerTasksSyncDate();
                                }

                                var weeklyDb = App.Database.Table<WeeklyTaskTable>().ToList().Where(t => t.No == Items[idx].No).FirstOrDefault();
                                App.Database.Delete(weeklyDb);
                                break;
                            case "OneTime":
                                if (App.AutoSync)
                                {
                                    var del = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("OneTimeTask").OnceAsync<OneTimeTaskTable>()).Where(item => item.Object.No == Items[idx].No).FirstOrDefault();
                                    await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("OneTimeTask").Child(del.Key).DeleteAsync();
                                    App.UpdateServerTasksSyncDate();
                                }

                                var oneTimeDb = App.Database.Table<OneTimeTaskTable>().ToList().Where(t => t.No == Items[idx].No).FirstOrDefault();
                                App.Database.Delete(oneTimeDb);
                                break;
                        }
                        break;
                    #endregion

                    default:
                        return;
                }

                if (answer != "아이콘 변경")
                {
                    TaskListRefresh();
                    if (loadingFlag) await PopupNavigation.Instance.RemovePageAsync(loading);
                }
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 할 일 추가 버튼 클릭 시
        private async void AddTaskButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (App.AutoSync && Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await App.Current.MainPage.DisplayAlert("할 일",
                        "인터넷에 연결되어 있지 않아 할 일을 추가할 수 없습니다.\n" +
                        "인터넷 상태를 확인하신 후 다시 시도해주시기 바랍니다.", "확인");
                    return;
                }

                AddTaskButton.IsEnabled = false;

                var popup = new AddTaskPopup(CurrentTaskList);

                popup.OnClosed += async (s, arg) =>
                {
                    if (arg.Task != null)
                    {
                        var loading = new LoadingPopup("할 일을 추가 중입니다.");
                        await PopupNavigation.Instance.PushAsync(loading);

                        switch (CurrentTaskList)
                        {
                            case "Daily":
                                var dailyNos = App.Database.Table<DailyTaskTable>().ToList().Select(x => x.No);
                                var daily = new DailyTaskTable { No = dailyNos.Count() == 0 ? 1 : dailyNos.Max(t => t) + 1, Content = arg.Task.Content, IconPath = arg.Task.IconPath, Status = false, CreateTime = DateTime.Now.ToString() };

                                if (App.AutoSync)
                                {
                                    await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("DailyTask").PostAsync(daily);
                                    App.UpdateServerTasksSyncDate();
                                }

                                App.Database.Insert(daily);
                                break;
                            case "Weekly":
                                var weeklyNos = App.Database.Table<WeeklyTaskTable>().ToList().Select(x => x.No);
                                var weekly = new WeeklyTaskTable { No = weeklyNos.Count() == 0 ? 1 : weeklyNos.Max(t => t) + 1, Content = arg.Task.Content, IconPath = arg.Task.IconPath, Status = false, CreateTime = DateTime.Now.ToString() };

                                if (App.AutoSync)
                                {
                                    await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("WeeklyTask").PostAsync(weekly);
                                    App.UpdateServerTasksSyncDate();
                                }

                                App.Database.Insert(weekly);
                                break;
                            case "OneTime":
                                var oneTimeNos = App.Database.Table<OneTimeTaskTable>().ToList().Select(x => x.No);
                                var oneTime = new OneTimeTaskTable { No = oneTimeNos.Count() == 0 ? 1 : oneTimeNos.Max(t => t) + 1, Content = arg.Task.Content, IconPath = arg.Task.IconPath, Status = false, CreateTime = DateTime.Now.ToString(), CompleteTime = "" };

                                if (App.AutoSync)
                                {
                                    await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("OneTimeTask").PostAsync(oneTime);
                                    App.UpdateServerTasksSyncDate();
                                }

                                App.Database.Insert(oneTime);

                                if (OTTCompleteListButton.Text == "미완료 보기") OTTCompleteListButton.Text = "완료 보기";
                                break;
                        }

                        TaskListRefresh();
                        await PopupNavigation.Instance.RemovePageAsync(loading);
                    }
                };

                await PopupNavigation.Instance.PushAsync(popup);

                AddTaskButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 일회성 할 일 목록 완료/미완료 전환 버튼
        private void OTTCompleteListButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (OTTCompleteListButton.Text == "완료 보기")
                {
                    OTTCompleteListButton.Text = "미완료 보기";
                    Title.Text = "할 일 [완료]";
                    TaskEmptyText.Text = "완료된 할 일이 없습니다.";
                }
                else
                {
                    OTTCompleteListButton.Text = "완료 보기";
                    Title.Text = "할 일 [미완료]";
                    TaskEmptyText.Text = "할 일이 없습니다.";
                }
                TaskListRefresh();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 할 일 새로고침 시
        private void TasksView_Refreshing(object sender, EventArgs e)
        {
            try
            {
                if (App.AutoSync && Connectivity.NetworkAccess == NetworkAccess.Internet)
                    OnAutoSync();
                else if (!App.AutoSync)
                    TaskListRefresh();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 할 일 새로고침 버튼 클릭 시
        private void RefreshTasksViewButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                RefreshTasksViewButton.IsEnabled = false;

                if (App.AutoSync && Connectivity.NetworkAccess == NetworkAccess.Internet)
                    OnAutoSync();
                else if (!App.AutoSync)
                    TaskListRefresh();

                Device.StartTimer(TimeSpan.FromSeconds(1), () => { RefreshTasksViewButton.IsEnabled = true; return false; });
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 변경된 서버 데이터 확인
        private async Task<bool> CheckChangedServerData()
        {
            var localSyncDateStr = App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "TasksSyncDate").FirstOrDefault().Value;
            DateTime localSyncDate = DateTime.Now;

            bool pass = false;

            if (localSyncDateStr == "")
                pass = true;
            else
                localSyncDate = Convert.ToDateTime(localSyncDateStr);

            var serverSyncDate = Convert.ToDateTime((await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).Where(item => item.Object.Key == "TasksSyncDate").FirstOrDefault().Object.Value);

            return !(!pass && DateTime.Compare(localSyncDate, serverSyncDate) >= 0);
        }
        #endregion
    }
}