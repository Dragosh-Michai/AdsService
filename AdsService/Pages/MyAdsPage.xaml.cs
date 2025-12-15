using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AdsService.Pages
{
    public partial class MyAdsPage : Page
    {
        public MyAdsPage()
        {
            InitializeComponent();
            Loaded += MyAdsPage_Loaded;
        }

        private void MyAdsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null)
            {
                MessageBox.Show("Для просмотра своих объявлений необходимо войти в систему.",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService.Navigate(new LoginPage());
                return;
            }

            LoadMyAds();
        }

        private void LoadMyAds()
        {
            try
            {
                using (var db = new Entities())
                {
                    var ads = (from ad in db.Ads
                               join status in db.AdStatuses on ad.StatusId equals status.StatusId
                               where ad.UserId == App.CurrentUser.UserId
                               orderby ad.PostDate descending
                               select new
                               {
                                   ad.AdId,
                                   ad.Title,
                                   ad.Price,
                                   ad.PostDate,
                                   StatusName = status.StatusName
                               }).ToList();

                    GridAds.ItemsSource = ads;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Кнопка "Добавить объявление"
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditAdPage(null, LoadMyAds));
        }

        // Кнопка "Завершённые"
        private void CompletedAdsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CompletedAdsPage());
        }

        // Кнопка "На главную"
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdsListPage());
        }

        // Кнопка "Редактировать"
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null && int.TryParse(button.Tag.ToString(), out int adId))
            {
                NavigationService.Navigate(new EditAdPage(adId, LoadMyAds));
            }
        }

        // Кнопка "Удалить"
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null && int.TryParse(button.Tag.ToString(), out int adId))
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить это объявление?\n\nЭто действие нельзя отменить.",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    DeleteAd(adId);
                }
            }
        }

        private void DeleteAd(int adId)
        {
            try
            {
                using (var db = new Entities())
                {
                    var ad = db.Ads.FirstOrDefault(a => a.AdId == adId && a.UserId == App.CurrentUser.UserId);

                    if (ad != null)
                    {
                        db.Ads.Remove(ad);
                        db.SaveChanges();

                        MessageBox.Show("Объявление успешно удалено!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadMyAds(); // Обновляем список
                    }
                    else
                    {
                        MessageBox.Show("Объявление не найдено!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Выбор строки в таблице
        private void GridAds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Можно добавить логику при выборе строки
        }
    }
}