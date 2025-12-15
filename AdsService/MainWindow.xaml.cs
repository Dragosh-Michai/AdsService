using System.Windows;
using System.Windows.Navigation;

namespace AdsService
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigated += MainFrame_Navigated;
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Удаление записей из журнала навигации для кнопки "Назад"
            while (MainFrame.CanGoBack)
            {
                MainFrame.RemoveBackEntry();
            }
        }
    }
}