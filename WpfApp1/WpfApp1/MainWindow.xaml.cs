using System.Threading.Tasks;
using System;
using System.Windows;
using WpfApp1.ViewModels;
using System.Collections.Generic;
using System.Net.Http;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private LocalApiServer _apiServer;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            // Запускаем микро-сервер в фоне при открытии программы
            _apiServer = new LocalApiServer();
            Task.Run(() => _apiServer.StartAsync());
        }

        // Останавливаем сервер при закрытии программы
        protected override void OnClosed(EventArgs e)
        {
            _apiServer?.Stop();
            base.OnClosed(e);
        }

        public async Task<List<User>> LoadUsersAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                string url = "https://jsonplaceholder.typicode.com/users";

                var json = await client.GetStringAsync(url);

                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(json);
            }
        }
    }
}
