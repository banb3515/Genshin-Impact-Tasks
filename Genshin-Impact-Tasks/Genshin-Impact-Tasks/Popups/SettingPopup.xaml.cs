using Firebase.Database;
using Firebase.Database.Query;

using Genshin_Impact_Tasks.Models;
using Genshin_Impact_Tasks.Pages;
using Genshin_Impact_Tasks.Utils;

using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
                    SyncIcon.Source = "Resources/sync_white.png";

                    Divider.Color = Color.White;
                }

                if (Device.RuntimePlatform == Device.UWP)
                {
                    VibrationSetting.IsVisible = false;
                    ThemePicker.WidthRequest = 160;
                    TADDayPicker.WidthRequest = 140;
                }

                #region 동기화 설정 초기화
                SyncSwitch.IsToggled = !string.IsNullOrWhiteSpace(App.SyncMail);

                if (!string.IsNullOrWhiteSpace(App.SyncMail))
                {
                    SyncMailText.IsVisible = true;
                    SyncMailText.Text = $"<{App.SyncMail}> 동기화 됨";
                } 
                else SyncMailText.IsVisible = false;
                #endregion

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
                InfoText.Text = $"[{App.Version}]";
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

        #region 동기화 스위치 토글 시
        private async void SyncSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            try
            {
                if (ClassId == "0") return;

                if (SyncSwitch.ClassId == "1")
                {
                    SyncSwitch.ClassId = "0";
                    return;
                }

                SyncSwitch.ClassId = "1";
                SyncSwitch.IsToggled = !SyncSwitch.IsToggled;

                if (SyncSwitch.IsToggled)
                {
                    await App.Current.MainPage.DisplayAlert("동기화", "동기화 활성화 시 데이터가 사용될 수 있습니다.", "확인");

                    var email = "";

                    while (true)
                    {
                        email = "";
                        email = await App.Current.MainPage.DisplayPromptAsync("동기화", "동기화할 이메일을 입력해주세요.", "확인", "취소", keyboard: Keyboard.Email);

                        if (email == null) return;

                        if (email.Trim() == "")
                        {
                            await App.Current.MainPage.DisplayAlert("동기화", "동기화할 이메일을 입력해주세요.", "확인");
                            continue;
                        }

                        try
                        {
                            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
                            {
                                await App.Current.MainPage.DisplayAlert("동기화", "올바른 이메일 형식을 입력해주세요.", "확인");
                                continue;
                            }
                        }
                        catch (RegexMatchTimeoutException)
                        {
                            await App.Current.MainPage.DisplayAlert("동기화", "올바른 이메일 형식을 입력해주세요.", "확인");
                            continue;
                        }

                        break;
                    }

                    var random = new Random();
                    var code = random.Next(00000000, 99999999).ToString("D8");
                    var sendMail = new Mail("원신 태스크 인증 번호", $"<h2>원신 태스크 동기화 인증 번호는 [{code}] 입니다.</h2>", email);
                    sendMail.Send();

                    while (true)
                    {
                        var auth = await App.Current.MainPage.DisplayPromptAsync("동기화", 
                            $"<{email}> (으)로 인증 번호가 전송되었습니다.\n인증 번호 8자리를 입력해주세요.\n(인증 번호가 안왔을 경우 스팸함을 확인해주세요.)", 
                            "확인", "취소", maxLength: 8, keyboard: Keyboard.Numeric);

                        if (auth == null) return;

                        if (auth.Trim() == "")
                        {
                            await App.Current.MainPage.DisplayAlert("동기화", "인증 번호를 입력해주세요.", "확인");
                            continue;
                        }

                        if (code != auth)
                        {
                            await App.Current.MainPage.DisplayAlert("동기화", "올바른 인증 번호를 입력해주세요.", "확인");
                            continue;
                        }

                        break;
                    }

                    bool answer = false;

                    try
                    {
                        if ((await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).ToList().Count > 0)
                            answer = await App.Current.MainPage.DisplayAlert("동기화",
                                "동기화 방법을 선택해주세요.\n" +
                                "- 서버 기준:\n[로컬(현재 기기)에 있는 정보를 모두 삭제]하고 서버에 있는 정보를 가져옵니다.\n" +
                                "- 로컬 기준:\n[서버에 있는 정보를 모두 삭제]하고 로컬 정보를 서버에 전송합니다.",
                                "서버 기준", "로컬 기준");
                    }
                    catch (NullReferenceException) { }

                    TaskTabView.TimerRepeat = false;
                    
                    #region 서버 기준
                    if (answer)
                    {
                        App.Database.Table<SettingTable>().ToList().ForEach(task => { App.Database.Delete(task); });
                        App.Database.Table<DailyTaskTable>().ToList().ForEach(task => { App.Database.Delete(task); });
                        App.Database.Table<WeeklyTaskTable>().ToList().ForEach(task => { App.Database.Delete(task); });
                        App.Database.Table<OneTimeTaskTable>().ToList().ForEach(task => { App.Database.Delete(task); });

                        // 동기화 시간 업데이트 - 서버
                        var syncDateFb = (await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).Where(item => item.Object.Key == "SyncDate").FirstOrDefault();
                        syncDateFb.Object.Value = DateTime.Now.ToString();
                        await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("Setting").Child(syncDateFb.Key).PutAsync(syncDateFb.Object);

                        (await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).ToList().ForEach(item => { App.Database.Insert(item.Object); });
                        (await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("DailyTask").OnceAsync<DailyTaskTable>()).ToList().ForEach(item => { App.Database.Insert(item.Object); });
                        (await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("WeeklyTask").OnceAsync<WeeklyTaskTable>()).ToList().ForEach(item => { App.Database.Insert(item.Object); });
                        (await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("OneTimeTask").OnceAsync<OneTimeTaskTable>()).ToList().ForEach(item => { App.Database.Insert(item.Object); });
                    }
                    #endregion

                    #region 로컬 기준
                    else
                    {
                        await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("Setting").DeleteAsync();
                        await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("DailyTask").DeleteAsync();
                        await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("WeeklyTask").DeleteAsync();
                        await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("OneTimeTask").DeleteAsync();

                        // 동기화 시간 업데이트 - 로컬
                        var syncDateDb = App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "SyncDate").FirstOrDefault();
                        syncDateDb.Value = DateTime.Now.ToString();
                        App.Database.Update(syncDateDb);

                        App.Database.Table<SettingTable>().ToList().ForEach(async s => { await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("Setting").PostAsync(s); });
                        App.Database.Table<DailyTaskTable>().ToList().ForEach(async t => { await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("DailyTask").PostAsync(t); });
                        App.Database.Table<WeeklyTaskTable>().ToList().ForEach(async t => { await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("WeeklyTask").PostAsync(t); });
                        App.Database.Table<OneTimeTaskTable>().ToList().ForEach(async t => { await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("OneTimeTask").PostAsync(t); });
                    }
                    #endregion

                    var syncMailDb = App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "SyncMail").FirstOrDefault();
                    syncMailDb.Value = email;
                    App.Database.Update(syncMailDb);

                    App.SyncMail = email;

                    SyncMailText.IsVisible = true;
                    SyncMailText.Text = $"<{App.SyncMail}> 동기화 됨";

                    SyncSwitch.ClassId = "1";
                    SyncSwitch.IsToggled = true;

                    if (App.Firebase == null)
                        App.Firebase = new FirebaseClient(App.FirebaseUrl);

                    await App.Current.MainPage.DisplayAlert("동기화", "동기화가 완료되었습니다.\n앱을 재시작해야 적용됩니다.", "앱 종료");
                    Process.GetCurrentProcess().Kill();
                }
                else
                {
                    var answer = await App.Current.MainPage.DisplayAlert("동기화", $"<{App.SyncMail}> 동기화를 해제합니다.", "해제", "취소");

                    if (!answer)
                    {
                        SyncSwitch.ClassId = "1";
                        SyncSwitch.IsToggled = true;
                        return;
                    }

                    SyncMailText.IsVisible = false;
                    SyncMailText.Text = "";

                    SyncSwitch.ClassId = "1";
                    SyncSwitch.IsToggled = false;

                    var syncMailDb = App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "SyncMail").FirstOrDefault();
                    syncMailDb.Value = "";
                    App.Database.Update(syncMailDb);

                    App.SyncMail = "";

                    if (App.Firebase != null)
                        App.Firebase.Dispose();
                    App.Firebase = null;
                }
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

                if (App.SyncMail != "" && Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).Where(item => item.Object.Key == "Theme").FirstOrDefault();
                    update.Object.Value = themeDb.Value;
                    await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("Setting").Child(update.Key).PutAsync(update.Object);
                }
                else
                {
                    var localDb = App.UpdateHistoryDb.Table<SettingTable>().ToList().Where(t => t.Key == "Theme");

                    if (localDb.ToList().Count > 0)
                        App.UpdateHistoryDb.Update(themeDb);
                    else
                        App.UpdateHistoryDb.Insert(themeDb);
                }

                await App.Current.MainPage.DisplayAlert("테마 변경", "변경된 테마를 적용하려면 앱을 재시작 해야합니다.", "확인");
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 진동 스위치 토글 시
        private async void VibrationSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            try
            {
                if (ClassId == "0") return;

                var vibrationDb = App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "Vibration").FirstOrDefault();
                vibrationDb.Value = VibrationSwitch.IsToggled ? "On" : "Off";
                App.Database.Update(vibrationDb);

                if (App.SyncMail != "" && Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).Where(item => item.Object.Key == "Vibration").FirstOrDefault();
                    update.Object.Value = vibrationDb.Value;
                    await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("Setting").Child(update.Key).PutAsync(update.Object);
                }
                else
                {
                    var localDb = App.UpdateHistoryDb.Table<SettingTable>().ToList().Where(t => t.Key == "Vibration");

                    if (localDb.ToList().Count > 0)
                        App.UpdateHistoryDb.Update(vibrationDb);
                    else
                        App.UpdateHistoryDb.Insert(vibrationDb);
                }

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
        private async void TADDayPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (ClassId == "0") return;

                var tadDb = App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "TaskAutoDelete").FirstOrDefault();
                tadDb.Value = TADDayPicker.SelectedIndex.ToString();
                App.Database.Update(tadDb);

                if (App.SyncMail != "" && Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).Where(item => item.Object.Key == "TaskAutoDelete").FirstOrDefault();
                    update.Object.Value = tadDb.Value;
                    await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("Setting").Child(update.Key).PutAsync(update.Object);
                }
                else
                {
                    var localDb = App.UpdateHistoryDb.Table<SettingTable>().ToList().Where(t => t.Key == "TaskAutoDelete");

                    if (localDb.ToList().Count > 0)
                        App.UpdateHistoryDb.Update(tadDb);
                    else
                        App.UpdateHistoryDb.Insert(tadDb);
                }
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion
    }
}