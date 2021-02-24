using Genshin_Impact_Tasks.Models;

using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Genshin_Impact_Tasks.Popups
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingPopup : PopupPage
    {
        // 테마 목록
        Dictionary<string, string> Themes { get; set; } = new Dictionary<string, string> 
        { 
            { "시스템 설정", "System" }, { "밝은 테마", "Light" }, { "어두운 테마", "Dark" } 
        };

        public SettingPopup()
        {
            try
            {
                InitializeComponent();

                if (App.UseDarkMode)
                {
                    MainFrame.BackgroundColor = Color.FromHex("333333");
                    ThemeIcon.Source = "Resources/theme_white.png";
                    VibrationIcon.Source = "Resources/vibration_white.png";
                    TADIcon.Source = "Resources/delete_white.png";
                    Divider.Color = Color.White;
                }

                if (Device.RuntimePlatform == Device.UWP)
                {
                    VibrationSetting.IsVisible = false;
                    ThemePicker.WidthRequest = 160;
                    TADDayPicker.WidthRequest = 140;
                }

                #region 테마 선택 초기화
                foreach (var key in Themes.Keys)
                    ThemePicker.Items.Add(key);

                switch (App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "Theme").FirstOrDefault().Value)
                {
                    case "System":
                        ThemePicker.SelectedIndex = 0;
                        break;
                    case "Light":
                        ThemePicker.SelectedIndex = 1;
                        break;
                    case "Dark":
                        ThemePicker.SelectedIndex = 2;
                        break;
                }
                #endregion

                #region 진동 사용 초기화
                VibrationSwitch.IsToggled = App.UseVibration;
                #endregion

                #region 완료된 할 일 삭제 주기 선택 초기화
                TADDayPicker.Items.Add("삭제 안함");
                for (int i = 1; i <= 60; i++)
                    TADDayPicker.Items.Add($"{i}일");

                TADDayPicker.SelectedIndex = Convert.ToInt32(App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "TaskAutoDelete").FirstOrDefault().Value);
                #endregion

                #region 정보 텍스트 초기화
                var dbInfo = new FileInfo(App.DbFilePath);
                InfoText.Text = $"[{App.Version}] DB 용량: {Math.Round(dbInfo.Length / 1024f / 1024f, 3)} MB";
                #endregion

                ClassId = "1";
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }

        #region 닫기 버튼 클릭 시
        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await PopupNavigation.Instance.PopAsync();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 테마 변경 시
        private async void ThemePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (ClassId == "0") return;

                var themeDb = App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "Theme").FirstOrDefault();
                themeDb.Value = Themes[ThemePicker.Items[ThemePicker.SelectedIndex]];
                App.Database.Update(themeDb);

                await App.Current.MainPage.DisplayAlert("테마 변경", "변경된 테마를 적용하려면 앱을 재시작 해야합니다.", "확인");
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 진동 스위치 토글 시
        private void VibrationSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            try
            {
                if (ClassId == "0") return;

                var vibrationDb = App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "Vibration").FirstOrDefault();
                vibrationDb.Value = VibrationSwitch.IsToggled ? "On" : "Off";
                App.Database.Update(vibrationDb);

                App.UseVibration = VibrationSwitch.IsToggled;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 라이선스 보기 버튼 클릭 시
        private async void ViewLicenseButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await PopupNavigation.Instance.PushAsync(new LicensePopup());
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region GitHub 이동 버튼 클릭 시
        private async void MoveGitHubButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await Launcher.OpenAsync(new Uri("https://github.com/banb3515/Genshin-Impact-Tasks"));
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 완료된 할 일 삭제 주기 변경 시
        private void TADDayPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (ClassId == "0") return;

                var tadDb = App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "TaskAutoDelete").FirstOrDefault();
                tadDb.Value = TADDayPicker.SelectedIndex.ToString();
                App.Database.Update(tadDb);
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion
    }
}