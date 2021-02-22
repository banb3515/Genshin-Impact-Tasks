namespace Genshin_Impact_Tasks.Models
{
    public class TaskModel
    {
        public int No { get; set; } // 임시 번호

        public string ImagePath { get; set; } // 이미지 경로

        public string Content { get; set; } // 내용

        public bool Status { get; set; } // 상태
    }
}
