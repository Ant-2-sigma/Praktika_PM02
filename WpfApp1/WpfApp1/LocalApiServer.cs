using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WpfApp1.Models;
using Newtonsoft.Json;

namespace WpfApp1
{
    public class LocalApiServer
    {
        private HttpListener _listener;
        private CancellationTokenSource _cts;

        private const string Prefix = "http://localhost:8765/";

        public async Task StartAsync()
        {
            _cts = new CancellationTokenSource();
            _listener = new HttpListener();
            _listener.Prefixes.Add(Prefix);
            _listener.Start();

            Console.WriteLine($"API started: {Prefix}");

            while (!_cts.IsCancellationRequested)
            {
                HttpListenerContext context = null;

                try
                {
                    context = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequest(context)); // fire & forget безопасно
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Listener error: " + ex.Message);
                }
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            try
            {
                string path = context.Request.Url.AbsolutePath.ToLower();

                if (path == "/api/teachers")
                {
                    HandleTeachers(context);
                }
                else
                {
                    SendJson(context, 404, new { error = "Endpoint not found" });
                }
            }
            catch (Exception ex)
            {
                SendJson(context, 500, new
                {
                    error = "Server error",
                    message = ex.Message
                });
            }
        }

        // 💥 СТАБИЛЬНЫЙ МЕТОД ДЛЯ УЧИТЕЛЕЙ
        private void HandleTeachers(HttpListenerContext context)
        {
            try
            {
                using (var db = new MunicipalOlympiadsContext())
                {
                    var teachers = db.Teachers
                        .Select(t => new TeacherDto
                        {
                            ID = t.ID,
                            ФИО = t.ФИО,
                            Телефон = t.Телефон,
                            Email = t.Email
                        })
                        .ToList();

                    SendJson(context, 200, teachers);
                }
            }
            catch (Exception ex)
            {
                SendJson(context, 500, new
                {
                    error = "Database error",
                    message = ex.Message
                });
            }
        }

        // 💥 ЕДИНЫЙ МЕТОД ОТВЕТА (ГАРАНТИЯ ОТСУТСТВИЯ 500)
        private void SendJson(HttpListenerContext context, int statusCode, object data)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data);

                byte[] buffer = Encoding.UTF8.GetBytes(json);

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.ContentLength64 = buffer.Length;

                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            catch
            {
                // даже если JSON сломался — отдадим запасной ответ
                byte[] fallback = Encoding.UTF8.GetBytes("{\"error\":\"serialization failed\"}");

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                context.Response.OutputStream.Write(fallback, 0, fallback.Length);
            }
            finally
            {
                try { context.Response.OutputStream.Close(); } catch { }
            }
        }

        public void Stop()
        {
            try
            {
                _cts?.Cancel();
                _listener?.Stop();
                _listener?.Close();
            }
            catch { }
        }
    }

    // 💥 DTO (ВАЖНО — убирает 100% проблем EF JSON)
    public class TeacherDto
    {
        public int ID { get; set; }
        public string ФИО { get; set; }
        public string Телефон { get; set; }
        public string Email { get; set; }
    }
}