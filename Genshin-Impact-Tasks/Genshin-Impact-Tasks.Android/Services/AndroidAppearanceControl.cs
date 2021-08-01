using Genshin_Impact_Tasks.Droid.Services;
using Genshin_Impact_Tasks.Services;

using System;

using Android.Views;

using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidAppearanceControl))]
namespace Genshin_Impact_Tasks.Droid.Services
{
    public class AndroidAppearanceControl : IAppearanceControl
    {
        #region 상태 바 색상 설정
        public void SetStatusBarColor(string hexColor)
        {
            try
            {
                MainActivity.Instance.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                MainActivity.Instance.Window.SetStatusBarColor(Android.Graphics.Color.ParseColor(hexColor));
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion
    }
}