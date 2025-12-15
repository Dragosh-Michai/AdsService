using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;



namespace AdsService
{
    public partial class App : Application
    {
        // Статическое свойство для хранения текущего пользователя
        public static Users CurrentUser { get; set; }

        // Свойство для проверки авторизации
        public static bool IsUserLoggedIn => CurrentUser != null;
    }
}