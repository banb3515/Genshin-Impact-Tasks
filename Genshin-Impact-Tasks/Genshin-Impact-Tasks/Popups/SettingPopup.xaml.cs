using Genshin_Impact_Tasks.Models;
using Genshin_Impact_Tasks.Pages;
using Genshin_Impact_Tasks.Utils;

using Firebase.Database;
using Firebase.Database.Query;

using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Genshin_Impact_Tasks.Services;

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

        private int PreviousSelected = 0;

        public SettingPopup()
        {
            try
            {
                InitializeComponent();

                if (App.UseDarkMode)
                {
                    MainFrame.BackgroundColor = Color.FromHex("333333");
                    
                    ThemeIcon.Source = "Resources/theme_white.png";
                    NotificationIcon.Source = "Resources/notification_white.png";
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

                    #region 자동 시작 사용 초기화
                    StartupTaskSwitch.IsToggled = App.UseStartupTask;
                    #endregion
                }
                else if (Device.RuntimePlatform == Device.Android)
                {
                    StartupTaskSetting.IsVisible = false;
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

                #region 알림 사용 초기화
                NotificationSwitch.IsToggled = App.UseNotification;
                #endregion

                #region 진동 사용 초기화
                VibrationSwitch.IsToggled = App.UseVibration;
                #endregion

                #region 완료된 할 일 삭제 주기 선택 초기화
                TADDayPicker.Items.Add("삭제 안함");
                for (int i = 1; i <= 60; i++)
                    TADDayPicker.Items.Add($"{i}일");

                var tadDay = Convert.ToInt32(App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "TaskAutoDelete").FirstOrDefault().Value);
                TADDayPicker.SelectedIndex = tadDay;
                PreviousSelected = tadDay;
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

        #region 모든 기능 활성화
        private void EnableAll(bool flag = true)
        {
            try
            {
                CloseButton.IsEnabled = flag;
                SyncSwitch.IsEnabled = flag;
                ThemePicker.IsEnabled = flag;
                NotificationSwitch.IsEnabled = flag;
                VibrationSwitch.IsEnabled = flag;
                TADDayPicker.IsEnabled = flag;
                BugButton.IsEnabled = flag;
                FeedbackButton.IsEnabled = flag;
                LicenseButton.IsEnabled = flag;
                GitHubButton.IsEnabled = flag;
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
                await PopupNavigation.Instance.RemovePageAsync(this);
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
                    if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                    {
                        await App.Current.MainPage.DisplayAlert("동기화", "인터넷에 연결되어 있지 않아 동기화를 할 수 없습니다.\n인터넷 연결 상태를 확인해주시기 바랍니다.", "확인");
                        return;
                    }

                    EnableAll(false);

                    var answer = await App.Current.MainPage.DisplayAlert("동기화",
                        "# 동기화 활성화 시 주의 사항 #\n" +
                        "1. 데이터가 사용될 수 있습니다.\n" +
                        "2. 모든 데이터는 서버에 저장됩니다.\n" +
                        "3. 동기화 해제 시 서버에 있는 데이터를 가져올 수 없습니다.\n" +
                        " - 다시 동기화 시 기존의 데이터를 가져올 수 있습니다.\n\n" +
                        " ! 동기화 활성화 후 인터넷에 연결되어있지 않을 시 제한되는 기능\n" +
                        " - 할 일 추가, 수정, 삭제\n" +
                        " - 파밍 상태 변경\n" +
                        " - 일부 설정 변경", "확인", "취소");

                    if (!answer)
                    {
                        EnableAll();
                        return;
                    }

                    var email = "";

                    while (true)
                    {
                        email = "";
                        email = await App.Current.MainPage.DisplayPromptAsync("동기화", "동기화할 이메일을 입력해주세요.", "확인", "취소", keyboard: Keyboard.Email);

                        if (email == null)
                        {
                            EnableAll();
                            return;
                        }

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

                    if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                    {
                        EnableAll();
                        await App.Current.MainPage.DisplayAlert("동기화", "인터넷에 연결되어 있지 않아 동기화를 할 수 없습니다.\n인터넷 연결 상태를 확인해주시기 바랍니다.", "확인");
                        return;
                    }

                    var random = new Random();
                    var code = random.Next(00000000, 99999999).ToString("D8");
                    var sendMail = new Mail("원신 태스크 인증 번호", $"<h2>원신 태스크 동기화 인증 번호는 [{code}] 입니다.</h2>", email);
                    sendMail.Send();

                    var timeout = DateTime.Now;

                    while (true)
                    {
                        var auth = await App.Current.MainPage.DisplayPromptAsync("동기화",
                            $"<{email}> (으)로 인증 번호가 전송되었습니다.\n인증 번호 8자리를 입력해주세요. (인증 번호 유효 시간 10분)\n(인증 번호가 안왔을 경우 스팸함을 확인해주세요.)",
                            "확인", "취소", maxLength: 8, keyboard: Keyboard.Numeric);

                        if (auth == null)
                        {
                            EnableAll();
                            return;
                        }

                        // 인증 번호 유효 시간 체크
                        if (DateTime.Compare(DateTime.Now, timeout.AddMinutes(10)) > 0)
                        {
                            EnableAll();
                            await App.Current.MainPage.DisplayAlert("동기화", "인증 번호가 만료되었습니다.\n다시 시도해주시기 바랍니다.", "확인");
                            return;
                        }

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

                    if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                    {
                        EnableAll();
                        await App.Current.MainPage.DisplayAlert("동기화", "인터넷에 연결되어 있지 않아 동기화를 할 수 없습니다.\n인터넷 연결 상태를 확인해주시기 바랍니다.", "확인");
                        return;
                    }

                    var loading = new LoadingPopup("동기화 중입니다.");
                    await PopupNavigation.Instance.PushAsync(loading);

                    bool answer2 = false;

                    var options = new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(App.FirebaseSecretKey) };
                    App.Firebase = new FirebaseClient(App.FirebaseUrl, options);

                    try
                    {
                        var tmp = await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>();

                        if (tmp != null && tmp.ToList().Count > 0)
                            answer2 = await App.Current.MainPage.DisplayAlert("동기화",
                                "동기화 방법을 선택해주세요.\n" +
                                "- 서버 기준:\n[로컬(현재 기기)에 있는 정보를 모두 삭제]하고 서버에 있는 정보를 가져옵니다.\n" +
                                "- 로컬 기준:\n[서버에 있는 정보를 모두 삭제]하고 로컬 정보를 서버에 전송합니다.",
                                "서버 기준", "로컬 기준");
                    }
                    catch (NullReferenceException) { }

                    TaskTabView.TimerRepeat = false;
                    FarmingTabView.TimerRepeat = false;

                    #region 서버 기준
                    if (answer2)
                    {
                        App.Database.Table<SettingTable>().ToList().ForEach(s =>
                        {
                            if (s.Key != "Theme" && s.Key != "Vibration" && s.Key != "SyncMail" && s.Key != "Version" && s.Key != "Notification" && s.Key != "StartupTask")
                                App.Database.Delete(s);
                        });
                        App.Database.DropTable<DailyTaskTable>();
                        App.Database.DropTable<WeeklyTaskTable>();
                        App.Database.DropTable<OneTimeTaskTable>();

                        App.Database.CreateTable<DailyTaskTable>();
                        App.Database.CreateTable<WeeklyTaskTable>();
                        App.Database.CreateTable<OneTimeTaskTable>();

                        (await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).ToList().ForEach(item =>
                        {
                            if (item.Object.Key != "Theme" && item.Object.Key != "Vibration" && item.Object.Key != "SyncMail" && item.Object.Key != "Version" && item.Object.Key != "Notification" && item.Object.Key != "StartupTask")
                                App.Database.Insert(item.Object);
                        });
                        (await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("DailyTask").OnceAsync<DailyTaskTable>()).ToList().ForEach(item => { App.Database.Insert(item.Object); });
                        (await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("WeeklyTask").OnceAsync<WeeklyTaskTable>()).ToList().ForEach(item => { App.Database.Insert(item.Object); });
                        (await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("OneTimeTask").OnceAsync<OneTimeTaskTable>()).ToList().ForEach(item => { App.Database.Insert(item.Object); });
                    }
                    #endregion

                    #region 로컬 기준
                    else
                    {
                        try
                        {
                            await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).DeleteAsync();
                        }
                        catch (NullReferenceException) { }

                        // 할 일 동기화 시간 업데이트 - 로컬
                        var syncDateDb = App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "TasksSyncDate").FirstOrDefault();
                        syncDateDb.Value = DateTime.Now.ToString();
                        App.Database.Update(syncDateDb);

                        foreach (var s in App.Database.Table<SettingTable>().ToList())
                            if (s.Key != "Theme" && s.Key != "Vibration" && s.Key != "SyncMail" && s.Key != "Version" && s.Key != "Notification" && s.Key != "StartupTask")
                                await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("Setting").PostAsync(s);
                        foreach (var t in App.Database.Table<DailyTaskTable>().ToList())
                            await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("DailyTask").PostAsync(t);
                        foreach (var t in App.Database.Table<WeeklyTaskTable>().ToList())
                            await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("WeeklyTask").PostAsync(t);
                        foreach (var t in App.Database.Table<OneTimeTaskTable>().ToList())
                            await App.Firebase.Child("UserData").Child(email.Replace('.', '_')).Child("OneTimeTask").PostAsync(t);
                    }
                    #endregion

                    var syncMailDb = App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "SyncMail").FirstOrDefault();
                    syncMailDb.Value = email;
                    App.Database.Update(syncMailDb);

                    SyncMailText.IsVisible = true;
                    SyncMailText.Text = $"<{email}> 동기화 됨";

                    SyncSwitch.ClassId = "1";
                    SyncSwitch.IsToggled = true;

                    await PopupNavigation.Instance.RemovePageAsync(loading);

                    await App.Current.MainPage.DisplayAlert("동기화", "동기화가 완료되었습니다.\n계속하려면 앱을 재시작해야합니다.", "앱 종료");
                    Process.GetCurrentProcess().Kill();
                }
                else
                {
                    var answer = await App.Current.MainPage.DisplayAlert("동기화",
                        $"<{App.SyncMail}> 동기화를 해제합니다.\n" +
                        $"동기화 해제 시 서버에 있는 데이터를 가져올 수 없습니다.\n" +
                        $"서버에 있는 데이터는 사라지지 않습니다.", "해제", "취소");

                    if (!answer)
                    {
                        SyncSwitch.ClassId = "1";
                        SyncSwitch.IsToggled = true;
                        return;
                    }

                    EnableAll(false);

                    SyncMailText.IsVisible = false;
                    SyncMailText.Text = "";

                    SyncSwitch.ClassId = "1";
                    SyncSwitch.IsToggled = false;

                    MainPage.TimerDow = false;
                    TaskTabView.TimerRepeat = false;
                    FarmingTabView.TimerRepeat = false;
                    App.AutoSync = false;

                    App.Database.Table<SettingTable>().ToList().ForEach(s =>
                    {
                        if (s.Key != "Theme" && s.Key != "Vibration" && s.Key != "Version" && s.Key != "Notification" && s.Key != "StartupTask")
                            App.Database.Delete(s);
                    });
                    App.Database.DropTable<DailyTaskTable>();
                    App.Database.DropTable<WeeklyTaskTable>();
                    App.Database.DropTable<OneTimeTaskTable>();

                    await App.Current.MainPage.DisplayAlert("동기화", "동기화를 해제하였습니다.\n계속하려면 앱을 재시작해야합니다.", "앱 종료");
                    Process.GetCurrentProcess().Kill();
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

                EnableAll(false);

                var themeDb = App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "Theme").FirstOrDefault();
                themeDb.Value = Themes[ThemePicker.Items[ThemePicker.SelectedIndex]];
                App.Database.Update(themeDb);

                await App.Current.MainPage.DisplayAlert("테마", "테마가 변경되었습니다.\n테마를 적용하려면 앱을 재시작해야합니다.", "앱 종료");
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 자동 시작 스위치 토글 시
        private async void StartupTaskSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            try
            {
                if (ClassId == "0") return;

                if (StartupTaskSwitch.ClassId == "1")
                {
                    StartupTaskSwitch.ClassId = "0";
                    return;
                }

                StartupTaskSwitch.ClassId = "1";
                StartupTaskSwitch.IsToggled = !StartupTaskSwitch.IsToggled;

                var result = false;

                if (StartupTaskSwitch.IsToggled)
                {
                    result = await DependencyService.Get<IStartupTask>().Enable();

                    if (!result) return;
                }
                else await DependencyService.Get<IStartupTask>().Disable();

                var stDb = App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "StartupTask").FirstOrDefault();
                stDb.Value = result ? "On" : "Off";
                App.Database.Update(stDb);

                StartupTaskSwitch.ClassId = "1";
                StartupTaskSwitch.IsToggled = result;
                App.UseStartupTask = result;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 알림 스위치 토글 시
        private void NotificationSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            try
            {
                if (ClassId == "0") return;

                var notiDb = App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "Notification").FirstOrDefault();
                notiDb.Value = NotificationSwitch.IsToggled ? "On" : "Off";
                App.Database.Update(notiDb);

                App.UseNotification = NotificationSwitch.IsToggled;
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

        #region 완료된 할 일 삭제 주기 변경 시
        private async void TADDayPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (ClassId == "0") return;

                if (App.AutoSync && Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    TADDayPicker.SelectedIndex = PreviousSelected;

                    await App.Current.MainPage.DisplayAlert("설정 변경",
                        "인터넷에 연결되어 있지 않아 일부 설정을 변경할 수 없습니다.\n" +
                        "인터넷 상태를 확인하신 후 다시 시도해주시기 바랍니다.", "확인");
                    return;
                }

                if (App.AutoSync)
                {
                    var update = (await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).Where(item => item.Object.Key == "TaskAutoDelete").FirstOrDefault();
                    update.Object.Value = TADDayPicker.SelectedIndex.ToString();
                    await App.Firebase.Child("UserData").Child(App.SyncMail.Replace('.', '_')).Child("Setting").Child(update.Key).PutAsync(update.Object);
                    App.UpdateServerTasksSyncDate();
                }

                var tadDb = App.Database.Table<SettingTable>().ToList().Where(s => s.Key == "TaskAutoDelete").FirstOrDefault();
                tadDb.Value = TADDayPicker.SelectedIndex.ToString();
                App.Database.Update(tadDb);

                PreviousSelected = TADDayPicker.SelectedIndex;
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

        #region 버그 제보/건의 사항 버튼 클릭 시
        private async void BugFeedbackButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var button = sender as Button;

                var subject = "";
                var body = "";

                switch (button.ClassId)
                {
                    case "Bug":
                        subject = "원신 태스크 - 버그 제보";
                        body = "- 모든 정보들은 버그 수정을 위해서만 쓰입니다." +
                            "\n\n# 디바이스 모델명: " + "(" + DeviceInfo.Manufacturer + ") " + DeviceInfo.Model +
                            "\n# OS 버전: " + DeviceInfo.Platform + " " + DeviceInfo.VersionString +
                            "\n\n# 버그 발생 계기: " +
                            "\n어떻게 해서 버그가 발생했는지 알려주시면 버그 수정에 도움이됩니다." +
                            "\n예) 할 일을 삭제하였더니 갑자기 튕겼습니다.\n\n- ";
                        break;
                    case "Feedback":
                        subject = "원신 태스크 - 건의 사항";
                        body = "# 추가 또는 변경되었으면 좋을 것 같은 기능을 알려주세요." +
                            "\n\n- ";
                        break;
                }

                var email = new EmailMessage
                {
                    Subject = subject,
                    Body = body,
                    To = new List<string> { "banb3515@outlook.kr" }
                };
                await Email.ComposeAsync(email);
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion
    }
}