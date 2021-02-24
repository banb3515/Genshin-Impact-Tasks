using Genshin_Impact_Tasks.Models;
using Genshin_Impact_Tasks.Popups;

using Rg.Plugins.Popup.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Genshin_Impact_Tasks.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TaskTabView : ContentView
    {
        string CurrentTaskList { get; set; } = "Daily"; // 현재 할 일 목록

        List<TaskModel> DailyTasks { get; set; } // 일일 반복 할 일
        List<TaskModel> WeeklyTasks { get; set; } // 주간 반복 할 일
        List<TaskModel> OneTimeTasks { get; set; } // 일회성 할 일
        List<TaskModel> CompleteOneTimeTasks { get; set; } // 완료된 일회성 할 일

        public TaskTabView()
        {
            try
            {
                InitializeComponent();

                Init();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }

        #region 초기화
        private void Init()
        {
            try
            {
                // 변수 초기화
                DailyTasks = new List<TaskModel>();
                WeeklyTasks = new List<TaskModel>();
                OneTimeTasks = new List<TaskModel>();
                CompleteOneTimeTasks = new List<TaskModel>();

                #region 데이터베이스 값 가져오기
                // 일일 반복
                App.Database.Table<DailyTaskTable>().ToList().ForEach(task => { DailyTasks.Add(new TaskModel { Content = task.Content, IconPath = task.IconPath, Status = task.Status }); });

                // 주간 반복
                App.Database.Table<WeeklyTaskTable>().ToList().ForEach(task => { WeeklyTasks.Add(new TaskModel { Content = task.Content, IconPath = task.IconPath, Status = task.Status }); });

                // 일회성
                App.Database.Table<OneTimeTaskTable>().ToList().ForEach(task => 
                { 
                    if (!task.Status)
                        OneTimeTasks.Add(new TaskModel { Content = task.Content, IconPath = task.IconPath, Status = task.Status }); 
                    else
                        CompleteOneTimeTasks.Add(new TaskModel { Content = task.Content, IconPath = task.IconPath, Status = task.Status });
                });
                #endregion

                if (DailyTasks.Count > 0) TaskEmptyText.IsVisible = false;
                TasksView.ItemsSource = DailyTasks;

                // 날짜 변경 감지
                Device.StartTimer(TimeSpan.FromSeconds(1), OnDetectDateChange);

                if (App.UseDarkMode)
                    Divider.Color = Color.White;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 날짜 변경 감지
        private bool OnDetectDateChange()
        {
            try
            {
                var date = Convert.ToDateTime(App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "Date").FirstOrDefault().Value);

                DateTime current;
                if (DateTime.Now.Hour >= 5) current = DateTime.Now;
                else current = DateTime.Now.AddDays(-1);

                #region 일일 반복 초기화
                if (current.ToString("yyyyMMdd") != date.ToString("yyyyMMdd"))
                {
                    App.Database.Table<DailyTaskTable>().ToList().ForEach(t =>
                    {
                        t.Status = false;
                        App.Database.Update(t);
                    });

                    for (int i = 0; i < DailyTasks.Count; i++)
                        DailyTasks[i].Status = false;

                    #region 주간 반복 초기화
                    if (current.DayOfWeek == DayOfWeek.Monday)
                    {
                        App.Database.Table<WeeklyTaskTable>().ToList().ForEach(t =>
                        {
                            t.Status = false;
                            App.Database.Update(t);
                        });

                        for (int i = 0; i < WeeklyTasks.Count; i++)
                            WeeklyTasks[i].Status = false;
                    }
                    #endregion

                    var dateDb = App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "Date").FirstOrDefault();
                    dateDb.Value = current.ToString();
                    App.Database.Update(dateDb);

                    TaskListRefresh();
                }
                #endregion
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }

            return true;
        }
        #endregion

        #region 할 일 목록 새로고침
        private void TaskListRefresh()
        {
            try
            {
                TaskEmptyText.IsVisible = true;
                TasksView.ItemsSource = null;

                switch (CurrentTaskList)
                {
                    case "Daily":
                        if (DailyTasks.Count > 0) TaskEmptyText.IsVisible = false;
                        TasksView.ItemsSource = DailyTasks;
                        break;
                    case "Weekly":
                        if (WeeklyTasks.Count > 0) TaskEmptyText.IsVisible = false;
                        TasksView.ItemsSource = WeeklyTasks;
                        break;
                    case "OneTime":
                        switch (OTTCompleteListButton.Text)
                        {
                            case "완료 보기":
                                Debug.WriteLine(OneTimeTasks.Count);
                                if (OneTimeTasks.Count > 0) TaskEmptyText.IsVisible = false;
                                TasksView.ItemsSource = OneTimeTasks;
                                break;
                            case "미완료 보기":
                                Debug.WriteLine(CompleteOneTimeTasks.Count);
                                if (CompleteOneTimeTasks.Count > 0) TaskEmptyText.IsVisible = false;
                                TasksView.ItemsSource = CompleteOneTimeTasks;
                                break;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 할 일 아이템 탭 시
        private void TasksView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                var item = e.Item as TaskModel;
                var idx = e.ItemIndex;

                switch (CurrentTaskList)
                {
                    case "Daily":
                        var dailyDb = App.Database.Table<DailyTaskTable>().ToList().Where(t => t.Content == DailyTasks[idx].Content).FirstOrDefault();
                        dailyDb.Status = !dailyDb.Status;
                        App.Database.Update(dailyDb);

                        DailyTasks[idx].Status = !DailyTasks[idx].Status;
                        break;
                    case "Weekly":
                        var weeklyDb = App.Database.Table<WeeklyTaskTable>().ToList().Where(t => t.Content == WeeklyTasks[idx].Content).FirstOrDefault();
                        weeklyDb.Status = !weeklyDb.Status;
                        App.Database.Update(weeklyDb);

                        WeeklyTasks[idx].Status = !WeeklyTasks[idx].Status;
                        break;
                    case "OneTime":
                        switch (OTTCompleteListButton.Text)
                        {
                            case "완료 보기":
                                var oneTimeDb = App.Database.Table<OneTimeTaskTable>().ToList().Where(t => t.Content == OneTimeTasks[idx].Content).FirstOrDefault();
                                oneTimeDb.Status = !oneTimeDb.Status;
                                oneTimeDb.CompleteTime = DateTime.Now.ToString();
                                App.Database.Update(oneTimeDb);

                                var oneTimeTmp = OneTimeTasks[idx];
                                oneTimeTmp.Status = !oneTimeTmp.Status;
                                CompleteOneTimeTasks.Add(oneTimeTmp);
                                OneTimeTasks.RemoveAt(idx);
                                break;
                            case "미완료 보기":
                                var cOneTimeDb = App.Database.Table<OneTimeTaskTable>().ToList().Where(t => t.Content == CompleteOneTimeTasks[idx].Content).FirstOrDefault();
                                cOneTimeDb.Status = !cOneTimeDb.Status;
                                cOneTimeDb.CompleteTime = "";
                                App.Database.Update(cOneTimeDb);

                                var cOneTimeTmp = CompleteOneTimeTasks[idx];
                                cOneTimeTmp.Status = !cOneTimeTmp.Status;
                                OneTimeTasks.Add(cOneTimeTmp);
                                CompleteOneTimeTasks.RemoveAt(idx);
                                break;
                        }
                        break;
                }

                TaskListRefresh();

                if (Device.RuntimePlatform == Device.UWP)
                    TasksView.ScrollTo(item, ScrollToPosition.Center, false);

                // 안드로이드, 진동 0.05초 동안 울림
                if (App.UseVibration && Device.RuntimePlatform == Device.Android)
                    Vibration.Vibrate(TimeSpan.FromMilliseconds(50));
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
                var answer = await App.Current.MainPage.DisplayActionSheet("더 보기", "취소", null, "아이콘 변경", "내용 편집", "삭제");

                var content = (sender as ImageButton).CommandParameter.ToString();

                int idx = -1;

                switch (CurrentTaskList)
                {
                    case "Daily":
                        idx = DailyTasks.FindIndex(t => t.Content == content);
                        break;
                    case "Weekly":
                        idx = WeeklyTasks.FindIndex(t => t.Content == content);
                        break;
                    case "OneTime":
                        switch (OTTCompleteListButton.Text)
                        {
                            case "완료 보기":
                                idx = OneTimeTasks.FindIndex(t => t.Content == content);
                                break;
                            case "미완료 보기":
                                idx = CompleteOneTimeTasks.FindIndex(t => t.Content == content);
                                break;
                        }
                        break;
                }

                switch (answer)
                {
                    #region 아이콘 변경
                    case "아이콘 변경":
                        var popup = new SelectIconPopup();

                        popup.OnClosed += (s, arg) =>
                        {
                            if (arg.IconPath != null)
                            {
                                switch (CurrentTaskList)
                                {
                                    case "Daily":
                                        var dailyDb = App.Database.Table<DailyTaskTable>().ToList().Where(t => t.Content == DailyTasks[idx].Content).FirstOrDefault();
                                        dailyDb.IconPath = arg.IconPath;
                                        App.Database.Update(dailyDb);

                                        DailyTasks[idx].IconPath = arg.IconPath;
                                        break;
                                    case "Weekly":
                                        var weeklyDb = App.Database.Table<WeeklyTaskTable>().ToList().Where(t => t.Content == WeeklyTasks[idx].Content).FirstOrDefault();
                                        weeklyDb.IconPath = arg.IconPath;
                                        App.Database.Update(weeklyDb);

                                        WeeklyTasks[idx].IconPath = arg.IconPath;
                                        break;
                                    case "OneTime":
                                        switch (OTTCompleteListButton.Text)
                                        {
                                            case "완료 보기":
                                                var oneTimeDb = App.Database.Table<OneTimeTaskTable>().ToList().Where(t => t.Content == OneTimeTasks[idx].Content).FirstOrDefault();
                                                oneTimeDb.IconPath = arg.IconPath;
                                                App.Database.Update(oneTimeDb);

                                                OneTimeTasks[idx].IconPath = arg.IconPath;
                                                break;
                                            case "미완료 보기":
                                                var cOneTimeDb = App.Database.Table<OneTimeTaskTable>().ToList().Where(t => t.Content == CompleteOneTimeTasks[idx].Content).FirstOrDefault();
                                                cOneTimeDb.IconPath = arg.IconPath;
                                                App.Database.Update(cOneTimeDb);

                                                CompleteOneTimeTasks[idx].IconPath = arg.IconPath;
                                                break;
                                        }
                                        break;
                                }

                                TaskListRefresh();
                            }
                        };

                        await PopupNavigation.Instance.PushAsync(popup);
                        break;
                    #endregion

                    #region 내용 편집
                    case "내용 편집":
                        string newContent = "";

                        while (true)
                        {
                            newContent = "";
                            newContent = await App.Current.MainPage.DisplayPromptAsync("내용 편집", "변경할 내용을 입력해주세요.", accept: "확인", cancel: "취소", placeholder: content);

                            if (string.IsNullOrWhiteSpace(newContent)) return;

                            newContent = newContent.Trim();

                            bool flag = false;

                            // 중복 확인, 일회성 할 일은 내용 중복을 허용함.
                            switch (CurrentTaskList)
                            {
                                case "Daily":
                                    if (App.Database.Table<DailyTaskTable>().ToList().Where(t => t.Content == newContent).Count() > 0)
                                        flag = true;
                                    break;
                                case "Weekly":
                                    if (App.Database.Table<WeeklyTaskTable>().ToList().Where(t => t.Content == newContent).Count() > 0)
                                        flag = true;
                                    break;
                            }

                            if (!flag) break;
                            else
                            {
                                if (content == newContent) return;

                                await App.Current.MainPage.DisplayAlert("내용 편집", $"[{newContent}] (은)는 이미 존재합니다.", "확인");
                            }
                        }

                        switch (CurrentTaskList)
                        {
                            case "Daily":
                                var dailyDb = App.Database.Table<DailyTaskTable>().ToList().Where(t => t.Content == DailyTasks[idx].Content).FirstOrDefault();
                                dailyDb.Content = newContent;
                                App.Database.Update(dailyDb);

                                DailyTasks[idx].Content = newContent;
                                break;
                            case "Weekly":
                                var weeklyDb = App.Database.Table<WeeklyTaskTable>().ToList().Where(t => t.Content == WeeklyTasks[idx].Content).FirstOrDefault();
                                weeklyDb.Content = newContent;
                                App.Database.Update(weeklyDb);

                                WeeklyTasks[idx].Content = newContent;
                                break;
                            case "OneTime":
                                switch (OTTCompleteListButton.Text)
                                {
                                    case "완료 보기":
                                        var oneTimeDb = App.Database.Table<OneTimeTaskTable>().ToList().Where(t => t.Content == OneTimeTasks[idx].Content).FirstOrDefault();
                                        oneTimeDb.Content = newContent;
                                        App.Database.Update(oneTimeDb);

                                        OneTimeTasks[idx].Content = newContent;
                                        break;
                                    case "미완료 보기":
                                        var cOneTimeDb = App.Database.Table<OneTimeTaskTable>().ToList().Where(t => t.Content == CompleteOneTimeTasks[idx].Content).FirstOrDefault();
                                        cOneTimeDb.Content = newContent;
                                        App.Database.Update(cOneTimeDb);

                                        CompleteOneTimeTasks[idx].Content = newContent;
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

                        switch (CurrentTaskList)
                        {
                            case "Daily":
                                var dailyDb = App.Database.Table<DailyTaskTable>().ToList().Where(t => t.Content == DailyTasks[idx].Content).FirstOrDefault();
                                App.Database.Delete(dailyDb);

                                DailyTasks.RemoveAt(idx);
                                break;
                            case "Weekly":
                                var weeklyDb = App.Database.Table<WeeklyTaskTable>().ToList().Where(t => t.Content == WeeklyTasks[idx].Content).FirstOrDefault();
                                App.Database.Delete(weeklyDb);

                                WeeklyTasks.RemoveAt(idx);
                                break;
                            case "OneTime":
                                switch (OTTCompleteListButton.Text)
                                {
                                    case "완료 보기":
                                        var oneTimeDb = App.Database.Table<OneTimeTaskTable>().ToList().Where(t => t.Content == OneTimeTasks[idx].Content).FirstOrDefault();
                                        App.Database.Delete(oneTimeDb);

                                        OneTimeTasks.RemoveAt(idx);
                                        break;
                                    case "미완료 보기":
                                        var cOneTimeDb = App.Database.Table<OneTimeTaskTable>().ToList().Where(t => t.Content == CompleteOneTimeTasks[idx].Content).FirstOrDefault();
                                        App.Database.Delete(cOneTimeDb);

                                        CompleteOneTimeTasks.RemoveAt(idx);
                                        break;
                                }
                                break;
                        }
                        break;
                    #endregion

                    default:
                        return;
                }

                TaskListRefresh();
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
                var popup = new AddTaskPopup(CurrentTaskList);

                popup.OnClosed += (s, arg) =>
                {
                    if (arg.Task != null)
                    {
                        switch (CurrentTaskList)
                        {
                            case "Daily":
                                var daily = new DailyTaskTable { Content = arg.Task.Content, IconPath = arg.Task.IconPath, Status = false, CreateTime = DateTime.Now.ToString() };
                                App.Database.Insert(daily);

                                DailyTasks.Add(arg.Task);
                                break;
                            case "Weekly":
                                var weekly = new WeeklyTaskTable { Content = arg.Task.Content, IconPath = arg.Task.IconPath, Status = false, CreateTime = DateTime.Now.ToString() };
                                App.Database.Insert(weekly);

                                WeeklyTasks.Add(arg.Task);
                                break;
                            case "OneTime":
                                var oneTime = new OneTimeTaskTable { Content = arg.Task.Content, IconPath = arg.Task.IconPath, Status = false, CreateTime = DateTime.Now.ToString(), CompleteTime = "" };
                                App.Database.Insert(oneTime);

                                OneTimeTasks.Add(arg.Task);

                                if (OTTCompleteListButton.Text == "미완료 보기") OTTCompleteListButton.Text = "완료 보기";
                                break;
                        }

                        TaskListRefresh();
                    }
                };

                await PopupNavigation.Instance.PushAsync(popup);
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
    }
}