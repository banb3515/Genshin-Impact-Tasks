using SQLite;

namespace Genshin_Impact_Tasks.Models
{
    [Table("DailyTask")]
    public class DailyTaskTable
    {
        [PrimaryKey, Column("Content")]
        public string Content { get; set; }

        [Column("IconPath")]
        public string IconPath { get; set; }

        [Column("Status")]
        public bool Status { get; set; }
    }
}
