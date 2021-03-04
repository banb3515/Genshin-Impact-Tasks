using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

using System;

using Xamarin.Forms.Xaml;

namespace Genshin_Impact_Tasks.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingPopup : PopupPage
    {
        public LoadingPopup(string header)
        {
            try
            {
                InitializeComponent();

                if (string.IsNullOrWhiteSpace(header)) Header.IsVisible = false;
                else Header.Text = header;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
    }
}