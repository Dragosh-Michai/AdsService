using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace AdsService.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        // Обработчик кнопки "Войти"
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            // Проверка заполненности полей
            if (string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Введите логин", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                LoginTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                PasswordBox.Focus();
                return;
            }

            try
            {
                using (var db = new Entities())
                {
                    // Поиск пользователя в базе данных
                    var user = db.Users.FirstOrDefault(u =>
                        u.Login == login && u.Password == password);

                    if (user != null)
                    {
                        // Сохранение информации о текущем пользователе
                        App.CurrentUser = user;

                        MessageBox.Show($"Добро пожаловать, {user.Login}!", "Успешный вход",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        // Переход на главную страницу с объявлениями
                        NavigationService.Navigate(new AdsListPage());
                    }
                    else
                    {
                        MessageBox.Show("Неверный логин или пароль", "Ошибка авторизации",
                            MessageBoxButton.OK, MessageBoxImage.Error);

                        // Очистка поля пароля
                        PasswordBox.Password = "";
                        PasswordBox.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик кнопки "Регистрация"
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            // Валидация введённых данных
            if (string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Введите логин", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                LoginTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                PasswordBox.Focus();
                return;
            }

            if (password.Length < 4)
            {
                MessageBox.Show("Пароль должен содержать не менее 4 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                PasswordBox.Focus();
                PasswordBox.SelectAll();
                return;
            }

            try
            {
                using (var db = new Entities())
                {
                    // Проверка уникальности логина
                    if (db.Users.Any(u => u.Login == login))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        LoginTextBox.Focus();
                        LoginTextBox.SelectAll();
                        return;
                    }

                    // Создание нового пользователя
                    var newUser = new Users
                    {
                        Login = login,
                        Password = password
                    };

                    db.Users.Add(newUser);
                    db.SaveChanges();

                    MessageBox.Show("Регистрация прошла успешно! Теперь вы можете войти в систему.",
                        "Успешная регистрация", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Очистка поля пароля после регистрации
                    PasswordBox.Password = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик кнопки "Назад к объявлениям"
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Возврат на страницу с объявлениями
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(new AdsListPage());
            }
        }

        // Автоматический вход при нажатии Enter в поле пароля
        private void PasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                LoginButton_Click(sender, e);
            }
        }
    }
}