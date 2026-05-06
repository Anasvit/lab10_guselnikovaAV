using System;
using System.Text.RegularExpressions;

namespace Lab10_GuselnikovaAV   
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Лабораторная работа №9");
            Console.WriteLine("Для выхода введите exit\n");

            while (true)
            {
                Console.Write("Логин: ");
                string login = Console.ReadLine();
                if (login == "exit") break;

                Console.Write("Пароль: ");
                string password = Console.ReadLine();
                if (password == "exit") break;

                Console.Write("Подтверждение пароля: ");
                string confirmPassword = Console.ReadLine();
                if (confirmPassword == "exit") break;

                string message;
                bool result = ValidateRegistration(login, password, confirmPassword, out message);

                Console.WriteLine("\nРезультат: " + (result ? "True" : "False"));
                Console.WriteLine("Сообщение: " + message);
                Console.WriteLine();
            }

            Console.WriteLine("Нажмите любую клавишу...");
            Console.ReadKey();
        }

        static bool ValidateRegistration(string login, string password, string confirmPassword, out string message)
        {
            DateTime time = DateTime.Now;

            // Логирование в консоль
            Console.WriteLine("\n[LOG] " + time + " | Логин: " + login + " | Пароль: " + password);

            // Проверка пустого логина
            if (string.IsNullOrEmpty(login))
            {
                message = "Пустой логин";
                Console.WriteLine("[LOG] Ошибка: " + message);
                return false;
            }

            // Проверка пустого пароля
            if (string.IsNullOrEmpty(password))
            {
                message = "Пустой пароль";
                Console.WriteLine("[LOG] Ошибка: " + message);
                return false;
            }

            // Проверка формата телефона
            if (login.StartsWith("+"))
            {
                string phonePattern = @"^\+\d{1,3}-\d{3}-\d{3}-\d{4}$";
                if (!Regex.IsMatch(login, phonePattern))
                {
                    message = "Неверный формат телефона";
                    Console.WriteLine("[LOG] Ошибка: " + message);
                    return false;
                }
            }
            // Проверка формата email
            else if (login.Contains("@"))
            {
                string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                if (!Regex.IsMatch(login, emailPattern))
                {
                    message = "Неверный формат почты";
                    Console.WriteLine("[LOG] Ошибка: " + message);
                    return false;
                }
            }
            // Обычная строка
            else
            {
                if (login.Length < 5)
                {
                    message = "Короткий логин";
                    Console.WriteLine("[LOG] Ошибка: " + message);
                    return false;
                }

                string stringPattern = @"^[a-zA-Z0-9_]+$";
                if (!Regex.IsMatch(login, stringPattern))
                {
                    message = "Недопустимые символы";
                    Console.WriteLine("[LOG] Ошибка: " + message);
                    return false;
                }
            }

            // Проверка длины пароля
            if (password.Length < 7)
            {
                message = "Короткий пароль";
                Console.WriteLine("[LOG] Ошибка: " + message);
                return false;
            }

            // Проверка совпадения паролей
            if (password != confirmPassword)
            {
                message = "Пароли не совпадают";
                Console.WriteLine("[LOG] Ошибка: " + message);
                return false;
            }

            // Всё ок
            message = "";
            Console.WriteLine("[LOG] Успешная регистрация");
            return true;
        }
    }
}