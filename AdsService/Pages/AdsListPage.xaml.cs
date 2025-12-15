using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace AdsService.Pages
{
    public partial class AdsListPage : Page
    {
        public AdsListPage()
        {
            InitializeComponent();
            Loaded += AdsListPage_Loaded;
        }

        private void AdsListPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadFilters();
                LoadAds();
                UpdateUserInterface();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обновление интерфейса в зависимости от авторизации
        private void UpdateUserInterface()
        {
            if (App.CurrentUser != null)
            {
                // Пользователь авторизован
                UserInfoTextBlock.Text = $"Вы вошли как: {App.CurrentUser.Login}";
                MyAdsButton.Visibility = Visibility.Visible;
                LoginButton.Content = "Выйти";
            }
            else
            {
                // Пользователь не авторизован
                UserInfoTextBlock.Text = "Вы не авторизованы";
                MyAdsButton.Visibility = Visibility.Collapsed;
                LoginButton.Content = "Войти / Зарегистрироваться";
            }
        }

        private void LoadFilters()
        {
            try
            {
                using (var db = new Entities())
                {
                    // Города
                    var cities = db.Cities.OrderBy(c => c.CityName).ToList();
                    CityComboBox.ItemsSource = cities;
                    CityComboBox.DisplayMemberPath = "CityName";
                    CityComboBox.SelectedValuePath = "CityId";

                    // Категории
                    var categories = db.Categories.OrderBy(c => c.CategoryName).ToList();
                    CategoryComboBox.ItemsSource = categories;
                    CategoryComboBox.DisplayMemberPath = "CategoryName";
                    CategoryComboBox.SelectedValuePath = "CategoryId";

                    // Типы
                    var types = db.AdTypes.OrderBy(t => t.TypeName).ToList();
                    TypeComboBox.ItemsSource = types;
                    TypeComboBox.DisplayMemberPath = "TypeName";
                    TypeComboBox.SelectedValuePath = "TypeId";

                    // Статусы
                    var statuses = db.AdStatuses.OrderBy(s => s.StatusName).ToList();
                    StatusComboBox.ItemsSource = statuses;
                    StatusComboBox.DisplayMemberPath = "StatusName";
                    StatusComboBox.SelectedValuePath = "StatusId";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAds()
        {
            try
            {
                using (var db = new Entities())
                {
                    // LINQ запрос с объединением таблиц
                    var query = from ad in db.Ads
                                join city in db.Cities on ad.CityId equals city.CityId
                                join category in db.Categories on ad.CategoryId equals category.CategoryId
                                join adType in db.AdTypes on ad.TypeId equals adType.TypeId
                                join status in db.AdStatuses on ad.StatusId equals status.StatusId
                                select new
                                {
                                    Ad = ad,
                                    CityName = city.CityName,
                                    CategoryName = category.CategoryName,
                                    TypeName = adType.TypeName,
                                    StatusName = status.StatusName
                                };

                    // Поиск по ключевым словам
                    if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
                    {
                        string searchText = SearchTextBox.Text.ToLower();
                        query = query.Where(x =>
                            x.Ad.Title.ToLower().Contains(searchText) ||
                            (x.Ad.Description != null && x.Ad.Description.ToLower().Contains(searchText)));
                    }

                    // Фильтрация по городу
                    if (CityComboBox.SelectedValue != null)
                    {
                        int cityId = (int)CityComboBox.SelectedValue;
                        query = query.Where(x => x.Ad.CityId == cityId);
                    }

                    // Фильтрация по категории
                    if (CategoryComboBox.SelectedValue != null)
                    {
                        int categoryId = (int)CategoryComboBox.SelectedValue;
                        query = query.Where(x => x.Ad.CategoryId == categoryId);
                    }

                    // Фильтрация по типу
                    if (TypeComboBox.SelectedValue != null)
                    {
                        int typeId = (int)TypeComboBox.SelectedValue;
                        query = query.Where(x => x.Ad.TypeId == typeId);
                    }

                    // Фильтрация по статусу
                    if (StatusComboBox.SelectedValue != null)
                    {
                        int statusId = (int)StatusComboBox.SelectedValue;
                        query = query.Where(x => x.Ad.StatusId == statusId);
                    }

                    // Сортировка по дате (новые сначала)
                    var ads = query
                        .OrderByDescending(x => x.Ad.PostDate)
                        .ToList()
                        .Select(x => new
                        {
                            x.Ad.AdId,
                            x.Ad.Title,
                            x.Ad.Description,
                            x.Ad.Price,
                            x.Ad.PostDate,
                            x.Ad.ImagePath,
                            CityName = x.CityName,
                            CategoryName = x.CategoryName,
                            TypeName = x.TypeName,
                            StatusName = x.StatusName
                        })
                        .ToList();

                    AdsItemsControl.ItemsSource = ads;

                    // Информация о количестве найденных объявлений
                    if (ads.Count == 0)
                    {
                        MessageBox.Show("По вашему запросу объявлений не найдено.", "Информация",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки объявлений: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчики событий поиска и фильтрации
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadAds();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAds();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            CityComboBox.SelectedIndex = -1;
            CategoryComboBox.SelectedIndex = -1;
            TypeComboBox.SelectedIndex = -1;
            StatusComboBox.SelectedIndex = -1;
            LoadAds();
        }

        // Обработчики кнопок авторизации
        private void MyAdsButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser != null)
            {
                NavigationService.Navigate(new MyAdsPage());
            }
            else
            {
                MessageBox.Show("Для просмотра своих объявлений необходимо войти в систему.",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService.Navigate(new LoginPage());
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser != null)
            {
                // Выход из системы
                var result = MessageBox.Show("Вы уверены, что хотите выйти из системы?",
                    "Подтверждение выхода", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    App.CurrentUser = null;
                    UpdateUserInterface();
                    MessageBox.Show("Вы успешно вышли из системы.", "Выход",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                // Переход на страницу входа
                NavigationService.Navigate(new LoginPage());
            }
        }

        // Обработчик кнопки "Завершённые"
        private void CompletedAdsButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser != null)
            {
                NavigationService.Navigate(new CompletedAdsPage());
            }
            else
            {
                MessageBox.Show("Для просмотра завершённых объявлений необходимо войти в систему.",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService.Navigate(new LoginPage());
            }
        }


    }
}