namespace Genshin_Impact_Tasks.Models
{
    public class TaskModel
    {
        public int No { get; set; } // 임시 번호

        public string Content { get; set; } // 내용

        public string IconPath { get; set; } // 아이콘 경로

        public bool Status { get; set; } // 상태
    }
}
