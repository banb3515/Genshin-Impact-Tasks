using Genshin_Impact_Tasks.Droid.Services;
using Genshin_Impact_Tasks.Services;

using Plugin.LocalNotification;

using System;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidNotificationManager))]
namespace Genshin_Impact_Tasks.Droid.Services
{
    public class AndroidNotificationManager : INotificationManager
    {
        #region 알림 전송
        [Obsolete]
        public void Send(string title, string text, int id, string iconPath)
        {
            try
            {
                var notification = new NotificationRequest
                {
                    Title = title,
                    Description = text,
                    NotificationId = id,
                    Android = new AndroidOptions
                    {
                        IconName = iconPath.Split('/')[1],
                        VibrationPattern = new long[] { 100, 500, 400, 100, 200 }
                    }
                };
                NotificationCenter.Current.Show(notification);
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 알림 전송 (NotifyTime)
        public void Send(string title, string text, int id, DateTime notifyTime, string iconPath)
        {
            try
            {
                var notification = new NotificationRequest
                {
                    Title = title,
                    Description = text,
                    NotificationId = id,
                    NotifyTime = notifyTime,
                    Android = new AndroidOptions
                    {
                        IconName = iconPath.Split('/')[1],
                        VibrationPattern = new long[] { 100, 500, 400, 100, 200 }
                    }
                };
                NotificationCenter.Current.Show(notification);
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
                NotificationCenter.Current.Cancel(id);
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 아이콘 가져오기
        private int GetIcon(int id)
        {
            switch (id)
            {
                #region 광물
                case 1000:
                    return Resource.Drawable.mineral_cor_lapis;
                case 1001:
                    return Resource.Drawable.mineral_noctilucous_jade;
                case 1002:
                    return Resource.Drawable.mineral_crystal_chunk;
                case 1003:
                    return Resource.Drawable.mineral_iron_chunk;
                case 1004:
                    return Resource.Drawable.mineral_white_iron_chunk;
                case 1005:
                    return Resource.Drawable.mineral_starsilver;
                case 1006:
                    return Resource.Drawable.mineral_electro_crystal;
                #endregion

                #region 채집물
                case 2000:
                    return Resource.Drawable.collection_qingxin;
                case 2001:
                    return Resource.Drawable.collection_silk_flower;
                case 2002:
                    return Resource.Drawable.collection_philanemo_mushroom;
                case 2003:
                    return Resource.Drawable.collection_cecilia;
                case 2004:
                    return Resource.Drawable.collection_valberry;
                case 2005:
                    return Resource.Drawable.collection_windwheel_aster;
                case 2006:
                    return Resource.Drawable.collection_calla_lily;
                case 2007:
                    return Resource.Drawable.collection_glaze_lily;
                case 2008:
                    return Resource.Drawable.collection_jueyun_chili;
                case 2009:
                    return Resource.Drawable.collection_violetgrass;
                case 2010:
                    return Resource.Drawable.collection_wolfhook;
                case 2011:
                    return Resource.Drawable.collection_small_lamp_grass;
                case 2012:
                    return Resource.Drawable.collection_dandelion_seed;
                case 2013:
                    return Resource.Drawable.npc_chloris;
                #endregion

                default:
                    return Resource.Drawable.icon;
            }
        }
        #endregion
    }
}