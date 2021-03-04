using Genshin_Impact_Tasks.Droid.Services;
using Genshin_Impact_Tasks.Services;
using Genshin_Impact_Tasks.Models;

using System;
using System.IO;
using System.Xml.Serialization;

using Android.App;
using Android.Content;

using AndroidX.Core.App;

using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidNotificationManager))]

namespace Genshin_Impact_Tasks.Droid.Services
{
    public class AndroidNotificationManager : INotificationManager
    {
        private readonly DateTime date1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal string randomNumber;

        #region 알림 전송
        [Obsolete]
        public void Send(string title, string text, int id, DateTime notifyTime, string uwpIconPath = null)
        {
            try
            {
                long totalMs = (long)(notifyTime.ToUniversalTime() - date1970).TotalMilliseconds;
                var context = Android.App.Application.Context;
                var notification = new LocalNotificationModel
                {
                    Title = title,
                    Text = text,
                    Id = id,
                    NotifyTime = notifyTime
                };

                var intent = new Intent(context, typeof(ScheduledAlarmHandler)).SetAction($"GenshinImpactTasks{id}");
                intent.PutExtra(ScheduledAlarmHandler.LocalNotificationKey, SerializeNotification(notification));

                var random = new Random();
                randomNumber = random.Next(100000, 999999).ToString("D6");
                var pendingIntent = PendingIntent.GetBroadcast(context, Convert.ToInt32(randomNumber), intent, PendingIntentFlags.OneShot);
                var alarm = Android.App.Application.Context.GetSystemService(Context.AlarmService) as AlarmManager;
                alarm.SetRepeating(AlarmType.RtcWakeup, totalMs, 0, pendingIntent);
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 알림 취소
        public void Cancel(int id)
        {
            try
            {
                var context = Android.App.Application.Context;
                var intent = new Intent(context, typeof(ScheduledAlarmHandler)).SetAction($"GenshinImpactTasks{id}");
                var pendingIntent = PendingIntent.GetBroadcast(Android.App.Application.Context, Convert.ToInt32(randomNumber), intent, PendingIntentFlags.OneShot);
                var alarm = Android.App.Application.Context.GetSystemService(Context.AlarmService) as AlarmManager;
                alarm.Cancel(pendingIntent);

                var manager = NotificationManagerCompat.From(Android.App.Application.Context);
                manager.CancelAll();
                manager.Cancel(id);
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region Activity 가져오기
        public static Intent GetLauncherActivity()
        {

            var packageName = global::Android.App.Application.Context.PackageName;
            return global::Android.App.Application.Context.PackageManager.GetLaunchIntentForPackage(packageName);
        }
        #endregion

        #region LocalNotificationModel 직렬화
        private string SerializeNotification(LocalNotificationModel notification)
        {

            var xmlSerializer = new XmlSerializer(notification.GetType());

            using (var stringWriter = new StringWriter())
            {
                xmlSerializer.Serialize(stringWriter, notification);
                return stringWriter.ToString();
            }
        }
        #endregion
    }
}