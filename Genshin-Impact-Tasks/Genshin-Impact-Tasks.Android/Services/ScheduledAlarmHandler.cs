using Genshin_Impact_Tasks.Models;

using System;
using System.IO;
using System.Xml.Serialization;

using Android.App;
using Android.Content;
using Android.Media;
using Android.Graphics;

using AndroidX.Core.App;

namespace Genshin_Impact_Tasks.Droid.Services
{
    [BroadcastReceiver(Enabled = true, Label = "Local Notifications Broadcast Receiver")]
    public class ScheduledAlarmHandler : BroadcastReceiver
    {
        public const string LocalNotificationKey = "GenshinImpactTasksLocalNotification";

        [Obsolete]
        public override void OnReceive(Context context, Intent intent)
        {
            var extra = intent.GetStringExtra(LocalNotificationKey);
            var notification = DeserializeNotification(extra);

            if (!App.Notifications.Contains(notification.Id)) return;

            int icon = Resource.Drawable.icon;

            #region Icon
            switch (notification.Id)
            {
                #region 광물
                case 1000:
                    icon = Resource.Drawable.mineral_cor_lapis;
                    break;
                case 1001:
                    icon = Resource.Drawable.mineral_noctilucous_jade;
                    break;
                case 1002:
                    icon = Resource.Drawable.mineral_crystal_chunk;
                    break;
                case 1003:
                    icon = Resource.Drawable.mineral_iron_chunk;
                    break;
                case 1004:
                    icon = Resource.Drawable.mineral_white_iron_chunk;
                    break;
                case 1005:
                    icon = Resource.Drawable.mineral_starsilver;
                    break;
                case 1006:
                    icon = Resource.Drawable.mineral_electro_crystal;
                    break;
                #endregion

                #region 채집물
                case 2000:
                    icon = Resource.Drawable.collection_qingxin;
                    break;
                case 2001:
                    icon = Resource.Drawable.collection_silk_flower;
                    break;
                case 2002:
                    icon = Resource.Drawable.collection_philanemo_mushroom;
                    break;
                case 2003:
                    icon = Resource.Drawable.collection_cecilia;
                    break;
                case 2004:
                    icon = Resource.Drawable.collection_valberry;
                    break;
                case 2005:
                    icon = Resource.Drawable.collection_windwheel_aster;
                    break;
                case 2006:
                    icon = Resource.Drawable.collection_calla_lily;
                    break;
                case 2007:
                    icon = Resource.Drawable.collection_glaze_lily;
                    break;
                case 2008:
                    icon = Resource.Drawable.collection_jueyun_chili;
                    break;
                case 2009:
                    icon = Resource.Drawable.collection_violetgrass;
                    break;
                case 2010:
                    icon = Resource.Drawable.collection_wolfhook;
                    break;
                case 2011:
                    icon = Resource.Drawable.collection_small_lamp_grass;
                    break;
                case 2012:
                    icon = Resource.Drawable.collection_dandelion_seed;
                    break;
                #endregion
            }
            #endregion

            var builder = new NotificationCompat.Builder(Application.Context)
                .SetContentTitle(notification.Title)
                .SetContentText(notification.Text)
                .SetLargeIcon(BitmapFactory.DecodeResource(Xamarin.Forms.Forms.Context.Resources, icon))
                .SetSmallIcon(Resource.Drawable.icon)
                .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Ringtone))
                .SetAutoCancel(true)
                .SetPriority((int)NotificationPriority.High)
                .SetVibrate(new long[0])
                .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate)
                .SetVisibility((int)NotificationVisibility.Public);

            var resultIntent = AndroidNotificationManager.GetLauncherActivity();
            resultIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            var stackBuilder = AndroidX.Core.App.TaskStackBuilder.Create(Application.Context);
            stackBuilder.AddNextIntent(resultIntent);

            var random = new Random();
            var randNum = random.Next(9999 - 1000) + 1000;

            var resultPendingIntent = stackBuilder.GetPendingIntent(randNum, (int)PendingIntentFlags.Immutable);
            builder.SetContentIntent(resultPendingIntent);

            var manager = context.GetSystemService(Context.NotificationService) as NotificationManager;

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                var importance = NotificationImportance.High;

                var channel = new NotificationChannel(notification.Id.ToString(), notification.Title, importance);
                channel.EnableLights(true);
                channel.EnableVibration(true);
                channel.SetShowBadge(true);
                channel.Importance = NotificationImportance.High;
                channel.SetVibrationPattern(new long[] { 100, 500, 100, 300, 200, 100 });

                if (manager != null)
                {
                    builder.SetChannelId(notification.Id.ToString());
                    manager.CreateNotificationChannel(channel);
                }
            }

            manager.Notify(randNum, builder.Build());
        }

        #region LocalNotificationModel 역직렬화
        private LocalNotificationModel DeserializeNotification(string notificationString)
        {

            var xmlSerializer = new XmlSerializer(typeof(LocalNotificationModel));
            using (var stringReader = new StringReader(notificationString))
            {
                var notification = (LocalNotificationModel)xmlSerializer.Deserialize(stringReader);
                return notification;
            }
        }
        #endregion
    }
}