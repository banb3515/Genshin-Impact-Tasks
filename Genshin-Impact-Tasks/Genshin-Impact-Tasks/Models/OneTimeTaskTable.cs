using SQLite;

namespace Genshin_Impact_Tasks.Models
{
    [Table("OneTimeTask")]
    public class OneTimeTaskTable
    {
        [PrimaryKey, Column("Content")]
        public string Content { get; set; }

        [Column("IconPath")]
        public string IconPath { get; set; }

        [Column("Status")]
        public bool Status { get; set; }
    }
}
