using System.Collections.ObjectModel;

namespace Genshin_Impact_Tasks.Models
{
    public class IconPickerModel
    {
        public bool Sub { get; set; } = false; // 하위 타입인지 확인, 기본 값: false

        public string Title { get; set; } // 제목

        public string IconPath { get; set; } // 아이콘 경로

        public ObservableCollection<IconPickerModel> SubIcons { get; set; } // 하위 아이콘 목록
    }
}
