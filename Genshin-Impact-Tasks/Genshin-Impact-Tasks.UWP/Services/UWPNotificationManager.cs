using Genshin_Impact_Tasks.Services;
using Genshin_Impact_Tasks.UWP.Services;

using System;
using System.Linq;

using Microsoft.Toolkit.Uwp.Notifications;

using Windows.UI.Notifications;

using Xamarin.Forms;

[assembly: Dependency(typeof(UWPNotificationManager))]
namespace Genshin_Impact_Tasks.UWP.Services
{
    public class UWPNotificationManager : INotificationManager
    {
        #region 알림 전송
        public void Send(string title, string text, int id, string uwpIconPath = null)
        {
            try
            {
                var content = new ToastContentBuilder()
                    .AddToastActivationInfo("GenshinImpactTasks", ToastActivationType.Background)
                    .AddText(title, AdaptiveTextStyle.Header)
                    .AddText(text, AdaptiveTextStyle.Subtitle)
                    .AddAppLogoOverride(new Uri($"ms-appx:///{uwpIconPath}"), ToastGenericAppLogoCrop.Circle)
                    .GetToastContent();

                var toast = new ToastNotification(content.GetXml());
                toast.Tag = id.ToString();
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            catch (Exception ex)
            {
                Genshin_Impact_Tasks.App.DisplayEx(ex);
            }
        }
        #endregion

        #region 알림 전송 (NotifyTime)
        public void Send(string title, string text, int id, DateTime notifyTime, string uwpIconPath = null)
        {
            try
            {
                var content = new ToastContentBuilder()
                    .AddToastActivationInfo("GenshinImpactTasks", ToastActivationType.Background)
                    .AddText(title, AdaptiveTextStyle.Header)
                    .AddText(text, AdaptiveTextStyle.Subtitle)
                    .AddAppLogoOverride(new Uri($"ms-appx:///{uwpIconPath}"), ToastGenericAppLogoCrop.Circle)
                    .GetToastContent();

                var toast = new ScheduledToastNotification(content.GetXml(), notifyTime);
                toast.Tag = id.ToString();

                ToastNotificationManager.CreateToastNotifier().AddToSchedule(toast);
            }
            catch (Exception ex)
            {
                Genshin_Impact_Tasks.App.DisplayEx(ex);
            }
        }
        #endregion

        #region 알림 취소
        public void Cancel(int id)
        {
            try
            {
                var notifier = ToastNotificationManager.CreateToastNotifier();
                var toasts = notifier.GetScheduledToastNotifications();
                var toRemove = toasts.FirstOrDefault(i => i.Tag == id.ToString());
                
                if (toRemove != null) notifier.RemoveFromSchedule(toRemove);
            }
            catch (Exception ex)
            {
                Genshin_Impact_Tasks.App.DisplayEx(ex);
            }
        }
        #endregion
    }
}
