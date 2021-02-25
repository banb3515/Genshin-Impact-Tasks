using Firebase.Database;
using Firebase.Database.Query;

using Genshin_Impact_Tasks.Models;

using SQLite;

using System;
using System.IO;
using System.Linq;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace Genshin_Impact_Tasks
{
    public partial class App : Application
    {
        public static string Version { get; } = "1.0 Dev"; // 앱 버전
        public static string SyncMail { get; set; } // 동기화 이메일
        public static bool UseDarkMode { get; set; } // 다크 모드 사용 여부
        public static bool UseVibration { get; set; } // 진동 사용 여부

        #region Database
        public static SQLiteConnection Database { get; set; }
        public static SQLiteConnection AddHistoryDb { get; set; }
        public static SQLiteConnection DeleteHistoryDb { get; set; }
        public static SQLiteConnection UpdateHistoryDb { get; set; }

        public static SQLiteOpenFlags OpenFlags = SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.ProtectionComplete;

        public static readonly string DbFileName = "git.db"; // 데이터베이스 파일 이름
        public static readonly string AddHistoryDbFileName = "add_history.db"; // 기록 데이터베이스 파일 이름
        public static readonly string DeleteHistoryDbFileName = "delete_history.db"; // 삭제 데이터베이스 파일 이름
        public static readonly string UpdateHistoryDbFileName = "update_history.db"; // 수정 기록 데이터베이스 파일 이름

        public static readonly string DbFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DbFileName); // 데이터베이스 파일 경로
        public static readonly string AddHistoryDbFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AddHistoryDbFileName); // 추가 기록 데이터베이스 파일 경로
        public static readonly string DeleteHistoryDbFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DeleteHistoryDbFileName); // 삭제 기록 데이터베이스 파일 경로
        public static readonly string UpdateHistoryDbFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), UpdateHistoryDbFileName); // 수정 기록 데이터베이스 파일 경로
        #endregion

        #region Firebase
        public static readonly string FirebaseUrl = "YOUR FIREBASE REALTIME DATABASE URL";
        public static FirebaseClient Firebase { get; set; }
        #endregion

        public App()
        {
            InitializeComponent();

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
                       

            #region Database
            Database = new SQLiteConnection(DbFilePath, OpenFlags);

            // 테이블이 존재하지 않을 시 테이블 생성
            Database.CreateTable<DailyTaskTable>();
            Database.CreateTable<WeeklyTaskTable>();
            Database.CreateTable<OneTimeTaskTable>();
            Database.CreateTable<SettingTable>();

            #region 기본 데이터 삽입
            #region 설정 (Setting)
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
            if (Database.Table<DailyTaskTable>().ToList().Count() == 0)
            {
                var taskDb = new DailyTaskTable { Content = "일일 의뢰", IconPath = "Resources/quest_commission.png", Status = false, CreateTime = DateTime.Now.ToString() };
                Database.Insert(taskDb);

                taskDb = new DailyTaskTable { Content = "레진 소모", IconPath = "Resources/other_fragile_resin.png", Status = false, CreateTime = DateTime.Now.ToString() };
                Database.Insert(taskDb);
            }
            #endregion

            #region 주간 반복 (WeeklyTask)
            if (Database.Table<WeeklyTaskTable>().ToList().Count() == 0)
            {
                var taskDb = new WeeklyTaskTable { Content = "주민 의뢰", IconPath = "Resources/npc_hertha.png", Status = false, CreateTime = DateTime.Now.ToString() };
                Database.Insert(taskDb);

                taskDb = new WeeklyTaskTable { Content = "현상 토벌", IconPath = "Resources/enemy_ruin_hunter.png", Status = false, CreateTime = DateTime.Now.ToString() };
                Database.Insert(taskDb);

                taskDb = new WeeklyTaskTable { Content = "풍마룡 (드발린)", IconPath = "Resources/boss_dvalin.png", Status = false, CreateTime = DateTime.Now.ToString() };
                Database.Insert(taskDb);

                taskDb = new WeeklyTaskTable { Content = "복풍의 왕랑 (안드리우스)", IconPath = "Resources/boss_andrius.png", Status = false, CreateTime = DateTime.Now.ToString() };
                Database.Insert(taskDb);

                taskDb = new WeeklyTaskTable { Content = "황금옥 (타르탈리아)", IconPath = "Resources/boss_tartaglia.png", Status = false, CreateTime = DateTime.Now.ToString() };
                Database.Insert(taskDb);
            }
            #endregion
            #endregion

            #region 변수 초기화
            #region Firebase
            if (SyncMail != "" && Connectivity.NetworkAccess == NetworkAccess.Internet)
                Firebase = new FirebaseClient(FirebaseUrl);
            #endregion

            #region 동기화 이메일
            SyncMail = Database.Table<SettingTable>().ToList().Where(s => s.Key == "SyncMail").FirstOrDefault().Value;
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

            #region 완료된 할 일 자동 삭제
            var tadDay = Convert.ToInt32(Database.Table<SettingTable>().ToList().Where(s => s.Key == "TaskAutoDelete").FirstOrDefault().Value);

            if (tadDay > 0)
            {
                var currentTime = DateTime.Now;

                App.Database.Table<OneTimeTaskTable>().ToList().ForEach(task =>
                {
                    if (task.Status)
                    {
                        var completeTime = Convert.ToDateTime(task.CompleteTime);
                        completeTime = completeTime.AddDays(tadDay);

                        if (DateTime.Compare(completeTime, currentTime) < 0)
                            Database.Delete(task);
                    }
                });
            }
            #endregion
            #endregion

            #region History Database
            AddHistoryDb = new SQLiteConnection(AddHistoryDbFilePath, OpenFlags);
            DeleteHistoryDb = new SQLiteConnection(DeleteHistoryDbFilePath, OpenFlags);
            UpdateHistoryDb = new SQLiteConnection(UpdateHistoryDbFilePath, OpenFlags);

            AddHistoryDb.CreateTable<DailyTaskTable>();
            AddHistoryDb.CreateTable<WeeklyTaskTable>();
            AddHistoryDb.CreateTable<OneTimeTaskTable>();
            AddHistoryDb.CreateTable<SettingTable>();

            DeleteHistoryDb.CreateTable<DailyTaskTable>();
            DeleteHistoryDb.CreateTable<WeeklyTaskTable>();
            DeleteHistoryDb.CreateTable<OneTimeTaskTable>();
            DeleteHistoryDb.CreateTable<SettingTable>();

            UpdateHistoryDb.CreateTable<DailyTaskTable>();
            UpdateHistoryDb.CreateTable<WeeklyTaskTable>();
            UpdateHistoryDb.CreateTable<OneTimeTaskTable>();
            UpdateHistoryDb.CreateTable<SettingTable>();
            #endregion

            // Syncfusion 라이센스 등록
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR SYNCFUSION LICENSE KEY");

            MainPage = new MainPage();
        }

        #region 인터넷 변경 감지
        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            try
            {
                if (SyncMail != "" && Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    if (Firebase == null)
                        Firebase = new FirebaseClient(FirebaseUrl);

                    ClearHistory();
                }
                else
                {
                    if (Firebase != null)
                        Firebase.Dispose();
                    Firebase = null;
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region Global Function
        #region 예외(오류) 표시 함수
        public static void DisplayEx(Exception ex)
        {
            try
            {
                Current.MainPage.DisplayAlert("Exception", $"오류가 발생하였습니다.\n- {ex.Message}", "확인");
            }
            catch (Exception) { }
        }
        #endregion

        #region History DB 정리
        public static void ClearHistory()
        {
            try
            {
                #region Add History
                AddHistoryDb.Table<DailyTaskTable>().ToList().ForEach(async task =>
                {
                    await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("DailyTask").PostAsync(task);
                    AddHistoryDb.Delete(task);
                });

                AddHistoryDb.Table<WeeklyTaskTable>().ToList().ForEach(async task =>
                {
                    await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("WeeklyTask").PostAsync(task);
                    AddHistoryDb.Delete(task);
                });

                AddHistoryDb.Table<OneTimeTaskTable>().ToList().ForEach(async task =>
                {
                    await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("OneTimeTask").PostAsync(task);
                    AddHistoryDb.Delete(task);
                });
                #endregion

                #region Update History
                UpdateHistoryDb.Table<SettingTable>().ToList().ForEach(async task =>
                {
                    var update = (await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).Where(item => item.Object.Key == task.Key).FirstOrDefault();
                    await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("Setting").Child(update.Key).PutAsync(task);
                    UpdateHistoryDb.Delete(task);
                });

                UpdateHistoryDb.Table<DailyTaskTable>().ToList().ForEach(async task =>
                {
                    var update = (await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("DailyTask").OnceAsync<DailyTaskTable>()).Where(item => item.Object.No == task.No).FirstOrDefault();
                    await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("DailyTask").Child(update.Key).PutAsync(task);
                    UpdateHistoryDb.Delete(task);
                });

                UpdateHistoryDb.Table<WeeklyTaskTable>().ToList().ForEach(async task =>
                {
                    var update = (await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("WeeklyTask").OnceAsync<WeeklyTaskTable>()).Where(item => item.Object.No == task.No).FirstOrDefault();
                    await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("WeeklyTask").Child(update.Key).PutAsync(task);
                    UpdateHistoryDb.Delete(task);
                });

                UpdateHistoryDb.Table<OneTimeTaskTable>().ToList().ForEach(async task =>
                {
                    var update = (await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("OneTimeTask").OnceAsync<DailyTaskTable>()).Where(item => item.Object.No == task.No).FirstOrDefault();
                    await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("OneTimeTask").Child(update.Key).PutAsync(task);
                    UpdateHistoryDb.Delete(task);
                });
                #endregion

                #region Delete History
                DeleteHistoryDb.Table<DailyTaskTable>().ToList().ForEach(async task =>
                {
                    var del = (await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("DailyTask").OnceAsync<DailyTaskTable>()).Where(item => item.Object.No == task.No).FirstOrDefault();
                    await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("DailyTask").Child(del.Key).DeleteAsync();
                    DeleteHistoryDb.Delete(task);
                });

                DeleteHistoryDb.Table<WeeklyTaskTable>().ToList().ForEach(async task =>
                {
                    var del = (await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("WeeklyTask").OnceAsync<WeeklyTaskTable>()).Where(item => item.Object.No == task.No).FirstOrDefault();
                    await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("WeeklyTask").Child(del.Key).DeleteAsync();
                    DeleteHistoryDb.Delete(task);
                });

                DeleteHistoryDb.Table<OneTimeTaskTable>().ToList().ForEach(async task =>
                {
                    var del = (await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("OneTimeTask").OnceAsync<OneTimeTaskTable>()).Where(item => item.Object.No == task.No).FirstOrDefault();
                    await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("OneTimeTask").Child(del.Key).DeleteAsync();
                    DeleteHistoryDb.Delete(task);
                });
                #endregion
            }
            catch (NullReferenceException) { }
        }
        #endregion
        #endregion

        #region On Function
        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
        #endregion
    }
}