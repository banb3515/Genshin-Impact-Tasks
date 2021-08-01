using Genshin_Impact_Tasks.Models;

using Plugin.LocalNotification;

using SQLite;

using System.IO;
using System.Linq;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Content.Res;
using Android.Util;

namespace Genshin_Impact_Tasks.Droid
{
    [Activity(Label = "원신 태스크", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity Instance { get; set; }

        [System.Obsolete]
        protected override void OnCreate(Bundle savedInstanceState)
        {
            InitFontScale();

            base.OnCreate(savedInstanceState);

            #region 테마 초기화
            using (var database = new SQLiteConnection(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "git.db"),
                SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.ProtectionComplete))
            {
                database.CreateTable<SettingTable>();

                if (database.Table<SettingTable>().ToList().Where(s => s.Key == "Theme").Count() == 0)
                {
                    var theme = new SettingTable { Key = "Theme", Value = "System" };
                    database.Insert(theme);
                }

                switch (database.Table<SettingTable>().ToList().Where(s => s.Key == "Theme").FirstOrDefault().Value)
                {
                    case "Light":
                        SetTheme(Resource.Style.Base_Theme_AppCompat_Light);
                        break;
                    case "Dark":
                        SetTheme(Resource.Style.Base_Theme_AppCompat);
                        break;
                }
            }
            #endregion

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            Rg.Plugins.Popup.Popup.Init(this);
            NotificationCenter.CreateNotificationChannel();

            LoadApplication(new App());

            Instance = this;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override void OnBackPressed()
        {
            Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed);
        }

        #region 폰트 크기 초기화
        [System.Obsolete]
        private void InitFontScale()
        {
            Configuration configuration = Resources.Configuration;
            configuration.FontScale = (float)1.0;
            //0.85 small, 1 standard, 1.15 big，1.3 more bigger ，1.45 supper big 
            DisplayMetrics metrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetMetrics(metrics);
            metrics.ScaledDensity = configuration.FontScale * metrics.Density;
            BaseContext.Resources.UpdateConfiguration(configuration, metrics);
        }
        #endregion
    }
}