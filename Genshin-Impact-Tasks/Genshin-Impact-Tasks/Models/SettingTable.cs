using SQLite;

namespace Genshin_Impact_Tasks.Models
{
    [Table("Setting")]
    public class SettingTable
    {
        [PrimaryKey, Column("Key")]
        public string Key { get; set; } // 설정 키

        [Column("Value")]
        public string Value { get; set; } // 설정 값
    }
}
