using SQLite;

namespace Genshin_Impact_Tasks.Models
{
    [Table("Setting")]
    public class SettingTable
    {
        [PrimaryKey, AutoIncrement, Column("No")]
        public int No { get; set; } // 고유 번호

        [Unique, Column("Key")]
        public string Key { get; set; } // 설정 키

        [Column("Value")]
        public string Value { get; set; } // 설정 값
    }
}
