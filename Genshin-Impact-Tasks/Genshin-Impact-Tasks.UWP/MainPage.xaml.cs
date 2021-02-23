using Syncfusion.XForms.UWP.TreeView;

namespace Genshin_Impact_Tasks.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            // Syncfusion.SfTreeView 초기화
            SfTreeViewRenderer.Init();

            LoadApplication(new Genshin_Impact_Tasks.App());
        }
    }
}
