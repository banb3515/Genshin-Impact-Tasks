using SQLite;

namespace Genshin_Impact_Tasks.Models
{
    [Table("DailyTask")]
    public class DailyTaskTable
    {
        [PrimaryKey, Column("No")]
        public int No { get; set; } // 고유 번호

        [Column("Content")]
        public string Content { get; set; } // 할 일 내용

        [Column("IconPath")]
        public string IconPath { get; set; } // 아이콘 경로

        [Column("Status")]
        public bool Status { get; set; } // 완료 상태

        [Column("CreateDate")]
        public string CreateTime { get; set; } // 생성된 날짜
    }
}