using System;

using Xamarin.Forms;

namespace Genshin_Impact_Tasks
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            Init();
        }

        #region MainPage 로드 시 실행되는 함수
        private void Init()
        {
            try
            {
                CurrentDateText.Text = DateTime.Now.ToString("dddd");
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex, this);
            }
        }
        #endregion

        #region 탭 드롭다운 버튼 클릭 시
        // 탭을 표시하거나 숨깁니다.
        private async void TabDropdownButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var button = (sender as Button);

                switch (button.ClassId)
                {
                    case "Show":
                        button.ClassId = "Hide";
                        button.Text = "▼";
                        button.IsEnabled = false;
                        EnableTabButton(false);
                        await Tab.TranslateTo(0, -50, 250, Easing.CubicIn);
                        Tab.IsVisible = false;
                        button.IsEnabled = true;
                        EnableTabButton();
                        break;
                    case "Hide":
                        button.ClassId = "Show";
                        button.Text = "▲";
                        button.IsEnabled = false;
                        Tab.IsVisible = true;
                        EnableTabButton(false);
                        await Tab.TranslateTo(0, -50, 0);
                        await Tab.TranslateTo(0, 0, 250, Easing.CubicIn);
                        button.IsEnabled = true;
                        EnableTabButton();
                        break;
                }
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex, this);
            }
        }
        #endregion

        #region 탭 버튼 활성화
        // 탭 버튼을 활성화하거나 비활성화합니다.
        private void EnableTabButton(bool enable = true)
        {
            try
            {
                TaskTab.IsEnabled = enable;
                DomainsTab.IsEnabled = enable;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex, this);
            }
        }
        #endregion

        #region 탭 이동
        private async void NavigateTab_Clicked(object sender, EventArgs e)
        {
            try
            {
                switch ((sender as Button).BindingContext as string)
                {
                    case "Task":
                        CurrentTabImage.Source = "Resources/task.png";
                        CurrentTabTitle.Text = "할 일";
                        break;
                    case "Domains":
                        CurrentTabImage.Source = "Resources/domains.png";
                        CurrentTabTitle.Text = "비경";
                        break;
                }

                TabDropdownButton.ClassId = "Hide";
                TabDropdownButton.Text = "▼";
                TabDropdownButton.IsEnabled = false;
                EnableTabButton(false);
                await Tab.TranslateTo(0, -50, 250, Easing.CubicIn);
                Tab.IsVisible = false;
                TabDropdownButton.IsEnabled = true;
                EnableTabButton();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex, this);
            }
        }
        #endregion
    }
}
