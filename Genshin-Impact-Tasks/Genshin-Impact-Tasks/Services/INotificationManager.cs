using System;

namespace Genshin_Impact_Tasks.Services
{
    public interface INotificationManager
    {
        // 알림 전송
        void Send(string title, string text, int id, DateTime notifyTime, string uwpIconPath = null);

        // 알림 취소
        void Cancel(int id);
    }
}
