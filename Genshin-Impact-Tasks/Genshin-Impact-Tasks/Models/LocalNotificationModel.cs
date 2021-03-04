using System;

namespace Genshin_Impact_Tasks.Models
{
    public class LocalNotificationModel
    {

        public string Title { get; set; }

        public string Text { get; set; }

        public int Id { get; set; }

        public DateTime NotifyTime { get; set; }
    }
}
