using Genshin_Impact_Tasks.Models;

using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Genshin_Impact_Tasks.Popups
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddTaskPopup : PopupPage
    {
        public bool UseDarkMode { get; set; } = false; // 다크 모드 사용 여부

        public string CurrentIconPath { get; set; } = ""; // 현재 선택된 아이콘 경로

        public List<TaskModel> tasks; // 중복 확인을 위한 임시 리스트

        public EventHandler<AddTaskResult> OnClosed;

        public AddTaskPopup(List<TaskModel> tasks)
        {
            if (App.Current.RequestedTheme == OSAppTheme.Dark) UseDarkMode = true;

            InitializeComponent();

            this.tasks = tasks;

            if (UseDarkMode)
                MainFrame.BackgroundColor = Color.FromHex("333333");

            ContentEntry.Focus();
        }

        #region 할 일 추가
        private async void AddTask()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ContentEntry.Text))
                {
                    await App.Current.MainPage.DisplayAlert("할 일 추가", "할 일을 입력해주세요.", "확인");
                    ContentEntry.Focus();
                    return;
                }

                if (tasks.Where(t => t.Content == ContentEntry.Text.Trim()).ToList().Count > 0)
                {
                    await App.Current.MainPage.DisplayAlert("할 일 추가", $"[{ContentEntry.Text.Trim()}] (은)는 이미 존재합니다.", "확인");
                    ContentEntry.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(CurrentIconPath)) CurrentIconPath = "Resources/task.png";

                OnClosed?.Invoke(this, new AddTaskResult { Task = new TaskModel { IconPath = CurrentIconPath, Content = ContentEntry.Text.Trim(), Status = false } });
                await PopupNavigation.Instance.PopAsync();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 닫기 버튼 클릭 시
        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                OnClosed?.Invoke(this, new AddTaskResult { Task = null });
                await PopupNavigation.Instance.PopAsync();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 아이콘 선택 버튼 클릭 시
        private async void SelectIconButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var popup = new SelectIconPopup();

                popup.OnClosed += (s, arg) =>
                {
                    if (arg.IconPath != null)
                    {
                        CurrentIconPath = arg.IconPath;

                        IconPreview.Source = CurrentIconPath;
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

        #region 내용 입력 완료 시
        private void EntryContent_Completed(object sender, EventArgs e)
        {
            try
            {
                AddTask();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 추가 버튼 클릭 시
        private void AddButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                AddTask();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion
    }

    // 응답 결과
    public class AddTaskResult
    {
        public TaskModel Task { get; set; } // 추가된 할 일
    }
}