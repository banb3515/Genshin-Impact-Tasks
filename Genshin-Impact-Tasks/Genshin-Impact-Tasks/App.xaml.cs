using System;

using Xamarin.Forms;

namespace Genshin_Impact_Tasks
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        #region Global Function
        #region 예외(오류) 표시 함수
        public static void DisplayEx(Exception ex)
        {
            Current.MainPage.DisplayAlert("Exception", $"오류가 발생하였습니다.\n- {ex.Message}", "확인");
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
