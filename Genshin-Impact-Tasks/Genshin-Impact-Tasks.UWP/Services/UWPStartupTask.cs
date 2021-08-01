using Genshin_Impact_Tasks.Services;
using Genshin_Impact_Tasks.UWP.Services;

using System;
using System.Threading.Tasks;

using Windows.ApplicationModel;

using Xamarin.Forms;

[assembly: Dependency(typeof(UWPStartupTask))]
namespace Genshin_Impact_Tasks.UWP.Services
{
    public class UWPStartupTask : IStartupTask
    {
        #region 자동 시작 활성화
        public async Task<bool> Enable()
        {
            try
            {
                var startupTask = await StartupTask.GetAsync("StartupGenshinImpactTasks");

                switch (startupTask.State)
                {
                    case StartupTaskState.Disabled:
                        var newState = await startupTask.RequestEnableAsync();
                        return newState == StartupTaskState.Enabled;
                    case StartupTaskState.DisabledByUser:
                        await Application.Current.MainPage.DisplayAlert("자동 실행",
                            "사용자가 비활성화를 하여 작업 관리자에서 직접 사용으로 변경해야합니다.\n" +
                            "- 작업 관리자 ->\n" +
                            "- 시작 프로그램 ->\n" +
                            "- 원신 태스크 상태 사용으로 변경\n" +
                            "(변경 후 앱을 재시작해주시기 바랍니다.)", "확인");
                        break;
                    case StartupTaskState.DisabledByPolicy:
                        await Application.Current.MainPage.DisplayAlert("자동 실행", "그룹 정책에 의해 자동 실행을 사용할 수 없거나 이 장치에서 지원되지 않는 기능입니다.", "확인");
                        break;
                }
                return false;
            }
            catch (Exception ex)
            {
                Genshin_Impact_Tasks.App.DisplayEx(ex);
                return false;
            }
        }
        #endregion

        #region 자동 시작 비활성화
        public async Task Disable()
        {
            try
            {
                var startupTask = await StartupTask.GetAsync("StartupGenshinImpactTasks");
                startupTask.Disable();
            }
            catch (Exception ex)
            {
                Genshin_Impact_Tasks.App.DisplayEx(ex);
            }
        }
        #endregion

        #region 자동 시작 상태 가져오기
        public async Task<bool> GetState()
        {
            try
            {
                var startupTask = await StartupTask.GetAsync("StartupGenshinImpactTasks");

                if (!((startupTask.State == StartupTaskState.Enabled) || (startupTask.State == StartupTaskState.EnabledByPolicy))) return false;
                else return true;
            }
            catch (Exception ex)
            {
                Genshin_Impact_Tasks.App.DisplayEx(ex);
                return false;
            }
        }
        #endregion
    }
}
