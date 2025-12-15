using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AdsService.Pages
{
    public partial class CompletedAdsPage : Page
    {
        public CompletedAdsPage()
        {
            InitializeComponent();
            Loaded += CompletedAdsPage_Loaded;
        }

        private void CompletedAdsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null)
            {
                MessageBox.Show("Для просмотра завершённых объявлений необходимо войти в систему.",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService.Navigate(new LoginPage());
                return;
            }

            LoadCompletedAds();
        }

        private void LoadCompletedAds()
        {
            try
            {
                using (var db = new Entities())
                {
                    // Простой запрос для получения завершённых объявлений
                    var completedAds = db.Ads
                        .Where(a => a.UserId == App.CurrentUser.UserId &&
                                   a.AdStatuses.StatusName == "Завершено" &&
                                   a.ProfitAmount.HasValue)
                        .OrderByDescending(a => a.PostDate)
                        .Select(a => new
                        {
                            a.AdId,
                            a.Title,
                            a.Description,
                            a.Price,
                            a.PostDate,
                            a.ProfitAmount,
                            CityName = a.Cities.CityName,
                            CategoryName = a.Categories.CategoryName,
                            TypeName = a.AdTypes.TypeName
                        })
                        .ToList();

                    decimal totalProfit = 0;

                    // Очищаем контейнер
                    AdsContainer.Children.Clear();

                    if (completedAds.Count == 0)
                    {
                        TotalProfitTextBlock.Text = "Нет завершённых сделок";
                        AdsCountTextBlock.Text = "";

                        // Сообщение об отсутствии данных
                        var noDataText = new TextBlock
                        {
                            Text = "У вас пока нет завершённых объявлений с указанной прибылью.",
                            FontSize = 16,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(20, 50, 20, 0),
                            TextAlignment = TextAlignment.Center,
                            TextWrapping = TextWrapping.Wrap
                        };

                        AdsContainer.Children.Add(noDataText);
                    }
                    else
                    {
                        // Создаём простой список
                        foreach (var ad in completedAds)
                        {
                            totalProfit += ad.ProfitAmount.Value;

                            // Используем Border для padding
                            var border = new Border
                            {
                                Margin = new Thickness(0, 0, 0, 15),
                                Background = Brushes.White,
                                BorderBrush = Brushes.LightGray,
                                BorderThickness = new Thickness(1),
                                Padding = new Thickness(15), // Padding здесь!
                                CornerRadius = new CornerRadius(5)
                            };

                            var stackPanel = new StackPanel();

                            // Заголовок
                            var titleText = new TextBlock
                            {
                                Text = ad.Title,
                                FontSize = 16,
                                FontWeight = FontWeights.Bold,
                                Margin = new Thickness(0, 0, 0, 8),
                                Foreground = new SolidColorBrush(Color.FromRgb(55, 71, 79))
                            };

                            // Прибыль (крупно и ярко)
                            var profitText = new TextBlock
                            {
                                Text = $"💰 Прибыль: {ad.ProfitAmount:N0} ₽",
                                FontSize = 18,
                                FontWeight = FontWeights.Bold,
                                Foreground = new SolidColorBrush(Color.FromRgb(255, 156, 26)),
                                Margin = new Thickness(0, 0, 0, 10)
                            };

                            // Детали
                            string dateString = ad.PostDate.HasValue
                                ? ad.PostDate.Value.ToString("dd.MM.yyyy")
                                : "не указана";

                            var detailsText = new TextBlock
                            {
                                Text = $"📍 {ad.CityName} | 🏷 {ad.CategoryName} | 📅 {dateString}",
                                FontSize = 12,
                                Margin = new Thickness(0, 0, 0, 8),
                                Foreground = new SolidColorBrush(Color.FromRgb(120, 144, 156))
                            };

                            // Описание (если есть)
                            if (!string.IsNullOrEmpty(ad.Description))
                            {
                                var descText = new TextBlock
                                {
                                    Text = ad.Description,
                                    FontSize = 12,
                                    TextWrapping = TextWrapping.Wrap,
                                    Margin = new Thickness(0, 0, 0, 5),
                                    Foreground = new SolidColorBrush(Color.FromRgb(120, 144, 156))
                                };
                                stackPanel.Children.Add(descText);
                            }

                            stackPanel.Children.Add(titleText);
                            stackPanel.Children.Add(profitText);
                            stackPanel.Children.Add(detailsText);

                            border.Child = stackPanel;
                            AdsContainer.Children.Add(border);
                        }

                        TotalProfitTextBlock.Text = $"💰 Общая прибыль: {totalProfit:N0} ₽";
                        AdsCountTextBlock.Text = $"📊 Завершённых сделок: {completedAds.Count}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки завершённых объявлений: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadCompletedAds();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(new MyAdsPage());
            }
        }
    }
}