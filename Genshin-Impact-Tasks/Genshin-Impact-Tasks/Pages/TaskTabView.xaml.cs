using Genshin_Impact_Tasks.Models;
using Genshin_Impact_Tasks.Popups;

using Rg.Plugins.Popup.Services;

using SQLite;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

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

                // 기기의 테마가 다크 모드일 경우
                if(App.Current.RequestedTheme == OSAppTheme.Dark)
                {
                    Title.TextColor = Color.White;
                    Line.Color = Color.White;
                }

                #region 데이터베이스 값 가져오기
                var db = new SQLiteConnection(App.DbFilePath, App.OpenFlags);

                // 일일 반복
                foreach (var data in db.Table<DailyTaskTable>())
                    DailyTasks.Add(new TaskModel { Content = data.Content, IconPath = data.IconPath, Status = data.Status });

                // 주간 반복
                foreach (var data in db.Table<WeeklyTaskTable>())
                    WeeklyTasks.Add(new TaskModel { Content = data.Content, IconPath = data.IconPath, Status = data.Status });

                // 일회성
                foreach (var data in db.Table<OneTimeTaskTable>())
                    OneTimeTasks.Add(new TaskModel { Content = data.Content, IconPath = data.IconPath, Status = data.Status });

                db.Close();
                #endregion

                TasksView.ItemsSource = DailyTasks;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 할 일 목록 새로고침
        private void TaskListRefresh()
        {
            try
            {
                TasksView.ItemsSource = null;

                switch (CurrentTaskList)
                {
                    case "Daily":
                        TasksView.ItemsSource = DailyTasks;
                        break;
                    case "Weekly":
                        TasksView.ItemsSource = WeeklyTasks;
                        break;
                    case "OneTime":
                        TasksView.ItemsSource = OneTimeTasks;
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
                        DailyTasks[idx].Status = !DailyTasks[idx].Status;
                        break;
                    case "Weekly":
                        WeeklyTasks[idx].Status = !WeeklyTasks[idx].Status;
                        break;
                    case "OneTime":
                        OneTimeTasks[idx].Status = !OneTimeTasks[idx].Status;
                        break;
                }

                TaskListRefresh();

                if(Device.RuntimePlatform == Device.UWP)
                    TasksView.ScrollTo(item, ScrollToPosition.Center, false);

                // 안드로이드, 진동 0.05초 동안 울림
                if (Device.RuntimePlatform == Device.Android)
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

                EnableTaskButton(false);

                _ = Title.FadeTo(0, 500, Easing.SpringOut);

                await TasksView.FadeTo(0, 300, Easing.SpringOut);
                TasksView.IsVisible = false;

                switch (task)
                {
                    case "Daily":
                        Title.Text = "일일 반복";
                        _ = Title.FadeTo(1, 150, Easing.CubicIn);
                        break;
                    case "Weekly":
                        Title.Text = "주간 반복";
                        _ = Title.FadeTo(1, 150, Easing.CubicIn);
                        break;
                    case "OneTime":
                        Title.Text = "할 일";
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
                        idx = OneTimeTasks.FindIndex(t => t.Content == content);
                        break;
                }

                switch (answer)
                {
                    case "아이콘 변경":
                        var popup = new SelectIconPopup();

                        popup.OnClosed += (s, arg) =>
                        {
                            if (arg.IconPath != null)
                            {
                                switch (CurrentTaskList)
                                {
                                    case "Daily":
                                        DailyTasks[idx].IconPath = arg.IconPath;
                                        break;
                                    case "Weekly":
                                        WeeklyTasks[idx].IconPath = arg.IconPath;
                                        break;
                                    case "OneTime":
                                        OneTimeTasks[idx].IconPath = arg.IconPath;
                                        break;
                                }

                                TaskListRefresh();
                            }
                        };

                        await PopupNavigation.Instance.PushAsync(popup);
                        break;
                    case "내용 편집":
                        string newContent = "";

                        while (true)
                        {
                            newContent = "";
                            newContent = await App.Current.MainPage.DisplayPromptAsync("내용 편집", "변경할 내용을 입력해주세요.", accept: "확인", cancel: "취소", placeholder: content);

                            if (string.IsNullOrWhiteSpace(newContent)) return;

                            newContent = newContent.Trim();

                            bool flag = false;

                            switch (CurrentTaskList)
                            {
                                case "Daily":
                                    if (DailyTasks.Where(t => t.Content == newContent).ToList().Count > 0)
                                        flag = true;
                                    break;
                                case "Weekly":
                                    if (WeeklyTasks.Where(t => t.Content == newContent).ToList().Count > 0)
                                        flag = true;
                                    break;
                                case "OneTime":
                                    if (OneTimeTasks.Where(t => t.Content == newContent).ToList().Count > 0)
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
                                DailyTasks[idx].Content = newContent;
                                break;
                            case "Weekly":
                                WeeklyTasks[idx].Content = newContent;
                                break;
                            case "OneTime":
                                OneTimeTasks[idx].Content = newContent;
                                break;
                        }
                        break;
                    case "삭제":
                        var remove = await App.Current.MainPage.DisplayAlert("삭제", $"[{content}] (을)를 삭제하시겠습니까?\n삭제된 항목은 복구할 수 없습니다.", "삭제", "취소");

                        if (!remove) return;

                        switch (CurrentTaskList)
                        {
                            case "Daily":
                                DailyTasks.RemoveAt(idx);
                                break;
                            case "Weekly":
                                WeeklyTasks.RemoveAt(idx);
                                break;
                            case "OneTime":
                                OneTimeTasks.RemoveAt(idx);
                                break;
                        }
                        break;
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

        #region 아이템 추가 버튼 클릭 시
        private async void AddTaskButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                AddTaskPopup popup = null;

                switch (CurrentTaskList)
                {
                    case "Daily":
                        popup = new AddTaskPopup(DailyTasks);
                        break;
                    case "Weekly":
                        popup = new AddTaskPopup(WeeklyTasks);
                        break;
                    case "OneTime":
                        popup = new AddTaskPopup(OneTimeTasks);
                        break;
                }

                popup.OnClosed += (s, arg) =>
                {
                    if (arg.Task != null)
                    {
                        switch (CurrentTaskList)
                        {
                            case "Daily":
                                DailyTasks.Add(arg.Task);
                                break;
                            case "Weekly":
                                WeeklyTasks.Add(arg.Task);
                                break;
                            case "OneTime":
                                OneTimeTasks.Add(arg.Task);
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
    }
}