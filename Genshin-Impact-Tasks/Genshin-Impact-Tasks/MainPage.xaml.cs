using Genshin_Impact_Tasks.Models;
using Genshin_Impact_Tasks.Pages;
using Genshin_Impact_Tasks.Popups;
using Genshin_Impact_Tasks.Services;

using Plugin.LatestVersion;

using Rg.Plugins.Popup.Services;

using System;
using System.Globalization;
using System.Linq;
using System.Threading;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace Genshin_Impact_Tasks
{
    public partial class MainPage : ContentPage
    {
        private string CurrentTab { get; set; }

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
                CurrentTab = "Task";
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
                string hexColor = App.GetDayOfWeekColor(CurrentDow);

                Device.BeginInvokeOnMainThread(async () =>
                {
                    // 날짜 전화 애니메이션 (0: 처음 실행O, 1: 처음 실행X, 처음 실행한 경우 애니메이션 X)
                    if (ClassId == "1")
                    {
                        _ = TopFrame.FadeTo(0.5, 250, Easing.SpringOut);
                        await CurrentDateText.FadeTo(0, 150, Easing.SpringOut);

                        if (Device.RuntimePlatform == Device.Android) DependencyService.Get<IAppearanceControl>().SetStatusBarColor($"#{hexColor}");
                        TopFrame.BackgroundColor = Color.FromHex(hexColor);
                        CurrentDateText.Text = date.ToString("dddd", new CultureInfo("ko-kr"));

                        _ = TopFrame.FadeTo(1, 1000, Easing.SpringIn);
                        await CurrentDateText.FadeTo(1, 1500, Easing.SpringIn);
                    }
                    else
                    {
                        ClassId = "1";
                        if (Device.RuntimePlatform == Device.Android) DependencyService.Get<IAppearanceControl>().SetStatusBarColor($"#{hexColor}");
                        TopFrame.BackgroundColor = Color.FromHex(hexColor);
                        CurrentDateText.Text = date.ToString("dddd", new CultureInfo("ko-kr"));
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

            TabListDropdownBtn.IsEnabled = false;
            TabBar.IsVisible = true;
            EnableTabButton(false);
            await TabBar.TranslateTo(0, -50, 0);
            _ = TabBar.TranslateTo(0, 0, 250, Easing.CubicOut);
            await TabView.TranslateTo(0, 55, 250, Easing.CubicOut);

            TabListDropdownBtn.IsEnabled = true;
            EnableTabButton();
        }
        #endregion

        #region 탭 목록 숨기기
        private async void HideTabList()
        {
            TabListDropdownBtn.Text = "▼";

            TabListDropdownBtn.IsEnabled = false;
            EnableTabButton(false);
            _ = TabBar.TranslateTo(0, -50, 250, Easing.CubicIn);
            await TabView.TranslateTo(0, 0, 250, Easing.CubicIn);

            TabBar.IsVisible = false;
            TabListDropdownBtn.IsEnabled = true;
            EnableTabButton();
        }
        #endregion

        #region 탭 버튼 활성화
        // 탭 버튼을 활성화하거나 비활성화합니다.
        private void EnableTabButton(bool enable = true)
        {
            try
            {
                TaskTabBtn.IsEnabled = enable;
                FarmingTabBtn.IsEnabled = enable;
                DomainsTabBtn.IsEnabled = enable;
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

                if (tab == CurrentTab)
                {
                    HideTabList();
                    return;
                }

                CurrentTab = tab;

                // 상단 좌측, 현재 보고있는 탭 정보 변경
                switch (tab)
                {
                    case "Task":
                        CurrentTabImage.Source = "Resources/task.png";
                        CurrentTabTitle.Text = "할 일";

                        TaskTab.IsVisible = true;
                        FarmingTab.IsVisible = false;

                        TaskTab.IsEnabled = true;
                        FarmingTab.IsEnabled = false;
                        //DomainsTab.IsEnabled = false;

                        TaskTabView.TimerRepeat = true;
                        FarmingTabView.TimerRepeat = false;
                        break;
                    case "Farming":
                        CurrentTabImage.Source = "Resources/tab_farming.png";
                        CurrentTabTitle.Text = "파밍";

                        FarmingTab.IsVisible = true;
                        TaskTab.IsVisible = false;

                        FarmingTab.IsEnabled = true;
                        TaskTab.IsEnabled = false;
                        //DomainsTab.IsEnabled = false;

                        TaskTabView.TimerRepeat = false;
                        FarmingTabView.TimerRepeat = true;
                        break;
                    case "Domains":
                        await DisplayAlert("비경", "개발 중 입니다.", "확인");

                        /*
                        CurrentTabImage.Source = "Resources/domains.png";
                        CurrentTabTitle.Text = "비경";

                        TabView.SelectedIndex = 2;

                        DomainsTab.IsEnabled = true;
                        TaskTab.IsEnabled = false;
                        FarmingTab.IsEnabled = false;

                        TaskTabView.TimerRepeat = false;
                        FarmingTabView.TimerRepeat = false;
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

        #region 메인 페이지 로드 시
        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
            try
            {
                #region 자동 시작 초기화 (UWP)
                if (Device.RuntimePlatform == Device.UWP)
                {
                    var thread = new Thread(async () =>
                    {
                        var state = await DependencyService.Get<IStartupTask>().GetState();
                        var stDb = App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "StartupTask").FirstOrDefault();
                        stDb.Value = state ? "On" : "Off";
                        App.Database.Update(stDb);

                        App.UseStartupTask = state;
                    });
                }
                #endregion

                #region 업데이트 확인
                if (Connectivity.NetworkAccess == NetworkAccess.Internet && !await CrossLatestVersion.Current.IsUsingLatestVersion())
                {
                    var answer = await DisplayAlert("업데이트", "새로운 버전으로 업데이트가 가능합니다.", "업데이트", "취소");

                    if (answer) await CrossLatestVersion.Current.OpenAppInStore();
                }
                #endregion
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion
    }
}