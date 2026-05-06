using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Serilog;

namespace Lab10_FIO
{
    class Program
    {
        static List<string> bannedLogins = new List<string>
        {
            "admin", "root", "user", "test", "guest",
            "administrator", "moderator", "support"
        };

        static void Main(string[] args)
        {
            string template = "{Timestamp:HH:mm:ss} | [{Level:u3}] | {Message:lj}{NewLine}{Exception}";
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: template)
                .WriteTo.File("logs/log_.txt", outputTemplate: template)
                .CreateLogger();

            Log.Information("Приложение запущено");
            Console.WriteLine("Лабораторная работа №9");
            Console.WriteLine("Проверка данных при регистрации");
            Console.WriteLine("Для выхода введите exit\n");

            while (true)
            {
                Console.Write("Логин: ");
                string login = Console.ReadLine();
                if (login == "exit") break;

                Console.Write("Пароль: ");
                string password = ReadPassword();
                if (password == "exit") break;

                Console.Write("Подтверждение пароля: ");
                string confirmPassword = ReadPassword();
                if (confirmPassword == "exit") break;

                string message;
                bool result = ValidateRegistration(login, password, confirmPassword, out message);

                Console.WriteLine("\nРезультат: " + (result ? "True" : "False"));
                Console.WriteLine("Сообщение: " + (string.IsNullOrEmpty(message) ? "(пусто)" : message));
                Console.WriteLine();
            }

            Log.Information("Приложение завершено");
            Log.CloseAndFlush();
            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }

        static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
                else if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Escape)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
            }
            while (true);
            return password;
        }

        static string MaskPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }

        static bool ValidateRegistration(string login, string password, string confirmPassword, out string message)
        {
            DateTime requestTime = DateTime.Now;
            string maskedPassword = MaskPassword(password);
            string maskedConfirm = MaskPassword(confirmPassword);

            try
            {
                Log.Debug("Валидация. Логин: {Login}", login);

                if (string.IsNullOrEmpty(login))
                {
                    message = "Ошибка: логин не может быть пустым";
                    LogFailure(requestTime, login, maskedPassword, maskedConfirm, message);
                    return false;
                }

                if (string.IsNullOrEmpty(password))
                {
                    message = "Ошибка: пароль не может быть пустым";
                    LogFailure(requestTime, login, maskedPassword, maskedConfirm, message);
                    return false;
                }

                if (login.StartsWith("+"))
                {
                    Log.Debug("Проверка формата телефона");
                    string phonePattern = @"^\+\d{1,3}-\d{3}-\d{3}-\d{4}$";
                    if (!Regex.IsMatch(login, phonePattern))
                    {
                        message = "Ошибка: неверный формат телефона (ожидается +x-xxx-xxx-xxxx)";
                        LogFailure(requestTime, login, maskedPassword, maskedConfirm, message);
                        return false;
                    }
                }
                else if (login.Contains("@"))
                {
                    Log.Debug("Проверка формата email");
                    string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                    if (!Regex.IsMatch(login, emailPattern))
                    {
                        message = "Ошибка: неверный формат электронной почты";
                        LogFailure(requestTime, login, maskedPassword, maskedConfirm, message);
                        return false;
                    }
                }
                else
                {
                    Log.Debug("Проверка строкового логина");

                    if (login.Length < 5)
                    {
                        message = "Ошибка: логин должен содержать минимум 5 символов";
                        LogFailure(requestTime, login, maskedPassword, maskedConfirm, message);
                        return false;
                    }

                    string stringPattern = @"^[a-zA-Z0-9_]+$";
                    if (!Regex.IsMatch(login, stringPattern))
                    {
                        message = "Ошибка: логин может содержать только латинские буквы, цифры и знак подчёркивания";
                        LogFailure(requestTime, login, maskedPassword, maskedConfirm, message);
                        return false;
                    }
                }

                foreach (string banned in bannedLogins)
                {
                    if (string.Equals(login, banned, StringComparison.OrdinalIgnoreCase))
                    {
                        message = "Ошибка: данный логин запрещён к использованию";
                        LogFailure(requestTime, login, maskedPassword, maskedConfirm, message);
                        return false;
                    }
                }

                if (password.Length < 7)
                {
                    message = "Ошибка: пароль должен содержать минимум 7 символов";
                    LogFailure(requestTime, login, maskedPassword, maskedConfirm, message);
                    return false;
                }

                if (Regex.IsMatch(password, @"[a-zA-Z]"))
                {
                    message = "Ошибка: пароль не должен содержать латинские буквы";
                    LogFailure(requestTime, login, maskedPassword, maskedConfirm, message);
                    return false;
                }

                if (!Regex.IsMatch(password, @"[А-ЯЁ]"))
                {
                    message = "Ошибка: пароль должен содержать хотя бы одну заглавную букву кириллицы";
                    LogFailure(requestTime, login, maskedPassword, maskedConfirm, message);
                    return false;
                }

                if (!Regex.IsMatch(password, @"[а-яё]"))
                {
                    message = "Ошибка: пароль должен содержать хотя бы одну строчную букву кириллицы";
                    LogFailure(requestTime, login, maskedPassword, maskedConfirm, message);
                    return false;
                }

                if (!Regex.IsMatch(password, @"[0-9]"))
                {
                    message = "Ошибка: пароль должен содержать хотя бы одну цифру";
                    LogFailure(requestTime, login, maskedPassword, maskedConfirm, message);
                    return false;
                }

                if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?`~]"))
                {
                    message = "Ошибка: пароль должен содержать хотя бы один специальный символ";
                    LogFailure(requestTime, login, maskedPassword, maskedConfirm, message);
                    return false;
                }

                if (password != confirmPassword)
                {
                    message = "Ошибка: пароль и подтверждение пароля не совпадают";
                    LogFailure(requestTime, login, maskedPassword, maskedConfirm, message);
                    return false;
                }

                message = "";
                LogSuccess(requestTime, login, maskedPassword, maskedConfirm);
                return true;
            }
            catch (Exception ex)
            {
                message = "Ошибка: внутренняя ошибка сервера";
                Log.Error(ex, "Исключение при валидации");
                return false;
            }
        }

        static void LogSuccess(DateTime time, string login, string maskedPass, string maskedConfirm)
        {
            Log.Information("=== Успешная регистрация ===");
            Log.Information("Дата-время: {Time}", time);
            Log.Information("Логин: {Login}", login);
            Log.Information("Пароль (маскированный): {Pass}", maskedPass);
            Log.Information("Подтверждение (маскированное): {Confirm}", maskedConfirm);
            Log.Information("Статус: Успешная регистрация");
        }

        static void LogFailure(DateTime time, string login, string maskedPass, string maskedConfirm, string errorMessage)
        {
            Log.Warning("=== Неуспешная регистрация ===");
            Log.Warning("Дата-время: {Time}", time);
            Log.Warning("Логин: {Login}", login);
            Log.Warning("Пароль (маскированный): {Pass}", maskedPass);
            Log.Warning("Подтверждение (маскированное): {Confirm}", maskedConfirm);
            Log.Warning("Ошибка: {Error}", errorMessage);
        }
    }
}