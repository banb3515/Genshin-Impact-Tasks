using Firebase.Database;
using Firebase.Database.Query;

using Genshin_Impact_Tasks.Models;
using Genshin_Impact_Tasks.Pages;

using SQLite;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace Genshin_Impact_Tasks
{
    public partial class App : Application
    {
        public static string Version { get; } = "1.0 Release"; // 앱 버전
        public static string SyncMail { get; set; } // 동기화 이메일
        public static bool UseDarkMode { get; set; } // 다크 모드 사용 여부
        public static bool UseVibration { get; set; } // 진동 사용 여부

        #region Database
        public static SQLiteConnection Database { get; set; }

        public static SQLiteOpenFlags OpenFlags = SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.ProtectionComplete;

        public static readonly string DbFileName = "git.db"; // 데이터베이스 파일 이름

        public static readonly string DbFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DbFileName); // 데이터베이스 파일 경로
        #endregion

        #region Firebase
        public static FirebaseClient Firebase { get; set; }
        public static readonly string FirebaseUrl = "YOUR FIREBASE REALTIME DATABASE URL";
        public static readonly string FirebaseSecretKey = "YOUR FIREBASE SECRET KEY";
        #endregion

        public App()
        {
            InitializeComponent();

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

            #region Database
            Database = new SQLiteConnection(DbFilePath, OpenFlags);

            // 테이블이 존재하지 않을 시 테이블 생성
            Database.CreateTable<SettingTable>();
            Database.CreateTable<DailyTaskTable>();
            Database.CreateTable<WeeklyTaskTable>();
            Database.CreateTable<OneTimeTaskTable>();

            #region 기본 데이터 삽입
            #region 설정 (Setting)
            #region 앱 버전
            if (Database.Table<SettingTable>().ToList().Where(s => s.Key == "Version").Count() == 0)
            {
                var versionDb = new SettingTable { Key = "Version", Value = Version };
                Database.Insert(versionDb);
            }
            #endregion

            #region 할 일 기본값
            if (Database.Table<SettingTable>().ToList().Where(s => s.Key == "DefaultDailyTask").Count() == 0)
            {
                var defaultTaskDb = new SettingTable { Key = "DefaultDailyTask", Value = "False" };
                Database.Insert(defaultTaskDb);
            }

            if (Database.Table<SettingTable>().ToList().Where(s => s.Key == "DefaultWeeklyTask").Count() == 0)
            {
                var defaultTaskDb = new SettingTable { Key = "DefaultWeeklyTask", Value = "False" };
                Database.Insert(defaultTaskDb);
            }
            #endregion

            #region 동기화 이메일
            if (Database.Table<SettingTable>().ToList().Where(s => s.Key == "SyncMail").Count() == 0)
            {
                var syncMailDb = new SettingTable { Key = "SyncMail", Value = "" };
                Database.Insert(syncMailDb);
            }
            #endregion

            #region 동기화 시간
            if (SyncMail != "" && Database.Table<SettingTable>().ToList().Where(s => s.Key == "SyncDate").Count() == 0)
            {
                var syncDateDb = new SettingTable { Key = "SyncDate", Value = "" };
                Database.Insert(syncDateDb);
            }
            #endregion

            #region Date
            if (Database.Table<SettingTable>().ToList().Where(s => s.Key == "Date").Count() == 0)
            {
                var dateDb = new SettingTable { Key = "Date", Value = DateTime.Now.ToString() };
                Database.Insert(dateDb);
            }
            #endregion

            #region 테마
            if (Database.Table<SettingTable>().ToList().Where(s => s.Key == "Theme").Count() == 0)
            {
                var themeDb = new SettingTable { Key = "Theme", Value = "System" };
                Database.Insert(themeDb);
            }
            #endregion

            #region 진동
            if (Database.Table<SettingTable>().ToList().Where(s => s.Key == "Vibration").Count() == 0)
            {
                var vibrationDb = new SettingTable { Key = "Vibration", Value = "On" };
                Database.Insert(vibrationDb);
            }
            #endregion

            #region 자동 삭제
            if (Database.Table<SettingTable>().ToList().Where(s => s.Key == "TaskAutoDelete").Count() == 0)
            {
                var tadDb = new SettingTable { Key = "TaskAutoDelete", Value = "0" };
                Database.Insert(tadDb);
            }
            #endregion
            #endregion

            #region 일일 반복 (DailyTask)
            if (Database.Table<SettingTable>().ToList().Where(s => s.Key == "DefaultDailyTask").FirstOrDefault().Value == "False" && Database.Table<DailyTaskTable>().ToList().Count() == 0)
            {
                var taskDb = new DailyTaskTable { No = 1, Content = "일일 의뢰", IconPath = "Resources/quest_commission.png", Status = false, CreateTime = DateTime.Now.ToString() };
                Database.Insert(taskDb);

                taskDb = new DailyTaskTable { No = 2, Content = "레진 소모", IconPath = "Resources/other_fragile_resin.png", Status = false, CreateTime = DateTime.Now.ToString() };
                Database.Insert(taskDb);

                var ddtDb = Database.Table<SettingTable>().ToList().Where(t => t.Key == "DefaultDailyTask").FirstOrDefault();
                ddtDb.Value = "True";
                Database.Update(ddtDb);
            }
            #endregion

            #region 주간 반복 (WeeklyTask)
            if (Database.Table<SettingTable>().ToList().Where(s => s.Key == "DefaultWeeklyTask").FirstOrDefault().Value == "False" && Database.Table<WeeklyTaskTable>().ToList().Count() == 0)
            {
                var taskDb = new WeeklyTaskTable { No = 1, Content = "주민 의뢰", IconPath = "Resources/npc_hertha.png", Status = false, CreateTime = DateTime.Now.ToString() };
                Database.Insert(taskDb);

                taskDb = new WeeklyTaskTable { No = 2, Content = "현상 토벌", IconPath = "Resources/enemy_ruin_hunter.png", Status = false, CreateTime = DateTime.Now.ToString() };
                Database.Insert(taskDb);

                taskDb = new WeeklyTaskTable { No = 3, Content = "풍마룡 (드발린)", IconPath = "Resources/boss_dvalin.png", Status = false, CreateTime = DateTime.Now.ToString() };
                Database.Insert(taskDb);

                taskDb = new WeeklyTaskTable { No = 4, Content = "복풍의 왕랑 (안드리우스)", IconPath = "Resources/boss_andrius.png", Status = false, CreateTime = DateTime.Now.ToString() };
                Database.Insert(taskDb);

                taskDb = new WeeklyTaskTable { No = 5, Content = "황금옥 (타르탈리아)", IconPath = "Resources/boss_tartaglia.png", Status = false, CreateTime = DateTime.Now.ToString() };
                Database.Insert(taskDb);

                var dwtDb = Database.Table<SettingTable>().ToList().Where(t => t.Key == "DefaultWeeklyTask").FirstOrDefault();
                dwtDb.Value = "True";
                Database.Update(dwtDb);
            }
            #endregion
            #endregion

            #region 변수 초기화
            #region 동기화 이메일
            SyncMail = Database.Table<SettingTable>().ToList().Where(s => s.Key == "SyncMail").FirstOrDefault().Value;
            #endregion

            #region Firebase
            if (SyncMail != "" && Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                var options = new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(FirebaseSecretKey) };
                Firebase = new FirebaseClient(FirebaseUrl, options);
            }
            #endregion

            #region 설정 - 테마
            switch (Database.Table<SettingTable>().ToList().Where(s => s.Key == "Theme").FirstOrDefault().Value)
            {
                case "System":
                    if (Current.RequestedTheme == OSAppTheme.Dark) UseDarkMode = true;
                    else UseDarkMode = false;
                    break;
                case "Light":
                    UseDarkMode = false;
                    break;
                case "Dark":
                    UseDarkMode = true;
                    break;
            }
            #endregion

            #region 설정 - 진동
            if (Database.Table<SettingTable>().ToList().Where(s => s.Key == "Vibration").FirstOrDefault().Value == "On") UseVibration = true;
            else UseVibration = false;
            #endregion
            #endregion
            #endregion
                    
            // Syncfusion 라이센스 등록
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR SYNCFUSION LICENSE KEY");

            MainPage = new MainPage();
        }

        #region 인터넷 변경 감지
        private async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            try
            {
                if (SyncMail == "") return;

                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    if (Firebase == null)
                        Firebase = new FirebaseClient(FirebaseUrl);
                }
                else
                {
                    if (Firebase != null)
                        Firebase.Dispose();
                    Firebase = null;

                    await Current.MainPage.DisplayAlert("인터넷 연결 끊김", 
                        "인터넷에 연결되어 있지 않아 기능이 제한됩니다.\n" +
                        "# 제한되는 기능\n" +
                        "- 할 일 추가, 삭제, 수정\n" +
                        "- 할 일 완료로 변경\n" +
                        "- 일부 설정 변경 불가능", "확인");
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region Global Function
        #region 예외(오류) 표시 함수
        public static async void DisplayEx(Exception ex)
        {
            try
            {
                if (ex.Message.Contains("Permission denied"))
                {
                    await Current.MainPage.DisplayAlert("서버에 연결할 수 없음", $"서버에 연결하기 위한 권한이 없습니다.", "확인");
                    return;
                }

                var answer = await Current.MainPage.DisplayAlert("버그", $"버그가 발생하였습니다.\n- {ex.Message}\n\n해당 버그를 개발자에게 제보하시겠습니까?", "제보", "취소");

                if (!answer) return;

                var body = "- 모든 정보들은 버그 수정을 위해서만 쓰입니다." +
                    "\n\n# 오류 발생 시각: " + DateTime.Now.ToString("yyyy년 M월 d일 H시 m분 s초") +
                    "\n# 디바이스 모델명: " + "(" + DeviceInfo.Manufacturer + ") " + DeviceInfo.Model +
                    "\n# OS 버전: " + DeviceInfo.Platform + " " + DeviceInfo.VersionString +
                    "\n\n# 오류 내용: " + ex.Message +
                    "\n# 오류 상세 내용: " + ex.StackTrace +
                    "\n\n# 버그 발생 계기: " +
                    "\n어떻게 해서 버그가 발생했는지 알려주시면 버그 수정에 도움이됩니다." +
                    "\n예) 설정 버튼을 눌렀는데 버그가 발생하였습니다.\n\n- ";

                var email = new EmailMessage
                {
                    Subject = "원신 태스크 - 버그 제보",
                    Body = body,
                    To = new List<string> { "banb3515@outlook.kr" } 
                };
                await Email.ComposeAsync(email);
            }
            catch (Exception) { }
        }
        #endregion
        
        #region 로컬 동기화 날짜 업데이트
        public static async void UpdateLocalSyncDate()
        {
            try
            {
                var server = (await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).Where(item => item.Object.Key == "SyncDate").FirstOrDefault();
                var local = Database.Table<SettingTable>().ToList().Where(s => s.Key == "SyncDate").FirstOrDefault();
                local.Value = server.Object.Value;
                Database.Update(local);
            }
            catch (Exception ex)
            {
                DisplayEx(ex);
            }
        }
        #endregion

        #region 서버 동기화 날짜 업데이트
        public static async void UpdateServerSyncDate()
        {
            try
            {
                var date = DateTime.Now.ToString();

                var local = Database.Table<SettingTable>().ToList().Where(s => s.Key == "SyncDate").FirstOrDefault();
                local.Value = date;
                Database.Update(local);

                var server = (await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).Where(item => item.Object.Key == "SyncDate").FirstOrDefault();
                server.Object.Value = date;
                await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("Setting").Child(server.Key).PutAsync(server.Object);
            }
            catch (Exception ex)
            {
                DisplayEx(ex);
            }
        }
        #endregion
        #endregion

        #region On Function
        #region 앱이 시작될 때
        protected override void OnStart()
        {
        }
        #endregion

        #region 앱이 백그라운드로 전환될 때
        protected override void OnSleep()
        {
            try
            {
                if (SyncMail != "")
                    TaskTabView.AutoSync = false;
            }
            catch (Exception ex)
            {
                DisplayEx(ex);
            }
        }
        #endregion

        #region 앱이 포그라운드로 전환될 때
        protected override void OnResume()
        {
            try
            {
                if (SyncMail != "")
                    TaskTabView.AutoSync = true;
            }
            catch (Exception ex)
            {
                DisplayEx(ex);
            }
        }
        #endregion
        #endregion
    }
}