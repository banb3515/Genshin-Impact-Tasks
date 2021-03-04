using SQLite;

namespace Genshin_Impact_Tasks.Models
{
    [Table("MineralFarming")]
    public class MineralFarmingTable
    {
        // 광물 식별 ID - 1000 ~ 1999
        [PrimaryKey, Column("Id")]
        public int Id { get; set; } // 식별 ID

        [Unique, Column("Name")]
        public string Name { get; set; } // 광물 이름

        [Column("IconPath")]
        public string IconPath { get; set; } // 아이콘 경로

        [Column("Status")]
        public bool Status { get; set; } // 완료 상태

        [Column("CompleteTime")]
        public string CompleteTime { get; set; } // 완료 시간

        [Column("RespawnHour")]
        public int RespawnHour { get; set; } // 리스폰까지 걸리는 시간
    }
}
