using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace AdsService.Pages
{
    public partial class EditAdPage : Page
    {
        private int? _adId; // null для добавления, число для редактирования
        private Action _refreshCallback;

        public EditAdPage(int? adId, Action refreshCallback = null)
        {
            _adId = adId;
            _refreshCallback = refreshCallback;
            InitializeComponent();
            Loaded += EditAdPage_Loaded;
        }

        private void EditAdPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Проверка авторизации для добавления/редактирования
            if (!App.IsUserLoggedIn)
            {
                MessageBox.Show("Для добавления или редактирования объявлений необходимо войти в систему.",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService.Navigate(new LoginPage());
                return;
            }

            LoadData();
            UpdateUI();
        }

        // Загрузка данных для формы
        private void LoadData()
        {
            try
            {
                using (var db = new Entities())
                {
                    // Загрузка справочников
                    CityComboBox.ItemsSource = db.Cities.OrderBy(c => c.CityName).ToList();
                    CategoryComboBox.ItemsSource = db.Categories.OrderBy(c => c.CategoryName).ToList();
                    TypeComboBox.ItemsSource = db.AdTypes.OrderBy(t => t.TypeName).ToList();
                    StatusComboBox.ItemsSource = db.AdStatuses.OrderBy(s => s.StatusName).ToList();

                    // Если редактирование существующего объявления
                    if (_adId.HasValue)
                    {
                        var ad = db.Ads.FirstOrDefault(a =>
                            a.AdId == _adId.Value && a.UserId == App.CurrentUser.UserId);

                        if (ad != null)
                        {
                            // Заполнение полей
                            TitleTextBox.Text = ad.Title;
                            DescriptionTextBox.Text = ad.Description;
                            PriceTextBox.Text = ad.Price.ToString();
                            CityComboBox.SelectedValue = ad.CityId;
                            CategoryComboBox.SelectedValue = ad.CategoryId;
                            TypeComboBox.SelectedValue = ad.TypeId;
                            StatusComboBox.SelectedValue = ad.StatusId;

                            // Если есть прибыль
                            if (ad.ProfitAmount.HasValue)
                            {
                                ProfitTextBox.Text = ad.ProfitAmount.Value.ToString();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Объявление не найдено или не принадлежит вам.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            NavigationService.GoBack();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обновление интерфейса
        private void UpdateUI()
        {
            PageTitle.Text = _adId.HasValue ? "Редактирование объявления" : "Добавление объявления";
            SaveButton.Content = _adId.HasValue ? "Сохранить изменения" : "Добавить объявление";
        }

        // Обработчик изменения статуса
        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatusComboBox.SelectedItem != null)
            {
                var status = (AdStatuses)StatusComboBox.SelectedItem;
                bool isCompleted = status.StatusName == "Завершено";

                // Показываем поле для прибыли только для завершённых объявлений
                ProfitPanel.Visibility = isCompleted ? Visibility.Visible : Visibility.Collapsed;

                if (isCompleted)
                {
                    ProfitTextBox.Focus();
                }
            }
        }

        // Валидация числовых полей
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        // Кнопка "Сохранить"
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация данных
            if (!ValidateData())
            {
                return;
            }

            try
            {
                using (var db = new Entities())
                {
                    Ads ad;

                    if (_adId.HasValue)
                    {
                        // Редактирование существующего объявления
                        ad = db.Ads.FirstOrDefault(a =>
                            a.AdId == _adId.Value && a.UserId == App.CurrentUser.UserId);

                        if (ad == null)
                        {
                            MessageBox.Show("Объявление не найдено.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else
                    {
                        // Создание нового объявления
                        ad = new Ads
                        {
                            UserId = App.CurrentUser.UserId,
                            PostDate = DateTime.Now
                        };
                        db.Ads.Add(ad);
                    }

                    // Заполнение полей
                    ad.Title = TitleTextBox.Text.Trim();
                    ad.Description = DescriptionTextBox.Text.Trim();

                    if (decimal.TryParse(PriceTextBox.Text, out decimal price))
                    {
                        ad.Price = price;
                    }

                    ad.CityId = (int)CityComboBox.SelectedValue;
                    ad.CategoryId = (int)CategoryComboBox.SelectedValue;
                    ad.TypeId = (int)TypeComboBox.SelectedValue;
                    ad.StatusId = (int)StatusComboBox.SelectedValue;

                    // Обработка прибыли для завершённых объявлений
                    var status = (AdStatuses)StatusComboBox.SelectedItem;
                    if (status.StatusName == "Завершено")
                    {
                        if (decimal.TryParse(ProfitTextBox.Text, out decimal profit) && profit >= 0)
                        {
                            ad.ProfitAmount = profit;
                        }
                        else
                        {
                            MessageBox.Show("Введите корректную сумму прибыли (неотрицательное число).",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else
                    {
                        ad.ProfitAmount = null;
                    }

                    db.SaveChanges();

                    // Уведомление об успехе
                    string message = _adId.HasValue
                        ? "Объявление успешно обновлено."
                        : "Объявление успешно добавлено.";

                    MessageBox.Show(message, "Успешно",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Вызов callback для обновления родительской страницы
                    _refreshCallback?.Invoke();

                    // Возврат назад
                    NavigationService.Navigate(new MyAdsPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Валидация данных формы
        private bool ValidateData()
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите название объявления.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TitleTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(PriceTextBox.Text) ||
                !decimal.TryParse(PriceTextBox.Text, out _))
            {
                MessageBox.Show("Введите корректную цену.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                PriceTextBox.Focus();
                return false;
            }

            if (CityComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите город.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                CityComboBox.Focus();
                return false;
            }

            if (CategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                CategoryComboBox.Focus();
                return false;
            }

            if (TypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип объявления.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TypeComboBox.Focus();
                return false;
            }

            if (StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус объявления.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusComboBox.Focus();
                return false;
            }

            // Дополнительная проверка для завершённых объявлений
            var status = (AdStatuses)StatusComboBox.SelectedItem;
            if (status.StatusName == "Завершено")
            {
                if (string.IsNullOrWhiteSpace(ProfitTextBox.Text) ||
                    !decimal.TryParse(ProfitTextBox.Text, out decimal profit) ||
                    profit < 0)
                {
                    MessageBox.Show("Для завершённых объявлений введите корректную сумму прибыли (неотрицательное число).",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    ProfitTextBox.Focus();
                    return false;
                }
            }

            return true;
        }

        // Кнопка "Отмена"
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Вместо GoBack() используем Navigate
            NavigationService.Navigate(new MyAdsPage());
        }
    }
}