using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WpfApp1.Models
{
    public static class ApiClient
    {
        private static readonly HttpClient _client = new HttpClient();
        private static string _baseUrl = "https://localhost:44320/api/";

        static ApiClient()
        {
            _client.Timeout = TimeSpan.FromSeconds(30);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static void SetBaseUrl(string url)
        {
            _baseUrl = url.TrimEnd('/') + "/";
        }

        public static async Task<List<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _client.GetAsync(_baseUrl + endpoint);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<T>>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"API GET ошибка ({endpoint}): {ex.Message}");
                throw;
            }
        }

        public static async Task<T> GetByIdAsync<T>(string endpoint, int id)
        {
            try
            {
                var response = await _client.GetAsync(_baseUrl + endpoint + "/" + id);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return default(T);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"API GET BY ID ошибка ({endpoint}/{id}): {ex.Message}");
                throw;
            }
        }

        public static async Task<T> PostAsync<T>(string endpoint, T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync(_baseUrl + endpoint, content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseJson);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"API POST ошибка ({endpoint}): {ex.Message}");
                throw;
            }
        }

        public static async Task<T> PutAsync<T>(string endpoint, int id, T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PutAsync(_baseUrl + endpoint + "/" + id, content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseJson);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"API PUT ошибка ({endpoint}/{id}): {ex.Message}");
                throw;
            }
        }

        public static async Task<bool> DeleteAsync(string endpoint, int id)
        {
            try
            {
                var response = await _client.DeleteAsync(_baseUrl + endpoint + "/" + id);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"API DELETE ошибка ({endpoint}/{id}): {ex.Message}");
                throw;
            }
        }

        public static async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _client.GetAsync(_baseUrl + "status");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}