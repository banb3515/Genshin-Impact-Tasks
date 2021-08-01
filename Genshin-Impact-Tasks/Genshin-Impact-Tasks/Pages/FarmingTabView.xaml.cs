using Genshin_Impact_Tasks.Models;
using Genshin_Impact_Tasks.Popups;
using Genshin_Impact_Tasks.Services;

using Rg.Plugins.Popup.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Genshin_Impact_Tasks.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FarmingTabView : ContentView
    {
        private string CurrentFarmingList { get; set; } = "Mineral"; // 현재 파밍 목록

        public static bool TimerRepeat { get; set; } = true;

        private List<FarmingModel> Minerals { get; set; }
        private List<FarmingModel> Collections { get; set; }

        public FarmingTabView()
        {
            try
            {
                InitializeComponent();

                Init();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }

        #region 초기화
        private void Init()
        {
            try
            {
                // 변수 초기화
                Minerals = new List<FarmingModel>();
                Collections = new List<FarmingModel>();

                #region 데이터 가져오기
                App.Database.Table<MineralFarmingTable>().ToList().ForEach(farming =>
                {
                    string respawn = null;
                    if (!string.IsNullOrWhiteSpace(farming.CompleteTime))
                        respawn = Convert.ToDateTime(farming.CompleteTime).AddHours(farming.RespawnHour).ToString("M월 d일 tt h시 m분 s초", new CultureInfo("ko-KR"));
                    Minerals.Add(new FarmingModel { Id = farming.Id, Name = farming.Name, IconPath = farming.IconPath, RespawnHour = farming.RespawnHour, Status = farming.Status, CompleteTime = farming.CompleteTime, RespawnTime = respawn });
                });
                App.Database.Table<CollectionFarmingTable>().ToList().ForEach(farming =>
                {
                    string respawn = null;
                    if (!string.IsNullOrWhiteSpace(farming.CompleteTime))
                        respawn = Convert.ToDateTime(farming.CompleteTime).AddHours(farming.RespawnHour).ToString("M월 d일 tt h시 m분 s초", new CultureInfo("ko-KR"));
                    Collections.Add(new FarmingModel { Id = farming.Id, Name = farming.Name, IconPath = farming.IconPath, RespawnHour = farming.RespawnHour, Status = farming.Status, CompleteTime = farming.CompleteTime, RespawnTime = respawn }); 
                });

                FarmingView.ItemsSource = Minerals;
                #endregion

                RespawnCheck();

                #region 알림 초기화
                foreach (var item in Minerals)
                {
                    if (!item.Status) continue;

                    DependencyService.Get<INotificationManager>().Cancel(item.Id);
                    App.Notifications.Add(item.Id);
                    DependencyService.Get<INotificationManager>().Send("광물 리젠 알림", $"[{item.Name}] 광물이 리젠되었습니다.", item.Id, DateTime.Now.AddHours(item.RespawnHour), item.IconPath);
                }
                #endregion

                Device.StartTimer(TimeSpan.FromSeconds(1), OnRespawnCheck);

                if (App.UseDarkMode)
                    Divider.Color = Color.White;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 리젠 확인 타이머
        private bool OnRespawnCheck()
        {
            try
            {
                if (!TimerRepeat) return true;

                RespawnCheck();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }

            return true;
        }
        #endregion

        #region 리젠 확인
        private void RespawnCheck()
        {
            try
            {
                bool flag = false;

                for (int idx = 0; idx < Minerals.Count; idx ++)
                {
                    if (Minerals[idx].CompleteTime != null && DateTime.Compare(DateTime.Now, Convert.ToDateTime(Minerals[idx].CompleteTime).AddHours(Minerals[idx].RespawnHour)) > 0)
                    {
                        var mDb = App.Database.Table<MineralFarmingTable>().ToList().Where(f => f.Name == Minerals[idx].Name).FirstOrDefault();
                        mDb.Status = false;
                        mDb.CompleteTime = null;
                        App.Database.Update(mDb);

                        Minerals[idx].Status = false;
                        Minerals[idx].CompleteTime = null;
                        Minerals[idx].RespawnTime = null;

                        flag = true;
                    }
                }

                for (int idx = 0; idx < Collections.Count; idx++)
                {
                    if (Collections[idx].CompleteTime != null && DateTime.Compare(DateTime.Now, Convert.ToDateTime(Collections[idx].CompleteTime).AddHours(Collections[idx].RespawnHour)) > 0)
                    {
                        var cDb = App.Database.Table<CollectionFarmingTable>().ToList().Where(f => f.Name == Collections[idx].Name).FirstOrDefault();
                        cDb.Status = false;
                        cDb.CompleteTime = null;
                        App.Database.Update(cDb);

                        Collections[idx].Status = false;
                        Collections[idx].CompleteTime = null;
                        Collections[idx].RespawnTime = null;

                        flag = true;
                    }
                }

                if (flag) FarmingListRefresh();
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 파밍 목록 새로고침
        public void FarmingListRefresh()
        {
            try
            {
                FarmingView.ItemsSource = null;

                switch (CurrentFarmingList)
                {
                    case "Mineral":
                        FarmingView.ItemsSource = Minerals;
                        break;
                    case "Collection":
                        FarmingView.ItemsSource = Collections;
                        break;
                }
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 파밍 목록 이동
        private async void NavigateFarmingList_Clicked(object sender, EventArgs e)
        {
            try
            {
                var farming = (sender as Button).BindingContext as string;

                if (CurrentFarmingList == farming) return;

                CurrentFarmingList = farming;

                EnableFarmingButton(false);

                _ = Title.FadeTo(0, 500, Easing.SpringOut);

                await FarmingView.FadeTo(0, 300, Easing.SpringOut);
                FarmingView.IsVisible = false;

                switch (farming)
                {
                    case "Mineral":
                        Title.Text = "광물";
                        _ = Title.FadeTo(1, 150, Easing.CubicIn);
                        break;
                    case "Collection":
                        Title.Text = "채집물";
                        _ = Title.FadeTo(1, 150, Easing.CubicIn);
                        break;
                }

                FarmingListRefresh();

                FarmingView.IsVisible = true;
                await FarmingView.FadeTo(1, 150, Easing.CubicIn);

                EnableFarmingButton(true);
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 파밍 목록 이동 버튼 활성화
        private void EnableFarmingButton(bool enable = true)
        {
            try
            {
                MineralButton.IsEnabled = enable;
                CollectionButton.IsEnabled = enable;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion

        #region 파밍 아이템 탭 시
        private async void FarmingView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                var loading = new LoadingPopup("");
                await PopupNavigation.Instance.PushAsync(loading);

                var item = e.Item as FarmingModel;
                int idx = e.ItemIndex;

                if (item.Status)
                {
                    var answer = await App.Current.MainPage.DisplayAlert("파밍", $"[{item.Name}] 리젠 타이머를 취소합니다.", "확인", "취소");

                    if (!answer)
                    {
                        await PopupNavigation.Instance.RemovePageAsync(loading);
                        return;
                    }
                }

                var date = DateTime.Now;

                switch (CurrentFarmingList)
                {
                    case "Mineral":
                        var mineralDb = App.Database.Table<MineralFarmingTable>().ToList().Where(f => f.Name == item.Name).FirstOrDefault();
                        mineralDb.Status = !mineralDb.Status;
                        if (item.Status) mineralDb.CompleteTime = null;
                        else mineralDb.CompleteTime = date.ToString();
                        App.Database.Update(mineralDb);

                        if (item.Status)
                        {
                            if (App.UseNotification)
                            {
                                if (App.Notifications.Contains(item.Id))
                                    App.Notifications.Remove(item.Id);
                                DependencyService.Get<INotificationManager>().Cancel(item.Id);
                            }

                            Minerals[idx].CompleteTime = null;
                            Minerals[idx].RespawnTime = null;
                        }
                        else
                        {
                            if (App.UseNotification)
                            {
                                if (!App.Notifications.Contains(item.Id))
                                    App.Notifications.Add(item.Id);
                                DependencyService.Get<INotificationManager>().Send("광물 리젠 알림", $"[{item.Name}] 광물이 리젠되었습니다.", item.Id, DateTime.Now.AddHours(item.RespawnHour), item.IconPath);
                            }

                            Minerals[idx].CompleteTime = date.ToString();
                            Minerals[idx].RespawnTime = date.AddHours(item.RespawnHour).ToString("M월 d일 tt h시 m분 s초", new CultureInfo("ko-KR"));
                        }

                        Minerals[idx].Status = !Minerals[idx].Status;
                        break;
                    case "Collection":
                        var collectionDb = App.Database.Table<CollectionFarmingTable>().ToList().Where(f => f.Name == item.Name).FirstOrDefault();
                        collectionDb.Status = !collectionDb.Status;
                        if (item.Status) collectionDb.CompleteTime = null;
                        else collectionDb.CompleteTime = date.ToString();
                        App.Database.Update(collectionDb);

                        if (item.Status)
                        {
                            if (App.UseNotification)
                            {
                                if (App.Notifications.Contains(item.Id))
                                    App.Notifications.Remove(item.Id);
                                DependencyService.Get<INotificationManager>().Cancel(item.Id);
                            }

                            Collections[idx].CompleteTime = null;
                            Collections[idx].RespawnTime = null;
                        }
                        else
                        {
                            if (App.UseNotification)
                            {
                                if (!App.Notifications.Contains(item.Id))
                                    App.Notifications.Add(item.Id);
                                DependencyService.Get<INotificationManager>().Send("채집물 리젠 알림", $"[{item.Name}] 채집물이 리젠되었습니다.", item.Id, DateTime.Now.AddHours(item.RespawnHour), item.IconPath);
                            }

                            Collections[idx].CompleteTime = date.ToString();
                            Collections[idx].RespawnTime = date.AddHours(item.RespawnHour).ToString("M월 d일 tt h시 m분 s초", new CultureInfo("ko-KR"));
                        }

                        Collections[idx].Status = !Collections[idx].Status;
                        break;
                }

                FarmingListRefresh();

                // 안드로이드, 진동 0.05초 동안 울림
                if (App.UseVibration && Device.RuntimePlatform == Device.Android)
                    Vibration.Vibrate(TimeSpan.FromMilliseconds(50));

                await PopupNavigation.Instance.RemovePageAsync(loading);
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
        #endregion
    }
}