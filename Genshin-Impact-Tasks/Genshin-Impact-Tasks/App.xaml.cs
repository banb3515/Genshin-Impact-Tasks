using Genshin_Impact_Tasks.Models;

using SQLite;

using System;
using System.Diagnostics;
using System.IO;

using Xamarin.Forms;

namespace Genshin_Impact_Tasks
{
    public partial class App : Application
    {
        public static SQLiteOpenFlags OpenFlags = SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.ProtectionComplete;

        public static readonly string DbFileName = "git.db"; // 데이터베이스 파일 이름
        public static readonly string DbFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DbFileName); // 데이터베이스 파일 경로

        public App()
        {
            #region Database
            var db = new SQLiteConnection(DbFilePath, OpenFlags);

            db.CreateTable<DailyTaskTable>();
            db.CreateTable<WeeklyTaskTable>();
            db.CreateTable<OneTimeTaskTable>();

            #region 기본 데이터 삽입
            #region 일일 반복
            if (db.Table<DailyTaskTable>().Count() == 0)
            {
                var task = new DailyTaskTable { Content = "일일 의뢰", IconPath = "Resources/quest_commission.png", Status = false };
                db.Insert(task);

                task = new DailyTaskTable { Content = "레진 소모", IconPath = "Resources/other_fragile_resin.png", Status = false };
                db.Insert(task);
            }
            #endregion

            #region 주간 반복
            if (db.Table<WeeklyTaskTable>().Count() == 0)
            {
                var task = new WeeklyTaskTable { Content = "주민 의뢰", IconPath = "Resources/npc_hertha.png", Status = false };
                db.Insert(task);

                task = new WeeklyTaskTable { Content = "현상 토벌", IconPath = "Resources/enemy_ruin_hunter.png", Status = false };
                db.Insert(task);

                task = new WeeklyTaskTable { Content = "풍마룡 (드발린)", IconPath = "Resources/boss_dvalin.png", Status = false };
                db.Insert(task);

                task = new WeeklyTaskTable { Content = "복풍의 왕랑 (안드리우스)", IconPath = "Resources/boss_andrius.png", Status = false };
                db.Insert(task);

                task = new WeeklyTaskTable { Content = "황금옥 (타르탈리아)", IconPath = "Resources/boss_tartaglia.png", Status = false };
                db.Insert(task);
            }
            #endregion
            #endregion

            db.Close();
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
