using Genshin_Impact_Tasks.Models;

using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Genshin_Impact_Tasks.Popups
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddTaskPopup : PopupPage
    {
        public string CurrentIconPath { get; set; } = ""; // 현재 선택된 아이콘 경로

        public string CurrentTaskList { get; set; }

        public EventHandler<AddTaskResult> OnClosed;

        public AddTaskPopup(string currentTaskList)
        {
            InitializeComponent();

            CurrentTaskList = currentTaskList;

            if (App.UseDarkMode)
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

                if (string.IsNullOrWhiteSpace(CurrentIconPath)) CurrentIconPath = "Resources/task.png";

                OnClosed?.Invoke(this, new AddTaskResult { Task = new TaskModel { IconPath = CurrentIconPath, Content = ContentEntry.Text.Trim(), Status = false } });
                await PopupNavigation.Instance.RemovePageAsync(this);
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
                await PopupNavigation.Instance.RemovePageAsync(this);
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
                IconPreview.IsEnabled = false;

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

                IconPreview.IsEnabled = true;
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