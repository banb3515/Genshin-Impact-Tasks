using Genshin_Impact_Tasks.Popups;

using Rg.Plugins.Popup.Services;

using System;

using Xamarin.Forms;

namespace Genshin_Impact_Tasks
{
    public partial class MainPage : ContentPage
    {
        public static bool TimerDow { get; set; }

        DayOfWeek CurrentDow { get; set; }

        public MainPage()
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
                TimerDow = true;

                if (App.UseDarkMode) BackgroundColor = Color.FromHex("242424");
                else BackgroundColor = Color.White;

                OnDayOfWeekTimer();

                // 1초 마다 요일이 변경되었는지 확인합니다.
                Device.StartTimer(TimeSpan.FromSeconds(1), OnDayOfWeekTimer);
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 요일 변경 타이머
        private bool OnDayOfWeekTimer()
        {
            try
            {
                if (!TimerDow) return true;

                // 오전 5시 이후에 요일 변경
                DateTime date;
                if (DateTime.Now.Hour >= 5) date = DateTime.Now;
                else date = DateTime.Now.AddDays(-1);

                if (ClassId == "1" && CurrentDow == date.DayOfWeek) return true;
                CurrentDow = date.DayOfWeek;

                // 요일별 색상 적용
                string hexColor = "000000";

                switch (CurrentDow)
                {
                    case DayOfWeek.Sunday:
                        hexColor = "CF513D";
                        break;
                    case DayOfWeek.Monday:
                        hexColor = "E2B446";
                        break;
                    case DayOfWeek.Tuesday:
                        hexColor = "FF93AC";
                        break;
                    case DayOfWeek.Wednesday:
                        hexColor = "54B948";
                        break;
                    case DayOfWeek.Thursday:
                        hexColor = "FF6600";
                        break;
                    case DayOfWeek.Friday:
                        hexColor = "19C5FF";
                        break;
                    case DayOfWeek.Saturday:
                        hexColor = "663199";
                        break;
                }

                Device.BeginInvokeOnMainThread(async () =>
                {
                    // 날짜 전화 애니메이션 (0: 처음 실행O, 1: 처음 실행X, 처음 실행한 경우 애니메이션 X)
                    if (ClassId == "1")
                    {
                        _ = TopFrame.FadeTo(0.5, 250, Easing.SpringOut);
                        await CurrentDateText.FadeTo(0, 150, Easing.SpringOut);

                        TopFrame.BackgroundColor = Color.FromHex(hexColor);
                        CurrentDateText.Text = date.ToString("dddd");

                        _ = TopFrame.FadeTo(1, 1000, Easing.SpringIn);
                        await CurrentDateText.FadeTo(1, 1500, Easing.SpringIn);
                    }
                    else
                    {
                        ClassId = "1";
                        TopFrame.BackgroundColor = Color.FromHex(hexColor);
                        CurrentDateText.Text = date.ToString("dddd");
                    }
                });
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }

            return true;
        }
        #endregion

        #region 탭 드롭다운 버튼 클릭 시
        // 탭을 표시하거나 숨깁니다.
        private void TabListDropdownBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (TabBar.IsVisible)
                    HideTabList();
                else
                    ShowTabList();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 탭 목록 보기
        private async void ShowTabList()
        {
            TabListDropdownBtn.Text = "▲";
            Test.RowDefinitions[2].Height = 60;

            TabListDropdownBtn.IsEnabled = false;
            TabBar.IsVisible = true;
            EnableTabButton(false);
            await TabBar.TranslateTo(0, -50, 0);
            _ = TabBar.TranslateTo(0, 0, 250, Easing.CubicOut);

            switch (CurrentTabTitle.Text)
            {
                case "할 일":
                    await TaskTab.TranslateTo(0, -50, 0);
                    await TaskTab.TranslateTo(0, 0, 250, Easing.CubicOut);
                    break;
                case "비경":
                    await DomainsTab.TranslateTo(0, -50, 0);
                    await DomainsTab.TranslateTo(0, 0, 250, Easing.CubicOut);
                    break;
                case "타이머":
                    await TimerTab.TranslateTo(0, -50, 0);
                    await TimerTab.TranslateTo(0, 0, 250, Easing.CubicOut);
                    break;
            }

            TabListDropdownBtn.IsEnabled = true;
            EnableTabButton();

            switch (CurrentTabTitle.Text)
            {
                case "할 일":
                    await TaskTab.TranslateTo(0, 0, 0);
                    break;
                case "비경":
                    await DomainsTab.TranslateTo(0, 0, 0);
                    break;
                case "타이머":
                    await TimerTab.TranslateTo(0, 0, 0);
                    break;
            }
        }
        #endregion

