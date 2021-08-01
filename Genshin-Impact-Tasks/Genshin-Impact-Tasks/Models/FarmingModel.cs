namespace Genshin_Impact_Tasks.Models
{
    public class FarmingModel
    {
        public int Id { get; set; } // 식별 ID

        public string Name { get; set; } // 광물/채집물 이름

        public string IconPath { get; set; } // 아이콘 경로

        public bool Status { get; set; } // 채광/채집 상태

        public string CompleteTime { get; set; } // 완료 시간

        public string RespawnTime { get; set; } // 리젠 완료 시간

        public int RespawnHour { get; set; } // 리스폰까지 걸리는 시간
    }
}
