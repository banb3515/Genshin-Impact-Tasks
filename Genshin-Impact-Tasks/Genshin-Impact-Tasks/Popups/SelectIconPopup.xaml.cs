using Genshin_Impact_Tasks.Models;

using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

using System;
using System.Collections.ObjectModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Genshin_Impact_Tasks.Popups
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectIconPopup : PopupPage
    {
        public bool UseDarkMode { get; set; } = false; // 다크 모드 사용 여부

        public ObservableCollection<IconPickerModel> IconNode { get; set; } // 아이콘 노드

        public EventHandler<SelectIconResult> OnClosed;

        public SelectIconPopup()
        {
            try
            {
                if (App.Current.RequestedTheme == OSAppTheme.Dark) UseDarkMode = true;

                InitializeComponent();

                if (UseDarkMode)
                    MainFrame.BackgroundColor = Color.FromHex("333333");

                // 추후 json으로 변경 예정
                var node = new ObservableCollection<IconPickerModel>();

                #region 노드 목록
                #region 보스
                var bossNode = new IconPickerModel
                {
                    Title = "보스",
                    IconPath = "Resources/boss_dvalin.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "풍마룡 (드발린)", IconPath = "Resources/boss_dvalin.png", Sub = true },
                        new IconPickerModel { Title = "복풍의 왕랑 (안드리우스)", IconPath = "Resources/boss_andrius.png", Sub = true },
                        new IconPickerModel { Title = "황금옥 (타르탈리아)", IconPath = "Resources/boss_tartaglia.png", Sub = true }
                    }
                };
                #endregion

                #region 캐릭터
                #endregion

                #region NPC
                var npcNode = new IconPickerModel
                {
                    Title = "NPC",
                    IconPath = "Resources/npc_hertha.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "헤르타 [페보니우스 기사단] 보급", IconPath = "Resources/npc_hertha.png", Sub = true }
                    }
                };
                #endregion

                #region 몬스터
                var enemyNode = new IconPickerModel
                {
                    Title = "몬스터",
                    IconPath = "Resources/enemy_ruin_hunter.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "유적 헌터", IconPath = "Resources/enemy_ruin_hunter.png", Sub = true }
                    }
                };
                #endregion

                #region 경험치
                var experienceNode = new IconPickerModel
                {
                    Title = "경험치",
                    IconPath = "Resources/experience_adventure.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "모험", IconPath = "Resources/experience_adventure.png", Sub = true },
                        new IconPickerModel { Title = "캐릭터", IconPath = "Resources/experience_character.png", Sub = true },
                        new IconPickerModel { Title = "리월 평판", IconPath = "Resources/experience_liyue_reputation.png", Sub = true },
                        new IconPickerModel { Title = "몬드 평판", IconPath = "Resources/experience_mondstadt_reputation.png", Sub = true },
                        new IconPickerModel { Title = "기행", IconPath = "Resources/experience_battle_pass.png", Sub = true }
                    }
                };
                #endregion

                #region 눈동자
                var eyesNode = new IconPickerModel
                {
                    Title = "눈동자",
                    IconPath = "Resources/eyes_anemoculus.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "바람 신의 눈동자", IconPath = "Resources/eyes_anemoculus.png", Sub = true },
                        new IconPickerModel { Title = "바위 신의 눈동자", IconPath = "Resources/eyes_geoculus.png", Sub = true },
                        new IconPickerModel { Title = "진홍의 옥수", IconPath = "Resources/eyes_crimson_agate.png", Sub = true }
                    }
                };
                #endregion

                #region 임무
                var questNode = new IconPickerModel
                {
                    Title = "임무",
                    IconPath = "Resources/quest_quest.png",
                    SubIcons = new ObservableCollection<IconPickerModel> 
                    { 
                        new IconPickerModel { Title = "임무", IconPath = "Resources/quest_quest.png", Sub = true },
                        new IconPickerModel { Title = "일일 의뢰", IconPath = "Resources/quest_commission.png", Sub = true }
                    }
                };
                #endregion

                #region 웨이포인트
                var waypointNode = new IconPickerModel
                {
                    Title = "웨이포인트",
                    IconPath = "Resources/waypoint_waypoint.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "웨이포인트", IconPath = "Resources/waypoint_waypoint.png", Sub = true },
                        new IconPickerModel { Title = "비경", IconPath = "Resources/waypoint_domains.png", Sub = true },
                        new IconPickerModel { Title = "나선비경", IconPath = "Resources/waypoint_spiral_abyss.png", Sub = true }
                    }
                };
                #endregion

                #region 강화
                var enhancementNode = new IconPickerModel
                {
                    Title = "강화",
                    IconPath = "Resources/enhancement_mystic_enhancement_ore.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "[1★] 정제용 하급 광물", IconPath = "Resources/enhancement_enhancement_ore.png", Sub = true },
                        new IconPickerModel { Title = "[2★] 정제용 광물", IconPath = "Resources/enhancement_fine_enhancement_ore.png", Sub = true },
                        new IconPickerModel { Title = "[3★] 정제용 마법 광물", IconPath = "Resources/enhancement_mystic_enhancement_ore.png", Sub = true },
                        new IconPickerModel { Title = "[2★] 방랑자의 경험", IconPath = "Resources/enhancement_wanderers_advice.png", Sub = true },
                        new IconPickerModel { Title = "[3★] 모험가의 경험", IconPath = "Resources/enhancement_adventurers_experience.png", Sub = true },
                        new IconPickerModel { Title = "[4★] 영웅의 경험", IconPath = "Resources/enhancement_heros_wit.png", Sub = true }
                    }
                };
                #endregion

                #region 아이템
                var itemNode = new IconPickerModel
                {
                    Title = "아이템",
                    IconPath = "Resources/item_story_key.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "전설 임무 열쇠", IconPath = "Resources/item_story_key.png", Sub = true },
                        new IconPickerModel { Title = "농축 레진", IconPath = "Resources/item_condensed_resin.png", Sub = true }
                    }
                };
                #endregion

                #region 기타
                var otherNode = new IconPickerModel
                {
                    Title = "기타",
                    IconPath = "Resources/other_fragile_resin.png",
                    SubIcons = new ObservableCollection<IconPickerModel>
                    {
                        new IconPickerModel { Title = "퓨어 레진", IconPath = "Resources/other_fragile_resin.png", Sub = true }
                    }
                };
                #endregion
                #endregion

                #region 노드 추가/적용
                node.Add(bossNode);
                node.Add(npcNode);
                node.Add(experienceNode);
                node.Add(eyesNode);
                node.Add(questNode);
                node.Add(waypointNode);
                node.Add(enhancementNode);
                node.Add(itemNode);
                node.Add(otherNode);

                IconNode = node;

                IconPickerTreeView.ItemsSource = IconNode;
                #endregion
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }

        #region 아이콘 선택 트리뷰 노드 선택 시
        private async void IconPickerTreeView_SelectionChanged(object sender, Syncfusion.XForms.TreeView.ItemSelectionChangedEventArgs e)
        {
            try
            {
                var item = IconPickerTreeView.CurrentItem as IconPickerModel;

                // 하위 노드인 경우에만 선택 완료 후 팝업 닫기
                if (item.Sub)
                {
                    OnClosed?.Invoke(this, new SelectIconResult { IconPath = item.IconPath });
                    await PopupNavigation.Instance.PopAsync();
                }
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 닫기 버튼 클릭 시
        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                OnClosed?.Invoke(this, new SelectIconResult { IconPath = null });
                await PopupNavigation.Instance.PopAsync();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion
    }

    // 응답 결과
    public class SelectIconResult
    {
        public string IconPath { get; set; } // 아이콘 경로
    }
}