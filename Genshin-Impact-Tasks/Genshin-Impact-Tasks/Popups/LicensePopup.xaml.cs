using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Genshin_Impact_Tasks.Popups
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LicensePopup : PopupPage
    {
        // 라이선스 목록
        Dictionary<string, string> Licenses = new Dictionary<string, string> 
        {
            { "Rg.Plugins.Popup", "The MIT License\n\nPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\n\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\n\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE." },
            { "sqlite-net-pcl", "Copyright (c) Krueger Systems, Inc.\n\nAll rights reserved.\n\nPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\n\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\n\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE." },
            { "Syncfusion.Xamarin.SfTreeView", "ESSENTIAL STUDIO SOFTWARE LICENSE AGREEMENT\n\nLicense document version 18.4 Page 1 of 52 CONFIDENTIAL Syncfusion, Inc., 2501 Aerial Center Parkway, Suite 200, Morrisville, North Carolina 27560 This Software License Agreement (the “Agreement”) is a legal agreement between you (“You”, “Your”, or “Customer”) and Syncfusion, Inc., a Delaware corporation with its principal place of business located at 2501 Aerial Center Parkway, Suite 200, Morrisville, NC 27560 (“Syncfusion”).\nThis license is for Essential Studio Enterprise Edition, Essential Studio WPF Edition, Essential Studio PDF Edition, Essential Studio Xamarin Edition, and Essential Studio Win Forms Edition. Syncfusion licenses its products on a per-copy basis (referred to below as Retail Licenses) or under a project license, a corporate division license, or an enterprise license. Your right to use any given copy of a Syncfusion Essential Studio software product is generally set forth in this Agreement. In the event that your copy of this software product is licensed under a project license, a division license, or global license, additional terms and conditions shall also apply which will be set forth in a separate written and signed agreement.\nCarefully read all of the terms and conditions of this Agreement prior to downloading or installing or using the Licensed Product (as that term is defined below). This Agreement between you and Syncfusion sets forth the terms and conditions of your use of the Licensed Product. For the purposes of this Agreement, the effective date of this Agreement shall be the date upon which you click the “YES” button below.\nBY CLICKING THE “YES” BUTTON, YOU ARE ACCEPTING ALL OF THE TERMS OF THIS AGREEMENT AND AGREE TO BE BOUND BY THE TERMS OF THIS AGREEMENT. THIS AGREEMENT CONSTITUTES A BINDING CONTRACT. IF YOU DO NOT AGREE TO ALL OF THE TERMS OF THIS AGREEMENT, CLICK THE “NO” BUTTON AND DO NOT DOWNLOAD AND/OR INSTALL OR OTHERWISE USE THE LICENSED PRODUCT.\nIF AFTER READING THIS AGREEMENT YOU HAVE ANY QUESTIONS ABOUT THIS AGREEMENT, PLEASE CONTACT SYNCFUSION PRIOR TO USING THE SOFTWARE PRODUCT VIA EMAIL AT SALES@SYNCFUSION.COM OR BY TELEPHONE AT (888)-9DOTNET [888-936-8638]." },
            { "Xamarin.Essentials", "Xamarin.Essentials\n\nThe MIT License (MIT)\n\nCopyright (c) Microsoft Corporation All rights reserved.\n\nPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\n\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\n\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE." },
            { "FirebaseDatabase.net", "The MIT License (MIT)\n\nCopyright (c) 2016 Step Up Labs\n\nPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\n\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\n\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE." },
            { "Xam.Plugin.LatestVersion", "MIT License\n\nCopyright (c) 2017 Ed Snider\n\nPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\n\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\n\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE." },
            { "Plugin.LocalNotification", "MIT License\n\nCopyright (c) 2018 Elvin (Tharindu)\n\nPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\n\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\n\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE." }
        };

        public LicensePopup()
        {
            try
            {
                InitializeComponent();

                if (App.UseDarkMode)
                    MainFrame.BackgroundColor = Color.FromHex("333333");

                if (Device.RuntimePlatform == Device.UWP)
                    MainGrid.HeightRequest = 60;

                foreach (var license in Licenses.Keys)
                    LicensePicker.Items.Add(license);

                LicensePicker.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }

        #region 닫기 버튼 클릭 시
        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await PopupNavigation.Instance.RemovePageAsync(this);
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 라이선스 보기 변경 시
        private void LicensePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                LicenseText.Text = Licenses[LicensePicker.Items[LicensePicker.SelectedIndex]];
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion
    }
}