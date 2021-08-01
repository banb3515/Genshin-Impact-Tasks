using Genshin_Impact_Tasks.Models;
using Genshin_Impact_Tasks.Pages;

using Firebase.Database;
using Firebase.Database.Query;

using SQLite;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace Genshin_Impact_Tasks
{
    public partial class App : Application
    {
        public static string Version { get; } = "1.7 Release"; // 앱 버전

        public static string SyncMail { get; set; } // 동기화 이메일
        public static bool AutoSync { get; set; } = false;
        public static bool UseDarkMode { get; set; } // 다크 모드 사용 여부
        public static bool UseVibration { get; set; } // 진동 사용 여부
        public static bool UseStartupTask { get; set; } // 자동 시작 사용 여부
        public static bool UseNotification { get; set; } // 알림 사용 여부

        public static List<int> Notifications { get; set; } // 알림 예약된 ID

        public static ObservableCollection<IconPickerModel> IconNode { get; set; } // 아이콘 선택 팝업 노드

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

            Notifications = new List<int>();

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

            #region Database
            Database = new SQLiteConnection(DbFilePath, OpenFlags);

            #region 테이블 생성
            Database.CreateTable<SettingTable>();
            Database.CreateTable<DailyTaskTable>();
            Database.CreateTable<WeeklyTaskTable>();
            Database.CreateTable<OneTimeTaskTable>();

            Database.CreateTable<MineralFarmingTable>();
            Database.CreateTable<CollectionFarmingTable>();
            #endregion

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

            #region 할 일 동기화 시간
            if (SyncMail != "" && Database.Table<SettingTable>().ToList().Where(s => s.Key == "TasksSyncDate").Count() == 0)
            {
                var syncDateDb = new SettingTable { Key = "TasksSyncDate", Value = "" };
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

            #region 자동 시작
            if (Device.RuntimePlatform == Device.UWP && Database.Table<SettingTable>().ToList().Where(s => s.Key == "StartupTask").Count() == 0)
            {
                var stDb = new SettingTable { Key = "StartupTask", Value = "Off" };
                Database.Insert(stDb);
            }
            #endregion

            #region 알림
            if (Database.Table<SettingTable>().ToList().Where(s => s.Key == "Notification").Count() == 0)
            {
                var notiDb = new SettingTable { Key = "Notification", Value = "Off" };
                Database.Insert(notiDb);
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

            #region 광물 파밍 (MineralFarming)
            var mfStartId = 1000;
            var mfDbList = new List<MineralFarmingTable>
            {
                new MineralFarmingTable { Id = mfStartId++, Name = "콜 라피스", IconPath = "Resources/mineral_cor_lapis.png", RespawnHour = 48, Status = false },
                new MineralFarmingTable { Id = mfStartId++, Name = "야박석", IconPath = "Resources/mineral_noctilucous_jade.png", RespawnHour = 48, Status = false },
                new MineralFarmingTable { Id = mfStartId++, Name = "수정덩이", IconPath = "Resources/mineral_crystal_chunk.png", RespawnHour = 72, Status = false },
                new MineralFarmingTable { Id = mfStartId++, Name = "철광", IconPath = "Resources/mineral_iron_chunk.png", RespawnHour = 24, Status = false },
                new MineralFarmingTable { Id = mfStartId++, Name = "백철", IconPath = "Resources/mineral_white_iron_chunk.png", RespawnHour = 48, Status = false },
                new MineralFarmingTable { Id = mfStartId++, Name = "성은 광석", IconPath = "Resources/mineral_starsilver.png", RespawnHour = 48, Status = false },
                new MineralFarmingTable { Id = mfStartId++, Name = "전기 수정", IconPath = "Resources/mineral_electro_crystal.png", RespawnHour = 48, Status = false }
            };

            if (Database.Table<MineralFarmingTable>().ToList().Count() == 0)
            {
                foreach (var db in mfDbList)
                    Database.Insert(db);
            }
            else
            {
                var db = Database.Table<MineralFarmingTable>().Select(x => x.Id);

                foreach (var item in mfDbList)
                    if (!db.Contains(item.Id))
                        Database.Insert(item);
            }
            #endregion

            #region 채집물 파밍 (CollectionFarming)
            var cfStartId = 2000;
            var cfDbList = new List<CollectionFarmingTable>
            {
                new CollectionFarmingTable { Id = cfStartId++, Name = "청심", IconPath = "Resources/collection_qingxin.png", RespawnHour = 48, Status = false },
                new CollectionFarmingTable { Id = cfStartId++, Name = "예상꽃", IconPath = "Resources/collection_silk_flower.png", RespawnHour = 48, Status = false },
                new CollectionFarmingTable { Id = cfStartId++, Name = "바람버섯", IconPath = "Resources/collection_philanemo_mushroom.png", RespawnHour = 48, Status = false },
                new CollectionFarmingTable { Id = cfStartId++, Name = "세실리아꽃", IconPath = "Resources/collection_cecilia.png", RespawnHour = 48, Status = false },
                new CollectionFarmingTable { Id = cfStartId++, Name = "낙락베리", IconPath = "Resources/collection_valberry.png", RespawnHour = 48, Status = false },
                new CollectionFarmingTable { Id = cfStartId++, Name = "풍차국화", IconPath = "Resources/collection_windwheel_aster.png", RespawnHour = 48, Status = false },
                new CollectionFarmingTable { Id = cfStartId++, Name = "통통연꽃", IconPath = "Resources/collection_calla_lily.png", RespawnHour = 48, Status = false },
                new CollectionFarmingTable { Id = cfStartId++, Name = "유리백합", IconPath = "Resources/collection_glaze_lily.png", RespawnHour = 48, Status = false },
                new CollectionFarmingTable { Id = cfStartId++, Name = "절운고추", IconPath = "Resources/collection_jueyun_chili.png", RespawnHour = 48, Status = false },
                new CollectionFarmingTable { Id = cfStartId++, Name = "유리주머니", IconPath = "Resources/collection_violetgrass.png", RespawnHour = 48, Status = false },
                new CollectionFarmingTable { Id = cfStartId++, Name = "고리고리 열매", IconPath = "Resources/collection_wolfhook.png", RespawnHour = 48, Status = false },
                new CollectionFarmingTable { Id = cfStartId++, Name = "등불꽃", IconPath = "Resources/collection_small_lamp_grass.png", RespawnHour = 48, Status = false },
                new CollectionFarmingTable { Id = cfStartId++, Name = "민들레 씨앗", IconPath = "Resources/collection_dandelion_seed.png", RespawnHour = 48, Status = false },
                new CollectionFarmingTable { Id = cfStartId++, Name = "(Beta) 클로리스", IconPath = "Resources/npc_chloris.png", RespawnHour = 72, Status = false },
            };

            if (Database.Table<CollectionFarmingTable>().ToList().Count() == 0)
            {
                foreach (var db in cfDbList)
                    Database.Insert(db);
            }
            else
            {
                var db = Database.Table<CollectionFarmingTable>().ToList().Select(x => x.Id);

                foreach (var item in cfDbList)
                    if (!db.Contains(item.Id))
                        Database.Insert(item);
            }
            #endregion
            #endregion

            #region 변수 초기화
            #region 동기화 이메일
            SyncMail = Database.Table<SettingTable>().ToList().Where(s => s.Key == "SyncMail").FirstOrDefault().Value;
            AutoSync = SyncMail != "";
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

            #region 설정 - 자동 시작
            if (Device.RuntimePlatform == Device.UWP)
            {
                if (Database.Table<SettingTable>().ToList().Where(s => s.Key == "StartupTask").FirstOrDefault().Value == "On") UseStartupTask = true;
                else UseStartupTask = false;
            }
            #endregion

            #region 설정 - 알림
            if (Database.Table<SettingTable>().ToList().Where(s => s.Key == "Notification").FirstOrDefault().Value == "On") UseNotification = true;
            else UseNotification = false;
            #endregion

            #region 설정 - 진동
            UseVibration = Database.Table<SettingTable>().ToList().Where(s => s.Key == "Vibration").FirstOrDefault().Value == "On";
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

        #region 날짜 별 색상 가져오기
        public static string GetDayOfWeekColor(DayOfWeek dow)
        {
            switch (dow)
            {
                case DayOfWeek.Sunday:
                    return "CF513D";
                case DayOfWeek.Monday:
                    return "E2B446";
                case DayOfWeek.Tuesday:
                    return "FF93AC";
                case DayOfWeek.Wednesday:
                    return "54B948";
                case DayOfWeek.Thursday:
                    return "FF6600";
                case DayOfWeek.Friday:
                    return "19C5FF";
                case DayOfWeek.Saturday:
                    return "663199";
            }

            return "000000";
        }
        #endregion

        #region 로컬 할 일 동기화 날짜 업데이트
        public static async void UpdateLocalTasksSyncDate()
        {
            try
            {
                var server = (await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).Where(item => item.Object.Key == "TasksSyncDate").FirstOrDefault();
                var local = Database.Table<SettingTable>().ToList().Where(s => s.Key == "TasksSyncDate").FirstOrDefault();
                local.Value = server.Object.Value;
                Database.Update(local);
            }
            catch (Exception ex)
            {
                DisplayEx(ex);
            }
        }
        #endregion

        #region 서버 할 일 동기화 날짜 업데이트
        public static async void UpdateServerTasksSyncDate()
        {
            try
            {
                var date = DateTime.Now.ToString();

                var local = Database.Table<SettingTable>().ToList().Where(s => s.Key == "TasksSyncDate").FirstOrDefault();
                local.Value = date;
                Database.Update(local);

                var server = (await Firebase.Child("UserData").Child(SyncMail.Replace('.', '_')).Child("Setting").OnceAsync<SettingTable>()).Where(item => item.Object.Key == "TasksSyncDate").FirstOrDefault();
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
            try
            {
                #region 아이콘 선택 팝업 노드 초기화
                var node = new ObservableCollection<IconPickerModel>();

                #region 노드 목록
                #region 캐릭터
                var characterNode = new IconPickerModel
                {
                    Title = "캐릭터",
                    IconPath = "Resources/character_traveler.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "여행자", IconPath = "Resources/character_traveler.png", Sub = true },
                        new IconPickerModel { Title = "진", IconPath = "Resources/character_jean.png", Sub = true },
                        new IconPickerModel { Title = "다이루크", IconPath = "Resources/character_diluc.png", Sub = true },
                        new IconPickerModel { Title = "모나", IconPath = "Resources/character_mona.png", Sub = true },
                        new IconPickerModel { Title = "각청", IconPath = "Resources/character_keqing.png", Sub = true },
                        new IconPickerModel { Title = "치치", IconPath = "Resources/character_qiqi.png", Sub = true },
                        new IconPickerModel { Title = "벤티", IconPath = "Resources/character_venti.png", Sub = true },
                        new IconPickerModel { Title = "클레", IconPath = "Resources/character_klee.png", Sub = true },
                        new IconPickerModel { Title = "타르탈리아", IconPath = "Resources/character_tartaglia.png", Sub = true },
                        new IconPickerModel { Title = "종려", IconPath = "Resources/character_zhongli.png", Sub = true },
                        new IconPickerModel { Title = "알베도", IconPath = "Resources/character_albedo.png", Sub = true },
                        new IconPickerModel { Title = "감우", IconPath = "Resources/character_ganyu.png", Sub = true },
                        new IconPickerModel { Title = "소", IconPath = "Resources/character_xiao.png", Sub = true },
                        new IconPickerModel { Title = "호두", IconPath = "Resources/character_hu_tao.png", Sub = true },

                        new IconPickerModel { Title = "엠버", IconPath = "Resources/character_amber.png", Sub = true },
                        new IconPickerModel { Title = "바바라", IconPath = "Resources/character_barbara.png", Sub = true },
                        new IconPickerModel { Title = "북두", IconPath = "Resources/character_beidou.png", Sub = true },
                        new IconPickerModel { Title = "베넷", IconPath = "Resources/character_bennett.png", Sub = true },
                        new IconPickerModel { Title = "중운", IconPath = "Resources/character_chongyun.png", Sub = true },
                        new IconPickerModel { Title = "디오나", IconPath = "Resources/character_diona.png", Sub = true },
                        new IconPickerModel { Title = "피슬", IconPath = "Resources/character_fischl.png", Sub = true },
                        new IconPickerModel { Title = "케이아", IconPath = "Resources/character_kaeya.png", Sub = true },
                        new IconPickerModel { Title = "리사", IconPath = "Resources/character_lisa.png", Sub = true },
                        new IconPickerModel { Title = "응광", IconPath = "Resources/character_ningguang.png", Sub = true },
                        new IconPickerModel { Title = "노엘", IconPath = "Resources/character_noelle.png", Sub = true },
                        new IconPickerModel { Title = "레이저", IconPath = "Resources/character_razor.png", Sub = true },
                        new IconPickerModel { Title = "설탕", IconPath = "Resources/character_sucrose.png", Sub = true },
                        new IconPickerModel { Title = "향릉", IconPath = "Resources/character_xiangling.png", Sub = true },
                        new IconPickerModel { Title = "행추", IconPath = "Resources/character_xingqiu.png", Sub = true },
                        new IconPickerModel { Title = "신염", IconPath = "Resources/character_xinyan.png", Sub = true }
                    }
                };
                #endregion

                #region NPC
                var npcNode = new IconPickerModel
                {
                    Title = "NPC",
                    IconPath = "Resources/npc_paimon.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "페이몬", IconPath = "Resources/npc_paimon.png", Sub = true },
                        new IconPickerModel { Title = "헤르타 [페보니우스 기사단] 보급", IconPath = "Resources/npc_hertha.png", Sub = true },
                        new IconPickerModel { Title = "클로리스 [식물학자]", IconPath = "Resources/npc_chloris.png", Sub = true },
                    }
                };
                #endregion

                #region 보스
                var bossNode = new IconPickerModel
                {
                    Title = "보스",
                    IconPath = "Resources/boss_dvalin.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "풍마룡 (드발린)", IconPath = "Resources/boss_dvalin.png", Sub = true },
                        new IconPickerModel { Title = "복풍의 왕랑 (안드리우스)", IconPath = "Resources/boss_andrius.png", Sub = true },
                        new IconPickerModel { Title = "황금옥 (타르탈리아)", IconPath = "Resources/boss_tartaglia.png", Sub = true },

                        new IconPickerModel { Title = "무상의 바람", IconPath = "Resources/boss_anemo_hypostasis.png", Sub = true },
                        new IconPickerModel { Title = "무상의 뇌전", IconPath = "Resources/boss_electro_hypostasis.png", Sub = true },
                        new IconPickerModel { Title = "무상의 바위", IconPath = "Resources/boss_geo_hypostasis.png", Sub = true },
                        new IconPickerModel { Title = "얼음 나무", IconPath = "Resources/boss_cryo_regisvine.png", Sub = true },
                        new IconPickerModel { Title = "물의 정령", IconPath = "Resources/boss_oceanid.png", Sub = true },
                        new IconPickerModel { Title = "폭염 나무", IconPath = "Resources/boss_pyro_regisvine.png", Sub = true },
                        new IconPickerModel { Title = "고대 바위 용 도마뱀", IconPath = "Resources/boss_primo_geovishap.png", Sub = true }
                    }
                };
                #endregion

                #region 몬스터
                var enemyNode = new IconPickerModel
                {
                    Title = "몬스터",
                    IconPath = "Resources/enemy_ruin_hunter.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "츄츄 폭도", IconPath = "Resources/enemy_mitachurl.png", Sub = true },
                        new IconPickerModel { Title = "츄츄왕", IconPath = "Resources/enemy_stonehide_lawachurl.png", Sub = true },
                        new IconPickerModel { Title = "심연 메이지", IconPath = "Resources/enemy_hydro_abyss_mage.png", Sub = true },
                        new IconPickerModel { Title = "유적 가디언", IconPath = "Resources/enemy_ruin_guard.png", Sub = true },
                        new IconPickerModel { Title = "유적 헌터", IconPath = "Resources/enemy_ruin_hunter.png", Sub = true },
                        new IconPickerModel { Title = "우인단·치친 술사", IconPath = "Resources/enemy_fatui_electro_cicin_mage.png", Sub = true },
                        new IconPickerModel { Title = "우인단·채무 처리인", IconPath = "Resources/enemy_fatui_pyro_agent.png", Sub = true },
                        new IconPickerModel { Title = "광풍의 핵", IconPath = "Resources/enemy_eye_of_the_storm.png", Sub = true },
                        new IconPickerModel { Title = "바위 용 도마뱀", IconPath = "Resources/enemy_geovishap.png", Sub = true },
                        new IconPickerModel { Title = "새끼 바위 용 도마뱀", IconPath = "Resources/enemy_geovishap_hatchling.png", Sub = true }
                    }
                };
                #endregion

                #region 무기
                var weaponNode = new IconPickerModel
                {
                    Title = "무기",
                    IconPath = "Resources/weapon_skyward_blade.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "천공의 검", IconPath = "Resources/weapon_skyward_blade.png", Sub = true },
                        new IconPickerModel { Title = "매의 검", IconPath = "Resources/weapon_aquila_favonia.png", Sub = true },
                        new IconPickerModel { Title = "참봉의 칼날", IconPath = "Resources/weapon_summit_shaper.png", Sub = true },
                        new IconPickerModel { Title = "반암결록", IconPath = "Resources/weapon_primordial_jade_cutter.png", Sub = true },

                        new IconPickerModel { Title = "천공의 긍지", IconPath = "Resources/weapon_skyward_pride.png", Sub = true },
                        new IconPickerModel { Title = "늑대의 말로", IconPath = "Resources/weapon_wolfs_gravestone.png", Sub = true },
                        new IconPickerModel { Title = "무공의 검", IconPath = "Resources/weapon_the_unforged.png", Sub = true },

                        new IconPickerModel { Title = "천공의 마루", IconPath = "Resources/weapon_skyward_spine.png", Sub = true },
                        new IconPickerModel { Title = "관홍의 창", IconPath = "Resources/weapon_vortex_vanquisher.png", Sub = true },
                        new IconPickerModel { Title = "화박연", IconPath = "Resources/weapon_primordial_jade_winged_spear.png", Sub = true },
                        new IconPickerModel { Title = "호마의 지팡이", IconPath = "Resources/weapon_staff_of_homa.png", Sub = true },

                        new IconPickerModel { Title = "천공의 두루마리", IconPath = "Resources/weapon_skyward_atlas.png", Sub = true },
                        new IconPickerModel { Title = "사풍 원서", IconPath = "Resources/weapon_lost_prayer_to_the_sacred_winds.png", Sub = true },
                        new IconPickerModel { Title = "속세의 자물쇠", IconPath = "Resources/weapon_memory_of_dust.png", Sub = true },

                        new IconPickerModel { Title = "천공의 날개", IconPath = "Resources/weapon_skyward_harp.png", Sub = true },
                        new IconPickerModel { Title = "아모스의 활", IconPath = "Resources/weapon_amos_bow.png", Sub = true }
                    }
                };
                #endregion

                #region 성유물
                var artifactNode = new IconPickerModel
                {
                    Title = "성유물",
                    IconPath = "Resources/artifact_gladiators_nostalgia.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "검투사의 피날레", IconPath = "Resources/artifact_gladiators_nostalgia.png", Sub = true },
                        new IconPickerModel { Title = "대지를 유랑하는 악단", IconPath = "Resources/artifact_troupes_dawnlight.png", Sub = true },
                        new IconPickerModel { Title = "옛 왕실의 의식", IconPath = "Resources/artifact_royal_flora.png", Sub = true },
                        new IconPickerModel { Title = "사랑받는 소녀", IconPath = "Resources/artifact_maidens_distant_love.png", Sub = true },
                        new IconPickerModel { Title = "날아오르는 유성", IconPath = "Resources/artifact_summer_nights_bloom.png", Sub = true },
                        new IconPickerModel { Title = "불타오르는 화염의 마녀", IconPath = "Resources/artifact_witchs_flower_of_blaze.png", Sub = true },
                        new IconPickerModel { Title = "불 위를 걷는 현인", IconPath = "Resources/artifact_lavawalker_resolution.png", Sub = true },
                        new IconPickerModel { Title = "몰락한 마음", IconPath = "Resources/artifact_gilded_corsage.png", Sub = true },
                        new IconPickerModel { Title = "번개 같은 분노", IconPath = "Resources/artifact_thunderbirds_mercy.png", Sub = true },
                        new IconPickerModel { Title = "뇌명을 평정한 존자", IconPath = "Resources/artifact_thundersoother_heart.png", Sub = true },
                        new IconPickerModel { Title = "청록색 그림자", IconPath = "Resources/artifact_in_remembrance_of_viridescent_fields.png", Sub = true },
                        new IconPickerModel { Title = "얼음바람 속에서 길잃은 용사", IconPath = "Resources/artifact_snowswept_memory.png", Sub = true },
                        new IconPickerModel { Title = "유구한 반암", IconPath = "Resources/artifact_flower_of_creviced_cliff.png", Sub = true },
                        new IconPickerModel { Title = "피에 물든 기사도", IconPath = "Resources/artifact_bloodstained_flower_of_iron.png", Sub = true }
                    }
                };
                #endregion

                #region 임무
                var questNode = new IconPickerModel
                {
                    Title = "임무",
                    IconPath = "Resources/quest_quest.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "임무", IconPath = "Resources/quest_quest.png", Sub = true },
                        new IconPickerModel { Title = "일일 의뢰", IconPath = "Resources/quest_commission.png", Sub = true }
                    }
                };
                #endregion

                #region 웨이포인트
                var waypointNode = new IconPickerModel
                {
                    Title = "웨이포인트",
                    IconPath = "Resources/waypoint_waypoint.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "웨이포인트", IconPath = "Resources/waypoint_waypoint.png", Sub = true },
                        new IconPickerModel { Title = "비경", IconPath = "Resources/waypoint_domains.png", Sub = true },
                        new IconPickerModel { Title = "나선비경", IconPath = "Resources/waypoint_spiral_abyss.png", Sub = true }
                    }
                };
                #endregion

                #region 눈동자
                var eyesNode = new IconPickerModel
                {
                    Title = "눈동자",
                    IconPath = "Resources/eyes_anemoculus.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "바람 신의 눈동자", IconPath = "Resources/eyes_anemoculus.png", Sub = true },
                        new IconPickerModel { Title = "바위 신의 눈동자", IconPath = "Resources/eyes_geoculus.png", Sub = true },
                        new IconPickerModel { Title = "진홍의 옥수", IconPath = "Resources/eyes_crimson_agate.png", Sub = true }
                    }
                };
                #endregion

                #region 광물
                var mineralNode = new IconPickerModel
                {
                    Title = "광물",
                    IconPath = "Resources/mineral_cor_lapis.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "철광", IconPath = "Resources/mineral_iron_chunk.png", Sub = true },
                        new IconPickerModel { Title = "백철", IconPath = "Resources/mineral_white_iron_chunk.png", Sub = true },
                        new IconPickerModel { Title = "수정덩이", IconPath = "Resources/mineral_crystal_chunk.png", Sub = true },
                        new IconPickerModel { Title = "성은 광석", IconPath = "Resources/mineral_starsilver.png", Sub = true },
                        new IconPickerModel { Title = "마법 수정석 조각", IconPath = "Resources/mineral_magical_crystal_chunk.png", Sub = true },
                        new IconPickerModel { Title = "전기 수정", IconPath = "Resources/mineral_electro_crystal.png", Sub = true },
                        new IconPickerModel { Title = "콜 라피스", IconPath = "Resources/mineral_cor_lapis.png", Sub = true },
                        new IconPickerModel { Title = "야박석", IconPath = "Resources/mineral_noctilucous_jade.png", Sub = true },
                    }
                };
                #endregion

                #region 채집물
                var collectionNode = new IconPickerModel
                {
                    Title = "채집물",
                    IconPath = "Resources/collection_qingxin.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "통통 연꽃", IconPath = "Resources/collection_calla_lily.png", Sub = true },
                        new IconPickerModel { Title = "세실리아꽃", IconPath = "Resources/collection_cecilia.png", Sub = true },
                        new IconPickerModel { Title = "민들레 씨앗", IconPath = "Resources/collection_dandelion_seed.png", Sub = true },
                        new IconPickerModel { Title = "바람버섯", IconPath = "Resources/collection_philanemo_mushroom.png", Sub = true },
                        new IconPickerModel { Title = "등불꽃", IconPath = "Resources/collection_small_lamp_grass.png", Sub = true },
                        new IconPickerModel { Title = "낙락베리", IconPath = "Resources/collection_valberry.png", Sub = true },
                        new IconPickerModel { Title = "풍차 국화", IconPath = "Resources/collection_windwheel_aster.png", Sub = true },
                        new IconPickerModel { Title = "고리고리 열매", IconPath = "Resources/collection_wolfhook.png", Sub = true },

                        new IconPickerModel { Title = "유리백합", IconPath = "Resources/collection_glaze_lily.png", Sub = true },
                        new IconPickerModel { Title = "절운고추", IconPath = "Resources/collection_jueyun_chili.png", Sub = true },
                        new IconPickerModel { Title = "청심", IconPath = "Resources/collection_qingxin.png", Sub = true },
                        new IconPickerModel { Title = "예상꽃", IconPath = "Resources/collection_silk_flower.png", Sub = true },
                        new IconPickerModel { Title = "유리주머니", IconPath = "Resources/collection_violetgrass.png", Sub = true },

                        new IconPickerModel { Title = "화염꽃 꽃술", IconPath = "Resources/collection_flaming_flower_stamen.png", Sub = true },
                        new IconPickerModel { Title = "서리꽃 꽃술", IconPath = "Resources/collection_mist_flower_corolla.png", Sub = true }
                    }
                };
                #endregion

                #region 음식
                var foodNode = new IconPickerModel
                {
                    Title = "음식",
                    IconPath = "Resources/food_adeptus_temptation.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "「높이 쌓기」", IconPath = "Resources/food_pile_em_up.png", Sub = true },
                        new IconPickerModel { Title = "선도장", IconPath = "Resources/food_adeptus_temptation.png", Sub = true },
                        new IconPickerModel { Title = "모험가 계란빵", IconPath = "Resources/food_adventurers_breakfast_sandwich.png", Sub = true },
                        new IconPickerModel { Title = "사과", IconPath = "Resources/food_apple.png", Sub = true },
                        new IconPickerModel { Title = "행인두부", IconPath = "Resources/food_almond_tofu.png", Sub = true },
                        new IconPickerModel { Title = "사과주", IconPath = "Resources/food_apple_cider.png", Sub = true },
                        new IconPickerModel { Title = "죽순 수프", IconPath = "Resources/food_bamboo_shoot_soup.png", Sub = true },
                        new IconPickerModel { Title = "바람신의 잡채", IconPath = "Resources/food_barbatos_ratatouille.png", Sub = true },
                        new IconPickerModel { Title = "라즈베리 허브 쥬스", IconPath = "Resources/food_berry_mint_rurst.png", Sub = true },
                        new IconPickerModel { Title = "삶은 검정 농어", IconPath = "Resources/food_black_back_perch_stew.png", Sub = true },
                        new IconPickerModel { Title = "통통 연꽃 해산물 수프", IconPath = "Resources/food_calla_lily_seafood_soup.png", Sub = true },
                        new IconPickerModel { Title = "버섯 닭꼬치", IconPath = "Resources/food_chicken_mushroom_skewer.png", Sub = true },
                        new IconPickerModel { Title = "냉채수육", IconPath = "Resources/food_cold_cut_platter.png", Sub = true },
                        new IconPickerModel { Title = "흥얼채", IconPath = "Resources/food_come_and_get_it.png", Sub = true },
                        new IconPickerModel { Title = "꽃게알 야채찜", IconPath = "Resources/food_crab_ham_veggie_bake.png", Sub = true },
                        new IconPickerModel { Title = "크림 스튜", IconPath = "Resources/food_cream_stew.png", Sub = true },
                        new IconPickerModel { Title = "수정 새우딤섬", IconPath = "Resources/food_crystal_shrimp.png", Sub = true },
                        new IconPickerModel { Title = "어부 토스트", IconPath = "Resources/food_fishermans_toast.png", Sub = true },
                        new IconPickerModel { Title = "불꽃 미트 스파게티", IconPath = "Resources/food_flaming_red_bolognese.png", Sub = true },
                        new IconPickerModel { Title = "무완자 튀김", IconPath = "Resources/food_fried_radish_balls.png", Sub = true },
                        new IconPickerModel { Title = "명월 딤섬", IconPath = "Resources/food_fullmoon_egg.png", Sub = true },
                        new IconPickerModel { Title = "골든크랩", IconPath = "Resources/food_golden_crab.png", Sub = true },
                        new IconPickerModel { Title = "황금 새우볼", IconPath = "Resources/food_golden_shrimp_balls.png", Sub = true },
                        new IconPickerModel { Title = "뜨끈 야채 스튜", IconPath = "Resources/food_goulash.png", Sub = true },
                        new IconPickerModel { Title = "흘호어 구이", IconPath = "Resources/food_grilled_tiger_fish.png", Sub = true },
                        new IconPickerModel { Title = "성수", IconPath = "Resources/food_holy_water.png", Sub = true },
                        new IconPickerModel { Title = "비옥야채쌈", IconPath = "Resources/food_jade_parcels.png", Sub = true },
                        new IconPickerModel { Title = "진주비취백옥탕", IconPath = "Resources/food_jewelry_soup.png", Sub = true },
                        new IconPickerModel { Title = "절운고추 치킨", IconPath = "Resources/food_jueyun_chili_chicken.png", Sub = true },
                        new IconPickerModel { Title = "절운 누룽지", IconPath = "Resources/food_jueyun_guoba.png", Sub = true },
                        new IconPickerModel { Title = "연꽃 파이", IconPath = "Resources/food_lotus_flower_crisp.png", Sub = true },
                        new IconPickerModel { Title = "연밥 계란찜", IconPath = "Resources/food_lotus_seed_and_bird_egg_soup.png", Sub = true },
                        new IconPickerModel { Title = "버섯고기말이", IconPath = "Resources/food_matsutake_meat_rolls.png", Sub = true },
                        new IconPickerModel { Title = "몬드 생선구이", IconPath = "Resources/food_mondstadt_grilled_fish.png", Sub = true },
                        new IconPickerModel { Title = "몬드 감자전", IconPath = "Resources/food_mondstadt_hash_brown.png", Sub = true },
                        new IconPickerModel { Title = "달빛 파이", IconPath = "Resources/food_moon_pie.png", Sub = true },
                        new IconPickerModel { Title = "모라육", IconPath = "Resources/food_mora_meat.png", Sub = true },
                        new IconPickerModel { Title = "버섯피자", IconPath = "Resources/food_mushroom_pizza.png", Sub = true },
                        new IconPickerModel { Title = "산미 울면", IconPath = "Resources/food_noodles_with_mountain_delicacies.png", Sub = true },
                        new IconPickerModel { Title = "북극 사과고기찜", IconPath = "Resources/food_northern_apple_stew.png", Sub = true },
                        new IconPickerModel { Title = "북극 훈제 닭", IconPath = "Resources/food_northern_smoked_chicken.png", Sub = true },
                        new IconPickerModel { Title = "대왕차", IconPath = "Resources/food_pops_teas.png", Sub = true },
                        new IconPickerModel { Title = "경책 가정식", IconPath = "Resources/food_qingce_stir_fry.png", Sub = true },
                        new IconPickerModel { Title = "무 수프", IconPath = "Resources/food_radish_veggie_soup.png", Sub = true },
                        new IconPickerModel { Title = "만족 샐러드", IconPath = "Resources/food_satisfying_salad.png", Sub = true },
                        new IconPickerModel { Title = "버터 송이구이", IconPath = "Resources/food_saut_fed_matsutake.png", Sub = true },
                        new IconPickerModel { Title = "탕수어", IconPath = "Resources/food_squirrel_fish.png", Sub = true },
                        new IconPickerModel { Title = "스테이크", IconPath = "Resources/food_steak.png", Sub = true },
                        new IconPickerModel { Title = "허니캐럿그릴", IconPath = "Resources/food_sticky_honey_roast.png", Sub = true },
                        new IconPickerModel { Title = "고기볶음", IconPath = "Resources/food_stir_fried_filet.png", Sub = true },
                        new IconPickerModel { Title = "바위항 지삼선", IconPath = "Resources/food_stone_harbor_delicacies.png", Sub = true },
                        new IconPickerModel { Title = "일몰 열매", IconPath = "Resources/food_sunsettia.png", Sub = true },
                        new IconPickerModel { Title = "버터 생선 구이", IconPath = "Resources/food_sunshine_sprat.png", Sub = true },
                        new IconPickerModel { Title = "달콤달콤 닭고기 스튜", IconPath = "Resources/food_sweet_madame.png", Sub = true },
                        new IconPickerModel { Title = "장원 팬케익", IconPath = "Resources/food_tea_break_pancake.png", Sub = true },
                        new IconPickerModel { Title = "티바트 달걀 프라이", IconPath = "Resources/food_teyvat_fried_egg.png", Sub = true },
                        new IconPickerModel { Title = "천추육", IconPath = "Resources/food_tianshu_meat.png", Sub = true },
                        new IconPickerModel { Title = "고기죽순데침", IconPath = "Resources/food_triple_layered_consomm.png", Sub = true },
                        new IconPickerModel { Title = "평화의 네모", IconPath = "Resources/food_universal_peace.png", Sub = true },
                        new IconPickerModel { Title = "채식 전복", IconPath = "Resources/food_vegetarian_abalone.png", Sub = true },
                        new IconPickerModel { Title = "고리고리 쥬스", IconPath = "Resources/food_wolfhook_juice.png", Sub = true },
                        new IconPickerModel { Title = "중원 내장꼬치", IconPath = "Resources/food_zhongyuan_chop_suey.png", Sub = true },
                    }
                };
                #endregion

                #region 경험치
                var experienceNode = new IconPickerModel
                {
                    Title = "경험치",
                    IconPath = "Resources/experience_adventure.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "모험", IconPath = "Resources/experience_adventure.png", Sub = true },
                        new IconPickerModel { Title = "캐릭터", IconPath = "Resources/experience_character.png", Sub = true },
                        new IconPickerModel { Title = "리월 평판", IconPath = "Resources/experience_liyue_reputation.png", Sub = true },
                        new IconPickerModel { Title = "몬드 평판", IconPath = "Resources/experience_mondstadt_reputation.png", Sub = true },
                        new IconPickerModel { Title = "기행", IconPath = "Resources/experience_battle_pass.png", Sub = true }
                    }
                };
                #endregion

                #region 강화/레벨업 재료
                var enhancementNode = new IconPickerModel
                {
                    Title = "강화/레벨업 재료",
                    IconPath = "Resources/enhancement_mystic_enhancement_ore.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "정제용 마법 광물", IconPath = "Resources/enhancement_mystic_enhancement_ore.png", Sub = true },
                        new IconPickerModel { Title = "영웅의 경험", IconPath = "Resources/enhancement_heros_wit.png", Sub = true }
                    }
                };
                #endregion

                #region 돌파 재료
                var ascensionNode = new IconPickerModel
                {
                    Title = "돌파 재료",
                    IconPath = "Resources/ascension_brilliant_diamond_gemstone.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "휘황찬란한 다이아몬드", IconPath = "Resources/ascension_brilliant_diamond_gemstone.png", Sub = true },
                        new IconPickerModel { Title = "자유로운 터키석", IconPath = "Resources/ascension_vayuda_turquoise_gemstone.png", Sub = true },
                        new IconPickerModel { Title = "서늘한 빙옥", IconPath = "Resources/ascension_shivada_jade_gemstone.png", Sub = true },
                        new IconPickerModel { Title = "승리의 자수정", IconPath = "Resources/ascension_vajrada_amethyst_gemstone.png", Sub = true },
                        new IconPickerModel { Title = "단단한 황옥", IconPath = "Resources/ascension_prithiva_topaz_gemstone.png", Sub = true },
                        new IconPickerModel { Title = "순수한 청금석", IconPath = "Resources/ascension_varunada_lazurite_gemstone.png", Sub = true },
                        new IconPickerModel { Title = "불타오르는 마노", IconPath = "Resources/ascension_agnidus_agate_gemstone.png", Sub = true },

                        new IconPickerModel { Title = "폭풍의 씨앗", IconPath = "Resources/ascension_hurricane_seed.png", Sub = true },
                        new IconPickerModel { Title = "서리의 핵", IconPath = "Resources/ascension_hoarfrost_core.png", Sub = true },
                        new IconPickerModel { Title = "뇌광 프리즘", IconPath = "Resources/ascension_lightning_prism.png", Sub = true },
                        new IconPickerModel { Title = "현암의 탑", IconPath = "Resources/ascension_basalt_pillar.png", Sub = true },
                        new IconPickerModel { Title = "물처럼 맑은 마음", IconPath = "Resources/ascension_cleansing_heart.png", Sub = true },
                        new IconPickerModel { Title = "꺼지지 않는 불씨", IconPath = "Resources/ascension_everflame_seed.png", Sub = true },
                        new IconPickerModel { Title = "설익은 옥", IconPath = "Resources/ascension_juvenile_jade.png", Sub = true },

                        new IconPickerModel { Title = "고탑 왕의 깨진 꿈", IconPath = "Resources/ascension_scattered_piece_of_decarabian_dream.png", Sub = true },
                        new IconPickerModel { Title = "칼바람 울프의 그리운 고향", IconPath = "Resources/ascension_boreal_wolfs_nostalgia.png", Sub = true },
                        new IconPickerModel { Title = "라이언 투사의 이념", IconPath = "Resources/ascension_dream_of_the_dandelion_gladiator.png", Sub = true },
                        new IconPickerModel { Title = "고운한림의 신체", IconPath = "Resources/ascension_divine_body_from_guyun.png", Sub = true },
                        new IconPickerModel { Title = "안개구름 속의 전환", IconPath = "Resources/ascension_mist_veiled_primo_elixir.png", Sub = true },
                        new IconPickerModel { Title = "흑운철 덩이", IconPath = "Resources/ascension_chunk_of_aerosiderite.png", Sub = true }
                    }
                };
                #endregion

                #region 특성 레벨업 재료
                var talentNode = new IconPickerModel
                {
                    Title = "특성 레벨업 재료",
                    IconPath = "Resources/talent_philosophies_of_freedom.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "「자유」의 철학", IconPath = "Resources/talent_philosophies_of_freedom.png", Sub = true },
                        new IconPickerModel { Title = "「투쟁」의 철학", IconPath = "Resources/talent_philosophies_of_resistance.png", Sub = true },
                        new IconPickerModel { Title = "「시문」의 철학", IconPath = "Resources/talent_philosophies_of_ballad.png", Sub = true },
                        new IconPickerModel { Title = "「번영」의 철학", IconPath = "Resources/talent_philosophies_of_prosperity.png", Sub = true },
                        new IconPickerModel { Title = "「근면」의 철학", IconPath = "Resources/talent_philosophies_of_diligence.png", Sub = true },
                        new IconPickerModel { Title = "「황금」의 철학", IconPath = "Resources/talent_philosophies_of_gold.png", Sub = true },

                        new IconPickerModel { Title = "북풍의 꼬리", IconPath = "Resources/talent_tail_of_boreas.png", Sub = true },
                        new IconPickerModel { Title = "북풍의 고리", IconPath = "Resources/talent_ring_of_boreas.png", Sub = true },
                        new IconPickerModel { Title = "북풍의 영혼상자", IconPath = "Resources/talent_spirit_locket_of_boreas.png", Sub = true },
                        new IconPickerModel { Title = "동풍의 깃털", IconPath = "Resources/talent_dvalins_plume.png", Sub = true },
                        new IconPickerModel { Title = "동풍의 발톱", IconPath = "Resources/talent_dvalins_claw.png", Sub = true },
                        new IconPickerModel { Title = "동풍의 숨결", IconPath = "Resources/talent_dvalins_sigh.png", Sub = true },
                        new IconPickerModel { Title = "하늘을 삼킨 고래·뿔", IconPath = "Resources/talent_tusk_of_monoceros_caeli.png", Sub = true },
                        new IconPickerModel { Title = "마왕의 칼날·조각", IconPath = "Resources/talent_shard_of_a_foul_legacy.png", Sub = true },
                        new IconPickerModel { Title = "무예의 혼·고영", IconPath = "Resources/talent_shadow_of_the_warrior.png", Sub = true },

                        new IconPickerModel { Title = "지식의 왕관", IconPath = "Resources/talent_crown_of_insight.png", Sub = true }
                    }
                };
                #endregion

                #region 재료
                var materialNode = new IconPickerModel
                {
                    Title = "재료",
                    IconPath = "Resources/material_slime_concentrate.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "흑수정 나팔", IconPath = "Resources/material_black_crystal_horn.png", Sub = true },
                        new IconPickerModel { Title = "지맥의 새싹", IconPath = "Resources/material_ley_line_sprouts.png", Sub = true },
                        new IconPickerModel { Title = "석화한 뼛조각", IconPath = "Resources/material_fossilized_bone_shard.png", Sub = true },
                        new IconPickerModel { Title = "안개 등심", IconPath = "Resources/material_mist_grass_wick.png", Sub = true },
                        new IconPickerModel { Title = "검사관의 제도", IconPath = "Resources/material_inspectors_sacrificial_knife.png", Sub = true },
                        new IconPickerModel { Title = "혼돈의 노심", IconPath = "Resources/material_chaos_core.png", Sub = true },

                        new IconPickerModel { Title = "슬라임 원액", IconPath = "Resources/material_slime_concentrate.png", Sub = true },
                        new IconPickerModel { Title = "불길한 가면", IconPath = "Resources/material_ominous_mask.png", Sub = true },
                        new IconPickerModel { Title = "역전의 화살촉", IconPath = "Resources/material_weathered_arrowhead.png", Sub = true },
                        new IconPickerModel { Title = "금주의 두루마리", IconPath = "Resources/material_forbidden_curse_scroll.png", Sub = true },
                        new IconPickerModel { Title = "골드 까마귀 휘장", IconPath = "Resources/material_golden_raven_insignia.png", Sub = true },
                        new IconPickerModel { Title = "위관의 휘장", IconPath = "Resources/material_lieutenants_insignia.png", Sub = true },
                        new IconPickerModel { Title = "원소 꽃꿀", IconPath = "Resources/material_energy_nectar.png", Sub = true },

                        new IconPickerModel { Title = "수정 코어", IconPath = "Resources/material_crystal_core.png", Sub = true },
                    }
                };
                #endregion

                #region 아이템
                var itemNode = new IconPickerModel
                {
                    Title = "아이템",
                    IconPath = "Resources/item_story_key.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "전설의 열쇠", IconPath = "Resources/item_story_key.png", Sub = true },
                        new IconPickerModel { Title = "농축 레진", IconPath = "Resources/item_condensed_resin.png", Sub = true }
                    }
                };
                #endregion

                #region 기원
                var wishNode = new IconPickerModel
                {
                    Title = "기원",
                    IconPath = "Resources/wish_primogem.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "원석", IconPath = "Resources/wish_primogem.png", Sub = true },
                        new IconPickerModel { Title = "뒤얽힌 인연", IconPath = "Resources/wish_intertwined_fate.png", Sub = true },
                        new IconPickerModel { Title = "만남의 인연", IconPath = "Resources/wish_acquaint_fate.png", Sub = true },
                        new IconPickerModel { Title = "공월 축복", IconPath = "Resources/wish_blessing_of_the_welkin_moon.png", Sub = true }
                    }
                };
                #endregion

                #region 기타
                var otherNode = new IconPickerModel
                {
                    Title = "기타",
                    IconPath = "Resources/other_fragile_resin.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "퓨어 레진", IconPath = "Resources/other_fragile_resin.png", Sub = true }
                    }
                };
                #endregion
                #endregion

                #region 노드 추가/적용
                node.Add(characterNode);
                node.Add(npcNode);
                node.Add(bossNode);
                node.Add(enemyNode);
                node.Add(weaponNode);
                node.Add(artifactNode);
                node.Add(questNode);
                node.Add(waypointNode);
                node.Add(eyesNode);
                node.Add(mineralNode);
                node.Add(collectionNode);
                node.Add(foodNode);
                node.Add(experienceNode);
                node.Add(enhancementNode);
                node.Add(ascensionNode);
                node.Add(talentNode);
                node.Add(materialNode);
                node.Add(itemNode);
                node.Add(wishNode);
                node.Add(otherNode);

                IconNode = node;
                #endregion
                #endregion
            }
            catch (Exception) 
            {
                Current.MainPage.DisplayAlert("버그", "아이콘 목록을 초기화할 수 없습니다.", "확인");
            }
        }
        #endregion

        #region 앱이 백그라운드로 전환될 때
        protected override void OnSleep()
        {
            try
            {
                if (SyncMail != "")
                    AutoSync = false;
                TaskTabView.TimerRepeat = false;
                FarmingTabView.TimerRepeat = false;
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
                    AutoSync = true;
                TaskTabView.TimerRepeat = true;
                FarmingTabView.TimerRepeat = true;
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