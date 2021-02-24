using Genshin_Impact_Tasks.Models;

using SQLite;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Xamarin.Forms;

namespace Genshin_Impact_Tasks
{
    public partial class App : Application
    {
        public static string Version { get; } = "1.0 Dev"; // 앱 버전
        public static bool UseDarkMode { get; set; } // 다크 모드 사용 여부
        public static bool UseVibration { get; set; } // 진동 사용 여부

        #region Database
        public static SQLiteConnection Database { get; set; }

        public static SQLiteOpenFlags OpenFlags = SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.ProtectionComplete;

        public static readonly string DbFileName = "git.db"; // 데이터베이스 파일 이름
        public static readonly string DbFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DbFileName); // 데이터베이스 파일 경로
        #endregion

        public App()
        {
            InitializeComponent();

            #region Database
            Database = new SQLiteConnection(DbFilePath, OpenFlags);

            // 테이블이 존재하지 않을 시 테이블 생성
            Database.CreateTable<DailyTaskTable>();
            Database.CreateTable<WeeklyTaskTable>();
            Database.CreateTable<OneTimeTaskTable>();
            Database.CreateTable<SettingTable>();

            #region 기본 데이터 삽입
            #region 설정 (Setting)
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
                var tarDb = new SettingTable { Key = "TaskAutoDelete", Value = "0" };
                Database.Insert(tarDb);
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

            // Syncfusion 라이센스 등록
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR LICENSE KEY");

            MainPage = new MainPage();
        }

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