using Genshin_Impact_Tasks.Models;

using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Genshin_Impact_Tasks.Popups
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectIconPopup : PopupPage
    {
        public EventHandler<SelectIconResult> OnClosed;

        public SelectIconPopup()
        {
            try
            {
                InitializeComponent();

                if (App.UseDarkMode)
                    MainFrame.BackgroundColor = Color.FromHex("333333");

                IconPickerTreeView.ItemsSource = App.IconNode;
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

                // 하위 노드인 경우 선택 완료 후 팝업 닫기
                if (item.Sub)
                {
                    OnClosed?.Invoke(this, new SelectIconResult { IconPath = item.IconPath });
                    await PopupNavigation.Instance.RemovePageAsync(this);
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
                await PopupNavigation.Instance.RemovePageAsync(this);
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