        #region 탭 목록 숨기기
        private async void HideTabList()
        {
            TabListDropdownBtn.Text = "▼";
            TabListDropdownBtn.IsEnabled = false;
            EnableTabButton(false);
            _ = TabBar.TranslateTo(0, -50, 250, Easing.CubicIn);

            switch (CurrentTabTitle.Text)
            {
                case "할 일":
                    await TaskTab.TranslateTo(0, -50, 250, Easing.CubicIn);
                    break;
                case "비경":
                    await DomainsTab.TranslateTo(0, -50, 250, Easing.CubicIn);
                    break;
                case "타이머":
                    await TimerTab.TranslateTo(0, -50, 250, Easing.CubicIn);
                    break;
            }

            TabBar.IsVisible = false;
            TabListDropdownBtn.IsEnabled = true;
            EnableTabButton();

            switch (CurrentTabTitle.Text)
            {
                case "할 일":
                    await TaskTab.TranslateTo(0, 0, 0);
                    break;
                case "비경":
                    await DomainsTab.TranslateTo(0, 0, 0);
                    break;
                case "타이머":
                    await TimerTab.TranslateTo(0, 0, 0);
                    break;
            }

            Test.RowDefinitions[2].Height = GridLength.Auto;
        }
        #endregion

        #region 탭 버튼 활성화
        // 탭 버튼을 활성화하거나 비활성화합니다.
        private void EnableTabButton(bool enable = true)
        {
            try
            {
                TaskTabBtn.IsEnabled = enable;
                DomainsTabBtn.IsEnabled = enable;
                TimerTabBtn.IsEnabled = enable;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 탭 이동
        private async void NavigateTab_Clicked(object sender, EventArgs e)
        {
            try
            {
                var tab = (sender as Button).BindingContext as string;

                // 상단 좌측, 현재 보고있는 탭 정보 변경
                switch (tab)
                {
                    case "Task":
                        CurrentTabImage.Source = "Resources/task.png";
                        CurrentTabTitle.Text = "할 일";

                        TaskTab.IsVisible = true;
                        DomainsTab.IsVisible = false;
                        TimerTab.IsVisible = false;
                        break;
                    case "Domains":
                        await DisplayAlert("비경", "개발 중 입니다.", "확인");

                        /*
                        CurrentTabImage.Source = "Resources/domains.png";
                        CurrentTabTitle.Text = "비경";

                        TaskTab.IsVisible = false;
                        DomainsTab.IsVisible = true;
                        TimerTab.IsVisible = false;
                        */
                        break;
                    case "Timer":
                        await DisplayAlert("타이머", "개발 중 입니다.", "확인");

                        /*
                        CurrentTabImage.Source = "Resources/timer.png";
                        CurrentTabTitle.Text = "타이머";

                        TaskTab.IsVisible = false;
                        DomainsTab.IsVisible = false;
                        TimerTab.IsVisible = true;
                        */
                        break;
                }

                HideTabList();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 상단 프레임 아래로 스와이프 시 탭 목록 보기
        private void TopFrame_DownSwiped(object sender, SwipedEventArgs e)
        {
            if(!TabBar.IsVisible && TabListDropdownBtn.IsEnabled)
                ShowTabList();
        }
        #endregion

        #region 상단 프레임 위로 스와이프 시 탭 목록 숨기기
        private void TopFrame_UpSwiped(object sender, SwipedEventArgs e)
        {
            if (TabBar.IsVisible && TabListDropdownBtn.IsEnabled)
                HideTabList();
        }
        #endregion

        #region 설정 버튼 클릭 시
        private async void SettingButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                SettingButton.IsEnabled = false;
                await PopupNavigation.Instance.PushAsync(new SettingPopup());
                SettingButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion
    }
}
