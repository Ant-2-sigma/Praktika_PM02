using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    /// <summary>
    /// Упрощенная реализация работы с внешним API.
    /// Подключается к публичному тестовому серверу JSONPlaceholder.
    /// </summary>
    public class SimpleApiService
    {
        private static readonly HttpClient _client = new HttpClient();
        // Бесплатный тестовый API в интернете
        private const string ApiUrl = "https://jsonplaceholder.typicode.com/users";

        /// <summary>
        /// Метод для тестирования интеграции: получает список пользователей из интернета
        /// </summary>
        public static async Task<string> GetExternalDataAsync()
        {
            try
            {
                // Отправляем GET-запрос
                var response = await _client.GetAsync(ApiUrl);

                // Проверяем, что сервер ответил успешно (код 200)
                response.EnsureSuccessStatusCode();

                // Считываем ответ в виде строки (JSON)
                string json = await response.Content.ReadAsStringAsync();
                return json;
            }
            catch (Exception ex)
            {
                return $"Ошибка при обращении к API: {ex.Message}";
            }
        }

        /// <summary>
        /// Парсит JSON ответ и достает только имена пользователей
        /// </summary>
        public static async Task<List<string>> GetUserNamesAsync()
        {
            string json = await GetExternalDataAsync();

            try
            {
                // Превращаем JSON-строку в список объектов C#
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var names = new List<string>();
                    foreach (JsonElement element in doc.RootElement.EnumerateArray())
                    {
                        names.Add(element.GetProperty("name").GetString());
                    }
                    return names;
                }
            }
            catch
            {
                return new List<string> { "Не удалось распарсить данные" };
            }
        }
    }
}