using Genshin_Impact_Tasks.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Genshin_Impact_Tasks.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TaskTabView : ContentView
    {
        string CurrentTaskList { get; set; } = "Daily"; // 현재 할 일 목록

        List<TaskModel> dailyTasks = new List<TaskModel>(); // 일일 반복 할 일
        List<TaskModel> weeklyTasks = new List<TaskModel>(); // 주간 반복 할 일
        List<TaskModel> oneTimeTasks = new List<TaskModel>(); // 일회성 할 일

        public TaskTabView()
        {
            try
            {
                InitializeComponent();

                for(int i = 1; i <= 5; i ++)
                {
                    dailyTasks.Add(new TaskModel
                    {
                        No = dailyTasks.Count,
                        ImagePath = "Resources/resin.png",
                        Content = $"일일 반복 {i}"
                    });
                }

                for (int i = 1; i <= 5; i++)
                {
                    weeklyTasks.Add(new TaskModel
                    {
                        No = weeklyTasks.Count,
                        ImagePath = "Resources/timer.png",
                        Content = $"주간 반복 {i}"
                    });
                }

                for (int i = 1; i <= 5; i++)
                {
                    oneTimeTasks.Add(new TaskModel
                    {
                        No = oneTimeTasks.Count,
                        ImagePath = "Resources/commission.png",
                        Content = $"할 일 {i}"
                    });
                }

                DailyTasksView.ItemsSource = dailyTasks;
                WeeklyTasksView.ItemsSource = weeklyTasks;
                OneTimeTasksView.ItemsSource = oneTimeTasks;

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
                // 기기의 테마가 다크 모드일 경우
                if(App.Current.RequestedTheme == OSAppTheme.Dark)
                {
                    Title.TextColor = Color.White;
                    Line.Color = Color.White;
                }
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 할 일 목록 새로고침
        private void TaskListRefresh(string type)
        {
            try
            {
                switch (type)
                {
                    case "Daily":
                        for (int i = 0; i < dailyTasks.Count; i++)
                            dailyTasks[i].No = i;
                        DailyTasksView.ItemsSource = null;
                        DailyTasksView.ItemsSource = dailyTasks;
                        break;
                    case "Weekly":
                        for (int i = 0; i < weeklyTasks.Count; i++)
                            weeklyTasks[i].No = i;
                        WeeklyTasksView.ItemsSource = null;
                        WeeklyTasksView.ItemsSource = weeklyTasks;
                        break;
                    case "OneTime":
                        for (int i = 0; i < oneTimeTasks.Count; i++)
                            oneTimeTasks[i].No = i;
                        OneTimeTasksView.ItemsSource = null;
                        OneTimeTasksView.ItemsSource = oneTimeTasks;
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
                var view = (sender as ListView).BindingContext as string;
                var idx = e.ItemIndex;

                switch (view)
                {
                    case "Daily":
                        dailyTasks[idx].Status = !dailyTasks[idx].Status;
                        break;
                    case "Weekly":
                        weeklyTasks[idx].Status = !weeklyTasks[idx].Status;
                        break;
                    case "OneTime":
                        oneTimeTasks[idx].Status = !oneTimeTasks[idx].Status;
                        break;
                }

                TaskListRefresh(view);

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

                EnableTaskButton(false);

                _ = Title.FadeTo(0, 500, Easing.SpringOut);

                switch (task)
                {
                    case "Daily":
                        switch (CurrentTaskList)
                        {
                            case "Weekly":
                                await WeeklyTasksView.FadeTo(0, 500, Easing.SpringOut);
                                WeeklyTasksView.IsVisible = false;
                                break;
                            case "OneTime":
                                await OneTimeTasksView.FadeTo(0, 500, Easing.SpringOut);
                                OneTimeTasksView.IsVisible = false;
                                break;
                        }

                        Title.Text = "일일 반복";
                        _ = Title.FadeTo(1, 250, Easing.CubicIn);

                        DailyTasksView.IsVisible = true;
                        await DailyTasksView.FadeTo(1, 250, Easing.CubicIn);
                        break;
                    case "Weekly":
                        switch (CurrentTaskList)
                        {
                            case "Daily":
                                await DailyTasksView.FadeTo(0, 500, Easing.SpringOut);
                                DailyTasksView.IsVisible = false;
                                break;
                            case "OneTime":
                                await OneTimeTasksView.FadeTo(0, 500, Easing.SpringOut);
                                OneTimeTasksView.IsVisible = false;
                                break;
                        }

                        Title.Text = "주간 반복";
                        _ = Title.FadeTo(1, 250, Easing.CubicIn);

                        WeeklyTasksView.IsVisible = true;
                        await WeeklyTasksView.FadeTo(1, 250, Easing.CubicIn);
                        break;
                    case "OneTime":
                        switch (CurrentTaskList)
                        {
                            case "Daily":
                                await DailyTasksView.FadeTo(0, 500, Easing.SpringOut);
                                DailyTasksView.IsVisible = false;
                                break;
                            case "Weekly":
                                await WeeklyTasksView.FadeTo(0, 500, Easing.SpringOut);
                                WeeklyTasksView.IsVisible = false;
                                break;
                        }

                        Title.Text = "할 일";
                        _ = Title.FadeTo(1, 250, Easing.CubicIn);

                        OneTimeTasksView.IsVisible = true;
                        await OneTimeTasksView.FadeTo(1, 250, Easing.CubicIn);
                        break;
                }

                CurrentTaskList = task;
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
                var answer = await App.Current.MainPage.DisplayActionSheet("더 보기", "취소", null, "편집", "삭제");

                var idx = Convert.ToInt32((sender as ImageButton).BindingContext);
                string content = "";

                switch (CurrentTaskList)
                {
                    case "Daily":
                        content = dailyTasks[idx].Content;
                        break;
                    case "Weekly":
                        content = weeklyTasks[idx].Content;
                        break;
                    case "OneTime":
                        content = oneTimeTasks[idx].Content;
                        break;
                }

                switch (answer)
                {
                    case "편집":
                        var newContent = await App.Current.MainPage.DisplayPromptAsync("편집", "변경할 내용을 입력해주세요.", accept: "확인", cancel: "취소", placeholder: content);

                        if (string.IsNullOrWhiteSpace(newContent)) return;

                        switch (CurrentTaskList)
                        {
                            case "Daily":
                                dailyTasks[idx].Content = newContent;
                                break;
                            case "Weekly":
                                weeklyTasks[idx].Content = newContent;
                                break;
                            case "OneTime":
                                oneTimeTasks[idx].Content = newContent;
                                break;
                        }
                        break;
                    case "삭제":
                        var remove = await App.Current.MainPage.DisplayAlert("삭제", $"[{content}] (을)를 삭제하시겠습니까?\n삭제된 항목은 복구할 수 없습니다.", "삭제", "취소");

                        if (!remove) return;

                        switch (CurrentTaskList)
                        {
                            case "Daily":
                                dailyTasks.RemoveAt(idx);
                                break;
                            case "Weekly":
                                weeklyTasks.RemoveAt(idx);
                                break;
                            case "OneTime":
                                oneTimeTasks.RemoveAt(idx);
                                break;
                        }
                        break;
                    default:
                        return;
                }

                TaskListRefresh(CurrentTaskList);
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion
    }
}